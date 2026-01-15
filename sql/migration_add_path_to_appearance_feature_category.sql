-- ============================================
-- 外观特性大类表 - 添加分类路径字段
-- 说明: 添加 F_PATH 字段，存储从根分类到当前分类的完整路径（用逗号分隔ID）
-- 格式示例: "rootId,parentId,currentId"
-- 创建时间: 2026-01-XX
-- ============================================

-- ============================================
-- MySQL版本
-- ============================================

-- 添加分类路径字段
ALTER TABLE `LAB_APPEARANCE_FEATURE_CATEGORY` 
ADD COLUMN `F_PATH` VARCHAR(500) NULL COMMENT '分类路径（从根分类到当前分类的完整路径，用逗号分隔ID）' 
AFTER `F_ROOTID`;

-- 创建索引以提高查询性能（用于 LIKE 查询）
CREATE INDEX `IDX_PATH` ON `LAB_APPEARANCE_FEATURE_CATEGORY`(`F_PATH`(255));

-- ============================================
-- 填充现有数据的 Path
-- ============================================

-- 方法1: 使用存储过程递归更新（推荐，适用于大数据量）
DELIMITER $$

DROP PROCEDURE IF EXISTS `UpdateCategoryPath`$$

CREATE PROCEDURE `UpdateCategoryPath`()
BEGIN
    DECLARE done INT DEFAULT FALSE;
    DECLARE v_id VARCHAR(50);
    DECLARE v_parent_id VARCHAR(50);
    DECLARE v_path VARCHAR(500);
    DECLARE v_parent_path VARCHAR(500);
    
    -- 游标：获取所有需要更新的分类
    DECLARE cur CURSOR FOR 
        SELECT F_Id, F_PARENTID 
        FROM LAB_APPEARANCE_FEATURE_CATEGORY 
        WHERE F_DeleteMark IS NULL
        ORDER BY 
            CASE WHEN F_PARENTID IS NULL OR F_PARENTID = '' THEN 0 ELSE 1 END,
            F_SORTCODE;
    
    DECLARE CONTINUE HANDLER FOR NOT FOUND SET done = TRUE;
    
    OPEN cur;
    
    read_loop: LOOP
        FETCH cur INTO v_id, v_parent_id;
        IF done THEN
            LEAVE read_loop;
        END IF;
        
        -- 如果父级为空，则当前分类就是根分类，路径就是自己的ID
        IF v_parent_id IS NULL OR v_parent_id = '' THEN
            SET v_path = v_id;
        ELSE
            -- 获取父级的路径
            SELECT F_PATH INTO v_parent_path
            FROM LAB_APPEARANCE_FEATURE_CATEGORY
            WHERE F_Id = v_parent_id AND F_DeleteMark IS NULL;
            
            -- 如果父级路径为空，构建路径：父级ID + 当前ID
            IF v_parent_path IS NULL OR v_parent_path = '' THEN
                SET v_path = CONCAT(v_parent_id, ',', v_id);
            ELSE
                -- 构建路径：父级路径 + 当前ID
                SET v_path = CONCAT(v_parent_path, ',', v_id);
            END IF;
        END IF;
        
        -- 更新 Path
        UPDATE LAB_APPEARANCE_FEATURE_CATEGORY
        SET F_PATH = v_path
        WHERE F_Id = v_id;
        
    END LOOP;
    
    CLOSE cur;
END$$

DELIMITER ;

-- 执行存储过程
CALL `UpdateCategoryPath`();

-- 删除存储过程（可选）
DROP PROCEDURE IF EXISTS `UpdateCategoryPath`;

-- ============================================
-- 方法2: 使用递归CTE更新（适用于SQL Server，MySQL 8.0+）
-- ============================================

/*
-- MySQL 8.0+ 版本
WITH RECURSIVE CategoryTree AS (
    -- 根节点（没有父级的分类）
    SELECT 
        F_Id,
        F_Id AS Path,
        F_PARENTID,
        0 AS Level
    FROM LAB_APPEARANCE_FEATURE_CATEGORY
    WHERE (F_PARENTID IS NULL OR F_PARENTID = '') AND F_DeleteMark IS NULL
    
    UNION ALL
    
    -- 递归查找子节点
    SELECT 
        c.F_Id,
        CONCAT(ct.Path, ',', c.F_Id) AS Path,
        c.F_PARENTID,
        ct.Level + 1
    FROM LAB_APPEARANCE_FEATURE_CATEGORY c
    INNER JOIN CategoryTree ct ON c.F_PARENTID = ct.F_Id
    WHERE c.F_DeleteMark IS NULL
)
UPDATE LAB_APPEARANCE_FEATURE_CATEGORY c
INNER JOIN CategoryTree ct ON c.F_Id = ct.F_Id
SET c.F_PATH = ct.Path;
*/

-- ============================================
-- 方法3: 简单更新（适用于小数据量，分步执行）
-- ============================================

/*
-- 第一步：更新根分类（没有父级的分类）
UPDATE LAB_APPEARANCE_FEATURE_CATEGORY
SET F_PATH = F_Id
WHERE (F_PARENTID IS NULL OR F_PARENTID = '') 
  AND F_DeleteMark IS NULL;

-- 第二步：更新一级子分类
UPDATE LAB_APPEARANCE_FEATURE_CATEGORY c
INNER JOIN LAB_APPEARANCE_FEATURE_CATEGORY p ON c.F_PARENTID = p.F_Id
SET c.F_PATH = CONCAT(p.F_PATH, ',', c.F_Id)
WHERE c.F_DeleteMark IS NULL 
  AND p.F_DeleteMark IS NULL
  AND (c.F_PATH IS NULL OR c.F_PATH = '');

-- 第三步：更新二级子分类（重复执行直到没有更多更新）
UPDATE LAB_APPEARANCE_FEATURE_CATEGORY c
INNER JOIN LAB_APPEARANCE_FEATURE_CATEGORY p ON c.F_PARENTID = p.F_Id
SET c.F_PATH = CONCAT(p.F_PATH, ',', c.F_Id)
WHERE c.F_DeleteMark IS NULL 
  AND p.F_DeleteMark IS NULL
  AND (c.F_PATH IS NULL OR c.F_PATH = '');

-- 继续重复第三步，直到受影响的行数为0
*/

-- ============================================
-- SQL Server版本
-- ============================================

/*
-- 添加分类路径字段
ALTER TABLE LAB_APPEARANCE_FEATURE_CATEGORY 
ADD F_PATH NVARCHAR(500) NULL;

-- 添加注释
EXEC sp_addextendedproperty 
    @name = N'MS_Description', 
    @value = N'分类路径（从根分类到当前分类的完整路径，用逗号分隔ID）', 
    @level0type = N'SCHEMA', @level0name = N'dbo', 
    @level1type = N'TABLE', @level1name = N'LAB_APPEARANCE_FEATURE_CATEGORY', 
    @level2type = N'COLUMN', @level2name = N'F_PATH';

-- 创建索引
CREATE INDEX IDX_PATH ON LAB_APPEARANCE_FEATURE_CATEGORY(F_PATH);

-- 使用递归CTE更新现有数据
WITH CategoryTree AS (
    -- 根节点（没有父级的分类）
    SELECT 
        F_Id,
        CAST(F_Id AS NVARCHAR(500)) AS Path,
        F_PARENTID,
        0 AS Level
    FROM LAB_APPEARANCE_FEATURE_CATEGORY
    WHERE (F_PARENTID IS NULL OR F_PARENTID = '') AND F_DeleteMark IS NULL
    
    UNION ALL
    
    -- 递归查找子节点
    SELECT 
        c.F_Id,
        CAST(ct.Path + ',' + c.F_Id AS NVARCHAR(500)) AS Path,
        c.F_PARENTID,
        ct.Level + 1
    FROM LAB_APPEARANCE_FEATURE_CATEGORY c
    INNER JOIN CategoryTree ct ON c.F_PARENTID = ct.F_Id
    WHERE c.F_DeleteMark IS NULL
)
UPDATE c
SET c.F_PATH = ct.Path
FROM LAB_APPEARANCE_FEATURE_CATEGORY c
INNER JOIN CategoryTree ct ON c.F_Id = ct.F_Id;
*/

-- ============================================
-- 验证数据
-- ============================================

-- 检查是否有未设置 Path 的记录（应该为0）
SELECT COUNT(*) AS UnsetPathCount
FROM LAB_APPEARANCE_FEATURE_CATEGORY
WHERE F_DeleteMark IS NULL 
  AND (F_PATH IS NULL OR F_PATH = '');

-- 检查根分类的 Path 是否等于自身ID（应该全部为true）
SELECT 
    F_Id,
    F_NAME,
    F_PARENTID,
    F_ROOTID,
    F_PATH,
    CASE WHEN F_PARENTID IS NULL OR F_PARENTID = '' THEN 
        CASE WHEN F_PATH = F_Id THEN '正确' ELSE '错误' END
    ELSE 
        CASE WHEN F_PATH LIKE CONCAT('%,', F_Id) OR F_PATH = F_Id THEN '正确' ELSE '错误' END
    END AS Status
FROM LAB_APPEARANCE_FEATURE_CATEGORY
WHERE F_DeleteMark IS NULL
ORDER BY F_ROOTID, F_PATH, F_SORTCODE;

-- 检查路径格式是否正确（应该以根分类ID开头）
SELECT 
    F_Id,
    F_NAME,
    F_PARENTID,
    F_ROOTID,
    F_PATH,
    CASE 
        WHEN F_PATH IS NULL OR F_PATH = '' THEN '路径为空'
        WHEN F_PARENTID IS NULL OR F_PARENTID = '' THEN 
            CASE WHEN F_PATH = F_Id THEN '正确' ELSE '错误' END
        ELSE 
            CASE 
                WHEN F_PATH LIKE CONCAT(F_ROOTID, ',%') OR F_PATH = F_ROOTID THEN '正确'
                ELSE '错误'
            END
    END AS Status
FROM LAB_APPEARANCE_FEATURE_CATEGORY
WHERE F_DeleteMark IS NULL
ORDER BY F_ROOTID, F_PATH, F_SORTCODE;

-- ============================================
-- 使用说明
-- ============================================
-- 1. 根据数据库类型选择对应的SQL脚本（MySQL或SQL Server）
-- 2. 对于MySQL，推荐使用方法1（存储过程）或方法2（递归CTE，需要MySQL 8.0+）
-- 3. 对于SQL Server，使用递归CTE方法
-- 4. 执行前请备份数据库
-- 5. 执行后请验证数据正确性
-- 6. 如果数据量很大，建议在低峰期执行
-- 7. Path 字段格式说明：
--    - 根分类：Path = "根分类ID"
--    - 一级子分类：Path = "根分类ID,一级子分类ID"
--    - 二级子分类：Path = "根分类ID,一级子分类ID,二级子分类ID"
--    - 以此类推...
