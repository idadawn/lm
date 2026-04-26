<template>
  <div class="nlq-assistant-entry">
    <a-button type="primary" class="assistant-launcher" @click="openDock">
      智能问数
    </a-button>

    <a-drawer
      v-model:visible="dockVisible"
      title="实验室智能问数"
      placement="right"
      :width="420"
      :mask-closable="true"
      :destroy-on-close="false"
      :body-style="{ padding: '0', overflow: 'hidden' }"
      @close="dockVisible = false"
    >
      <template #extra>
        <a-button type="link" @click="openFullscreen">全屏</a-button>
      </template>
      <iframe
        ref="dockFrameRef"
        class="assistant-iframe"
        :src="dockIframeSrc"
        frameborder="0"
        @load="postAuthToFrame('dock')"
      />
    </a-drawer>

    <a-drawer
      v-model:visible="fullscreenVisible"
      title="实验室智能问数"
      placement="right"
      :width="'100%'"
      wrap-class-name="full-drawer"
      :mask-closable="true"
      :destroy-on-close="false"
      :body-style="{ padding: '0', overflow: 'hidden' }"
      @close="fullscreenVisible = false"
    >
      <iframe
        ref="fullscreenFrameRef"
        class="assistant-iframe"
        :src="fullscreenIframeSrc"
        frameborder="0"
        @load="postAuthToFrame('fullscreen')"
      />
    </a-drawer>
  </div>
</template>

<script setup lang="ts">
  import { computed, nextTick, ref } from 'vue';
  import { getToken, getAuthCache } from '/@/utils/auth';
  import { PERMISSIONS_KEY, USER_INFO_KEY } from '/@/enums/cacheEnum';

  interface UserInfo {
    userId?: string;
    userAccount?: string;
    organizeId?: string;
  }

  interface PermissionInfo {
    modelId?: string;
    moduleName?: string;
  }

  type FrameMode = 'dock' | 'fullscreen';

  const dockVisible = ref(false);
  const fullscreenVisible = ref(false);
  const dockFrameRef = ref<HTMLIFrameElement | null>(null);
  const fullscreenFrameRef = ref<HTMLIFrameElement | null>(null);
  const assistantBaseUrl = 'http://127.0.0.1:13000';

  const dockIframeSrc = computed(() => `${assistantBaseUrl}/?embed=1&mode=dock`);
  const fullscreenIframeSrc = computed(() => `${assistantBaseUrl}/?embed=1&mode=fullscreen`);

  function buildPermissions(): string[] {
    const permissionList = (getAuthCache(PERMISSIONS_KEY) || []) as PermissionInfo[];
    return permissionList.flatMap(item => [item.moduleName, item.modelId]).filter(Boolean) as string[];
  }

  function buildAuthPayload() {
    const token = getToken();
    const userInfo = (getAuthCache(USER_INFO_KEY) || {}) as UserInfo;

    return {
      access_token: token,
      token_type: 'Bearer',
      user_id: userInfo.userId,
      account: userInfo.userAccount,
      tenant_id: userInfo.organizeId,
      origin: 'embedded',
      permissions: buildPermissions(),
    };
  }

  function getFrameRef(mode: FrameMode) {
    return mode === 'dock' ? dockFrameRef.value : fullscreenFrameRef.value;
  }

  function postAuthToFrame(mode: FrameMode) {
    const frame = getFrameRef(mode);
    const frameWindow = frame?.contentWindow;
    if (!frameWindow) return;

    frameWindow.postMessage(
      {
        type: 'NLQ_AUTH_CONTEXT',
        payload: buildAuthPayload(),
      },
      assistantBaseUrl,
    );
  }

  async function openDock() {
    dockVisible.value = true;
    await nextTick();
    setTimeout(() => postAuthToFrame('dock'), 300);
  }

  async function openFullscreen() {
    dockVisible.value = false;
    fullscreenVisible.value = true;
    await nextTick();
    setTimeout(() => postAuthToFrame('fullscreen'), 300);
  }
</script>

<style lang="less" scoped>
  .nlq-assistant-entry {
    position: fixed;
    right: 24px;
    bottom: 24px;
    z-index: 1100;
  }

  .assistant-launcher {
    height: 48px;
    padding: 0 18px;
    border-radius: 999px;
    box-shadow: 0 14px 34px rgba(24, 144, 255, 0.28);
    font-weight: 600;
  }

  .assistant-iframe {
    width: 100%;
    height: calc(100vh - 110px);
    border: 0;
    display: block;
    background: #fff;
  }
</style>
