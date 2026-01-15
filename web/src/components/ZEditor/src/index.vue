<template>
  <div class="z-editor-page">
    <div class="mindMapContent">
      <MindMap ref="mindMapRef" :source="source" :auth-add="true" :auth-delete="true" />
    </div>
    <a-drawer v-model:visible="drawerStatus" :title="currentItem.name" placement="right">
      <z-editor-form
        v-if="currentItem"
        :form="currentItem"
        :status-options="statusOptions"
        @update="updateItem"
        @delete="deleteItem" />
    </a-drawer>
  </div>
</template>
<script lang="ts" setup>
  import { ref, toRefs, createVNode } from 'vue';
  import { props as _props } from './props';
  import zEditorForm from '../components/editorForm/index.vue';
  import { deleteIndicatorValueChain } from '/@/api/createModel/model';
  import { ResultEnum } from '/@/enums/httpEnum';
  import { message, Modal, Drawer as ADrawer } from 'ant-design-vue';
  import { ExclamationCircleOutlined } from '@ant-design/icons-vue';
  import { useI18n } from '/@/hooks/web/useI18n';
  import { MindMap } from '/@/components/MindMap';
  import { useMindMapCallback, useMindMapResult } from '/@/components/MindMap/hooks/useMindMap';
  // import { useMindMapCallback, useMindMapResult } from '/@/components/MindMap/hooks/useMindMapMore';

  defineOptions({
    name: 'ZEditor',
  });
  const { t } = useI18n();
  const props = defineProps(_props);
  const { source, statusOptions } = toRefs(props);
  // 画布元素
  const mindMapRef = ref(null);

  /**
   * @description 右侧面板显示
   */
  const drawerStatus = ref(false);

  useMindMapCallback({
    nodeClick(_node) {
      drawerStatus.value = true;
    },
  });

  const { graph, updateItem, currentItem } = useMindMapResult();

  /**
   * @description 删除
   * @param { String } nodeId
   * @param { Object } _model
   */
  const deleteItem = (nodeId?, _model?) => {
    Modal.confirm({
      title: t('common.tipTitle'),
      icon: createVNode(ExclamationCircleOutlined),
      centered: true,
      content: t('common.delTip'),
      onOk() {
        const id = nodeId ? nodeId : currentItem.value.id;
        return deleteIndicatorValueChain(id).then(res => {
          if (res.code === ResultEnum.SUCCESS) {
            message.success(res.msg);
            graph.value.removeChild(id);
          } else {
            message.error(res.msg);
          }
        });
      },
      onCancel() {},
    });
  };
</script>
<style lang="less" scoped>
  .z-editor-page {
    height: 100%;
    width: 100%;
    display: flex;
    justify-content: space-between;

    .editor-left {
      width: 200px;
      height: 100%;
      border-right: 1px solid #ccc;
    }

    .mindMapContent {
      height: 100%;
      flex: 1;
      z-index: 1;
    }

    .sketchpad {
      height: 100%;
      flex: 1;
      z-index: 1;
    }

    .editor-right {
      width: 300px;
      height: 100%;
      border-left: 1px solid #ccc;
      padding: 8px 10px;
      box-sizing: border-box;
      animation-duration: 0.2s;
    }
  }
</style>
