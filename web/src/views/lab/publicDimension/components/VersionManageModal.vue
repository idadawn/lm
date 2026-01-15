<template>
  <BasicModal
    v-bind="$attrs"
    @register="registerModal"
    title="版本管理"
    :width="800"
    :showOkBtn="false"
    @cancel="handleCancel">
    <div class="version-manage">
      <div class="mb-4">
        <a-alert
          :message="`公共维度：${dimensionName}`"
          type="info"
          show-icon
          class="mb-4" />
      </div>

      <a-table
        :columns="columns"
        :data-source="versionList"
        :loading="loading"
        :pagination="false"
        row-key="id"
        size="middle">
        <template #bodyCell="{ column, record }">
          <template v-if="column.key === 'version'">
            <a-tag :color="record.isCurrent === 1 ? 'purple' : 'default'">
              {{ record.versionName || `v${record.version}.0` }}
            </a-tag>
            <a-tag v-if="record.isCurrent === 1" color="success" size="small" class="ml-2">
              当前版本
            </a-tag>
          </template>
          <template v-else-if="column.key === 'createTime'">
            {{ formatTime(record.creatorTime) }}
          </template>
        </template>
      </a-table>

      <a-empty v-if="!loading && versionList.length === 0" description="暂无版本记录" :image="false" />
    </div>
  </BasicModal>
</template>

<script lang="ts" setup>
  import { ref } from 'vue';
  import { BasicModal, useModalInner } from '/@/components/Modal';
  import { useMessage } from '/@/hooks/web/useMessage';
  import { getVersionList } from '/@/api/lab/publicDimension';
  import { formatToDateTime } from '/@/utils/dateUtil';

  const emit = defineEmits(['register']);

  const { createMessage } = useMessage();
  const dimensionId = ref('');
  const dimensionName = ref('');
  const versionList = ref<any[]>([]);
  const loading = ref(false);

  // 表格列定义
  const columns = [
    {
      title: '版本',
      key: 'version',
      width: 150,
    },
    {
      title: '版本说明',
      dataIndex: 'versionDescription',
      key: 'versionDescription',
    },
    {
      title: '创建时间',
      key: 'createTime',
      width: 180,
    },
    {
      title: '创建人',
      dataIndex: 'creatorUserId',
      key: 'creatorUserId',
      width: 120,
    },
  ];

  // 初始化函数
  async function init(data?: any) {
    dimensionId.value = data?.dimensionId || '';
    dimensionName.value = data?.dimensionName || '';
    if (dimensionId.value) {
      await loadVersionList();
    }
  }

  const [registerModal, { closeModal }] = useModalInner(init);

  // 加载版本列表
  async function loadVersionList() {
    if (!dimensionId.value) return;
    
    loading.value = true;
    try {
      const res = await getVersionList(dimensionId.value);
      const list = Array.isArray(res) ? res : (res?.data || []);
      versionList.value = list;
    } catch (error: any) {
      console.error('加载版本列表失败', error);
      createMessage.error('加载版本列表失败');
    } finally {
      loading.value = false;
    }
  }

  // 格式化时间
  function formatTime(time: string | Date | null | undefined): string {
    if (!time) return '-';
    return formatToDateTime(time);
  }

  // 取消按钮
  function handleCancel() {
    closeModal();
  }
</script>

<style lang="less" scoped>
.version-manage {
  .mb-4 {
    margin-bottom: 16px;
  }
  
  .ml-2 {
    margin-left: 8px;
  }
}
</style>
