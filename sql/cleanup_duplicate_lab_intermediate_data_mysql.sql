-- =============================================================================
-- 清理 LAB_INTERMEDIATE_DATA 中同一炉号的多条重复记录（保留一条，删除其余）
-- 数据库：MySQL 8.0+（需支持窗口函数 ROW_NUMBER）
-- 表名：LAB_INTERMEDIATE_DATA（与 IntermediateDataEntity 一致）
--
-- 保留规则（优先级从高到低）：
--   1. F_RAW_DATA_ID 仍存在于 LAB_RAW_DATA 且未逻辑删除的记录优先保留
--   2. 否则保留 F_CREATORTIME 最新的一条
--   3. 再否则保留 F_Id 字典序最小的一条（稳定）
--
-- 仅处理：F_FURNACE_NO 非空、且 F_DeleteMark 为 0 或 NULL 的记录
--
-- 使用步骤：
--   1. 先执行「一、预览」中的查询，确认重复数量与样例
--   2. 在测试库或备份后，执行「二、删除」
--   3. 如需软删除而非物理删除，见「三、软删除变体」
-- =============================================================================

-- -----------------------------------------------------------------------------
-- 一、预览：按租户 + 炉号统计重复组数与总冗余行数
-- -----------------------------------------------------------------------------
SELECT
    COALESCE(F_TenantId, '') AS tenant_key,
    F_FURNACE_NO,
    COUNT(*) AS cnt,
    COUNT(*) - 1 AS redundant_rows
FROM LAB_INTERMEDIATE_DATA
WHERE (F_DeleteMark IS NULL OR F_DeleteMark = 0)
  AND F_FURNACE_NO IS NOT NULL
  AND TRIM(F_FURNACE_NO) <> ''
GROUP BY COALESCE(F_TenantId, ''), F_FURNACE_NO
HAVING COUNT(*) > 1
ORDER BY redundant_rows DESC, cnt DESC;

-- 预览：本次将删除的总行数（与下面 DELETE 条件一致）
SELECT COUNT(*) AS rows_to_delete
FROM (
    SELECT
        i.F_Id,
        ROW_NUMBER() OVER (
            PARTITION BY COALESCE(i.F_TenantId, ''), i.F_FURNACE_NO
            ORDER BY
                EXISTS (
                    SELECT 1
                    FROM LAB_RAW_DATA r
                    WHERE r.F_Id = i.F_RAW_DATA_ID
                      AND (r.F_DeleteMark IS NULL OR r.F_DeleteMark = 0)
                ) DESC,
                i.F_CREATORTIME DESC,
                i.F_Id
        ) AS rn
    FROM LAB_INTERMEDIATE_DATA i
    WHERE (i.F_DeleteMark IS NULL OR i.F_DeleteMark = 0)
      AND i.F_FURNACE_NO IS NOT NULL
      AND TRIM(i.F_FURNACE_NO) <> ''
) AS ranked
WHERE ranked.rn > 1;

-- 预览：将要删除的 F_Id 列表（仅前 500 条示例，全量请去掉 LIMIT）
/*
SELECT d.F_Id,
       d.F_TenantId,
       d.F_FURNACE_NO,
       d.F_RAW_DATA_ID,
       d.F_CREATORTIME,
       d.rn
FROM (
    SELECT
        i.F_Id,
        i.F_TenantId,
        i.F_FURNACE_NO,
        i.F_RAW_DATA_ID,
        i.F_CREATORTIME,
        ROW_NUMBER() OVER (
            PARTITION BY COALESCE(i.F_TenantId, ''), i.F_FURNACE_NO
            ORDER BY
                EXISTS (
                    SELECT 1
                    FROM LAB_RAW_DATA r
                    WHERE r.F_Id = i.F_RAW_DATA_ID
                      AND (r.F_DeleteMark IS NULL OR r.F_DeleteMark = 0)
                ) DESC,
                i.F_CREATORTIME DESC,
                i.F_Id
        ) AS rn
    FROM LAB_INTERMEDIATE_DATA i
    WHERE (i.F_DeleteMark IS NULL OR i.F_DeleteMark = 0)
      AND i.F_FURNACE_NO IS NOT NULL
      AND TRIM(i.F_FURNACE_NO) <> ''
) AS d
WHERE d.rn > 1
ORDER BY d.F_FURNACE_NO, d.F_Id
LIMIT 500;
*/

-- -----------------------------------------------------------------------------
-- 二、物理删除重复行（保留每组一条）
-- 执行前请先备份： mysqldump ... LAB_INTERMEDIATE_DATA > backup.sql
-- 建议在事务中执行，确认行数后再 COMMIT
-- -----------------------------------------------------------------------------
/*
START TRANSACTION;

DELETE FROM LAB_INTERMEDIATE_DATA
WHERE F_Id IN (
    SELECT x.F_Id
    FROM (
        SELECT
            i.F_Id,
            ROW_NUMBER() OVER (
                PARTITION BY COALESCE(i.F_TenantId, ''), i.F_FURNACE_NO
                ORDER BY
                    EXISTS (
                        SELECT 1
                        FROM LAB_RAW_DATA r
                        WHERE r.F_Id = i.F_RAW_DATA_ID
                          AND (r.F_DeleteMark IS NULL OR r.F_DeleteMark = 0)
                    ) DESC,
                    i.F_CREATORTIME DESC,
                    i.F_Id
            ) AS rn
        FROM LAB_INTERMEDIATE_DATA i
        WHERE (i.F_DeleteMark IS NULL OR i.F_DeleteMark = 0)
          AND i.F_FURNACE_NO IS NOT NULL
          AND TRIM(i.F_FURNACE_NO) <> ''
    ) AS x
    WHERE x.rn > 1
);

-- 查看受影响行数后：
-- COMMIT;
-- 或
-- ROLLBACK;
*/

-- -----------------------------------------------------------------------------
-- 三、软删除变体（若业务要求只打删除标记，不物理删）
-- 将 F_DeleteMark=1, F_DeleteTime=NOW() 等按项目规范填写
-- -----------------------------------------------------------------------------
/*
START TRANSACTION;

UPDATE LAB_INTERMEDIATE_DATA t
INNER JOIN (
    SELECT x.F_Id
    FROM (
        SELECT
            i.F_Id,
            ROW_NUMBER() OVER (
                PARTITION BY COALESCE(i.F_TenantId, ''), i.F_FURNACE_NO
                ORDER BY
                    EXISTS (
                        SELECT 1
                        FROM LAB_RAW_DATA r
                        WHERE r.F_Id = i.F_RAW_DATA_ID
                          AND (r.F_DeleteMark IS NULL OR r.F_DeleteMark = 0)
                    ) DESC,
                    i.F_CREATORTIME DESC,
                    i.F_Id
            ) AS rn
        FROM LAB_INTERMEDIATE_DATA i
        WHERE (i.F_DeleteMark IS NULL OR i.F_DeleteMark = 0)
          AND i.F_FURNACE_NO IS NOT NULL
          AND TRIM(i.F_FURNACE_NO) <> ''
    ) AS x
    WHERE x.rn > 1
) dup ON t.F_Id = dup.F_Id
SET
    t.F_DeleteMark = 1,
    t.F_DeleteTime = NOW();

-- COMMIT;
*/
