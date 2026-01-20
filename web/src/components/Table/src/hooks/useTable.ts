import type { BasicTableProps, TableActionType, FetchParams, BasicColumn } from '../types/table';
import type { PaginationProps } from '../types/pagination';
import type { DynamicProps } from '/#/utils';
import type { FormActionType } from '/@/components/Form';
import type { WatchStopHandle } from 'vue';
import { getDynamicProps } from '/@/utils';
import { ref, onUnmounted, unref, watch, toRaw } from 'vue';
import { isProdMode } from '/@/utils/env';
import { error } from '/@/utils/log';

type Props = Partial<DynamicProps<BasicTableProps>>;

type UseTableMethod = TableActionType & {
  getForm: () => FormActionType;
};

export function useTable(tableProps?: Props): [
  (instance: TableActionType, formInstance: UseTableMethod) => void,
  TableActionType & {
    getForm: () => FormActionType;
  },
] {
  const tableRef = ref<Nullable<TableActionType>>(null);
  const loadedRef = ref<Nullable<boolean>>(false);
  const formRef = ref<Nullable<UseTableMethod>>(null);

  let stopWatch: WatchStopHandle;

  function register(instance: TableActionType, formInstance: UseTableMethod) {
    isProdMode() &&
      onUnmounted(() => {
        tableRef.value = null;
        loadedRef.value = null;
      });

    if (unref(loadedRef) && isProdMode() && instance === unref(tableRef)) return;

    tableRef.value = instance;
    formRef.value = formInstance;
    tableProps && instance.setProps(getDynamicProps(tableProps));
    loadedRef.value = true;

    stopWatch?.();

    stopWatch = watch(
      () => tableProps,
      () => {
        tableProps && instance.setProps(getDynamicProps(tableProps));
      },
      {
        immediate: true,
        deep: true,
      },
    );
  }

  function getTableInstance(): TableActionType | null {
    const table = unref(tableRef);
    return table as TableActionType | null;
  }

  const methods: TableActionType & {
    getForm: () => FormActionType;
  } = {
    reload: async (opt?: FetchParams) => {
      const table = getTableInstance();
      if (!table) {
        console.warn('The table instance has not been obtained yet, please make sure the table is presented when performing the table operation!');
        return;
      }
      return await table.reload(opt);
    },
    setProps: (props: Partial<BasicTableProps>) => {
      const table = getTableInstance();
      if (!table) return;
      table.setProps(props);
    },
    redoHeight: () => {
      const table = getTableInstance();
      if (!table) return;
      table.redoHeight();
    },
    setSelectedRows: (rows: Recordable[]) => {
      const table = getTableInstance();
      if (!table) return;
      return toRaw(table.setSelectedRows(rows));
    },
    setLoading: (loading: boolean) => {
      const table = getTableInstance();
      if (!table) return;
      table.setLoading(loading);
    },
    getDataSource: () => {
      const table = getTableInstance();
      if (!table) return [];
      return table.getDataSource();
    },
    getRawDataSource: () => {
      const table = getTableInstance();
      if (!table) return [];
      return table.getRawDataSource();
    },
    getFetchParams: () => {
      const table = getTableInstance();
      if (!table) return {};
      return table.getFetchParams();
    },
    getColumns: ({ ignoreIndex = false }: { ignoreIndex?: boolean } = {}) => {
      const table = getTableInstance();
      if (!table) return [];
      const columns = table.getColumns({ ignoreIndex }) || [];
      return toRaw(columns);
    },
    setColumns: (columns: BasicColumn[]) => {
      const table = getTableInstance();
      if (!table) return;
      table.setColumns(columns);
    },
    setTableData: (values: any[]) => {
      const table = getTableInstance();
      if (!table) return;
      return table.setTableData(values);
    },
    setPagination: (info: Partial<PaginationProps>) => {
      const table = getTableInstance();
      if (!table) return;
      return table.setPagination(info);
    },
    deleteSelectRowByKey: (key: string) => {
      const table = getTableInstance();
      if (!table) return;
      table.deleteSelectRowByKey(key);
    },
    getSelectRowKeys: () => {
      const table = getTableInstance();
      if (!table) return [];
      return toRaw(table.getSelectRowKeys());
    },
    getSelectRows: () => {
      const table = getTableInstance();
      if (!table) return [];
      return toRaw(table.getSelectRows());
    },
    clearSelectedRowKeys: () => {
      const table = getTableInstance();
      if (!table) return;
      table.clearSelectedRowKeys();
    },
    setSelectedRowKeys: (keys: string[] | number[]) => {
      const table = getTableInstance();
      if (!table) return;
      table.setSelectedRowKeys(keys);
    },
    getPaginationRef: () => {
      const table = getTableInstance();
      if (!table) return undefined;
      return table.getPaginationRef();
    },
    getSize: () => {
      const table = getTableInstance();
      if (!table) return undefined;
      return toRaw(table.getSize());
    },
    updateTableData: (index: number, key: string, value: any) => {
      const table = getTableInstance();
      if (!table) return;
      return table.updateTableData(index, key, value);
    },
    deleteTableDataRecord: (rowKey: string | number | string[] | number[]) => {
      const table = getTableInstance();
      if (!table) return;
      return table.deleteTableDataRecord(rowKey);
    },
    insertTableDataRecord: (record: Recordable | Recordable[], index?: number) => {
      const table = getTableInstance();
      if (!table) return;
      return table.insertTableDataRecord(record, index);
    },
    updateTableDataRecord: (rowKey: string | number, record: Recordable) => {
      const table = getTableInstance();
      if (!table) return;
      return table.updateTableDataRecord(rowKey, record);
    },
    findTableDataRecord: (rowKey: string | number) => {
      const table = getTableInstance();
      if (!table) return undefined;
      return table.findTableDataRecord(rowKey);
    },
    getRowSelection: () => {
      const table = getTableInstance();
      if (!table) return undefined;
      return toRaw(table.getRowSelection());
    },
    getCacheColumns: () => {
      const table = getTableInstance();
      if (!table) return [];
      return toRaw(table.getCacheColumns());
    },
    getForm: () => {
      return unref(formRef) as unknown as FormActionType;
    },
    setShowPagination: async (show: boolean) => {
      const table = getTableInstance();
      if (!table) return;
      table.setShowPagination(show);
    },
    getShowPagination: () => {
      const table = getTableInstance();
      if (!table) return false;
      return toRaw(table.getShowPagination());
    },
    expandAll: () => {
      const table = getTableInstance();
      if (!table) return;
      table.expandAll();
    },
    expandRows: (keys: string[]) => {
      const table = getTableInstance();
      if (!table) return;
      table.expandRows(keys);
    },
    collapseAll: () => {
      const table = getTableInstance();
      if (!table) return;
      table.collapseAll();
    },
    scrollTo: (pos: string) => {
      const table = getTableInstance();
      if (!table) return;
      table.scrollTo(pos);
    },
  };

  return [register, methods];
}
