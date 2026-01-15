<template>
  <div class="page-content-wrapper documentPreview-wrapper">
    <div class="page-content-wrapper-center">
      <div class="page-content-wrapper-search-box">
        <BasicForm @register="registerForm" @submit="handleSubmit" @reset="handleReset" />
      </div>
      <div class="page-content-wrapper-content bg-white">
        <a-tabs v-model:activeKey="activeKey" type="card" class="page-content-wrapper-tabs" destroyInactiveTabPane>
          <a-tab-pane key="localPreview" tab="本地预览"></a-tab-pane>
          <a-tab-pane key="yozoOnlinePreview" tab="在线预览"></a-tab-pane>
        </a-tabs>
        <div class="p-10px">
          <a-alert message="本地预览支持doc/docx/xls/xlsx/ppt/pptx/pdf等办公文档。" type="warning" show-icon v-if="activeKey === 'localPreview'" />
          <a-alert message="免责声明：永中文档预览组件不属于JNPF产品，只用于介绍第三方组件如何在《KPI管理系统》中使用。" type="warning" show-icon v-else />
        </div>
        <BasicTable @register="registerTable" :searchInfo="getSearchInfo">
          <template #bodyCell="{ column, record }">
            <template v-if="column.key === 'fileName'">
              <p class="link-text" @click="handleView(record.fileId, record.fileName)">{{ record.fileName }}</p>
            </template>
          </template>
        </BasicTable>
      </div>
    </div>
    <Preview ref="filePreviewRef" />
  </div>
</template>
<script lang="ts" setup>
  import { reactive, ref, toRefs, computed, nextTick } from 'vue';
  import { getDocumentPreviewList } from '/@/api/extend/documentPreview';
  import { BasicForm, useForm } from '/@/components/Form';
  import { useI18n } from '/@/hooks/web/useI18n';
  import { BasicTable, useTable } from '/@/components/Table';
  import Preview from './Preview.vue';

  defineOptions({ name: 'extend-documentPreview' });

  interface State {
    activeKey: string;
    keyword: string;
  }

  const { t } = useI18n();
  const filePreviewRef = ref<any>(null);
  const state = reactive<State>({
    activeKey: 'localPreview',
    keyword: '',
  });
  const { activeKey } = toRefs(state);

  const getSearchInfo = computed(() => ({ keyword: state.keyword }));

  const [registerForm] = useForm({
    baseColProps: { span: 6 },
    showActionButtonGroup: true,
    showAdvancedButton: true,
    compact: true,
    labelAlign: 'left',
    labelWidth: 60,
    schemas: [
      {
        field: 'keyword',
        label: t('common.keyword'),
        component: 'Input',
        componentProps: {
          placeholder: t('common.enterKeyword'),
          submitOnPressEnter: true,
        },
      },
    ],
  });

  const [registerTable, { reload }] = useTable({
    api: getDocumentPreviewList,
    columns: [
      { title: '文件名称', dataIndex: 'fileName' },
      { title: '文件类型', dataIndex: 'fileType', width: 150 },
      { title: '文件大小', dataIndex: 'fileSize', width: 150 },
    ],
    pagination: false,
    showTableSetting: false,
  });

  function handleSubmit(values) {
    state.keyword = values?.keyword || '';
    handleSearch();
  }
  function handleReset() {
    state.keyword = '';
    handleSearch();
  }
  function handleSearch() {
    nextTick(() => {
      reload();
    });
  }
  function handleView(id, name) {
    const data = {
      id,
      name,
      type: state.activeKey,
    };
    filePreviewRef.value?.init(data);
  }
</script>
<style lang="less" scoped>
  .documentPreview-wrapper {
    &.page-content-wrapper .page-content-wrapper-center .page-content-wrapper-content {
      .page-content-wrapper-tabs.ant-tabs-card {
        height: auto;
      }
    }
  }
</style>
