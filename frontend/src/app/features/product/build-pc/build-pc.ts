import { Component, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TranslatePipe } from '../../../core/pipes/translate.pipe';
import { MOCK_PRODUCTS } from '../../../core/mocks/product.mock';
import { ProductCard } from '../../../core/models/product.model';

@Component({
  selector: 'app-build-pc',
  standalone: true,
  imports: [CommonModule, TranslatePipe],
  templateUrl: './build-pc.html',
  styleUrl: './build-pc.css',
})
export class BuildPc {
  currentStepId = signal('cpu');
  
  steps = [
    { id: 'cpu', label: 'build.step_cpu', icon: 'memory', category: 'Processors' },
    { id: 'mb', label: 'build.step_mb', icon: 'developer_board', category: 'Motherboards' },
    { id: 'ram', label: 'build.step_ram', icon: 'memory_alt', category: 'Memory' },
    { id: 'gpu', label: 'build.step_gpu', icon: 'videogame_asset', category: 'Graphics Cards' },
    { id: 'storage', label: 'build.step_storage', icon: 'database', category: 'Storage' },
    { id: 'psu', label: 'build.step_psu', icon: 'bolt', category: 'Power Supplies' },
    { id: 'case', label: 'build.step_case', icon: 'fan_indirect', category: 'Cases' }
  ];

  selectedComponents = signal<Record<string, ProductCard | null>>({
    cpu: null,
    mb: null,
    ram: null,
    gpu: null,
    storage: null,
    psu: null,
    case: null
  });

  filteredProducts = computed(() => {
    const currentStep = this.steps.find(s => s.id === this.currentStepId());
    if (!currentStep) return [];
    return MOCK_PRODUCTS.filter(p => p.category === currentStep.category);
  });

  totalPrice = computed(() => {
    return Object.values(this.selectedComponents()).reduce((acc, p) => acc + (p?.price || 0), 0);
  });

  progress = computed(() => {
    const selected = Object.values(this.selectedComponents()).filter(p => p !== null).length;
    return Math.round((selected / this.steps.length) * 100);
  });

  selectStep(id: string) {
    this.currentStepId.set(id);
  }

  selectComponent(product: ProductCard) {
    this.selectedComponents.update(prev => ({
      ...prev,
      [this.currentStepId()]: product
    }));
    
    // Auto advance to next step
    const currentIndex = this.steps.findIndex(s => s.id === this.currentStepId());
    if (currentIndex < this.steps.length - 1) {
      this.currentStepId.set(this.steps[currentIndex + 1].id);
    }
  }

  getStepStatus(id: string) {
    if (this.selectedComponents()[id]) return 'build.status_installed';
    if (this.currentStepId() === id) return 'build.status_selecting';
    return 'build.status_pending';
  }
}
