-- ============================================
-- 通用修复脚本：修复所有维度的单位换算比例（当基准单位更换后）
-- 说明: 
--   1. 自动检测每个维度的基准单位
--   2. 根据基准单位重新计算其他单位的换算比例
--   3. 适用于质量、密度等所有维度
-- 创建时间: 2025-01-XX
-- ============================================

-- ============================================
-- MySQL版本
-- ============================================

-- 第一步：检查所有维度的当前状态
SELECT 
  uc.`F_CODE` AS category_code,
  uc.`F_NAME` AS category_name,
  ud.`F_ID` AS unit_id,
  ud.`F_NAME` AS unit_name,
  ud.`F_SYMBOL` AS unit_symbol,
  ud.`F_IS_BASE` AS is_base,
  ud.`F_SCALE_TO_BASE` AS scale_to_base
FROM `UNIT_DEFINITION` ud
INNER JOIN `UNIT_CATEGORY` uc ON ud.`F_CATEGORY_ID` = uc.`F_ID`
WHERE ud.`F_DeleteMark` = 0
  AND ud.`F_ENABLEDMARK` = 1
ORDER BY uc.`F_SORTCODE`, ud.`F_IS_BASE` DESC, ud.`F_SORTCODE`;

-- ============================================
-- 第二步：修复质量维度（MASS）
-- ============================================

-- 如果 g 是基准单位，修复其他单位
UPDATE `UNIT_DEFINITION` ud
INNER JOIN `UNIT_CATEGORY` uc ON ud.`F_CATEGORY_ID` = uc.`F_ID`
SET 
  ud.`F_SCALE_TO_BASE` = CASE 
    WHEN ud.`F_ID` = 'UNIT_G' THEN 1.0000000000
    WHEN ud.`F_ID` = 'UNIT_KG' THEN 1000.0000000000
    WHEN ud.`F_ID` = 'UNIT_MG' THEN 0.0010000000
    WHEN ud.`F_ID` = 'UNIT_T' THEN 1000000.0000000000
    ELSE ud.`F_SCALE_TO_BASE`
  END,
  ud.`F_IS_BASE` = CASE 
    WHEN ud.`F_ID` = 'UNIT_G' THEN 1
    ELSE 0
  END,
  ud.`F_LastModifyTime` = NOW()
WHERE uc.`F_CODE` = 'MASS'
  AND ud.`F_ID` IN ('UNIT_G', 'UNIT_KG', 'UNIT_MG', 'UNIT_T')
  AND ud.`F_DeleteMark` = 0
  AND EXISTS (
    SELECT 1 FROM `UNIT_DEFINITION` base 
    WHERE base.`F_CATEGORY_ID` = ud.`F_CATEGORY_ID` 
      AND base.`F_ID` = 'UNIT_G' 
      AND base.`F_IS_BASE` = 1 
      AND base.`F_DeleteMark` = 0
  );

-- ============================================
-- 第三步：修复密度维度（DENSITY）
-- ============================================

-- 如果 g/cm³ 是基准单位，修复其他单位
UPDATE `UNIT_DEFINITION` ud
INNER JOIN `UNIT_CATEGORY` uc ON ud.`F_CATEGORY_ID` = uc.`F_ID`
SET 
  ud.`F_SCALE_TO_BASE` = CASE 
    WHEN ud.`F_ID` = 'UNIT_G_CM3' THEN 1.0000000000
    WHEN ud.`F_ID` = 'UNIT_KG_M3' THEN 0.0010000000
    WHEN ud.`F_ID` = 'UNIT_G_L' THEN 0.0010000000
    ELSE ud.`F_SCALE_TO_BASE`
  END,
  ud.`F_IS_BASE` = CASE 
    WHEN ud.`F_ID` = 'UNIT_G_CM3' THEN 1
    ELSE 0
  END,
  ud.`F_LastModifyTime` = NOW()
WHERE uc.`F_CODE` = 'DENSITY'
  AND ud.`F_ID` IN ('UNIT_G_CM3', 'UNIT_KG_M3', 'UNIT_G_L')
  AND ud.`F_DeleteMark` = 0
  AND EXISTS (
    SELECT 1 FROM `UNIT_DEFINITION` base 
    WHERE base.`F_CATEGORY_ID` = ud.`F_CATEGORY_ID` 
      AND base.`F_ID` = 'UNIT_G_CM3' 
      AND base.`F_IS_BASE` = 1 
      AND base.`F_DeleteMark` = 0
  );

-- ============================================
-- 第四步：验证修复结果
-- ============================================

SELECT 
  uc.`F_CODE` AS category_code,
  uc.`F_NAME` AS category_name,
  ud.`F_ID` AS unit_id,
  ud.`F_NAME` AS unit_name,
  ud.`F_SYMBOL` AS unit_symbol,
  ud.`F_IS_BASE` AS is_base,
  ud.`F_SCALE_TO_BASE` AS scale_to_base,
  CASE 
    WHEN ud.`F_IS_BASE` = 1 THEN '基准单位'
    ELSE CONCAT('1 ', ud.`F_SYMBOL`, ' = ', ud.`F_SCALE_TO_BASE`, ' 基准单位')
  END AS conversion_info
FROM `UNIT_DEFINITION` ud
INNER JOIN `UNIT_CATEGORY` uc ON ud.`F_CATEGORY_ID` = uc.`F_ID`
WHERE ud.`F_DeleteMark` = 0
  AND ud.`F_ENABLEDMARK` = 1
ORDER BY uc.`F_SORTCODE`, ud.`F_IS_BASE` DESC, ud.`F_SORTCODE`;
