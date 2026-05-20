import { Injectable, signal } from '@angular/core';

export type ConfirmType = 'warning' | 'danger' | 'info';

@Injectable({
  providedIn: 'root',
})
export class ConfirmService {
  isOpen = signal<boolean>(false);
  message = signal<string>('');
  title = signal<string>('');
  type = signal<ConfirmType>('warning');
  
  private resolveFn: ((value: boolean) => void) | null = null;

  confirm(message: string, title = 'Xác nhận', type: ConfirmType = 'warning'): Promise<boolean> {
    this.message.set(message);
    this.title.set(title);
    this.type.set(type);
    this.isOpen.set(true);

    return new Promise<boolean>((resolve) => {
      this.resolveFn = resolve;
    });
  }

  approve() {
    this.isOpen.set(false);
    if (this.resolveFn) {
      this.resolveFn(true);
      this.resolveFn = null;
    }
  }

  decline() {
    this.isOpen.set(false);
    if (this.resolveFn) {
      this.resolveFn(false);
      this.resolveFn = null;
    }
  }
}
