<template>
  <div class="page-content-wrapper">
    <div class="page-content-wrapper-center">
      <div class="page-content-wrapper-content">
        <BasicTable @register="registerTable">
          <template #tableTitle>
            **A预警详细因子分析
            <!-- <a-button type="primary" preIcon="icon-ym icon-ym-btn-add" @click="addOrUpdateHandle()">新建规则</a-button> -->
          </template>
          <template #bodyCell="{ column, record }">
            <template v-if="column.key === 'projectPhase'">
              <a-tag color="error" v-if="record.remark == '1'">下一级</a-tag>
              <a-tag color="error" v-else>偏低</a-tag>
            </template>
          </template>
        </BasicTable>
      </div>
    </div>
  </div>
</template>
<script lang="ts" setup>
  import { onMounted, reactive } from 'vue';
  import { BasicTable, useTable, TableAction, BasicColumn, ActionItem } from '/@/components/Table';
  import { getTableList, delTable } from '/@/api/extend/table';
  import { useModal } from '/@/components/Modal';
  import { useI18n } from '/@/hooks/web/useI18n';
  import { useMessage } from '/@/hooks/web/useMessage';
  import { useBaseStore } from '/@/store/modules/base';
  import { useRouter } from 'vue-router';

  defineOptions({ name: 'extend-tableDemo-commonTable' });
  const router = useRouter();

  interface State {
    industryTypeList: any[];
  }

  const state = reactive<State>({
    industryTypeList: [],
  });
  const { t } = useI18n();
  const baseStore = useBaseStore();
  const { createMessage } = useMessage();
  const columns: BasicColumn[] = [
    { title: '**因子', dataIndex: 'projectName', width: 80 },
    { title: '**含**率', dataIndex: 'projectCode', width: 80 },
    { title: '影响因素', dataIndex: 'principal', width: 80 },
    { title: '原因', dataIndex: 'remark', width: 80 },
  ];
  const [registerForm, { openModal: openFormModal }] = useModal();
  const [registerTable, { reload }] = useTable({
    api: getTableList,
    columns,
    useSearchForm: false,
  });
  function getTableActions(record): ActionItem[] {
    return [];
  }
  async function init() {
    state.industryTypeList = (await baseStore.getDictionaryData('IndustryType')) as any[];

    state.industryTypeList = [];
  }

  onMounted(() => init());
</script>
