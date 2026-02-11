<template>
    <span class="condition-cell" v-if="displayGroups.length">
        <template v-for="(group, gIdx) in displayGroups" :key="gIdx">
            <!-- Group Connector -->
            <span v-if="gIdx > 0" class="connector" :class="group.logic === '或' ? 'or' : 'and'">
                {{ group.logic }}
            </span>

            <span class="group-block">
                <!-- 条件组标签（与弹窗一致） -->
                <span v-if="group.items.length || group.subGroups.length" class="group-label">条件组 {{ group.groupLabel }} </span>
                <!-- Items -->
                <template v-for="(item, iIdx) in group.items" :key="'i-' + iIdx">
                    <span v-if="item.connector" class="connector" :class="item.connector === '或' ? 'or' : 'and'">
                        {{ item.connector }}
                    </span>
                    <span class="inline-condition">
                        <span class="field">{{ item.left }}</span>
                        <span class="op">{{ item.op }}</span>
                        <span class="val" v-if="!item.isNoValue">{{ item.right }}</span>
                    </span>
                </template>

                <!-- SubGroups -->
                <template v-for="(sg, sgIdx) in group.subGroups" :key="'sg-' + sgIdx">
                    <span v-if="sg.connector" class="connector" :class="sg.connector === '或' ? 'or' : 'and'">
                        {{ sg.connector }}
                    </span>
                    <span class="sub-group-block">
                        <span class="sub-label">子组 {{ sg.subLabel }}：</span>
                        <template v-for="(subItem, siIdx) in sg.items" :key="'si-' + siIdx">
                            <span v-if="subItem.connector" class="connector"
                                :class="subItem.connector === '或' ? 'or' : 'and'">
                                {{ subItem.connector }}
                            </span>
                            <span class="inline-condition">
                                <span class="field">{{ subItem.left }}</span>
                                <span class="op">{{ subItem.op }}</span>
                                <span class="val" v-if="!subItem.isNoValue">{{ subItem.right }}</span>
                            </span>
                        </template>
                    </span>
                </template>
            </span>
        </template>
    </span>
</template>

<script lang="ts" setup>
import { computed } from 'vue';

const props = defineProps({
    condition: {
        type: String,
        default: ''
    },
    /** 可用列选项，用于将列名显示为中文（与 RulePreviewCard 一致） */
    fieldOptions: {
        type: Array as () => { value: string; label: string }[],
        default: () => []
    },
    /** 特性列表，用于将 CONTAINS_ANY 等右值 ID 显示为中文 */
    featureList: { type: Array as () => any[], default: () => [] },
    /** 特性大类列表 */
    featureCategoryList: { type: Array as () => { id: string; name: string }[], default: () => [] },
    /** 特性等级列表 */
    featureSeverityList: { type: Array as () => { id: string; name: string }[], default: () => [] }
});

/** 将列名/变量名转为中文显示名（与 RulePreviewCard.getFieldName 一致） */
function getFieldDisplayName(fieldValue: string): string {
    const options = props.fieldOptions || [];
    const raw = (fieldValue || '').replace(/[\[\]]/g, '').trim();
    const field = options.find((f: { value: string }) => f.value === raw);
    const label = field ? field.label : raw;
    return label.replace(/\s*\(.*?\)\s*/g, '').trim();
}

/** 将条件右值转为展示文本（特性大类/等级/特性 ID 转中文名称，与 RulePreviewCard.getConditionValue 一致） */
function getConditionValueDisplay(c: { leftExpr: string; operator: string; rightValue: string }): string {
    const op = c.operator || '';
    const isMultiSelect = op === 'CONTAINS_ANY' || op === 'NOT_CONTAINS_ANY';
    if (!isMultiSelect) return c.rightValue ?? '';

    const fieldKey = (c.leftExpr || '').replace(/[\[\]]/g, '').trim();
    const fieldLower = fieldKey.toLowerCase();
    const fieldOption = (props.fieldOptions || []).find((f: { value: string }) => f.value === fieldKey);
    const fieldLabel = (fieldOption?.label ?? '').toLowerCase();

    let ids: string[] = [];
    try {
        const parsed = JSON.parse(c.rightValue);
        ids = Array.isArray(parsed) ? parsed.map((x: any) => String(x)) : [String(parsed)];
    } catch {
        ids = String(c.rightValue ?? '').split(',').map(s => s.trim()).filter(Boolean);
    }
    if (ids.length === 0) return c.rightValue ?? '';

    const categories = props.featureCategoryList || [];
    const severities = props.featureSeverityList || [];
    const features = props.featureList || [];

    const idMatch = (item: { id?: string | number }, idStr: string) => String(item.id) === String(idStr);

    if (fieldLower.includes('category') || fieldLabel.includes('大类') || fieldLabel.includes('category')) {
        if (categories.length > 0) {
            return ids.map(id => {
                const cat = categories.find((x: any) => idMatch(x, id));
                return cat ? cat.name : id;
            }).join('、');
        }
    }
    if (fieldLower.includes('level') || fieldLower.includes('severity') || fieldLabel.includes('等级') || fieldLabel.includes('severity')) {
        if (severities.length > 0) {
            return ids.map(id => {
                const level = severities.find((l: any) => idMatch(l, id));
                return level ? level.name : id;
            }).join('、');
        }
    }
    if (fieldLower.includes('feature') || fieldLabel.includes('特性')) {
        if (features.length > 0) {
            return ids.map(id => {
                const feature = features.find((f: any) => idMatch(f, id));
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

const displayGroups = computed(() => {
    const conditionJson = props.condition;
    if (!conditionJson) return [];
    try {
        const data = JSON.parse(conditionJson);
        if (!data || !Array.isArray(data.groups)) return [];

        const options = props.fieldOptions || [];
        const groups: any[] = [];

        // Helper to process a single condition（字段名与右值均使用中文展示）
        const processCondition = (c: any) => {
            let leftRaw = c.leftExpr || '';
            if (leftRaw.includes('[')) {
                leftRaw = leftRaw.replace(/[\[\]]/g, '');
            }
            const left = options.length > 0 ? getFieldDisplayName(leftRaw) : leftRaw;
            const op = c.operator || '';
            const isNoValue = op === 'IS_NULL' || op === 'NOT_NULL';

            const opMap: Record<string, string> = {
                '=': '=',
                '>': '>',
                '<': '<',
                '>=': '≥',
                '<=': '≤',
                '!=': '≠',
                'CONTAINS': '包含',
                'NOT_CONTAINS': '不包含',
                'CONTAINS_ANY': '包含任意',
                'NOT_CONTAINS_ANY': '不包含任意',
                'IS_NULL': '为空',
                'NOT_NULL': '不为空'
            };
            const displayOp = opMap[op] || op;
            const right = isNoValue ? '' : getConditionValueDisplay(c);

            return {
                left,
                op: displayOp,
                right,
                isNoValue
            };
        };

        data.groups.forEach((group: any, groupIdx: number) => {
            const groupObj = {
                groupLabel: String.fromCharCode(65 + groupIdx),
                logic: group.logic === 'OR' ? '或' : '且',
                items: [] as any[],
                subGroups: [] as any[]
            };

            if (group.conditions) {
                group.conditions.forEach((c: any, idx: number) => {
                    groupObj.items.push({
                        ...processCondition(c),
                        connector: idx > 0 ? (group.logic === 'OR' ? '或' : '且') : null
                    });
                });
            }

            if (group.subGroups) {
                group.subGroups.forEach((sg: any, sgIdx: number) => {
                    const subGroupObj = {
                        subLabel: String.fromCharCode(65 + sgIdx),
                        items: [] as any[],
                        connector: sgIdx > 0 ? (group.logic === 'OR' ? '或' : '且') : null,
                        logic: sg.logic === 'OR' ? '或' : '且'
                    };
                    if (sg.conditions) {
                        sg.conditions.forEach((c: any, cIdx: number) => {
                            subGroupObj.items.push({
                                ...processCondition(c),
                                connector: cIdx > 0 ? (sg.logic === 'OR' ? '或' : '且') : null
                            });
                        });
                    }
                    if (subGroupObj.items.length > 0) {
                        groupObj.subGroups.push(subGroupObj);
                    }
                });
            }

            if (groupObj.items.length > 0 || groupObj.subGroups.length > 0) {
                groups.push(groupObj);
            }
        });

        return groups;
    } catch (e) {
        return [];
    }
});
</script>

<style lang="less" scoped>
.condition-cell {
    /* No specific styles here, inherits inline context */
}

.connector {
    display: inline-block;
    font-size: 11px;
    font-weight: bold;
    padding: 0 4px;
    vertical-align: middle;

    &.and {
        color: #93c5fd;
    }

    &.or {
        color: #fcd34d;
    }
}

.group-block {
    display: inline;
}

.inline-condition {
    display: inline-flex;
    align-items: baseline;
    gap: 4px;
    background: #f1f5f9;
    border-radius: 4px;
    padding: 1px 6px;
    color: #334155;
    font-size: 12px;
    vertical-align: middle;
    border: 1px solid transparent;

    .field {
        font-weight: 500;
        color: #475569;
    }

    .op {
        color: #94a3b8;
        font-weight: bold;
        font-family: Arial, sans-serif;
    }

    .val {
        color: #0f172a;
        font-family: monospace;
        font-weight: 600;
    }
}

.group-label {
    font-size: 11px;
    color: #64748b;
    font-weight: 500;
    margin-right: 2px;
}

.sub-group-block {
    display: inline-flex;
    align-items: center;
    gap: 4px;
    background: #f8fafc;
    border: 1px dashed #e2e8f0;
    border-radius: 4px;
    padding: 2px 4px;
    vertical-align: middle;

    .sub-label {
        font-size: 10px;
        color: #94a3b8;
    }
}
</style>
