import { Routes } from '@angular/router';
import { authGuard } from '../../core/guards/auth-guard';

export const CART_ROUTES: Routes = [
  { path: '', loadComponent: () => import('./cart-page/cart-page').then(m => m.CartPage) },
  { path: 'checkout', canActivate: [authGuard], loadComponent: () => import('./checkout/checkout').then(m => m.Checkout) },
  { path: 'payment', canActivate: [authGuard], loadComponent: () => import('./payment/payment').then(m => m.Payment) }
];
