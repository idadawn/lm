<template>
  <div class="flex flex-col gap-6 p-2 max-w-4xl mx-auto h-full overflow-y-auto pb-20">
    <!-- Intro -->
    <div class="bg-blue-50 border border-blue-200 rounded-lg p-4 mb-2 flex gap-3">
      <div class="bg-blue-100 p-2 rounded-full h-fit text-blue-600">
        <Icon icon="ant-design:branches-outlined" :size="20" />
      </div>
      <div>
        <h3 class="font-bold text-blue-900">判定逻辑配置</h3>
        <p class="text-sm text-blue-700 mt-1">
          系统将按照<span class="font-bold">从上到下</span>
          的顺序执行规则。一旦满足某条规则的条件，即返回对应的结果，并停止后续判断。
        </p>
      </div>
    </div>

    <!-- Rule List -->
    <div v-for="(rule, ruleIdx) in rules" :key="rule.id" class="relative pl-8">
      <!-- Visual Flow Line -->
      <div class="absolute left-3 top-0 bottom-0 w-0.5 bg-slate-200"></div>
      <div
        class="absolute left-0 top-6 w-6 h-6 bg-slate-100 border-2 border-slate-300 text-slate-500 rounded-full flex items-center justify-center text-xs font-bold z-10">
        {{ ruleIdx + 1 }}
      </div>

      <div class="bg-white rounded-xl border border-slate-200 shadow-sm overflow-hidden">
        <!-- Rule Header / Result -->
        <div class="bg-slate-50 px-4 py-3 border-b border-slate-200 flex items-center justify-between">
          <div class="flex items-center gap-3">
            <span class="text-sm font-medium text-slate-500">如果满足以下条件，则返回:</span>
            <div class="flex items-center gap-2">
              <Icon icon="ant-design:arrow-right-outlined" :size="16" class="text-slate-400" />
              <input v-model="rule.resultValue"
                class="w-40 font-bold text-blue-700 border-b border-blue-300 bg-transparent focus:outline-none focus:border-blue-600 px-1"
                placeholder="例如: A" @change="emitChange" />
            </div>
          </div>
          <button @click="removeRule(ruleIdx)" class="text-xs text-red-500 hover:text-red-700 hover:underline">
            删除规则
          </button>
        </div>

        <!-- Conditions Area -->
        <div class="p-4 bg-slate-50/50">
          <div class="bg-white border border-slate-200 rounded-lg p-3">
            <!-- Global Logic Toggle (Currently only support Root Group Logic) -->
            <div class="flex items-center gap-2 mb-3">
              <span class="text-xs font-bold text-slate-500 uppercase">当</span>
              <div class="inline-flex bg-slate-100 p-0.5 rounded-md border border-slate-200">
                <button @click="updateGroupLogic(ruleIdx, 'AND')"
                  :class="['px-3 py-0.5 text-xs rounded font-medium transition-all', rule.rootGroup.logic === 'AND' ? 'bg-white shadow-sm text-blue-600' : 'text-slate-500 hover:text-slate-700']">
                  满足所有 (AND)
                </button>
                <button @click="updateGroupLogic(ruleIdx, 'OR')"
                  :class="['px-3 py-0.5 text-xs rounded font-medium transition-all', rule.rootGroup.logic === 'OR' ? 'bg-white shadow-sm text-orange-600' : 'text-slate-500 hover:text-slate-700']">
                  满足任一 (OR)
                </button>
              </div>
              <span class="text-xs font-bold text-slate-500 uppercase">条件时:</span>
            </div>

            <!-- Conditions List -->
            <div class="space-y-2 relative">
              <div v-if="rule.rootGroup.conditions.length > 0"
                :class="['absolute top-2 bottom-2 left-[-10px] w-1.5 rounded-l', rule.rootGroup.logic === 'AND' ? 'bg-blue-200' : 'bg-orange-200']">
              </div>

              <div v-for="(condition, cIdx) in rule.rootGroup.conditions" :key="condition.id"
                class="flex items-center gap-2 mb-2 bg-white p-2 rounded border border-slate-200 shadow-sm hover:border-blue-300 transition-colors">
                <!-- Field Select -->
                <div class="w-1/3 min-w-[120px]">
                  <Select v-model:value="condition.fieldId" size="small" class="w-full !text-xs" :options="props.fields"
                    :field-names="{ label: 'name', value: 'id' }" @change="emitChange" />
                </div>

                <!-- Operator Select -->
                <div class="w-[100px] shrink-0">
                  <Select v-model:value="condition.operator" size="small" class="w-full !text-xs"
                    :options="RULE_OPERATORS" @change="emitChange" />
                </div>

                <!-- Value Input -->
                <div class="flex-1">
                  <Input v-if="!['IS_NULL', 'NOT_NULL'].includes(condition.operator)" v-model:value="condition.value"
                    size="small" placeholder="比较值" @change="emitChange" />
                  <span v-else
                    class="text-xs text-slate-400 italic bg-slate-50 px-2 py-1 rounded block text-center border">
                    无需输入值
                  </span>
                </div>

                <button @click="removeCondition(ruleIdx, cIdx)"
                  class="p-1.5 text-red-500 hover:bg-red-50 rounded transition-colors">
                  <Icon icon="ant-design:delete-outlined" :size="14" />
                </button>
              </div>

              <div v-if="rule.rootGroup.conditions.length === 0"
                class="text-center py-4 text-slate-400 text-sm border border-dashed rounded bg-slate-50">
                暂无条件，请添加
              </div>
            </div>

            <div class="mt-3">
              <button @click="addCondition(ruleIdx)"
                class="w-full py-1 text-xs border border-dashed border-slate-300 text-slate-500 rounded hover:bg-slate-50 hover:border-blue-300 hover:text-blue-500 flex items-center justify-center gap-1 transition-colors">
                <Icon icon="ant-design:plus-outlined" :size="12" /> 添加条件
              </button>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- Add Rule Button -->
    <div class="pl-8 relative">
      <div class="absolute left-3 top-0 bottom-0 w-0.5 bg-slate-200"></div>
      <button @click="addRule"
        class="w-full h-12 border-2 border-dashed border-slate-300 text-slate-500 rounded-lg hover:border-blue-400 hover:text-blue-600 hover:bg-blue-50 transition-all flex items-center justify-center font-medium">
        <Icon icon="ant-design:plus-outlined" :size="16" class="mr-2" /> 添加优先级规则
      </button>
    </div>

    <!-- Default Fallback -->
    <div class="pl-8 relative mt-4">
      <div class="absolute left-3 top-0 h-8 w-0.5 bg-slate-200"></div>
      <div
        class="absolute left-0 top-2 w-6 h-6 bg-slate-800 text-white rounded-full flex items-center justify-center text-xs font-bold z-10">
        终
      </div>

      <div class="bg-slate-800 text-white rounded-xl shadow-lg p-5 flex items-center justify-between">
        <div>
          <h4 class="font-bold flex items-center gap-2">
            <Icon icon="ant-design:check-circle-outlined" :size="20" class="text-green-400" />
            默认结果 (Else)
          </h4>
          <p class="text-slate-300 text-xs mt-1">
            如果以上所有规则都不满足，则返回此值。
          </p>
        </div>
        <div class="flex items-center gap-3">
          <span class="text-sm font-medium text-slate-300">返回:</span>
          <input :value="defaultValueLocal" @input="updateDefaultValue"
            class="w-32 bg-slate-700 border border-slate-600 text-white font-bold px-2 py-1 rounded focus:outline-none focus:border-slate-500" />
        </div>
      </div>
    </div>
  </div>
</template>

<script lang="ts" setup>
import { ref, watch, PropType } from 'vue';
import { Icon } from '/@/components/Icon';
import { Select, Input } from 'ant-design-vue';
import { RULE_OPERATORS, JudgmentRule } from './types';
import { buildShortUUID } from '/@/utils/uuid';

const props = defineProps({
  value: { type: String, default: '' }, // JSON string of rules
  defaultValue: { type: String, default: '' },
  fields: { type: Array as PropType<any[]>, default: () => [] }
});

const emit = defineEmits(['update:value', 'update:defaultValue', 'change']);

const rules = ref<JudgmentRule[]>([]);
const defaultValueLocal = ref('');

// Init from props
watch(
  () => props.value,
  (val) => {
    try {
      if (val) {
        const parsed = JSON.parse(val);
        if (Array.isArray(parsed)) {
          rules.value = parsed;
        } else {
          rules.value = [];
        }
      } else {
        rules.value = [];
      }
    } catch (e) {
      console.error('Failed to parse rules JSON', e);
      rules.value = [];
    }
  },
  { immediate: true }
);

watch(
  () => props.defaultValue,
  (val) => {
    defaultValueLocal.value = val;
  },
  { immediate: true }
);

function emitChange() {
  const json = JSON.stringify(rules.value);
  emit('update:value', json);
  emit('change', json);
}

function updateDefaultValue(e: any) {
  const val = e.target.value;
  defaultValueLocal.value = val;
  emit('update:defaultValue', val);
}

// --- Actions ---

function addRule() {
  rules.value.push({
    id: buildShortUUID(),
    resultValue: '',
    rootGroup: {
      id: buildShortUUID(),
      logic: 'AND',
      conditions: [],
    },
  });
  emitChange();
}

function removeRule(index: number) {
  rules.value.splice(index, 1);
  emitChange();
}

function updateGroupLogic(ruleIndex: number, logic: 'AND' | 'OR') {
  if (rules.value[ruleIndex]?.rootGroup) {
    rules.value[ruleIndex].rootGroup.logic = logic;
    emitChange();
  }
}

function addCondition(ruleIndex: number) {
  const rule = rules.value[ruleIndex];
  if (!rule) return;

  // Try to find a default field to select
  const defaultFieldId = props.fields.length > 0 ? props.fields[0].id : '';

  rule.rootGroup.conditions.push({
    id: buildShortUUID(),
    fieldId: defaultFieldId,
    operator: '=',
    value: '',
  });
  emitChange();
}

function removeCondition(ruleIndex: number, conditionIndex: number) {
  const rule = rules.value[ruleIndex];
  if (rule?.rootGroup?.conditions) {
    rule.rootGroup.conditions.splice(conditionIndex, 1);
    emitChange();
  }
}
</script>

<style scoped>
/* Tailwind-like utilities if unocss/windicss is configured, else inline styles mostly used above */
</style>
