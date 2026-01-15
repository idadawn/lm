<template>
  <div class="kpi-chart-container">
    <div class="mb-4 flex items-center gap-4">
      <a-select
        v-model:value="selectedMetrics"
        mode="multiple"
        style="min-width: 300px"
        placeholder="请选择指标"
        :options="metricOptions"
        @change="handleMetricChange"
        :max-tag-count="3"
        allow-clear
      />
      <a-select
        v-model:value="selectedDimension"
        style="width: 200px"
        placeholder="请选择维度"
        :options="dimensionOptions"
        :disabled="!selectedMetrics.length"
      />
      <a-button type="primary" @click="handleSearch" :loading="loading" :disabled="!canSearch">
        分析
      </a-button>
    </div>

    <div v-if="loading" class="flex justify-center items-center h-96">
      <a-spin />
    </div>
    <div v-else ref="chartRef" class="w-full h-[600px]"></div>
  </div>
</template>

<script setup lang="ts">
  import { ref, onMounted, computed, watch } from 'vue';
  import { Select as ASelect, Button as AButton, Spin as ASpin, message } from 'ant-design-vue';
  import { useECharts } from '/@/hooks/web/useECharts';
  import { defHttp } from '/@/utils/http/axios';

  // Interfaces
  interface MetricInfo {
    id: string;
    name: string;
  }

  interface TableFieldOutput {
    field: string;
    fieldName: string;
    dataType: string;
  }

  interface MetricOption {
    label: string;
    value: string;
  }

  interface DimensionOption {
    label: string;
    value: string;
    fieldData: TableFieldOutput;
  }

  // State
  const chartRef = ref<HTMLDivElement | null>(null);
  const { setOptions } = useECharts(chartRef as any);
  
  const metricOptions = ref<MetricOption[]>([]);
  const selectedMetrics = ref<string[]>([]);
  
  const dimensionOptions = ref<DimensionOption[]>([]);
  const selectedDimension = ref<string | undefined>(undefined);
  
  const loading = ref(false);

  // Computed
  const canSearch = computed(() => selectedMetrics.value.length > 0 && selectedDimension.value);

  // API Calls
  const fetchMetricList = async () => {
    try {
      const res = await defHttp.post({ url: '/api/kpi/v1/MetricInfo/list', params: { PageSize: 1000, CurrentPage: 1 } });
      if (res && res.list) {
        metricOptions.value = res.list.map((m: any) => ({ label: m.name, value: m.id }));
      }
    } catch (error) {
      console.error('Failed to fetch metrics:', error);
    }
  };

  const fetchDimensions = async (metricIds: string[]) => {
    if (!metricIds.length) {
      dimensionOptions.value = [];
      selectedDimension.value = undefined;
      return;
    }
    try {
      const res = await defHttp.post({ 
        url: '/api/kpi/v1/MetricInfo/dims', 
        params: { metricIds } 
      });
      
      if (res && res.dimensions) {
        dimensionOptions.value = res.dimensions.map((d: TableFieldOutput) => ({
          label: d.fieldName || d.field,
          value: d.field,
          fieldData: d
        }));
        
        // Auto-select first dimension if none selected or current invalid
        if (!selectedDimension.value || !dimensionOptions.value.find(d => d.value === selectedDimension.value)) {
           if (dimensionOptions.value.length > 0) {
             selectedDimension.value = dimensionOptions.value[0].value;
           } else {
             selectedDimension.value = undefined;
           }
        }
      }
    } catch (error) {
      console.error('Failed to fetch dimensions:', error);
    }
  };

  const handleMetricChange = () => {
    fetchDimensions(selectedMetrics.value);
  };

  const handleSearch = async () => {
    if (!canSearch.value) return;
    
    loading.value = true;
    try {
      const dimOption = dimensionOptions.value.find(d => d.value === selectedDimension.value);
      if (!dimOption) return;

      // Fetch data for EACH metric separately to get their values
      const tasks = selectedMetrics.value.map(async (metricId) => {
        const metricLabel = metricOptions.value.find(m => m.value === metricId)?.label || metricId;
        const params = {
          MetricId: metricId,
          Dimensions: dimOption.fieldData,
          AggType: 'SUM', // Default to SUM for now, could be dynamic
          Limit: 100
        };
        try {
           const res = await defHttp.post({ url: '/api/kpi/v1//MetricInfo/chart_data', params });
           return {
             name: metricLabel,
             data: res?.list || [] // backend GetMetricChartDataAsync returns ModelChartDataOutput which uses 'List' for data beans
           };
        } catch (e) {
           console.error(`Failed for ${metricLabel}`, e);
           return { name: metricLabel, data: [] };
        }
      });

      const results = await Promise.all(tasks);
      renderChart(results, dimOption.label);

    } catch (error) {
      console.error('Analysis failed:', error);
      message.error('分析失败');
    } finally {
      loading.value = false;
    }
  };

  const renderChart = (seriesDataList: any[], dimLabel: string) => {
    // Collect all unique X-axis values
    const xValuesSet = new Set<string>();
    seriesDataList.forEach(series => {
        series.data.forEach((item: any) => {
            if (item.dimension) xValuesSet.add(item.dimension);
        });
    });
    const xData = Array.from(xValuesSet).sort();

    const series = seriesDataList.map(s => {
        // Map data to xData alignment
        const dataMap = new Map(s.data.map((item: any) => [item.dimension, item.value]));
        const alignedData = xData.map(x => dataMap.get(x) || 0);

        return {
            name: s.name,
            type: 'line' as const, // Default to line for comparison
            smooth: true,
            data: alignedData
        };
    });

    setOptions({
      tooltip: { trigger: 'axis' },
      legend: { data: seriesDataList.map(s => s.name) },
      xAxis: { type: 'category', data: xData, name: dimLabel },
      yAxis: { type: 'value' },
      series: series
    });
  };

  onMounted(() => {
    fetchMetricList();
  });
</script>

<style scoped>
.kpi-chart-container {
  padding: 16px;
  background-color: #fff;
}
</style>
