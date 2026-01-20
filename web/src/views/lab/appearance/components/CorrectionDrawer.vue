<template>
  <BasicDrawer
    v-bind="$attrs"
    @register="registerDrawer"
    title="人工修正匹配列表"
    width="800px"
    destroyOnClose
    :showFooter="false"
  >
    <BasicTable @register="registerTable">
      <template #bodyCell="{ column, record }">
        <template v-if="column.key === 'action'">
          <TableAction
            :actions="[
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
            ]"
          />
        </template>
        <template v-else-if="column.key === 'correctedFeatureName'">
          <div v-if="record.status === 'Pending'" class="select-feature-cell" @click="handleOpenFeatureDialog(record)">
            <span v-if="record.correctedFeatureName" class="feature-name">{{ record.correctedFeatureName }}</span>
            <span v-else class="placeholder">选择特性</span>
            <span class="click-hint">点击选择</span>
          </div>
          <span v-else>{{ record.correctedFeatureName }}</span>
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
  </BasicDrawer>
</template>
<script lang="ts" setup>
  import { ref, onMounted } from 'vue';
  import { BasicDrawer, useDrawerInner } from '/@/components/Drawer';
  import { BasicTable, useTable, TableAction, BasicColumn } from '/@/components/Table';
  import { useMessage } from '/@/hooks/web/useMessage';
  import { getCorrectionList, deleteCorrection, confirmCorrection, updateCorrection, AppearanceFeatureCorrection } from '/@/api/lab/featureLearning';
  import FeatureSelectDialog from './FeatureSelectDialog.vue';
  import type { AppearanceFeatureInfo } from '/@/api/lab/appearance';

  const { createMessage } = useMessage();
  const featureSelectDialogRef = ref<any>(null);
  const currentRecord = ref<AppearanceFeatureCorrection | null>(null);

  const columns: BasicColumn[] = [
    { title: '输入文本', dataIndex: 'inputText', width: 200 },
    { title: '自动匹配特性', dataIndex: 'autoMatchedFeatureName', width: 150 },
    { title: '人工修正特性', dataIndex: 'correctedFeatureName', key: 'correctedFeatureName', width: 200 },
    { title: '匹配模式', dataIndex: 'matchModeText', width: 100 },
    { title: '处理状态', dataIndex: 'status', key: 'status', width: 100 },
    { title: '备注', dataIndex: 'remark', width: 200 },
    { title: '创建时间', dataIndex: 'creatorTime', width: 150 },
  ];

  const [registerDrawer, { setDrawerProps, closeDrawer }] = useDrawerInner(async (data) => {
    reload();
  });

  const [registerTable, { reload }] = useTable({
    api: getCorrectionList,
    columns,
    bordered: true,
    showTableSetting: true,
    actionColumn: {
      width: 120,
      title: '操作',
      dataIndex: 'action',
    },
  });

  // 打开特性选择对话框
  function handleOpenFeatureDialog(record: AppearanceFeatureCorrection) {
    currentRecord.value = record;
    featureSelectDialogRef.value?.open();
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
</script>

<style lang="less" scoped>
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
