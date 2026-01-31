<template>
  <div class="p-4 h-full flex flex-col gap-4">
    <div class="bg-white p-4 rounded-lg shadow-sm">
      <div class="flex justify-between items-center mb-4">
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
        <div class="flex gap-2">
            <a-button type="primary" @click="handleCreate" class="bg-blue-600 border-blue-600 hover:bg-blue-500">
                <plus-outlined /> 新增公式
            </a-button>
            <a-button @click="handleInitialize" :loading="initializing">
                更新列
            </a-button>
        </div>
      </div>
      <!-- 类型筛选 -->
      <!-- 筛选栏 -->
      <div class="flex items-center gap-4">
        <div class="flex items-center gap-2">
          <span class="text-gray-600 text-sm">来源筛选：</span>
          <a-select
            v-model:value="sourceTypeFilter"
            style="width: 150px"
            placeholder="请选择来源"
            allow-clear
          >
            <a-select-option value="SYSTEM">系统默认</a-select-option>
            <a-select-option value="CUSTOM">自定义</a-select-option>
          </a-select>
        </div>

        <div class="flex items-center gap-2">
          <span class="text-gray-600 text-sm">类型筛选：</span>
          <a-select
            v-model:value="typeFilter"
            style="width: 150px"
            placeholder="请选择类型"
            allow-clear
          >
            <a-select-option value="CALC">计算</a-select-option>
            <a-select-option value="JUDGE">判定</a-select-option>
            <a-select-option value="NO">只展示</a-select-option>
          </a-select>
        </div>

        <div class="flex items-center gap-2">
            <span class="text-gray-600 text-sm">关键字：</span>
            <a-input-search
                v-model:value="searchKeyword"
                placeholder="名称/列名"
                style="width: 200px"
                allow-clear
            />
        </div>
      </div>
    </div>

    <div class="flex-1 overflow-auto bg-gray-50 p-4 rounded-lg">
      <a-spin :spinning="loading">
        <a-table
          :columns="columns"
          :data-source="filteredDataList"
          :pagination="false"
          row-key="id"
          :scroll="{ x: 1300 }"
        >
          <template #bodyCell="{ column, record }">
            <template v-if="column.key === 'displayName'">
              <div class="flex flex-col">
                  <span>{{ record.displayName || record.formulaName }}</span>
                  <span class="text-xs text-gray-400" v-if="record.columnName !== (record.displayName || record.formulaName)">{{ record.columnName }}</span>
              </div>
            </template>
            <template v-else-if="column.key === 'formulaType'">
               <a-tag :color="record.formulaType === 'CALC' ? 'blue' : record.formulaType === 'JUDGE' ? 'orange' : 'green'">
                {{ record.formulaType === 'CALC' ? '计算' : record.formulaType === 'JUDGE' ? '判定' : '只展示' }}
              </a-tag>
            </template>
            <template v-else-if="column.key === 'sourceType'">
               <a-tag :color="record.sourceType === 'CUSTOM' ? 'purple' : 'default'">
                {{ record.sourceType === 'CUSTOM' ? '自定义' : '系统' }}
              </a-tag>
            </template>
            <template v-else-if="column.key === 'formula'">
              <template v-if="record.formulaType === 'JUDGE'">
                <a-tag color="orange">判定规则（查看详情请编辑）</a-tag>
              </template>
              <template v-else-if="record.formulaType === 'NO'">
                <span class="text-gray-400 text-xs italic">无需公式</span>
              </template>
              <template v-else>
                <code class="text-xs bg-gray-100 px-2 py-1 rounded" v-if="record.formula">{{ record.formula }}</code>
                <span v-else class="text-gray-300 text-xs italic">未配置</span>
              </template>
            </template>
            <template v-else-if="column.key === 'isEnabled'">
              <a-tag :color="record.isEnabled ? 'green' : 'red'">
                {{ record.isEnabled ? '启用' : '禁用' }}
              </a-tag>
            </template>
            <template v-else-if="column.key === 'action'">
              <a-space>
                <a-button type="link" size="small" @click="handleEdit(record, 'Attributes')">编辑</a-button>
                <a-button v-if="record.formulaType !== 'NO'" type="link" size="small" @click="handleOpenFormulaBuilder(record)">
                  {{ record.formulaType === 'JUDGE' ? '查看判定' : '编辑公式' }}
                </a-button>
                <a-popconfirm
                    v-if="record.sourceType === 'CUSTOM'"
                    title="确定要删除此自定义公式吗？"
                    @confirm="handleDelete(record)"
                >
                    <a-button type="link" danger size="small">删除</a-button>
                </a-popconfirm>
              </a-space>
            </template>
          </template>
        </a-table>
        <div v-if="filteredDataList.length === 0 && !loading" class="flex flex-col items-center justify-center h-64 text-gray-400">
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

    <!-- 判定等级查看弹窗 -->
    <JudgmentLevelViewModal @register="registerJudgmentViewModal" />
  </div>
</template>

<script lang="ts" setup>
import { ref, onMounted, computed } from 'vue';
import { useModal } from '/@/components/Modal';
import { useMessage } from '/@/hooks/web/useMessage';
import {
  getIntermediateDataFormulaList,
  initializeIntermediateDataFormula,
  deleteIntermediateDataFormula,
} from '/@/api/lab/intermediateDataFormula';
import { PlusOutlined } from '@ant-design/icons-vue';
import IntermediateDataFormulaForm from './components/IntermediateDataFormulaForm.vue';
import FormulaBuilder from './components/FormulaBuilder.vue';
import JudgmentLevelViewModal from './components/JudgmentLevelViewModal.vue';
import type { IntermediateDataFormula } from '/@/api/lab/types/intermediateDataFormula';
import { updateFormula } from '/@/api/lab/intermediateDataFormula';

const [registerFormModal, { openModal: openFormModal }] = useModal();
const [registerFormulaBuilderModal, { openModal: openFormulaBuilderModal }] = useModal();
const [registerJudgmentViewModal, { openModal: openJudgmentViewModal }] = useModal();
const { createMessage } = useMessage();
const dataList = ref<IntermediateDataFormula[]>([]);
const loading = ref(false);
const initializing = ref(false);
const typeFilter = ref<string | undefined>(undefined);
const sourceTypeFilter = ref<string | undefined>(undefined);
const searchKeyword = ref<string>('');

// 默认不显示类型为"只展示"的记录
const filteredDataList = computed(() => {
  let filtered = dataList.value;
  
  // 如果筛选器为空，默认过滤掉 NO 类型
  if (!typeFilter.value) {
    filtered = filtered.filter(item => item.formulaType !== 'NO');
  } else {
    // 如果选择了特定类型，只显示该类型
    filtered = filtered.filter(item => item.formulaType === typeFilter.value);
  }

  // 来源筛选
  if (sourceTypeFilter.value) {
    filtered = filtered.filter(item => item.sourceType === sourceTypeFilter.value);
  }

  // 关键字筛选
  if (searchKeyword.value) {
    const keyword = searchKeyword.value.toLowerCase();
    filtered = filtered.filter(item => 
        (item.formulaName && item.formulaName.toLowerCase().includes(keyword)) ||
        (item.displayName && item.displayName.toLowerCase().includes(keyword)) ||
        (item.columnName && item.columnName.toLowerCase().includes(keyword))
    );
  }
  
  return filtered;
});

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
    key: 'displayName',
    width: 150,
  },
  {
    title: '类型',
    dataIndex: 'formulaType',
    key: 'formulaType',
    width: 80,
  },
  {
    title: '来源',
    dataIndex: 'sourceType',
    key: 'sourceType',
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

const handleCreate = () => {
    openFormModal(true, { isUpdate: false, mode: 'Attributes' });
};

const handleDelete = async (record: IntermediateDataFormula) => {
    try {
        await deleteIntermediateDataFormula(record.id);
        createMessage.success('删除成功');
        loadData();
    } catch (error: any) {
        createMessage.error(error.message || '删除失败');
    }
};

const handleOpenFormulaBuilder = async (record: IntermediateDataFormula) => {
  if (record.formulaType === 'JUDGE') {
    openJudgmentViewModal(true, { formulaId: record.id });
  } else {
    openFormulaBuilderModal(true, { record });
  }
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
