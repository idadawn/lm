<template>
  <section class="common-pane condition-pane">
    <ScrollContainer class="config-content">
      <a-row class="condition-list condition-list-header">
        <a-col :span="8">条件字段</a-col>
        <a-col :span="4">比较</a-col>
        <a-col :span="8">数据值</a-col>
        <a-col :span="3">逻辑</a-col>
        <a-col :span="1"></a-col>
      </a-row>
      <a-row class="condition-list" v-for="(item, index) in formConf" :key="index">
        <a-col :span="8" class="!flex items-center">
          <jnpf-select v-model:value="item.fieldType" :options="conditionTypeOptions" @change="onFieldTypeChange(item)" class="w-85px" />
          <jnpf-select
            v-model:value="item.field"
            :options="usedFormItems"
            allowClear
            showSearch
            :fieldNames="{ options: 'options1' }"
            class="flex-1"
            @change="(val, data) => onFieldChange(item, val, data, index)"
            v-if="item.fieldType === 1" />
          <a-button @click="editFormula(item, index)" class="flex-1" v-if="item.fieldType === 3">公式编辑</a-button>
        </a-col>
        <a-col :span="4">
          <jnpf-select class="w-full" v-model:value="item.symbol" :options="symbolOptions" @change="(val, data) => onSymbolChange(item, val, data)" />
        </a-col>
        <a-col :span="8" class="!flex items-center">
          <jnpf-select v-model:value="item.fieldValueType" :options="conditionTypeOptions1" class="w-85px" />
          <jnpf-select
            v-model:value="item.fieldValue"
            :options="usedFormItems"
            allowClear
            showSearch
            :fieldNames="{ options: 'options1' }"
            class="flex-1"
            @change="(val, data) => onFieldValueChange(item, val, data)"
            v-if="item.fieldValueType === 1" />
          <div class="flex-1 w-150px" v-if="item.fieldValueType === 2">
            <template v-if="item.jnpfKey === 'inputNumber'">
              <a-input-number v-model:value="item.fieldValue" placeholder="请输入" :precision="item.precision" />
            </template>
            <template v-else-if="item.jnpfKey === 'calculate'">
              <a-input-number v-model:value="item.fieldValue" placeholder="请输入" :precision="2" />
            </template>
            <template v-else-if="['rate', 'slider'].includes(item.jnpfKey)">
              <a-input-number v-model:value="item.fieldValue" placeholder="请输入" />
            </template>
            <template v-else-if="item.jnpfKey === 'switch'">
              <jnpf-switch v-model:value="item.fieldValue" />
            </template>
            <template v-else-if="item.jnpfKey === 'timePicker'">
              <jnpf-time-picker v-model:value="item.fieldValue" :format="item.format || 'HH:mm:ss'" allowClear />
            </template>
            <template v-else-if="['datePicker', 'createTime', 'modifyTime'].includes(item.jnpfKey)">
              <jnpf-date-picker
                v-model:value="item.fieldValue"
                :format="item.format || 'YYYY-MM-DD HH:mm:ss'"
                @change="onConditionDateChange($event, item)"
                allowClear />
            </template>
            <template v-else-if="['organizeSelect', 'currOrganize'].includes(item.jnpfKey)">
              <jnpf-organize-select v-model:value="item.fieldValue" allowClear @change="(val, data) => onConditionListChange(item, val, data)" />
            </template>
            <template v-else-if="['depSelect'].includes(item.jnpfKey)">
              <jnpf-dep-select v-model:value="item.fieldValue" allowClear @change="(val, data) => onConditionObjChange(item, val, data)" />
            </template>
            <template v-else-if="item.jnpfKey === 'roleSelect'">
              <jnpf-role-select v-model:value="item.fieldValue" allowClear @change="(val, data) => onConditionObjChange(item, val, data)" />
            </template>
            <template v-else-if="item.jnpfKey === 'groupSelect'">
              <jnpf-group-select v-model:value="item.fieldValue" allowClear @change="(val, data) => onConditionObjChange(item, val, data)" />
            </template>
            <template v-else-if="['posSelect', 'currPosition'].includes(item.jnpfKey)">
              <jnpf-pos-select v-model:value="item.fieldValue" allowClear @change="(val, data) => onConditionObjChange(item, val, data)" />
            </template>
            <template v-else-if="['userSelect', 'createUser', 'modifyUser'].includes(item.jnpfKey)">
              <jnpf-user-select v-model:value="item.fieldValue" hasSys allowClear @change="(val, data) => onConditionObjChange(item, val, data)" />
            </template>
            <template v-else-if="['usersSelect'].includes(item.jnpfKey)">
              <jnpf-users-select v-model:value="item.fieldValue" allowClear @change="(val, data) => onConditionObjChange(item, val, data)" />
            </template>
            <template v-else-if="item.jnpfKey === 'areaSelect'">
              <jnpf-area-select
                v-model:value="item.fieldValue"
                :level="item.level"
                allowClear
                @change="(val, data) => onConditionListChange(item, val, data)" />
            </template>
            <template v-else>
              <a-input v-model:value="item.fieldValue" placeholder="请输入" allowClear />
            </template>
          </div>
        </a-col>
        <a-col :span="3">
          <jnpf-select class="w-full" v-model:value="item.logic" :options="logicOptions" @change="(val, data) => onLogicChange(item, val, data)" />
        </a-col>
        <a-col :span="1" class="text-center">
          <i class="icon-ym icon-ym-btn-clearn" @click="delCondition(index)" />
        </a-col>
      </a-row>
      <a-button preIcon="icon-ym icon-ym-btn-add" @click="addCondition" class="mt-10px ml-4px">添加条件</a-button>
    </ScrollContainer>
    <FormulaModal @register="registerFormulaModal" @confirm="updateFormula" />
  </section>
</template>
<script lang="ts" setup>
  import { ref } from 'vue';
  import { ScrollContainer } from '/@/components/Container';
  import { useModal } from '/@/components/Modal';
  import FormulaModal from './FormulaModal.vue';
  import { formatToDateTime } from '/@/utils/dateUtil';
  import { cloneDeep } from 'lodash-es';

  const props = defineProps([
    'value' /* 当前节点数据 */,
    'processData' /* 整个节点数据 */,
    'formInfo',
    'formConf',
    'formFieldList',
    'printTplOptions',
    'flowType',
    'formFieldsOptions',
    'usedFormItems',
    'funcOptions',
    'funcRequiredOptions',
    'initFormOperates',
    'updateAllNodeFormOperates',
    'getFormFieldList',
  ]);
  defineOptions({ name: 'conditionNode', inheritAttrs: false });
  defineExpose({ getContent });
  const [registerFormulaModal, { openModal: openFormulaModal }] = useModal();
  const conditionTypeOptions = [
    { id: 1, fullName: '字段' },
    { id: 3, fullName: '公式' },
  ];
  const conditionTypeOptions1 = [
    { id: 1, fullName: '字段' },
    { id: 2, fullName: '自定义' },
  ];
  const symbolOptions = [
    { id: '>=', fullName: '大于等于' },
    { id: '>', fullName: '大于' },
    { id: '==', fullName: '等于' },
    { id: '<=', fullName: '小于等于' },
    { id: '<', fullName: '小于' },
    { id: '<>', fullName: '不等于' },
    { id: 'like', fullName: '包含' },
    { id: 'notLike', fullName: '不包含' },
  ];
  const logicOptions = [
    { id: '&&', fullName: '并且' },
    { id: '||', fullName: '或者' },
  ];
  const emptyItem = {
    fieldName: '',
    symbolName: '',
    fieldValue: undefined,
    fieldType: 1,
    fieldValueType: 2,
    fieldLabel: '',
    fieldValueJnpfKey: '',
    logicName: '并且',
    field: '',
    symbol: '',
    logic: '&&',
    jnpfKey: '',
  };
  const activeIndex = ref(0);

  function addCondition() {
    props.formConf.push(cloneDeep(emptyItem));
  }
  function delCondition(index) {
    props.formConf.splice(index, 1);
  }
  function editFormula(item, index) {
    activeIndex.value = index;
    openFormulaModal(true, { value: item.field, fieldsOptions: props.formFieldsOptions });
  }
  function updateFormula(formula) {
    props.formConf[activeIndex.value].field = formula;
    props.formConf[activeIndex.value].fieldName = formula;
  }
  function onFieldTypeChange(item) {
    item.field = '';
    handleNull(item);
  }
  function onFieldChange(item, val, data, index) {
    if (!val) return handleNull(item);
    item.fieldName = data.__config__.label;
    item.jnpfKey = data.__config__.jnpfKey;
    const newItem = cloneDeep(emptyItem);
    for (let key of Object.keys(newItem)) {
      newItem[key] = item[key];
    }
    item = { ...newItem, ...data };
    if (item.fieldValueType == 2) {
      item.fieldValue = undefined;
      item.fieldLabel = '';
      item.fieldValueJnpfKey = '';
    }
    props.formConf[index] = item;
  }
  function handleNull(item) {
    item.fieldName = '';
    item.jnpfKey = '';
    if (item.fieldValueType == 2) {
      item.fieldValue = undefined;
      item.fieldLabel = '';
      item.fieldValueJnpfKey = '';
    }
  }
  function onSymbolChange(item, val, data) {
    item.symbolName = val ? data.fullName : '';
  }
  function onFieldValueChange(item, val, data) {
    item.fieldLabel = val ? data.fullName : '';
    item.fieldValueJnpfKey = val ? data.__config__.jnpfKey : '';
  }
  function onLogicChange(item, val, data) {
    item.logicName = val ? data.fullName : '';
  }
  function onConditionDateChange(val, item) {
    if (!val) return (item.fieldLabel = '');
    const format = item.format || 'YYYY-MM-DD HH:mm:ss';
    item.fieldLabel = formatToDateTime(val, format);
  }
  function onConditionListChange(item, val, data) {
    if (!val) return (item.fieldLabel = '');
    const labelList = data.map(o => o.fullName);
    item.fieldLabel = labelList.join('/');
  }
  function onConditionObjChange(item, val, data) {
    if (!val) return (item.fieldLabel = '');
    item.fieldLabel = data.fullName || '';
  }
  function getContent() {
    let content = '';
    for (let i = 0; i < props.formConf.length; i++) {
      const e = props.formConf[i];
      const text = `[${e.fieldName} ${e.symbolName} ${e.fieldLabel ? e.fieldLabel : e.fieldValue || e.fieldValue === 0 ? e.fieldValue : ''}]`;
      const logicName = ` ${i + 1 == props.formConf.length ? '' : e.logicName}`;
      content += text + ' ' + logicName + '\n';
    }
    return content;
  }
</script>
