<template>
  <div class="extended-attributes-form">
    <a-divider orientation="left">扩展属性</a-divider>

    <!-- 表格布局 -->
    <div class="attributes-table-wrapper">
      <a-table :dataSource="localAttributes" :columns="columns" :pagination="false"
        :rowKey="(record, index) => record.key || `attr-${index}`" size="small" bordered>
        <template #bodyCell="{ column, record, index }">
          <template v-if="column.key === 'attributeName'">
            <span>{{ record.label }}</span>
            <span v-if="record.unit || record.unitId" class="unit-text">
              ({{ record.unit || '加载中...' }})
            </span>
          </template>
          <template v-else-if="column.key === 'value'">
            <a-input-number v-if="record.type === 'decimal' || record.type === 'int'" v-model:value="record.value"
              :placeholder="`请输入${record.label}`" :precision="record.type === 'decimal' ? (record.precision ?? 2) : 0"
              :step="record.type === 'int' ? 1 : 0.01" style="width: 100%" @change="handleAttributeChange" />
            <a-input v-else v-model="record.value" :placeholder="`请输入${record.label}`" style="width: 100%"
              @change="handleAttributeChange" />
          </template>
          <template v-else-if="column.key === 'type'">
            <a-tag :color="getTypeColor(record.type)">
              {{ getTypeText(record.type) }}
            </a-tag>
          </template>
          <template v-else-if="column.key === 'action'">
            <a-space>
              <a-button type="link" size="small" @click="editAttribute(index)" :icon="h(EditOutlined)">
                编辑
              </a-button>
            </a-space>
          </template>
        </template>
      </a-table>
    </div>

    <!-- 空状态 -->
    <a-empty v-if="localAttributes.length === 0" description="暂无扩展属性" :image="false" style="margin: 20px 0" />

    <!-- 添加/编辑属性对话框 -->
    <a-modal v-model:visible="showAddModal" :title="editingIndex !== null ? '编辑扩展属性' : '添加扩展属性'"
      @ok="handleAddAttribute" @cancel="resetAddForm">
      <a-form :model="addForm" :label-col="{ span: 6 }" :wrapper-col="{ span: 16 }">
        <a-form-item label="属性名称" required>
          <a-input v-model:value="addForm.label" placeholder="请输入属性名称" @change="generateKey" />
        </a-form-item>
        <a-form-item label="属性键名" required>
          <a-input v-model:value="addForm.key" placeholder="属性键名（自动生成为英文）" />
        </a-form-item>
        <a-form-item label="属性类型" required>
          <a-select v-model:value="addForm.type" @change="handleTypeChange">
            <a-select-option value="decimal">小数</a-select-option>
            <a-select-option value="int">整数</a-select-option>
            <a-select-option value="text">文本</a-select-option>
          </a-select>
        </a-form-item>
        <a-form-item v-if="addForm.type === 'decimal'" label="精度">
          <a-input-number v-model:value="addForm.precision" :min="0" :max="10" placeholder="小数位数" />
        </a-form-item>
        <a-form-item label="单位">
          <UnitSelect v-model="addForm.unitId" :placeholder="'请选择单位（留空表示无单位）'" @change="handleUnitChange" />
        </a-form-item>
        <a-form-item label="属性值">
          <a-input-number v-if="addForm.type === 'decimal' || addForm.type === 'int'" v-model:value="addForm.value"
            :placeholder="`请输入${addForm.label || '属性'}的值`"
            :precision="addForm.type === 'decimal' ? addForm.precision : 0" :step="addForm.type === 'int' ? 1 : 0.01"
            style="width: 100%" />
          <a-input v-else v-model:value="addForm.value" :placeholder="`请输入${addForm.label || '属性'}的值`" />
        </a-form-item>
        <a-form-item v-if="editingIndex === null" label="是否公共属性">
          <a-switch v-model:checked="addForm.isPublic" />
          <span style="margin-left: 8px">公共属性可以快捷给其他产品使用</span>
        </a-form-item>
        <a-form-item v-if="editingIndex === null && addForm.isPublic" label="默认值" required>
          <a-input v-model:value="addForm.defaultValue" :placeholder="`请输入${addForm.label}的默认值`" />
        </a-form-item>
      </a-form>
    </a-modal>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, watch, h } from 'vue';
import { EditOutlined } from '@ant-design/icons-vue';
import UnitSelect from '/@/components/Lab/UnitSelect.vue';
import { getUnitById, loadAllUnits } from '/@/utils/lab/unit';

interface Attribute {
  key: string;
  label: string;
  type: 'decimal' | 'int' | 'text';
  value: any;
  unit?: string; // 单位符号（向后兼容）
  unitId?: string; // 单位ID（新字段）
  precision?: number;
  isCore?: boolean;
  isPublic?: boolean;
  sortCode?: number;
}

interface AddForm {
  key: string;
  label: string;
  type: 'decimal' | 'int' | 'text';
  unit?: string; // 单位符号（向后兼容）
  unitId?: string; // 单位ID（新字段）
  precision?: number;
  isPublic?: boolean;
  defaultValue?: string;
  value?: any;
}

const props = defineProps({
  modelValue: {
    type: Object,
    default: () => ({})
  },
  // 预定义的扩展属性配置
  predefinedAttributes: {
    type: Array as () => Array<Partial<Attribute>>,
    default: () => [
      {
        key: 'width',
        label: '宽度',
        type: 'decimal',
        unit: 'm',
        precision: 2
      },
      {
        key: 'thickness',
        label: '厚度',
        type: 'decimal',
        unit: 'mm',
        precision: 2
      },
      {
        key: 'weight',
        label: '重量',
        type: 'decimal',
        unit: 'kg',
        precision: 3
      },
      {
        key: 'tensileStrength',
        label: '抗拉强度',
        type: 'decimal',
        unit: 'MPa',
        precision: 1
      },
      {
        key: 'yieldStrength',
        label: '屈服强度',
        type: 'decimal',
        unit: 'MPa',
        precision: 1
      },
      {
        key: 'elongation',
        label: '延伸率',
        type: 'decimal',
        unit: '%',
        precision: 1
      }
    ]
  },
  // 是否显示“添加扩展属性”和“清空所有”按钮
  showAddAndClear: {
    type: Boolean,
    default: true
  }
});

const emit = defineEmits(['update:modelValue', 'change']);

const localAttributes = ref<Attribute[]>([]);
const showAddModal = ref(false);
const editingIndex = ref<number | null>(null); // 正在编辑的属性索引，null 表示新建
const isInternalUpdate = ref(false); // 标记是否是内部更新，避免循环更新

// 表格列定义
const columns = [
  {
    title: '属性名称',
    key: 'attributeName',
    dataIndex: 'label',
    width: '25%',
  },
  {
    title: '属性值',
    key: 'value',
    width: '30%',
  },
  {
    title: '类型',
    key: 'type',
    dataIndex: 'type',
    width: '15%',
  },
  {
    title: '精度',
    key: 'precision',
    dataIndex: 'precision',
    width: '10%',
    customRender: ({ record }: any) => {
      return record.type === 'decimal' ? (record.precision ?? 2) : '-';
    },
  },
  {
    title: '操作',
    key: 'action',
    width: '20%',
    align: 'center',
  },
];

const addForm = reactive<AddForm>({
  key: '',
  label: '',
  type: 'decimal',
  unit: '',
  unitId: undefined,
  precision: 2,
  isPublic: false,
  defaultValue: '',
  value: undefined
});

// 获取类型颜色
function getTypeColor(type: string) {
  const colorMap: Record<string, string> = {
    decimal: 'blue',
    int: 'green',
    text: 'orange',
  };
  return colorMap[type] || 'default';
}

// 获取类型文本
function getTypeText(type: string) {
  const textMap: Record<string, string> = {
    decimal: '小数',
    int: '整数',
    text: '文本',
  };
  return textMap[type] || type;
}

// 监听属性值变化
watch(
  () => props.modelValue,
  (newVal) => {
    // 如果是内部更新触发的，跳过重新加载
    if (isInternalUpdate.value) {
      isInternalUpdate.value = false;
      return;
    }

    if (newVal) {
      loadAttributes(newVal);
    }
  },
  { immediate: true, deep: true }
);

// 监听本地属性变化
watch(
  localAttributes,
  () => {
    // 标记为内部更新，避免触发 props.modelValue 的 watch
    isInternalUpdate.value = true;

    // 返回属性实体列表格式
    const attributes = localAttributes.value.map((attr, index) => ({
      attributeKey: attr.key,
      attributeName: attr.label,
      valueType: attr.type,
      attributeValue: attr.value !== null && attr.value !== undefined && attr.value !== ''
        ? String(attr.value)
        : null,
      unit: attr.unit || null, // 向后兼容
      unitId: attr.unitId || null, // 新字段：单位ID
      precision: attr.precision,
      sortCode: attr.sortCode ?? (index + 1)
    })).filter(attr => {
      // 属性只在有值时包含
      return attr.attributeValue !== null && attr.attributeValue !== '';
    });

    emit('update:modelValue', attributes);
    emit('change', attributes);
  },
  { deep: true }
);

// 加载属性（从属性实体列表加载）
async function loadAttributes(attributes: any[] | Record<string, any>) {
  localAttributes.value = [];

  // 预加载所有单位信息
  await loadAllUnits();

  // 如果传入的是数组（属性实体列表）
  if (Array.isArray(attributes)) {
    for (const attr of attributes) {
      // 如果有单位ID但没有单位符号，尝试加载单位信息
      let unitSymbol = attr.unit;
      if (attr.unitId && !unitSymbol) {
        const unit = await getUnitById(attr.unitId);
        if (unit) {
          unitSymbol = unit.symbol;
        }
      }

      const attribute: Attribute = {
        key: attr.attributeKey || attr.key,
        label: attr.attributeName || attr.label,
        type: attr.valueType || attr.type || 'text',
        value: attr.attributeValue !== undefined && attr.attributeValue !== null
          ? (attr.valueType === 'decimal' ? parseFloat(attr.attributeValue) :
            attr.valueType === 'int' ? parseInt(attr.attributeValue) : attr.attributeValue)
          : undefined,
        unit: unitSymbol, // 向后兼容，优先使用已有符号，否则从单位ID加载
        unitId: attr.unitId, // 新字段：单位ID
        precision: attr.precision,
        isCore: attr.isCore || false, // 后端已删除此字段，默认为 false
        isPublic: attr.isPublic || false, // 后端已删除此字段，默认为 false
        sortCode: attr.sortCode
      };

      // 属性只在有值时显示
      if (attribute.value !== undefined && attribute.value !== null && attribute.value !== '') {
        localAttributes.value.push(attribute);
      }
    }

    // 按排序码排序
    localAttributes.value.sort((a, b) => (a.sortCode || 0) - (b.sortCode || 0));
    return;
  }

  // 向后兼容：从对象字典加载（从 PropertyJson）
  const attributeValues = attributes as Record<string, any>;

  // 加载预定义属性
  props.predefinedAttributes.forEach(predefined => {
    // 如果属性值存在，使用属性值；否则使用预定义的默认值
    const value = attributeValues && attributeValues[predefined.key!] !== undefined
      ? attributeValues[predefined.key!]
      : predefined.value;

    // 预定义属性只在有值时才显示
    if (value !== undefined && value !== null && value !== '') {
      localAttributes.value.push({
        ...predefined,
        value: value
      } as Attribute);
    }
  });

  // 加载自定义属性
  if (attributeValues) {
    Object.keys(attributeValues).forEach(key => {
      if (!localAttributes.value.find(attr => attr.key === key)) {
        localAttributes.value.push({
          key,
          label: key,
          type: 'text',
          value: attributeValues[key]
        });
      }
    });
  }
}

// 处理属性变化（触发 watch）
function handleAttributeChange() {
  // watch 会自动处理
}

// 移除属性
// function removeAttribute(index: number) {
//   if (index >= 0 && index < localAttributes.value.length) {
//     // 从数组中移除指定索引的属性
//     localAttributes.value.splice(index, 1);
//     // Vue 的响应式系统会自动触发 watch，更新父组件
//   }
// }

// 生成键名
function generateKey() {
  if (addForm.label) {
    // 简单的中文转拼音逻辑（实际项目中可能需要更完善的转换）
    const pinyin = addForm.label
      .replace(/长度/g, 'length')
      .replace(/层数/g, 'layers')
      .replace(/密度/g, 'density')
      .replace(/宽度/g, 'width')
      .replace(/厚度/g, 'thickness')
      .replace(/重量/g, 'weight')
      .replace(/强度/g, 'strength')
      .replace(/延伸率/g, 'elongation')
      .replace(/[\u4e00-\u9fa5]/g, '')
      .toLowerCase();

    addForm.key = pinyin || addForm.label.toLowerCase().replace(/\s+/g, '_');
  }
}

// 处理类型变化
function handleTypeChange() {
  if (addForm.type === 'decimal') {
    addForm.precision = 2;
  } else if (addForm.type === 'int') {
    addForm.precision = 0;
  }
}

// 处理单位变化
function handleUnitChange(_unitId: string | undefined, unit: any) {
  // 同时更新单位符号（向后兼容）
  if (unit) {
    addForm.unit = unit.symbol;
    // 如果选择了单位且当前精度为空或未设置，自动使用单位的精度
    if (addForm.type === 'decimal' && (addForm.precision === undefined || addForm.precision === null)) {
      addForm.precision = unit.precision;
    }
  } else {
    addForm.unit = '';
  }
}

// 重置添加表单
function resetAddForm() {
  addForm.key = '';
  addForm.label = '';
  addForm.type = 'decimal';
  addForm.unit = '';
  addForm.unitId = undefined;
  addForm.precision = 2;
  addForm.isPublic = false;
  addForm.defaultValue = '';
  addForm.value = undefined;
  editingIndex.value = null;
}

// 编辑属性
async function editAttribute(index: number) {
  if (index < 0 || index >= localAttributes.value.length) {
    return;
  }

  const attr = localAttributes.value[index];
  editingIndex.value = index;

  // 预加载所有单位信息
  await loadAllUnits();

  // 如果有单位ID但没有单位符号，尝试加载单位信息
  let unitId = attr.unitId;
  let unitSymbol = attr.unit;
  if (unitId && !unitSymbol) {
    const unit = await getUnitById(unitId);
    if (unit) {
      unitSymbol = unit.symbol;
    }
  }

  // 填充表单
  addForm.key = attr.key;
  addForm.label = attr.label;
  addForm.type = attr.type;
  addForm.unit = unitSymbol || '';
  addForm.unitId = unitId || undefined;
  addForm.precision = attr.precision ?? (attr.type === 'decimal' ? 2 : 0);
  addForm.isPublic = attr.isPublic || false;
  addForm.defaultValue = '';
  addForm.value = attr.value;

  showAddModal.value = true;
}

// 处理添加/编辑属性
function handleAddAttribute() {
  if (!addForm.key || !addForm.label) {
    return;
  }

  const attrData: Attribute = {
    key: addForm.key,
    label: addForm.label,
    type: addForm.type,
    unit: addForm.unit || undefined, // 向后兼容
    unitId: addForm.unitId || undefined, // 新字段：单位ID
    precision: addForm.type === 'decimal' ? addForm.precision : (addForm.type === 'int' ? 0 : undefined),
    value: addForm.value !== undefined && addForm.value !== null && addForm.value !== ''
      ? addForm.value
      : (addForm.isPublic ? (addForm.defaultValue || undefined) : undefined),
    isPublic: addForm.isPublic,
    sortCode: editingIndex.value !== null
      ? localAttributes.value[editingIndex.value].sortCode ?? (editingIndex.value + 1)
      : localAttributes.value.length + 1
  };

  if (editingIndex.value !== null) {
    // 编辑模式：更新现有属性
    localAttributes.value[editingIndex.value] = attrData;
  } else {
    // 新建模式：添加新属性
    localAttributes.value.push(attrData);
  }

  showAddModal.value = false;
  resetAddForm();
}

// // 清空所有属性
// function clearAllAttributes() {
//   localAttributes.value = [];
// }

</script>

<style scoped lang="less">
.extended-attributes-form {
  margin-top: 16px;
  min-height: 200px;

  :deep(.ant-divider) {
    margin: 16px 0;
  }
}

.attributes-table-wrapper {
  margin: 16px 0;

  :deep(.ant-table) {
    .unit-text {
      color: #999;
      font-size: 12px;
      margin-left: 4px;
    }

    .ant-table-tbody>tr>td {
      padding: 12px;
    }
  }
}

.action-buttons {
  margin-top: 16px;
  text-align: center;
  padding-bottom: 8px;
}
</style>