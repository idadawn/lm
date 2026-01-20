-- ============================================
-- 产品规格公共属性定义表 - 创建脚本
-- 说明: 
--   1. 存储公共属性定义，所有产品都可以使用
--   2. 公共属性有默认值，新建产品时自动添加
-- 创建时间: 2025-01-XX
-- ============================================

-- ============================================
-- MySQL版本
-- ============================================

CREATE TABLE IF NOT EXISTS `LAB_PRODUCT_SPEC_PUBLIC_ATTRIBUTE` (
    `F_Id` VARCHAR(50) NOT NULL COMMENT '主键ID',
    `F_ATTRIBUTE_NAME` VARCHAR(100) NOT NULL COMMENT '属性名称（如：长度、层数、密度）',
    `F_ATTRIBUTE_KEY` VARCHAR(100) NOT NULL COMMENT '属性键名（如：length、layers、density）',
    `F_VALUE_TYPE` VARCHAR(20) NOT NULL COMMENT '属性值类型（decimal、int、text）',
    `F_DEFAULT_VALUE` VARCHAR(500) NULL COMMENT '默认值（存储为字符串）',
    `F_UNIT` VARCHAR(20) NULL COMMENT '属性单位（如：m、mm、kg、MPa、%）',
    `F_PRECISION` INT NULL COMMENT '精度（仅用于数字类型，小数位数）',
    `F_SORTCODE` BIGINT NULL COMMENT '排序码',
    `F_CREATORUSERID` VARCHAR(50) NULL COMMENT '创建人ID',
    `F_CREATORTIME` DATETIME NULL COMMENT '创建时间',
    `F_LastModifyUserId` VARCHAR(50) NULL COMMENT '最后修改人ID',
    `F_LastModifyTime` DATETIME NULL COMMENT '最后修改时间',
    `F_DeleteMark` INT NULL COMMENT '删除标记',
    `F_DeleteTime` DATETIME NULL COMMENT '删除时间',
    `F_DeleteUserId` VARCHAR(50) NULL COMMENT '删除人ID',
    `F_TenantId` VARCHAR(50) NULL COMMENT '租户ID',
    PRIMARY KEY (`F_Id`),
    INDEX `IDX_ATTRIBUTE_KEY` (`F_ATTRIBUTE_KEY`),
    INDEX `IDX_DELETEMARK` (`F_DeleteMark`),
    UNIQUE KEY `UK_ATTRIBUTE_KEY` (`F_ATTRIBUTE_KEY`, `F_DeleteMark`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COMMENT='产品规格公共属性定义表';

-- ============================================
-- SQL Server版本
-- ============================================

/*
CREATE TABLE [LAB_PRODUCT_SPEC_PUBLIC_ATTRIBUTE] (
    [F_Id] VARCHAR(50) NOT NULL PRIMARY KEY,
    [F_ATTRIBUTE_NAME] NVARCHAR(100) NOT NULL,
    [F_ATTRIBUTE_KEY] VARCHAR(100) NOT NULL,
    [F_VALUE_TYPE] VARCHAR(20) NOT NULL,
    [F_DEFAULT_VALUE] NVARCHAR(500) NULL,
    [F_UNIT] VARCHAR(20) NULL,
    [F_PRECISION] INT NULL,
    [F_SORTCODE] BIGINT NULL,
    [F_CREATORUSERID] VARCHAR(50) NULL,
    [F_CREATORTIME] DATETIME NULL,
    [F_LastModifyUserId] VARCHAR(50) NULL,
    [F_LastModifyTime] DATETIME NULL,
    [F_DeleteMark] INT NULL,
    [F_DeleteTime] DATETIME NULL,
    [F_DeleteUserId] VARCHAR(50) NULL,
    [F_TenantId] VARCHAR(50) NULL,
    CONSTRAINT [UK_ATTRIBUTE_KEY] UNIQUE ([F_ATTRIBUTE_KEY], [F_DeleteMark])
);

CREATE INDEX [IDX_ATTRIBUTE_KEY] ON [LAB_PRODUCT_SPEC_PUBLIC_ATTRIBUTE] ([F_ATTRIBUTE_KEY]);
CREATE INDEX [IDX_DELETEMARK] ON [LAB_PRODUCT_SPEC_PUBLIC_ATTRIBUTE] ([F_DeleteMark]);
*/

-- ============================================
-- PostgreSQL版本
-- ============================================

/*
CREATE TABLE IF NOT EXISTS "LAB_PRODUCT_SPEC_PUBLIC_ATTRIBUTE" (
    "F_Id" VARCHAR(50) NOT NULL PRIMARY KEY,
    "F_ATTRIBUTE_NAME" VARCHAR(100) NOT NULL,
    "F_ATTRIBUTE_KEY" VARCHAR(100) NOT NULL,
    "F_VALUE_TYPE" VARCHAR(20) NOT NULL,
    "F_DEFAULT_VALUE" VARCHAR(500) NULL,
    "F_UNIT" VARCHAR(20) NULL,
    "F_PRECISION" INT NULL,
    "F_SORTCODE" BIGINT NULL,
    "F_CREATORUSERID" VARCHAR(50) NULL,
    "F_CREATORTIME" TIMESTAMP NULL,
    "F_LastModifyUserId" VARCHAR(50) NULL,
    "F_LastModifyTime" TIMESTAMP NULL,
    "F_DeleteMark" INT NULL,
    "F_DeleteTime" TIMESTAMP NULL,
    "F_DeleteUserId" VARCHAR(50) NULL,
    "F_TenantId" VARCHAR(50) NULL,
    CONSTRAINT "UK_ATTRIBUTE_KEY" UNIQUE ("F_ATTRIBUTE_KEY", "F_DeleteMark")
);

CREATE INDEX "IDX_ATTRIBUTE_KEY" ON "LAB_PRODUCT_SPEC_PUBLIC_ATTRIBUTE" ("F_ATTRIBUTE_KEY");
CREATE INDEX "IDX_DELETEMARK" ON "LAB_PRODUCT_SPEC_PUBLIC_ATTRIBUTE" ("F_DeleteMark");
*/
