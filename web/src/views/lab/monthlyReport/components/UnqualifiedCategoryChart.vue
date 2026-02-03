<template>
    <div class="chart-card">
        <div class="chart-header">
            <h4 class="chart-title">
                <Icon icon="ant-design:pie-chart-outlined" :size="16" />
                不合格分类分析
            </h4>
        </div>
        <div ref="chartRef" class="chart-container"></div>
    </div>
</template>

<script lang="ts" setup>
import { ref, onMounted, watch, type Ref } from 'vue';
import { useECharts } from '/@/hooks/web/useECharts';
import { Icon } from '/@/components/Icon';
import type { UnqualifiedCategory } from '/@/api/lab/monthlyQualityReport';

interface Props {
    data: UnqualifiedCategory[];
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
    if (!props.data.length && !chartRef.value) return;

    // 按重量排序，取前5
    const sortedData = [...props.data].sort((a, b) => b.weight - a.weight).slice(0, 5);
    const categories = sortedData.map(d => d.categoryName);
    const weights = sortedData.map(d => d.weight);

    // 颜色渐变
    const colors = ['#ff4d4f', '#ff7a45', '#ffa940', '#ffbb33', '#ffd666'];

    const option: any = {
        tooltip: {
            trigger: 'axis',
            axisPointer: { type: 'shadow' },
            backgroundColor: 'rgba(255, 255, 255, 0.95)',
            borderColor: '#e8e8e8',
            borderWidth: 1,
            textStyle: { color: '#333' },
            formatter: (params: any) => {
                const item = params[0];
                return `<div style="font-weight: 500">${item.name}</div>
                  <div style="margin-top: 4px">重量: ${item.value.toFixed(1)} kg</div>`;
            },
        },
        grid: {
            left: 90,
            right: 40,
            top: 8,
            bottom: 8,
        },
        xAxis: {
            type: 'value',
            axisLine: { show: false },
            axisLabel: {
                fontSize: 10,
                color: '#8c8c8c',
                formatter: (value: number) => value >= 1000 ? `${(value / 1000).toFixed(0)}k` : value,
            },
            splitLine: { lineStyle: { color: '#f0f0f0', type: 'dashed' } },
        },
        yAxis: {
            type: 'category',
            data: categories.reverse(),
            axisLine: { show: false },
            axisTick: { show: false },
            axisLabel: {
                fontSize: 11,
                color: '#595959',
                width: 80,
                overflow: 'truncate',
            },
        },
        series: [
            {
                type: 'bar',
                data: weights.reverse().map((value, index) => ({
                    value,
                    itemStyle: {
                        color: {
                            type: 'linear',
                            x: 0, y: 0, x2: 1, y2: 0,
                            colorStops: [
                                { offset: 0, color: colors[colors.length - 1 - index] || '#ffd666' },
                                { offset: 1, color: '#fff1f0' },
                            ],
                        },
                        borderRadius: [0, 4, 4, 0],
                    },
                })),
                barWidth: 16,
                label: {
                    show: true,
                    position: 'right',
                    fontSize: 10,
                    color: '#8c8c8c',
                    formatter: (params: any) => `${params.value.toFixed(1)} kg`,
                },
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
    height: 180px;
}
</style>
