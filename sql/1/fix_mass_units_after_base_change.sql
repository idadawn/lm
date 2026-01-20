-- ============================================
-- 修复质量单位换算比例（当基准单位从 kg 改为 g 后）
-- 说明: 
--   1. 如果 g 是基准单位，需要修复其他单位的换算比例
--   2. 如果 kg 不是基准单位，需要修复它的换算比例
-- 创建时间: 2025-01-XX
-- ============================================

-- ============================================
-- MySQL版本
-- ============================================

-- 检查当前状态
SELECT 
  ud.`F_ID` AS unit_id,
  ud.`F_NAME` AS unit_name,
  ud.`F_SYMBOL` AS unit_symbol,
  ud.`F_IS_BASE` AS is_base,
  ud.`F_SCALE_TO_BASE` AS scale_to_base,
  uc.`F_NAME` AS category_name
FROM `UNIT_DEFINITION` ud
INNER JOIN `UNIT_CATEGORY` uc ON ud.`F_CATEGORY_ID` = uc.`F_ID`
WHERE uc.`F_CODE` = 'MASS'
  AND ud.`F_DeleteMark` = 0
  AND ud.`F_ENABLEDMARK` = 1
ORDER BY ud.`F_SORTCODE`;

-- ============================================
-- 修复方案1: 如果 g 是基准单位，修复其他单位
-- ============================================

-- 假设 g 是基准单位（F_IS_BASE = 1），需要修复：
-- 1. kg 的换算比例应该是 1000（1 kg = 1000 g）
-- 2. mg 的换算比例应该是 0.001（1 mg = 0.001 g）
-- 3. t 的换算比例应该是 1000000（1 t = 1000000 g）

-- 修复 g（确保它是基准单位，换算比例为1）
UPDATE `UNIT_DEFINITION` 
SET 
  `F_SCALE_TO_BASE` = 1.0000000000,
  `F_IS_BASE` = 1,
  `F_LastModifyTime` = NOW()
WHERE `F_ID` = 'UNIT_G'
  AND `F_DeleteMark` = 0;

-- 修复 kg（1 kg = 1000 g）
UPDATE `UNIT_DEFINITION` 
SET 
  `F_SCALE_TO_BASE` = 1000.0000000000,
  `F_IS_BASE` = 0,
  `F_LastModifyTime` = NOW()
WHERE `F_ID` = 'UNIT_KG'
  AND `F_DeleteMark` = 0;

-- 修复 mg（1 mg = 0.001 g）
UPDATE `UNIT_DEFINITION` 
SET 
  `F_SCALE_TO_BASE` = 0.0010000000,
  `F_IS_BASE` = 0,
  `F_LastModifyTime` = NOW()
WHERE `F_ID` = 'UNIT_MG'
  AND `F_DeleteMark` = 0;

-- 修复 t（1 t = 1000000 g）
UPDATE `UNIT_DEFINITION` 
SET 
  `F_SCALE_TO_BASE` = 1000000.0000000000,
  `F_IS_BASE` = 0,
  `F_LastModifyTime` = NOW()
WHERE `F_ID` = 'UNIT_T'
  AND `F_DeleteMark` = 0;

-- ============================================
-- 修复方案2: 如果 kg 应该是基准单位，恢复原始状态
-- ============================================

/*
-- 恢复 kg 为基准单位
UPDATE `UNIT_DEFINITION` 
SET 
  `F_SCALE_TO_BASE` = 1.0000000000,
  `F_IS_BASE` = 1,
  `F_LastModifyTime` = NOW()
WHERE `F_ID` = 'UNIT_KG'
  AND `F_DeleteMark` = 0;

-- 修复 g（1 g = 0.001 kg）
UPDATE `UNIT_DEFINITION` 
SET 
  `F_SCALE_TO_BASE` = 0.0010000000,
  `F_IS_BASE` = 0,
  `F_LastModifyTime` = NOW()
WHERE `F_ID` = 'UNIT_G'
  AND `F_DeleteMark` = 0;

-- 修复 mg（1 mg = 0.000001 kg）
UPDATE `UNIT_DEFINITION` 
SET 
  `F_SCALE_TO_BASE` = 0.0000010000,
  `F_IS_BASE` = 0,
  `F_LastModifyTime` = NOW()
WHERE `F_ID` = 'UNIT_MG'
  AND `F_DeleteMark` = 0;

-- 修复 t（1 t = 1000 kg）
UPDATE `UNIT_DEFINITION` 
SET 
  `F_SCALE_TO_BASE` = 1000.0000000000,
  `F_IS_BASE` = 0,
  `F_LastModifyTime` = NOW()
WHERE `F_ID` = 'UNIT_T'
  AND `F_DeleteMark` = 0;
*/

-- ============================================
-- 验证修复结果
-- ============================================

SELECT 
  ud.`F_ID` AS unit_id,
  ud.`F_NAME` AS unit_name,
  ud.`F_SYMBOL` AS unit_symbol,
  ud.`F_IS_BASE` AS is_base,
  ud.`F_SCALE_TO_BASE` AS scale_to_base,
  CASE 
    WHEN ud.`F_IS_BASE` = 1 THEN '基准单位'
    ELSE CONCAT('1 ', ud.`F_SYMBOL`, ' = ', ud.`F_SCALE_TO_BASE`, ' 基准单位')
  END AS conversion_info,
  uc.`F_NAME` AS category_name
FROM `UNIT_DEFINITION` ud
INNER JOIN `UNIT_CATEGORY` uc ON ud.`F_CATEGORY_ID` = uc.`F_ID`
WHERE uc.`F_CODE` = 'MASS'
  AND ud.`F_DeleteMark` = 0
  AND ud.`F_ENABLEDMARK` = 1
ORDER BY ud.`F_IS_BASE` DESC, ud.`F_SORTCODE`;

-- ============================================
-- 换算验证示例
-- ============================================

/*
-- 验证换算（以 g 为基准单位）：
-- 1 g = 1 g ✓
-- 1 kg = 1000 g ✓
-- 1 mg = 0.001 g ✓
-- 1 t = 1000000 g ✓

-- 验证反向换算：
-- 1 kg = ? g: 1 kg * 1000 = 1000 g ✓
-- 1 mg = ? g: 1 mg * 0.001 = 0.001 g ✓
-- 1 t = ? g: 1 t * 1000000 = 1000000 g ✓
*/
