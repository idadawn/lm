-- ============================================
-- 快速添加/更新密度单位 g/cm³
-- 说明: 如果单位已存在则更新，不存在则插入
-- 创建时间: 2025-01-XX
-- ============================================

-- ============================================
-- MySQL版本
-- ============================================

-- 方法1: 使用 INSERT ... ON DUPLICATE KEY UPDATE（需要主键或唯一索引）
-- 注意：如果 F_ID 是主键，可以直接使用

-- 先检查密度维度是否存在，如果不存在则创建
INSERT INTO `UNIT_CATEGORY` (`F_ID`, `F_NAME`, `F_CODE`, `F_DESCRIPTION`, `F_SORTCODE`, `F_CREATORTIME`, `F_ENABLEDMARK`, `F_DeleteMark`) 
VALUES ('CAT_DENSITY', '密度', 'DENSITY', '密度单位维度', 3, NOW(), 1, 0)
ON DUPLICATE KEY UPDATE 
  `F_NAME` = '密度',
  `F_CODE` = 'DENSITY',
  `F_DESCRIPTION` = '密度单位维度',
  `F_SORTCODE` = 3,
  `F_ENABLEDMARK` = 1,
  `F_DeleteMark` = 0;

-- 添加或更新 g/cm³ 单位
-- 换算关系：1 g/cm³ = 1000 kg/m³（基准单位是 kg/m³）
INSERT INTO `UNIT_DEFINITION` (
  `F_ID`, 
  `F_CATEGORY_ID`, 
  `F_NAME`, 
  `F_SYMBOL`, 
  `F_IS_BASE`, 
  `F_SCALE_TO_BASE`, 
  `F_OFFSET`, 
  `F_PRECISION`, 
  `F_SORTCODE`, 
  `F_CREATORTIME`, 
  `F_ENABLEDMARK`, 
  `F_DeleteMark`
) VALUES (
  'UNIT_G_CM3', 
  'CAT_DENSITY', 
  '克/立方厘米', 
  'g/cm³', 
  0, 
  1000.0000000000,  -- 1 g/cm³ = 1000 kg/m³
  0.0000000000, 
  2, 
  2, 
  NOW(), 
  1, 
  0
)
ON DUPLICATE KEY UPDATE
  `F_CATEGORY_ID` = 'CAT_DENSITY',
  `F_NAME` = '克/立方厘米',
  `F_SYMBOL` = 'g/cm³',
  `F_IS_BASE` = 0,
  `F_SCALE_TO_BASE` = 1000.0000000000,
  `F_OFFSET` = 0.0000000000,
  `F_PRECISION` = 2,
  `F_SORTCODE` = 2,
  `F_ENABLEDMARK` = 1,
  `F_DeleteMark` = 0,
  `F_LastModifyTime` = NOW();

-- ============================================
-- 方法2: 如果数据库不支持 ON DUPLICATE KEY UPDATE，可以使用以下方式
-- ============================================

/*
-- 先删除（如果存在）
DELETE FROM `UNIT_DEFINITION` WHERE `F_ID` = 'UNIT_G_CM3' AND `F_DeleteMark` = 0;

-- 再插入
INSERT INTO `UNIT_DEFINITION` (
  `F_ID`, 
  `F_CATEGORY_ID`, 
  `F_NAME`, 
  `F_SYMBOL`, 
  `F_IS_BASE`, 
  `F_SCALE_TO_BASE`, 
  `F_OFFSET`, 
  `F_PRECISION`, 
  `F_SORTCODE`, 
  `F_CREATORTIME`, 
  `F_ENABLEDMARK`, 
  `F_DeleteMark`
) VALUES (
  'UNIT_G_CM3', 
  'CAT_DENSITY', 
  '克/立方厘米', 
  'g/cm³', 
  0, 
  1000.0000000000,
  0.0000000000, 
  2, 
  2, 
  NOW(), 
  1, 
  0
);
*/

-- ============================================
-- 方法3: 使用 REPLACE INTO（如果主键存在则替换）
-- ============================================

/*
REPLACE INTO `UNIT_DEFINITION` (
  `F_ID`, 
  `F_CATEGORY_ID`, 
  `F_NAME`, 
  `F_SYMBOL`, 
  `F_IS_BASE`, 
  `F_SCALE_TO_BASE`, 
  `F_OFFSET`, 
  `F_PRECISION`, 
  `F_SORTCODE`, 
  `F_CREATORTIME`, 
  `F_ENABLEDMARK`, 
  `F_DeleteMark`
) VALUES (
  'UNIT_G_CM3', 
  'CAT_DENSITY', 
  '克/立方厘米', 
  'g/cm³', 
  0, 
  1000.0000000000,
  0.0000000000, 
  2, 
  2, 
  NOW(), 
  1, 
  0
);
*/

-- ============================================
-- 验证查询：检查单位是否添加成功
-- ============================================

-- 查询密度维度的所有单位
SELECT 
  ud.`F_ID` AS unit_id,
  ud.`F_NAME` AS unit_name,
  ud.`F_SYMBOL` AS unit_symbol,
  ud.`F_IS_BASE` AS is_base,
  ud.`F_SCALE_TO_BASE` AS scale_to_base,
  ud.`F_PRECISION` AS precision,
  uc.`F_NAME` AS category_name
FROM `UNIT_DEFINITION` ud
INNER JOIN `UNIT_CATEGORY` uc ON ud.`F_CATEGORY_ID` = uc.`F_ID`
WHERE uc.`F_CODE` = 'DENSITY'
  AND ud.`F_DeleteMark` = 0
  AND ud.`F_ENABLEDMARK` = 1
ORDER BY ud.`F_SORTCODE`;
