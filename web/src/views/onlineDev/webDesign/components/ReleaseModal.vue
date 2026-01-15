<template>
  <BasicModal v-bind="$attrs" @register="registerModal" title="同步菜单" @ok="handleSubmit" class="jnpf-release-modal">
    <a-alert message="将该功能的按钮、列表、表单及数据权限同步至系统菜单" type="warning" show-icon />
    <div class="release-main">
      <div class="item" :class="{ active: dataForm.pc === 1 }" @click="selectToggle('pc')">
        <i class="item-icon icon-ym icon-ym-pc"></i>
        <p class="item-title">同步Web端菜单</p>
        <div class="icon-checked">
          <check-outlined />
        </div>
      </div>
      <div class="item" :class="{ active: dataForm.app === 1 }" @click="selectToggle('app')">
        <i class="item-icon icon-ym icon-ym-mobile"></i>
        <p class="item-title">同步APP端菜单</p>
        <div class="icon-checked">
          <check-outlined />
        </div>
      </div>
    </div>
    <a-form class="release-form-main" :colon="false" :model="dataForm" :rules="rules" :labelCol="{ style: { width: '50px' } }" ref="formElRef">
      <template v-if="!record.pcIsRelease">
        <a-form-item label="上级" name="pcModuleParentId" v-if="dataForm.pc">
          <JnpfTreeSelect
            v-model:value="dataForm.pcModuleParentId"
            :options="treeData"
            placeholder="选择上级菜单"
            :allowClear="false"
            :dropdownMatchSelectWidth="false"
            @change="onPcChange" />
        </a-form-item>
      </template>
      <template v-if="!record.appIsRelease">
        <a-form-item v-if="(!dataForm.pc || record.pcIsRelease) && dataForm.app"></a-form-item>
        <a-form-item label="上级" name="appModuleParentId" v-if="dataForm.app">
          <JnpfTreeSelect
            v-model:value="dataForm.appModuleParentId"
            :options="appTreeData"
            placeholder="选择上级菜单"
            :allowClear="false"
            :dropdownMatchSelectWidth="false"
            @change="onAppChange" />
        </a-form-item>
      </template>
    </a-form>
  </BasicModal>
</template>
<script lang="ts" setup>
  import { release } from '/@/api/onlineDev/visualDev';
  import { BasicModal, useModalInner } from '/@/components/Modal';
  import { ref, reactive, toRefs, nextTick } from 'vue';
  import type { FormInstance } from 'ant-design-vue';
  import { CheckOutlined } from '@ant-design/icons-vue';
  import { getMenuSelector } from '/@/api/system/menu';
  import { useMessage } from '/@/hooks/web/useMessage';

  interface State {
    dataForm: any;
    record: any;
    rules: any;
    treeData: any[];
    appTreeData: any[];
  }

  const emit = defineEmits(['register', 'reload']);
  const { createMessage, createConfirm } = useMessage();
  const [registerModal, { changeOkLoading, closeModal }] = useModalInner(init);
  const formElRef = ref<FormInstance>();
  const state = reactive<State>({
    dataForm: {
      pc: 1,
      app: 1,
      pcModuleParentId: '',
      appModuleParentId: '',
      pcSystemId: '',
      appSystemId: '',
    },
    record: {},
    rules: {
      pcModuleParentId: [{ required: true, message: '上级菜单不能为空', trigger: 'change' }],
      appModuleParentId: [{ required: true, message: '上级菜单不能为空', trigger: 'change' }],
    },
    treeData: [],
    appTreeData: [],
  });
  const { dataForm, record, rules, treeData, appTreeData } = toRefs(state);

  function init(data) {
    state.record = data;
    state.dataForm = {
      pc: 1,
      app: 1,
      pcModuleParentId: '',
      appModuleParentId: '',
      pcSystemId: '',
      appSystemId: '',
    };
    getMenuOptions();
    getAppMenuOptions();
    nextTick(() => {
      formElRef.value?.clearValidate();
    });
  }
  function getMenuOptions() {
    getMenuSelector({ category: 'Web' }, 0).then(res => {
      state.treeData = res.data.list;
    });
  }
  function getAppMenuOptions() {
    getMenuSelector({ category: 'App' }, 0).then(res => {
      let list = res.data.list || [];
      for (let index = 0; index < list.length; index++) {
        const item = list[index];
        if (item.type == 0) item.disabled = true;
      }
      state.appTreeData = list;
    });
  }
  function onPcChange(_id, data) {
    state.dataForm.pcSystemId = data.systemId;
  }
  function onAppChange(_id, data) {
    state.dataForm.appSystemId = data.systemId;
  }
  function selectToggle(key) {
    state.dataForm[key] = state.dataForm[key] === 1 ? 0 : 1;
  }
  async function handleSubmit() {
    try {
      if (!state.dataForm.pc && !state.dataForm.app) return createMessage.error('请至少选择一种菜单同步方式');
      const values = await formElRef.value?.validate();
      if (!values) return;
      createConfirm({
        iconType: 'warning',
        title: '提示',
        content: '发布模板会覆盖当前线上版本且进行菜单同步，是否继续?',
        onOk: () => {
          changeOkLoading(true);
          release(state.record.id, state.dataForm)
            .then(res => {
              changeOkLoading(false);
              createMessage.success(res.msg);
              emit('reload');
              closeModal();
            })
            .catch(() => {
              changeOkLoading(false);
            });
        },
      });
    } catch (_) {}
  }
</script>
