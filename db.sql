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
    meta_title VARCHAR(255),
    meta_description VARCHAR(500),
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

-- 9. Bảng coupons
CREATE TABLE coupons (
    coupon_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    code VARCHAR(50) UNIQUE NOT NULL,
    description TEXT,
    discount_type VARCHAR(20) NOT NULL,
    discount_value DECIMAL(15,2) NOT NULL,
    min_order_amount DECIMAL(15,2) DEFAULT 0,
    max_discount DECIMAL(15,2),
    usage_limit INT,
    used_count INT DEFAULT 0,
    per_user_limit INT DEFAULT 1,
    start_date TIMESTAMP WITH TIME ZONE NOT NULL,
    end_date TIMESTAMP WITH TIME ZONE NOT NULL,
    is_active BOOLEAN DEFAULT TRUE,
    created_by UUID REFERENCES users(user_id),
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

-- 10. Bảng orders
CREATE TABLE orders (
    order_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID NOT NULL,
    order_code VARCHAR(20) UNIQUE NOT NULL,
    total_amount DECIMAL(15,2) NOT NULL,
    status INT DEFAULT 1,
    payment_method VARCHAR(50) NOT NULL,
    payment_status INT DEFAULT 1,
    shipping_address_id UUID NOT NULL,
    shipping_fee DECIMAL(15,2) DEFAULT 0,
    discount_amount DECIMAL(15,2) DEFAULT 0,
    cancelled_reason TEXT,
    cancelled_by UUID,
    coupon_id UUID,
    notes TEXT,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (user_id) REFERENCES users(user_id),
    FOREIGN KEY (shipping_address_id) REFERENCES addresses(address_id),
    FOREIGN KEY (cancelled_by) REFERENCES users(user_id),
    FOREIGN KEY (coupon_id) REFERENCES coupons(coupon_id)
);

-- 11. Bảng order_items
CREATE TABLE order_items (
    order_item_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    order_id UUID NOT NULL,
    product_id UUID NOT NULL,
    quantity INT NOT NULL,
    unit_price DECIMAL(15,2) NOT NULL,
    FOREIGN KEY (order_id) REFERENCES orders(order_id) ON DELETE CASCADE,
    FOREIGN KEY (product_id) REFERENCES products(product_id)
);

-- 12. Bảng payments
CREATE TABLE payments (
    payment_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    order_id UUID NOT NULL,
    amount DECIMAL(15,2) NOT NULL,
    payment_method VARCHAR(50) NOT NULL,
    transaction_id VARCHAR(100),
    status INT DEFAULT 1,
    paid_at TIMESTAMP WITH TIME ZONE NULL,
    gateway_response JSONB,
    return_url VARCHAR(500),
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (order_id) REFERENCES orders(order_id) ON DELETE CASCADE
);

-- 13. Bảng order_status_history
CREATE TABLE order_status_history (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    order_id UUID NOT NULL REFERENCES orders(order_id) ON DELETE CASCADE,
    old_status INT,
    new_status INT NOT NULL,
    changed_by UUID NOT NULL REFERENCES users(user_id),
    note TEXT,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

-- 14. Bảng shipments
CREATE TABLE shipments (
    shipment_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    order_id UUID NOT NULL REFERENCES orders(order_id) ON DELETE CASCADE,
    carrier VARCHAR(50) NOT NULL,
    tracking_code VARCHAR(100),
    shipping_fee DECIMAL(15,2) DEFAULT 0,
    estimated_delivery DATE,
    actual_delivery TIMESTAMP WITH TIME ZONE,
    status VARCHAR(20) DEFAULT 'pending',
    packed_by UUID REFERENCES users(user_id),
    packed_at TIMESTAMP WITH TIME ZONE,
    qc_passed BOOLEAN DEFAULT FALSE,
    qc_notes TEXT,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

-- 15. Bảng return_requests
CREATE TABLE return_requests (
    return_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    order_id UUID NOT NULL REFERENCES orders(order_id),
    user_id UUID NOT NULL REFERENCES users(user_id),
    reason VARCHAR(50) NOT NULL,
    description TEXT,
    status VARCHAR(20) DEFAULT 'pending',
    refund_amount DECIMAL(15,2),
    processed_by UUID REFERENCES users(user_id),
    processed_at TIMESTAMP WITH TIME ZONE,
    admin_note TEXT,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

-- 16. Bảng return_request_items
CREATE TABLE return_request_items (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    return_id UUID NOT NULL REFERENCES return_requests(return_id) ON DELETE CASCADE,
    order_item_id UUID NOT NULL REFERENCES order_items(order_item_id),
    quantity INT NOT NULL,
    reason_detail TEXT
);

-- 17. Bảng return_request_images
CREATE TABLE return_request_images (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    return_id UUID NOT NULL REFERENCES return_requests(return_id) ON DELETE CASCADE,
    image_url VARCHAR(500) NOT NULL,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

-- 18. Bảng wishlists
CREATE TABLE wishlists (
    wishlist_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID NOT NULL REFERENCES users(user_id) ON DELETE CASCADE,
    product_id UUID NOT NULL REFERENCES products(product_id) ON DELETE CASCADE,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    UNIQUE(user_id, product_id)
);

-- 19. Bảng coupon_usages
CREATE TABLE coupon_usages (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    coupon_id UUID NOT NULL REFERENCES coupons(coupon_id),
    user_id UUID NOT NULL REFERENCES users(user_id),
    order_id UUID NOT NULL REFERENCES orders(order_id),
    discount_amount DECIMAL(15,2) NOT NULL,
    used_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

-- 20. Bảng flash_sales
CREATE TABLE flash_sales (
    flash_sale_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    title VARCHAR(255) NOT NULL,
    start_time TIMESTAMP WITH TIME ZONE NOT NULL,
    end_time TIMESTAMP WITH TIME ZONE NOT NULL,
    is_active BOOLEAN DEFAULT TRUE,
    created_by UUID REFERENCES users(user_id),
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

-- 21. Bảng flash_sale_items
CREATE TABLE flash_sale_items (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    flash_sale_id UUID NOT NULL REFERENCES flash_sales(flash_sale_id) ON DELETE CASCADE,
    product_id UUID NOT NULL REFERENCES products(product_id),
    flash_price DECIMAL(15,2) NOT NULL,
    stock_limit INT NOT NULL,
    sold_count INT DEFAULT 0,
    UNIQUE(flash_sale_id, product_id)
);

-- 22. Bảng activity_logs
CREATE TABLE activity_logs (
    log_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID NOT NULL REFERENCES users(user_id),
    action VARCHAR(50) NOT NULL,
    entity_type VARCHAR(50),
    entity_id UUID,
    old_value TEXT,
    new_value TEXT,
    ip_address VARCHAR(45),
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

-- 23. Bảng news_categories
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

-- 24. Bảng news
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

-- 25. Bảng banners
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

-- 26. Bảng reviews
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

-- 27. Bảng review_images
CREATE TABLE review_images (
    image_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    review_id UUID NOT NULL,
    image_url VARCHAR(500) NOT NULL,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (review_id) REFERENCES reviews(review_id) ON DELETE CASCADE
);

-- 28. Bảng review_helpful_votes
CREATE TABLE review_helpful_votes (
    vote_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    review_id UUID NOT NULL,
    user_id UUID NOT NULL,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    UNIQUE(review_id, user_id),
    FOREIGN KEY (review_id) REFERENCES reviews(review_id) ON DELETE CASCADE,
    FOREIGN KEY (user_id) REFERENCES users(user_id) ON DELETE CASCADE
);

-- 29. Bảng review_replies
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

-- 30. Bảng refresh_tokens
CREATE TABLE refresh_tokens (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID NOT NULL,
    token VARCHAR(500) NOT NULL,
    expires_at TIMESTAMP WITH TIME ZONE NOT NULL,
    is_revoked BOOLEAN DEFAULT FALSE,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (user_id) REFERENCES users(user_id) ON DELETE CASCADE
);

-- 31. Bảng password_reset_tokens
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

-- 4. Products
INSERT INTO products (product_id, category_id, brand_id, name, slug, sku, regular_price, sale_price, stock_quantity, warranty_months, specifications, status, meta_title, meta_description, created_at, updated_at) VALUES 
('d0000000-0000-0000-0000-000000000001', 'c0000002-0000-0000-0000-000000000007', 'b0000000-0000-0000-0000-000000000001', 'ASUS ROG Strix RTX 4090 OC', 'vga-asus-rog-strix-rtx-4090-oc', 'RTX4090-ROG-STRIX', 58000000, 55900000, 10, 36, '{"chipset": "RTX 4090", "vram": "24GB GDDR6X", "bus": "384-bit", "core_clock": "2610 MHz", "power": "450W", "slots": "3.5 slot"}', 2, 'ASUS ROG Strix RTX 4090 OC - Card đồ họa mạnh nhất', 'Mua ngay ASUS ROG Strix RTX 4090 OC chính hãng tại GearVN. Hỗ trợ trả góp 0%, bảo hành 36 tháng.', NOW(), NOW()),
('d0000000-0000-0000-0000-000000000002', 'c0000002-0000-0000-0000-000000000007', 'b0000000-0000-0000-0000-000000000002', 'MSI Suprim X RTX 4080 Super', 'vga-msi-suprim-x-rtx-4080-super', 'RTX4080S-MSI-SUPRIM', 35000000, 33500000, 15, 36, '{"chipset": "RTX 4080 Super", "vram": "16GB GDDR6X", "bus": "256-bit", "core_clock": "2640 MHz", "power": "320W", "slots": "3.3 slot"}', 2, 'MSI Suprim X RTX 4080 Super - Hiệu năng cực đỉnh', 'MSI Suprim X RTX 4080 Super thiết kế sang trọng, hiệu năng mạnh mẽ cho game thủ và designer.', NOW(), NOW());

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
