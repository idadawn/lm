<template>
  <Header :class="getHeaderClass">
    <!-- left start -->
    <div :class="`${prefixCls}-left`">
      <!-- logo -->
      <AppLogo
        v-if="getShowHeaderLogo || getIsMobile"
        :class="`${prefixCls}-logo`"
        :theme="getHeaderTheme"
        :style="getLogoWidth" />
      <LayoutTrigger
        v-if="(getShowContent && getShowHeaderTrigger && !getSplit && !getIsMixSidebar) || getIsMobile"
        :theme="getHeaderTheme"
        :sider="false" />
      <!-- <LayoutBreadcrumb v-if="getShowContent && getShowBread" :theme="getHeaderTheme" /> -->
    </div>
    <!-- left end -->

    <!-- menu start -->
    <div :class="`${prefixCls}-menu`" v-if="getShowTopMenu && !getIsMobile">
      <LayoutMenu :isHorizontal="true" :theme="getHeaderTheme" :splitType="getSplitType" :menuMode="getMenuMode" />
    </div>
    <!-- menu-end -->

    <!-- action  -->
    <div :class="`${prefixCls}-action`">
      <AppSearch :class="`${prefixCls}-action__item search-item`" v-if="getShowSearch" />

      <ErrorAction v-if="getUseErrorHandle" :class="`${prefixCls}-action__item error-action`" />

      <FullScreen v-if="getShowFullScreen" :class="`${prefixCls}-action__item fullscreen-item`" />

      <AppLocalePicker v-if="getShowLocalePicker" :showText="false" :class="`${prefixCls}-action__item`" />

      <UserDropDown :theme="getHeaderTheme" />

      <ResetPwdForm @register="registerForm" />
    </div>
  </Header>
</template>
<script lang="ts">
  import { defineComponent, unref, computed, reactive, toRefs, onMounted } from 'vue';

  import { propTypes } from '/@/utils/propTypes';

  import { Layout } from 'ant-design-vue';

  import { AppLogo, AppSearch, AppLocalePicker } from '/@/components/Application';
  import LayoutMenu from '../menu/index.vue';
  import LayoutTrigger from '../trigger/index.vue';

  import { useHeaderSetting } from '/@/hooks/setting/useHeaderSetting';
  import { useMenuSetting } from '/@/hooks/setting/useMenuSetting';
  import { useRootSetting } from '/@/hooks/setting/useRootSetting';

  import { MenuModeEnum, MenuSplitTyeEnum } from '/@/enums/menuEnum';
  import { SettingButtonPositionEnum } from '/@/enums/appEnum';

  import { UserDropDown, LayoutBreadcrumb, FullScreen, ErrorAction } from './components';
  import { useAppInject } from '/@/hooks/web/useAppInject';
  import { useDesign } from '/@/hooks/web/useDesign';

  import { createAsyncComponent } from '/@/utils/factory/createAsyncComponent';
  import { useLocale } from '/@/locales/useLocale';
  import { useI18n } from '/@/hooks/web/useI18n';

  import { useModal } from '/@/components/Modal';
  import ResetPwdForm from './components/ResetPwdForm.vue';
  import { getSysConfig } from '/@/api/system/sysConfig';
  import { updatePasswordMessage } from '/@/api/basic/user';
  import { useUserStore } from '/@/store/modules/user';



  export default defineComponent({
    name: 'LayoutHeader',
    components: {
      Header: Layout.Header,
      AppLogo,
      LayoutTrigger,
      LayoutBreadcrumb,
      LayoutMenu,
      UserDropDown,
      AppLocalePicker,
      FullScreen,
      AppSearch,
      ErrorAction,
      ResetPwdForm,
    },
    props: {
      fixed: propTypes.bool,
    },
    setup(props) {
      const { t } = useI18n();
      const { prefixCls } = useDesign('layout-header');
      const { getShowTopMenu, getShowHeaderTrigger, getSplit, getIsMixMode, getMenuWidth, getIsMixSidebar } =
        useMenuSetting();
      const { getUseErrorHandle, getShowSettingButton, getSettingButtonPosition } = useRootSetting();

      const {
        getHeaderTheme,
        getShowFullScreen,
        getShowNotice,
        getShowContent,
        getShowBread,
        getShowHeaderLogo,
        getShowHeader,
        getShowSearch,
      } = useHeaderSetting();

      const { getShowLocalePicker } = useLocale();

      const { getIsMobile } = useAppInject();

      const userStore = useUserStore();
      const [registerForm, { openModal: openFormModal }] = useModal();

      const getHeaderClass = computed(() => {
        const theme = unref(getHeaderTheme);
        return [
          prefixCls,
          {
            [`${prefixCls}--fixed`]: props.fixed,
            [`${prefixCls}--mobile`]: unref(getIsMobile),
            [`${prefixCls}--${theme}`]: theme,
          },
        ];
      });

      const getLogoWidth = computed(() => {
        if (!unref(getIsMixMode) || unref(getIsMobile)) {
          return {};
        }
        const width = unref(getMenuWidth) < 180 ? 180 : unref(getMenuWidth);
        return { width: `${width}px` };
      });

      const getSplitType = computed(() => {
        return unref(getSplit) ? MenuSplitTyeEnum.TOP : MenuSplitTyeEnum.NONE;
      });

      const getMenuMode = computed(() => {
        return unref(getSplit) ? MenuModeEnum.HORIZONTAL : null;
      });
      const getUserInfo = computed(() => userStore.getUserInfo || {});

      function initData() {
        updatePasswordMessage();
        getSysConfig()
          .then(res => {
            if (
              !(unref(getUserInfo) as any)?.changePasswordDate &&
              res.data.mandatoryModificationOfInitialPassword == 1
            )
              openFormModal(true, {
                id: (unref(getUserInfo) as any)?.userId,
                account: (unref(getUserInfo) as any)?.userAccount,
              });
          })
          .catch(() => {});
      }

      onMounted(() => {
        initData();
      });

      return {
        t,
        prefixCls,
        getHeaderClass,
        getShowHeaderLogo,
        getHeaderTheme,
        getShowHeaderTrigger,
        getIsMobile,
        getShowBread,
        getShowContent,
        getSplitType,
        getSplit,
        getMenuMode,
        getShowTopMenu,
        getShowLocalePicker,
        getShowFullScreen,
        getUseErrorHandle,
        getLogoWidth,
        getIsMixSidebar,
        getShowSearch,
        registerForm,
        initData,
      };
    },
  });
</script>
<style lang="less">
  @import url('./index.less');
</style>
