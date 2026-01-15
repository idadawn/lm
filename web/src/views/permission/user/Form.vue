<template>
  <BasicPopup v-bind="$attrs" @register="registerPopup" :title="getTitle" showOkBtn @ok="handleSubmit">
    <BasicForm @register="registerForm" class="!px-30px" />
  </BasicPopup>
</template>
<script lang="ts" setup>
  import { createUser, updateUser, getUserInfo } from '/@/api/permission/user';
  import { getPositionByOrganize } from '/@/api/permission/position';
  import { getRoleByOrganize } from '/@/api/permission/role';
  import { ref, unref, computed } from 'vue';
  import { BasicPopup, usePopupInner } from '/@/components/Popup';
  import { BasicForm, useForm, FormSchema } from '/@/components/Form';
  import { useMessage } from '/@/hooks/web/useMessage';
  import { useBaseStore } from '/@/store/modules/base';
  import formValidate from '/@/utils/formValidate';

  const emit = defineEmits(['register', 'reload']);
  const { createMessage } = useMessage();
  const baseStore = useBaseStore();
  const schemas: FormSchema[] = [
    {
      field: 'groupTitleField1',
      label: '',
      component: 'GroupTitle',
      componentProps: { content: '账户信息', helpMessage: '新用户初始密码默认是0000' },
      colProps: { sm: 24, span: 24 },
    },
    {
      field: 'account',
      label: '账户',
      component: 'Input',
      componentProps: { placeholder: '账户名称', maxlength: 50 },
      rules: [
        { required: true, trigger: 'blur', message: '请输入账户名称' },
        { validator: formValidate('fullName', '账户不能含有特殊符号'), trigger: 'blur' },
      ],
    },
    {
      field: 'realName',
      label: '姓名',
      component: 'Input',
      componentProps: { placeholder: '真实姓名', maxlength: 50 },
      rules: [
        { required: true, trigger: 'blur', message: '请输入真实姓名' },
        { validator: formValidate('fullName', '真实姓名不能含有特殊符号'), trigger: 'blur' },
      ],
    },
    {
      field: 'gender',
      label: '性别',
      component: 'Select',
      componentProps: { placeholder: '选择性别', fieldNames: { value: 'enCode' }, showSearch: true },
      rules: [{ required: true, trigger: 'blur', message: '请选择性别' }],
    },
    {
      field: 'email',
      label: '电子邮箱',
      component: 'Input',
      componentProps: { placeholder: '电子邮箱', maxlength: 50 },
    },
    {
      field: 'organizeIdTree',
      label: '所属组织',
      component: 'OrganizeSelect',
      componentProps: { placeholder: '选择所属组织', multiple: true, auth: true, onChange: onOrgChange },
      rules: [{ required: true, trigger: 'blur', message: '请选择所属组织', type: 'array' }],
    },
    {
      field: 'managerId',
      label: '直属主管',
      component: 'UserSelect',
      componentProps: { placeholder: '选择直属主管' },
    },
    {
      field: 'positionId',
      label: '岗位',
      component: 'Select',
      componentProps: { placeholder: '选择岗位', multiple: true, showSearch: true, fieldNames: { options: 'children' }, onDropdownVisibleChange },
    },
    {
      field: 'roleId',
      label: '角色',
      component: 'Select',
      componentProps: { placeholder: '选择角色', multiple: true, showSearch: true, fieldNames: { options: 'children' }, onDropdownVisibleChange },
    },
    {
      field: 'sortCode',
      label: '排序',
      defaultValue: 0,
      component: 'InputNumber',
      componentProps: {
        min: 0,
        max: 999999,
      },
    },
    {
      field: 'enabledMark',
      label: '状态',
      defaultValue: 1,
      component: 'Select',
      componentProps: {
        placeholder: '选择状态',
        options: [
          { fullName: '启用', id: 1 },
          { fullName: '锁定', id: 2 },
          { fullName: '禁用', id: 0 },
        ],
      },
    },
    {
      field: 'description',
      label: '说明',
      component: 'Textarea',
      colProps: { sm: 24, span: 24 },
      componentProps: { placeholder: '说明', maxlength: 300 },
    },
    {
      field: 'groupTitleField2',
      label: '',
      component: 'GroupTitle',
      componentProps: { content: '个人资料' },
      colProps: { sm: 24, span: 24 },
    },
    {
      field: 'headIcon',
      label: '头像',
      component: 'UploadImgSingle',
      componentProps: { tipText: '上传头像', type: 'userAvatar' },
      colProps: { sm: 24, span: 24 },
    },
    {
      field: 'nation',
      label: '民族',
      component: 'Select',
      componentProps: { placeholder: '选择民族', showSearch: true },
    },
    {
      field: 'nativePlace',
      label: '籍贯',
      component: 'Input',
      componentProps: { placeholder: '籍贯', maxlength: 50 },
    },
    {
      field: 'certificatesType',
      label: '证件类型',
      component: 'Select',
      componentProps: { placeholder: '选择证件类型', showSearch: true },
    },
    {
      field: 'entryDate',
      label: '入职时间',
      component: 'DatePicker',
      componentProps: { placeholder: '选择入职时间' },
    },
    {
      field: 'certificatesNumber',
      label: '证件号码',
      component: 'Input',
      componentProps: { placeholder: '证件号码', maxlength: 50 },
    },
    {
      field: 'education',
      label: '文化程度',
      component: 'Select',
      componentProps: { placeholder: '选择文化程度', showSearch: true },
    },
    {
      field: 'birthday',
      label: '出生年月',
      component: 'DatePicker',
      componentProps: { placeholder: '选择出生年月' },
    },
    {
      field: 'telePhone',
      label: '办公电话',
      component: 'Input',
      componentProps: { placeholder: '办公电话', maxlength: 20 },
    },
    {
      field: 'landline',
      label: '办公座机',
      component: 'Input',
      componentProps: { placeholder: '办公座机', maxlength: 50 },
    },
    {
      field: 'mobilePhone',
      label: '手机号码',
      component: 'Input',
      componentProps: { placeholder: '手机号码', maxlength: 20 },
    },
    {
      field: 'urgentContacts',
      label: '紧急联系',
      component: 'Input',
      componentProps: { placeholder: '紧急联系', maxlength: 50 },
    },
    {
      field: 'urgentTelePhone',
      label: '紧急电话',
      component: 'Input',
      componentProps: { placeholder: '紧急电话', maxlength: 50 },
    },
    {
      field: 'postalAddress',
      label: '通讯地址',
      component: 'Textarea',
      colProps: { sm: 24, span: 24 },
      componentProps: { placeholder: '通讯地址', maxlength: 300 },
    },
  ];
  const [registerForm, { setFieldsValue, resetFields, validate, updateSchema }] = useForm({
    baseColProps: { sm: 12, xs: 24 },
    schemas: schemas,
  });
  const [registerPopup, { closePopup, changeLoading, changeOkLoading }] = usePopupInner(init);
  const id = ref('');
  const organizeIdTree = ref<any[]>([]);

  const getTitle = computed(() => (!unref(id) ? '新建用户' : '编辑用户'));

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
    const sexRes = await baseStore.getDictionaryData('sex');
    updateSchema({ field: 'gender', componentProps: { options: sexRes } });
    const nationRes = await baseStore.getDictionaryData('Nation');
    updateSchema({ field: 'nation', componentProps: { options: nationRes } });
    const educationRes = await baseStore.getDictionaryData('Education');
    updateSchema({ field: 'education', componentProps: { options: educationRes } });
    const certificateTypeRes = await baseStore.getDictionaryData('certificateType');
    updateSchema({ field: 'certificatesType', componentProps: { options: certificateTypeRes } });
  }
  function onDropdownVisibleChange(open) {
    if (!open) return;
    if (!organizeIdTree.value || !organizeIdTree.value.length) createMessage.warning('请先选择所属组织');
  }
  function onOrgChange(val) {
    setFieldsValue({ positionId: [], roleId: [] });
    organizeIdTree.value = val || [];
    if (!val || !val.length) return;
    getOptionsByOrgIds(val);
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
