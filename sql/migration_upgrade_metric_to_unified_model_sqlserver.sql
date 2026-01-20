-- ============================================
-- 指标定义表升级脚本（统一模型）- SQL Server版本
-- 数据库类型：SQL Server
-- 创建日期：2026-01-20
-- 说明：将lab_metric_definition表升级到统一模型
-- 执行顺序：在现有表创建后执行
-- ============================================

-- 1. 添加新字段
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[lab_metric_definition]') AND name = N'formula_language')
BEGIN
    ALTER TABLE [dbo].[lab_metric_definition]
    ADD [formula_language] NVARCHAR(20) DEFAULT N'EXCEL' NOT NULL,
        [metric_type] NVARCHAR(20) DEFAULT N'STANDARD' NOT NULL,
        [return_type] NVARCHAR(20) DEFAULT N'NUMBER' NOT NULL,
        [unit_name] NVARCHAR(50) NULL,
        [is_system] BIT DEFAULT 0 NOT NULL,
        [version] INT DEFAULT 1 NOT NULL,
        [is_versioned] BIT DEFAULT 0 NOT NULL,
        [has_variables] BIT DEFAULT 0 NOT NULL,
        [store_results] BIT DEFAULT 0 NOT NULL,
        [precision] INT NULL,
        [calculation_order] INT DEFAULT 0 NOT NULL;
END

-- 2. 添加is_enabled字段（兼容新模型）
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[lab_metric_definition]') AND name = N'is_enabled')
BEGIN
    ALTER TABLE [dbo].[lab_metric_definition]
    ADD [is_enabled] BIT DEFAULT 1 NOT NULL;
END

-- 3. 将现有status字段的数据迁移到is_enabled字段
UPDATE [dbo].[lab_metric_definition]
SET [is_enabled] = CASE
    WHEN [status] = 1 THEN 1
    WHEN [status] = 0 THEN 0
    ELSE 1
END
WHERE [F_DeleteMark] = 0;

-- 4. 创建扩展表（如果不存在）

-- 指标变量绑定表
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = N'lab_metric_variable')
BEGIN
    CREATE TABLE [dbo].[lab_metric_variable] (
        -- 基础字段（从CLDEntityBase继承）
        [F_ID] NVARCHAR(50) NOT NULL,
        [F_TENANTID] NVARCHAR(50) NULL,
        [F_CREATORTIME] DATETIME NULL,
        [F_CREATORUSERID] NVARCHAR(50) NULL,
        [F_ENABLEDMARK] INT DEFAULT 1,
        [F_LastModifyTime] DATETIME NULL,
        [F_LastModifyUserId] NVARCHAR(50) NULL,
        [F_DeleteMark] INT DEFAULT 0,
        [F_DeleteTime] DATETIME NULL,
        [F_DeleteUserId] NVARCHAR(50) NULL,

        -- 业务字段
        [metric_id] NVARCHAR(50) NOT NULL,
        [variable_name] NVARCHAR(100) NOT NULL,
        [source_type] NVARCHAR(20) NOT NULL,
        [source_id] NVARCHAR(100) NULL,
        [data_type] NVARCHAR(20) NOT NULL,
        [is_required] BIT DEFAULT 1,
        [default_value] NVARCHAR(200) NULL,
        [sort_order] INT DEFAULT 0,

        CONSTRAINT [PK_lab_metric_variable] PRIMARY KEY ([F_ID]),
        CONSTRAINT [UK_METRIC_VARIABLE] UNIQUE ([metric_id], [variable_name], [F_TENANTID], [F_DeleteMark]),
        CONSTRAINT [FK_METRIC_VARIABLE_METRIC] FOREIGN KEY ([metric_id])
            REFERENCES [dbo].[lab_metric_definition] ([F_ID]) ON DELETE CASCADE
    );

    -- 创建索引
    CREATE INDEX [IDX_METRIC_ID] ON [dbo].[lab_metric_variable] ([metric_id], [F_TENANTID]);
    CREATE INDEX [IDX_VARIABLE_NAME] ON [dbo].[lab_metric_variable] ([variable_name], [F_TENANTID]);
    CREATE INDEX [IDX_SOURCE_TYPE] ON [dbo].[lab_metric_variable] ([source_type], [F_TENANTID]);
END

-- 指标公式版本表
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = N'lab_metric_formula_version')
BEGIN
    CREATE TABLE [dbo].[lab_metric_formula_version] (
        -- 基础字段（从CLDEntityBase继承）
        [F_ID] NVARCHAR(50) NOT NULL,
        [F_TENANTID] NVARCHAR(50) NULL,
        [F_CREATORTIME] DATETIME NULL,
        [F_CREATORUSERID] NVARCHAR(50) NULL,
        [F_ENABLEDMARK] INT DEFAULT 1,
        [F_LastModifyTime] DATETIME NULL,
        [F_LastModifyUserId] NVARCHAR(50) NULL,
        [F_DeleteMark] INT DEFAULT 0,
        [F_DeleteTime] DATETIME NULL,
        [F_DeleteUserId] NVARCHAR(50) NULL,

        -- 业务字段
        [metric_id] NVARCHAR(50) NOT NULL,
        [version] INT NOT NULL,
        [version_name] NVARCHAR(100) NULL,
        [formula] NVARCHAR(MAX) NOT NULL,
        [change_reason] NVARCHAR(500) NULL,
        [is_current] BIT DEFAULT 0,

        CONSTRAINT [PK_lab_metric_formula_version] PRIMARY KEY ([F_ID]),
        CONSTRAINT [UK_METRIC_VERSION] UNIQUE ([metric_id], [version], [F_TENANTID], [F_DeleteMark]),
        CONSTRAINT [FK_METRIC_VERSION_METRIC] FOREIGN KEY ([metric_id])
            REFERENCES [dbo].[lab_metric_definition] ([F_ID]) ON DELETE CASCADE
    );

    -- 创建索引
    CREATE INDEX [IDX_METRIC_ID] ON [dbo].[lab_metric_formula_version] ([metric_id], [F_TENANTID]);
    CREATE INDEX [IDX_VERSION] ON [dbo].[lab_metric_formula_version] ([version], [F_TENANTID]);
    CREATE INDEX [IDX_IS_CURRENT] ON [dbo].[lab_metric_formula_version] ([is_current], [F_TENANTID]);
END

-- 指标计算结果表
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = N'lab_metric_result')
BEGIN
    CREATE TABLE [dbo].[lab_metric_result] (
        -- 基础字段（从CLDEntityBase继承）
        [F_ID] NVARCHAR(50) NOT NULL,
        [F_TENANTID] NVARCHAR(50) NULL,
        [F_CREATORTIME] DATETIME NULL,
        [F_CREATORUSERID] NVARCHAR(50) NULL,
        [F_ENABLEDMARK] INT DEFAULT 1,
        [F_LastModifyTime] DATETIME NULL,
        [F_LastModifyUserId] NVARCHAR(50) NULL,
        [F_DeleteMark] INT DEFAULT 0,
        [F_DeleteTime] DATETIME NULL,
        [F_DeleteUserId] NVARCHAR(50) NULL,

        -- 业务字段
        [source_data_id] NVARCHAR(50) NOT NULL,
        [metric_code] NVARCHAR(50) NOT NULL,
        [calculated_value] NVARCHAR(200) NULL,
        [calculation_time] DATETIME NOT NULL,
        [formula_version] INT DEFAULT 1,
        [calculation_status] NVARCHAR(20) DEFAULT N'SUCCESS',
        [error_message] NVARCHAR(1000) NULL,

        CONSTRAINT [PK_lab_metric_result] PRIMARY KEY ([F_ID]),
        CONSTRAINT [UK_SOURCE_METRIC] UNIQUE ([source_data_id], [metric_code], [F_TENANTID], [F_DeleteMark])
    );

    -- 创建索引
    CREATE INDEX [IDX_METRIC_CODE] ON [dbo].[lab_metric_result] ([metric_code], [F_TENANTID]);
    CREATE INDEX [IDX_SOURCE_DATA_ID] ON [dbo].[lab_metric_result] ([source_data_id], [F_TENANTID]);
    CREATE INDEX [IDX_CALCULATION_TIME] ON [dbo].[lab_metric_result] ([calculation_time], [F_TENANTID]);
    CREATE INDEX [IDX_CALCULATION_STATUS] ON [dbo].[lab_metric_result] ([calculation_status], [F_TENANTID]);
END

-- 5. 创建索引优化查询性能（如果不存在）
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = N'IDX_IS_ENABLED' AND object_id = OBJECT_ID(N'[dbo].[lab_metric_definition]'))
    CREATE INDEX [IDX_IS_ENABLED] ON [dbo].[lab_metric_definition] ([is_enabled], [F_TENANTID]);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = N'IDX_METRIC_TYPE' AND object_id = OBJECT_ID(N'[dbo].[lab_metric_definition]'))
    CREATE INDEX [IDX_METRIC_TYPE] ON [dbo].[lab_metric_definition] ([metric_type], [F_TENANTID]);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = N'IDX_RETURN_TYPE' AND object_id = OBJECT_ID(N'[dbo].[lab_metric_definition]'))
    CREATE INDEX [IDX_RETURN_TYPE] ON [dbo].[lab_metric_definition] ([return_type], [F_TENANTID]);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = N'IDX_IS_VERSIONED' AND object_id = OBJECT_ID(N'[dbo].[lab_metric_definition]'))
    CREATE INDEX [IDX_IS_VERSIONED] ON [dbo].[lab_metric_definition] ([is_versioned], [F_TENANTID]);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = N'IDX_HAS_VARIABLES' AND object_id = OBJECT_ID(N'[dbo].[lab_metric_definition]'))
    CREATE INDEX [IDX_HAS_VARIABLES] ON [dbo].[lab_metric_definition] ([has_variables], [F_TENANTID]);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = N'IDX_STORE_RESULTS' AND object_id = OBJECT_ID(N'[dbo].[lab_metric_definition]'))
    CREATE INDEX [IDX_STORE_RESULTS] ON [dbo].[lab_metric_definition] ([store_results], [F_TENANTID]);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = N'IDX_IS_SYSTEM' AND object_id = OBJECT_ID(N'[dbo].[lab_metric_definition]'))
    CREATE INDEX [IDX_IS_SYSTEM] ON [dbo].[lab_metric_definition] ([is_system], [F_TENANTID]);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = N'IDX_SORT_ORDER' AND object_id = OBJECT_ID(N'[dbo].[lab_metric_definition]'))
    CREATE INDEX [IDX_SORT_ORDER] ON [dbo].[lab_metric_definition] ([sort_order], [F_TENANTID]);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = N'IDX_CALCULATION_ORDER' AND object_id = OBJECT_ID(N'[dbo].[lab_metric_definition]'))
    CREATE INDEX [IDX_CALCULATION_ORDER] ON [dbo].[lab_metric_definition] ([calculation_order], [F_TENANTID]);

-- 6. 注意事项：
--    a. 执行此脚本前请备份数据库
--    b. status字段可以保留作为兼容性字段，或后续删除
--    c. 如果需要删除status字段，执行：ALTER TABLE [dbo].[lab_metric_definition] DROP COLUMN [status];
--    d. 如果需要删除旧索引，执行相应DROP INDEX语句