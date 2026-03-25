<template>
  <div>
    <BasicTable @register="registerTable">
      <template #toolbar>
        <a-button type="primary" @click="handleCreate">新增配置</a-button>
      </template>
      <template #bodyCell="{ column, record }">
        <template v-if="column.key === 'action' || column.dataIndex === 'action'">
          <TableAction
            :actions="[
              {
                icon: 'clarity:note-edit-line',
                label: '编辑',
                tooltip: '编辑',
                onClick: handleEdit.bind(null, record),
              },
              {
                icon: 'ant-design:delete-outlined',
                label: '删除',
                color: 'error',
                popConfirm: {
                  title: '是否确认删除',
                  placement: 'left',
                  confirm: handleDelete.bind(null, record),
                },
                ifShow: !record.isSystem,
              },
            ]"
          />
        </template>
      </template>
    </BasicTable>
    <ReportConfigDrawer @register="registerDrawer" @success="handleSuccess" />
  </div>
</template>

<script lang="ts" setup>
defineOptions({ name: 'ReportConfigList' });

import { onMounted, ref } from 'vue';
import { BasicTable, useTable, TableAction } from '/@/components/Table';
import { deleteReportConfig, getReportConfigList } from '/@/api/lab/reportConfig';
import { getIntermediateDataFormulaList } from '/@/api/lab/intermediateDataFormula';
import { useDrawer } from '/@/components/Drawer';
import { useMessage } from '/@/hooks/web/useMessage';
import ReportConfigDrawer from './components/ReportConfigDrawer.vue';

const { createMessage } = useMessage();
const [registerDrawer, { openDrawer }] = useDrawer();
const formulaNameMap = ref<Record<string, string>>({});

function formatStatisticMode(record: Recordable) {
  if (record.isPercentage) {
    return '占比';
  }

  return record.isShowRatio ? '重量汇总 + 占比' : '重量汇总';
}

async function loadFormulaNameMap() {
  try {
    const res: any = await getIntermediateDataFormulaList('JUDGE');
    const formulaList = Array.isArray(res) ? res : res?.data || [];
    const nextMap: Record<string, string> = {};

    formulaList.forEach((item: any) => {
      const label = item?.displayName || item?.formulaName || item?.columnName;
      if (item?.id && label) {
        nextMap[item.id] = label;
      }
      if (item?.columnName && label && !nextMap[item.columnName]) {
        nextMap[item.columnName] = label;
      }
    });

    formulaNameMap.value = nextMap;
  } catch (error) {
    formulaNameMap.value = {};
  }
}

const [registerTable, { reload }] = useTable({
  title: '指标配置列表',
  api: getReportConfigList,
  columns: [
    { title: '名称', dataIndex: 'name', align: 'center', width: 180 },
    {
      title: '判定列',
      dataIndex: 'formulaId',
      align: 'center',
      width: 180,
      customRender: ({ text }) => formulaNameMap.value[text] || text || '-',
    },
    {
      title: '包含等级',
      dataIndex: 'levelNames',
      customRender: ({ text }) => (Array.isArray(text) ? text.join('、') : text),
    },
    {
      title: '统计方式',
      dataIndex: 'statMode',
      align: 'center',
      width: 140,
      customRender: ({ record }) => formatStatisticMode(record),
    },
    {
      title: '头部展示',
      dataIndex: 'isHeader',
      align: 'center',
      width: 90,
      customRender: ({ text }) => (text ? '是' : '否'),
    },
    {
      title: '报表展示',
      dataIndex: 'isShowInReport',
      align: 'center',
      width: 90,
      customRender: ({ text }) => (text ? '是' : '否'),
    },
    { title: '排序', dataIndex: 'sortOrder', align: 'center', width: 80 },
    { title: '说明', dataIndex: 'description' },
  ],
  actionColumn: {
    width: 120,
    title: '操作',
    dataIndex: 'action',
  },
  pagination: false,
  useSearchForm: false,
  showTableSetting: true,
  bordered: true,
  showIndexColumn: false,
});

function handleCreate() {
  openDrawer(true, {
    isUpdate: false,
  });
}

function handleEdit(record: Recordable) {
  openDrawer(true, {
    record,
    isUpdate: true,
  });
}

async function handleDelete(record: Recordable) {
  await deleteReportConfig(record.id);
  createMessage.success('删除成功');
  reload();
}

function handleSuccess() {
  reload();
}

onMounted(() => {
  loadFormulaNameMap();
});
</script>
