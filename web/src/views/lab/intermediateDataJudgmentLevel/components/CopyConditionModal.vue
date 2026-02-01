<template>
    <BasicModal v-bind="$attrs" @register="registerModal" title="拷贝判定条件" @ok="handleSubmit" :width="500">
        <div class="p-4">
            <a-form layout="vertical">
                <a-form-item label="拷贝方式">
                    <a-radio-group v-model:value="copyMode">
                        <a-radio value="new">新建等级</a-radio>
                        <a-radio value="existing">覆盖现有等级</a-radio>
                    </a-radio-group>
                </a-form-item>

                <template v-if="copyMode === 'new'">
                    <div class="p-4 bg-gray-50 rounded text-gray-500 mb-4">
                        选择此选项将跳转到“新建等级”页面，并自动填充源等级的判定条件和属性。
                    </div>
                </template>

                <template v-else>
                    <a-form-item label="选择目标等级" required>
                        <a-select v-model:value="targetLevelId" placeholder="请选择要覆盖的目标等级">
                            <a-select-option v-for="level in availableLevels" :key="level.id" :value="level.id">
                                {{ level.name }} ({{ level.code }})
                            </a-select-option>
                        </a-select>
                        <div class="text-red-500 text-xs mt-1" v-if="targetLevelId">
                            注意：目标等级原有的判定条件将被完全覆盖！
                        </div>
                    </a-form-item>
                </template>
            </a-form>
        </div>
    </BasicModal>
</template>

<script lang="ts" setup>
import { ref, computed } from 'vue';
import { BasicModal, useModalInner } from '/@/components/Modal';
import { useMessage } from '/@/hooks/web/useMessage';
import {
    getIntermediateDataJudgmentLevelById,
    updateIntermediateDataJudgmentLevel,
} from '/@/api/lab/intermediateDataJudgmentLevel';

const emit = defineEmits(['register', 'success', 'clone']);
const { createMessage } = useMessage();

const copyMode = ref<'new' | 'existing'>('new');
const sourceLevelId = ref('');
const formulaId = ref('');
const allLevels = ref<any[]>([]);

const newLevelCode = ref('');
const newLevelName = ref('');
const newLevelPriority = ref(0);
const targetLevelId = ref(undefined);

// 过滤掉源等级自己
const availableLevels = computed(() => {
    return allLevels.value.filter(l => l.id !== sourceLevelId.value && !l.isDefault);
});

const [registerModal, { setModalProps, closeModal }] = useModalInner((data) => {
    setModalProps({ confirmLoading: false });
    copyMode.value = 'new';
    sourceLevelId.value = data.sourceLevel.id;
    formulaId.value = data.formulaId;
    allLevels.value = data.allLevels || [];

    // 预填充新等级信息 (基于源等级)
    newLevelCode.value = `${data.sourceLevel.code}_COPY`;
    newLevelName.value = `${data.sourceLevel.name} (拷贝)`;
    newLevelPriority.value = (data.sourceLevel.priority || 0) + 1;
    targetLevelId.value = undefined;
});

const handleSubmit = async () => {
    try {
        setModalProps({ confirmLoading: true });

        // 1. 获取源等级的完整数据（包含condition JSON）
        const sourceRes = await getIntermediateDataJudgmentLevelById(sourceLevelId.value);
        const sourceData = sourceRes?.data || sourceRes;

        if (!sourceData || !sourceData.condition) {
            createMessage.warning('源等级没有判定条件数据');
            // 即使没有条件，也可以拷贝属性
        }

        const sourceCondition = sourceData?.condition;

        if (copyMode.value === 'new') {
            // 模式：新建等级
            // 直接关闭拷贝弹窗，并触发父组件打开“新建等级”弹窗，同时把源数据传过去
            closeModal();
            emit('clone', { ...sourceData, id: null });
            return;
        } else {
            if (!targetLevelId.value) {
                createMessage.error('请选择目标等级');
                return;
            }

            // 获取目标等级现有数据
            const targetRes = await getIntermediateDataJudgmentLevelById(targetLevelId.value);
            const targetData = targetRes?.data || targetRes;

            if (!targetData) {
                createMessage.error('目标等级数据加载失败');
                return;
            }

            // 更新目标等级的条件
            await updateIntermediateDataJudgmentLevel({
                ...targetData,
                condition: sourceCondition, // 核心：覆盖条件
            });
        }

        createMessage.success('拷贝成功');
        closeModal();
        emit('success');
    } catch (error) {
        console.error(error);
        createMessage.error('拷贝失败');
    } finally {
        setModalProps({ confirmLoading: false });
    }
};
</script>
