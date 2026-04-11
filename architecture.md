# Project Architecture

## 📂 Folder Structure

### 🌐 Frontend (Angular)
- `src/app/core/models`: TypeScript interfaces for domain entities.
- `src/app/core/services`: Services for API interactions and global logic.
- `src/app/features/admin`: Administration modules (CMS).
  - `brand-management`: UI for managing brands (not implemented in this task but present).
  - `cms-banner`: UI for managing banners (current focus).
  - `cms-news`: UI for managing news articles and categories.
  - `manage-product`: UI for managing products.

### ⚙️ Backend (.NET Core)
- `Controllers`: API endpoints (REST).
- `Data`: DB Context and configurations.
- `DTOs`: Data Transfer Objects for requests/promises.
- `Models`: Entity Framework models.
- `Services`: Business logic layer.
- `UnitOfWork`: Repository and Unit of Work patterns.
- `MapperProfiles`: AutoMapper configurations.

## 🛠️ Components & Services

### Banner Management
- **Component**: `CmsBanner` (`frontend/src/app/features/admin/cms-banner`)
- **Service**: `BannerService` (`frontend/src/app/core/services/banner.service.ts`)
- **Model**: `Banner`, `CreateBanner`, `UpdateBanner` (`frontend/src/app/core/models/banner.model.ts`)
- **API**: `BannersController` (`backend/Controllers/BannersController.cs`)

### News Management
- **Component**: `CmsNews` (`frontend/src/app/features/admin/cms-news`)
- **Service**: `NewsService` (`frontend/src/app/core/services/news.service.ts`)
- **Model**: `News`, `NewsCategory` (`frontend/src/app/core/models/news.model.ts`)
- **API**: `NewsController`, `NewsCategoriesController`

## 🔄 Data Flow
1. Frontend Component calls Service.
2. Service makes HTTP request to Backend Controller.
3. Controller calls Business Service.
4. Business Service uses UnitOfWork/Repository to interact with Database.
5. Result is mapped to DTO and returned to Frontend.
