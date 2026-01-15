<template>
  <BasicModal
    v-bind="$attrs"
    @register="registerModal"
    defaultFullscreen
    :footer="null"
    :closable="false"
    :keyboard="false"
    class="jnpf-full-modal full-modal"
    destroyOnClose>
    <template #title>
      <div class="jnpf-full-modal-header">
        <div class="header-title">
          <img src="../../../assets/images/zhichang.png" class="header-logo" />
          <p class="header-txt"> · 打印模板</p>
        </div>
        <a-steps v-model:current="activeStep" type="navigation" size="small">
          <a-step title="基础设置" />
          <a-step title="打印设置" :disabled="activeStep <= 1" />
        </a-steps>
        <a-space class="options" :size="10">
          <a-button @click="handlePrev" :disabled="activeStep <= 0 || btnLoading">{{ t('common.prev') }}</a-button>
          <a-button @click="handleNext" :disabled="activeStep >= 1 || loading || btnLoading" :loading="nextBtnLoading">{{ t('common.next') }} </a-button>
          <a-button type="primary" @click="handleSubmit()" :disabled="loading || activeStep != 1" :loading="btnLoading">{{ t('common.saveText') }}</a-button>
          <a-button @click="closeModal()">{{ t('common.cancelText') }}</a-button>
        </a-space>
      </div>
    </template>
    <div class="basic-content">
      <a-row type="flex" justify="center" align="middle" class="basic-content" v-show="activeStep === 0">
        <a-col :span="12" :xxl="10" class="basic-form">
          <BasicForm @register="registerForm">
            <template #tips>
              <JnpfGroupTitle content="数据来源" class="mb-10px" />
              <JnpfAlert message="默认打印的时候必须传业务主键@formId给SQL语句里面作为Where查询条件" type="warning" show-icon />
            </template>
            <template #sqlTemplate>
              <a-form-item-rest>
                <a-button preIcon="icon-ym icon-ym-btn-add" @click="handleAddSql()">新增SQL</a-button>
                <a-row v-for="(item, i) in sqlTemplate" :key="i" class="mt-10px">
                  <a-col :span="21">
                    <a-textarea v-model:value="item.sql" placeholder="请输入SQL查询语句&存储过程语句" type="textarea" :autoSize="{ minRows: 3, maxRows: 10 }" />
                  </a-col>
                  <a-col :span="2" :offset="1" class="delBtn">
                    <a-button type="danger" preIcon="icon-ym icon-ym-nav-close" @click="handleDelSql(i)"> </a-button>
                  </a-col>
                </a-row>
              </a-form-item-rest>
            </template>
          </BasicForm>
        </a-col>
      </a-row>
      <PrintDesign
        v-if="activeStep === 1"
        :treeData="treeData"
        v-model:value="dataForm.printTemplate"
        :pageParam="pageParam"
        @pageParamChange="pageParamChange"
        @input="onDesignChange"
        :type="dataForm.type" />
    </div>
  </BasicModal>
</template>
<script lang="ts" setup>
  import { reactive, toRefs, watch, nextTick } from 'vue';
  import { BasicModal, useModalInner } from '/@/components/Modal';
  import { useI18n } from '/@/hooks/web/useI18n';
  import PrintDesign from '/@/components/PrintDesign/index.vue';
  import { getPrintDevInfo, updatePrintDev, createPrintDev, getFields } from '/@/api/system/printDev';
  import { BasicForm, useForm } from '/@/components/Form';
  import formValidate from '/@/utils/formValidate';
  import { getDataSourceSelector } from '/@/api/systemData/dataSource';
  import { useMessage } from '/@/hooks/web/useMessage';

  const emit = defineEmits(['register', 'reload']);

  interface State {
    activeStep: number;
    btnLoading: boolean;
    nextBtnLoading: boolean;
    loading: boolean;
    treeData: any;
    dataForm: any;
    pageParam: any;
    dbOptions: any[];
    sqlTemplate: any[];
    showPrint: boolean;
  }

  const state = reactive<State>({
    activeStep: 0,
    btnLoading: false,
    nextBtnLoading: false,
    loading: false,
    treeData: [],
    dataForm: {
      id: '',
      fullName: '',
      enCode: '',
      dbLinkId: '0',
      type: 1,
      enabledMark: 1,
      sortCode: 0,
      category: '',
      sqlTemplate: '',
      leftFields: '',
      printTemplate: '',
      description: '',
    },
    pageParam: {
      type: '2',
      width: '210',
      height: '297',
      mt: '10',
      mb: '10',
      ml: '10',
      mr: '10',
      direction: '纵向',
    },
    dbOptions: [],
    sqlTemplate: [],
    showPrint: true,
  });
  const { activeStep, btnLoading, loading, treeData, dataForm, pageParam, sqlTemplate, nextBtnLoading } = toRefs(state);
  const { t } = useI18n();
  const { createMessage } = useMessage();
  const [registerModal, { closeModal, changeLoading }] = useModalInner(init);
  const [registerForm, { setFieldsValue, resetFields, validate, updateSchema, clearValidate }] = useForm({
    schemas: [
      {
        field: 'fullName',
        label: '模板名称',
        component: 'Input',
        componentProps: { placeholder: '模板名称', maxlength: 100 },
        rules: [{ required: true, trigger: 'blur', message: '模板名称不能为空' }],
      },
      {
        field: 'enCode',
        label: '模板编码',
        component: 'Input',
        componentProps: { placeholder: '模板编码', maxlength: 50 },
        rules: [
          { required: true, trigger: 'blur', message: '模板编码不能为空' },
          { validator: formValidate('enCode'), trigger: 'blur' },
        ],
      },
      {
        field: 'category',
        label: '模板分类',
        component: 'Select',
        componentProps: { placeholder: '选择分类', showSearch: true, fieldNames: { value: 'enCode' } },
        rules: [{ required: true, trigger: 'change', message: '模板分类不能为空' }],
      },
      {
        field: 'type',
        label: '模板类型',
        defaultValue: 1,
        component: 'Radio',
        componentProps: {
          options: [
            { id: 1, fullName: '流程表单' },
            { id: 2, fullName: '功能表单' },
          ],
          optionType: 'button',
          buttonStyle: 'solid',
        },
        rules: [{ required: true, trigger: 'change', message: '模板类型不能为空', type: 'number' }],
      },
      {
        field: 'sortCode',
        label: '模板排序',
        defaultValue: 0,
        component: 'InputNumber',
        componentProps: { min: 0, max: 999999 },
      },
      {
        field: 'enabledMark',
        label: '模板状态',
        defaultValue: 1,
        component: 'Switch',
      },
      {
        field: 'description',
        label: '模板说明',
        component: 'Textarea',
        componentProps: { placeholder: '模板说明' },
      },
      {
        field: 'tips',
        label: '',
        component: 'Input',
        slot: 'tips',
      },
      {
        field: 'dbLinkId',
        label: '数据连接',
        component: 'Select',
        defaultValue: '0',
        componentProps: { placeholder: '请选择数据库', allowClear: false, showSearch: true, fieldNames: { options: 'children' } },
        rules: [{ required: true, trigger: 'change', message: '数据库不能为空' }],
      },
      {
        field: 'sqlTemplate',
        label: 'SQL语句',
        component: 'Select',
        slot: 'sqlTemplate',
        required: true,
      },
    ],
  });

  watch(
    () => state.sqlTemplate,
    value => {
      nextTick(() => {
        setFieldsValue({ sqlTemplate: value });
      });
    },
    { deep: true },
  );

  function init(data) {
    state.sqlTemplate = [];
    state.dataForm.printTemplate = '';
    resetFields();
    clearValidate();
    getDataSource();
    state.activeStep = 0;
    state.dataForm.id = data.id || '';
    if (data?.categoryList) updateSchema({ field: 'category', componentProps: { options: data.categoryList } });
    if (state.dataForm.id) {
      changeLoading(true);
      state.showPrint = false;
      getPrintDevInfo(state.dataForm.id)
        .then(res => {
          state.dataForm = res.data;
          state.sqlTemplate = (state.dataForm.sqlTemplate && JSON.parse(state.dataForm.sqlTemplate)) || [];
          let e = (res.data.pageParam && JSON.parse(res.data.pageParam)) || {};
          state.pageParam = e;
          state.pageParam.direction = '纵向';
          setFieldsValue(state.dataForm);
          state.showPrint = true;
          changeLoading(false);
        })
        .catch(() => {
          state.loading = false;
          state.showPrint = true;
          changeLoading(false);
        });
    }
  }
  function getDataSource() {
    getDataSourceSelector().then(res => {
      let list = res.data.list || [];
      list = list.filter(o => o.children && o.children.length);
      if (list[0] && list[0].children && list[0].children.length) list[0] = list[0].children[0];
      delete list[0].children;
      state.dbOptions = list;
      updateSchema([{ field: 'dbLinkId', componentProps: { options: state.dbOptions } }]);
    });
  }
  function handleSubmit() {
    state.btnLoading = true;
    const formMethod = state.dataForm.id ? updatePrintDev : createPrintDev;
    formMethod(state.dataForm)
      .then(res => {
        state.btnLoading = false;
        createMessage.success(res.msg);
        closeModal();
        setTimeout(() => {
          emit('reload');
        }, 200);
      })
      .catch(() => {
        state.btnLoading = false;
      });
  }
  function handlePrev() {
    state.activeStep -= 1;
  }
  async function handleNext() {
    if (state.activeStep < 1) {
      const values = await validate();
      if (!values) return;
      if (!exist()) return;
      state.dataForm = { ...state.dataForm, ...values, sqlTemplate: JSON.stringify(state.sqlTemplate) };
      state.nextBtnLoading = true;
      const query = {
        dbLinkId: values.dbLinkId,
        sqlTemplate: state.dataForm.sqlTemplate,
      };
      getFields(query)
        .then(res => {
          state.treeData = res.data;
          state.activeStep += 1;
          state.nextBtnLoading = false;
        })
        .catch(() => {
          state.nextBtnLoading = false;
        });
    }
  }
  function pageParamChange(pageParam) {
    state.dataForm.pageParam = pageParam && JSON.stringify(pageParam);
  }
  function handleAddSql() {
    state.sqlTemplate.push({ sql: '' });
  }
  function handleDelSql(i) {
    state.sqlTemplate.splice(i, 1);
  }
  function exist() {
    if (!state.sqlTemplate.length) {
      createMessage.error('请输入SQL语句');
      return false;
    }
    let isOk = true;
    for (let i = 0; i < state.sqlTemplate.length; i++) {
      const e = state.sqlTemplate[i];
      if (!e.sql) {
        createMessage.error(`第${i + 1}行SQL语句不能为空`);
        isOk = false;
        break;
      }
    }
    return isOk;
  }
  function onDesignChange(val) {
    state.dataForm.printTemplate = val;
  }
</script>
