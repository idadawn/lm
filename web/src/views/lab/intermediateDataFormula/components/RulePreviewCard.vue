<template>
    <div class="rule-preview-card">
        <div v-if="localRule.groups && localRule.groups.length > 0" class="groups-list">
            <template v-for="(group, groupIdx) in localRule.groups" :key="group.id">
                <!-- 组间连接线 (AND) -->
                <div v-if="groupIdx > 0" class="group-connector">
                    <span class="connector-label">且</span>
                </div>

                <div class="preview-group-item">
                    <!-- 左侧：组标题 -->
                    <div class="group-left">
                        <div class="group-title">条件组 {{ getGroupLabel(groupIdx) }}</div>
                    </div>

                    <!-- 右侧：紧凑内容区域 -->
                    <div class="group-right">
                        <!-- 简单模式 -->
                        <div v-if="group.mode === 'simple'" class="conditions-inline-container">
                            <div v-if="!group.conditions || group.conditions.length === 0" class="empty-text">(无条件)
                            </div>
                            <template v-else>
                                <span v-for="(condition, cIdx) in group.conditions" :key="condition.id"
                                    class="inline-item-wrapper">
                                    <!-- 连接符 -->
                                    <span v-if="cIdx > 0" class="inline-connector" :class="group.logic?.toLowerCase()">
                                        {{ group.logic === 'OR' ? '或' : '且' }}
                                    </span>
                                    <!-- 条件本体 -->
                                    <span class="inline-condition">
                                        <span class="field">{{ getFieldName(condition.leftExpr) }}</span>
                                        <span class="op">{{ getOperatorLabel(condition.operator) }}</span>
                                        <span class="val" :title="getConditionValue(condition)">{{
                                            getConditionValue(condition) }}</span>
                                    </span>
                                </span>
                            </template>
                        </div>

                        <!-- 嵌套模式 -->
                        <div v-else class="nested-container">
                            <template v-for="(subGroup, subIdx) in group.subGroups" :key="subGroup.id">
                                <!-- 子组连接符 -->
                                <div v-if="subIdx > 0" class="sub-group-connector-line">
                                    <span class="connector-text" :class="group.logic?.toLowerCase()">
                                        {{ group.logic === 'OR' ? '或' : '且' }}
                                    </span>
                                </div>

                                <!-- 子组块 -->
                                <div class="sub-group-inline-block">
                                    <strong class="sub-group-label">子组 {{ String.fromCharCode(65 + subIdx) }}：</strong>
                                    <span class="sub-conditions-inline">
                                        <div v-if="!subGroup.conditions || subGroup.conditions.length === 0"
                                            class="empty-text">(无条件)</div>
                                        <template v-else>
                                            <span v-for="(subCond, scIdx) in subGroup.conditions" :key="subCond.id"
                                                class="inline-item-wrapper">
                                                <span v-if="scIdx > 0" class="inline-connector"
                                                    :class="subGroup.logic?.toLowerCase()">
                                                    {{ subGroup.logic === 'OR' ? '或' : '且' }}
                                                </span>
                                                <span class="inline-condition">
                                                    <span class="field">{{ getFieldName(subCond.leftExpr) }}</span>
                                                    <span class="op">{{ getOperatorLabel(subCond.operator) }}</span>
                                                    <span class="val" :title="getConditionValue(subCond)">{{
                                                        getConditionValue(subCond) }}</span>
                                                </span>
                                            </span>
                                        </template>
                                    </span>
                                </div>
                            </template>
                            <div v-if="!group.subGroups || group.subGroups.length === 0" class="empty-text">(无子组)</div>
                        </div>
                    </div>
                </div>
            </template>
        </div>

        <div v-else class="empty-state">
            <span class="text-gray-400">无条件配置</span>
        </div>
    </div>
</template>

<script lang="ts" setup>
import { ref, PropType, watch } from 'vue';
import { AdvancedJudgmentRule } from './advancedJudgmentTypes';

const props = defineProps({
    rule: { type: Object as PropType<AdvancedJudgmentRule>, required: true },
    fieldOptions: { type: Array as PropType<any[]>, default: () => [] },
    featureList: { type: Array as PropType<any[]>, default: () => [] },
    featureCategoryList: { type: Array as PropType<any[]>, default: () => [] },
    featureSeverityList: { type: Array as PropType<any[]>, default: () => [] },
});

const localRule = ref<AdvancedJudgmentRule>({ ...props.rule });

watch(() => props.rule, (newRule) => {
    localRule.value = { ...newRule };
}, { deep: true });

function getGroupLabel(index: number) {
    return String.fromCharCode(65 + index);
}

function getFieldName(fieldValue: string) {
    const field = props.fieldOptions.find(f => f.value === fieldValue);
    let label = field ? field.label : fieldValue;
    return label.replace(/\s*\(.*?\)\s*/g, '').trim();
}

function getOperatorLabel(op: string) {
    const map: Record<string, string> = {
        '=': '=',
        '>': '>',
        '>=': '≥',
        '<': '<',
        '<=': '≤',
        '!=': '≠',
        'IS_NULL': '为空',
        'NOT_NULL': '不为空',
        'CONTAINS': '包含',
        'NOT_CONTAINS': '不包含',
        'CONTAINS_ANY': '包含任意',
        'NOT_CONTAINS_ANY': '不包含任意'
    };
    return map[op] || op;
}

function getConditionValue(condition: any) {
    const op = condition.operator;
    const isMultiSelect = op === 'CONTAINS_ANY' || op === 'NOT_CONTAINS_ANY';

    if (!isMultiSelect) return condition.rightValue;

    const fieldKey = condition.leftExpr;
    const fieldLower = fieldKey.toLowerCase();
    // 也检查字段的显示标签
    const fieldOption = props.fieldOptions.find(f => f.value === fieldKey);
    const fieldLabel = fieldOption?.label?.toLowerCase() || '';

    // 解析IDs - 兼容JSON数组和单个ID字符串
    let ids: string[] = [];
    try {
        const parsed = JSON.parse(condition.rightValue);
        ids = Array.isArray(parsed) ? parsed : [String(parsed)];
    } catch {
        // 不是JSON，可能是单个ID或逗号分隔的字符串
        ids = String(condition.rightValue).split(',').map(s => s.trim()).filter(Boolean);
    }

    if (ids.length === 0) return condition.rightValue;

    // DEBUG: 追踪匹配逻辑
    if (fieldLower.includes('category') || fieldLower.includes('level') || fieldLower.includes('feature')) {

    }

    // 1. 特性大类 (appearanceCategory / 特性大类)
    if (fieldLower.includes('category') || fieldLabel.includes('大类') || fieldLabel.includes('category')) {
        if (props.featureCategoryList.length > 0) {
            const result = ids.map(id => {
                const cat = props.featureCategoryList.find(c => c.id === id);

                return cat ? cat.name : id;
            }).join('、');
            return result;
        }
    }

    // 2. 特性等级 (appearanceSeverity / level / 特性等级)
    else if (fieldLower.includes('level') || fieldLower.includes('severity') || fieldLabel.includes('等级') || fieldLabel.includes('severity')) {
        if (props.featureSeverityList.length > 0) {
            const result = ids.map(id => {
                const level = props.featureSeverityList.find(l => l.id === id);

                return level ? level.name : id;
            }).join('、');
            return result;
        }
    }

    // 3. 具体特性 (feature / 特性)
    else if (fieldLower.includes('feature') || fieldLabel.includes('特性')) {
        if (props.featureList.length > 0) {
            return ids.map(id => {
                const feature = props.featureList.find(f => f.id === id);
                if (feature) {
                    const category = feature.rootCategory || feature.category || '';
                    const severity = feature.severityLevel || '';
                    return `[${category} ${feature.name} ${severity}]`.trim().replace(/\s+/g, ' ');
                }
                return id;
            }).join('、');
        }
    }

    return ids.join('、');
}
</script>

<style lang="less" scoped>
.rule-preview-card {
    font-size: 13px;
    width: 100%;
}

.groups-list {
    display: flex;
    flex-direction: column;
    gap: 6px;
    /* 进一步减小默认间距 */
}

.group-connector {
    display: flex;
    align-items: center;
    justify-content: center;
    position: relative;
    height: 12px;
    /* 更紧凑的连接线 */
    margin: 2px 0;

    &::before {
        content: '';
        position: absolute;
        left: 40px;
        right: 40px;
        top: 50%;
        height: 1px;
        border-top: 1px dashed #e2e8f0;
        /* 更淡的线条 */
        z-index: 1;
    }

    .connector-label {
        position: relative;
        z-index: 2;
        background: #f1f5f9;
        color: #64748b;
        font-size: 11px;
        padding: 0 8px;
        border-radius: 10px;
        font-weight: 500;
    }
}

.preview-group-item {
    display: flex;
    background: white;
    border: 1px solid #e2e8f0;
    border-radius: 6px;
    overflow: hidden;
    align-items: stretch;
    transition: all 0.2s;

    &:hover {
        border-color: #cbd5e1;
        box-shadow: 0 1px 2px rgba(0, 0, 0, 0.03);
    }
}

/* 左侧：组信息 */
.group-left {
    width: 70px;
    /* 极致紧凑 */
    background: #f8fafc;
    border-right: 1px solid #eef2f6;
    display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: center;
    padding: 4px;
    /* 减小内边距 */
    flex-shrink: 0;
}

.group-title {
    font-weight: 600;
    color: #64748b;
    font-size: 12px;
    text-align: center;
    line-height: 1.2;
}

/* 右侧：紧凑内容 */
.group-right {
    flex: 1;
    padding: 8px 10px;
    /* 减小内边距 */
    background: #fff;
    min-width: 0;
    display: flex;
    flex-direction: column;
    justify-content: center;
}

.conditions-inline-container {
    display: flex;
    flex-wrap: wrap;
    align-items: center;
    gap: 6px;
    /* 条件间的间距 */
    line-height: 1.5;
}

.nested-container {
    display: flex;
    flex-direction: column;
    gap: 4px;
}

.sub-group-inline-block {
    display: flex;
    flex-wrap: wrap;
    align-items: center;
    gap: 4px;
    background: #fcfcfc;
    border-radius: 4px;
    padding: 2px 4px;
}

.sub-group-label {
    color: #94a3b8;
    margin-right: 2px;
    font-size: 11px;
    font-weight: 500;
}

.sub-conditions-inline {
    display: contents;
}

/* 内联元素样式 */
.inline-item-wrapper {
    display: inline-flex;
    align-items: center;
    gap: 6px;
}

.inline-connector {
    font-weight: bold;
    padding: 0 2px;
    border-radius: 3px;
    font-size: 11px;
    /* 略微减小连接符字号 */

    &.and {
        color: #3b82f6;
    }

    &.or {
        color: #f59e0b;
    }
}

.inline-condition {
    display: inline-flex;
    align-items: baseline;
    gap: 4px;
    background: #f1f5f9;
    /* 稍微深一点的背景增加对比 */
    border: 1px solid transparent;
    /* 移除默认边框，更现代 */
    border-radius: 4px;
    padding: 1px 6px;
    color: #334155;
    font-size: 12px;
    transition: background 0.2s;

    &:hover {
        background: #e2e8f0;
    }

    .field {
        font-weight: 500;
        color: #475569;
    }

    .op {
        color: #94a3b8;
        font-weight: bold;
        /* 符号加粗 */
        font-family: Arial, Helvetica, sans-serif;
        /* 确保符号显示清晰 */
    }

    .val {
        color: #0f172a;
        font-family: ui-monospace, SFMono-Regular, Menlo, Monaco, Consolas, monospace;
        font-weight: 600;
    }
}

.sub-group-connector-line {
    display: flex;
    align-items: center;
    padding-left: 10px;
    height: 10px;

    .connector-text {
        font-size: 11px;
        font-weight: bold;
        color: #cbd5e1;

        &.and {
            color: #93c5fd;
        }

        &.or {
            color: #fcd34d;
        }
    }
}

.empty-text {
    color: #cbd5e1;
    font-style: italic;
    font-size: 12px;
}

.empty-state {
    padding: 12px;
    text-align: center;
}
</style>
