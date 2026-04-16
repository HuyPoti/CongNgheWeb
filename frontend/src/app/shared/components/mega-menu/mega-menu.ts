import { Component, Input, OnChanges, SimpleChanges } from '@angular/core';
import { RouterLink } from '@angular/router';
import { CommonModule } from '@angular/common';
import { Category } from '../../../core/models/category.model';

@Component({
  selector: 'app-mega-menu',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './mega-menu.html',
})
export class MegaMenu implements OnChanges {
  @Input() categories: Category[] = [];

  activeCategory: Category | null = null;

  ngOnChanges(changes: SimpleChanges): void {
    if (!changes['categories']) return;

    const roots = this.rootCategories;
    if (!this.activeCategory || !roots.some((c) => c.categoryId === this.activeCategory?.categoryId)) {
      this.activeCategory = roots[0] ?? null;
    }
  }

  get rootCategories(): Category[] {
    return this.categories.filter((c) => c.parentId === null);
  }

  getChildren(parentId: string): Category[] {
    return this.categories.filter((c) => c.parentId === parentId);
  }

  get activeChildren(): Category[] {
    if (!this.activeCategory) return [];
    return this.getChildren(this.activeCategory.categoryId);
  }

  setActiveCategory(category: Category): void {
    this.activeCategory = category;
  }
}
