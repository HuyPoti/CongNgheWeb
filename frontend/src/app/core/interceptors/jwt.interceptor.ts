import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, switchMap, throwError } from 'rxjs';
import { AuthService } from '../services/auth.service';
import { Router } from '@angular/router';

let isRefreshing = false;

export const jwtInterceptor: HttpInterceptorFn = (req, next) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  const token = typeof window !== 'undefined' ? localStorage.getItem('token') : null;

  // Gắn token vào header
  if (token) {
    req = req.clone({
      setHeaders: { Authorization: `Bearer ${token}` },
    });
  }

  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      // Nếu lỗi 401 (token hết hạn) VÀ không phải đang refresh
      if (error.status === 401 && !req.url.includes('/auth/refresh-token') && !isRefreshing) {
        isRefreshing = true;

        return authService.refreshAccessToken().pipe(
          switchMap((res) => {
            isRefreshing = false;
            // Gửi lại request ban đầu với token mới
            const newReq = req.clone({
              setHeaders: { Authorization: `Bearer ${res.token}` },
            });
            return next(newReq);
          }),
          catchError((refreshError) => {
            isRefreshing = false;
            // Refresh cũng lỗi → đăng xuất
            authService.logout();
            router.navigate(['/auth/login']);
            return throwError(() => refreshError);
          })
        );
      }

      return throwError(() => error);
    })
  );
};