<template>
  <BasicModal v-bind="$attrs" @register="registerModal" title="公共属性管理" :width="900" :showOkBtn="false"
    @cancel="handleCancel">
    <div class="public-attribute-list">
      <!-- 工具栏 -->
      <div class="toolbar mb-4 flex justify-between items-center">
        <a-button type="primary" :icon="h(PlusOutlined)" @click="handleAdd">
          新增公共属性
        </a-button>
      </div>

      <!-- 表格 -->
      <a-table v-if="!loading || dataList.length > 0" :columns="columns" :data-source="dataList" :loading="loading"
        :pagination="false" row-key="id" size="middle">
        <template #bodyCell="{ column, record }">
          <template v-if="column.key === 'valueType'">
            <a-tag :color="getValueTypeColor(record.valueType)">
              {{ getValueTypeLabel(record.valueType) }}
            </a-tag>
          </template>
          <template v-else-if="column.key === 'precision'">
            <span v-if="record.valueType === 'decimal'">{{ record.precision ?? '-' }}</span>
            <span v-else>-</span>
          </template>
          <template v-else-if="column.key === 'action'">
            <a-space>
              <a-button type="link" size="small" @click="handleEdit(record)">编辑</a-button>
              <a-button type="link" size="small" danger @click="handleDelete(record)">删除</a-button>
            </a-space>
          </template>
        </template>
      </a-table>

      <!-- 空状态 -->
      <a-empty v-if="!loading && dataList.length === 0" description="暂无公共属性" :image="false" />
    </div>

    <!-- 表单模态框 -->
    <PublicAttributeForm @register="registerFormModal" @reload="loadData" />
  </BasicModal>
</template>

<script lang="ts" setup>
import { ref, h } from 'vue';
import { BasicModal, useModalInner } from '/@/components/Modal';
import { useMessage } from '/@/hooks/web/useMessage';
import { PlusOutlined } from '@ant-design/icons-vue';
import { getPublicAttributes, deletePublicAttribute } from '/@/api/lab/productSpecPublicAttribute';
import { useModal } from '/@/components/Modal';
import PublicAttributeForm from './PublicAttributeForm.vue';

const emit = defineEmits(['register', 'reload']);

const [registerFormModal, { openModal: openFormModal }] = useModal();
const { createMessage, createConfirm } = useMessage();

const dataList = ref<any[]>([]);
const loading = ref(false);

// 初始化函数
function init(_data?: any) {
  loadData();
}

const [registerModal, { closeModal }] = useModalInner(init);

// 表格列定义
const columns = [
  {
    title: '属性名称',
    dataIndex: 'attributeName',
    key: 'attributeName',
    width: 150,
  },
  {
    title: '属性键名',
    dataIndex: 'attributeKey',
    key: 'attributeKey',
    width: 150,
  },
  {
    title: '属性类型',
    key: 'valueType',
    width: 100,
  },
  {
    title: '精度',
    key: 'precision',
    width: 80,
  },
  {
    title: '单位',
    dataIndex: 'unit',
    key: 'unit',
    width: 100,
  },
  {
    title: '默认值',
    dataIndex: 'defaultValue',
    key: 'defaultValue',
    width: 120,
  },
  {
    title: '排序码',
    dataIndex: 'sortCode',
    key: 'sortCode',
    width: 100,
  },
  {
    title: '操作',
    key: 'action',
    width: 120,
    fixed: 'right',
  },
];

// 获取属性类型标签
function getValueTypeLabel(valueType: string): string {
  const map: Record<string, string> = {
    decimal: '小数',
    int: '整数',
    text: '文本',
  };
  return map[valueType] || valueType;
}

// 获取属性类型颜色
function getValueTypeColor(valueType: string): string {
  const map: Record<string, string> = {
    decimal: 'blue',
    int: 'green',
    text: 'orange',
  };
  return map[valueType] || 'default';
}

// 加载数据
async function loadData() {
  loading.value = true;
  try {
    const res = await getPublicAttributes();
    const list = res.data || res || [];
    dataList.value = list;
  } catch (error: any) {
    console.error('加载公共属性列表失败', error);
    createMessage.error('加载公共属性列表失败');
  } finally {
    loading.value = false;
  }
}

// 新增
function handleAdd() {
  openFormModal(true, { id: '' });
}

// 编辑
function handleEdit(record: any) {
  openFormModal(true, { id: record.id });
}

// 删除
function handleDelete(record: any) {
  createConfirm({
    iconType: 'warning',
    title: '确认删除',
    content: `确定要删除公共属性"${record.attributeName}"吗？`,
    onOk: async () => {
      try {
        await deletePublicAttribute(record.id);
        createMessage.success('删除成功');
        loadData();
        emit('reload');
      } catch (error: any) {
        const errorMsg = error?.response?.data?.msg || error?.message || '删除失败';
        createMessage.error(errorMsg);
      }
    },
  });
}

// 取消按钮
function handleCancel() {
  closeModal();
}
</script>

<style lang="less" scoped>
.public-attribute-list {
  .toolbar {
    padding: 0;
  }
}
</style>
