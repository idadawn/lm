<template>
  <div class="page-content-wrapper">
    <div class="page-content-wrapper-center">
      <div class="page-content-wrapper-content">
        <a-tabs v-model:activeKey="activeTab" type="card">
          <!-- 磁性数据页签 -->
          <a-tab-pane key="data" tab="磁性数据">
            <div class="table-container">
              <BasicTable @register="registerTable">
                <template #tableTitle>
                  <a-upload :before-upload="handleQuickImport" :show-upload-list="false" accept=".xlsx,.xls">
                    <a-button type="primary">
                      <UploadOutlined /> 快捷导入
                    </a-button>
                  </a-upload>
                </template>
                <template #action="{ record }">

                  <TableAction :actions="[
                    {
                      icon: 'ant-design:delete-outlined',
                      color: 'error',
                      popConfirm: {
                        title: '是否确认删除',
                        confirm: handleDelete.bind(null, record),
                      },
                    },
                  ]" />
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
            </div>
          </a-tab-pane>
        </a-tabs>
      </div>
      <MagneticDataImportQuickModal @register="registerQuickModal" @reload="handleImportSuccess" />
    </div>
  </div>

</template>


<script lang="ts" setup>
import { ref, onMounted, nextTick } from 'vue';
import { getMagneticRawDataList, deleteMagneticRawData } from '/@/api/lab/magneticData';
import { BasicTable, useTable, TableAction, BasicColumn } from '/@/components/Table';
import { useMessage } from '/@/hooks/web/useMessage';
import { useI18n } from '/@/hooks/web/useI18n';
import { formatToDateTime } from '/@/utils/dateUtil';

import { UploadOutlined } from '@ant-design/icons-vue';
import { useModal } from '/@/components/Modal';
import MagneticDataImportQuickModal from './MagneticDataImportQuickModal.vue';
import { createMagneticImportSession, uploadAndParseMagneticData } from '/@/api/lab/magneticData';


defineOptions({ name: 'MagneticData' });

const { createMessage } = useMessage();

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
  // 处理返回数据，转换成分页格式
  afterFetch: (data) => {
    // 后端返回格式: { list: [], pagination: { currentPage, pageSize, total } }
    if (data && data.list) {
      return {
        items: data.list,
        total: data.pagination?.total || data.list.length,
      };
    }
    return data;
  },
});

// 快捷导入模态窗
const [registerQuickModal, { openModal: openQuickModal }] = useModal();

function handleImportSuccess() {
  reload();
  createMessage.success('导入完成，数据已刷新');
}

// 快捷导入处理
async function handleQuickImport(file: File) {
  const isExcel = file.type === 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet' ||
    file.type === 'application/vnd.ms-excel' ||
    file.name.endsWith('.xlsx') ||
    file.name.endsWith('.xls');

  if (!isExcel) {
    createMessage.error('只能上传Excel文件！');
    return false;
  }

  try {
    createMessage.loading({ content: '正在快速解析文件...', key: 'quickImport', duration: 0 });

    // 1. 读取文件并转换为 Base64
    const reader = new FileReader();
    const fileData = await new Promise<string>((resolve, reject) => {
      reader.onload = () => {
        const res = reader.result as string;
        resolve(res.split(',')[1]);
      };
      reader.onerror = reject;
      reader.readAsDataURL(file);
    });

    // 2. 创建会话并保存文件
    const sessionId = await createMagneticImportSession({
      fileName: file.name,
      fileData: fileData,
    });

    if (!sessionId) throw new Error('创建会话失败');

    // 3. 执行解析
    await uploadAndParseMagneticData(sessionId, {
      fileName: file.name,
      fileData: fileData,
    });

    createMessage.success({ content: '解析成功，请最后核对数据', key: 'quickImport' });

    // 4. 打开模态窗并直接显现第二步
    openQuickModal(true, {
      importSessionId: sessionId,
    });
  } catch (error: any) {

    console.error('快捷导入失败:', error);
    createMessage.error({ content: error.message || '快捷导入失败', key: 'quickImport' });
  }

  return false; // 阻止自动上传
}

onMounted(() => {
  nextTick(() => {
    reload();
  });
});
</script>

<style scoped>
/* 表格容器样式 */
.table-container {
  width: 100%;
}

/* 针对工业大表格的样式微调 */
.table-container :deep(.ant-table-thead > tr > th) {
  padding: 4px 8px;
  font-size: 12px;
  background-color: #fafafa;
  text-align: center !important;
}

/* 表格单元格紧凑布局 */
.table-container :deep(.ant-table-tbody > tr > td) {
  padding: 4px 8px;
  font-size: 12px;
}

/* 负数红色显示 */
.text-danger {
  color: #f5222d;
}
</style>
