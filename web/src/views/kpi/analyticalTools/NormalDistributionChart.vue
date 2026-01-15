<template>
  <div class="mb-2">
    <a-form
      ref="formRef"
      :model="formState"
      name="horizontal_login"
      layout="inline"
      autocomplete="off"
      :label-col="labelCol"
      :colon="false"
      class="item-mb">
      <a-form-item label="炉号" name="furnaceNumber">
        <a-input v-model:value="formState.furnaceNumber" placeholder="请输入炉号" allowClear />
      </a-form-item>

      <a-form-item label="批号" name="batchNumber">
        <a-input v-model:value="formState.batchNumber" placeholder="请输入批号" allowClear />
      </a-form-item>

      <a-form-item label="时间" name="datetime">
        <a-range-picker v-model:value="formState.datetime" show-time />
      </a-form-item>

      <a-form-item label="规格上线" name="batchNumber">
        <a-input v-model:value="formState.batchNumber" placeholder="规格上线" allowClear />
      </a-form-item>

      <a-form-item label="规格下线" name="batchNumber">
        <a-input v-model:value="formState.batchNumber" placeholder="规格下线" allowClear />
      </a-form-item>

      <a-form-item label=" ">
        <a-button type="primary" html-type="submit" class="mr-4">搜索</a-button>
        <a-button @click="resetForm">重置</a-button>
      </a-form-item>
    </a-form>
  </div>

  <div class="my-2">
    <a-dropdown-button class="mr-4" :trigger="['click']">
      {{ dropdownContent }}
      <template #overlay>
        <a-menu @click="selectStandardDeviation">
          <a-menu-item :key="index + 1" v-for="(item, index) in plainOptions">{{ item }}</a-menu-item>
        </a-menu>
      </template>
      <template #icon><DownOutlined /></template>
    </a-dropdown-button>
    <a-button type="primary" ghost danger @click="showDataModal">数据详情</a-button>
  </div>

  <spin :spinning="spinning">
    <a-row>
      <a-col :span="24">
        <div ref="normalDistributionChart" class="normal-distribution-chart"></div>
      </a-col>
    </a-row>
  </spin>

  <a-modal v-model:visible="dataDetailsVisible" title="数据详情">
    <div class="px-6 py-3">
      <a-list item-layout="horizontal" :data-source="dataItem.list">
        <template #renderItem="{ item }">
          <a-list-item>
            <a-list-item-meta description="">
              <template #title>
                {{ item.label }}
              </template>
            </a-list-item-meta>
            {{ item.value }}
          </a-list-item>
        </template>
      </a-list>
    </div>
  </a-modal>
</template>

<script setup lang="ts">
  import { ref, onMounted, Ref, reactive } from 'vue';
  import { useECharts } from '/@/hooks/web/useECharts';
  import { normalDistributionOption } from './config/utils';
  import { defHttp } from '/@/utils/http/axios';
  import { ResultEnum } from '/@/enums/httpEnum';
  import {
    Spin,
    Modal as AModal,
    RangePicker as ARangePicker,
    List as AList,
    ListItem as AListItem,
    ListItemMeta as AListItemMeta,
  } from 'ant-design-vue';
  import { DownOutlined } from '@ant-design/icons-vue';
  import type { FormInstance } from 'ant-design-vue';
  import type { Dayjs } from 'dayjs';

  type RangeValue = [Dayjs, Dayjs];

  interface FormState {
    furnaceNumber: string;
    batchNumber: string;
    datetime: RangeValue | undefined;
  }

  interface DataItem {
    label: string;
    name: string;
    value: string | number | string[] | number[] | undefined;
  }

  const dataItem = reactive<{ list: DataItem[] }>({
    list: [
      {
        label: '均值',
        name: 'mean',
        value: '1',
      },
      {
        label: '方差',
        name: 'variance',
        value: '2',
      },
      {
        label: '标准差',
        name: 'standardDeviation',
        value: '3',
      },
      {
        label: '一倍标准差',
        name: 'oneStandardDeviation',
        value: '3',
      },
      {
        label: '二倍标准差',
        name: 'twoStandardDeviation',
        value: '3',
      },
      {
        label: '三倍标准差',
        name: 'threeStandardDeviation',
        value: '3',
      },
      {
        label: '频率',
        name: 'frequency',
        value: [1, 2, 3, 4, 5],
      },
      {
        label: '正态分布',
        name: 'normalDistribution',
        value: [1, 2, 3, 4, 5],
      },
    ],
  });

  const formRef = ref<FormInstance>();

  const formState = reactive<FormState>({
    furnaceNumber: '',
    batchNumber: '',
    datetime: undefined,
  });

  const labelCol = ref({ style: { width: '150px' } });

  const dataDetailsVisible = ref(false);

  const resetForm = () => {
    formRef.value?.resetFields();
  };

  const showDataModal = () => {
    dataDetailsVisible.value = true;
  };

  const plainOptions = ref(['一倍标准差', '二倍标准差', '三倍标准差']);

  const dropdownContent = ref('请选择标准差');

  const selectStandardDeviation = e => {
    dropdownContent.value = e.domEvent.target.innerText;
  };

  // 正态分布Chart Box
  const normalDistributionChart = ref<HTMLDivElement | null>(null);
  // 正态分布数据详情
  // const ndDataSet = ref<DataSet>();
  const ndSetOptions = ref();

  const spinning = ref(false);

  const apiInfo = () => {
    spinning.value = true;
    return defHttp.get({ url: `/api/kpi/v1/analysisdata/list` });
  };

  // 页面加载完成
  onMounted(() => {
    // 正态分布
    const { setOptions: ndOptions } = useECharts(normalDistributionChart as Ref<HTMLDivElement>);
    ndSetOptions.value = ndOptions;
    initNormalDistribution();
  });

  /**
   * @Description：正态分布
   * @Date：2023-11-28
   * */
  const initNormalDistribution = () => {
    if (!normalDistributionChart.value) return;
    apiInfo().then(res => {
      if (res.code === ResultEnum.SUCCESS) {
        spinning.value = false;
        const data = res.data;
        const options = normalDistributionOption(data);
        ndSetOptions.value(options);

        dataItem.list.forEach(item => {
          if (item.name === 'frequency') {
            item.value = data.yAxisHistogram;
          }

          if (item.name === 'normalDistribution') {
            item.value = data.yAxis;
          }
        });
      }
    });
  };

</script>

<style lang="less" scoped>
  .nd-title {
    color: rgba(0, 0, 0, 0.45);
    font-size: 14px;
  }

  .nd-value {
    overflow: hidden;
    color: rgba(0, 0, 0, 0.88);
    font-size: 16px;
    word-wrap: break-word;
    word-break: normal;
  }

  .normal-distribution-chart {
    width: 100%;
    height: calc(100vh - 320px);
    margin-bottom: 4px;
  }

  .item-mb {
    height: 90px;
  }

  .ant-list-item {
    padding: 6px 0;
  }
</style>
