<template>
  <div class="page-content-wrapper">
    <div class="page-content-wrapper-center">
      <div class="page-content-wrapper-content">
        <BasicTable @register="registerTable">
          <template #tableTitle>
            <BasicButton type="primary" preIcon="icon-ym icon-ym-btn-add" @click="addOrUpdateHandle(undefined)">新建规则
            </BasicButton>
          </template>
          <template #bodyCell="{ column, record }">
            <template v-if="column.key === 'action'">
              <TableAction :actions="getTableActions(record)" />
            </template>
            <template v-if="column.key === 'projectPhase'">
              <a-tag color="error" v-if="record.projectPhase == '1'">是</a-tag>
              <a-tag color="error" v-else>否</a-tag>
            </template>
          </template>
        </BasicTable>
      </div>
    </div>
    <Form @register="registerForm" @reload="reload" />
  </div>
</template>
<script lang="ts" setup>
import { onMounted, reactive } from 'vue';
import { BasicTable, useTable, TableAction, BasicColumn, ActionItem } from '/@/components/Table';
import { Button as BasicButton } from '/@/components/Button';
import { getTableList, delTable } from '/@/api/extend/table';
import { useModal } from '/@/components/Modal';
import { useI18n } from '/@/hooks/web/useI18n';
import { useMessage } from '/@/hooks/web/useMessage';
import { useBaseStore } from '/@/store/modules/base';
import Form from '/@/views/extend/tableDemo/commonForm/index.vue';
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
  { title: '指标名称', dataIndex: 'projectName', width: 80 },
  { title: '指标编码', dataIndex: 'projectCode', width: 80 },
  { title: '是否预警', dataIndex: 'projectPhase', width: 80 },
  { title: '预警方向', dataIndex: 'customerName', width: 80 },
  { title: '预警阈值', dataIndex: 'principal', width: 80 },
  { title: '预警备注', dataIndex: 'remark', width: 80 },
];
const [registerForm] = useModal();
const [registerTable, { reload }] = useTable({
  api: getTableList,
  columns,
  useSearchForm: true,
  formConfig: {
    schemas: [
      {
        field: 'keyword',
        label: t('common.indexName'),
        component: 'Input',
        componentProps: {
          placeholder: t('common.enterIndexName'),
          submitOnPressEnter: true,
        },
      },
      {
        field: 'keyword',
        label: t('common.departmentName'),
        component: 'Input',
        componentProps: {
          placeholder: t('common.enterDepartment'),
          submitOnPressEnter: true,
        },
      },
    ],
  },
  actionColumn: {
    width: 100,
    title: '操作',
    dataIndex: 'action',
  },
});
function getTableActions(record): ActionItem[] {
  return [
    {
      label: record.projectPhase == '1' ? t('common.closeStateText') : t('common.openStateText'),
      onClick: changeWarningState.bind(null, record.id, record),
    },
    {
      label: t('common.editText'),
      onClick: addOrUpdateHandle.bind(null, record),
    },
    {
      label: t('common.delText'),
      color: 'error',
      modelConfirm: {
        onOk: handleDelete.bind(null, record.id),
      },
    },
  ];
}
function addOrUpdateHandle(record) {
  // openFormModal(true, { id, industryTypeList: state.industryTypeList });
  if (record) {
    router.push('/warning/maintenance?config=' + record.projectCode);
  } else {
    //新建规则，对于指标第一次进行预警维护
    router.push('/warning/maintenance?config=');
  }
}
//开启关闭预警状态
function changeWarningState(_id, record) {
  // 接口请求开启关闭
  let resMsg = '';
  if (record.projectPhase == '1') {
    resMsg = record.projectName + '关闭成功';
  } else {
    resMsg = record.projectName + '开启成功';
  }

  createMessage.success(resMsg);
  reload();
}
function handleDelete(id) {
  delTable(id).then(res => {
    createMessage.success(res.msg);
    reload();
  });
}
async function init() {
  state.industryTypeList = (await baseStore.getDictionaryData('IndustryType')) as any[];

  state.industryTypeList = [];
}

onMounted(() => init());
</script>
