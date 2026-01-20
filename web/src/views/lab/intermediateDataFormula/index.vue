<template>
  <div class="p-4 h-full flex flex-col gap-4">
    <div class="bg-white p-4 rounded-lg shadow-sm flex justify-between items-center">
      <div>
        <h1 class="text-lg font-bold flex items-center gap-2">
          <span class="text-blue-600">
            <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none"
              stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
              <path d="M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z"></path>
              <polyline points="14 2 14 8 20 8"></polyline>
              <line x1="16" y1="13" x2="8" y2="13"></line>
              <line x1="16" y1="17" x2="8" y2="17"></line>
              <polyline points="10 9 9 9 8 9"></polyline>
            </svg>
          </span>
          公式维护
        </h1>
        <p class="text-gray-500 text-sm mt-1">维护中间数据表中的计算公式，支持从多个数据源引用变量。</p>
      </div>
      <!-- 移除手动新增，改为初始化 -->
      <a-button type="primary" @click="handleInitialize" :loading="initializing" class="bg-blue-600 border-blue-600 hover:bg-blue-500">
        初始化公式
      </a-button>
    </div>

    <div class="flex-1 overflow-auto bg-gray-50 p-4 rounded-lg">
      <a-spin :spinning="loading">
        <a-table
          :columns="columns"
          :data-source="dataList"
          :pagination="false"
          row-key="id"
          :scroll="{ x: 1300 }"
        >
          <template #bodyCell="{ column, record }">
            <template v-if="column.key === 'formulaType'">
               <a-tag :color="record.formulaType === 'CALC' ? 'blue' : 'orange'">
                {{ record.formulaType === 'CALC' ? '计算' : '判定' }}
              </a-tag>
            </template>
            <template v-else-if="column.key === 'formula'">
              <code class="text-xs bg-gray-100 px-2 py-1 rounded" v-if="record.formula">{{ record.formula }}</code>
              <span v-else class="text-gray-300 text-xs italic">未配置</span>
            </template>
            <template v-else-if="column.key === 'isEnabled'">
              <a-tag :color="record.isEnabled ? 'green' : 'red'">
                {{ record.isEnabled ? '启用' : '禁用' }}
              </a-tag>
            </template>
            <template v-else-if="column.key === 'action'">
              <a-space>
                <a-button type="link" size="small" @click="handleEdit(record, 'Attributes')">编辑</a-button>
                <a-button type="link" size="small" @click="handleOpenFormulaBuilder(record)">编辑公式</a-button>
                <!-- 删除功能保留，防止脏数据，但主要操作应为禁用 -->
                <a-button type="link" size="small" danger @click="handleDelete(record)">删除</a-button>
              </a-space>
            </template>
          </template>
        </a-table>
        <div v-if="dataList.length === 0 && !loading" class="flex flex-col items-center justify-center h-64 text-gray-400">
          <svg xmlns="http://www.w3.org/2000/svg" width="64" height="64" viewBox="0 0 24 24" fill="none"
            stroke="currentColor" stroke-width="1" stroke-linecap="round" stroke-linejoin="round" class="mb-4">
            <path d="M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z"></path>
            <polyline points="14 2 14 8 20 8"></polyline>
          </svg>
          <p class="text-lg mb-2">暂无公式数据</p>
          <a-button type="primary" @click="handleInitialize">初始化公式数据</a-button>
        </div>
      </a-spin>
    </div>

    <!-- 表单模态框 -->
    <IntermediateDataFormulaForm @register="registerFormModal" @reload="loadData" />
    
    <!-- 公式构建器模态框 -->
    <FormulaBuilder @register="registerFormulaBuilderModal" @save="handleSaveFormula" />
  </div>
</template>

<script lang="ts" setup>
import { ref, onMounted } from 'vue';
import { useModal } from '/@/components/Modal';
import { useMessage } from '/@/hooks/web/useMessage';
import {
  getIntermediateDataFormulaList,
  deleteIntermediateDataFormula,
  initializeIntermediateDataFormula,
} from '/@/api/lab/intermediateDataFormula';
import IntermediateDataFormulaForm from './components/IntermediateDataFormulaForm.vue';
import FormulaBuilder from './components/FormulaBuilder.vue';
import type { IntermediateDataFormula } from '/@/api/lab/types/intermediateDataFormula';
import { updateIntermediateDataFormula, updateFormula } from '/@/api/lab/intermediateDataFormula';

const [registerFormModal, { openModal: openFormModal }] = useModal();
const [registerFormulaBuilderModal, { openModal: openFormulaBuilderModal }] = useModal();
const { createMessage, createConfirm } = useMessage();

const dataList = ref<IntermediateDataFormula[]>([]);
const loading = ref(false);
const initializing = ref(false);

const columns = [
  {
    title: '排序',
    dataIndex: 'sortOrder',
    key: 'sortOrder',
    width: 80,
    sorter: (a: any, b: any) => a.sortOrder - b.sortOrder,
  },
  {
    title: '公式名称',
    dataIndex: 'formulaName',
    key: 'formulaName',
    width: 150,
  },
  {
    title: '列名',
    dataIndex: 'columnName',
    key: 'columnName',
    width: 150,
  },
  {
    title: '类型',
    dataIndex: 'formulaType',
    key: 'formulaType',
    width: 80,
  },
  {
    title: '公式',
    key: 'formula',
    width: 300,
  },
  {
    title: '单位',
    dataIndex: 'unitName',
    key: 'unitName',
    width: 80,
  },
  {
    title: '精度',
    dataIndex: 'precision',
    key: 'precision',
    width: 70,
  },
  {
    title: '状态',
    key: 'isEnabled',
    width: 80,
  },
  {
    title: '操作',
    key: 'action',
    width: 160,
    fixed: 'right',
  },
];

const loadData = async () => {
  loading.value = true;
  try {
    const res: any = await getIntermediateDataFormulaList();
    dataList.value = res.data || res || [];
  } catch (error) {
    console.error('加载公式列表失败:', error);
    createMessage.error('加载公式列表失败');
    dataList.value = [];
  } finally {
    loading.value = false;
  }
};

const handleInitialize = async () => {
  initializing.value = true;
  try {
    await initializeIntermediateDataFormula();
    createMessage.success('初始化成功');
    await loadData();
  } catch (error: any) {
    console.error('初始化失败:', error);
    createMessage.error(error.message || '初始化失败');
  } finally {
    initializing.value = false;
  }
};

const handleEdit = (record: IntermediateDataFormula, mode: 'Attributes' | 'Formula') => {
  openFormModal(true, { isUpdate: true, record, mode });
};

const handleOpenFormulaBuilder = (record: IntermediateDataFormula) => {
  openFormulaBuilderModal(true, { record });
};

const handleSaveFormula = async (data: { id: string; formula: string }) => {
  try {
    await updateFormula(data.id, data.formula);
    createMessage.success('公式保存成功');
    loadData();
  } catch (error: any) {
    createMessage.error(error.message || '保存失败');
  }
};

const handleDelete = (record: IntermediateDataFormula) => {
  createConfirm({
    iconType: 'warning',
    title: '确认删除',
    content: `确定要删除公式 "${record.formulaName}" 吗？建议仅禁用而非删除。`,
    onOk: async () => {
      try {
        await deleteIntermediateDataFormula(record.id);
        createMessage.success('删除成功');
        loadData();
      } catch (error) {
        console.error('删除失败:', error);
        createMessage.error('删除失败');
      }
    },
  });
};

onMounted(() => {
  loadData();
});
</script>

<style scoped>
code {
  font-family: 'Courier New', monospace;
  word-break: break-all;
}
</style>
