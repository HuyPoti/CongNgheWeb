import { Component, signal, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ToastService } from '../../../core/services/toast.service';
import { NewsService } from '../../../core/services/news.service';
import { News, NewsCategory, CreateNews, UpdateNews, CreateNewsCategory, UpdateNewsCategory } from '../../../core/models/news.model';

@Component({
  selector: 'app-cms-news',
  imports: [CommonModule, FormsModule],
  templateUrl: './cms-news.html',
  styles: ``,
})
export class CmsNews implements OnInit {
  private toast = inject(ToastService);
  private newsService = inject(NewsService);

  activeTab = signal<'articles' | 'categories'>('articles');
  
  newsCategories = signal<NewsCategory[]>([]);
  newsList = signal<News[]>([]);
  
  showModal = signal(false);
  editingNews = signal<News | null>(null);
  form: Partial<News> = {};

  showCatModal = signal(false);
  editingCat = signal<NewsCategory | null>(null);
  catForm: Partial<NewsCategory> = {};

  ngOnInit() {
    this.loadData();
  }

  getCategoryName(id: string) { 
    if (!id) return 'N/A';
    const targetId = id.toLowerCase();
    return this.newsCategories().find(c => c.categoryId.toLowerCase() === targetId)?.name || 'N/A'; 
  }

  loadData() {
    this.newsService.getCategories().subscribe({
      next: (data) => {
        // Chuẩn hóa ID về chữ thường để so sánh chính xác
        const normalized = data.map(c => ({ ...c, categoryId: c.categoryId.toLowerCase() }));
        this.newsCategories.set(normalized);
      },
      error: () => this.toast.error('Lỗi tải danh mục tin')
    });

    this.newsService.getNews().subscribe({
      next: (data) => this.newsList.set(data),
      error: () => this.toast.error('Lỗi tải danh sách bài viết')
    });
  }

  // === UI Helpers cho Danh mục ===
  openCreateCat() {
    this.editingCat.set(null);
    this.catForm = { name: '', slug: '', description: '', parentId: null, isActive: true };
    this.showCatModal.set(true);
  }

  openEditCat(cat: NewsCategory) {
    this.editingCat.set(cat);
    this.catForm = { ...cat };
    this.showCatModal.set(true);
  }

  closeCatModal() { this.showCatModal.set(false); }

  autoCatSlug() {
    if (this.catForm.name) {
      this.catForm.slug = this.catForm.name.toLowerCase().replace(/\s+/g, '-').replace(/[^a-z0-9-]/g, '');
    }
  }

  saveCat() {
    if (!this.catForm.name?.trim() || !this.catForm.slug?.trim()) {
      this.toast.warning('Vui lòng nhập tên và slug danh mục tin');
      return;
    }

    if (this.editingCat()) {
      const id = this.editingCat()!.categoryId;
      const updateData: UpdateNewsCategory = {
        name: this.catForm.name,
        slug: this.catForm.slug,
        description: this.catForm.description,
        parentId: this.catForm.parentId,
        isActive: this.catForm.isActive
      };
      
      this.newsService.updateCategory(id, updateData).subscribe({
        next: () => {
          this.toast.success('Cập nhật danh mục tin thành công');
          this.loadData();
          this.closeCatModal();
        },
        error: () => this.toast.error('Lỗi khi cập nhật danh mục')
      });
    } else {
      const createData: CreateNewsCategory = {
        name: this.catForm.name,
        slug: this.catForm.slug,
        description: this.catForm.description,
        parentId: this.catForm.parentId,
        isActive: this.catForm.isActive ?? true
      };

      this.newsService.createCategory(createData).subscribe({
        next: () => {
          this.toast.success('Thêm danh mục tin mới thành công');
          this.loadData();
          this.closeCatModal();
        },
        error: () => this.toast.error('Lỗi khi thêm danh mục')
      });
    }
  }

  deleteCat(cat: NewsCategory) {
    if (confirm(`Xóa danh mục tin "${cat.name}"?`)) {
      this.newsService.deleteCategory(cat.categoryId).subscribe({
        next: () => {
          this.toast.success('Đã xóa danh mục tin');
          this.loadData();
        },
        error: () => this.toast.error('Lỗi khi xóa danh mục')
      });
    }
  }

  // === UI Helpers cho Bài viết ===
  openCreateArticle() {
    this.editingNews.set(null);
    this.form = {
      title: '', slug: '', categoryId: '', content: '', excerpt: '',
      imageUrl: '', isPublished: false, metaTitle: '', metaDescription: ''
    };
    this.showModal.set(true);
  }

  openEditArticle(news: News) {
    this.editingNews.set(news);
    // Đảm bảo categoryId được gán vào form là chữ thường để khớp với danh mục đã chuẩn hóa
    this.form = { 
      ...news,
      categoryId: news.categoryId?.toLowerCase()
    };
    this.showModal.set(true);
  }

  closeModal() { this.showModal.set(false); }

  autoSlug() {
    if (this.form.title) {
      this.form.slug = this.form.title.toLowerCase().replace(/\s+/g, '-').replace(/[^a-z0-9-]/g, '');
    }
  }

  saveArticle() {
    if (!this.form.title?.trim() || !this.form.slug?.trim() || !this.form.categoryId) {
      this.toast.warning('Vui lòng nhập đầy đủ Tiêu đề, Slug và Danh mục');
      return;
    }

    if (this.editingNews()) {
      const id = this.editingNews()!.newId;
      const updateData: UpdateNews = {
        title: this.form.title,
        slug: this.form.slug,
        categoryId: this.form.categoryId,
        content: this.form.content ?? '',
        excerpt: this.form.excerpt,
        imageUrl: this.form.imageUrl,
        isPublished: this.form.isPublished,
        metaTitle: this.form.metaTitle,
        metaDescription: this.form.metaDescription
      };

      this.newsService.updateNews(id, updateData).subscribe({
        next: () => {
          this.toast.success('Cập nhật bài viết thành công');
          this.loadData();
          this.closeModal();
        },
        error: () => this.toast.error('Lỗi khi cập nhật bài viết')
      });
    } else {
      const createData: CreateNews = {
        title: this.form.title,
        slug: this.form.slug,
        categoryId: this.form.categoryId,
        content: this.form.content ?? '',
        excerpt: this.form.excerpt,
        imageUrl: this.form.imageUrl,
        isPublished: this.form.isPublished ?? false,
        authorId: '11111111-1111-1111-1111-111111111111',
        metaTitle: this.form.metaTitle,
        metaDescription: this.form.metaDescription
      };

      this.newsService.createNews(createData).subscribe({
        next: () => {
          this.toast.success('Thêm bài viết mới thành công');
          this.loadData();
          this.closeModal();
        },
        error: () => this.toast.error('Lỗi khi tạo bài viết')
      });
    }
  }

  deleteArticle(news: News) {
    if (confirm(`Xóa bài viết "${news.title}"?`)) {
      this.newsService.deleteNews(news.newId).subscribe({
        next: () => {
          this.toast.success('Đã xóa bài viết');
          this.loadData();
        },
        error: () => this.toast.error('Lỗi khi xóa bài viết')
      });
    }
  }

  togglePublish(news: News) {
    const updateData: UpdateNews = { isPublished: !news.isPublished };
    
    this.newsService.updateNews(news.newId, updateData).subscribe({
      next: () => {
        this.toast.info(updateData.isPublished ? 'Đã xuất bản bài viết' : 'Đã gỡ bài viết');
        this.loadData();
      },
      error: () => this.toast.error('Lỗi đổi trạng thái xuất bản')
    });
  }

}