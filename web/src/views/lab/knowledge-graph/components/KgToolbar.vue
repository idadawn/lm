<template>
  <div class="kg-toolbar">
    <a-space>
      <a-button type="primary" :loading="resyncing" @click="$emit('resync')">
        <template #icon><ReloadOutlined /></template>
        全量重建
      </a-button>
      <a-button :loading="loading" @click="$emit('refresh')">
        <template #icon><SyncOutlined /></template>
        刷新
      </a-button>
      <a-input-search
        v-model:value="localSearch"
        placeholder="搜索规格、规则、公式..."
        style="width: 240px"
        @search="onSearch"
        allow-clear
        @change="onSearchChange"
      />
      <a-radio-group
        v-model:value="localViewMode"
        option-type="button"
        :options="[
          { label: '网格', value: 'grid' },
          { label: '图谱', value: 'graph' },
        ]"
        @change="$emit('update:viewMode', localViewMode)"
      />
    </a-space>
  </div>
</template>

<script lang="ts" setup>
import { ref, watch, h } from 'vue';
import { ReloadOutlined, SyncOutlined, SearchOutlined } from '@ant-design/icons-vue';
import type { ViewMode } from '../types/ontology';

const props = defineProps<{
  loading: boolean;
  resyncing: boolean;
  searchText: string;
  viewMode: ViewMode;
}>();

const emit = defineEmits<{
  (e: 'resync'): void;
  (e: 'refresh'): void;
  (e: 'search', value: string): void;
  (e: 'update:viewMode', value: ViewMode): void;
}>();

const localSearch = ref(props.searchText);
const localViewMode = ref<ViewMode>(props.viewMode);

watch(() => props.searchText, (v) => { localSearch.value = v; });
watch(() => props.viewMode, (v) => { localViewMode.value = v; });

let debounceTimer: ReturnType<typeof setTimeout> | null = null;

function onSearch(value: string) {
  if (debounceTimer) clearTimeout(debounceTimer);
  debounceTimer = setTimeout(() => {
    emit('search', value);
  }, 300);
}

function onSearchChange() {
  if (!localSearch.value) {
    emit('search', '');
  }
}
</script>

<style lang="less" scoped>
.kg-toolbar {
  padding: 10px 16px;
  border-bottom: 1px solid #F1F5F9;
  flex-shrink: 0;
  background: #fff;
}
</style>
