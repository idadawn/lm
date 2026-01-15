<template>
  <BasicPopup v-bind="$attrs" @register="registerPopup" :title="title" class="full-popup">
    <div class="page-content-wrapper-search-box ml-10px mt-10px">
      <BasicForm @register="registerSearchForm" @submit="handleSubmit" @reset="handleReset" />
    </div>
    <a-tabs v-model:activeKey="listQuery.category" type="card" class="jnpf-content-table-tabs" destroyInactiveTabPane>
      <a-tab-pane key="Web" tab="Web菜单">
        <BasicTable @register="registerWebTable" :columns="menuTableColumns" :searchInfo="getSearchInfo">
          <template #tableTitle>
            <a-button type="primary" preIcon="icon-ym icon-ym-btn-add" @click="addOrUpdateHandle()">新建</a-button>
            <a-button type="link" pre-icon="icon-ym icon-ym-btn-upload" @click="uploadTpl"> {{ t('common.importText') }} </a-button>
          </template>
          <template #bodyCell="{ column, record }">
            <template v-if="column.key === 'icon'">
              <i :class="record.icon + ' table-icon'" />
            </template>
            <template v-if="column.key === 'enabledMark'">
              <a-tag :color="record.enabledMark == 1 ? 'success' : 'error'">{{ record.enabledMark == 1 ? '启用' : '禁用' }}</a-tag>
            </template>
            <template v-if="column.key === 'action'">
              <TableAction :actions="getTableActions(record)" :dropDownActions="getDropDownActions(record)" />
            </template>
          </template>
        </BasicTable>
      </a-tab-pane>
      <a-tab-pane key="App" tab="App菜单">
        <BasicTable @register="registerAppTable" :columns="menuTableColumns" :searchInfo="getSearchInfo">
          <template #tableTitle>
            <a-button type="primary" preIcon="icon-ym icon-ym-btn-add" @click="addOrUpdateHandle()">新建</a-button>
            <a-button type="link" pre-icon="icon-ym icon-ym-btn-upload" @click="uploadTpl"> {{ t('common.importText') }} </a-button>
          </template>
          <template #bodyCell="{ column, record }">
            <template v-if="column.key === 'icon'">
              <i :class="record.icon + ' table-icon'" />
            </template>
            <template v-if="column.key === 'enabledMark'">
              <a-tag :color="record.enabledMark == 1 ? 'success' : 'error'">{{ record.enabledMark == 1 ? '启用' : '禁用' }}</a-tag>
            </template>
            <template v-if="column.key === 'action'">
              <TableAction :actions="getTableActions(record)" :dropDownActions="getDropDownActions(record)" />
            </template>
          </template>
        </BasicTable>
      </a-tab-pane>
    </a-tabs>
    <Form @register="registerForm" @reload="reloadTable" />
    <ButtonAuthorizeListDrawer @register="registerButtonAuthorize" />
    <ColumnAuthorizeListDrawer @register="registerColumnAuthorize" />
    <FormAuthorizeListDrawer @register="registerFormAuthorize" />
    <DataAuthorizeListDrawer @register="registerDataAuthorize" />
    <BasicModal @register="registerModal" title="所属上级" showOkBtn @ok="handleUploadSubmit" destroyOnClose>
      <BasicForm @register="registerUploadForm" />
      <jnpf-upload-btn
        v-show="false"
        :url="'/api/system/Menu/' + systemId + '/Action/Import'"
        :data="{ parentId, category: listQuery.category }"
        @on-success="onUploadSuccess"
        @on-error="onUploadError"
        @before-upload="onBeforeUpload"
        accept=".bm"
        ref="uploadRef">
      </jnpf-upload-btn>
    </BasicModal>
  </BasicPopup>
</template>
<script lang="ts" setup>
  import { ref, computed, reactive, toRefs, nextTick, watch, unref } from 'vue';
  import { BasicPopup, usePopupInner } from '/@/components/Popup';
  import { useMessage } from '/@/hooks/web/useMessage';
  import { BasicForm, useForm, FormSchema } from '/@/components/Form';
  import { useI18n } from '/@/hooks/web/useI18n';
  import { BasicTable, useTable, TableAction, BasicColumn, ActionItem } from '/@/components/Table';
  import { getMenuList, delMenu, exportMenu, getMenuSelector } from '/@/api/system/menu';
  import { useModal } from '/@/components/Modal';
  import { downloadByUrl } from '/@/utils/file/download';
  import { BasicModal } from '/@/components/Modal';
  import { useDrawer } from '/@/components/Drawer';
  import Form from './Form.vue';
  import ButtonAuthorizeListDrawer from '../buttonAuthorize/index.vue';
  import ColumnAuthorizeListDrawer from '../columnAuthorize/index.vue';
  import FormAuthorizeListDrawer from '../formAuthorize/index.vue';
  import DataAuthorizeListDrawer from '../dataAuthorize/index.vue';

  interface State {
    listQuery: any;
    systemId: string;
    title: string;
    parentId: string;
  }

  const { createMessage } = useMessage();
  const { t } = useI18n();
  const state = reactive<State>({
    listQuery: {
      category: 'Web',
      keyword: '',
    },
    systemId: '',
    title: '',
    parentId: '',
  });
  const { listQuery, title, systemId, parentId } = toRefs(state);
  const uploadRef = ref<any>(null);

  const getSearchInfo = computed(() => ({ keyword: state.listQuery.keyword, category: state.listQuery.category, systemId: state.systemId }));

  const menuTableColumns: BasicColumn[] = [
    { title: '菜单名称', dataIndex: 'fullName', width: 260 },
    { title: '菜单地址', dataIndex: 'urlAddress' },
    { title: '图标', dataIndex: 'icon', width: 50, align: 'center' },
    {
      title: '类型',
      dataIndex: 'type',
      width: 70,
      align: 'center',
      customRender: ({ record }) => {
        if (record.type === 1) return '目录';
        if (record.type === 2) return '页面';
        if (record.type === 3) return '功能';
        if (record.type === 4) return '字典';
        if (record.type === 5) return '报表';
        if (record.type === 6) return '大屏';
        if (record.type === 7) return '外链';
        if (record.type === 8) return '门户';
      },
    },
    { title: '排序', dataIndex: 'sortCode', width: 70, align: 'center' },
    { title: '状态', dataIndex: 'enabledMark', width: 70, align: 'center' },
  ];
  const schemas: FormSchema[] = [
    {
      field: 'parentId',
      label: '上级',
      component: 'TreeSelect',
      componentProps: { placeholder: '选择上级菜单' },
      rules: [{ required: true, message: '上级菜单不能为空', trigger: 'change' }],
    },
  ];
  const [registerPopup] = usePopupInner(init);
  const [registerSearchForm, { resetFields }] = useForm({
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
  const [registerForm, { openModal: openFormModal }] = useModal();
  const [registerWebTable, { reload: reloadWebTable }] = useTable({
    api: getMenuList,
    immediate: false,
    isTreeTable: true,
    pagination: false,
    actionColumn: {
      width: 150,
      title: '操作',
      dataIndex: 'action',
    },
  });
  const [registerAppTable, { reload: reloadAppTable }] = useTable({
    api: getMenuList,
    immediate: false,
    isTreeTable: true,
    pagination: false,
    actionColumn: {
      width: 150,
      title: '操作',
      dataIndex: 'action',
    },
  });
  const [registerModal, { openModal: openUploadFormModal, closeModal, setModalProps }] = useModal();
  const [registerUploadForm, { validate, resetFields: resetUploadFormFields, updateSchema }] = useForm({ labelWidth: 50, schemas: schemas });
  const [registerButtonAuthorize, { openDrawer: openButtonAuthorizeDrawer }] = useDrawer();
  const [registerColumnAuthorize, { openDrawer: openColumnAuthorizeDrawer }] = useDrawer();
  const [registerFormAuthorize, { openDrawer: openFormAuthorizeDrawer }] = useDrawer();
  const [registerDataAuthorize, { openDrawer: openDataAuthorizeDrawer }] = useDrawer();

  watch(
    () => state.listQuery.category,
    () => {
      nextTick(() => resetFields());
    },
    { deep: true },
  );

  function init(data) {
    state.systemId = data.id;
    state.title = data.title + '的菜单管理';
    state.listQuery.category = 'Web';
    nextTick(() => reloadWebTable({ page: 1 }));
  }
  function getTableActions(record): ActionItem[] {
    return [
      {
        label: t('common.editText'),
        onClick: addOrUpdateHandle.bind(null, record.id, ''),
      },
      {
        label: t('common.delText'),
        color: 'error',
        disabled: record.isMain == 1,
        modelConfirm: {
          onOk: handleDelete.bind(null, record.id),
        },
      },
    ];
  }
  function getDropDownActions(record): ActionItem[] {
    const list = [{ label: '新建子级', onClick: addOrUpdateHandle.bind(null, '', record.id) }];
    if (record?.type == 1) return state.listQuery.category === 'Web' ? list : [];
    return [
      {
        label: '按钮权限',
        ifShow: record.isButtonAuthorize === 1 && [2, 3, 4].indexOf(record.type) > -1,
        onClick: handleButtonAuthorize.bind(null, record),
      },
      {
        label: '列表权限',
        ifShow: record.isColumnAuthorize === 1 && [2, 3, 4].indexOf(record.type) > -1,
        onClick: handleColumnAuthorize.bind(null, record),
      },
      {
        label: '表单权限',
        ifShow: record.isFormAuthorize === 1 && [2, 3, 4].indexOf(record.type) > -1,
        onClick: handleFormAuthorize.bind(null, record),
      },
      {
        label: '数据权限',
        ifShow: record.isDataAuthorize === 1 && [2, 3, 4].indexOf(record.type) > -1,
        onClick: handleDataAuthorize.bind(null, record),
      },
      {
        label: '导出模板',
        onClick: handleExportMenu.bind(null, record.id),
      },
    ];
  }
  function handleSubmit(values) {
    state.listQuery.keyword = values?.keyword || '';
    handleSearch();
  }
  function handleReset() {
    state.listQuery.keyword = '';
    handleSearch();
  }
  function handleSearch() {
    nextTick(() => reloadTable());
  }
  function handleDelete(id) {
    delMenu(id).then(res => {
      createMessage.success(res.msg);
      reloadTable();
    });
  }
  function handleButtonAuthorize(record) {
    openButtonAuthorizeDrawer(true, { id: record.id, fullName: record.fullName });
  }
  function handleColumnAuthorize(record) {
    openColumnAuthorizeDrawer(true, { id: record.id, fullName: record.fullName, type: record.type });
  }
  function handleFormAuthorize(record) {
    openFormAuthorizeDrawer(true, { id: record.id, fullName: record.fullName, type: record.type });
  }
  function handleDataAuthorize(record) {
    openDataAuthorizeDrawer(true, { id: record.id, fullName: record.fullName, type: record.type });
  }
  function handleExportMenu(id) {
    exportMenu(id).then(res => {
      downloadByUrl({ url: res.data.url });
    });
  }
  function addOrUpdateHandle(id = '', parentId = '') {
    openFormModal(true, { id, category: state.listQuery.category, systemId: state.systemId, parentId });
  }
  function uploadTpl() {
    openUploadFormModal(true);
    nextTick(() => {
      setTimeout(() => {
        resetUploadFormFields();
        getMenuSelectorList();
        updateSchema({ field: 'parentId', componentProps: { lastLevel: state.listQuery.category == 'App' } });
      }, 0);
    });
  }
  async function handleUploadSubmit() {
    const values = await validate();
    if (!values) return;
    state.parentId = values.parentId;
    const upload = unref(uploadRef);
    upload?.handlerBtnClick();
  }
  function onUploadSuccess() {
    onUploadError();
    closeModal();
    reloadTable();
  }
  function onBeforeUpload() {
    setModalProps({ confirmLoading: true });
  }
  function onUploadError() {
    setModalProps({ confirmLoading: false });
  }
  function getMenuSelectorList() {
    getMenuSelector({ category: state.listQuery.category }, '', state.systemId).then(res => {
      let topItem = {
        fullName: '顶级节点',
        hasChildren: true,
        id: '-1',
        children: res.data.list,
      };
      updateSchema({ field: 'parentId', componentProps: { options: [topItem] } });
    });
  }
  function reloadTable() {
    state.listQuery.category == 'Web' ? reloadWebTable({ page: 1 }) : reloadAppTable({ page: 1 });
  }
</script>
