import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: 'admin',
    loadComponent: () => import('./layouts/admin-layout/admin-layout').then(m => m.AdminLayout),
    children: [
      {
        path: '',
        loadChildren: () => import('./features/admin/admin.routes').then(m => m.ADMIN_ROUTES)
      }
    ]
  },
  {
    path: '',
    loadComponent: () => import('./layouts/main-layout/main-layout').then(m => m.MainLayout),
    children: [
      {
        path: '',
        loadComponent: () => import('./features/home/home').then(m => m.HomeComponent)
      },
      {
        path: 'tech-news',
        loadComponent: () => import('./features/tech-news/tech-news').then(m => m.TechNews)
      },
      {
        path: 'tech-news/:id',
        loadComponent: () => import('./features/tech-news/news-detail/news-detail').then(m => m.NewsDetail)
      },
      {
        path: 'auth',
        loadChildren: () => import('./features/auth/auth.routes').then(m => m.AUTH_ROUTES)
      },
      {
        path: 'user',
        loadChildren: () => import('./features/user/user.routes').then(m => m.USER_ROUTES)
      },
      {
        path: 'product',
        loadChildren: () => import('./features/product/product.routes').then(m => m.PRODUCT_ROUTES)
      },
      {
        path: 'build-pc',
        loadComponent: () => import('./features/build-pc/build-pc').then(m => m.BuildPc)
      },
      {
        path: 'comparison',
        loadComponent: () => import('./features/comparison/comparison').then(m => m.Comparison)
      },
      {
        path: 'cart',
        loadChildren: () => import('./features/cart/cart.routes').then(m => m.CART_ROUTES)
      }
    ]
  }
];
