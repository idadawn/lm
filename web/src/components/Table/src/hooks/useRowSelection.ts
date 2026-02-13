import { isFunction } from '/@/utils/is';
import type { BasicTableProps, TableRowSelection } from '../types/table';
import { computed, ComputedRef, nextTick, Ref, ref, toRaw, unref, watch } from 'vue';
import { ROW_KEY } from '../const';
import { omit } from 'lodash-es';
import { findNodeAll } from '/@/utils/helper/treeHelper';

export function useRowSelection(propsRef: ComputedRef<BasicTableProps>, tableData: Ref<Recordable[]>, emit: EmitType) {
  const selectedRowKeysRef = ref<string[]>([]);
  const selectedRowRef = ref<Recordable[]>([]);

  const getRowSelectionRef = computed((): TableRowSelection | null => {
    const { rowSelection } = unref(propsRef);
    if (!rowSelection) {
      return null;
    }

    return {
      selectedRowKeys: unref(selectedRowKeysRef),
      onChange: (selectedRowKeys: string[]) => {
        // 延迟更新，避免同步 reactive 级联导致 80+ 列表格卡顿
        nextTick(() => setSelectedRowKeys(selectedRowKeys));
      },
      ...omit(rowSelection, ['onChange']),
    };
  });

  watch(
    () => unref(propsRef).rowSelection?.selectedRowKeys,
    (v: string[]) => {
      setSelectedRowKeys(v);
    },
  );

  // 选中行变化时通知外部（不使用 deep，因为我们总是替换数组引用而非原地修改）
  let emitTimer: ReturnType<typeof setTimeout> | null = null;
  watch(
    selectedRowKeysRef,
    () => {
      // 防抖 50ms，避免快速连续选择时频繁触发
      if (emitTimer) clearTimeout(emitTimer);
      emitTimer = setTimeout(() => {
        const { rowSelection } = unref(propsRef);
        if (rowSelection) {
          const { onChange } = rowSelection;
          if (onChange && isFunction(onChange)) onChange(getSelectRowKeys(), getSelectRows());
        }
        if (unref(tableData).length) {
          emit('selection-change', {
            keys: getSelectRowKeys(),
            rows: getSelectRows(),
          });
        }
      }, 50);
    },
  );

  const getAutoCreateKey = computed(() => {
    return unref(propsRef).autoCreateKey && !unref(propsRef).rowKey;
  });

  const getRowKey = computed(() => {
    const { rowKey } = unref(propsRef);
    return unref(getAutoCreateKey) ? ROW_KEY : rowKey;
  });

  function setSelectedRowKeys(rowKeys: string[]) {
    selectedRowKeysRef.value = rowKeys;
    // 延迟执行昂贵的行查找操作，只同步更新 keys（控制复选框状态）
    nextTick(() => {
      const rowKeyField = unref(getRowKey) as string;
      const keySet = new Set(rowKeys);
      const allSelectedRows = findNodeAll(
        toRaw(unref(tableData)).concat(toRaw(unref(selectedRowRef))),
        item => keySet.has(item[rowKeyField]),
        {
          children: propsRef.value.childrenColumnName ?? 'children',
        },
      );
      // 按 rowKeys 顺序排列
      const rowMap = new Map<string, Recordable>();
      allSelectedRows.forEach(row => rowMap.set(row[rowKeyField], row));
      const trueSelectedRows: Recordable[] = [];
      rowKeys?.forEach((key: string) => {
        const found = rowMap.get(key);
        if (found) trueSelectedRows.push(found);
      });
      selectedRowRef.value = trueSelectedRows;
    });
  }

  function setSelectedRows(rows: Recordable[]) {
    selectedRowRef.value = rows;
  }

  function clearSelectedRowKeys() {
    selectedRowRef.value = [];
    selectedRowKeysRef.value = [];
  }

  function deleteSelectRowByKey(key: string) {
    selectedRowKeysRef.value = unref(selectedRowKeysRef).filter(item => item !== key);
  }

  function getSelectRowKeys() {
    return unref(selectedRowKeysRef);
  }

  function getSelectRows<T = Recordable>() {
    return unref(selectedRowRef) as T[];
  }

  function getRowSelection() {
    return unref(getRowSelectionRef)!;
  }

  return {
    getRowSelection,
    getRowSelectionRef,
    getSelectRows,
    getSelectRowKeys,
    setSelectedRowKeys,
    clearSelectedRowKeys,
    deleteSelectRowByKey,
    setSelectedRows,
  };
}
