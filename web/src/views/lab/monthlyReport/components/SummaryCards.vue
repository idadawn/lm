<template>
    <div class="summary-cards" :style="{ gridTemplateColumns: `repeat(${Math.min(cards.length, 5)}, 1fr)` }">
        <div v-for="card in cards" :key="card.key" class="summary-card" :class="card.colorClass">
            <div class="card-icon">
                <Icon :icon="card.icon" :size="28" />
            </div>
            <div class="card-content">
                <div class="card-label">{{ card.label }}</div>
                <div class="card-value">
                    <a-spin v-if="loading" :size="'small'" />
                    <template v-else>
                        <CountTo :startVal="0" :endVal="card.value" :duration="1500" :decimals="card.decimals"
                            separator="," />
                        <span class="value-unit">{{ card.unit }}</span>
                    </template>
                </div>
                <div v-if="card.subValue !== undefined" class="card-sub">
                    <span class="sub-label">占比</span>
                    <span class="sub-value">{{ card.subValue }}%</span>
                </div>
            </div>
        </div>
    </div>
</template>

<script lang="ts" setup>
import { computed } from 'vue';
import { Icon } from '/@/components/Icon';
import { CountTo } from '/@/components/CountTo';
import type { SummaryData } from '/@/api/lab/monthlyQualityReport';
import type { ReportConfig } from '/@/api/lab/reportConfig';

interface Props {
    data: SummaryData;
    loading?: boolean;
    reportConfigs?: ReportConfig[];
}

interface Card {
    key: string;
    label: string;
    value: number;
    unit: string;
    decimals: number;
    icon: string;
    colorClass: string;
    subValue?: number;
}

const props = withDefaults(defineProps<Props>(), {
    loading: false,
    reportConfigs: () => [],
});

function getDynamicStat(name: string) {
    return props.data?.dynamicStats?.[name] || { weight: 0, rate: 0 };
}

// 颜色主题映射（循环使用）
const colorClasses = ['blue', 'cyan', 'purple', 'green'];
const icons = [
    'ant-design:star-outlined',
    'ant-design:like-outlined',
    'ant-design:trophy-outlined',
    'ant-design:rocket-outlined'
];

// 卡片配置
const cards = computed(() => {
    // 基本卡片：检验总重
    const result: Card[] = [
        {
            key: 'total',
            label: '检验总重',
            value: props.data?.totalWeight ?? 0,
            unit: 'kg',
            decimals: 1,
            icon: 'ant-design:experiment-outlined',
            colorClass: 'purple',
        }
    ];

    // 动态配置卡片 - 只展示 isHeader=true 的配置
    const headerConfigs = (props.reportConfigs || []).filter(c => c.isHeader);
    const dynamicCards = headerConfigs.map((config, index) => {
        // 后端返回的 dynamicStats key 是 config.id
        const stat = getDynamicStat(config.id);

        if (config.isPercentage) {
            // 百分比类型：主值显示百分比，不显示副值
            return {
                key: `config-${config.id}`,
                label: config.name,
                value: stat.rate,
                unit: '%',
                decimals: 2,
                icon: icons[index % icons.length],
                colorClass: colorClasses[index % colorClasses.length],
            };
        } else {
            // 非百分比类型：主值显示求和(weight)，isShowRatio 时下方显示占比
            return {
                key: `config-${config.id}`,
                label: config.name,
                value: stat.weight,
                unit: 'kg',
                decimals: 1,
                subValue: config.isShowRatio ? stat.rate : undefined,
                icon: icons[index % icons.length],
                colorClass: colorClasses[index % colorClasses.length],
            };
        }
    });

    result.push(...dynamicCards);

    return result;
});
</script>

<style lang="less" scoped>
.summary-cards {
    display: grid;
    grid-template-columns: repeat(5, 1fr);
    gap: 16px;
    margin-bottom: 16px;

    @media (max-width: 1400px) {
        grid-template-columns: repeat(3, 1fr);
    }

    @media (max-width: 900px) {
        grid-template-columns: repeat(2, 1fr);
    }

    @media (max-width: 600px) {
        grid-template-columns: 1fr;
    }
}

.summary-card {
    display: flex;
    align-items: center;
    gap: 16px;
    padding: 20px;
    background: #fff;
    border-radius: 12px;
    box-shadow: 0 2px 12px rgba(0, 0, 0, 0.06);
    transition: all 0.3s ease;

    &:hover {
        transform: translateY(-2px);
        box-shadow: 0 6px 20px rgba(0, 0, 0, 0.1);
    }

    // 颜色主题
    &.purple {
        .card-icon {
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
        }

        .card-value {
            color: #764ba2;
        }
    }

    &.green {
        .card-icon {
            background: linear-gradient(135deg, #52c41a 0%, #73d13d 100%);
        }

        .card-value {
            color: #52c41a;
        }
    }

    &.blue {
        .card-icon {
            background: linear-gradient(135deg, #1890ff 0%, #40a9ff 100%);
        }

        .card-value {
            color: #1890ff;
        }
    }

    &.cyan {
        .card-icon {
            background: linear-gradient(135deg, #13c2c2 0%, #36cfc9 100%);
        }

        .card-value {
            color: #13c2c2;
        }
    }

    &.orange {
        .card-icon {
            background: linear-gradient(135deg, #fa8c16 0%, #ffc53d 100%);
        }

        .card-value {
            color: #fa8c16;
        }
    }
}

.card-icon {
    width: 56px;
    height: 56px;
    border-radius: 12px;
    display: flex;
    align-items: center;
    justify-content: center;
    color: #fff;
    flex-shrink: 0;
}

.card-content {
    flex: 1;
    min-width: 0;
}

.card-label {
    font-size: 13px;
    color: #8c8c8c;
    margin-bottom: 6px;
}

.card-value {
    font-size: 28px;
    font-weight: 700;
    line-height: 1.2;
    display: flex;
    align-items: baseline;
    gap: 4px;

    .value-unit {
        font-size: 14px;
        font-weight: 400;
        color: #8c8c8c;
    }
}

.card-sub {
    display: flex;
    align-items: center;
    gap: 6px;
    margin-top: 6px;
    font-size: 12px;

    .sub-label {
        color: #bfbfbf;
    }

    .sub-value {
        color: #595959;
        font-weight: 500;
    }
}
</style>
