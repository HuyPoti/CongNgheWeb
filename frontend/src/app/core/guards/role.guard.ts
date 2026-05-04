import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';
import { ToastService } from '../services/toast.service';

export const roleGuard: CanActivateFn = (route, state) => {
  const authService = inject(AuthService);
  const router = inject(Router);
  const toast = inject(ToastService);

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

  // 4. Nếu là customer cố tình vào admin/employee, báo lỗi và đẩy ra
  toast.error('Từ chối truy cập! Bạn không có quyền hạn.');
  
  if (userRole === 'customer') {
    router.navigate(['/']); // Đẩy khách hàng về trang chủ
  } else if (userRole === 'staff') {
    router.navigate(['/employee/dashboard']); // Đẩy nhân viên về đúng trang của họ
  } else {
    router.navigate(['/portal']);
  }

  return false;
};
