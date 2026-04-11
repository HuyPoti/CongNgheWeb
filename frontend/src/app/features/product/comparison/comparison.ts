import { Component, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { CommonModule } from '@angular/common';
import { TranslatePipe } from '../../../core/pipes/translate.pipe';
import { ComparisonService } from '../../../core/services/comparison';

@Component({
  selector: 'app-comparison',
  imports: [RouterLink, CommonModule, TranslatePipe],
  templateUrl: './comparison.html',
  styles: ``,
})
export class Comparison {
  comparisonService = inject(ComparisonService);

  // Specs to compare for demo purposes
  specKeys = [
    { group: 'General', items: ['Category'] },
  ];

  getSpecValue(productId: string, specKey: string): string {
    const product = this.comparisonService.selectedProducts().find(p => p.id === productId);
    if (!product) return '—';
    return product.specs[specKey] || '—';
  }

  getSpecKeys(): string[] {
    const products = this.comparisonService.selectedProducts();
    const allKeys = new Set<string>();
    products.forEach(p => {
      Object.keys(p.specs).forEach(k => allKeys.add(k));
    });
    return Array.from(allKeys);
  }
}
