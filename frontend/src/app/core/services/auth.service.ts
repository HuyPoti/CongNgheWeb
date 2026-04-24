import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import {
  AuthResponse,
  UserDto,
  LoginDto,
  RegisterDto,
  VerifyEmailDto,
  ResendVerificationDto,
  UpdateProfileDto,
} from '../models/auth.models';
import { BehaviorSubject, Observable, tap } from 'rxjs';
import { ForgotPasswordDto, ResetPasswordDto, ChangePasswordDto } from '../models/auth.models';
import { SocialAuthService } from '@abacritt/angularx-social-login';

@Injectable({
  providedIn: 'root',
})
export class AuthService {
  private http = inject(HttpClient);
  private socialAuthService = inject(SocialAuthService, { optional: true });
  private apiUrl = `${environment.apiUrl}/auth`;
  private profileUrl = `${environment.apiUrl}/profile`;

  private currentUserSubject = new BehaviorSubject<UserDto | null>(this.getUserFromStorage());
  currentUser$ = this.currentUserSubject.asObservable();

  // Login thường
  login(credentials: LoginDto): Observable<AuthResponse> {
    return this.http
      .post<AuthResponse>(`${this.apiUrl}/login`, credentials)
      .pipe(tap((res) => this.setSession(res)));
  }

  // Đăng ký
  register(data: RegisterDto): Observable<AuthResponse> {
    return this.http
      .post<AuthResponse>(`${this.apiUrl}/register`, data)
      .pipe(tap((res) => this.setSession(res)));
  }

  // Google Login - Gọi sang API C#
  googleLogin(idToken: string): Observable<AuthResponse> {
    return this.http
      .post<AuthResponse>(`${this.apiUrl}/google-login`, { idToken })
      .pipe(tap((res) => this.setSession(res)));
  }

  forgotPassword(data: ForgotPasswordDto): Observable<{ message: string }> {
    return this.http.post<{ message: string }>(`${this.apiUrl}/forgot-password`, data);
  }

  resetPassword(data: ResetPasswordDto): Observable<{ message: string }> {
    return this.http.post<{ message: string }>(`${this.apiUrl}/reset-password`, data);
  }

  logout() {
    const refreshToken =
      typeof localStorage !== 'undefined' ? localStorage.getItem('refreshToken') : null;
    // Gọi API thu hồi refresh token phía server
    if (refreshToken) {
      this.http.post(`${this.apiUrl}/logout`, { refreshToken }).subscribe();
    }
    if (typeof localStorage !== 'undefined') {
      localStorage.removeItem('token');
      localStorage.removeItem('refreshToken'); // ← THÊM
      localStorage.removeItem('user');
    }
    this.currentUserSubject.next(null);
    if (this.socialAuthService) {
      try {
        this.socialAuthService
          .signOut()
          .catch((err: unknown) => console.log('Google sign out error', err));
      } catch (e: unknown) {
        console.log('Google sign out error', e);
      }
    }
  }

  getToken(): string | null {
    return typeof localStorage !== 'undefined' ? localStorage.getItem('token') : null;
  }
  isLoggedIn(): boolean {
    return !!this.getToken();
  }
  get currentUserValue(): UserDto | null {
    return this.currentUserSubject.value;
  }

  private setSession(authResult: AuthResponse) {
    if (typeof localStorage !== 'undefined') {
      localStorage.setItem('token', authResult.token);
      localStorage.setItem('refreshToken', authResult.refreshToken); // ← THÊM
      localStorage.setItem('user', JSON.stringify(authResult.user));
    }
    this.currentUserSubject.next(authResult.user);
  }

  private getUserFromStorage(): UserDto | null {
    if (typeof localStorage !== 'undefined') {
      const userStr = localStorage.getItem('user');
      return userStr ? JSON.parse(userStr) : null;
    }
    return null;
  }

  getRefreshToken(): string | null {
    return typeof localStorage !== 'undefined' ? localStorage.getItem('refreshToken') : null;
  }

  refreshAccessToken(): Observable<AuthResponse> {
    const refreshToken = this.getRefreshToken();
    return this.http
      .post<AuthResponse>(`${this.apiUrl}/refresh-token`, { refreshToken })
      .pipe(tap((res) => this.setSession(res)));
  }

  verifyEmail(data: VerifyEmailDto): Observable<{ message: string }> {
    return this.http.post<{ message: string }>(`${this.apiUrl}/verify-email`, data);
  }

  resendVerification(data: ResendVerificationDto): Observable<{ message: string }> {
    return this.http.post<{ message: string }>(`${this.apiUrl}/resend-verification`, data);
  }

  changePassword(data: ChangePasswordDto): Observable<{ message: string }> {
    return this.http.post<{ message: string }>(`${this.apiUrl}/change-password`, data);
  }

  updateProfile(data: UpdateProfileDto): Observable<UserDto> {
    return this.http.put<UserDto>(`${this.profileUrl}`, data).pipe(
      tap((user) => {
        if (typeof localStorage !== 'undefined') {
          localStorage.setItem('user', JSON.stringify(user));
          this.currentUserSubject.next(user);
        }
      }),
    );
  }

  getProfile(): Observable<UserDto> {
    return this.http.get<UserDto>(this.profileUrl);
  }
}
