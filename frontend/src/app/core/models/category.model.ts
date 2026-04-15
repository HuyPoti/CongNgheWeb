export interface Category {
  categoryId: string;           
  name: string;
  slug: string;
  description: string | null;
  parentId: string | null;      
  imageUrl: string | null;
  isActive: boolean;
  createdAt: string;            
  updatedAt: string;            
}
export interface CreateCategory { name: string; slug: string; description?: string; parentId?: string | null; imageUrl?: string; isActive: boolean; }
export interface UpdateCategory { name?: string; slug?: string; description?: string | null; parentId?: string | null; imageUrl?: string | null; isActive?: boolean; }