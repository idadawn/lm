<template>
  <div class="h-full flex p-4 gap-4">
    <!-- 左侧：判定列列表 -->
    <div class="w-[300px] flex-none bg-white rounded-lg shadow-sm flex flex-col">
      <div class="p-4 border-b">
        <h2 class="text-lg font-bold">判定项目</h2>
      </div>
      <div class="flex-1 overflow-auto p-2">
        <a-spin :spinning="loadingFormula">
          <div v-if="formulaList.length === 0" class="text-center text-gray-400 py-10">
            暂无判定项目
          </div>
          <div
            v-else
            v-for="item in formulaList"
            :key="item.id"
            class="p-3 mb-2 rounded cursor-pointer transition-colors border hover:border-blue-400 hover:bg-blue-50"
            :class="selectedFormulaId === item.id ? 'bg-blue-50 border-blue-500' : 'bg-white border-gray-200'"
            @click="handleSelectFormula(item)"
          >
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
      <div class="flex-1 overflow-auto p-4">
        <div v-if="!selectedFormulaId" class="h-full flex items-center justify-center text-gray-400">
          请先在左侧选择一个判定项目
        </div>
        <a-spin v-else :spinning="loadingLevel">
          <a-table
            :columns="columns"
            :data-source="levelList"
            :pagination="false"
            row-key="id"
          >
            <template #bodyCell="{ column, record }">
              <template v-if="column.key === 'color'">
                <div class="flex items-center gap-2" v-if="record.color">
                  <div class="w-4 h-4 rounded border" :style="{ backgroundColor: record.color }"></div>
                  <span>{{ record.color }}</span>
                </div>
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
                <div v-if="record.isDefault" class="text-gray-400 cursor-not-allowed" title="系统默认生成，不可编辑删除">
                  系统默认
                </div>
                <a-space v-else>
                  <a-button type="link" size="small" @click="handleEditCondition(record)">条件</a-button>
                  <a-divider type="vertical" />
                  <a-button type="link" size="small" @click="handleEdit(record)">编辑</a-button>
                  <a-popconfirm title="确认删除？" @confirm="handleDelete(record.id)">
                    <a-button type="link" size="small" danger>删除</a-button>
                  </a-popconfirm>
                </a-space>
              </template>
            </template>
          </a-table>
        </a-spin>
      </div>
    </div>

    <!-- 表单模态框 -->
    <LevelForm @register="registerModal" @success="handleSuccess" />
    <LevelConditionModal @register="registerConditionModal" @success="handleSuccess" />
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
  import LevelForm from './components/form.vue';
  import LevelConditionModal from './components/LevelConditionModal.vue';
  import { useSortable } from '/@/hooks/web/useSortable';
  import { nextTick } from 'vue';

  const { createMessage } = useMessage();
  const [registerModal, { openModal }] = useModal();
  const [registerConditionModal, { openModal: openConditionModal }] = useModal();

  const loadingFormula = ref(false);
  const formulaList = ref<any[]>([]);
  const selectedFormulaId = ref<string>('');
  const selectedFormula = ref<any>(null);

  const loadingLevel = ref(false);
  const levelList = ref<any[]>([]);
  // ... (keep existing refs)

  // ... (keep existing methods)

  const handleEditCondition = (record: any) => {
    openConditionModal(true, {
      levelId: record.id,
      formulaId: selectedFormulaId.value,
      levelName: record.name,
    });
  };

  const handleEdit = (record: any) => {
    openModal(true, {
      isUpdate: true,
      record,
      formulaId: selectedFormulaId.value,
    });
  };

  const columns = [
    { title: '优先级', dataIndex: 'priority', width: 80 },
    { title: '等级代码', dataIndex: 'code', width: 120 },
    { title: '等级名称', dataIndex: 'name', width: 150 },
    { title: '质量状态', dataIndex: 'qualityStatus', width: 100 },
    { title: '颜色', key: 'color', width: 100 },
    { title: '统计', key: 'isStatistic', width: 80 },
    { title: '默认', key: 'isDefault', width: 80 },
    { title: '说明', dataIndex: 'description' },
    { title: '操作', key: 'action', width: 120, fixed: 'right' },
  ];

  // 初始化拖拽
  const initSortable = () => {
    nextTick(() => {
      const el = document.querySelectorAll('.ant-table-tbody')[0] as HTMLElement;
      if (!el) return;
      
      const { initSortable } = useSortable(el, {
        handle: '.ant-table-row',
        onEnd: async (evt) => {
          const { oldIndex, newIndex } = evt;
          if (oldIndex === newIndex) return;
          
          const currRow = levelList.value.splice(oldIndex!, 1)[0];
          levelList.value.splice(newIndex!, 0, currRow);

          // 获取新的ID排序
          const ids = levelList.value.map(item => item.id);
          try {
            await updateIntermediateDataJudgmentLevelSort(ids);
            createMessage.success('排序更新成功');
            // 重新加载以获取最新状态（顺便刷新优先级显示）
            loadLevels();
          } catch (error) {
            console.error(error);
            createMessage.error('排序更新失败');
          }
        },
      });
      initSortable();
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
      const res: any = await getIntermediateDataJudgmentLevelList({ formulaId: selectedFormulaId.value });
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
    loadFormulas();
  });
</script>

