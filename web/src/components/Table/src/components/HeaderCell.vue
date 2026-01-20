<template>
  <EditTableHeaderCell v-if="getIsEdit">
    <component v-if="isVNode" :is="getTitle" />
    <template v-else>{{ getTitle }}</template>
  </EditTableHeaderCell>
  <template v-else>
    <component v-if="isVNode" :is="getTitle" />
    <span v-else>{{ getTitle }}</span>
  </template>
  <BasicHelp v-if="getHelpMessage" :text="getHelpMessage" :class="`${prefixCls}__help`" />
</template>
<script lang="ts">
  import type { PropType, VNode } from 'vue';
  import type { BasicColumn } from '../types/table';
  import { defineComponent, computed } from 'vue';
  import BasicHelp from '/@/components/Basic/src/BasicHelp.vue';
  import EditTableHeaderCell from './EditTableHeaderIcon.vue';
  import { useDesign } from '/@/hooks/web/useDesign';

  export default defineComponent({
    name: 'TableHeaderCell',
    components: {
      EditTableHeaderCell,
      BasicHelp,
    },
    props: {
      column: {
        type: Object as PropType<BasicColumn>,
        default: () => ({}),
      },
    },
    setup(props) {
      const { prefixCls } = useDesign('basic-table-header-cell');

      const getIsEdit = computed(() => !!props.column?.edit);
      const getTitle = computed(() => {
        const title = props.column?.customTitle || props.column?.title;
        // 如果是函数，调用它
        if (typeof title === 'function') {
          return title();
        }
        return title;
      });
      const isVNode = computed(() => {
        const title = getTitle.value;
        return title && typeof title === 'object' && 'type' in title;
      });
      const getHelpMessage = computed(() => props.column?.helpMessage);

      return { prefixCls, getIsEdit, getTitle, isVNode, getHelpMessage };
    },
  });
</script>
<style lang="less">
  @prefix-cls: ~'@{namespace}-basic-table-header-cell';

  .@{prefix-cls} {
    &__help {
      margin-left: 8px;
      color: rgb(0 0 0 / 65%) !important;
    }
  }
</style>
