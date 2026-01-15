<template>
  <div :class="prefixCls">
    <JnpfGroupTitle
      :content="config.__config__.label"
      :helpMessage="config.__config__.tipLabel"
      v-if="config.__config__.showTitle && config.__config__.label"
      :bordered="false" />
    <a-table
      :data-source="tableFormData"
      :columns="getColumns"
      size="small"
      :pagination="false"
      :scroll="{ x: 'max-content' }"
      :bordered="formStyle === 'word-form'">
      <template #headerCell="{ column }">
        <span class="required-sign" v-if="column.__config__ && column.__config__.required">*</span>{{ column.title }}
        <BasicHelp v-if="column.title && column.__config__ && column.__config__.tipLabel" :text="column.__config__.tipLabel" />
      </template>
      <template #bodyCell="{ column, index }">
        <template v-if="column.key === 'index'">{{ index + 1 }}</template>
        <template v-for="(item, cIndex) in tableData">
          <template v-if="column.key === item.__vModel__ && column.__config__.formId === item.__config__.formId">
            <div :key="item.__config__.formId">
              <JnpfRelationForm
                v-if="item.__config__.tag === 'JnpfRelationForm'"
                :rowIndex="index"
                :tableVModel="config.__vModel__"
                :componentVModel="item.__vModel__"
                v-model:value="tableFormData[index][cIndex].value"
                v-bind="getConfById(item.__config__.formId, index)"
                :formData="formData"
                @blur="onFormBlur(index, cIndex, $event)"
                @change="(val, data) => onFormDataChange(index, cIndex, item.__config__.tag, val, data)" />
              <JnpfPopupSelect
                v-else-if="item.__config__.tag === 'JnpfPopupSelect'"
                :rowIndex="index"
                :tableVModel="config.__vModel__"
                :componentVModel="item.__vModel__"
                v-model:value="tableFormData[index][cIndex].value"
                v-bind="getConfById(item.__config__.formId, index)"
                :formData="formData"
                @blur="onFormBlur(index, cIndex, $event)"
                @change="(val, data) => onFormDataChange(index, cIndex, item.__config__.tag, val, data)" />
              <component
                v-else
                :is="item.__config__.tag"
                :rowIndex="index"
                :tableVModel="config.__vModel__"
                :componentVModel="item.__vModel__"
                v-model:value="tableFormData[index][cIndex].value"
                v-bind="getConfById(item.__config__.formId, index)"
                :formData="formData"
                @blur="onFormBlur(index, cIndex, $event)"
                @change="(val, data) => onFormDataChange(index, cIndex, item.__config__.tag, val, data)" />
              <div class="error-tip required-sign" v-show="!tableFormData[index][cIndex].valid">{{ column.title }}不能为空</div>
              <div class="error-tip required-sign" v-show="tableFormData[index][cIndex].valid && !tableFormData[index][cIndex].regValid">
                {{ tableFormData[index][cIndex].regErrorText }}
              </div>
            </div>
          </template>
        </template>
        <template v-if="column.key === 'action'">
          <a-button class="action-btn" type="link" color="error" @click="removeRow(index)" size="small">删除</a-button>
        </template>
      </template>
      <template #summary v-if="tableFormData.length && config.showSummary">
        <a-table-summary fixed>
          <a-table-summary-row>
            <a-table-summary-cell :index="0">合计</a-table-summary-cell>
            <a-table-summary-cell v-for="(item, index) in getColumnSum" :key="index" :index="index + 1">{{ item }}</a-table-summary-cell>
            <a-table-summary-cell :index="getColumnSum.length + 1" v-if="!disabled"></a-table-summary-cell>
          </a-table-summary-row>
        </a-table-summary>
      </template>
    </a-table>
    <div class="table-add-action" @click="addItem" v-if="!disabled">
      <a-button type="link" preIcon="icon-ym icon-ym-btn-add">{{ config.actionText }}</a-button>
    </div>
    <SelectModal :config="config.addTableConf" :formData="formData" ref="selectModal" @select="addForSelect" />
  </div>
</template>

<script lang="ts" setup>
  import { computed, inject, reactive, unref, nextTick, toRefs, ref } from 'vue';
  import { useDesign } from '/@/hooks/web/useDesign';
  import { dyOptionsList } from '/@/components/FormGenerator/src/helper/config';
  import { getDictionaryDataSelector } from '/@/api/systemData/dictionary';
  import { getDataInterfaceRes } from '/@/api/systemData/dataInterface';
  import { Form } from 'ant-design-vue';
  import { getScriptFunc, getDateTimeUnit, thousandsFormat } from '/@/utils/jnpf';
  import { getRealProps } from '/@/components/FormGenerator/src/helper/transform';
  import SelectModal from '/@/components/CommonModal/src/SelectModal.vue';
  import { JnpfRelationForm } from '/@/components/Jnpf';
  import { useMessage } from '/@/hooks/web/useMessage';
  import { useI18n } from '/@/hooks/web/useI18n';
  import dayjs from 'dayjs';

  interface State {
    tableFormData: any[];
    tableData: any[];
    activeRowIndex: number;
    isAddRow: boolean;
  }

  defineOptions({ name: 'JnpfInputTable', inheritAttrs: false });
  const props = defineProps({
    config: {
      type: Object,
      default: () => {},
    },
    value: {
      type: Array,
      default: () => [],
    },
    formData: Object,
    relations: Object,
    vModel: String,
    disabled: {
      type: Boolean,
      default: false,
    },
  });
  const emit = defineEmits(['update:value', 'change']);
  const { createConfirm } = useMessage();
  const { t } = useI18n();
  const formItemContext = Form.useInjectFormItemContext();
  const parameter: any = inject('parameter');
  const formStyle: string | undefined = inject('formStyle');
  const { prefixCls } = useDesign('input-table');
  const state = reactive<State>({
    tableFormData: [],
    tableData: [],
    activeRowIndex: 0,
    isAddRow: true,
  });
  const { tableFormData, tableData } = toRefs(state);
  const selectModal = ref(null);

  defineExpose({
    handleRelationForParent,
    submit,
    setTableFormData,
    setTableShowOrHide,
    resetTable,
    reset,
  });

  const childRelations = computed(() => {
    let obj = {};
    for (let key in props.relations) {
      if (key.includes('-')) {
        let tableVModel = key.split('-')[0];
        if (tableVModel === props.vModel) {
          let newKey = key.split('-')[1];
          obj[newKey] = props.relations[key];
        }
      }
    }
    return obj;
  });
  const getColumns = computed(() => {
    const noColumn = { width: 50, title: '序号', dataIndex: 'index', key: 'index', align: 'center', customRender: ({ index }) => index + 1, fixed: 'left' };
    const actionColumn = { title: '操作', dataIndex: 'action', key: 'action', width: 50, fixed: 'right' };
    const list = state.tableData
      .map(o => ({
        ...o,
        dataIndex: o.__vModel__,
        key: o.__vModel__,
        width: o.__config__.columnWidth,
        title: o.__config__.label,
        customCell: () => ({ class: 'align-top' }),
      }))
      .filter(o => !o.__config__.noShow && (!o.__config__.visibility || (Array.isArray(o.__config__.visibility) && o.__config__.visibility.includes('pc'))));
    return props.disabled ? [noColumn, ...list] : [noColumn, ...list, actionColumn];
  });
  const getColumnSum = computed(() => {
    const sums: any[] = [];
    const isSummary = key => props.config.summaryField.includes(key);
    const useThousands = key => state.tableData.some(o => o.__vModel__ === key && o.thousands);
    const tableData = state.tableData.filter(o => !o.__config__.noShow && (!o.__config__.visibility || o.__config__.visibility.includes('pc')));
    tableData.forEach((column, index) => {
      let sumVal = state.tableFormData.reduce((sum, d) => sum + getCmpValOfRow(d, column.__vModel__), 0);
      if (!isSummary(column.__vModel__)) sumVal = '';
      sumVal = Number.isNaN(sumVal) ? '' : sumVal;
      const realVal = sumVal && !Number.isInteger(sumVal) ? Number(sumVal).toFixed(2) : sumVal;
      sums[index] = useThousands(column.__vModel__) ? thousandsFormat(realVal) : realVal;
    });
    return sums;
  });

  state.tableData = props.config.__config__.children;
  buildOptions();
  if (props.value && Array.isArray(props.value) && props.value.length) {
    props.value.forEach(t => addRow(t));
  } else {
    // addRow()
  }

  function buildOptions() {
    state.tableData.forEach(cur => {
      const config = cur.__config__;
      if (dyOptionsList.indexOf(config.jnpfKey) > -1) {
        if (config.dataType === 'dictionary') {
          if (!config.dictionaryType) return;
          getDictionaryDataSelector(config.dictionaryType).then(res => {
            cur.options = res.data.list;
          });
        }
        if (config.dataType === 'dynamic') {
          if (!config.propsUrl) return;
          let query = {
            paramList: config.templateJson ? getDefaultParamList(config.templateJson, props.formData) : [],
          };
          getDataInterfaceRes(config.propsUrl, query).then(res => {
            cur.options = Array.isArray(res.data) ? res.data : [];
          });
        }
      }
    });
  }
  function handleRelationForParent(e, defaultValue, notSetDefault) {
    if (!state.tableFormData.length) return;
    for (let i = 0; i < state.tableFormData.length; i++) {
      let row: any[] = state.tableFormData[i];
      for (let j = 0; j < row.length; j++) {
        let item = row[j];
        const vModel = item.jnpfKey === 'popupSelect' ? item.__vModel__.substring(0, item.__vModel__.indexOf('_jnpfRelation_')) : item.__vModel__;
        if (e.__vModel__ === vModel) {
          if (!notSetDefault) item.value = defaultValue;
          if (e.opType === 'setOptions') {
            item.config.options = [];
            const query = { paramList: getParamList(e.__config__.templateJson, props.formData, i) };
            getDataInterfaceRes(e.__config__.propsUrl, query).then(res => {
              const realData = res.data;
              item.config.options = Array.isArray(realData) ? realData : [];
            });
          }
          if (e.opType === 'setUserOptions') {
            const value = (props.formData as any)[e.relationField] || [];
            item.config.ableRelationIds = Array.isArray(value) ? value : [value];
          }
          if (e.opType === 'setStartTime') {
            const value = (props.formData as any)[e.__config__.startRelationField] || null;
            item.config.startTime = value;
          }
          if (e.opType === 'setEndTime') {
            const value = (props.formData as any)[e.__config__.endRelationField] || null;
            item.config.endTime = value;
          }
        }
      }
    }
    updateParentData();
  }
  function handleRelation(data, rowIndex) {
    const currRelations = unref(childRelations);
    for (let key in currRelations) {
      if (key === data.__vModel__) {
        for (let i = 0; i < currRelations[key].length; i++) {
          const e = currRelations[key][i];
          const config = e.__config__;
          const jnpfKey = config.jnpfKey;
          let defaultValue: any = null;
          if (
            ['checkbox', 'cascader', 'organizeSelect'].includes(jnpfKey) ||
            (['select', 'treeSelect', 'popupSelect', 'popupTableSelect', 'userSelect'].includes(jnpfKey) && e.multiple)
          ) {
            defaultValue = [];
          }
          let row: any[] = state.tableFormData[rowIndex];
          for (let j = 0; j < row.length; j++) {
            let item = row[j];
            const vModel = item.jnpfKey === 'popupSelect' ? item.__vModel__.substring(0, item.__vModel__.indexOf('_jnpfRelation_')) : item.__vModel__;
            if (e.__vModel__ === vModel) {
              if (e.opType === 'setOptions') {
                item.config.options = [];
                const query = { paramList: getParamList(e.__config__.templateJson, props.formData, rowIndex) };
                getDataInterfaceRes(e.__config__.propsUrl, query).then(res => {
                  const realData = res.data;
                  item.config.options = Array.isArray(realData) ? realData : [];
                });
              }
              if (e.opType === 'setUserOptions') {
                const value = getFieldVal(e.relationField, rowIndex) || [];
                item.config.ableRelationIds = Array.isArray(value) ? value : [value];
              }
              if (e.opType === 'setStartTime') {
                const value = getFieldVal(e.__config__.startRelationField, rowIndex) || null;
                item.config.startTime = value;
              }
              if (e.opType === 'setEndTime') {
                const value = getFieldVal(e.__config__.endRelationField, rowIndex) || null;
                item.config.endTime = value;
              }
              if (item.value !== defaultValue) {
                item.value = defaultValue;
                nextTick(() => handleRelation(item, rowIndex));
              }
            }
          }
        }
      }
    }
    updateParentData();
  }
  function buildRowAttr(rowIndex) {
    let row: any[] = state.tableFormData[rowIndex];
    for (let i = 0; i < row.length; i++) {
      const cur = row[i].config;
      const config = cur.__config__;
      if (dyOptionsList.indexOf(config.jnpfKey) > -1) {
        if (config.dataType === 'dynamic') {
          if (!config.propsUrl || !config.templateJson || !config.templateJson.length) return;
          let query = {
            paramList: config.templateJson ? getParamList(config.templateJson, props.formData, rowIndex) : [],
          };
          getDataInterfaceRes(config.propsUrl, query).then(res => {
            let realData = res.data;
            cur.options = Array.isArray(realData) ? realData : [];
          });
        }
      }
      if (config.jnpfKey === 'userSelect' && cur.relationField && cur.selectType !== 'all' && cur.selectType !== 'custom') {
        let value = getFieldVal(cur.relationField, rowIndex) || [];
        cur.ableRelationIds = Array.isArray(value) ? value : [value];
      }
      if (config.jnpfKey === 'datePicker' || config.jnpfKey === 'timePicker') {
        if (config.startTimeRule && config.startTimeType == 2 && config.startRelationField) {
          cur.startTime = getFieldVal(config.startRelationField, rowIndex) || null;
        }
        if (config.endTimeRule && config.endTimeType == 2 && config.endRelationField) {
          cur.endTime = getFieldVal(config.endRelationField, rowIndex) || null;
        }
      }
    }
  }
  function getParamList(templateJson, formData, index) {
    for (let i = 0; i < templateJson.length; i++) {
      if (templateJson[i].relationField) {
        if (templateJson[i].relationField.includes('-')) {
          let childVModel = templateJson[i].relationField.split('-')[1];
          let list = state.tableFormData[index].filter(o => o.__vModel__ === childVModel);
          if (!list.length) {
            templateJson[i].defaultValue = '';
          } else {
            let item = list[0];
            templateJson[i].defaultValue = item.value;
          }
        } else {
          templateJson[i].defaultValue = formData[templateJson[i].relationField] || '';
        }
      }
    }
    return templateJson;
  }
  function getDefaultParamList(templateJson, formData) {
    for (let i = 0; i < templateJson.length; i++) {
      if (templateJson[i].relationField) {
        if (templateJson[i].relationField.includes('-')) {
          let childVModel = templateJson[i].relationField.split('-')[1];
          let list = state.tableData.filter(o => o.__vModel__ === childVModel);
          templateJson[i].defaultValue = '';
          if (list.length) templateJson[i].defaultValue = list[0].__config__.defaultValue;
        } else {
          templateJson[i].defaultValue = formData[templateJson[i].relationField] || '';
        }
      }
    }
    return templateJson;
  }
  function getFieldVal(field, rowIndex) {
    let val = '';
    if (field.includes('-')) {
      let childVModel = field.split('-')[1];
      let list = state.tableFormData[rowIndex].filter(o => o.__vModel__ === childVModel);
      if (!list.length) {
        val = '';
      } else {
        let item = list[0];
        val = item.value;
      }
    } else {
      val = (props.formData as any)[field] || '';
    }
    return val;
  }
  function clearAddRowFlag() {
    nextTick(() => {
      state.isAddRow = false;
    });
  }
  function setTableFormData(prop, value) {
    let activeRow: any[] = state.tableFormData[state.activeRowIndex];
    for (let i = 0; i < activeRow.length; i++) {
      let vModel = activeRow[i].__vModel__;
      if (activeRow[i].__vModel__.indexOf('_jnpfRelation_') >= 0) {
        vModel = activeRow[i].__vModel__.substring(0, activeRow[i].__vModel__.indexOf('_jnpfRelation_'));
      }
      if (vModel === prop) {
        activeRow[i].value = value;
        break;
      }
    }
  }
  function setTableShowOrHide(prop, value) {
    for (let i = 0; i < state.tableData.length; i++) {
      if (state.tableData[i].__vModel__ === prop) {
        state.tableData[i].__config__.noShow = value;
        break;
      }
    }
  }
  function onFormBlur(rowIndex, colIndex, e) {
    const data: any = state.tableFormData[rowIndex][colIndex];
    if (data && data.on && data.on.blur) {
      const func: any = getScriptFunc(data.on.blur);
      if (!func) return;
      func({
        data: e,
        ...unref(parameter),
      });
    }
  }
  function onFormDataChange(rowIndex, colIndex, _tag, val, row) {
    if (state.isAddRow) return;
    const data: any = state.tableFormData[rowIndex][colIndex];
    state.activeRowIndex = rowIndex;
    if (data && data.on && data.on.change) {
      const func: any = getScriptFunc(data.on.change);
      if (!func) return;
      const value = row ? row : val;
      func({
        data: value,
        ...unref(parameter),
      });
    }
    data.required && (data.valid = checkData(data));
    data.regList && data.regList.length && (data.regValid = checkRegData(data));
    updateParentData();
    handleRelation(data, rowIndex);
  }
  /**
   * 校验单个表单数据
   * @param {CmpConfig} 组件配置对象
   */
  function checkData({ value }) {
    if ([null, undefined, ''].includes(value)) return false;
    if (Array.isArray(value)) return value.length > 0;
    return true;
  }
  function checkRegData(col) {
    let res = true;
    for (let i = 0; i < col.regList.length; i++) {
      const item = col.regList[i];
      if (item.pattern) {
        let pattern = eval(item.pattern);
        if (col.value && !pattern.test(col.value)) {
          res = false;
          col.regErrorText = item.message;
          break;
        }
      }
    }
    return res;
  }
  /**
   * 校验表格数据必填项
   */
  function submit() {
    let res = true;
    const checkCol = col => {
      col.required && !checkData(col) && (res = col.valid = false);
      col.regList && col.regList.length && !checkRegData(col) && (res = col.regValid = false);
    };
    state.tableFormData.forEach(row => row.forEach(checkCol));
    return res ? getTableValue() : false;
  }
  /**
   * 根据formId获取完整组件配置
   */
  function getConfById(formId, rowIndex) {
    let item = state.tableFormData[rowIndex].find(t => t.formId === formId).config;
    let itemConfig = item.__config__;
    let newObj = {};
    item = getRealProps(item, itemConfig.jnpfKey);
    for (const key in item) {
      if (!['__config__', '__vModel__', 'on'].includes(key)) {
        newObj[key] = item[key];
      }
      if (key === 'disabled') {
        newObj[key] = props.disabled || item[key];
      }
    }
    if (['relationForm', 'popupSelect'].includes(itemConfig.jnpfKey)) {
      newObj['field'] = item.__vModel__ + '_jnpfRelation_' + rowIndex;
    }
    if (['relationFormAttr', 'popupAttr'].includes(itemConfig.jnpfKey)) {
      let prop = newObj['relationField'].split('_jnpfTable_')[0];
      newObj['relationField'] = prop + '_jnpfRelation_' + rowIndex;
    }
    return newObj;
  }
  /**
   * 获取默认行数据
   */
  function getEmptyRow(val, rowIndex) {
    const currDate = new Date();
    return state.tableData.map((t: any) => {
      let options = [];
      if (dyOptionsList.indexOf(t.__config__.jnpfKey) > -1) options = t.options;
      if (t.__config__.jnpfKey === 'datePicker' && t.__config__.defaultCurrent) {
        const realCurrDate = dayjs(currDate).startOf(getDateTimeUnit(t.format)).valueOf();
        t.__config__.defaultValue = realCurrDate;
      }
      if (t.__config__.jnpfKey === 'timePicker' && t.__config__.defaultCurrent) {
        t.__config__.defaultValue = dayjs(currDate).format(t.format || 'HH:mm:ss');
      }
      let res = {
        tag: t.__config__.tag,
        formId: t.__config__.formId,
        value: val ? val[t.__vModel__] : t.__config__.defaultValue,
        options,
        valid: true,
        regValid: true,
        regErrorText: '',
        on: t.on || {},
        jnpfKey: t.__config__.jnpfKey,
        __vModel__: ['relationForm', 'popupSelect'].includes(t.__config__.jnpfKey) ? t.__vModel__ + '_jnpfRelation_' + rowIndex : t.__vModel__,
        regList: t.__config__.regList || [],
        required: t.__config__.required,
        rowData: val || {},
        config: t,
      };
      return res;
    });
  }
  // 获取表格数据
  function getTableValue() {
    return state.tableFormData.map(row =>
      (row as any[]).reduce((p, c) => {
        let str = c.__vModel__;
        if (c.__vModel__ && c.__vModel__.indexOf('_jnpfRelation_') >= 0) {
          str = c.__vModel__.substring(0, c.__vModel__.indexOf('_jnpfRelation_'));
        }
        p[str] = c.value;
        if (c.rowData) p = { ...c.rowData, ...p };
        return p;
      }, {}),
    );
  }
  // 更新父级数据 触发计算公式更新
  function updateParentData() {
    const newVal = getTableValue();
    emit('update:value', newVal);
    emit('change', newVal);
    formItemContext.onFieldChange();
  }
  function removeRow(index) {
    createConfirm({
      iconType: 'warning',
      title: t('common.tipTitle'),
      content: '此操作将永久删除该数据, 是否继续?',
      onOk: () => {
        state.tableFormData.splice(index, 1);
        nextTick(() => {
          updateParentData();
        });
      },
    });
  }
  function addRow(val?) {
    state.isAddRow = true;
    if (!Array.isArray(state.tableFormData)) state.tableFormData = [];
    const rowIndex = state.tableFormData.length;
    state.tableFormData.push(JSON.parse(JSON.stringify(getEmptyRow(val, rowIndex))));
    buildRowAttr(rowIndex);
    clearAddRowFlag();
    nextTick(() => {
      updateParentData();
    });
  }
  function addItem() {
    if (props.config.addType == 1) {
      openSelectDialog();
    } else {
      addRow();
    }
  }
  function openSelectDialog() {
    (unref(selectModal) as any)?.openSelectModal();
  }
  function addForSelect(data) {
    data.forEach(t => addRow(t));
  }
  function getCmpValOfRow(row, key) {
    if (!props.config.summaryField.length) return '';
    const isSummary = key => props.config.summaryField.includes(key);
    const target = row.find(t => t.__vModel__ === key);
    if (!target) return '';
    let data = isNaN(target.value) ? 0 : Number(target.value);
    if (isSummary(key)) return data || 0;
    return '';
  }
  function resetTable() {
    state.tableData = props.config.__config__.children;
    state.tableFormData = [];
    // addRow()
  }
  function reset() {
    state.tableData.map(t => {
      let index = state.tableFormData[0].findIndex(c => c.vModel === t.vModel);
      if (index === -1) return;
      for (let i = 0; i < state.tableFormData.length; i++) {
        state.tableFormData[i][index].value = t.defaultValue;
      }
    });
  }
</script>
<style lang="less" scoped>
  @prefix-cls: ~'@{namespace}-input-table';

  .@{prefix-cls} {
    .error-tip {
      font-size: 12px;
    }
  }
</style>
