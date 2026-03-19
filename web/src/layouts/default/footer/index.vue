<template>
  <Footer :class="prefixCls" v-if="getShowLayoutFooter" ref="footerRef">
    <div :class="`${prefixCls}__links`">
      <a @click="openWindow(SITE_URL)">{{ t('layout.footer.onlinePreview') }}</a>

      <a @click="openWindow(DOC_URL)">{{ t('layout.footer.onlineDocument') }}</a>
    </div>
    <div>
      Copyright ©2020 Vben Admin 
      <span :class="`${prefixCls}__version`" @click="showVersionModal">
        v{{ version }}
      </span>
    </div>

    <!-- 版本信息弹窗 -->
    <a-modal
      v-model:open="versionModalVisible"
      :title="t('layout.footer.versionInfo')"
      :footer="null"
      width="500px"
    >
      <div v-if="versionInfo">
        <a-alert
          v-if="!versionInfo.isCompatible"
          :message="t('layout.footer.versionMismatch')"
          :description="versionInfo.message"
          type="warning"
          show-icon
          :style="{ marginBottom: '16px' }"
        />
        <a-descriptions :column="2" bordered size="small">
          <a-descriptions-item :label="t('layout.footer.webVersion')">
            {{ versionInfo.webVersion }}
          </a-descriptions-item>
          <a-descriptions-item :label="t('layout.footer.apiVersion')">
            {{ versionInfo.apiVersion }}
          </a-descriptions-item>
        </a-descriptions>

        <div :class="`${prefixCls}__changelog`">
          <h4>{{ t('layout.footer.changelog') }}</h4>
          <a-timeline>
            <a-timeline-item
              v-for="item in changelog"
              :key="item.version"
              :color="item.version === versionInfo.webVersion ? 'green' : 'blue'"
            >
              <p>
                <strong>v{{ item.version }}</strong> - {{ item.date }}
              </p>
              <ul v-if="item.added?.length">
                <li v-for="(content, idx) in item.added" :key="idx">{{ content }}</li>
              </ul>
              <ul v-if="item.fixed?.length" class="fixed">
                <li v-for="(content, idx) in item.fixed" :key="idx">{{ content }}</li>
              </ul>
            </a-timeline-item>
          </a-timeline>
        </div>
      </div>
      <a-spin v-else />
    </a-modal>
  </Footer>
</template>

<script lang="ts">
  import { computed, defineComponent, unref, ref, onMounted } from 'vue';
  import { Layout, Modal } from 'ant-design-vue';

  import { GithubFilled } from '@ant-design/icons-vue';

  import { DOC_URL, SITE_URL } from '/@/settings/siteSetting';
  import { openWindow } from '/@/utils';

  import { useI18n } from '/@/hooks/web/useI18n';
  import { useRootSetting } from '/@/hooks/setting/useRootSetting';
  import { useRouter } from 'vue-router';
  import { useDesign } from '/@/hooks/web/useDesign';
  import { useLayoutHeight } from '../content/useContentViewHeight';
  import { getVersion, getChangelog, type VersionOutput, type ChangelogItem } from '/@/api/system/version';

  // Web 版本号 - 需要与 package.json 保持一致
  const WEB_VERSION = '1.1.0';

  export default defineComponent({
    name: 'LayoutFooter',
    components: { Footer: Layout.Footer, GithubFilled },
    setup() {
      const { t } = useI18n();
      const { getShowFooter } = useRootSetting();
      const { currentRoute } = useRouter();
      const { prefixCls } = useDesign('layout-footer');

      const footerRef = ref<ComponentRef>(null);
      const { setFooterHeight } = useLayoutHeight();

      const version = ref(WEB_VERSION);
      const versionModalVisible = ref(false);
      const versionInfo = ref<VersionOutput | null>(null);
      const changelog = ref<ChangelogItem[]>([]);

      const getShowLayoutFooter = computed(() => {
        if (unref(getShowFooter)) {
          const footerEl = unref(footerRef)?.$el;
          setFooterHeight(footerEl?.offsetHeight || 0);
        } else {
          setFooterHeight(0);
        }
        return unref(getShowFooter) && !unref(currentRoute).meta?.hiddenFooter;
      });

      // 显示版本弹窗
      const showVersionModal = async () => {
        versionModalVisible.value = true;
        try {
          // 并行请求版本信息和更新日志
          const [versionData, changelogData] = await Promise.all([
            getVersion(WEB_VERSION),
            getChangelog(),
          ]);
          versionInfo.value = versionData;
          changelog.value = changelogData;
        } catch (e) {
          console.error('Failed to load version info:', e);
        }
      };

      return {
        getShowLayoutFooter,
        prefixCls,
        t,
        DOC_URL,
        SITE_URL,
        openWindow,
        footerRef,
        version,
        versionModalVisible,
        versionInfo,
        changelog,
        showVersionModal,
      };
    },
  });
</script>
<style lang="less" scoped>
  @prefix-cls: ~'@{namespace}-layout-footer';

  @normal-color: rgba(0, 0, 0, 0.45);

  @hover-color: rgba(0, 0, 0, 0.85);

  .@{prefix-cls} {
    color: @normal-color;
    text-align: center;

    &__links {
      margin-bottom: 8px;

      a {
        color: @normal-color;

        &:hover {
          color: @hover-color;
        }
      }
    }

    &__github {
      margin: 0 30px;

      &:hover {
        color: @hover-color;
      }
    }

    &__version {
      cursor: pointer;
      margin-left: 8px;

      &:hover {
        color: @hover-color;
      }
    }

    &__changelog {
      margin-top: 16px;
      max-height: 400px;
      overflow-y: auto;

      h4 {
        margin-bottom: 12px;
      }

      ul {
        margin: 0;
        padding-left: 20px;

        &.fixed {
          color: #52c41a;
        }
      }
    }
  }
</style>
