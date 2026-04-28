import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-activity-logs',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="p-6">
      <h1 class="text-2xl font-bold mb-4">Activity Logs (Stub)</h1>
      <p class="text-gray-600">This feature is under development.</p>
    </div>
  `
})
export class ActivityLogsComponent {}
