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

-- Bảng products
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

CREATE TABLE product_images (
    image_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    product_id UUID NOT NULL,
    image_url VARCHAR(500) NOT NULL,
    is_primary BOOLEAN DEFAULT FALSE,
    sort_order INT DEFAULT 0,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (product_id) REFERENCES products(product_id) ON DELETE CASCADE
);

CREATE TABLE product_specs (
    spec_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    product_id UUID NOT NULL,
    spec_key VARCHAR(100) NOT NULL,
    spec_value VARCHAR(500) NOT NULL,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (product_id) REFERENCES products(product_id) ON DELETE CASCADE
);

CREATE TABLE cart_items (
    cart_item_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID NOT NULL,
    product_id UUID NOT NULL,
    quantity INT NOT NULL DEFAULT 1,
    added_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (user_id) REFERENCES users(user_id) ON DELETE CASCADE,
    FOREIGN KEY (product_id) REFERENCES products(product_id) ON DELETE CASCADE
);

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

-- Bảng orders
-- status: 1=pending, 2=confirmed, 3=processing, 4=shipping, 5=delivered, 6=cancelled
-- payment_status: 1=unpaid, 2=paid, 3=refunded
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

CREATE TABLE order_items (
    order_item_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    order_id UUID NOT NULL,
    product_id UUID NOT NULL,
    quantity INT NOT NULL,
    unit_price DECIMAL(15,2) NOT NULL,
    FOREIGN KEY (order_id) REFERENCES orders(order_id) ON DELETE CASCADE,
    FOREIGN KEY (product_id) REFERENCES products(product_id)
);

-- Bảng payments
-- status: 1=pending, 2=success, 3=failed
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

-- Bảng banners
-- position: 1=homepage_slider, 2=homepage_mid, 3=category_top, 4=news_top
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


-- ===========================================
-- DỮ LIỆU MẪU (SEED DATA)
-- ===========================================

-- 1. users
INSERT INTO users (user_id, email, password_hash, full_name, role) VALUES 
('11111111-1111-1111-1111-111111111111', 'admin@example.com', 'hashed_pw_1', 'Quản trị viên', 1),
('22222222-2222-2222-2222-222222222222', 'staff@example.com', 'hashed_pw_2', 'Nhân viên A', 2),
('33333333-3333-3333-3333-333333333333', 'customer@example.com', 'hashed_pw_3', 'Khách hàng B', 3);

-- 2. categories
INSERT INTO categories (category_id, name, slug) VALUES 
('c0000000-0000-0000-0000-000000000001', 'Điện thoại', 'dien-thoai'),
('c0000000-0000-0000-0000-000000000002', 'Laptop', 'laptop');

-- 3. brands
INSERT INTO brands (brand_id, name, slug) VALUES 
('b0000000-0000-0000-0000-000000000001', 'Apple', 'apple'),
('b0000000-0000-0000-0000-000000000002', 'Samsung', 'samsung');

-- 4. products
INSERT INTO products (product_id, category_id, brand_id, name, slug, sku, regular_price, status) VALUES 
('d0000000-0000-0000-0000-000000000001', 'c0000000-0000-0000-0000-000000000001', 'b0000000-0000-0000-0000-000000000001', 'iPhone 15 Pro Max', 'iphone-15-pro-max', 'IP15PM', 29900000, 2),
('d0000000-0000-0000-0000-000000000002', 'c0000000-0000-0000-0000-000000000002', 'b0000000-0000-0000-0000-000000000001', 'MacBook Pro M2', 'macbook-pro-m2', 'MBPM2', 35000000, 2);

-- 5. product_images
INSERT INTO product_images (product_id, image_url, is_primary) VALUES 
('d0000000-0000-0000-0000-000000000001', 'https://example.com/ip15pm.jpg', TRUE),
('d0000000-0000-0000-0000-000000000002', 'https://example.com/mbpm2.jpg', TRUE);

-- 6. product_specs
INSERT INTO product_specs (product_id, spec_key, spec_value) VALUES 
('d0000000-0000-0000-0000-000000000001', 'RAM', '8GB'),
('d0000000-0000-0000-0000-000000000002', 'RAM', '16GB');

-- 7. cart_items
INSERT INTO cart_items (user_id, product_id, quantity) VALUES 
('33333333-3333-3333-3333-333333333333', 'd0000000-0000-0000-0000-000000000001', 1);

-- 8. addresses
INSERT INTO addresses (address_id, user_id, recipient_name, phone, address_line, province, district, ward) VALUES 
('a0000000-0000-0000-0000-000000000001', '33333333-3333-3333-3333-333333333333', 'Khách hàng B', '0123456789', '123 Đường X', 'TP Hồ Chí Minh', 'Quận 1', 'Phường Bến Nghé');

-- 9. orders
INSERT INTO orders (order_id, user_id, order_code, total_amount, status, payment_method, shipping_address_id) VALUES 
('e0000000-0000-0000-0000-000000000001', '33333333-3333-3333-3333-333333333333', 'ORD001', 29900000, 1, 'COD', 'a0000000-0000-0000-0000-000000000001');

-- 10. order_items
INSERT INTO order_items (order_id, product_id, quantity, unit_price) VALUES 
('e0000000-0000-0000-0000-000000000001', 'd0000000-0000-0000-0000-000000000001', 1, 29900000);

-- 11. payments
INSERT INTO payments (order_id, amount, payment_method, status) VALUES 
('e0000000-0000-0000-0000-000000000001', 29900000, 'COD', 1);

-- 12. news_categories
INSERT INTO news_categories (category_id, name, slug) VALUES 
('f0000000-0000-0000-0000-000000000001', 'Công nghệ', 'cong-nghe');

-- 13. news
INSERT INTO news (title, slug, category_id, content, author_id) VALUES 
('Apple ra mắt iPhone mới', 'apple-ra-mat-iphone-moi', 'f0000000-0000-0000-0000-000000000001', 'Nội dung bài viết...', '11111111-1111-1111-1111-111111111111');

-- 14. banners
INSERT INTO banners (image_url, position) VALUES 
('https://example.com/banner1.jpg', 1);
