import { Routes } from '@angular/router';

export const PRODUCT_ROUTES: Routes = [
  { path: 'list', loadComponent: () => import('./product-list/product-list').then(m => m.ProductList) },
  { path: 'build-pc', loadComponent: () => import('./build-pc/build-pc').then(m => m.BuildPc) },
  { path: 'comparison', loadComponent: () => import('./comparison/comparison').then(m => m.Comparison) },
  { path: 'reviews', loadComponent: () => import('./reviews/reviews').then(m => m.Reviews) },
  { path: 'category/:id', loadComponent: () => import('./category/category').then(m => m.Category) },
  { path: ':id', loadComponent: () => import('./product-detail/product-detail').then(m => m.ProductDetail) }
];
