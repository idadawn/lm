<template>
  <div :class="['cell-content', cellClass]" @click="handleClick">
    <EditableCell
      v-if="hasPermission"
      :record="record"
      :field="column.key"
      :value="record[column.key]"
      type="number"
      :precision="getFieldPrecision ? getFieldPrecision(column.key) : undefined"
      @save="handleSave"
    />
    <span v-else>{{ record[column.key] }}</span>
  </div>
</template>

<script lang="ts" setup>
import EditableCell from '../EditableCell.vue';

const props = defineProps<{
  record: any;
  column: any;
  cellClass: string;
  hasPermission?: boolean;
  getFieldPrecision?: (field: string) => number;
}>();

const emit = defineEmits<{
  click: [];
  save: [value: any];
}>();

const handleClick = () => emit('click');

const handleSave = (value: any) => {
  emit('save', value);
};
</script>

<style scoped>
.cell-content {
  width: 100%;
  height: 100%;
  padding: 4px;
  cursor: pointer;
  transition: background-color 0.3s;
}

.cell-content:hover {
  opacity: 0.8;
}
</style>
