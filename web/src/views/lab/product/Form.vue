<template>
  <BasicModal
    v-bind="$attrs"
    @register="registerModal"
    :title="getTitle"
    @ok="handleSubmit(0)"
    :showContinueBtn="!id"
    @continue="handleSubmit(1)"
    @cancel="handleCancel"
    :width="1200"
    wrapClassName="product-spec-modal"
    :wrapperProps="{ minHeight: 400 }">
    <div class="product-spec-form-content">
      <Tabs default-active-key="1">
        <TabPane key="1" tab="基本信息">
          <BasicForm @register="registerForm"></BasicForm>
          <!-- 版本说明提示（仅编辑时显示） -->
          <div v-if="id && createNewVersion" style="margin: -16px 0 16px 0; padding: 8px 16px; background: #e6f7ff; border-left: 3px solid #1890ff; border-radius: 2px;">
            <p style="margin: 0; color: #666; font-size: 12px;">
              提示：创建新版本后，历史数据将继续使用旧版本，新数据将使用新版本。
            </p>
          </div>
          <ExtendedAttributesForm
            v-model="extendedAttributes"
            @change="handleExtendedAttributesChange" />
        </TabPane>
        <TabPane key="2" tab="版本管理" v-if="id">
          <VersionManage :productSpecId="id" />
        </TabPane>
      </Tabs>
    </div>
  </BasicModal>
</template>
<script lang="ts" setup>
  import { createProductSpec, updateProductSpec, getProductSpecInfo } from '/@/api/lab/product';
  import { getPublicAttributes } from '/@/api/lab/productSpecPublicAttribute';
  import { ref, unref, computed, watch } from 'vue';
  import { BasicModal, useModalInner } from '/@/components/Modal';
  import { BasicForm, useForm } from '/@/components/Form';
  import { useMessage } from '/@/hooks/web/useMessage';
  import ExtendedAttributesForm from './ExtendedAttributesForm.vue';
  import VersionManage from './components/VersionManage.vue';
  import { Tabs, TabPane } from 'ant-design-vue';

  const emit = defineEmits(['register', 'reload']);

  const [registerForm, { setFieldsValue, resetFields, validate, getFieldsValue, updateSchema }] = useForm({
    schemas: [
      {
        field: 'code',
        label: '规格代码',
        component: 'Input',
        componentProps: { placeholder: '如 120' },
        rules: [{ required: true, trigger: 'blur', message: '必填' }],
      },
      {
        field: 'name',
        label: '规格名称',
        component: 'Input',
        componentProps: { placeholder: '请输入规格名称' },
        rules: [{ required: true, trigger: 'blur', message: '必填' }],
      },
      {
        field: 'detectionColumns',
        label: '检测列',
        component: 'Input',
        componentProps: { placeholder: '如 13,15,18,22' },
        rules: [{ required: true, trigger: 'blur', message: '必填' }],
      },
      {
        field: 'createNewVersion',
        label: '创建新版本',
        component: 'Switch',
        componentProps: {
          checkedChildren: '是',
          unCheckedChildren: '否',
        },
        ifShow: false, // 初始隐藏，通过 updateSchema 动态显示
        defaultValue: false,
        valuePropName: 'checked', // Switch 组件使用 checked 属性
      },
      {
        field: 'versionDescription',
        label: '版本说明',
        component: 'Input',
        componentProps: {
          placeholder: '请输入版本说明（可选，如：调整密度参数）',
          maxlength: 500,
          showCount: true,
        },
        ifShow: false, // 初始隐藏，通过 updateSchema 动态显示
      },
      {
        field: 'description',
        label: '描述',
        component: 'InputTextArea',
      } as any,
    ],
  });
  const [registerModal, { closeModal, changeLoading, changeOkLoading, changeContinueLoading }] = useModalInner(init);
  const { createMessage } = useMessage();
  const id = ref('');
  const isContinue = ref<boolean>(false);
  const extendedAttributes = ref<any[]>([]);
  const createNewVersion = ref<boolean>(false);
  const versionDescription = ref<string>('');

  const getTitle = computed(() => (!unref(id) ? '新建产品规格' : '编辑产品规格'));

  // 监听创建新版本选项变化，动态更新版本说明字段的显示
  watch(() => {
    const formValues = getFieldsValue();
    return formValues?.createNewVersion;
  }, (newVal) => {
    if (id.value) {
      createNewVersion.value = newVal === true;
      // 更新版本说明字段的显示
      updateSchema({
        field: 'versionDescription',
        ifShow: newVal === true,
      });
      // 如果取消选择，清空版本说明
      if (!newVal) {
        versionDescription.value = '';
        setFieldsValue({ versionDescription: '' });
      }
    }
  });

  function init(data) {
    changeLoading(true);
    changeOkLoading(false);
    changeContinueLoading(false);
    isContinue.value = false;
    resetFields();
    id.value = data?.id || '';
    // 重置版本升级选项
    createNewVersion.value = false;
    versionDescription.value = '';
    
    // 更新版本相关字段的显示状态
    if (id.value) {
      // 编辑时显示创建新版本字段
      updateSchema([
        {
          field: 'createNewVersion',
          ifShow: true,
        },
        {
          field: 'versionDescription',
          ifShow: false,
        }
      ]);
    } else {
      // 新建时隐藏版本相关字段
      updateSchema([
        {
          field: 'createNewVersion',
          ifShow: false,
        },
        {
          field: 'versionDescription',
          ifShow: false,
        }
      ]);
    }
    
    if (id.value) {
      getProductSpecInfo(id.value).then(res => {
        const { attributes, ...basicData } = res.data;
        // 设置表单值，包括版本相关字段
        setFieldsValue({
          ...basicData,
          createNewVersion: true, // 编辑模式下默认选"是"
          versionDescription: '',
        });
        // 加载扩展属性（从属性实体列表）
        if (attributes && Array.isArray(attributes)) {
          extendedAttributes.value = attributes;
        } else {
          // 如果没有属性列表，初始化为空数组
          extendedAttributes.value = [];
        }
        changeLoading(false);
      });
    } else {
      // 新建时加载公共属性
      extendedAttributes.value = [];
      loadPublicAttributes();
      changeLoading(false);
    }
  }

  function handleExtendedAttributesChange(values) {
    extendedAttributes.value = values;
  }

  // 加载公共属性
  async function loadPublicAttributes() {
    try {
      const res = await getPublicAttributes();
      if (res && res.data && Array.isArray(res.data) && res.data.length > 0) {
        // 将公共属性转换为扩展属性格式
        const publicAttrs = res.data.map(attr => ({
          attributeKey: attr.attributeKey || attr.key,
          attributeName: attr.attributeName || attr.label,
          valueType: attr.valueType || attr.type || 'text',
          attributeValue: attr.defaultValue || attr.value || '',
          unit: attr.unit || null,
          precision: attr.precision || (attr.valueType === 'decimal' ? 2 : 0),
          sortCode: attr.sortCode || 0
        }));
        extendedAttributes.value = publicAttrs;
      } else if (res && Array.isArray(res) && res.length > 0) {
        // 兼容直接返回数组的情况
        const publicAttrs = res.map(attr => ({
          attributeKey: attr.attributeKey || attr.key,
          attributeName: attr.attributeName || attr.label,
          valueType: attr.valueType || attr.type || 'text',
          attributeValue: attr.defaultValue || attr.value || '',
          unit: attr.unit || null,
          precision: attr.precision || (attr.valueType === 'decimal' ? 2 : 0),
          sortCode: attr.sortCode || 0
        }));
        extendedAttributes.value = publicAttrs;
      }
    } catch (error) {
      console.error('加载公共属性失败', error);
    }
  }

  async function handleSubmit(type) {
    const changeLoadingMethod = type == 1 ? changeContinueLoading : changeOkLoading;
    try {
      const values = await validate();
      if (!values) {
        console.warn('表单验证失败');
        return;
      }
      changeLoadingMethod(true);

    // 从表单值中获取版本相关字段
    const formValues = getFieldsValue();
    const shouldCreateVersion = formValues?.createNewVersion === true || createNewVersion.value;
    const versionDesc = formValues?.versionDescription || versionDescription.value || '';

    // 构建请求数据
    const query = {
      ...values,
      id: id.value,
      // 将扩展属性列表发送给后端
      attributes: extendedAttributes.value,
      // 版本升级选项（仅编辑时）
      ...(id.value ? {
        createNewVersion: shouldCreateVersion,
        versionDescription: shouldCreateVersion ? versionDesc : undefined
      } : {})
    };

    const formMethod = id.value ? updateProductSpec : createProductSpec;
    formMethod(query)
      .then(res => {
        createMessage.success(res.msg);
        changeLoadingMethod(false);
        if (type == 1) {
          resetFields();
          // 重置扩展属性并重新加载公共属性
          extendedAttributes.value = [];
          loadPublicAttributes();
          isContinue.value = true;
        } else {
          closeModal();
          emit('reload');
        }
      })
      .catch((error) => {
        console.error('提交失败:', error);
        changeLoadingMethod(false);
        // 错误信息会在 useMessage 中自动显示
      });
    } catch (error) {
      console.error('表单验证错误:', error);
      changeLoadingMethod(false);
    }
  }
  function handleCancel() {
    if (isContinue.value == true) emit('reload');
  }
</script>

<style scoped lang="less">
.product-spec-form-content {
  padding: 0;
  min-height: 300px;
  
  :deep(.ant-form) {
    margin-bottom: 0;
  }
}
</style>

<style lang="less">
// 全局样式，确保产品规格模态框有足够的高度
.ant-modal.product-spec-modal {
  .ant-modal-body {
    min-height: 400px;
    max-height: 70vh;
    
    & > .scrollbar {
      min-height: 400px;
      
      & > .scrollbar__wrap {
        & > .scrollbar__view {
          min-height: 400px;
        }
      }
    }
  }
}
</style>