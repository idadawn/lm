<template>
  <a-modal
    v-model:open="visible"
    title="人工匹配外观特性"
    :width="800"
    :confirm-loading="confirmLoading"
    @ok="handleConfirm"
    @cancel="handleCancel">
    <div class="feature-match-dialog">
      <!-- 输入文本显示 -->
      <div class="input-section">
        <div class="label">输入文本：</div>
        <div class="input-text">{{ inputText }}</div>
      </div>

      <!-- 自动匹配结果（如果有） -->
      <div v-if="autoMatchResults.length > 0" class="auto-match-section">
        <div class="section-title">
          <span>自动匹配结果</span>
          <a-tag color="blue">{{ autoMatchResults.length }} 个候选</a-tag>
        </div>
        <div class="match-results">
          <div
            v-for="(result, index) in autoMatchResults"
            :key="result.id || index"
            class="match-item"
            :class="{ 'selected': selectedMatchId === result.id }"
            @click="handleSelectMatch(result)">
            <div class="match-header">
              <a-radio :checked="selectedMatchId === result.id" />
              <div class="match-info">
                <div class="match-info-item">
                  <span class="label">特征大类:</span>
                  <span class="value">{{ result.rootCategory || result.category || '-' }}</span>
                </div>
                <div class="match-info-item">
                  <span class="label">特性分类:</span>
                  <span class="value">{{ result.categoryPath || '-' }}</span>
                </div>
                <div class="match-info-item">
                  <span class="label">特征名称:</span>
                  <span class="value">{{ result.name || '-' }}</span>
                </div>
                <div class="match-info-item">
                  <span class="label">特性等级:</span>
                  <span class="value level-badge">{{ result.severityLevel || '默认' }}</span>
                </div>
                <div class="match-info-item">
                  <span class="label">匹配方式:</span>
                  <a-tag v-if="result.matchMethod === 'keyword'" color="green" size="small">关键字匹配</a-tag>
                  <a-tag v-else-if="result.matchMethod === 'ai'" color="orange" size="small">AI匹配</a-tag>
                  <a-tag v-else-if="result.matchMethod === 'name'" color="blue" size="small">名称匹配</a-tag>
                  <a-tag v-else-if="result.matchMethod === 'fuzzy'" color="purple" size="small">模糊匹配</a-tag>
                  <span v-else class="value">{{ getMatchMethodText(result.matchMethod) }}</span>
                </div>
              </div>
            </div>
            <div v-if="result.description" class="match-description">{{ result.description }}</div>
            <div v-if="result.requiresSeverityConfirmation && result.suggestedSeverity" class="severity-confirmation">
              <a-alert
                :message="`建议等级：${result.suggestedSeverity}（需要确认）`"
                type="warning"
                show-icon
                style="margin-top: 8px">
                <template #action>
                  <a-button
                    type="link"
                    size="small"
                    @click.stop="handleAddKeywordToFeature(result, result.suggestedSeverity)">
                    添加到关键字列表
                  </a-button>
                </template>
              </a-alert>
            </div>
            <div v-if="result.variantList && result.variantList.length > 0" class="match-variants">
              <a-tag
                v-for="(variant, vIdx) in result.variantList"
                :key="vIdx"
                size="small"
                color="purple">
                {{ variant.name }}{{ variant.severity ? ` (${variant.severity})` : '' }}
              </a-tag>
            </div>
          </div>
        </div>
      </div>

      <!-- 手动选择 -->
      <div class="manual-select-section">
        <div class="section-title">
          <span>手动选择</span>
          <a-button type="link" size="small" @click="showManualSelect = !showManualSelect">
            {{ showManualSelect ? '收起' : '展开' }}
          </a-button>
        </div>
        <div v-if="showManualSelect" class="manual-select-content">
          <a-select
            v-model:value="selectedCategory"
            placeholder="选择大类"
            style="width: 200px; margin-bottom: 12px"
            @change="handleCategoryChange">
            <a-select-option v-for="cat in categoryOptions" :key="cat" :value="cat">
              {{ cat }}
            </a-select-option>
          </a-select>
          <a-select
            v-model:value="manualSelectedFeatureId"
            placeholder="选择特性"
            style="width: 100%"
            show-search
            :filter-option="filterFeatureOption">
            <a-select-option
              v-for="feature in filteredFeatures"
              :key="feature.id"
              :value="feature.id">
              <div>
                <span class="feature-option-name">{{ feature.name }}</span>
                <span v-if="feature.description" class="feature-option-desc">
                  - {{ feature.description }}
                </span>
              </div>
            </a-select-option>
          </a-select>
        </div>
      </div>

      <!-- 创建新特性 -->
      <div class="create-new-section">
        <div class="section-title">
          <span>创建新特性</span>
          <a-button type="link" size="small" @click="showCreateNew = !showCreateNew">
            {{ showCreateNew ? '收起' : '展开' }}
          </a-button>
        </div>
        <div v-if="showCreateNew" class="create-new-content">
          <a-alert
            message="如果现有特性都不匹配，可以创建新特性。已删除的特性名称可重新使用。"
            type="info"
            show-icon
            style="margin-bottom: 12px" />
          <a-form :model="newFeatureForm" layout="vertical">
            <a-form-item label="特性大类" required>
              <a-tree-select
                v-model:value="newFeatureForm.categoryId"
                placeholder="请选择特性大类"
                allow-clear
                show-search
                tree-default-expand-all
                :tree-data="categoryTreeData"
                :field-names="{ label: 'name', value: 'id' }"
                style="width: 100%"
              />
            </a-form-item>
            <a-form-item label="特性名称" required>
              <a-input
                v-model:value="newFeatureForm.name"
                placeholder="例如: 脆" />
            </a-form-item>
            <a-form-item label="特性等级" required>
              <a-select
                v-model:value="newFeatureForm.severityLevelId"
                placeholder="请选择特性等级"
                allow-clear
                show-search
                :options="severityLevelOptions"
                :field-names="{ label: 'fullName', value: 'id' }"
                style="width: 100%"
              />
            </a-form-item>
            <a-form-item label="描述">
              <a-textarea
                v-model:value="newFeatureForm.description"
                placeholder="例如: 容易碎裂..."
                :rows="2" />
            </a-form-item>
          </a-form>
        </div>
      </div>

      <!-- 匹配模式选择 -->
      <div class="match-mode-section">
        <a-radio-group v-model:value="matchMode" @change="handleMatchModeChange">
          <a-radio value="auto">使用自动匹配结果</a-radio>
          <a-radio value="manual">使用手动选择</a-radio>
          <a-radio value="create">创建新特性</a-radio>
        </a-radio-group>
      </div>
    </div>
  </a-modal>
</template>

<script lang="ts" setup>
  import { ref, computed, watch } from 'vue';
  import { useMessage } from '/@/hooks/web/useMessage';
  import { AppearanceFeatureInfo, getAppearanceFeatureList, createAppearanceFeature, addKeywordToFeature } from '/@/api/lab/appearance';
  import { getAllAppearanceFeatureCategories, type AppearanceFeatureCategoryInfo } from '/@/api/lab/appearanceCategory';
  import { getEnabledSeverityLevels, type SeverityLevelInfo } from '/@/api/lab/severityLevel';

  const emit = defineEmits(['confirm', 'cancel', 'keyword-added']);

  const { createMessage } = useMessage();

  // Props
  const props = defineProps<{
    inputText: string;
    autoMatchResults?: AppearanceFeatureInfo[];
  }>();

  // State
  const visible = ref(false);
  const confirmLoading = ref(false);
  const selectedMatchId = ref<string>('');
  const showManualSelect = ref(false);
  const showCreateNew = ref(false);
  const selectedCategory = ref<string>('');
  const manualSelectedFeatureId = ref<string>('');
  const matchMode = ref<'auto' | 'manual' | 'create'>('auto');

  // 所有特性列表
  const allFeatures = ref<AppearanceFeatureInfo[]>([]);
  const categoryOptions = ref<string[]>([]);

  // 特性大类树（创建新特性用）
  const categories = ref<AppearanceFeatureCategoryInfo[]>([]);
  const categoryTreeData = computed(() => convertToTreeData(categories.value));

  function convertToTreeData(cats: AppearanceFeatureCategoryInfo[]): any[] {
    if (!cats?.length) return [];
    const convertNode = (node: AppearanceFeatureCategoryInfo): any => {
      const treeNode: any = { id: node.id, name: node.name };
      if (node.children?.length) treeNode.children = node.children.map(convertNode);
      return treeNode;
    };
    return cats.map(convertNode);
  }

  // 特性等级选项（创建新特性用）
  const severityLevels = ref<SeverityLevelInfo[]>([]);
  const severityLevelOptions = computed(() =>
    severityLevels.value.map((level) => ({
      fullName: level.description ? `${level.name} (${level.description})` : level.name,
      id: level.id,
    }))
  );

  // 新特性表单（必须传 categoryId、severityLevelId，后端不对比已删除记录）
  const newFeatureForm = ref({
    categoryId: '',
    name: '',
    severityLevelId: '',
    description: '',
  });

  // 过滤后的特性列表（根据选择的大类）
  const filteredFeatures = computed(() => {
    if (!selectedCategory.value) {
      return allFeatures.value;
    }
    return allFeatures.value.filter(f => f.category === selectedCategory.value);
  });

  // 初始化
  const init = async () => {
    try {
      const [featuresRes, categoriesRes, levelsRes]: any[] = await Promise.all([
        getAppearanceFeatureList({ pageSize: 1000 }),
        getAllAppearanceFeatureCategories(),
        getEnabledSeverityLevels(),
      ]);

      let features: AppearanceFeatureInfo[] = [];
      if (Array.isArray(featuresRes)) features = featuresRes;
      else if (featuresRes?.data && Array.isArray(featuresRes.data)) features = featuresRes.data;
      else if (featuresRes?.list && Array.isArray(featuresRes.list)) features = featuresRes.list;
      allFeatures.value = features;

      const catSet = new Set<string>();
      allFeatures.value.forEach((f) => { if (f.category) catSet.add(f.category); });
      categoryOptions.value = Array.from(catSet).sort();

      categories.value = Array.isArray(categoriesRes) ? categoriesRes : categoriesRes?.data ?? [];
      severityLevels.value = Array.isArray(levelsRes) ? levelsRes : levelsRes?.data ?? levelsRes?.list ?? [];
    } catch (error) {
      console.error('加载数据失败:', error);
    }

    if (props.autoMatchResults && props.autoMatchResults.length > 0) {
      selectedMatchId.value = props.autoMatchResults[0].id;
      matchMode.value = 'auto';
    } else {
      matchMode.value = 'manual';
      showManualSelect.value = true;
    }
  };

  // 打开对话框
  const open = () => {
    visible.value = true;
    init();
  };

  // 关闭对话框
  const close = () => {
    visible.value = false;
    // 重置状态
    selectedMatchId.value = '';
    manualSelectedFeatureId.value = '';
    selectedCategory.value = '';
    showManualSelect.value = false;
    showCreateNew.value = false;
    newFeatureForm.value = {
      categoryId: '',
      name: '',
      severityLevelId: '',
      description: '',
    };
  };

  // 选择自动匹配结果
  const handleSelectMatch = (result: AppearanceFeatureInfo) => {
    selectedMatchId.value = result.id;
    matchMode.value = 'auto';
  };

  // 大类变化
  const handleCategoryChange = () => {
    manualSelectedFeatureId.value = '';
  };

  // 匹配模式变化
  const handleMatchModeChange = () => {
    if (matchMode.value === 'manual') {
      showManualSelect.value = true;
      showCreateNew.value = false;
    } else if (matchMode.value === 'create') {
      showCreateNew.value = true;
      showManualSelect.value = false;
    }
  };

  // 特性选项过滤
  const filterFeatureOption = (input: string, option: any) => {
    const text = option.children?.[0]?.children || '';
    return text.toLowerCase().includes(input.toLowerCase());
  };

  // 获取匹配方式显示文本
  const getMatchMethodText = (method: string | undefined): string => {
    const methodMap: Record<string, string> = {
      name: '特性名称精确匹配',
      keyword: '关键字匹配',
      variant: '变体名称匹配',
      ai: 'AI分析',
      fuzzy: '文本模糊匹配',
    };
    return methodMap[method || ''] || method || '未知';
  };

  // 将关键字添加到特性记录
  const handleAddKeywordToFeature = async (feature: AppearanceFeatureInfo, keyword: string) => {
    if (!feature?.id || !keyword) {
      createMessage.warning('参数不完整');
      return;
    }

    try {
      await addKeywordToFeature({
        FeatureId: feature.id,
        Keyword: keyword,
      });
      createMessage.success(`已将"${keyword}"添加到特性"${feature.name}"的关键字列表`);
      
      // 更新匹配结果，移除确认提示（通过重新加载匹配结果）
      // 注意：这里不直接修改props，而是通知父组件刷新
      emit('keyword-added', { featureId: feature.id, keyword });
    } catch (error: any) {
      console.error('添加关键字失败:', error);
      const errorMsg = error?.response?.data?.msg || error?.message || '添加关键字失败';
      createMessage.error(errorMsg);
    }
  };

  // 确认
  const handleConfirm = async () => {
    try {
      confirmLoading.value = true;

      let result: AppearanceFeatureInfo | null = null;

      if (matchMode.value === 'auto') {
        // 使用自动匹配结果
        if (selectedMatchId.value) {
          result = props.autoMatchResults?.find(r => r.id === selectedMatchId.value) || null;
        }
      } else if (matchMode.value === 'manual') {
        // 使用手动选择
        if (manualSelectedFeatureId.value) {
          result = allFeatures.value.find(f => f.id === manualSelectedFeatureId.value) || null;
        } else {
          createMessage.warning('请选择特性');
          return;
        }
      } else if (matchMode.value === 'create') {
        // 创建新特性（必须选择特性大类和特性等级，已删除的特性名称可重复使用）
        const form = newFeatureForm.value;
        if (!form.categoryId?.trim()) {
          createMessage.warning('请选择特性大类');
          return;
        }
        if (!form.name?.trim()) {
          createMessage.warning('请填写特性名称');
          return;
        }
        if (!form.severityLevelId?.trim()) {
          createMessage.warning('请选择特性等级');
          return;
        }

        const created = await createAppearanceFeature({
          categoryId: form.categoryId,
          name: form.name.trim(),
          severityLevelId: form.severityLevelId,
          description: form.description?.trim() || undefined,
        });

        // 重新加载特性列表
        await init();

        // 找到刚创建的特性
        if (created?.data?.id) {
          result = allFeatures.value.find(f => f.id === created.data.id) || null;
        }
      }

      if (result) {
        emit('confirm', {
          feature: result,
          inputText: props.inputText,
          matchMode: matchMode.value,
        });
        close();
      } else {
        createMessage.error('未找到匹配的特性');
      }
    } catch (error: any) {
      console.error('确认匹配失败:', error);
      const errorMsg = error?.response?.data?.msg || error?.message || '操作失败';
      createMessage.error(errorMsg);
    } finally {
      confirmLoading.value = false;
    }
  };

  // 取消
  const handleCancel = () => {
    emit('cancel');
    close();
  };

  // 监听自动匹配结果变化
  watch(
    () => props.autoMatchResults,
    (newResults) => {
      if (newResults && newResults.length > 0 && visible.value) {
        selectedMatchId.value = newResults[0].id;
        matchMode.value = 'auto';
      }
    },
    { immediate: true }
  );

  // 暴露方法
  defineExpose({
    open,
    close,
  });
</script>

<style scoped lang="less">
  .feature-match-dialog {
    .input-section {
      margin-bottom: 20px;
      padding: 12px;
      background: #f5f5f5;
      border-radius: 4px;

      .label {
        font-weight: 500;
        margin-bottom: 8px;
        color: #666;
      }

      .input-text {
        font-size: 14px;
        color: #333;
        word-break: break-all;
      }
    }

    .auto-match-section,
    .manual-select-section,
    .create-new-section {
      margin-bottom: 20px;
      padding: 16px;
      border: 1px solid #e8e8e8;
      border-radius: 4px;

      .section-title {
        display: flex;
        justify-content: space-between;
        align-items: center;
        margin-bottom: 12px;
        font-weight: 500;
      }
    }

    .match-results {
      display: flex;
      flex-direction: column;
      gap: 8px;

      .match-item {
        padding: 12px;
        border: 1px solid #d9d9d9;
        border-radius: 4px;
        cursor: pointer;
        transition: all 0.2s;

        &:hover {
          border-color: #1890ff;
          background: #f0f7ff;
        }

        &.selected {
          border-color: #1890ff;
          background: #e6f7ff;
        }

        .match-header {
          display: flex;
          align-items: flex-start;
          gap: 12px;
          margin-bottom: 8px;

          .match-info {
            flex: 1;
            display: flex;
            flex-direction: column;
            gap: 6px;

            .match-info-item {
              display: flex;
              align-items: center;
              gap: 8px;
              font-size: 13px;

              .label {
                color: #666;
                font-weight: 500;
                min-width: 80px;
              }

              .value {
                color: #333;
                font-weight: 500;

                &.level-badge {
                  display: inline-block;
                  padding: 2px 8px;
                  background: #ff9800;
                  color: white;
                  border-radius: 2px;
                  font-size: 12px;
                }
              }
            }
          }
        }

        .severity-confirmation {
          margin-top: 8px;
        }

        .match-description {
          font-size: 12px;
          color: #666;
          margin-bottom: 8px;
        }

        .match-variants {
          display: flex;
          flex-wrap: wrap;
          gap: 4px;
        }
      }
    }

    .manual-select-content {
      .feature-option-name {
        font-weight: 500;
      }

      .feature-option-desc {
        color: #999;
        font-size: 12px;
      }
    }

    .match-mode-section {
      margin-top: 20px;
      padding: 16px;
      background: #fafafa;
      border-radius: 4px;
    }
  }
</style>