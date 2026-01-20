<template>
  <div class="step2-preview-container">
    <!-- 步骤说明 -->
    <a-alert
      message="第二步：数据解析与预览"
      description="系统正在解析Excel文件，解析完成后将显示数据预览。请检查数据是否正确。"
      type="info"
      show-icon
      style="margin-bottom: 20px" />

    <!-- 解析状态 -->
    <div v-if="parsing" class="parsing-status">
      <a-spin size="large" tip="正在解析数据，请稍候..." />
    </div>

    <!-- 数据预览 -->
    <div v-if="previewData && !parsing" class="preview-section">
      <h3 class="section-title">数据预览</h3>
      <div class="preview-statistics">
        <a-row :gutter="16">
          <a-col :span="8">
            <Statistic title="总行数" :value="previewData.statistics.totalRows" />
          </a-col>
          <a-col :span="8">
            <Statistic title="有效数据" :value="previewData.statistics.validDataRows"
                       :value-style="{ color: previewData.statistics.validDataRows > 0 ? '#52c41a' : '#ff4d4f' }" />
          </a-col>
          <a-col :span="8">
            <Statistic title="无效数据" :value="previewData.statistics.invalidDataRows"
                       :value-style="{ color: '#ff4d4f' }" />
          </a-col>
        </a-row>
        
        <!-- 无有效数据警告 -->
        <a-alert
          v-if="previewData.statistics.validDataRows === 0 && previewData.statistics.totalRows > 0"
          type="error"
          show-icon
          style="margin-top: 16px">
          <template #message>
            <span style="font-weight: 600;">没有有效数据可以导入！</span>
          </template>
          <template #description>
            <div>
              <p style="margin-bottom: 8px;">所有数据因炉号格式不符而被标记为无效。</p>
              <p style="margin-bottom: 4px;"><strong>炉号格式要求：</strong>[产线数字][班次汉字][8位日期]-[炉号]-[卷号]-[分卷号]</p>
              <p style="margin-bottom: 0;"><strong>示例：</strong>1甲20251101-1-4-1 或 1甲20251101-1-4-1脆</p>
            </div>
          </template>
        </a-alert>
        
        <!-- 数据库中已存在炉号提示 -->
        <a-alert
          v-if="hasExistingFurnaceNos"
          type="info"
          show-icon
          style="margin-top: 16px">
          <template #message>
            <span style="font-weight: 600;">检测到数据库中已存在的炉号</span>
          </template>
          <template #description>
            <div>
              <p style="margin-bottom: 0;">部分炉号在数据库中已存在，这些数据将被忽略，不会保存到数据库中。</p>
            </div>
          </template>
        </a-alert>
      </div>

      <!-- 筛选与统计工具栏 -->
      <div class="preview-toolbar">
        <a-radio-group v-model:value="filterType" button-style="solid">
          <a-radio-button value="all">
            全部数据 ({{ previewData.statistics.totalRows }})
          </a-radio-button>
          <a-radio-button value="valid">
            有效数据 ({{ previewData.statistics.validDataRows }})
          </a-radio-button>
          <a-radio-button value="invalid">
            无效数据 ({{ previewData.statistics.invalidDataRows }})
          </a-radio-button>
          <a-radio-button v-if="hasDuplicateFurnaceNos" value="duplicate">
            重复数据 ({{ duplicateCount }})
          </a-radio-button>
        </a-radio-group>
      </div>

      <!-- 数据表格预览（左右分栏） -->
      <div class="preview-split-container">
        <!-- 左侧：原始数据 -->
        <div class="preview-left-panel">
          <div class="panel-header">原始数据</div>
          <a-table
            ref="leftTableRef"
            :columns="leftColumns"
            :data-source="paginatedRows"
            :pagination="paginationConfig"
            @change="handleTableChange"
            size="small"
            :scroll="{ x: 'max-content' }"
            class="sync-scroll-table left-table">
            <template #bodyCell="{ column, record }">
              <template v-if="column.dataIndex === 'prodDate'">
                {{ formatDate(record.prodDate) }}
              </template>
              <template v-else-if="column.key === 'columnCount'">
                {{ (() => {
                  let count = 0;
                  for (let i = 1; i <= 22; i++) {
                    if (record[`detection${i}`] !== null && record[`detection${i}`] !== undefined) {
                      count++;
                    }
                  }
                  return count;
                })() }}
              </template>
              <template v-else>
                {{ record[column.dataIndex] ?? '-' }}
              </template>
            </template>
          </a-table>
        </div>

        <!-- 右侧：导入数据 -->
        <div class="preview-right-panel">
          <div class="panel-header">导入数据</div>
          <a-table
            ref="rightTableRef"
            :columns="rightColumns"
            :data-source="paginatedRows"
            :pagination="paginationConfig"
            @change="handleTableChange"
            size="small"
            :scroll="{ x: 'max-content' }"
            class="sync-scroll-table right-table">
            <template #bodyCell="{ column, record }">
              <template v-if="column.dataIndex === 'prodDate'">
                {{ formatDate(record.prodDate) }}
              </template>
              <template v-else-if="column.dataIndex === 'isValidData'">
                <div style="display: flex; align-items: center; gap: 8px; white-space: nowrap;">
                  <a-tag :color="getStatusColor(record)" style="margin: 0;">
                    {{ getStatusText(record) }}
                  </a-tag>
                  <template v-if="record.isDuplicateInFile">
                    <a-checkbox 
                      v-model:checked="record.selectedForImport"
                      @change="handleDuplicateSelection(record)"
                      style="font-size: 12px; margin: 0;">
                      保留此条
                    </a-checkbox>
                  </template>
                  <template v-else-if="record.existsInDatabase">
                    <a-tag color="orange" style="font-size: 12px; margin: 0;">数据库中已存在，将被忽略</a-tag>
                  </template>
                </div>
              </template>
              <template v-else-if="column.key === 'columnCount'">
                {{ (() => {
                  let count = 0;
                  for (let i = 1; i <= 22; i++) {
                    if (record[`detection${i}`] !== null && record[`detection${i}`] !== undefined) {
                      count++;
                    }
                  }
                  return count;
                })() }}
              </template>
              <template v-else-if="column.dataIndex === 'furnaceNoParsed'">
                <template v-if="record.furnaceNo && record.featureSuffix">
                  {{ record.furnaceNo.replace(new RegExp(record.featureSuffix + '$'), '') }}
                </template>
                <template v-else-if="record.lineNo !== undefined && record.shift !== undefined && 
                                     record.furnaceNoParsed !== undefined && record.coilNo !== undefined && 
                                     record.subcoilNo !== undefined && record.prodDate">
                  {{ formatFurnaceNo(record) }}
                </template>
                <template v-else-if="record.furnaceNo">
                  {{ record.furnaceNo.replace(/[脆软硬]$/, '') }}
                </template>
                <template v-else>
                  -
                </template>
              </template>
              <template v-else>
                {{ record[column.dataIndex] ?? '-' }}
              </template>
            </template>
          </a-table>
        </div>
      </div>
      
      <!-- 如果有数据但分页后为空（理论不应发生），或者空数据提示 -->
      <div v-if="filteredRows.length === 0" class="preview-empty">
        暂无符合条件的数据
      </div>
    </div>

    <!-- 错误状态 -->
    <div v-if="parseError" class="error-status">
      <a-result
        status="error"
        title="数据解析失败"
        :sub-title="parseError">
        <template #extra>
          <a-button type="primary" @click="handleRetry">重试</a-button>
        </template>
      </a-result>
    </div>
  </div>
</template>

<script lang="ts" setup>
import { ref, computed, nextTick, onMounted, watch } from 'vue';
import { message, Statistic } from 'ant-design-vue';
import { uploadAndParse, updateDuplicateSelections } from '/@/api/lab/rawData';
import type {
  ImportStrategy,
  DataPreviewResult,
} from '/@/api/lab/types/rawData';

const props = defineProps({
  importSessionId: {
    type: String,
    required: true,
  },
  fileName: {
    type: String,
    required: true,
  },
  importStrategy: {
    type: String as () => ImportStrategy,
    required: true,
  },
});

const emit = defineEmits(['next', 'prev', 'cancel']);

// 状态
const parsing = ref(false);
const previewData = ref<DataPreviewResult | null>(null);
const parseError = ref<string>('');
const showErrorDetails = ref(false);

const leftTableRef = ref();
const rightTableRef = ref();
const isScrolling = ref(false);

// 筛选与分页状态
const filterType = ref<string>('all');
const currentPage = ref(1);
const pageSize = ref(20);

// 监听筛选类型变化，重置分页
watch(filterType, () => {
  currentPage.value = 1;
});

// 计算重复数据数量
const duplicateCount = computed(() => {
  if (!previewData.value?.rows) return 0;
  return previewData.value.rows.filter((row: any) => row.isDuplicateInFile).length;
});

// 过滤后的数据
const filteredRows = computed(() => {
  if (!previewData.value?.rows) return [];
  
  if (filterType.value === 'valid') {
    return previewData.value.rows.filter(row => row.isValidData);
  } else if (filterType.value === 'invalid') {
    return previewData.value.rows.filter(row => !row.isValidData);
  } else if (filterType.value === 'duplicate') {
    return previewData.value.rows.filter((row: any) => row.isDuplicateInFile);
  }
  return previewData.value.rows;
});

// 分页配置
const paginationConfig = computed(() => ({
  current: currentPage.value,
  pageSize: pageSize.value,
  total: filteredRows.value.length,
  showSizeChanger: true,
  showQuickJumper: true,
  showTotal: (total) => `共 ${total} 条`,
}));

// 分页后的当前页数据
const paginatedRows = computed(() => {
  const start = (currentPage.value - 1) * pageSize.value;
  const end = start + pageSize.value;
  return filteredRows.value.slice(start, end);
});

// 处理表格分页变化
function handleTableChange(pagination) {
  currentPage.value = pagination.current;
  pageSize.value = pagination.pageSize;
  
  // 重置滚动条位置
  nextTick(() => {
    const leftTableBody = leftTableRef.value?.$el?.querySelector('.ant-table-body');
    const rightTableBody = rightTableRef.value?.$el?.querySelector('.ant-table-body');
    
    if (leftTableBody) {
      leftTableBody.scrollTop = 0;
    }
    if (rightTableBody) {
      rightTableBody.scrollTop = 0;
    }
    
    // 重新初始化滚动同步（因为表格内容可能已更新）
    initSyncScroll();
  });
}

// 格式化日期
function formatDate(date: string | number | undefined) {
  if (!date) return '-';
  try {
    let d: Date;
    // 处理时间戳（可能是字符串或数字）
    if (typeof date === 'number' || (typeof date === 'string' && /^\d+$/.test(date))) {
      const timestamp = typeof date === 'string' ? parseInt(date, 10) : date;
      // 判断是秒级还是毫秒级时间戳
      d = new Date(timestamp > 1000000000000 ? timestamp : timestamp * 1000);
    } else {
      d = new Date(date);
    }
    
    if (isNaN(d.getTime())) return String(date);
    
    const year = d.getFullYear();
    const month = d.getMonth() + 1; // getMonth() returns 0-11
    const day = d.getDate();
    // 格式: YYYY/M/D matches user request "2025/11/1"
    return `${year}/${month}/${day}`;
  } catch {
    return String(date);
  }
}

// 格式化炉号（不带特性描述）
function formatFurnaceNo(record: any): string {
  if (!record.prodDate) return '-';
  const date = formatDate(record.prodDate).replace(/\//g, '');
  const shiftMap: Record<number, string> = { 1: '甲', 2: '乙', 3: '丙' };
  const shiftStr = shiftMap[record.shift] || String(record.shift);
  return `${record.lineNo}${shiftStr}${date}-${record.furnaceNoParsed}-${record.coilNo}-${record.subcoilNo}`;
}

// 获取状态颜色
function getStatusColor(record: any): string {
  if (record.existsInDatabase) {
    return 'orange';
  }
  if (record.isDuplicateInFile) {
    return 'warning';
  }
  if (record.status === 'failed' || !record.isValidData) {
    return 'error';
  }
  return 'success';
}

// 获取状态文本
function getStatusText(record: any): string {
  if (record.existsInDatabase) {
    return '数据库中已存在';
  }
  if (record.isDuplicateInFile) {
    return '重复（需选择）';
  }
  if (record.status === 'failed' || !record.isValidData) {
    return '无效';
  }
  return '有效';
}

// 处理重复炉号选择
function handleDuplicateSelection(record: any) {
  console.log('handleDuplicateSelection: 被调用', record);
  if (!previewData.value?.rows) {
    console.warn('handleDuplicateSelection: previewData.value?.rows 为空');
    return;
  }
  
  // 找到所有相同标准炉号的数据
  const sameFurnaceNoRows = previewData.value.rows.filter(
    (r: any) => r.standardFurnaceNo === record.standardFurnaceNo && r.isDuplicateInFile
  );
  
  // 如果当前行被选中，取消其他行的选中状态，并将其他行标记为无效
  if (record.selectedForImport) {
    sameFurnaceNoRows.forEach((r: any) => {
      if (r.id !== record.id) {
        r.selectedForImport = false;
        // 将未选择的数据标记为无效，不再参与后续工作
        r.isValidData = false;
        r.status = 'failed';
        if (!r.errorMessage) {
          r.errorMessage = '重复数据，已选择保留其他数据';
        } else if (!r.errorMessage.includes('重复数据，已选择保留其他数据')) {
          r.errorMessage = `重复数据，已选择保留其他数据；${r.errorMessage}`;
        }
      } else {
        // 确保被选中的行是有效的
        r.isValidData = true;
        if (r.status === 'failed') {
          r.status = 'duplicate'; // 保持重复标记，但状态改为duplicate
        }
      }
    });
    
    // 更新统计信息
    updateStatistics();
  } else {
    // 如果取消选择，需要检查是否还有其他行被选中
    const hasOtherSelected = sameFurnaceNoRows.some((r: any) => r.id !== record.id && r.selectedForImport === true);
    if (!hasOtherSelected) {
      // 如果没有其他行被选中，恢复所有行的有效状态（但保持重复标记）
      sameFurnaceNoRows.forEach((r: any) => {
        if (r.id !== record.id) {
          // 恢复为有效，但保持重复标记
          r.isValidData = true;
          r.status = 'duplicate';
          // 移除"重复数据，已选择保留其他数据"的错误信息
          if (r.errorMessage && r.errorMessage.includes('重复数据，已选择保留其他数据')) {
            r.errorMessage = r.errorMessage.replace(/重复数据，已选择保留其他数据[；;]?/g, '').trim();
            if (!r.errorMessage) {
              r.errorMessage = undefined;
            }
          }
        }
      });
      // 恢复当前行为有效（如果之前是有效的）
      if (record.isValidData === false && record.status === 'failed') {
        record.isValidData = true;
        record.status = 'duplicate';
      }
      updateStatistics();
    }
  }
}

// 更新统计信息
function updateStatistics() {
  if (!previewData.value?.rows) return;
  
  const totalRows = previewData.value.rows.length;
  const validDataRows = previewData.value.rows.filter((r: any) => r.isValidData === true || r.isValidData === 1).length;
  const invalidDataRows = totalRows - validDataRows;
  
  if (previewData.value.statistics) {
    previewData.value.statistics.totalRows = totalRows;
    previewData.value.statistics.validDataRows = validDataRows;
    previewData.value.statistics.invalidDataRows = invalidDataRows;
    previewData.value.statistics.successRows = validDataRows;
    previewData.value.statistics.failRows = invalidDataRows;
  }
}

// 获取产品规格名称 (Removed unused function)
// 加载产品规格列表 (Removed unused function)

// 左侧列定义（原始数据）
// 行号、日期、炉号（和Excel一致带特性描述）、宽度、带材重量、检测列1-22
const leftColumns = computed(() => {
  const columns: any[] = [
    { title: '行号', dataIndex: 'rowIndex', width: 60, fixed: 'left' },
    { 
      title: '日期', 
      dataIndex: 'prodDate', 
      width: 100,
      customRender: ({ text }) => formatDate(text)
    },
    { title: '炉号', dataIndex: 'furnaceNo', width: 150 },
    { title: '宽度', dataIndex: 'width', width: 80 },
    { title: '带材重量', dataIndex: 'coilWeight', width: 100 },
  ];
  
  // 添加检测列1-22
  for (let i = 1; i <= 22; i++) {
    columns.push({
      title: `检测${i}`,
      dataIndex: `detection${i}`,
      width: 80,
      align: 'right',
      customRender: ({ text, record }) => {
        const value = record[`detection${i}`];
        return value !== null && value !== undefined ? value : '-';
      }
    });
  }
  
  return columns;
});

// 右侧列定义（导入数据）
// 行号、日期、炉号(不带特性描述)、特性描述、数据列数、炉号解析状态
const rightColumns = computed(() => {
  const columns: any[] = [
    { title: '行号', dataIndex: 'rowIndex', width: 60, fixed: 'left' },
    { 
      title: '日期', 
      dataIndex: 'prodDate', 
      width: 100,
      customRender: ({ text }) => formatDate(text)
    },
    { 
      title: '炉号', 
      dataIndex: 'furnaceNoParsed', 
      width: 150,
      customRender: ({ record }) => {
        // 从原始炉号中移除特性描述，生成不带特性描述的炉号
        // 格式：[产线数字][班次汉字][8位日期]-[炉号]-[卷号]-[分卷号]
        if (record.furnaceNo && record.featureSuffix) {
          // 移除特性描述后缀
          return record.furnaceNo.replace(new RegExp(record.featureSuffix + '$'), '');
        }
        // 如果有解析后的字段，组合生成炉号
        if (record.lineNo !== undefined && record.shift !== undefined && 
            record.furnaceNoParsed !== undefined && record.coilNo !== undefined && 
            record.subcoilNo !== undefined && record.prodDate) {
          const date = formatDate(record.prodDate).replace(/\//g, '');
          const shiftMap: Record<number, string> = { 1: '甲', 2: '乙', 3: '丙' };
          const shiftStr = shiftMap[record.shift] || String(record.shift);
          return `${record.lineNo}${shiftStr}${date}-${record.furnaceNoParsed}-${record.coilNo}-${record.subcoilNo}`;
        }
        // 如果都没有，尝试从原始炉号中移除特性描述
        if (record.furnaceNo) {
          // 尝试移除常见的特性描述（脆、软等）
          return record.furnaceNo.replace(/[脆软硬]$/, '');
        }
        return '-';
      }
    },
    { title: '特性描述', dataIndex: 'featureSuffix', width: 100 },
    { 
      title: '数据列数', 
      key: 'columnCount', 
      width: 100,
      align: 'center',
      customRender: ({ record }) => {
        // 计算detection1-detection22字段中有值的列数
        let count = 0;
        for (let i = 1; i <= 22; i++) {
          const key = `detection${i}`;
          if (record[key] !== null && record[key] !== undefined) {
            count++;
          }
        }
        return count;
      }
    },
    { 
      title: '炉号解析状态', 
      dataIndex: 'isValidData', 
      width: 180, 
      fixed: 'right',
      align: 'center'
    }
  ];

  return columns;
});

// 计算是否有重复的炉号（已移除提示，但保留计算属性以防其他地方使用）
const hasDuplicateFurnaceNos = computed(() => {
  if (!previewData.value?.rows) return false;
  return previewData.value.rows.some((row: any) => row.isDuplicateInFile);
});

// 计算是否有数据库中已存在的炉号
const hasExistingFurnaceNos = computed(() => {
  if (!previewData.value?.rows) return false;
  return previewData.value.rows.some((row: any) => row.existsInDatabase);
});

// 计算是否可以进入下一步（必须有有效数据，且重复的炉号都已选择）
const canGoNext = computed(() => {
  if (previewData.value === null || parsing.value) return false;
  // 必须至少有一条有效数据才能继续
  const validCount = previewData.value.statistics?.validDataRows || 0;
  if (validCount === 0) return false;
  
  // 检查重复的炉号是否都已选择
  if (previewData.value.rows) {
    const duplicateGroups = new Map<string, any[]>();
    previewData.value.rows.forEach((row: any) => {
      if (row.isDuplicateInFile && row.standardFurnaceNo) {
        if (!duplicateGroups.has(row.standardFurnaceNo)) {
          duplicateGroups.set(row.standardFurnaceNo, []);
        }
        duplicateGroups.get(row.standardFurnaceNo)!.push(row);
      }
    });
    
    // 检查每个重复组是否至少有一条被选中
    for (const [_, rows] of duplicateGroups) {
      const hasSelected = rows.some((row: any) => row.selectedForImport === true);
      if (!hasSelected) {
        return false; // 有重复组没有选择，不能继续
      }
    }
  }
  
  return true;
});

// 解析数据
async function parseData() {
  if (!props.importSessionId || !props.fileName) {
    parseError.value = '会话ID或文件名缺失';
    return;
  }

  parsing.value = true;
  parseError.value = '';

  try {
    // 调用解析接口（文件数据已在第一步保存到后端，通过 sessionId 获取）
    const result = await uploadAndParse({
      fileName: props.fileName,
      importStrategy: props.importStrategy,
      importSessionId: props.importSessionId,
    });

    // 保存预览数据
    previewData.value = result.preview;

    // 为数据添加行号，并初始化重复炉号选择状态
    if (previewData.value?.rows) {
      previewData.value.rows.forEach((row, index) => {
        row.rowIndex = index + 1;
        // 对于重复的炉号，默认选中第一条（按行号排序）
        if (row.isDuplicateInFile && row.selectedForImport === undefined) {
          // 找到所有相同标准炉号的数据
          const sameFurnaceNoRows = previewData.value.rows.filter(
            (r: any) => r.standardFurnaceNo === row.standardFurnaceNo && r.isDuplicateInFile
          );
          // 按行号排序，第一条默认选中
          sameFurnaceNoRows.sort((a: any, b: any) => (a.rowIndex || 0) - (b.rowIndex || 0));
          if (sameFurnaceNoRows[0]?.id === row.id) {
            row.selectedForImport = true;
          } else {
            row.selectedForImport = false;
          }
        }
      });
    }

    // 检查解析结果
    const validCount = previewData.value?.statistics?.validDataRows || 0;
    const totalCount = previewData.value?.statistics?.totalRows || 0;
    
    // 标记为已解析（无论是否有有效数据，只要解析成功就标记）
    hasParsed.value = true;
    
    if (validCount === 0 && totalCount > 0) {
      message.warning(`数据解析完成，但没有有效数据！共 ${totalCount} 行数据，全部因炉号格式不符而标记为无效。请检查炉号格式是否符合：[产线数字][班次汉字][8位日期]-[炉号]-[卷号]-[分卷号]，例如：1甲20251101-1-4-1`);
    } else if (validCount > 0) {
      message.success(`数据解析成功，有效数据 ${validCount} 条`);
    } else {
      message.warning('未解析到任何数据，请检查Excel文件');
    }
    
    // 初始化同步滚动（延迟以确保表格已完全渲染）
    nextTick(() => {
      setTimeout(() => {
        initSyncScroll();
      }, 200);
    });
  } catch (error: any) {
    parseError.value = error.message || '数据解析失败';
    message.error(parseError.value);
    previewData.value = null;
    // 解析失败时不标记为已解析，允许重试
    hasParsed.value = false;
  } finally {
    parsing.value = false;
  }
}

// 同步滚动逻辑
function initSyncScroll() {
  nextTick(() => {
    const leftTableBody = leftTableRef.value?.$el?.querySelector('.ant-table-body');
    const rightTableBody = rightTableRef.value?.$el?.querySelector('.ant-table-body');

    if (!leftTableBody || !rightTableBody) {
      // 如果表格还没渲染，延迟重试
      setTimeout(() => initSyncScroll(), 100);
      return;
    }

    // 移除之前的监听器（如果存在）
    if ((leftTableBody as any)._handleLeftScroll) {
      leftTableBody.removeEventListener('scroll', (leftTableBody as any)._handleLeftScroll);
    }
    if ((rightTableBody as any)._handleRightScroll) {
      rightTableBody.removeEventListener('scroll', (rightTableBody as any)._handleRightScroll);
    }

    const handleLeftScroll = (e: Event) => {
      if (isScrolling.value) return;
      isScrolling.value = true;
      const scrollTop = (e.target as HTMLElement).scrollTop;
      rightTableBody.scrollTop = scrollTop;
      // 使用requestAnimationFrame或setTimeout重置标志位，防止死循环
      window.requestAnimationFrame(() => {
        isScrolling.value = false;
      });
    };

    const handleRightScroll = (e: Event) => {
      if (isScrolling.value) return;
      isScrolling.value = true;
      const scrollTop = (e.target as HTMLElement).scrollTop;
      leftTableBody.scrollTop = scrollTop;
      window.requestAnimationFrame(() => {
        isScrolling.value = false;
      });
    };

    // 保存引用以便清理
    (leftTableBody as any)._handleLeftScroll = handleLeftScroll;
    (rightTableBody as any)._handleRightScroll = handleRightScroll;

    leftTableBody.addEventListener('scroll', handleLeftScroll, { passive: true });
    rightTableBody.addEventListener('scroll', handleRightScroll, { passive: true });

    // 清理函数存储
    (window as any)._step2ScrollCleanups = () => {
      if ((leftTableBody as any)._handleLeftScroll) {
        leftTableBody.removeEventListener('scroll', (leftTableBody as any)._handleLeftScroll);
        delete (leftTableBody as any)._handleLeftScroll;
      }
      if ((rightTableBody as any)._handleRightScroll) {
        rightTableBody.removeEventListener('scroll', (rightTableBody as any)._handleRightScroll);
        delete (rightTableBody as any)._handleRightScroll;
      }
    };
  });
}

// 重试解析
function handleRetry() {
  hasParsed.value = false; // 重置解析标记，允许重新解析
  parseData();
}

// 下一步
async function handleNext() {
  console.log('handleNext: 被调用');
  if (!previewData.value) {
    console.warn('handleNext: previewData.value 为空');
    message.warning('数据尚未解析完成，请稍候');
    return;
  }

  // 检查是否有有效数据
  const validCount = previewData.value.statistics?.validDataRows || 0;
  if (validCount === 0) {
    message.error('没有有效数据！请检查Excel文件中的炉号格式是否正确。炉号格式要求：[产线数字][班次汉字][8位日期]-[炉号]-[卷号]-[分卷号]，例如：1甲20251101-1-4-1');
    return;
  }

  // 检查重复的炉号是否都已选择，并保存选择结果
  if (previewData.value.rows) {
    const duplicateGroups = new Map<string, any[]>();
    previewData.value.rows.forEach((row: any) => {
      if (row.isDuplicateInFile && row.standardFurnaceNo) {
        if (!duplicateGroups.has(row.standardFurnaceNo)) {
          duplicateGroups.set(row.standardFurnaceNo, []);
        }
        duplicateGroups.get(row.standardFurnaceNo)!.push(row);
      }
    });
    
    // 如果有重复数据，检查每个重复组是否至少有一条被选中
    if (duplicateGroups.size > 0) {
      for (const [furnaceNo, rows] of duplicateGroups) {
        const hasSelected = rows.some((row: any) => row.selectedForImport === true);
        if (!hasSelected) {
          message.warning(`炉号 ${furnaceNo} 存在重复，请选择保留哪条数据`);
          return;
        }
      }
      
      // 保存重复数据的选择结果到后端
      console.log('开始保存重复数据选择结果...');
      try {
        await saveDuplicateSelections();
        console.log('重复数据选择结果保存完成');
        // 保存成功后，等待一小段时间确保后端数据已更新
        await new Promise(resolve => setTimeout(resolve, 500));
      } catch (error: any) {
        console.error('保存重复数据选择结果失败:', error);
        message.error('保存重复数据选择结果失败，请重试');
        return;
      }
    } else {
      console.log('没有重复数据，跳过保存');
    }
  }

  emit('next');
}

// 保存重复数据的选择结果到后端
async function saveDuplicateSelections() {
  if (!previewData.value?.rows || !props.importSessionId) {
    console.warn('saveDuplicateSelections: 缺少必要的数据或会话ID');
    return;
  }
  
  console.log('saveDuplicateSelections: 开始处理，总数据行数:', previewData.value.rows.length);
  
  // 找出所有需要更新的重复数据
  const updates: { rawDataId: string; isValidData: boolean }[] = [];
  const duplicateGroups = new Map<string, any[]>();
  
  // 按标准炉号分组
  previewData.value.rows.forEach((row: any) => {
    if (row.isDuplicateInFile && row.standardFurnaceNo) {
      if (!duplicateGroups.has(row.standardFurnaceNo)) {
        duplicateGroups.set(row.standardFurnaceNo, []);
      }
      duplicateGroups.get(row.standardFurnaceNo)!.push(row);
    }
  });
  
  console.log(`saveDuplicateSelections: 找到 ${duplicateGroups.size} 个重复炉号组`);
  
  if (duplicateGroups.size === 0) {
    console.log('saveDuplicateSelections: 没有重复数据，无需保存');
    return;
  }
  
  // 对于每个重复组，更新所有数据的状态
  for (const [furnaceNo, rows] of duplicateGroups) {
    console.log(`处理重复炉号组: ${furnaceNo}, 共 ${rows.length} 条数据`);
    
    // 检查是否有被选中的数据
    const selectedRows = rows.filter((r: any) => r.selectedForImport === true);
    console.log(`  - 被选中的数据: ${selectedRows.length} 条`);
    
    rows.forEach((row: any) => {
      // 根据 selectedForImport 确定是否有效
      // 如果 selectedForImport 为 true，则有效；否则无效
      const isValid = row.selectedForImport === true;
      
      // 同时检查 isValidData 字段，确保一致性
      const currentIsValid = row.isValidData === true || row.isValidData === 1;
      
      if (isValid !== currentIsValid) {
        console.warn(`  - 数据ID: ${row.id}, selectedForImport 和 isValidData 不一致! selectedForImport: ${row.selectedForImport}, isValidData: ${row.isValidData}`);
        // 使用 selectedForImport 的值，因为它更准确
        row.isValidData = isValid;
      }
      
      console.log(`  - 数据ID: ${row.id}, selectedForImport: ${row.selectedForImport}, isValidData: ${row.isValidData}, 保存为: ${isValid}`);
      updates.push({
        rawDataId: row.id,
        isValidData: isValid
      });
    });
  }
  
  if (updates.length === 0) {
    console.log('saveDuplicateSelections: 没有需要更新的重复数据');
    return;
  }
  
  console.log(`saveDuplicateSelections: 准备保存 ${updates.length} 条重复数据的更新:`, updates);
  console.log(`saveDuplicateSelections: 调用API，会话ID: ${props.importSessionId}`);
  
  try {
    // 调用API保存更新
    await updateDuplicateSelections(props.importSessionId, updates);
    console.log('saveDuplicateSelections: 重复数据选择结果已保存到后端成功:', updates.length, '条数据');
  } catch (error: any) {
    console.error('saveDuplicateSelections: 保存重复数据选择结果失败:', error);
    // 抛出错误，阻止进入下一步，确保数据一致性
    throw error;
  }
}

// 保存并进入下一步（供父组件调用）
async function saveAndNext() {
  await handleNext();
}

// 标记是否已经解析过（避免重复解析）
const hasParsed = ref(false);

// 组件挂载时自动解析数据（如果有会话ID）
onMounted(() => {
  if (props.importSessionId && !hasParsed.value) {
    parseData();
  }
});

// 监听 importSessionId 变化，当从空变为有值时触发解析
watch(
  () => props.importSessionId,
  (newId, oldId) => {
    // 当 importSessionId 从空变为有值，且还没有解析过时，触发解析
    if (newId && !oldId && !hasParsed.value) {
      parseData();
    }
  },
  { immediate: false }
);

// 暴露给父组件
defineExpose({
  canGoNext,
  saveAndNext,
});
</script>

<style lang="less" scoped>
.step2-preview-container {
  padding: 0;
  position: relative;
}

.section-title {
  font-size: 15px;
  font-weight: 600;
  margin-bottom: 12px;
  color: #262626;
  line-height: 1.5;
}

.parsing-status {
  padding: 60px 0;
  text-align: center;
}

.preview-section {
  margin-bottom: 24px;
  padding: 16px;
  background: #fafafa;
  border-radius: 8px;
  border: 1px solid #f0f0f0;
}

.preview-statistics {
  padding: 20px;
  background: #fff;
  border-radius: 8px;
  margin-bottom: 16px;
  border: 1px solid #f0f0f0;
}

.error-alert {
  margin-bottom: 16px;
  margin-top: 16px;
}

.error-summary {
  max-height: 150px;
  overflow-y: auto;
}

.error-item {
  padding: 4px 0;
  font-size: 12px;
  color: #595959;
  border-bottom: 1px solid #f0f0f0;

  &:last-child {
    border-bottom: none;
  }
}

.more-errors {
  margin-top: 8px;
  font-size: 12px;
  color: #8c8c8c;
  font-style: italic;
}

.preview-split-container {
  display: flex;
  gap: 16px;
  min-height: 400px; /* 最小高度，允许根据内容扩展 */
  margin-bottom: 16px;
}

.preview-left-panel, 
.preview-right-panel {
  flex: 1;
  display: flex;
  flex-direction: column;
  background: #fff;
  border: 1px solid #f0f0f0;
  border-radius: 8px;
  overflow: hidden;
}

.panel-header {
  padding: 12px 16px;
  font-weight: 600;
  background: #fafafa;
  border-bottom: 1px solid #f0f0f0;
}

.sync-scroll-table {
  flex: 1;
  display: flex;
  flex-direction: column;
  min-height: 0;
  
  :deep(.ant-table) {
    display: flex;
    flex-direction: column;
    flex: 1;
  }
  
  :deep(.ant-table-container) {
    display: flex;
    flex-direction: column;
    flex: 1;
  }
  
  :deep(.ant-table-body) {
    flex: 1;
    overflow: auto !important; /* 同时支持横向和纵向滚动 */
    max-height: 500px; /* 限制表格内容区域的最大高度 */
  }
  
  :deep(.ant-table-thead > tr > th),
  :deep(.ant-table-tbody > tr > td) {
    height: 40px; /* 固定行高 */
    line-height: 24px;
    padding: 8px 12px !important;
    white-space: nowrap; /* 防止内容换行导致行高不一致 */
    overflow: hidden;
    text-overflow: ellipsis;
  }
  
  // 确保右侧表格的状态列内容在一行显示
  :deep(.ant-table-cell-fix-right) {
    .ant-tag,
    .ant-checkbox-wrapper {
      white-space: nowrap;
      display: inline-flex;
      align-items: center;
    }
  }
  
  :deep(.ant-pagination) {
    margin-top: 16px;
    flex-shrink: 0;
    padding: 0 16px;
  }
}

.preview-more {
  padding: 12px;
  text-align: center;
  color: #8c8c8c;
  font-size: 14px;
  background: #fafafa;
  border-top: 1px solid #f0f0f0;
}

.furnace-no-cell {
  .parsed-info {
    margin-top: 4px;
  }
}

.preview-toolbar {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 16px;
}

.pagination-info {
  font-size: 14px;
  color: #8c8c8c;
}

.preview-empty {
  text-align: center;
  padding: 40px 0;
  color: #8c8c8c;
  background: #fff;
  border: 1px solid #f0f0f0;
  border-radius: 8px;
}

.detection-value {
  font-family: 'Courier New', monospace;
  font-size: 12px;
}

.error-status {
  padding: 40px 0;
}
</style>
