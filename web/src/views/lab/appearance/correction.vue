<template>
  <a-modal v-model:visible="visible" title="人工修正匹配列表" :width="1400" :confirm-loading="confirmLoading"
    :ok-button-props="{ style: { display: 'none' } }" cancel-text="关闭" @cancel="handleCancel" destroy-on-close>
    <div class="page-content-wrapper" style="width: 100%; height: 640px">
      <div class="page-content-wrapper-center">
        <div class="page-content-wrapper-content">
          <!-- 表格 -->
          <BasicTable @register="registerTable">
            <template #tableTitle>
              <a-upload :show-upload-list="false" :before-upload="handleBeforeUpload" accept=".xlsx,.xls">
                <a-button type="primary" :loading="uploadLoading">
                  <template #icon>
                    <UploadOutlined />
                  </template>
                  上传Excel生成修正列表
                </a-button>
              </a-upload>
            </template>
            <template #bodyCell="{ column, record }">
              <template v-if="column.key === 'action'">
                <TableAction :actions="[
                  {
                    label: '确认',
                    icon: 'ant-design:check-outlined',
                    color: 'success',
                    popConfirm: {
                      title: '确认将此文本添加为关键词吗？',
                      confirm: handleConfirm.bind(null, record),
                    },
                    ifShow: () => record.status !== 'Confirmed'
                  },
                  {
                    label: '删除',
                    icon: 'ant-design:delete-outlined',
                    color: 'error',
                    popConfirm: {
                      title: '是否确认删除？',
                      confirm: handleDelete.bind(null, record),
                    },
                  },
                ]" />
              </template>
              <template v-else-if="column.key === 'correctedFeatureName'">
                <div v-if="record.status === 'Pending'" class="select-feature-cell"
                  @click.stop="handleOpenFeatureDialog(record)">
                  <span v-if="record.correctedFeatureName" class="feature-name">{{ record.correctedFeatureName }}</span>
                  <span v-else class="placeholder">选择特性</span>
                  <span class="click-hint">点击选择</span>
                </div>
                <span v-else>{{ record.correctedFeatureName }}</span>
              </template>
              <template v-else-if="column.key === 'matchConfidence'">
                <span v-if="record.matchConfidence !== undefined">
                  {{ Math.round(record.matchConfidence * 100) }}%
                </span>
                <span v-else>-</span>
              </template>
              <template v-else-if="column.key === 'status'">
                <a-tag :color="record.status === 'Confirmed' ? 'green' : 'orange'">
                  {{ record.status === 'Confirmed' ? '已确认' : '待处理' }}
                </a-tag>
              </template>
            </template>
          </BasicTable>

          <!-- 特性选择对话框 -->
          <FeatureSelectDialog ref="featureSelectDialogRef" @confirm="handleFeatureSelectConfirm" />
        </div>
      </div>
    </div>
  </a-modal>
</template>

<script lang="ts" setup>
defineOptions({ name: 'labAppearanceCorrection' });
import { ref, onMounted } from 'vue';
import { UploadOutlined } from '@ant-design/icons-vue';
import { BasicTable, useTable, TableAction, BasicColumn } from '/@/components/Table';
import { useMessage } from '/@/hooks/web/useMessage';
import { getCorrectionList, deleteCorrection, confirmCorrection, updateCorrection, uploadExcelForCorrection, AppearanceFeatureCorrection } from '/@/api/lab/featureLearning';
import FeatureSelectDialog from './components/FeatureSelectDialog.vue';
import { batchMatchAppearanceFeature, type AppearanceFeatureInfo } from '/@/api/lab/appearance';
import { getAppearanceFeatureList } from '/@/api/lab/appearanceFeature';

const { createMessage } = useMessage();
const featureSelectDialogRef = ref<any>(null);
const currentRecord = ref<AppearanceFeatureCorrection | null>(null);
const visible = ref(false);
const confirmLoading = ref(false);
const autoOpen = ref(false);
const matchConfidenceMap = ref<Record<string, number>>({});
const uploadLoading = ref(false);

// 处理Excel上传
async function handleBeforeUpload(file: File) {
  uploadLoading.value = true;
  try {
    const result = await uploadExcelForCorrection(file);
    createMessage.success(result.message || '上传处理完成');
    // 刷新列表
    reload();
  } catch (error: any) {
    const errorMsg = error?.response?.data?.msg || error?.message || '上传失败';
    createMessage.error(errorMsg);
  } finally {
    uploadLoading.value = false;
  }
  return false; // 阻止默认上传行为
}

const columns: BasicColumn[] = [
  { title: '输入文本', dataIndex: 'inputText', width: 200 },
  { title: '自动匹配特性', dataIndex: 'autoMatchedFeatureName', width: 150 },
  { title: '人工修正特性', dataIndex: 'correctedFeatureName', key: 'correctedFeatureName', width: 200 },
  { title: '匹配模式', dataIndex: 'matchModeText', width: 100 },
  { title: '匹配度', dataIndex: 'matchConfidence', key: 'matchConfidence', width: 100 },
  { title: '处理状态', dataIndex: 'status', key: 'status', width: 100 },
  { title: '备注', dataIndex: 'remark', width: 200 },
  { title: '创建时间', dataIndex: 'creatorTime', width: 150 },
];

const [registerTable, { reload, setTableData }] = useTable({
  api: getCorrectionList,
  columns,
  bordered: true,
  showTableSetting: true,
  actionColumn: {
    width: 120,
    title: '操作',
    dataIndex: 'action',
  },
  afterFetch: (list: any[]) => {
    return list.map(item => ({
      ...item,
      matchConfidence: matchConfidenceMap.value[item.id],
    }));
  },
});

function handleCancel() {
  visible.value = false;
}

// 打开特性选择对话框
function handleOpenFeatureDialog(record: AppearanceFeatureCorrection) {
  currentRecord.value = record;
  if (featureSelectDialogRef.value) {
    // 传入已绑定的特性ID，用于默认选中
    featureSelectDialogRef.value.open(record.correctedFeatureId);
  } else {
    console.error('featureSelectDialogRef 未初始化');
  }
}

// 特性选择确认回调
async function handleFeatureSelectConfirm(features: AppearanceFeatureInfo[]) {
  if (!currentRecord.value || features.length === 0) return;

  const feature = features[0];
  if (features.length > 1) {
    createMessage.warning('当前仅支持关联单个特性，将自动使用第一个选中的特性。');
  }

  try {
    await updateCorrection(currentRecord.value.id, { featureId: feature.id });
    createMessage.success('已更新关联特性');
    reload();
  } catch (error: any) {
    const errorMsg = error?.response?.data?.msg || error?.message || '更新失败';
    createMessage.error(errorMsg);
  } finally {
    currentRecord.value = null;
  }
}

async function handleConfirm(record: AppearanceFeatureCorrection) {
  try {
    await confirmCorrection(record.id);
    createMessage.success('已确认并添加关键词');
    reload();
  } catch (error: any) {
    createMessage.error('操作失败');
  }
}

async function handleDelete(record: AppearanceFeatureCorrection) {
  try {
    await deleteCorrection(record.id);
    createMessage.success('删除成功');
    reload();
  } catch (error: any) {
    createMessage.error('删除失败');
  }
}

function normalizeCorrectionList(res: any): AppearanceFeatureCorrection[] {
  if (Array.isArray(res)) return res;
  if (res?.data && Array.isArray(res.data)) return res.data;
  if (res?.list && Array.isArray(res.list)) return res.list;
  return [];
}

async function autoConfirmPending() {
  confirmLoading.value = true;
  try {
    const response = await getCorrectionList();
    const list = normalizeCorrectionList(response);
    if (!Array.isArray(list) || list.length === 0) {
      matchConfidenceMap.value = {};
      setTableData([]);
      return;
    }

    const pendingItems = list.filter(
      item => item.status === 'Pending' && item.inputText
    );
    if (pendingItems.length === 0) {
      matchConfidenceMap.value = {};
      setTableData(list);
      return;
    }

    const featuresResponse = await getAppearanceFeatureList({ keyword: '' });
    const allFeatures = featuresResponse.list || [];

    const batchResult = await batchMatchAppearanceFeature({
      items: pendingItems.map(item => ({ id: item.id, query: item.inputText })),
    });

    const grouped = new Map<string, any[]>();
    (batchResult || []).forEach(item => {
      if (!grouped.has(item.id)) grouped.set(item.id, []);
      grouped.get(item.id)?.push(item);
    });

    const updatedMap: Record<string, number> = {};
    for (const record of pendingItems) {
      const matches = grouped.get(record.id) || [];
      const perfectMatch = matches.find(m => m.isPerfectMatch);
      if (!perfectMatch) {
        continue;
      }

      const featureId = resolveFeatureId(
        perfectMatch,
        allFeatures
      );
      if (!featureId) {
        continue;
      }

      await updateCorrection(record.id, {
        featureId,
        status: 'Confirmed',
        remark: '系统确认',
      });
      updatedMap[record.id] = 1;
    }

    matchConfidenceMap.value = updatedMap;
    const refreshedResponse = await getCorrectionList();
    const refreshed = normalizeCorrectionList(refreshedResponse);
    setTableData(
      refreshed.map(item => ({
        ...item,
        matchConfidence: updatedMap[item.id],
      }))
    );
  } catch (error: any) {
    console.error('[Correction] 自动确认失败:', error);
  } finally {
    confirmLoading.value = false;
  }
}

function resolveFeatureId(match: any, allFeatures: AppearanceFeatureInfo[]) {
  if (!match?.featureName) return '';

  const candidates = allFeatures.filter(f => f.name === match.featureName);
  if (candidates.length === 0) return '';

  if (match.category) {
    const byCategory = candidates.filter(f => f.category === match.category);
    if (byCategory.length === 1) return byCategory[0].id;
    if (byCategory.length > 1) {
      return resolveBySeverity(match, byCategory);
    }
  }

  return resolveBySeverity(match, candidates);
}

function resolveBySeverity(match: any, candidates: AppearanceFeatureInfo[]) {
  if (match.severityLevel) {
    const bySeverity = candidates.filter(f => f.severityLevel === match.severityLevel);
    if (bySeverity.length > 0) return bySeverity[0].id;
  }

  return candidates[0]?.id || '';
}

function open(options?: { autoOpen?: boolean }) {
  if (options && typeof options.autoOpen === 'boolean') {
    autoOpen.value = options.autoOpen;
  }
  visible.value = true;
  autoConfirmPending();
}

function close() {
  visible.value = false;
}

onMounted(() => {
  if (autoOpen.value) {
    open({ autoOpen: true });
  }
});

defineExpose({
  open,
  close,
});
</script>

<style scoped>
.select-feature-cell {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 4px 8px;
  cursor: pointer;
  border-radius: 4px;
  transition: all 0.2s;

  &:hover {
    background-color: #f0f0f0;
  }

  .feature-name {
    flex: 1;
    color: #333;
  }

  .placeholder {
    flex: 1;
    color: #999;
  }

  .click-hint {
    font-size: 12px;
    color: #1890ff;
    margin-left: 8px;
  }
}
</style>
