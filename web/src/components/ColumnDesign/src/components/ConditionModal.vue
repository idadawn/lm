<template>
  <BasicModal v-bind="$attrs" @register="registerModal" title="过滤规则配置" :width="800" @ok="handleSubmit" destroyOnClose class="jnpf-super-query-modal">
    <div class="super-query-main condition-main">
      <a-row class="condition-list condition-list-header">
        <a-col :span="4">逻辑</a-col>
        <a-col :span="6">条件字段</a-col>
        <a-col :span="4">条件符号</a-col>
        <a-col :span="9">数据值</a-col>
        <a-col :span="1"></a-col>
      </a-row>
      <a-row class="condition-list" :key="index" :gutter="8" v-for="(item, index) in conditionList">
        <a-col :span="4">
          <jnpf-select class="w-full" v-model:value="item.logic" :options="logicOptions" />
        </a-col>
        <a-col :span="6">
          <jnpf-select
            v-model:value="item.field"
            :options="fieldOptions"
            showSearch
            allowClear
            :fieldNames="{ options: 'options1' }"
            @change="(val, data) => onFieldChange(val, data, item, index)" />
        </a-col>
        <a-col :span="4">
          <jnpf-select
            v-model:value="item.symbol"
            :options="getSymbolOptions(item.jnpfKey)"
            :dropdownMatchSelectWidth="false"
            @change="(val, data) => onSymbolChange(val, data, item)" />
        </a-col>
        <a-col :span="9">
          <template v-if="item.jnpfKey === 'inputNumber'">
            <jnpf-number-range v-model:value="item.fieldValue" :precision="item.precision" :disabled="item.disabled" v-if="item.symbol == 'between'" />
            <jnpf-input-number v-model:value="item.fieldValue" placeholder="请输入" :precision="item.precision" :disabled="item.disabled" v-else />
          </template>
          <template v-else-if="item.jnpfKey === 'calculate'">
            <jnpf-number-range v-model:value="item.fieldValue" :disabled="item.disabled" :precision="item.precision || 0" v-if="item.symbol == 'between'" />
            <jnpf-input-number v-model:value="item.fieldValue" placeholder="请输入" :precision="item.precision || 0" :disabled="item.disabled" v-else />
          </template>
          <template v-else-if="['rate', 'slider'].includes(item.jnpfKey)">
            <jnpf-number-range v-model:value="item.fieldValue" :disabled="item.disabled" :precision="0" v-if="item.symbol == 'between'" />
            <a-input-number v-model:value="item.fieldValue" placeholder="请输入" :precision="0" :disabled="item.disabled" v-else />
          </template>
          <template v-else-if="item.jnpfKey === 'switch'">
            <jnpf-switch v-model:value="item.fieldValue" :disabled="item.disabled" />
          </template>
          <template v-else-if="item.jnpfKey === 'timePicker'">
            <jnpf-time-range v-model:value="item.fieldValue" :format="item.format" allowClear :disabled="item.disabled" v-if="item.symbol == 'between'" />
            <jnpf-time-picker v-model:value="item.fieldValue" :format="item.format" allowClear :disabled="item.disabled" v-else />
          </template>
          <template v-else-if="['datePicker', 'createTime', 'modifyTime'].includes(item.jnpfKey)">
            <jnpf-date-range
              v-model:value="item.fieldValue"
              :format="item.format || 'YYYY-MM-DD HH:mm:ss'"
              allowClear
              :disabled="item.disabled"
              v-if="item.symbol == 'between'" />
            <jnpf-date-picker v-model:value="item.fieldValue" :format="item.format || 'YYYY-MM-DD HH:mm:ss'" allowClear :disabled="item.disabled" v-else />
          </template>
          <template v-else-if="['organizeSelect', 'currOrganize'].includes(item.jnpfKey)">
            <jnpf-organize-select v-model:value="item.fieldValue" allowClear :multiple="item.multiple" :disabled="item.disabled" />
          </template>
          <template v-else-if="['depSelect'].includes(item.jnpfKey)">
            <jnpf-dep-select
              v-model:value="item.fieldValue"
              allowClear
              :selectType="item.selectType"
              :ableDepIds="item.ableDepIds"
              :multiple="item.multiple"
              :disabled="item.disabled" />
          </template>
          <template v-else-if="item.jnpfKey === 'roleSelect'">
            <jnpf-role-select v-model:value="item.fieldValue" allowClear :multiple="item.multiple" :disabled="item.disabled" />
          </template>
          <template v-else-if="item.jnpfKey === 'groupSelect'">
            <jnpf-group-select v-model:value="item.fieldValue" allowClear :multiple="item.multiple" :disabled="item.disabled" />
          </template>
          <template v-else-if="item.jnpfKey === 'posSelect'">
            <jnpf-pos-select
              v-model:value="item.fieldValue"
              allowClear
              :selectType="item.selectType"
              :ableDepIds="item.ableDepIds"
              :ablePosIds="item.ablePosIds"
              :multiple="item.multiple"
              :disabled="item.disabled" />
          </template>
          <template v-else-if="item.jnpfKey === 'currPosition'">
            <jnpf-pos-select v-model:value="item.fieldValue" allowClear :multiple="item.multiple" :disabled="item.disabled" />
          </template>
          <template v-else-if="['createUser', 'modifyUser'].includes(item.jnpfKey)">
            <jnpf-user-select v-model:value="item.fieldValue" allowClear :multiple="item.multiple" :disabled="item.disabled" />
          </template>
          <template v-else-if="['userSelect'].includes(item.jnpfKey)">
            <jnpf-user-select
              v-model:value="item.fieldValue"
              allowClear
              :selectType="item.selectType != 'all' || item.selectType != 'custom' ? 'all' : item.selectType"
              :ableDepIds="item.ableDepIds"
              :ablePosIds="item.ablePosIds"
              :ableUserIds="item.ableUserIds"
              :ableRoleIds="item.ableRoleIds"
              :ableGroupIds="item.ableGroupIds"
              :multiple="item.multiple"
              :disabled="item.disabled" />
          </template>
          <template v-else-if="['usersSelect'].includes(item.jnpfKey)">
            <jnpf-users-select
              v-model:value="item.fieldValue"
              allowClear
              :selectType="item.selectType"
              :ableIds="item.ableIds"
              :multiple="item.multiple"
              :disabled="item.disabled" />
          </template>
          <template v-else-if="item.jnpfKey === 'areaSelect'">
            <jnpf-area-select
              v-model:value="item.fieldValue"
              :level="item.level"
              allowClear
              :multiple="item.multiple"
              :disabled="item.disabled"
              :key="item.cellKey" />
          </template>
          <template v-else-if="['select', 'radio', 'checkbox'].includes(item.jnpfKey)">
            <jnpf-select
              v-model:value="item.fieldValue"
              placeholder="请选择"
              showSearch
              allowClear
              :options="item.options"
              :fieldNames="item.props"
              :multiple="item.multiple"
              :disabled="item.disabled" />
          </template>
          <template v-else-if="item.jnpfKey === 'cascader'">
            <jnpf-cascader
              v-model:value="item.fieldValue"
              :options="item.options"
              :fieldNames="item.props"
              :showAllLevels="item.showAllLevels"
              showSearch
              allowClear
              placeholder="请选择"
              :multiple="item.multiple"
              :disabled="item.disabled" />
          </template>
          <template v-else-if="item.jnpfKey === 'treeSelect'">
            <jnpf-tree-select
              v-model:value="item.fieldValue"
              :options="item.options"
              :fieldNames="item.props"
              showSearch
              allowClear
              placeholder="请选择"
              :multiple="item.multiple"
              :disabled="item.disabled" />
          </template>
          <template v-else-if="item.jnpfKey === 'relationForm'">
            <jnpf-relation-form
              v-model:value="item.fieldValue"
              placeholder="请选择"
              :modelId="item.modelId"
              allowClear
              :columnOptions="item.columnOptions"
              :relationField="item.relationField"
              :hasPage="item.hasPage"
              :pageSize="item.pageSize"
              :popupType="item.popupType"
              :popupTitle="item.popupTitle"
              :popupWidth="item.popupWidth"
              :disabled="item.disabled" />
          </template>
          <template v-else-if="item.jnpfKey === 'popupSelect' || item.jnpfKey === 'popupTableSelect'">
            <jnpf-popup-select
              v-model:value="item.fieldValue"
              placeholder="请选择"
              :interfaceId="item.interfaceId"
              allowClear
              :multiple="item.multiple"
              :columnOptions="item.columnOptions"
              :propsValue="item.propsValue"
              :templateJson="item.templateJson"
              :relationField="item.relationField"
              :hasPage="item.hasPage"
              :pageSize="item.pageSize"
              :popupType="item.popupType"
              :popupTitle="item.popupTitle"
              :popupWidth="item.popupWidth"
              :disabled="item.disabled" />
          </template>
          <template v-else-if="item.jnpfKey === 'autoComplete'">
            <jnpf-auto-complete
              v-model:value="item.fieldValue"
              placeholder="请选择"
              allowClear
              :interfaceId="item.interfaceId"
              :relationField="item.relationField"
              :templateJson="item.templateJson"
              :total="item.total"
              :disabled="item.disabled" />
          </template>
          <template v-else>
            <a-input v-model:value="item.fieldValue" placeholder="请输入" allowClear :disabled="item.disabled" />
          </template>
        </a-col>
        <a-col :span="1" class="text-center">
          <i class="icon-ym icon-ym-btn-clearn" @click="delCondition(index)" />
        </a-col>
      </a-row>
      <a-button preIcon="icon-ym icon-ym-btn-add" @click="addCondition" class="mt-10px ml-4px">添加条件</a-button>
    </div>
  </BasicModal>
</template>
<script lang="ts" setup>
  import { reactive, toRefs } from 'vue';
  import { BasicModal, useModalInner } from '/@/components/Modal';
  import { getDictionaryDataSelector } from '/@/api/systemData/dictionary';
  import { getDataInterfaceRes } from '/@/api/systemData/dataInterface';
  import { useMessage } from '/@/hooks/web/useMessage';
  import { cloneDeep } from 'lodash-es';
  import { dyOptionsList } from '/@/components/FormGenerator/src/helper/config';
  import { JnpfRelationForm } from '/@/components/Jnpf';
  import { isEmpty } from '/@/utils/is';

  interface State {
    conditionList: any[];
    fieldOptions: any[];
  }
  const emit = defineEmits(['register', 'confirm']);
  const [registerModal, { closeModal, changeLoading }] = useModalInner(init);

  const { createMessage } = useMessage();
  const notSupportList = [
    'treeSelect',
    'relationForm',
    'relationFormAttr',
    'popupSelect',
    'popupAttr',
    'popupTableSelect',
    'switch',
    'uploadFile',
    'uploadImg',
    'colorPicker',
    'rate',
    'slider',
    'editor',
    'link',
    'button',
    'text',
    'alert',
    'usersSelect',
    'table',
  ];
  const emptyItem = { fieldValue: '', field: '', symbol: '', logic: '&&', jnpfKey: '', cellKey: +new Date() };
  const logicOptions = [
    { id: '&&', fullName: '并且' },
    { id: '||', fullName: '或者' },
  ];
  const baseSymbolOptions = [
    { id: '==', fullName: '等于' },
    { id: '<>', fullName: '不等于' },
    { id: 'like', fullName: '包含' },
    { id: 'notLike', fullName: '不包含' },
    { id: 'null', fullName: '为空' },
    { id: 'notNull', fullName: '不为空' },
  ];
  const rangeSymbolOptions = [
    { id: '>=', fullName: '大于等于' },
    { id: '>', fullName: '大于' },
    { id: '==', fullName: '等于' },
    { id: '<=', fullName: '小于等于' },
    { id: '<', fullName: '小于' },
    { id: '<>', fullName: '不等于' },
    { id: 'between', fullName: '介于' },
    { id: 'null', fullName: '为空' },
    { id: 'notNull', fullName: '不为空' },
  ];
  const selectSymbolOptions = [
    { id: 'in', fullName: '包含任意一个' },
    { id: 'notIn', fullName: '不包含任意一个' },
    { id: 'null', fullName: '为空' },
    { id: 'notNull', fullName: '不为空' },
  ];
  const useRangeSymbolList = ['calculate', 'inputNumber', 'datePicker', 'timePicker', 'createTime', 'modifyTime'];
  const useSelectSymbolList = [
    'radio',
    'checkbox',
    'select',
    'treeSelect',
    'cascader',
    'areaSelect',
    'organizeSelect',
    'depSelect',
    'posSelect',
    'userSelect',
    'usersSelect',
    'roleSelect',
    'groupSelect',
    'createUser',
    'modifyUser',
    'currOrganize',
    'currPosition',
  ];
  const state = reactive<State>({
    conditionList: [],
    fieldOptions: [],
  });
  const { conditionList, fieldOptions } = toRefs(state);

  function init(data) {
    changeLoading(true);
    state.conditionList = cloneDeep(data.ruleList || []);
    const fieldOptions = data.formFieldsOptions.filter(o => !notSupportList.includes(o.__config__.jnpfKey));
    state.fieldOptions = buildOptions(fieldOptions);
    if (!state.conditionList.length) addCondition();
    changeLoading(false);
  }
  function buildOptions(componentList) {
    componentList.forEach(cur => {
      cur.disabled = false;
      const config = cur.__config__;
      if (dyOptionsList.includes(config.jnpfKey)) {
        if (config.dataType === 'dictionary') {
          if (!config.dictionaryType) return;
          getDictionaryDataSelector(config.dictionaryType).then(res => {
            cur.options = res.data.list;
          });
        }
        if (config.dataType === 'dynamic') {
          if (!config.propsUrl) return;
          getDataInterfaceRes(config.propsUrl).then(res => {
            cur.options = Array.isArray(res.data) ? res.data : [];
          });
        }
      }
    });
    return componentList;
  }
  function onFieldChange(val, data, item, index) {
    item.cellKey = +new Date();
    item.fieldValue = undefined;
    const newItem = cloneDeep(emptyItem);
    for (let key of Object.keys(newItem)) {
      newItem[key] = item[key];
    }
    if (!val) {
      item.jnpfKey = '';
      item.symbol = undefined;
      item.disabled = false;
      return;
    }
    item = { ...newItem, ...data };
    const config = data.__config__;
    if (item.jnpfKey != config.jnpfKey) item.symbol = undefined;
    item.jnpfKey = data.__config__?.jnpfKey || '';
    item.disabled = ['null', 'notNull'].includes(item.symbol);
    item.multiple = ['in', 'notIn'].includes(item.symbol);
    state.conditionList[index] = item;
  }
  function onSymbolChange(val, _data, item) {
    item.fieldValue = undefined;
    item.disabled = ['null', 'notNull'].includes(val);
    item.multiple = ['in', 'notIn'].includes(val);
  }
  function addCondition() {
    state.conditionList.push(cloneDeep(emptyItem));
  }
  function delCondition(index) {
    state.conditionList.splice(index, 1);
  }
  function getSymbolOptions(jnpfKey) {
    if (useRangeSymbolList.includes(jnpfKey)) return rangeSymbolOptions;
    if (useSelectSymbolList.includes(jnpfKey)) return selectSymbolOptions;
    return baseSymbolOptions;
  }
  function exist() {
    let isOk = true;
    for (let i = 0; i < state.conditionList.length; i++) {
      const e = state.conditionList[i];
      if (!e.field) {
        createMessage.warning('条件字段不能为空');
        isOk = false;
        return;
      }
      if (!e.symbol) {
        createMessage.warning('条件符号不能为空');
        isOk = false;
        return;
      }
      if (!['null', 'notNull'].includes(e.symbol) && ((!e.fieldValue && e.fieldValue !== 0) || isEmpty(e.fieldValue))) {
        createMessage.warning('数据值不能为空');
        isOk = false;
        return;
      }
    }
    return isOk;
  }
  function handleSubmit() {
    if (!exist()) return;
    emit('confirm', cloneDeep(state.conditionList));
    closeModal();
  }
</script>
