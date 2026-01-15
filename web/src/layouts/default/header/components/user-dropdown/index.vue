<template>
  <Dropdown placement="bottom" :overlayClassName="`${prefixCls}-dropdown-overlay`">
    <span :class="[prefixCls, `${prefixCls}--${theme}`]" class="flex">
      <Avatar :class="`${prefixCls}__header`" :src="apiUrl + getUserInfo.headIcon" :size="26" />
      <span :class="`${prefixCls}__info hidden md:block`">
        <span :class="`${prefixCls}__name truncate`"> {{ getUserInfo.userName }}</span>
      </span>
    </span>
    <template #overlay>
      <Menu @click="handleMenuClick">
        <a-menu-item key="profile">
          <span class="flex items-center">
            <i class="icon-ym icon-ym-header-userInfo mr-1" />
            <span>{{ t('layout.header.profile') }}</span>
          </span>
        </a-menu-item>
        <a-sub-menu key="system" v-if="getUserInfo.systemIds && getUserInfo.systemIds.length > 1" class="system-menu-item">
          <template #title>
            <span class="flex items-center">
              <i class="icon-ym icon-ym-systemToggle mr-1" />
              <span>{{ t('layout.header.systemChange') }}</span>
            </span>
          </template>
          <a-menu-item v-for="item in getUserInfo.systemIds" :key="item.id" @click.stop="toggleSystem(item.id, 'System')" :disabled="!!item.currentSystem">
            <span class="flex items-center">
              <i :class="`${item.icon} mr-1`" />
              <span>{{ item.name }}</span>
            </span>
          </a-menu-item>
        </a-sub-menu>
        <!-- <a-menu-item key="feedBack">
          <span class="flex items-center">
            <i class="icon-ym icon-ym-header-feedBack mr-1" />
            <span>{{ t('layout.header.feedback') }}</span>
          </span>
        </a-menu-item>
        <a-menu-item key="about">
          <span class="flex items-center">
            <i class="icon-ym icon-ym-header-about mr-1" />
            <span>{{ t('layout.header.about') }}</span>
          </span>
        </a-menu-item>
        <a-menu-item key="statement">
          <span class="flex items-center">
            <i class="icon-ym icon-ym-generator-card mr-1" />
            <span>{{ t('layout.header.statement') }}</span>
          </span>
        </a-menu-item>
        <a-menu-item key="lock" v-if="getUseLockPage">
          <span class="flex items-center">
            <i class="icon-ym icon-ym-header-lockScreen mr-1" />
            <span>{{ t('layout.header.tooltipLock') }}</span>
          </span>
        </a-menu-item> -->
        <MenuDivider />
        <a-menu-item key="logout">
          <span class="flex items-center">
            <i class="icon-ym icon-ym-header-loginOut mr-1" />
            <span>{{ t('layout.header.dropdownItemLoginOut') }}</span>
          </span>
        </a-menu-item>
      </Menu>
    </template>
  </Dropdown>
  <AboutModal @register="registerAboutModal" />
  <StatementModal @register="registerStatementModal" />
</template>
<script lang="ts">
  import { Dropdown, Menu, Avatar } from 'ant-design-vue';
  import type { MenuInfo } from 'ant-design-vue/lib/menu/src/interface';
  import { defineComponent, computed } from 'vue';
  import { DOC_URL } from '/@/settings/siteSetting';
  import { useGlobSetting } from '/@/hooks/setting';
  import { useUserStore } from '/@/store/modules/user';
  import { useLockStore } from '/@/store/modules/lock';
  import { useHeaderSetting } from '/@/hooks/setting/useHeaderSetting';
  import { useI18n } from '/@/hooks/web/useI18n';
  import { useMessage } from '/@/hooks/web/useMessage';
  import { useDesign } from '/@/hooks/web/useDesign';
  import { propTypes } from '/@/utils/propTypes';
  import { openWindow } from '/@/utils';
  import { useModal } from '/@/components/Modal';
  import AboutModal from '../AboutModal.vue';
  import StatementModal from '../StatementModal.vue';
  import { useGo } from '/@/hooks/web/usePage';
  import { setMajor } from '/@/api/permission/userSetting';

  export default defineComponent({
    name: 'UserDropdown',
    components: {
      Dropdown,
      Menu,
      Avatar,
      MenuDivider: Menu.Divider,
      AboutModal,
      StatementModal,
    },
    props: {
      theme: propTypes.oneOf(['dark', 'light']),
    },
    setup() {
      const globSetting = useGlobSetting();
      const apiUrl = globSetting.apiUrl;
      const { prefixCls } = useDesign('header-user-dropdown');
      const { t } = useI18n();
      const { createMessage } = useMessage();
      const go = useGo();
      const { getUseLockPage } = useHeaderSetting();
      const userStore = useUserStore();
      const lockStore = useLockStore();
      const [registerAboutModal, { openModal: openAboutModal }] = useModal();
      const [registerStatementModal, { openModal: openStatementModal }] = useModal();

      const getUserInfo = computed(() => userStore.getUserInfo || {});

      function handleMenuClick(e: MenuInfo) {
        if (e.key === 'logout') return handleLoginOut();
        if (e.key === 'doc') return openDoc();
        if (e.key === 'lock') return handleLock();
        if (e.key === 'profile') return go('/profile');
        if (e.key === 'feedBack') return openFeedBack();
        if (e.key === 'statement') return openStatementModal(true);
        if (e.key === 'about') return openAboutModal(true);
      }
      function handleLock() {
        lockStore.setLockInfo({
          isLock: true,
        });
      }
      function handleLoginOut() {
        userStore.confirmLoginOut();
      }
      function openDoc() {
        openWindow(DOC_URL);
      }
      function openFeedBack() {
        openWindow('http://mail.qq.com/cgi-bin/qm_share?t=qm_mailme&email=dA0dGhkVHQcbEgA0BQVaFxsZ');
      }
      function toggleSystem(majorId, majorType) {
        let query = { majorId, majorType };
        setMajor(query).then(res => {
          createMessage.success(res.msg).then(() => {
            location.reload();
          });
        });
      }

      return {
        apiUrl,
        prefixCls,
        t,
        getUserInfo,
        handleMenuClick,
        getUseLockPage,
        registerAboutModal,
        registerStatementModal,
        toggleSystem,
      };
    },
  });
</script>
<style lang="less">
  @prefix-cls: ~'@{namespace}-header-user-dropdown';

  .@{prefix-cls} {
    height: @header-height;
    padding: 0 0 0 10px;
    padding-right: 10px;
    overflow: hidden;
    font-size: 12px;
    cursor: pointer;
    align-items: center;

    &__header {
      margin-right: 4px;
    }

    &__name {
      font-size: 14px;
    }

    &--dark {
      &:hover {
        background-color: @header-dark-bg-hover-color;
      }
    }

    &--light {
      &:hover {
        background-color: @header-light-bg-hover-color;
      }

      .@{prefix-cls}__name {
        color: @text-color-base;
      }

      .@{prefix-cls}__desc {
        color: @header-light-desc-color;
      }
    }
  }

  .system-menu-item {
    .ant-dropdown-menu-submenu-arrow {
      display: none !important;
    }
  }
</style>
