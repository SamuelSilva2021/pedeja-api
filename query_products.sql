-- Query to check how image URLs are stored in products table
SELECT 
    id, 
    name, 
    image_url,
    CASE 
        WHEN image_url LIKE '%/%' THEN 'Contains path separator'
        ELSE 'Filename only'
    END as path_type
FROM products 
WHERE image_url IS NOT NULL 
LIMIT 10;
