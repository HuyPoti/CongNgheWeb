import { Routes } from '@angular/router';

export const EMPLOYEE_ROUTES: Routes = [
  { path: '', redirectTo: 'orders', pathMatch: 'full' },
  {
    path: 'orders',
    loadComponent: () => import('./emp-orders/emp-orders').then((m) => m.EmpOrders),
  },
  {
    path: 'products',
    loadComponent: () => import('./emp-products/emp-products').then((m) => m.EmpProducts),
  },
  {
    path: 'reviews',
    loadComponent: () => import('./emp-reviews/emp-reviews').then((m) => m.EmpReviews),
  },
  {
    path: 'customers',
    loadComponent: () => import('./emp-customers/emp-customers').then((m) => m.EmpCustomers),
  },
  {
    path: 'packing-slip',
    loadComponent: () => import('./packing-slip/packing-slip.component').then((m) => m.PackingSlipComponent),
  },
];
