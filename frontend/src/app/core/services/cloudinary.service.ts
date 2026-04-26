import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { UserDto } from '../models/auth.models';

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
export class CloudinaryService{
    private http = inject(HttpClient);
    private uploadUrl = `${environment.apiUrl}/uploads`;
    /**
     * Upload anh dai dien (Avatar).
     * Backend se tu ghi de anh cu -> tiet kiem dung luong.
     */
    uploadAvatar(file: File): Observable<AvatarUploadResponse>{
        const formData = new FormData();
        formData.append('file', file);
        return this.http.post<AvatarUploadResponse>(`${this.uploadUrl}/avatar`,formData);
    }

    /**
     * Upload anh cho cac muc dich khac (Product, Banner, News,...).
     * @param folder - Ten thu muc: 'products' | 'banners' | 'news' | 'reviews'
     * @param file - File anh can upload
     */
    uploadImage(folder: string, file: File): Observable<ImageUploadResponse>{
        const formData = new FormData();
        formData.append('file', file);
        return this.http.post<ImageUploadResponse>(
            `${this.uploadUrl}/${folder}`, formData
        );
    }

    /**
     * Xoa anh khoi Cloudinary.
     * @param publicId - ID cong khai cua anh (VD: "products/abc123")
    */
    deleteImage(publicId: string): Observable<{message: string}>{
        return this.http.delete<{message: string}>(this.uploadUrl, {
            params: {publicId},
        });
    }

    /**
     * HELPER: Validate file truoc khi upload (goi o Frontend de UX nhanh hon).
     * @returns null neu hop le, string loi neu khong hop le
     */
    validateImageFile(file: File, maxSizeMB = 2): string | null {
      const allowedTypes = [
        'image/jpeg', 'image/png', 'image/webp', 'image/gif'
      ];

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