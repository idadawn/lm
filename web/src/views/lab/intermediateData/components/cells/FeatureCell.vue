<template>
  <div
    :class="['cell-content', 'feature-list-cell', { 'editable': hasPermission }, cellClass]"
    @click="handleClick"
    @dblclick="handleDblClick"
  >
    <template v-if="column.key === 'featureSuffix'">
      <a-tag v-if="record.featureSuffix" color="orange">{{ record.featureSuffix }}</a-tag>
      <span v-else>-</span>
    </template>
    <template v-else>
      <a-space v-if="matchedFeatures.length > 0" wrap size="small">
        <a-tag v-for="feature in matchedFeatures" :key="feature.id" color="blue">
          {{ feature.label }}
        </a-tag>
      </a-space>
      <span v-else>-</span>
    </template>
    <EditOutlined v-if="hasPermission" class="feature-edit-icon" />
  </div>
</template>

<script lang="ts" setup>
import { computed } from 'vue';
import { EditOutlined } from '@ant-design/icons-vue';

const props = defineProps<{
  record: any;
  column: any;
  cellClass: string;
  hasPermission?: boolean;
  getMatchedFeatureLabels?: (record: any) => Array<{ id: string; label: string }>;
}>();

const emit = defineEmits<{
  click: [];
  dblclick: [];
}>();

const handleClick = () => emit('click');
const handleDblClick = () => emit('dblclick');

const matchedFeatures = computed(() => {
  if (props.getMatchedFeatureLabels) {
    return props.getMatchedFeatureLabels(props.record);
  }
  return [];
});
</script>

<style scoped>
.cell-content {
  width: 100%;
  height: 100%;
  padding: 4px;
  cursor: pointer;
  transition: background-color 0.3s;
  display: flex;
  align-items: center;
  gap: 4px;
}

.cell-content:hover {
  opacity: 0.8;
}

.feature-list-cell.editable {
  position: relative;
}

.feature-list-cell.editable:hover {
  background-color: #f0f5ff !important;
}

.feature-edit-icon {
  font-size: 12px;
  color: #1890ff;
  opacity: 0;
  transition: opacity 0.2s;
  flex-shrink: 0;
}

.feature-list-cell.editable:hover .feature-edit-icon {
  opacity: 1;
}
</style>
