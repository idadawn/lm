<template>
  <div class="dcr-container">
    <div class="dcr-title">
      <span class="pr-2">维度贡献排名</span>
      <a-tooltip>
        <template #title>依据各维度的基尼系数排名</template>
        <ExclamationCircleOutlined class="dcr-icon" />
      </a-tooltip>
    </div>
    <!-- <div class="flex py-2">
      <div class="mr-2 dcr-Breadcrumb"><FallOutlined /> 下钻层级</div>
      <template v-if="true">
        <div class="dcr-title dcr-title-reset">
          <span class="pr-2">当前未下钻 </span>
          <a-tooltip>
            <template #title>点击柱状图下钻，深入探索归因。</template>
            <ExclamationCircleOutlined class="dcr-icon" />
          </a-tooltip>
        </div>
      </template>
      <template v-else>
        <breadcrumb separator=">">
          <breadcrumb-item href="">Home</breadcrumb-item>
          <breadcrumb-item href="">Application Center</breadcrumb-item>
          <breadcrumb-item href="">Application List</breadcrumb-item>
          <breadcrumb-item>An Application</breadcrumb-item>
      </template>
    </div> -->
    <div class="mt-3">
      <DimensionContributionRankingList :tableData="state.tableData" />
    </div>
  </div>
</template>

<script setup lang="ts">
  import { ExclamationCircleOutlined, FallOutlined } from '@ant-design/icons-vue';
  import DimensionContributionRankingList from './DimensionContributionRankingList.vue';
  import { ref, toRefs, onMounted, Ref, watch, reactive } from 'vue';

  defineOptions({
    name: 'DimensionContributionRanking',
  });

  const props = defineProps<{
    tableData: any;
  }>();

  const state = reactive<State>({
    tableData: '',
  });

  onMounted(() => {
    console.log('----+++++++++00--------', props.tableData);
  });
  watch(
    () => props,
    (newValue, oldValue) => {
      console.log('----+++++++++--------', newValue);
      state.tableData = newValue.tableData;
    },
    { deep: true },
  );
  // watch(
  //   () => props.tableData.value,
  //   () => {
  //     console.log(props);
  //   },
  //   {
  //     deep: true,
  //   },
  // );
</script>

<style lang="less" scoped>
  .dcr-container {
    .dcr-title {
      span {
        font-weight: 600;
        font-size: 14px;
        line-height: 22px;
        color: #546174;
      }
      .dcr-icon {
        font-size: 12px;
        cursor: pointer;
        color: #a5b2c5;
        svg {
          vertical-align: text-bottom;
        }
      }
    }
    .dcr-Breadcrumb {
      font-weight: 400;
      font-size: 12px;
      line-height: 18px;
      color: #546174;
      background: #ecf0f8;
      padding: 2px 4px 2px 6px;
      border-radius: 6px;
    }
    .dcr-title-reset {
      span {
        font-weight: inherit;
        font-size: 12px;
        color: #8b99ae;
        svg {
          vertical-align: text-bottom;
        }
      }
    }
  }
</style>
