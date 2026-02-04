<template>
  <BasicPopup
    v-bind="$attrs"
    @register="registerPopup"
    class="full-popup"
    :title="title"
    showOkBtn
    :show-back-icon="dataForm.objectType != 'Batch'"
    :show-cancel-btn="dataForm.objectType != 'Batch'"
    @ok="handleSubmit"
    :okButtonProps="{ disabled: activeStep < maxStep }">
    <template #insertToolbar>
      <a-space :size="10">
        <a-dropdown :disabled="loading">
          <template #overlay>
            <a-menu>
              <a-menu-item @click="handleCheckAll(true)">全部勾选</a-menu-item>
              <a-menu-item @click="handleCheckAll(false)">取消全选</a-menu-item>
              <a-menu-item @click="handleExpandAll(true)">展开所有</a-menu-item>
              <a-menu-item @click="handleExpandAll(false)">折叠所有</a-menu-item>
            </a-menu>
          </template>
          <a-button>操作<DownOutlined /></a-button>
        </a-dropdown>
        <a-button @click="handlePrev" :disabled="activeStep <= 0 || loading">{{ t('common.prev') }}</a-button>
        <a-button @click="handleNext" :disabled="activeStep >= maxStep || loading">{{ t('common.next') }} </a-button>
      </a-space>
    </template>
    <div class="auth-container">
      <a-steps v-model:current="activeStep" type="navigation" size="small">
        <a-step title="应用权限" disabled />
        <a-step title="菜单权限" disabled />
        <a-step title="按钮权限" disabled />
        <a-step title="列表权限" disabled />
        <a-step title="表单权限" disabled />
        <a-step title="数据权限" disabled />
        <a-step title="选择角色" disabled v-if="maxStep > 5" />
      </a-steps>
      <div class="main">
        <BasicTree
          :treeData="treeData"
          ref="treeRef"
          checkable
          blockNode
          clickRowToExpand
          defaultExpandAll
          :fieldNames="activeStep == 6 ? { key: 'onlyId' } : { key: 'id' }"
          :loading="loading"
          @check="onCheck"
          :key="key"
          class="overflow-auto" />
      </div>
    </div>
  </BasicPopup>
</template>
<script lang="ts" setup>
  import { getAuthorizeValues, updateAuthorizeList, updateBatchAuthorize } from '/@/api/permission/authorize';
  import { getRoleSelectorByPermission } from '/@/api/permission/role';
  import { BasicPopup, usePopupInner } from '/@/components/Popup';
  import { BasicTree, TreeActionType } from '/@/components/Tree';
  import { reactive, toRefs, ref, unref, nextTick } from 'vue';
  import { DownOutlined } from '@ant-design/icons-vue';
  import { useMessage } from '/@/hooks/web/useMessage';
  import { useI18n } from '/@/hooks/web/useI18n';

  interface State {
    title: string;
    activeStep: number;
    maxStep: number;
    treeData: any[];
    loading: boolean;
    objectId: string;
    dataForm: any;
    params: any;
    systemIdsAuthorizeTree: any[];
    moduleAuthorizeTree: any[];
    buttonAuthorizeTree: any[];
    columnAuthorizeTree: any[];
    formAuthorizeTree: any[];
    resourceAuthorizeTree: any[];
    systemIdsAllData: any[];
    moduleAllData: any[];
    buttonAllData: any[];
    columnAllData: any[];
    formAllData: any[];
    resourceAllData: any[];
    moduleIdsTemp: any[];
    systemIds: any[];
    key: number;
  }

  const { createMessage } = useMessage();
  const { t } = useI18n();
  const [registerPopup, { closePopup, changeOkLoading }] = usePopupInner(init);
  const treeRef = ref<Nullable<TreeActionType>>(null);
  const state = reactive<State>({
    title: '',
    activeStep: 0,
    maxStep: 5,
    treeData: [],
    loading: false,
    objectId: '',
    dataForm: {
      objectType: '',
      module: [],
      systemIds: [],
      button: [],
      column: [],
      form: [],
      resource: [],
      roleIds: [],
    },
    params: {
      type: 'system',
      moduleIds: '',
    },
    systemIdsAuthorizeTree: [],
    moduleAuthorizeTree: [],
    buttonAuthorizeTree: [],
    columnAuthorizeTree: [],
    formAuthorizeTree: [],
    resourceAuthorizeTree: [],
    systemIdsAllData: [],
    moduleAllData: [],
    buttonAllData: [],
    columnAllData: [],
    formAllData: [],
    resourceAllData: [],
    moduleIdsTemp: [],
    systemIds: [],
    key: 0,
  });
  const { dataForm, title, activeStep, maxStep, treeData, loading, key } = toRefs(state);

  function init(data) {
    resetState();
    state.objectId = data.id;
    state.dataForm.objectType = data.type;
    if (data.type === 'User') {
      state.title = '用户权限 - ' + data.fullName;
    } else if (data.type === 'Position') {
      state.title = '岗位权限 - ' + data.fullName;
    } else if (data.type === 'Role') {
      state.title = '角色权限 - ' + data.fullName;
    } else if (data.type === 'Batch') {
      state.title = data.fullName;
    }
    state.maxStep = data.type === 'Batch' ? 6 : 5;
    getAuthorizeList();
  }
  function resetState() {
    state.activeStep = 0;
    state.dataForm = {
      objectType: '',
      module: [],
      systemIds: [],
      button: [],
      column: [],
      form: [],
      resource: [],
      roleIds: [],
    };
    state.params = {
      type: 'system',
      moduleIds: '',
    };
    state.moduleIdsTemp = [];
    state.systemIds = [];
  }
  function getAuthorizeList() {
    state.loading = true;
    state.treeData = [];
    getTree().setCheckedKeys([]);
    getAuthorizeValues(state.objectId, state.params)
      .then(res => {
        state.key = +new Date();
        const key = getKey();
        state[key + 'AuthorizeTree'] = res.data.list || [];
        state[key + 'AllData'] = res.data.all || [];
        state.treeData = state[key + 'AuthorizeTree'];
        const parentKeys = getTree().getParentKeys(state.treeData);
        const dataIds = [...new Set([...state.dataForm[key], ...res.data.ids, ...state.moduleIdsTemp])];
        setDataFormValue(dataIds);
        const checkedKeys = dataIds.filter(o => !parentKeys.includes(o));
        nextTick(() => {
          getTree().setCheckedKeys(checkedKeys);
          state.loading = false;
        });
      })
      .catch(() => {
        state.loading = false;
      });
  }
  function getRoleList() {
    state.loading = true;
    state.treeData = [];
    getTree().setCheckedKeys([]);
    getRoleSelectorByPermission()
      .then(res => {
        state.treeData = res.data.list || [];
        getTree().setCheckedKeys(state.dataForm.roleIds);
        state.loading = false;
      })
      .catch(() => {
        state.loading = false;
      });
  }
  function getKey() {
    if (state.activeStep === 0) return 'systemIds';
    if (state.activeStep === 1) return 'module';
    if (state.activeStep === 2) return 'button';
    if (state.activeStep === 3) return 'column';
    if (state.activeStep === 4) return 'form';
    if (state.activeStep === 5) return 'resource';
    if (state.activeStep === 6) return 'roleIds';
    return 'systemIds';
  }
  function onCheck(checkedKeys, e) {
    const halfCheckedKeys = state.activeStep === 6 ? [] : e.halfCheckedKeys;
    let dataIds = [...checkedKeys, ...halfCheckedKeys];
    setDataFormValue(dataIds);
  }
  function setDataFormValue(dataIds) {
    const key = getKey();
    state.dataForm[key] = dataIds;
    if (state.activeStep === 0 || state.activeStep === 1) state.moduleIdsTemp = state.dataForm[key];
    if (state.activeStep === 0) state.systemIds = state.moduleIdsTemp;
  }
  function handlePrev() {
    state.activeStep -= 1;
    handelInitData();
  }
  function handleNext() {
    state.activeStep += 1;
    handelInitData();
  }
  function handelInitData() {
    if (state.activeStep === 6) return getRoleList();
    const key = getKey();
    state.params.type = key === 'systemIds' ? 'system' : key;
    state.params.moduleIds = key === 'module' ? state.systemIds.toString() : state.moduleIdsTemp.toString();
    getAuthorizeList();
  }
  function handleCheckAll(boo) {
    getTree().checkAll(boo);
  }
  function handleExpandAll(boo) {
    getTree().expandAll(boo);
  }
  function getTree() {
    const tree = unref(treeRef);
    if (!tree) throw new Error('tree is null!');
    return tree;
  }
  function handleSubmit() {
    changeOkLoading(true);
    if (state.maxStep === 6) {
      updateBatchAuthorize(state.dataForm)
        .then(res => {
          createMessage.success(res.msg);
          changeOkLoading(false);
          init({ id: '0', fullName: '权限批量设置', type: 'Batch' });
        })
        .catch(() => {
          changeOkLoading(false);
        });
    } else {
      updateAuthorizeList(state.objectId, state.dataForm)
        .then(res => {
          createMessage.success(res.msg);
          closePopup();
          changeOkLoading(false);
        })
        .catch(() => {
          changeOkLoading(false);
        });
    }
  }
</script>
<style lang="less" scoped>
  .auth-container {
    height: 100%;
    display: flex;
    flex-direction: column;
    .ant-steps {
      flex-shrink: 0;
    }
    .main {
      flex: 1;
      padding: 20px;
      overflow: hidden;
    }
  }
</style>
