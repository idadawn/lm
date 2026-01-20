<template>
  <div class="page-content-wrapper">
    <div class="page-content-wrapper-center">
      <div class="page-content-wrapper-content">
        <a-tabs v-model:activeKey="activeTab" type="card">
          <!-- 磁性数据页签 -->
          <a-tab-pane key="data" tab="磁性数据">
            <BasicTable @register="registerTable">
              <template #toolbar>
                <a-button type="primary" @click="handleImport">
                  <SolutionOutlined /> 分步导入
                </a-button>
              </template>
              <template #action="{ record }">
                <TableAction
                  :actions="[
                    {
                      icon: 'ant-design:delete-outlined',
                      color: 'error',
                      popConfirm: {
                        title: '是否确认删除',
                        confirm: handleDelete.bind(null, record),
                      },
                    },
                  ]"
                />
              </template>
              <template #bodyCell="{ column, record }">
                <template v-if="column.key === 'isScratched'">
                  <a-tag :color="record.isScratched === 1 ? 'warning' : 'default'">
                    {{ record.isScratched === 1 ? '带K' : '无' }}
                  </a-tag>
                </template>
                <template v-else-if="column.key === 'isValid'">
                  <a-tag :color="record.isValid === 1 ? 'success' : 'error'">
                    {{ record.isValid === 1 ? '有效' : '无效' }}
                  </a-tag>
                </template>
                <template v-else-if="column.key === 'detectionTime'">
                  {{ record.detectionTime ? formatToDateTime(record.detectionTime) : '-' }}
                </template>
                <template v-else-if="column.key === 'creatorTime'">
                  {{ record.creatorTime ? formatToDateTime(record.creatorTime) : '-' }}
                </template>
              </template>
            </BasicTable>
          </a-tab-pane>
        </a-tabs>
      </div>
      <MagneticDataImportWizard @register="registerImportWizard" @reload="handleImportSuccess" />
    </div>
  </div>
</template>

<script lang="ts" setup>
  import { ref, onMounted, nextTick } from 'vue';
  import { getMagneticRawDataList, deleteMagneticRawData } from '/@/api/lab/magneticData';
  import { BasicTable, useTable, TableAction, BasicColumn } from '/@/components/Table';
  import { useMessage } from '/@/hooks/web/useMessage';
  import { useI18n } from '/@/hooks/web/useI18n';
  import { usePopup } from '/@/components/Popup';
  import { formatToDateTime } from '/@/utils/dateUtil';
  import MagneticDataImportWizard from './MagneticDataImportWizard.vue';
  import { SolutionOutlined } from '@ant-design/icons-vue';

  defineOptions({ name: 'MagneticData' });

  const { createMessage, createConfirm } = useMessage();
  const { t } = useI18n();

  // 页签状态
  const activeTab = ref('data');

  // 处理删除
  async function handleDelete(record: Recordable) {
    await deleteMagneticRawData(record.id);
    createMessage.success('删除成功');
    reload();
  }

  // 表格列配置
  const columns: BasicColumn[] = [
    { title: '检测时间', dataIndex: 'detectionTime', width: 160, fixed: 'left', sorter: true },
    { title: '原始炉号', dataIndex: 'originalFurnaceNo', width: 150, fixed: 'left' },
    { title: '解析炉号', dataIndex: 'furnaceNo', width: 100 },
    { title: 'Ps铁损', dataIndex: 'psLoss', width: 100, align: 'right' },
    { title: 'Ss激磁功率', dataIndex: 'ssPower', width: 100, align: 'right' },
    { title: 'Hc', dataIndex: 'hc', width: 100, align: 'right' },
    { title: '刻痕(K)', dataIndex: 'isScratched', key: 'isScratched', width: 80, align: 'center' },
    { title: '数据状态', dataIndex: 'isValid', key: 'isValid', width: 100, align: 'center' },
    { title: '错误信息', dataIndex: 'errorMessage', width: 200 },
    { title: '录入日期', dataIndex: 'creatorTime', key: 'creatorTime', width: 160 },
  ];

  const [registerTable, { reload }] = useTable({
    api: getMagneticRawDataList,
    columns,
    useSearchForm: true,
    showTableSetting: true,
    bordered: true,
    actionColumn: {
      width: 80,
      title: '操作',
      dataIndex: 'action',
      slots: { customRender: 'action' },
    },
    formConfig: {
      baseColProps: { span: 6 },
      labelWidth: 100,
      schemas: [
        {
          field: 'keyword',
          label: t('common.keyword'),
          component: 'Input',
          componentProps: {
            placeholder: '炉号/原始炉号',
            submitOnPressEnter: true,
          },
        },
        {
          field: 'dateRange',
          label: '检测日期',
          component: 'DateRange',
          componentProps: {
            placeholder: ['开始日期', '结束日期'],
          },
        },
        {
          field: 'isValidData',
          label: '数据状态',
          component: 'Select',
          defaultValue: 1, // 默认显示有效数据
          componentProps: {
            options: [
              { label: '全部', value: -1 },
              { label: '有效数据', value: 1 },
              { label: '无效数据', value: 0 },
            ],
            fieldNames: { label: 'label', value: 'value' },
          },
        },
      ],
      fieldMapToTime: [['dateRange', ['startDate', 'endDate'], 'YYYY-MM-DD']],
    },
    // 处理参数
    beforeFetch: (params) => {
      // 如果选择全部(-1)，则不传isValidData或传null
      if (params.isValidData === -1) {
        delete params.isValidData;
      }
      return params;
    },
  });

  // 导入向导
  const [registerImportWizard, { openPopup: openImportWizard }] = usePopup();

  function handleImport() {
    openImportWizard(true);
  }

  function handleImportSuccess() {
    reload();
    createMessage.success('导入完成，数据已刷新');
  }

  // 初始化
  onMounted(() => {
    nextTick(() => {
      reload();
    });
  });
</script>

<style scoped>
</style>
