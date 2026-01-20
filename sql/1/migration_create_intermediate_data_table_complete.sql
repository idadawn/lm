-- ============================================
-- 中间数据表（LAB_INTERMEDIATE_DATA）完整创建脚本
-- 创建时间：2026-01-20
-- 描述：根据 IntermediateDataEntity.cs 实体类生成完整的表结构
-- ============================================

DROP TABLE IF EXISTS `lab_intermediate_data`;

CREATE TABLE `lab_intermediate_data` (
    -- ============================================
    -- 基础字段（从CLDEntityBase继承）
    -- ============================================
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
    
    -- ============================================
    -- 关联字段
    -- ============================================
    `F_RAW_DATA_ID` VARCHAR(50) DEFAULT NULL COMMENT '原始数据ID',
    
    -- ============================================
    -- 日期和炉号相关字段
    -- ============================================
    `F_DETECTION_DATE` VARCHAR(10) DEFAULT NULL COMMENT '检测日期',
    `F_FURNACE_NO_FORMATTED` VARCHAR(200) DEFAULT NULL COMMENT '炉号（格式：[产线数字][班次汉字][8位日期]-[炉次号]-[卷号]-[分卷号]）',
    `F_PROD_DATE` DATETIME DEFAULT NULL COMMENT '生产日期（原始数据日期）',
    `F_SPRAY_NO` VARCHAR(50) DEFAULT NULL COMMENT '喷次（8位日期-炉号）',
    `F_LINE_NO` INT DEFAULT NULL COMMENT '产线（从炉号解析）',
    `F_SHIFT` VARCHAR(10) DEFAULT NULL COMMENT '班次（从炉号解析，存储原始汉字：甲、乙、丙）',
    `F_SHIFT_NUMERIC` INT DEFAULT NULL COMMENT '班次数字（用于排序：甲=1, 乙=2, 丙=3）',
    `F_FURNACE_BATCH_NO` INT DEFAULT NULL COMMENT '炉次号（从炉号解析）',
    `F_COIL_NO` DECIMAL(18, 2) DEFAULT NULL COMMENT '卷号（从炉号解析，支持小数）',
    `F_SUBCOIL_NO` DECIMAL(18, 2) DEFAULT NULL COMMENT '分卷号（从炉号解析，支持小数）',
    
    -- ============================================
    -- 性能数据（可编辑）
    -- ============================================
    `F_PERF_SS_POWER` DECIMAL(18, 4) DEFAULT NULL COMMENT '1.35T 50Hz Ss激磁功率 (VA/kg)',
    `F_PERF_PS_LOSS` DECIMAL(18, 4) DEFAULT NULL COMMENT '1.35T 50Hz Ps铁损 (W/kg)',
    `F_PERF_HC` DECIMAL(18, 4) DEFAULT NULL COMMENT '1.35T 50Hz Hc (A/m)',
    `F_AFTER_SS_POWER` DECIMAL(18, 4) DEFAULT NULL COMMENT '刻痕后性能 Ss激磁功率 (VA/kg)',
    `F_AFTER_PS_LOSS` DECIMAL(18, 4) DEFAULT NULL COMMENT '刻痕后性能 Ps铁损 (W/kg)',
    `F_AFTER_HC` DECIMAL(18, 4) DEFAULT NULL COMMENT '刻痕后性能 Hc (A/m)',
    `F_PERF_EDITOR_ID` VARCHAR(50) DEFAULT NULL COMMENT '性能数据编辑人ID',
    `F_PERF_EDITOR_NAME` VARCHAR(50) DEFAULT NULL COMMENT '性能数据编辑人姓名',
    `F_PERF_EDIT_TIME` DATETIME DEFAULT NULL COMMENT '性能数据编辑时间',
    `F_PERF_EDITOR` VARCHAR(50) DEFAULT NULL COMMENT '性能录入员，自动获取当前用户',
    `F_PERF_JUDGE_NAME` VARCHAR(50) DEFAULT NULL COMMENT '性能判定人',
    
    -- ============================================
    -- 重量和尺寸字段
    -- ============================================
    `F_ONE_METER_WT` DECIMAL(18, 2) DEFAULT NULL COMMENT '一米带材重量(g)：F_FOUR_METER_WT / STD_LENGTH',
    `F_FOUR_METER_WT` DECIMAL(18, 1) DEFAULT NULL COMMENT '四米带材重量（g），原始样段称重数据',
    `F_WIDTH` DECIMAL(18, 2) DEFAULT NULL COMMENT '宽度',
    `F_STRIP_WIDTH` DECIMAL(18, 2) DEFAULT NULL COMMENT '带宽 (mm)',
    `F_COIL_WEIGHT` DECIMAL(18, 2) DEFAULT NULL COMMENT '带材重量',
    `F_SINGLE_COIL_WEIGHT` DECIMAL(18, 2) DEFAULT NULL COMMENT '单卷重量(kg)',
    `F_COIL_WEIGHT_KG` DECIMAL(18, 2) DEFAULT NULL COMMENT '单卷重量（kg）',
    
    -- ============================================
    -- 带厚分布（1米带厚，检测列/层数）
    -- ============================================
    `F_THICK_1` DECIMAL(18, 2) DEFAULT NULL COMMENT '带厚1 (μm)，前端动态计算列：F_LAM_DIST_i / LAYERS',
    `F_THICK_2` DECIMAL(18, 2) DEFAULT NULL COMMENT '带厚2',
    `F_THICK_3` DECIMAL(18, 2) DEFAULT NULL COMMENT '带厚3',
    `F_THICK_4` DECIMAL(18, 2) DEFAULT NULL COMMENT '带厚4',
    `F_THICK_5` DECIMAL(18, 2) DEFAULT NULL COMMENT '带厚5',
    `F_THICK_6` DECIMAL(18, 2) DEFAULT NULL COMMENT '带厚6',
    `F_THICK_7` DECIMAL(18, 2) DEFAULT NULL COMMENT '带厚7',
    `F_THICK_8` DECIMAL(18, 2) DEFAULT NULL COMMENT '带厚8',
    `F_THICK_9` DECIMAL(18, 2) DEFAULT NULL COMMENT '带厚9',
    `F_THICK_10` DECIMAL(18, 2) DEFAULT NULL COMMENT '带厚10',
    `F_THICK_11` DECIMAL(18, 2) DEFAULT NULL COMMENT '带厚11',
    `F_THICK_12` DECIMAL(18, 2) DEFAULT NULL COMMENT '带厚12',
    `F_THICK_13` DECIMAL(18, 2) DEFAULT NULL COMMENT '带厚13',
    `F_THICK_14` DECIMAL(18, 2) DEFAULT NULL COMMENT '带厚14',
    `F_THICK_15` DECIMAL(18, 2) DEFAULT NULL COMMENT '带厚15',
    `F_THICK_16` DECIMAL(18, 2) DEFAULT NULL COMMENT '带厚16',
    `F_THICK_17` DECIMAL(18, 2) DEFAULT NULL COMMENT '带厚17',
    `F_THICK_18` DECIMAL(18, 2) DEFAULT NULL COMMENT '带厚18',
    `F_THICK_19` DECIMAL(18, 2) DEFAULT NULL COMMENT '带厚19',
    `F_THICK_20` DECIMAL(18, 2) DEFAULT NULL COMMENT '带厚20',
    `F_THICK_21` DECIMAL(18, 2) DEFAULT NULL COMMENT '带厚21',
    `F_THICK_22` DECIMAL(18, 2) DEFAULT NULL COMMENT '带厚22',
    `F_THICK_ABNORMAL` VARCHAR(200) DEFAULT NULL COMMENT '带厚异常标记（JSON格式，标记哪些带厚需要红色标注）',
    
    -- ============================================
    -- 带厚统计字段
    -- ============================================
    `F_THICK_RANGE` VARCHAR(50) DEFAULT NULL COMMENT '带厚范围（最小值～最大值）',
    `F_THICK_MIN` DECIMAL(18, 2) DEFAULT NULL COMMENT '带厚最小值，MIN(F_THICK_1..22)',
    `F_THICK_MAX` DECIMAL(18, 2) DEFAULT NULL COMMENT '带厚最大值，MAX(F_THICK_1..22)',
    `F_THICK_DIFF` DECIMAL(18, 1) DEFAULT NULL COMMENT '带厚极差 (μm)：MAX(F_THICK_i) - MIN(F_THICK_i)',
    `F_AVG_THICKNESS` DECIMAL(18, 2) DEFAULT NULL COMMENT '平均厚度 (μm)：AVG(F_LAM_DIST_1..22) / LAYERS',
    
    -- ============================================
    -- 物理性能字段
    -- ============================================
    `F_DENSITY` DECIMAL(18, 2) DEFAULT NULL COMMENT '密度 (g/cm³)：(F_ONE_M_WT * 1000) / (F_WIDTH * F_AVG_THICK)',
    `F_LAM_FACTOR` DECIMAL(18, 2) DEFAULT NULL COMMENT '叠片系数 (%)：F_FOUR_M_WT / (F_WIDTH * 400 * F_AVG_THICK * THEO_DENSITY * 10^-7)',
    
    -- ============================================
    -- 外观特性（可编辑）
    -- ============================================
    `F_FEATURE_SUFFIX` VARCHAR(50) DEFAULT NULL COMMENT '特性描述（从炉号解析）',
    `F_TOUGHNESS` VARCHAR(50) DEFAULT NULL COMMENT '韧性',
    `F_FISH_SCALE` VARCHAR(50) DEFAULT NULL COMMENT '鱼鳞纹',
    `F_BREAK_COUNT` INT DEFAULT NULL COMMENT '断头数(个)',
    `F_APPEAR_EDITOR_ID` VARCHAR(50) DEFAULT NULL COMMENT '外观检验员编辑人ID',
    `F_APPEAR_EDITOR_NAME` VARCHAR(50) DEFAULT NULL COMMENT '外观检验员编辑人姓名',
    `F_APPEAR_EDIT_TIME` DATETIME DEFAULT NULL COMMENT '外观检验员编辑时间',
    `F_MID_SI_LEFT` VARCHAR(50) DEFAULT NULL COMMENT '中Si (左)，中Si含量左侧检测值',
    `F_MID_SI_RIGHT` VARCHAR(50) DEFAULT NULL COMMENT '中Si (右)，中Si含量右侧检测值',
    `F_MID_B_LEFT` VARCHAR(50) DEFAULT NULL COMMENT '中B (左)，中B含量左侧检测值',
    `F_MID_B_RIGHT` VARCHAR(50) DEFAULT NULL COMMENT '中B (右)，中B含量右侧检测值',
    `F_L_PATTERN_W` DECIMAL(18, 2) DEFAULT NULL COMMENT '左花纹纹宽，实测宽度',
    `F_L_PATTERN_S` DECIMAL(18, 2) DEFAULT NULL COMMENT '左花纹纹间距，实测间距',
    `F_M_PATTERN_W` DECIMAL(18, 2) DEFAULT NULL COMMENT '中花纹纹宽，实测宽度',
    `F_M_PATTERN_S` DECIMAL(18, 2) DEFAULT NULL COMMENT '中花纹纹间距，实测间距',
    `F_R_PATTERN_W` DECIMAL(18, 2) DEFAULT NULL COMMENT '右花纹纹宽，实测宽度',
    `F_R_PATTERN_S` DECIMAL(18, 2) DEFAULT NULL COMMENT '右花纹纹间距，实测间距',
    `F_APPEAR_FEATURE` VARCHAR(200) DEFAULT NULL COMMENT '外观特性（原始特性汉字），炉号后缀解析（如"脆"）',
    `F_APPEARANCE_FEATURE_IDS` JSON DEFAULT NULL COMMENT '匹配后的特性ID列表（JSON格式，数组：["feature-id-1", "feature-id-2", ...]）',
    `F_APPEAR_JUDGE` VARCHAR(50) DEFAULT NULL COMMENT '外观检验员，自动获取当前用户',
    
    -- ============================================
    -- 判定字段
    -- ============================================
    `F_MAGNETIC_RES` VARCHAR(50) DEFAULT NULL COMMENT '磁性能判定，根据 Ps 铁损值逻辑判断',
    `F_THICK_RES` VARCHAR(50) DEFAULT NULL COMMENT '厚度判定，平均厚度达标判定',
    `F_LAM_FACTOR_RES` VARCHAR(50) DEFAULT NULL COMMENT '叠片系数判定，叠片系数达标判定',
    
    -- ============================================
    -- 检测数据列（原始检测列，固定22列）
    -- ============================================
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
    
    -- ============================================
    -- 叠片系数厚度分布（原始检测列，数据库核心存储列）
    -- ============================================
    `F_LAM_DIST_1` DECIMAL(18, 1) DEFAULT NULL COMMENT '叠片系数分布1，数据库核心存储列（对应原始总厚度）',
    `F_LAM_DIST_2` DECIMAL(18, 1) DEFAULT NULL COMMENT '叠片系数分布2，数据库核心存储列（对应原始总厚度）',
    `F_LAM_DIST_3` DECIMAL(18, 1) DEFAULT NULL COMMENT '叠片系数分布3，数据库核心存储列（对应原始总厚度）',
    `F_LAM_DIST_4` DECIMAL(18, 1) DEFAULT NULL COMMENT '叠片系数分布4，数据库核心存储列（对应原始总厚度）',
    `F_LAM_DIST_5` DECIMAL(18, 1) DEFAULT NULL COMMENT '叠片系数分布5，数据库核心存储列（对应原始总厚度）',
    `F_LAM_DIST_6` DECIMAL(18, 1) DEFAULT NULL COMMENT '叠片系数分布6，数据库核心存储列（对应原始总厚度）',
    `F_LAM_DIST_7` DECIMAL(18, 1) DEFAULT NULL COMMENT '叠片系数分布7，数据库核心存储列（对应原始总厚度）',
    `F_LAM_DIST_8` DECIMAL(18, 1) DEFAULT NULL COMMENT '叠片系数分布8，数据库核心存储列（对应原始总厚度）',
    `F_LAM_DIST_9` DECIMAL(18, 1) DEFAULT NULL COMMENT '叠片系数分布9，数据库核心存储列（对应原始总厚度）',
    `F_LAM_DIST_10` DECIMAL(18, 1) DEFAULT NULL COMMENT '叠片系数分布10，数据库核心存储列（对应原始总厚度）',
    `F_LAM_DIST_11` DECIMAL(18, 1) DEFAULT NULL COMMENT '叠片系数分布11，数据库核心存储列（对应原始总厚度）',
    `F_LAM_DIST_12` DECIMAL(18, 1) DEFAULT NULL COMMENT '叠片系数分布12，数据库核心存储列（对应原始总厚度）',
    `F_LAM_DIST_13` DECIMAL(18, 1) DEFAULT NULL COMMENT '叠片系数分布13，数据库核心存储列（对应原始总厚度）',
    `F_LAM_DIST_14` DECIMAL(18, 1) DEFAULT NULL COMMENT '叠片系数分布14，数据库核心存储列（对应原始总厚度）',
    `F_LAM_DIST_15` DECIMAL(18, 1) DEFAULT NULL COMMENT '叠片系数分布15，数据库核心存储列（对应原始总厚度）',
    `F_LAM_DIST_16` DECIMAL(18, 1) DEFAULT NULL COMMENT '叠片系数分布16，数据库核心存储列（对应原始总厚度）',
    `F_LAM_DIST_17` DECIMAL(18, 1) DEFAULT NULL COMMENT '叠片系数分布17，数据库核心存储列（对应原始总厚度）',
    `F_LAM_DIST_18` DECIMAL(18, 1) DEFAULT NULL COMMENT '叠片系数分布18，数据库核心存储列（对应原始总厚度）',
    `F_LAM_DIST_19` DECIMAL(18, 1) DEFAULT NULL COMMENT '叠片系数分布19，数据库核心存储列（对应原始总厚度）',
    `F_LAM_DIST_20` DECIMAL(18, 1) DEFAULT NULL COMMENT '叠片系数分布20，数据库核心存储列（对应原始总厚度）',
    `F_LAM_DIST_21` DECIMAL(18, 1) DEFAULT NULL COMMENT '叠片系数分布21，数据库核心存储列（对应原始总厚度）',
    `F_LAM_DIST_22` DECIMAL(18, 1) DEFAULT NULL COMMENT '叠片系数分布22，数据库核心存储列（对应原始总厚度）',
    
    -- ============================================
    -- 其他计算字段
    -- ============================================
    `F_MAX_THICKNESS_RAW` DECIMAL(18, 2) DEFAULT NULL COMMENT '最大厚度（检测列最大值）',
    `F_MAX_AVG_THICKNESS` DECIMAL(18, 2) DEFAULT NULL COMMENT '最大平均厚度（最大值/层数）',
    `F_STRIP_TYPE` DECIMAL(18, 2) DEFAULT NULL COMMENT '带型，依据中段与两侧均值差计算',
    `F_SEGMENT_TYPE` VARCHAR(20) DEFAULT NULL COMMENT '分段类型（前端/中端/后端）',
    `F_REMARK` VARCHAR(500) DEFAULT NULL COMMENT '备注',
    `F_FIRST_INSPECTION` VARCHAR(50) DEFAULT NULL COMMENT '一次交检',
    `F_REQUIRES_MANUAL_CONFIRM` TINYINT(1) DEFAULT NULL COMMENT '是否需要人工确认（特性匹配置信度 < 90% 时需要人工确认）',
    
    -- ============================================
    -- 产品规格相关字段
    -- ============================================
    `F_PRODUCT_SPEC_ID` VARCHAR(50) DEFAULT NULL COMMENT '产品规格ID',
    `F_PRODUCT_SPEC_CODE` VARCHAR(50) DEFAULT NULL COMMENT '产品规格代码',
    `F_PRODUCT_SPEC_NAME` VARCHAR(100) DEFAULT NULL COMMENT '产品规格名称',
    `F_PRODUCT_SPEC_VERSION` VARCHAR(50) DEFAULT NULL COMMENT '产品规格版本（注意：中间数据表中应为int类型，但当前实体类定义为string）',
    `F_DETECTION_COLUMNS` VARCHAR(100) DEFAULT NULL COMMENT '检测列（从产品规格中获取）',
    
    -- ============================================
    -- 主键和索引
    -- ============================================
    PRIMARY KEY (`F_ID`),
    KEY `idx_raw_data_id` (`F_RAW_DATA_ID`),
    KEY `idx_prod_date` (`F_PROD_DATE`),
    KEY `idx_furnace_no_formatted` (`F_FURNACE_NO_FORMATTED`),
    KEY `idx_product_spec_id` (`F_PRODUCT_SPEC_ID`),
    KEY `idx_tenant_id` (`F_TENANTID`),
    KEY `idx_product_spec_version` (`F_PRODUCT_SPEC_ID`, `F_PRODUCT_SPEC_VERSION`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='中间数据表';
