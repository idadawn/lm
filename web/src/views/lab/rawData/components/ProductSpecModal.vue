<template>
  <BasicModal
    v-bind="$attrs"
    @register="registerModal"
    title="产品规格详情"
    :width="600"
    :show-ok-btn="false"
    destroyOnClose>

    <div v-if="productSpec" class="product-spec-view">
      <!-- 基本信息 -->
      <a-row :gutter="24">
        <a-col :span="12">
          <div class="view-form-item">
            <label class="view-label">规格代码</label>
            <div class="view-value">{{ productSpec.code }}</div>
          </div>
        </a-col>
        <a-col :span="12">
          <div class="view-form-item">
            <label class="view-label">规格名称</label>
            <div class="view-value">{{ productSpec.name }}</div>
          </div>
        </a-col>
      </a-row>

      <a-row :gutter="24">
        <a-col :span="12">
          <div class="view-form-item">
            <label class="view-label">检测列</label>
            <div class="view-value">{{ productSpec.detectionColumns }}</div>
          </div>
        </a-col>
        <a-col :span="12">
          <div class="view-form-item">
            <label class="view-label">长度(m)</label>
            <div class="view-value">{{ productSpec.length || '-' }}</div>
          </div>
        </a-col>
      </a-row>

      <a-row :gutter="24">
        <a-col :span="12">
          <div class="view-form-item">
            <label class="view-label">层数</label>
            <div class="view-value">{{ productSpec.layers || '-' }}</div>
          </div>
        </a-col>
        <a-col :span="12">
          <div class="view-form-item">
            <label class="view-label">密度</label>
            <div class="view-value">{{ productSpec.density || '-' }}</div>
          </div>
        </a-col>
      </a-row>

      <!-- 描述信息 -->
      <div v-if="productSpec.description" class="view-form-item">
        <label class="view-label">描述</label>
        <div class="view-value description">{{ productSpec.description }}</div>
      </div>
    </div>
    <a-empty v-else description="产品规格信息不存在" />
  </BasicModal>
</template>

<script lang="ts" setup>
  import { ref } from 'vue';
  import { BasicModal, useModalInner } from '/@/components/Modal';
  import { getProductSpecInfo } from '/@/api/lab/product';

  const emit = defineEmits(['register']);
  const [registerModal] = useModalInner(init);
  const productSpec = ref<any>(null);

  async function init(data) {
    productSpec.value = null;
    if (data?.productSpecId) {
      try {
        const res = await getProductSpecInfo(data.productSpecId);
        const specData = res.data;
        // 从扩展属性列表中解析长度、层数、密度
        if (specData.attributes && Array.isArray(specData.attributes)) {
          const lengthAttr = specData.attributes.find(a => a.attributeKey === 'length');
          const layersAttr = specData.attributes.find(a => a.attributeKey === 'layers');
          const densityAttr = specData.attributes.find(a => a.attributeKey === 'density');
          
          specData.length = lengthAttr ? parseFloat(lengthAttr.attributeValue) : null;
          specData.layers = layersAttr ? parseInt(layersAttr.attributeValue) : null;
          specData.density = densityAttr ? parseFloat(densityAttr.attributeValue) : null;
        }
        productSpec.value = specData;
      } catch (error) {
        console.error('获取产品规格信息失败', error);
      }
    }
  }
</script>

<style lang="less" scoped>
.product-spec-view {
  padding: 16px 24px;
}

.view-form-item {
  margin-bottom: 16px;

  .view-label {
    display: block;
    color: #666;
    font-size: 14px;
    margin-bottom: 4px;
  }

  .view-value {
    background-color: #f5f5f5;
    border: 1px solid #d9d9d9;
    border-radius: 4px;
    padding: 8px 12px;
    font-size: 14px;
    color: #262626;
    min-height: 32px;
    line-height: 1.5;

    &.description {
      min-height: 60px;
      padding: 12px;
      line-height: 1.6;
    }
  }
}

// 响应式布局
@media (max-width: 768px) {
  .product-spec-view {
    padding: 12px 16px;
  }
}
</style>