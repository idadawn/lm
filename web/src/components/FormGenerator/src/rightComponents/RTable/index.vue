<template>
  <a-form-item label="显示标题">
    <a-switch v-model:checked="activeData.__config__.showTitle" />
  </a-form-item>
  <a-form-item label="动作文字">
    <a-input v-model:value="activeData.actionText" placeholder="请输入动作文字" />
  </a-form-item>
  <a-form-item label="动作设置">
    <jnpf-switch v-model:value="activeData.addType" />
  </a-form-item>
  <a-form-item label="动作表单" v-if="activeData.addType == 1">
    <a-button block @click="editConf()">配置表单</a-button>
  </a-form-item>
  <a-form-item label="合计设置">
    <jnpf-switch v-model:value="activeData.showSummary" />
  </a-form-item>
  <a-form-item label="合计字段" v-if="activeData.showSummary">
    <jnpf-select v-model:value="activeData.summaryField" placeholder="请选择合计字段" :options="childrenList" allowClear showSearch multiple />
  </a-form-item>
  <AddConfigModal @register="registerModal" @confirm="updateConf" />
</template>
<script lang="ts" setup>
  import { computed } from 'vue';
  import { useModal } from '/@/components/Modal';
  import AddConfigModal from './AddConfigModal.vue';

  const defaultAddTableConf = {
    popupTitle: '选择数据',
    popupType: 'dialog',
    popupWidth: '800px',
    interfaceId: '',
    interfaceName: '',
    templateJson: [],
    hasPage: true,
    pageSize: 20,
    columnOptions: [],
    relationOptions: [],
  };

  defineOptions({ inheritAttrs: false });
  const props = defineProps(['activeData']);
  const [registerModal, { openModal }] = useModal();

  const childrenList = computed(() => {
    const list = props.activeData.__config__.children.filter(o => ['input', 'inputNumber', 'calculate'].includes(o.__config__.jnpfKey) && o.__vModel__);
    return list.map(o => ({
      id: o.__vModel__,
      fullName: o.__config__.label,
    }));
  });

  function editConf() {
    if (!props.activeData.addTableConf) {
      props.activeData.addTableConf = JSON.parse(JSON.stringify(defaultAddTableConf));
    }
    let addTableConf = JSON.parse(JSON.stringify(props.activeData.addTableConf));
    openModal(true, { addTableConf, children: props.activeData.__config__.children });
  }
  function updateConf(data) {
    props.activeData.addTableConf = data;
  }
</script>
