import { Component, signal } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-checkout',
  imports: [CommonModule],
  templateUrl: './checkout.html',
  styleUrl: './checkout.css',
})
export class Checkout {
  currentStep = signal(1);

  nextStep() {
    this.currentStep.update(s => s < 3 ? s + 1 : s);
    window.scrollTo({ top: 0, behavior: 'smooth' });
  }

  setStep(step: number) {
    if (step < this.currentStep()) {
      this.currentStep.set(step);
    }
  }
}
