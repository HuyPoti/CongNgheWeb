import { Routes } from '@angular/router';

export const USER_ROUTES: Routes = [
  {
    path: '',
    loadComponent: () => import('./user-layout/user-layout').then(m => m.UserLayout),
    children: [
      { path: 'profile', loadComponent: () => import('./profile/profile').then(m => m.Profile) },
      { path: 'orders', loadComponent: () => import('./orders/orders').then(m => m.Orders) },
      { path: 'order-tracking', loadComponent: () => import('./order-tracking/order-tracking').then(m => m.OrderTrackingComponent) },
      { path: 'return-request', loadComponent: () => import('./return-request/return-request').then(m => m.ReturnRequestComponent) },
      { path: 'wishlist', loadComponent: () => import('./wishlist/wishlist').then(m => m.WishlistComponent) },
      { path: 'settings', loadComponent: () => import('./settings/settings').then(m => m.Settings) },
      { path: '', redirectTo: 'profile', pathMatch: 'full' },
    ],
  },
];
