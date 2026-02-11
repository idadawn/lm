<template>
  <div class="editable-cell" @dblclick="startEdit">
    <template v-if="editing">
      <a-input v-if="type === 'text'" ref="inputRef" v-model:value="editValue" size="small" @blur="handleSave"
        @pressEnter="handleSave" @keyup.esc="cancelEdit" />
      <a-input v-else-if="type === 'number'" ref="inputRef" :value="editStr" size="small"
        @input="onNumberStrInput"
        @blur="handleSave" @pressEnter="handleSave" @keyup.esc="cancelEdit" />
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
import { ref, shallowRef, computed, nextTick, watch } from 'vue';
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
/** 本次编辑是否已触发保存，避免 blur + pressEnter 重复触发导致重复请求 */
const savedThisEdit = ref(false);
const editValue = shallowRef<any>(null);
/** 数字类型编辑时的字符串，根据精度限制小数位数 */
const editStr = ref('');
const inputRef = shallowRef<any>(null);

/** 小数点后允许的最大位数，优先使用 precision prop，默认 4 */
const maxDecimals = computed(() => props.precision ?? 4);

/** 数字输入：只允许数字、一个负号、一个小数点，且小数点后位数不超过精度限制 */
function onNumberStrInput(e: Event) {
  const input = e.target as HTMLInputElement;
  const raw = input.value;
  let s = raw.replace(/[^\d.-]/g, '');
  if (s.startsWith('-')) {
    const rest = s.slice(1).replace(/-/g, '');
    s = '-' + rest;
  } else {
    s = s.replace(/-/g, '');
  }
  const dotCount = (s.match(/\./g) || []).length;
  if (dotCount > 1) {
    const firstDot = s.indexOf('.');
    s = s.slice(0, firstDot + 1) + s.slice(firstDot + 1).replace(/\./g, '');
  }
  const parts = s.split('.');
  if (parts.length === 2 && parts[1].length > maxDecimals.value) {
    parts[1] = parts[1].slice(0, maxDecimals.value);
    s = parts.join('.');
  }
  editStr.value = s;
  // 当处理后的值与原始输入不同时，强制同步原生 input 的 DOM 值
  if (s !== raw) {
    nextTick(() => {
      input.value = s;
    });
  }
}

function formatDisplayValue(val: any) {
  if (val === null || val === undefined || val === '') return '-';
  // 后端已处理精度，直接显示
  return val;
}

function startEdit() {
  editing.value = true;
  savedThisEdit.value = false;
  editValue.value = props.value;
  if (props.type === 'number') {
    editStr.value =
      props.value !== null && props.value !== undefined && props.value !== ''
        ? String(Number(props.value))
        : '';
  }
  nextTick(() => {
    inputRef.value?.focus();
  });
}

function handleSave() {
  if (savedThisEdit.value) return;
  if (props.type === 'number') {
    const num =
      editStr.value === '' || editStr.value === '-'
        ? null
        : Number(editStr.value);
    const same =
      num === props.value ||
      (num == null && (props.value == null || props.value === ''));
    if (!same && (num == null || !Number.isNaN(num))) {
      savedThisEdit.value = true;
      emit('save', num);
    }
  } else if (editValue.value !== props.value) {
    savedThisEdit.value = true;
    emit('save', editValue.value);
  }
  editing.value = false;
}

function cancelEdit() {
  editing.value = false;
  editValue.value = props.value;
  if (props.type === 'number') {
    editStr.value =
      props.value !== null && props.value !== undefined && props.value !== ''
        ? String(Number(props.value))
        : '';
  }
}

watch(
  () => props.value,
  (val) => {
    if (!editing.value) {
      editValue.value = val;
      if (props.type === 'number') {
        editStr.value =
          val !== null && val !== undefined && val !== ''
            ? String(Number(val))
            : '';
      }
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
