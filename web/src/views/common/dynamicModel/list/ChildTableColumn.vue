<template>
  <div class="child-table-column">
    <template v-if="!expand">
      <tr v-for="(item, index) in fewData" class="child-table__row" :key="index">
        <td v-for="(headItem, i) in head" :key="i" :style="{ width: `${headItem.width}px` }" :class="{ 'td-flex-1': !headItem.width }">
          <div class="cell" v-if="headItem.jnpfKey === 'relationForm'">
            <p class="link-text" :title="item[headItem.dataIndex]" @click="toDetail(headItem.modelId, item[`${headItem.dataIndex}_id`])">
              {{ item[headItem.dataIndex] }}
            </p>
          </div>
          <div class="cell" v-else-if="headItem.jnpfKey === 'inputNumber'">
            <jnpf-input-number v-model:value="item[headItem.dataIndex]" :precision="headItem.precision" :thousands="headItem.thousands" disabled detailed />
          </div>
          <div class="cell" v-else-if="headItem.jnpfKey === 'calculate'">
            <jnpf-calculate
              v-model:value="item[headItem.dataIndex]"
              :isStorage="headItem.isStorage"
              :precision="headItem.precision"
              :thousands="headItem.thousands"
              detailed />
          </div>
          <div class="cell" :title="item[headItem.dataIndex]" v-else>{{ item[headItem.dataIndex] }}</div>
        </td>
      </tr>
    </template>
    <template v-if="expand">
      <tr v-for="(item, index) in data" class="child-table__row" :key="index">
        <td v-for="(headItem, i) in head" :key="i" :style="{ width: `${headItem.width}px` }" :class="{ 'td-flex-1': !headItem.width }">
          <div class="cell" v-if="headItem.jnpfKey === 'relationForm'">
            <p class="link-text" :title="item[headItem.dataIndex]" @click="toDetail(headItem.modelId, item[`${headItem.dataIndex}_id`])">
              {{ item[headItem.dataIndex] }}
            </p>
          </div>
          <div class="cell" v-else-if="headItem.jnpfKey === 'inputNumber'">
            <jnpf-input-number v-model:value="item[headItem.dataIndex]" :precision="headItem.precision" :thousands="headItem.thousands" disabled detailed />
          </div>
          <div class="cell" v-else-if="headItem.jnpfKey === 'calculate'">
            <jnpf-calculate
              v-model:value="item[headItem.dataIndex]"
              :isStorage="headItem.isStorage"
              :precision="headItem.precision"
              :thousands="headItem.thousands"
              detailed />
          </div>
          <div class="cell" :title="item[headItem.dataIndex]" v-else>{{ item[headItem.dataIndex] }}</div>
        </td>
      </tr>
    </template>
    <div class="expand-more-btn" v-if="data && data.length > defaultNumber">
      <a-button v-if="expand" type="link" @click="toggleExpand">隐藏部分</a-button>
      <a-button v-if="!expand" type="link" @click="toggleExpand">加载更多</a-button>
    </div>
  </div>
</template>

<script lang="ts" setup>
  import { computed } from 'vue';
  import type { PropType } from 'vue';

  defineOptions({ name: 'childTableColumn' });
  const props = defineProps({
    data: { type: Array as PropType<any[]>, default: () => [] },
    head: { type: Array as PropType<any[]>, default: () => [] },
    defaultNumber: { type: Number, default: 3 },
    expand: { type: Boolean, default: false },
  });
  const emit = defineEmits(['toggleExpand', 'toDetail']);

  const fewData = computed(() => (props.data ? props.data.slice(0, props.defaultNumber) : []));

  function toggleExpand() {
    emit('toggleExpand');
  }
  function toDetail(modelId, id) {
    emit('toDetail', modelId, id);
  }
</script>

<style scoped></style>
