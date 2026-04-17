import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { AuthResponse, UserDto, LoginDto, RegisterDto } from '../models/auth.models';
import { BehaviorSubject, Observable, tap } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class AuthService {
  private http = inject(HttpClient);
  private apiUrl = `${environment.apiUrl}/auth`;

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

  logout() {
    if (typeof localStorage !== 'undefined') {
      localStorage.removeItem('token');
      localStorage.removeItem('user');
    }
    this.currentUserSubject.next(null);
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
}
