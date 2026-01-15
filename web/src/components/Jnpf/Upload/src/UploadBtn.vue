<template>
  <a-upload
    :class="$attrs.class"
    v-model:file-list="fileList"
    :action="getAction"
    :headers="getHeaders"
    :data="data"
    :accept="accept"
    :before-upload="beforeUpload"
    @change="handleChange"
    :showUploadList="false">
    <a-button ref="uploadBtnRef" :type="buttonType" :loading="loading" :pre-icon="showIcon ? 'icon-ym icon-ym-btn-upload' : ''">
      {{ buttonText || t('common.importText') }}
    </a-button>
  </a-upload>
</template>

<script lang="ts" setup>
  import { Upload as AUpload } from 'ant-design-vue';
  import type { UploadChangeParam } from 'ant-design-vue';
  import { computed, ref, unref } from 'vue';
  import { useGlobSetting } from '/@/hooks/setting';
  import { getToken } from '/@/utils/auth';
  import { useMessage } from '/@/hooks/web/useMessage';
  import { useI18n } from '/@/hooks/web/useI18n';

  defineOptions({ name: 'JnpfUploadBtn', inheritAttrs: false });
  const props = defineProps({
    url: { type: String, default: '' },
    buttonText: { type: String, default: '' },
    buttonType: { type: String, default: 'link' },
    data: { type: Object, default: () => {} },
    showIcon: { type: Boolean, default: true },
    accept: { type: String, default: '*' },
  });
  const emit = defineEmits(['on-success', 'on-error', 'before-upload']);
  const { createMessage } = useMessage();
  const { t } = useI18n();
  const globSetting = useGlobSetting();
  const fileList = ref([]);
  const uploadBtnRef = ref<ComponentRef>(null);
  const loading = ref(false);

  const getAction = computed(() => globSetting.apiUrl + props.url);
  const getHeaders = computed(() => ({ Authorization: getToken() as string }));

  defineExpose({ handlerBtnClick });

  function beforeUpload() {
    loading.value = true;
    emit('before-upload');
  }

  function handleChange({ file }: UploadChangeParam) {
    if (file.status === 'uploading') return (loading.value = true);
    if (file.status === 'error') {
      loading.value = false;
      createMessage.error('上传失败');
      emit('on-error');
      return;
    }
    if (file.status === 'done') {
      loading.value = false;
      if (file.response.code === 200) {
        createMessage.success(file.response.msg);
        emit('on-success');
      } else {
        createMessage.error(file.response.msg);
        emit('on-error');
      }
    }
  }
  function handlerBtnClick() {
    const uploadBtn = unref(uploadBtnRef);
    if (!uploadBtn) return;
    const el = uploadBtn.$el as any;
    el.click();
  }
</script>
