export interface UserDto {
  userId: string;
  email: string;
  fullName: string;
  phone?: string;
  role: string;
  isActive: boolean;
  avatarUrl?: string;
}

export interface AuthResponse {
  token: string;
  refreshToken: string;  // ← THÊM
  user: UserDto;
}

export interface LoginDto {
  email: string;
  password: string;
}

export interface RegisterDto {
  email: string;
  password: string;
  fullName: string;
  phone?: string;
}

export interface ForgotPasswordDto{
  email: string;
}

export interface ResetPasswordDto{
  email: string;
  otpCode: string;
  newPassword: string;
}
