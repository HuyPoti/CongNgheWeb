import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { environment } from '../../../environments/environment';
import { UserDto } from '../models/auth.models';
import { ApiResponse } from '../models/api-response.model';

// Response tra ve khi upload avatar (co ca user moi)
export interface AvatarUploadResponse {
  imageUrl: string;
  user: UserDto;
}

// Response tra ve khi upload anh chung (chi co URL)
export interface ImageUploadResponse {
  imageUrl: string;
}

@Injectable({
  providedIn: 'root',
})
export class CloudinaryService {
  private http = inject(HttpClient);
  private uploadUrl = `${environment.apiUrl}/uploads`;
  /**
   * Upload anh dai dien (Avatar).
   * Backend se tu ghi de anh cu -> tiet kiem dung luong.
   */
  uploadAvatar(file: File): Observable<AvatarUploadResponse> {
    const formData = new FormData();
    formData.append('file', file);
    return this.http
      .post<ApiResponse<AvatarUploadResponse>>(`${this.uploadUrl}/avatar`, formData)
      .pipe(map((res) => res.data));
  }

  /**
   * Upload anh cho cac muc dich khac (Product, Banner, News,...).
   * @param folder - Ten thu muc: 'products' | 'banners' | 'news' | 'reviews'
   * @param file - File anh can upload
   */
  uploadImage(folder: string, file: File): Observable<ImageUploadResponse> {
    const formData = new FormData();
    formData.append('file', file);
    return this.http
      .post<ApiResponse<ImageUploadResponse>>(`${this.uploadUrl}/${folder}`, formData)
      .pipe(map((res) => res.data));
  }

  /**
   * Xoa anh khoi Cloudinary.
   * @param publicId - ID cong khai cua anh (VD: "products/abc123")
   */
  deleteImage(publicId: string): Observable<{ message: string }> {
    return this.http
      .delete<ApiResponse<{ message: string }>>(this.uploadUrl, {
        params: { publicId },
      })
      .pipe(map((res) => res.data));
  }

  /**
   * HELPER: Validate file truoc khi upload (goi o Frontend de UX nhanh hon).
   * @returns null neu hop le, string loi neu khong hop le
   */
  validateImageFile(file: File, maxSizeMB = 2): string | null {
    const allowedTypes = ['image/jpeg', 'image/png', 'image/webp', 'image/gif'];

    if (!allowedTypes.includes(file.type)) {
      return 'Chi ho tro dinh dang: JPG, PNG, WebP, GIF.';
    }

    const maxSizeBytes = maxSizeMB * 1024 * 1024;
    if (file.size > maxSizeBytes) {
      return `Dung luong anh vuot qua ${maxSizeMB}MB.`;
    }

    return null; // Hop le
  }
}