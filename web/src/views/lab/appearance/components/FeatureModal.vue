<template>
  <BasicModal 
    v-bind="$attrs" 
    @register="registerModal" 
    :title="getTitle" 
    @ok="handleSubmit"
    @cancel="handleCancel"
    :width="800"
    :height="550"
    :minHeight="400">
    <BasicForm @register="registerForm">
      <template #keywordsInput="{ model, field }">
        <div class="keywords-tag-container">
          <a-input
            v-model:value="keywordInput"
            placeholder="输入关键词后按回车添加"
            @press-enter="handleAddKeyword"
            @blur="handleAddKeyword"
            allow-clear>
            <template #suffix>
              <span class="keyword-count">{{ model[field]?.length || 0 }} 个关键词</span>
            </template>
          </a-input>
          <div class="keywords-tags" v-if="model[field] && model[field].length > 0">
            <a-tag
              v-for="(keyword, index) in model[field]"
              :key="index"
              closable
              @close="handleRemoveKeyword(model, field, index)"
              color="blue">
              {{ keyword }}
            </a-tag>
          </div>
        </div>
      </template>
    </BasicForm>
  </BasicModal>
</template>

<script lang="ts" setup>
  import { ref, computed, unref, onMounted, nextTick } from 'vue';
  import { BasicModal, useModalInner } from '/@/components/Modal';
  import { BasicForm, useForm } from '/@/components/Form';
  import { createAppearanceFeature, updateAppearanceFeature, getAppearanceFeatureList } from '/@/api/lab/appearance';
  import { getEnabledSeverityLevels, SeverityLevelInfo } from '/@/api/lab/severityLevel';
  import { getAllAppearanceFeatureCategories, createAppearanceFeatureCategory, AppearanceFeatureCategoryInfo } from '/@/api/lab/appearanceCategory';
  import { useMessage } from '/@/hooks/web/useMessage';

  const emit = defineEmits(['success', 'register']);
  const isUpdate = ref(true);
  const rowId = ref('');
  const { createMessage } = useMessage();
  const severityLevels = ref<SeverityLevelInfo[]>([]);
  const categories = ref<AppearanceFeatureCategoryInfo[]>([]);
  const keywordInput = ref('');

  const getSeverityOptions = computed(() => {
    return severityLevels.value.map(level => ({
      fullName: level.description ? `${level.name} (${level.description})` : level.name,
      id: level.name,
    }));
  });

  // 将特性大类数据转换为树形数据格式（用于TreeSelect）
  const categoryTreeData = computed(() => {
    return convertToTreeData(categories.value);
  });

  // 转换函数：将扁平或嵌套的categories转换为TreeSelect需要的格式
  function convertToTreeData(cats: AppearanceFeatureCategoryInfo[]): any[] {
    if (!cats || cats.length === 0) {
      return [];
    }
    
    const convertNode = (node: AppearanceFeatureCategoryInfo): any => {
      const treeNode: any = {
        id: node.id,
        fullName: node.name,
        name: node.name,
        value: node.id,
      };
      
      if (node.children && Array.isArray(node.children) && node.children.length > 0) {
        treeNode.children = node.children.map(convertNode);
      }
      
      return treeNode;
    };
    
    return cats.map(convertNode);
  }

  const [registerForm, { setFieldsValue, resetFields, validate, getFieldsValue, updateSchema, getSchema }] = useForm({
    labelWidth: 100,
    baseColProps: { span: 24 },
    schemas: [
      {
        field: 'categoryId',
        label: '特性大类',
        component: 'TreeSelect',
        required: true,
        componentProps: {
          placeholder: '请选择特性大类',
          showSearch: true,
          allowClear: true,
          options: categoryTreeData,
          fieldNames: {
            value: 'id',
            label: 'name',
            children: 'children',
          },
          treeDefaultExpandAll: true,
        },
      },
      {
        field: 'name',
        label: '特性名称',
        component: 'Input',
        required: true,
        componentProps: {
          placeholder: '例如: 脆',
        },
        helpMessage: '必填字段，用于统计和分类，例如：脆、皱、痕等。特性名称必须唯一，不能重复。',
        colProps: { span: 12 },
        rules: [
          {
            validator: async (rule, value) => {
              if (!value || !value.trim()) {
                return Promise.resolve();
              }
              const trimmedValue = value.trim();
              try {
                // 检查特性名称是否已存在
                const res = await getAppearanceFeatureList({ name: trimmedValue });
                let features = [];
                if (Array.isArray(res)) {
                  features = res;
                } else if (res?.data && Array.isArray(res.data)) {
                  features = res.data;
                } else if (res?.list && Array.isArray(res.list)) {
                  features = res.list;
                }
                
                // 如果是编辑模式，排除当前记录
                if (unref(isUpdate) && rowId.value) {
                  features = features.filter(f => f.id !== rowId.value);
                }
                
                if (features.length > 0) {
                  return Promise.reject('特性名称已存在，不能重复');
                }
                return Promise.resolve();
              } catch (error) {
                console.error('检查特性名称唯一性失败:', error);
                // 验证失败时不阻止提交，让后端再次验证
                return Promise.resolve();
              }
            },
            trigger: 'blur',
          },
        ],
      },
      {
        field: 'severity',
        label: '特征等级',
        component: 'Select',
        required: true,
        componentProps: {
          placeholder: '请选择特征等级',
          showSearch: true,
          allowClear: true,
          options: [],
          fieldNames: {
            label: 'fullName',
            value: 'id',
          },
        },
        defaultValue: undefined,
        colProps: { span: 12 },
      },
      {
        field: 'keywords',
        label: '关键词',
        component: 'Input',
        slot: 'keywordsInput',
        helpMessage: '用于精确匹配的关键词列表，例如：脆、碎、易断、发脆。输入关键词后按回车或失去焦点时自动添加',
      },
      {
        field: 'description',
        label: '简单描述',
        component: 'InputTextArea',
        componentProps: {
          placeholder: '例如: 容易碎裂...',
          rows: 3,
        },
      } as any,
    ],
    showActionButtonGroup: false,
    actionColOptions: {
      span: 23,
    },
  });

  const [registerModal, { setModalProps, closeModal }] = useModalInner(async data => {
    resetFields();
    setModalProps({ confirmLoading: false, loading: true });
    isUpdate.value = !!data?.isUpdate;
    keywordInput.value = '';

    // 重新加载特性大类列表和特征等级列表
    await Promise.all([loadCategories(), loadSeverityLevels()]);
    
    // 确保数据加载完成后再设置表单值
    await nextTick();
    
    // 再次确保 schema 已更新（防止 resetFields 重置了 schema）
    if (severityLevels.value.length > 0) {
      const options = getSeverityOptions.value;
      updateSchema({
        field: 'severity',
        componentProps: {
          options: options,
          fieldNames: {
            label: 'fullName',
            value: 'id',
          },
        },
      });
      console.log('模态框打开后重新更新特征等级选项，共', options.length, '个选项');
    }
    
    setModalProps({ loading: false });

    if (unref(isUpdate)) {
      rowId.value = data.record.id;
      
      // 获取特征等级名称
      // 优先使用 severityLevel 字段（名称），如果没有则根据 severityLevelId 查找
      let severityValue = data.record.severityLevel;
      if (!severityValue && data.record.severityLevelId) {
        const severityLevel = severityLevels.value.find(l => l.id === data.record.severityLevelId);
        if (severityLevel) {
          severityValue = severityLevel.name;
        }
      }
      // 如果还是没有，尝试从 variantList 获取
      if (!severityValue && data.record.variantList && data.record.variantList.length > 0) {
        severityValue = data.record.variantList[0]?.severity;
      }
      
      // 处理关键词：将 JSON 字符串转换为数组
      let keywordsArray: string[] = [];
      if (data.record.keywords) {
        try {
          keywordsArray = typeof data.record.keywords === 'string' 
            ? JSON.parse(data.record.keywords) 
            : data.record.keywords;
        } catch (e) {
          // 如果解析失败，尝试按逗号分割
          keywordsArray = data.record.keywords.split(',').map((k: string) => k.trim()).filter((k: string) => k);
        }
      }
      
      setFieldsValue({
        ...data.record,
        categoryId: data.record.categoryId,
        severity: severityValue,
        keywords: keywordsArray,
      });
    } else {
      // 新增时，如果传入了 categoryId，直接使用
      setFieldsValue({
        categoryId: data?.categoryId || undefined,
        severity: undefined,
        keywords: [],
      });
      keywordInput.value = '';
    }
  });

  const getTitle = computed(() => (!unref(isUpdate) ? '新增外观特性' : '编辑外观特性'));

  function handleCancel() {
    closeModal();
  }

  // 添加关键词
  function handleAddKeyword() {
    if (!keywordInput.value || !keywordInput.value.trim()) {
      return;
    }
    
    const keyword = keywordInput.value.trim();
    const values = getFieldsValue();
    const keywords = values.keywords || [];
    
    // 检查是否已存在
    if (!keywords.includes(keyword)) {
      keywords.push(keyword);
      setFieldsValue({ keywords });
    }
    
    keywordInput.value = '';
  }

  // 删除关键词
  function handleRemoveKeyword(model: any, field: string, index: number) {
    const keywords = [...(model[field] || [])];
    keywords.splice(index, 1);
    setFieldsValue({ [field]: keywords });
  }

  async function handleSubmit() {
    try {
      const values = await validate();
      
      // 将 severity 名称转换为 severityLevelId
      if (values.severity) {
        const severityLevel = severityLevels.value.find(l => l.name === values.severity);
        if (severityLevel) {
          values.severityLevelId = severityLevel.id;
        } else {
          createMessage.error('未找到对应的特性等级');
          return;
        }
      }
      
      // 删除 severity 字段，只保留 severityLevelId
      delete values.severity;
      
      // 处理关键词：将数组转换为 JSON 字符串
      if (values.keywords && Array.isArray(values.keywords)) {
        values.keywords = JSON.stringify(values.keywords);
      }
      
      setModalProps({ confirmLoading: true });
      if (unref(isUpdate)) {
        await updateAppearanceFeature({ ...values, id: rowId.value });
      } else {
        await createAppearanceFeature(values);
      }
      createMessage.success(unref(isUpdate) ? '编辑成功' : '新增成功');
      closeModal();
      emit('success');
    } catch (error: any) {
      console.error('保存失败:', error);
      const errorMsg = error?.response?.data?.msg || error?.message || (unref(isUpdate) ? '编辑失败' : '新增失败');
      createMessage.error(errorMsg);
    } finally {
      setModalProps({ confirmLoading: false });
    }
  }

  // 加载特征等级列表
  const loadSeverityLevels = async (updateSchemaOnLoad = true) => {
    try {
      console.log('开始加载特征等级列表...');
      const res: any = await getEnabledSeverityLevels();
      console.log('特征等级API响应:', res);
      
      // 处理多种可能的响应格式
      let levels: SeverityLevelInfo[] = [];
      if (Array.isArray(res)) {
        // 直接返回数组
        levels = res;
      } else if (res && typeof res === 'object') {
        // 包装在对象中
        if (Array.isArray(res.data)) {
          levels = res.data;
        } else if (Array.isArray(res.list)) {
          levels = res.list;
        } else if (Array.isArray(res.result)) {
          levels = res.result;
        } else if (Array.isArray(res.items)) {
          levels = res.items;
        }
      }
      
      severityLevels.value = levels;
      console.log('特征等级数据:', severityLevels.value);
      console.log('特征等级数据数量:', severityLevels.value.length);
      console.log('特征等级选项:', getSeverityOptions.value);
      
      // 确保数据加载后再更新 schema（虽然已经在 schema 中使用 computed，但为了确保更新，还是调用一次）
      if (severityLevels.value.length > 0) {
        const options = getSeverityOptions.value;
        console.log('特征等级数据已加载，共', severityLevels.value.length, '条');
        console.log('特征等级选项:', options);
        
        // 只有在需要更新 schema 且表单已注册的情况下才更新
        if (updateSchemaOnLoad) {
          // 使用 nextTick 确保在 DOM 更新后执行
          await nextTick();
          
          // 更新表单 schema 中的选项（确保组件能响应数据变化）
          try {
            updateSchema({
              field: 'severity',
              componentProps: {
                options: options,
                fieldNames: {
                  label: 'fullName',
                  value: 'id',
                },
              },
            });
            console.log('特征等级选项已更新，共', options.length, '个选项');
          } catch (error) {
            // 表单还未注册，这是正常的（例如在 onMounted 时调用）
            console.debug('表单尚未注册，跳过 schema 更新（将在模态框打开时更新）');
          }
        }
      } else {
        console.warn('特征等级数据为空，请检查API响应格式');
      }
    } catch (error) {
      console.error('加载特征等级失败:', error);
      severityLevels.value = [];
    }
  };

  // 加载特性大类列表
  const loadCategories = async (updateSchemaOnLoad = true) => {
    try {
      const res: any = await getAllAppearanceFeatureCategories();
      if (Array.isArray(res)) {
        categories.value = res;
      } else if (res?.data && Array.isArray(res.data)) {
        categories.value = res.data;
      } else {
        categories.value = [];
      }
      // 只有在需要更新 schema 且表单已注册的情况下才更新
      if (updateSchemaOnLoad) {
        try {
          // 更新特性大类树形数据
          updateSchema({
            field: 'categoryId',
            componentProps: {
              options: categoryTreeData.value,
            },
          });
        } catch (error) {
          // 表单还未注册，这是正常的（例如在 onMounted 时调用）
          console.debug('表单尚未注册，跳过 schema 更新（将在模态框打开时更新）');
        }
      }
    } catch (error) {
      console.error('加载特性大类列表失败:', error);
      categories.value = [];
    }
  };


  onMounted(() => {
    // 预加载数据，但不更新 schema（表单还未注册）
    // schema 的更新会在模态框打开时（useModalInner 回调中）进行
    loadSeverityLevels(false).catch(err => {
      console.debug('组件挂载时加载特征等级失败（这是正常的，将在模态框打开时重试）:', err);
    });
    loadCategories(false).catch(err => {
      console.debug('组件挂载时加载特性大类失败（这是正常的，将在模态框打开时重试）:', err);
    });
  });
</script>

<style scoped>
  /* 优化表单标签和输入框之间的间距 */
  :deep(.ant-form-item-label) {
    padding-right: 16px !important;
  }
  
  :deep(.ant-form-item-label > label) {
    margin-right: 0 !important;
  }

  /* 优化特征等级区域的样式 */
  :deep(.ant-form-item) {
    margin-bottom: 16px;
  }

  /* 优化表单内容区域 */
  :deep(.basic-form) {
    min-height: auto;
  }

  /* 优化表单内容区域 */
  :deep(.ant-form) {
    padding: 0;
  }

  /* 关键词标签容器样式 */
  .keywords-tag-container {
    width: 100%;
  }

  .keywords-tags {
    margin-top: 8px;
    min-height: 32px;
    display: flex;
    flex-wrap: wrap;
    gap: 8px;
    padding: 8px;
    border: 1px solid #d9d9d9;
    border-radius: 4px;
    background-color: #fafafa;
  }

  .keywords-tags:empty {
    display: none;
  }

  .keyword-count {
    font-size: 12px;
    color: #8c8c8c;
    margin-right: 4px;
  }

  :deep(.keywords-tags .ant-tag) {
    margin: 0;
    padding: 4px 8px;
    font-size: 13px;
  }
</style>
