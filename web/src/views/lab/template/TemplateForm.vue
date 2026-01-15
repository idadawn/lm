<template>
  <BasicModal
    v-bind="$attrs"
    @register="registerModal"
    :title="getTitle"
    @ok="handleSubmit"
    @cancel="handleCancel"
    :width="800">
    <BasicForm @register="registerForm"></BasicForm>
  </BasicModal>
</template>

<script lang="ts" setup>
import { createExcelTemplate, updateExcelTemplate, getExcelTemplateById, validateTemplateConfig } from '/@/api/lab/excelTemplate';
import { ref, unref, computed } from 'vue';
import { BasicModal, useModalInner } from '/@/components/Modal';
import { BasicForm, useForm } from '/@/components/Form';
import { useMessage } from '/@/hooks/web/useMessage';

const emit = defineEmits(['register', 'reload']);

// 产品规格选项（可以从API动态获取）
const productSpecOptions = ref<any[]>([]);

const [registerForm, { setFieldsValue, resetFields, validate, getFieldsValue }] = useForm({
  schemas: [
    {
      field: 'templateName',
      label: '模板名称',
      component: 'Input',
      componentProps: {
        placeholder: '如：标准产品导入模板',
      },
      rules: [{ required: true, trigger: 'blur', message: '必填' }],
    },
    {
      field: 'templateCode',
      label: '模板编码',
      component: 'Input',
      componentProps: { placeholder: '如：STANDARD_IMPORT（唯一编码）' },
      rules: [{ required: true, trigger: 'blur', message: '必填' }],
    },
    {
      field: 'description',
      label: '模板描述',
      component: 'InputTextArea',
      componentProps: {
        placeholder: '描述模板的用途和特点',
        rows: 2,
      },
    },
    {
      field: 'templateType',
      label: '模板类型',
      component: 'Select',
      componentProps: {
        placeholder: '请选择模板类型',
        options: [
          { label: '系统模板', value: 'system' },
          { label: '个人模板', value: 'user' },
        ],
        fieldNames: { label: 'label', value: 'value' },
      },
      rules: [{ required: true, trigger: 'change', message: '必填' }],
    },
    {
      field: 'productSpecId',
      label: '关联产品规格',
      component: 'Select',
      componentProps: {
        placeholder: '请选择关联的产品规格（可选）',
        options: productSpecOptions.value,
        showSearch: true,
        filterOption: (input: string, option: any) => {
          return option.label.toLowerCase().indexOf(input.toLowerCase()) >= 0;
        },
      },
    },
    {
      field: 'isDefault',
      label: '默认模板',
      component: 'Switch',
      componentProps: {
        checkedChildren: '是',
        unCheckedChildren: '否',
      },
      helpMessage: '设为默认模板后，导入时无匹配模板将自动使用此模板',
    },
    {
      field: 'sortCode',
      label: '排序码',
      component: 'InputNumber',
      componentProps: {
        placeholder: '用于排序，数字越小越靠前',
        min: 0,
      },
    },
    {
      field: 'configJson',
      label: '模板配置',
      component: 'InputTextArea',
      componentProps: {
        placeholder: '请输入JSON格式的模板配置',
        rows: 12,
        style: { fontFamily: 'Consolas, Monaco, "Courier New", monospace' },
        onChange: (e: any) => {
          const configJson = e.target?.value || e;
          if (configJson) {
            // 实时验证JSON格式（前端基本验证）
            try {
              JSON.parse(configJson);
              setFieldsValue({ configJsonError: false });
            } catch (error) {
              setFieldsValue({ configJsonError: true });
            }
          }
        },
      },
      rules: [
        { required: true, trigger: 'blur', message: '必填' },
        {
          validator: async (_rule: any, value: string) => {
            if (!value) return Promise.resolve();

            try {
              JSON.parse(value);
              // 调用后端验证API
              await validateTemplateConfig(value);
              return Promise.resolve();
            } catch (error: any) {
              const errorMsg = error?.response?.data?.msg || error?.message || 'JSON格式错误';
              return Promise.reject(errorMsg);
            }
          },
          trigger: 'blur',
        },
      ],
      helpMessage: 'JSON格式的模板配置，包含字段映射、验证规则等。可使用JSON编辑器编辑。',
    },
  ],
});

const [registerModal, { closeModal, changeLoading, changeOkLoading }] = useModalInner(init);
const { createMessage } = useMessage();
const id = ref('');
const isNew = ref(true);

const getTitle = computed(() => (!unref(id) ? '新建导入模板' : '编辑导入模板'));

async function init(data: any) {
  changeLoading(true);
  changeOkLoading(false);
  resetFields();
  id.value = data.id || '';
  isNew.value = !id.value;

  // 加载产品规格选项（这里需要调用API获取产品规格列表）
  // loadProductSpecs();

  if (id.value) {
    // 编辑模式：加载模板数据
    try {
      const response = await getExcelTemplateById(id.value);
      const templateData = response.data || response;

      // 设置表单值
      setFieldsValue({
        templateName: templateData.templateName,
        templateCode: templateData.templateCode,
        description: templateData.description,
        templateType: templateData.templateType,
        productSpecId: templateData.productSpecId,
        isDefault: templateData.isDefault,
        sortCode: templateData.sortCode,
        configJson: templateData.configJson,
      });
    } catch (error) {
      console.error('加载模板数据失败:', error);
      createMessage.error('加载模板数据失败');
    }
  } else {
    // 新建模式：设置默认值
    setFieldsValue({
      templateType: 'system',
      isDefault: 0,
    });
  }

  changeLoading(false);
}

// 加载产品规格列表
async function loadProductSpecs() {
  // TODO: 调用产品规格API获取列表
  // const response = await getProductSpecs();
  // productSpecOptions.value = (response.data || response).map((spec: any) => ({
  //   label: `${spec.name} (${spec.code})`,
  //   value: spec.id,
  // }));
}

async function handleSubmit() {
  try {
    changeOkLoading(true);

    const values = await validate();
    if (!values) return;

    // 验证配置JSON
    if (values.configJson) {
      try {
        await validateTemplateConfig(values.configJson);
      } catch (error: any) {
        const errorMsg = error?.response?.data?.msg || error?.message || '模板配置验证失败';
        createMessage.error(errorMsg);
        changeOkLoading(false);
        return;
      }
    }

    if (isNew.value) {
      await createExcelTemplate(values);
      createMessage.success('创建成功');
    } else {
      await updateExcelTemplate(id.value, values);
      createMessage.success('更新成功');
    }

    closeModal();
    emit('reload');
  } catch (error: any) {
    const errorMsg = error?.response?.data?.msg || error?.message || '保存失败';
    createMessage.error(errorMsg);
  } finally {
    changeOkLoading(false);
  }
}

function handleCancel() {
  closeModal();
}
</script>