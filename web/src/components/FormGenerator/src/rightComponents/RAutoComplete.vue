<template>
  <a-form-item label="远端数据">
    <interface-modal :value="activeData.interfaceId" :title="activeData.interfaceName" popupTitle="远端数据" @change="onInterfaceIdChange" />
  </a-form-item>
  <a-form-item label="显示字段">
    <a-input v-model:value="activeData.relationField" placeholder="请输入显示字段" />
  </a-form-item>
  <a-table :data-source="activeData.templateJson" :columns="columns" size="small" :pagination="false" v-if="activeData.templateJson.length" class="mb-18px">
    <template #bodyCell="{ column, record }">
      <template v-if="column.key === 'field'">
        <span class="required-sign">{{ record.required ? '*' : '' }}</span>
        {{ record.field }}{{ record.fieldName ? '(' + record.fieldName + ')' : '' }}
      </template>
      <template v-if="column.key === 'relationField'">
        <jnpf-select
          v-model:value="record.relationField"
          placeholder="请选择表单字段"
          :options="getFormFieldsOptions"
          allowClear
          showSearch
          :fieldNames="{ options: 'options1' }"
          class="!w-135px"
          @change="onRelationFieldChange($event, record)" />
      </template>
    </template>
  </a-table>
  <a-form-item>
    <template #label>显示条数<BasicHelp text="最大值只能设置为50" /></template>
    <a-input-number placeholder="请输入显示条数" v-model:value="activeData.total" :min="1" :max="50" :precision="0" />
  </a-form-item>
  <a-form-item label="能否清空">
    <a-switch v-model:checked="activeData.clearable" />
  </a-form-item>
  <a-form-item label="能否多选" v-if="jnpfKey === 'popupTableSelect'">
    <a-switch v-model:checked="activeData.multiple" @change="onMultipleChange" />
  </a-form-item>
</template>
<script lang="ts" setup>
  import { computed, unref } from 'vue';
  import { useDynamic } from '../hooks/useDynamic';
  import { InterfaceModal } from '/@/components/CommonModal';

  defineOptions({ inheritAttrs: false });
  const props = defineProps(['activeData', 'dicOptions']);
  const { formFieldsOptions, onRelationFieldChange, onMultipleChange } = useDynamic(props.activeData);
  const columns = [
    { width: 50, title: '序号', align: 'center', customRender: ({ index }) => index + 1 },
    { title: '参数名称', dataIndex: 'field', key: 'field', width: 135 },
    { title: '表单字段', dataIndex: 'relationField', key: 'relationField', width: 135 },
  ];

  const jnpfKey = computed(() => unref(props.activeData).__config__?.jnpfKey);
  const getFormFieldsOptions = computed(() => [{ id: '@keyword', fullName: '@keyword' }, ...unref(formFieldsOptions)]);

  function onInterfaceIdChange(val, row) {
    if (!val) {
      props.activeData.interfaceId = '';
      props.activeData.interfaceName = '';
      props.activeData.templateJson = [];
      props.activeData.__config__.defaultValue = '';
      return;
    }
    if (props.activeData.__config__.propsUrl === val) return;
    props.activeData.interfaceId = val;
    props.activeData.interfaceName = row.fullName;
    props.activeData.templateJson = row.requestParameters ? JSON.parse(row.requestParameters) : [];
    props.activeData.__config__.defaultValue = '';
  }
</script>
