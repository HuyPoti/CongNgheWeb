import { Component, Output, EventEmitter, inject, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { ShopStateService } from '../../../core/services/shop-state.service';
import { ProductService } from '../../../core/services/product.service';

@Component({
  selector: 'app-search-overlay',
  standalone: true,
  imports: [FormsModule, CommonModule],
  templateUrl: './search-overlay.html',
  styles: ``,
})
export class SearchOverlay implements OnInit {
  @Output() closeOverlay = new EventEmitter<void>();
  private router = inject(Router);
  private shopStateService = inject(ShopStateService);
  private productService = inject(ProductService);

  searchQuery = '';
  trendingProducts: string[] = [];

  ngOnInit() {
    this.productService.fetchClientProducts({ page: 1, pageSize: 5 }).subscribe({
      next: (res) => {
        this.trendingProducts = res.items.map(p => p.name);
      },
      error: () => {
        this.trendingProducts = [];
      }
    });
  }

  searchFor(term: string) {
    this.searchQuery = term;
    this.onSearch();
  }

  onSearch() {
    if (this.searchQuery.trim()) {
      this.shopStateService.setSearchKeyword(this.searchQuery.trim());
      this.shopStateService.setCurrentPage(1);
      void this.router.navigate(['/product/list']);
      this.closeOverlay.emit();
    }
  }
}

