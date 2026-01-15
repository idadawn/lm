<template>
  <BasicPopup v-bind="$attrs" @register="registerPopup" :title="getTitle" showOkBtn @ok="handleSubmit">
    <BasicForm @register="registerForm" class="!px-30px" />
    <a-form :model="exampleForm">
      <a-form-item label="格式">
        <a-input v-model:value="exampleForm.value">
          <template #suffix>
            <info-circle-outlined style="color: rgba(0, 0, 0, 0.45)" />
          </template>
        </a-input>
      </a-form-item>
    </a-form>
  </BasicPopup>
</template>
<script lang="ts" setup>
  import { createUser, updateUser, getUserInfo } from '/@/api/permission/user';
  import { getPositionByOrganize } from '/@/api/permission/position';
  import { getRoleByOrganize } from '/@/api/permission/role';
  import { ref, unref, computed, reactive } from 'vue';
  import { BasicPopup, usePopupInner } from '/@/components/Popup';
  import { BasicForm, useForm, FormSchema } from '/@/components/Form';
  import { useMessage } from '/@/hooks/web/useMessage';
  import { useBaseStore } from '/@/store/modules/base';

  const emit = defineEmits(['register', 'reload']);
  const { createMessage } = useMessage();
  const baseStore = useBaseStore();
  const exampleForm = reactive({
    value: '',
  });
  const schemas: FormSchema[] = [
    {
      field: 'account',
      label: '指标名称',
      component: 'Input',
      componentProps: { placeholder: '指标名称', maxlength: 50 },
      rules: [{ required: true, trigger: 'blur', message: '请输入指标名称' }],
    },
    {
      field: 'realName',
      label: '指标编码',
      component: 'Input',
      componentProps: { placeholder: '指标编码', maxlength: 50 },
      rules: [{ required: true, trigger: 'blur', message: '请输入指标编码' }],
    },
    {
      field: 'gender',
      label: '指标目录',
      component: 'Select',
      componentProps: { placeholder: '选择指标目录' },
      rules: [{ required: true, trigger: 'blur', message: '请选择指标目录' }],
    },
    {
      field: 'mobilePhone',
      label: '指标类型',
      component: 'Select',
      componentProps: { placeholder: '选择指标类型' },
      rules: [{ required: true, trigger: 'blur', message: '请选择指标类型' }],
    },
    {
      field: 'organize',
      label: '标签',
      component: 'Select',
      componentProps: { placeholder: '选择标签' },
      rules: [{ required: true, trigger: 'blur', message: '请选择标签' }],
    },
    {
      field: 'roleName',
      label: '创建人',
      component: 'Input',
      componentProps: { placeholder: '创建人', maxlength: 50 },
      rules: [{ required: true, trigger: 'blur', message: '请输入创建人' }],
    },
    {
      field: 'creatorTime',
      label: '日期',
      component: 'DatePicker',
      componentProps: { placeholder: '选择日期' },
    },
    {
      field: 'enabledMark',
      label: '状态',
      defaultValue: 1,
      component: 'Select',
      componentProps: {
        placeholder: '选择状态',
        options: [
          { fullName: '已上线', id: 1 },
          { fullName: '已下线', id: 2 },
        ],
      },
    },
  ];
  const [registerForm, { setFieldsValue, resetFields, validate, updateSchema }] = useForm({
    baseColProps: { sm: 12, xs: 24 },
    schemas: schemas,
  });
  const [registerPopup, { closePopup, changeLoading, changeOkLoading }] = usePopupInner(init);
  const id = ref('');
  const organizeIdTree = ref<any[]>([]);

  const getTitle = computed(() => (!unref(id) ? '新建指标' : '编辑指标'));

  function init(data) {
    changeLoading(true);
    resetFields();
    id.value = data.id;
    updateSchema({ field: 'account', componentProps: { readonly: !!unref(id) } });
    getOptions();
    if (id.value) {
      getUserInfo(id.value).then(res => {
        const data = {
          ...res.data,
          roleId: res.data.roleId ? res.data.roleId.split(',') : [],
          positionId: res.data.positionId ? res.data.positionId.split(',') : [],
        };
        organizeIdTree.value = res.data.organizeIdTree || [];
        if (res.data.organizeIdTree?.length) getOptionsByOrgIds(res.data.organizeIdTree);
        setFieldsValue(data);
        changeLoading(false);
      });
    } else {
      organizeIdTree.value = data.organizeIdTree?.length ? [data.organizeIdTree] : [];
      if (organizeIdTree.value?.length) getOptionsByOrgIds(organizeIdTree.value);
      setFieldsValue({ organizeIdTree: organizeIdTree.value });
      updateSchema({ field: 'positionId', componentProps: { options: [] } });
      updateSchema({ field: 'roleId', componentProps: { options: [] } });
      changeLoading(false);
    }
  }
  async function getOptions() {
    const targetDirectory = (await baseStore.getDictionaryData('targetDirectory')) as any;
    const publishStatus = (await baseStore.getDictionaryData('publishStatus')) as any;
    const label = (await baseStore.getDictionaryData('label')) as any;
    const targetType = (await baseStore.getDictionaryData('targetType')) as any;
    // 指标目录
    targetDirectory.value = targetDirectory as any[];
    updateSchema({ field: 'gender', componentProps: { options: targetDirectory } });
    // 发布状态
    publishStatus.value = publishStatus as any[];
    updateSchema({ field: 'enabledMark', componentProps: { options: publishStatus } });
    // 标签
    label.value = label as any[];
    updateSchema({ field: 'organize', componentProps: { options: label } });
    // 指标类型
    targetType.value = targetType as any[];
    updateSchema({ field: 'mobilePhone', componentProps: { options: targetType } });
  }
  function getOptionsByOrgIds(organizeIdTree) {
    const organizeIds = organizeIdTree.map(o => o[o.length - 1]);
    getPositionByOrganize(organizeIds).then(res => {
      const list = res.data.list.filter(o => o.children && Array.isArray(o.children) && o.children.length);
      updateSchema({ field: 'positionId', componentProps: { options: list } });
    });
    getRoleByOrganize(organizeIds).then(res => {
      const list = res.data.list.filter(o => o.children && Array.isArray(o.children) && o.children.length);
      updateSchema({ field: 'roleId', componentProps: { options: list } });
    });
  }
  async function handleSubmit() {
    const values = await validate();
    if (!values) return;
    changeOkLoading(true);
    const organizeIds = values.organizeIdTree.map(o => o[o.length - 1]);
    const query = {
      ...values,
      id: id.value,
      organizeId: organizeIds.join(),
      positionId: values.positionId && values.positionId.length ? values.positionId.join() : '',
      roleId: values.roleId && values.roleId.length ? values.roleId.join() : '',
    };
    const formMethod = id.value ? updateUser : createUser;
    formMethod(query)
      .then(res => {
        createMessage.success(res.msg);
        changeOkLoading(false);
        closePopup();
        emit('reload');
      })
      .catch(() => {
        changeOkLoading(false);
      });
  }
</script>
