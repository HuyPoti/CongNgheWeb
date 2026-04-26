import { Component, Input, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-flash-sale-badge',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="inline-flex items-center gap-2 bg-gradient-to-r from-yellow-400 to-orange-500 text-white px-3 py-1.5 rounded-xl shadow-lg shadow-orange-200 animate-pulse">
      <i class="fas fa-bolt text-xs"></i>
      <span class="text-[10px] font-black uppercase tracking-tighter">Flash Sale</span>
      <div class="w-[1px] h-3 bg-white/30"></div>
      <span class="text-[10px] font-mono font-bold">{{ timeLeft }}</span>
    </div>
  `,
})
export class FlashSaleBadgeComponent implements OnInit, OnDestroy {
  @Input() endTime: Date = new Date(Date.now() + 3600000); // Mặc định 1h nữa
  timeLeft = '00:00:00';
  private timer: ReturnType<typeof setInterval> | undefined;

  ngOnInit() {
    this.startTimer();
  }

  ngOnDestroy() {
    if (this.timer) clearInterval(this.timer);
  }

  private startTimer() {
    this.timer = setInterval(() => {
      const now = new Date().getTime();
      const distance = this.endTime.getTime() - now;

      if (distance < 0) {
        this.timeLeft = '00:00:00';
        clearInterval(this.timer);
        return;
      }

      const hours = Math.floor((distance % (1000 * 60 * 60 * 24)) / (1000 * 60 * 60));
      const minutes = Math.floor((distance % (1000 * 60 * 60)) / (1000 * 60));
      const seconds = Math.floor((distance % (1000 * 60)) / 1000);

      this.timeLeft = `${this.pad(hours)}:${this.pad(minutes)}:${this.pad(seconds)}`;
    }, 1000);
  }

  private pad(num: number) {
    return num < 10 ? '0' + num : num;
  }
}
