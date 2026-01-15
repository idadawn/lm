<template>
  <div class="version-manage-container">
    <a-row :gutter="20">
      <!-- 左侧：版本列表 -->
      <a-col :span="10">
        <a-card title="版本列表" :bordered="false" class="box-card">
          <template #extra>
            <a-button type="link" @click="loadVersionList" title="刷新">
              <ReloadOutlined />
            </a-button>
          </template>
          <a-table
            :dataSource="versionList"
            :columns="versionColumns"
            :pagination="false"
            :scroll="{ y: 500 }"
            size="small"
            row-key="version"
            :row-selection="{ type: 'radio', selectedRowKeys: selectedRowKeys, onChange: onSelectChange }"
            @row-click="handleRowClick"
          >
            <template #bodyCell="{ column, record }">
              <template v-if="column.dataIndex === 'versionName'">
                <a-space>
                  <a-tag v-if="record.isCurrent === 1 || record.isCurrent === true" color="green">当前</a-tag>
                  <span>{{ record.versionName || `v${record.version}.0` }}</span>
                </a-space>
              </template>
              <template v-else-if="column.dataIndex === 'createTime'">
                {{ formatDate(record.creatorTime || record.createTime) }}
              </template>
              <template v-else-if="column.dataIndex === 'versionDescription'">
                <span :title="record.versionDescription">{{ record.versionDescription || '-' }}</span>
              </template>
            </template>
          </a-table>
        </a-card>
      </a-col>

      <!-- 右侧：版本详情 -->
      <a-col :span="14">
        <a-card title="版本详情" :bordered="false" class="box-card">
          <template #title>
            版本详情
            <span v-if="selectedVersion" style="font-weight: normal; margin-left: 8px">
              - {{ selectedVersion.versionName }}
            </span>
          </template>
          <a-empty v-if="!selectedVersion" description="请选择左侧版本查看详情" />
          <div v-else>
            <a-descriptions :column="2" bordered size="small" style="margin-bottom: 16px">
              <a-descriptions-item label="版本名称">
                <a-tag v-if="selectedVersion.isCurrent === 1 || selectedVersion.isCurrent === true" color="green">当前</a-tag>
                <span style="margin-left: 8px">{{ selectedVersion.versionName || `v${selectedVersion.version}.0` }}</span>
              </a-descriptions-item>
              <a-descriptions-item label="版本号">
                v{{ selectedVersion.version }}.0
              </a-descriptions-item>
              <a-descriptions-item label="创建时间" :span="2">
                {{ formatDate(selectedVersion.creatorTime || selectedVersion.createTime) }}
              </a-descriptions-item>
              <a-descriptions-item label="变更说明" :span="2">
                {{ selectedVersion.versionDescription || '-' }}
              </a-descriptions-item>
            </a-descriptions>
            <a-divider orientation="left">属性列表</a-divider>
            <a-table
              :dataSource="selectedAttributes"
              :columns="attributeColumns"
              :pagination="false"
              :scroll="{ y: 400 }"
              size="small"
              row-key="id"
              bordered
            >
              <template #bodyCell="{ column, record }">
                <template v-if="column.dataIndex === 'attributeValue'">
                  {{ record.attributeValue || '-' }}
                </template>
                <template v-else-if="column.dataIndex === 'unit'">
                  {{ record.unit || '-' }}
                </template>
              </template>
              <template #emptyText>
                <a-empty description="该版本暂无属性数据" :image="false" />
              </template>
            </a-table>
          </div>
        </a-card>
      </a-col>
    </a-row>
  </div>
</template>

<script setup lang="ts">
import { ref, watch } from 'vue';
import { ReloadOutlined } from '@ant-design/icons-vue';
import { getProductSpecVersionList, getProductSpecVersionAttributes } from '/@/api/lab/product';
import type { TableColumnType } from 'ant-design-vue';

const props = defineProps({
  productSpecId: {
    type: String,
    required: true,
  },
});

const versionList = ref<any[]>([]);
const selectedVersion = ref<any>(null);
const selectedAttributes = ref<any[]>([]);
const selectedRowKeys = ref<number[]>([]);

// 版本列表表格列定义
const versionColumns: TableColumnType[] = [
  {
    title: '版本名称',
    dataIndex: 'versionName',
    key: 'versionName',
    width: 150,
  },
  {
    title: '创建时间',
    dataIndex: 'createTime',
    key: 'createTime',
    width: 180,
  },
  {
    title: '变更说明',
    dataIndex: 'versionDescription',
    key: 'versionDescription',
    ellipsis: {
      showTitle: true,
    },
  },
];

// 属性详情表格列定义
const attributeColumns: TableColumnType[] = [
  {
    title: '属性名称',
    dataIndex: 'attributeName',
    key: 'attributeName',
    width: 120,
  },
  {
    title: '键名',
    dataIndex: 'attributeKey',
    key: 'attributeKey',
    width: 100,
  },
  {
    title: '属性值',
    dataIndex: 'attributeValue',
    key: 'attributeValue',
  },
  {
    title: '单位',
    dataIndex: 'unit',
    key: 'unit',
    width: 80,
  },
];

// 加载版本列表
const loadVersionList = async () => {
  if (!props.productSpecId) return;
  try {
    const response = await getProductSpecVersionList(props.productSpecId);
    // 确保返回的是数组格式（可能被包装在 data 字段中）
    const res = Array.isArray(response) ? response : (response?.data || []);
    
    if (!Array.isArray(res)) {
      console.warn('版本列表格式不正确:', response);
      versionList.value = [];
      return;
    }
    
    versionList.value = res;

    // 默认选中当前版本
    if (res && res.length > 0) {
      const current = res.find((v: any) => v.isCurrent === 1 || v.isCurrent === true) || res[0];
      if (current) {
        selectedRowKeys.value = [current.version];
        handleVersionSelect(current);
      }
    }
  } catch (error) {
    console.error('获取版本列表失败', error);
  }
};

// 行选择变化
const onSelectChange = (selectedKeys: number[]) => {
  selectedRowKeys.value = selectedKeys;
  if (selectedKeys.length > 0) {
    const record = versionList.value.find(v => v.version === selectedKeys[0]);
    if (record) {
      handleVersionSelect(record);
    }
  }
};

// 行点击事件
const handleRowClick = (record: any) => {
  selectedRowKeys.value = [record.version];
  handleVersionSelect(record);
};

// 选中版本
const handleVersionSelect = async (record: any) => {
  if (!record) return;
  selectedVersion.value = record;
  try {
    const response = await getProductSpecVersionAttributes({
      productSpecId: props.productSpecId,
      version: record.version,
    });
    // 处理返回数据格式
    const res = Array.isArray(response) ? response : (response?.data || []);
    selectedAttributes.value = res || [];
  } catch (error) {
    console.error('获取版本详情失败', error);
    selectedAttributes.value = [];
  }
};

// 格式化日期
const formatDate = (dateStr: string | Date) => {
  if (!dateStr) return '-';
  try {
    const date = typeof dateStr === 'string' ? new Date(dateStr) : dateStr;
    if (isNaN(date.getTime())) return '-';
    return date.toLocaleString('zh-CN', {
      year: 'numeric',
      month: '2-digit',
      day: '2-digit',
      hour: '2-digit',
      minute: '2-digit',
      second: '2-digit',
    });
  } catch (error) {
    return '-';
  }
};

watch(
  () => props.productSpecId,
  (val) => {
    if (val) {
      loadVersionList();
      selectedVersion.value = null;
      selectedAttributes.value = [];
    }
  },
  { immediate: true }
);
</script>

<style scoped lang="scss">
.version-manage-container {
  padding: 16px;
  background: #f5f5f5;
  min-height: 100%;

  .box-card {
    height: 650px;
    margin-bottom: 0;
    box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
    border-radius: 8px;
    
    :deep(.ant-card-head) {
      background: #fafafa;
      border-bottom: 2px solid #e8e8e8;
      border-radius: 8px 8px 0 0;
      
      .ant-card-head-title {
        font-weight: 600;
        font-size: 16px;
      }
    }
    
    :deep(.ant-card-body) {
      padding: 16px;
    }
  }

  :deep(.ant-table) {
    .ant-table-container {
      border: 1px solid #e8e8e8;
      border-radius: 6px;
    }
    
    .ant-table-tbody > tr {
      cursor: pointer;
      transition: all 0.3s;
      
      &:hover {
        background: #f0f7ff;
      }
      
      &.ant-table-row-selected {
        background: #e6f7ff;
      }
    }
    
    .ant-table-thead > tr > th {
      background: #fafafa;
      font-weight: 600;
    }
  }

  :deep(.ant-descriptions) {
    .ant-descriptions-item-label {
      font-weight: 600;
      background: #fafafa;
    }
  }
  
  :deep(.ant-empty) {
    margin: 40px 0;
  }
}
</style>