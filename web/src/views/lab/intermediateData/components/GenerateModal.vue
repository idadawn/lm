<template>
  <a-modal
    :visible="visible"
    title="生成中间数据"
    :width="500"
    :maskClosable="false"
    @ok="handleOk"
    @cancel="handleCancel"
    @update:visible="(val) => emit('update:visible', val)"
  >
    <a-form :model="formState" :label-col="{ span: 6 }" :wrapper-col="{ span: 16 }">
      <a-form-item label="产品规格" required>
        <a-select
          v-model:value="formState.productSpecId"
          placeholder="请选择产品规格"
          @change="handleProductSpecChange"
        >
          <a-select-option v-for="spec in productSpecOptions" :key="spec.id" :value="spec.id">
            {{ spec.name }}
          </a-select-option>
        </a-select>
      </a-form-item>

      <a-form-item label="规格版本" v-if="formState.productSpecId">
        <a-select
          v-model:value="formState.productSpecVersion"
          placeholder="默认使用最新版本"
          allowClear
        >
          <a-select-option v-for="ver in versionList" :key="ver.version" :value="ver.version">
            {{ ver.versionName }} <span v-if="ver.isCurrent">(当前)</span>
          </a-select-option>
        </a-select>
      </a-form-item>

      <a-form-item label="日期范围">
        <a-range-picker
          v-model:value="formState.dateRange"
          style="width: 100%"
          format="YYYY-MM-DD"
        />
      </a-form-item>

      <a-form-item label="覆盖已有">
        <a-switch v-model:checked="formState.forceRegenerate" />
        <span style="margin-left: 8px; color: #999">开启后将覆盖已生成的数据</span>
      </a-form-item>
    </a-form>

    <template #footer>
      <a-button @click="handleCancel">取消</a-button>
      <a-button type="primary" :loading="loading" @click="handleOk">
        生成
      </a-button>
    </template>

    <!-- 结果展示 -->
    <a-result
      v-if="result"
      :status="result.failedCount > 0 ? 'warning' : 'success'"
      :title="`生成完成`"
      :sub-title="`成功: ${result.successCount}, 跳过: ${result.skippedCount}, 失败: ${result.failedCount}`"
    >
      <template #extra>
        <a-button type="primary" @click="handleClose">确定</a-button>
      </template>
      <div v-if="result.errors && result.errors.length > 0" class="error-list">
        <p style="color: #ff4d4f; margin-bottom: 8px">错误信息：</p>
        <div v-for="(error, index) in result.errors.slice(0, 5)" :key="index" class="error-item">
          {{ error }}
        </div>
        <div v-if="result.errors.length > 5" class="error-more">
          ... 还有 {{ result.errors.length - 5 }} 条错误
        </div>
      </div>
    </a-result>
  </a-modal>
</template>

<script lang="ts" setup>
  import { ref, reactive, watch } from 'vue';
  import { useMessage } from '/@/hooks/web/useMessage';
  import { generateIntermediateData } from '/@/api/lab/intermediateData';
  import { getProductSpecVersionList } from '/@/api/lab/product';
  import type { Dayjs } from 'dayjs';

  const props = defineProps<{
    visible: boolean;
    productSpecId: string;
    productSpecOptions: any[];
  }>();

  const emit = defineEmits<{
    (e: 'update:visible', visible: boolean): void;
    (e: 'success'): void;
  }>();

  const { createMessage } = useMessage();

  const loading = ref(false);
  const result = ref<any>(null);
  const versionList = ref<any[]>([]);

  const formState = reactive({
    productSpecId: '',
    productSpecVersion: undefined as number | undefined,
    dateRange: null as [Dayjs, Dayjs] | null,
    forceRegenerate: false,
  });

  watch(
    () => props.visible,
    (val) => {
      if (val) {
        formState.productSpecId = props.productSpecId;
        formState.productSpecVersion = undefined;
        formState.dateRange = null;
        formState.forceRegenerate = false;
        result.value = null;
        versionList.value = [];
        
        if (formState.productSpecId) {
            handleProductSpecChange(formState.productSpecId);
        }
      }
    },
  );

  async function handleProductSpecChange(val) {
      if (!val) {
          versionList.value = [];
          formState.productSpecVersion = undefined;
          return;
      }
      try {
          const response = await getProductSpecVersionList(val);
          // 确保返回的是数组格式（可能被包装在 data 字段中）
          const res = Array.isArray(response) ? response : (response?.data || []);
          
          if (!Array.isArray(res)) {
              console.warn('版本列表格式不正确:', response);
              versionList.value = [];
              return;
          }
          
          versionList.value = res;
      } catch (error) {
          console.error('获取版本列表失败', error);
      }
  }

  async function handleOk() {
    if (!formState.productSpecId) {
      createMessage.warning('请选择产品规格');
      return;
    }

    loading.value = true;
    try {
      const params: any = {
        productSpecId: formState.productSpecId,
        productSpecVersion: formState.productSpecVersion,
        forceRegenerate: formState.forceRegenerate,
      };

      if (formState.dateRange) {
        params.startDate = formState.dateRange[0].format('YYYY-MM-DD');
        params.endDate = formState.dateRange[1].format('YYYY-MM-DD');
      }

      const res = await generateIntermediateData(params);
      result.value = res.data || res;

      if (result.value.successCount > 0) {
        emit('success');
      }
    } catch (error: any) {
      createMessage.error(error.message || '生成失败');
    } finally {
      loading.value = false;
    }
  }

  function handleCancel() {
    emit('update:visible', false);
  }

  function handleClose() {
    emit('update:visible', false);
  }
</script>

<style scoped>
  .error-list {
    margin-top: 16px;
    padding: 12px;
    background: #fff1f0;
    border-radius: 4px;
    max-height: 200px;
    overflow-y: auto;
  }

  .error-item {
    font-size: 12px;
    color: #666;
    margin-bottom: 4px;
  }

  .error-more {
    font-size: 12px;
    color: #999;
    font-style: italic;
  }
</style>
