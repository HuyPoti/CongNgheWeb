import { Component, Output, EventEmitter } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-search-overlay',
  imports: [FormsModule, RouterLink, CommonModule],
  templateUrl: './search-overlay.html',
  styles: ``,
})
export class SearchOverlay {
  @Output() closeOverlay = new EventEmitter<void>();

  searchQuery = '';
  selectedCategory = 'All';
  categories = ['All', 'GPUs', 'CPUs', 'Motherboards', 'RAM', 'Storage', 'Monitors', 'Peripherals'];

  searchFor(term: string) {
    this.searchQuery = term;
  }

  onSearch() {
    // Placeholder: will integrate with a real search service later
    console.log('Searching for:', this.searchQuery, 'Category:', this.selectedCategory);
  }
}
