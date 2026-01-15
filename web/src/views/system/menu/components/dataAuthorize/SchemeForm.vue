<template>
  <BasicModal v-bind="$attrs" @register="registerModal" :title="getTitle" showOkBtn @ok="handleSubmit" destroy-on-close>
    <a-form :colon="false" :labelCol="{ style: { width: '0' } }" :model="dataForm" :rules="dataRule" ref="formElRef">
      <a-form-item name="enCode">
        <a-input v-model:value="dataForm.enCode" placeholder="请输入编码" />
      </a-form-item>
      <a-form-item name="fullName">
        <a-input v-model:value="dataForm.fullName" placeholder="请输入方案名称" />
      </a-form-item>
      <a-form-item v-for="(item, index) in condition" :key="index">
        <a-row :gutter="5">
          <a-col :span="7">
            <jnpf-select v-model:value="item.logic" placeholder="请选择" :options="logicOptions" />
          </a-col>
          <a-col :span="7">
            <a-button preIcon="icon-ym icon-ym-btn-add" @click="addItem(index)">添加条件</a-button>
          </a-col>
          <a-col :span="10" style="text-align: right">
            <a-button type="danger" preIcon="icon-ym icon-ym-nav-close" @click="delGroup(index)">删除分组</a-button>
          </a-col>
        </a-row>
        <a-row :gutter="5" v-for="(subItem, i) in item.groups" :key="index + i" class="mt-10px">
          <a-col :span="7">
            <jnpf-select
              v-model:value="subItem.id"
              placeholder="选择字段"
              allowClear
              @change="changeField($event, subItem)"
              :options="fieldOptions"
              class="!w-142px" />
          </a-col>
          <a-col :span="7">
            <jnpf-select v-model:value="subItem.op" placeholder="选择符号" allowClear :options="subItem.opOptions" />
          </a-col>
          <a-col :span="8">
            <a-input v-model:value="subItem.value" placeholder="输入字段值" :readonly="subItem.readonly" />
          </a-col>
          <a-col :span="2">
            <a-button type="danger" preIcon="icon-ym icon-ym-nav-close" @click="delItem(index, i)" block />
          </a-col>
        </a-row>
      </a-form-item>
      <a-form-item>
        <a-button @click="addGroup">添加分组</a-button>
      </a-form-item>
    </a-form>
  </BasicModal>
</template>
<script lang="ts" setup>
  import { computed, reactive, toRefs, toRaw, ref } from 'vue';
  import { BasicModal, useModalInner } from '/@/components/Modal';
  import { createScheme, updateScheme, getSchemeInfo, getDataAuthorizeList } from '/@/api/system/dataAuthorize';
  import { useMessage } from '/@/hooks/web/useMessage';
  import type { FormInstance } from 'ant-design-vue';

  interface State {
    dataForm: any;
    dataRule: any;
    condition: any[];
    logicOptions: any[];
    fieldOptions: any[];
    opOptions: any[];
  }

  const state = reactive<State>({
    dataForm: {
      id: '',
      moduleId: '',
      enCode: '',
      fullName: '',
      conditionJson: '',
      conditionText: '',
    },
    dataRule: {
      fullName: [{ required: true, message: '方案名称不能为空', trigger: 'blur' }],
      enCode: [
        { required: true, message: '编码不能为空', trigger: 'blur' },
        { max: 50, message: '字典编码最多为50个字符！', trigger: 'blur' },
      ],
    },
    condition: [
      {
        logic: 'and',
        groups: [
          {
            id: '',
            field: '',
            type: '',
            op: '',
            value: '',
            opOptions: [],
          },
        ],
      },
    ],
    logicOptions: [
      {
        id: 'and',
        fullName: 'and',
      },
      {
        id: 'or',
        fullName: 'or',
      },
    ],
    fieldOptions: [],
    opOptions: [
      { id: 'Equal', fullName: '等于' },
      { id: 'NotEqual', fullName: '不等于' },
      { id: 'GreaterThan', fullName: '大于' },
      { id: 'GreaterThanOrEqual', fullName: '大于等于' },
      { id: 'LessThan', fullName: '小于' },
      { id: 'LessThanOrEqual', fullName: '小于等于' },
      { id: 'Included', fullName: '包含' },
      { id: 'NotIncluded', fullName: '不包含' },
      { id: 'IsNull', fullName: '是null' },
      { id: 'IsNotNull', fullName: '不是null' },
    ],
  });
  const { dataForm, dataRule, condition, logicOptions, fieldOptions } = toRefs(state);
  const formElRef = ref<FormInstance>();

  const getTitle = computed(() => (!state.dataForm.id ? '新建方案' : '编辑方案'));
  const emit = defineEmits(['register', 'reload']);
  const { createMessage } = useMessage();
  const [registerModal, { closeModal, changeLoading, changeOkLoading }] = useModalInner(init);

  function init(data) {
    state.dataForm.enCode = '';
    state.dataForm.fullName = '';
    setDefault();
    state.dataForm.id = data.id || '';
    state.dataForm.moduleId = data.moduleId || '';
    changeLoading(true);
    getDataAuthorizeList({ moduleId: state.dataForm.moduleId })
      .then(res => {
        state.fieldOptions = res.data.list;
        state.fieldOptions.map(o => {
          o.enCode = o.enCode.replace('_jnpf_', '.');
          o.enCode = o.enCode.replace('jnpf_', '');
        });
        changeLoading(false);
      })
      .then(() => {
        changeLoading(false);
      });
    if (state.dataForm.id) {
      changeLoading(true);
      getSchemeInfo(state.dataForm.id)
        .then(res => {
          state.dataForm = res.data;
          if (res.data.conditionJson)
            state.condition = res.data.conditionJson ? JSON.parse(res.data.conditionJson) : [];
          for (let i = 0; i < state.condition.length; i++) {
            let groups = state.condition[i].groups;
            for (let j = 0; j < groups.length; j++) {
              let e = groups[j];
              let list = state.fieldOptions.filter(o => o.id === groups[j].id);
              e.opOptions = list.length ? getOptions(list[0]) : [];
              if (e.conditionText !== 'text') {
                e.readonly = true;
              } else {
                e.readonly = false;
              }
            }
          }
          changeLoading(false);
        })
        .then(() => {
          changeLoading(false);
        });
    }
  }
  async function handleSubmit() {
    try {
      const values = await formElRef?.value?.validate();
      if (!values) return;
      let conditionText = '';
      let conditionValid = true;
      let condition = JSON.parse(JSON.stringify(state.condition));
      outer: for (let i = 0; i < condition.length; i++) {
        let e = condition[i];
        let text = '';
        if (i > 0) text += e.logic === 'and' ? '而且' : '或者';
        text += '【';
        for (let j = 0; j < e.groups.length; j++) {
          let ee = e.groups[j];
          let item = state.fieldOptions.filter(o => o.id === ee.id)[0];
          if (!item) {
            createMessage.warning('方案内条件配置不完整，请检查条件内容');
            conditionValid = false;
            break outer;
          }
          ee.bindTable = item.bindTable;
          if (!ee.field || !ee.id || !ee.op || !ee.value) {
            createMessage.warning('方案内条件配置不完整，请检查条件内容');
            conditionValid = false;
            break outer;
          }
          delete ee.readonly;
          delete ee.opOptions;
          text += `${j == 0 ? '' : ' and '}{${getFieldText(ee.id)}} {${getOpText(ee.op)}} {${ee.value}}`;
        }
        text += '】';
        conditionText += text;
      }
      if (!conditionValid) return;
      changeOkLoading(true);
      state.dataForm.conditionText = conditionText;
      state.dataForm.conditionJson = JSON.stringify(condition);
      const formMethod = state.dataForm.id ? updateScheme : createScheme;
      formMethod(state.dataForm)
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
  function getOpText(val) {
    if (!val) return val;
    let list = state.opOptions.filter(o => o.id == val);
    if (!list.length) return val;
    return list[0].fullName || val;
  }
  function getFieldText(val) {
    if (!val) return val;
    let list = state.fieldOptions.filter(o => o.id == val);
    if (!list.length) return val;
    return list[0].fullName || val;
  }
  function getOptions(fieldItem) {
    let newOpOptions = [];
    let options = fieldItem.conditionSymbol ? fieldItem.conditionSymbol.split(',') : [];
    outer: for (let i = 0; i < options.length; i++) {
      inner: for (let j = 0; j < state.opOptions.length; j++) {
        if (options[i] === state.opOptions[j].id) {
          (newOpOptions as any[]).push(toRaw(state.opOptions[j]));
          break inner;
        }
      }
    }
    return newOpOptions;
  }
  function addItem(index) {
    state.condition[index].groups.push({
      id: '',
      field: '',
      type: '',
      op: '',
      value: '',
      opOptions: [],
    });
  }
  function delItem(index, childIndex) {
    state.condition[index].groups.splice(childIndex, 1);
  }
  function delGroup(index) {
    state.condition.splice(index, 1);
  }
  function addGroup() {
    state.condition.push({ logic: 'and', groups: [{ id: '', field: '', type: '', op: '', value: '', opOptions: [] }] });
  }
  function setDefault() {
    state.condition = [{ logic: 'and', groups: [{ id: '', field: '', type: '', op: '', value: '', opOptions: [] }] }];
  }
  function changeField(val, item) {
    if (!val) {
      item.id = '';
      item.field = '';
      item.type = '';
      item.op = '';
      item.value = '';
      item.opOptions = [];
      item.readonly = false;
    } else {
      item.op = '';
      item.value = '';
      item.readonly = false;
      let fieldItem = state.fieldOptions.filter(o => o.id === val)[0];
      item.type = fieldItem.type;
      item.field = fieldItem.enCode;
      item.opOptions = getOptions(fieldItem);
      item.field = fieldItem.enCode;
      item.fieldRule = fieldItem.fieldRule || 0;
      if (fieldItem.conditionText !== 'text') {
        item.readonly = true;
        item.value = fieldItem.conditionText;
      }
    }
  }
</script>
