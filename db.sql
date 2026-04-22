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
    is_email_verified BOOLEAN DEFAULT FALSE,
    email_verification_otp VARCHAR(6),
    otp_expires_at TIMESTAMP WITH TIME ZONE,
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
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
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
-- status: 1=draft, 2=published, 3=deleted
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
    is_active BOOLEAN DEFAULT TRUE,
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
    is_active INT DEFAULT 1,
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

-- 18. Bảng review_replies
CREATE TABLE review_replies (
    reply_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    review_id UUID NOT NULL,
    user_id UUID NOT NULL,
    content TEXT NOT NULL,
    is_active INT DEFAULT 1,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (review_id) REFERENCES reviews(review_id) ON DELETE CASCADE,
    FOREIGN KEY (user_id) REFERENCES users(user_id) ON DELETE CASCADE
);

-- 19. Bảng refresh_tokens
CREATE TABLE refresh_tokens (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID NOT NULL,
    token VARCHAR(500) NOT NULL,
    expires_at TIMESTAMP WITH TIME ZONE NOT NULL,
    is_revoked BOOLEAN DEFAULT FALSE,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (user_id) REFERENCES users(user_id) ON DELETE CASCADE
);

-- 20. Bảng password_reset_tokens
CREATE TABLE password_reset_tokens (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID NOT NULL,
    otp_code VARCHAR(6) NOT NULL,
    expires_at TIMESTAMP WITH TIME ZONE NOT NULL,
    is_used BOOLEAN DEFAULT FALSE,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (user_id) REFERENCES users(user_id) ON DELETE CASCADE
);


-- ===========================================
-- SEED DATA
-- ===========================================

-- 1. Users
INSERT INTO users (user_id, email, password_hash, full_name, phone, role, is_active, created_at, updated_at) VALUES 
('11111111-1111-1111-1111-111111111111', 'admin@gearvn.id.vn', '$2a$11$qRbnM8T8UqjK9qgQcE7Ue.N4FfM3oX5tO1J/j.CgE7n8m7uK/uD8m', 'Huy Poti Admin', '0123456789', 1, TRUE, NOW(), NOW()),
('22222222-2222-2222-2222-222222222222', 'customer@example.com', '$2a$11$qRbnM8T8UqjK9qgQcE7Ue.N4FfM3oX5tO1J/j.CgE7n8m7uK/uD8m', 'Khách hàng Demo', '0987654321', 3, TRUE, NOW(), NOW());

-- 2. Brands
INSERT INTO brands (brand_id, name, slug, is_active, created_at, updated_at) VALUES 
('b0000000-0000-0000-0000-000000000001', 'ASUS', 'asus', TRUE, NOW(), NOW()),
('b0000000-0000-0000-0000-000000000002', 'MSI', 'msi', TRUE, NOW(), NOW()),
('b0000000-0000-0000-0000-000000000003', 'Gigabyte', 'gigabyte', TRUE, NOW(), NOW()),
('b0000000-0000-0000-0000-000000000004', 'NVIDIA', 'nvidia', TRUE, NOW(), NOW()),
('b0000000-0000-0000-0000-000000000005', 'AMD', 'amd', TRUE, NOW(), NOW());

-- 3. Categories
-- Parent Categories
INSERT INTO categories (category_id, name, slug, parent_id, is_active, created_at) VALUES 
('c0000001-0000-0000-0000-000000000001', 'CPU', 'cpu', NULL, TRUE, NOW()),
('c0000001-0000-0000-0000-000000000002', 'Mainboard', 'mainboard', NULL, TRUE, NOW()),
('c0000001-0000-0000-0000-000000000003', 'RAM', 'ram', NULL, TRUE, NOW()),
('c0000001-0000-0000-0000-000000000004', 'GPU (Card đồ họa)', 'gpu', NULL, TRUE, NOW()),
('c0000001-0000-0000-0000-000000000005', 'SSD', 'ssd', NULL, TRUE, NOW()),
('c0000001-0000-0000-0000-000000000006', 'HDD', 'hdd', NULL, TRUE, NOW()),
('c0000001-0000-0000-0000-000000000007', 'PSU (Nguồn)', 'psu', NULL, TRUE, NOW()),
('c0000001-0000-0000-0000-000000000008', 'Laptop Gaming', 'laptop-gaming', NULL, TRUE, NOW());

-- Child Categories
INSERT INTO categories (category_id, name, slug, parent_id, is_active, created_at) VALUES 
('c0000002-0000-0000-0000-000000000001', 'Intel', 'cpu-intel', 'c0000001-0000-0000-0000-000000000001', TRUE, NOW()),
('c0000002-0000-0000-0000-000000000002', 'AMD', 'cpu-amd', 'c0000001-0000-0000-0000-000000000001', TRUE, NOW()),
('c0000002-0000-0000-0000-000000000007', 'NVIDIA', 'gpu-nvidia', 'c0000001-0000-0000-0000-000000000004', TRUE, NOW()),
('c0000002-0000-0000-0000-000000000008', 'AMD', 'gpu-amd', 'c0000001-0000-0000-0000-000000000004', TRUE, NOW());

-- 4. Products (Dành cho Compare Page)
-- Sản phẩm 1: RTX 4090
INSERT INTO products (product_id, category_id, brand_id, name, slug, sku, regular_price, sale_price, stock_quantity, warranty_months, specifications, status, created_at, updated_at) VALUES 
('d0000000-0000-0000-0000-000000000001', 'c0000002-0000-0000-0000-000000000007', 'b0000000-0000-0000-0000-000000000001', 'ASUS ROG Strix RTX 4090 OC', 'vga-asus-rog-strix-rtx-4090-oc', 'RTX4090-ROG-STRIX', 58000000, 55900000, 10, 36, '{"chipset": "RTX 4090", "vram": "24GB GDDR6X", "bus": "384-bit", "core_clock": "2610 MHz", "power": "450W", "slots": "3.5 slot"}', 2, NOW(), NOW());

-- Sản phẩm 2: RTX 4080 Super
INSERT INTO products (product_id, category_id, brand_id, name, slug, sku, regular_price, sale_price, stock_quantity, warranty_months, specifications, status, created_at, updated_at) VALUES 
('d0000000-0000-0000-0000-000000000002', 'c0000002-0000-0000-0000-000000000007', 'b0000000-0000-0000-0000-000000000002', 'MSI Suprim X RTX 4080 Super', 'vga-msi-suprim-x-rtx-4080-super', 'RTX4080S-MSI-SUPRIM', 35000000, 33500000, 15, 36, '{"chipset": "RTX 4080 Super", "vram": "16GB GDDR6X", "bus": "256-bit", "core_clock": "2640 MHz", "power": "320W", "slots": "3.3 slot"}', 2, NOW(), NOW());

-- Sản phẩm 3: RTX 4070 Ti Super
INSERT INTO products (product_id, category_id, brand_id, name, slug, sku, regular_price, sale_price, stock_quantity, warranty_months, specifications, status, created_at, updated_at) VALUES 
('d0000000-0000-0000-0000-000000000003', 'c0000002-0000-0000-0000-000000000007', 'b0000000-0000-0000-0000-000000000003', 'Gigabyte AORUS Master RTX 4070 Ti Super', 'vga-gigabyte-aorus-master-rtx-4070-ti-super', 'RTX4070TIS-AORUS', 26000000, 24900000, 20, 36, '{"chipset": "RTX 4070 Ti Super", "vram": "16GB GDDR6X", "bus": "256-bit", "core_clock": "2670 MHz", "power": "285W", "slots": "3.0 slot"}', 2, NOW(), NOW());

-- Sản phẩm 4: RX 7900 XTX
INSERT INTO products (product_id, category_id, brand_id, name, slug, sku, regular_price, sale_price, stock_quantity, warranty_months, specifications, status, created_at, updated_at) VALUES 
('d0000000-0000-0000-0000-000000000004', 'c0000002-0000-0000-0000-000000000008', 'b0000000-0000-0000-0000-000000000001', 'ASUS TUF Gaming RX 7900 XTX OC', 'vga-asus-tuf-rx-7900-xtx-oc', 'RX7900XTX-ASUS-TUF', 32000000, 29900000, 8, 36, '{"chipset": "RX 7900 XTX", "vram": "24GB GDDR6", "bus": "384-bit", "core_clock": "2615 MHz", "power": "355W", "slots": "3.6 slot"}', 2, NOW(), NOW());

-- 5. Product Specs (Chi tiết để so sánh)
INSERT INTO product_specs (spec_id, product_id, spec_key, spec_value) VALUES 
-- RTX 4090
(gen_random_uuid(), 'd0000000-0000-0000-0000-000000000001', 'Nhân CUDA', '16384'),
(gen_random_uuid(), 'd0000000-0000-0000-0000-000000000001', 'Xung nhịp tối đa', '2610 MHz'),
(gen_random_uuid(), 'd0000000-0000-0000-0000-000000000001', 'Bộ nhớ', '24GB GDDR6X'),
(gen_random_uuid(), 'd0000000-0000-0000-0000-000000000001', 'Tiêu thụ điện', '450W'),
-- RTX 4080 Super
(gen_random_uuid(), 'd0000000-0000-0000-0000-000000000002', 'Nhân CUDA', '10240'),
(gen_random_uuid(), 'd0000000-0000-0000-0000-000000000002', 'Xung nhịp tối đa', '2640 MHz'),
(gen_random_uuid(), 'd0000000-0000-0000-0000-000000000002', 'Bộ nhớ', '16GB GDDR6X'),
(gen_random_uuid(), 'd0000000-0000-0000-0000-000000000002', 'Tiêu thụ điện', '320W'),
-- RTX 4070 Ti Super
(gen_random_uuid(), 'd0000000-0000-0000-0000-000000000003', 'Nhân CUDA', '8448'),
(gen_random_uuid(), 'd0000000-0000-0000-0000-000000000003', 'Xung nhịp tối đa', '2670 MHz'),
(gen_random_uuid(), 'd0000000-0000-0000-0000-000000000003', 'Bộ nhớ', '16GB GDDR6X'),
(gen_random_uuid(), 'd0000000-0000-0000-0000-000000000003', 'Tiêu thụ điện', '285W'),
-- RX 7900 XTX
(gen_random_uuid(), 'd0000000-0000-0000-0000-000000000004', 'Nhân Stream', '6144'),
(gen_random_uuid(), 'd0000000-0000-0000-0000-000000000004', 'Xung nhịp tối đa', '2615 MHz'),
(gen_random_uuid(), 'd0000000-0000-0000-0000-000000000004', 'Bộ nhớ', '24GB GDDR6'),
(gen_random_uuid(), 'd0000000-0000-0000-0000-000000000004', 'Tiêu thụ điện', '355W');

-- 6. Product Images
INSERT INTO product_images (product_id, image_url, is_primary, sort_order) VALUES 
('d0000000-0000-0000-0000-000000000001', 'https://dlcdnwebimgs.asus.com/gain/392DE691-8AF2-4FD0-8914-996489370894', TRUE, 0),
('d0000000-0000-0000-0000-000000000002', 'https://vga.msi.com/cdn/shop/files/suprim-x-4080-super.png', TRUE, 0),
('d0000000-0000-0000-0000-000000000003', 'https://static.gigabyte.com/StaticFile/Image/Global/e3124888be6ec9f9e1e1a8a2d1d2d2d2/Product/37841', TRUE, 0),
('d0000000-0000-0000-0000-000000000004', 'https://dlcdnwebimgs.asus.com/gain/452DE691-8AF2-4FD0-8914-996489370894', TRUE, 0);

-- 7. Reviews
INSERT INTO reviews (review_id, product_id, user_id, rating, comment, is_active, is_verified_purchase) VALUES 
('a0000000-0000-0000-0000-000000000001', 'd0000000-0000-0000-0000-000000000001', '22222222-2222-2222-2222-222222222222', 5, 'Card quá mạnh, render video 4K nhanh như gió!', 1, TRUE);

-- 8. Banner
INSERT INTO banners (banner_id, title, subtitle, image_url, link_url, position, sort_order, is_active) VALUES 
('e0000000-0000-0000-0000-000000000001', 'Siêu Phẩm RTX 40 Series', 'Sức mạnh đồ họa đỉnh cao', 'https://gearvn.com/cdn/shop/files/pc_gaming_slider_600x480.jpg', '/product/list', 1, 0, TRUE);
