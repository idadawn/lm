<template>
  <BasicModal
    v-bind="$attrs"
    @register="registerModal"
    title="预览"
    @ok="handleConfirm"
    canFullscreen
    destroy-on-close
    :cancel-text="formConf.cancelButtonText"
    :ok-text="formConf.confirmButtonText">
    <Parser ref="parserRef" :formConf="formConf" @submit="submitForm" :key="key" v-if="!loading" />
  </BasicModal>
</template>
<script lang="ts" setup>
  import { ref, unref } from 'vue';
  import { BasicModal, useModalInner } from '/@/components/Modal';
  import Parser from '/@/components/FormGenerator/src/components/Parser.vue';

  defineProps(['formConf']);
  defineEmits(['register']);
  const [registerModal, { changeOkLoading }] = useModalInner(init);
  const parserRef = ref<any>(null);
  const loading = ref(true);
  const key = ref(+new Date());

  function init() {
    loading.value = true;
    key.value = +new Date();
    loading.value = false;
  }
  function getParser() {
    const parser = unref(parserRef);
    if (!parser) {
      throw new Error('parser is null!');
    }
    return parser;
  }

  function handleConfirm() {
    getParser().handleSubmit();
  }
  function submitForm(data, callback) {
    changeOkLoading(true);
    if (callback && typeof callback === 'function') callback();
    changeOkLoading(false);
  }
</script>
