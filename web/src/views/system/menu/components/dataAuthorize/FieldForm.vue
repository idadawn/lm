<template>
  <BasicModal v-bind="$attrs" @register="registerModal" :title="getTitle" showOkBtn @ok="handleSubmit" destroy-on-close>
    <BasicForm @register="registerForm">
      <template #enCode="{ model, field }">
        <FieldModal :value="model[field]" @change="onNameChange" :treeData="treeData" :menuType="menuType" :dataType="dataType" />
      </template>
      <template #fieldRule="{ model, field }">
        <a-select v-model:value="model[field]" :options="fieldRuleOptions" placeholder="请选择字段规则" @change="onFileRuleChange">
          <template #option="{ value, label }">
            {{ label }}<BasicHelp v-if="value != 0" :text="`与主表是一对${value == 1 ? '一' : '多'}的主从关系`" />
          </template>
        </a-select>
      </template>
    </BasicForm>
  </BasicModal>
</template>
<script lang="ts" setup>
  import { computed, reactive, toRefs } from 'vue';
  import { BasicModal, useModalInner } from '/@/components/Modal';
  import { BasicForm, useForm, FormSchema } from '/@/components/Form';
  import { createField, updateField, getFieldInfo } from '/@/api/system/dataAuthorize';
  import { useMessage } from '/@/hooks/web/useMessage';
  import { getVisualTables } from '/@/api/system/authorize';
  import FieldModal from '../connectForm/FieldModal.vue';

  interface State {
    moduleId: string;
    id: string;
    menuType: number;
    dataType: number;
    treeData: any[];
    fieldRuleOptions: any[];
  }

  const state = reactive<State>({
    moduleId: '',
    id: '',
    menuType: 0,
    dataType: 0,
    treeData: [],
    fieldRuleOptions: [
      { value: 0, label: '主表规则' },
      { value: 1, label: '副表规则' },
      { value: 2, label: '子表规则' },
    ],
  });
  const { fieldRuleOptions, treeData, menuType, dataType } = toRefs(state);
  const schemas: FormSchema[] = [
    {
      field: 'enCode',
      label: '字段名称',
      component: 'Input',
      slot: 'enCode',
      componentProps: { placeholder: '请输入字段名称', maxlength: 200 },
      rules: [{ required: true, trigger: 'change', message: '字段名称不能为空' }],
    },
    {
      ifShow: ({ values }) => values.bindTable,
      field: 'bindTable',
      label: '数据库表',
      component: 'Input',
      componentProps: { placeholder: '请输入数据库表', disabled: true },
    },
    {
      field: 'fieldRule',
      label: '字段规则',
      component: 'Select',
      slot: 'fieldRule',
      defaultValue: 0,
      componentProps: { placeholder: '请选择字段规则' },
      rules: [{ required: true, trigger: 'blur', message: '字段规则不能为空', type: 'number' }],
    },
    {
      ifShow: ({ values }) => values.fieldRule === 2,
      field: 'childTableKey',
      label: '关联字段',
      component: 'Input',
      componentProps: { placeholder: '请输入关联主表的子表控件名称' },
      helpMessage: '输入表单设计内设计子表的控件字段名，例：tableField107',
      rules: [{ required: true, trigger: 'blur', message: '关联字段不能为空' }],
    },
    {
      field: 'fullName',
      label: '字段说明',
      component: 'Input',
      componentProps: { placeholder: '输入字段说明' },
      rules: [{ required: true, trigger: 'blur', message: '字段说明不能为空' }],
    },
    {
      field: 'type',
      label: '字段类型',
      component: 'Select',
      componentProps: {
        placeholder: '请选择字段类型',
        options: [
          { id: 'string', fullName: 'string' },
          { id: 'int', fullName: 'int' },
          { id: 'double', fullName: 'double' },
          { id: 'varchar', fullName: 'varchar' },
          { id: 'datetime', fullName: 'datetime' },
          { id: 'text', fullName: 'text' },
          { id: 'bigint', fullName: 'bigint' },
        ],
      },
      rules: [{ required: true, trigger: 'change', message: '请选择字段类型' }],
    },
    {
      field: 'conditionSymbol',
      label: '条件符号',
      component: 'Select',
      componentProps: {
        placeholder: '请选择条件符号',
        multiple: true,
        options: [
          { id: 'Equal', fullName: '等于' },
          { id: 'NotEqual', fullName: '不等于' },
          { id: 'GreaterThan', fullName: '大于' },
          { id: 'GreaterThanOrEqual', fullName: '大于等于' },
          { id: 'LessThan', fullName: '小于' },
          { id: 'LessThanOrEqual', fullName: '小于等于' },
          { id: 'Included', fullName: '包含' },
          { id: 'NotIncluded', fullName: '不包含' },
        ],
      },
      rules: [{ required: true, trigger: 'change', message: '请选择条件符号', type: 'array' }],
    },
    {
      field: 'conditionText',
      label: '条件内容',
      component: 'Select',
      componentProps: {
        placeholder: '请选择条件内容',
        options: [
          { id: 'text', fullName: '任意文本' },
          { id: '@userId', fullName: '当前用户' },
          { id: '@userAraSubordinates', fullName: '当前用户及下属' },
          { id: '@organizeId', fullName: '当前组织' },
          { id: '@organizationAndSuborganization', fullName: '当前组织及子组织' },
          { id: '@branchManageOrganize', fullName: '当前分管组织' },
          { id: '@branchManageOrganizeAndSub', fullName: '当前分管组织及子组织' },
        ],
      },
      rules: [{ required: true, trigger: 'change', message: '请选择条件内容' }],
    },
    {
      field: 'description',
      label: '备注',
      component: 'Textarea',
      componentProps: { rows: 3 },
    },
  ];
  const getTitle = computed(() => (!state.id ? '新建字段' : '编辑字段'));
  const emit = defineEmits(['register', 'reload']);
  const { createMessage } = useMessage();
  const [registerForm, { setFieldsValue, validate, resetFields, clearValidate }] = useForm({ labelWidth: 100, schemas: schemas });
  const [registerModal, { closeModal, changeLoading, changeOkLoading }] = useModalInner(init);

  function init(data) {
    resetFields();
    state.id = data.id;
    state.moduleId = data.moduleId;
    state.menuType = data.type;
    state.dataType = data.dataType;
    changeLoading(true);
    getVisualTables(state.moduleId, 3)
      .then(res => {
        let data: any[] = [];
        for (const key in res?.data?.linkTables) {
          const obj = {
            tableName: res.data.linkTables[key],
            dbLink: res.data.linkId,
          };
          data.push(obj);
        }
        state.treeData = data;
        changeLoading(false);
      })
      .then(() => {
        changeLoading(false);
      });
    if (state.id) {
      changeLoading(true);
      getFieldInfo(state.id)
        .then(res => {
          const data = res.data;
          data.conditionSymbol = data.conditionSymbol ? data.conditionSymbol.split(',') : [];
          setFieldsValue(data);
          changeLoading(false);
        })
        .then(() => {
          changeLoading(false);
        });
    }
  }
  async function handleSubmit() {
    const values = await validate();
    if (!values) return;
    changeOkLoading(true);
    const query = {
      ...values,
      conditionSymbol: values.conditionSymbol.join(),
      moduleId: state.moduleId,
      id: state.id,
    };
    const formMethod = state.id ? updateField : createField;
    formMethod(query)
      .then(res => {
        createMessage.success(res.msg);
        changeOkLoading(false);
        closeModal();
        emit('reload');
      })
      .catch(() => {
        changeOkLoading(false);
      });
  }
  function onNameChange(val, tableName, fieldName) {
    setFieldsValue({ enCode: val || '' });
    if (tableName) setFieldsValue({ bindTable: tableName });
    if (fieldName) setFieldsValue({ fullName: fieldName });
    if (val) clearValidate();
  }
  function onFileRuleChange() {
    setFieldsValue({ childTableKey: '' });
  }
</script>
