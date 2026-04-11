import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ProductCard } from '../../../core/models/product.model';
import { MOCK_PRODUCTS } from '../../../core/mocks/product.mock';

@Component({
  selector: 'app-category',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './category.html',
  styles: ``,
})
export class Category {
  products: ProductCard[] = MOCK_PRODUCTS;
  brands = ['NVIDIA', 'AMD', 'ASUS', 'MSI', 'INTEL', 'G.SKILL', 'SAMSUNG', 'CORSAIR', 'NZXT', 'LIAN LI'];
  vramSizes = ['8GB', '12GB', '16GB', '20GB', '24GB'];
}
