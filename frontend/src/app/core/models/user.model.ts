// Model định nghĩa kiểu dữ liệu User ở frontend
// Khớp với UserDto từ backend
export interface User {
  userId: string;       // ← camelCase (Angular convention)
  email: string;
  fullName: string;
  phone: string | null;
  role: 'customer' | 'admin' | 'staff';
  createdAt: string;    // ← Nhận dạng ISO string từ API
  updatedAt: string;
}
export interface CreateUser {
  email: string;
  password: string;
  fullName: string;
  phone?: string;
  role: string;
}
export interface UpdateUser {
  email?: string;
  fullName?: string;
  phone?: string;
  role?: string;
}