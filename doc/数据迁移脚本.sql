-- ============================================
-- 原始数据导入优化 - 数据迁移脚本
-- 数据库类型：MySQL
-- 创建日期：2025-01-XX
-- 说明：将Detection1-22字段迁移到JSON字段，设置有效数据标识，修复中间数据表炉号
-- ============================================

-- ============================================
-- 1. 将Detection1-22字段迁移到JSON字段（原始数据表）
-- ============================================
-- 注意：执行前请备份数据库！

UPDATE LAB_RAW_DATA
SET F_DETECTION_DATA = JSON_OBJECT(
    IFNULL(F_DETECTION_1, NULL), IFNULL(F_DETECTION_1, NULL),
    IFNULL(F_DETECTION_2, NULL), IFNULL(F_DETECTION_2, NULL),
    IFNULL(F_DETECTION_3, NULL), IFNULL(F_DETECTION_3, NULL),
    IFNULL(F_DETECTION_4, NULL), IFNULL(F_DETECTION_4, NULL),
    IFNULL(F_DETECTION_5, NULL), IFNULL(F_DETECTION_5, NULL),
    IFNULL(F_DETECTION_6, NULL), IFNULL(F_DETECTION_6, NULL),
    IFNULL(F_DETECTION_7, NULL), IFNULL(F_DETECTION_7, NULL),
    IFNULL(F_DETECTION_8, NULL), IFNULL(F_DETECTION_8, NULL),
    IFNULL(F_DETECTION_9, NULL), IFNULL(F_DETECTION_9, NULL),
    IFNULL(F_DETECTION_10, NULL), IFNULL(F_DETECTION_10, NULL),
    IFNULL(F_DETECTION_11, NULL), IFNULL(F_DETECTION_11, NULL),
    IFNULL(F_DETECTION_12, NULL), IFNULL(F_DETECTION_12, NULL),
    IFNULL(F_DETECTION_13, NULL), IFNULL(F_DETECTION_13, NULL),
    IFNULL(F_DETECTION_14, NULL), IFNULL(F_DETECTION_14, NULL),
    IFNULL(F_DETECTION_15, NULL), IFNULL(F_DETECTION_15, NULL),
    IFNULL(F_DETECTION_16, NULL), IFNULL(F_DETECTION_16, NULL),
    IFNULL(F_DETECTION_17, NULL), IFNULL(F_DETECTION_17, NULL),
    IFNULL(F_DETECTION_18, NULL), IFNULL(F_DETECTION_18, NULL),
    IFNULL(F_DETECTION_19, NULL), IFNULL(F_DETECTION_19, NULL),
    IFNULL(F_DETECTION_20, NULL), IFNULL(F_DETECTION_20, NULL),
    IFNULL(F_DETECTION_21, NULL), IFNULL(F_DETECTION_21, NULL),
    IFNULL(F_DETECTION_22, NULL), IFNULL(F_DETECTION_22, NULL)
)
WHERE F_DETECTION_DATA IS NULL;

-- 更精确的迁移方式（只迁移有值的列）：
-- 注意：MySQL的JSON_OBJECT需要成对的键值，上面的方式可能不正确
-- 建议使用存储过程或应用程序进行迁移，或者使用以下方式：

-- 方式1：使用存储过程（推荐）
DELIMITER $$

CREATE PROCEDURE MigrateDetectionDataToJson()
BEGIN
    DECLARE done INT DEFAULT FALSE;
    DECLARE v_id VARCHAR(50);
    DECLARE v_detection1, v_detection2, v_detection3, v_detection4, v_detection5 DECIMAL(18,2);
    DECLARE v_detection6, v_detection7, v_detection8, v_detection9, v_detection10 DECIMAL(18,2);
    DECLARE v_detection11, v_detection12, v_detection13, v_detection14, v_detection15 DECIMAL(18,2);
    DECLARE v_detection16, v_detection17, v_detection18, v_detection19, v_detection20 DECIMAL(18,2);
    DECLARE v_detection21, v_detection22 DECIMAL(18,2);
    DECLARE v_json TEXT;
    
    DECLARE cur CURSOR FOR 
        SELECT F_ID, F_DETECTION_1, F_DETECTION_2, F_DETECTION_3, F_DETECTION_4, F_DETECTION_5,
               F_DETECTION_6, F_DETECTION_7, F_DETECTION_8, F_DETECTION_9, F_DETECTION_10,
               F_DETECTION_11, F_DETECTION_12, F_DETECTION_13, F_DETECTION_14, F_DETECTION_15,
               F_DETECTION_16, F_DETECTION_17, F_DETECTION_18, F_DETECTION_19, F_DETECTION_20,
               F_DETECTION_21, F_DETECTION_22
        FROM LAB_RAW_DATA
        WHERE F_DETECTION_DATA IS NULL;
    
    DECLARE CONTINUE HANDLER FOR NOT FOUND SET done = TRUE;
    
    OPEN cur;
    
    read_loop: LOOP
        FETCH cur INTO v_id, v_detection1, v_detection2, v_detection3, v_detection4, v_detection5,
                       v_detection6, v_detection7, v_detection8, v_detection9, v_detection10,
                       v_detection11, v_detection12, v_detection13, v_detection14, v_detection15,
                       v_detection16, v_detection17, v_detection18, v_detection19, v_detection20,
                       v_detection21, v_detection22;
        
        IF done THEN
            LEAVE read_loop;
        END IF;
        
        -- 构建JSON字符串（只包含有值的列）
        SET v_json = '{';
        
        IF v_detection1 IS NOT NULL THEN SET v_json = CONCAT(v_json, '"1":', v_detection1, ','); END IF;
        IF v_detection2 IS NOT NULL THEN SET v_json = CONCAT(v_json, '"2":', v_detection2, ','); END IF;
        IF v_detection3 IS NOT NULL THEN SET v_json = CONCAT(v_json, '"3":', v_detection3, ','); END IF;
        IF v_detection4 IS NOT NULL THEN SET v_json = CONCAT(v_json, '"4":', v_detection4, ','); END IF;
        IF v_detection5 IS NOT NULL THEN SET v_json = CONCAT(v_json, '"5":', v_detection5, ','); END IF;
        IF v_detection6 IS NOT NULL THEN SET v_json = CONCAT(v_json, '"6":', v_detection6, ','); END IF;
        IF v_detection7 IS NOT NULL THEN SET v_json = CONCAT(v_json, '"7":', v_detection7, ','); END IF;
        IF v_detection8 IS NOT NULL THEN SET v_json = CONCAT(v_json, '"8":', v_detection8, ','); END IF;
        IF v_detection9 IS NOT NULL THEN SET v_json = CONCAT(v_json, '"9":', v_detection9, ','); END IF;
        IF v_detection10 IS NOT NULL THEN SET v_json = CONCAT(v_json, '"10":', v_detection10, ','); END IF;
        IF v_detection11 IS NOT NULL THEN SET v_json = CONCAT(v_json, '"11":', v_detection11, ','); END IF;
        IF v_detection12 IS NOT NULL THEN SET v_json = CONCAT(v_json, '"12":', v_detection12, ','); END IF;
        IF v_detection13 IS NOT NULL THEN SET v_json = CONCAT(v_json, '"13":', v_detection13, ','); END IF;
        IF v_detection14 IS NOT NULL THEN SET v_json = CONCAT(v_json, '"14":', v_detection14, ','); END IF;
        IF v_detection15 IS NOT NULL THEN SET v_json = CONCAT(v_json, '"15":', v_detection15, ','); END IF;
        IF v_detection16 IS NOT NULL THEN SET v_json = CONCAT(v_json, '"16":', v_detection16, ','); END IF;
        IF v_detection17 IS NOT NULL THEN SET v_json = CONCAT(v_json, '"17":', v_detection17, ','); END IF;
        IF v_detection18 IS NOT NULL THEN SET v_json = CONCAT(v_json, '"18":', v_detection18, ','); END IF;
        IF v_detection19 IS NOT NULL THEN SET v_json = CONCAT(v_json, '"19":', v_detection19, ','); END IF;
        IF v_detection20 IS NOT NULL THEN SET v_json = CONCAT(v_json, '"20":', v_detection20, ','); END IF;
        IF v_detection21 IS NOT NULL THEN SET v_json = CONCAT(v_json, '"21":', v_detection21, ','); END IF;
        IF v_detection22 IS NOT NULL THEN SET v_json = CONCAT(v_json, '"22":', v_detection22, ','); END IF;
        
        -- 移除最后一个逗号
        IF RIGHT(v_json, 1) = ',' THEN
            SET v_json = LEFT(v_json, LENGTH(v_json) - 1);
        END IF;
        
        SET v_json = CONCAT(v_json, '}');
        
        -- 如果JSON不为空（至少有一个值），则更新
        IF v_json != '{}' THEN
            UPDATE LAB_RAW_DATA
            SET F_DETECTION_DATA = v_json
            WHERE F_ID = v_id;
        END IF;
    END LOOP;
    
    CLOSE cur;
END$$

DELIMITER ;

-- 执行存储过程
CALL MigrateDetectionDataToJson();

-- 删除存储过程
DROP PROCEDURE IF EXISTS MigrateDetectionDataToJson;

-- ============================================
-- 2. 设置有效数据标识（IsValidData）
-- ============================================
-- 符合炉号解析规则的数据设置为有效数据
-- 判断标准：F_IMPORT_STATUS = 0 且 F_FURNACE_NO_PARSED 不为空

UPDATE LAB_RAW_DATA
SET F_IS_VALID_DATA = 1
WHERE F_IMPORT_STATUS = 0 
  AND F_FURNACE_NO_PARSED IS NOT NULL 
  AND F_FURNACE_NO_PARSED != '';

-- ============================================
-- 3. 修复中间数据表的炉号（去掉特性汉字）
-- ============================================
-- 注意：这个操作需要从原始数据表获取去掉特性汉字后的炉号
-- 由于MySQL不支持直接调用C#的FurnaceNoHelper，建议使用应用程序进行修复
-- 或者使用以下SQL（需要根据实际特性汉字模式调整）：

-- 方式1：如果特性汉字是固定的几个字符，可以使用REPLACE
-- UPDATE LAB_INTERMEDIATE_DATA id
-- INNER JOIN LAB_RAW_DATA rd ON id.F_RAW_DATA_ID = rd.F_ID
-- SET id.F_FURNACE_NO = REPLACE(REPLACE(REPLACE(rd.F_FURNACE_NO, '脆', ''), '硬', ''), '软', '')
-- WHERE rd.F_FURNACE_NO IS NOT NULL;

-- 方式2：使用正则表达式（MySQL 8.0+）
-- UPDATE LAB_INTERMEDIATE_DATA id
-- INNER JOIN LAB_RAW_DATA rd ON id.F_RAW_DATA_ID = rd.F_ID
-- SET id.F_FURNACE_NO = REGEXP_REPLACE(rd.F_FURNACE_NO, '[脆硬软]', '')
-- WHERE rd.F_FURNACE_NO IS NOT NULL;

-- 方式3：推荐使用应用程序进行修复（调用FurnaceNoHelper.RemoveFeatureSuffix）
-- 这样可以确保逻辑的一致性

-- ============================================
-- 4. 验证迁移结果
-- ============================================

-- 检查JSON字段迁移情况
SELECT 
    COUNT(*) AS total_rows,
    COUNT(F_DETECTION_DATA) AS rows_with_json,
    COUNT(*) - COUNT(F_DETECTION_DATA) AS rows_without_json
FROM LAB_RAW_DATA;

-- 检查有效数据标识设置情况
SELECT 
    F_IS_VALID_DATA,
    COUNT(*) AS count
FROM LAB_RAW_DATA
GROUP BY F_IS_VALID_DATA;

-- 检查JSON字段格式（随机抽样）
SELECT 
    F_ID,
    F_DETECTION_DATA,
    JSON_TYPE(F_DETECTION_DATA) AS json_type,
    JSON_LENGTH(F_DETECTION_DATA) AS json_length
FROM LAB_RAW_DATA
WHERE F_DETECTION_DATA IS NOT NULL
LIMIT 10;

-- ============================================
-- 5. 回滚脚本（如果需要回滚）
-- ============================================
-- 注意：回滚前请备份数据！

-- 清空JSON字段（如果需要）
-- UPDATE LAB_RAW_DATA SET F_DETECTION_DATA = NULL;

-- 重置有效数据标识
-- UPDATE LAB_RAW_DATA SET F_IS_VALID_DATA = 0;

-- ============================================
-- 注意事项
-- ============================================
-- 1. 执行迁移前，请务必备份数据库
-- 2. 建议在测试环境先执行，验证无误后再在生产环境执行
-- 3. 大数据量时，建议分批执行，避免长时间锁表
-- 4. 中间数据表的炉号修复建议使用应用程序进行，确保逻辑一致性
-- 5. 迁移完成后，验证数据的完整性和正确性
