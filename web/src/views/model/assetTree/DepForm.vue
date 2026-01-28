<template>
  <BasicModal v-bind="$attrs" @register="registerModal" :title="getTitle" @ok="handleSubmit">
    <BasicForm @register="registerForm">
      <template #parentId="{ model, field }">
        <jnpf-tree-select v-model:value="model[field]" :options="treeData" allowClear />
      </template>
    </BasicForm>
  </BasicModal>
</template>
<script lang="ts" setup>
  import {
    postMetriccategory,
    putMetriccategory,
    getMetriccategory,
    getMetriccategoryList,
  } from '/@/api/targetDirectory';
  import { ref, unref, computed } from 'vue';
  import { BasicModal, useModalInner } from '/@/components/Modal';
  import { BasicForm, useForm } from '/@/components/Form';
  import { useMessage } from '/@/hooks/web/useMessage';

  const emit = defineEmits(['register', 'reload']);
  const [registerForm, { setFieldsValue, resetFields, validate, clearValidate }] = useForm({
    schemas: [
      {
        field: 'parentId',
        label: '上级目录',
        component: 'TreeSelect',
        slot: 'parentId',
        rules: [{ required: true, trigger: 'change', message: '上级目录不能为空' }],
      },
      {
        field: 'fullName',
        label: '目录名称',
        component: 'Input',
        componentProps: { placeholder: '输入名称', maxlength: 50 },
        rules: [{ required: true, trigger: 'blur', message: '请输入目录名称' }],
      },
      {
        field: 'ownId',
        label: '所有者',
        component: 'UserSelect',
        componentProps: { placeholder: '选择所有者' },
        rules: [{ required: true, trigger: 'blur', message: '选择所有者' }],
      },
      {
        field: 'description',
        label: '描述',
        component: 'Textarea',
        componentProps: { placeholder: '说明' },
      },
    ],
  });
  const [registerModal, { closeModal, changeLoading, changeOkLoading }] = useModalInner(init);
  const id = ref('');
  const treeData = ref([{}]);

  const { createMessage } = useMessage();

  const getTitle = computed(() => (!unref(id) ? '新建目录' : '编辑目录'));

  function init(data) {
    getMetriccategoryList({}).then(res => {
      treeData.value = [{ fullName: '顶级节点', id: '-1', children: res.data }];
    });

    resetFields();
    id.value = data.id;
    if (id.value) {
      changeLoading(true);
      getMetriccategory(id.value).then(res => {
        setFieldsValue(res.data);
        changeLoading(false);
      });
    }
  }

  async function handleSubmit() {
    const values = await validate();
    // 
    if (!values) return;
    changeOkLoading(true);

    const query = {
      ...values,
      id: id.value,
    };
    const formMethod = id.value ? putMetriccategory : postMetriccategory;

    formMethod(query)
      .then(res => {
        createMessage.success(res.msg);
        changeOkLoading(false);
        closeModal();
        setTimeout(() => {
          emit('reload');
        }, 300);
      })
      .catch(() => {
        changeOkLoading(false);
      });
  }
</script>
