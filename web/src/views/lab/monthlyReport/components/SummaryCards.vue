<template>
    <div class="summary-cards">
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

interface Props {
    data: SummaryData;
    loading?: boolean;
}

const props = withDefaults(defineProps<Props>(), {
    loading: false,
});

// 卡片配置
const cards = computed(() => [
    {
        key: 'total',
        label: '检验总重',
        value: props.data?.totalWeight ?? 0,
        unit: 'kg',
        decimals: 1,
        icon: 'ant-design:experiment-outlined',
        colorClass: 'purple',
    },
    {
        key: 'qualified',
        label: '合格率',
        value: props.data?.qualifiedRate ?? 0,
        unit: '%',
        decimals: 2,
        icon: 'ant-design:check-circle-outlined',
        colorClass: 'green',
    },
    {
        key: 'classA',
        label: 'A类总计',
        value: props.data?.classAWeight ?? 0,
        unit: 'kg',
        decimals: 1,
        subValue: props.data?.classARate ?? 0,
        icon: 'ant-design:star-outlined',
        colorClass: 'blue',
    },
    {
        key: 'classB',
        label: 'B类总计',
        value: props.data?.classBWeight ?? 0,
        unit: 'kg',
        decimals: 1,
        subValue: props.data?.classBRate ?? 0,
        icon: 'ant-design:like-outlined',
        colorClass: 'cyan',
    },
    {
        key: 'unqualified',
        label: '不合格总计',
        value: props.data?.unqualifiedWeight ?? 0,
        unit: 'kg',
        decimals: 1,
        subValue: props.data?.unqualifiedRate ?? 0,
        icon: 'ant-design:warning-outlined',
        colorClass: 'orange',
    },
]);
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
