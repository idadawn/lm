<template>
  <BasicModal v-bind="$attrs" @register="registerModal" :title="getTitle" @ok="handleSubmit">
    <a-form :model="formState" :label-col="{ span: 6 }" :wrapper-col="{ span: 16 }">
      <a-form-item label="产品规格">
        <a-select v-model:value="formState.productSpecId" placeholder="请选择产品规格" allowClear show-search
          :filter-option="filterOption">
          <a-select-option v-for="item in productSpecList" :key="item.id" :value="item.id">
            {{ item.name }}
          </a-select-option>
        </a-select>
      </a-form-item>
      <a-form-item label="等级名称" required>
        <a-input v-model:value="formState.name" placeholder="请输入等级名称" />
      </a-form-item>
      <a-form-item label="是否默认">
        <a-switch v-model:checked="formState.isDefault" />
        <span class="text-xs text-gray-400 ml-2">作为兜底判定条件，每组只能有一个</span>
      </a-form-item>
      <a-form-item label="业务说明">
        <a-textarea v-model:value="formState.description" :rows="3" />
      </a-form-item>

    </a-form>
  </BasicModal>
</template>

<script lang="ts" setup>
import { ref, computed, unref, onMounted } from 'vue';
import { BasicModal, useModalInner } from '/@/components/Modal';
import { useMessage } from '/@/hooks/web/useMessage';
import {
  createIntermediateDataJudgmentLevel,
  updateIntermediateDataJudgmentLevel,
} from '/@/api/lab/intermediateDataJudgmentLevel';
import { getProductSpecList } from '/@/api/lab/productSpec';

const emit = defineEmits(['success', 'register']);
const { createMessage } = useMessage();

const isUpdate = ref(true);
const formulaId = ref('');
const rowId = ref('');

const formState = ref({
  name: '',
  qualityStatus: 2,
  priority: 0,
  color: '',
  isStatistic: false,
  isDefault: false,
  description: '',
  condition: '',
  productSpecId: '',
});

const productSpecList = ref<any[]>([]);

const filterOption = (input: string, option: any) => {
  return option.children?.[0]?.children?.toLowerCase()?.includes(input.toLowerCase());
};

const loadProductSpecList = async () => {
  try {
    const res = await getProductSpecList({});
    productSpecList.value = res.list || [];
  } catch (error) {
    console.error('加载产品规格失败', error);
  }
};

onMounted(() => {
  loadProductSpecList();
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
      productSpecId: data.record.productSpecId || '',
    };
  } else {
    rowId.value = '';
    formState.value = {
      name: '',
      qualityStatus: 2,
      priority: 0,
      color: '',
      isStatistic: false,
      isDefault: false,
      description: '',
      condition: '',
      productSpecId: data?.productSpecId || '',
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

    // Process condition if it exists (e.g. from copy)
    let finalCondition = formState.value.condition;
    if (finalCondition) {
      try {
        const ruleObj = JSON.parse(finalCondition);
        if (ruleObj && typeof ruleObj === 'object') {
          // Update result value to match the new name
          ruleObj.resultValue = formState.value.name;
          finalCondition = JSON.stringify(ruleObj);
        }
      } catch (e) {
        console.warn('Failed to parse/update condition JSON on submit', e);
        // keep original if parse fails
      }
    }

    const values = {
      ...formState.value,
      condition: finalCondition,
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
