<template>
  <BasicModal v-bind="$attrs" @register="registerModal" :title="getTitle" showOkBtn @ok="handleSubmit">
    <BasicForm @register="registerForm">
      <template #appSecret="{ model, field }">
        <a-input-password v-model:value="model[field]" placeholder="请输入AppSecret " show-password>
          <template #addonAfter>
            <loading-outlined class="mr-5px" v-if="testLoading" />
            <span class="cursor-pointer" @click="handleTest">测试</span>
          </template>
        </a-input-password>
      </template>
    </BasicForm>
  </BasicModal>
</template>
<script lang="ts" setup>
  import { computed, reactive, toRefs } from 'vue';
  import { BasicModal, useModalInner } from '/@/components/Modal';
  import { BasicForm, useForm, FormSchema } from '/@/components/Form';
  import { useMessage } from '/@/hooks/web/useMessage';
  import { create, update, getInfo, testConfig } from '/@/api/msgCenter/accountConfig';
  import formValidate from '/@/utils/formValidate';
  import { LoadingOutlined } from '@ant-design/icons-vue';

  interface State {
    dataForm: any;
    testLoading: boolean;
  }

  const state = reactive<State>({
    dataForm: {},
    testLoading: false,
  });
  const schemas: FormSchema[] = [
    {
      field: 'fullName',
      label: '名称',
      component: 'Input',
      componentProps: { placeholder: '请输入名称' },
      rules: [
        { required: true, trigger: 'blur', message: '请输入名称' },
        { max: 50, message: '业务名称最多为50个字符！', trigger: 'blur' },
      ],
    },
    {
      field: 'enCode',
      label: '编码',
      component: 'Input',
      componentProps: { placeholder: '请输入编码', maxlength: 50 },
      rules: [
        { required: true, trigger: 'blur', message: '请输入编码' },
        { max: 50, message: '编码最多为50个字符！', trigger: 'blur' },
        { validator: formValidate('enCode'), trigger: 'blur' },
      ],
    },
    {
      field: 'agentId',
      label: 'AgentId',
      component: 'Input',
      helpMessage: '请在“钉钉开发者后台-应用开发-应用信息”页中获得',
      componentProps: { placeholder: '请输入AgentId' },
      rules: [{ required: true, trigger: 'blur', message: '请输入AgentId' }],
    },
    {
      field: 'appId',
      label: 'Appkey',
      component: 'Input',
      helpMessage: '请在“钉钉开发者后台-应用开发-应用信息”页中获得',
      componentProps: { placeholder: '请输入Appkey' },
      rules: [{ required: true, trigger: 'blur', message: '请输入Appkey' }],
    },
    {
      field: 'appSecret',
      label: 'AppSecret',
      component: 'Input',
      helpMessage: '请在“钉钉开发者后台-应用开发-应用信息”页中获得',
      slot: 'appSecret',
      rules: [{ required: true, trigger: 'blur', message: '请输入AppSecret' }],
    },
    {
      field: 'sortCode',
      label: '排序',
      component: 'InputNumber',
      defaultValue: 0,
      componentProps: { min: '0', max: '999999', placeholder: '排序' },
    },
    {
      field: 'enabledMark',
      label: '状态',
      component: 'Switch',
      defaultValue: 1,
    },
    {
      field: 'description',
      label: '说明',
      component: 'Textarea',
      componentProps: { rows: 3 },
    },
  ];
  const { testLoading } = toRefs(state);
  const getTitle = computed(() => (!state.dataForm.id ? '新建' : '编辑'));
  const emit = defineEmits(['register', 'reload']);
  const { createMessage } = useMessage();
  const [registerForm, { setFieldsValue, validate, resetFields }] = useForm({ labelWidth: 120, schemas: schemas });
  const [registerModal, { closeModal, changeLoading, changeOkLoading }] = useModalInner(init);

  function init(data) {
    state.testLoading = false;
    resetFields();
    state.dataForm.id = data.id;
    if (state.dataForm.id) {
      changeLoading(true);
      getInfo(state.dataForm.id).then(res => {
        const data = res.data;
        state.dataForm = data;
        setFieldsValue(data);
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
      id: state.dataForm.id,
      type: 4,
    };
    const formMethod = state.dataForm.id ? update : create;
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
  async function handleTest() {
    const values = await validate();
    if (!values) return;
    state.testLoading = true;
    const query = {
      ...values,
      id: state.dataForm.id,
      type: 4,
      testType: 'testDingTalkConnect',
    };
    testConfig(query)
      .then(res => {
        createMessage.success(res.msg);
        state.testLoading = false;
      })
      .catch(() => {
        state.testLoading = false;
      });
  }
</script>
