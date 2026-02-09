<template>
  <div class="h-full flex p-4 gap-4">
    <!-- 左侧：产品规格 + 判定列列表 -->
    <div class="w-[300px] flex-none bg-white rounded-lg shadow-sm flex flex-col">
      <div class="p-4 border-b">
        <h2 class="text-lg font-bold mb-3">产品规格</h2>
        <a-select v-model:value="selectedProductSpecId" placeholder="请选择产品规格" allowClear show-search
          :filter-option="filterOption" style="width: 100%" @change="handleProductSpecChange">
          <a-select-option v-for="item in productSpecList" :key="item.id" :value="item.id">
            {{ item.name }}
          </a-select-option>
        </a-select>
      </div>
      <div class="p-4 border-b">
        <h2 class="text-lg font-bold">判定项目</h2>
      </div>
      <div class="flex-1 overflow-auto p-2">
        <a-spin :spinning="loadingFormula">
          <div v-if="formulaList.length === 0" class="text-center text-gray-400 py-10">
            暂无判定项目
          </div>
          <div v-else v-for="item in formulaList" :key="item.id"
            class="p-3 mb-2 rounded cursor-pointer transition-colors border hover:border-blue-400 hover:bg-blue-50"
            :class="selectedFormulaId === item.id ? 'bg-blue-50 border-blue-500' : 'bg-white border-gray-200'"
            @click="handleSelectFormula(item)">
            <div class="font-medium text-gray-800">{{ item.displayName || item.formulaName }}</div>
            <div class="text-xs text-gray-500 mt-1 flex justify-between">
              <span>{{ item.columnName }}</span>
              <a-tag color="orange" size="small">判定</a-tag>
            </div>
          </div>
        </a-spin>
      </div>
    </div>

    <!-- 右侧：等级列表 -->
    <div class="flex-1 w-0 bg-white rounded-lg shadow-sm flex flex-col">
      <div class="p-4 border-b flex justify-between items-center">
        <h2 class="text-lg font-bold">
          等级配置
          <span v-if="selectedFormula" class="text-sm font-normal text-gray-500 ml-2">
            - {{ selectedFormula.displayName || selectedFormula.formulaName }}
          </span>
        </h2>
        <a-button type="primary" :disabled="!selectedFormulaId" @click="handleAdd">
          新增等级
        </a-button>
      </div>
      <div class="flex-1 overflow-hidden p-4 flex flex-col">
        <div v-if="!selectedFormulaId" class="h-full flex items-center justify-center text-gray-400">
          请先在左侧选择一个判定项目
        </div>
        <a-spin v-else :spinning="loadingLevel">
          <div class="table-container">
            <a-table :columns="columns" :data-source="levelList" :pagination="false" row-key="id"
              :scroll="{ x: tableScrollX, y: 'calc(100vh - 300px)' }">
              <template #bodyCell="{ column, record }">
                <template v-if="column.key === 'conditionCount'">
                  {{ getConditionCount(record.condition) }}
                </template>
                <template v-else-if="column.key === 'color'">
                  <div class="flex items-center gap-2" v-if="record.color">
                    <div class="w-4 h-4 rounded border" :style="{ backgroundColor: record.color }"></div>
                    <span>{{ record.color }}</span>
                  </div>
                </template>
                <template v-else-if="column.key === 'qualityStatus'">
                  <a-tag :color="getQualityStatusColor(record.qualityStatus)">
                    {{ getQualityStatusText(record.qualityStatus) }}
                  </a-tag>
                </template>
                <template v-else-if="column.key === 'conditionText'">
                  <a-tooltip color="#fff" placement="topLeft" v-if="record.condition">
                    <template #title>
                      <ConditionCell :condition="record.condition" />
                    </template>
                    <div class="truncate w-full block">
                      <ConditionCell :condition="record.condition" />
                    </div>
                  </a-tooltip>
                </template>
                <template v-else-if="column.key === 'isStatistic'">
                  <a-tag :color="record.isStatistic ? 'green' : 'gray'">
                    {{ record.isStatistic ? '是' : '否' }}
                  </a-tag>
                </template>
                <template v-else-if="column.key === 'isDefault'">
                  <a-tag :color="record.isDefault ? 'blue' : 'gray'">
                    {{ record.isDefault ? '是' : '否' }}
                  </a-tag>
                </template>
                <template v-else-if="column.key === 'action'">
                  <!-- 已移除系统默认兜底等级，所有等级可编辑删除 -->
                  <a-space>
                    <a-button type="link" size="small" @click="handleEditCondition(record)">条件</a-button>
                    <a-divider type="vertical" />
                    <a-button type="link" size="small" @click="handleCopy(record)">拷贝</a-button>
                    <a-divider type="vertical" />
                    <a-button type="link" size="small" @click="handleEdit(record)">编辑</a-button>
                    <a-popconfirm title="确认删除？" @confirm="handleDelete(record.id)">
                      <a-button type="link" size="small" danger>删除</a-button>
                    </a-popconfirm>
                  </a-space>
                </template>
              </template>
            </a-table>
          </div>
        </a-spin>
      </div>
    </div>

    <!-- 表单模态框 -->
    <LevelForm @register="registerModal" @success="handleSuccess" />
    <LevelConditionModal @register="registerConditionModal" @success="handleSuccess" />
    <CopyConditionModal @register="registerCopyModal" @success="handleSuccess" @clone="handleCloneToNew" />
  </div>
</template>

<script lang="ts" setup>
import { ref, onMounted } from 'vue';
import { useMessage } from '/@/hooks/web/useMessage';
import { useModal } from '/@/components/Modal';
import { getIntermediateDataFormulaList } from '/@/api/lab/intermediateDataFormula';
import {
  getIntermediateDataJudgmentLevelList,
  deleteIntermediateDataJudgmentLevel,
  updateIntermediateDataJudgmentLevelSort,
} from '/@/api/lab/intermediateDataJudgmentLevel';
import { getProductSpecList } from '/@/api/lab/productSpec';
import LevelForm from './components/form.vue';
import LevelConditionModal from './components/LevelConditionModal.vue';
import CopyConditionModal from './components/CopyConditionModal.vue';
import ConditionCell from './components/ConditionCell.vue';
import { useSortable } from '/@/hooks/web/useSortable';
import { nextTick } from 'vue';

const { createMessage } = useMessage();
const [registerModal, { openModal }] = useModal();
const [registerConditionModal, { openModal: openConditionModal }] = useModal();
const [registerCopyModal, { openModal: openCopyModal }] = useModal();

const loadingFormula = ref(false);
const formulaList = ref<any[]>([]);
const selectedFormulaId = ref<string>('');
const selectedFormula = ref<any>(null);

const loadingLevel = ref(false);
const levelList = ref<any[]>([]);

// 产品规格相关
const productSpecList = ref<any[]>([]);
const selectedProductSpecId = ref<string | undefined>(undefined);

const filterOption = (input: string, option: any) => {
  return option.children?.[0]?.children?.toLowerCase()?.includes(input.toLowerCase());
};

const loadProductSpecList = async () => {
  try {
    const res = await getProductSpecList({});
    productSpecList.value = res.list || [];
  } catch (error) {
    console.error('加载产品规格失败', error);
  }
};

const handleProductSpecChange = () => {
  loadLevels();
};
// ... (keep existing refs)

// ... (keep existing methods)

const handleEditCondition = (record: any) => {
  openConditionModal(true, {
    levelId: record.id,
    formulaId: selectedFormulaId.value,
    levelName: record.name,
  });
};

const handleCopy = (record: any) => {
  openCopyModal(true, {
    sourceLevel: record,
    formulaId: selectedFormulaId.value,
    allLevels: levelList.value,
  });
};

const handleCloneToNew = (record: any) => {
  openModal(true, {
    isUpdate: false, // 标记为新建
    record: { ...record, name: `复制-${record.name}` }, // 预填充数据，修改名称以示区别
    formulaId: selectedFormulaId.value,
  });
};

const handleEdit = (record: any) => {
  openModal(true, {
    isUpdate: true,
    record,
    formulaId: selectedFormulaId.value,
  });
};

const getConditionCount = (conditionJson: string) => {
  if (!conditionJson) return 0;
  try {
    const data = JSON.parse(conditionJson);
    if (!data || !Array.isArray(data.groups)) return 0;

    let count = 0;
    for (const group of data.groups) {
      // 统计当前组的条件
      if (Array.isArray(group.conditions)) {
        count += group.conditions.length;
      }
      // 统计子组的条件
      if (Array.isArray(group.subGroups)) {
        for (const subGroup of group.subGroups) {
          if (Array.isArray(subGroup.conditions)) {
            count += subGroup.conditions.length;
          }
        }
      }
    }
    return count;
  } catch (e) {
    return 0;
  }
};

const getQualityStatusText = (status: number) => {
  switch (status) {
    case 0: return '合格';
    case 1: return '不合格';
    case 2: return '其他';
    default: return '未知';
  }
};

const getQualityStatusColor = (status: number) => {
  switch (status) {
    case 0: return 'success';
    case 1: return 'error';
    case 2: return 'default';
    default: return 'default';
  }
};

const columns = [
  { title: '产品规格', dataIndex: 'productSpecName', width: 120 },
  { title: '等级名称', dataIndex: 'name', width: 120 },
  { title: '优先级', dataIndex: 'priority', width: 80 },
  { title: '条件个数', key: 'conditionCount', width: 100 },
  { title: '默认', key: 'isDefault', width: 80 },
  { title: '判定条件', key: 'conditionText', width: 200, ellipsis: true },
  // { title: '说明', dataIndex: 'description' },
  { title: '操作', key: 'action', width: 300, fixed: 'right' },
];

// 计算固定宽度列的总宽度（不包括弹性列）
const ca = columns.reduce((total, column) => {
  const width = Number(column.width) || 0;
  return total + width;
}, 0);

// scroll.x 设置为大于列宽总和的值，确保水平滚动条正常显示
// 非固定列宽度之和不超过 scroll.x
const tableScrollX = ca + 200; // 额外留出弹性空间

// 初始化拖拽
let sortableInstance: any = null;

const initSortable = () => {
  nextTick(() => {
    const el = document.querySelectorAll('.ant-table-tbody')[0] as HTMLElement;
    if (!el) return;

    // 如果已有实例，先销毁
    if (sortableInstance) {
      sortableInstance.destroy();
      sortableInstance = null;
    }

    const { initSortable: createSortable } = useSortable(el, {
      handle: '.ant-table-row',
      onEnd: async (evt) => {
        const { oldIndex, newIndex } = evt;
        if (oldIndex === undefined || newIndex === undefined || oldIndex === newIndex) return;

        // 直接从 DOM 中读取拖拽后所有行的 ID 顺序
        const rows = el.querySelectorAll('tr.ant-table-row');
        const ids: string[] = [];
        rows.forEach((row) => {
          const id = row.getAttribute('data-row-key');
          if (id) {
            ids.push(id);
          }
        });

        console.log('拖拽排序 - oldIndex:', oldIndex, 'newIndex:', newIndex);
        console.log('拖拽排序 - 从DOM读取的ids顺序:', ids);

        if (ids.length === 0) {
          createMessage.error('获取排序数据失败');
          return;
        }

        try {
          await updateIntermediateDataJudgmentLevelSort(ids);
          createMessage.success('排序更新成功');
          // 重新加载以获取最新状态
          await loadLevels();
        } catch (error) {
          console.error(error);
          createMessage.error('排序更新失败');
          await loadLevels();
        }
      },
    });
    sortableInstance = createSortable();
  });
};

import { useRoute } from 'vue-router';

const route = useRoute();

// 加载判定项目列表
const loadFormulas = async () => {
  loadingFormula.value = true;
  try {
    const res: any = await getIntermediateDataFormulaList();
    const list = res.data || res || [];
    // 过滤出 JUDGE 类型
    formulaList.value = list.filter((item: any) => item.formulaType === 'JUDGE');

    // 优先使用路由参数选中
    const queryId = route.query.formulaId as string;
    if (queryId) {
      const target = formulaList.value.find(item => item.id === queryId);
      if (target) {
        handleSelectFormula(target);
        return;
      }
    }

    // 默认选中第一条
    if (formulaList.value.length > 0) {
      handleSelectFormula(formulaList.value[0]);
    }
  } catch (error) {
    console.error(error);
    createMessage.error('加载判定项目失败');
  } finally {
    loadingFormula.value = false;
  }
};

// 选择判定项目
const handleSelectFormula = (item: any) => {
  selectedFormulaId.value = item.id;
  selectedFormula.value = item;
  loadLevels();
};

// 加载等级列表
const loadLevels = async () => {
  if (!selectedFormulaId.value) return;
  loadingLevel.value = true;
  try {
    const params: any = { formulaId: selectedFormulaId.value };
    if (selectedProductSpecId.value) {
      params.productSpecId = selectedProductSpecId.value;
    }
    const res: any = await getIntermediateDataJudgmentLevelList(params);
    levelList.value = res.data || res || [];
    // 数据加载完成后初始化拖拽
    initSortable();
  } catch (error) {
    console.error(error);
    createMessage.error('加载等级列表失败');
  } finally {
    loadingLevel.value = false;
  }
};

const handleAdd = () => {
  openModal(true, {
    isUpdate: false,
    formulaId: selectedFormulaId.value,
    productSpecId: selectedProductSpecId.value,
  });
};

const handleDelete = async (id: string) => {
  try {
    await deleteIntermediateDataJudgmentLevel(id);
    createMessage.success('删除成功');
    loadLevels();
  } catch (error) {
    console.error(error);
    createMessage.error('删除失败');
  }
};

const handleSuccess = () => {
  loadLevels();
};

onMounted(() => {
  loadProductSpecList();
  loadFormulas();
});
</script>

<style lang="less" scoped>
.table-container {
  width: 100%;
  height: calc(100vh - 220px);
  overflow: auto;

  :deep(.ant-table-wrapper) {
    height: 100%;

    .ant-spin-nested-loading,
    .ant-spin-container {
      height: 100%;
    }

    .ant-table {
      height: 100%;
      overflow: visible !important;
    }

    .ant-table-container {
      height: 100%;
      display: flex;
      flex-direction: column;
      overflow: visible !important;
    }

    .ant-table-header {
      flex-shrink: 0;
      overflow: visible !important;
    }

    // 关键：当使用固定列时，需要设置 content 层的水平滚动
    .ant-table-content {
      overflow-x: auto !important;
      overflow-y: hidden !important;
    }

    .ant-table-body {
      flex: 1;
      overflow-x: auto !important;
      overflow-y: auto !important;
    }

    // 固定列的阴影效果
    .ant-table-cell-fix-left-last::after,
    .ant-table-cell-fix-right-first::after {
      box-shadow: inset 10px 0 8px -8px rgba(0, 0, 0, 0.1);
    }
  }
}
</style>
