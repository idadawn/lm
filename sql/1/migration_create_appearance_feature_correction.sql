-- ============================================
-- 外观特性人工修正记录表创建脚本
-- 表名: LAB_APPEARANCE_FEATURE_CORRECTION
-- 说明: 用于记录人工修正外观特性匹配的记录，用于后续学习和优化匹配算法
-- 创建时间: 2026-01-11
-- ============================================

-- 创建外观特性人工修正记录表
-- 注意：MySQL使用VARCHAR，SQL Server使用NVARCHAR
-- 如果使用SQL Server，请将VARCHAR改为NVARCHAR
CREATE TABLE LAB_APPEARANCE_FEATURE_CORRECTION (
    -- 主键ID
    F_Id VARCHAR(50) PRIMARY KEY,
    
    -- 原始输入文本
    -- 用户输入的原始特性描述，如："脆"、"微脆"、"脆有划痕"
    F_INPUT_TEXT VARCHAR(500) NOT NULL,
    
    -- 自动匹配的特征ID（如果有）
    -- 系统自动匹配到的特征ID，如果匹配失败则为NULL
    F_AUTO_MATCHED_FEATURE_ID VARCHAR(50),
    
    -- 人工修正后的特征ID
    -- 用户最终选择的特征ID，必填
    F_CORRECTED_FEATURE_ID VARCHAR(50) NOT NULL,
    
    -- 匹配模式
    -- auto: 使用自动匹配结果
    -- manual: 手动选择
    -- create: 创建新特性
    F_MATCH_MODE VARCHAR(20) NOT NULL,
    
    -- 使用场景
    -- test: 测试页面
    -- import: 数据导入
    F_SCENARIO VARCHAR(20) NOT NULL,
    
    -- 备注
    -- 可选的备注信息，如：炉号、测试说明等
    F_REMARK VARCHAR(500),
    
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

-- 创建输入文本索引（用于分析常见输入模式）
-- 注意：MySQL不支持WHERE子句，创建普通索引即可
CREATE INDEX IX_LAB_APPEARANCE_FEATURE_CORRECTION_INPUT_TEXT 
ON LAB_APPEARANCE_FEATURE_CORRECTION(F_INPUT_TEXT);

-- 创建修正特征ID索引（用于统计特征被修正的频率）
CREATE INDEX IX_LAB_APPEARANCE_FEATURE_CORRECTION_CORRECTED_FEATURE_ID 
ON LAB_APPEARANCE_FEATURE_CORRECTION(F_CORRECTED_FEATURE_ID);

-- 创建匹配模式索引（用于分析不同匹配模式的效果）
CREATE INDEX IX_LAB_APPEARANCE_FEATURE_CORRECTION_MATCH_MODE 
ON LAB_APPEARANCE_FEATURE_CORRECTION(F_MATCH_MODE);

-- 创建使用场景索引（用于区分不同场景的修正记录）
CREATE INDEX IX_LAB_APPEARANCE_FEATURE_CORRECTION_SCENARIO 
ON LAB_APPEARANCE_FEATURE_CORRECTION(F_SCENARIO);

-- 创建创建时间索引（用于按时间分析修正趋势）
CREATE INDEX IX_LAB_APPEARANCE_FEATURE_CORRECTION_CREATORTIME 
ON LAB_APPEARANCE_FEATURE_CORRECTION(F_CREATORTIME);

-- 创建租户ID索引（用于多租户数据隔离）
CREATE INDEX IX_LAB_APPEARANCE_FEATURE_CORRECTION_TENANTID 
ON LAB_APPEARANCE_FEATURE_CORRECTION(F_TenantId);

-- 创建删除标记索引（用于过滤已删除数据）
CREATE INDEX IX_LAB_APPEARANCE_FEATURE_CORRECTION_DELETEMARK 
ON LAB_APPEARANCE_FEATURE_CORRECTION(F_DELETEMARK);

-- 创建复合索引（修正特征ID+删除标记，用于查询未删除的修正记录）
CREATE INDEX IX_LAB_APPEARANCE_FEATURE_CORRECTION_CORRECTED_DELETEMARK 
ON LAB_APPEARANCE_FEATURE_CORRECTION(F_CORRECTED_FEATURE_ID, F_DELETEMARK);

-- ============================================
-- 表注释
-- ============================================

-- 添加表注释（如果数据库支持）
-- EXEC sp_addextendedproperty 
--     @name = N'MS_Description', 
--     @value = N'外观特性人工修正记录表，用于记录人工修正匹配的记录，用于后续学习和优化', 
--     @level0type = N'SCHEMA', @level0name = N'dbo', 
--     @level1type = N'TABLE', @level1name = N'LAB_APPEARANCE_FEATURE_CORRECTION';

-- ============================================
-- 字段注释（SQL Server语法）
-- ============================================

-- EXEC sp_addextendedproperty 
--     @name = N'MS_Description', 
--     @value = N'原始输入文本', 
--     @level0type = N'SCHEMA', @level0name = N'dbo', 
--     @level1type = N'TABLE', @level1name = N'LAB_APPEARANCE_FEATURE_CORRECTION', 
--     @level2type = N'COLUMN', @level2name = N'F_INPUT_TEXT';

-- EXEC sp_addextendedproperty 
--     @name = N'MS_Description', 
--     @value = N'自动匹配的特征ID（如果有）', 
--     @level0type = N'SCHEMA', @level0name = N'dbo', 
--     @level1type = N'TABLE', @level1name = N'LAB_APPEARANCE_FEATURE_CORRECTION', 
--     @level2type = N'COLUMN', @level2name = N'F_AUTO_MATCHED_FEATURE_ID';

-- EXEC sp_addextendedproperty 
--     @name = N'MS_Description', 
--     @value = N'人工修正后的特征ID', 
--     @level0type = N'SCHEMA', @level0name = N'dbo', 
--     @level1type = N'TABLE', @level1name = N'LAB_APPEARANCE_FEATURE_CORRECTION', 
--     @level2type = N'COLUMN', @level2name = N'F_CORRECTED_FEATURE_ID';

-- EXEC sp_addextendedproperty 
--     @name = N'MS_Description', 
--     @value = N'匹配模式（auto/manual/create）', 
--     @level0type = N'SCHEMA', @level0name = N'dbo', 
--     @level1type = N'TABLE', @level1name = N'LAB_APPEARANCE_FEATURE_CORRECTION', 
--     @level2type = N'COLUMN', @level2name = N'F_MATCH_MODE';

-- EXEC sp_addextendedproperty 
--     @name = N'MS_Description', 
--     @value = N'使用场景（test/import）', 
--     @level0type = N'SCHEMA', @level0name = N'dbo', 
--     @level1type = N'TABLE', @level1name = N'LAB_APPEARANCE_FEATURE_CORRECTION', 
--     @level2type = N'COLUMN', @level2name = N'F_SCENARIO';

-- ============================================
-- 使用说明
-- ============================================
-- 1. 此表用于记录人工修正匹配的记录，用于后续数据分析和算法优化
-- 2. 可以通过分析修正记录来：
--    - 发现常见匹配失败的模式
--    - 优化关键词匹配规则
--    - 训练AI模型
--    - 统计人工修正频率
-- 3. 建议定期清理过期的修正记录（如：保留最近6个月的记录）
-- 4. 执行此脚本前，请确保数据库连接正确
-- 5. 如果表已存在，请先删除或使用 IF NOT EXISTS 语法
-- 6. 数据类型说明：
--    - MySQL: 使用 VARCHAR
--    - SQL Server: 使用 NVARCHAR（需要将脚本中的VARCHAR改为NVARCHAR）
-- 7. 索引说明：
--    - MySQL不支持CREATE INDEX中的WHERE子句，已移除
--    - 已创建复合索引以提高查询性能
