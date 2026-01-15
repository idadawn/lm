<template>
  <div class="common-container">
    <template v-if="config.popupType === 'dialog'">
      <a-modal
        v-model:visible="visible"
        :title="config.popupTitle"
        :width="config.popupWidth"
        class="common-container-modal"
        @ok="handleSubmit"
        @cancel="handleCancel"
        :maskClosable="false">
        <template #closeIcon>
          <ModalClose :canFullscreen="false" @cancel="handleCancel" />
        </template>
        <div class="jnpf-common-search-box jnpf-common-search-box-modal">
          <a-form :colon="false" labelAlign="right" :model="listQuery" ref="formElRef" :class="getFormClass">
            <a-row :gutter="10">
              <a-col :span="8">
                <a-form-item :label="t('common.keyword')" name="keyword">
                  <a-input v-model:value="listQuery.keyword" :placeholder="t('common.enterKeyword')" allowClear @pressEnter="handleSearch" />
                </a-form-item>
              </a-col>
              <a-col :span="8">
                <a-form-item label=" ">
                  <a-button type="primary" class="mr-2" @click="handleSearch">{{ t('common.queryText') }}</a-button>
                  <a-button @click="handleReset">{{ t('common.resetText') }}</a-button>
                </a-form-item>
              </a-col>
            </a-row>
          </a-form>
          <div class="jnpf-common-search-box-right">
            <a-tooltip placement="top">
              <template #title>
                <span>{{ t('common.redo') }}</span>
              </template>
              <RedoOutlined class="jnpf-common-search-box-right-icon" @click="initData" />
            </a-tooltip>
          </div>
        </div>
        <a-table :data-source="list" v-bind="getTableBindValues" @change="handleTableChange" ref="tableElRef"></a-table>
      </a-modal>
    </template>
    <template v-if="config.popupType === 'drawer'">
      <a-drawer :title="config.popupTitle" :width="config.popupWidth" v-model:visible="visible" :class="drawerPrefixCls + ' common-container-drawer'">
        <div class="common-container-drawer-body">
          <div class="jnpf-common-search-box jnpf-common-search-box-modal">
            <a-form :colon="false" labelAlign="right" :model="listQuery" ref="formElRef" :class="getFormClass">
              <a-row :gutter="10">
                <a-col :span="8">
                  <a-form-item :label="t('common.keyword')" name="keyword">
                    <a-input v-model:value="listQuery.keyword" :placeholder="t('common.enterKeyword')" allowClear @pressEnter="handleSearch" />
                  </a-form-item>
                </a-col>
                <a-col :span="8">
                  <a-form-item label=" ">
                    <a-button type="primary" class="mr-2" @click="handleSearch">{{ t('common.queryText') }}</a-button>
                    <a-button @click="handleReset">{{ t('common.resetText') }}</a-button>
                  </a-form-item>
                </a-col>
              </a-row>
            </a-form>
            <div class="jnpf-common-search-box-right">
              <a-tooltip placement="top">
                <template #title>
                  <span>{{ t('common.redo') }}</span>
                </template>
                <RedoOutlined class="jnpf-common-search-box-right-icon" @click="initData" />
              </a-tooltip>
            </div>
          </div>
          <a-table :data-source="list" v-bind="getTableBindValues" @change="handleTableChange" ref="tableElRef"></a-table>
        </div>
        <DrawerFooter showFooter @close="handleCancel" @ok="handleSubmit"></DrawerFooter>
      </a-drawer>
    </template>
  </div>
</template>

<script lang="ts" setup>
  import { getDataInterfaceDataSelect } from '/@/api/systemData/dataInterface';
  import { Modal as AModal, Drawer as ADrawer } from 'ant-design-vue';
  import { ref, unref, computed, nextTick, reactive, toRefs } from 'vue';
  import ModalClose from '/@/components/Modal/src/components/ModalClose.vue';
  import { useI18n } from '/@/hooks/web/useI18n';
  import { useDesign } from '/@/hooks/web/useDesign';
  import { useUserStore } from '/@/store/modules/user';
  import DrawerFooter from '/@/components/Drawer/src/components/DrawerFooter.vue';
  import { RedoOutlined } from '@ant-design/icons-vue';
  import type { FormInstance } from 'ant-design-vue';

  interface State {
    list: any[];
    listQuery: any;
    loading: boolean;
    total: number;
    selectedRowKeys: any[];
    selectedRows: any[];
  }

  defineOptions({ name: 'SelectModal', inheritAttrs: false });
  const props = defineProps({
    config: {
      type: Object,
      default: () => {},
    },
    formData: Object,
  });
  const emit = defineEmits(['select']);
  const { t } = useI18n();
  const { prefixCls: drawerPrefixCls } = useDesign('basic-drawer');
  const { prefixCls: formPrefixCls } = useDesign('basic-form');
  const { prefixCls: tablePrefixCls } = useDesign('basic-table');

  const selectRow = ref<any>(null);
  const visible = ref(false);
  const userStore = useUserStore();
  const formElRef = ref<FormInstance>();
  const tableElRef = ref<any>(null);
  const indexColumn = { width: 50, title: '序号', dataIndex: 'index', key: 'index', align: 'center', customRender: ({ index }) => index + 1 };
  const state = reactive<State>({
    list: [],
    listQuery: {
      keyword: '',
      currentPage: 1,
      pageSize: 20,
    },
    loading: false,
    total: 0,
    selectedRowKeys: [],
    selectedRows: [],
  });
  const { listQuery, list } = toRefs(state);

  const getUserInfo = computed(() => userStore.getUserInfo || {});
  const getFormClass = computed(() => {
    return [formPrefixCls, `${formPrefixCls}--compact`, 'search-form'];
  });
  const getColumns = computed<any[]>(() => {
    const columns = (props.config.columnOptions as any).map(o => ({ title: o.label, dataIndex: o.value }));
    return [indexColumn, ...columns];
  });
  const searchInfo = computed(() => {
    const paramList = getParamList();
    const info: any = {
      interfaceId: props.config.interfaceId,
      columnOptions: (props.config.columnOptions as any).map(o => o.value).join(','),
      paramList,
    };
    return info;
  });
  const getPagination = computed<any>(() => {
    return {
      current: state.listQuery.currentPage,
      pageSize: state.listQuery.pageSize,
      size: 'small',
      defaultPageSize: 20,
      showTotal: total => t('component.table.total', { total }),
      showSizeChanger: true,
      pageSizeOptions: ['20', '50', '80', '100'],
      showQuickJumper: true,
      total: state.total,
    };
  });
  const getRowSelection = computed<any>(() => ({
    type: 'checkbox',
    onChange: setSelectedRowKeys,
  }));
  const getScrollY = computed(() => {
    let height = props.config.popupType === 'drawer' ? window.innerHeight - 120 - 52 - 38 : window.innerHeight * 0.7 - 52 - 38;
    height -= 44;
    return height;
  });
  const getTableBindValues = computed(() => {
    return {
      columns: unref(getColumns),
      pagination: unref(getPagination),
      rowSelection: unref(getRowSelection),
      size: 'small',
      loading: state.loading,
      rowKey: record => record,
      scroll: {
        y: unref(getScrollY),
      },
      class: unref(tablePrefixCls),
    };
  });

  defineExpose({ openSelectModal });

  function getForm() {
    const form = unref(formElRef);
    if (!form) {
      throw new Error('form is null!');
    }
    return form;
  }
  function openSelectModal() {
    visible.value = true;
    setTimeout(() => {
      nextTick(() => {
        handleReset();
        state.selectedRowKeys = [];
        state.selectedRows = [];
        const tableEl = tableElRef.value?.$el;
        let bodyEl = tableEl.querySelector('.ant-table-body');
        bodyEl!.style.height = `${unref(getScrollY)}px`;
      });
    }, 50);
  }
  function handleCancel() {
    visible.value = false;
  }
  function handleSubmit() {
    if (!state.selectedRowKeys.length && !state.selectedRows.length) return;
    selectRow.value = state.selectedRows;
    let checked: any[] = [];
    for (let i = 0; i < unref(selectRow).length; i++) {
      const e = unref(selectRow)[i];
      let item = {};
      for (let j = 0; j < props.config.relationOptions.length; j++) {
        const row = props.config.relationOptions[j];
        item[row.field] = row.type === 1 ? e[row.value] : row.value;
      }
      checked.push(item);
    }
    emit('select', checked);
    handleCancel();
  }
  function getParamList() {
    let templateJson: any[] = props.config.templateJson;
    if (!props.formData) return templateJson;
    for (let i = 0; i < templateJson.length; i++) {
      templateJson[i].defaultValue = props.formData[templateJson[i].relationField] || '';
      if (templateJson[i].jnpfKey === 'createUser') {
        templateJson[i].defaultValue = unref(getUserInfo).userId;
      }
      if (templateJson[i].jnpfKey === 'createTime') {
        templateJson[i].defaultValue = new Date().getTime();
      }
      if (templateJson[i].jnpfKey === 'currOrganize') {
        templateJson[i].defaultValue = unref(getUserInfo).organizeId;
      }
      if (templateJson[i].jnpfKey === 'currPosition') {
        if (unref(getUserInfo).positionIds && unref(getUserInfo).positionIds.length) {
          let item: any = unref(getUserInfo).positionIds[0];
          templateJson[i].defaultValue = item.id;
        }
      }
    }
    return templateJson;
  }

  function handleSearch() {
    state.listQuery.currentPage = 1;
    state.listQuery.pageSize = 20;
    initData();
  }
  function handleReset() {
    getForm().resetFields();
    state.listQuery.keyword = '';
    handleSearch();
  }
  function initData() {
    if (!props.config.interfaceId) return;
    state.loading = true;
    const query = {
      ...state.listQuery,
      ...unref(searchInfo),
    };
    getDataInterfaceDataSelect(query)
      .then(res => {
        state.list = res.data.list;
        state.total = res.data.pagination.total;
        state.loading = false;
      })
      .catch(() => {
        state.loading = false;
      });
  }
  function handleTableChange(pagination) {
    state.listQuery.currentPage = pagination.current;
    state.listQuery.pageSize = pagination.pageSize;
    initData();
  }
  function setSelectedRowKeys(_selectedRowKeys, selectedRows) {
    state.selectedRows = selectedRows;
  }
</script>
