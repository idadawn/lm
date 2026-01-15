<template>
  <BasicModal v-bind="$attrs" width="500px" @register="registerModal" title="发送测试" showOkBtn @ok="handleSubmit">
    <BasicForm @register="registerForm" />
  </BasicModal>
</template>
<script lang="ts" setup>
  import { reactive } from 'vue';
  import { BasicModal, useModalInner } from '/@/components/Modal';
  import { BasicForm, useForm, FormSchema } from '/@/components/Form';
  import { useMessage } from '/@/hooks/web/useMessage';
  import { testConfig } from '/@/api/msgCenter/accountConfig';

  interface State {
    dataForm: any;
  }

  const state = reactive<State>({
    dataForm: {},
  });
  const schemas: FormSchema[] = [
    {
      field: 'testSendEmail',
      label: '收件邮箱',
      component: 'UserSelect',
      componentProps: { placeholder: '收件人' },
      rules: [{ required: true, trigger: 'change', message: '请选择收件人' }],
    },
    {
      field: 'testEmailTitle',
      label: '邮件标题',
      component: 'Input',
      componentProps: { placeholder: '邮件标题' },
      rules: [
        { required: true, trigger: 'blur', message: '请输入邮件标题' },
        { max: 50, message: '编码最多为50个字符！', trigger: 'blur' },
      ],
    },
    {
      field: 'testEmailContent',
      label: '邮件内容',
      component: 'Input',
      componentProps: { placeholder: '邮件内容' },
      rules: [{ required: true, trigger: 'blur', message: '请输入邮件内容' }],
    },
  ];
  const { createMessage } = useMessage();
  const [registerForm, { setFieldsValue, validate, resetFields }] = useForm({ labelWidth: 80, schemas: schemas });
  const [registerModal, { changeOkLoading }] = useModalInner(init);

  function init(data) {
    resetFields();
    state.dataForm = data;
    setFieldsValue({ testEmailTitle: '测试', testEmailContent: '测试' });
  }
  async function handleSubmit() {
    const values = await validate();
    if (!values) return;
    changeOkLoading(true);
    values.testSendEmail = [values.testSendEmail];
    const query = {
      ...values,
      ...state.dataForm,
      testType: 'testSendMail',
    };
    testConfig(query)
      .then(res => {
        createMessage.success(res.msg);
        changeOkLoading(false);
      })
      .catch(() => {
        changeOkLoading(false);
      });
  }
</script>
