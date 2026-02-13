import type { BasicTableProps, TableRowSelection, BasicColumn } from '../types/table';
import { Ref, ComputedRef, ref } from 'vue';
import { computed, unref, nextTick, watch } from 'vue';
import { getViewportOffset } from '/@/utils/domUtils';
import { isBoolean } from '/@/utils/is';
import { useWindowSizeFn } from '/@/hooks/event/useWindowSizeFn';
import { useModalContext } from '/@/components/Modal';
import { onMountedOrActivated } from '/@/hooks/core/onMountedOrActivated';
import { useDebounceFn } from '@vueuse/core';
import { useEventListener } from '/@/hooks/event/useEventListener';

/** 距底部约 20% 时触发加载更多 */
const CURSOR_LOAD_THRESHOLD = 0.8;
/** 游标底部栏高度（表格外单独渲染时的预留高度） */
const CURSOR_FOOTER_HEIGHT = 44;

export interface CursorScrollActions {
  loadMore: () => Promise<void>;
  hasMore: Ref<boolean> | ComputedRef<boolean>;
  loadingMore: Ref<boolean>;
  /** 每页条数（用于计算当前页） */
  pageSize?: number;
  /** 已加载的条数 */
  loadedItems?: ComputedRef<number>;
  /** 服务端总条数 */
  totalItems?: Ref<number>;
}

export function useTableScroll(
  propsRef: ComputedRef<BasicTableProps>,
  tableElRef: Ref<ComponentRef>,
  columnsRef: ComputedRef<BasicColumn[]>,
  rowSelectionRef: ComputedRef<TableRowSelection | null>,
  getDataSourceRef: ComputedRef<Recordable[]>,
  wrapRef: Ref<HTMLElement | null>,
  formRef: Ref<ComponentRef>,
  cursorActions?: (() => CursorScrollActions | undefined) | CursorScrollActions,
) {

  const tableHeightRef: Ref<Nullable<number | string>> = ref(167);
  const modalFn = useModalContext();

  // Greater than animation time 280
  const debounceRedoHeight = useDebounceFn(redoHeight, 100);

  const getCanResize = computed(() => {
    const { canResize, scroll } = unref(propsRef);
    return canResize && !(scroll || {}).y;
  });

  watch(
    () => [unref(getCanResize), unref(getDataSourceRef)?.length],
    () => {
      debounceRedoHeight();
    },
    {
      flush: 'post',
    },
  );

  function redoHeight() {
    nextTick(() => {
      calcTableHeight();
    });
  }

  function setHeight(height: number) {
    tableHeightRef.value = height;
    //  Solve the problem of modal adaptive height calculation when the form is placed in the modal
    modalFn?.redoModalHeight?.();
  }

  // No need to repeat queries
  let paginationEl: HTMLElement | null;
  let footerEl: HTMLElement | null;
  // 原生合计
  let summaryEl: HTMLElement | null;
  let bodyEl: HTMLElement | null;

  async function calcTableHeight() {
    const { resizeHeightOffset, pagination, maxHeight, isCanResizeParent, useSearchForm } = unref(propsRef);
    const tableData = unref(getDataSourceRef);

    const table = unref(tableElRef);
    if (!table) return;

    const tableEl: Element = table.$el;
    if (!tableEl) return;

    if (!bodyEl) {
      bodyEl = tableEl.querySelector('.ant-table-body');
      if (!bodyEl) return;
    }

    const hasScrollBarY = bodyEl.scrollHeight > bodyEl.clientHeight;
    const hasScrollBarX = bodyEl.scrollWidth > bodyEl.clientWidth;

    if (hasScrollBarY) {
      tableEl.classList.contains('hide-scrollbar-y') && tableEl.classList.remove('hide-scrollbar-y');
    } else {
      !tableEl.classList.contains('hide-scrollbar-y') && tableEl.classList.add('hide-scrollbar-y');
    }

    if (hasScrollBarX) {
      tableEl.classList.contains('hide-scrollbar-x') && tableEl.classList.remove('hide-scrollbar-x');
    } else {
      !tableEl.classList.contains('hide-scrollbar-x') && tableEl.classList.add('hide-scrollbar-x');
    }

    bodyEl!.style.height = 'unset';

    if (!unref(getCanResize) || !unref(tableData)) return;

    await nextTick();
    // Add a delay to get the correct bottomIncludeBody paginationHeight footerHeight headerHeight

    // const headEl = tableEl.querySelector('.ant-table-thead');
    const headEl = tableEl.querySelector('.ant-table-header');

    if (!headEl) return;

    // Table height from bottom height-custom offset
    let paddingHeight = 10;
    // Pager height
    let paginationHeight = 0;
    const isCursorPaginationMode = unref(propsRef).paginationMode === 'cursor';
    if (!isBoolean(pagination) && !isCursorPaginationMode) {
      paginationEl = tableEl.querySelector('.ant-pagination') as HTMLElement;
      if (paginationEl) {
        const offsetHeight = paginationEl.offsetHeight;
        paginationHeight += (offsetHeight || 0) + 10;
      } else {
        // TODO First fix 34
        paginationHeight += 34;
      }
    } else {
      paginationHeight = 0;
    }

    let footerHeight = 0;
    if (isCursorPaginationMode) {
      footerHeight += CURSOR_FOOTER_HEIGHT;
    } else if (!isBoolean(pagination)) {
      if (!footerEl) {
        footerEl = tableEl.querySelector('.ant-table-footer') as HTMLElement;
      }
      if (footerEl) {
        const offsetHeight = footerEl.offsetHeight;
        footerHeight += offsetHeight || 0;
      }
    }
    if (!summaryEl) {
      summaryEl = tableEl.querySelector('.ant-table-summary') as HTMLElement;
    }
    footerHeight += summaryEl?.offsetHeight || 0;

    let headerHeight = 0;
    if (headEl) {
      headerHeight = (headEl as HTMLElement).offsetHeight;
    }

    let bottomIncludeBody = 0;
    if (unref(wrapRef) && isCanResizeParent) {
      const tablePadding = 0;
      const formMargin = 0;
      let paginationMargin = 10;
      const wrapHeight = unref(wrapRef)?.offsetHeight ?? 0;

      let formHeight = unref(formRef)?.$el.offsetHeight ?? 0;
      if (formHeight) {
        formHeight += formMargin;
      }
      if (isBoolean(pagination) && !pagination) {
        paginationMargin = 0;
      }
      if (isBoolean(useSearchForm) && !useSearchForm) {
        paddingHeight = 0;
      }

      const headerCellHeight = (tableEl.querySelector('.ant-table-title') as HTMLElement)?.offsetHeight ?? 0;

      bottomIncludeBody = wrapHeight - formHeight - headerCellHeight - tablePadding - paginationMargin;
    } else {
      // Table height from bottom
      bottomIncludeBody = getViewportOffset(headEl).bottomIncludeBody;
    }

    let height = bottomIncludeBody - (resizeHeightOffset || 0) - paddingHeight - paginationHeight - footerHeight - headerHeight;
    height = (height > maxHeight! ? (maxHeight as number) : height) ?? height;
    setHeight(height);

    bodyEl!.style.height = `${height}px`;
  }
  useWindowSizeFn(calcTableHeight, 280);

  function setupCursorScrollListener() {
    const actions = typeof cursorActions === 'function' ? cursorActions() : cursorActions;
    if (!actions) return;
    const table = unref(tableElRef);
    if (!table?.$el) return;
    const bodyEl = table.$el.querySelector('.ant-table-body') as HTMLElement;
    if (!bodyEl) return;
    const { pageSize = 50, loadedItems, totalItems } = actions;
    // 页码指示器 DOM 元素（直接更新 DOM，避免响应式引起整表重渲染）
    let pageIndicatorEl: HTMLElement | null = null;

    /**
     * 检查是否需要加载更多数据。
     * loadMore 本身通过 loadingMoreRef 防止重复调用，
     * 这里只需判断：有更多数据 + 未在加载中 + 滚动比例达到阈值。
     */
    const checkAndLoad = () => {
      const { loadMore, hasMore, loadingMore } = actions;
      const { scrollTop, clientHeight, scrollHeight } = bodyEl;

      // 直接更新页码指示器 DOM（不走 Vue 响应式）
      if (loadedItems && pageSize > 0) {
        const loaded = unref(loadedItems);
        if (loaded > 0 && scrollHeight > 0) {
          const firstVisibleRow = Math.floor(scrollTop / scrollHeight * loaded);
          const currentPage = Math.floor(firstVisibleRow / pageSize) + 1;
          const totalPages = Math.ceil((totalItems ? unref(totalItems) : loaded) / pageSize) || 1;
          if (!pageIndicatorEl) {
            pageIndicatorEl = table.$el.querySelector('.cursor-page-indicator') as HTMLElement;
          }
          if (pageIndicatorEl) {
            pageIndicatorEl.textContent = totalPages > 1 ? `第 ${currentPage} 页 / 共 ${totalPages} 页` : '';
          }
        }
      }

      if (unref(loadingMore) || !unref(hasMore)) return;
      // 内容不足以填满视口时 scrollHeight <= clientHeight，直接触发加载
      if (scrollHeight <= clientHeight) {
        loadMore();
        return;
      }
      const ratio = (scrollTop + clientHeight) / scrollHeight;
      if (ratio >= CURSOR_LOAD_THRESHOLD) {
        loadMore();
      }
    };

    // 使用 throttle（而非 debounce）让滚动时更及时响应
    useEventListener({
      el: bodyEl,
      name: 'scroll',
      listener: checkAndLoad,
      isDebounce: false,
      wait: 150,
      options: true,
    });

    // 首次检查：如果初始数据不足以填满视口，自动加载更多
    nextTick(checkAndLoad);
  }

  onMountedOrActivated(() => {
    calcTableHeight();
    nextTick(() => {
      debounceRedoHeight();
      if (unref(propsRef).paginationMode === 'cursor' && cursorActions) {
        nextTick(setupCursorScrollListener);
      }
    });
  });

  const getScrollX = computed(() => {
    let width = 0;
    if (unref(rowSelectionRef)) {
      width += 60;
    }

    // TODO props ?? 0;
    const NORMAL_WIDTH = 150;

    const columns = unref(columnsRef).filter(item => !item.defaultHidden);
    columns.forEach(item => {
      width += Number.parseFloat(item.width as string) || 0;
    });
    const unsetWidthColumns = columns.filter(item => !Reflect.has(item, 'width'));

    const len = unsetWidthColumns.length;
    if (len !== 0) {
      width += len * NORMAL_WIDTH;
    }

    const table = unref(tableElRef);
    const tableWidth = table?.$el?.offsetWidth ?? 0;
    return tableWidth > width ? '100%' : width;
  });

  const getScrollRef = computed(() => {
    const tableHeight = unref(tableHeightRef);
    const { canResize, scroll } = unref(propsRef);
    return {
      x: unref(getScrollX),
      y: canResize ? tableHeight : null,
      scrollToFirstRowOnChange: false,
      ...scroll,
    };
  });

  return { getScrollRef, redoHeight };
}
