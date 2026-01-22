-- 添加导入文件哈希字段（用于重复上传识别）

-- MySQL 版本
SET @db_name = DATABASE();

-- lab_raw_data_import_session
SELECT COUNT(*) INTO @session_field_exists
FROM information_schema.COLUMNS
WHERE TABLE_SCHEMA = @db_name
    AND TABLE_NAME = 'lab_raw_data_import_session'
    AND COLUMN_NAME = 'F_SOURCE_FILE_HASH';

SET @session_sql = IF(@session_field_exists = 0,
    'ALTER TABLE `lab_raw_data_import_session` ADD COLUMN `F_SOURCE_FILE_HASH` VARCHAR(128) DEFAULT NULL COMMENT \'Excel源文件哈希\';',
    'SELECT \"F_SOURCE_FILE_HASH already exists in lab_raw_data_import_session\" AS Result;'
);

PREPARE stmt_session FROM @session_sql;
EXECUTE stmt_session;
DEALLOCATE PREPARE stmt_session;

SELECT COUNT(*) INTO @session_md5_exists
FROM information_schema.COLUMNS
WHERE TABLE_SCHEMA = @db_name
    AND TABLE_NAME = 'lab_raw_data_import_session'
    AND COLUMN_NAME = 'F_SOURCE_FILE_MD5';

SET @session_md5_sql = IF(@session_md5_exists = 0,
    'ALTER TABLE `lab_raw_data_import_session` ADD COLUMN `F_SOURCE_FILE_MD5` VARCHAR(32) DEFAULT NULL COMMENT \'Excel源文件MD5\';',
    'SELECT \"F_SOURCE_FILE_MD5 already exists in lab_raw_data_import_session\" AS Result;'
);

PREPARE stmt_session_md5 FROM @session_md5_sql;
EXECUTE stmt_session_md5;
DEALLOCATE PREPARE stmt_session_md5;

SELECT COUNT(*) INTO @session_valid_hash_exists
FROM information_schema.COLUMNS
WHERE TABLE_SCHEMA = @db_name
    AND TABLE_NAME = 'lab_raw_data_import_session'
    AND COLUMN_NAME = 'F_VALID_DATA_HASH';

SET @session_valid_hash_sql = IF(@session_valid_hash_exists = 0,
    'ALTER TABLE `lab_raw_data_import_session` ADD COLUMN `F_VALID_DATA_HASH` VARCHAR(128) DEFAULT NULL COMMENT \'有效数据哈希\';',
    'SELECT \"F_VALID_DATA_HASH already exists in lab_raw_data_import_session\" AS Result;'
);

PREPARE stmt_session_valid_hash FROM @session_valid_hash_sql;
EXECUTE stmt_session_valid_hash;
DEALLOCATE PREPARE stmt_session_valid_hash;

-- lab_raw_data_import_log
SELECT COUNT(*) INTO @log_field_exists
FROM information_schema.COLUMNS
WHERE TABLE_SCHEMA = @db_name
    AND TABLE_NAME = 'lab_raw_data_import_log'
    AND COLUMN_NAME = 'F_SOURCE_FILE_HASH';

SET @log_sql = IF(@log_field_exists = 0,
    'ALTER TABLE `lab_raw_data_import_log` ADD COLUMN `F_SOURCE_FILE_HASH` VARCHAR(128) DEFAULT NULL COMMENT \'Excel源文件哈希\';',
    'SELECT \"F_SOURCE_FILE_HASH already exists in lab_raw_data_import_log\" AS Result;'
);

PREPARE stmt_log FROM @log_sql;
EXECUTE stmt_log;
DEALLOCATE PREPARE stmt_log;

SELECT COUNT(*) INTO @log_md5_exists
FROM information_schema.COLUMNS
WHERE TABLE_SCHEMA = @db_name
    AND TABLE_NAME = 'lab_raw_data_import_log'
    AND COLUMN_NAME = 'F_SOURCE_FILE_MD5';

SET @log_md5_sql = IF(@log_md5_exists = 0,
    'ALTER TABLE `lab_raw_data_import_log` ADD COLUMN `F_SOURCE_FILE_MD5` VARCHAR(32) DEFAULT NULL COMMENT \'Excel源文件MD5\';',
    'SELECT \"F_SOURCE_FILE_MD5 already exists in lab_raw_data_import_log\" AS Result;'
);

PREPARE stmt_log_md5 FROM @log_md5_sql;
EXECUTE stmt_log_md5;
DEALLOCATE PREPARE stmt_log_md5;

SELECT COUNT(*) INTO @log_valid_hash_exists
FROM information_schema.COLUMNS
WHERE TABLE_SCHEMA = @db_name
    AND TABLE_NAME = 'lab_raw_data_import_log'
    AND COLUMN_NAME = 'F_VALID_DATA_HASH';

SET @log_valid_hash_sql = IF(@log_valid_hash_exists = 0,
    'ALTER TABLE `lab_raw_data_import_log` ADD COLUMN `F_VALID_DATA_HASH` VARCHAR(128) DEFAULT NULL COMMENT \'有效数据哈希\';',
    'SELECT \"F_VALID_DATA_HASH already exists in lab_raw_data_import_log\" AS Result;'
);

PREPARE stmt_log_valid_hash FROM @log_valid_hash_sql;
EXECUTE stmt_log_valid_hash;
DEALLOCATE PREPARE stmt_log_valid_hash;
