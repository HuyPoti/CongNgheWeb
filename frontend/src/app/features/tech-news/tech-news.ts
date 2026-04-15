import { Component, computed, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { TranslatePipe } from '../../core/pipes/translate.pipe';
import { News, NewsCategory } from '../../core/models/news.model';
import { NewsService } from '../../core/services/news.service';

@Component({
  selector: 'app-tech-news',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink, TranslatePipe],
  templateUrl: './tech-news.html',
  styleUrl: './tech-news.css',
})
export class TechNews implements OnInit {
  private newsService = inject(NewsService);

  protected readonly fallbackImage = 'https://images.unsplash.com/photo-1518770660439-4636190af475?auto=format&fit=crop&w=1600&q=80';
  protected loading = signal(true);
  protected error = signal('');
  protected activeCategory = signal('All');
  protected searchKeyword = signal('');

  private allNews = signal<News[]>([]);
  private categoriesData = signal<NewsCategory[]>([]);

  protected categories = computed(() => [
    'All',
    ...this.categoriesData()
      .filter((cat) => cat.isActive)
      .map((cat) => cat.name),
  ]);

  protected filteredNews = computed(() => {
    const selectedCategory = this.activeCategory();
    const keyword = this.searchKeyword().trim().toLowerCase();

    return this.allNews().filter((news) => {
      const isPublished = news.isPublished;
      const isActive = news.isActive ?? true;
      const categoryName = this.getCategoryName(news).toLowerCase();
      const matchCategory = selectedCategory === 'All' || categoryName === selectedCategory.toLowerCase();
      const matchKeyword =
        keyword.length === 0 ||
        news.title.toLowerCase().includes(keyword) ||
        (news.excerpt ?? '').toLowerCase().includes(keyword);

      return isPublished && isActive && matchCategory && matchKeyword;
    });
  });

  protected featuredNews = computed(() => this.filteredNews()[0] ?? null);
  protected newsList = computed(() => this.filteredNews().slice(1));

  ngOnInit(): void {
    this.loadCategories();
    this.loadNews();
  }

  protected selectCategory(category: string): void {
    this.activeCategory.set(category);
  }

  protected updateSearch(value: string): void {
    this.searchKeyword.set(value);
  }

  protected getCategoryName(news: News): string {
    if (news.categoryName) {
      return news.categoryName;
    }

    const matched = this.categoriesData().find(
      (cat) => cat.categoryId.toLowerCase() === news.categoryId.toLowerCase(),
    );

    return matched?.name ?? 'General';
  }

  protected getDisplayDate(news: News): string {
    const sourceDate = news.publishedAt ?? news.createdAt;
    if (!sourceDate) {
      return '';
    }

    return new Date(sourceDate).toLocaleDateString('vi-VN', {
      day: '2-digit',
      month: '2-digit',
      year: 'numeric',
    });
  }

  private loadNews(): void {
    this.loading.set(true);
    this.error.set('');

    this.newsService.getNews().subscribe({
      next: (news) => {
        const sortedNews = [...news].sort(
          (left, right) =>
            new Date(right.publishedAt ?? right.createdAt).getTime() -
            new Date(left.publishedAt ?? left.createdAt).getTime(),
        );
        this.allNews.set(sortedNews);
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Không thể tải dữ liệu tin tức. Vui lòng thử lại.');
        this.loading.set(false);
      },
    });
  }

  private loadCategories(): void {
    this.newsService.getCategories().subscribe({
      next: (categories) => this.categoriesData.set(categories),
      error: () => {
        this.categoriesData.set([]);
      },
    });
  }
}
