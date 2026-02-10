<template>
    <div class="chart-card">
        <div class="chart-header">
            <h4 class="chart-title">
                <Icon icon="ant-design:bar-chart-outlined" :size="16" />
                班次产量对比
            </h4>
        </div>
        <div ref="chartRef" class="chart-container"></div>
    </div>
</template>

<script lang="ts" setup>
import { ref, onMounted, watch, type Ref } from 'vue';
import { useECharts } from '/@/hooks/web/useECharts';
import { Icon } from '/@/components/Icon';
import type { ShiftComparison } from '/@/api/lab/monthlyQualityReport';
import type { ReportConfig } from '/@/api/lab/reportConfig';

interface Props {
    data: ShiftComparison[];
    loading?: boolean;
    reportConfigs?: ReportConfig[];
}

const props = withDefaults(defineProps<Props>(), {
    loading: false,
    data: () => [],
    reportConfigs: () => [],
});

const chartRef = ref<HTMLDivElement | null>(null);
const { setOptions } = useECharts(chartRef as Ref<HTMLDivElement>);

// 班次颜色
const shiftColors: Record<string, string> = {
    '甲': '#1890ff',
    '乙': '#52c41a',
    '丙': '#fa8c16',
};

onMounted(() => {
    updateChart();
});

watch(() => [props.data, props.reportConfigs], () => {
    updateChart();
}, { deep: true });

function updateChart() {
    if (!props.data.length && !chartRef.value) return;

    const shifts = props.data.map(d => `${d.shift}班`);
    const weights = props.data.map(d => d.totalWeight);
    const rates = props.data.map(d => d.qualifiedRate);

    // Dynamic series
    const series: any[] = [
        {
            name: '产量',
            type: 'bar',
            data: weights.map((value, index) => ({
                value,
                itemStyle: {
                    color: {
                        type: 'linear',
                        x: 0, y: 0, x2: 0, y2: 1,
                        colorStops: [
                            { offset: 0, color: shiftColors[props.data[index]?.shift] || '#1890ff' },
                            { offset: 1, color: 'rgba(24, 144, 255, 0.2)' },
                        ],
                    },
                    borderRadius: [4, 4, 0, 0],
                },
            })),
            barWidth: 40,
            label: {
                show: true,
                position: 'top',
                fontSize: 10,
                color: '#595959',
                formatter: (params: any) => `${(params.value / 1000).toFixed(1)}k`,
            },
        },
        {
            name: '合格率',
            type: 'line',
            yAxisIndex: 1,
            data: rates,
            symbol: 'circle',
            symbolSize: 8,
            lineStyle: { width: 2, color: '#52c41a' },
            itemStyle: { color: '#52c41a', borderWidth: 2, borderColor: '#fff' },
        }
    ];

    // Configured dynamic stats
    const legendData = ['产量', '合格率'];

    if (props.reportConfigs && props.reportConfigs.length > 0) {
        const colors = ['#1890ff', '#13c2c2', '#722ed1', '#fa8c16', '#f5222d'];
        const reportVisibleConfigs = props.reportConfigs.filter(c => c.isShowInReport);
        reportVisibleConfigs.forEach((config, index) => {
            // Avoid adding "合格率" again if it's the same name
            if (config.name === '合格率') return;

            const rates = props.data.map(d => d.dynamicStats?.[config.id] || 0);
            const color = colors[index % colors.length];

            legendData.push(config.name);
            series.push({
                name: config.name,
                type: 'line',
                yAxisIndex: 1,
                data: rates,
                smooth: true,
                symbol: 'circle',
                symbolSize: 5,
                lineStyle: { width: 2, color: color },
                itemStyle: { color: color },
            });
        });
    }

    const option: any = {
        tooltip: {
            trigger: 'axis',
            axisPointer: { type: 'shadow' },
            backgroundColor: 'rgba(255, 255, 255, 0.95)',
            borderColor: '#e8e8e8',
            borderWidth: 1,
            textStyle: { color: '#333' },
            formatter: (params: any) => {
                const header = `<div style="font-weight: 500; margin-bottom: 8px">${params[0].axisValue}</div>`;
                let rows = '';

                params.forEach((param: any) => {
                    let valueStr = '';
                    if (param.seriesType === 'bar') {
                        valueStr = `${param.value.toFixed(1)} kg`;
                    } else {
                        valueStr = `${param.value}%`;
                    }

                    rows += `<div style="display: flex; align-items: center; gap: 6px; margin-top: 4px">
                    <span style="width: 8px; height: 8px; border-radius: 50%; background: ${param.color}"></span>
                    <span>${param.seriesName}: ${valueStr}</span>
                  </div>`;
                });

                return header + rows;
            },
        },
        legend: {
            data: legendData,
            bottom: 0,
            itemWidth: 12,
            itemHeight: 8,
            textStyle: { fontSize: 11 },
        },
        grid: {
            left: 50,
            right: 50,
            top: 16,
            bottom: 36,
        },
        xAxis: {
            type: 'category',
            data: shifts,
            axisLine: { lineStyle: { color: '#e8e8e8' } },
            axisLabel: { fontSize: 12, color: '#595959', fontWeight: 500 },
            axisTick: { show: false },
        },
        yAxis: [
            {
                type: 'value',
                name: 'kg',
                nameTextStyle: { fontSize: 10, color: '#8c8c8c', padding: [0, 24, 0, 0] },
                axisLine: { show: false },
                axisLabel: {
                    fontSize: 10,
                    color: '#8c8c8c',
                    formatter: (value: number) => value >= 1000 ? `${(value / 1000).toFixed(0)}k` : value,
                },
                splitLine: { lineStyle: { color: '#f0f0f0', type: 'dashed' } },
            },
            {
                type: 'value',
                name: '%',
                min: 0,
                max: 100,
                nameTextStyle: { fontSize: 10, color: '#8c8c8c', padding: [0, 0, 0, 24] },
                axisLine: { show: false },
                axisLabel: { fontSize: 10, color: '#8c8c8c' },
                splitLine: { show: false },
            },
        ],
        series: series,
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
