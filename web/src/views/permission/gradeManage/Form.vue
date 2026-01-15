<template>
  <BasicModal v-bind="$attrs" @register="registerModal" :title="getTitle" :width="900" @ok="handleSubmit" class="grade-modal">
    <a-alert message="设置当前组织部门的管理员和相关操作权限" type="warning" showIcon class="!mb-20px" />
    <a-form :colon="false" :labelCol="{ style: { width: '100px' } }" :model="dataForm" :rules="dataRule" ref="formElRef">
      <a-form-item label="设置管理员" name="userId">
        <jnpf-user-select v-model:value="dataForm.userId" placeholder="请选择管理员" @change="initData" :disabled="!!dataForm.id" v-if="getIsAdmin" />
        <grade-user-select v-model:value="dataForm.userId" placeholder="请选择管理员" @change="initData" :disabled="!!dataForm.id" v-else />
      </a-form-item>
    </a-form>
    <a-table
      :data-source="list"
      :columns="columns"
      size="small"
      :pagination="false"
      rowKey="organizeId"
      defaultExpandAllRows
      :scroll="{ y: '400px' }"
      v-loading="loading"
      :key="key">
      <template #bodyCell="{ column, record }">
        <template v-if="column.key === 'fullName'">
          <i :class="record.icon"></i>
          {{ record.fullName }}
        </template>
        <template v-if="column.key === 'thisLayer'">
          <JnpfCheckboxSingle label="查看" checked disabled v-if="record.thisLayerSelect === 2" />
          <JnpfCheckboxSingle label="查看" disabled v-if="record.thisLayerSelect === 3" />
          <template v-if="record.thisLayerSelect === 0 || record.thisLayerSelect === 1">
            <JnpfCheckboxSingle label="查看" v-model:value="record.thisLayerSelect" @change="onThisLayerSelectChange($event, record)" />
          </template>
          <JnpfCheckboxSingle label="添加" checked disabled v-if="record.thisLayerAdd === 2" />
          <JnpfCheckboxSingle label="添加" disabled v-if="record.thisLayerAdd === 3" />
          <template v-if="record.thisLayerAdd === 0 || record.thisLayerAdd === 1">
            <JnpfCheckboxSingle label="添加" v-model:value="record.thisLayerAdd" :disabled="!record.thisLayerSelect" />
          </template>
          <JnpfCheckboxSingle label="编辑" checked disabled v-if="record.thisLayerEdit === 2" />
          <JnpfCheckboxSingle label="编辑" disabled v-if="record.thisLayerEdit === 3" />
          <template v-if="record.thisLayerEdit === 0 || record.thisLayerEdit === 1">
            <JnpfCheckboxSingle label="编辑" v-model:value="record.thisLayerEdit" :disabled="!record.thisLayerSelect" />
          </template>
          <JnpfCheckboxSingle label="删除" checked disabled v-if="record.thisLayerDelete === 2" />
          <JnpfCheckboxSingle label="删除" disabled v-if="record.thisLayerDelete === 3" />
          <template v-if="record.thisLayerDelete === 0 || record.thisLayerDelete === 1">
            <JnpfCheckboxSingle label="删除" v-model:value="record.thisLayerDelete" :disabled="!record.thisLayerSelect" />
          </template>
        </template>
        <template v-if="column.key === 'subLayer'">
          <JnpfCheckboxSingle label="查看" checked disabled v-if="record.subLayerSelect === 2" />
          <JnpfCheckboxSingle label="查看" disabled v-if="record.subLayerSelect === 3" />
          <template v-if="record.subLayerSelect === 0 || record.subLayerSelect === 1">
            <JnpfCheckboxSingle label="查看" v-model:value="record.subLayerSelect" @change="onSubLayerSelectChange($event, record)" />
          </template>
          <JnpfCheckboxSingle label="添加" checked disabled v-if="record.subLayerAdd === 2" />
          <JnpfCheckboxSingle label="添加" disabled v-if="record.subLayerAdd === 3" />
          <template v-if="record.subLayerAdd === 0 || record.subLayerAdd === 1">
            <JnpfCheckboxSingle label="添加" v-model:value="record.subLayerAdd" :disabled="!record.subLayerSelect" />
          </template>
          <JnpfCheckboxSingle label="编辑" checked disabled v-if="record.subLayerEdit === 2" />
          <JnpfCheckboxSingle label="编辑" disabled v-if="record.subLayerEdit === 3" />
          <template v-if="record.subLayerEdit === 0 || record.subLayerEdit === 1">
            <JnpfCheckboxSingle label="编辑" v-model:value="record.subLayerEdit" :disabled="!record.subLayerSelect" />
          </template>
          <JnpfCheckboxSingle label="删除" checked disabled v-if="record.subLayerDelete === 2" />
          <JnpfCheckboxSingle label="删除" disabled v-if="record.subLayerDelete === 3" />
          <template v-if="record.subLayerDelete === 0 || record.subLayerDelete === 1">
            <JnpfCheckboxSingle label="删除" v-model:value="record.subLayerDelete" :disabled="!record.subLayerSelect" />
          </template>
        </template>
      </template>
    </a-table>
  </BasicModal>
</template>
<script lang="ts" setup>
  import { getSelectorOrgList, create } from '/@/api/permission/gradeManage';
  import { ref, computed, reactive, toRefs, nextTick } from 'vue';
  import { BasicModal, useModalInner } from '/@/components/Modal';
  import { useMessage } from '/@/hooks/web/useMessage';
  import { useUserStore } from '/@/store/modules/user';
  import type { FormInstance } from 'ant-design-vue';
  import GradeUserSelect from './GradeUserSelect.vue';

  interface State {
    dataForm: any;
    dataRule: any;
    loading: boolean;
    key: number;
    list: any[];
  }

  const emit = defineEmits(['register', 'reload']);
  const formElRef = ref<FormInstance>();
  const columns = [
    { title: '组织架构', dataIndex: 'fullName', key: 'fullName' },
    { title: '组织操作权限(本层级)', dataIndex: 'thisLayer', key: 'thisLayer', width: 280 },
    { title: '子组织操作权限(子层级)', dataIndex: 'subLayer', key: 'subLayer', width: 280 },
  ];
  const state = reactive<State>({
    dataForm: {
      id: '',
      userId: '',
    },
    dataRule: {
      userId: [{ required: true, message: '请选择管理员', trigger: 'change' }],
    },
    loading: false,
    key: +new Date(),
    list: [],
  });
  const { dataForm, dataRule, list, loading, key } = toRefs(state);
  const [registerModal, { closeModal, changeOkLoading }] = useModalInner(init);
  const { createMessage } = useMessage();
  const userStore = useUserStore();

  const getTitle = computed(() => (!state.dataForm.id ? '新建分管' : '编辑分管'));
  const getIsAdmin = computed(() => !!userStore.getUserInfo?.isAdministrator);

  function init(data) {
    state.dataForm.id = data.id;
    state.dataForm.userId = data.id || '';
    state.list = [];
    nextTick(() => {
      formElRef.value?.clearValidate();
      initData();
    });
  }
  function initData() {
    state.loading = true;
    getSelectorOrgList(state.dataForm.userId).then(res => {
      state.list = res.data.list || [];
      state.loading = false;
      state.key = +new Date();
    });
  }
  function onThisLayerSelectChange(val, record) {
    if (val) return;
    if (record.thisLayerAdd === 1) record.thisLayerAdd = 0;
    if (record.thisLayerEdit === 1) record.thisLayerEdit = 0;
    if (record.thisLayerDelete === 1) record.thisLayerDelete = 0;
  }
  function onSubLayerSelectChange(val, record) {
    if (val) return;
    if (record.subLayerAdd === 1) record.subLayerAdd = 0;
    if (record.subLayerEdit === 1) record.subLayerEdit = 0;
    if (record.subLayerDelete === 1) record.subLayerDelete = 0;
  }
  async function handleSubmit() {
    try {
      const values = await formElRef.value?.validate();
      if (!values) return;
      changeOkLoading(true);
      const query = { ...state.dataForm, orgAdminModel: state.list };
      create(query)
        .then(res => {
          createMessage.success(res.msg);
          changeOkLoading(false);
          closeModal();
          emit('reload');
        })
        .catch(() => {
          changeOkLoading(false);
        });
    } catch (_) {}
  }
</script>
<style lang="less">
  .grade-modal {
    .scrollbar {
      padding: 20px !important;
    }
  }
</style>
