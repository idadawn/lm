<template>
  <div class="editable-cell" @dblclick="startEdit">
    <template v-if="editing">
      <a-input v-if="type === 'text'" ref="inputRef" v-model:value="editValue" size="small" @blur="handleSave"
        @pressEnter="handleSave" @keyup.esc="cancelEdit" />
      <a-input-number v-else-if="type === 'number'" ref="inputRef" v-model:value="editValue" size="small"
        :precision="precision ?? 2" @blur="handleSave" @pressEnter="handleSave" @keyup.esc="cancelEdit" />
    </template>
    <template v-else>
      <span class="cell-value" :class="{ empty: !value && value !== 0 }">
        {{ formatDisplayValue(value) }}
      </span>
      <EditOutlined class="edit-icon" />
    </template>
  </div>
</template>

<script lang="ts" setup>
import { ref, shallowRef, nextTick, watch } from 'vue';
import { EditOutlined } from '@ant-design/icons-vue';

const props = defineProps<{
  record: any;
  field: string;
  value: any;
  type?: 'text' | 'number';
  precision?: number;
}>();

const emit = defineEmits<{
  (e: 'save', value: any): void;
}>();

const editing = ref(false);
const editValue = shallowRef<any>(null);
const inputRef = shallowRef<any>(null);

function formatDisplayValue(val: any) {
  if (val === null || val === undefined || val === '') return '-';
  // 后端已处理精度，直接显示
  return val;
}

function startEdit() {
  editing.value = true;
  editValue.value = props.value;
  nextTick(() => {
    inputRef.value?.focus();
  });
}

function handleSave() {
  if (editValue.value !== props.value) {
    emit('save', editValue.value);
  }
  editing.value = false;
}

function cancelEdit() {
  editing.value = false;
  editValue.value = props.value;
}

watch(
  () => props.value,
  (val) => {
    if (!editing.value) {
      editValue.value = val;
    }
  },
);
</script>

<style scoped>
.editable-cell {
  display: flex;
  align-items: center;
  min-height: 24px;
  cursor: pointer;
  padding: 2px 4px;
  border-radius: 4px;
  transition: background 0.2s;
}

.editable-cell:hover {
  background: #f0f0f0;
}

.editable-cell:hover .edit-icon {
  opacity: 1;
}

.cell-value {
  flex: 1;
}

.cell-value.empty {
  color: #999;
}

.edit-icon {
  margin-left: 4px;
  font-size: 12px;
  color: #1890ff;
  opacity: 0;
  transition: opacity 0.2s;
}

:deep(.ant-input),
:deep(.ant-input-number) {
  width: 100% !important;
  min-width: 80px !important;
}
</style>
