-- Poxiao.Lab.Entity.IntermediateDataEntity
-- Modify DetectionColumns to INT
ALTER TABLE `LAB_INTERMEDIATE_DATA` MODIFY COLUMN `F_DETECTION_COLUMNS` INT NULL COMMENT '检测列';

-- Poxiao.Lab.Entity.IntermediateDataFormulaEntity
-- Add DefaultValue if not exists
ALTER TABLE `LAB_INTERMEDIATE_DATA_FORMULA` ADD COLUMN `F_DEFAULT_VALUE` VARCHAR(100) NULL COMMENT '默认值';

-- Poxiao.Lab.Entity.ProductSpecEntity
-- Modify DetectionColumns to INT
ALTER TABLE `LAB_PRODUCT_SPEC` MODIFY COLUMN `F_DETECTION_COLUMNS` INT NULL COMMENT '对应原始数据有效列(数量)';

-- Poxiao.Lab.Entity.RawDataEntity
-- Modify DetectionColumns to INT
ALTER TABLE `LAB_RAW_DATA` MODIFY COLUMN `F_DETECTION_COLUMNS` INT NULL COMMENT '检测列(数量)';
