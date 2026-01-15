-- ============================================
-- 外观特性大类表 - 添加根分类ID字段
-- 说明: 添加 F_ROOTID 字段，用于快速追溯到最顶层分类
-- 创建时间: 2026-01-XX
-- ============================================

-- ============================================
-- MySQL版本
-- ============================================

-- 添加根分类ID字段
ALTER TABLE `LAB_APPEARANCE_FEATURE_CATEGORY` 
ADD COLUMN `F_ROOTID` VARCHAR(50) NULL COMMENT '根分类ID（最顶层分类的ID）' 
AFTER `F_PARENTID`;

-- 创建索引以提高查询性能
CREATE INDEX `IDX_ROOTID` ON `LAB_APPEARANCE_FEATURE_CATEGORY`(`F_ROOTID`);

-- ============================================
-- 填充现有数据的 RootId
-- ============================================

-- 方法1: 使用存储过程递归更新（推荐，适用于大数据量）
-- 注意：如果数据库不支持存储过程，请使用方法2

DELIMITER $$

DROP PROCEDURE IF EXISTS `UpdateCategoryRootId`$$

CREATE PROCEDURE `UpdateCategoryRootId`()
BEGIN
    DECLARE done INT DEFAULT FALSE;
    DECLARE v_id VARCHAR(50);
    DECLARE v_parent_id VARCHAR(50);
    DECLARE v_root_id VARCHAR(50);
    
    -- 游标：获取所有需要更新的分类
    DECLARE cur CURSOR FOR 
        SELECT F_Id, F_PARENTID 
        FROM LAB_APPEARANCE_FEATURE_CATEGORY 
        WHERE F_DeleteMark IS NULL;
    
    DECLARE CONTINUE HANDLER FOR NOT FOUND SET done = TRUE;
    
    OPEN cur;
    
    read_loop: LOOP
        FETCH cur INTO v_id, v_parent_id;
        IF done THEN
            LEAVE read_loop;
        END IF;
        
        -- 如果父级为空，则当前分类就是根分类
        IF v_parent_id IS NULL OR v_parent_id = '' THEN
            SET v_root_id = v_id;
        ELSE
            -- 向上查找根分类
            SET v_root_id = v_id;
            WHILE v_parent_id IS NOT NULL AND v_parent_id != '' DO
                SELECT F_PARENTID INTO v_parent_id
                FROM LAB_APPEARANCE_FEATURE_CATEGORY
                WHERE F_Id = v_parent_id AND F_DeleteMark IS NULL;
                
                IF v_parent_id IS NOT NULL AND v_parent_id != '' THEN
                    SET v_root_id = v_parent_id;
                END IF;
            END WHILE;
        END IF;
        
        -- 更新 RootId
        UPDATE LAB_APPEARANCE_FEATURE_CATEGORY
        SET F_ROOTID = v_root_id
        WHERE F_Id = v_id;
        
    END LOOP;
    
    CLOSE cur;
END$$

DELIMITER ;

-- 执行存储过程
CALL `UpdateCategoryRootId`();

-- 删除存储过程（可选）
DROP PROCEDURE IF EXISTS `UpdateCategoryRootId`;

-- ============================================
-- 方法2: 使用递归CTE更新（适用于SQL Server，MySQL 8.0+）
-- ============================================

/*
-- MySQL 8.0+ 版本
WITH RECURSIVE CategoryTree AS (
    -- 根节点（没有父级的分类）
    SELECT 
        F_Id,
        F_Id AS RootId,
        F_PARENTID,
        0 AS Level
    FROM LAB_APPEARANCE_FEATURE_CATEGORY
    WHERE (F_PARENTID IS NULL OR F_PARENTID = '') AND F_DeleteMark IS NULL
    
    UNION ALL
    
    -- 递归查找子节点
    SELECT 
        c.F_Id,
        ct.RootId,
        c.F_PARENTID,
        ct.Level + 1
    FROM LAB_APPEARANCE_FEATURE_CATEGORY c
    INNER JOIN CategoryTree ct ON c.F_PARENTID = ct.F_Id
    WHERE c.F_DeleteMark IS NULL
)
UPDATE LAB_APPEARANCE_FEATURE_CATEGORY c
INNER JOIN CategoryTree ct ON c.F_Id = ct.F_Id
SET c.F_ROOTID = ct.RootId;
*/

-- ============================================
-- 方法3: 简单更新（适用于小数据量，分步执行）
-- ============================================

/*
-- 第一步：更新根分类（没有父级的分类）
UPDATE LAB_APPEARANCE_FEATURE_CATEGORY
SET F_ROOTID = F_Id
WHERE (F_PARENTID IS NULL OR F_PARENTID = '') 
  AND F_DeleteMark IS NULL;

-- 第二步：更新一级子分类
UPDATE LAB_APPEARANCE_FEATURE_CATEGORY c
INNER JOIN LAB_APPEARANCE_FEATURE_CATEGORY p ON c.F_PARENTID = p.F_Id
SET c.F_ROOTID = COALESCE(p.F_ROOTID, p.F_Id)
WHERE c.F_DeleteMark IS NULL 
  AND p.F_DeleteMark IS NULL
  AND c.F_ROOTID IS NULL;

-- 第三步：更新二级子分类（重复执行直到没有更多更新）
UPDATE LAB_APPEARANCE_FEATURE_CATEGORY c
INNER JOIN LAB_APPEARANCE_FEATURE_CATEGORY p ON c.F_PARENTID = p.F_Id
SET c.F_ROOTID = COALESCE(p.F_ROOTID, p.F_Id)
WHERE c.F_DeleteMark IS NULL 
  AND p.F_DeleteMark IS NULL
  AND c.F_ROOTID IS NULL;

-- 继续重复第三步，直到受影响的行数为0
*/

-- ============================================
-- SQL Server版本
-- ============================================

/*
-- 添加根分类ID字段
ALTER TABLE LAB_APPEARANCE_FEATURE_CATEGORY 
ADD F_ROOTID NVARCHAR(50) NULL;

-- 添加注释
EXEC sp_addextendedproperty 
    @name = N'MS_Description', 
    @value = N'根分类ID（最顶层分类的ID）', 
    @level0type = N'SCHEMA', @level0name = N'dbo', 
    @level1type = N'TABLE', @level1name = N'LAB_APPEARANCE_FEATURE_CATEGORY', 
    @level2type = N'COLUMN', @level2name = N'F_ROOTID';

-- 创建索引
CREATE INDEX IDX_ROOTID ON LAB_APPEARANCE_FEATURE_CATEGORY(F_ROOTID);

-- 使用递归CTE更新现有数据
WITH CategoryTree AS (
    -- 根节点（没有父级的分类）
    SELECT 
        F_Id,
        F_Id AS RootId,
        F_PARENTID,
        0 AS Level
    FROM LAB_APPEARANCE_FEATURE_CATEGORY
    WHERE (F_PARENTID IS NULL OR F_PARENTID = '') AND F_DeleteMark IS NULL
    
    UNION ALL
    
    -- 递归查找子节点
    SELECT 
        c.F_Id,
        ct.RootId,
        c.F_PARENTID,
        ct.Level + 1
    FROM LAB_APPEARANCE_FEATURE_CATEGORY c
    INNER JOIN CategoryTree ct ON c.F_PARENTID = ct.F_Id
    WHERE c.F_DeleteMark IS NULL
)
UPDATE c
SET c.F_ROOTID = ct.RootId
FROM LAB_APPEARANCE_FEATURE_CATEGORY c
INNER JOIN CategoryTree ct ON c.F_Id = ct.F_Id;
*/

-- ============================================
-- 验证数据
-- ============================================

-- 检查是否有未设置 RootId 的记录（应该为0）
SELECT COUNT(*) AS UnsetRootIdCount
FROM LAB_APPEARANCE_FEATURE_CATEGORY
WHERE F_DeleteMark IS NULL 
  AND F_ROOTID IS NULL;

-- 检查根分类的 RootId 是否等于自身（应该全部为true）
SELECT 
    F_Id,
    F_NAME,
    F_PARENTID,
    F_ROOTID,
    CASE WHEN F_PARENTID IS NULL OR F_PARENTID = '' THEN 
        CASE WHEN F_ROOTID = F_Id THEN '正确' ELSE '错误' END
    ELSE '子分类' END AS Status
FROM LAB_APPEARANCE_FEATURE_CATEGORY
WHERE F_DeleteMark IS NULL
ORDER BY F_ROOTID, F_PARENTID, F_SORTCODE;

-- ============================================
-- 使用说明
-- ============================================
-- 1. 根据数据库类型选择对应的SQL脚本（MySQL或SQL Server）
-- 2. 对于MySQL，推荐使用方法1（存储过程）或方法2（递归CTE，需要MySQL 8.0+）
-- 3. 对于SQL Server，使用递归CTE方法
-- 4. 执行前请备份数据库
-- 5. 执行后请验证数据正确性
-- 6. 如果数据量很大，建议在低峰期执行
