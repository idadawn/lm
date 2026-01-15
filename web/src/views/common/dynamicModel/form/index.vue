<template>
  <div class="page-content-wrapper bg-white">
    <FormPopup @register="registerFormPopup" />
    <FlowParser @register="registerFlowParser" @reload="getFlowOptions()" />
    <BasicModal v-bind="$attrs" @register="registerFlowListModal" title="请选择流程" :footer="null" :width="400" destroyOnClose class="jnpf-flow-list-modal">
      <div class="template-list">
        <ScrollContainer>
          <div class="template-item" v-for="item in flowList" :key="item.id" @click="selectFlow(item)">
            {{ item.fullName }}
          </div>
        </ScrollContainer>
      </div>
    </BasicModal>
  </div>
</template>

<script lang="ts" setup>
  import { reactive, onMounted, toRefs } from 'vue';
  import { getFlowList } from '/@/api/workFlow/flowEngine';
  import { useMessage } from '/@/hooks/web/useMessage';
  import { BasicModal, useModal } from '/@/components/Modal';
  import { usePopup } from '/@/components/Popup';
  import { ScrollContainer } from '/@/components/Container';
  import FormPopup from './FormPopup.vue';
  import FlowParser from '/@/views/workFlow/components/FlowParser.vue';

  interface State {
    flowList: any[];
    flowItem: any;
  }

  const props = defineProps(['config', 'modelId', 'isPreview']);
  const { createMessage } = useMessage();
  const [registerFormPopup, { openPopup: openFormPopup }] = usePopup();
  const [registerFlowParser, { openPopup: openFlowParser }] = usePopup();
  const [registerFlowListModal, { openModal: openFlowListModal, closeModal: closeFlowListModal }] = useModal();
  const state = reactive<State>({
    flowList: [],
    flowItem: {},
  });
  const { flowList } = toRefs(state);

  function getFlowOptions() {
    getFlowList(props.config.flowId, '1').then(res => {
      const flowList = res.data;
      state.flowList = flowList;
      if (state.flowItem.id) return selectFlow(state.flowItem);
      if (!flowList.length) return createMessage.error('流程不存在');
      if (flowList.length === 1) return selectFlow(flowList[0]);
      openFlowListModal(true);
    });
  }
  function selectFlow(item) {
    closeFlowListModal();
    state.flowItem = item;
    const data = {
      id: '',
      flowId: item.id,
      opType: '-1',
      hideCancelBtn: true,
      modelId: props.modelId,
      isPreview: props.isPreview,
    };
    openFlowParser(true, data);
  }
  function init() {
    if (props.config.enableFlow) return getFlowOptions();
    const data = {
      modelId: props.modelId,
      isPreview: props.isPreview,
      ...props.config,
    };
    openFormPopup(true, data);
  }

  onMounted(() => {
    init();
  });
</script>
