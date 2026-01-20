-- ============================================
-- 外观特性大类表创建脚本
-- 表名: LAB_APPEARANCE_FEATURE_CATEGORY
-- 说明: 用于管理外观特性的大类分类，如：韧性、脆边、麻点、划痕等
-- 创建时间: 2026-01-11
-- ============================================

-- 创建外观特性大类表
-- 注意：MySQL使用VARCHAR，SQL Server使用NVARCHAR
-- 如果使用SQL Server，请将VARCHAR改为NVARCHAR
CREATE TABLE LAB_APPEARANCE_FEATURE_CATEGORY (
    -- 主键ID
    F_Id VARCHAR(50) PRIMARY KEY,
    
    -- 大类名称（如：韧性、脆边、麻点、划痕、网眼、毛边、亮线、劈裂、棱、龟裂纹）
    -- 必填字段，唯一性由应用层保证
    F_NAME VARCHAR(100) NOT NULL,
    
    -- 大类描述（可选）
    -- 用于说明该大类的特征和用途
    F_DESCRIPTION VARCHAR(500),
    
    -- 排序码
    -- 用于控制大类在列表中的显示顺序，数值越小越靠前
    F_SORTCODE BIGINT,
    
    -- ========== 基础字段（继承自CLDEntityBase） ==========
    
    -- 创建人ID
    F_CREATORUSERID VARCHAR(50),
    
    -- 创建时间
    F_CREATORTIME DATETIME,
    
    -- 最后修改人ID
    F_LASTMODIFYUSERID VARCHAR(50),
    
    -- 最后修改时间
    F_LASTMODIFYTIME DATETIME,
    
    -- 删除标记（0=未删除，1=已删除）
    -- 使用软删除机制，删除时只标记不物理删除
    F_DELETEMARK INT,
    
    -- 删除时间
    F_DELETETIME DATETIME,
    
    -- 删除人ID
    F_DELETEUSERID VARCHAR(50),
    
    -- 租户ID（多租户支持）
    F_TenantId VARCHAR(50)
);

-- ============================================
-- 创建索引
-- ============================================

-- 创建大类名称索引（用于快速查询和唯一性检查）
-- 注意：MySQL不支持WHERE子句，创建普通索引即可
CREATE INDEX IX_LAB_APPEARANCE_FEATURE_CATEGORY_NAME 
ON LAB_APPEARANCE_FEATURE_CATEGORY(F_NAME);

-- 创建排序码索引（用于排序查询）
CREATE INDEX IX_LAB_APPEARANCE_FEATURE_CATEGORY_SORTCODE 
ON LAB_APPEARANCE_FEATURE_CATEGORY(F_SORTCODE);

-- 创建租户ID索引（用于多租户数据隔离）
CREATE INDEX IX_LAB_APPEARANCE_FEATURE_CATEGORY_TENANTID 
ON LAB_APPEARANCE_FEATURE_CATEGORY(F_TenantId);

-- 创建删除标记索引（用于过滤已删除数据）
CREATE INDEX IX_LAB_APPEARANCE_FEATURE_CATEGORY_DELETEMARK 
ON LAB_APPEARANCE_FEATURE_CATEGORY(F_DELETEMARK);

-- 创建复合索引（名称+删除标记，用于查询未删除的大类）
-- 这样可以提高 WHERE F_NAME = ? AND F_DELETEMARK IS NULL 的查询性能
CREATE INDEX IX_LAB_APPEARANCE_FEATURE_CATEGORY_NAME_DELETEMARK 
ON LAB_APPEARANCE_FEATURE_CATEGORY(F_NAME, F_DELETEMARK);

-- ============================================
-- 表注释
-- ============================================

-- 添加表注释（如果数据库支持）
-- EXEC sp_addextendedproperty 
--     @name = N'MS_Description', 
--     @value = N'外观特性大类表，用于管理外观特性的大类分类', 
--     @level0type = N'SCHEMA', @level0name = N'dbo', 
--     @level1type = N'TABLE', @level1name = N'LAB_APPEARANCE_FEATURE_CATEGORY';

-- ============================================
-- 字段注释（SQL Server语法）
-- ============================================

-- EXEC sp_addextendedproperty 
--     @name = N'MS_Description', 
--     @value = N'主键ID', 
--     @level0type = N'SCHEMA', @level0name = N'dbo', 
--     @level1type = N'TABLE', @level1name = N'LAB_APPEARANCE_FEATURE_CATEGORY', 
--     @level2type = N'COLUMN', @level2name = N'F_Id';

-- EXEC sp_addextendedproperty 
--     @name = N'MS_Description', 
--     @value = N'大类名称（如：韧性、脆边、麻点等）', 
--     @level0type = N'SCHEMA', @level0name = N'dbo', 
--     @level1type = N'TABLE', @level1name = N'LAB_APPEARANCE_FEATURE_CATEGORY', 
--     @level2type = N'COLUMN', @level2name = N'F_NAME';

-- EXEC sp_addextendedproperty 
--     @name = N'MS_Description', 
--     @value = N'大类描述', 
--     @level0type = N'SCHEMA', @level0name = N'dbo', 
--     @level1type = N'TABLE', @level1name = N'LAB_APPEARANCE_FEATURE_CATEGORY', 
--     @level2type = N'COLUMN', @level2name = N'F_DESCRIPTION';

-- EXEC sp_addextendedproperty 
--     @name = N'MS_Description', 
--     @value = N'排序码，用于控制显示顺序', 
--     @level0type = N'SCHEMA', @level0name = N'dbo', 
--     @level1type = N'TABLE', @level1name = N'LAB_APPEARANCE_FEATURE_CATEGORY', 
--     @level2type = N'COLUMN', @level2name = N'F_SORTCODE';

-- ============================================
-- 默认数据（可选）
-- ============================================

-- 如果需要，可以在这里插入默认的10个大类
-- 注意：需要先确保有有效的租户ID和用户ID

/*
INSERT INTO LAB_APPEARANCE_FEATURE_CATEGORY 
    (F_Id, F_NAME, F_DESCRIPTION, F_SORTCODE, F_CREATORUSERID, F_CREATORTIME, F_LASTMODIFYUSERID, F_LASTMODIFYTIME, F_DELETEMARK, F_TenantId)
VALUES
    (NEWID(), '韧性', '外观特性大类：韧性', 1, 'SYSTEM', GETDATE(), 'SYSTEM', GETDATE(), NULL, 'DEFAULT'),
    (NEWID(), '脆边', '外观特性大类：脆边', 2, 'SYSTEM', GETDATE(), 'SYSTEM', GETDATE(), NULL, 'DEFAULT'),
    (NEWID(), '麻点', '外观特性大类：麻点', 3, 'SYSTEM', GETDATE(), 'SYSTEM', GETDATE(), NULL, 'DEFAULT'),
    (NEWID(), '划痕', '外观特性大类：划痕', 4, 'SYSTEM', GETDATE(), 'SYSTEM', GETDATE(), NULL, 'DEFAULT'),
    (NEWID(), '网眼', '外观特性大类：网眼', 5, 'SYSTEM', GETDATE(), 'SYSTEM', GETDATE(), NULL, 'DEFAULT'),
    (NEWID(), '毛边', '外观特性大类：毛边', 6, 'SYSTEM', GETDATE(), 'SYSTEM', GETDATE(), NULL, 'DEFAULT'),
    (NEWID(), '亮线', '外观特性大类：亮线', 7, 'SYSTEM', GETDATE(), 'SYSTEM', GETDATE(), NULL, 'DEFAULT'),
    (NEWID(), '劈裂', '外观特性大类：劈裂', 8, 'SYSTEM', GETDATE(), 'SYSTEM', GETDATE(), NULL, 'DEFAULT'),
    (NEWID(), '棱', '外观特性大类：棱', 9, 'SYSTEM', GETDATE(), 'SYSTEM', GETDATE(), NULL, 'DEFAULT'),
    (NEWID(), '龟裂纹', '外观特性大类：龟裂纹', 10, 'SYSTEM', GETDATE(), 'SYSTEM', GETDATE(), NULL, 'DEFAULT');
*/

-- ============================================
-- 使用说明
-- ============================================
-- 1. 执行此脚本前，请确保数据库连接正确
-- 2. 如果表已存在，请先删除或使用 IF NOT EXISTS 语法
-- 3. 索引创建是可选的，但建议创建以提高查询性能
-- 4. 默认数据插入需要根据实际情况调整租户ID和用户ID
-- 5. 建议在生产环境执行前先在测试环境验证
-- 6. 数据类型说明：
--    - MySQL: 使用 VARCHAR
--    - SQL Server: 使用 NVARCHAR（需要将脚本中的VARCHAR改为NVARCHAR）
-- 7. 索引说明：
--    - MySQL不支持CREATE INDEX中的WHERE子句，已移除
--    - 已创建复合索引以提高查询性能
