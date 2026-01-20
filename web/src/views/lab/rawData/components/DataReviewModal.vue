<template>
  <BasicModal
    v-bind="$attrs"
    @register="registerModal"
    title="数据核对"
    :width="1400"
    :show-cancel-btn="false"
    :show-ok-btn="false"
    destroyOnClose>
    <div class="data-review-content">
      <div style="margin-bottom: 20px">
        <a-alert
          v-if="skippedRows > 0"
          :message="`已自动跳过上次导入的 ${skippedRows} 行数据。本次从第 ${skippedRows + 1} 行开始导入。`"
          type="warning"
          show-icon
          style="margin-bottom: 10px" />
        <a-alert
          message="请对比左侧原始数据与右侧系统识别结果。数据已自动对齐，请确认无误后点击提交。"
          type="info"
          show-icon />
      </div>

      <!-- 筛选工具栏 -->
      <div style="margin-bottom: 16px; display: flex; justify-content: flex-end; align-items: center">
        <span style="margin-right: 8px">查看状态：</span>
        <a-radio-group v-model:value="statusFilter" button-style="solid">
          <a-radio-button value="all">全部</a-radio-button>
          <a-radio-button value="success">识别成功</a-radio-button>
          <a-radio-button value="failed">识别失败</a-radio-button>
        </a-radio-group>
      </div>

      <div class="review-container">
        <!-- 左侧：原始数据 -->
        <div class="review-panel">
          <div class="panel-header">
            <h3>原始数据</h3>
            <a-tag color="blue">当前展示 {{ filteredOriginalData.length }} 条</a-tag>
          </div>
          <div class="panel-content">
            <a-table
              :columns="originalColumns"
              :data-source="filteredOriginalData"
              :pagination="pagination"
              @change="handleTableChange"
              :scroll="{ x: 'max-content', y: 500 }"
              size="small"
              bordered>
              <template #bodyCell="{ column, record, index }">
                <template v-if="column.key === 'index'">
                  {{ (pagination.current - 1) * pagination.pageSize + index + 1 }}
                </template>
                <template v-else>
                  <span v-if="isDateColumn(column.title)">{{ formatDate(record[column.dataIndex]) }}</span>
                  <span v-else>{{ record[column.dataIndex] ?? '-' }}</span>
                </template>
              </template>
            </a-table>
          </div>
        </div>

        <!-- 右侧：系统识别结果 -->
        <div class="review-panel">
          <div class="panel-header">
            <h3>系统识别结果</h3>
            <a-tag color="green">当前展示 {{ filteredParsedData.length }} 条</a-tag>
          </div>
          <div class="panel-content">
            <a-table
              :columns="parsedColumns"
              :data-source="filteredParsedData"
              :pagination="pagination"
              @change="handleTableChange"
              :scroll="{ x: 'max-content', y: 500 }"
              size="small"
              bordered>
              <template #bodyCell="{ column, record, index }">
                <template v-if="column.key === 'index'">
                  {{ (pagination.current - 1) * pagination.pageSize + index + 1 }}
                </template>
                <template v-else-if="column.key === 'status'">
                  <a-tag :color="record.status === 'success' ? 'success' : 'error'">
                    {{ record.status === 'success' ? '识别成功' : '识别失败' }}
                  </a-tag>
                </template>
                <template v-else-if="column.key === 'prodDate'">
                  {{ formatDate(record[column.key]) }}
                </template>
                <template v-else-if="column.key === 'featureSuffix'">
                  <div class="feature-suffix-cell">
                    <span>{{ record[column.dataIndex] ?? '-' }}</span>
                    <a-button
                      type="link"
                      size="small"
                      @click="handleEditFeatureSuffix(record, index)"
                      style="padding: 0; margin-left: 8px">
                      <template #icon><EditOutlined /></template>
                    </a-button>
                  </div>
                </template>
                <template v-else>
                  <span>{{ record[column.dataIndex] ?? '-' }}</span>
                </template>
              </template>
            </a-table>
          </div>
        </div>
      </div>

      <!-- 统计信息 -->
      <div class="review-statistics" style="margin-top: 20px">
        <a-row :gutter="16">
          <a-col :span="8">
            <a-statistic title="本次导入总行数" :value="statistics.total" />
          </a-col>
          <a-col :span="8">
            <a-statistic title="识别成功" :value="statistics.success" :value-style="{ color: '#3f8600' }" />
          </a-col>
          <a-col :span="8">
            <a-statistic title="识别失败" :value="statistics.failed" :value-style="{ color: '#cf1322' }" />
          </a-col>
        </a-row>
      </div>

      <!-- 进度条 -->
      <div v-if="submitting" class="import-progress">
        <a-progress :percent="90" status="active" :show-info="false" />
        <div style="text-align: center; margin-top: 8px; color: #666">正在导入数据，请稍候...</div>
      </div>
    </div>
    <template #insertFooter>
      <a-button @click="handleCancel" :disabled="submitting">取消</a-button>
      <a-button type="primary" :loading="submitting" @click="handleSubmit">
        确认提交
      </a-button>
    </template>
    
    <!-- 人工匹配外观特性对话框 -->
    <FeatureMatchDialog
      ref="featureMatchDialogRef"
      :input-text="currentEditFeatureSuffix"
      :auto-match-results="autoMatchResults"
      @confirm="handleFeatureMatchConfirm"
      @cancel="handleFeatureMatchCancel"
    />
  </BasicModal>
</template>

<script lang="ts" setup>
  import { ref, computed, watch } from 'vue';
  import { EditOutlined } from '@ant-design/icons-vue';
  import { BasicModal, useModalInner } from '/@/components/Modal';
  import { useMessage } from '/@/hooks/web/useMessage';
  import { importRawData } from '/@/api/lab/rawData';
  import { formatToDate } from '/@/utils/dateUtil';
  import FeatureMatchDialog from '/@/views/lab/appearance/components/FeatureMatchDialog.vue';
  import { matchAppearanceFeature, AppearanceFeatureInfo } from '/@/api/lab/appearance';

  const emit = defineEmits(['register', 'success']);

  const { createMessage } = useMessage();
  const [registerModal, { closeModal }] = useModalInner(init);

  const originalData = ref<any[]>([]);
  const parsedData = ref<any[]>([]);
  const headerOrder = ref<string[]>([]); // 原始表头顺序
  const skippedRows = ref(0); // 跳过的行数
  const fileData = ref<any>(null);
  const submitting = ref(false);
  
  // 筛选状态
  const statusFilter = ref('all');
  
  // 人工匹配外观特性
  const featureMatchDialogRef = ref<any>(null);
  const currentEditRecord = ref<any>(null);
  const currentEditIndex = ref<number>(-1);
  const currentEditFeatureSuffix = ref<string>('');
  const autoMatchResults = ref<AppearanceFeatureInfo[]>([]);

  // 分页配置（同步两个表格）
  const pagination = ref({
    current: 1,
    pageSize: 10,
    total: 0,
    showSizeChanger: true,
  });

  function handleTableChange(pag: any) {
    pagination.value = { ...pagination.value, ...pag };
  }

  // 过滤后的数据（联动过滤）
  const filteredIndices = computed(() => {
    return parsedData.value.map((item, index) => {
        if (statusFilter.value === 'all') return index;
        if (statusFilter.value === 'success' && item.status === 'success') return index;
        if (statusFilter.value === 'failed' && item.status !== 'success') return index;
        return -1;
    }).filter(index => index !== -1);
  });

  const filteredOriginalData = computed(() => {
    return filteredIndices.value.map(index => originalData.value[index]);
  });

  const filteredParsedData = computed(() => {
    return filteredIndices.value.map(index => parsedData.value[index]);
  });

  // 监听过滤变化，重置分页
  watch(statusFilter, () => {
    pagination.value.current = 1;
    pagination.value.total = filteredIndices.value.length;
  });

  // 原始数据列配置（使用后端返回的顺序 或 默认Object.keys）
  const originalColumns = computed(() => {
    if (originalData.value.length === 0) return [];
    
    // 如果有后端返回的顺序，优先使用
    let keys: string[] = [];
    if (headerOrder.value && headerOrder.value.length > 0) {
        keys = [...headerOrder.value];
    } else {
        // 否则使用第一行的key（可能无序）
        keys = Object.keys(originalData.value[0]);
    }

    // 重新排序：日期、炉号、宽度、带材重量 排在前面
    const priority = ['日期', '炉号', '宽度', '带材重量'];
    keys.sort((a, b) => {
        const indexA = priority.indexOf(a);
        const indexB = priority.indexOf(b);
        
        if (indexA !== -1 && indexB !== -1) return indexA - indexB;
        if (indexA !== -1) return -1;
        if (indexB !== -1) return 1;
        
        // 其他字段保持原顺序 (在keys列表中的相对顺序)
        return 0;
    });

    return [
      { title: '序号', key: 'index', width: 60, fixed: 'left' },
      ...keys.map(key => ({
        title: key,
        dataIndex: key,
        key: key,
        width: 120,
      })),
    ];
  });

  // 解析结果列配置
  const parsedColumns = computed(() => {
    return [
      { title: '序号', key: 'index', width: 60, fixed: 'left' },
      { title: '日期', dataIndex: 'prodDate', key: 'prodDate', width: 120 },
      { title: '原始炉号', dataIndex: 'furnaceNo', key: 'furnaceNo', width: 150 },
      { title: '产线', dataIndex: 'lineNo', key: 'lineNo', width: 80 },
      { title: '班次', dataIndex: 'shift', key: 'shift', width: 80 },
      { title: '炉次号', dataIndex: 'furnaceBatchNo', key: 'furnaceBatchNo', width: 100 },
      { title: '卷号', dataIndex: 'coilNo', key: 'coilNo', width: 80 },
      { title: '分卷', dataIndex: 'subcoilNo', key: 'subcoilNo', width: 80 },
      { title: '带宽', dataIndex: 'width', key: 'width', width: 100 },
      { title: '重量', dataIndex: 'coilWeight', key: 'coilWeight', width: 120 },
      { title: '产品规格', dataIndex: 'productSpecName', key: 'productSpecName', width: 150 },
      { title: '检测列', dataIndex: 'detectionColumns', key: 'detectionColumns', width: 120 },
      { title: '外观特性', dataIndex: 'featureSuffix', key: 'featureSuffix', width: 120 },
      { title: '状态', key: 'status', width: 100 },
    ];
  });

  // 统计信息
  const statistics = computed(() => {
    const total = parsedData.value.length;
    const success = parsedData.value.filter(item => item.status === 'success').length;
    const failed = total - success;
    return { total, success, failed };
  });

  function init(data: any) {
    originalData.value = data?.originalData || [];
    parsedData.value = data?.parsedData || [];
    headerOrder.value = data?.headerOrder || [];
    skippedRows.value = data?.skippedRows || 0;
    fileData.value = data?.fileData || null;
    
    // 重置状态
    submitting.value = false;
    statusFilter.value = 'all';
    
    pagination.value = {
      current: 1,
      pageSize: 10,
      total: parsedData.value.length,
      showSizeChanger: true,
    };
  }

  // 格式化日期
  function formatDate(val: any) {
    if (!val) return '-';
    
    // 尝试直接格式化
    const formatted = formatToDate(val);
    if (formatted !== 'Invalid Date' && formatted !== 'Invalid date') {
        // 如果是1970年（通常是把Excel序号当毫秒处理了），尝试作为Excel Serial Date解析
        if (formatted.startsWith('1970-01-01')) {
            // 继续向下尝试
        } else {
            return formatted;
        }
    }

    // 处理 Excel 序列号 (e.g. 45234 or "45234")
    const numVal = Number(val);
    if (!isNaN(numVal) && numVal > 25569 && numVal < 2958465) {
        // 25569 (1970-01-01) ~ 2958465 (9999-12-31)
        // Excel base date is 1899-12-30 (due to leap year bug 1900)
        // JS base date is 1970-01-01. Difference is 25569 days.
        const date = new Date((numVal - 25569) * 86400 * 1000);
        return formatToDate(date);
    }
    
    // 处理 . 分隔符日期 (2025.11.01)
    if (typeof val === 'string' && val.includes('.')) {
        const normalized = val.replace(/\./g, '-');
        const formattedNorm = formatToDate(normalized);
        if (formattedNorm !== 'Invalid Date' && formattedNorm !== 'Invalid date') {
            return formattedNorm;
        }
    }

    // 无法解析时显示原始值
    return val;
  }

  // 是否是日期列（用于原始数据）
  function isDateColumn(key: string) {
    return key.includes('日期') || key.toLowerCase().includes('date');
  }

  async function handleSubmit() {
    if (!fileData.value) {
      createMessage.warning('文件数据不存在');
      return;
    }

    submitting.value = true;
    try {
      const result = await importRawData(fileData.value);
      createMessage.success(`导入完成：成功 ${result.data.successCount} 条，失败 ${result.data.failCount} 条`);
      emit('success', result);
      closeModal();
    } catch (error: any) {
      createMessage.error(error.message || '导入失败');
    } finally {
      submitting.value = false;
    }
  }

  function handleCancel() {
    closeModal();
    originalData.value = [];
    parsedData.value = [];
    headerOrder.value = [];
    skippedRows.value = 0;
    fileData.value = null;
  }

  // 编辑外观特性
  const handleEditFeatureSuffix = async (record: any, index: number) => {
    currentEditRecord.value = record;
    currentEditIndex.value = index;
    currentEditFeatureSuffix.value = record.featureSuffix || '';

    // 如果有特性描述，先尝试自动匹配
    if (currentEditFeatureSuffix.value) {
      try {
        const res: any = await matchAppearanceFeature({ text: currentEditFeatureSuffix.value });
        let resultArray: AppearanceFeatureInfo[] = [];
        if (Array.isArray(res)) {
          resultArray = res;
        } else if (res?.data && Array.isArray(res.data)) {
          resultArray = res.data;
        } else if (res?.list && Array.isArray(res.list)) {
          resultArray = res.list;
        }
        autoMatchResults.value = resultArray;
      } catch (error) {
        console.error('自动匹配失败:', error);
        autoMatchResults.value = [];
      }
    } else {
      autoMatchResults.value = [];
    }

    // 打开对话框
    featureMatchDialogRef.value?.open();
  };

  // 人工匹配确认
  const handleFeatureMatchConfirm = async (data: any) => {
    if (currentEditRecord.value && currentEditIndex.value >= 0) {
      // 更新解析数据中的外观特性
      const targetRecord = parsedData.value[currentEditIndex.value];
      if (targetRecord) {
        targetRecord.featureSuffix = data.feature.name;
        // 可以在这里添加更多字段，如 category 等
        createMessage.success(`已更新外观特性: ${data.feature.category} - ${data.feature.name}`);
      }

      // 保存人工修正记录到后端
      try {
        const { saveAppearanceFeatureCorrection } = await import('/@/api/lab/appearance');
        await saveAppearanceFeatureCorrection({
          inputText: data.inputText,
          autoMatchedFeatureId: autoMatchResults.value.length > 0 ? autoMatchResults.value[0].id : undefined,
          correctedFeatureId: data.feature.id,
          matchMode: data.matchMode,
          scenario: 'import',
          remark: `导入数据核对时人工修正，炉号: ${targetRecord.furnaceNo}`,
        });
        console.log('人工修正记录已保存');
      } catch (error) {
        console.error('保存人工修正记录失败:', error);
        // 不显示错误提示，避免影响用户体验
      }
    }
    currentEditRecord.value = null;
    currentEditIndex.value = -1;
    currentEditFeatureSuffix.value = '';
    autoMatchResults.value = [];
  };

  // 人工匹配取消
  const handleFeatureMatchCancel = () => {
    currentEditRecord.value = null;
    currentEditIndex.value = -1;
    currentEditFeatureSuffix.value = '';
    autoMatchResults.value = [];
  };
</script>

<style scoped>
  .data-review-content {
    padding: 10px 0;
  }

  .review-container {
    display: flex;
    gap: 16px;
    min-height: 500px;
  }

  .review-panel {
    flex: 1;
    display: flex;
    flex-direction: column;
    border: 1px solid #d9d9d9;
    border-radius: 4px;
    overflow: hidden;
  }

  .panel-header {
    display: flex;
    justify-content: space-between;
    align-items: center;
    padding: 12px 16px;
    background: #fafafa;
    border-bottom: 1px solid #d9d9d9;
  }

  .panel-header h3 {
    margin: 0;
    font-size: 16px;
    font-weight: 500;
  }

  .panel-content {
    flex: 1;
    padding: 16px;
    overflow: auto;
  }

  /* 强制固定表格行高以对齐 */
  :deep(.ant-table-row) {
    height: 50px !important;
  }

  :deep(.ant-table-cell) {
    white-space: nowrap;
    overflow: hidden;
    text-overflow: ellipsis;
  }

  .review-statistics {
    padding: 16px;
    background: #fafafa;
    border-radius: 4px;
  }

  .import-progress {
    margin-top: 16px;
    padding: 0 16px;
  }

  .feature-suffix-cell {
    display: flex;
    align-items: center;
    justify-content: space-between;
  }
</style>
