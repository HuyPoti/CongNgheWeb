export interface Brand { 
  brandId: string; 
  name: string; 
  slug: string; 
  logoUrl: string | null; 
  description: string | null; 
  isActive: boolean;
  createdAt: string; 
}

export interface CreateBrand { 
  name: string; 
  slug: string; 
  logoUrl?: string; 
  description?: string; 
}

export interface UpdateBrand { 
  name?: string; 
  slug?: string; 
  logoUrl?: string | null; 
  description?: string | null; 
  isActive?: boolean;
}
