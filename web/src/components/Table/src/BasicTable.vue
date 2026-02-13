<template>
  <div ref="wrapRef" :class="getWrapperClass">
    <BasicForm
      ref="formRef"
      submitOnReset
      v-bind="getFormProps"
      v-if="getBindValues.useSearchForm"
      :tableAction="tableAction"
      @register="registerForm"
      @submit="handleSearchInfoChange"
      @advanced-change="redoHeight"
      class="search-form">
      <template #[replaceFormSlotKey(item)]="data" v-for="item in getFormSlotKeys">
        <slot :name="item" v-bind="data || {}"></slot>
      </template>
    </BasicForm>

    <Table ref="tableElRef" v-bind="getBindValues" :rowClassName="getRowClassName" v-show="getEmptyDataIsShowTable" @change="handleTableChange">
      <template #[item]="data" v-for="item in Object.keys($slots)" :key="item">
        <slot :name="item" v-bind="data || {}"></slot>
      </template>
      <template #headerCell="{ column }">
        <HeaderCell :column="column" />
      </template>
      <!-- 增加对antdv3.x兼容 -->
      <template #bodyCell="data">
        <slot name="bodyCell" v-bind="data || {}"></slot>
      </template>
      <!-- <template #[`header-${column.dataIndex}`] v-for="(column, index) in columns" :key="index">
        <HeaderCell :column="column" />
      </template> -->
    </Table>
    <!-- 游标模式：加载更多居中浮动提示（DOM 切换显隐，不走 Vue 响应式，避免 ant-design-vue 级联更新） -->
    <div v-if="isCursorMode" ref="cursorLoadingRef" class="cursor-loading-indicator">
      <div class="cursor-spinner"></div>
      <span>加载中...</span>
    </div>
    <!-- 游标模式：表格外单独渲染底部栏，不传 footer 给 Table，避免 Ant Table 内部 watchEffect 无限递归 -->
    <CursorFooter v-if="isCursorMode" />
  </div>
</template>
<script lang="ts">
  import type { BasicTableProps, TableActionType, SizeType, ColumnChangeParam } from './types/table';

  import { defineComponent, ref, shallowRef, computed, unref, toRaw, inject, watch, watchEffect, provide } from 'vue';
  import { Table } from 'ant-design-vue';
  import { BasicForm, useForm } from '/@/components/Form';
  import { PageWrapperFixedHeightKey } from '/@/enums/pageEnum';
  import HeaderCell from './components/HeaderCell.vue';
  import CursorFooter from './components/CursorFooter.vue';
  import { InnerHandlers } from './types/table';
  import { CURSOR_STATE_KEY } from './components/CursorFooter.vue';

  import { usePagination } from './hooks/usePagination';
  import { useColumns } from './hooks/useColumns';
  import { useDataSource } from './hooks/useDataSource';
  import { useLoading } from './hooks/useLoading';
  import { useRowSelection } from './hooks/useRowSelection';
  import { useTableScroll } from './hooks/useTableScroll';
  import { useTableScrollTo } from './hooks/useScrollTo';
  import { useCustomRow } from './hooks/useCustomRow';
  import { useTableStyle } from './hooks/useTableStyle';
  import { useTableHeader } from './hooks/useTableHeader';
  import { useTableExpand } from './hooks/useTableExpand';
  import { createTableContext } from './hooks/useTableContext';
  import { useTableFooter } from './hooks/useTableFooter';
  import { useTableForm } from './hooks/useTableForm';
  import { useDesign } from '/@/hooks/web/useDesign';

  import { omit } from 'lodash-es';
  import { basicProps } from './props';
  import { isFunction, isBoolean } from '/@/utils/is';
  import { warn } from '/@/utils/log';

  export default defineComponent({
    name: 'BasicTable',
    components: {
      Table,
      BasicForm,
      HeaderCell,
      CursorFooter,
    },
    props: basicProps,
    emits: [
      'fetch-success',
      'fetch-error',
      'selection-change',
      'register',
      'row-click',
      'row-dbClick',
      'row-contextmenu',
      'row-mouseenter',
      'row-mouseleave',
      'edit-end',
      'edit-cancel',
      'edit-row-end',
      'edit-change',
      'expanded-rows-change',
      'change',
      'columns-change',
    ],
    setup(props, { attrs, emit, slots, expose }) {
      const tableElRef = ref(null);
      const tableData = shallowRef<Recordable[]>([]);

      const wrapRef = ref(null);
      const formRef = ref(null);
      const innerPropsRef = ref<Partial<BasicTableProps>>();

      const { prefixCls } = useDesign('basic-table');
      const [registerForm, formActions] = useForm();

      const getProps = computed(() => {
        return { ...props, ...unref(innerPropsRef) } as BasicTableProps;
      });

      const isCursorMode = computed(() => unref(getProps).paginationMode === 'cursor');

      const isFixedHeightPage = inject(PageWrapperFixedHeightKey, false);
      watchEffect(() => {
        unref(isFixedHeightPage) &&
          props.canResize &&
          warn("'canResize' of BasicTable may not work in PageWrapper with 'fixedHeight' (especially in hot updates)");
      });

      const { getLoading, setLoading } = useLoading(getProps);
      const { getPaginationInfo, getPagination, setPagination, setShowPagination, getShowPagination } = usePagination(getProps);

      const {
        getRowSelection,
        getRowSelectionRef,
        getSelectRows,
        setSelectedRows,
        clearSelectedRowKeys,
        getSelectRowKeys,
        deleteSelectRowByKey,
        setSelectedRowKeys,
      } = useRowSelection(getProps, tableData, emit);

      const { getExpandOption, expandAll, expandRows, collapseAll, getIsExpanded } = useTableExpand(getProps, tableData, emit);

      const {
        handleTableChange: onTableChange,
        getDataSourceRef,
        getDataSource,
        getRawDataSource,
        getFetchParams,
        setTableData,
        updateTableDataRecord,
        deleteTableDataRecord,
        insertTableDataRecord,
        findTableDataRecord,
        fetch,
        getRowKey,
        reload,
        loadMore,
        hasMoreCursor,
        getCursorTotal,
        cursorTotalRef,
        loadingMoreRef,
        getAutoCreateKey,
        updateTableData,
      } = useDataSource(
        getProps,
        {
          tableData,
          getPaginationInfo,
          setLoading,
          setPagination,
          getFieldsValue: formActions.getFieldsValue,
          clearSelectedRowKeys,
          expandAll,
        },
        emit,
      );

      function handleTableChange(...args) {
        onTableChange.call(undefined, ...args);
        emit('change', ...args);
        // 解决通过useTable注册onChange时不起作用的问题
        const { onChange } = unref(getProps);
        onChange && isFunction(onChange) && onChange.call(undefined, ...args);
      }

      const { getViewColumns, getColumns, setCacheColumnsByField, setCacheColumns, setColumns, getColumnsRef, getCacheColumns } = useColumns(
        getProps,
        getPaginationInfo,
      );

      const { scrollTo } = useTableScrollTo(tableElRef, getDataSourceRef);

      const cursorPageSize = computed(() => {
        const { pagination } = unref(getProps);
        return (!isBoolean(pagination) && pagination?.pageSize) || 50;
      });

      const loadedItemsCount = computed(() => unref(getDataSourceRef)?.length || 0);

      const cursorActions = computed(() =>
        unref(getProps).paginationMode === 'cursor'
          ? {
              loadMore,
              hasMore: hasMoreCursor,
              loadingMore: loadingMoreRef,
              pageSize: unref(cursorPageSize),
              loadedItems: loadedItemsCount,
              totalItems: cursorTotalRef,
            }
          : undefined,
      );

      const { getScrollRef, redoHeight } = useTableScroll(
        getProps,
        tableElRef,
        getColumnsRef,
        getRowSelectionRef,
        getDataSourceRef,
        wrapRef,
        formRef,
        () => unref(cursorActions),
      );

      const { customRow } = useCustomRow(getProps, {
        setSelectedRowKeys,
        getSelectRowKeys,
        clearSelectedRowKeys,
        getAutoCreateKey,
        emit,
      });

      const { getRowClassName } = useTableStyle(getProps, prefixCls);

      const handlers: InnerHandlers = {
        onColumnsChange: (data: ColumnChangeParam[]) => {
          emit('columns-change', data);
          // support useTable
          unref(getProps).onColumnsChange?.(data);
        },
      };

      const { getHeaderProps } = useTableHeader(getProps, slots, handlers);

      // 加载中指示器：通过 DOM 操作切换显隐，避免触发 ant-design-vue 组件的响应式级联更新
      const cursorLoadingRef = ref<HTMLElement | null>(null);
      watch(loadingMoreRef, (loading) => {
        const el = cursorLoadingRef.value;
        if (el) el.style.display = loading ? 'flex' : 'none';
      }, { flush: 'post' });

      // 通过 provide 注入 cursor 状态给 CursorFooter（不通过 props 传递，保证 footer 函数引用稳定）
      provide(CURSOR_STATE_KEY, {
        cursorTotalRef,
        loadingMoreRef,
        hasMore: hasMoreCursor,
        loadedItems: loadedItemsCount,
        scrollTo,
      });

      const { getFooterProps } = useTableFooter(
        getProps,
        getScrollRef,
        tableElRef,
        getDataSourceRef,
      );

      const { getFormProps, replaceFormSlotKey, getFormSlotKeys, handleSearchInfoChange } = useTableForm(getProps, slots, fetch, getLoading);

      const getBindValues = computed(() => {
        const dataSource = unref(getDataSourceRef);
        const cursorMode = unref(isCursorMode);
        let propsData: Recordable = {
          ...attrs,
          ...unref(getProps),
          customRow: unref(getProps).customRow || customRow,
          ...unref(getHeaderProps),
          scroll: unref(getScrollRef),
          loading: unref(getLoading),
          tableLayout: 'fixed',
          rowSelection: unref(getRowSelectionRef),
          rowKey: unref(getRowKey),
          columns: toRaw(unref(getViewColumns)),
          pagination: toRaw(unref(getPaginationInfo)),
          dataSource,
          footer: cursorMode ? undefined : unref(getFooterProps),
          ...unref(getExpandOption),
        };
        // if (slots.expandedRowRender) {
        //   propsData = omit(propsData, 'scroll');
        // }

        propsData = omit(propsData, ['class', 'onChange']);
        return propsData;
      });

      const getWrapperClass = computed(() => {
        const values = unref(getBindValues);
        return [
          prefixCls,
          attrs.class,
          {
            [`${prefixCls}-form-container`]: values.useSearchForm,
            [`${prefixCls}--inset`]: values.inset,
          },
        ];
      });

      const getEmptyDataIsShowTable = computed(() => {
        const { emptyDataIsShowTable, useSearchForm } = unref(getProps);
        if (emptyDataIsShowTable || !useSearchForm) {
          return true;
        }
        return !!unref(getDataSourceRef).length;
      });

      function setProps(props: Partial<BasicTableProps>) {
        innerPropsRef.value = { ...unref(innerPropsRef), ...props };
      }

      const tableAction: TableActionType = {
        reload,
        getSelectRows,
        setSelectedRows,
        clearSelectedRowKeys,
        getSelectRowKeys,
        deleteSelectRowByKey,
        setPagination,
        setTableData,
        updateTableDataRecord,
        deleteTableDataRecord,
        insertTableDataRecord,
        findTableDataRecord,
        redoHeight,
        setSelectedRowKeys,
        setColumns,
        setLoading,
        getDataSource,
        getRawDataSource,
        getFetchParams,
        setProps,
        getRowSelection,
        getPaginationRef: getPagination,
        getColumns,
        getCacheColumns,
        emit,
        updateTableData,
        setShowPagination,
        getShowPagination,
        setCacheColumnsByField,
        expandAll,
        expandRows,
        collapseAll,
        getIsExpanded,
        scrollTo,
        getCursorTotal,
        loadMore,
        getSize: () => {
          return unref(getBindValues).size as SizeType;
        },
        setCacheColumns,
      };
      createTableContext({ ...tableAction, wrapRef, getBindValues });

      expose(tableAction);

      emit('register', tableAction, formActions);

      return {
        formRef,
        tableElRef,
        getBindValues,
        getLoading,
        registerForm,
        handleSearchInfoChange,
        getEmptyDataIsShowTable,
        handleTableChange,
        getRowClassName,
        wrapRef,
        tableAction,
        redoHeight,
        isCursorMode,
        cursorLoadingRef,
        getFormProps: getFormProps as any,
        replaceFormSlotKey,
        getFormSlotKeys,
        getWrapperClass,
        columns: getViewColumns,
      };
    },
  });
</script>
<style lang="less">
  @border-color: #cecece4d;

  @prefix-cls: ~'@{namespace}-basic-table';

  [data-theme='dark'] {
    .ant-table-tbody > tr:hover.ant-table-row-selected > td,
    .ant-table-tbody > tr.ant-table-row-selected td {
      background-color: #262626;
    }
  }

  .@{prefix-cls} {
    position: relative;
    max-width: 100%;
    height: 100%;

    &-row__striped {
      td {
        background-color: @app-content-background;
      }
    }

    &-form-container {
      .ant-form {
        width: 100%;
        padding: 10px 10px 0;
        margin-bottom: 10px;
        background-color: @component-background;
      }
    }

    .ant-table-cell {
      .ant-tag {
        margin-right: 0;
      }
    }

    .ant-table-wrapper {
      height: 100%;
      background-color: @component-background;
      border-radius: 2px;

      .ant-table-title {
        min-height: 40px;
        padding: 0 !important;
      }

      .ant-table.ant-table-bordered .ant-table-title {
        border: none !important;
      }
    }

    .ant-table {
      width: 100%;
      overflow-x: hidden;

      &-title {
        display: flex;
        border-bottom: none;
        justify-content: space-between;
        align-items: center;
      }

      //.ant-table-tbody > tr.ant-table-row-selected td {
      //background-color: fade(@primary-color, 8%) !important;
      //}
    }

    .ant-pagination {
      margin: 10px 0 0;
      padding: 0 10px 10px;
    }

    .ant-table-footer {
      padding: 0;

      .ant-table-wrapper {
        padding: 0;
      }

      table {
        border: none !important;
      }

      .ant-table-body {
        overflow-x: hidden !important;
        //  overflow-y: scroll !important;
      }

      td {
        padding: 12px 8px;
      }
    }

    &--inset {
      .ant-table-wrapper {
        padding: 0;
      }
    }

    .cursor-loading-indicator {
      display: none;
      position: absolute;
      top: 50%;
      left: 50%;
      transform: translate(-50%, -50%);
      align-items: center;
      gap: 8px;
      padding: 10px 24px;
      background: rgba(255, 255, 255, 0.95);
      border-radius: 20px;
      box-shadow: 0 2px 12px rgba(0, 0, 0, 0.15);
      z-index: 10;
      pointer-events: none;

      .cursor-spinner {
        width: 16px;
        height: 16px;
        border: 2px solid #e8e8e8;
        border-top-color: @primary-color;
        border-radius: 50%;
        animation: cursor-spin 0.8s linear infinite;
      }

      span {
        color: @primary-color;
        font-size: 14px;
      }
    }
  }

  @keyframes cursor-spin {
    to {
      transform: rotate(360deg);
    }
  }
</style>
