import type { BasicTableProps, FetchParams, SorterResult } from '../types/table';
import type { PaginationProps } from '../types/pagination';
import { ref, shallowRef, unref, ComputedRef, computed, onMounted, watch, reactive, Ref, watchEffect, nextTick } from 'vue';
import { useTimeoutFn } from '/@/hooks/core/useTimeout';
import { buildUUID } from '/@/utils/uuid';
import { isFunction, isBoolean, isObject } from '/@/utils/is';
import { get, cloneDeep, merge } from 'lodash-es';
import { FETCH_SETTING, ROW_KEY, PAGE_SIZE } from '../const';

interface ActionType {
  getPaginationInfo: ComputedRef<boolean | PaginationProps>;
  setPagination: (info: Partial<PaginationProps>) => void;
  setLoading: (loading: boolean) => void;
  getFieldsValue: () => Recordable;
  clearSelectedRowKeys: () => void;
  expandAll: () => void;
  tableData: Ref<Recordable[]>;
}

interface SearchState {
  sortInfo: Recordable;
  filterInfo: Record<string, string[]>;
}
export function useDataSource(
  propsRef: ComputedRef<BasicTableProps>,
  { getPaginationInfo, setPagination, setLoading, getFieldsValue, clearSelectedRowKeys, expandAll, tableData }: ActionType,
  emit: EmitType,
) {
  const searchState = reactive<SearchState>({
    sortInfo: {},
    filterInfo: {},
  });
  const dataSourceRef = shallowRef<Recordable[]>([]);
  const rawDataSourceRef = ref<Recordable>({});
  const fetchParams = ref<Recordable>({});
  // 游标分页：下一页游标（无更多为 null）
  const nextCursorRef = ref<string | number | null>(null);
  // 游标分页：服务端返回的总条数（用于展示「共 N 条」）
  const cursorTotalRef = ref(0);
  // 游标分页：是否正在加载更多（避免重复请求）
  const loadingMoreRef = ref(false);

  watchEffect(() => {
    tableData.value = unref(dataSourceRef);
  });
  watch(
    () => unref(propsRef).dataSource,
    () => {
      const { dataSource, api } = unref(propsRef);
      !api && dataSource && (dataSourceRef.value = dataSource);
    },
    {
      immediate: true,
      deep: true,
    },
  );

  function handleTableChange(pagination: PaginationProps, filters: Partial<Recordable<string[]>>, sorter: SorterResult) {
    const { clearSelectOnPageChange, sortFn, filterFn } = unref(propsRef);
    if (clearSelectOnPageChange) {
      clearSelectedRowKeys();
    }
    setPagination(pagination);

    const params: Recordable = {};
    if (sorter && isFunction(sortFn)) {
      const sortInfo = sortFn(sorter);
      searchState.sortInfo = sortInfo;
      params.sortInfo = sortInfo;
    }

    if (filters && isFunction(filterFn)) {
      const filterInfo = filterFn(filters);
      searchState.filterInfo = filterInfo;
      params.filterInfo = filterInfo;
    }
    fetch(params);
  }

  function setTableKey(items: any[]) {
    if (!items || !Array.isArray(items)) return;
    items.forEach(item => {
      if (!item[ROW_KEY]) {
        item[ROW_KEY] = buildUUID();
      }
      if (item.children && item.children.length) {
        setTableKey(item.children);
      }
    });
  }

  const getAutoCreateKey = computed(() => {
    return unref(propsRef).autoCreateKey && !unref(propsRef).rowKey;
  });

  const getRowKey = computed(() => {
    const { rowKey } = unref(propsRef);
    return unref(getAutoCreateKey) ? ROW_KEY : rowKey;
  });

  const getDataSourceRef = computed(() => {
    const dataSource = unref(dataSourceRef);
    if (!dataSource || dataSource.length === 0) {
      return unref(dataSourceRef);
    }
    if (unref(getAutoCreateKey)) {
      const firstItem = dataSource[0];
      const lastItem = dataSource[dataSource.length - 1];

      if (firstItem && lastItem) {
        if (!firstItem[ROW_KEY] || !lastItem[ROW_KEY]) {
          const data = cloneDeep(unref(dataSourceRef));
          data.forEach(item => {
            if (!item[ROW_KEY]) {
              item[ROW_KEY] = buildUUID();
            }
            if (item.children && item.children.length) {
              setTableKey(item.children);
            }
          });
          dataSourceRef.value = data;
        }
      }
    }
    return unref(dataSourceRef);
  });

  async function updateTableData(index: number, key: string, value: any) {
    const record = dataSourceRef.value[index];
    if (record) {
      record[key] = value;
      // shallowRef 需要新引用才能触发更新
      dataSourceRef.value = dataSourceRef.value.slice();
    }
    return dataSourceRef.value[index];
  }

  function updateTableDataRecord(rowKey: string | number, record: Recordable): Recordable | undefined {
    const row = findTableDataRecord(rowKey);

    if (row) {
      for (const field in row) {
        if (Reflect.has(record, field)) row[field] = record[field];
      }
      dataSourceRef.value = dataSourceRef.value.slice();
      return row;
    }
  }

  function deleteTableDataRecord(rowKey: string | number | string[] | number[]) {
    if (!dataSourceRef.value || dataSourceRef.value.length == 0) return;
    const rowKeyName = unref(getRowKey);
    if (!rowKeyName) return;
    const rowKeys = !Array.isArray(rowKey) ? [rowKey] : rowKey;

    function deleteRow(data, key) {
      const row: { index: number; data: [] } = findRow(data, key);
      if (row === null || row.index === -1) {
        return;
      }
      row.data.splice(row.index, 1);

      function findRow(data, key) {
        if (data === null || data === undefined) {
          return null;
        }
        for (let i = 0; i < data.length; i++) {
          const row = data[i];
          let targetKeyName: string = rowKeyName as string;
          if (isFunction(rowKeyName)) {
            targetKeyName = rowKeyName(row);
          }
          if (row[targetKeyName] === key) {
            return { index: i, data };
          }
          if (row.children?.length > 0) {
            const result = findRow(row.children, key);
            if (result != null) {
              return result;
            }
          }
        }
        return null;
      }
    }

    for (const key of rowKeys) {
      deleteRow(dataSourceRef.value, key);
      deleteRow(unref(propsRef).dataSource, key);
    }
    dataSourceRef.value = dataSourceRef.value.slice();
    setPagination({
      total: unref(propsRef).dataSource?.length,
    });
  }

  function insertTableDataRecord(record: Recordable | Recordable[], index: number): Recordable[] | undefined {
    // if (!dataSourceRef.value || dataSourceRef.value.length == 0) return;
    index = index ?? dataSourceRef.value?.length;
    const _record = isObject(record) ? [record as Recordable] : (record as Recordable[]);
    const arr = unref(dataSourceRef);
    arr.splice(index, 0, ..._record);
    dataSourceRef.value = arr.slice();
    return unref(dataSourceRef);
  }

  function findTableDataRecord(rowKey: string | number) {
    if (!dataSourceRef.value || dataSourceRef.value.length == 0) return;

    const rowKeyName = unref(getRowKey);
    if (!rowKeyName) return;

    const { childrenColumnName = 'children' } = unref(propsRef);

    const findRow = (array: any[]) => {
      let ret;
      array.some(function iter(r) {
        if (typeof rowKeyName === 'function') {
          if ((rowKeyName(r) as string) === rowKey) {
            ret = r;
            return true;
          }
        } else {
          if (Reflect.has(r, rowKeyName) && r[rowKeyName] === rowKey) {
            ret = r;
            return true;
          }
        }
        return r[childrenColumnName] && r[childrenColumnName].some(iter);
      });
      return ret;
    };

    // const row = dataSourceRef.value.find(r => {
    //   if (typeof rowKeyName === 'function') {
    //     return (rowKeyName(r) as string) === rowKey
    //   } else {
    //     return Reflect.has(r, rowKeyName) && r[rowKeyName] === rowKey
    //   }
    // })
    return findRow(dataSourceRef.value);
  }

  const isCursorMode = computed(() => unref(propsRef).paginationMode === 'cursor');

  async function fetch(opt?: FetchParams) {
    const { api, searchInfo, defSort, fetchSetting, beforeFetch, afterFetch, useSearchForm, pagination } = unref(propsRef);
    if (!api || !isFunction(api)) return;
    const cursorMode = unref(isCursorMode);
    // loadMore 总是传 cursor 参数，reload/首屏不传 cursor → 用 cursor 区分
    const isFirstOrReload = cursorMode && opt?.cursor === undefined;
    const isLoadMore = cursorMode && opt?.cursor !== undefined;

    try {
      if (!isLoadMore) setLoading(true);
      else loadingMoreRef.value = true;

      const { pageField, sizeField, listField, totalField, cursorField, nextCursorField } = Object.assign(
        {},
        FETCH_SETTING,
        fetchSetting,
      );
      let pageParams: Recordable = {};

      const paginationInfo = unref(getPaginationInfo) as PaginationProps | boolean;
      const pageSize = isBoolean(paginationInfo) ? PAGE_SIZE : (paginationInfo?.pageSize ?? PAGE_SIZE);
      const current = isBoolean(paginationInfo) ? 1 : (paginationInfo?.current ?? 1);
      const isNoPagination = (isBoolean(pagination) && !pagination) || isBoolean(getPaginationInfo);

      if (cursorMode) {
        // 游标模式：首屏或 reload 不传 cursor；加载更多传 cursor（或兼容用 page）
        const cursorParamName = cursorField || pageField;
        if (isFirstOrReload) {
          pageParams[pageField] = 1;
          pageParams[sizeField] = pageSize;
          nextCursorRef.value = null;
        } else {
          const cursorVal = opt?.cursor ?? opt?.page ?? 1;
          pageParams[cursorParamName] = cursorVal;
          pageParams[sizeField] = pageSize;
        }
      } else if (isNoPagination) {
        pageParams = {};
      } else {
        pageParams[pageField] = (opt && opt.page) || current;
        pageParams[sizeField] = pageSize;
      }

      const { sortInfo = {}, filterInfo } = searchState;

      let params: Recordable = merge(
        pageParams,
        useSearchForm ? getFieldsValue() : {},
        searchInfo,
        opt?.searchInfo ?? {},
        defSort,
        sortInfo,
        filterInfo,
        opt?.sortInfo ?? {},
        opt?.filterInfo ?? {},
      );
      if (beforeFetch && isFunction(beforeFetch)) {
        params = (await beforeFetch(params)) || params;
      }
      fetchParams.value = params;

      const res = await api(params);
      const data = res.data;
      rawDataSourceRef.value = data;

      const isArrayResult = Array.isArray(data);

      let resultItems: Recordable[] = isArrayResult ? data : get(data, listField);
      let resultTotal: number = 0;
      if (!isNoPagination || cursorMode) resultTotal = isArrayResult ? data.length : get(data, totalField);

      if (cursorMode) {
        cursorTotalRef.value = resultTotal || 0;
        if (afterFetch && isFunction(afterFetch)) {
          resultItems = (await afterFetch(resultItems)) || resultItems;
        }
        if (isFirstOrReload) {
          dataSourceRef.value = resultItems;
          // 兼容模式：无 nextCursorField 时用页码模拟；有则用接口返回
          if (nextCursorField) {
            nextCursorRef.value = get(data, nextCursorField) ?? (resultItems.length >= pageSize ? 2 : null);
          } else {
            nextCursorRef.value = resultItems.length >= pageSize ? 2 : null;
          }
        } else {
          const prev = dataSourceRef.value || [];
          dataSourceRef.value = [...prev, ...resultItems];
          const currentPage = Number(params[pageField] ?? params[cursorField || pageField] ?? 1);
          if (nextCursorField) {
            nextCursorRef.value = get(data, nextCursorField) ?? null;
          } else {
            nextCursorRef.value = resultItems.length >= pageSize ? currentPage + 1 : null;
          }
        }
        // ⚠️ cursor 模式下不调用 setPagination，避免写入 configRef 触发无限递归更新
      } else {
        // 假如数据变少，导致总页数变少并小于当前选中页码
        if (Number(resultTotal)) {
          const currentTotalPage = Math.ceil(resultTotal / pageSize);
          if (current > currentTotalPage) {
            setPagination({ current: currentTotalPage });
            return await fetch(opt);
          }
        }
        if (afterFetch && isFunction(afterFetch)) {
          resultItems = (await afterFetch(resultItems)) || resultItems;
        }
        dataSourceRef.value = resultItems;
        setPagination({ total: resultTotal || 0 });
        if (opt && opt.page) {
          setPagination({ current: opt.page || 1 });
        }
      }

      emit('fetch-success', {
        items: cursorMode ? unref(dataSourceRef) : resultItems,
        total: cursorMode ? cursorTotalRef.value : resultTotal,
      });
      return resultItems;
    } catch (error) {
      emit('fetch-error', error);
      if (!isLoadMore) dataSourceRef.value = [];
      // cursor 模式下不调用 setPagination
      if (!cursorMode) {
        setPagination({ total: 0 });
      }
    } finally {
      const { isTreeTable, defaultExpandAllRows } = unref(propsRef);
      nextTick(() => {
        if (isTreeTable && defaultExpandAllRows) expandAll();
      });
      setLoading(false);
      loadingMoreRef.value = false;
    }
  }

  /** 游标模式：加载下一页（滚动到底部约 20% 时由 useCursorScroll 调用） */
  async function loadMore() {
    if (!unref(isCursorMode) || nextCursorRef.value == null || loadingMoreRef.value) return;
    await fetch({ cursor: nextCursorRef.value, page: typeof nextCursorRef.value === 'number' ? nextCursorRef.value : undefined });
  }

  const hasMoreCursor = computed(() => nextCursorRef.value != null);
  const getCursorTotal = () => cursorTotalRef.value;

  function setTableData<T = Recordable>(values: T[]) {
    dataSourceRef.value = values as [];
  }

  function getDataSource<T = Recordable>() {
    return getDataSourceRef.value as T[];
  }

  function getRawDataSource<T = Recordable>() {
    return rawDataSourceRef.value as T;
  }

  function getFetchParams<T = Recordable>() {
    return fetchParams.value as T;
  }

  async function reload(opt?: FetchParams) {
    return await fetch(opt);
  }

  onMounted(() => {
    useTimeoutFn(() => {
      unref(propsRef).immediate && fetch();
    }, 16);
  });

  return {
    getDataSourceRef,
    getDataSource,
    getRawDataSource,
    getFetchParams,
    getRowKey,
    setTableData,
    getAutoCreateKey,
    fetch,
    reload,
    loadMore,
    hasMoreCursor,
    getCursorTotal,
    cursorTotalRef,
    isCursorMode,
    loadingMoreRef,
    updateTableData,
    updateTableDataRecord,
    deleteTableDataRecord,
    insertTableDataRecord,
    findTableDataRecord,
    handleTableChange,
  };
}
