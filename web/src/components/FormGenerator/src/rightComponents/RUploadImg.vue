<template>
  <a-form-item label="上传提示">
    <jnpf-textarea v-model:value="activeData.tipText" placeholder="请输入上传提示" :rows="2" />
  </a-form-item>
  <a-form-item label="文件大小">
    <a-input-number v-model:value="activeData.fileSize" placeholder="文件大小" :min="1" :precision="0">
      <template #addonAfter>
        <a-select v-model:value="activeData.sizeUnit" style="width: 66px">
          <a-select-option value="KB">KB</a-select-option>
          <a-select-option value="MB">MB</a-select-option>
        </a-select>
      </template>
    </a-input-number>
  </a-form-item>
  <a-form-item label="最大上传数">
    <a-input-number v-model:value="activeData.limit" placeholder="最大上传数" :min="1" :precision="0" />
  </a-form-item>
  <a-form-item label="上传路径">
    <jnpf-radio v-model:value="activeData.pathType" :options="pathTypeOptions" optionType="button" button-style="solid" class="right-radio" />
  </a-form-item>
  <template v-if="activeData.pathType === 'selfPath'">
    <a-form-item label="分用户存储">
      <jnpf-switch v-model:value="activeData.isAccount" />
    </a-form-item>
    <a-form-item label="文件夹名">
      <a-input v-model:value="activeData.folder" placeholder="请输入文件夹名" />
    </a-form-item>
  </template>
</template>
<script lang="ts" setup>
  defineOptions({ inheritAttrs: false });
  defineProps(['activeData']);

  const pathTypeOptions = [
    { id: 'defaultPath', fullName: '默认路径' },
    { id: 'selfPath', fullName: '自定义路径' },
  ];
</script>
