<template>
  <BasicPopup v-bind="$attrs" @register="registerPopup" :title="getTitle" showOkBtn @ok="handleSubmit">
    <BasicForm @register="registerForm" class="!px-30px">
      <template #parentId>
        <jnpfOrganizeSelect
          v-model:value="organizeIdTree"
          placeholder="选择上级公司"
          auth
          isOnlyOrg
          @change="onOrganizeChange"
          :currOrgId="id || '0'"
          :parentId="parentId || '0'" />
      </template>
    </BasicForm>
  </BasicPopup>
</template>
<script lang="ts" setup>
  import { create, update, getInfo } from '/@/api/permission/organize';
  import { ref, unref, computed } from 'vue';
  import { BasicPopup, usePopupInner } from '/@/components/Popup';
  import { BasicForm, useForm, FormSchema } from '/@/components/Form';
  import { useMessage } from '/@/hooks/web/useMessage';
  import { useBaseStore } from '/@/store/modules/base';
  import { useOrganizeStore } from '/@/store/modules/organize';
  import formValidate from '/@/utils/formValidate';
  import dayjs, { Dayjs } from 'dayjs';

  const emit = defineEmits(['register', 'reload']);
  const { createMessage } = useMessage();
  const baseStore = useBaseStore();
  const organizeStore = useOrganizeStore();
  const disabledDate = (current: Dayjs) => current && current > dayjs().endOf('day');
  const schemas: FormSchema[] = [
    {
      field: 'groupTitleField1',
      label: '',
      component: 'GroupTitle',
      componentProps: { content: '基础信息' },
      colProps: { sm: 24, span: 24 },
    },
    {
      field: 'parentId',
      label: '上级公司',
      component: 'OrganizeSelect',
      slot: 'parentId',
      rules: [{ required: true, trigger: 'change', message: '上级公司不能为空' }],
    },
    {
      field: 'fullName',
      label: '公司名称',
      component: 'Input',
      componentProps: { placeholder: '输入公司名称', maxlength: 50 },
      rules: [{ required: true, trigger: 'blur', message: '请输入公司名称' }],
    },
    {
      field: 'enCode',
      label: '公司编码',
      component: 'Input',
      componentProps: { placeholder: '输入公司编码', maxlength: 50 },
      rules: [
        { required: true, trigger: 'blur', message: '请输入公司编码' },
        {
          validator: formValidate('enCode', '公司编码只能输入英文、数字和小数点且小数点不能放在首尾'),
          trigger: 'blur',
        },
      ],
    },
    {
      field: 'propertyJson.shortName',
      label: '公司简称',
      component: 'Input',
      componentProps: { placeholder: '输入公司简称' },
    },
    {
      field: 'propertyJson.enterpriseNature',
      label: '公司性质',
      component: 'Select',
      componentProps: { placeholder: '请选择公司性质', showSearch: true },
    },
    {
      field: 'propertyJson.industry',
      label: '所属行业',
      component: 'TreeSelect',
      componentProps: { placeholder: '请选择所属行业', showSearch: true },
    },
    {
      field: 'propertyJson.foundedTime',
      label: '成立时间',
      component: 'DatePicker',
      componentProps: { placeholder: '选择日期', disabledDate },
    },
    {
      field: 'propertyJson.telePhone',
      label: '公司电话',
      component: 'Input',
      componentProps: { placeholder: '输入公司电话' },
    },
    {
      field: 'propertyJson.fax',
      label: '公司传真',
      component: 'Input',
      componentProps: { placeholder: '输入公司传真' },
    },
    {
      field: 'propertyJson.webSite',
      label: '公司主页',
      component: 'Input',
      componentProps: { placeholder: '输入公司主页' },
    },
    {
      field: 'propertyJson.address',
      label: '公司地址',
      component: 'Input',
      componentProps: { placeholder: '输入公司地址' },
      colProps: { sm: 24, span: 24 },
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
      componentProps: { placeholder: '输入说明' },
      colProps: { sm: 24, span: 24 },
    },
    {
      field: 'groupTitleField2',
      label: '',
      component: 'GroupTitle',
      componentProps: { content: '经营信息' },
      colProps: { sm: 24, span: 24 },
    },
    {
      field: 'propertyJson.managerName',
      label: '公司法人',
      component: 'Input',
      componentProps: { placeholder: '输入公司法人' },
    },
    {
      field: 'propertyJson.managerTelePhone',
      label: '联系电话',
      component: 'Input',
      componentProps: { placeholder: '输入联系电话' },
    },
    {
      field: 'propertyJson.managerMobilePhone',
      label: '联系手机',
      component: 'Input',
      componentProps: { placeholder: '输入联系手机' },
    },
    {
      field: 'propertyJson.manageEmail',
      label: '联系邮箱',
      component: 'Input',
      componentProps: { placeholder: '输入联系邮箱' },
    },
    {
      field: 'propertyJson.bankName',
      label: '开户银行',
      component: 'Input',
      componentProps: { placeholder: '输入开户银行' },
    },
    {
      field: 'propertyJson.bankAccount',
      label: '银行账户',
      component: 'Input',
      componentProps: { placeholder: '输入银行账户' },
    },
    {
      field: 'propertyJson.businessscope',
      label: '经营范围',
      component: 'Textarea',
      componentProps: { placeholder: '输入经营范围' },
      colProps: { sm: 24, span: 24 },
    },
  ];
  const [registerForm, { setFieldsValue, resetFields, validate, updateSchema, clearValidate, getFieldsValue }] =
    useForm({
      baseColProps: { sm: 12, xs: 24 },
      schemas: schemas,
    });
  const [registerPopup, { closePopup, changeLoading, changeOkLoading }] = usePopupInner(init);
  const id = ref('');
  const parentId = ref('0');
  const organizeIdTree = ref([]);

  const getTitle = computed(() => (!unref(id) ? '新建公司' : '编辑公司'));

  function init(data) {
    changeLoading(true);
    resetFields();
    id.value = data.id;
    parentId.value = data.parentId || '0';
    organizeIdTree.value = [];
    getOptions();
    if (id.value) {
      getInfo(id.value).then(res => {
        organizeIdTree.value = res.data.organizeIdTree || [];
        const propertyJson = res.data.propertyJson ? JSON.parse(res.data.propertyJson) : {};
        const data = { ...res.data, propertyJson };
        setFieldsValue(data);
        changeLoading(false);
      });
    } else {
      changeLoading(false);
    }
  }
  async function getOptions() {
    // 获取公司性质
    const natureRes = await baseStore.getDictionaryData('EnterpriseNature');
    updateSchema({ field: 'propertyJson.enterpriseNature', componentProps: { options: natureRes } });
    // 获取所属行业
    const industryRes = await baseStore.getDictionaryData('IndustryType');
    updateSchema({ field: 'propertyJson.industry', componentProps: { options: industryRes } });
  }
  function onOrganizeChange(val) {
    setFieldsValue({ parentId: !val || !val.length ? '' : val[val.length - 1] });
    if (!val || !val.length) return;
    clearValidate('parentId');
  }
  async function handleSubmit() {
    const values = await validate();
    if (!values) return;
    changeOkLoading(true);
    const query = {
      enabledMark: 1,
      id: id.value,
      ...values,
      organizeIdTree: organizeIdTree.value,
      ...getFieldsValue(),
    };
    const formMethod = id.value ? update : create;
    formMethod(query)
      .then(res => {
        createMessage.success(res.msg);
        changeOkLoading(false);
        organizeStore.resetState();
        closePopup();
        emit('reload');
      })
      .catch(() => {
        changeOkLoading(false);
      });
  }
</script>
