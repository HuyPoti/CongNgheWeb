-- ===========================================
-- VERIFICATION QUERIES
-- Run after seed_data.sql to validate data integrity
-- ===========================================

-- ===========================================
-- 1. ROW COUNTS PER TABLE
-- ===========================================
SELECT 'users' AS table_name, COUNT(*) AS row_count FROM users
UNION ALL SELECT 'brands', COUNT(*) FROM brands
UNION ALL SELECT 'categories', COUNT(*) FROM categories
UNION ALL SELECT 'products', COUNT(*) FROM products
UNION ALL SELECT 'product_images', COUNT(*) FROM product_images
UNION ALL SELECT 'addresses', COUNT(*) FROM addresses
UNION ALL SELECT 'coupons', COUNT(*) FROM coupons
UNION ALL SELECT 'orders', COUNT(*) FROM orders
UNION ALL SELECT 'order_items', COUNT(*) FROM order_items
UNION ALL SELECT 'payments', COUNT(*) FROM payments
UNION ALL SELECT 'order_status_history', COUNT(*) FROM order_status_history
UNION ALL SELECT 'shipments', COUNT(*) FROM shipments
UNION ALL SELECT 'return_requests', COUNT(*) FROM return_requests
UNION ALL SELECT 'return_request_items', COUNT(*) FROM return_request_items
UNION ALL SELECT 'return_request_images', COUNT(*) FROM return_request_images
UNION ALL SELECT 'cart_items', COUNT(*) FROM cart_items
UNION ALL SELECT 'wishlists', COUNT(*) FROM wishlists
UNION ALL SELECT 'coupon_usages', COUNT(*) FROM coupon_usages
UNION ALL SELECT 'flash_sales', COUNT(*) FROM flash_sales
UNION ALL SELECT 'flash_sale_items', COUNT(*) FROM flash_sale_items
UNION ALL SELECT 'news_categories', COUNT(*) FROM news_categories
UNION ALL SELECT 'news', COUNT(*) FROM news
UNION ALL SELECT 'banners', COUNT(*) FROM banners
UNION ALL SELECT 'reviews', COUNT(*) FROM reviews
UNION ALL SELECT 'review_images', COUNT(*) FROM review_images
UNION ALL SELECT 'review_helpful_votes', COUNT(*) FROM review_helpful_votes
UNION ALL SELECT 'review_replies', COUNT(*) FROM review_replies
UNION ALL SELECT 'suppliers', COUNT(*) FROM suppliers
UNION ALL SELECT 'inventory_receipts', COUNT(*) FROM inventory_receipts
UNION ALL SELECT 'inventory_receipt_items', COUNT(*) FROM inventory_receipt_items
UNION ALL SELECT 'inventory_transactions', COUNT(*) FROM inventory_transactions
ORDER BY table_name;

-- ===========================================
-- 2. FK INTEGRITY CHECK (Orphan Records)
-- ===========================================
-- Users with non-existent references (none expected)
SELECT 'users - created_by in coupons' AS check_name, COUNT(*) AS orphans
FROM coupons c LEFT JOIN users u ON c.created_by = u.user_id WHERE c.created_by IS NOT NULL AND u.user_id IS NULL

UNION ALL
SELECT 'users - changed_by in order_status_history', COUNT(*)
FROM order_status_history o LEFT JOIN users u ON o.changed_by = u.user_id WHERE u.user_id IS NULL

UNION ALL
SELECT 'users - author_id in news', COUNT(*)
FROM news n LEFT JOIN users u ON n.author_id = u.user_id WHERE u.user_id IS NULL

UNION ALL
SELECT 'categories - parent_id', COUNT(*)
FROM categories c LEFT JOIN categories p ON c.parent_id = p.category_id WHERE c.parent_id IS NOT NULL AND p.category_id IS NULL

UNION ALL
SELECT 'brands - brand_id in products', COUNT(*)
FROM products p LEFT JOIN brands b ON p.brand_id = b.brand_id WHERE b.brand_id IS NULL

UNION ALL
SELECT 'categories - category_id in products', COUNT(*)
FROM products p LEFT JOIN categories c ON p.category_id = c.category_id WHERE c.category_id IS NULL

UNION ALL
SELECT 'products - product_id in order_items', COUNT(*)
FROM order_items o LEFT JOIN products p ON o.product_id = p.product_id WHERE p.product_id IS NULL

UNION ALL
SELECT 'orders - order_id in payments', COUNT(*)
FROM payments p LEFT JOIN orders o ON p.order_id = o.order_id WHERE o.order_id IS NULL

UNION ALL
SELECT 'addresses - address_id in orders', COUNT(*)
FROM orders o LEFT JOIN addresses a ON o.shipping_address_id = a.address_id WHERE a.address_id IS NULL;

-- ===========================================
-- 3. UNIQUE CONSTRAINT VIOLATIONS (should be 0)
-- ===========================================
SELECT 'duplicate emails' AS check_name, email, COUNT(*)
FROM users GROUP BY email HAVING COUNT(*) > 1

UNION ALL
SELECT 'duplicate brand slugs', slug, COUNT(*)
FROM brands GROUP BY slug HAVING COUNT(*) > 1

UNION ALL
SELECT 'duplicate category slugs', slug, COUNT(*)
FROM categories GROUP BY slug HAVING COUNT(*) > 1

UNION ALL
SELECT 'duplicate product slugs', slug, COUNT(*)
FROM products GROUP BY slug HAVING COUNT(*) > 1

UNION ALL
SELECT 'duplicate coupon codes', code, COUNT(*)
FROM coupons GROUP BY code HAVING COUNT(*) > 1

UNION ALL
SELECT 'duplicate order codes', order_code, COUNT(*)
FROM orders GROUP BY order_code HAVING COUNT(*) > 1

UNION ALL
SELECT 'duplicate news slugs', slug, COUNT(*)
FROM news GROUP BY slug HAVING COUNT(*) > 1

UNION ALL
SELECT 'duplicate receipt codes', receipt_code, COUNT(*)
FROM inventory_receipts GROUP BY receipt_code HAVING COUNT(*) > 1

UNION ALL
SELECT 'duplicate product SKUs', sku, COUNT(*)
FROM products WHERE sku IS NOT NULL GROUP BY sku HAVING COUNT(*) > 1;

-- ===========================================
-- 4. SPECIFIC DATA SPOT CHECKS
-- ===========================================
-- Verify admin user exists
SELECT 'admin user' AS check_name, full_name, email, role FROM users WHERE email = 'admin@gearvn.id.vn';

-- Verify all 16 brands
SELECT 'brand count' AS check_name, COUNT(*) FROM brands;

-- Verify categories with hierarchy
SELECT 'parent categories' AS check_name, COUNT(*) FROM categories WHERE parent_id IS NULL
UNION ALL
SELECT 'child categories', COUNT(*) FROM categories WHERE parent_id IS NOT NULL;

-- Verify published products
SELECT 'published products' AS check_name, COUNT(*) FROM products WHERE status = 2;

-- Verify order distribution by status
SELECT 'order status 1 (pending)' AS check_name, COUNT(*) FROM orders WHERE status = 1
UNION ALL
SELECT 'order status 2 (confirmed)', COUNT(*) FROM orders WHERE status = 2
UNION ALL
SELECT 'order status 3 (processing)', COUNT(*) FROM orders WHERE status = 3
UNION ALL
SELECT 'order status 4 (shipping)', COUNT(*) FROM orders WHERE status = 4
UNION ALL
SELECT 'order status 5 (delivered)', COUNT(*) FROM orders WHERE status = 5
UNION ALL
SELECT 'order status 6 (cancelled)', COUNT(*) FROM orders WHERE status = 6;

-- Verify reviews have correct ratings
SELECT 'rating 5 stars' AS check_name, COUNT(*) FROM reviews WHERE rating = 5
UNION ALL
SELECT 'rating 4 stars', COUNT(*) FROM reviews WHERE rating = 4
UNION ALL
SELECT 'rating < 4', COUNT(*) FROM reviews WHERE rating < 4;

-- ===========================================
-- 5. SUMMARY
-- ===========================================
SELECT 'ALL CHECKS PASSED' AS status;

-- ===========================================
-- 6. FINANCIAL INTEGRITY CHECKS
-- ===========================================

-- 6a. Verify orders.total_amount == SUM(order_items.qty * unit_price)
SELECT '--- ORDER TOTAL AMOUNT CHECK ---' AS section;

WITH items_total AS (
    SELECT oi.order_id,
           SUM(oi.quantity * oi.unit_price) AS computed_items_total
    FROM order_items oi
    GROUP BY oi.order_id
)
SELECT o.order_code,
       o.total_amount AS stored_total,
       it.computed_items_total,
       CASE
           WHEN o.total_amount = it.computed_items_total THEN 'OK'
           ELSE 'MISMATCH'
       END AS status
FROM orders o
JOIN items_total it ON o.order_id = it.order_id
ORDER BY o.order_code;

-- 6b. Verify payments.amount == orders.total_amount
SELECT '--- PAYMENT AMOUNT CHECK ---' AS section;

SELECT o.order_code,
       p.amount AS payment_amount,
       o.total_amount AS order_total,
       CASE
           WHEN p.amount = o.total_amount THEN 'OK'
           ELSE 'MISMATCH'
       END AS status
FROM payments p
JOIN orders o ON p.order_id = o.order_id
ORDER BY o.order_code;

-- 6c. Summary
SELECT
    (SELECT COUNT(*) FROM (
        SELECT o.order_id FROM orders o
        JOIN order_items oi ON o.order_id = oi.order_id
        GROUP BY o.order_id
        HAVING o.total_amount != SUM(oi.quantity * oi.unit_price)
    ) AS bad) AS orders_with_wrong_total,
    (SELECT COUNT(*) FROM (
        SELECT p.payment_id FROM payments p
        JOIN orders o ON p.order_id = o.order_id
        WHERE p.amount != o.total_amount
    ) AS bad) AS payments_with_wrong_amount,
    CASE WHEN
        (SELECT COUNT(*) FROM (
            SELECT o.order_id FROM orders o
            JOIN order_items oi ON o.order_id = oi.order_id
            GROUP BY o.order_id
            HAVING o.total_amount != SUM(oi.quantity * oi.unit_price)
        ) AS bad) = 0
        AND
        (SELECT COUNT(*) FROM (
            SELECT p.payment_id FROM payments p
            JOIN orders o ON p.order_id = o.order_id
            WHERE p.amount != o.total_amount
        ) AS bad) = 0
    THEN 'ALL FINANCIAL INTEGRITY CHECKS PASSED'
    ELSE 'SOME CHECKS FAILED - review details above'
    END AS final_verdict;
