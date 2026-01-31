<template>
  <BasicModal
    v-bind="$attrs"
    @register="registerModal"
    :title="getTitle"
    @ok="handleSubmit"
  >
    <a-form :model="formState" :label-col="{ span: 6 }" :wrapper-col="{ span: 16 }">
      <a-form-item label="等级名称" required>
        <a-input v-model:value="formState.name" placeholder="请输入等级名称" />
      </a-form-item>
      <a-form-item label="质量状态">
        <a-radio-group v-model:value="formState.qualityStatus">
          <a-radio value="合格">合格</a-radio>
          <a-radio value="不合格">不合格</a-radio>
          <a-radio value="其他">其他</a-radio>
        </a-radio-group>
      </a-form-item>
      <a-form-item label="展示颜色">
        <div class="flex gap-2">
          <a-input v-model:value="formState.color" type="color" style="width: 50px; padding: 0;" />
          <a-input v-model:value="formState.color" placeholder="#FFFFFF" />
        </div>
      </a-form-item>
      <a-form-item label="是否统计">
        <a-switch v-model:checked="formState.isStatistic" />
      </a-form-item>
      <a-form-item label="是否默认">
        <a-switch v-model:checked="formState.isDefault" />
         <span class="text-xs text-gray-400 ml-2">作为兜底判定条件，每组只能有一个</span>
      </a-form-item>
      <a-form-item label="业务说明">
        <a-textarea v-model:value="formState.description" :rows="3" />
      </a-form-item>
      <a-form-item label="判定条件">
        <a-textarea 
          v-model:value="formState.condition" 
          :rows="5" 
          placeholder='请输入判定条件(JSON格式)，例如：{"groups": [...]}'
        />
        <span class="text-xs text-gray-400">JSON格式存储判定条件公式</span>
      </a-form-item>
    </a-form>
  </BasicModal>
</template>

<script lang="ts" setup>
  import { ref, computed, unref } from 'vue';
  import { BasicModal, useModalInner } from '/@/components/Modal';
  import { useMessage } from '/@/hooks/web/useMessage';
  import {
    createIntermediateDataJudgmentLevel,
    updateIntermediateDataJudgmentLevel,
  } from '/@/api/lab/intermediateDataJudgmentLevel';

  const emit = defineEmits(['success', 'register']);
  const { createMessage } = useMessage();

  const isUpdate = ref(true);
  const formulaId = ref('');
  const rowId = ref('');

  const formState = ref({
    name: '',
    qualityStatus: '其他',
    priority: 0,
    color: '',
    isStatistic: false,
    isDefault: false,
    description: '',
    condition: '',
  });

  const [registerModal, { setModalProps, closeModal }] = useModalInner(async (data) => {
    setModalProps({ confirmLoading: false });
    isUpdate.value = !!data?.isUpdate;
    formulaId.value = data?.formulaId;

    if (isUpdate.value && data.record) {
      rowId.value = data.record.id;
      formState.value = {
        name: data.record.name,
        qualityStatus: data.record.qualityStatus,
        priority: data.record.priority,
        color: data.record.color,
        isStatistic: data.record.isStatistic,
        isDefault: data.record.isDefault,
        description: data.record.description,
        condition: data.record.condition || '',
      };
    } else {
      rowId.value = '';
      formState.value = {
        name: '',
        qualityStatus: '其他',
        priority: 0,
        color: '',
        isStatistic: false,
        isDefault: false,
        description: '',
        condition: '',
      };
    }
  });

  const getTitle = computed(() => (!unref(isUpdate) ? '新增等级' : '编辑等级'));

  const handleSubmit = async () => {
    try {
      if (!formState.value.name) {
        createMessage.warning('请输入等级名称');
        return;
      }

      setModalProps({ confirmLoading: true });
      const values = {
        ...formState.value,
        formulaId: formulaId.value,
      };

      if (unref(isUpdate)) {
        await updateIntermediateDataJudgmentLevel({ ...values, id: rowId.value });
      } else {
        await createIntermediateDataJudgmentLevel(values);
      }

      closeModal();
      emit('success');
    } catch (error: any) {
      console.error(error);
      createMessage.error(error.message || '保存失败');
    } finally {
      setModalProps({ confirmLoading: false });
    }
  };
</script>
