<template>
  <jnpf-group-title content="第三方服务绑定" class="mb-20px" />
  <div class="socials-list">
    <div class="socials-item" v-for="(item, i) in list" :key="i">
      <div class="socials-item-main">
        <img :src="item.logo" class="item-img" />
        <div class="item-txt">
          <p class="item-name">{{ item.name }}</p>
          <p class="item-desc">{{ item.describetion }}</p>
        </div>
        <div class="item-btn">
          <a-button v-if="item.entity" @click="handleDel(item.entity.userId, item.entity.id)">解绑</a-button>
          <a-button v-if="!item.entity" type="primary" @click="handleBind(item.enname)">绑定</a-button>
        </div>
      </div>
    </div>
  </div>
</template>
<script lang="ts" setup>
  import { getSocialsUserList, deleteSocials, socialsBind } from '/@/api/permission/socialsUser';
  import { reactive, toRefs, onMounted, onUnmounted } from 'vue';
  import { useMessage } from '/@/hooks/web/useMessage';

  interface State {
    list: any[];
    winUrl: any;
    listenerLoad: boolean;
  }

  const { createMessage, createConfirm } = useMessage();
  const state = reactive<State>({
    list: [],
    winUrl: '',
    listenerLoad: false,
  });
  const { list } = toRefs(state);
  const messageKey = 'callback';

  function initData() {
    state.list = [];
    getSocialsUserList().then(res => {
      state.list = res.data;
    });
  }
  function handleDel(userId, id) {
    createConfirm({
      iconType: 'warning',
      title: '提示',
      content: '确定要解除该账号绑定?',
      onOk: () => {
        deleteSocials(userId, id)
          .then(res => {
            createMessage.success(res.msg).then(() => {
              initData();
            });
          })
          .catch(() => {
            initData();
          });
      },
    });
  }
  function handleBind(name) {
    bindListener();
    socialsBind(name).then(res => {
      if (state.winUrl && !state.winUrl.closed) {
        state.winUrl.location.replace(res.msg);
        state.winUrl.focus();
        return;
      }
      const iWidth = 750;
      const iHeight = 500;
      const iLeft = (window.screen.width - iWidth) / 2;
      const iTop = (window.screen.height - iHeight) / 2;
      state.winUrl = window.open(
        res.msg,
        '_blank',
        `height=${iHeight},innerHeight=${iHeight},width=${iWidth},innerWidth=${iWidth},top=${iTop},left=${iLeft},toolbar=no,menubar=no,scrollbars=auto,resizeable=no,location=no,status=no`,
      );
    });
  }
  function bindListener() {
    if (!state.listenerLoad) {
      window.addEventListener('message', e => {
        const res = typeof e.data === 'string' ? JSON.parse(e.data) : e.data;
        if (res.code == '200') {
          createMessage.success({ content: res.message, key: messageKey }).then(() => {
            initData();
            window.removeEventListener('message', () => {});
          });
        }
        if (res.code == '201') {
          createMessage.error({ content: res.message, key: messageKey }).then(() => {
            initData();
            window.removeEventListener('message', () => {});
          });
        }
      });
    }
    state.listenerLoad = true;
  }

  onMounted(() => {
    initData();
  });
  onUnmounted(() => {
    window.removeEventListener('message', () => {});
  });
</script>
<style lang="less" scoped>
  .socials-list {
    padding: 0 40px;
    .socials-item {
      padding: 10px 0;
      border-bottom: 1px solid @border-color-base;
    }
    .socials-item-main {
      display: flex;
      align-items: center;
      padding: 10px;
      height: 100px;
      &:hover {
        background-color: @selected-hover-bg;
      }
      .item-img {
        width: 80px;
        height: 80px;
        display: block;
        margin-right: 14px;
        flex-shrink: 0;
      }
      .item-txt {
        height: 80px;
        flex: 1;
        .item-name {
          line-height: 22px;
          font-size: 16px;
          margin-bottom: 16px;
          font-weight: 600;
        }
        .item-desc {
          color: @text-color-label;
          font-size: 12px;
          line-height: 18px;
        }
      }
      .item-btn {
        width: 70px;
        text-align: right;
        flex-shrink: 0;
      }
    }
  }
</style>
