<template>
  <div class="bg-white rounded-lg p-4 shadow-sm border border-gray-100 hover:shadow-md transition-shadow relative group">
    <div class="flex items-start justify-between mb-3">
      <div class="flex items-center gap-3">
         <div class="w-12 h-12 rounded bg-gray-100 flex items-center justify-center text-xl font-bold text-gray-700">
             {{ feature.category.charAt(0) }}
         </div>
         <div>
             <h3 class="font-bold text-lg text-gray-800">{{ feature.category }}</h3>
             <p class="text-xs text-gray-500">{{ feature.description || '暂无描述' }}</p>
         </div>
      </div>
      <div class="hidden group-hover:flex gap-2">
          <a-button type="link" size="small" @click="$emit('view-vector', feature)">
            <InfoCircleOutlined /> 向量
          </a-button>
          <a-button type="link" size="small" @click="$emit('edit', feature)">编辑</a-button>
          <a-button type="link" size="small" danger @click="$emit('delete', feature)">删除</a-button>
      </div>
    </div>

    <!-- 特性等级 -->
    <div class="mb-3" v-if="feature.variantList && feature.variantList.length > 0">
        <div class="text-xs text-gray-500 mb-1">特性等级:</div>
        <div class="flex flex-wrap gap-2">
            <span 
                v-for="(variant, index) in feature.variantList" 
                :key="`variant-${index}`" 
                class="px-2 py-1 bg-purple-50 border border-purple-100 rounded text-sm text-purple-700 flex items-center gap-1"
            >
                <span class="font-medium">{{ variant.name }}</span>
                <span class="text-xs text-purple-500" v-if="variant.severity">({{ variant.severity }})</span>
            </span>
        </div>
    </div>

    <div class="bg-gray-800 text-white text-xs px-3 py-2 rounded flex justify-between items-center cursor-pointer hover:bg-gray-700 transition-colors" @click="$emit('edit', feature)">
        <span>编辑特性 (Vector)...</span>
        <span class="text-lg">+</span>
    </div>
  </div>
</template>

<script lang="ts" setup>
  import { PropType } from 'vue';
  import { InfoCircleOutlined } from '@ant-design/icons-vue';
  import { AppearanceFeatureInfo } from '/@/api/lab/appearance';

  defineProps({
    feature: {
      type: Object as PropType<AppearanceFeatureInfo>,
      required: true,
    },
  });

  defineEmits(['edit', 'delete', 'view-vector']);
</script>