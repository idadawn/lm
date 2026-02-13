<template>
  <div class="cursor-pagination-footer">
    <span class="cursor-total">
      <template v-if="total > 0">
        已加载 {{ loaded }} 条 / 共 {{ total }} 条
      </template>
      <template v-else>共 0 条</template>
    </span>
    <span v-if="!hasMore && total > 0" class="load-complete">
      <CheckCircleOutlined /> 已全部加载
    </span>
    <span class="cursor-page-indicator"></span>
    <a-button type="link" size="small" class="back-to-top" @click="handleBackToTop">
      <VerticalAlignTopOutlined /> 回到顶部
    </a-button>
  </div>
</template>

<script lang="ts">
  import { defineComponent, computed, inject, type Ref, type ComputedRef } from 'vue';
  import { VerticalAlignTopOutlined, CheckCircleOutlined } from '@ant-design/icons-vue';

  // 与 BasicTable provide 的 key 保持一致
  export const CURSOR_STATE_KEY = Symbol('cursorState');

  export interface CursorStateInjection {
    cursorTotalRef: Ref<number>;
    loadingMoreRef: Ref<boolean>;
    hasMore: ComputedRef<boolean>;
    loadedItems: ComputedRef<number>;
    scrollTo: (pos: string) => void;
  }

  export default defineComponent({
    name: 'CursorFooter',
    components: { VerticalAlignTopOutlined, CheckCircleOutlined },
    setup() {
      const cursorState = inject<CursorStateInjection | null>(CURSOR_STATE_KEY, null);

      const total = computed(() => cursorState?.cursorTotalRef?.value ?? 0);
      const loaded = computed(() => cursorState?.loadedItems?.value ?? 0);
      const hasMore = computed(() => cursorState?.hasMore?.value ?? true);

      function handleBackToTop() {
        cursorState?.scrollTo?.('top');
      }

      return { total, loaded, hasMore, handleBackToTop };
    },
  });
</script>

<style lang="less" scoped>
  .cursor-pagination-footer {
    display: flex;
    align-items: center;
    gap: 12px;
    padding: 10px 12px;
    font-size: 12px;
    color: var(--ant-text-color-secondary);
    background: var(--component-background);
    border-top: 1px solid var(--ant-border-color);

    .cursor-total {
      flex: 1;
    }

    .load-complete {
      display: inline-flex;
      align-items: center;
      gap: 4px;
      color: @success-color;
      font-size: 12px;
    }

    .cursor-page-indicator {
      font-size: 13px;
      font-weight: 500;
      color: var(--ant-text-color);

      &:empty {
        display: none;
      }
    }

    .back-to-top {
      padding: 0 4px;
    }
  }
</style>
