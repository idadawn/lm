<template>
  <BasicModal v-bind="$attrs" @register="registerModal" :title="getTitle" showOkBtn @ok="handleSubmit">
    <errorDetailMsgModal />
  </BasicModal>
</template>
<script lang="ts" setup>
  import { unref, computed, reactive } from 'vue';
  import { BasicModal, useModalInner } from '/@/components/Modal';
  import errorDetailMsgModal from './errorDetailMsgModal.vue';

  interface State {
    dataForm: any;
  }

  const state = reactive<State>({
    dataForm: {},
  });
  const getTitle = '预警详情';
  const emit = defineEmits(['register', 'reload']);
  const [registerModal, { closeModal }] = useModalInner(init);

  function init(data) {}
  async function handleSubmit() {
    closeModal();
  }
</script>
<style lang="less" scoped>
  .tem-container {
    background: @app-main-background;
    overflow: auto;
    position: relative;
    padding: 0;
    .signs {
      position: absolute;
      right: 10px;
      top: 70px;
      &.print {
        top: 20px;
      }
    }
    .tem_list {
      width: 800px;
      margin: 0 auto;
      background: @component-background;
      color: @text-color-label;
      position: relative;
      padding: 0 40px 15px;
      font-size: 12px;
      position: relative;
    }

    h1 {
      padding-top: 36px;
      font-size: 24px;
    }
    h2 {
      font-size: 18px;
    }
    h1,
    h2 {
      text-align: center;
      color: @text-color-label;
      font-weight: 700;
    }
    .title {
      border-bottom: 2px dashed #000;
      padding-left: 30px;
      line-height: 30px;
      font-size: 12px;
    }
    .lip {
      padding: 20px 0;
    }
    .table-box {
      padding: 0 1px 0 0;
    }
    .demo-form-inline {
      display: flex;
      flex-wrap: wrap;
      justify-content: space-between;
    }
    .temdate {
      text-align: right;
      margin: 20px 60px;
    }
    .seal {
      text-align: right;
      margin: 10px 120px;
      padding-bottom: 40px;
    }
    .signBtn {
      display: flex;
      justify-content: space-between;
    }
    .content1 {
      display: flex;
      justify-content: space-between;
      align-items: center;
      flex-wrap: wrap;
      .item {
        width: 345px;
        padding: 8px 0;
        border-bottom: 1px solid @border-color-base1;
        span {
          font-size: 12px;
          padding: 10px 0;
          color: @text-color-label;
        }
      }
    }
    .vdr.active:before {
      outline: 0;
    }
    .vdr.active {
      img {
        width: 100%;
        height: 100%;
      }
    }
  }
</style>
