<template>
    <BasicModal v-bind="$attrs" @register="registerModal" title="批量复制等级" @ok="handleSubmit" :width="600">
        <div class="p-4">
            <a-alert type="info" show-icon class="mb-4">
                <template #message>
                    将当前判定项目 <strong>{{ sourceFormulaName }}</strong> 下的
                    <strong v-if="sourceProductSpecName">({{ sourceProductSpecName }})</strong>
                    <strong>{{ sourceLevelCount }}</strong> 个等级复制到目标项目中。
                </template>
            </a-alert>

            <a-form layout="vertical">
                <a-form-item label="选择目标判定项目" required>
                    <a-select v-model:value="targetFormulaIds" mode="multiple" placeholder="请选择要复制到的目标判定项目"
                        :filter-option="filterOption" show-search allow-clear style="width: 100%">
                        <a-select-option v-for="item in availableFormulas" :key="item.id" :value="item.id"
                            :filterText="item.displayName || item.formulaName">
                            {{ item.displayName || item.formulaName }}
                        </a-select-option>
                    </a-select>
                </a-form-item>

                <a-form-item label="选择目标产品规格(可选)">
                    <a-select v-model:value="targetProductSpecIds" mode="multiple" placeholder="留空则保持原产品规格，选择后将复制到指定规格下"
                        :filter-option="filterOption" show-search allow-clear style="width: 100%">
                        <a-select-option v-for="item in productSpecOptions" :key="item.id" :value="item.id"
                            :filterText="item.name">
                            {{ item.name }}
                        </a-select-option>
                    </a-select>
                    <div class="text-xs text-gray-400 mt-1">
                        如果选择了目标规格，系统会将等级复制到每个选中的规格下（笛卡尔积）。如果不选，则保持源等级的规格归属。
                    </div>
                </a-form-item>

                <a-form-item>
                    <a-checkbox v-model:checked="overwriteExisting">
                        覆盖已存在的同名等级
                    </a-checkbox>
                    <div class="text-xs text-gray-400 mt-1">
                        勾选后，目标位置（判定项目+产品规格）已有的同名等级将被覆盖其条件和属性；不勾选则跳过。
                    </div>
                </a-form-item>

                <a-alert v-if="overwriteExisting" type="warning" show-icon class="mt-2">
                    <template #message>
                        注意：覆盖模式会替换目标等级的判定条件、质量状态、颜色等属性，此操作不可撤销！
                    </template>
                </a-alert>
            </a-form>
        </div>
    </BasicModal>
</template>

<script lang="ts" setup>
import { ref, computed } from 'vue';
import { BasicModal, useModalInner } from '/@/components/Modal';
import { useMessage } from '/@/hooks/web/useMessage';
import { batchCopyLevels } from '/@/api/lab/intermediateDataJudgmentLevel';

const emit = defineEmits(['register', 'success']);
const { createMessage, createConfirm } = useMessage();

const sourceFormulaId = ref('');
const sourceFormulaName = ref('');
const sourceProductSpecId = ref('');
const sourceProductSpecName = ref('');
const sourceLevelCount = ref(0);
const allFormulas = ref<any[]>([]);
const productSpecList = ref<any[]>([]);

const targetFormulaIds = ref<string[]>([]);
const targetProductSpecIds = ref<string[]>([]);
const overwriteExisting = ref(false);

// 判定项目列表按 id 去重
const availableFormulas = computed(() => {
    const list = allFormulas.value || [];
    const seen = new Set<string>();
    return list.filter((item: any) => {
        const id = item?.id;
        if (id == null || seen.has(id)) return false;
        seen.add(id);
        return true;
    });
});

// 产品规格列表按 id 去重
const productSpecOptions = computed(() => {
    const list = productSpecList.value || [];
    const seen = new Set<string>();
    return list.filter((item: any) => {
        const id = item?.id;
        if (id == null || seen.has(id)) return false;
        seen.add(id);
        return true;
    });
});

const filterOption = (input: string, option: any) => {
    const text = option.filterText || '';
    return text.toLowerCase().includes(input.toLowerCase());
};

const [registerModal, { setModalProps, closeModal }] = useModalInner((data) => {
    setModalProps({ confirmLoading: false });
    sourceFormulaId.value = data.formulaId;
    sourceFormulaName.value = data.formulaName;
    sourceProductSpecId.value = data.productSpecId || '';
    sourceProductSpecName.value = data.productSpecName || ''; // 需要从外部传入或在列表里查找
    sourceLevelCount.value = data.levelCount || 0;
    allFormulas.value = data.allFormulas || [];
    productSpecList.value = data.productSpecList || [];

    targetFormulaIds.value = [];
    targetProductSpecIds.value = [];
    overwriteExisting.value = false;
});

const handleSubmit = async () => {
    if (!targetFormulaIds.value.length) {
        createMessage.warning('请至少选择一个目标判定项目');
        return;
    }

    const doSubmit = async () => {
        try {
            setModalProps({ confirmLoading: true });
            const res: any = await batchCopyLevels({
                sourceFormulaId: sourceFormulaId.value,
                targetFormulaIds: [...new Set(targetFormulaIds.value)],
                overwriteExisting: overwriteExisting.value,
                sourceProductSpecId: sourceProductSpecId.value,
                targetProductSpecIds: [...new Set(targetProductSpecIds.value)],
            });

            const data = res?.data || res || {};
            const msg = `复制完成！新增 ${data.copiedCount || 0} 个，覆盖 ${data.overwrittenCount || 0} 个，跳过 ${data.skippedCount || 0} 个`;
            createMessage.success(msg);
            closeModal();
            emit('success');
        } catch (error) {
            console.error(error);
            createMessage.error('批量复制失败');
        } finally {
            setModalProps({ confirmLoading: false });
        }
    };

    if (overwriteExisting.value) {
        createConfirm({
            iconType: 'warning',
            title: '确认覆盖',
            content: `您选择了覆盖模式，目标位置已有的同名等级将被覆盖。确定继续？`,
            onOk: doSubmit,
        });
    } else {
        await doSubmit();
    }
};
</script>
