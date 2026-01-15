<template>
  <BasicModal v-bind="$attrs" @register="registerModal" :title="getTitle" @ok="handleSubmit">
    <BasicForm @register="registerForm">
    </BasicForm>
  </BasicModal>
</template>
<script lang="ts" setup>
  import { addIndicator, getMetricgotDetail, getTagSelectorList, putIndicator } from '/@/api/createModel/model';
  import { computed, ref } from "vue";
  import { BasicModal, useModalInner } from '/@/components/Modal';
  import { BasicForm, useForm } from '/@/components/Form';
  import { useMessage } from '/@/hooks/web/useMessage';
  import formValidate from '/@/utils/formValidate';
  import { ResultEnum } from '/@/enums/httpEnum';
  import { GotTypeEnum } from '/@/enums/publicEnum'

  const emit = defineEmits(['register', 'reload']);

  const tagOptions = ref([]);
  const [registerForm, { setFieldsValue, resetFields, validate, clearValidate }] = useForm({
    schemas: [
      {
        field: 'fullName',
        label: '名称',
        component: 'Input',
        componentProps: { placeholder: '输入名称', maxlength: 50 },
        rules: [
          { required: true, trigger: 'blur', message: '请输入名称' },
        ],
      },
      {
        field: 'description',
        label: '描述',
        component: 'Textarea',
        componentProps: { placeholder: '描述' },
      },
      {
        field: 'metricTag',
        label: '标签',
        component: 'Select',
        componentProps: { placeholder: '输入新标签', maxlength: 50, options: tagOptions, multiple: true },
      },
    ],
  });
  const [registerModal, { closeModal, changeLoading, changeOkLoading }] = useModalInner(init);

  const { createMessage } = useMessage();

  const getTitle = computed(() => initRef.value?.id ? '编辑价值链' : '新建价值链');

  const initRef = ref();

  /**
   * 初始化
   */
  function init(data) {
    resetFields();
    initRef.value = data;

    getTagSelectorList().then(res => {
      tagOptions.value = res.data.map(({ id, name }) => ({ id, fullName: name }));
    });

    if (data.id) {
      changeLoading(true);
      getMetricgotDetail(data.id).then(res => {
        const values = {
          ...res.data,
          metricTag: (res.data.metricTag && res.data.metricTag.split(',')) || [],
          fullName: res.data.name,
        };
        setFieldsValue(values);
        changeLoading(false);
      });
    }
  }

  async function handleSubmit() {
    const values = await validate();
    if (!values) return;
    changeOkLoading(true);
    const query = {
      type: GotTypeEnum.cov,
      name: values.fullName,
      imgName: '',
      metricTag: (values.metricTag && values.metricTag.length > 0) ? values.metricTag.join(',') : '',
      description: values.description,
    };
    let formMethod = addIndicator;
    if (initRef.value.id) {
      query['id'] = initRef.value.id;
      formMethod = putIndicator;
    }
    formMethod(query)
      .then(res => {
        createMessage.success(res.msg);
        changeOkLoading(false);
        if (res.code === ResultEnum.SUCCESS) {
          // organizeStore.resetState();
          closeModal();
          setTimeout(() => {
            emit('reload');
          }, 300);
        }
      })
      .catch(() => {
        changeOkLoading(false);
      });
  }
</script>
