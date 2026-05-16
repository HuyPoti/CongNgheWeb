import { inject, PLATFORM_ID } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';
import { ToastService } from '../services/toast.service';

export const roleGuard: CanActivateFn = (route) => {
  const authService = inject(AuthService);
  const router = inject(Router);
  const toast = inject(ToastService);
  const platformId = inject(PLATFORM_ID);

  // 0. Nếu đang ở Server (SSR), cho phép qua để tránh bị redirect nhầm khi F5
  // client-side hydration sẽ chạy lại guard này ở Browser sau đó.
  if (!isPlatformBrowser(platformId)) {
    return true;
  }

  const currentUser = authService.currentUserValue;

  // 1. Kiểm tra xem user đã đăng nhập chưa
  if (!currentUser) {
    toast.error('Vui lòng đăng nhập để truy cập!');
    router.navigate(['/portal']); // Chuyển hướng về trang đăng nhập nội bộ
    return false;
  }

  // Lấy role mong muốn từ data của route (được config trong app.routes.ts)
  const expectedRoles: string[] = route.data['roles'] || [];
  const userRole = currentUser.role.toLowerCase();

  // 2. Nếu route không yêu cầu role cụ thể, cho phép qua
  if (expectedRoles.length === 0) {
    return true;
  }

  // 3. Kiểm tra xem role của user có nằm trong danh sách cho phép không
  if (expectedRoles.includes(userRole)) {
    return true;
  }

  // 4. Nếu không có quyền, đẩy về trang 404 để bảo mật (che giấu sự tồn tại của route)
  // hoặc có thể tạo trang 403 riêng nếu muốn.
  router.navigate(['/404']);
  return false;
};
