<template>
  <div class="page-content-wrapper bg-white">
    <iframe :src="state.url" width="100%" height="100%" frameborder="0" />
  </div>
</template>
<script lang="ts" setup>
  import { reactive, onMounted } from 'vue';
  import { useGlobSetting } from '/@/hooks/setting';
  import { getToken } from '/@/utils/auth';
  import { useRoute } from 'vue-router';

  interface State {
    url: string;
  }

  defineOptions({ name: 'dynamicDataReport' });
  defineEmits(['register']);
  const { report } = useGlobSetting();
  const state = reactive<State>({
    url: '',
  });

  function init() {
    const route = useRoute();
    const id = route.meta.relationId;
    if (!id) return;
    state.url = `${report}/preview.html?id=${id}&token=${getToken()}&page=1&from=menu`;
  }

  onMounted(() => {
    init();
  });
</script>
