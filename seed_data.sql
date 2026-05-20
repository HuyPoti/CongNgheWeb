-- ===========================================
-- CLEANUP SECTION
-- Xóa dữ liệu seed cũ từ db.sql và các lần chạy trước
-- để đảm bảo idempotent execution.
-- Thứ tự DELETE: bảng con trước, bảng cha sau.
-- ===========================================

-- Level 1: Bảng leaf (không có bảng con phụ thuộc)
DELETE FROM inventory_transactions;
DELETE FROM return_request_images;
DELETE FROM return_request_items;
DELETE FROM review_replies;
DELETE FROM review_helpful_votes;
DELETE FROM review_images;
DELETE FROM flash_sale_items;
DELETE FROM coupon_usages;
DELETE FROM wishlists;
DELETE FROM cart_items;
DELETE FROM order_status_history;
DELETE FROM payments;
DELETE FROM order_items;
DELETE FROM shipments;

-- Level 2: Bảng phụ thuộc Level 1
DELETE FROM return_requests;
DELETE FROM reviews;
DELETE FROM flash_sales;
DELETE FROM inventory_receipt_items;
DELETE FROM inventory_receipts;

-- Level 3: Bảng phụ thuộc Level 2
DELETE FROM orders;
DELETE FROM product_images;
DELETE FROM news;

-- Level 4: Bảng phụ thuộc Level 3
DELETE FROM products;
DELETE FROM addresses;
DELETE FROM coupons;
DELETE FROM banners;

-- Level 5: Bảng self-ref hoặc không FK
DELETE FROM categories;
DELETE FROM news_categories;
DELETE FROM brands;
DELETE FROM suppliers;

-- Level 6: Bảng gốc
DELETE FROM users;

-- ===========================================
-- SEED DATA - Cong Nghe Web
-- Computer hardware & accessories
-- Version: 1.0
-- ===========================================
-- NOTE: Excludes tables: activity_logs, password_reset_tokens, refresh_tokens
-- UUID convention: 000000TT-0000-0000-0000-0000000000XX
--   TT = table number (hex)
--   XX = record number (hex)
-- All passwords for test accounts: 123456
-- ===========================================

-- ===========================================
-- 1. USERS (10 rows)
-- role: 1=admin, 2=staff, 3=customer
-- ===========================================
INSERT INTO users (user_id, email, password_hash, full_name, phone, avatar_url, role, is_active, is_email_verified, created_at, updated_at) VALUES
('00000001-0000-0000-0000-000000000001', 'admin@gearvn.id.vn', '$2a$11$qRbnM8T8UqjK9qgQcE7Ue.N4FfM3oX5tO1J/j.CgE7n8m7uK/uD8m', 'Nguyễn Văn Admin', '0901000001', NULL, 1, TRUE, TRUE, NOW(), NOW()),
('00000001-0000-0000-0000-000000000002', 'staff1@gearvn.id.vn', '$2a$11$qRbnM8T8UqjK9qgQcE7Ue.N4FfM3oX5tO1J/j.CgE7n8m7uK/uD8m', 'Trần Thị Nhân Viên', '0901000002', NULL, 2, TRUE, TRUE, NOW(), NOW()),
('00000001-0000-0000-0000-000000000003', 'staff2@gearvn.id.vn', '$2a$11$qRbnM8T8UqjK9qgQcE7Ue.N4FfM3oX5tO1J/j.CgE7n8m7uK/uD8m', 'Lê Văn Staff', '0901000003', NULL, 2, TRUE, TRUE, NOW(), NOW()),
('00000001-0000-0000-0000-000000000004', 'customer1@example.com', '$2a$11$qRbnM8T8UqjK9qgQcE7Ue.N4FfM3oX5tO1J/j.CgE7n8m7uK/uD8m', 'Phạm Minh Tuấn', '0901000004', NULL, 3, TRUE, TRUE, NOW(), NOW()),
('00000001-0000-0000-0000-000000000005', 'customer2@example.com', '$2a$11$qRbnM8T8UqjK9qgQcE7Ue.N4FfM3oX5tO1J/j.CgE7n8m7uK/uD8m', 'Hoàng Thị Lan', '0901000005', NULL, 3, TRUE, FALSE, NOW(), NOW()),
('00000001-0000-0000-0000-000000000006', 'customer3@example.com', '$2a$11$qRbnM8T8UqjK9qgQcE7Ue.N4FfM3oX5tO1J/j.CgE7n8m7uK/uD8m', 'Đặng Quốc Huy', '0901000006', NULL, 3, TRUE, TRUE, NOW(), NOW()),
('00000001-0000-0000-0000-000000000007', 'customer4@example.com', '$2a$11$qRbnM8T8UqjK9qgQcE7Ue.N4FfM3oX5tO1J/j.CgE7n8m7uK/uD8m', 'Vũ Thị Hương', '0901000007', NULL, 3, TRUE, TRUE, NOW(), NOW()),
('00000001-0000-0000-0000-000000000008', 'customer5@example.com', '$2a$11$qRbnM8T8UqjK9qgQcE7Ue.N4FfM3oX5tO1J/j.CgE7n8m7uK/uD8m', 'Ngô Văn Phúc', '0901000008', NULL, 3, TRUE, FALSE, NOW(), NOW()),
('00000001-0000-0000-0000-000000000009', 'customer6@example.com', '$2a$11$qRbnM8T8UqjK9qgQcE7Ue.N4FfM3oX5tO1J/j.CgE7n8m7uK/uD8m', 'Bùi Thanh Mai', '0901000009', NULL, 3, TRUE, TRUE, NOW(), NOW()),
('00000001-0000-0000-0000-00000000000a', 'customer7@example.com', '$2a$11$qRbnM8T8UqjK9qgQcE7Ue.N4FfM3oX5tO1J/j.CgE7n8m7uK/uD8m', 'Đỗ Minh Hoàng', '0901000010', NULL, 3, TRUE, TRUE, NOW(), NOW()) ON CONFLICT (email) DO NOTHING;

-- ===========================================
-- 2. BRANDS (16 rows)
-- ===========================================
INSERT INTO brands (brand_id, name, slug, logo_url, description, is_active, created_at, updated_at) VALUES
('00000002-0000-0000-0000-000000000001', 'Intel', 'intel', 'https://example.com/logos/intel.png', 'Intel Corporation - Nhà sản xuất CPU hàng đầu thế giới', TRUE, NOW(), NOW()),
('00000002-0000-0000-0000-000000000002', 'AMD', 'amd', 'https://example.com/logos/amd.png', 'Advanced Micro Devices - CPU & GPU', TRUE, NOW(), NOW()),
('00000002-0000-0000-0000-000000000003', 'NVIDIA', 'nvidia', 'https://example.com/logos/nvidia.png', 'NVIDIA Corporation - GPU & AI', TRUE, NOW(), NOW()),
('00000002-0000-0000-0000-000000000004', 'ASUS', 'asus', 'https://example.com/logos/asus.png', 'ASUS - Mainboard, VGA, Laptop', TRUE, NOW(), NOW()),
('00000002-0000-0000-0000-000000000005', 'MSI', 'msi', 'https://example.com/logos/msi.png', 'Micro-Star International - Mainboard, VGA, Laptop', TRUE, NOW(), NOW()),
('00000002-0000-0000-0000-000000000006', 'Gigabyte', 'gigabyte', 'https://example.com/logos/gigabyte.png', 'GIGABYTE Technology - Mainboard, VGA', TRUE, NOW(), NOW()),
('00000002-0000-0000-0000-000000000007', 'Samsung', 'samsung', 'https://example.com/logos/samsung.png', 'Samsung Electronics - SSD, RAM, Monitor', TRUE, NOW(), NOW()),
('00000002-0000-0000-0000-000000000008', 'Kingston', 'kingston', 'https://example.com/logos/kingston.png', 'Kingston Technology - RAM, SSD', TRUE, NOW(), NOW()),
('00000002-0000-0000-0000-000000000009', 'Corsair', 'corsair', 'https://example.com/logos/corsair.png', 'Corsair Components - RAM, PSU, Case, Cooler', TRUE, NOW(), NOW()),
('00000002-0000-0000-0000-00000000000a', 'Western Digital', 'western-digital', 'https://example.com/logos/wd.png', 'Western Digital - HDD, SSD', TRUE, NOW(), NOW()),
('00000002-0000-0000-0000-00000000000b', 'Seagate', 'seagate', 'https://example.com/logos/seagate.png', 'Seagate Technology - HDD, SSD', TRUE, NOW(), NOW()),
('00000002-0000-0000-0000-00000000000c', 'Cooler Master', 'cooler-master', 'https://example.com/logos/coolermaster.png', 'Cooler Master - Case, PSU, Cooling', TRUE, NOW(), NOW()),
('00000002-0000-0000-0000-00000000000d', 'Noctua', 'noctua', 'https://example.com/logos/noctua.png', 'Noctua - Quạt & tản nhiệt cao cấp', TRUE, NOW(), NOW()),
('00000002-0000-0000-0000-00000000000e', 'NZXT', 'nzxt', 'https://example.com/logos/nzxt.png', 'NZXT - Case, Cooling, PSU', TRUE, NOW(), NOW()),
('00000002-0000-0000-0000-00000000000f', 'Logitech', 'logitech', 'https://example.com/logos/logitech.png', 'Logitech - Chuột, Bàn phím, Tai nghe', TRUE, NOW(), NOW()),
('00000002-0000-0000-0000-000000000010', 'Razer', 'razer', 'https://example.com/logos/razer.png', 'Razer - Gaming Gear cao cấp', TRUE, NOW(), NOW()) ON CONFLICT (slug) DO NOTHING;

-- ===========================================
-- 3. CATEGORIES (22 rows - 12 parents + 10 children)
-- ===========================================
INSERT INTO categories (category_id, name, slug, description, parent_id, image_url, is_active, created_at, updated_at) VALUES
('00000003-0000-0000-0000-000000000001', 'CPU', 'cpu', 'Bộ vi xử lý trung tâm - Intel, AMD', NULL, NULL, TRUE, NOW(), NOW()),
('00000003-0000-0000-0000-000000000002', 'Mainboard', 'mainboard', 'Bo mạch chủ - Intel, AMD', NULL, NULL, TRUE, NOW(), NOW()),
('00000003-0000-0000-0000-000000000003', 'RAM', 'ram', 'Bộ nhớ trong - DDR4, DDR5', NULL, NULL, TRUE, NOW(), NOW()),
('00000003-0000-0000-0000-000000000004', 'VGA', 'vga', 'Card đồ họa rời - NVIDIA, AMD', NULL, NULL, TRUE, NOW(), NOW()),
('00000003-0000-0000-0000-000000000005', 'SSD', 'ssd', 'Ổ cứng thể rắn - NVMe, SATA', NULL, NULL, TRUE, NOW(), NOW()),
('00000003-0000-0000-0000-000000000006', 'HDD', 'hdd', 'Ổ cứng cơ - 2.5 inch, 3.5 inch', NULL, NULL, TRUE, NOW(), NOW()),
('00000003-0000-0000-0000-000000000007', 'PSU', 'psu', 'Nguồn máy tính', NULL, NULL, TRUE, NOW(), NOW()),
('00000003-0000-0000-0000-000000000008', 'PC Case', 'pc-case', 'Vỏ máy tính', NULL, NULL, TRUE, NOW(), NOW()),
('00000003-0000-0000-0000-000000000009', 'Cooling', 'cooling', 'Tản nhiệt - Air Cooler, AIO', NULL, NULL, TRUE, NOW(), NOW()),
('00000003-0000-0000-0000-00000000000a', 'Monitor', 'monitor', 'Màn hình máy tính', NULL, NULL, TRUE, NOW(), NOW()),
('00000003-0000-0000-0000-00000000000b', 'Keyboard', 'keyboard', 'Bàn phím - Cơ,薄膜', NULL, NULL, TRUE, NOW(), NOW()),
('00000003-0000-0000-0000-00000000000c', 'Mouse', 'mouse', 'Chuột máy tính - Gaming, Văn phòng', NULL, NULL, TRUE, NOW(), NOW()) ON CONFLICT (slug) DO NOTHING;

-- Child categories
INSERT INTO categories (category_id, name, slug, description, parent_id, image_url, is_active, created_at, updated_at) VALUES
('00000003-0000-0000-0000-00000000000d', 'CPU Intel', 'cpu-intel', 'CPU Intel Core i3/i5/i7/i9', '00000003-0000-0000-0000-000000000001', NULL, TRUE, NOW(), NOW()),
('00000003-0000-0000-0000-00000000000e', 'CPU AMD', 'cpu-amd', 'CPU AMD Ryzen 3/5/7/9', '00000003-0000-0000-0000-000000000001', NULL, TRUE, NOW(), NOW()),
('00000003-0000-0000-0000-00000000000f', 'Mainboard Intel', 'mainboard-intel', 'Mainboard chipset Intel', '00000003-0000-0000-0000-000000000002', NULL, TRUE, NOW(), NOW()),
('00000003-0000-0000-0000-000000000010', 'Mainboard AMD', 'mainboard-amd', 'Mainboard chipset AMD', '00000003-0000-0000-0000-000000000002', NULL, TRUE, NOW(), NOW()),
('00000003-0000-0000-0000-000000000011', 'RAM DDR4', 'ram-ddr4', 'RAM DDR4 2400-4000MHz', '00000003-0000-0000-0000-000000000003', NULL, TRUE, NOW(), NOW()),
('00000003-0000-0000-0000-000000000012', 'RAM DDR5', 'ram-ddr5', 'RAM DDR5 4800-8000MHz', '00000003-0000-0000-0000-000000000003', NULL, TRUE, NOW(), NOW()),
('00000003-0000-0000-0000-000000000013', 'VGA NVIDIA', 'vga-nvidia', 'Card đồ họa NVIDIA GeForce', '00000003-0000-0000-0000-000000000004', NULL, TRUE, NOW(), NOW()),
('00000003-0000-0000-0000-000000000014', 'VGA AMD', 'vga-amd', 'Card đồ họa AMD Radeon', '00000003-0000-0000-0000-000000000004', NULL, TRUE, NOW(), NOW()),
('00000003-0000-0000-0000-000000000015', 'SSD NVMe', 'ssd-nvme', 'SSD NVMe PCIe 3.0/4.0/5.0', '00000003-0000-0000-0000-000000000005', NULL, TRUE, NOW(), NOW()),
('00000003-0000-0000-0000-000000000016', 'SSD SATA', 'ssd-sata', 'SSD SATA III 2.5 inch', '00000003-0000-0000-0000-000000000005', NULL, TRUE, NOW(), NOW()) ON CONFLICT (slug) DO NOTHING;

-- ===========================================
-- 4. PRODUCTS (15 rows - computer hardware)
-- ===========================================
INSERT INTO products (product_id, category_id, brand_id, name, slug, sku, regular_price, sale_price, stock_quantity, warranty_months, description, specifications, status, meta_title, meta_description, created_at, updated_at) VALUES
('00000004-0000-0000-0000-000000000001',
 '00000003-0000-0000-0000-00000000000d', '00000002-0000-0000-0000-000000000001',
 'Intel Core i9-14900K', 'intel-core-i9-14900k', 'CPU-I9-14900K',
 12990000, 11990000, 25, 36,
 'Intel Core i9-14900K thế hệ Raptor Lake Refresh, 24 nhân 32 luồng, xung nhịp lên đến 6.0GHz, hỗ trợ DDR5 và PCIe 5.0.',
 '{"cores": "24 (8P + 16E)", "threads": "32", "base_clock": "3.2 GHz", "boost_clock": "6.0 GHz", "cache": "36MB L3", "tdp": "125W (253W max)", "socket": "LGA 1700", "memory_type": "DDR5-5600 / DDR4-3200"}',
 2,
 'Intel Core i9-14900K chính hãng | GearVN',
 'Intel Core i9-14900K - CPU mạnh nhất cho gaming và workstation. Bảo hành 36 tháng.',
 NOW(), NOW()),

('00000004-0000-0000-0000-000000000002',
 '00000003-0000-0000-0000-00000000000e', '00000002-0000-0000-0000-000000000002',
 'AMD Ryzen 9 7950X', 'amd-ryzen-9-7950x', 'CPU-R9-7950X',
 14490000, 12990000, 20, 36,
 'AMD Ryzen 9 7950X thế hệ Ryzen 7000 Zen 4, 16 nhân 32 luồng, xung nhịp lên đến 5.7GHz, hỗ trợ DDR5 và PCIe 5.0.',
 '{"cores": "16", "threads": "32", "base_clock": "4.5 GHz", "boost_clock": "5.7 GHz", "cache": "64MB L3 + 16MB L2", "tdp": "170W", "socket": "AM5", "memory_type": "DDR5-5200"}',
 2,
 'AMD Ryzen 9 7950X chính hãng | GearVN',
 'AMD Ryzen 9 7950X - CPU AMD mạnh nhất, 16 nhân, bảo hành 36 tháng.',
 NOW(), NOW()),

('00000004-0000-0000-0000-000000000003',
 '00000003-0000-0000-0000-00000000000f', '00000002-0000-0000-0000-000000000004',
 'ASUS ROG Strix Z790-E Gaming WiFi', 'asus-rog-strix-z790-e-gaming-wifi', 'MB-ASUS-Z790E',
 8990000, 8490000, 15, 36,
 'ASUS ROG Strix Z790-E Gaming WiFi - Mainboard Intel Z790 cao cấp, hỗ trợ DDR5, PCIe 5.0, WiFi 6E.',
 '{"chipset": "Intel Z790", "socket": "LGA 1700", "form_factor": "ATX", "memory_slots": "4x DDR5", "max_memory": "128GB", "pcie_x16": "2x PCIe 5.0 x16", "m2_slots": "5x M.2", "wifi": "WiFi 6E", "lan": "2.5Gb Ethernet"}',
 2,
 'ASUS ROG Strix Z790-E Gaming WiFi | GearVN',
 'Mainboard ASUS ROG Strix Z790-E Gaming WiFi chính hãng, bảo hành 36 tháng.',
 NOW(), NOW()),

('00000004-0000-0000-0000-000000000004',
 '00000003-0000-0000-0000-000000000010', '00000002-0000-0000-0000-000000000005',
 'MSI MAG B650 Tomahawk WiFi', 'msi-mag-b650-tomahawk-wifi', 'MB-MSI-B650',
 5590000, 5190000, 18, 36,
 'MSI MAG B650 Tomahawk WiFi - Mainboard AMD B650 tầm trung, hỗ trợ DDR5, PCIe 4.0, WiFi 6E.',
 '{"chipset": "AMD B650", "socket": "AM5", "form_factor": "ATX", "memory_slots": "4x DDR5", "max_memory": "192GB", "pcie_x16": "1x PCIe 4.0 x16", "m2_slots": "3x M.2", "wifi": "WiFi 6E", "lan": "2.5Gb Ethernet"}',
 2,
 'MSI MAG B650 Tomahawk WiFi | GearVN',
 'Mainboard MSI MAG B650 Tomahawk WiFi chính hãng, bảo hành 36 tháng.',
 NOW(), NOW()),

('00000004-0000-0000-0000-000000000005',
 '00000003-0000-0000-0000-000000000012', '00000002-0000-0000-0000-000000000009',
 'Corsair Vengeance DDR5 32GB (2x16GB) 6000MHz', 'corsair-vengeance-ddr5-32gb-6000', 'RAM-COR-32GB-D5',
 3290000, 2990000, 40, 60,
 'Corsair Vengeance DDR5-6000MHz 32GB (2x16GB) - RAM hiệu năng cao cho gaming và workstation.',
 '{"capacity": "32GB (2x16GB)", "type": "DDR5", "speed": "6000MHz", "timing": "CL36", "voltage": "1.35V", "heat_sink": "Có", "rgb": "Có"}',
 2,
 'Corsair Vengeance DDR5 32GB 6000MHz | GearVN',
 'RAM Corsair Vengeance DDR5 32GB (2x16GB) 6000MHz chính hãng, bảo hành 60 tháng.',
 NOW(), NOW()),

('00000004-0000-0000-0000-000000000006',
 '00000003-0000-0000-0000-000000000011', '00000002-0000-0000-0000-000000000008',
 'Kingston Fury Beast DDR4 32GB (2x16GB) 3200MHz', 'kingston-fury-beast-ddr4-32gb-3200', 'RAM-KGN-32GB-D4',
 1890000, 1690000, 50, 60,
 'Kingston Fury Beast DDR4-3200MHz 32GB (2x16GB) - RAM phổ thông hiệu năng cao, tản nhiệt tốt.',
 '{"capacity": "32GB (2x16GB)", "type": "DDR4", "speed": "3200MHz", "timing": "CL16", "voltage": "1.35V", "heat_sink": "Có", "rgb": "Không"}',
 2,
 'Kingston Fury Beast DDR4 32GB 3200MHz | GearVN',
 'RAM Kingston Fury Beast DDR4 32GB (2x16GB) 3200MHz chính hãng, bảo hành 60 tháng.',
 NOW(), NOW()),

('00000004-0000-0000-0000-000000000007',
 '00000003-0000-0000-0000-000000000013', '00000002-0000-0000-0000-000000000003',
 'NVIDIA GeForce RTX 4090 Founders Edition', 'nvidia-geforce-rtx-4090-fe', 'VGA-RTX4090-FE',
 47990000, 45990000, 5, 36,
 'NVIDIA GeForce RTX 4090 Founders Edition - Card đồ họa mạnh nhất thế giới, 24GB GDDR6X, Ada Lovelace.',
 '{"chipset": "RTX 4090", "vram": "24GB GDDR6X", "bus": "384-bit", "core_clock": "2520 MHz", "boost_clock": "2520 MHz", "cuda_cores": "16384", "power": "450W", "slots": "3 slot"}',
 2,
 'NVIDIA RTX 4090 Founders Edition | GearVN',
 'NVIDIA GeForce RTX 4090 Founders Edition chính hãng, bảo hành 36 tháng.',
 NOW(), NOW()),

('00000004-0000-0000-0000-000000000008',
 '00000003-0000-0000-0000-000000000014', '00000002-0000-0000-0000-000000000002',
 'AMD Radeon RX 7900 XTX 24GB', 'amd-radeon-rx-7900-xtx', 'VGA-RX7900XTX',
 28990000, 26990000, 8, 36,
 'AMD Radeon RX 7900 XTX - Card đồ họa flagship của AMD, 24GB GDDR6, RDNA 3.',
 '{"chipset": "RX 7900 XTX", "vram": "24GB GDDR6", "bus": "384-bit", "core_clock": "2300 MHz", "boost_clock": "2500 MHz", "stream_processors": "6144", "power": "355W", "slots": "2.5 slot"}',
 2,
 'AMD Radeon RX 7900 XTX 24GB | GearVN',
 'AMD Radeon RX 7900 XTX 24GB chính hãng, bảo hành 36 tháng.',
 NOW(), NOW()),

('00000004-0000-0000-0000-000000000009',
 '00000003-0000-0000-0000-000000000015', '00000002-0000-0000-0000-000000000007',
 'Samsung 990 Pro 2TB NVMe PCIe 4.0', 'samsung-990-pro-2tb', 'SSD-SAM-2TB',
 6490000, 5990000, 30, 60,
 'Samsung 990 Pro 2TB - SSD NVMe PCIe 4.0 tốc độ đọc 7450MB/s, ghi 6900MB/s.',
 '{"capacity": "2TB", "interface": "NVMe PCIe 4.0 x4", "read_speed": "7450 MB/s", "write_speed": "6900 MB/s", "form_factor": "M.2 2280", "nand": "V-NAND TLC", "tbw": "1200TB"}',
 2,
 'Samsung 990 Pro 2TB NVMe | GearVN',
 'SSD Samsung 990 Pro 2TB NVMe PCIe 4.0 chính hãng, bảo hành 60 tháng.',
 NOW(), NOW()),

('00000004-0000-0000-0000-00000000000a',
 '00000003-0000-0000-0000-000000000016', '00000002-0000-0000-0000-000000000008',
 'Kingston A400 480GB SATA III', 'kingston-a400-480gb', 'SSD-KGN-480GB',
 890000, 799000, 60, 36,
 'Kingston A400 480GB - SSD SATA III giá rẻ, phù hợp nâng cấp máy tính cũ.',
 '{"capacity": "480GB", "interface": "SATA III 6Gb/s", "read_speed": "500 MB/s", "write_speed": "450 MB/s", "form_factor": "2.5 inch", "nand": "TLC"}',
 2,
 'Kingston A400 480GB SATA III | GearVN',
 'SSD Kingston A400 480GB SATA III chính hãng, bảo hành 36 tháng.',
 NOW(), NOW()),

('00000004-0000-0000-0000-00000000000b',
 '00000003-0000-0000-0000-000000000006', '00000002-0000-0000-0000-00000000000b',
 'Seagate Barracuda 2TB HDD 7200rpm', 'seagate-barracuda-2tb', 'HDD-SEA-2TB',
 1590000, 1450000, 35, 36,
 'Seagate Barracuda 2TB - Ổ cứng HDD 3.5 inch, 7200rpm, bộ nhớ đệm 256MB, phù hợp lưu trữ dữ liệu.',
 '{"capacity": "2TB", "interface": "SATA III 6Gb/s", "rpm": "7200", "cache": "256MB", "form_factor": "3.5 inch", "recording_tech": "CMR"}',
 2,
 'Seagate Barracuda 2TB 7200rpm | GearVN',
 'HDD Seagate Barracuda 2TB 7200rpm chính hãng, bảo hành 36 tháng.',
 NOW(), NOW()),

('00000004-0000-0000-0000-00000000000c',
 '00000003-0000-0000-0000-000000000007', '00000002-0000-0000-0000-000000000009',
 'Corsair RM850x Shift 850W 80+ Gold', 'corsair-rm850x-shift-850w', 'PSU-COR-850W',
 3990000, 3590000, 22, 120,
 'Corsair RM850x Shift 850W - Nguồn ATX 3.0, 80+ Gold, modular, với connector side interface độc đáo.',
 '{"wattage": "850W", "certification": "80+ Gold", "modular": "Full Modular", "standard": "ATX 3.0", "fan": "135mm", "protection": "OVP/SCP/OCP/OTP", "warranty": "10 năm"}',
 2,
 'Corsair RM850x Shift 850W 80+ Gold | GearVN',
 'PSU Corsair RM850x Shift 850W 80+ Gold chính hãng, bảo hành 120 tháng.',
 NOW(), NOW()),

('00000004-0000-0000-0000-00000000000d',
 '00000003-0000-0000-0000-000000000008', '00000002-0000-0000-0000-00000000000e',
 'NZXT H7 Flow White', 'nzxt-h7-flow-white', 'CASE-NZXT-H7',
 2790000, 2590000, 12, 24,
 'NZXT H7 Flow White - Case ATX tản nhiệt tốt, mặt trước mesh, kính cường lực bên hông.',
 '{"form_factor": "Mid Tower ATX", "motherboard_support": "E-ATX/ATX/mATX/ITX", "gpu_clearance": "400mm", "cpu_cooler_clearance": "185mm", "fan_support": "3x 120mm / 2x 140mm (front)", "radiator_support": "360mm (front)", "material": "SGCC Steel + Tempered Glass", "color": "White"}',
 2,
 'NZXT H7 Flow White | GearVN',
 'Case NZXT H7 Flow White chính hãng, tản nhiệt tối ưu, bảo hành 24 tháng.',
 NOW(), NOW()),

('00000004-0000-0000-0000-00000000000e',
 '00000003-0000-0000-0000-000000000009', '00000002-0000-0000-0000-00000000000d',
 'Noctua NH-D15 Chromax Black', 'noctua-nh-d15-chromax-black', 'COOL-NH-D15',
 3290000, 2990000, 10, 72,
 'Noctua NH-D15 Chromax Black - Tản nhiệt khí cao cấp nhất, 2 quạt 140mm, hiệu năng ngang AIO 240mm.',
 '{"type": "Dual Tower Air Cooler", "fan": "2x NF-A15 140mm", "socket_support": "LGA 1700/1200/115x, AM5/AM4", "tdp": "~250W", "height": "165mm", "material": "6 heat pipes + Aluminum fins"}',
 2,
 'Noctua NH-D15 Chromax Black | GearVN',
 'Tản nhiệt Noctua NH-D15 Chromax Black chính hãng, bảo hành 72 tháng.',
 NOW(), NOW()),

('00000004-0000-0000-0000-00000000000f',
 '00000003-0000-0000-0000-00000000000c', '00000002-0000-0000-0000-00000000000f',
 'Logitech G Pro X Superlight Wireless', 'logitech-g-pro-x-superlight', 'MOUSE-LOGI-SL',
 3290000, 2890000, 28, 24,
 'Logitech G Pro X Superlight - Chuột gaming không dây siêu nhẹ 63g, sensor HERO 25K, pin 70 giờ.',
 '{"weight": "63g", "sensor": "HERO 25K", "dpi": "100 - 25,600", "connection": "LIGHTSPEED Wireless", "battery_life": "70 hours", "switches": "Omron 50M", "color": "Black"}',
 2,
 'Logitech G Pro X Superlight | GearVN',
 'Chuột Logitech G Pro X Superlight chính hãng, siêu nhẹ 63g, bảo hành 24 tháng.',
 NOW(), NOW()) ON CONFLICT (slug) DO NOTHING;

-- ===========================================
-- 5. PRODUCT IMAGES (16 rows - 1 per product)
-- ===========================================
INSERT INTO product_images (image_id, product_id, image_url, is_primary, sort_order, created_at) VALUES
('00000005-0000-0000-0000-000000000001', '00000004-0000-0000-0000-000000000001', 'https://example.com/img/products/cpu-i9-14900k-1.jpg', TRUE, 1, NOW()),
('00000005-0000-0000-0000-000000000002', '00000004-0000-0000-0000-000000000001', 'https://example.com/img/products/cpu-i9-14900k-2.jpg', FALSE, 2, NOW()),
('00000005-0000-0000-0000-000000000003', '00000004-0000-0000-0000-000000000002', 'https://example.com/img/products/cpu-r9-7950x-1.jpg', TRUE, 1, NOW()),
('00000005-0000-0000-0000-000000000004', '00000004-0000-0000-0000-000000000003', 'https://example.com/img/products/mb-asus-z790e-1.jpg', TRUE, 1, NOW()),
('00000005-0000-0000-0000-000000000005', '00000004-0000-0000-0000-000000000004', 'https://example.com/img/products/mb-msi-b650-1.jpg', TRUE, 1, NOW()),
('00000005-0000-0000-0000-000000000006', '00000004-0000-0000-0000-000000000005', 'https://example.com/img/products/ram-corsair-d5-1.jpg', TRUE, 1, NOW()),
('00000005-0000-0000-0000-000000000007', '00000004-0000-0000-0000-000000000006', 'https://example.com/img/products/ram-kgn-d4-1.jpg', TRUE, 1, NOW()),
('00000005-0000-0000-0000-000000000008', '00000004-0000-0000-0000-000000000007', 'https://example.com/img/products/vga-rtx4090-fe-1.jpg', TRUE, 1, NOW()),
('00000005-0000-0000-0000-000000000009', '00000004-0000-0000-0000-000000000008', 'https://example.com/img/products/vga-rx7900xtx-1.jpg', TRUE, 1, NOW()),
('00000005-0000-0000-0000-00000000000a', '00000004-0000-0000-0000-000000000009', 'https://example.com/img/products/ssd-sam-990pro-1.jpg', TRUE, 1, NOW()),
('00000005-0000-0000-0000-00000000000b', '00000004-0000-0000-0000-00000000000a', 'https://example.com/img/products/ssd-kgn-a400-1.jpg', TRUE, 1, NOW()),
('00000005-0000-0000-0000-00000000000c', '00000004-0000-0000-0000-00000000000b', 'https://example.com/img/products/hdd-sea-2tb-1.jpg', TRUE, 1, NOW()),
('00000005-0000-0000-0000-00000000000d', '00000004-0000-0000-0000-00000000000c', 'https://example.com/img/products/psu-cor-rm850x-1.jpg', TRUE, 1, NOW()),
('00000005-0000-0000-0000-00000000000e', '00000004-0000-0000-0000-00000000000d', 'https://example.com/img/products/case-nzxt-h7-1.jpg', TRUE, 1, NOW()),
('00000005-0000-0000-0000-00000000000f', '00000004-0000-0000-0000-00000000000e', 'https://example.com/img/products/cool-noctua-d15-1.jpg', TRUE, 1, NOW()),
('00000005-0000-0000-0000-000000000010', '00000004-0000-0000-0000-00000000000f', 'https://example.com/img/products/mouse-logi-sl-1.jpg', TRUE, 1, NOW());

-- ===========================================
-- 6. ADDRESSES (10 rows)
-- ===========================================
INSERT INTO addresses (address_id, user_id, recipient_name, phone, address_line, province, ward, is_default, created_at) VALUES
('00000006-0000-0000-0000-000000000001', '00000001-0000-0000-0000-000000000004', 'Phạm Minh Tuấn', '0901000004', '123 Nguyễn Huệ, P. Bến Nghé', 'TP Hồ Chí Minh', 'Phường Bến Nghé', TRUE, NOW()),
('00000006-0000-0000-0000-000000000002', '00000001-0000-0000-0000-000000000004', 'Phạm Minh Tuấn', '0901000004', '456 Lê Lợi, P. Bến Thành', 'TP Hồ Chí Minh', 'Phường Bến Thành', FALSE, NOW()),
('00000006-0000-0000-0000-000000000003', '00000001-0000-0000-0000-000000000005', 'Hoàng Thị Lan', '0901000005', '789 Trần Hưng Đạo', 'Hà Nội', 'Phường Hàng Bài', TRUE, NOW()),
('00000006-0000-0000-0000-000000000004', '00000001-0000-0000-0000-000000000006', 'Đặng Quốc Huy', '0901000006', '25 Lê Duẩn', 'Đà Nẵng', 'Phường Thạch Thang', TRUE, NOW()),
('00000006-0000-0000-0000-000000000005', '00000001-0000-0000-0000-000000000007', 'Vũ Thị Hương', '0901000007', '88 Nguyễn Văn Linh', 'Hải Phòng', 'Phường An Dương', TRUE, NOW()),
('00000006-0000-0000-0000-000000000006', '00000001-0000-0000-0000-000000000008', 'Ngô Văn Phúc', '0901000008', '15 Hoàng Diệu', 'Cần Thơ', 'Phường An Lạc', TRUE, NOW()),
('00000006-0000-0000-0000-000000000007', '00000001-0000-0000-0000-000000000009', 'Bùi Thanh Mai', '0901000009', '67 Phạm Văn Đồng', 'Hà Nội', 'Phường Dịch Vọng', TRUE, NOW()),
('00000006-0000-0000-0000-000000000008', '00000001-0000-0000-0000-00000000000a', 'Đỗ Minh Hoàng', '0901000010', '234 Hai Bà Trưng', 'TP Hồ Chí Minh', 'Phường Võ Thị Sáu', TRUE, NOW()),
('00000006-0000-0000-0000-000000000009', '00000001-0000-0000-0000-000000000004', 'Phạm Minh Tuấn', '0901000004', '50 Bạch Đằng', 'Đà Nẵng', 'Phường Hải Châu 1', FALSE, NOW()),
('00000006-0000-0000-0000-00000000000a', '00000001-0000-0000-0000-000000000005', 'Hoàng Thị Lan', '0901000005', '100 Mỹ Đình', 'Hà Nội', 'Phường Mỹ Đình 2', FALSE, NOW());

-- ===========================================
-- 7. COUPONS (10 rows)
-- ===========================================
INSERT INTO coupons (coupon_id, code, description, discount_type, discount_value, min_order_amount, max_discount, usage_limit, used_count, per_user_limit, start_date, end_date, is_active, created_by, created_at) VALUES
('00000007-0000-0000-0000-000000000001', 'WELCOME10', 'Giảm 10% cho đơn hàng đầu tiên', 'percentage', 10, 500000, 500000, 100, 15, 1, NOW() - INTERVAL '30 days', NOW() + INTERVAL '60 days', TRUE, '00000001-0000-0000-0000-000000000001', NOW()),
('00000007-0000-0000-0000-000000000002', 'GIAM200K', 'Giảm 200,000đ cho đơn từ 2 triệu', 'fixed_amount', 200000, 2000000, NULL, 50, 22, 1, NOW() - INTERVAL '15 days', NOW() + INTERVAL '45 days', TRUE, '00000001-0000-0000-0000-000000000001', NOW()),
('00000007-0000-0000-0000-000000000003', 'SUMMER2024', 'Giảm 15% mùa hè', 'percentage', 15, 1000000, 1000000, 200, 45, 2, NOW() - INTERVAL '10 days', NOW() + INTERVAL '20 days', TRUE, '00000001-0000-0000-0000-000000000001', NOW()),
('00000007-0000-0000-0000-000000000004', 'BUILDPC', 'Giảm 5% khi build PC', 'percentage', 5, 10000000, 1000000, 50, 8, 1, NOW() - INTERVAL '60 days', NOW() + INTERVAL '30 days', TRUE, '00000001-0000-0000-0000-000000000001', NOW()),
('00000007-0000-0000-0000-000000000005', 'FREE50K', 'Freeship 50,000đ', 'fixed_amount', 50000, 300000, NULL, 500, 102, 3, NOW() - INTERVAL '90 days', NOW() + INTERVAL '90 days', TRUE, '00000001-0000-0000-0000-000000000002', NOW()),
('00000007-0000-0000-0000-000000000006', 'VIPMEMBER', 'Giảm 20% cho thành viên VIP', 'percentage', 20, 2000000, 2000000, 30, 5, 1, NOW() - INTERVAL '5 days', NOW() + INTERVAL '55 days', TRUE, '00000001-0000-0000-0000-000000000001', NOW()),
('00000007-0000-0000-0000-000000000007', 'RAMUPGRADE', 'Giảm 300K khi mua RAM', 'fixed_amount', 300000, 2000000, NULL, 80, 12, 2, NOW() - INTERVAL '20 days', NOW() + INTERVAL '40 days', TRUE, '00000001-0000-0000-0000-000000000001', NOW()),
('00000007-0000-0000-0000-000000000008', 'BLACKFRID24', 'Black Friday 30%', 'percentage', 30, 3000000, 3000000, 999, 3, 1, NOW() - INTERVAL '180 days', NOW() + INTERVAL '10 days', TRUE, '00000001-0000-0000-0000-000000000003', NOW()),
('00000007-0000-0000-0000-000000000009', 'NEWYEAR25', 'Năm mới giảm 25%', 'percentage', 25, 1000000, 1500000, 100, 0, 1, NOW() + INTERVAL '20 days', NOW() + INTERVAL '50 days', FALSE, '00000001-0000-0000-0000-000000000001', NOW()),
('00000007-0000-0000-0000-00000000000a', 'SSD50', 'Giảm 50K khi mua SSD', 'fixed_amount', 50000, 500000, NULL, 150, 33, 1, NOW() - INTERVAL '14 days', NOW() + INTERVAL '14 days', TRUE, '00000001-0000-0000-0000-000000000002', NOW()) ON CONFLICT (code) DO NOTHING;

-- ===========================================
-- 8. ORDERS (10 rows)
-- status: 1=pending, 2=confirmed, 3=processing, 4=shipping, 5=delivered, 6=cancelled
-- ===========================================
INSERT INTO orders (order_id, user_id, order_code, total_amount, status, payment_method, payment_status, shipping_address_id, shipping_fee, discount_amount, notes, created_at, updated_at) VALUES
('00000008-0000-0000-0000-000000000001', '00000001-0000-0000-0000-000000000004', 'ORD-000001', 60870000, 5, 'COD', 2, '00000006-0000-0000-0000-000000000001', 35000, 500000, 'Giao hàng giờ hành chính', NOW() - INTERVAL '10 days', NOW() - INTERVAL '8 days'),
('00000008-0000-0000-0000-000000000002', '00000001-0000-0000-0000-000000000005', 'ORD-000002', 6370000, 5, 'Banking', 2, '00000006-0000-0000-0000-000000000003', 30000, 200000, NULL, NOW() - INTERVAL '8 days', NOW() - INTERVAL '6 days'),
('00000008-0000-0000-0000-000000000003', '00000001-0000-0000-0000-000000000006', 'ORD-000003', 15980000, 4, 'COD', 1, '00000006-0000-0000-0000-000000000004', 50000, 0, 'Gọi trước khi giao', NOW() - INTERVAL '5 days', NOW() - INTERVAL '4 days'),
('00000008-0000-0000-0000-000000000004', '00000001-0000-0000-0000-000000000007', 'ORD-000004', 8080000, 3, 'VNPay', 1, '00000006-0000-0000-0000-000000000005', 0, 50000, NULL, NOW() - INTERVAL '3 days', NOW() - INTERVAL '2 days'),
('00000008-0000-0000-0000-000000000005', '00000001-0000-0000-0000-000000000008', 'ORD-000005', 49580000, 2, 'COD', 1, '00000006-0000-0000-0000-000000000006', 35000, 0, 'Giao cuối tuần', NOW() - INTERVAL '2 days', NOW() - INTERVAL '1 day'),
('00000008-0000-0000-0000-000000000006', '00000001-0000-0000-0000-000000000009', 'ORD-000006', 2990000, 6, 'Banking', 3, '00000006-0000-0000-0000-000000000007', 0, 300000, 'Khách hủy vì tìm thấy giá rẻ hơn', NOW() - INTERVAL '7 days', NOW() - INTERVAL '6 days'),
('00000008-0000-0000-0000-000000000007', '00000001-0000-0000-0000-000000000004', 'ORD-000007', 1450000, 5, 'Momo', 2, '00000006-0000-0000-0000-000000000002', 30000, 0, NULL, NOW() - INTERVAL '4 days', NOW() - INTERVAL '2 days'),
('00000008-0000-0000-0000-000000000008', '00000001-0000-0000-0000-000000000006', 'ORD-000008', 5990000, 1, 'COD', 1, '00000006-0000-0000-0000-000000000004', 35000, 0, NULL, NOW() - INTERVAL '1 day', NOW()),
('00000008-0000-0000-0000-000000000009', '00000001-0000-0000-0000-00000000000a', 'ORD-000009', 14670000, 5, 'Banking', 2, '00000006-0000-0000-0000-000000000008', 0, 500000, 'Đã gọi xác nhận', NOW() - INTERVAL '6 days', NOW() - INTERVAL '4 days'),
('00000008-0000-0000-0000-00000000000a', '00000001-0000-0000-0000-000000000005', 'ORD-000010', 799000, 6, 'COD', 1, '00000006-0000-0000-0000-00000000000a', 30000, 0, 'Khách không nhận hàng', NOW() - INTERVAL '3 days', NOW() - INTERVAL '2 days') ON CONFLICT (order_code) DO NOTHING;

-- ===========================================
-- 9. ORDER ITEMS (18 rows)
-- ===========================================
INSERT INTO order_items (order_item_id, order_id, product_id, quantity, unit_price) VALUES
('00000009-0000-0000-0000-000000000001', '00000008-0000-0000-0000-000000000001', '00000004-0000-0000-0000-000000000001', 1, 11990000),
('00000009-0000-0000-0000-000000000002', '00000008-0000-0000-0000-000000000001', '00000004-0000-0000-0000-000000000007', 1, 45990000),
('00000009-0000-0000-0000-000000000003', '00000008-0000-0000-0000-000000000002', '00000004-0000-0000-0000-000000000006', 2, 1690000),
('00000009-0000-0000-0000-000000000004', '00000008-0000-0000-0000-000000000002', '00000004-0000-0000-0000-000000000005', 1, 2990000),
('00000009-0000-0000-0000-000000000005', '00000008-0000-0000-0000-000000000003', '00000004-0000-0000-0000-000000000002', 1, 12990000),
('00000009-0000-0000-0000-000000000006', '00000008-0000-0000-0000-000000000003', '00000004-0000-0000-0000-00000000000e', 1, 2990000),
('00000009-0000-0000-0000-000000000007', '00000008-0000-0000-0000-000000000004', '00000004-0000-0000-0000-000000000004', 1, 5190000),
('00000009-0000-0000-0000-000000000008', '00000008-0000-0000-0000-000000000004', '00000004-0000-0000-0000-00000000000f', 1, 2890000),
('00000009-0000-0000-0000-000000000009', '00000008-0000-0000-0000-000000000005', '00000004-0000-0000-0000-000000000007', 1, 45990000),
('00000009-0000-0000-0000-00000000000a', '00000008-0000-0000-0000-000000000005', '00000004-0000-0000-0000-00000000000c', 1, 3590000),
('00000009-0000-0000-0000-00000000000b', '00000008-0000-0000-0000-000000000006', '00000004-0000-0000-0000-000000000005', 1, 2990000),
('00000009-0000-0000-0000-00000000000c', '00000008-0000-0000-0000-000000000007', '00000004-0000-0000-0000-00000000000b', 1, 1450000),
('00000009-0000-0000-0000-00000000000d', '00000008-0000-0000-0000-000000000008', '00000004-0000-0000-0000-000000000009', 1, 5990000),
('00000009-0000-0000-0000-00000000000e', '00000008-0000-0000-0000-000000000009', '00000004-0000-0000-0000-000000000003', 1, 8490000),
('00000009-0000-0000-0000-00000000000f', '00000008-0000-0000-0000-000000000009', '00000004-0000-0000-0000-00000000000d', 1, 2590000),
('00000009-0000-0000-0000-000000000010', '00000008-0000-0000-0000-000000000009', '00000004-0000-0000-0000-00000000000c', 1, 3590000),
('00000009-0000-0000-0000-000000000011', '00000008-0000-0000-0000-00000000000a', '00000004-0000-0000-0000-00000000000a', 1, 799000),
('00000009-0000-0000-0000-000000000012', '00000008-0000-0000-0000-000000000001', '00000004-0000-0000-0000-00000000000f', 1, 2890000);

-- ===========================================
-- 10. PAYMENTS (10 rows)
-- ===========================================
INSERT INTO payments (payment_id, order_id, amount, payment_method, transaction_id, status, paid_at, gateway_response, return_url, created_at, updated_at) VALUES
('0000000a-0000-0000-0000-000000000001', '00000008-0000-0000-0000-000000000001', 60870000, 'COD', NULL, 2, NOW() - INTERVAL '8 days', NULL, NULL, NOW() - INTERVAL '10 days', NOW() - INTERVAL '8 days'),
('0000000a-0000-0000-0000-000000000002', '00000008-0000-0000-0000-000000000002', 6370000, 'Banking', 'TXN-20240115-001', 2, NOW() - INTERVAL '7 days', '{"bank": "VCB", "status": "success"}', NULL, NOW() - INTERVAL '8 days', NOW() - INTERVAL '6 days'),
('0000000a-0000-0000-0000-000000000003', '00000008-0000-0000-0000-000000000003', 15980000, 'COD', NULL, 1, NULL, NULL, NULL, NOW() - INTERVAL '5 days', NOW() - INTERVAL '4 days'),
('0000000a-0000-0000-0000-000000000004', '00000008-0000-0000-0000-000000000004', 8080000, 'VNPay', 'VNP-20240120-001', 1, NULL, NULL, 'https://gearvn.id.vn/payment/return', NOW() - INTERVAL '3 days', NOW()),
('0000000a-0000-0000-0000-000000000005', '00000008-0000-0000-0000-000000000005', 49580000, 'COD', NULL, 1, NULL, NULL, NULL, NOW() - INTERVAL '2 days', NOW()),
('0000000a-0000-0000-0000-000000000006', '00000008-0000-0000-0000-000000000006', 2990000, 'Banking', 'TXN-20240112-002', 3, NULL, '{"bank": "VCB", "status": "refunded"}', NULL, NOW() - INTERVAL '7 days', NOW() - INTERVAL '6 days'),
('0000000a-0000-0000-0000-000000000007', '00000008-0000-0000-0000-000000000007', 1450000, 'Momo', 'MOMO-20240116-001', 2, NOW() - INTERVAL '3 days', '{"provider": "Momo", "status": "success"}', NULL, NOW() - INTERVAL '4 days', NOW() - INTERVAL '2 days'),
('0000000a-0000-0000-0000-000000000008', '00000008-0000-0000-0000-000000000008', 5990000, 'COD', NULL, 1, NULL, NULL, NULL, NOW() - INTERVAL '1 day', NOW()),
('0000000a-0000-0000-0000-000000000009', '00000008-0000-0000-0000-000000000009', 14670000, 'Banking', 'TXN-20240114-003', 2, NOW() - INTERVAL '5 days', '{"bank": "Techcombank", "status": "success"}', NULL, NOW() - INTERVAL '6 days', NOW() - INTERVAL '4 days'),
('0000000a-0000-0000-0000-00000000000a', '00000008-0000-0000-0000-00000000000a', 799000, 'COD', NULL, 1, NULL, NULL, NULL, NOW() - INTERVAL '3 days', NOW());

-- ===========================================
-- 11. ORDER STATUS HISTORY (12 rows)
-- ===========================================
INSERT INTO order_status_history (id, order_id, old_status, new_status, changed_by, note, created_at) VALUES
('0000000b-0000-0000-0000-000000000001', '00000008-0000-0000-0000-000000000001', NULL, 1, '00000001-0000-0000-0000-000000000004', 'Đặt hàng', NOW() - INTERVAL '10 days'),
('0000000b-0000-0000-0000-000000000002', '00000008-0000-0000-0000-000000000001', 1, 2, '00000001-0000-0000-0000-000000000002', 'Xác nhận đơn hàng', NOW() - INTERVAL '9 days'),
('0000000b-0000-0000-0000-000000000003', '00000008-0000-0000-0000-000000000001', 2, 3, '00000001-0000-0000-0000-000000000003', 'Đang xử lý', NOW() - INTERVAL '9 days'),
('0000000b-0000-0000-0000-000000000004', '00000008-0000-0000-0000-000000000001', 3, 4, '00000001-0000-0000-0000-000000000003', 'Đã bàn giao vận chuyển', NOW() - INTERVAL '8 days'),
('0000000b-0000-0000-0000-000000000005', '00000008-0000-0000-0000-000000000001', 4, 5, '00000001-0000-0000-0000-000000000003', 'Giao hàng thành công', NOW() - INTERVAL '8 days'),
('0000000b-0000-0000-0000-000000000006', '00000008-0000-0000-0000-000000000006', NULL, 1, '00000001-0000-0000-0000-000000000009', 'Đặt hàng', NOW() - INTERVAL '7 days'),
('0000000b-0000-0000-0000-000000000007', '00000008-0000-0000-0000-000000000006', 1, 6, '00000001-0000-0000-0000-000000000009', 'Khách hủy đơn', NOW() - INTERVAL '6 days'),
('0000000b-0000-0000-0000-000000000008', '00000008-0000-0000-0000-000000000002', NULL, 1, '00000001-0000-0000-0000-000000000005', 'Đặt hàng', NOW() - INTERVAL '8 days'),
('0000000b-0000-0000-0000-000000000009', '00000008-0000-0000-0000-000000000002', 1, 2, '00000001-0000-0000-0000-000000000002', 'Xác nhận', NOW() - INTERVAL '7 days'),
('0000000b-0000-0000-0000-00000000000a', '00000008-0000-0000-0000-000000000002', 2, 3, '00000001-0000-0000-0000-000000000002', 'Đang xử lý', NOW() - INTERVAL '7 days'),
('0000000b-0000-0000-0000-00000000000b', '00000008-0000-0000-0000-000000000002', 3, 4, '00000001-0000-0000-0000-000000000003', 'Đã gửi hàng', NOW() - INTERVAL '6 days'),
('0000000b-0000-0000-0000-00000000000c', '00000008-0000-0000-0000-000000000002', 4, 5, '00000001-0000-0000-0000-000000000003', 'Giao hàng thành công', NOW() - INTERVAL '6 days');

-- ===========================================
-- 12. SHIPMENTS (10 rows)
-- ===========================================
INSERT INTO shipments (shipment_id, order_id, carrier, tracking_code, shipping_fee, estimated_delivery, actual_delivery, status, packed_by, packed_at, qc_passed, qc_notes, created_at, updated_at) VALUES
('0000000c-0000-0000-0000-000000000001', '00000008-0000-0000-0000-000000000001', 'GHN', 'GHN-2024-00001', 35000, NOW() - INTERVAL '9 days', NOW() - INTERVAL '8 days', 'delivered', '00000001-0000-0000-0000-000000000003', NOW() - INTERVAL '9 days', TRUE, NULL, NOW() - INTERVAL '10 days', NOW() - INTERVAL '8 days'),
('0000000c-0000-0000-0000-000000000002', '00000008-0000-0000-0000-000000000002', 'GHTK', 'GHTK-2024-00001', 30000, NOW() - INTERVAL '7 days', NOW() - INTERVAL '6 days', 'delivered', '00000001-0000-0000-0000-000000000002', NOW() - INTERVAL '8 days', TRUE, NULL, NOW() - INTERVAL '8 days', NOW() - INTERVAL '6 days'),
('0000000c-0000-0000-0000-000000000003', '00000008-0000-0000-0000-000000000003', 'Viettel Post', 'VTP-2024-00001', 50000, NOW() - INTERVAL '4 days', NULL, 'shipping', '00000001-0000-0000-0000-000000000003', NOW() - INTERVAL '5 days', TRUE, 'Đã kiểm tra đầy đủ linh kiện', NOW() - INTERVAL '5 days', NOW() - INTERVAL '4 days'),
('0000000c-0000-0000-0000-000000000004', '00000008-0000-0000-0000-000000000004', 'GHN', 'GHN-2024-00002', 0, NOW() + INTERVAL '2 days', NULL, 'pending', NULL, NULL, FALSE, NULL, NOW() - INTERVAL '3 days', NOW()),
('0000000c-0000-0000-0000-000000000005', '00000008-0000-0000-0000-000000000005', 'GHN', 'GHN-2024-00003', 35000, NOW() + INTERVAL '3 days', NULL, 'pending', NULL, NULL, FALSE, NULL, NOW() - INTERVAL '2 days', NOW()),
('0000000c-0000-0000-0000-000000000006', '00000008-0000-0000-0000-000000000006', 'GHTK', NULL, 0, NULL, NULL, 'pending', NULL, NULL, FALSE, NULL, NOW() - INTERVAL '7 days', NOW() - INTERVAL '6 days'),
('0000000c-0000-0000-0000-000000000007', '00000008-0000-0000-0000-000000000007', 'GHN', 'GHN-2024-00004', 30000, NOW() - INTERVAL '3 days', NOW() - INTERVAL '2 days', 'delivered', '00000001-0000-0000-0000-000000000002', NOW() - INTERVAL '5 days', TRUE, NULL, NOW() - INTERVAL '4 days', NOW() - INTERVAL '2 days'),
('0000000c-0000-0000-0000-000000000008', '00000008-0000-0000-0000-000000000008', 'Viettel Post', NULL, 35000, NOW() + INTERVAL '5 days', NULL, 'pending', NULL, NULL, FALSE, NULL, NOW() - INTERVAL '1 day', NOW()),
('0000000c-0000-0000-0000-000000000009', '00000008-0000-0000-0000-000000000009', 'GHN', 'GHN-2024-00005', 0, NOW() - INTERVAL '5 days', NOW() - INTERVAL '4 days', 'delivered', '00000001-0000-0000-0000-000000000003', NOW() - INTERVAL '7 days', TRUE, NULL, NOW() - INTERVAL '6 days', NOW() - INTERVAL '4 days'),
('0000000c-0000-0000-0000-00000000000a', '00000008-0000-0000-0000-00000000000a', 'GHTK', NULL, 30000, NULL, NULL, 'pending', NULL, NULL, FALSE, NULL, NOW() - INTERVAL '3 days', NOW());

-- ===========================================
-- 13. RETURN REQUESTS (10 rows)
-- ===========================================
INSERT INTO return_requests (return_id, order_id, user_id, reason, description, status, refund_amount, processed_by, processed_at, admin_note, created_at, updated_at) VALUES
('0000000d-0000-0000-0000-000000000001', '00000008-0000-0000-0000-000000000002', '00000001-0000-0000-0000-000000000005', 'Sản phẩm lỗi', 'RAM bị lỗi không boot được máy', 'approved', 3380000, '00000001-0000-0000-0000-000000000001', NOW() - INTERVAL '5 days', 'Đồng ý đổi trả, kiểm tra hàng về', NOW() - INTERVAL '7 days', NOW() - INTERVAL '5 days'),
('0000000d-0000-0000-0000-000000000002', '00000008-0000-0000-0000-000000000006', '00000001-0000-0000-0000-000000000009', 'Không còn nhu cầu', 'Đã tìm thấy sản phẩm khác phù hợp hơn', 'completed', 2690000, '00000001-0000-0000-0000-000000000002', NOW() - INTERVAL '4 days', 'Đã hoàn tiền', NOW() - INTERVAL '6 days', NOW() - INTERVAL '4 days'),
('0000000d-0000-0000-0000-000000000003', '00000008-0000-0000-0000-00000000000a', '00000001-0000-0000-0000-000000000005', 'Sai sản phẩm', 'Nhận được SSD 240GB thay vì 480GB', 'approved', 799000, '00000001-0000-0000-0000-000000000001', NOW() - INTERVAL '1 day', 'Xác nhận sai hàng, hoàn tiền', NOW() - INTERVAL '2 days', NOW() - INTERVAL '1 day'),
('0000000d-0000-0000-0000-000000000004', '00000008-0000-0000-0000-000000000005', '00000001-0000-0000-0000-000000000008', 'Hàng không như mô tả', 'Nguồn không có cáp 12VHPWR như mô tả', 'pending', NULL, NULL, NULL, NULL, NOW(), NOW()),
('0000000d-0000-0000-0000-000000000005', '00000008-0000-0000-0000-000000000001', '00000001-0000-0000-0000-000000000004', 'Sản phẩm lỗi', 'Mainboard bị lỗi khe cắm RAM', 'pending', NULL, NULL, NULL, NULL, NOW(), NOW()),
('0000000d-0000-0000-0000-000000000006', '00000008-0000-0000-0000-000000000004', '00000001-0000-0000-0000-000000000007', 'Giao hàng chậm', 'Giao hàng trễ 5 ngày so với cam kết', 'rejected', NULL, '00000001-0000-0000-0000-000000000001', NOW(), 'Giao hàng chậm không thuộc chính sách đổi trả', NOW() - INTERVAL '1 day', NOW()),
('0000000d-0000-0000-0000-000000000007', '00000008-0000-0000-0000-000000000007', '00000001-0000-0000-0000-000000000004', 'Sản phẩm lỗi', 'HDD bị bad sector sau 2 ngày sử dụng', 'approved', 1450000, '00000001-0000-0000-0000-000000000002', NOW() - INTERVAL '1 day', 'Xác nhận lỗi, đổi hàng mới', NOW() - INTERVAL '3 days', NOW() - INTERVAL '1 day'),
('0000000d-0000-0000-0000-000000000008', '00000008-0000-0000-0000-000000000001', '00000001-0000-0000-0000-000000000004', 'Không còn nhu cầu', 'Đã nâng cấp lên đời card mới hơn', 'pending', NULL, NULL, NULL, NULL, NOW(), NOW()),
('0000000d-0000-0000-0000-000000000009', '00000008-0000-0000-0000-000000000003', '00000001-0000-0000-0000-000000000006', 'Sản phẩm lỗi', 'Quạt tản nhiệt kêu to bất thường', 'rejected', NULL, '00000001-0000-0000-0000-000000000002', NOW(), 'Đã kiểm tra, sản phẩm hoạt động bình thường', NOW() - INTERVAL '2 days', NOW()),
('0000000d-0000-0000-0000-00000000000a', '00000008-0000-0000-0000-000000000009', '00000001-0000-0000-0000-00000000000a', 'Sai sản phẩm', 'Nhận nhầm màu case', 'completed', 2590000, '00000001-0000-0000-0000-000000000001', NOW() - INTERVAL '3 days', 'Đã đổi sang đúng màu', NOW() - INTERVAL '5 days', NOW() - INTERVAL '3 days');

-- ===========================================
-- 14. RETURN REQUEST ITEMS (10 rows)
-- ===========================================
INSERT INTO return_request_items (id, return_id, order_item_id, quantity, reason_detail) VALUES
('0000000e-0000-0000-0000-000000000001', '0000000d-0000-0000-0000-000000000001', '00000009-0000-0000-0000-000000000003', 2, 'Cả 2 thanh RAM đều không boot được'),
('0000000e-0000-0000-0000-000000000002', '0000000d-0000-0000-0000-000000000002', '00000009-0000-0000-0000-00000000000b', 1, 'Chưa sử dụng, còn nguyên seal'),
('0000000e-0000-0000-0000-000000000003', '0000000d-0000-0000-0000-000000000003', '00000009-0000-0000-0000-000000000011', 1, 'Bị gửi sai dung lượng'),
('0000000e-0000-0000-0000-000000000004', '0000000d-0000-0000-0000-000000000004', '00000009-0000-0000-0000-00000000000a', 1, 'Thiếu cáp 12VHPWR'),
('0000000e-0000-0000-0000-000000000005', '0000000d-0000-0000-0000-000000000005', '00000009-0000-0000-0000-000000000012', 1, 'Khe RAM số 2 không hoạt động'),
('0000000e-0000-0000-0000-000000000006', '0000000d-0000-0000-0000-000000000006', '00000009-0000-0000-0000-000000000007', 1, 'Delay quá lâu'),
('0000000e-0000-0000-0000-000000000007', '0000000d-0000-0000-0000-000000000007', '00000009-0000-0000-0000-00000000000c', 1, 'Bad sector xuất hiện sau 2 ngày dùng'),
('0000000e-0000-0000-0000-000000000008', '0000000d-0000-0000-0000-000000000008', '00000009-0000-0000-0000-000000000002', 1, 'Muốn đổi lên RTX 5090'),
('0000000e-0000-0000-0000-000000000009', '0000000d-0000-0000-0000-000000000009', '00000009-0000-0000-0000-000000000006', 1, 'Quạt kêu lạch cạch khi quay'),
('0000000e-0000-0000-0000-000000000010', '0000000d-0000-0000-0000-00000000000a', '00000009-0000-0000-0000-00000000000f', 1, 'Đặt màu đen nhưng nhận màu trắng');

-- ===========================================
-- 15. RETURN REQUEST IMAGES (10 rows)
-- ===========================================
INSERT INTO return_request_images (id, return_id, image_url, created_at) VALUES
('0000000f-0000-0000-0000-000000000001', '0000000d-0000-0000-0000-000000000001', 'https://example.com/img/returns/ram-loi-1.jpg', NOW() - INTERVAL '7 days'),
('0000000f-0000-0000-0000-000000000002', '0000000d-0000-0000-0000-000000000001', 'https://example.com/img/returns/ram-loi-2.jpg', NOW() - INTERVAL '7 days'),
('0000000f-0000-0000-0000-000000000003', '0000000d-0000-0000-0000-000000000003', 'https://example.com/img/returns/ssd-sai-1.jpg', NOW() - INTERVAL '2 days'),
('0000000f-0000-0000-0000-000000000004', '0000000d-0000-0000-0000-000000000004', 'https://example.com/img/returns/psu-thieu-cap-1.jpg', NOW()),
('0000000f-0000-0000-0000-000000000005', '0000000d-0000-0000-0000-000000000005', 'https://example.com/img/returns/mainboard-loi-1.jpg', NOW()),
('0000000f-0000-0000-0000-000000000006', '0000000d-0000-0000-0000-000000000007', 'https://example.com/img/returns/hdd-badsector-1.jpg', NOW() - INTERVAL '3 days'),
('0000000f-0000-0000-0000-000000000007', '0000000d-0000-0000-0000-000000000009', 'https://example.com/img/returns/fan-noisy-1.jpg', NOW() - INTERVAL '2 days'),
('0000000f-0000-0000-0000-000000000008', '0000000d-0000-0000-0000-000000000009', 'https://example.com/img/returns/fan-noisy-2.jpg', NOW() - INTERVAL '2 days'),
('0000000f-0000-0000-0000-000000000009', '0000000d-0000-0000-0000-00000000000a', 'https://example.com/img/returns/case-sai-mau-1.jpg', NOW() - INTERVAL '5 days'),
('0000000f-0000-0000-0000-00000000000a', '0000000d-0000-0000-0000-00000000000a', 'https://example.com/img/returns/case-dung-mau-1.jpg', NOW() - INTERVAL '5 days');

-- ===========================================
-- 16. CART ITEMS (12 rows)
-- ===========================================
INSERT INTO cart_items (cart_item_id, user_id, product_id, quantity, added_at) VALUES
('00000010-0000-0000-0000-000000000001', '00000001-0000-0000-0000-000000000006', '00000004-0000-0000-0000-000000000002', 1, NOW() - INTERVAL '2 days'),
('00000010-0000-0000-0000-000000000002', '00000001-0000-0000-0000-000000000006', '00000004-0000-0000-0000-000000000005', 2, NOW() - INTERVAL '2 days'),
('00000010-0000-0000-0000-000000000003', '00000001-0000-0000-0000-000000000007', '00000004-0000-0000-0000-00000000000f', 1, NOW() - INTERVAL '1 day'),
('00000010-0000-0000-0000-000000000004', '00000001-0000-0000-0000-000000000007', '00000004-0000-0000-0000-000000000007', 1, NOW() - INTERVAL '1 day'),
('00000010-0000-0000-0000-000000000005', '00000001-0000-0000-0000-000000000008', '00000004-0000-0000-0000-000000000009', 1, NOW() - INTERVAL '12 hours'),
('00000010-0000-0000-0000-000000000006', '00000001-0000-0000-0000-000000000009', '00000004-0000-0000-0000-000000000005', 1, NOW() - INTERVAL '6 hours'),
('00000010-0000-0000-0000-000000000007', '00000001-0000-0000-0000-000000000009', '00000004-0000-0000-0000-000000000006', 1, NOW() - INTERVAL '6 hours'),
('00000010-0000-0000-0000-000000000008', '00000001-0000-0000-0000-00000000000a', '00000004-0000-0000-0000-00000000000c', 1, NOW() - INTERVAL '3 hours'),
('00000010-0000-0000-0000-000000000009', '00000001-0000-0000-0000-00000000000a', '00000004-0000-0000-0000-00000000000d', 1, NOW() - INTERVAL '3 hours'),
('00000010-0000-0000-0000-00000000000a', '00000001-0000-0000-0000-000000000004', '00000004-0000-0000-0000-00000000000e', 1, NOW() - INTERVAL '1 hour'),
('00000010-0000-0000-0000-00000000000b', '00000001-0000-0000-0000-000000000005', '00000004-0000-0000-0000-00000000000b', 2, NOW() - INTERVAL '30 minutes'),
('00000010-0000-0000-0000-00000000000c', '00000001-0000-0000-0000-000000000005', '00000004-0000-0000-0000-000000000009', 1, NOW() - INTERVAL '30 minutes');

-- ===========================================
-- 17. WISHLISTS (10 rows)
-- ===========================================
INSERT INTO wishlists (wishlist_id, user_id, product_id, created_at) VALUES
('00000011-0000-0000-0000-000000000001', '00000001-0000-0000-0000-000000000004', '00000004-0000-0000-0000-000000000007', NOW() - INTERVAL '15 days'),
('00000011-0000-0000-0000-000000000002', '00000001-0000-0000-0000-000000000004', '00000004-0000-0000-0000-000000000001', NOW() - INTERVAL '14 days'),
('00000011-0000-0000-0000-000000000003', '00000001-0000-0000-0000-000000000005', '00000004-0000-0000-0000-000000000009', NOW() - INTERVAL '10 days'),
('00000011-0000-0000-0000-000000000004', '00000001-0000-0000-0000-000000000006', '00000004-0000-0000-0000-000000000008', NOW() - INTERVAL '8 days'),
('00000011-0000-0000-0000-000000000005', '00000001-0000-0000-0000-000000000006', '00000004-0000-0000-0000-000000000007', NOW() - INTERVAL '8 days'),
('00000011-0000-0000-0000-000000000006', '00000001-0000-0000-0000-000000000007', '00000004-0000-0000-0000-00000000000e', NOW() - INTERVAL '5 days'),
('00000011-0000-0000-0000-000000000007', '00000001-0000-0000-0000-000000000008', '00000004-0000-0000-0000-000000000003', NOW() - INTERVAL '3 days'),
('00000011-0000-0000-0000-000000000008', '00000001-0000-0000-0000-000000000009', '00000004-0000-0000-0000-000000000005', NOW() - INTERVAL '2 days'),
('00000011-0000-0000-0000-000000000009', '00000001-0000-0000-0000-00000000000a', '00000004-0000-0000-0000-00000000000d', NOW() - INTERVAL '1 day'),
('00000011-0000-0000-0000-00000000000a', '00000001-0000-0000-0000-00000000000a', '00000004-0000-0000-0000-000000000001', NOW()) ON CONFLICT (user_id, product_id) DO NOTHING;

-- ===========================================
-- 18. COUPON USAGES (10 rows)
-- ===========================================
INSERT INTO coupon_usages (id, coupon_id, user_id, order_id, discount_amount, used_at) VALUES
('00000012-0000-0000-0000-000000000001', '00000007-0000-0000-0000-000000000001', '00000001-0000-0000-0000-000000000004', '00000008-0000-0000-0000-000000000001', 500000, NOW() - INTERVAL '10 days'),
('00000012-0000-0000-0000-000000000002', '00000007-0000-0000-0000-000000000002', '00000001-0000-0000-0000-000000000005', '00000008-0000-0000-0000-000000000002', 200000, NOW() - INTERVAL '8 days'),
('00000012-0000-0000-0000-000000000003', '00000007-0000-0000-0000-000000000003', '00000001-0000-0000-0000-000000000005', '00000008-0000-0000-0000-00000000000a', 119850, NOW() - INTERVAL '3 days'),
('00000012-0000-0000-0000-000000000004', '00000007-0000-0000-0000-000000000005', '00000001-0000-0000-0000-000000000007', '00000008-0000-0000-0000-000000000004', 50000, NOW() - INTERVAL '3 days'),
('00000012-0000-0000-0000-000000000005', '00000007-0000-0000-0000-000000000009', '00000001-0000-0000-0000-00000000000a', '00000008-0000-0000-0000-000000000009', 500000, NOW() - INTERVAL '6 days'),
('00000012-0000-0000-0000-000000000006', '00000007-0000-0000-0000-000000000005', '00000001-0000-0000-0000-000000000005', '00000008-0000-0000-0000-00000000000a', 50000, NOW() - INTERVAL '3 days'),
('00000012-0000-0000-0000-000000000007', '00000007-0000-0000-0000-000000000007', '00000001-0000-0000-0000-000000000009', '00000008-0000-0000-0000-000000000006', 300000, NOW() - INTERVAL '7 days'),
('00000012-0000-0000-0000-000000000008', '00000007-0000-0000-0000-000000000003', '00000001-0000-0000-0000-000000000008', '00000008-0000-0000-0000-000000000005', 2000000, NOW() - INTERVAL '2 days'),
('00000012-0000-0000-0000-000000000009', '00000007-0000-0000-0000-000000000005', '00000001-0000-0000-0000-000000000004', '00000008-0000-0000-0000-000000000007', 50000, NOW() - INTERVAL '4 days'),
('00000012-0000-0000-0000-00000000000a', '00000007-0000-0000-0000-00000000000a', '00000001-0000-0000-0000-000000000004', '00000008-0000-0000-0000-000000000001', 50000, NOW() - INTERVAL '10 days');

-- ===========================================
-- 19. FLASH SALES (10 rows)
-- ===========================================
INSERT INTO flash_sales (flash_sale_id, title, start_time, end_time, is_active, created_by, created_at) VALUES
('00000013-0000-0000-0000-000000000001', 'Giờ Vàng Giảm Sốc - CPU Intel', NOW() - INTERVAL '15 days', NOW() - INTERVAL '14 days', FALSE, '00000001-0000-0000-0000-000000000001', NOW() - INTERVAL '20 days'),
('00000013-0000-0000-0000-000000000002', 'Flash Sale RAM - Giảm đến 30%', NOW() - INTERVAL '10 days', NOW() + INTERVAL '5 days', TRUE, '00000001-0000-0000-0000-000000000001', NOW() - INTERVAL '15 days'),
('00000013-0000-0000-0000-000000000003', 'Deal SSD - Nâng cấp ổ cứng', NOW() - INTERVAL '5 days', NOW() + INTERVAL '10 days', TRUE, '00000001-0000-0000-0000-000000000002', NOW() - INTERVAL '10 days'),
('00000013-0000-0000-0000-000000000004', 'Giảm Sốc VGA - Chơi Game Đỉnh', NOW() + INTERVAL '5 days', NOW() + INTERVAL '7 days', FALSE, '00000001-0000-0000-0000-000000000001', NOW() - INTERVAL '5 days'),
('00000013-0000-0000-0000-000000000005', 'Build PC Giá Rẻ - Tuần Lễ Xanh', NOW() - INTERVAL '30 days', NOW() - INTERVAL '23 days', FALSE, '00000001-0000-0000-0000-000000000003', NOW() - INTERVAL '35 days'),
('00000013-0000-0000-0000-000000000006', 'Flash Sale Chuột & Bàn Phím', NOW() - INTERVAL '3 days', NOW() + INTERVAL '4 days', TRUE, '00000001-0000-0000-0000-000000000001', NOW() - INTERVAL '7 days'),
('00000013-0000-0000-0000-000000000007', 'Mùa Hè Xả Kho - Tản Nhiệt', NOW() + INTERVAL '10 days', NOW() + INTERVAL '13 days', FALSE, '00000001-0000-0000-0000-000000000002', NOW()),
('00000013-0000-0000-0000-000000000008', 'Deal Nguồn Máy Tính Chính Hãng', NOW() - INTERVAL '2 days', NOW() + INTERVAL '5 days', TRUE, '00000001-0000-0000-0000-000000000001', NOW() - INTERVAL '7 days'),
('00000013-0000-0000-0000-000000000009', 'Flash Sale Nửa Đêm - Case & Cooling', NOW() + INTERVAL '1 day', NOW() + INTERVAL '2 days', FALSE, '00000001-0000-0000-0000-000000000003', NOW() - INTERVAL '3 days'),
('00000013-0000-0000-0000-00000000000a', 'Chuột Gaming Cao Cấp - Giảm 40%', NOW() - INTERVAL '60 days', NOW() - INTERVAL '58 days', FALSE, '00000001-0000-0000-0000-000000000001', NOW() - INTERVAL '65 days');

-- ===========================================
-- 20. FLASH SALE ITEMS (12 rows)
-- ===========================================
INSERT INTO flash_sale_items (id, flash_sale_id, product_id, flash_price, stock_limit, sold_count) VALUES
('00000014-0000-0000-0000-000000000001', '00000013-0000-0000-0000-000000000002', '00000004-0000-0000-0000-000000000005', 2690000, 20, 15),
('00000014-0000-0000-0000-000000000002', '00000013-0000-0000-0000-000000000002', '00000004-0000-0000-0000-000000000006', 1390000, 30, 22),
('00000014-0000-0000-0000-000000000003', '00000013-0000-0000-0000-000000000003', '00000004-0000-0000-0000-000000000009', 5490000, 15, 8),
('00000014-0000-0000-0000-000000000004', '00000013-0000-0000-0000-000000000003', '00000004-0000-0000-0000-00000000000a', 699000, 50, 35),
('00000014-0000-0000-0000-000000000005', '00000013-0000-0000-0000-000000000006', '00000004-0000-0000-0000-00000000000f', 2490000, 20, 12),
('00000014-0000-0000-0000-000000000006', '00000013-0000-0000-0000-000000000008', '00000004-0000-0000-0000-00000000000c', 2990000, 15, 7),
('00000014-0000-0000-0000-000000000007', '00000013-0000-0000-0000-000000000001', '00000004-0000-0000-0000-000000000001', 10990000, 10, 10),
('00000014-0000-0000-0000-000000000008', '00000013-0000-0000-0000-000000000001', '00000004-0000-0000-0000-000000000002', 11990000, 10, 6),
('00000014-0000-0000-0000-000000000009', '00000013-0000-0000-0000-000000000009', '00000004-0000-0000-0000-00000000000d', 2190000, 10, 3),
('00000014-0000-0000-0000-00000000000a', '00000013-0000-0000-0000-000000000009', '00000004-0000-0000-0000-00000000000e', 2490000, 12, 4),
('00000014-0000-0000-0000-00000000000b', '00000013-0000-0000-0000-000000000006', '00000004-0000-0000-0000-000000000003', 7990000, 8, 2),
('00000014-0000-0000-0000-00000000000c', '00000013-0000-0000-0000-000000000008', '00000004-0000-0000-0000-000000000007', 43990000, 3, 1) ON CONFLICT (flash_sale_id, product_id) DO NOTHING;

-- ===========================================
-- 21. NEWS CATEGORIES (10 rows)
-- ===========================================
INSERT INTO news_categories (category_id, name, slug, description, parent_id, is_active, created_at) VALUES
('00000015-0000-0000-0000-000000000001', 'Tin Công Nghệ', 'tin-cong-nghe', 'Tin tức công nghệ mới nhất', NULL, TRUE, NOW()),
('00000015-0000-0000-0000-000000000002', 'Đánh Giá Sản Phẩm', 'danh-gia-san-pham', 'Review chi tiết các sản phẩm công nghệ', NULL, TRUE, NOW()),
('00000015-0000-0000-0000-000000000003', 'Khuyến Mãi', 'khuyen-mai', 'Chương trình khuyến mãi, giảm giá', NULL, TRUE, NOW()),
('00000015-0000-0000-0000-000000000004', 'Hướng Dẫn', 'huong-dan', 'Hướng dẫn sử dụng, lắp ráp, cấu hình', NULL, TRUE, NOW()),
('00000015-0000-0000-0000-000000000005', 'Game', 'game', 'Tin tức và review game', NULL, TRUE, NOW()),
('00000015-0000-0000-0000-000000000006', 'Thủ Thuật Máy Tính', 'thu-thuat-may-tinh', 'Mẹo vặt, thủ thuật Windows, phần mềm', NULL, TRUE, NOW()),
('00000015-0000-0000-0000-000000000007', 'So Sánh Sản Phẩm', 'so-sanh-san-pham', 'So sánh chi tiết các sản phẩm cùng phân khúc', NULL, TRUE, NOW()),
('00000015-0000-0000-0000-000000000008', 'Laptop', 'laptop', 'Tin tức về laptop các hãng', NULL, TRUE, NOW()),
('00000015-0000-0000-0000-000000000009', 'PC - Linh Kiện', 'pc-linh-kien', 'Build PC, linh kiện máy tính', NULL, TRUE, NOW()),
('00000015-0000-0000-0000-00000000000a', 'Phụ Kiện', 'phu-kien', 'Bàn phím, chuột, tai nghe, ghế gaming', NULL, TRUE, NOW()) ON CONFLICT (slug) DO NOTHING;

-- ===========================================
-- 22. NEWS (10 rows)
-- ===========================================
INSERT INTO news (news_id, title, slug, category_id, content, excerpt, author_id, image_url, is_active, is_published, published_at, views, meta_title, meta_description, created_at, updated_at) VALUES
('00000016-0000-0000-0000-000000000001',
 'Đánh giá Intel Core i9-14900K: CPU mạnh nhất cho gaming và workstation',
 'danh-gia-intel-core-i9-14900k',
 '00000015-0000-0000-0000-000000000002',
 '<p>Intel Core i9-14900K là flagship thế hệ Raptor Lake Refresh với 24 nhân 32 luồng, xung boost lên đến 6.0GHz. So với thế hệ trước, i9-14900K có xung nhịp cao hơn 200MHz và cải thiện hiệu năng đơn luồng đáng kể.</p><p>Trong các bài test gaming, i9-14900K cho hiệu năng vượt trội so với AMD Ryzen 9 7950X ở hầu hết các tựa game. Tuy nhiên, mức tiêu thụ điện năng khá cao, đặc biệt khi ép xung.</p>',
 'Intel Core i9-14900K với 24 nhân 32 luồng, xung boost 6.0GHz - CPU mạnh nhất của Intel cho gaming và workstation.',
 '00000001-0000-0000-0000-000000000001',
 'https://example.com/img/news/i9-14900k-review.jpg',
 TRUE, TRUE, NOW() - INTERVAL '5 days', 1520,
 'Đánh giá Intel Core i9-14900K | GearVN Blog',
 'Intel Core i9-14900K review: CPU mạnh nhất Intel với 24 nhân, 6.0GHz boost. So sánh hiệu năng gaming và workstation.',
 NOW() - INTERVAL '7 days', NOW() - INTERVAL '5 days'),

('00000016-0000-0000-0000-000000000002',
 'NVIDIA RTX 4090: Card đồ họa mạnh nhất thế giới 2024',
 'nvidia-rtx-4090-card-do-hoa-manh-nhat',
 '00000015-0000-0000-0000-000000000002',
 '<p>RTX 4090 dựa trên kiến trúc Ada Lovelace với 16384 CUDA Cores và 24GB GDDR6X. Trong các bài test 4K gaming, RTX 4090 vượt xa RTX 3090 Ti từ 60-100% hiệu năng.</p><p>Công nghệ DLSS 3 với Frame Generation là điểm nhấn lớn nhất, giúp tăng gấp đôi FPS trong các tựa game hỗ trợ.</p>',
 'RTX 4090 với 16384 CUDA Cores, 24GB GDDR6X và DLSS 3 - Card đồ họa mạnh nhất thế giới hiện nay.',
 '00000001-0000-0000-0000-000000000001',
 'https://example.com/img/news/rtx4090-review.jpg',
 TRUE, TRUE, NOW() - INTERVAL '3 days', 2340,
 'NVIDIA RTX 4090 Review | GearVN Blog',
 'NVIDIA GeForce RTX 4090 Founders Edition review: sức mạnh đồ họa đỉnh cao với 24GB VRAM, DLSS 3.',
 NOW() - INTERVAL '5 days', NOW() - INTERVAL '3 days'),

('00000016-0000-0000-0000-000000000003',
 'Hướng dẫn build PC Gaming tầm trung 30 triệu đồng 2024',
 'huong-dan-build-pc-gaming-30-trieu',
 '00000015-0000-0000-0000-000000000004',
 '<p>Build PC gaming 30 triệu là phân khúc được nhiều game thủ quan tâm nhất hiện nay. Với số tiền này, bạn hoàn toàn có thể sở hữu một dàn máy chơi mượt mọi tựa game ở độ phân giải 1440p.</p><p>Cấu hình đề xuất: CPU Intel Core i5-14600K, VGA RTX 4070, RAM 32GB DDR5, SSD 1TB NVMe...</p>',
 'Hướng dẫn build PC gaming 30 triệu đồng với cấu hình chi tiết và giải thích từng linh kiện cho game thủ.',
 '00000001-0000-0000-0000-000000000002',
 'https://example.com/img/news/build-pc-guide.jpg',
 TRUE, TRUE, NOW() - INTERVAL '7 days', 3450,
 'Build PC Gaming 30 triệu | GearVN Blog',
 'Hướng dẫn build PC gaming 30 triệu đồng - cấu hình chi tiết cho game thủ 2024.',
 NOW() - INTERVAL '10 days', NOW() - INTERVAL '7 days'),

('00000016-0000-0000-0000-000000000004',
 'So sánh RAM DDR4 vs DDR5: Có nên nâng cấp?',
 'so-sanh-ram-ddr4-vs-ddr5',
 '00000015-0000-0000-0000-000000000007',
 '<p>DDR5 đã xuất hiện được hơn 2 năm và giá đã giảm đáng kể. Liệu có đáng để nâng cấp từ DDR4 lên DDR5? Bài viết này sẽ so sánh chi tiết về hiệu năng, giá cả và khả năng tương thích.</p><p>Trong các tựa game, DDR5 cho tốc độ khung hình cao hơn 5-10% so với DDR4 ở cùng độ trễ. Tuy nhiên, với các tác vụ văn phòng, sự khác biệt không đáng kể.</p>',
 'So sánh RAM DDR4 và DDR5: hiệu năng, giá cả, độ trễ - có nên nâng cấp lên DDR5 trong năm 2024?',
 '00000001-0000-0000-0000-000000000001',
 'https://example.com/img/news/ddr4-vs-ddr5.jpg',
 TRUE, TRUE, NOW() - INTERVAL '10 days', 2120,
 'So sánh DDR4 vs DDR5 | GearVN Blog',
 'So sánh RAM DDR4 và DDR5 chi tiết: hiệu năng gaming, giá bán, nên chọn loại nào cho build PC 2024?',
 NOW() - INTERVAL '12 days', NOW() - INTERVAL '10 days'),

('00000016-0000-0000-0000-000000000005',
 'Cách chọn nguồn máy tính (PSU) phù hợp cho dàn PC',
 'cach-chon-nguon-may-tinh-psu-phu-hop',
 '00000015-0000-0000-0000-000000000006',
 '<p>Nguồn máy tính là linh kiện quan trọng nhất nhưng thường bị xem nhẹ. Một bộ nguồn kém chất lượng có thể gây hại cho toàn bộ hệ thống.</p><p>Bài viết hướng dẫn cách tính công suất, chọn chứng chỉ 80+, thương hiệu uy tín và các loại modular phù hợp.</p>',
 'Hướng dẫn chọn nguồn máy tính (PSU) phù hợp: cách tính công suất, chọn 80+ Gold, modular và thương hiệu uy tín.',
 '00000001-0000-0000-0000-000000000002',
 'https://example.com/img/news/psu-guide.jpg',
 TRUE, TRUE, NOW() - INTERVAL '8 days', 980,
 'Cách chọn PSU phù hợp | GearVN Blog',
 'Hướng dẫn chọn nguồn máy tính PSU: tính công suất, chứng chỉ 80+, nguồn modular, thương hiệu uy tín.',
 NOW() - INTERVAL '10 days', NOW() - INTERVAL '8 days'),

('00000016-0000-0000-0000-000000000006',
 'Đánh giá Samsung 990 Pro 2TB: SSD NVMe tốc độ cao',
 'danh-gia-samsung-990-pro-2tb',
 '00000015-0000-0000-0000-000000000002',
 '<p>Samsung 990 Pro là SSD NVMe PCIe 4.0 hàng đầu với tốc độ đọc 7,450MB/s và ghi 6,900MB/s. Đây là lựa chọn tối ưu cho gaming và xử lý nội dung chuyên nghiệp.</p><p>Với 2TB dung lượng và TBW 1,200TB, 990 Pro đáp ứng tốt nhu cầu lưu trữ của hầu hết người dùng cao cấp.</p>',
 'Samsung 990 Pro 2TB NVMe PCIe 4.0 - Đánh giá chi tiết tốc độ, hiệu năng và độ bền của SSD flagship Samsung.',
 '00000001-0000-0000-0000-000000000001',
 'https://example.com/img/news/samsung-990pro.jpg',
 TRUE, TRUE, NOW() - INTERVAL '4 days', 1450,
 'Đánh giá Samsung 990 Pro 2TB | GearVN Blog',
 'Samsung 990 Pro 2TB NVMe SSD review: tốc độ 7,450MB/s, hiệu năng gaming, render - có đáng mua?',
 NOW() - INTERVAL '6 days', NOW() - INTERVAL '4 days'),

('00000016-0000-0000-0000-000000000007',
 'Top 10 bàn phím cơ tốt nhất 2024 - Gaming & Văn phòng',
 'top-10-ban-phim-co-tot-nhat-2024',
 '00000015-0000-0000-0000-00000000000a',
 '<p>Bàn phím cơ ngày càng phổ biến nhờ cảm giác gõ tuyệt vời và độ bền cao. Dưới đây là top 10 bàn phím cơ đáng mua nhất 2024 cho cả gaming và văn phòng.</p><p>Danh sách bao gồm: Razer Huntsman V3, Logitech G Pro X, Keychron Q-series, và nhiều lựa chọn khác...</p>',
 'Top 10 bàn phím cơ tốt nhất 2024 cho gaming và văn phòng - Razer, Logitech, Keychron, ASUS ROG.',
 '00000001-0000-0000-0000-000000000002',
 'https://example.com/img/news/top-keyboard.jpg',
 TRUE, TRUE, NOW() - INTERVAL '2 days', 3100,
 'Top 10 bàn phím cơ 2024 | GearVN Blog',
 'Top 10 bàn phím cơ tốt nhất 2024: gaming, văn phòng, dưới 1 triệu, dưới 3 triệu - đánh giá chi tiết.',
 NOW() - INTERVAL '5 days', NOW() - INTERVAL '2 days'),

('00000016-0000-0000-0000-000000000008',
 'AMD Ryzen 9 7950X Review: 16 nhân cho workstation',
 'amd-ryzen-9-7950x-review-16-nhan',
 '00000015-0000-0000-0000-000000000002',
 '<p>AMD Ryzen 9 7950X là CPU 16 nhân 32 luồng mạnh nhất của AMD trên nền tảng AM5. Với kiến trúc Zen 4, hiệu năng đa luồng vượt trội so với Intel i9-14900K trong các tác vụ render, biên tập video.</p><p>Tuy nhiên, gaming thuần túy thì i9-14900K vẫn nhỉnh hơn một chút. Lựa chọn giữa hai CPU này phụ thuộc vào nhu cầu sử dụng chính của bạn.</p>',
 'AMD Ryzen 9 7950X review: CPU 16 nhân Zen 4 cho workstation, so sánh với Intel i9-14900K.',
 '00000001-0000-0000-0000-000000000001',
 'https://example.com/img/news/r9-7950x-review.jpg',
 TRUE, TRUE, NOW() - INTERVAL '6 days', 1100,
 'AMD Ryzen 9 7950X Review | GearVN Blog',
 'AMD Ryzen 9 7950X review: 16 nhân, Zen 4, hiệu năng workstation và gaming, so sánh với Intel Core i9-14900K.',
 NOW() - INTERVAL '8 days', NOW() - INTERVAL '6 days'),

('00000016-0000-0000-0000-000000000009',
 'Hướng dẫn vệ sinh máy tính và bảo trì PC định kỳ',
 'huong-dan-ve-sinh-may-tinh-bao-tri-pc',
 '00000015-0000-0000-0000-000000000006',
 '<p>Vệ sinh máy tính định kỳ giúp tăng tuổi thọ linh kiện và giảm nhiệt độ hoạt động. Bạn nên vệ sinh 3-6 tháng một lần tùy môi trường.</p><p>Bài viết hướng dẫn chi tiết cách vệ sinh từng bộ phận: quạt, tản nhiệt, nguồn, bo mạch chủ và các linh kiện khác.</p>',
 'Hướng dẫn vệ sinh máy tính và bảo trì PC định kỳ: các bước chi tiết, dụng cụ cần thiết và lưu ý quan trọng.',
 '00000001-0000-0000-0000-000000000002',
 'https://example.com/img/news/pc-cleaning.jpg',
 TRUE, TRUE, NOW() - INTERVAL '9 days', 780,
 'Vệ sinh máy tính định kỳ | GearVN Blog',
 'Hướng dẫn vệ sinh máy tính PC đúng cách: các bước vệ sinh, bảo trì linh kiện, tản nhiệt, quạt.',
 NOW() - INTERVAL '11 days', NOW() - INTERVAL '9 days'),

('00000016-0000-0000-0000-00000000000a',
 'So sánh Intel Core i5 vs i7 vs i9: Nên chọn CPU nào?',
 'so-sanh-intel-core-i5-i7-i9',
 '00000015-0000-0000-0000-000000000007',
 '<p>Intel Core thế hệ 14 có 3 dòng chính: i5, i7 và i9. Mỗi dòng phục vụ một nhu cầu khác nhau từ cơ bản đến chuyên nghiệp.</p><p>Nếu chỉ gaming và làm việc cơ bản, i5-14600K là lựa chọn tối ưu. Nếu cần thêm hiệu năng đa nhiệm, i7-14700K với 20 nhân là đủ dùng. Còn i9-14900K dành cho những ai cần hiệu năng tuyệt đối.</p>',
 'So sánh Intel Core i5 vs i7 vs i9 chi tiết về hiệu năng, giá cả và nhu cầu sử dụng - nên chọn CPU nào?',
 '00000001-0000-0000-0000-000000000001',
 'https://example.com/img/news/i5-vs-i7-vs-i9.jpg',
 TRUE, TRUE, NOW() - INTERVAL '1 day', 2890,
 'So sánh Intel i5 i7 i9 | GearVN Blog',
 'So sánh Intel Core i5 vs i7 vs i9: hiệu năng gaming, render, giá bán - nên chọn CPU Intel nào phù hợp?',
 NOW() - INTERVAL '3 days', NOW() - INTERVAL '1 day') ON CONFLICT (slug) DO NOTHING;

-- ===========================================
-- 23. BANNERS (10 rows)
-- ===========================================
INSERT INTO banners (banner_id, title, subtitle, image_url, link_url, position, sort_order, is_active, start_date, end_date, created_at, updated_at) VALUES
('00000017-0000-0000-0000-000000000001', 'Build PC Gaming 2024', 'Cấu hình mạnh mẽ - Giá tốt nhất', 'https://example.com/img/banners/build-pc.jpg', '/build-pc', 1, 1, TRUE, CURRENT_DATE, CURRENT_DATE + INTERVAL '30 days', NOW(), NOW()),
('00000017-0000-0000-0000-000000000002', 'Intel Core i9-14900K', 'CPU mạnh nhất - Giảm đến 1 triệu', 'https://example.com/img/banners/i9-14900k.jpg', '/product/intel-core-i9-14900k', 1, 2, TRUE, CURRENT_DATE, CURRENT_DATE + INTERVAL '15 days', NOW(), NOW()),
('00000017-0000-0000-0000-000000000003', 'RTX 4090 - Sức mạnh không giới hạn', 'Card đồ họa flagship chỉ từ 46 triệu', 'https://example.com/img/banners/rtx4090.jpg', '/product/nvidia-geforce-rtx-4090-fe', 1, 3, TRUE, CURRENT_DATE, CURRENT_DATE + INTERVAL '45 days', NOW(), NOW()),
('00000017-0000-0000-0000-000000000004', 'RAM DDR5 - Giảm đến 30%', 'Nâng cấp RAM cho gaming mượt mà', 'https://example.com/img/banners/ram-d5-sale.jpg', '/product/corsair-vengeance-ddr5-32gb-6000', 2, 1, TRUE, CURRENT_DATE, CURRENT_DATE + INTERVAL '7 days', NOW(), NOW()),
('00000017-0000-0000-0000-000000000005', 'SSD Samsung 990 Pro', 'Tốc độ đọc 7450MB/s - Giá sốc', 'https://example.com/img/banners/ssd-sale.jpg', '/product/samsung-990-pro-2tb', 2, 2, TRUE, CURRENT_DATE - INTERVAL '5 days', CURRENT_DATE + INTERVAL '10 days', NOW(), NOW()),
('00000017-0000-0000-0000-000000000006', 'GearVN Summer Sale', 'Giảm đến 40% hàng ngàn sản phẩm', 'https://example.com/img/banners/summer-sale.jpg', '/product', 1, 4, FALSE, CURRENT_DATE + INTERVAL '20 days', CURRENT_DATE + INTERVAL '30 days', NOW(), NOW()),
('00000017-0000-0000-0000-000000000007', 'Mainboard ASUS ROG', 'Bo mạch chủ cao cấp cho game thủ', 'https://example.com/img/banners/asus-rog-mb.jpg', '/product/asus-rog-strix-z790-e-gaming-wifi', 2, 3, TRUE, CURRENT_DATE, CURRENT_DATE + INTERVAL '20 days', NOW(), NOW()),
('00000017-0000-0000-0000-000000000008', 'Nguồn Corsair RM850x', 'Nguồn ATX 3.0 - 80+ Gold - Bảo hành 10 năm', 'https://example.com/img/banners/corsair-psu.jpg', '/product/corsair-rm850x-shift-850w', 2, 4, TRUE, CURRENT_DATE, CURRENT_DATE + INTERVAL '14 days', NOW(), NOW()),
('00000017-0000-0000-0000-000000000009', 'NZXT H7 Flow Case', 'Case đẹp - Tản nhiệt tối ưu', 'https://example.com/img/banners/nzxt-h7.jpg', '/product/nzxt-h7-flow-white', 1, 5, TRUE, CURRENT_DATE, CURRENT_DATE + INTERVAL '25 days', NOW(), NOW()),
('00000017-0000-0000-0000-00000000000a', 'Logitech G Pro X Superlight', 'Chuột wireless 63g - Giảm 400K', 'https://example.com/img/banners/logitech-superlight.jpg', '/product/logitech-g-pro-x-superlight', 2, 5, TRUE, CURRENT_DATE, CURRENT_DATE + INTERVAL '10 days', NOW(), NOW());

-- ===========================================
-- 24. REVIEWS (10 rows)
-- ===========================================
INSERT INTO reviews (review_id, product_id, user_id, rating, comment, is_active, is_verified_purchase, created_at, updated_at) VALUES
('00000018-0000-0000-0000-000000000001', '00000004-0000-0000-0000-000000000001', '00000001-0000-0000-0000-000000000004', 5, 'CPU quá mạnh mẽ! Mình build PC mới và con chip này đáp ứng mọi nhu cầu gaming 4K và render video. Nhiệt độ idle tầm 35 độ, full load tầm 80 độ với tản AIO 360mm.', 1, TRUE, NOW() - INTERVAL '9 days', NOW() - INTERVAL '9 days'),
('00000018-0000-0000-0000-000000000002', '00000004-0000-0000-0000-000000000001', '00000001-0000-0000-0000-000000000005', 4, 'Hiệu năng xuất sắc nhưng hơi nóng. Cần đầu tư tản nhiệt tốt nếu muốn ép xung. Mình dùng Noctua NH-D15 thì idle 38 độ, gaming 70 độ.', 1, FALSE, NOW() - INTERVAL '6 days', NOW() - INTERVAL '6 days'),
('00000018-0000-0000-0000-000000000003', '00000004-0000-0000-0000-000000000002', '00000001-0000-0000-0000-000000000006', 5, 'AMD lại làm tốt! Dùng cho render 3D và dựng phim, 16 nhân 32 luồng xử lý cực nhanh. Main AM5 còn upgrade được lâu dài.', 1, TRUE, NOW() - INTERVAL '4 days', NOW() - INTERVAL '4 days'),
('00000018-0000-0000-0000-000000000004', '00000004-0000-0000-0000-000000000005', '00000001-0000-0000-0000-000000000007', 5, 'RAM DDR5 6000MHz chơi game mượt mà, latency thấp. RGB đẹp, tản nhiệt tốt. Mua 2 bộ 32GB nâng cấp lên 64GB.', 1, TRUE, NOW() - INTERVAL '7 days', NOW() - INTERVAL '7 days'),
('00000018-0000-0000-0000-000000000005', '00000004-0000-0000-0000-000000000007', '00000001-0000-0000-0000-000000000008', 5, 'RTX 4090 đúng là quái vật! Cyberpunk 2077 4K DLSS 3 Ray Tracing Ultra ra hơn 100fps. Nhiệt độ max 72 độ, khá ấn tượng với card 450W.', 1, TRUE, NOW() - INTERVAL '8 days', NOW() - INTERVAL '8 days'),
('00000018-0000-0000-0000-000000000006', '00000004-0000-0000-0000-000000000009', '00000001-0000-0000-0000-000000000009', 5, 'SSD cực nhanh, boot Windows trong 5 giây. Tốc độ đọc thực tế đạt 7.1GB/s. Nhiệt độ có heatsink tầm 45 độ khi hoạt động.', 1, TRUE, NOW() - INTERVAL '3 days', NOW() - INTERVAL '3 days'),
('00000018-0000-0000-0000-000000000007', '00000004-0000-0000-0000-00000000000c', '00000001-0000-0000-0000-00000000000a', 5, 'Nguồn Corsair chất lượng như mong đợi. Cable modular dễ dàng quản lý dây. Quạt 135mm chạy rất êm. Bảo hành 10 năm yên tâm.', 1, TRUE, NOW() - INTERVAL '5 days', NOW() - INTERVAL '5 days'),
('00000018-0000-0000-0000-000000000008', '00000004-0000-0000-0000-00000000000d', '00000001-0000-0000-0000-000000000004', 4, 'Case NZXT H7 Flow đẹp, tản nhiệt tốt nhờ mặt trước mesh. Tuy nhiên giá hơi cao so với các case cùng phân khúc. Kính cường lực dễ bám bụi.', 1, FALSE, NOW() - INTERVAL '2 days', NOW() - INTERVAL '2 days'),
('00000018-0000-0000-0000-000000000009', '00000004-0000-0000-0000-00000000000e', '00000001-0000-0000-0000-000000000006', 5, 'Tản nhiệt khí ngon nhất mình từng dùng. Cân tốt i9-14900K mà không cần AIO. Quạt màu đen Chromax sang trọng, dễ phối màu case.', 1, TRUE, NOW() - INTERVAL '3 days', NOW() - INTERVAL '3 days'),
('00000018-0000-0000-0000-00000000000a', '00000004-0000-0000-0000-00000000000f', '00000001-0000-0000-0000-000000000007', 5, 'Chuột siêu nhẹ 63g nhưng cảm giác vẫn chắc chắn. Sensor HERO 25K chính xác tuyệt đối. Pin dùng 2 tuần mới sạc. Không dây mà không cảm nhận delay.', 1, TRUE, NOW() - INTERVAL '1 day', NOW() - INTERVAL '1 day');

-- ===========================================
-- 25. REVIEW IMAGES (10 rows)
-- ===========================================
INSERT INTO review_images (image_id, review_id, image_url, created_at) VALUES
('00000019-0000-0000-0000-000000000001', '00000018-0000-0000-0000-000000000001', 'https://example.com/img/reviews/i9-installed.jpg', NOW() - INTERVAL '9 days'),
('00000019-0000-0000-0000-000000000002', '00000018-0000-0000-0000-000000000001', 'https://example.com/img/reviews/cinebench-score.jpg', NOW() - INTERVAL '9 days'),
('00000019-0000-0000-0000-000000000003', '00000018-0000-0000-0000-000000000005', 'https://example.com/img/reviews/rtx4090-build.jpg', NOW() - INTERVAL '8 days'),
('00000019-0000-0000-0000-000000000004', '00000018-0000-0000-0000-000000000005', 'https://example.com/img/reviews/cp2077-4k-fps.jpg', NOW() - INTERVAL '8 days'),
('00000019-0000-0000-0000-000000000005', '00000018-0000-0000-0000-000000000005', 'https://example.com/img/reviews/rtx4090-temp.jpg', NOW() - INTERVAL '8 days'),
('00000019-0000-0000-0000-000000000006', '00000018-0000-0000-0000-000000000006', 'https://example.com/img/reviews/ssd-speed-test.jpg', NOW() - INTERVAL '3 days'),
('00000019-0000-0000-0000-000000000007', '00000018-0000-0000-0000-000000000007', 'https://example.com/img/reviews/psu-installed.jpg', NOW() - INTERVAL '5 days'),
('00000019-0000-0000-0000-000000000008', '00000018-0000-0000-0000-000000000009', 'https://example.com/img/reviews/nh-d15-build.jpg', NOW() - INTERVAL '3 days'),
('00000019-0000-0000-0000-000000000009', '00000018-0000-0000-0000-00000000000a', 'https://example.com/img/reviews/superlight-box.jpg', NOW() - INTERVAL '1 day'),
('00000019-0000-0000-0000-00000000000a', '00000018-0000-0000-0000-000000000004', 'https://example.com/img/reviews/ddr5-rgb.jpg', NOW() - INTERVAL '7 days');

-- ===========================================
-- 26. REVIEW HELPFUL VOTES (10 rows)
-- ===========================================
INSERT INTO review_helpful_votes (vote_id, review_id, user_id, created_at) VALUES
('0000001a-0000-0000-0000-000000000001', '00000018-0000-0000-0000-000000000001', '00000001-0000-0000-0000-000000000006', NOW() - INTERVAL '8 days'),
('0000001a-0000-0000-0000-000000000002', '00000018-0000-0000-0000-000000000001', '00000001-0000-0000-0000-000000000007', NOW() - INTERVAL '8 days'),
('0000001a-0000-0000-0000-000000000003', '00000018-0000-0000-0000-000000000001', '00000001-0000-0000-0000-000000000008', NOW() - INTERVAL '7 days'),
('0000001a-0000-0000-0000-000000000004', '00000018-0000-0000-0000-000000000005', '00000001-0000-0000-0000-000000000009', NOW() - INTERVAL '7 days'),
('0000001a-0000-0000-0000-000000000005', '00000018-0000-0000-0000-000000000005', '00000001-0000-0000-0000-00000000000a', NOW() - INTERVAL '6 days'),
('0000001a-0000-0000-0000-000000000006', '00000018-0000-0000-0000-000000000003', '00000001-0000-0000-0000-000000000004', NOW() - INTERVAL '3 days'),
('0000001a-0000-0000-0000-000000000007', '00000018-0000-0000-0000-000000000004', '00000001-0000-0000-0000-000000000005', NOW() - INTERVAL '6 days'),
('0000001a-0000-0000-0000-000000000008', '00000018-0000-0000-0000-000000000006', '00000001-0000-0000-0000-000000000004', NOW() - INTERVAL '2 days'),
('0000001a-0000-0000-0000-000000000009', '00000018-0000-0000-0000-000000000007', '00000001-0000-0000-0000-000000000006', NOW() - INTERVAL '4 days'),
('0000001a-0000-0000-0000-00000000000a', '00000018-0000-0000-0000-00000000000a', '00000001-0000-0000-0000-000000000008', NOW() - INTERVAL '1 day'),
('0000001a-0000-0000-0000-00000000000b', '00000018-0000-0000-0000-000000000005', '00000001-0000-0000-0000-000000000004', NOW() - INTERVAL '7 days') ON CONFLICT (review_id, user_id) DO NOTHING;

-- ===========================================
-- 27. REVIEW REPLIES (10 rows)
-- ===========================================
INSERT INTO review_replies (reply_id, review_id, user_id, content, is_active, created_at, updated_at) VALUES
('0000001b-0000-0000-0000-000000000001', '00000018-0000-0000-0000-000000000001', '00000001-0000-0000-0000-000000000002', 'Cảm ơn bạn đã tin tưởng mua hàng tại GearVN! Chúc bạn có những trải nghiệm tuyệt vời với CPU i9-14900K.', 1, NOW() - INTERVAL '8 days', NOW() - INTERVAL '8 days'),
('0000001b-0000-0000-0000-000000000002', '00000018-0000-0000-0000-000000000002', '00000001-0000-0000-0000-000000000002', 'Bạn có thể tham khảo thêm tản AIO 360mm của Corsair hoặc NZXT để tối ưu nhiệt độ cho i9-14900K nhé!', 1, NOW() - INTERVAL '5 days', NOW() - INTERVAL '5 days'),
('0000001b-0000-0000-0000-000000000003', '00000018-0000-0000-0000-000000000003', '00000001-0000-0000-0000-000000000001', 'Cảm ơn bạn đã chia sẻ! Ryzen 9 7950X quả thực rất mạnh cho công việc render và dựng phim.', 1, NOW() - INTERVAL '3 days', NOW() - INTERVAL '3 days'),
('0000001b-0000-0000-0000-000000000004', '00000018-0000-0000-0000-000000000004', '00000001-0000-0000-0000-000000000003', 'Cảm ơn bạn. RAM Corsair Vengeance được khách hàng đánh giá rất tốt về độ ổn định và RGB đẹp.', 1, NOW() - INTERVAL '6 days', NOW() - INTERVAL '6 days'),
('0000001b-0000-0000-0000-000000000005', '00000018-0000-0000-0000-000000000005', '00000001-0000-0000-0000-000000000002', 'RTX 4090 quả thực là một quái vật! GearVN có hỗ trợ trả góp 0% cho dòng sản phẩm này bạn nhé.', 1, NOW() - INTERVAL '7 days', NOW() - INTERVAL '7 days'),
('0000001b-0000-0000-0000-000000000006', '00000018-0000-0000-0000-000000000006', '00000001-0000-0000-0000-000000000002', 'Samsung 990 Pro đúng là SSD đáng mua nhất hiện nay. Nên kết hợp với heatsink trên mainboard để đạt hiệu năng tối đa bạn nhé.', 1, NOW() - INTERVAL '2 days', NOW() - INTERVAL '2 days'),
('0000001b-0000-0000-0000-000000000007', '00000018-0000-0000-0000-000000000007', '00000001-0000-0000-0000-000000000001', 'Cảm ơn bạn đã đánh giá! Corsair RM850x là một trong những nguồn bán chạy nhất tại GearVN nhờ độ ổn định và chế độ bảo hành 10 năm.', 1, NOW() - INTERVAL '4 days', NOW() - INTERVAL '4 days'),
('0000001b-0000-0000-0000-000000000008', '00000018-0000-0000-0000-000000000008', '00000001-0000-0000-0000-000000000003', 'Case NZXT H7 Flow có bản màu đen và trắng. Bạn nên dùng thêm quạt NZXT F120 RGB để tăng tính thẩm mỹ nhé!', 1, NOW() - INTERVAL '1 day', NOW() - INTERVAL '1 day'),
('0000001b-0000-0000-0000-000000000009', '00000018-0000-0000-0000-000000000009', '00000001-0000-0000-0000-000000000002', 'Noctua NH-D15 Chromax vừa ra mắt bản màu trắng, bạn có thể tham khảo thêm nếu build case màu trắng.', 1, NOW() - INTERVAL '2 days', NOW() - INTERVAL '2 days'),
('0000001b-0000-0000-0000-00000000000a', '00000018-0000-0000-0000-00000000000a', '00000001-0000-0000-0000-000000000001', 'Logitech G Pro X Superlight là chuột được nhiều pro gamer sử dụng nhất hiện nay. GearVN có hỗ trợ đổi trong 30 ngày nếu không ưng ý!', 1, NOW(), NOW());

-- ===========================================
-- 28. SUPPLIERS (10 rows)
-- ===========================================
INSERT INTO suppliers (supplier_id, name, contact_name, phone, email, address, is_active, created_at, updated_at) VALUES
('0000001c-0000-0000-0000-000000000001', 'Intel Việt Nam', 'Nguyễn Văn A', '024-1234-5678', 'intel.vn@intel.com', 'Tầng 10, Tòa nhà Keangnam, Hà Nội', TRUE, NOW(), NOW()),
('0000001c-0000-0000-0000-000000000002', 'AMD Việt Nam (Đại lý)', 'Mr. David Wilson', '028-9876-5432', 'amd-vn@amd.com', 'Tầng 5, Tòa nhà Bitexco, TP Hồ Chí Minh', TRUE, NOW(), NOW()),
('0000001c-0000-0000-0000-000000000003', 'Synnex FPT - Nhà phân phối ASUS', 'Trần Minh Đức', '024-4567-8901', 'synnex.fpt@synnex.com', 'Tòa nhà FPT, Cầu Giấy, Hà Nội', TRUE, NOW(), NOW()),
('0000001c-0000-0000-0000-000000000004', 'NVIDIA Partner Việt Nam', 'Lê Hoàng Anh', '028-2345-6789', 'nvidia.partner@nvidia.com', 'Tầng 8, Tòa nhà Saigon Centre, TP Hồ Chí Minh', TRUE, NOW(), NOW()),
('0000001c-0000-0000-0000-000000000005', 'Samsung Electronics VN', 'Park Min Jun', '024-7890-1234', 'samsung.vn@samsung.com', 'Khu công nghệ cao, Thủ Đức, TP Hồ Chí Minh', TRUE, NOW(), NOW()),
('0000001c-0000-0000-0000-000000000006', 'Corsair Asia Distribution', 'Michael Chen', '+65-6789-0123', 'corsair.dist@corsair.com', 'Số 123, Orchard Road, Singapore', TRUE, NOW(), NOW()),
('0000001c-0000-0000-0000-000000000007', 'Western Digital VN', 'Ngô Văn Khoa', '024-5678-9012', 'wd-vn@westerndigital.com', 'Tầng 12, Handico Tower, Hà Nội', TRUE, NOW(), NOW()),
('0000001c-0000-0000-0000-000000000008', 'Kingston Technology VN', 'Vũ Thị Hoa', '028-3456-7890', 'kingston.vn@kingston.com', 'Tòa nhà Pearl Plaza, Bình Thạnh, TP Hồ Chí Minh', TRUE, NOW(), NOW()),
('0000001c-0000-0000-0000-000000000009', 'Logitech Vietnam', 'Đặng Quốc Bảo', '024-9012-3456', 'logitech.vn@logitech.com', 'Tầng 7, tòa nhà Capital Place, Hà Nội', TRUE, NOW(), NOW()),
('0000001c-0000-0000-0000-00000000000a', 'Công ty TNHH GearVN Import', 'Hoàng Văn Phú', '028-8901-2345', 'import@gearvn.id.vn', 'Số 456, Nguyễn Văn Linh, Quận 7, TP Hồ Chí Minh', TRUE, NOW(), NOW());

-- ===========================================
-- 29. INVENTORY RECEIPTS (10 rows)
-- ===========================================
INSERT INTO inventory_receipts (receipt_id, receipt_code, supplier_id, created_by, total_amount, notes, status, created_at, updated_at) VALUES
('0000001d-0000-0000-0000-000000000001', 'NCC-2024-001', '0000001c-0000-0000-0000-000000000001', '00000001-0000-0000-0000-000000000002', 299750000, 'Nhập đợt 1 CPU Intel i9-14900K', 2, NOW() - INTERVAL '20 days', NOW() - INTERVAL '18 days'),
('0000001d-0000-0000-0000-000000000002', 'NCC-2024-002', '0000001c-0000-0000-0000-000000000002', '00000001-0000-0000-0000-000000000002', 259800000, 'Nhập CPU AMD Ryzen 9 7950X', 2, NOW() - INTERVAL '18 days', NOW() - INTERVAL '16 days'),
('0000001d-0000-0000-0000-000000000003', 'NCC-2024-003', '0000001c-0000-0000-0000-000000000003', '00000001-0000-0000-0000-000000000002', 127350000, 'Nhập Mainboard ASUS ROG Strix Z790-E', 2, NOW() - INTERVAL '15 days', NOW() - INTERVAL '13 days'),
('0000001d-0000-0000-0000-000000000004', 'NCC-2024-004', '0000001c-0000-0000-0000-000000000006', '00000001-0000-0000-0000-000000000003', 131600000, 'Nhập RAM Corsair Vengeance DDR5', 2, NOW() - INTERVAL '12 days', NOW() - INTERVAL '10 days'),
('0000001d-0000-0000-0000-000000000005', 'NCC-2024-005', '0000001c-0000-0000-0000-000000000008', '00000001-0000-0000-0000-000000000003', 84500000, 'Nhập RAM Kingston Fury DDR4', 2, NOW() - INTERVAL '10 days', NOW() - INTERVAL '8 days'),
('0000001d-0000-0000-0000-000000000006', 'NCC-2024-006', '0000001c-0000-0000-0000-000000000004', '00000001-0000-0000-0000-000000000002', 229950000, 'Nhập VGA RTX 4090 FE', 2, NOW() - INTERVAL '25 days', NOW() - INTERVAL '23 days'),
('0000001d-0000-0000-0000-000000000007', 'NCC-2024-007', '0000001c-0000-0000-0000-000000000005', '00000001-0000-0000-0000-000000000003', 129800000, 'Nhập SSD Samsung 990 Pro 2TB', 2, NOW() - INTERVAL '8 days', NOW() - INTERVAL '6 days'),
('0000001d-0000-0000-0000-000000000008', 'NCC-2024-008', '0000001c-0000-0000-0000-000000000007', '00000001-0000-0000-0000-000000000002', 43500000, 'Nhập HDD Seagate Barracuda 2TB', 2, NOW() - INTERVAL '6 days', NOW() - INTERVAL '4 days'),
('0000001d-0000-0000-0000-000000000009', 'NCC-2024-009', '0000001c-0000-0000-0000-000000000006', '00000001-0000-0000-0000-000000000003', 89875000, 'Nhập PSU Corsair RM850x', 2, NOW() - INTERVAL '5 days', NOW() - INTERVAL '3 days'),
('0000001d-0000-0000-0000-00000000000a', 'NCC-2024-010', '0000001c-0000-0000-0000-000000000009', '00000001-0000-0000-0000-000000000002', 86700000, 'Nhập Chuột Logitech GPX Superlight', 1, NOW() - INTERVAL '2 days', NOW()) ON CONFLICT (receipt_code) DO NOTHING;

-- ===========================================
-- 30. INVENTORY RECEIPT ITEMS (15 rows)
-- ===========================================
INSERT INTO inventory_receipt_items (item_id, receipt_id, product_id, quantity, unit_price, total_price) VALUES
('0000001e-0000-0000-0000-000000000001', '0000001d-0000-0000-0000-000000000001', '00000004-0000-0000-0000-000000000001', 25, 11990000, 299750000),
('0000001e-0000-0000-0000-000000000002', '0000001d-0000-0000-0000-000000000002', '00000004-0000-0000-0000-000000000002', 20, 12990000, 259800000),
('0000001e-0000-0000-0000-000000000003', '0000001d-0000-0000-0000-000000000003', '00000004-0000-0000-0000-000000000003', 15, 8490000, 127350000),
('0000001e-0000-0000-0000-000000000004', '0000001d-0000-0000-0000-000000000004', '00000004-0000-0000-0000-000000000005', 40, 3290000, 131600000),
('0000001e-0000-0000-0000-000000000005', '0000001d-0000-0000-0000-000000000005', '00000004-0000-0000-0000-000000000006', 50, 1690000, 84500000),
('0000001e-0000-0000-0000-000000000006', '0000001d-0000-0000-0000-000000000006', '00000004-0000-0000-0000-000000000007', 5, 45990000, 229950000),
('0000001e-0000-0000-0000-000000000007', '0000001d-0000-0000-0000-000000000007', '00000004-0000-0000-0000-000000000009', 20, 6490000, 129800000),
('0000001e-0000-0000-0000-000000000008', '0000001d-0000-0000-0000-000000000008', '00000004-0000-0000-0000-00000000000b', 30, 1450000, 43500000),
('0000001e-0000-0000-0000-000000000009', '0000001d-0000-0000-0000-000000000009', '00000004-0000-0000-0000-00000000000c', 25, 3595000, 89875000),
('0000001e-0000-0000-0000-00000000000a', '0000001d-0000-0000-0000-00000000000a', '00000004-0000-0000-0000-00000000000f', 30, 2890000, 86700000),
('0000001e-0000-0000-0000-00000000000b', '0000001d-0000-0000-0000-000000000003', '00000004-0000-0000-0000-000000000004', 18, 5190000, 93420000),
('0000001e-0000-0000-0000-00000000000c', '0000001d-0000-0000-0000-000000000009', '00000004-0000-0000-0000-000000000007', 3, 45990000, 137970000),
('0000001e-0000-0000-0000-00000000000d', '0000001d-0000-0000-0000-000000000006', '00000004-0000-0000-0000-00000000000e', 10, 2990000, 29900000),
('0000001e-0000-0000-0000-00000000000e', '0000001d-0000-0000-0000-000000000004', '00000004-0000-0000-0000-00000000000d', 12, 2590000, 31080000),
('0000001e-0000-0000-0000-00000000000f', '0000001d-0000-0000-0000-000000000007', '00000004-0000-0000-0000-00000000000a', 60, 799000, 47940000);

-- ===========================================
-- 31. INVENTORY TRANSACTIONS (15 rows)
-- transaction_type: 1=Nhập kho, 2=Xuất bán, 3=Hoàn hàng, 4=Xuất hủy
-- ===========================================
INSERT INTO inventory_transactions (transaction_id, product_id, transaction_type, reference_id, quantity_changed, stock_after, created_by, notes, created_at) VALUES
('0000001f-0000-0000-0000-000000000001', '00000004-0000-0000-0000-000000000001', 1, '0000001d-0000-0000-0000-000000000001', 25, 25, '00000001-0000-0000-0000-000000000002', 'Nhập kho CPU Intel i9-14900K', NOW() - INTERVAL '20 days'),
('0000001f-0000-0000-0000-000000000002', '00000004-0000-0000-0000-000000000001', 2, '00000008-0000-0000-0000-000000000001', -1, 24, '00000001-0000-0000-0000-000000000003', 'Xuất bán đơn ORD-000001', NOW() - INTERVAL '10 days'),
('0000001f-0000-0000-0000-000000000003', '00000004-0000-0000-0000-000000000002', 1, '0000001d-0000-0000-0000-000000000002', 20, 20, '00000001-0000-0000-0000-000000000002', 'Nhập kho CPU AMD Ryzen 9 7950X', NOW() - INTERVAL '18 days'),
('0000001f-0000-0000-0000-000000000004', '00000004-0000-0000-0000-000000000002', 2, '00000008-0000-0000-0000-000000000003', -1, 19, '00000001-0000-0000-0000-000000000003', 'Xuất bán đơn ORD-000003', NOW() - INTERVAL '5 days'),
('0000001f-0000-0000-0000-000000000005', '00000004-0000-0000-0000-000000000007', 1, '0000001d-0000-0000-0000-000000000006', 5, 5, '00000001-0000-0000-0000-000000000002', 'Nhập kho RTX 4090 FE', NOW() - INTERVAL '25 days'),
('0000001f-0000-0000-0000-000000000006', '00000004-0000-0000-0000-000000000007', 2, '00000008-0000-0000-0000-000000000001', -1, 4, '00000001-0000-0000-0000-000000000003', 'Xuất bán đơn ORD-000001', NOW() - INTERVAL '10 days'),
('0000001f-0000-0000-0000-000000000007', '00000004-0000-0000-0000-000000000005', 1, '0000001d-0000-0000-0000-000000000004', 40, 40, '00000001-0000-0000-0000-000000000003', 'Nhập kho RAM Corsair DDR5', NOW() - INTERVAL '12 days'),
('0000001f-0000-0000-0000-000000000008', '00000004-0000-0000-0000-000000000005', 2, '00000008-0000-0000-0000-000000000002', -1, 39, '00000001-0000-0000-0000-000000000002', 'Xuất bán đơn ORD-000002', NOW() - INTERVAL '8 days'),
('0000001f-0000-0000-0000-000000000009', '00000004-0000-0000-0000-000000000006', 3, '0000000d-0000-0000-0000-000000000001', 2, 50, '00000001-0000-0000-0000-000000000002', 'Hoàn hàng Kingston DDR4 từ yêu cầu trả lại RM-000001', NOW() - INTERVAL '5 days'),
('0000001f-0000-0000-0000-00000000000a', '00000004-0000-0000-0000-000000000009', 1, '0000001d-0000-0000-0000-000000000007', 20, 20, '00000001-0000-0000-0000-000000000003', 'Nhập kho SSD Samsung 990 Pro', NOW() - INTERVAL '8 days'),
('0000001f-0000-0000-0000-00000000000b', '00000004-0000-0000-0000-000000000009', 2, '00000008-0000-0000-0000-000000000008', -1, 19, '00000001-0000-0000-0000-000000000003', 'Xuất bán đơn ORD-000008', NOW() - INTERVAL '1 day'),
('0000001f-0000-0000-0000-00000000000c', '00000004-0000-0000-0000-00000000000c', 1, '0000001d-0000-0000-0000-000000000009', 25, 25, '00000001-0000-0000-0000-000000000003', 'Nhập kho PSU Corsair RM850x', NOW() - INTERVAL '5 days'),
('0000001f-0000-0000-0000-00000000000d', '00000004-0000-0000-0000-00000000000c', 2, '00000008-0000-0000-0000-000000000005', -1, 24, '00000001-0000-0000-0000-000000000002', 'Xuất bán đơn ORD-000005', NOW() - INTERVAL '2 days'),
('0000001f-0000-0000-0000-00000000000e', '00000004-0000-0000-0000-00000000000f', 1, '0000001d-0000-0000-0000-00000000000a', 30, 30, '00000001-0000-0000-0000-000000000002', 'Nhập kho Logitech GPX Superlight', NOW() - INTERVAL '2 days'),
('0000001f-0000-0000-0000-00000000000f', '00000004-0000-0000-0000-00000000000f', 2, '00000008-0000-0000-0000-000000000004', -1, 29, '00000001-0000-0000-0000-000000000003', 'Xuất bán đơn ORD-000004', NOW() - INTERVAL '3 days');


-- ===========================================
-- 31b. MISSING INVENTORY TRANSACTIONS (28 rows)
-- Added to fix incomplete inventory tracking
-- RECEIPT (type=1): 11 rows, SALE (type=2): 12 rows, RETURN (type=3): 5 rows
-- ===========================================

-- Missing RECEIPT transactions (11 rows)
INSERT INTO inventory_transactions (transaction_id, product_id, transaction_type, reference_id, quantity_changed, stock_after, created_by, notes, created_at)
VALUES
-- Receipt 3: ASUS Z790-E +15
('0000001f-0000-0000-0000-000000000010', (SELECT product_id FROM products WHERE slug = 'asus-rog-strix-z790-e-gaming-wifi'),
 1, (SELECT receipt_id FROM inventory_receipts WHERE receipt_code = 'NCC-2024-003'), 15, 15,
 '00000001-0000-0000-0000-000000000002', 'Nhập kho Mainboard ASUS ROG Strix Z790-E', NOW() - INTERVAL '15 days'),
-- Receipt 3: MSI B650 +18
('0000001f-0000-0000-0000-000000000011', (SELECT product_id FROM products WHERE slug = 'msi-mag-b650-tomahawk-wifi'),
 1, (SELECT receipt_id FROM inventory_receipts WHERE receipt_code = 'NCC-2024-003'), 18, 18,
 '00000001-0000-0000-0000-000000000002', 'Nhập kho Mainboard MSI MAG B650', NOW() - INTERVAL '15 days'),
-- Receipt 4: NZXT H7 Flow +12
('0000001f-0000-0000-0000-000000000012', (SELECT product_id FROM products WHERE slug = 'nzxt-h7-flow-white'),
 1, (SELECT receipt_id FROM inventory_receipts WHERE receipt_code = 'NCC-2024-004'), 12, 12,
 '00000001-0000-0000-0000-000000000003', 'Nhập kho Case NZXT H7 Flow White', NOW() - INTERVAL '12 days'),
-- Receipt 5: Kingston DDR4 +50
('0000001f-0000-0000-0000-000000000013', (SELECT product_id FROM products WHERE slug = 'kingston-fury-beast-ddr4-32gb-3200'),
 1, (SELECT receipt_id FROM inventory_receipts WHERE receipt_code = 'NCC-2024-005'), 50, 50,
 '00000001-0000-0000-0000-000000000003', 'Nhập kho RAM Kingston Fury DDR4', NOW() - INTERVAL '10 days'),
-- Receipt 6: Noctua NH-D15 +10
('0000001f-0000-0000-0000-000000000014', (SELECT product_id FROM products WHERE slug = 'noctua-nh-d15-chromax-black'),
 1, (SELECT receipt_id FROM inventory_receipts WHERE receipt_code = 'NCC-2024-006'), 10, 10,
 '00000001-0000-0000-0000-000000000002', 'Nhập kho Tản nhiệt Noctua NH-D15', NOW() - INTERVAL '25 days'),
-- Receipt 7: Kingston A400 +60
('0000001f-0000-0000-0000-000000000015', (SELECT product_id FROM products WHERE slug = 'kingston-a400-480gb'),
 1, (SELECT receipt_id FROM inventory_receipts WHERE receipt_code = 'NCC-2024-007'), 60, 60,
 '00000001-0000-0000-0000-000000000003', 'Nhập kho SSD Kingston A400 480GB', NOW() - INTERVAL '8 days'),
-- Receipt 8: Seagate 2TB +30
('0000001f-0000-0000-0000-000000000016', (SELECT product_id FROM products WHERE slug = 'seagate-barracuda-2tb'),
 1, (SELECT receipt_id FROM inventory_receipts WHERE receipt_code = 'NCC-2024-008'), 30, 30,
 '00000001-0000-0000-0000-000000000002', 'Nhập kho HDD Seagate Barracuda 2TB', NOW() - INTERVAL '6 days'),
-- Receipt 9: RTX 4090 +3
('0000001f-0000-0000-0000-000000000017', (SELECT product_id FROM products WHERE slug = 'nvidia-geforce-rtx-4090-fe'),
 1, (SELECT receipt_id FROM inventory_receipts WHERE receipt_code = 'NCC-2024-009'), 3, 3,
 '00000001-0000-0000-0000-000000000003', 'Nhập kho RTX 4090 FE đợt 2', NOW() - INTERVAL '5 days');

-- Missing SALE transactions (12 rows)
INSERT INTO inventory_transactions (transaction_id, product_id, transaction_type, reference_id, quantity_changed, stock_after, created_by, notes, created_at)
VALUES
-- Order 1: Logitech GPX Superlight -1
('0000001f-0000-0000-0000-000000000018', (SELECT product_id FROM products WHERE slug = 'logitech-g-pro-x-superlight'),
 2, (SELECT order_id FROM orders WHERE order_code = 'ORD-000001'), -1, 29,
 '00000001-0000-0000-0000-000000000003', 'Xuất bán đơn ORD-000001', NOW() - INTERVAL '10 days'),
-- Order 2: Kingston DDR4 -2
('0000001f-0000-0000-0000-000000000019', (SELECT product_id FROM products WHERE slug = 'kingston-fury-beast-ddr4-32gb-3200'),
 2, (SELECT order_id FROM orders WHERE order_code = 'ORD-000002'), -2, 48,
 '00000001-0000-0000-0000-000000000002', 'Xuất bán đơn ORD-000002', NOW() - INTERVAL '8 days'),
-- Order 3: Noctua NH-D15 -1
('0000001f-0000-0000-0000-00000000001a', (SELECT product_id FROM products WHERE slug = 'noctua-nh-d15-chromax-black'),
 2, (SELECT order_id FROM orders WHERE order_code = 'ORD-000003'), -1, 9,
 '00000001-0000-0000-0000-000000000003', 'Xuất bán đơn ORD-000003', NOW() - INTERVAL '5 days'),
-- Order 4: MSI B650 -1
('0000001f-0000-0000-0000-00000000001b', (SELECT product_id FROM products WHERE slug = 'msi-mag-b650-tomahawk-wifi'),
 2, (SELECT order_id FROM orders WHERE order_code = 'ORD-000004'), -1, 17,
 '00000001-0000-0000-0000-000000000003', 'Xuất bán đơn ORD-000004', NOW() - INTERVAL '3 days'),
-- Order 5: RTX 4090 -1
('0000001f-0000-0000-0000-00000000001c', (SELECT product_id FROM products WHERE slug = 'nvidia-geforce-rtx-4090-fe'),
 2, (SELECT order_id FROM orders WHERE order_code = 'ORD-000005'), -1, 6,
 '00000001-0000-0000-0000-000000000002', 'Xuất bán đơn ORD-000005', NOW() - INTERVAL '2 days'),
-- Order 6: Corsair DDR5 -1
('0000001f-0000-0000-0000-00000000001d', (SELECT product_id FROM products WHERE slug = 'corsair-vengeance-ddr5-32gb-6000'),
 2, (SELECT order_id FROM orders WHERE order_code = 'ORD-000006'), -1, 38,
 '00000001-0000-0000-0000-000000000002', 'Xuất bán đơn ORD-000006', NOW() - INTERVAL '7 days'),
-- Order 7: Seagate 2TB -1
('0000001f-0000-0000-0000-00000000001e', (SELECT product_id FROM products WHERE slug = 'seagate-barracuda-2tb'),
 2, (SELECT order_id FROM orders WHERE order_code = 'ORD-000007'), -1, 29,
 '00000001-0000-0000-0000-000000000003', 'Xuất bán đơn ORD-000007', NOW() - INTERVAL '4 days'),
-- Order 9: ASUS Z790-E -1
('0000001f-0000-0000-0000-00000000001f', (SELECT product_id FROM products WHERE slug = 'asus-rog-strix-z790-e-gaming-wifi'),
 2, (SELECT order_id FROM orders WHERE order_code = 'ORD-000009'), -1, 14,
 '00000001-0000-0000-0000-000000000003', 'Xuất bán đơn ORD-000009', NOW() - INTERVAL '6 days'),
-- Order 9: NZXT H7 Flow -1
('0000001f-0000-0000-0000-000000000020', (SELECT product_id FROM products WHERE slug = 'nzxt-h7-flow-white'),
 2, (SELECT order_id FROM orders WHERE order_code = 'ORD-000009'), -1, 11,
 '00000001-0000-0000-0000-000000000003', 'Xuất bán đơn ORD-000009', NOW() - INTERVAL '6 days'),
-- Order 9: Corsair RM850x -1
('0000001f-0000-0000-0000-000000000021', (SELECT product_id FROM products WHERE slug = 'corsair-rm850x-shift-850w'),
 2, (SELECT order_id FROM orders WHERE order_code = 'ORD-000009'), -1, 23,
 '00000001-0000-0000-0000-000000000003', 'Xuất bán đơn ORD-000009', NOW() - INTERVAL '6 days'),
-- Order 10: Kingston A400 -1
('0000001f-0000-0000-0000-000000000022', (SELECT product_id FROM products WHERE slug = 'kingston-a400-480gb'),
 2, (SELECT order_id FROM orders WHERE order_code = 'ORD-000010'), -1, 59,
 '00000001-0000-0000-0000-000000000003', 'Xuất bán đơn ORD-000010', NOW() - INTERVAL '3 days');

-- Missing RETURN transactions (5 rows, only for approved/completed returns)
INSERT INTO inventory_transactions (transaction_id, product_id, transaction_type, reference_id, quantity_changed, stock_after, created_by, notes, created_at)
VALUES
-- Return 2 (completed): Corsair DDR5 +1
('0000001f-0000-0000-0000-000000000023', (SELECT product_id FROM products WHERE slug = 'corsair-vengeance-ddr5-32gb-6000'),
 3, (SELECT return_id FROM return_requests LIMIT 1 OFFSET 1), 1, 39,
 '00000001-0000-0000-0000-000000000002', 'Hoàn hàng Corsair DDR5 từ yêu cầu trả lại RM-000002', NOW() - INTERVAL '4 days'),
-- Return 3 (approved): Kingston A400 +1
('0000001f-0000-0000-0000-000000000024', (SELECT product_id FROM products WHERE slug = 'kingston-a400-480gb'),
 3, (SELECT return_id FROM return_requests LIMIT 1 OFFSET 2), 1, 60,
 '00000001-0000-0000-0000-000000000001', 'Hoàn hàng Kingston A400 từ yêu cầu trả lại RM-000003', NOW() - INTERVAL '1 day'),
-- Return 7 (approved): Seagate 2TB +1
('0000001f-0000-0000-0000-000000000025', (SELECT product_id FROM products WHERE slug = 'seagate-barracuda-2tb'),
 3, (SELECT return_id FROM return_requests LIMIT 1 OFFSET 6), 1, 30,
 '00000001-0000-0000-0000-000000000002', 'Hoàn hàng Seagate 2TB từ yêu cầu trả lại RM-000007', NOW() - INTERVAL '1 day'),
-- Return 10 (completed): NZXT H7 Flow +1
('0000001f-0000-0000-0000-000000000026', (SELECT product_id FROM products WHERE slug = 'nzxt-h7-flow-white'),
 3, (SELECT return_id FROM return_requests LIMIT 1 OFFSET 9), 1, 12,
 '00000001-0000-0000-0000-000000000001', 'Hoàn hàng NZXT H7 từ yêu cầu trả lại RM-000010', NOW() - INTERVAL '3 days');
