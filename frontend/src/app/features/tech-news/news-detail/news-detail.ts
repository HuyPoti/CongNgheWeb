import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { NewsService } from '../../../core/services/news.service';
import { News } from '../../../core/models/news.model';

@Component({
  selector: 'app-news-detail',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './news-detail.html',
  styleUrl: './news-detail.css',
})
export class NewsDetail implements OnInit {
  private route = inject(ActivatedRoute);
  private newsService = inject(NewsService);

  protected readonly fallbackImage =
    'https://images.unsplash.com/photo-1518770660439-4636190af475?auto=format&fit=crop&w=1600&q=80';
  protected loading = signal(true);
  protected error = signal('');
  protected article = signal<News | null>(null);

  ngOnInit(): void {
    this.route.paramMap.subscribe((params) => {
      const id = params.get('id');
      if (!id) {
        this.error.set('ID bài viết không hợp lệ.');
        this.loading.set(false);
        return;
      }

      this.fetchArticle(id);
    });
  }

  protected getArticleParagraphs(content: string): string[] {
    return content
      .split(/\r?\n\r?\n/)
      .map((paragraph) => paragraph.trim())
      .filter((paragraph) => paragraph.length > 0);
  }

  protected getDisplayDate(article: News): string {
    const sourceDate = article.publishedAt ?? article.createdAt;
    if (!sourceDate) {
      return '';
    }

    return new Date(sourceDate).toLocaleDateString('vi-VN', {
      day: '2-digit',
      month: '2-digit',
      year: 'numeric',
    });
  }

  private fetchArticle(newsId: string): void {
    this.loading.set(true);
    this.error.set('');

    this.newsService.getNewsById(newsId).subscribe({
      next: (article) => {
        this.article.set(article);
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Không thể tải bài viết hoặc bài viết không tồn tại.');
        this.article.set(null);
        this.loading.set(false);
      },
    });
  }
}
