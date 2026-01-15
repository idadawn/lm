<template>
  <BasicDrawer v-bind="$attrs" @register="registerDrawer" width="280px" class="full-drawer portal-drawer" title="切换门户">
    <div class="main">
      <div v-if="list.length">
        <div class="item" v-for="(item, i) in list" :key="i">
          <p class="item-title">{{ item.fullName }}</p>
          <div class="item-list">
            <div class="item-list-item" v-for="(child, ii) in item.children" :key="ii" @click="selectItem(child.id)" :class="{ active: activeId === child.id }">
              <p class="com-hover">{{ child.fullName }}</p>
              <CheckCircleFilled class="icon" />
            </div>
          </div>
        </div>
      </div>
      <p class="noData-txt" v-else>暂无数据</p>
    </div>
  </BasicDrawer>
</template>
<script lang="ts" setup>
  import { BasicDrawer, useDrawerInner } from '/@/components/Drawer';
  import { useMessage } from '/@/hooks/web/useMessage';
  import { getPortalSelector, setDefaultPortal } from '/@/api/onlineDev/portal';
  import { reactive, toRefs } from 'vue';
  import { CheckCircleFilled } from '@ant-design/icons-vue';
  import { useUserStore } from '/@/store/modules/user';

  interface State {
    list: any[];
    activeId: string;
  }

  const state = reactive<State>({
    list: [],
    activeId: '',
  });
  const { list, activeId } = toRefs(state);
  const userStore = useUserStore();
  const emit = defineEmits(['register', 'refresh']);
  const { createMessage } = useMessage();
  const [registerDrawer, { changeLoading, closeDrawer }] = useDrawerInner(init);

  function init(data) {
    state.activeId = data.id || '';
    initData();
  }
  function initData() {
    changeLoading(true);
    getPortalSelector(1)
      .then(res => {
        state.list = res?.data?.list || [];
        changeLoading(false);
      })
      .catch(() => {
        changeLoading(false);
      });
  }
  function selectItem(id) {
    if (state.activeId == id) return;
    changeLoading(true);
    setDefaultPortal(id)
      .then(res => {
        state.activeId = id;
        emit('refresh', id);
        changeLoading(false);
        createMessage.success(res.msg);
        closeDrawer();
        userStore.setUserInfo({ portalId: id });
      })
      .catch(() => {
        changeLoading(false);
      });
  }
</script>
<style lang="less">
  .portal-drawer {
    .main {
      padding: 10px 20px;
      height: 100%;
      overflow: auto;
      overflow-x: hidden;
      .item {
        .item-title {
          font-size: 12px;
          line-height: 30px;
          color: #999;
        }
        .item-list {
          font-size: 14px;
          color: #707070;
          .item-list-item {
            display: flex;
            justify-content: space-between;
            align-items: center;
            height: 45px;
            cursor: pointer;
            .icon {
              font-size: 18px;
              color: #bdbdbd;
            }
            &.active .icon {
              color: #1890ff;
            }
          }
        }
      }
      .noData-txt {
        font-size: 14px;
        color: #909399;
        line-height: 20px;
        text-align: center;
        padding-top: 10px;
      }
    }
  }
</style>
