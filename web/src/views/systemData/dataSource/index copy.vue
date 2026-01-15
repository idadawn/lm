<template>
  <div class="page-content-wrapper">
    <div class="page-content-wrapper-left">
      <BasicLeftTree
        title="数据源"
        :treeData="treeData"
        :loading="treeLoading"
        @reload="reloadTree"
        @select="handleTreeSelect" />
    </div>
    <div class="page-content-wrapper-center">
      <div class="page-content-wrapper-content">
        <BasicTable @register="registerTable">
          <template #tableTitle>
            <a-button type="primary" preIcon="icon-ym icon-ym-btn-add" @click="addOrUpdateHandle()">{{
              t('common.addText')
            }}</a-button>
          </template>
          <template #bodyCell="{ column, record }">
            <template v-if="column.key === 'action'">
              <TableAction :actions="getTableActions(record)" />
            </template>
          </template>
        </BasicTable>
      </div>
    </div>
    <Form @register="registerForm" @reload="reload" @dbTypeModal="dbTypeModal" />
    <!-- 新增数据库 -->
    <a-modal width="940px" v-model:visible="state.addVisible" title="" :footer="null">
      <SourceAdd @getdbType="getdbType" />
    </a-modal>
  </div>
</template>
<script lang="ts" setup>
  import { BasicTable, useTable, TableAction, BasicColumn, ActionItem } from '/@/components/Table';
  import { BasicLeftTree, TreeItem } from '/@/components/Tree';
  import { Modal as AModal } from 'ant-design-vue';
  import { useI18n } from '/@/hooks/web/useI18n';
  import { getDataSourceList, delDataSource } from '/@/api/systemData/dataSource';
  import { getMetriccategoryList } from '/@/api/targetDirectory';
  import { useMessage } from '/@/hooks/web/useMessage';
  import { useModal } from '/@/components/Modal';
  import { onMounted, ref, reactive } from 'vue';
  import { useBaseStore } from '/@/store/modules/base';
  import Form from './Form.vue';
  import SourceAdd from './components/sourceAdd.vue';

  defineOptions({ name: 'systemData-dataSource' });
  const state = reactive({
    addVisible: false, //新建
    dbType: 'MySQL', //数据库类型
  });
  const treeData = ref<any[]>([]);
  const treeLoading = ref(false);

  const { t } = useI18n();
  const { createMessage } = useMessage();
  const baseStore = useBaseStore();
  const columns: BasicColumn[] = [
    { title: '连接名称', dataIndex: 'fullName' },
    { title: '连接驱动', dataIndex: 'dbType', width: 150 },
    { title: '主机地址', dataIndex: 'host', width: 200 },
    { title: '端口', dataIndex: 'port', width: 60 },
    { title: '创建人', dataIndex: 'creatorUser', width: 120 },
    { title: '创建时间', dataIndex: 'creatorTime', width: 150, format: 'date|YYYY-MM-DD HH:mm' },
    { title: '最后修改时间', dataIndex: 'lastModifyTime', width: 150, format: 'date|YYYY-MM-DD HH:mm' },
    { title: '排序', dataIndex: 'sortCode', width: 70 },
  ];
  const [registerForm, { openModal: openFormModal }] = useModal();
  const [registerTable, { reload, setLoading, getForm }] = useTable({
    api: getDataSourceList,
    columns,
    useSearchForm: true,
    formConfig: {
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
        {
          field: 'dbType',
          label: '连接驱动',
          component: 'Select',
          componentProps: {
            placeholder: '请选择连接驱动',
            fieldNames: { label: 'fullName', value: 'enCode' },
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
  function handleDelete(id) {
    delDataSource(id).then(res => {
      createMessage.success(res.msg);
      reload();
    });
  }
  // 添加数据类型弹层
  function addOrUpdateHandle(id = '') {
    if (!id) {
      state.addVisible = true;
    } else {
      openFormModal(true, { id });
    }
  }
  async function getDictionaryData() {
    const categoryList = (await baseStore.getDictionaryData('dbType')) as any[];
    getForm().updateSchema({ field: 'dbType', componentProps: { options: categoryList } });
  }
  // 左侧菜单
  function initData(isInit = false) {
    treeLoading.value = true;
    if (isInit) setLoading(true);
    getMetriccategoryList({}).then(res => {
      treeData.value = [{ fullName: '顶级节点', id: '', children: res.data }];
      treeLoading.value = false;
      if (isInit) reload({ page: 1 });
    });
  }

  function reloadTree() {
    treeData.value = [];
    initData();
  }
  function handleTreeSelect(id, _node, nodePath) {
    console.log(id);
    // if (searchInfo.menuId === id) return;
    // searchInfo.menuId = id;
    // reload();
  }

  // 点击数据图标时
  function getdbType(dbType) {
    openFormModal(true, { dbType });
  }
  // 新建成功时关掉弹层
  function dbTypeModal() {
    state.addVisible = false;
  }
  onMounted(() => {
    getDictionaryData();
    initData(true);
  });
</script>
