-- ============================================
-- 原始数据导入优化 - 数据库表结构SQL
-- 数据库类型：MySQL
-- 创建日期：2025-01-XX
-- ============================================

-- ============================================
-- 1. 原始数据表（LAB_RAW_DATA）
-- ============================================
DROP TABLE IF EXISTS `LAB_RAW_DATA`;

CREATE TABLE `LAB_RAW_DATA` (
    -- 基础字段（从CLDEntityBase继承）
    `F_ID` VARCHAR(50) NOT NULL COMMENT '主键ID',
    `F_TENANTID` VARCHAR(50) DEFAULT NULL COMMENT '租户ID',
    `F_CREATORTIME` DATETIME DEFAULT NULL COMMENT '创建时间',
    `F_CREATORUSERID` VARCHAR(50) DEFAULT NULL COMMENT '创建用户ID',
    `F_ENABLEDMARK` INT DEFAULT 1 COMMENT '启用标识（1-启用，0-禁用）',
    `F_LastModifyTime` DATETIME DEFAULT NULL COMMENT '修改时间',
    `F_LastModifyUserId` VARCHAR(50) DEFAULT NULL COMMENT '修改用户ID',
    `F_DeleteMark` INT DEFAULT 0 COMMENT '删除标志（0-未删除，1-已删除）',
    `F_DeleteTime` DATETIME DEFAULT NULL COMMENT '删除时间',
    `F_DeleteUserId` VARCHAR(50) DEFAULT NULL COMMENT '删除用户ID',
    
    -- 业务字段
    `F_PROD_DATE` DATETIME DEFAULT NULL COMMENT '生产日期',
    `F_FURNACE_NO` VARCHAR(100) DEFAULT NULL COMMENT '原始炉号（包含特性汉字）',
    `F_LINE_NO` VARCHAR(10) DEFAULT NULL COMMENT '产线（从炉号解析）',
    `F_SHIFT` VARCHAR(10) DEFAULT NULL COMMENT '班次（从炉号解析）',
    `F_FURNACE_NO_PARSED` VARCHAR(50) DEFAULT NULL COMMENT '炉号（从炉号解析）',
    `F_COIL_NO` VARCHAR(50) DEFAULT NULL COMMENT '卷号（从炉号解析）',
    `F_SUBCOIL_NO` VARCHAR(50) DEFAULT NULL COMMENT '分卷号（从炉号解析）',
    `F_FEATURE_SUFFIX` VARCHAR(50) DEFAULT NULL COMMENT '特性描述（从炉号解析，原始特性汉字）',
    `F_WIDTH` DECIMAL(18, 2) DEFAULT NULL COMMENT '宽度',
    `F_COIL_WEIGHT` DECIMAL(18, 2) DEFAULT NULL COMMENT '带材重量',
    
    -- 产品规格字段（用于后续计算）
    `F_PRODUCT_SPEC_ID` VARCHAR(50) DEFAULT NULL COMMENT '产品规格ID',
    `F_PRODUCT_SPEC_CODE` VARCHAR(50) DEFAULT NULL COMMENT '产品规格代码',
    `F_PRODUCT_SPEC_NAME` VARCHAR(100) DEFAULT NULL COMMENT '产品规格名称',
    `F_DETECTION_COLUMNS` VARCHAR(100) DEFAULT NULL COMMENT '检测列（从产品规格中获取）',
    
    -- 检测数据列（保留原有字段，但建议使用JSON字段）
    `F_DETECTION_1` DECIMAL(18, 2) DEFAULT NULL COMMENT '检测数据列1',
    `F_DETECTION_2` DECIMAL(18, 2) DEFAULT NULL COMMENT '检测数据列2',
    `F_DETECTION_3` DECIMAL(18, 2) DEFAULT NULL COMMENT '检测数据列3',
    `F_DETECTION_4` DECIMAL(18, 2) DEFAULT NULL COMMENT '检测数据列4',
    `F_DETECTION_5` DECIMAL(18, 2) DEFAULT NULL COMMENT '检测数据列5',
    `F_DETECTION_6` DECIMAL(18, 2) DEFAULT NULL COMMENT '检测数据列6',
    `F_DETECTION_7` DECIMAL(18, 2) DEFAULT NULL COMMENT '检测数据列7',
    `F_DETECTION_8` DECIMAL(18, 2) DEFAULT NULL COMMENT '检测数据列8',
    `F_DETECTION_9` DECIMAL(18, 2) DEFAULT NULL COMMENT '检测数据列9',
    `F_DETECTION_10` DECIMAL(18, 2) DEFAULT NULL COMMENT '检测数据列10',
    `F_DETECTION_11` DECIMAL(18, 2) DEFAULT NULL COMMENT '检测数据列11',
    `F_DETECTION_12` DECIMAL(18, 2) DEFAULT NULL COMMENT '检测数据列12',
    `F_DETECTION_13` DECIMAL(18, 2) DEFAULT NULL COMMENT '检测数据列13',
    `F_DETECTION_14` DECIMAL(18, 2) DEFAULT NULL COMMENT '检测数据列14',
    `F_DETECTION_15` DECIMAL(18, 2) DEFAULT NULL COMMENT '检测数据列15',
    `F_DETECTION_16` DECIMAL(18, 2) DEFAULT NULL COMMENT '检测数据列16',
    `F_DETECTION_17` DECIMAL(18, 2) DEFAULT NULL COMMENT '检测数据列17',
    `F_DETECTION_18` DECIMAL(18, 2) DEFAULT NULL COMMENT '检测数据列18',
    `F_DETECTION_19` DECIMAL(18, 2) DEFAULT NULL COMMENT '检测数据列19',
    `F_DETECTION_20` DECIMAL(18, 2) DEFAULT NULL COMMENT '检测数据列20',
    `F_DETECTION_21` DECIMAL(18, 2) DEFAULT NULL COMMENT '检测数据列21',
    `F_DETECTION_22` DECIMAL(18, 2) DEFAULT NULL COMMENT '检测数据列22',
    
    -- 新增字段（优化方案）
    `F_DETECTION_DATA` JSON DEFAULT NULL COMMENT '检测数据列（JSON格式，存储动态检测数据，格式：{"1": 值1, "2": 值2, ...}）',
    `F_IS_VALID_DATA` INT DEFAULT 0 COMMENT '有效数据标识（0-非有效数据，1-有效数据，符合炉号解析规则）',
    `F_SOURCE_FILE_ID` VARCHAR(50) DEFAULT NULL COMMENT 'Excel源文件ID（关联文件服务）',
    `F_IMPORT_SESSION_ID` VARCHAR(50) DEFAULT NULL COMMENT '导入会话ID（关联导入会话）',
    
    -- 特性信息字段（用于后续计算）
    `F_APPEARANCE_FEATURE_IDS` JSON DEFAULT NULL COMMENT '匹配后的特性ID列表（JSON格式，数组：["feature-id-1", "feature-id-2", ...]）',
    
    -- 导入相关字段
    `F_IMPORT_ERROR` VARCHAR(500) DEFAULT NULL COMMENT '导入错误信息',
    `F_IMPORT_STATUS` INT DEFAULT 0 COMMENT '导入状态（0-成功，1-失败）',
    `F_SORTCODE` BIGINT DEFAULT NULL COMMENT '排序码',
    
    PRIMARY KEY (`F_ID`),
    KEY `idx_prod_date` (`F_PROD_DATE`),
    KEY `idx_furnace_no` (`F_FURNACE_NO`),
    KEY `idx_product_spec_id` (`F_PRODUCT_SPEC_ID`),
    KEY `idx_import_session_id` (`F_IMPORT_SESSION_ID`),
    KEY `idx_is_valid_data` (`F_IS_VALID_DATA`),
    KEY `idx_import_status` (`F_IMPORT_STATUS`),
    KEY `idx_tenant_id` (`F_TENANTID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='原始数据表';

-- ============================================
-- 2. 中间数据表（LAB_INTERMEDIATE_DATA）
-- ============================================
DROP TABLE IF EXISTS `LAB_INTERMEDIATE_DATA`;

CREATE TABLE `LAB_INTERMEDIATE_DATA` (
    -- 基础字段（从CLDEntityBase继承）
    `F_ID` VARCHAR(50) NOT NULL COMMENT '主键ID',
    `F_TENANTID` VARCHAR(50) DEFAULT NULL COMMENT '租户ID',
    `F_CREATORTIME` DATETIME DEFAULT NULL COMMENT '创建时间',
    `F_CREATORUSERID` VARCHAR(50) DEFAULT NULL COMMENT '创建用户ID',
    `F_ENABLEDMARK` INT DEFAULT 1 COMMENT '启用标识（1-启用，0-禁用）',
    `F_LastModifyTime` DATETIME DEFAULT NULL COMMENT '修改时间',
    `F_LastModifyUserId` VARCHAR(50) DEFAULT NULL COMMENT '修改用户ID',
    `F_DeleteMark` INT DEFAULT 0 COMMENT '删除标志（0-未删除，1-已删除）',
    `F_DeleteTime` DATETIME DEFAULT NULL COMMENT '删除时间',
    `F_DeleteUserId` VARCHAR(50) DEFAULT NULL COMMENT '删除用户ID',
    
    -- 关联字段
    `F_RAW_DATA_ID` VARCHAR(50) DEFAULT NULL COMMENT '原始数据ID',
    
    -- 日期相关字段
    `F_DATE_MONTH` VARCHAR(10) DEFAULT NULL COMMENT '日期（yyyy-MM格式，可手动修改）',
    `F_PROD_DATE` DATETIME DEFAULT NULL COMMENT '生产日期（原始数据日期）',
    
    -- 炉号相关字段
    `F_SPRAY_NO` VARCHAR(100) DEFAULT NULL COMMENT '喷次（产线+班次+日期+炉号组合）',
    `F_LINE_NO` VARCHAR(10) DEFAULT NULL COMMENT '产线',
    `F_SHIFT` VARCHAR(10) DEFAULT NULL COMMENT '班次',
    `F_FURNACE_NO_PARSED` VARCHAR(50) DEFAULT NULL COMMENT '炉号（解析后的炉号数字部分）',
    `F_FURNACE_NO` VARCHAR(100) DEFAULT NULL COMMENT '原始炉号（去掉特性汉字后的炉号）',
    `F_COIL_NO` VARCHAR(50) DEFAULT NULL COMMENT '卷号',
    `F_SUBCOIL_NO` VARCHAR(50) DEFAULT NULL COMMENT '分卷号',
    
    -- 产品规格字段（用于后续计算）
    `F_PRODUCT_SPEC_ID` VARCHAR(50) DEFAULT NULL COMMENT '产品规格ID',
    `F_PRODUCT_SPEC_NAME` VARCHAR(100) DEFAULT NULL COMMENT '产品规格名称',
    
    -- 重量和尺寸字段
    `F_ONE_METER_WEIGHT` DECIMAL(18, 2) DEFAULT NULL COMMENT '一米带材重量(g)：原始数据带材重量/产品规格长度',
    `F_STRIP_WIDTH` DECIMAL(18, 2) DEFAULT NULL COMMENT '带宽(mm)',
    
    -- 厚度相关字段
    `F_THICKNESS_RANGE` VARCHAR(50) DEFAULT NULL COMMENT '带厚范围（最小值～最大值）',
    `F_THICKNESS_MIN` DECIMAL(18, 2) DEFAULT NULL COMMENT '带厚最小值',
    `F_THICKNESS_MAX` DECIMAL(18, 2) DEFAULT NULL COMMENT '带厚最大值',
    `F_THICKNESS_DIFF` DECIMAL(18, 1) DEFAULT NULL COMMENT '带厚极差：最大值-最小值，一位小数',
    `F_AVG_THICKNESS` DECIMAL(18, 2) DEFAULT NULL COMMENT '平均厚度',
    
    -- 物理性能字段
    `F_DENSITY` DECIMAL(18, 2) DEFAULT NULL COMMENT '密度 (g/cm3)',
    `F_LAMINATION_FACTOR` DECIMAL(18, 2) DEFAULT NULL COMMENT '叠片系数',
    
    -- 外观特性字段
    `F_APPEARANCE_FEATURE` VARCHAR(200) DEFAULT NULL COMMENT '外观特性（原始特性汉字）',
    `F_APPEARANCE_FEATURE_IDS` JSON DEFAULT NULL COMMENT '匹配后的特性ID列表（JSON格式，数组：["feature-id-1", "feature-id-2", ...]）',
    
    -- 其他业务字段
    `F_BREAK_COUNT` INT DEFAULT NULL COMMENT '断头数（个）',
    `F_COIL_WEIGHT_KG` DECIMAL(18, 2) DEFAULT NULL COMMENT '单卷重量（kg）',
    `F_APPEARANCE_INSPECTOR_ID` VARCHAR(50) DEFAULT NULL COMMENT '外观检验员ID',
    `F_APPEARANCE_INSPECTOR_NAME` VARCHAR(50) DEFAULT NULL COMMENT '外观检验员姓名',
    
    -- 判定字段
    `F_MAGNETIC_RESULT` VARCHAR(50) DEFAULT NULL COMMENT '磁性能判定',
    `F_THICKNESS_RESULT` VARCHAR(50) DEFAULT NULL COMMENT '厚度判定',
    `F_LAMINATION_RESULT` VARCHAR(50) DEFAULT NULL COMMENT '叠片系数判定',
    
    -- 其他计算字段
    `F_FOUR_METER_WEIGHT` DECIMAL(18, 2) DEFAULT NULL COMMENT '四米带材重量（g）',
    `F_MAX_THICKNESS_RAW` DECIMAL(18, 2) DEFAULT NULL COMMENT '最大厚度（检测列最大值）',
    `F_MAX_AVG_THICKNESS` DECIMAL(18, 2) DEFAULT NULL COMMENT '最大平均厚度（最大值/层数）',
    `F_STRIP_TYPE` DECIMAL(18, 2) DEFAULT NULL COMMENT '带型（中间段相对两侧段的位置判断）',
    `F_SEGMENT_TYPE` VARCHAR(20) DEFAULT NULL COMMENT '分段类型（前端/中端/后端）',
    `F_REMARK` VARCHAR(500) DEFAULT NULL COMMENT '备注',
    
    -- 带厚分布（1米带厚，检测列/层数）
    `F_THICKNESS_1` DECIMAL(18, 2) DEFAULT NULL COMMENT '带厚1',
    `F_THICKNESS_2` DECIMAL(18, 2) DEFAULT NULL COMMENT '带厚2',
    `F_THICKNESS_3` DECIMAL(18, 2) DEFAULT NULL COMMENT '带厚3',
    `F_THICKNESS_4` DECIMAL(18, 2) DEFAULT NULL COMMENT '带厚4',
    `F_THICKNESS_5` DECIMAL(18, 2) DEFAULT NULL COMMENT '带厚5',
    `F_THICKNESS_6` DECIMAL(18, 2) DEFAULT NULL COMMENT '带厚6',
    `F_THICKNESS_7` DECIMAL(18, 2) DEFAULT NULL COMMENT '带厚7',
    `F_THICKNESS_8` DECIMAL(18, 2) DEFAULT NULL COMMENT '带厚8',
    `F_THICKNESS_9` DECIMAL(18, 2) DEFAULT NULL COMMENT '带厚9',
    `F_THICKNESS_10` DECIMAL(18, 2) DEFAULT NULL COMMENT '带厚10',
    `F_THICKNESS_ABNORMAL` VARCHAR(200) DEFAULT NULL COMMENT '带厚异常标记（JSON格式，标记哪些带厚需要红色标注）',
    
    -- 叠片系数厚度分布（原始检测列）
    `F_LAMINATION_DIST_1` DECIMAL(18, 2) DEFAULT NULL COMMENT '叠片系数分布1',
    `F_LAMINATION_DIST_2` DECIMAL(18, 2) DEFAULT NULL COMMENT '叠片系数分布2',
    `F_LAMINATION_DIST_3` DECIMAL(18, 2) DEFAULT NULL COMMENT '叠片系数分布3',
    `F_LAMINATION_DIST_4` DECIMAL(18, 2) DEFAULT NULL COMMENT '叠片系数分布4',
    `F_LAMINATION_DIST_5` DECIMAL(18, 2) DEFAULT NULL COMMENT '叠片系数分布5',
    `F_LAMINATION_DIST_6` DECIMAL(18, 2) DEFAULT NULL COMMENT '叠片系数分布6',
    `F_LAMINATION_DIST_7` DECIMAL(18, 2) DEFAULT NULL COMMENT '叠片系数分布7',
    `F_LAMINATION_DIST_8` DECIMAL(18, 2) DEFAULT NULL COMMENT '叠片系数分布8',
    `F_LAMINATION_DIST_9` DECIMAL(18, 2) DEFAULT NULL COMMENT '叠片系数分布9',
    `F_LAMINATION_DIST_10` DECIMAL(18, 2) DEFAULT NULL COMMENT '叠片系数分布10',
    
    -- 性能数据（可编辑）
    `F_PERF_SS_POWER` DECIMAL(18, 2) DEFAULT NULL COMMENT '1.35T 50Hz Ss激磁功率',
    `F_PERF_PS_LOSS` DECIMAL(18, 2) DEFAULT NULL COMMENT '1.35T 50Hz Ps铁损',
    `F_PERF_HC` DECIMAL(18, 2) DEFAULT NULL COMMENT '1.35T 50Hz Hc',
    `F_PERF_AFTER_SS_POWER` DECIMAL(18, 2) DEFAULT NULL COMMENT '刻痕后性能 Ss激磁功率',
    `F_PERF_AFTER_PS_LOSS` DECIMAL(18, 2) DEFAULT NULL COMMENT '刻痕后性能 Ps铁损',
    `F_PERF_AFTER_HC` DECIMAL(18, 2) DEFAULT NULL COMMENT '刻痕后性能 Hc',
    `F_PERF_EDITOR_ID` VARCHAR(50) DEFAULT NULL COMMENT '性能数据编辑人ID',
    `F_PERF_EDITOR_NAME` VARCHAR(50) DEFAULT NULL COMMENT '性能数据编辑人姓名',
    `F_PERF_EDIT_TIME` DATETIME DEFAULT NULL COMMENT '性能数据编辑时间',
    `F_PERF_JUDGE_NAME` VARCHAR(50) DEFAULT NULL COMMENT '性能判定人',
    
    -- 外观特性（可编辑）
    `F_TOUGHNESS` VARCHAR(50) DEFAULT NULL COMMENT '韧性',
    `F_FISH_SCALE` VARCHAR(50) DEFAULT NULL COMMENT '鱼鳞纹',
    `F_MID_SI` VARCHAR(50) DEFAULT NULL COMMENT '中Si',
    `F_MID_B` VARCHAR(50) DEFAULT NULL COMMENT '中B',
    `F_LEFT_PATTERN` VARCHAR(50) DEFAULT NULL COMMENT '左花纹',
    `F_MID_PATTERN` VARCHAR(50) DEFAULT NULL COMMENT '中花纹',
    `F_RIGHT_PATTERN` VARCHAR(50) DEFAULT NULL COMMENT '右花纹',
    `F_APPEAR_EDITOR_ID` VARCHAR(50) DEFAULT NULL COMMENT '外观特性编辑人ID',
    `F_APPEAR_EDITOR_NAME` VARCHAR(50) DEFAULT NULL COMMENT '外观特性编辑人姓名',
    `F_APPEAR_EDIT_TIME` DATETIME DEFAULT NULL COMMENT '外观特性编辑时间',
    `F_APPEAR_JUDGE_NAME` VARCHAR(50) DEFAULT NULL COMMENT '外观检验员',
    
    -- 其他字段
    `F_DETECTION_COLUMNS` VARCHAR(100) DEFAULT NULL COMMENT '检测列配置（从产品规格中获取）',
    `F_SORTCODE` BIGINT DEFAULT NULL COMMENT '排序码',
    
    PRIMARY KEY (`F_ID`),
    KEY `idx_raw_data_id` (`F_RAW_DATA_ID`),
    KEY `idx_prod_date` (`F_PROD_DATE`),
    KEY `idx_furnace_no` (`F_FURNACE_NO`),
    KEY `idx_product_spec_id` (`F_PRODUCT_SPEC_ID`),
    KEY `idx_tenant_id` (`F_TENANTID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='中间数据表';

-- ============================================
-- 3. 导入会话表（lab_raw_data_import_session）
-- ============================================
DROP TABLE IF EXISTS `lab_raw_data_import_session`;

CREATE TABLE `lab_raw_data_import_session` (
    `F_ID` VARCHAR(50) NOT NULL COMMENT '主键ID',
    `F_TENANTID` VARCHAR(50) DEFAULT NULL COMMENT '租户ID',
    `F_FILE_NAME` VARCHAR(200) NOT NULL COMMENT '文件名',
    `F_SOURCE_FILE_ID` VARCHAR(50) DEFAULT NULL COMMENT 'Excel源文件ID',
    `F_IMPORT_STRATEGY` VARCHAR(20) DEFAULT 'incremental' COMMENT '导入策略：incremental/full/overwrite/deduplicate',
    `F_CURRENT_STEP` INT DEFAULT 0 COMMENT '当前步骤（0-第一步，1-第二步，2-第三步，3-第四步）',
    `F_TOTAL_ROWS` INT DEFAULT 0 COMMENT '总行数',
    `F_VALID_DATA_ROWS` INT DEFAULT 0 COMMENT '有效数据行数',
    `F_STATUS` VARCHAR(20) DEFAULT 'pending' COMMENT '状态：pending/in_progress/completed/failed/cancelled',
    `F_CREATOR_USER_ID` VARCHAR(50) DEFAULT NULL COMMENT '创建人ID',
    `F_CREATOR_TIME` DATETIME DEFAULT NULL COMMENT '创建时间',
    `F_LAST_MODIFY_TIME` DATETIME DEFAULT NULL COMMENT '最后修改时间',
    PRIMARY KEY (`F_ID`),
    KEY `idx_status` (`F_STATUS`),
    KEY `idx_creator` (`F_CREATOR_USER_ID`),
    KEY `idx_tenant_id` (`F_TENANTID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='原始数据导入会话表';

-- ============================================
-- 4. 索引说明
-- ============================================
-- 原始数据表索引：
-- - idx_prod_date: 生产日期索引，用于按日期查询
-- - idx_furnace_no: 炉号索引，用于炉号查询
-- - idx_product_spec_id: 产品规格ID索引，用于关联查询和计算
-- - idx_import_session_id: 导入会话ID索引，用于关联导入会话
-- - idx_is_valid_data: 有效数据标识索引，用于筛选有效数据
-- - idx_import_status: 导入状态索引，用于筛选导入状态
-- - idx_tenant_id: 租户ID索引，用于多租户隔离

-- 中间数据表索引：
-- - idx_raw_data_id: 原始数据ID索引，用于关联原始数据
-- - idx_prod_date: 生产日期索引，用于按日期查询
-- - idx_furnace_no: 炉号索引，用于炉号查询
-- - idx_product_spec_id: 产品规格ID索引，用于关联查询和计算
-- - idx_tenant_id: 租户ID索引，用于多租户隔离

-- 导入会话表索引：
-- - idx_status: 状态索引，用于查询未完成的导入
-- - idx_creator: 创建人索引，用于查询用户的导入会话
-- - idx_tenant_id: 租户ID索引，用于多租户隔离

-- ============================================
-- 5. 字段说明
-- ============================================
-- JSON字段格式说明：
-- 
-- 1. F_DETECTION_DATA（原始数据表）：
--    格式：{"1": 123.45, "2": 234.56, "3": 345.67, ...}
--    说明：键为检测列序号（字符串），值为检测数据（数字）
--    示例：{"1": 123.45, "2": 234.56, "3": 345.67, "4": 456.78}
--
-- 2. F_APPEARANCE_FEATURE_IDS（原始数据表和中间数据表）：
--    格式：["feature-id-1", "feature-id-2", "feature-id-3"]
--    说明：特性ID数组，一个特性汉字可以匹配多个特性（1:n关系）
--    示例：["appearance-feature-001", "appearance-feature-002"]
--
-- 3. F_THICKNESS_ABNORMAL（中间数据表）：
--    格式：["1", "3", "5"]
--    说明：需要红色标注的带厚序号数组
--    示例：["1", "3", "5"]

-- ============================================
-- 6. 注意事项
-- ============================================
-- 1. 所有表都包含租户ID字段（F_TENANTID），支持多租户隔离
-- 2. 所有表都包含软删除字段（F_DeleteMark），支持软删除
-- 3. JSON字段使用MySQL 5.7+的JSON类型，支持JSON查询和索引
-- 4. 产品规格ID和特性ID字段已包含，可用于后续计算和关联查询
-- 5. 原始数据表保留Detection1-22字段，但建议使用F_DETECTION_DATA JSON字段
-- 6. 中间数据表的F_FURNACE_NO字段存储的是去掉特性汉字后的炉号
-- 7. 中间数据表的F_APPEARANCE_FEATURE字段存储的是原始特性汉字
-- 8. 中间数据表的F_APPEARANCE_FEATURE_IDS字段存储的是匹配后的特性ID列表（JSON格式）
