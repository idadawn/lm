<template>
  <BasicModal v-bind="$attrs" @register="registerModal" :title="getTitle" @ok="handleSubmit">
    <BasicForm @register="registerForm" />
  </BasicModal>
</template>
<script lang="ts" setup>
  import {
    getDictionaryTypeSelector,
    getDictionaryTypeInfo as getInfo,
    createDictionaryType as create,
    updateDictionaryType as update,
  } from '/@/api/systemData/dictionary';
  import { ref, unref, computed } from 'vue';
  import { BasicModal, useModalInner } from '/@/components/Modal';
  import { BasicForm, useForm } from '/@/components/Form';
  import { useMessage } from '/@/hooks/web/useMessage';
  import { useBaseStore } from '/@/store/modules/base';
  import formValidate from '/@/utils/formValidate';

  const emit = defineEmits(['register', 'reload']);
  const [registerForm, { setFieldsValue, resetFields, validate, updateSchema }] = useForm({
    labelWidth: 60,
    schemas: [
      {
        field: 'parentId',
        label: '上级',
        component: 'TreeSelect',
        componentProps: { placeholder: '选择项目上级', showSearch: true },
        rules: [{ required: true, trigger: 'blur', message: '请选择项目上级' }],
      },
      {
        field: 'fullName',
        label: '名称',
        component: 'Input',
        componentProps: { placeholder: '输入名称', maxlength: 50 },
        rules: [
          { required: true, trigger: 'blur', message: '请输入名称' },
          { validator: formValidate('fullName', '名称不能含有特殊符号'), trigger: 'blur' },
        ],
      },
      {
        field: 'enCode',
        label: '编码',
        component: 'Input',
        componentProps: { placeholder: '输入编码', maxlength: 50 },
        rules: [
          { required: true, trigger: 'blur', message: '请输入编码' },
          { validator: formValidate('enCode'), trigger: 'blur' },
        ],
      },
      {
        field: 'isTree',
        label: '树形',
        defaultValue: 0,
        component: 'Switch',
      },
      {
        field: 'sortCode',
        label: '排序',
        defaultValue: 0,
        component: 'InputNumber',
        componentProps: { min: 0, max: 999999 },
      },
      {
        field: 'description',
        label: '说明',
        component: 'Textarea',
        componentProps: { placeholder: '说明' },
      },
    ],
  });
  const [registerModal, { closeModal, changeLoading, changeOkLoading }] = useModalInner(init);
  const id = ref('');
  const treeData = ref([]);
  const { createMessage } = useMessage();
  const baseStore = useBaseStore();

  const getTitle = computed(() => (!unref(id) ? '新建分类' : '编辑分类'));

  function init(data) {
    changeLoading(true);
    resetFields();
    id.value = data.id;
    updateSchema({ field: 'isTree', componentProps: { disabled: !!unref(id) } });
    getDictionaryTypeSelector(data.id).then(res => {
      const item = {
        fullName: '顶级节点',
        hasChildren: true,
        id: '-1',
        children: res.data.list,
      };
      const list = [item];
      treeData.value = list as [];
      updateSchema([
        {
          field: 'parentId',
          componentProps: { options: treeData.value },
        },
      ]);
      if (id.value) {
        getInfo(id.value).then(res => {
          setFieldsValue(res.data);
          changeLoading(false);
        });
      } else {
        changeLoading(false);
      }
    });
  }

  async function handleSubmit() {
    const values = await validate();
    if (!values) return;
    changeOkLoading(true);
    const query = {
      ...values,
      id: id.value,
    };
    const formMethod = id.value ? update : create;
    formMethod(query)
      .then(res => {
        createMessage.success(res.msg);
        changeOkLoading(false);
        baseStore.setDictionaryList();
        closeModal();
        setTimeout(() => {
          emit('reload');
        }, 200);
      })
      .catch(() => {
        changeOkLoading(false);
      });
  }
</script>
