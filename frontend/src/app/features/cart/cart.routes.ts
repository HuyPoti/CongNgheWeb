import { Routes } from '@angular/router';

export const CART_ROUTES: Routes = [
  { path: '', loadComponent: () => import('./cart-page/cart-page').then(m => m.CartPage) },
  { path: 'checkout', loadComponent: () => import('./checkout/checkout').then(m => m.Checkout) },
  { path: 'payment', loadComponent: () => import('./payment/payment').then(m => m.Payment) }
];
