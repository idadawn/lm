<template>
  <div class="page-content-wrapper">
    <div class="page-content-wrapper-center">
      <div class="page-content-wrapper-content">
        <BasicTable @register="registerTable">
          <template #tableTitle>
            <a-button type="error" preIcon="icon-ym icon-ym-btn-clearn" @click="handleDelAllCache()">清空所有</a-button>
          </template>
          <template #bodyCell="{ column, record }">
            <template v-if="column.key === 'name'">
              <a :title="record.name" @click="handleView(record.name)">{{ record.name }}</a>
            </template>
            <template v-if="column.key === 'action'">
              <TableAction :actions="getTableActions(record)" />
            </template>
          </template>
        </BasicTable>
      </div>
    </div>
    <Form @register="registerForm" @reload="reload" />
  </div>
</template>
<script lang="ts" setup>
  import { BasicTable, useTable, TableAction, BasicColumn, ActionItem } from '/@/components/Table';
  import { useI18n } from '/@/hooks/web/useI18n';
  import { getCacheList, delCache, delAllCache } from '/@/api/system/cache';
  import { toFileSize } from '/@/utils/jnpf';
  import { useMessage } from '/@/hooks/web/useMessage';
  import { usePopup } from '/@/components/Popup';
  import Form from './Form.vue';

  defineOptions({ name: 'system-cache' });

  const { t } = useI18n();
  const { createMessage, createConfirm } = useMessage();
  const columns: BasicColumn[] = [
    { title: '名称', dataIndex: 'name', sorter: true },
    { title: '过期', dataIndex: 'overdueTime', sorter: true, width: 150, format: 'date|YYYY-MM-DD HH:mm' },
    { title: '大小', dataIndex: 'cacheSize', width: 90, format: toFileSize as any },
  ];
  const [registerForm, { openPopup: openFormPopup }] = usePopup();
  const [registerTable, { reload }] = useTable({
    api: getCacheList,
    columns,
    useSearchForm: true,
    formConfig: {
      schemas: [
        {
          field: 'keyword',
          label: t('common.keyword'),
          component: 'Input',
          componentProps: {
            placeholder: t('common.enterKeyword'),
            submitOnPressEnter: true,
          },
        },
      ],
    },
    actionColumn: {
      width: 50,
      title: '操作',
      dataIndex: 'action',
    },
  });

  function getTableActions(record): ActionItem[] {
    return [
      {
        label: t('common.delText'),
        color: 'error',
        modelConfirm: {
          onOk: handleDelete.bind(null, record.name),
        },
      },
    ];
  }
  function handleDelete(id) {
    delCache(id).then(res => {
      createMessage.success(res.msg);
      reload();
    });
  }
  function handleDelAllCache() {
    createConfirm({
      iconType: 'warning',
      title: t('common.tipTitle'),
      content: '您确定要清空所有缓存数据吗, 是否继续?',
      onOk: () => {
        delAllCache().then(res => {
          createMessage.success(res.msg);
          reload();
        });
      },
    });
  }
  function handleView(id) {
    openFormPopup(true, { id });
  }
</script>
