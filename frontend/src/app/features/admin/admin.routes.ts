import { Routes } from '@angular/router';

export const ADMIN_ROUTES: Routes = [
  { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
  { path: 'dashboard', loadComponent: () => import('./dashboard/dashboard').then(m => m.Dashboard) },
  { path: 'manage-product', loadComponent: () => import('./manage-product/manage-product').then(m => m.ManageProduct) },
  { path: 'manage-order', loadComponent: () => import('./manage-order/manage-order').then(m => m.ManageOrder) },
  { path: 'inventory', loadComponent: () => import('./inventory/inventory').then(m => m.Inventory) },
  { path: 'category-hierarchy', loadComponent: () => import('./category-hierarchy/category-hierarchy').then(m => m.CategoryHierarchy) },
  { path: 'customer-crm', loadComponent: () => import('./customer-crm/customer-crm').then(m => m.CustomerCrm) },
  { path: 'employee-management', loadComponent: () => import('./employee-management/employee-management').then(m => m.EmployeeManagement) },
  { path: 'cms-banner', loadComponent: () => import('./cms-banner/cms-banner').then(m => m.CmsBanner) },
  { path: 'brand-management', loadComponent: () => import('./brand-management/brand-management').then(m => m.BrandManagement) },
  { path: 'cms-news', loadComponent: () => import('./cms-news/cms-news').then(m => m.CmsNews) },
  { path: 'reviews', loadComponent: () => import('./reviews/reviews').then(m => m.AdminReviewsComponent) },
  { path: 'coupons', loadComponent: () => import('./coupons/coupon-list').then(m => m.CouponListComponent) },
  { path: 'flash-sales', loadComponent: () => import('./flash-sales/flash-sale-list').then(m => m.FlashSaleListComponent) },
];
