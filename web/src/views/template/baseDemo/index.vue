<template>
  <div class="page-content-wrapper">
    <div class="page-content-wrapper-center">
      <div class="page-content-wrapper-content">
        <div @click="goChartEdtor">编辑仪表盘</div>
      </div>
    </div>
    <Form @register="registerForm" />
  </div>
</template>
<script lang="ts" setup>
import { usePopup } from '/@/components/Popup';
import Form from './Form.vue';

defineOptions({ name: 'permission-organize' });

const [registerForm, { openPopup: openFormPopup }] = usePopup();

const goChartEdtor = () => {
  const id = 'chartId1';
  openFormPopup(true, { id });
}

</script>
<style scoped>
.page-content-wrapper .page-content-wrapper-center .page-content-wrapper-content {
  /* flex: 1;
    overflow: hidden; */
  background: #fff;
}
</style>
