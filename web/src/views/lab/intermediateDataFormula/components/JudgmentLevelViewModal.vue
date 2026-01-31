<template>
  <BasicModal
    v-bind="$attrs"
    @register="registerModal"
    title="判定等级预览"
    :showOkBtn="false"
    cancelText="关闭"
    width="900px"
    :minHeight="650"
  >
    <div class="judgment-view-container">
      <!-- 加载中 -->
      <div v-if="loading" class="loading-wrapper">
        <a-spin />
      </div>

      <!-- 有数据 -->
      <div v-else-if="sortedLevels.length > 0" class="levels-wrapper">
        <!-- 可滚动的非默认等级列表 -->
        <div class="scrollable-levels">
          <div
            v-for="(level, index) in normalLevels"
            :key="level.id"
            class="level-card"
          >
            <div class="level-content">
              <div class="level-left">
                <div class="level-order">{{ index + 1 }}</div>
                <div
                  class="level-color-dot"
                  :style="{ backgroundColor: level.color || '#cbd5e1' }"
                ></div>
                <span class="level-name">{{ level.name }}</span>
                <span v-if="level.qualityStatus" class="quality-badge">{{ level.qualityStatus }}</span>
              </div>

              <div class="level-right">
                <a-tag v-if="hasCondition(level)" color="blue">{{ getConditionCount(level) }}个条件</a-tag>
                <a-tag v-else color="default">无条件</a-tag>
                <a-button type="link" size="small" @click="handleViewCondition(level)">
                  查看条件
                </a-button>
              </div>
            </div>
          </div>
        </div>

        <!-- 固定在底部的默认等级 -->
        <div v-if="defaultLevel" class="fixed-default-section">
          <div class="level-card is-default">
            <div class="level-content">
              <div class="level-left">
                <div class="level-order is-default">
                  <svg xmlns="http://www.w3.org/2000/svg" width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round">
                    <circle cx="12" cy="12" r="10"></circle>
                    <line x1="12" y1="16" x2="12" y2="12"></line>
                    <line x1="12" y1="8" x2="12.01" y2="8"></line>
                  </svg>
                </div>
                <div
                  class="level-color-dot"
                  :style="{ backgroundColor: defaultLevel.color || '#94a3b8' }"
                ></div>
                <span class="level-name">{{ defaultLevel.name }}</span>
                <a-tag color="warning">兜底默认</a-tag>
                <span v-if="defaultLevel.qualityStatus" class="quality-badge">{{ defaultLevel.qualityStatus }}</span>
              </div>

              <div class="level-right">
                <span class="default-hint">当所有条件都不满足时使用</span>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- 无数据 -->
      <div v-else class="empty-state">
        <div class="empty-icon">📋</div>
        <p class="empty-title">暂无判定等级</p>
        <p class="empty-desc">请先在判定等级管理中配置</p>
      </div>
    </div>
    <!-- 条件查看弹窗 -->
    <LevelConditionModal @register="registerConditionModal" />
  </BasicModal>
</template>

<script lang="ts" setup>
import { ref, computed } from 'vue';
import { BasicModal, useModalInner, useModal } from '/@/components/Modal';
import { useMessage } from '/@/hooks/web/useMessage';
import { getIntermediateDataJudgmentLevelList } from '/@/api/lab/intermediateDataJudgmentLevel';
import LevelConditionModal from '/@/views/lab/intermediateDataJudgmentLevel/components/LevelConditionModal.vue';

defineEmits(['register']);
const { createMessage } = useMessage();

const loading = ref(false);
const formulaId = ref('');
const levels = ref<any[]>([]);

// 条件查看弹窗
const [registerConditionModal, { openModal: openConditionModal }] = useModal();

// 排序后的等级列表
const sortedLevels = computed(() => levels.value);

// 非默认等级
const normalLevels = computed(() => levels.value.filter(l => !l.isDefault));

// 默认等级
const defaultLevel = computed(() => levels.value.find(l => l.isDefault));

const [registerModal, { setModalProps }] = useModalInner(async (data) => {
  setModalProps({ confirmLoading: false });
  formulaId.value = data.formulaId;
  levels.value = [];

  await loadData();
});

const loadData = async () => {
  loading.value = true;
  try {
    const levelsRes: any = await getIntermediateDataJudgmentLevelList({ formulaId: formulaId.value });
    levels.value = Array.isArray(levelsRes) ? levelsRes : (levelsRes.data || []);
  } catch (error) {
    console.error(error);
    createMessage.error('加载数据失败');
  } finally {
    loading.value = false;
  }
};

const hasCondition = (level: any): boolean => {
  if (!level.condition) return false;
  try {
    const parsed = JSON.parse(level.condition);
    return parsed && parsed.groups && parsed.groups.length > 0;
  } catch {
    return false;
  }
};

// 获取条件数量
const getConditionCount = (level: any): number => {
  try {
    const parsed = JSON.parse(level.condition);
    if (!parsed || !parsed.groups) return 0;
    return parsed.groups.reduce((sum: number, g: any) => sum + (g.conditions?.length || 0), 0);
  } catch {
    return 0;
  }
};

// 查看条件 - 打开条件查看弹窗
const handleViewCondition = (level: any) => {
  openConditionModal(true, {
    levelId: level.id,
    formulaId: formulaId.value,
    levelName: level.name,
    readOnly: true,
  });
};
</script>

<style scoped lang="less">
.judgment-view-container {
  padding: 16px 16px 8px 16px;
  background: #f8fafc;
  height: 100%;
  min-height: 600px;
  display: flex;
  flex-direction: column;
  overflow: visible;
}

.loading-wrapper {
  display: flex;
  justify-content: center;
  align-items: center;
  height: 200px;
}

.levels-wrapper {
  display: flex;
  flex-direction: column;
  flex: 1;
  min-height: 0; 
  /* overflow: hidden; Removed to support sticky footer relative to modal body scroll if needed */
}

.scrollable-levels {
  flex: 1;
  overflow-y: auto;
  display: flex;
  flex-direction: column;
  gap: 8px;
  padding-right: 4px;
  min-height: 0;
}

/* 固定底部的默认等级区域 - 不参与滚动 */
.fixed-default-section {
  position: sticky;
  bottom: 0;
  z-index: 10;
  background-color: #f8fafc;
  flex-shrink: 0;
  margin-top: 8px;
  padding-top: 8px;
  padding-bottom: 8px; /* add some padding at bottom for better look */
  border-top: 1px dashed #cbd5e1;
}

.level-card {
  background: #fff;
  border-radius: 8px;
  border: 1px solid #e2e8f0;
  transition: background-color 0.15s ease;

  &:hover {
    background: #f8fafc;
  }

  &.is-default {
    background: linear-gradient(135deg, #fffbeb 0%, #fef3c7 100%);
    border: 1px solid #fcd34d;
  }
}

.level-content {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 12px 16px;
  gap: 16px;
}

.level-left {
  display: flex;
  align-items: center;
  gap: 10px;
  flex: 1;
  min-width: 0;
}

.level-right {
  display: flex;
  align-items: center;
  gap: 8px;
  flex-shrink: 0;
}

.level-order {
  width: 26px;
  height: 26px;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 12px;
  font-weight: 600;
  color: #fff;
  background: linear-gradient(135deg, #3b82f6 0%, #2563eb 100%);
  border-radius: 6px;
  flex-shrink: 0;

  &.is-default {
    background: linear-gradient(135deg, #f59e0b 0%, #d97706 100%);
  }
}

.level-color-dot {
  width: 12px;
  height: 12px;
  border-radius: 50%;
  flex-shrink: 0;
  border: 2px solid rgba(255, 255, 255, 0.8);
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.15);
}

.level-name {
  font-size: 14px;
  font-weight: 600;
  color: #1e293b;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.quality-badge {
  font-size: 11px;
  padding: 2px 6px;
  background: #f1f5f9;
  color: #475569;
  border-radius: 4px;
  white-space: nowrap;
}

.default-hint {
  font-size: 12px;
  color: #92400e;
  font-style: italic;
}

.empty-state {
  text-align: center;
  padding: 48px 24px;

  .empty-icon {
    font-size: 48px;
    margin-bottom: 16px;
    opacity: 0.6;
  }

  .empty-title {
    font-size: 16px;
    color: #475569;
    margin-bottom: 4px;
  }

  .empty-desc {
    font-size: 13px;
    color: #94a3b8;
  }
}
</style>
