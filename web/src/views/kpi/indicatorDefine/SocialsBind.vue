<template>
  <BasicPopup v-bind="$attrs" @register="registerPopup" title="第三方服务绑定管理">
    <div class="socials-list">
      <div class="socials-item" v-for="(item, i) in list" :key="i">
        <div class="socials-item-main">
          <img :src="item.logo" class="item-img" />
          <div class="item-txt">
            <p class="item-name">{{ item.name }}</p>
            <p class="item-desc">{{ item.describetion }}</p>
          </div>
          <div class="item-btn">
            <a-button v-if="item.entity" @click="handleDel(item.entity.id)">解绑</a-button>
          </div>
        </div>
      </div>
    </div>
  </BasicPopup>
</template>

<script lang="ts" setup>
  import { getSocialsUserListByUser, deleteSocials } from '/@/api/permission/socialsUser';
  import { reactive, toRefs } from 'vue';
  import { BasicPopup, usePopupInner } from '/@/components/Popup';
  import { useMessage } from '/@/hooks/web/useMessage';

  interface State {
    list: any[];
    userId: string;
  }

  const { createMessage, createConfirm } = useMessage();
  const [registerPopup, { changeLoading }] = usePopupInner(init);
  const state = reactive<State>({
    list: [],
    userId: '',
  });
  const { list } = toRefs(state);

  function init(data) {
    changeLoading(true);
    state.userId = data.id;
    initData();
  }
  function initData() {
    state.list = [];
    changeLoading(true);
    getSocialsUserListByUser(state.userId).then(res => {
      state.list = res.data;
      changeLoading(false);
    });
  }
  function handleDel(id) {
    createConfirm({
      iconType: 'warning',
      title: '提示',
      content: '确定要解除该账号绑定?',
      onOk: () => {
        deleteSocials(state.userId, id)
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
