<template>
  <BasicModal v-bind="$attrs" @register="registerModal" :title="getTitle" showOkBtn @ok="handleSubmit">
    <BasicForm @register="registerForm">
      <template #icon="{ model, field }">
        <a-row type="flex">
          <div style="flex: 1; margin-right: 10px">
            <jnpf-icon-picker v-model:value="model[field]" placeholder="请选择图标" />
          </div>
          <a-form-item-rest>
            <jnpf-color-picker v-model:value="propertyJson.iconBackgroundColor" size="small" :predefine="predefineList" name="iconBackground" />
          </a-form-item-rest>
        </a-row>
      </template>
    </BasicForm>
  </BasicModal>
</template>
<script lang="ts" setup>
  import { ref, toRefs, unref, computed, reactive } from 'vue';
  import { BasicModal, useModalInner } from '/@/components/Modal';
  import { BasicForm, useForm, FormSchema } from '/@/components/Form';
  import { create, update, getInfo } from '/@/api/system/system';
  import { useMessage } from '/@/hooks/web/useMessage';

  interface State {
    propertyJson: any;
  }

  const predefineList = ['#008cff', '#35b8e0', '#00cc88', '#ff9d00', '#ff4d4d', '#5b69bc', '#ff8acc', '#3b3e47', '#282828'];
  const id = ref('');
  const state = reactive<State>({
    propertyJson: {
      iconBackgroundColor: '',
    },
  });
  const { propertyJson } = toRefs(state);
  const schemas: FormSchema[] = [
    {
      field: 'fullName',
      label: '应用名称',
      component: 'Input',
      componentProps: { placeholder: '输入名称', maxlength: 50 },
      rules: [{ required: true, trigger: 'blur', message: '应用不能为空' }],
    },
    {
      field: 'enCode',
      label: '应用编码',
      component: 'Input',
      componentProps: { placeholder: '输入编码', maxlength: 50 },
      rules: [{ required: true, message: '应用编码不能为空', trigger: 'blur' }],
    },
    {
      field: 'icon',
      label: '图标',
      component: 'Input',
      slot: 'icon',
      componentProps: { placeholder: '请选择' },
      rules: [{ required: true, trigger: 'change', message: '应用图标不能为空' }],
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
      componentProps: { rows: 4 },
    },
  ];
  const getTitle = computed(() => (!unref(id) ? '新建应用' : '编辑应用'));
  const emit = defineEmits(['register', 'reload']);
  const { createMessage } = useMessage();
  const [registerForm, { setFieldsValue, validate, resetFields }] = useForm({ labelWidth: 80, schemas: schemas });
  const [registerModal, { closeModal, changeLoading, changeOkLoading }] = useModalInner(init);

  function init(data) {
    resetFields();
    id.value = data.id;
    if (id.value) {
      changeLoading(true);
      getInfo(id.value).then(res => {
        const data = res.data;
        const propertyJson = data.propertyJson ? JSON.parse(data.propertyJson) : null;
        state.propertyJson = propertyJson || { iconBackgroundColor: '' };
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
      propertyJson: JSON.stringify(state.propertyJson),
      id: id.value,
    };
    const formMethod = id.value ? update : create;
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
</script>
