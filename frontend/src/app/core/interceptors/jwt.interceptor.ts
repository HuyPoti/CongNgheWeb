import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { inject, PLATFORM_ID } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import { catchError, switchMap, throwError, finalize } from 'rxjs';
import { AuthService } from '../services/auth.service';
import { Router } from '@angular/router';

let isRefreshing = false;

export const jwtInterceptor: HttpInterceptorFn = (req, next) => {
  const authService = inject(AuthService);
  const router = inject(Router);
  const platformId = inject(PLATFORM_ID);
  const isBrowser = isPlatformBrowser(platformId);

  const token = isBrowser ? localStorage.getItem('token') : null;

  // Gắn token vào header
  if (token) {
    req = req.clone({
      setHeaders: { Authorization: `Bearer ${token}` },
    });
  }

  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      // Nếu lỗi 401 (token hết hạn) VÀ không phải đang refresh VÀ đang ở Browser
      if (
        isBrowser &&
        error.status === 401 && 
        !req.url.includes('/auth/refresh-token') && 
        !isRefreshing
      ) {
        isRefreshing = true;

        return authService.refreshAccessToken().pipe(
          switchMap((res) => {
            // Gửi lại request ban đầu với token mới
            const newReq = req.clone({
              setHeaders: { Authorization: `Bearer ${res.token}` },
            });
            return next(newReq);
          }),
          catchError((refreshError) => {
            // Refresh cũng lỗi → đăng xuất
            authService.logout();
            router.navigate(['/auth/login']);
            return throwError(() => refreshError);
          }),
          finalize(() => {
            isRefreshing = false;
          })
        );
      }

      return throwError(() => error);
    })
  );
};