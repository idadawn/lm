<template>
  <a-form-item label="可选范围" v-if="['depSelect', 'posSelect', 'usersSelect'].includes(jnpfKey)">
    <jnpf-select v-model:value="activeData.selectType" :options="selectTypeOptions" @change="onTypeChange" />
    <template v-if="activeData.selectType === 'custom'">
      <template v-if="jnpfKey === 'depSelect'">
        <JnpfDepSelect v-model:value="activeData.ableDepIds" modalTitle="添加部门" buttonType="button" multiple class="!mt-10px" @change="onChange" />
      </template>
      <template v-if="jnpfKey === 'posSelect'">
        <JnpfDepSelect v-model:value="activeData.ableDepIds" modalTitle="添加部门" buttonType="button" multiple class="!mt-10px" @change="onChange" />
        <JnpfPosSelect v-model:value="activeData.ablePosIds" modalTitle="添加岗位" buttonType="button" multiple class="!mt-10px" @change="onChange" />
      </template>
      <template v-if="jnpfKey === 'usersSelect'">
        <JnpfUsersSelect v-model:value="activeData.ableIds" modalTitle="添加用户" buttonType="button" multiple class="!mt-10px" @change="onChange" />
      </template>
    </template>
  </a-form-item>
  <a-form-item label="可选范围" v-if="jnpfKey === 'userSelect'">
    <jnpf-select v-model:value="activeData.selectType" :options="userSelectTypeOptions" @change="onTypeChange" />
    <template v-if="activeData.selectType === 'custom'">
      <JnpfDepSelect v-model:value="activeData.ableDepIds" modalTitle="添加部门" buttonType="button" multiple class="!mt-10px" @change="onChange" />
      <JnpfPosSelect v-model:value="activeData.ablePosIds" modalTitle="添加岗位" buttonType="button" multiple class="!mt-10px" @change="onChange" />
      <JnpfUserSelect v-model:value="activeData.ableUserIds" modalTitle="添加用户" buttonType="button" multiple class="!mt-10px" @change="onChange" />
      <JnpfRoleSelect v-model:value="activeData.ableRoleIds" modalTitle="添加角色" buttonType="button" multiple class="!mt-10px" @change="onChange" />
      <JnpfGroupSelect v-model:value="activeData.ableGroupIds" modalTitle="添加分组" buttonType="button" multiple class="!mt-10px" @change="onChange" />
    </template>
    <template v-if="['dep', 'pos', 'role', 'group'].includes(activeData.selectType)">
      <jnpf-radio v-model:value="activeData.relationField" :options="formFieldsOptions" class="relation-radio" />
    </template>
  </a-form-item>
  <a-form-item label="默认值">
    <JnpfOrganizeSelect
      v-model:value="activeData.__config__.defaultValue"
      :multiple="activeData.multiple"
      :disabled="activeData.__config__.defaultCurrent"
      v-if="jnpfKey === 'organizeSelect'" />
    <JnpfRoleSelect
      v-model:value="activeData.__config__.defaultValue"
      :multiple="activeData.multiple"
      :disabled="activeData.__config__.defaultCurrent"
      v-if="jnpfKey === 'roleSelect'" />
    <JnpfGroupSelect v-model:value="activeData.__config__.defaultValue" :multiple="activeData.multiple" v-if="jnpfKey === 'groupSelect'" />
    <JnpfDepSelect
      v-model:value="activeData.__config__.defaultValue"
      :multiple="activeData.multiple"
      :selectType="activeData.selectType"
      :ableDepIds="activeData.ableDepIds"
      :disabled="activeData.__config__.defaultCurrent"
      v-if="jnpfKey === 'depSelect'" />
    <JnpfPosSelect
      v-model:value="activeData.__config__.defaultValue"
      :multiple="activeData.multiple"
      :selectType="activeData.selectType"
      :ableDepIds="activeData.ableDepIds"
      :ablePosIds="activeData.ablePosIds"
      :disabled="activeData.__config__.defaultCurrent"
      v-if="jnpfKey === 'posSelect'" />
    <JnpfUserSelect
      v-model:value="activeData.__config__.defaultValue"
      :multiple="activeData.multiple"
      :selectType="activeData.selectType"
      :ableDepIds="activeData.ableDepIds"
      :ablePosIds="activeData.ablePosIds"
      :ableUserIds="activeData.ableUserIds"
      :ableRoleIds="activeData.ableRoleIds"
      :ableGroupIds="activeData.ableGroupIds"
      :disabled="activeData.__config__.defaultCurrent"
      v-if="jnpfKey === 'userSelect'" />
    <JnpfUsersSelect
      v-model:value="activeData.__config__.defaultValue"
      :multiple="activeData.multiple"
      :selectType="activeData.selectType"
      :ableIds="activeData.ableIds"
      :disabled="activeData.__config__.defaultCurrent"
      v-if="jnpfKey === 'usersSelect'" />
  </a-form-item>
  <a-form-item
    v-if="
      activeData.__config__.jnpfKey === 'userSelect' || activeData.__config__.jnpfKey === 'depSelect' || activeData.__config__.jnpfKey === 'organizeSelect'
    ">
    <a-checkbox
      v-model:checked="activeData.__config__.defaultCurrent"
      class="float-right !-mr-8px"
      :disabled="activeData.selectType !== 'all'"
      @change="onChange">
      <span v-if="activeData.__config__.jnpfKey === 'userSelect'">默认为当前登录用户</span>
      <span v-else-if="activeData.__config__.jnpfKey === 'depSelect'">默认为当前登录部门</span>
      <span v-else>默认为当前登录组织</span>
    </a-checkbox>
  </a-form-item>
  <a-form-item label="能否清空">
    <a-switch v-model:checked="activeData.clearable" />
  </a-form-item>
  <a-form-item label="能否多选">
    <a-switch v-model:checked="activeData.multiple" @change="onChange" />
  </a-form-item>
</template>
<script lang="ts" setup>
  import { computed, unref } from 'vue';
  defineOptions({ inheritAttrs: false });
  const props = defineProps(['activeData', 'drawingList']);

  const selectTypeOptions = [
    { id: 'all', fullName: '全部' },
    { id: 'custom', fullName: '自定义' },
  ];
  const userSelectTypeOptions = [
    ...selectTypeOptions,
    { id: 'dep', fullName: '部门组件联动' },
    { id: 'pos', fullName: '岗位组件联动' },
    { id: 'role', fullName: '角色组件联动' },
    { id: 'group', fullName: '分组组件联动' },
  ];

  const jnpfKey = computed(() => unref(props.activeData).__config__?.jnpfKey);
  const formFieldsOptions = computed(() => {
    if (props.activeData.selectType === 'all' || props.activeData.selectType === 'custom') return [];
    let list: any[] = [];
    const loop = (data, parent?) => {
      if (!data) return;
      if (data.__config__ && isIncludesTable(data) && data.__config__.children && Array.isArray(data.__config__.children)) {
        loop(data.__config__.children, data);
      }
      if (Array.isArray(data)) data.forEach(d => loop(d, parent));
      if (data.__vModel__ && data.__config__.jnpfKey === `${props.activeData.selectType}Select` && data.__vModel__ !== props.activeData.__vModel__) {
        const isTableChild = parent && parent.__config__ && parent.__config__.jnpfKey === 'table';
        list.push({
          id: isTableChild ? parent.__vModel__ + '-' + data.__vModel__ : data.__vModel__,
          fullName: isTableChild ? parent.__config__.label + '-' + data.__config__.label : data.__config__.label,
          ...data,
        });
      }
    };
    loop(props.drawingList);
    return list;
  });

  function isIncludesTable(data) {
    if ((!data.__config__.layout || data.__config__.layout === 'rowFormItem') && data.__config__.jnpfKey !== 'table') return true;
    if (props.activeData.__config__.isSubTable) return props.activeData.__config__.parentVModel === data.__vModel__;
    return data.__config__.jnpfKey !== 'table';
  }
  function onTypeChange() {
    onChange();
    if (props.activeData.__config__.jnpfKey === 'usersSelect') {
      return (props.activeData.ableIds = []);
    }
    props.activeData.ableDepIds = [];
    if (props.activeData.__config__.jnpfKey === 'posSelect') {
      props.activeData.ablePosIds = [];
    }
    if (props.activeData.__config__.jnpfKey === 'userSelect') {
      props.activeData.ablePosIds = [];
      props.activeData.ableUserIds = [];
      props.activeData.ableRoleIds = [];
      props.activeData.ableGroupIds = [];
      props.activeData.relationField = '';
    }
    props.activeData.__config__.defaultCurrent = false;
  }
  function onChange() {
    props.activeData.__config__.defaultValue = props.activeData.multiple || unref(jnpfKey) === 'organizeSelect' ? [] : '';
  }
</script>
<style lang="less" scoped>
  .relation-radio {
    margin-top: 10px;
    :deep(.ant-radio-wrapper) {
      display: block;
    }
  }
</style>
