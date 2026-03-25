<template>
  <BasicDrawer
    v-bind="$attrs"
    :title="getTitle"
    width="560px"
    showFooter
    @ok="handleSubmit"
    @register="registerDrawer"
  >
    <BasicForm @register="registerForm" />
  </BasicDrawer>
</template>

<script lang="ts" setup>
import { computed, ref, unref } from 'vue';
import { BasicDrawer, useDrawerInner } from '/@/components/Drawer';
import { BasicForm, useForm } from '/@/components/Form';
import { addReportConfig, updateReportConfig } from '/@/api/lab/reportConfig';
import { getIntermediateDataJudgmentLevelList } from '/@/api/lab/intermediateDataJudgmentLevel';
import { getIntermediateDataFormulaList } from '/@/api/lab/intermediateDataFormula';
import { useMessage } from '/@/hooks/web/useMessage';

const emit = defineEmits(['success', 'register']);
const isUpdate = ref(true);
const currentId = ref('');
const { createMessage } = useMessage();

const formulaOptions = ref<any[]>([]);
const levelOptions = ref<any[]>([]);

async function loadFormulaOptions() {
  try {
    const res: any = await getIntermediateDataFormulaList('JUDGE');
    const formulaList = Array.isArray(res) ? res : res?.data || [];
    const optionMap = new Map<string, { fullName: string; id: string }>();

    formulaList.forEach((item: any) => {
      const value = item?.id;
      const label = item?.displayName || item?.formulaName || item?.columnName;
      if (!value || !label || optionMap.has(value)) {
        return;
      }

      optionMap.set(value, { fullName: label, id: value });
    });

    formulaOptions.value = Array.from(optionMap.values()).sort((a, b) =>
      a.fullName.localeCompare(b.fullName, 'zh-CN'),
    );
  } catch (error) {
    console.error('加载判定列失败', error);
    formulaOptions.value = [];
  }
}

async function loadLevelOptions(formulaId: string) {
  if (!formulaId) {
    levelOptions.value = [];
    return;
  }

  try {
    const res: any = await getIntermediateDataJudgmentLevelList({ formulaId });
    const list = Array.isArray(res) ? res : res?.data || [];
    const levelNameSet = new Set<string>();

    levelOptions.value = list
      .map((item: any) => item?.name)
      .filter((name: string | undefined) => {
        if (!name || levelNameSet.has(name)) {
          return false;
        }

        levelNameSet.add(name);
        return true;
      })
      .map((name: string) => ({
        fullName: name,
        id: name,
      }));
  } catch (error) {
    console.error('加载判定等级失败', error);
    levelOptions.value = [];
  }
}

async function handleFormulaChange(value: string) {
  await loadLevelOptions(value);
  setFieldsValue({ levelNames: [] });
}

const [registerForm, { setFieldsValue, resetFields, validate }] = useForm({
  labelWidth: 110,
  schemas: [
    {
      field: 'name',
      label: '统计名称',
      component: 'Input',
      required: true,
      componentProps: {
        placeholder: '例如：综合合格率、A类占比',
      },
    },
    {
      field: 'sortOrder',
      label: '排序',
      component: 'InputNumber',
      defaultValue: 0,
      componentProps: {
        min: 0,
      },
    },
    {
      field: 'formulaId',
      label: '判定列',
      component: 'Select',
      componentProps: () => ({
        options: formulaOptions.value,
        showSearch: true,
        fieldNames: { label: 'fullName', value: 'id' },
        optionFilterProp: 'fullName',
        placeholder: '请选择判定列',
        onChange: (value: string) => {
          handleFormulaChange(value);
        },
      }),
      required: true,
    },
    {
      field: 'levelNames',
      label: '包含等级',
      component: 'Select',
      componentProps: () => ({
        options: levelOptions.value,
        fieldNames: { label: 'fullName', value: 'id' },
        multiple: true,
        optionFilterProp: 'fullName',
        placeholder: '请选择要统计的等级',
      }),
      required: true,
    },
    {
      field: 'isHeader',
      label: '头部展示',
      component: 'Switch',
      defaultValue: false,
    },
    {
      field: 'isPercentage',
      label: '仅统计占比',
      component: 'Switch',
      defaultValue: false,
    },
    {
      field: 'isShowInReport',
      label: '报表展示',
      component: 'Switch',
      defaultValue: true,
    },
    {
      field: 'isShowRatio',
      label: '显示占比',
      component: 'Switch',
      defaultValue: true,
    },
    {
      field: 'description',
      label: '说明',
      component: 'Textarea',
      componentProps: {
        rows: 3,
        placeholder: '说明统计口径，例如：按首检结果中的 A/B 进行统计',
      },
    },
  ],
  showActionButtonGroup: false,
  actionColOptions: {
    span: 24,
  },
});

const [registerDrawer, { setDrawerProps, closeDrawer }] = useDrawerInner(async (data) => {
  resetFields();
  levelOptions.value = [];
  setDrawerProps({ confirmLoading: false });
  isUpdate.value = !!data?.isUpdate;

  await loadFormulaOptions();

  if (unref(isUpdate)) {
    currentId.value = data.record.id;

    if (data.record.formulaId) {
      await loadLevelOptions(data.record.formulaId);
    }

    setFieldsValue({
      ...data.record,
    });
    return;
  }

  setFieldsValue({
    sortOrder: 0,
    isHeader: false,
    isPercentage: false,
    isShowInReport: true,
    isShowRatio: true,
    levelNames: [],
  });
});

const getTitle = computed(() => (!unref(isUpdate) ? '新增配置' : '编辑配置'));

async function handleSubmit() {
  try {
    const values = await validate();
    setDrawerProps({ confirmLoading: true });

    if (unref(isUpdate)) {
      await updateReportConfig({ ...values, id: unref(currentId) });
    } else {
      await addReportConfig(values);
    }

    closeDrawer();
    emit('success');
    createMessage.success('保存成功');
  } finally {
    setDrawerProps({ confirmLoading: false });
  }
}
</script>
