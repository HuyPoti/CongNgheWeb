import { Component, computed, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { firstValueFrom } from 'rxjs';
import { TranslatePipe } from '../../core/pipes/translate.pipe';
import { ProductCard, ProductListItemDto } from '../../core/models/product.model';
import { ProductService } from '../../core/services/product.service';
import { CartService } from '../../core/services/cart.service';

type StepId = 'cpu' | 'mb' | 'ram' | 'gpu' | 'storage' | 'psu' | 'case';

interface BuildStep {
  id: StepId;
  label: string;
  icon: string;
  slugCandidates: string[];
}

@Component({
  selector: 'app-build-pc',
  standalone: true,
  imports: [CommonModule, TranslatePipe],
  templateUrl: './build-pc.html',
  styleUrl: './build-pc.css',
})
export class BuildPc {
  private readonly productService = inject(ProductService);
  private readonly cartService = inject(CartService);
  private readonly router = inject(Router);

  private readonly stateKey = 'build_pc_state_v1';
  private requestId = 0;

  currentStepId = signal<StepId>('cpu');

  readonly steps: BuildStep[] = [
    { id: 'cpu', label: 'build.step_cpu', icon: 'memory', slugCandidates: ['cpu', 'processor', 'processors'] },
    {
      id: 'mb',
      label: 'build.step_mb',
      icon: 'developer_board',
      slugCandidates: ['mainboard', 'motherboard', 'motherboards', 'mb'],
    },
    { id: 'ram', label: 'build.step_ram', icon: 'memory_alt', slugCandidates: ['ram', 'memory'] },
    { id: 'gpu', label: 'build.step_gpu', icon: 'videogame_asset', slugCandidates: ['gpu', 'graphics-card', 'vga'] },
    { id: 'storage', label: 'build.step_storage', icon: 'database', slugCandidates: ['storage', 'ssd', 'hdd'] },
    { id: 'psu', label: 'build.step_psu', icon: 'bolt', slugCandidates: ['psu', 'power-supply', 'power-supplies'] },
    { id: 'case', label: 'build.step_case', icon: 'fan_indirect', slugCandidates: ['case', 'pc-case', 'chassis'] },
  ];

  selectedComponents = signal<Record<StepId, ProductCard | null>>({
    cpu: null,
    mb: null,
    ram: null,
    gpu: null,
    storage: null,
    psu: null,
    case: null,
  });

  availableProducts = signal<ProductCard[]>([]);
  productSpecsByProductId = signal<Record<string, Record<string, string>>>({});
  isLoadingProducts = signal(false);
  hasLoadingError = signal(false);
  isSaving = signal(false);

  totalPrice = computed(() => {
    return Object.values(this.selectedComponents()).reduce((acc, p) => acc + (p?.price ?? 0), 0);
  });

  progress = computed(() => {
    const selected = Object.values(this.selectedComponents()).filter((p) => p !== null).length;
    return Math.round((selected / this.steps.length) * 100);
  });

  selectedCount = computed(() => Object.values(this.selectedComponents()).filter((p) => p !== null).length);

  compatibilityIssues = computed(() => {
    const selected = this.selectedComponents();
    const issues: string[] = [];

    const cpu = selected.cpu;
    const mb = selected.mb;
    const ram = selected.ram;
    const gpu = selected.gpu;
    const psu = selected.psu;

    if (cpu && mb) {
      const cpuSocket = this.getSpecValue(cpu.id, ['socket', 'cpu_socket', 'socket_type']);
      const mbSocket = this.getSpecValue(mb.id, ['socket', 'cpu_socket', 'supported_socket']);

      if (cpuSocket && mbSocket && !this.isSameSpec(cpuSocket, mbSocket)) {
        issues.push('build.compat_cpu_mb_socket');
      }
    }

    if (ram && mb) {
      const ramType = this.getSpecValue(ram.id, ['ram_type', 'memory_type', 'type']);
      const mbRamType = this.getSpecValue(mb.id, ['ram_type', 'memory_type', 'supported_memory']);

      if (ramType && mbRamType && !this.isSameSpec(ramType, mbRamType)) {
        issues.push('build.compat_ram_mb_type');
      }
    }

    if (psu) {
      const cpuTdp = this.parseWatts(this.getSpecValue(cpu?.id, ['tdp', 'power_draw', 'power']));
      const gpuTdp = this.parseWatts(this.getSpecValue(gpu?.id, ['tdp', 'power_draw', 'power']));
      const psuWatt = this.parseWatts(this.getSpecValue(psu.id, ['wattage', 'power', 'capacity_watt']));

      const estimatedLoad = cpuTdp + gpuTdp + 120;
      const required = Math.round(estimatedLoad * 1.2);

      if (psuWatt > 0 && required > psuWatt) {
        issues.push('build.compat_psu_power');
      }
    }

    return issues;
  });

  canAddToCart = computed(
    () => this.selectedCount() === this.steps.length && this.compatibilityIssues().length === 0,
  );

  constructor() {
    this.restoreState();
    void this.loadProductsForStep(this.currentStepId());

    const restored = Object.values(this.selectedComponents()).filter(
      (item): item is ProductCard => item !== null,
    );
    for (const item of restored) {
      void this.loadProductSpecs(item.id);
    }
  }

  selectStep(id: string) {
    const stepId = id as StepId;
    this.currentStepId.set(stepId);
    this.persistState();
    void this.loadProductsForStep(stepId);
  }

  async retryLoadProducts() {
    await this.loadProductsForStep(this.currentStepId());
  }

  selectComponent(product: ProductCard) {
    if (product.stockQuantity <= 0) return;

    this.selectedComponents.update((prev) => ({
      ...prev,
      [this.currentStepId()]: product,
    }));
    void this.loadProductSpecs(product.id);
    this.persistState();

    // Auto advance to next step
    const currentIndex = this.steps.findIndex((s) => s.id === this.currentStepId());
    if (currentIndex < this.steps.length - 1) {
      this.currentStepId.set(this.steps[currentIndex + 1].id);
      void this.loadProductsForStep(this.currentStepId());
    }
  }

  clearStepSelection(stepId: string) {
    const id = stepId as StepId;
    this.selectedComponents.update((prev) => ({
      ...prev,
      [id]: null,
    }));
    this.currentStepId.set(id);
    this.persistState();
    void this.loadProductsForStep(id);
  }

  resetBuild() {
    const shouldReset = window.confirm('Xoa toan bo cau hinh hien tai?');
    if (!shouldReset) return;

    this.selectedComponents.set({
      cpu: null,
      mb: null,
      ram: null,
      gpu: null,
      storage: null,
      psu: null,
      case: null,
    });
    this.currentStepId.set('cpu');
    this.persistState();
    void this.loadProductsForStep('cpu');
  }

  async saveBuild() {
    this.isSaving.set(true);
    this.persistState();
    setTimeout(() => this.isSaving.set(false), 500);
  }

  addBuildToCart() {
    if (!this.canAddToCart()) return;

    const selected = Object.values(this.selectedComponents()).filter(
      (item): item is ProductCard => item !== null,
    );

    for (const item of selected) {
      this.cartService.addToCart(item);
    }

    void this.router.navigateByUrl('/cart');
  }

  getStepStatus(id: string) {
    const stepId = id as StepId;
    if (this.selectedComponents()[stepId]) return 'build.status_installed';
    if (this.currentStepId() === id) return 'build.status_selecting';
    return 'build.status_pending';
  }

  private async loadProductsForStep(stepId: StepId): Promise<void> {
    const step = this.steps.find((s) => s.id === stepId);
    if (!step) {
      this.availableProducts.set([]);
      return;
    }

    const requestId = ++this.requestId;
    this.isLoadingProducts.set(true);
    this.hasLoadingError.set(false);
    this.availableProducts.set([]);

    try {
      let cards: ProductCard[] = [];

      for (const slug of step.slugCandidates) {
        const res = await firstValueFrom(
          this.productService.fetchClientProducts({
            page: 1,
            pageSize: 30,
            categorySlug: slug,
            sortBy: 'newest',
          }),
        );

        if (requestId !== this.requestId) return;

        cards = res.items.map((p) => this.toCard(p));
        if (cards.length > 0) break;
      }

      this.availableProducts.set(cards);
    } catch {
      if (requestId !== this.requestId) return;
      this.hasLoadingError.set(true);
      this.availableProducts.set([]);
    } finally {
      if (requestId !== this.requestId) return;
      this.isLoadingProducts.set(false);
    }
  }

  private toCard(p: ProductListItemDto): ProductCard {
    return {
      id: p.id,
      name: p.name,
      price: p.price,
      regularPrice: p.regularPrice,
      salePrice: p.salePrice,
      image: p.thumbnailUrl ?? '',
      category: p.categoryName,
      brand: p.brandName,
      brandId: p.brandId,
      stockQuantity: p.stockQuantity,
      warrantyMonths: p.warrantyMonths,
      specs: {},
    };
  }

  private async loadProductSpecs(productId: string): Promise<void> {
    const cached = this.productSpecsByProductId()[productId];
    if (cached) return;

    try {
      const specs = await firstValueFrom(this.productService.getSpecs(productId));
      const normalized: Record<string, string> = {};
      for (const spec of specs) {
        normalized[this.normalizeSpecKey(spec.specKey)] = spec.specValue;
      }

      this.productSpecsByProductId.update((prev) => ({
        ...prev,
        [productId]: normalized,
      }));
    } catch {
      this.productSpecsByProductId.update((prev) => ({
        ...prev,
        [productId]: {},
      }));
    }
  }

  private getSpecValue(productId: string | undefined, keys: string[]): string {
    if (!productId) return '';

    const specs = this.productSpecsByProductId()[productId];
    if (!specs) return '';

    for (const key of keys) {
      const value = specs[this.normalizeSpecKey(key)];
      if (value && value.trim().length > 0) {
        return value;
      }
    }

    return '';
  }

  private normalizeSpecKey(key: string): string {
    return key
      .toLowerCase()
      .trim()
      .replace(/[^a-z0-9]+/g, '_')
      .replace(/^_+|_+$/g, '');
  }

  private isSameSpec(a: string, b: string): boolean {
    return this.normalizeSpecKey(a).includes(this.normalizeSpecKey(b)) ||
      this.normalizeSpecKey(b).includes(this.normalizeSpecKey(a));
  }

  private parseWatts(input: string): number {
    if (!input) return 0;
    const match = input.match(/\d+/);
    return match ? Number(match[0]) : 0;
  }

  private persistState() {
    if (!this.canUseStorage()) return;

    const payload = {
      currentStepId: this.currentStepId(),
      selectedComponents: this.selectedComponents(),
    };
    localStorage.setItem(this.stateKey, JSON.stringify(payload));
  }

  private restoreState() {
    if (!this.canUseStorage()) return;

    const raw = localStorage.getItem(this.stateKey);
    if (!raw) return;

    try {
      const parsed = JSON.parse(raw) as {
        currentStepId?: StepId;
        selectedComponents?: Partial<Record<StepId, ProductCard | null>>;
      };

      if (parsed.currentStepId && this.steps.some((s) => s.id === parsed.currentStepId)) {
        this.currentStepId.set(parsed.currentStepId);
      }

      if (parsed.selectedComponents) {
        this.selectedComponents.set({
          cpu: parsed.selectedComponents.cpu ?? null,
          mb: parsed.selectedComponents.mb ?? null,
          ram: parsed.selectedComponents.ram ?? null,
          gpu: parsed.selectedComponents.gpu ?? null,
          storage: parsed.selectedComponents.storage ?? null,
          psu: parsed.selectedComponents.psu ?? null,
          case: parsed.selectedComponents.case ?? null,
        });
      }
    } catch {
      localStorage.removeItem(this.stateKey);
    }
  }

  private canUseStorage(): boolean {
    return typeof window !== 'undefined' && typeof localStorage !== 'undefined';
  }
}
