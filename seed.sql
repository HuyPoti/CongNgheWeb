-- ==========================================================
-- SEED DATA FOR GEARVN MOCK SYSTEM (10+ ENTRIES PER TABLE)
-- Tables EXCLUDED: categories, banners
-- Uses consistent UUID prefixes and explicit time fields
-- ==========================================================

-- 0. CLEANUP (Except categories and banners)
TRUNCATE TABLE users, brands, products, product_images, 
               addresses, coupons, coupon_usages, orders, order_items, order_status_history, payments, 
               shipments, cart_items, news_categories, news, reviews, review_images, review_helpful_votes, review_replies, 
               return_requests, return_request_items, return_request_images, wishlists, flash_sales, flash_sale_items, 
               activity_logs, refresh_tokens, password_reset_tokens RESTART IDENTITY CASCADE;

-- 1. SEED BRANDS (10)
INSERT INTO brands (brand_id, name, slug, is_active, created_at, updated_at) VALUES 
('bbbbbbbb-bbbb-bbbb-bbbb-000000000001', 'ASUS', 'asus', TRUE, NOW(), NOW()),
('bbbbbbbb-bbbb-bbbb-bbbb-000000000002', 'MSI', 'msi', TRUE, NOW(), NOW()),
('bbbbbbbb-bbbb-bbbb-bbbb-000000000003', 'Gigabyte', 'gigabyte', TRUE, NOW(), NOW()),
('bbbbbbbb-bbbb-bbbb-bbbb-000000000004', 'Intel', 'intel', TRUE, NOW(), NOW()),
('bbbbbbbb-bbbb-bbbb-bbbb-000000000005', 'AMD', 'amd', TRUE, NOW(), NOW()),
('bbbbbbbb-bbbb-bbbb-bbbb-000000000006', 'Samsung', 'samsung', TRUE, NOW(), NOW()),
('bbbbbbbb-bbbb-bbbb-bbbb-000000000007', 'Kingston', 'kingston', TRUE, NOW(), NOW()),
('bbbbbbbb-bbbb-bbbb-bbbb-000000000008', 'Western Digital', 'wd', TRUE, NOW(), NOW()),
('bbbbbbbb-bbbb-bbbb-bbbb-000000000009', 'Corsair', 'corsair', TRUE, NOW(), NOW()),
('bbbbbbbb-bbbb-bbbb-bbbb-000000000010', 'G.Skill', 'gskill', TRUE, NOW(), NOW());

-- 2. SEED USERS (10)
INSERT INTO users (user_id, email, password_hash, full_name, role, is_active, is_email_verified, created_at, updated_at) VALUES 
('aaaaaaaa-aaaa-aaaa-aaaa-000000000001', 'admin@example.com', 'has_admin', 'Admin User', 1, TRUE, TRUE, NOW(), NOW()),
('aaaaaaaa-aaaa-aaaa-aaaa-000000000002', 'staff@example.com', 'has_staff', 'Staff User', 2, TRUE, TRUE, NOW(), NOW()),
('aaaaaaaa-aaaa-aaaa-aaaa-000000000003', 'customer1@gmail.com', 'hash', 'Nguyen Van A', 3, TRUE, TRUE, NOW(), NOW()),
('aaaaaaaa-aaaa-aaaa-aaaa-000000000004', 'customer2@gmail.com', 'hash', 'Tran Van B', 3, TRUE, TRUE, NOW(), NOW()),
('aaaaaaaa-aaaa-aaaa-aaaa-000000000005', 'customer3@gmail.com', 'hash', 'Le Thi C', 3, TRUE, TRUE, NOW(), NOW()),
('aaaaaaaa-aaaa-aaaa-aaaa-000000000006', 'customer4@gmail.com', 'hash', 'Pham Van D', 3, TRUE, TRUE, NOW(), NOW()),
('aaaaaaaa-aaaa-aaaa-aaaa-000000000007', 'customer5@gmail.com', 'hash', 'Hoang Thi E', 3, TRUE, TRUE, NOW(), NOW()),
('aaaaaaaa-aaaa-aaaa-aaaa-000000000008', 'customer6@gmail.com', 'hash', 'Do Van F', 3, TRUE, TRUE, NOW(), NOW()),
('aaaaaaaa-aaaa-aaaa-aaaa-000000000009', 'customer7@gmail.com', 'hash', 'Vu Thi G', 3, TRUE, TRUE, NOW(), NOW()),
('aaaaaaaa-aaaa-aaaa-aaaa-000000000010', 'customer8@gmail.com', 'hash', 'Bui Van H', 3, TRUE, TRUE, NOW(), NOW());

-- 3. SEED PRODUCTS (10)
INSERT INTO products (product_id, category_id, brand_id, name, slug, sku, regular_price, sale_price, stock_quantity, warranty_months, specifications, status, meta_title, meta_description, created_at, updated_at) VALUES 
('cccccccc-cccc-cccc-cccc-000000000001', 'c0000002-0000-0000-0000-000000000001', 'bbbbbbbb-bbbb-bbbb-bbbb-000000000004', 'Intel Core i9-14900K', 'i9-14900k', 'S1', 16000000, 15000000, 50, 36, '{"Socket": "LGA 1700", "Nhân/Luồng": "24C/32T", "Xung nhịp": "3.2GHz - 6.0GHz", "Bộ nhớ đệm": "36MB L3", "TDP": "125W"}', 2, 'Intel Core i9-14900K - GearVN', 'Intel Core i9-14900K thế hệ 14 mạnh mẽ nhất.', NOW(), NOW()),
('cccccccc-cccc-cccc-cccc-000000000002', 'c0000002-0000-0000-0000-000000000001', 'bbbbbbbb-bbbb-bbbb-bbbb-000000000005', 'AMD Ryzen 9 7950X', 'r9-7950x', 'S2', 15000000, 14000000, 40, 36, '{"Socket": "AM5", "Nhân/Luồng": "16C/32T", "Xung nhịp": "4.5GHz - 5.7GHz", "Bộ nhớ đệm": "64MB L3", "TDP": "170W"}', 2, 'AMD Ryzen 9 7950X - GearVN', 'AMD Ryzen 9 7950X hiệu năng cực đỉnh.', NOW(), NOW()),
('cccccccc-cccc-cccc-cccc-000000000003', 'c0000002-0000-0000-0000-000000000001', 'bbbbbbbb-bbbb-bbbb-bbbb-000000000001', 'ASUS ROG RTX 4090 OC', 'rtx-4090-rog', 'S3', 55000000, 52000000, 10, 36, '{"Nhân CUDA": "16384", "Bộ nhớ": "24GB GDDR6X", "Bus": "384-bit", "Nguồn yêu cầu": "850W", "Cổng xuất": "DP, HDMI"}', 2, 'ASUS ROG RTX 4090 OC - Sức mạnh đồ họa', 'ASUS ROG RTX 4090 OC chính hãng GearVN.', NOW(), NOW()),
('cccccccc-cccc-cccc-cccc-000000000004', 'c0000002-0000-0000-0000-000000000001', 'bbbbbbbb-bbbb-bbbb-bbbb-000000000002', 'MSI RTX 4080 Suprim X', 'rtx-4080-msi', 'S4', 35000000, 33000000, 15, 36, '{"Nhân CUDA": "9728", "Bộ nhớ": "16GB GDDR6X", "Bus": "256-bit", "Nguồn yêu cầu": "750W", "Cổng xuất": "DP, HDMI"}', 2, 'MSI RTX 4080 Suprim X - GearVN', 'MSI RTX 4080 Suprim X thiết kế sang trọng.', NOW(), NOW()),
('cccccccc-cccc-cccc-cccc-000000000005', 'c0000002-0000-0000-0000-000000000001', 'bbbbbbbb-bbbb-bbbb-bbbb-000000000009', 'Corsair Dominator 32GB DDR5', 'ddr5-32', 'S5', 4500000, 4200000, 100, 36, '{"Loại": "DDR5", "Dung lượng": "32GB (2x16)", "Bus": "5600MHz", "Độ trễ": "CL36", "Led": "RGB"}', 2, 'Corsair Dominator 32GB DDR5 - RAM cao cấp', 'RAM Corsair Dominator DDR5 hiệu năng cao.', NOW(), NOW()),
('cccccccc-cccc-cccc-cccc-000000000006', 'c0000002-0000-0000-0000-000000000001', 'bbbbbbbb-bbbb-bbbb-bbbb-000000000006', 'Samsung 990 Pro 1TB NVMe', '990-1tb', 'S6', 3500000, 3200000, 80, 60, '{"Chuẩn": "M.2 NVMe Gen4", "Dung lượng": "1TB", "Tốc độ đọc": "7450MB/s", "Tốc độ ghi": "6900MB/s", "Cache": "1GB LPDRR4"}', 2, 'Samsung 990 Pro 1TB NVMe - GearVN', 'SSD Samsung 990 Pro siêu nhanh.', NOW(), NOW()),
('cccccccc-cccc-cccc-cccc-000000000007', 'c0000002-0000-0000-0000-000000000001', 'bbbbbbbb-bbbb-bbbb-bbbb-000000000001', 'Laptop ROG Strix G16', 'g16', 'S7', 45000000, 42000000, 20, 24, '{"Màn hình": "16 inch 165Hz", "CPU": "i7-13650HX", "GPU": "RTX 4060", "RAM": "16GB DDR5", "SSD": "512GB PCIe"}', 2, 'Laptop ROG Strix G16 - GearVN', 'Laptop gaming ROG Strix G16 màn hình 165Hz.', NOW(), NOW()),
('cccccccc-cccc-cccc-cccc-000000000008', 'c0000002-0000-0000-0000-000000000001', 'bbbbbbbb-bbbb-bbbb-bbbb-000000000004', 'Intel Core i7-14700K', 'i7-14700k', 'S8', 11000000, 10500000, 60, 36, '{"Socket": "LGA 1700", "Nhân/Luồng": "20C/28T", "Xung nhịp": "3.4GHz - 5.6GHz", "Bộ nhớ đệm": "33MB L3", "TDP": "125W"}', 2, 'Intel Core i7-14700K - GearVN', 'CPU Intel Core i7-14700K chính hãng.', NOW(), NOW()),
('cccccccc-cccc-cccc-cccc-000000000009', 'c0000002-0000-0000-0000-000000000001', 'bbbbbbbb-bbbb-bbbb-bbbb-000000000010', 'G.Skill Trident Z5 32GB', 'z5-32', 'S9', 4800000, 4500000, 50, 36, '{"Loại": "DDR5", "Dung lượng": "32GB (2x16)", "Bus": "6000MHz", "Độ trễ": "CL30", "Led": "RGB"}', 2, 'G.Skill Trident Z5 32GB - RAM RGB', 'RAM G.Skill Trident Z5 DDR5 LED RGB.', NOW(), NOW()),
('cccccccc-cccc-cccc-cccc-000000000010', 'c0000002-0000-0000-0000-000000000001', 'bbbbbbbb-bbbb-bbbb-bbbb-000000000009', 'Nguồn Corsair RM850x', 'rm850x', 'S10', 3500000, 3200000, 40, 120, '{"Công suất": "850W", "Chứng nhận": "80 Plus Gold", "Kiểu": "Full Modular", "Kích thước": "ATX", "Bảo hành": "10 năm"}', 2, 'Nguồn Corsair RM850x - GearVN', 'Nguồn máy tính Corsair RM850x 80 Plus Gold.', NOW(), NOW());

-- 4. SEED PRODUCT IMAGES (10)
INSERT INTO product_images (image_id, product_id, image_url, is_primary, sort_order, created_at) VALUES 
('dddddddd-dddd-dddd-dddd-000000000001', 'cccccccc-cccc-cccc-cccc-000000000001', 'https://th.bing.com/th/id/OIP.PAQJn4xvCw4t-3PrHKfrHAAAAA?w=195&h=275&c=7&r=0&o=7&dpr=1.3&pid=1.7&rm=3', TRUE, 1, NOW()),
('dddddddd-dddd-dddd-dddd-000000000002', 'cccccccc-cccc-cccc-cccc-000000000002', 'https://th.bing.com/th/id/OIP.PAQJn4xvCw4t-3PrHKfrHAAAAA?w=195&h=275&c=7&r=0&o=7&dpr=1.3&pid=1.7&rm=3', TRUE, 1, NOW()),
('dddddddd-dddd-dddd-dddd-000000000003', 'cccccccc-cccc-cccc-cccc-000000000003', 'https://th.bing.com/th/id/OIP.PAQJn4xvCw4t-3PrHKfrHAAAAA?w=195&h=275&c=7&r=0&o=7&dpr=1.3&pid=1.7&rm=3', TRUE, 1, NOW()),
('dddddddd-dddd-dddd-dddd-000000000004', 'cccccccc-cccc-cccc-cccc-000000000004', 'https://th.bing.com/th/id/OIP.PAQJn4xvCw4t-3PrHKfrHAAAAA?w=195&h=275&c=7&r=0&o=7&dpr=1.3&pid=1.7&rm=3', TRUE, 1, NOW()),
('dddddddd-dddd-dddd-dddd-000000000005', 'cccccccc-cccc-cccc-cccc-000000000005', 'https://th.bing.com/th/id/OIP.PAQJn4xvCw4t-3PrHKfrHAAAAA?w=195&h=275&c=7&r=0&o=7&dpr=1.3&pid=1.7&rm=3', TRUE, 1, NOW()),
('dddddddd-dddd-dddd-dddd-000000000006', 'cccccccc-cccc-cccc-cccc-000000000006', 'https://th.bing.com/th/id/OIP.PAQJn4xvCw4t-3PrHKfrHAAAAA?w=195&h=275&c=7&r=0&o=7&dpr=1.3&pid=1.7&rm=3', TRUE, 1, NOW()),
('dddddddd-dddd-dddd-dddd-000000000007', 'cccccccc-cccc-cccc-cccc-000000000007', 'https://th.bing.com/th/id/OIP.PAQJn4xvCw4t-3PrHKfrHAAAAA?w=195&h=275&c=7&r=0&o=7&dpr=1.3&pid=1.7&rm=3', TRUE, 1, NOW()),
('dddddddd-dddd-dddd-dddd-000000000008', 'cccccccc-cccc-cccc-cccc-000000000008', 'https://th.bing.com/th/id/OIP.PAQJn4xvCw4t-3PrHKfrHAAAAA?w=195&h=275&c=7&r=0&o=7&dpr=1.3&pid=1.7&rm=3', TRUE, 1, NOW()),
('dddddddd-dddd-dddd-dddd-000000000009', 'cccccccc-cccc-cccc-cccc-000000000009', 'https://th.bing.com/th/id/OIP.PAQJn4xvCw4t-3PrHKfrHAAAAA?w=195&h=275&c=7&r=0&o=7&dpr=1.3&pid=1.7&rm=3', TRUE, 1, NOW()),
('dddddddd-dddd-dddd-dddd-000000000010', 'cccccccc-cccc-cccc-cccc-000000000010', 'https://th.bing.com/th/id/OIP.PAQJn4xvCw4t-3PrHKfrHAAAAA?w=195&h=275&c=7&r=0&o=7&dpr=1.3&pid=1.7&rm=3', TRUE, 1, NOW());

-- 5. SEED COUPONS (2)
INSERT INTO coupons (coupon_id, code, description, discount_type, discount_value, min_order_amount, start_date, end_date, is_active) VALUES 
('11111111-1111-1111-1111-000000000001', 'GEARVN100', 'Giảm 100k cho đơn từ 1tr', 'fixed', 100000, 1000000, NOW(), NOW() + INTERVAL '30 days', TRUE),
('11111111-1111-1111-1111-000000000002', 'NEWUSER', 'Giảm 5% cho thành viên mới', 'percent', 5, 0, NOW(), NOW() + INTERVAL '365 days', TRUE);

-- 6. SEED CART ITEMS (10)
INSERT INTO cart_items (user_id, product_id, quantity, added_at) VALUES 
('aaaaaaaa-aaaa-aaaa-aaaa-000000000003', 'cccccccc-cccc-cccc-cccc-000000000001', 1, NOW()),
('aaaaaaaa-aaaa-aaaa-aaaa-000000000003', 'cccccccc-cccc-cccc-cccc-000000000003', 1, NOW()),
('aaaaaaaa-aaaa-aaaa-aaaa-000000000004', 'cccccccc-cccc-cccc-cccc-000000000007', 1, NOW()),
('aaaaaaaa-aaaa-aaaa-aaaa-000000000005', 'cccccccc-cccc-cccc-cccc-000000000005', 2, NOW()),
('aaaaaaaa-aaaa-aaaa-aaaa-000000000006', 'cccccccc-cccc-cccc-cccc-000000000006', 1, NOW()),
('aaaaaaaa-aaaa-aaaa-aaaa-000000000007', 'cccccccc-cccc-cccc-cccc-000000000002', 1, NOW()),
('aaaaaaaa-aaaa-aaaa-aaaa-000000000008', 'cccccccc-cccc-cccc-cccc-000000000004', 1, NOW()),
('aaaaaaaa-aaaa-aaaa-aaaa-000000000009', 'cccccccc-cccc-cccc-cccc-000000000009', 2, NOW()),
('aaaaaaaa-aaaa-aaaa-aaaa-000000000010', 'cccccccc-cccc-cccc-cccc-000000000010', 1, NOW()),
('aaaaaaaa-aaaa-aaaa-aaaa-000000000003', 'cccccccc-cccc-cccc-cccc-000000000008', 1, NOW());

-- 7. SEED ADDRESSES (10)
INSERT INTO addresses (address_id, user_id, recipient_name, phone, address_line, province, district, ward, is_default, created_at) VALUES 
('dddddddd-dddd-dddd-dddd-000000000001', 'aaaaaaaa-aaaa-aaaa-aaaa-000000000003', 'Nguyen Van A', '0912345678', '45 Le Loi', 'HCM', 'Q1', 'Ben Nghe', TRUE, NOW()),
('dddddddd-dddd-dddd-dddd-000000000002', 'aaaaaaaa-aaaa-aaaa-aaaa-000000000004', 'Tran Van B', '0922345678', '123 Nguyen Hue', 'HCM', 'Q1', 'Ben Nghe', TRUE, NOW()),
('dddddddd-dddd-dddd-dddd-000000000003', 'aaaaaaaa-aaaa-aaaa-aaaa-000000000005', 'Le Thi C', '0932345678', '67 Tran Hung Dao', 'Da Nang', 'Hai Chau', 'Thach Thang', TRUE, NOW()),
('dddddddd-dddd-dddd-dddd-000000000004', 'aaaaaaaa-aaaa-aaaa-aaaa-000000000006', 'Pham Van D', '0942345678', '12 Hang Dao', 'Ha Noi', 'Hoan Kiem', 'Hang Dao', TRUE, NOW()),
('dddddddd-dddd-dddd-dddd-000000000005', 'aaaaaaaa-aaaa-aaaa-aaaa-000000000007', 'Hoang Thi E', '0952345678', '89 Xuan Thuy', 'Ha Noi', 'Cau Giay', 'Dich Vong', TRUE, NOW()),
('dddddddd-dddd-dddd-dddd-000000000006', 'aaaaaaaa-aaaa-aaaa-aaaa-000000000008', 'Do Van F', '0962345678', '34 Vo Van Tan', 'HCM', 'Q3', 'P6', TRUE, NOW()),
('dddddddd-dddd-dddd-dddd-000000000007', 'aaaaaaaa-aaaa-aaaa-aaaa-000000000009', 'Vu Thi G', '0972345678', '21 Ly Tu Trong', 'HCM', 'Q1', 'Ben Nghe', TRUE, NOW()),
('dddddddd-dddd-dddd-dddd-000000000008', 'aaaaaaaa-aaaa-aaaa-aaaa-000000000010', 'Bui Van H', '0982345678', '56 Phan Dinh Phung', 'Can Tho', 'Ninh Kieu', 'An Cu', TRUE, NOW()),
('dddddddd-dddd-dddd-dddd-000000000009', 'aaaaaaaa-aaaa-aaaa-aaaa-000000000003', 'Nguyen Van A-2', '0912345678', '78 CMT8', 'HCM', 'Tan Binh', 'P10', FALSE, NOW()),
('dddddddd-dddd-dddd-dddd-000000000010', 'aaaaaaaa-aaaa-aaaa-aaaa-000000000004', 'Tran Van B-2', '0922345678', '90 Dien Bien Phu', 'HCM', 'Binh Thanh', 'P15', FALSE, NOW());

-- 8. SEED ORDERS (10)
INSERT INTO orders (order_id, user_id, order_code, total_amount, status, payment_method, shipping_address_id, shipping_fee, discount_amount, created_at, updated_at) VALUES 
('eeeeeeee-eeee-eeee-eeee-000000000001', 'aaaaaaaa-aaaa-aaaa-aaaa-000000000003', 'ORD-001', 70000000, 5, 'VNPAY', 'dddddddd-dddd-dddd-dddd-000000000001', 30000, 0, NOW(), NOW()),
('eeeeeeee-eeee-eeee-eeee-000000000002', 'aaaaaaaa-aaaa-aaaa-aaaa-000000000004', 'ORD-002', 10500000, 1, 'COD', 'dddddddd-dddd-dddd-dddd-000000000002', 30000, 0, NOW(), NOW()),
('eeeeeeee-eeee-eeee-eeee-000000000003', 'aaaaaaaa-aaaa-aaaa-aaaa-000000000005', 'ORD-003', 42000000, 5, 'BANK', 'dddddddd-dddd-dddd-dddd-000000000003', 0, 0, NOW(), NOW()),
('eeeeeeee-eeee-eeee-eeee-000000000004', 'aaaaaaaa-aaaa-aaaa-aaaa-000000000006', 'ORD-004', 3200000, 4, 'VNPAY', 'dddddddd-dddd-dddd-dddd-000000000004', 30000, 0, NOW(), NOW()),
('eeeeeeee-eeee-eeee-eeee-000000000005', 'aaaaaaaa-aaaa-aaaa-aaaa-000000000007', 'ORD-005', 14000000, 2, 'COD', 'dddddddd-dddd-dddd-dddd-000000000005', 30000, 0, NOW(), NOW()),
('eeeeeeee-eeee-eeee-eeee-000000000006', 'aaaaaaaa-aaaa-aaaa-aaaa-000000000008', 'ORD-006', 33000000, 3, 'BANK', 'dddddddd-dddd-dddd-dddd-000000000006', 0, 0, NOW(), NOW()),
('eeeeeeee-eeee-eeee-eeee-000000000007', 'aaaaaaaa-aaaa-aaaa-aaaa-000000000009', 'ORD-007', 9000000, 5, 'VNPAY', 'dddddddd-dddd-dddd-dddd-000000000007', 0, 0, NOW(), NOW()),
('eeeeeeee-eeee-eeee-eeee-000000000008', 'aaaaaaaa-aaaa-aaaa-aaaa-000000000010', 'ORD-008', 3500000, 5, 'BANK', 'dddddddd-dddd-dddd-dddd-000000000008', 0, 0, NOW(), NOW()),
('eeeeeeee-eeee-eeee-eeee-000000000009', 'aaaaaaaa-aaaa-aaaa-aaaa-000000000003', 'ORD-009', 15000000, 5, 'VNPAY', 'dddddddd-dddd-dddd-dddd-000000000001', 30000, 0, NOW(), NOW()),
('eeeeeeee-eeee-eeee-eeee-000000000010', 'aaaaaaaa-aaaa-aaaa-aaaa-000000000004', 'ORD-010', 4800000, 5, 'COD', 'dddddddd-dddd-dddd-dddd-000000000002', 30000, 0, NOW(), NOW());

-- 9. SEED ORDER ITEMS (10)
INSERT INTO order_items (order_id, product_id, quantity, unit_price) VALUES 
('eeeeeeee-eeee-eeee-eeee-000000000001', 'cccccccc-cccc-cccc-cccc-000000000001', 1, 15000000),
('eeeeeeee-eeee-eeee-eeee-000000000001', 'cccccccc-cccc-cccc-cccc-000000000003', 1, 55000000),
('eeeeeeee-eeee-eeee-eeee-000000000002', 'cccccccc-cccc-cccc-cccc-000000000008', 1, 10500000),
('eeeeeeee-eeee-eeee-eeee-000000000003', 'cccccccc-cccc-cccc-cccc-000000000007', 1, 42000000),
('eeeeeeee-eeee-eeee-eeee-000000000004', 'cccccccc-cccc-cccc-cccc-000000000006', 1, 3200000),
('eeeeeeee-eeee-eeee-eeee-000000000005', 'cccccccc-cccc-cccc-cccc-000000000002', 1, 14000000),
('eeeeeeee-eeee-eeee-eeee-000000000006', 'cccccccc-cccc-cccc-cccc-000000000004', 1, 33000000),
('eeeeeeee-eeee-eeee-eeee-000000000007', 'cccccccc-cccc-cccc-cccc-000000000009', 2, 4500000),
('eeeeeeee-eeee-eeee-eeee-000000000008', 'cccccccc-cccc-cccc-cccc-000000000010', 1, 3500000),
('eeeeeeee-eeee-eeee-eeee-000000000009', 'cccccccc-cccc-cccc-cccc-000000000001', 1, 15000000);

-- 10. SEED PAYMENTS (10)
INSERT INTO payments (payment_id, order_id, amount, payment_method, status, gateway_response, return_url, created_at, updated_at) VALUES 
('ffffffff-ffff-ffff-ffff-000000000001', 'eeeeeeee-eeee-eeee-eeee-000000000001', 70000000, 'VNPAY', 2, '{"vnp_ResponseCode": "00", "vnp_TransactionNo": "123456"}', 'http://localhost:4200/payment/vnpay-return', NOW(), NOW()),
('ffffffff-ffff-ffff-ffff-000000000002', 'eeeeeeee-eeee-eeee-eeee-000000000002', 10500000, 'COD', 1, NULL, NULL, NOW(), NOW());
('ffffffff-ffff-ffff-ffff-000000000003', 'eeeeeeee-eeee-eeee-eeee-000000000003', 42000000, 'BANK', 2, NULL, NULL, NOW(), NOW()),
('ffffffff-ffff-ffff-ffff-000000000004', 'eeeeeeee-eeee-eeee-eeee-000000000004', 3200000, 'VNPAY', 2, '{"vnp_ResponseCode": "00"}', 'http://localhost:4200/payment/vnpay-return', NOW(), NOW()),
('ffffffff-ffff-ffff-ffff-000000000005', 'eeeeeeee-eeee-eeee-eeee-000000000005', 14000000, 'COD', 1, NULL, NULL, NOW(), NOW()),
('ffffffff-ffff-ffff-ffff-000000000006', 'eeeeeeee-eeee-eeee-eeee-000000000006', 33000000, 'BANK', 2, NULL, NULL, NOW(), NOW()),
('ffffffff-ffff-ffff-ffff-000000000007', 'eeeeeeee-eeee-eeee-eeee-000000000007', 9000000, 'VNPAY', 2, '{"vnp_ResponseCode": "00"}', 'http://localhost:4200/payment/vnpay-return', NOW(), NOW()),
('ffffffff-ffff-ffff-ffff-000000000008', 'eeeeeeee-eeee-eeee-eeee-000000000008', 3500000, 'BANK', 2, NULL, NULL, NOW(), NOW()),
('ffffffff-ffff-ffff-ffff-000000000009', 'eeeeeeee-eeee-eeee-eeee-000000000009', 15000000, 'VNPAY', 2, '{"vnp_ResponseCode": "00"}', 'http://localhost:4200/payment/vnpay-return', NOW(), NOW()),
('ffffffff-ffff-ffff-ffff-000000000010', 'eeeeeeee-eeee-eeee-eeee-000000000010', 4800000, 'COD', 2, NULL, NULL, NOW(), NOW());

-- 11. SEED ORDER STATUS HISTORY
INSERT INTO order_status_history (id, order_id, old_status, new_status, changed_by, note) VALUES 
(gen_random_uuid(), 'eeeeeeee-eeee-eeee-eeee-000000000001', 4, 5, 'aaaaaaaa-aaaa-aaaa-aaaa-000000000001', 'Giao hàng thành công'),
(gen_random_uuid(), 'eeeeeeee-eeee-eeee-eeee-000000000002', NULL, 1, 'aaaaaaaa-aaaa-aaaa-aaaa-000000000003', 'Khách đặt hàng mới');

-- 12. SEED SHIPMENTS
INSERT INTO shipments (shipment_id, order_id, carrier, tracking_code, status, packed_by) VALUES 
(gen_random_uuid(), 'eeeeeeee-eeee-eeee-eeee-000000000001', 'GHTK', 'TRACK001', 'delivered', 'aaaaaaaa-aaaa-aaaa-aaaa-000000000001'),
(gen_random_uuid(), 'eeeeeeee-eeee-eeee-eeee-000000000004', 'ViettelPost', 'TRACK004', 'shipping', 'aaaaaaaa-aaaa-aaaa-aaaa-000000000002');

-- 13. SEED WISHLISTS
INSERT INTO wishlists (user_id, product_id) VALUES 
('aaaaaaaa-aaaa-aaaa-aaaa-000000000003', 'd0000000-0000-0000-0000-000000000001'),
('aaaaaaaa-aaaa-aaaa-aaaa-000000000003', 'd0000000-0000-0000-0000-000000000002');

-- 14. SEED FLASH SALES
INSERT INTO flash_sales (flash_sale_id, title, start_time, end_time, created_by) VALUES 
('99999999-9999-9999-9999-000000000001', 'Siêu Sale Đồ Họa', NOW(), NOW() + INTERVAL '1 day', 'aaaaaaaa-aaaa-aaaa-aaaa-000000000001');

INSERT INTO flash_sale_items (flash_sale_id, product_id, flash_price, stock_limit) VALUES 
('99999999-9999-9999-9999-000000000001', 'd0000000-0000-0000-0000-000000000001', 65000000, 5),
('99999999-9999-9999-9999-000000000001', 'd0000000-0000-0000-0000-000000000002', 28000000, 10);

-- 15. SEED ACTIVITY LOGS
INSERT INTO activity_logs (user_id, action, entity_type, entity_id) VALUES 
('aaaaaaaa-aaaa-aaaa-aaaa-000000000001', 'LOGIN', 'User', 'aaaaaaaa-aaaa-aaaa-aaaa-000000000001'),
('aaaaaaaa-aaaa-aaaa-aaaa-000000000003', 'CREATE_ORDER', 'Order', 'eeeeeeee-eeee-eeee-eeee-000000000001');

-- 16. SEED NEWS CATEGORIES (10)
INSERT INTO news_categories (category_id, name, slug, created_at, is_active) VALUES 
('aaaaaaaa-1111-1111-1111-000000000001', 'Tin Công Nghệ', 'tin-cong-nghe', NOW(), TRUE),
('aaaaaaaa-1111-1111-1111-000000000002', 'Đánh Giá Sản Phẩm', 'danh-gia-san-pham', NOW(), TRUE),
('aaaaaaaa-1111-1111-1111-000000000003', 'Thủ Thuật PC', 'thu-thuat-pc', NOW(), TRUE),
('aaaaaaaa-1111-1111-1111-000000000004', 'Tin Khuyến Mãi', 'tin-khuyen-mai', NOW(), TRUE),
('aaaaaaaa-1111-1111-1111-000000000005', 'Gaming News', 'gaming-news', NOW(), TRUE),
('aaaaaaaa-1111-1111-1111-000000000006', 'AI & Future', 'ai-future', NOW(), TRUE),
('aaaaaaaa-1111-1111-1111-000000000007', 'Hardware Guide', 'hardware-guide', NOW(), TRUE),
('aaaaaaaa-1111-1111-1111-000000000008', 'Software News', 'software-news', NOW(), TRUE),
('aaaaaaaa-1111-1111-1111-000000000009', 'E-Sports', 'e-sports', NOW(), TRUE),
('aaaaaaaa-1111-1111-1111-000000000010', 'Build PC Guide', 'build-pc-guide', NOW(), TRUE);

-- 12. SEED NEWS (10)
INSERT INTO news (news_id, title, slug, category_id, author_id, content, created_at, updated_at, is_active, is_published, views) VALUES 
('bbbbbbbb-1111-1111-1111-000000000001', 'Card đồ họa mới của NVIDIA', 'vga-nvidia-news', 'aaaaaaaa-1111-1111-1111-000000000001', 'aaaaaaaa-aaaa-aaaa-aaaa-000000000001', 'Nội dung tin tức...', NOW(), NOW(), TRUE, TRUE, 100),
('bbbbbbbb-1111-1111-1111-000000000002', 'Đánh giá RTX 4090', 'rtx-4090-review', 'aaaaaaaa-1111-1111-1111-000000000002', 'aaaaaaaa-aaaa-aaaa-aaaa-000000000002', 'Nội dung...', NOW(), NOW(), TRUE, TRUE, 200),
('bbbbbbbb-1111-1111-1111-000000000003', 'Cách tối ưu Windows 11', 'optimize-win11', 'aaaaaaaa-1111-1111-1111-000000000003', 'aaaaaaaa-aaaa-aaaa-aaaa-000000000002', 'Nội dung...', NOW(), NOW(), TRUE, TRUE, 300),
('bbbbbbbb-1111-1111-1111-000000000004', 'Siêu sale GearVN 12/12', 'sale-12-12', 'aaaaaaaa-1111-1111-1111-000000000004', 'aaaaaaaa-aaaa-aaaa-aaaa-000000000001', 'Nội dung...', NOW(), NOW(), TRUE, TRUE, 400),
('bbbbbbbb-1111-1111-1111-000000000005', 'GTA VI sẽ ra mắt năm 2025', 'gta-vi-news', 'aaaaaaaa-1111-1111-1111-000000000005', 'aaaaaaaa-aaaa-aaaa-aaaa-000000000001', 'Nội dung...', NOW(), NOW(), TRUE, TRUE, 500),
('bbbbbbbb-1111-1111-1111-000000000006', 'AI đang thay đổi ngành đồ họa', 'ai-graphics', 'aaaaaaaa-1111-1111-1111-000000000006', 'aaaaaaaa-aaaa-aaaa-aaaa-000000000001', 'Nội dung...', NOW(), NOW(), TRUE, TRUE, 600),
('bbbbbbbb-1111-1111-1111-000000000007', 'Chọn nguồn thế nào cho RTX 4080', 'psu-for-4080', 'aaaaaaaa-1111-1111-1111-000000000007', 'aaaaaaaa-aaaa-aaaa-aaaa-000000000001', 'Nội dung...', NOW(), NOW(), TRUE, TRUE, 700),
('bbbbbbbb-1111-1111-1111-000000000008', 'Chrome cập nhật tiết kiệm pin', 'chrome-update', 'aaaaaaaa-1111-1111-1111-000000000008', 'aaaaaaaa-aaaa-aaaa-aaaa-000000000001', 'Nội dung...', NOW(), NOW(), TRUE, TRUE, 800),
('bbbbbbbb-1111-1111-1111-000000000009', 'Chung kết thế giới LOL 2024', 'lol-world-2024', 'aaaaaaaa-1111-1111-1111-000000000009', 'aaaaaaaa-aaaa-aaaa-aaaa-000000000001', 'Nội dung...', NOW(), NOW(), TRUE, TRUE, 900),
('bbbbbbbb-1111-1111-1111-000000000010', 'Dàn PC 50 triệu chạy mượt AAA', 'pc-build-50m', 'aaaaaaaa-1111-1111-1111-000000000010', 'aaaaaaaa-aaaa-aaaa-aaaa-000000000002', 'Nội dung...', NOW(), NOW(), TRUE, TRUE, 1000);

-- 13. SEED REVIEWS (10)
INSERT INTO reviews (review_id, product_id, user_id, rating, comment, created_at, updated_at) VALUES 
('cccccccc-1111-1111-1111-000000000001', 'cccccccc-cccc-cccc-cccc-000000000001', 'aaaaaaaa-aaaa-aaaa-aaaa-000000000003', 5, 'Hiệu năng tuyệt vời, rất đáng mua!', NOW(), NOW()),
('cccccccc-1111-1111-1111-000000000002', 'cccccccc-cccc-cccc-cccc-000000000002', 'aaaaaaaa-aaaa-aaaa-aaaa-000000000004', 4, 'Hơi nóng nhưng mạnh.', NOW(), NOW()),
('cccccccc-1111-1111-1111-000000000003', 'cccccccc-cccc-cccc-cccc-000000000003', 'aaaaaaaa-aaaa-aaaa-aaaa-000000000005', 5, 'Quái vật đồ họa!', NOW(), NOW()),
('cccccccc-1111-1111-1111-000000000004', 'cccccccc-cccc-cccc-cccc-000000000004', 'aaaaaaaa-aaaa-aaaa-aaaa-000000000006', 4, 'Thiết kế đẹp.', NOW(), NOW()),
('cccccccc-1111-1111-1111-000000000005', 'cccccccc-cccc-cccc-cccc-000000000005', 'aaaaaaaa-aaaa-aaaa-aaaa-000000000007', 5, 'Bus cao, ổn định.', NOW(), NOW()),
('cccccccc-1111-1111-1111-000000000006', 'cccccccc-cccc-cccc-cccc-000000000006', 'aaaaaaaa-aaaa-aaaa-aaaa-000000000008', 5, 'SSD siêu nhanh.', NOW(), NOW()),
('cccccccc-1111-1111-1111-000000000007', 'cccccccc-cccc-cccc-cccc-000000000007', 'aaaaaaaa-aaaa-aaaa-aaaa-000000000009', 5, 'Laptop gaming hoàn hảo.', NOW(), NOW()),
('cccccccc-1111-1111-1111-000000000008', 'cccccccc-cccc-cccc-cccc-000000000008', 'aaaaaaaa-aaaa-aaaa-aaaa-000000000010', 4, 'Tốt trong tầm giá.', NOW(), NOW()),
('cccccccc-1111-1111-1111-000000000009', 'cccccccc-cccc-cccc-cccc-000000000009', 'aaaaaaaa-aaaa-aaaa-aaaa-000000000003', 5, 'Led đẹp tuyệt.', NOW(), NOW()),
('cccccccc-1111-1111-1111-000000000010', 'cccccccc-cccc-cccc-cccc-000000000010', 'aaaaaaaa-aaaa-aaaa-aaaa-000000000004', 5, 'Nguồn êm, dây bọc tốt.', NOW(), NOW());

-- 14. SEED REVIEW IMAGES (10)
INSERT INTO review_images (review_id, image_url, created_at) VALUES 
('cccccccc-1111-1111-1111-000000000001', 'img-review-1.jpg', NOW()), 
('cccccccc-1111-1111-1111-000000000003', 'img-review-3.jpg', NOW()),
('cccccccc-1111-1111-1111-000000000007', 'img-review-7.jpg', NOW());
-- Thêm dummy nếu cần đủ 10 dòng
INSERT INTO review_images (review_id, image_url, created_at) VALUES 
('cccccccc-1111-1111-1111-000000000001', 'img-2.jpg', NOW()), ('cccccccc-1111-1111-1111-000000000001', 'img-3.jpg', NOW()),
('cccccccc-1111-1111-1111-000000000001', 'img-4.jpg', NOW()), ('cccccccc-1111-1111-1111-000000000001', 'img-5.jpg', NOW()),
('cccccccc-1111-1111-1111-000000000001', 'img-6.jpg', NOW()), ('cccccccc-1111-1111-1111-000000000001', 'img-7.jpg', NOW()),
('cccccccc-1111-1111-1111-000000000001', 'img-8.jpg', NOW());

-- 15. SEED REVIEW VOTES (10)
INSERT INTO review_helpful_votes (review_id, user_id, created_at) VALUES 
('cccccccc-1111-1111-1111-000000000001', 'aaaaaaaa-aaaa-aaaa-aaaa-000000000004', NOW()),
('cccccccc-1111-1111-1111-000000000001', 'aaaaaaaa-aaaa-aaaa-aaaa-000000000005', NOW()),
('cccccccc-1111-1111-1111-000000000002', 'aaaaaaaa-aaaa-aaaa-aaaa-000000000003', NOW()),
('cccccccc-1111-1111-1111-000000000003', 'aaaaaaaa-aaaa-aaaa-aaaa-000000000006', NOW()),
('cccccccc-1111-1111-1111-000000000004', 'aaaaaaaa-aaaa-aaaa-aaaa-000000000007', NOW()),
('cccccccc-1111-1111-1111-000000000005', 'aaaaaaaa-aaaa-aaaa-aaaa-000000000008', NOW()),
('cccccccc-1111-1111-1111-000000000006', 'aaaaaaaa-aaaa-aaaa-aaaa-000000000009', NOW()),
('cccccccc-1111-1111-1111-000000000007', 'aaaaaaaa-aaaa-aaaa-aaaa-000000000010', NOW()),
('cccccccc-1111-1111-1111-000000000008', 'aaaaaaaa-aaaa-aaaa-aaaa-000000000003', NOW()),
('cccccccc-1111-1111-1111-000000000010', 'aaaaaaaa-aaaa-aaaa-aaaa-000000000005', NOW());

-- 16. SEED REVIEW REPLIES (10)
INSERT INTO review_replies (review_id, user_id, content, created_at, updated_at) VALUES 
('cccccccc-1111-1111-1111-000000000001', 'aaaaaaaa-aaaa-aaaa-aaaa-000000000002', 'Cảm ơn bạn đã ủng hộ GearVN!', NOW(), NOW()),
('cccccccc-1111-1111-1111-000000000002', 'aaaaaaaa-aaaa-aaaa-aaaa-000000000002', 'Chào bạn, dòng này hiệu năng cao nên cần tản tốt ạ.', NOW(), NOW()),
('cccccccc-1111-1111-1111-000000000003', 'aaaaaaaa-aaaa-aaaa-aaaa-000000000002', 'Hàng khủng quá bạn ơi!', NOW(), NOW()),
('cccccccc-1111-1111-1111-000000000004', 'aaaaaaaa-aaaa-aaaa-aaaa-000000000002', 'Dạ cảm ơn bạn.', NOW(), NOW()),
('cccccccc-1111-1111-1111-000000000005', 'aaaaaaaa-aaaa-aaaa-aaaa-000000000002', 'Dạ.', NOW(), NOW()),
('cccccccc-1111-1111-1111-000000000006', 'aaaaaaaa-aaaa-aaaa-aaaa-000000000002', 'Dạ.', NOW(), NOW()),
('cccccccc-1111-1111-1111-000000000007', 'aaaaaaaa-aaaa-aaaa-aaaa-000000000002', 'Dạ.', NOW(), NOW()),
('cccccccc-1111-1111-1111-000000000008', 'aaaaaaaa-aaaa-aaaa-aaaa-000000000002', 'Dạ.', NOW(), NOW()),
('cccccccc-1111-1111-1111-000000000009', 'aaaaaaaa-aaaa-aaaa-aaaa-000000000002', 'Dạ.', NOW(), NOW()),
('cccccccc-1111-1111-1111-000000000010', 'aaaaaaaa-aaaa-aaaa-aaaa-000000000002', 'Dạ.', NOW(), NOW());

-- -- 17. SEED REFRESH TOKENS (10)
-- INSERT INTO refresh_tokens (user_id, token, expires_at, created_at) VALUES 
-- ('aaaaaaaa-aaaa-aaaa-aaaa-000000000001', 'token-admin-1', NOW() + INTERVAL '30 days', NOW()),
-- ('aaaaaaaa-aaaa-aaaa-aaaa-000000000002', 'token-staff-1', NOW() + INTERVAL '30 days', NOW()),
-- ('aaaaaaaa-aaaa-aaaa-aaaa-000000000003', 'token-cus-1', NOW() + INTERVAL '30 days', NOW()),
-- ('aaaaaaaa-aaaa-aaaa-aaaa-000000000004', 'token-cus-2', NOW() + INTERVAL '30 days', NOW()),
-- ('aaaaaaaa-aaaa-aaaa-aaaa-000000000005', 'token-cus-3', NOW() + INTERVAL '30 days', NOW()),
-- ('aaaaaaaa-aaaa-aaaa-aaaa-000000000006', 'token-cus-4', NOW() + INTERVAL '30 days', NOW()),
-- ('aaaaaaaa-aaaa-aaaa-aaaa-000000000007', 'token-cus-5', NOW() + INTERVAL '30 days', NOW()),
-- ('aaaaaaaa-aaaa-aaaa-aaaa-000000000008', 'token-cus-6', NOW() + INTERVAL '30 days', NOW()),
-- ('aaaaaaaa-aaaa-aaaa-aaaa-000000000009', 'token-cus-7', NOW() + INTERVAL '30 days', NOW()),
-- ('aaaaaaaa-aaaa-aaaa-aaaa-000000000010', 'token-cus-8', NOW() + INTERVAL '30 days', NOW());

-- -- 18. SEED PASSWORD RESET TOKENS (10)
-- INSERT INTO password_reset_tokens (user_id, otp_code, expires_at, created_at) VALUES 
-- ('aaaaaaaa-aaaa-aaaa-aaaa-000000000001', '111111', NOW() + INTERVAL '1 hour', NOW()),
-- ('aaaaaaaa-aaaa-aaaa-aaaa-000000000002', '222222', NOW() + INTERVAL '1 hour', NOW()),
-- ('aaaaaaaa-aaaa-aaaa-aaaa-000000000003', '333333', NOW() + INTERVAL '1 hour', NOW()),
-- ('aaaaaaaa-aaaa-aaaa-aaaa-000000000004', '444444', NOW() + INTERVAL '1 hour', NOW()),
-- ('aaaaaaaa-aaaa-aaaa-aaaa-000000000005', '555555', NOW() + INTERVAL '1 hour', NOW()),
-- ('aaaaaaaa-aaaa-aaaa-aaaa-000000000006', '666666', NOW() + INTERVAL '1 hour', NOW()),
-- ('aaaaaaaa-aaaa-aaaa-aaaa-000000000007', '777777', NOW() + INTERVAL '1 hour', NOW()),
-- ('aaaaaaaa-aaaa-aaaa-aaaa-000000000008', '888888', NOW() + INTERVAL '1 hour', NOW()),
-- ('aaaaaaaa-aaaa-aaaa-aaaa-000000000009', '999999', NOW() + INTERVAL '1 hour', NOW()),
-- ('aaaaaaaa-aaaa-aaaa-aaaa-000000000010', '000000', NOW() + INTERVAL '1 hour', NOW());
