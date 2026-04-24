import { Routes } from '@angular/router';

export const PRODUCT_ROUTES: Routes = [
  { path: 'list', loadComponent: () => import('./product-list/product-list').then(m => m.ProductList) },
  { path: 'category/:id', loadComponent: () => import('./category/category').then(m => m.Category) },
  { path: ':slug', loadComponent: () => import('./product-detail/product-detail').then(m => m.ProductDetail) }
];
