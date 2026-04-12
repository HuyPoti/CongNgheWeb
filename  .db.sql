-- ===========================================
-- DATABASE SCHEMA
-- ===========================================

-- 1. Bảng users
-- role: 1=admin, 2=staff, 3=customer
CREATE TABLE users (
    user_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    email VARCHAR(255) UNIQUE NOT NULL,
    password_hash VARCHAR(255) NOT NULL,
    full_name VARCHAR(100) NOT NULL,
    phone VARCHAR(20),
    role INT DEFAULT 3,
    is_active BOOLEAN DEFAULT TRUE,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

-- 2. Bảng categories
CREATE TABLE categories (
    category_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    name VARCHAR(100) NOT NULL,
    slug VARCHAR(100) UNIQUE NOT NULL,
    description TEXT,
    parent_id UUID,
    image_url VARCHAR(255),
    is_active BOOLEAN DEFAULT TRUE,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (parent_id) REFERENCES categories(category_id) ON DELETE SET NULL
);

-- 3. Bảng brands
CREATE TABLE brands (
    brand_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    name VARCHAR(100) NOT NULL,
    slug VARCHAR(100) UNIQUE NOT NULL,
    logo_url VARCHAR(255),
    description TEXT,
    is_active BOOLEAN DEFAULT TRUE,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

-- 4. Bảng products
-- status: 1=draft, 2=published
CREATE TABLE products (
    product_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    category_id UUID NOT NULL,
    brand_id UUID NOT NULL,
    name VARCHAR(255) NOT NULL,
    slug VARCHAR(255) UNIQUE NOT NULL,
    sku VARCHAR(50) UNIQUE,
    regular_price DECIMAL(15,2) NOT NULL,
    sale_price DECIMAL(15,2),
    stock_quantity INT NOT NULL DEFAULT 0,
    warranty_months INT DEFAULT 0,
    description TEXT,
    specifications JSONB,
    status INT DEFAULT 1,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (category_id) REFERENCES categories(category_id),
    FOREIGN KEY (brand_id) REFERENCES brands(brand_id)
);

-- 5. Bảng product_images
CREATE TABLE product_images (
    image_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    product_id UUID NOT NULL,
    image_url VARCHAR(500) NOT NULL,
    is_primary BOOLEAN DEFAULT FALSE,
    sort_order INT DEFAULT 0,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (product_id) REFERENCES products(product_id) ON DELETE CASCADE
);

-- 6. Bảng product_specs
CREATE TABLE product_specs (
    spec_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    product_id UUID NOT NULL,
    spec_key VARCHAR(100) NOT NULL,
    spec_value VARCHAR(500) NOT NULL,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (product_id) REFERENCES products(product_id) ON DELETE CASCADE
);

-- 7. Bảng cart_items
CREATE TABLE cart_items (
    cart_item_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID NOT NULL,
    product_id UUID NOT NULL,
    quantity INT NOT NULL DEFAULT 1,
    added_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (user_id) REFERENCES users(user_id) ON DELETE CASCADE,
    FOREIGN KEY (product_id) REFERENCES products(product_id) ON DELETE CASCADE
);

-- 8. Bảng addresses
CREATE TABLE addresses (
    address_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID NOT NULL,
    recipient_name VARCHAR(100) NOT NULL,
    phone VARCHAR(20) NOT NULL,
    address_line VARCHAR(255) NOT NULL,
    province VARCHAR(100) NOT NULL,
    district VARCHAR(100) NOT NULL,
    ward VARCHAR(100) NOT NULL,
    is_default BOOLEAN DEFAULT FALSE,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (user_id) REFERENCES users(user_id) ON DELETE CASCADE
);

-- 9. Bảng orders
CREATE TABLE orders (
    order_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID NOT NULL,
    order_code VARCHAR(20) UNIQUE NOT NULL,
    total_amount DECIMAL(15,2) NOT NULL,
    status INT DEFAULT 1,
    payment_method VARCHAR(50) NOT NULL,
    payment_status INT DEFAULT 1,
    shipping_address_id UUID NOT NULL,
    notes TEXT,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (user_id) REFERENCES users(user_id),
    FOREIGN KEY (shipping_address_id) REFERENCES addresses(address_id)
);

-- 10. Bảng order_items
CREATE TABLE order_items (
    order_item_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    order_id UUID NOT NULL,
    product_id UUID NOT NULL,
    quantity INT NOT NULL,
    unit_price DECIMAL(15,2) NOT NULL,
    FOREIGN KEY (order_id) REFERENCES orders(order_id) ON DELETE CASCADE,
    FOREIGN KEY (product_id) REFERENCES products(product_id)
);

-- 11. Bảng payments
CREATE TABLE payments (
    payment_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    order_id UUID NOT NULL,
    amount DECIMAL(15,2) NOT NULL,
    payment_method VARCHAR(50) NOT NULL,
    transaction_id VARCHAR(100),
    status INT DEFAULT 1,
    paid_at TIMESTAMP WITH TIME ZONE NULL,
    FOREIGN KEY (order_id) REFERENCES orders(order_id) ON DELETE CASCADE
);

-- 12. Bảng news_categories
CREATE TABLE news_categories (
    category_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    name VARCHAR(100) NOT NULL,
    slug VARCHAR(100) UNIQUE NOT NULL,
    description TEXT,
    parent_id UUID,
    is_active BOOLEAN DEFAULT TRUE,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (parent_id) REFERENCES news_categories(category_id) ON DELETE SET NULL
);

-- 13. Bảng news
CREATE TABLE news (
    news_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    title VARCHAR(255) NOT NULL,
    slug VARCHAR(255) UNIQUE NOT NULL,
    category_id UUID NOT NULL,
    content TEXT NOT NULL,
    excerpt TEXT,
    author_id UUID NOT NULL,
    image_url VARCHAR(500),
    is_published BOOLEAN DEFAULT FALSE,
    published_at TIMESTAMP WITH TIME ZONE NULL,
    views INT DEFAULT 0,
    meta_title VARCHAR(255),
    meta_description VARCHAR(500),
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (category_id) REFERENCES news_categories(category_id),
    FOREIGN KEY (author_id) REFERENCES users(user_id)
);

-- 14. Bảng banners
CREATE TABLE banners (
    banner_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    title VARCHAR(255),
    subtitle VARCHAR(255),
    image_url VARCHAR(500) NOT NULL,
    link_url VARCHAR(500),
    position INT DEFAULT 1,
    sort_order INT DEFAULT 0,
    is_active BOOLEAN DEFAULT TRUE,
    start_date DATE,
    end_date DATE,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

-- 15. Bảng reviews
CREATE TABLE reviews (
    review_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    product_id UUID NOT NULL,
    user_id UUID NOT NULL,
    rating INT NOT NULL CHECK (rating >= 1 AND rating <= 5),
    comment TEXT,
    status INT DEFAULT 2,
    is_verified_purchase BOOLEAN DEFAULT FALSE,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (product_id) REFERENCES products(product_id) ON DELETE CASCADE,
    FOREIGN KEY (user_id) REFERENCES users(user_id) ON DELETE CASCADE
);

-- 16. Bảng review_images
CREATE TABLE review_images (
    image_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    review_id UUID NOT NULL,
    image_url VARCHAR(500) NOT NULL,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (review_id) REFERENCES reviews(review_id) ON DELETE CASCADE
);

-- 17. Bảng review_helpful_votes
CREATE TABLE review_helpful_votes (
    vote_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    review_id UUID NOT NULL,
    user_id UUID NOT NULL,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    UNIQUE(review_id, user_id),
    FOREIGN KEY (review_id) REFERENCES reviews(review_id) ON DELETE CASCADE,
    FOREIGN KEY (user_id) REFERENCES users(user_id) ON DELETE CASCADE
);

-- ===========================================
-- SEED DATA (Dựa trên CreateDtos)
-- ===========================================

-- 1. Users (Email, PasswordHash, FullName, Phone, Role)
INSERT INTO users (user_id, email, password_hash, full_name, phone, role, is_active, created_at, updated_at) VALUES 
('11111111-1111-1111-1111-111111111111', 'admin@gearvn.id.vn', '$2a$11$qRbnM8T8UqjK9qgQcE7Ue.N4FfM3oX5tO1J/j.CgE7n8m7uK/uD8m', 'Huy Poti Admin', '0123456789', 1, TRUE, NOW(), NOW()),
('22222222-2222-2222-2222-222222222222', 'customer@example.com', '$2a$11$qRbnM8T8UqjK9qgQcE7Ue.N4FfM3oX5tO1J/j.CgE7n8m7uK/uD8m', 'Khách hàng Demo', '0987654321', 3, TRUE, NOW(), NOW());

-- 2. Categories (Name, Slug, Description, ParentId, ImageUrl, IsActive)
INSERT INTO categories (category_id, name, slug, description, is_active, created_at, updated_at) VALUES 
('c0000000-0000-0000-0000-000000000001', 'Linh kiện PC', 'linh-kien-pc', 'CPU, RAM, VGA, Mainboard...', TRUE, NOW(), NOW()),
('c0000000-0000-0000-0000-000000000002', 'Laptop Gaming', 'laptop-gaming', 'Laptop cấu hình cao chơi game', TRUE, NOW(), NOW());

-- 3. Brands (Name, Slug, LogoUrl, Description, IsActive)
INSERT INTO brands (brand_id, name, slug, is_active, created_at, updated_at) VALUES 
('b0000000-0000-0000-0000-000000000001', 'ASUS', 'asus', TRUE, NOW(), NOW()),
('b0000000-0000-0000-0000-000000000002', 'MSI', 'msi', TRUE, NOW(), NOW());

-- 4. Products (CategoryId, BrandId, Name, Slug, Sku, RegularPrice, SalePrice, StockQuantity, WarrantyMonths, Description, Specifications, Status)
INSERT INTO products (product_id, category_id, brand_id, name, slug, sku, regular_price, sale_price, stock_quantity, warranty_months, status, created_at, updated_at) VALUES 
('d0000000-0000-0000-0000-000000000001', 'c0000000-0000-0000-0000-000000000001', 'b0000000-0000-0000-0000-000000000001', 'VGA ASUS ROG Strix RTX 4090', 'vga-asus-rog-strix-rtx-4090', 'RTX4090-ROG', 55000000, 52900000, 10, 36, 2, NOW(), NOW()),
('d0000000-0000-0000-0000-000000000002', 'c0000000-0000-0000-0000-000000000002', 'b0000000-0000-0000-0000-000000000002', 'Laptop MSI Katana 15', 'laptop-msi-katana-15', 'MSI-K15', 25000000, 23500000, 5, 24, 2, NOW(), NOW());

-- 5. Product Images (ImageId, ProductId, ImageUrl, IsPrimary, SortOrder, CreatedAt)
INSERT INTO product_images (image_id, product_id, image_url, is_primary, sort_order, created_at) VALUES 
('f0000000-0000-0000-0000-000000000001', 'd0000000-0000-0000-0000-000000000001', 'https://ttgshop.vn/media/product/250_1072100124_dsc09857_copy.jpg', TRUE, 0, NOW()),
('f0000000-0000-0000-0000-000000000002', 'd0000000-0000-0000-0000-000000000002', 'https://ttgshop.vn/media/product/250_11116.jpg', TRUE, 0, NOW());

-- 6. Product Specs (SpecId, ProductId, SpecKey, SpecValue, CreatedAt)
INSERT INTO product_specs (spec_id, product_id, spec_key, spec_value, created_at) VALUES 
('a0000000-0000-0000-0000-000000000001', 'd0000000-0000-0000-0000-000000000001', 'Dung lượng VRAM', '24GB GDDR6X', NOW()),
('a0000000-0000-0000-0000-000000000002', 'd0000000-0000-0000-0000-000000000002', 'CPU', 'Intel Core i7-13620H', NOW());

-- 7. Banners (BannerId, Title, Subtitle, ImageUrl, LinkUrl, Position, SortOrder, IsActive)
INSERT INTO banners (banner_id, title, subtitle, image_url, link_url, position, sort_order, is_active) VALUES 
('e0000000-0000-0000-0000-000000000001', 'Siêu Phẩm RTX 40 Series', 'Sức mạnh đồ họa đỉnh cao', 'https://gearvn.com/cdn/shop/files/pc_gaming_slider_600x480.jpg', '/product/list', 1, 0, TRUE),
('e0000000-0000-0000-0000-000000000002', 'Laptop Gaming Giá Rẻ', 'Ưu đãi cực khủng mùa tựu trường', 'https://phongvu.vn/media/banner/06_Jun607548773950fb28fc586ecc495f5ac2.png', '/product/list', 2, 0, TRUE),
('e0000000-0000-0000-0000-000000000003', 'Màn Hình Đồ Họa', 'Độ chuẩn màu 100% sRGB', 'https://phongvu.vn/media/banner/06_Jun73248386e8ca779430c77431e7f09805.png', '/product/list', 2, 1, TRUE),
('e0000000-0000-0000-0000-000000000004', 'Promo Đặc Biệt', 'Dành riêng cho game thủ', 'https://gearvn.com/cdn/shop/files/Bannaer_Mid_Dashboard.jpg', '/product/list', 2, 2, TRUE);

-- 8. Reviews (ReviewId, ProductId, UserId, Rating, Comment, Status, IsVerifiedPurchase)
INSERT INTO reviews (review_id, product_id, user_id, rating, comment, status, is_verified_purchase) VALUES 
('a0000000-0000-0000-0000-000000000001', 'd0000000-0000-0000-0000-000000000001', '22222222-2222-2222-2222-222222222222', 5, 'Card quá mạnh, render video 4K nhanh như gió!', 2, TRUE);
