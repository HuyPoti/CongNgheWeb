export interface UserDto {
  userId: string;
  email: string;
  fullName: string;
  phone?: string;
  role: string;
  isActive: boolean;
  hasPassword: boolean;
  avatarUrl?: string;
}

export interface AuthResponse {
  token: string;
  refreshToken: string; // ← THÊM
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

export interface ForgotPasswordDto {
  email: string;
}

export interface ResetPasswordDto {
  email: string;
  otpCode: string;
  newPassword: string;
}

export interface VerifyEmailDto {
  email: string;
  otpCode: string;
}

export interface ResendVerificationDto {
  email: string;
}

export interface ChangePasswordDto {
  currentPassword: string;
  newPassword: string;
}

export interface UpdateProfileDto {
  fullName?: string;
  phone?: string;
  avatarUrl?: string;
}
