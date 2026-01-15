<template>
  <div class="page-content-wrapper">
    <div class="page-content-wrapper-left">
      <BasicLeftTree
        title="组织机构"
        :treeData="treeData"
        :loading="treeLoading"
        @reload="reloadTree"
        @select="handleTreeSelect"
        :dropDownActions="leftDropDownActions" />
    </div>
    <div class="page-content-wrapper-center">
      <div class="page-content-wrapper-content">
        <BasicTable @register="registerTable" :searchInfo="searchInfo" ref="tableRef">
          <template #tableTitle>
            <a-button type="primary" preIcon="icon-ym icon-ym-btn-add" @click="addOrUpdateHandle()">{{ t('common.addText') }}</a-button>
            <a-button type="link" @click="handleExport"><i class="icon-ym icon-ym-btn-download button-preIcon"></i>{{ t('common.exportText') }}</a-button>
            <a-button type="link" @click="handleImport"><i class="icon-ym icon-ym-btn-upload button-preIcon"></i>{{ t('common.importText') }}</a-button>
          </template>
          <template #bodyCell="{ column, record }">
            <template v-if="column.key === 'enabledMark'">
              <a-tag :color="record.enabledMark == 1 ? 'success' : record.enabledMark == 2 ? 'warning' : 'error'">
                {{ record.enabledMark == 1 ? '启用' : record.enabledMark == 2 ? '锁定' : '禁用' }}
              </a-tag>
            </template>
            <template v-if="column.key === 'action' && !record.isAdministrator">
              <TableAction :actions="getTableActions(record)" :dropDownActions="getDropDownActions(record)" />
            </template>
          </template>
        </BasicTable>
      </div>
    </div>
    <Form @register="registerForm" @reload="reload" />
    <ResetPassword @register="registerPsdModal" />
    <OrgTree @register="registerOrgTree" />
    <SocialsBind @register="registerSocialsBind" />
    <ExportModal @register="registerExportModal" />
    <ImportModal @register="registerImportModal" @reload="reload" />
  </div>
</template>
<script lang="ts" setup>
  import { reactive, ref, onMounted } from 'vue';
  import { BasicLeftTree, TreeItem } from '/@/components/Tree';
  import { getUserList, unlockUser, delUser } from '/@/api/permission/user';
  import { getDepartmentSelectorByAuth } from '/@/api/permission/organize';
  import { BasicTable, useTable, TableAction, BasicColumn, FormProps, ActionItem } from '/@/components/Table';
  import { useMessage } from '/@/hooks/web/useMessage';
  import { useI18n } from '/@/hooks/web/useI18n';
  import { useModal } from '/@/components/Modal';
  import { usePopup } from '/@/components/Popup';
  import Form from './Form.vue';
  import ResetPassword from './ResetPassword.vue';
  import OrgTree from './OrgTree.vue';
  import SocialsBind from './SocialsBind.vue';
  import ExportModal from './ExportModal.vue';
  import ImportModal from './ImportModal.vue';
  import { createLocalStorage } from '/@/utils/cache';

  defineOptions({ name: 'permission-user' });

  const { createMessage } = useMessage();
  const { t } = useI18n();
  const ls = createLocalStorage();
  const [registerPsdModal, { openModal: openPsdModal }] = useModal();
  const [registerExportModal, { openModal: openExportModal }] = useModal();
  const [registerImportModal, { openModal: openImportModal }] = useModal();
  const [registerForm, { openPopup: openFormPopup }] = usePopup();
  const [registerOrgTree, { openPopup: openOrgTreePopup }] = usePopup();
  const [registerSocialsBind, { openPopup: openSocialsBindPopup }] = usePopup();

  const columns: BasicColumn[] = [
    { title: '账号', dataIndex: 'account', width: 100 },
    { title: '姓名', dataIndex: 'realName', width: 100 },
    {
      title: '性别',
      dataIndex: 'gender',
      width: 90,
      align: 'center',
      customRender: ({ record }) => {
        const text = record.gender == 1 ? '男' : record.gender == 2 ? '女' : '保密';
        return text;
      },
    },
    { title: '手机', dataIndex: 'mobilePhone', width: 120 },
    { title: '所属组织', dataIndex: 'organize' },
    { title: '创建时间', dataIndex: 'creatorTime', width: 150, format: 'date|YYYY-MM-DD HH:mm' },
    { title: '排序', dataIndex: 'sortCode', width: 70, align: 'center' },
    { title: '状态', dataIndex: 'enabledMark', width: 70, align: 'center' },
  ];
  const searchInfo = reactive({
    organizeId: '',
  });
  const leftDropDownActions = [
    {
      label: '架构图',
      onClick: openOrgTreePopup.bind(null, true, {}),
    },
  ];
  const treeLoading = ref(false);
  const treeData = ref<TreeItem[]>([]);
  const organizeIdTree = ref([]);
  const [registerTable, { reload, setLoading, getForm, getFetchParams }] = useTable({
    api: getUserList,
    columns,
    immediate: false,
    useSearchForm: true,
    formConfig: getFormConfig(),
    actionColumn: {
      width: 150,
      title: '操作',
      dataIndex: 'action',
    },
  });

  function getFormConfig(): Partial<FormProps> {
    return {
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
    };
  }
  function getTableActions(record): ActionItem[] {
    return [
      {
        label: t('common.editText'),
        onClick: addOrUpdateHandle.bind(null, record.id),
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
  function getDropDownActions(record): ActionItem[] {
    return [
      {
        label: '重置密码',
        onClick: handleResetPwd.bind(null, record.id, record.account),
      },
      {
        label: '解除锁定',
        ifShow: record.enabledMark === 2,
        modelConfirm: {
          title: '解除锁定',
          content: '此操作将解除该账户锁定, 是否继续?',
          onOk: handleUnlock.bind(null, record.id),
        },
      },
      {
        label: '绑定管理',
        ifShow: !!ls.get('useSocials'),
        onClick: handleSocialsBind.bind(null, record.id),
      },
    ];
  }
  function initData(isInit = false) {
    treeLoading.value = true;
    if (isInit) setLoading(true);
    getDepartmentSelectorByAuth().then(res => {
      treeData.value = res.data.list;
      treeLoading.value = false;
      if (isInit) reload({ page: 1 });
    });
  }
  function reloadTree() {
    treeData.value = [];
    initData();
  }
  function handleTreeSelect(id, _node, nodePath) {
    if (!id || searchInfo.organizeId === id) return;
    searchInfo.organizeId = id;
    organizeIdTree.value = nodePath.map(o => o.id);
    getForm().resetFields();
  }
  function addOrUpdateHandle(id = '') {
    openFormPopup(true, { id, organizeIdTree: organizeIdTree.value || [] });
  }
  function handleDelete(id) {
    delUser(id).then(res => {
      createMessage.success(res.msg);
      reload();
    });
  }
  function handleResetPwd(id, account) {
    openPsdModal(true, { id, account });
  }
  function handleSocialsBind(id) {
    openSocialsBindPopup(true, { id });
  }
  function handleUnlock(id) {
    unlockUser(id).then(res => {
      createMessage.success(res.msg);
      reload();
    });
  }
  function handleExport() {
    const listQuery = {
      ...getFetchParams(),
      organizeId: getFetchParams().organizeId || '',
      keyword: getFetchParams().keyword || '',
    };
    openExportModal(true, { listQuery });
  }
  function handleImport() {
    openImportModal(true, {});
  }

  onMounted(() => {
    initData(true);
  });
</script>
