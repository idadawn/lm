<template>
    <div class="chart-card">
        <div class="chart-header">
            <h4 class="chart-title">
                <Icon icon="ant-design:line-chart-outlined" :size="16" />
                质量趋势分析
            </h4>
        </div>
        <div ref="chartRef" class="chart-container"></div>
    </div>
</template>

<script lang="ts" setup>
import { ref, onMounted, watch, type Ref } from 'vue';
import { useECharts } from '/@/hooks/web/useECharts';
import { Icon } from '/@/components/Icon';
import type { QualityTrend } from '/@/api/lab/monthlyQualityReport';
import dayjs from 'dayjs';

interface Props {
    data: QualityTrend[];
    loading?: boolean;
}

const props = withDefaults(defineProps<Props>(), {
    loading: false,
    data: () => [],
});

const chartRef = ref<HTMLDivElement | null>(null);
const { setOptions } = useECharts(chartRef as Ref<HTMLDivElement>);

onMounted(() => {
    updateChart();
});

watch(() => props.data, () => {
    updateChart();
}, { deep: true });

function updateChart() {
    if (!props.data.length) return;

    const dates = props.data.map(d => dayjs(d.date).format('MM-DD'));
    const qualifiedRates = props.data.map(d => d.qualifiedRate);
    const classARates = props.data.map(d => d.classARate);
    const classBRates = props.data.map(d => d.classBRate);

    const option: any = {
        tooltip: {
            trigger: 'axis',
            backgroundColor: 'rgba(255, 255, 255, 0.95)',
            borderColor: '#e8e8e8',
            borderWidth: 1,
            textStyle: { color: '#333' },
            formatter: (params: any) => {
                let result = `<div style="font-weight: 500; margin-bottom: 8px">${params[0].axisValue}</div>`;
                params.forEach((param: any) => {
                    result += `<div style="display: flex; align-items: center; gap: 6px; margin: 4px 0">
              <span style="width: 8px; height: 8px; border-radius: 50%; background: ${param.color}"></span>
              <span>${param.seriesName}: ${param.value}%</span>
            </div>`;
                });
                return result;
            },
        },
        legend: {
            data: ['合格率', 'A类占比', 'B类占比'],
            bottom: 0,
            itemWidth: 12,
            itemHeight: 8,
            textStyle: { fontSize: 11 },
        },
        grid: {
            left: 40,
            right: 16,
            top: 16,
            bottom: 36,
        },
        xAxis: {
            type: 'category',
            data: dates,
            axisLine: { lineStyle: { color: '#e8e8e8' } },
            axisLabel: { fontSize: 10, color: '#8c8c8c' },
            axisTick: { show: false },
        },
        yAxis: {
            type: 'value',
            min: 0,
            max: 100,
            axisLine: { show: false },
            axisLabel: {
                fontSize: 10,
                color: '#8c8c8c',
                formatter: '{value}%',
            },
            splitLine: { lineStyle: { color: '#f0f0f0', type: 'dashed' } },
        },
        series: [
            {
                name: '合格率',
                type: 'line',
                data: qualifiedRates,
                smooth: true,
                symbol: 'circle',
                symbolSize: 6,
                lineStyle: { width: 2, color: '#52c41a' },
                itemStyle: { color: '#52c41a' },
                areaStyle: {
                    color: {
                        type: 'linear',
                        x: 0, y: 0, x2: 0, y2: 1,
                        colorStops: [
                            { offset: 0, color: 'rgba(82, 196, 26, 0.3)' },
                            { offset: 1, color: 'rgba(82, 196, 26, 0.05)' },
                        ],
                    },
                },
            },
            {
                name: 'A类占比',
                type: 'line',
                data: classARates,
                smooth: true,
                symbol: 'circle',
                symbolSize: 5,
                lineStyle: { width: 2, color: '#1890ff' },
                itemStyle: { color: '#1890ff' },
            },
            {
                name: 'B类占比',
                type: 'line',
                data: classBRates,
                smooth: true,
                symbol: 'circle',
                symbolSize: 5,
                lineStyle: { width: 2, color: '#13c2c2' },
                itemStyle: { color: '#13c2c2' },
            },
        ],
    };

    setOptions(option);
}
</script>

<style lang="less" scoped>
.chart-card {
    background: #fff;
    border-radius: 12px;
    padding: 16px;
    box-shadow: 0 2px 12px rgba(0, 0, 0, 0.04);
}

.chart-header {
    margin-bottom: 12px;
}

.chart-title {
    display: flex;
    align-items: center;
    gap: 6px;
    font-size: 14px;
    font-weight: 600;
    color: #262626;
    margin: 0;
}

.chart-container {
    height: 200px;
}
</style>
