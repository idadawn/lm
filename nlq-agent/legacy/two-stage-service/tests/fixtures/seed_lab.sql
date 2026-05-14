-- seed_lab.sql — 测试数据种子
-- 12 产品规格 × ≥100 LAB_INTERMEDIATE_DATA 样本/规格
-- UTC 时间戳，CLDEntityBase 审计列齐全
-- 至少 1 个规格满足：合格率 ≥ 75% 且 抽样数量 ≥ 100

SET NAMES utf8mb4;
SET TIME_ZONE = '+00:00';

-- ── 产品规格（12 个）──────────────────────────────────────
INSERT INTO LAB_PRODUCT_SPEC
    (F_ID, F_CODE, F_NAME, F_VERSION, F_DETECTION_COLUMNS, F_DESCRIPTION, F_SORTCODE,
     F_CREATORTIME, F_CREATORUSERID, F_ENABLEDMARK, F_TenantId)
VALUES
    ('spec_001', '120', '规格120', 'V1', 10, '0.12mm取向硅钢', 1,
     '2025-01-01 00:00:00', 'system', 1, 'tenant_001'),
    ('spec_002', '142', '规格142', 'V1', 10, '0.14mm取向硅钢', 2,
     '2025-01-01 00:00:00', 'system', 1, 'tenant_001'),
    ('spec_003', '170', '规格170', 'V1', 12, '0.17mm取向硅钢', 3,
     '2025-01-01 00:00:00', 'system', 1, 'tenant_001'),
    ('spec_004', '213', '规格213', 'V1', 12, '0.21mm取向硅钢', 4,
     '2025-01-01 00:00:00', 'system', 1, 'tenant_001'),
    ('spec_005', '085', '规格085', 'V1', 10, '0.085mm取向硅钢', 5,
     '2025-01-01 00:00:00', 'system', 1, 'tenant_001'),
    ('spec_006', '100', '规格100', 'V1', 10, '0.10mm取向硅钢', 6,
     '2025-01-01 00:00:00', 'system', 1, 'tenant_001'),
    ('spec_007', '130', '规格130', 'V1', 10, '0.13mm取向硅钢', 7,
     '2025-01-01 00:00:00', 'system', 1, 'tenant_001'),
    ('spec_008', '150', '规格150', 'V1', 12, '0.15mm取向硅钢', 8,
     '2025-01-01 00:00:00', 'system', 1, 'tenant_001'),
    ('spec_009', '180', '规格180', 'V1', 12, '0.18mm取向硅钢', 9,
     '2025-01-01 00:00:00', 'system', 1, 'tenant_001'),
    ('spec_010', '200', '规格200', 'V1', 12, '0.20mm取向硅钢', 10,
     '2025-01-01 00:00:00', 'system', 1, 'tenant_001'),
    ('spec_011', '230', '规格230', 'V1', 14, '0.23mm取向硅钢', 11,
     '2025-01-01 00:00:00', 'system', 1, 'tenant_001'),
    ('spec_012', '300', '规格300', 'V1', 14, '0.30mm取向硅钢', 12,
     '2025-01-01 00:00:00', 'system', 1, 'tenant_001');


-- ── 中间检测数据 ──────────────────────────────────────────
-- 用存储过程批量生成，每个规格 ≥100 条样本

DELIMITER //

DROP PROCEDURE IF EXISTS _seed_lab_data//

CREATE PROCEDURE _seed_lab_data()
BEGIN
    DECLARE i INT DEFAULT 0;
    DECLARE spec_idx INT DEFAULT 1;
    DECLARE sample_count INT;
    DECLARE qualified_pct FLOAT;
    DECLARE v_spec_id VARCHAR(50);
    DECLARE v_spec_code VARCHAR(50);
    DECLARE v_spec_name VARCHAR(100);
    DECLARE v_res_mag VARCHAR(50);
    DECLARE v_res_thick VARCHAR(50);
    DECLARE v_res_lam VARCHAR(50);
    DECLARE v_base_date DATETIME;

    SET v_base_date = UTC_TIMESTAMP();

    WHILE spec_idx <= 12 DO
        -- 每个 spec 的抽样数量和合格率
        -- spec_001: 120 samples, 75% qualified (恰好满足条件)
        -- spec_002: 200 samples, 60% qualified (合格率不达标)
        -- spec_003:  80 samples, 95% qualified (抽样数量不达标)
        -- spec_004: 150 samples, 88% qualified (都达标)
        -- spec_005: 110 samples, 70% qualified
        -- spec_006: 130 samples, 82% qualified
        -- spec_007: 100 samples, 65% qualified
        -- spec_008: 160 samples, 90% qualified
        -- spec_009: 140 samples, 55% qualified
        -- spec_010: 105 samples, 78% qualified
        -- spec_011: 170 samples, 45% qualified
        -- spec_012: 120 samples, 92% qualified

        SET sample_count = ELT(spec_idx,
            120, 200, 80, 150, 110, 130, 100, 160, 140, 105, 170, 120);
        SET qualified_pct = ELT(spec_idx,
            0.75, 0.60, 0.95, 0.88, 0.70, 0.82, 0.65, 0.90, 0.55, 0.78, 0.45, 0.92);

        SET v_spec_id = CONCAT('spec_', LPAD(spec_idx, 3, '0'));
        SET v_spec_code = ELT(spec_idx,
            '120','142','170','213','085','100','130','150','180','200','230','300');
        SET v_spec_name = ELT(spec_idx,
            '规格120','规格142','规格170','规格213','规格085','规格100',
            '规格130','规格150','规格180','规格200','规格230','规格300');

        SET i = 0;
        WHILE i < sample_count DO
            -- 按 qualified_pct 比例决定是否合格
            IF (i / sample_count) < qualified_pct THEN
                SET v_res_mag = '合格';
                SET v_res_thick = '合格';
                SET v_res_lam = '合格';
            ELSE
                -- 不合格样本：至少一项不合格
                SET v_res_mag = IF(RAND() < 0.5, '不合格', '合格');
                SET v_res_thick = IF(RAND() < 0.5, '不合格', '合格');
                SET v_res_lam = IF(RAND() < 0.3, '不合格', '合格');
            END IF;

            INSERT INTO LAB_INTERMEDIATE_DATA (
                F_ID, F_RAW_DATA_ID, F_DETECTION_DATE, F_PROD_DATE,
                F_FURNACE_NO, F_LINE_NO, F_SHIFT, F_SHIFT_NUMERIC,
                F_FURNACE_BATCH_NO, F_PRODUCT_SPEC_ID, F_PRODUCT_SPEC_CODE,
                F_PRODUCT_SPEC_NAME, F_PRODUCT_SPEC_VERSION,
                F_PERF_PS_LOSS, F_PERF_SS_POWER, F_PERF_HC,
                F_WIDTH, F_AVG_THICKNESS, F_SINGLE_COIL_WEIGHT,
                F_LAMINATION_FACTOR, F_DENSITY,
                F_MAGNETIC_RES, F_THICK_RES, F_LAM_FACTOR_RES,
                F_FIRST_INSPECTION,
                F_CALC_STATUS, F_JUDGE_STATUS,
                F_CREATORTIME, F_CREATORUSERID, F_ENABLEDMARK, F_TenantId
            ) VALUES (
                UUID(),                                       -- F_ID
                CONCAT('raw_', LPAD(i, 5, '0')),              -- F_RAW_DATA_ID
                DATE(DATE_ADD(v_base_date, INTERVAL -i DAY)), -- F_DETECTION_DATE
                DATE_ADD(v_base_date, INTERVAL -(spec_idx * 100 + i) HOUR), -- F_PROD_DATE
                CONCAT('FH-', spec_idx, '-', i),              -- F_FURNACE_NO
                (spec_idx MOD 3) + 1,                         -- F_LINE_NO
                ELT((spec_idx MOD 3) + 1, '甲', '乙', '丙'),  -- F_SHIFT
                (spec_idx MOD 3) + 1,                         -- F_SHIFT_NUMERIC
                i MOD 20,                                     -- F_FURNACE_BATCH_NO
                v_spec_id,                                    -- F_PRODUCT_SPEC_ID
                v_spec_code,                                  -- F_PRODUCT_SPEC_CODE
                v_spec_name,                                  -- F_PRODUCT_SPEC_NAME
                'V1',                                         -- F_PRODUCT_SPEC_VERSION
                ROUND(0.5 + RAND() * 0.8, 6),                -- F_PERF_PS_LOSS
                ROUND(1.0 + RAND() * 0.5, 6),                -- F_PERF_SS_POWER
                ROUND(20 + RAND() * 30, 6),                  -- F_PERF_HC
                ROUND(1000 + RAND() * 200, 6),               -- F_WIDTH
                ROUND(spec_idx * 10 + RAND() * 5, 6),        -- F_AVG_THICKNESS
                ROUND(500 + RAND() * 2000, 6),               -- F_SINGLE_COIL_WEIGHT
                ROUND(95 + RAND() * 5, 6),                   -- F_LAMINATION_FACTOR
                ROUND(7.65 + RAND() * 0.1, 6),               -- F_DENSITY
                v_res_mag,                                    -- F_MAGNETIC_RES
                v_res_thick,                                  -- F_THICK_RES
                v_res_lam,                                    -- F_LAM_FACTOR_RES
                IF(v_res_mag='合格' AND v_res_thick='合格' AND v_res_lam='合格', '合格', '不合格'),
                'SUCCESS',                                    -- F_CALC_STATUS
                'SUCCESS',                                    -- F_JUDGE_STATUS
                DATE_ADD(UTC_TIMESTAMP(), INTERVAL -(spec_idx * 100 + i) HOUR), -- F_CREATORTIME (UTC)
                'system',                                     -- F_CREATORUSERID
                1,                                            -- F_ENABLEDMARK
                'tenant_001'                                  -- F_TenantId
            );

            SET i = i + 1;
        END WHILE;

        SET spec_idx = spec_idx + 1;
    END WHILE;
END//

DELIMITER ;

CALL _seed_lab_data();
DROP PROCEDURE IF EXISTS _seed_lab_data;

-- 验证种子数据
SELECT
    F_PRODUCT_SPEC_ID,
    COUNT(*) AS sample_count,
    SUM(CASE WHEN F_MAGNETIC_RES = '合格'
             AND F_THICK_RES = '合格'
             AND F_LAM_FACTOR_RES = '合格'
        THEN 1 ELSE 0 END) AS qualified_count,
    ROUND(
        SUM(CASE WHEN F_MAGNETIC_RES = '合格'
                 AND F_THICK_RES = '合格'
                 AND F_LAM_FACTOR_RES = '合格'
            THEN 1 ELSE 0 END) * 100.0 / COUNT(*), 2
    ) AS qualified_rate
FROM LAB_INTERMEDIATE_DATA
GROUP BY F_PRODUCT_SPEC_ID
ORDER BY F_PRODUCT_SPEC_ID;
