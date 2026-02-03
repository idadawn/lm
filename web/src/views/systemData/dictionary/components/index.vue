<template>
  <BasicDrawer v-bind="$attrs" @register="registerDrawer" title="字典分类管理" width="700px" class="full-drawer"
    destroy-on-close>
    <BasicTable @register="registerTable">
      <template #tableTitle>
        <a-button type="primary" preIcon="icon-ym icon-ym-btn-add" @click="addOrUpdateHandle()">{{ t('common.addText')
          }}</a-button>
        <jnpf-upload-btn url="/api/system/DictionaryData/Action/Import" accept=".bdd"
          @on-success="reload"></jnpf-upload-btn>
      </template>
      <template #bodyCell="{ column, record }">
        <template v-if="column.key === 'isTree'">
          <a-tag :color="record.isTree == 1 ? 'success' : 'error'">{{ record.isTree == 1 ? '是' : '否' }}</a-tag>
        </template>
        <template v-if="column.key === 'action'">
          <TableAction :actions="[
            {
              label: t('common.editText'),
              onClick: addOrUpdateHandle.bind(null, record.id),
            },
            {
              label: t('common.delText'),
              color: 'error',
              modelConfirm: {
                onOk: handleDelete.bind(null, record.id),
              },
            },
            {
              label: t('common.exportText'),
              modelConfirm: {
                onOk: handleExport.bind(null, record.id),
                content: '您确定要导出该字典分类, 是否继续?',
              },
            },
          ]" />
        </template>
      </template>
    </BasicTable>
  </BasicDrawer>
  <BasicForm @register="registerForm" @reload="reload" />
</template>
<script lang="ts" setup>
import { getDictionaryType, delDictionaryType, exportData } from '/@/api/systemData/dictionary';
import BasicForm from './Form.vue';
import { BasicDrawer, useDrawerInner } from '/@/components/Drawer';
import { BasicTable, useTable, TableAction, BasicColumn } from '/@/components/Table';
import { useModal } from '/@/components/Modal';
import { useBaseStore } from '/@/store/modules/base';
import { useMessage } from '/@/hooks/web/useMessage';
import { useI18n } from '/@/hooks/web/useI18n';
import { downloadByUrl } from '/@/utils/file/download';

const { createMessage } = useMessage();
const { t } = useI18n();
const baseStore = useBaseStore();
const [registerDrawer, { }] = useDrawerInner();
const [registerForm, { openModal: openFormModal }] = useModal();
const columns: BasicColumn[] = [
  {
    title: '名称',
    dataIndex: 'fullName',
  },
  {
    title: '编码',
    dataIndex: 'enCode',
  },
  {
    title: '排序',
    dataIndex: 'sortCode',
    width: 70,
    align: 'center',
  },
  {
    title: '是否树',
    dataIndex: 'isTree',
    width: 70,
    align: 'center',
  },
];
const [registerTable, { reload }] = useTable({
  api: getDictionaryType,
  columns,
  pagination: false,
  isTreeTable: true,
  resizeHeightOffset: -10,
  actionColumn: {
    width: 150,
    title: '操作',
    dataIndex: 'action',
  },
});

function addOrUpdateHandle(id = '') {
  openFormModal(true, { id });
}
function handleDelete(id) {
  return delDictionaryType(id).then(res => {
    createMessage.success(res.msg);
    baseStore.setDictionaryList();
    reload();
  }).catch(error => {
    console.error('删除字典类型失败:', error);
    // 确保错误时也能正确关闭弹窗
    throw error;
  }).finally(() => {
    // 强制清除可能残留的遮罩层
    setTimeout(() => {
      // 查找所有确认对话框相关的元素
      const confirmModals = document.querySelectorAll('.ant-modal-confirm');
      confirmModals.forEach(modal => {
        const wrap = modal.closest('.ant-modal-wrap.ant-modal-confirm-centered');
        if (wrap) {
          (wrap as HTMLElement).style.display = 'none';
          if (wrap.parentNode) {
            wrap.parentNode.removeChild(wrap);
          }
        }
      });

      // 清理所有modal相关的遮罩层
      const allModalMasks = document.querySelectorAll('.ant-modal-mask');
      allModalMasks.forEach(mask => {
        if (mask.parentNode) {
          mask.parentNode.removeChild(mask);
        }
      });

      // 清理body上的样式
      document.body.style.overflow = '';
      document.body.style.paddingRight = '';

      // 移除所有ant-modal相关的类
      const allModals = document.querySelectorAll('[class*="ant-modal"]');
      allModals.forEach(el => {
        if (el.className.includes('ant-modal') && el.parentNode) {
          el.parentNode.removeChild(el);
        }
      });
    }, 0);
  });
}
function handleExport(id) {
  exportData(id).then(res => {
    downloadByUrl({ url: res.data.url });
  });
}
</script>
