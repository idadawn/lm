<template>
  <BasicModal v-bind="$attrs" @register="registerModal" :title="getTitle" @ok="handleSubmit" :width="600">
    <BasicForm @register="registerForm">
      <template #parentSelect="{ model, field }">
        <jnpf-tree-select 
          v-model:value="model[field]" 
          :options="parentTreeOptions" 
          allowClear 
          placeholder="请选择父级分类（留空表示顶级分类）"
        />
      </template>
    </BasicForm>
  </BasicModal>
</template>

<script lang="ts" setup>
  import { ref, computed, unref, onMounted } from 'vue';
  import { BasicModal, useModalInner } from '/@/components/Modal';
  import { BasicForm, useForm } from '/@/components/Form';
  import { 
    createAppearanceFeatureCategory, 
    updateAppearanceFeatureCategory,
    getAllAppearanceFeatureCategories,
    AppearanceFeatureCategoryInfo
  } from '/@/api/lab/appearanceCategory';
  import { useMessage } from '/@/hooks/web/useMessage';

  const emit = defineEmits(['success', 'register']);

  const isUpdate = ref(false);
  const rowId = ref('');
  const { createMessage } = useMessage();
  const allCategories = ref<AppearanceFeatureCategoryInfo[]>([]);
  const parentTreeOptions = ref<any[]>([]);

  // 递归获取所有子节点ID（包括子节点的子节点）
  const getAllChildIds = (categoryId: string, categories: AppearanceFeatureCategoryInfo[]): string[] => {
    if (!Array.isArray(categories) || categories.length === 0) {
      return [];
    }
    
    const childIds: string[] = [];
    const findChildren = (node: AppearanceFeatureCategoryInfo) => {
      if (node.children && Array.isArray(node.children) && node.children.length > 0) {
        node.children.forEach(child => {
          childIds.push(child.id);
          findChildren(child);
        });
      }
    };
    
    // 先找到目标节点
    const findNode = (nodes: AppearanceFeatureCategoryInfo[]): AppearanceFeatureCategoryInfo | null => {
      if (!Array.isArray(nodes)) {
        return null;
      }
      for (const node of nodes) {
        if (node.id === categoryId) {
          return node;
        }
        if (node.children && Array.isArray(node.children) && node.children.length > 0) {
          const found = findNode(node.children);
          if (found) return found;
        }
      }
      return null;
    };
    
    const targetNode = findNode(categories);
    if (targetNode) {
      findChildren(targetNode);
    }
    
    return childIds;
  };

  // 将树形数据转换为 jnpf-tree-select 需要的格式
  const convertToTreeSelectOptions = (categories: AppearanceFeatureCategoryInfo[], excludeId?: string): any[] => {
    // 确保 categories 是数组
    if (!Array.isArray(categories)) {
      console.warn('convertToTreeSelectOptions: categories 不是数组', categories);
      return [];
    }
    
    const excludeIds = excludeId ? new Set([excludeId, ...getAllChildIds(excludeId, categories)]) : new Set<string>();
    
    const convertNode = (node: AppearanceFeatureCategoryInfo): any | null => {
      if (excludeIds.has(node.id)) {
        return null;
      }
      
      const option: any = {
        id: node.id,
        fullName: node.name,
        children: node.children ? node.children.map(convertNode).filter(Boolean) : undefined
      };
      
      return option;
    };

    return categories.map(convertNode).filter(Boolean);
  };

  // 加载所有大类数据
  const loadAllCategories = async () => {
    try {
      const res: any = await getAllAppearanceFeatureCategories();
      // 处理不同的响应格式
      let data: AppearanceFeatureCategoryInfo[] = [];
      if (Array.isArray(res)) {
        data = res;
      } else if (res?.data && Array.isArray(res.data)) {
        data = res.data;
      } else if (res?.list && Array.isArray(res.list)) {
        data = res.list;
      } else {
        console.warn('未知的响应格式:', res);
        data = [];
      }
      allCategories.value = data || [];
    } catch (error) {
      console.error('加载大类列表失败:', error);
      allCategories.value = [];
    }
  };

  const [registerForm, { setFieldsValue, resetFields, validate }] = useForm({
    labelWidth: 100,
    schemas: [
      {
        field: 'parentId',
        label: '父级分类',
        component: 'Input',
        slot: 'parentSelect',
        helpMessage: '选择父级分类，留空表示顶级分类',
      },
      {
        field: 'name',
        label: '大类名称',
        component: 'Input',
        required: true,
        componentProps: {
          placeholder: '例如: 韧性、脆边、麻点',
        },
        helpMessage: '必填字段，用于分类外观特性',
      },
    ],
    showActionButtonGroup: false,
  });

  const [registerModal, { setModalProps, closeModal }] = useModalInner(async (data) => {
    resetFields();
    setModalProps({ confirmLoading: false });
    isUpdate.value = !!data?.isUpdate;

    // 加载大类列表
    await loadAllCategories();

    // 确保 allCategories.value 是数组
    if (!Array.isArray(allCategories.value)) {
      console.error('加载的大类数据不是数组:', allCategories.value);
      allCategories.value = [];
    }

    if (unref(isUpdate)) {
      console.log('[编辑分类] 接收到的 data:', data);
      console.log('[编辑分类] data.record:', data.record);
      
      // 检查 record 是否存在
      if (!data.record) {
        console.error('[编辑分类] 缺少 record:', data);
        createMessage.error('编辑失败：缺少分类信息');
        closeModal();
        return;
      }
      
      // 处理可能的 API 响应格式：如果 record 是完整的响应对象，则提取 data 字段
      let record = data.record;
      if (record && record.data && typeof record.data === 'object') {
        console.log('[编辑分类] 检测到 API 响应格式，提取 data 字段');
        record = record.data;
      }
      
      console.log('[编辑分类] 处理后的 record:', record);
      console.log('[编辑分类] record 的所有键:', record ? Object.keys(record) : 'record 为空');
      
      // 尝试多种可能的 ID 字段名称
      const recordId = record.id || record.Id || record.ID || record.F_Id || record.f_Id;
      
      if (!recordId) {
        console.error('[编辑分类] 缺少ID字段，record 内容:', JSON.stringify(record, null, 2));
        createMessage.error('编辑失败：缺少分类ID');
        closeModal();
        return;
      }
      
      rowId.value = recordId;
      console.log('[编辑分类] 设置 rowId:', rowId.value);
      
      // 编辑时，过滤掉当前项及其子项
      parentTreeOptions.value = convertToTreeSelectOptions(allCategories.value, recordId);
      setFieldsValue({
        ...record,
        // 确保 id 字段存在（使用找到的 ID）
        id: recordId,
        // 确保 parentId 正确设置（如果是 null 或 undefined，保持原样，后端会处理为 "-1"）
        parentId: record.parentId || undefined,
      });
    } else {
      // 新增时，显示所有大类
      parentTreeOptions.value = convertToTreeSelectOptions(allCategories.value);
      setFieldsValue({
        parentId: undefined,
      });
    }
  });

  const getTitle = computed(() => (!unref(isUpdate) ? '新增大类' : '编辑大类'));

  async function handleSubmit() {
    try {
      const values = await validate();
      setModalProps({ confirmLoading: true });

      // 确保 parentId 为空字符串时转换为 undefined
      if (values.parentId === '' || values.parentId === null) {
        values.parentId = undefined;
      }

      if (unref(isUpdate)) {
        if (!rowId.value) {
          createMessage.error('缺少分类ID，无法更新');
          return;
        }
        console.log('[编辑分类] 提交 - ID:', rowId.value, 'Values:', values);
        
        // 确保 id 字段正确设置
        const updateData = {
          ...values,
          id: rowId.value,
        };
        
        // 确保 parentId 处理正确（空字符串或 null 转换为 undefined，后端会处理为 "-1"）
        if (updateData.parentId === '' || updateData.parentId === null) {
          updateData.parentId = undefined;
        }
        
        console.log('[编辑分类] 最终提交数据:', updateData);
        await updateAppearanceFeatureCategory(updateData);
      } else {
        await createAppearanceFeatureCategory(values);
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

  onMounted(() => {
    loadAllCategories();
  });
</script>
