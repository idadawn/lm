<template>
  <BasicModal
    v-bind="$attrs"
    @register="registerModal"
    :title="title"
    helpMessage="小程序不支持在线JS脚本"
    :width="1000"
    @ok="handleSubmit"
    destroyOnClose
    class="form-script-modal">
    <div class="form-script-modal-body">
      <div class="main-board">
        <div class="main-board-editor">
          <MonacoEditor ref="editorRef" v-model="text" />
        </div>
        <div class="main-board-tips">
          <p>支持JavaScript的脚本</p>
          <template v-if="funcName === 'afterOnload'">
            <p>data--列表行数据，tableRef--表格DOM元素，request--异步请求(url,methods,data)</p>
          </template>
          <template v-if="funcName === 'rowStyle'">
            <p>row--列表行数据，rowIndex--列表行下标</p>
          </template>
          <template v-if="funcName === 'cellStyle'">
            <p>row--列表行数据，column--列表列数据</p>
            <p>rowIndex--列表行下标，columnIndex--列表列下标</p>
          </template>
        </div>
      </div>
    </div>
  </BasicModal>
</template>
<script lang="ts" setup>
  import { ref } from 'vue';
  import { BasicModal, useModalInner } from '/@/components/Modal';
  import { MonacoEditor } from '/@/components/CodeEditor';

  const emit = defineEmits(['register', 'confirm']);
  const [registerModal, { closeModal }] = useModalInner(init);
  const editorRef = ref(null);
  const text = ref('');
  const funcName = ref('');
  const title = ref('');
  function init(data) {
    text.value = data.text;
    funcName.value = data.funcName;
    title.value = getFuncText(data.funcName);
  }
  function getFuncText(key) {
    let text = '';
    switch (key) {
      case 'afterOnload':
        text = '表格事件';
        break;
      case 'rowStyle':
        text = '表格行样式';
        break;
      case 'cellStyle':
        text = '单元格样式';
        break;
      default:
        text = '';
        break;
    }
    return text;
  }
  function handleSubmit() {
    emit('confirm', text.value);
    closeModal();
  }
</script>
