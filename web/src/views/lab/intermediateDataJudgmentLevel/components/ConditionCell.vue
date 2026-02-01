<template>
    <span class="condition-cell" v-if="displayGroups.length">
        <template v-for="(group, gIdx) in displayGroups" :key="gIdx">
            <!-- Group Connector -->
            <span v-if="gIdx > 0" class="connector" :class="group.logic === '或' ? 'or' : 'and'">
                {{ group.logic }}
            </span>

            <span class="group-block">
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
                        <span class="sub-label">子组</span>
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
    }
});

const displayGroups = computed(() => {
    const conditionJson = props.condition;
    if (!conditionJson) return [];
    try {
        const data = JSON.parse(conditionJson);
        if (!data || !Array.isArray(data.groups)) return [];

        const groups: any[] = [];

        // Helper to process a single condition
        const processCondition = (c: any) => {
            let left = c.leftExpr || '';
            if (left.includes('[')) { // Remove brackets for display if simple
                left = left.replace(/[\[\]]/g, '');
            }
            const op = c.operator || '';
            let right = c.rightValue || '';
            if (right === '[]') right = '';

            // Translate common operators
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

            return {
                left,
                op: displayOp,
                right,
                isNoValue: op === 'IS_NULL' || op === 'NOT_NULL'
            };
        };

        data.groups.forEach((group: any) => {
            const groupObj = {
                logic: group.logic === 'OR' ? '或' : '且',
                items: [] as any[],
                subGroups: [] as any[]
            };

            // Simple conditions
            if (group.conditions) {
                group.conditions.forEach((c: any, idx: number) => {
                    groupObj.items.push({
                        ...processCondition(c),
                        connector: idx > 0 ? (group.logic === 'OR' ? '或' : '且') : null
                    });
                });
            }

            // SubGroups
            if (group.subGroups) {
                group.subGroups.forEach((sg: any, sgIdx: number) => {
                    const subGroupObj = {
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
