-- Script para atualizar os caminhos das imagens dos produtos existentes
-- Os produtos foram criados em 2025-08, então as imagens estão em products/2025/08/

UPDATE products 
SET image_url = CONCAT('products/2025/08/', image_url)
WHERE image_url IS NOT NULL 
  AND image_url != ''
  AND image_url NOT LIKE '%/%'  -- Só atualiza se não contém barra (não é um path completo)
  AND created_at >= '2025-08-01';

-- Verificar o resultado
SELECT id, name, image_url, created_at 
FROM products 
WHERE image_url IS NOT NULL 
ORDER BY id;
