<template>
  <BasicModal v-bind="$attrs" @register="registerModal" title="新建流程表单" :footer="null" :width="600" class="jnpf-add-modal">
    <div class="add-main">
      <div class="add-item add-item-left" @click="add(0, 2)">
        <i class="add-icon icon-ym icon-ym-launchFlow"></i>
        <div class="add-txt">
          <p class="add-title">自定义表单</p>
          <p class="add-desc">在线可视化流程表单</p>
        </div>
      </div>
      <div class="add-item" @click="add(0, 1)">
        <i class="add-icon icon-ym icon-ym-funcFlow"></i>
        <div class="add-txt">
          <p class="add-title">系统表单</p>
          <p class="add-desc">代码生成的流程表单</p>
        </div>
      </div>
    </div>
  </BasicModal>
</template>
<script lang="ts" setup>
  import { BasicModal, useModalInner } from '/@/components/Modal';

  const emit = defineEmits(['register', 'select']);

  const [registerModal, { closeModal }] = useModalInner();

  function add(flowType, formType) {
    emit('select', flowType, formType);
    closeModal();
  }
</script>
