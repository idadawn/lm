<template>
  <BasicDrawer
    v-bind="$attrs"
    @register="registerDrawer"
    width="280px"
    class="full-drawer message-drawer"
    :closeFunc="handleClose">
    <template #title>
      <div class="title">预警列表</div>
      <!-- <p class="link-text" @click="gotoCenter">更多</p> -->
    </template>
    <div class="content1">
      <div class="item" @click="gotoCenter">
        <span>****</span>
        <span>超出</span>
        <span>上限</span>
      </div>
      <div class="item">
        <span>****</span>
        <span>超出</span>
        <span>上限</span>
      </div>
    </div>
  </BasicDrawer>
  <Form @register="registerForm" @reload="reload" />
</template>
<script lang="ts" setup>
  import { getMessageList, readAllMsg, readInfo as readMsgInfo } from '/@/api/system/message';
  import { useModal } from '/@/components/Modal';
  import { BasicDrawer, useDrawerInner } from '/@/components/Drawer';
  import { ScrollContainer, ScrollActionType } from '/@/components/Container';
  import { useMessage } from '/@/hooks/web/useMessage';
  import { useI18n } from '/@/hooks/web/useI18n';
  import { reactive, toRefs, nextTick, ref } from 'vue';
  import { Empty, Badge } from 'ant-design-vue';
  import { useWebSocket } from '/@/hooks/web/useWebSocket';
  import { useRouter } from 'vue-router';
  import { DownOutlined } from '@ant-design/icons-vue';
  import { toDateValue } from '/@/utils/jnpf';
  import { encryptByBase64 } from '/@/utils/cipher';

  import Form from './homeModal.vue';

  interface State {
    finish: boolean;
    loading: boolean;
    listQuery: any;
    list: any[];
    type: string;
    isNoRead: boolean;
  }
  const emit = defineEmits(['register', 'readMsg']);
  const router = useRouter();
  const { t } = useI18n();
  const simpleImage = ref(Empty.PRESENTED_IMAGE_SIMPLE);
  const [registerDetail, { openModal: openDetailModal }] = useModal();
  const [registerScheduleDetail, { openModal: openScheduleDetailModal }] = useModal();
  const [registerDrawer, { changeLoading, closeDrawer }] = useDrawerInner(init);
  const infiniteBody = ref<Nullable<ScrollActionType>>(null);

  const [registerForm, { openModal: openFormModal }] = useModal();
  // const [registerMessageDrawer, { openDrawer: openMessageDrawer }] = useDrawer();

  const state = reactive<State>({
    finish: false,
    loading: false,
    listQuery: {
      keyword: '',
      currentPage: 1,
      pageSize: 20,
      sort: 'desc',
      type: '',
      isRead: 0,
    },
    list: [],
    type: '全部',
    isNoRead: true,
  });
  const { list, listQuery, type, isNoRead } = toRefs(state);

  const { onWebSocket } = useWebSocket();

  onWebSocket((data: any) => {
    // 消息推送（消息公告用的）
    if (data.method == 'messagePush') {
      initData();
    }
  });

  function init() {
    state.listQuery.isRead = 0;
    state.listQuery.keyword = '';
    state.listQuery.type = '';
    state.isNoRead = true;
    initData();
    nextTick(() => {
      bindScroll();
    });
  }
  function initData() {
    changeLoading(true);
    state.finish = false;
    state.listQuery.currentPage = 1;
    state.list = [];
    getMsgList();
  }
  function bindScroll() {
    const bodyRef = infiniteBody.value;
    const vBody = bodyRef?.getScrollWrap();
    vBody?.addEventListener('scroll', () => {
      if (vBody.scrollHeight - vBody.clientHeight - vBody.scrollTop <= 200 && !state.loading && !state.finish) {
        state.listQuery.currentPage += 1;
        getMsgList();
      }
    });
  }
  function getMsgList() {
    state.loading = true;
    getMessageList(state.listQuery).then(res => {
      if (res.data.list.length < state.listQuery.pageSize) state.finish = true;
      state.list = [...state.list, ...res.data.list];
      state.loading = false;
      changeLoading(false);
    });
  }
  function readInfo(item) {
    readMsgInfo(item.id).then(res => {
      if (item.isRead == '0') {
        item.isRead = '1';
        emit('readMsg');
      }
      if (item.type == 4) {
        let bodyText = res.data.bodyText ? JSON.parse(res.data.bodyText) : {};
        if (bodyText.type == 3) return;
        openScheduleDetailModal(true, { id: bodyText.id, groupId: bodyText.groupId });
      } else if (item.type == 2 && item.flowType == 2) {
        const bodyText = JSON.parse(res.data.bodyText);
        closeDrawer();
        router.push('/workFlow/entrust?config=' + bodyText.type);
      } else {
        if (item.type == 1 || item.type == 3) {
          openDetailModal(true, { id: item.id, type: 1 });
        } else {
          if (!res.data.bodyText) return;
          closeDrawer();
          router.push('/workFlowDetail?config=' + encodeURIComponent(encryptByBase64(res.data.bodyText)));
        }
      }
    });
  }
  function handleTypeClick({ key }) {
    state.type = key;
    if (key == '全部') state.listQuery.type = '';
    if (key == '公告') state.listQuery.type = 1;
    if (key == '流程') state.listQuery.type = 2;
    if (key == '系统') state.listQuery.type = 3;
    if (key == '日程') state.listQuery.type = 4;
    initData();
  }
  function onIsReadChange(val) {
    state.listQuery.isRead = val ? 0 : '';
    initData();
  }
  function handleSearch() {
    initData();
  }
  async function handleClose() {
    const bodyRef = infiniteBody.value;
    const vBody = bodyRef?.getScrollWrap();
    vBody?.removeEventListener('scroll', function () {});
    return true;
  }
  function gotoCenter() {
    closeDrawer();
    openFormModal(true, { id: '', industryTypeList: [] });
  }
</script>
<style lang="less">
  .message-drawer {
    .ant-drawer-title {
      display: flex;
      align-items: center;
      justify-content: space-between;
      .link-text {
        font-size: 14px;
      }
    }
    .msg-list {
      height: calc(100% - 114px);
    }
    .msg-list-item {
      position: relative;
      display: block;
      padding: 0 10px;
      background-color: @component-background;
      border-bottom: 1px solid @border-color-base1;
      height: 60px;
      display: flex;
      align-items: center;
      cursor: pointer;
      &:hover {
        background-color: @hover-background;
      }
      .item-icon {
        background-color: #1890ff;
        width: 36px;
        height: 36px;
        display: inline-block;
        font-size: 22px;
        color: #fff;
        line-height: 36px;
        border-radius: 50%;
        text-align: center;
        &.flow-icon {
          background-color: #33cc51;
        }
        &.notice-icon {
          background-color: #e09f0c;
        }
        &.schedule-icon {
          background-color: #7777ff;
        }
      }
      .msg-list-txt {
        margin-left: 14px;
        overflow: hidden;
        flex: 1;
        padding-top: 1px;
        min-width: 0;
        .title {
          font-size: 14px;
          margin-bottom: 5px;
          line-height: 20px;
          overflow: auto;
          display: flex;
          align-items: center;
          justify-content: space-between;
          height: 20px;
          overflow: hidden;
          .title-left {
            flex: 1;
            min-width: 0;
            white-space: nowrap;
            text-overflow: ellipsis;
            overflow: hidden;
          }
        }
        .name {
          font-size: 12px;
          color: @text-color-secondary;
          height: 18px;
          display: flex;
          align-items: center;
          justify-content: space-between;
          .content {
            display: inline-block;
            overflow: hidden;
            text-overflow: ellipsis;
            white-space: nowrap;
            flex: 1;
            min-width: 0;
          }
          .time {
            flex-shrink: 0;
            margin-left: 5px;
          }
        }
      }
    }
    .content1 {
      padding: 10px 15px 0 15px;
      box-sizing: border-box;
      .item {
        cursor: pointer;
      }
    }
  }
</style>
