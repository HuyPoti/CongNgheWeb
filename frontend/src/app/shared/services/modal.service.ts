import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';

export interface ModalConfig {
  title: string;
  data?: any;
  width?: string;
}

@Injectable({
  providedIn: 'root'
})
export class ModalService {
  private modalState$ = new BehaviorSubject<{ isOpen: boolean; config?: ModalConfig }>({ isOpen: false });
  
  modalState = this.modalState$.asObservable();

  open(config: ModalConfig) {
    this.modalState$.next({ isOpen: true, config });
  }

  close() {
    this.modalState$.next({ isOpen: false });
  }
}
