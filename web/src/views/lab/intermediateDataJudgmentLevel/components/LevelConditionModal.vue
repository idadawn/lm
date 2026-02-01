<template>
  <BasicModal v-bind="$attrs" @register="registerModal" :title="modalTitle" @ok="handleSubmit" :width="1200"
    :min-height="600" :show-ok-btn="!isReadOnly">
    <div class="p-4">
      <div v-if="loading" class="text-center py-10">
        <a-spin />
      </div>
      <div v-else>
        <!-- 使用规则卡片 -->
        <RuleCard v-if="currentRule" :rule="currentRule" :rule-index="0" :field-options="fieldOptions"
          :read-only-result="true" :borderless="true" :read-only="isReadOnly" hint="条件组之间为「或」关系，组内条件为「且」关系"
          @update:rule="handleRuleUpdate" @open-formula-editor="handleOpenFormulaEditor" />
      </div>
    </div>

    <FormulaBuilder @register="registerFormulaModal" @save="handleSaveFormula" />
  </BasicModal>
</template>

<script lang="ts" setup>
import { ref } from 'vue';
import { BasicModal, useModalInner, useModal } from '/@/components/Modal';
import { useMessage } from '/@/hooks/web/useMessage';
import { Icon } from '/@/components/Icon';
import RuleCard from '/@/views/lab/intermediateDataFormula/components/RuleCard.vue';
import FormulaBuilder from '/@/views/lab/intermediateDataFormula/components/FormulaBuilder.vue';
import {
  getIntermediateDataJudgmentLevelById,
  updateIntermediateDataJudgmentLevel,
} from '/@/api/lab/intermediateDataJudgmentLevel';
import {
  getAvailableColumns,
} from '/@/api/lab/intermediateDataFormula';
import { AdvancedJudgmentRule } from '/@/views/lab/intermediateDataFormula/components/advancedJudgmentTypes';

const emit = defineEmits(['register', 'success']);
const { createMessage } = useMessage();

const modalTitle = ref('编辑判定条件');
const loading = ref(false);
const levelId = ref('');
const formulaId = ref('');
const levelName = ref('');
const resultValue = ref('');
const availableFields = ref<any[]>([]);
const currentLevel = ref<any>(null);
const currentRule = ref<AdvancedJudgmentRule | null>(null);
const isReadOnly = ref(false);

const fieldOptions = ref<any[]>([]);
const currentFormulaCallback = ref<((formula: string) => void) | null>(null);
const [registerFormulaModal, { openModal: openFormulaModal }] = useModal();

const [registerModal, { setModalProps, closeModal }] = useModalInner(async (data) => {
  setModalProps({ confirmLoading: false });
  levelId.value = data.levelId;
  formulaId.value = data.formulaId;
  levelName.value = data.levelName;
  resultValue.value = data.levelName;
  isReadOnly.value = !!data.readOnly;
  modalTitle.value = isReadOnly.value
    ? `查看判定条件: ${levelName.value}`
    : `编辑判定条件: ${levelName.value}`;

  await loadData();
});

const loadData = async () => {
  loading.value = true;
  try {
    // 1. 获取可用列
    const fieldsRes: any = await getAvailableColumns();
    availableFields.value = fieldsRes.data || fieldsRes || [];
    fieldOptions.value = availableFields.value.map(f => ({
      label: f.displayName ? `${f.displayName} (${f.columnName})` : f.columnName,
      value: f.columnName,
      featureCategories: f.featureCategories || [],
      featureLevels: f.featureLevels || [],
    }));

    // 2. 获取当前判定等级
    const levelRes = await getIntermediateDataJudgmentLevelById(levelId.value);
    console.log('DEBUG levelRes:', levelRes);
    // 处理可能的嵌套结构 (有些API返回 { data: {...} })
    const levelData = levelRes?.data || levelRes;
    currentLevel.value = levelData;

    // 3. 解析condition字段中的规则
    if (levelData && levelData.condition) {
      try {
        const rule = JSON.parse(levelData.condition);
        console.log('DEBUG parsed rule:', rule);
        if (rule && rule.groups) {
          // Force resultValue to match the current level name
          // This fixes the issue where copied levels retain the old name in condition JSON
          rule.resultValue = levelData.name;
          currentRule.value = rule;
        } else {
          createDefaultRule();
        }
      } catch (e) {
        console.error('Condition JSON解析失败', e);
        createDefaultRule();
      }
    } else {
      createDefaultRule();
    }
  } catch (error) {
    console.error(error);
    createMessage.error('加载数据失败');
  } finally {
    loading.value = false;
  }
};

function createDefaultRule() {
  currentRule.value = {
    id: generateId(),
    resultValue: resultValue.value,
    groups: [],
  };
}

const handleRuleUpdate = (rule: AdvancedJudgmentRule) => {
  currentRule.value = rule;
};

const handleOpenFormulaEditor = (data: any) => {
  if (isReadOnly.value) return;

  currentFormulaCallback.value = data.onSave;
  openFormulaModal(true, {
    record: {
      id: data.conditionId,
      formula: data.currentValue,
      formulaName: '自定义条件',
      columnName: '判定条件'
    }
  });
};

const handleSaveFormula = (data: { id: string; formula: string }) => {
  if (currentFormulaCallback.value) {
    currentFormulaCallback.value(data.formula);
    currentFormulaCallback.value = null;
  }
};

const handleSubmit = async () => {
  try {
    setModalProps({ confirmLoading: true });

    if (!currentRule.value) {
      createMessage.error('没有要保存的条件');
      return;
    }

    // 将条件转换为JSON字符串
    const conditionJson = JSON.stringify(currentRule.value);

    // 更新判定等级的condition字段（保留其他原有字段）
    await updateIntermediateDataJudgmentLevel({
      ...currentLevel.value,
      id: levelId.value,
      formulaId: formulaId.value,
      name: currentLevel.value?.name || levelName.value,
      condition: conditionJson,
    });

    createMessage.success('保存成功');
    closeModal();
    emit('success');
  } catch (error) {
    console.error(error);
    createMessage.error('保存失败');
  } finally {
    setModalProps({ confirmLoading: false });
  }
};

function generateId() {
  return Date.now().toString(36) + Math.random().toString(36).substr(2);
}
</script>
