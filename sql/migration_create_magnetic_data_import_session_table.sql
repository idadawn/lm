-- ============================================
-- 磁性数据导入会话表
-- 数据库类型：MySQL
-- 创建日期：2025-01-XX
-- ============================================

CREATE TABLE IF NOT EXISTS `lab_magnetic_data_import_session` (
    `F_ID` VARCHAR(50) NOT NULL COMMENT '主键ID',
    `F_TENANTID` VARCHAR(50) DEFAULT NULL COMMENT '租户ID',
    `F_FILE_NAME` VARCHAR(200) NOT NULL COMMENT '文件名',
    `F_SOURCE_FILE_ID` VARCHAR(500) DEFAULT NULL COMMENT 'Excel源文件ID',
    `F_PARSED_DATA_FILE` VARCHAR(500) DEFAULT NULL COMMENT '解析后的数据JSON文件路径（临时存储，完成导入后才写入数据库）',
    `F_CURRENT_STEP` INT DEFAULT 0 COMMENT '当前步骤（0-第一步，1-第二步）',
    `F_TOTAL_ROWS` INT DEFAULT 0 COMMENT '总行数',
    `F_VALID_DATA_ROWS` INT DEFAULT 0 COMMENT '有效数据行数',
    `F_STATUS` VARCHAR(20) DEFAULT 'pending' COMMENT '状态：pending/in_progress/completed/failed/cancelled',
    `F_CREATOR_USER_ID` VARCHAR(50) DEFAULT NULL COMMENT '创建人ID',
    `F_CREATOR_TIME` DATETIME DEFAULT NULL COMMENT '创建时间',
    `F_LAST_MODIFY_TIME` DATETIME DEFAULT NULL COMMENT '最后修改时间',
    `F_LAST_MODIFY_USER_ID` VARCHAR(50) DEFAULT NULL COMMENT '修改用户ID',
    `F_ENABLEDMARK` INT DEFAULT 1 COMMENT '启用标识（1-启用，0-禁用）',
    `F_DeleteMark` INT DEFAULT 0 COMMENT '删除标志（0-未删除，1-已删除）',
    `F_DeleteTime` DATETIME DEFAULT NULL COMMENT '删除时间',
    `F_DeleteUserId` VARCHAR(50) DEFAULT NULL COMMENT '删除用户ID',
    PRIMARY KEY (`F_ID`),
    KEY `idx_status` (`F_STATUS`),
    KEY `idx_creator` (`F_CREATOR_USER_ID`),
    KEY `idx_tenant_id` (`F_TENANTID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='磁性数据导入会话表';
