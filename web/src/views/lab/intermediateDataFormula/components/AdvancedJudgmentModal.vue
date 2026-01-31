<template>
  <BasicModal
    v-bind="$attrs"
    @register="registerModal"
    title="高级判定规则配置"
    @ok="handleSubmit"
    width="1200px"
    :minHeight="800"
  >
    <div class="p-2" v-loading="loading">
      <AdvancedJudgmentEditor
        v-if="!loading"
        v-model:value="formulaValue"
        v-model:defaultValue="defaultValue"
        :fields="availableFields"
        :levels="levels"
      />
    </div>
  </BasicModal>
</template>

<script lang="ts" setup>
  import { ref } from 'vue';
  import { BasicModal, useModalInner } from '/@/components/Modal';
  import { useMessage } from '/@/hooks/web/useMessage';
  import AdvancedJudgmentEditor from './AdvancedJudgmentEditor.vue';
  import {
    getIntermediateDataFormula,
    updateIntermediateDataFormula,
    getAvailableColumns,
  } from '/@/api/lab/intermediateDataFormula';
  import { getIntermediateDataJudgmentLevelList } from '/@/api/lab/intermediateDataJudgmentLevel';
  import type { IntermediateDataFormula } from '/@/api/lab/types/intermediateDataFormula';

  const emit = defineEmits(['register', 'success']);
  const { createMessage } = useMessage();

  const loading = ref(false);
  const formulaId = ref('');
  const formulaRecord = ref<IntermediateDataFormula | null>(null);
  
  const formulaValue = ref('');
  const defaultValue = ref('');
  const availableFields = ref<any[]>([]);
  const levels = ref<any[]>([]);

  const [registerModal, { setModalProps, closeModal }] = useModalInner(async (data) => {
    setModalProps({ confirmLoading: false });
    formulaId.value = data.formulaId;
    formulaValue.value = '';
    defaultValue.value = '';
    levels.value = [];
    
    await loadData();
  });

  const loadData = async () => {
    loading.value = true;
    try {
      // 1. Load available fields
      const fieldsRes: any = await getAvailableColumns();
      // Ensure we pass the array directly
      availableFields.value = Array.isArray(fieldsRes) ? fieldsRes : (fieldsRes.data || []);

      // 2. Load levels
      const levelsRes: any = await getIntermediateDataJudgmentLevelList({ formulaId: formulaId.value });
      levels.value = Array.isArray(levelsRes) ? levelsRes : (levelsRes.data || []);

      // 3. Load formula details
      const formulaRes = await getIntermediateDataFormula(formulaId.value);
      formulaRecord.value = formulaRes;
      
      formulaValue.value = formulaRes.formula || '';
      defaultValue.value = formulaRes.defaultValue || '';

    } catch (error) {
      console.error(error);
      createMessage.error('加载数据失败');
    } finally {
      loading.value = false;
    }
  };

  const handleSubmit = async () => {
    if (!formulaRecord.value) return;

    try {
      setModalProps({ confirmLoading: true });
      
      // Update formula and defaultValue
      // We need to pass the other required fields from the record
      const params = {
        tableName: formulaRecord.value.tableName,
        columnName: formulaRecord.value.columnName,
        formulaName: formulaRecord.value.formulaName,
        formulaType: formulaRecord.value.formulaType,
        formula: formulaValue.value,
        defaultValue: defaultValue.value,
        // Optional fields
        formulaLanguage: formulaRecord.value.formulaLanguage,
        unitId: formulaRecord.value.unitId,
        precision: formulaRecord.value.precision,
        isEnabled: formulaRecord.value.isEnabled,
        sortOrder: formulaRecord.value.sortOrder,
        remark: formulaRecord.value.remark,
      };

      await updateIntermediateDataFormula(formulaId.value, params);
      
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
</script>
