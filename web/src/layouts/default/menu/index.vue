<script lang="tsx">
  import type { PropType, CSSProperties } from 'vue';

  import { computed, defineComponent, unref, toRef, ref } from 'vue';
  import { BasicMenu } from '/@/components/Menu';
  import { SimpleMenu } from '/@/components/SimpleMenu';
  import { AppLogo } from '/@/components/Application';

  import { MenuModeEnum, MenuSplitTyeEnum } from '/@/enums/menuEnum';

  import { useMenuSetting } from '/@/hooks/setting/useMenuSetting';
  import { ScrollContainer } from '/@/components/Container';

  import { useGo } from '/@/hooks/web/usePage';
  import { useSplitMenu } from './useLayoutMenu';
  import { openWindow } from '/@/utils';
  import { propTypes } from '/@/utils/propTypes';
  import { isUrl } from '/@/utils/is';
  import { useRootSetting } from '/@/hooks/setting/useRootSetting';
  import { useAppInject } from '/@/hooks/web/useAppInject';
  import { useDesign } from '/@/hooks/web/useDesign';

  export default defineComponent({
    name: 'LayoutMenu',
    props: {
      theme: propTypes.oneOf(['light', 'dark']),

      splitType: {
        type: Number as PropType<MenuSplitTyeEnum>,
        default: MenuSplitTyeEnum.NONE,
      },

      isHorizontal: propTypes.bool,
      // menu Mode
      menuMode: {
        type: [String] as PropType<Nullable<MenuModeEnum>>,
        default: '',
      },
    },
    setup(props) {
      const go = useGo();

      const { getMenuMode, getMenuType, getMenuTheme, getCollapsed, getCollapsedShowTitle, getAccordion, getIsHorizontal, getIsSidebarType, getSplit } =
        useMenuSetting();
      const { getShowLogo } = useRootSetting();

      const { prefixCls } = useDesign('layout-menu');

      const { menusRef } = useSplitMenu(toRef(props, 'splitType'));

      const { getIsMobile } = useAppInject();

      const getComputedMenuMode = computed(() => (unref(getIsMobile) ? MenuModeEnum.INLINE : props.menuMode || unref(getMenuMode)));

      const getComputedMenuTheme = computed(() => props.theme || unref(getMenuTheme));

      const getIsShowLogo = computed(() => unref(getShowLogo) && unref(getIsSidebarType));

      const getUseScroll = computed(() => {
        return !unref(getIsHorizontal) && (unref(getIsSidebarType) || props.splitType === MenuSplitTyeEnum.LEFT || props.splitType === MenuSplitTyeEnum.NONE);
      });

      const getWrapperStyle = computed((): CSSProperties => {
        return {
          height: `calc(100% - ${unref(getIsShowLogo) ? '48px' : '0px'} - 44px)`,
        };
      });

      const showAppPanel = ref(false);

      const getLogoClass = computed(() => {
        return [
          `${prefixCls}-logo`,
          unref(getComputedMenuTheme),
          {
            [`${prefixCls}--mobile`]: unref(getIsMobile),
          },
        ];
      });

      const getCommonProps = computed(() => {
        const menus = unref(menusRef);
        return {
          menus,
          beforeClickFn: beforeMenuClickFn,
          items: menus,
          theme: unref(getComputedMenuTheme),
          accordion: unref(getAccordion),
          collapse: unref(getCollapsed),
          collapsedShowTitle: unref(getCollapsedShowTitle),
          onMenuClick: handleMenuClick,
        };
      });
      /**
       * click menu
       * @param menu
       */

      function handleMenuClick(path: string) {
        go(path);
      }

      /**
       * before click menu
       * @param menu
       */
      async function beforeMenuClickFn(path: string) {
        if (!isUrl(path)) {
          return true;
        }
        openWindow(path);
        return false;
      }

      function renderHeader() {
        if (!unref(getIsShowLogo) && !unref(getIsMobile)) return null;

        return <AppLogo showTitle={!unref(getCollapsed)} class={unref(getLogoClass)} theme={unref(getComputedMenuTheme)} />;
      }

      function renderMenu() {
        const { menus, ...menuProps } = unref(getCommonProps);
        if (!menus || !menus.length) return null;
        return !props.isHorizontal ? (
          <SimpleMenu {...menuProps} isSplitMenu={unref(getSplit)} items={menus} />
        ) : (
          <BasicMenu
            {...(menuProps as any)}
            isHorizontal={props.isHorizontal}
            type={unref(getMenuType)}
            showLogo={unref(getIsShowLogo)}
            mode={unref(getComputedMenuMode as any)}
            items={menus}
          />
        );
      }

      function renderFooter() {
        if (props.isHorizontal || unref(getCollapsed)) return null;
        const theme = unref(getComputedMenuTheme);
        return (
          <div class={`${prefixCls}-footer ${theme}`}>
            {showAppPanel.value && (
              <div class={`${prefixCls}-footer-panel`}>
                <div class={`${prefixCls}-footer-panel-inner`}>
                  <div class={`${prefixCls}-footer-tabs`}>
                    <div class={`${prefixCls}-footer-tab active`}>手机应用</div>
                  </div>
                  <div class={`${prefixCls}-footer-panel-body`}>
                    <img src="/resource/img/lmapp.png" alt="移动端下载" class={`${prefixCls}-footer-qrcode`} />
                    <div class={`${prefixCls}-footer-text`}>扫码下载</div>
                  </div>
                </div>
              </div>
            )}
            <div class={`${prefixCls}-footer-trigger`} onClick={() => { showAppPanel.value = !showAppPanel.value; }}>
              <span class={`${prefixCls}-footer-trigger-icon`}>{showAppPanel.value ? '\u2715' : '\u2193'}</span>
              <span class={`${prefixCls}-footer-trigger-text`}>获取应用程序</span>
            </div>
          </div>
        );
      }

      return () => {
        return (
          <>
            {renderHeader()}
            {unref(getUseScroll) ? <ScrollContainer style={unref(getWrapperStyle)}>{() => renderMenu()}</ScrollContainer> : renderMenu()}
            {renderFooter()}
          </>
        );
      };
    },
  });
</script>
<style lang="less">
  @prefix-cls: ~'@{namespace}-layout-menu';
  @logo-prefix-cls: ~'@{namespace}-app-logo';

  .@{prefix-cls} {
    &-logo {
      height: @header-height;
    }

    &--mobile {
      .@{logo-prefix-cls} {
        &__title {
          opacity: 100%;
        }
      }
    }

    &-footer {
      position: relative;
      display: flex;
      flex-direction: column;
      justify-content: flex-end;
      border-top: 1px solid transparent;
      transition: all 0.3s;
      overflow: visible;

      &.dark {
        border-top-color: rgba(255, 255, 255, 0.06);
      }

      &.light {
        border-top-color: rgba(0, 0, 0, 0.04);
      }

      &-panel {
        position: absolute;
        bottom: 44px;
        left: 10px;
        right: 10px;
        z-index: 10;
        padding-bottom: 8px;

        &-inner {
          background: #fff;
          border-radius: 10px;
          box-shadow: 0 -2px 12px rgba(0, 0, 0, 0.08);
          padding: 12px;
        }

        &-body {
          display: flex;
          flex-direction: column;
          align-items: center;
        }
      }

      &-tabs {
        display: flex;
        justify-content: center;
        margin-bottom: 10px;
      }

      &-tab {
        padding: 4px 12px;
        font-size: 13px;
        color: #666;
        border-radius: 6px;
        cursor: default;

        &.active {
          background: #f0f0f0;
          color: #333;
          font-weight: 500;
        }
      }

      &-qrcode {
        width: 100px;
        height: 100px;
        border-radius: 6px;
        object-fit: contain;
        background: #fff;
        padding: 4px;
        border: 1px solid #eee;
        box-sizing: border-box;
      }

      &-text {
        margin-top: 8px;
        font-size: 12px;
        color: #888;
        text-align: center;
        white-space: nowrap;
      }

      &-trigger {
        height: 38px;
        display: flex;
        align-items: center;
        justify-content: center;
        cursor: pointer;
        gap: 6px;
        padding-bottom: 4px;
        transition: all 0.2s;

        &:hover {
          opacity: 0.8;
        }

        &-icon {
          font-size: 13px;
          line-height: 1;
        }

        &-text {
          font-size: 13px;
        }
      }
    }

    &-footer.dark &-footer-trigger {
      color: rgba(255, 255, 255, 0.85);
    }

    &-footer.light &-footer-trigger {
      color: rgba(0, 0, 0, 0.65);
    }
  }
</style>
