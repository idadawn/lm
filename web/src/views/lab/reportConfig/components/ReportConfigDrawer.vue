<template>
    <BasicDrawer v-bind="$attrs" @register="registerDrawer" :title="getTitle" width="500px" showFooter
        @ok="handleSubmit">
        <BasicForm @register="registerForm" />
    </BasicDrawer>
</template>

<script lang="ts" setup>
import { ref, computed, unref } from 'vue';
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

// 公式选项和等级选项
const formulaOptions = ref<any[]>([]);
const levelOptions = ref<any[]>([]);
const selectedFormulaId = ref<string>('');

// 加载判定公式列表
async function loadFormulaOptions() {
    try {
        const res: any = await getIntermediateDataFormulaList();
        const list = Array.isArray(res) ? res : (res.data || []);
        formulaOptions.value = list.map((item: any) => ({
            id: item.id,
            fullName: item.formulaName || item.columnName,
        }));
    } catch (e) {
        console.error('加载判定公式失败', e);
    }
}

// 根据公式ID加载等级列表
async function loadLevelOptions(formulaId: string) {
    if (!formulaId) {
        levelOptions.value = [];
        return;
    }
    try {
        const res: any = await getIntermediateDataJudgmentLevelList({ FormulaId: formulaId });
        const list = Array.isArray(res) ? res : (res.data || []);
        levelOptions.value = list.map((item: any) => ({
            id: item.name,
            fullName: item.name,
        }));
    } catch (e) {
        console.error('加载判定等级失败', e);
        levelOptions.value = [];
    }
}

// 监听公式选择变化
async function handleFormulaChange(val: string) {
    selectedFormulaId.value = val;
    await loadLevelOptions(val);
    // 重置已选等级
    setFieldsValue({ levelNames: [] });
}

const [registerForm, { setFieldsValue, resetFields, validate }] = useForm({
    labelWidth: 100,
    schemas: [
        {
            field: 'name',
            label: '统计名称',
            component: 'Input',
            required: true,
            componentProps: {
                placeholder: '例如：A类占比',
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
            label: '参考判定',
            component: 'Select',
            componentProps: () => {
                return {
                    options: formulaOptions.value,
                    placeholder: '请选择判定公式',
                    onChange: (val: any) => {
                        handleFormulaChange(val);
                    },
                };
            },
            required: false,
        },
        {
            field: 'levelNames',
            label: '包含等级',
            component: 'Select',
            componentProps: () => {
                return {
                    options: levelOptions.value,
                    multiple: true,
                    placeholder: '请先选择参考判定',
                };
            },
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
            label: '是否百分比',
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
            label: '描述',
            component: 'Textarea',
        },
    ],
    showActionButtonGroup: false,
    actionColOptions: {
        span: 24,
    },
});

const [registerDrawer, { setDrawerProps, closeDrawer }] = useDrawerInner(async (data) => {
    resetFields();
    setDrawerProps({ confirmLoading: false });
    isUpdate.value = !!data?.isUpdate;

    // 加载公式列表
    await loadFormulaOptions();

    if (unref(isUpdate)) {
        currentId.value = data.record.id;

        // 如果有公式ID，加载对应的等级选项
        if (data.record.formulaId) {
            selectedFormulaId.value = data.record.formulaId;
            await loadLevelOptions(data.record.formulaId);
        }

        setFieldsValue({
            ...data.record,
        });
    }
});

const getTitle = computed(() => (!unref(isUpdate) ? '新增配置' : '编辑配置'));

async function handleSubmit() {
    try {
        const values = await validate();
        setDrawerProps({ confirmLoading: true });

        // 提交所有字段，包括 formulaId
        const submitValues = values;

        if (unref(isUpdate)) {
            await updateReportConfig({ ...submitValues, id: unref(currentId) });
        } else {
            await addReportConfig(submitValues);
        }

        closeDrawer();
        emit('success');
        createMessage.success('保存成功');
    } finally {
        setDrawerProps({ confirmLoading: false });
    }
}
</script>
