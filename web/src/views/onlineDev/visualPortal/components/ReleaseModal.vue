<template>
  <BasicModal v-bind="$attrs" @register="registerModal" title="同步门户" @ok="handleSubmit" class="jnpf-release-modal">
    <a-alert message="将该门户同步至应用门户，是否继续？" type="warning" show-icon />
    <div class="release-main">
      <div class="item" :class="{ active: dataForm.pc === 1 }" @click="selectToggle('pc')">
        <i class="item-icon icon-ym icon-ym-pc"></i>
        <p class="item-title">同步Web端门户</p>
        <div class="icon-checked">
          <check-outlined />
        </div>
      </div>
      <div class="item" :class="{ active: dataForm.app === 1 }" @click="selectToggle('app')">
        <i class="item-icon icon-ym icon-ym-mobile"></i>
        <p class="item-title">同步APP端门户</p>
        <div class="icon-checked">
          <check-outlined />
        </div>
      </div>
    </div>
    <a-form class="release-form-main" :colon="false" :model="dataForm" hideRequiredMark :rules="rules" :labelCol="{ style: { width: '40px' } }" ref="formElRef">
      <template v-if="!record.pcIsRelease">
        <a-form-item label="应用" name="pcModuleParentId" v-if="dataForm.pc">
          <JnpfSelect v-model:value="dataForm.pcModuleParentId" :options="treeData" multiple placeholder="选择应用" :allowClear="false" />
        </a-form-item>
      </template>
      <template v-if="!record.appIsRelease">
        <a-form-item v-if="(!dataForm.pc || record.pcIsRelease) && dataForm.app"></a-form-item>
        <a-form-item label="应用" name="appModuleParentId" v-if="dataForm.app">
          <JnpfSelect v-model:value="dataForm.appModuleParentId" :options="treeData" multiple placeholder="选择应用" :allowClear="false" />
        </a-form-item>
      </template>
    </a-form>
  </BasicModal>
</template>
<script lang="ts" setup>
  import { release } from '/@/api/onlineDev/portal';
  import { BasicModal, useModalInner } from '/@/components/Modal';
  import { ref, reactive, toRefs, nextTick } from 'vue';
  import type { FormInstance } from 'ant-design-vue';
  import { CheckOutlined } from '@ant-design/icons-vue';
  import { getSystemList } from '/@/api/system/system';
  import { useMessage } from '/@/hooks/web/useMessage';

  interface State {
    dataForm: any;
    record: any;
    rules: any;
    treeData: any[];
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
      pcModuleParentId: [{ required: true, message: '应用不能为空', trigger: 'change' }],
      appModuleParentId: [{ required: true, message: '应用不能为空', trigger: 'change' }],
    },
    treeData: [],
  });
  const { dataForm, record, rules, treeData } = toRefs(state);

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
    nextTick(() => {
      formElRef.value?.clearValidate();
    });
  }
  function getMenuOptions() {
    getSystemList({ enableMark: 1 }).then(res => {
      state.treeData = res.data.list || [];
    });
  }
  function selectToggle(key) {
    state.dataForm[key] = state.dataForm[key] === 1 ? 0 : 1;
  }
  async function handleSubmit() {
    try {
      if (!state.dataForm.pc && !state.dataForm.app) return createMessage.error('请至少选择一种门户同步方式');
      const values = await formElRef.value?.validate();
      if (!values) return;
      state.dataForm.pcSystemId = state.dataForm.pcModuleParentId.toString();
      state.dataForm.appSystemId = state.dataForm.appModuleParentId.toString();
      createConfirm({
        iconType: 'warning',
        title: '提示',
        content: '发布确定后会覆盖当前线上版本且进行门户同步，是否继续?',
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
