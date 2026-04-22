import { Component, computed, inject, signal } from '@angular/core';
import { CommonModule, CurrencyPipe, DatePipe } from '@angular/common';
import { Router } from '@angular/router';
import { firstValueFrom } from 'rxjs';
import { TranslatePipe } from '../../core/pipes/translate.pipe';
import { ProductCard, ProductListItemDto } from '../../core/models/product.model';
import { ProductService } from '../../core/services/product.service';
import { CartService } from '../../core/services/cart.service';
import { ToastService } from '../../core/services/toast.service';
import { jsPDF } from 'jspdf';
import autoTable from 'jspdf-autotable';

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
  private readonly toastService = inject(ToastService);
  private readonly router = inject(Router);
  private readonly currencyPipe = inject(CurrencyPipe);
  private readonly datePipe = inject(DatePipe);

  private readonly stateKey = 'build_pc_state_v1';
  private requestId = 0;

  currentStepId = signal<StepId>('cpu');

  readonly steps: BuildStep[] = [
    {
      id: 'cpu',
      label: 'build.step_cpu',
      icon: 'memory',
      slugCandidates: ['cpu', 'processor', 'processors'],
    },
    {
      id: 'mb',
      label: 'build.step_mb',
      icon: 'developer_board',
      slugCandidates: ['mainboard', 'motherboard', 'motherboards', 'mb'],
    },
    { id: 'ram', label: 'build.step_ram', icon: 'memory_alt', slugCandidates: ['ram', 'memory'] },
    {
      id: 'gpu',
      label: 'build.step_gpu',
      icon: 'videogame_asset',
      slugCandidates: ['gpu', 'graphics-card', 'vga'],
    },
    {
      id: 'storage',
      label: 'build.step_storage',
      icon: 'database',
      slugCandidates: ['storage', 'ssd', 'hdd'],
    },
    {
      id: 'psu',
      label: 'build.step_psu',
      icon: 'bolt',
      slugCandidates: ['psu', 'power-supply', 'power-supplies'],
    },
    {
      id: 'case',
      label: 'build.step_case',
      icon: 'fan_indirect',
      slugCandidates: ['case', 'pc-case', 'chassis'],
    },
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

  stepQuantities = signal<Record<StepId, number>>({
    cpu: 1,
    mb: 1,
    ram: 1,
    gpu: 1,
    storage: 1,
    psu: 1,
    case: 1,
  });

  availableProducts = signal<ProductCard[]>([]);
  productSpecsByProductId = signal<Record<string, Record<string, string>>>({});
  isLoadingProducts = signal(false);
  hasLoadingError = signal(false);
  isSaving = signal(false);
  isModalOpen = signal(false);

  totalPrice = computed(() => {
    const selected = this.selectedComponents();
    const quantities = this.stepQuantities();
    return Object.entries(selected).reduce((acc, [stepId, p]) => {
      if (!p) return acc;
      const qty = quantities[stepId as StepId] || 1;
      return acc + p.price * qty;
    }, 0);
  });

  progress = computed(() => {
    const selected = Object.values(this.selectedComponents()).filter((p) => p !== null).length;
    return Math.round((selected / this.steps.length) * 100);
  });

  selectedCount = computed(
    () => Object.values(this.selectedComponents()).filter((p) => p !== null).length,
  );

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
      const psuWatt = this.parseWatts(
        this.getSpecValue(psu.id, ['wattage', 'power', 'capacity_watt']),
      );

      const estimatedLoad = cpuTdp + gpuTdp + 120;
      const required = Math.round(estimatedLoad * 1.2);

      if (psuWatt > 0 && required > psuWatt) {
        issues.push('build.compat_psu_power');
      }
    }

    return issues;
  });

  canAddToCart = computed(() => this.selectedCount() > 0);

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

  openModal(id: string) {
    const stepId = id as StepId;
    this.currentStepId.set(stepId);
    this.isModalOpen.set(true);
    void this.loadProductsForStep(stepId);
  }

  closeModal() {
    this.isModalOpen.set(false);
  }

  updateQuantity(stepId: string, delta: number) {
    const id = stepId as StepId;
    const current = this.stepQuantities()[id] || 1;
    const next = Math.max(1, current + delta);

    const product = this.selectedComponents()[id];
    if (product && next > product.stockQuantity) {
      this.toastService.warning(`Số lượng tối đa cho phép là ${product.stockQuantity}`);
      return;
    }

    this.stepQuantities.update((prev) => ({ ...prev, [id]: next }));
    this.persistState();
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
    this.toastService.success(`Đã thêm ${product.name} vào cấu hình`);

    this.closeModal();
  }

  clearStepSelection(stepId: string) {
    const id = stepId as StepId;
    const product = this.selectedComponents()[id];
    this.selectedComponents.update((prev) => ({
      ...prev,
      [id]: null,
    }));
    this.currentStepId.set(id);
    this.persistState();
    void this.loadProductsForStep(id);
    if (product) {
      this.toastService.info(`Đã gỡ bỏ ${product.name}`);
    }
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
    this.stepQuantities.set({
      cpu: 1,
      mb: 1,
      ram: 1,
      gpu: 1,
      storage: 1,
      psu: 1,
      case: 1,
    });
    this.currentStepId.set('cpu');
    this.persistState();
    void this.loadProductsForStep('cpu');
    this.toastService.info('Đã xóa toàn bộ cấu hình');
  }

  async saveBuild() {
    this.isSaving.set(true);
    this.persistState();
    setTimeout(() => {
      this.isSaving.set(false);
      this.toastService.success('Đã lưu cấu hình hiện tại');
    }, 500);
  }

  addBuildToCart() {
    if (!this.canAddToCart()) return;

    const selectedEntries = Object.entries(this.selectedComponents());
    const quantities = this.stepQuantities();

    for (const [stepId, item] of selectedEntries) {
      if (item) {
        const qty = quantities[stepId as StepId] || 1;
        for (let i = 0; i < qty; i++) {
          this.cartService.addToCart(item);
        }
      }
    }

    this.toastService.success('Đã thêm toàn bộ linh kiện vào giỏ hàng');
    void this.router.navigateByUrl('/cart');
  }

  async exportToPdf() {
    if (this.selectedCount() === 0) {
      this.toastService.warning('Please select components before exporting');
      return;
    }

    this.toastService.info('Generating PDF with images...');

    try {
      const doc = new jsPDF();
      const primaryColor: [number, number, number] = [37, 99, 235]; // blue-600

      // Header
      doc.setFontSize(22);
      doc.setTextColor(primaryColor[0], primaryColor[1], primaryColor[2]);
      doc.text('ZX Computer - PC Builder', 105, 20, { align: 'center' });

      doc.setFontSize(10);
      doc.setTextColor(100, 100, 100);
      doc.text(
        `Created at: ${this.datePipe.transform(new Date(), 'MMMM dd, yyyy HH:mm', '+0700', 'en-US')}`,
        105,
        28,
        {
          align: 'center',
        },
      );

      // Helper: format number as "1,234,567 VND" (ASCII-safe, no Unicode symbol)
      const formatVnd = (value: number): string => {
        return value.toLocaleString('en-US') + ' VND';
      };

      // Fetch all product images as base64
      const selectedSteps = this.steps.filter((step) => this.selectedComponents()[step.id]);
      const imageMap = new Map<string, string>();

      await Promise.all(
        selectedSteps.map(async (step) => {
          const p = this.selectedComponents()[step.id]!;
          if (p.image) {
            try {
              const base64 = await this.loadImageAsBase64(p.image);
              imageMap.set(step.id, base64);
            } catch {
              // Skip image if loading fails
            }
          }
        }),
      );

      // Table Data – 6 columns: No | Image | Product Name | Qty | Unit Price | Total
      const tableData = selectedSteps.map((step, index) => {
        const p = this.selectedComponents()[step.id]!;
        const qty = this.stepQuantities()[step.id] || 1;
        const label = this.translateLabel(step.label);
        return [
          index + 1,
          '', // Image placeholder – drawn via didDrawCell
          `[${label}] ${p.name}`,
          qty,
          formatVnd(p.price),
          formatVnd(p.price * qty),
        ];
      });

      // Store step IDs for image lookup in didDrawCell
      const stepIds = selectedSteps.map((s) => s.id);

      autoTable(doc, {
        startY: 35,
        head: [['No', 'Image', 'Product Name', 'Qty', 'Unit Price', 'Total']],
        body: tableData,
        theme: 'striped',
        headStyles: {
          fillColor: primaryColor,
          textColor: [255, 255, 255],
          fontSize: 10,
          fontStyle: 'bold',
          halign: 'center',
          valign: 'middle',
        },
        columnStyles: {
          0: { cellWidth: 12, halign: 'center', valign: 'middle' },
          1: { cellWidth: 22, halign: 'center', valign: 'middle' },
          2: { cellWidth: 'auto', valign: 'middle' },
          3: { cellWidth: 15, halign: 'center', valign: 'middle' },
          4: { cellWidth: 32, halign: 'right', valign: 'middle' },
          5: { cellWidth: 32, halign: 'right', valign: 'middle' },
        },
        styles: { fontSize: 9, cellPadding: 4, minCellHeight: 20 },
        margin: { left: 14, right: 14 },
        didDrawCell: (data) => {
          // Draw image in column index 1 (Image column), only for body rows
          if (data.section === 'body' && data.column.index === 1) {
            const stepId = stepIds[data.row.index];
            const base64 = imageMap.get(stepId);
            if (base64) {
              const imgSize = 16;
              const x = data.cell.x + (data.cell.width - imgSize) / 2;
              const y = data.cell.y + (data.cell.height - imgSize) / 2;
              doc.addImage(base64, 'JPEG', x, y, imgSize, imgSize);
            }
          }
        },
      });

      // Total
      // eslint-disable-next-line @typescript-eslint/no-explicit-any
      const finalY = (doc as any).lastAutoTable.finalY || 40;
      doc.setFontSize(13);
      doc.setTextColor(primaryColor[0], primaryColor[1], primaryColor[2]);
      doc.text(`TOTAL: ${formatVnd(this.totalPrice())}`, 196, finalY + 12, { align: 'right' });

      doc.setFontSize(9);
      doc.setTextColor(150, 150, 150);
      doc.text('Thank you for choosing ZX Computer Build Service!', 105, finalY + 24, {
        align: 'center',
      });

      doc.save(`zx-computer-build-${new Date().getTime()}.pdf`);
      this.toastService.success('PDF configuration exported successfully');
    } catch (error) {
      console.error('PDF Error:', error);
      this.toastService.error('Error occurred while generating PDF');
    }
  }

  private loadImageAsBase64(url: string): Promise<string> {
    return new Promise((resolve, reject) => {
      const img = new Image();
      img.crossOrigin = 'anonymous';
      img.onload = () => {
        const canvas = document.createElement('canvas');
        canvas.width = img.naturalWidth;
        canvas.height = img.naturalHeight;
        const ctx = canvas.getContext('2d');
        if (!ctx) {
          reject(new Error('Canvas context not available'));
          return;
        }
        // White background (for transparent PNGs)
        ctx.fillStyle = '#FFFFFF';
        ctx.fillRect(0, 0, canvas.width, canvas.height);
        ctx.drawImage(img, 0, 0);
        resolve(canvas.toDataURL('image/jpeg', 0.8));
      };
      img.onerror = () => reject(new Error('Image load failed'));
      img.src = url;
    });
  }

  private translateLabel(key: string): string {
    // Simple translation fallback if needed, but here we just return descriptive names
    const mapping: Record<string, string> = {
      'build.step_cpu': 'CPU',
      'build.step_mb': 'Mainboard',
      'build.step_ram': 'RAM',
      'build.step_gpu': 'Graphics Card',
      'build.step_storage': 'Storage',
      'build.step_psu': 'Power Supply',
      'build.step_case': 'Chassis',
    };
    return mapping[key] || key;
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
      if (requestId === this.requestId) {
        this.isLoadingProducts.set(false);
      }
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
    return (
      this.normalizeSpecKey(a).includes(this.normalizeSpecKey(b)) ||
      this.normalizeSpecKey(b).includes(this.normalizeSpecKey(a))
    );
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
      stepQuantities: this.stepQuantities(),
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
        stepQuantities?: Partial<Record<StepId, number>>;
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

      if (parsed.stepQuantities) {
        this.stepQuantities.set({
          cpu: parsed.stepQuantities.cpu ?? 1,
          mb: parsed.stepQuantities.mb ?? 1,
          ram: parsed.stepQuantities.ram ?? 1,
          gpu: parsed.stepQuantities.gpu ?? 1,
          storage: parsed.stepQuantities.storage ?? 1,
          psu: parsed.stepQuantities.psu ?? 1,
          case: parsed.stepQuantities.case ?? 1,
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
