<template>
  <BasicModal v-bind="$attrs" @register="registerModal" :title="modalTitle" @ok="handleSubmit" @cancel="handleCancel"
    :width="'min(1280px, calc(100vw - 32px))'" :minHeight="380" :canFullscreen="true" class="formula-builder-modal">

    <div v-if="showRangeModeToggle" class="formula-mode-switch">
      <a-radio-group v-model:value="editorMode" button-style="solid" size="middle">
        <a-radio-button value="range">规格模板模式</a-radio-button>
        <a-radio-button value="advanced">高级公式模式</a-radio-button>
      </a-radio-group>
    </div>

    <!-- 检测是否为范围列,显示不同的编辑器 -->
    <template v-if="editorMode === 'range'">
      <!-- 范围列配置模式 -->
      <div class="range-formula-config">
        <a-form :model="rangeConfig" layout="vertical" class="range-formula-form">
          <div class="range-formula-layout">
            <div class="range-config-main">
              <a-alert message="范围计算列配置" description="此列将对多个检测列或带厚列进行统计计算,自动适配不同产品的实际列数" type="info" show-icon
                style="margin-bottom: 12px; padding: 8px 12px;" />

          <!-- 列类型选择 -->
          <a-form-item label="数据列类型" required>
            <a-radio-group v-model:value="rangeConfig.prefix" button-style="solid" size="large">
              <a-radio-button value="Detection">
                <span class="radio-option">
                  <Icon icon="ant-design:radar-chart-outlined" :size="16" />
                  <span>检测数据列 (Detection)</span>
                </span>
              </a-radio-button>
              <a-radio-button value="Thickness">
                <span class="radio-option">
                  <Icon icon="ant-design:column-height-outlined" :size="16" />
                  <span>带厚列 (Thickness)</span>
                </span>
              </a-radio-button>
            </a-radio-group>
          </a-form-item>

          <!-- 操作类型 -->
          <a-form-item label="计算类型" required>
            <a-select v-model:value="rangeConfig.operation" size="large">
              <a-select-option value="AVG">
                <div class="operation-option">
                  <Icon icon="ant-design:calculator-outlined" :size="16" />
                  <span>平均值 (AVG)</span>
                </div>
              </a-select-option>
              <a-select-option value="MAX">
                <div class="operation-option">
                  <Icon icon="ant-design:arrow-up-outlined" :size="16" />
                  <span>最大值 (MAX)</span>
                </div>
              </a-select-option>
              <a-select-option value="MIN">
                <div class="operation-option">
                  <Icon icon="ant-design:arrow-down-outlined" :size="16" />
                  <span>最小值 (MIN)</span>
                </div>
              </a-select-option>
              <a-select-option value="SUM">
                <div class="operation-option">
                  <Icon icon="ant-design:plus-outlined" :size="16" />
                  <span>求和 (SUM)</span>
                </div>
              </a-select-option>
              <a-select-option value="COUNT">
                <div class="operation-option">
                  <Icon icon="ant-design:number-outlined" :size="16" />
                  <span>计数 (COUNT)</span>
                </div>
              </a-select-option>
              <a-select-option value="DIFF_FIRST">
                <div class="operation-option">
                  <Icon icon="ant-design:step-backward-outlined" :size="16" />
                  <span>前N列差值 (DIFF_FIRST)</span>
                </div>
              </a-select-option>
              <a-select-option value="DIFF_LAST">
                <div class="operation-option">
                  <Icon icon="ant-design:step-forward-outlined" :size="16" />
                  <span>后N列差值 (DIFF_LAST)</span>
                </div>
              </a-select-option>
            </a-select>
          </a-form-item>

          <a-form-item v-if="rangeConfig.operation !== 'DIFF_FIRST' && rangeConfig.operation !== 'DIFF_LAST'" label="常用逻辑模板">
            <a-radio-group v-model:value="activeRangePreset" button-style="solid" size="large" class="range-template-tabs">
              <a-radio-button
                v-for="preset in rangePresets"
                :key="preset.key"
                :value="preset.key"
                @click="applyRangePreset(preset.key)"
              >
                {{ preset.label }}
              </a-radio-button>
            </a-radio-group>
            <div v-if="activeRangePreset === 'LEFT' || activeRangePreset === 'MIDDLE' || activeRangePreset === 'RIGHT'" class="preset-count-row">
              <span class="preset-count-label">
                {{
                  activeRangePreset === 'LEFT'
                    ? '从左边开始几列'
                    : activeRangePreset === 'MIDDLE'
                      ? '中间取几列'
                      : '从右边开始几列'
                }}
              </span>
              <a-input-number
                v-model:value="presetColumnCount"
                :min="1"
                :max="currentColumnCount"
                size="middle"
                class="preset-count-input"
              />
            </div>
          </a-form-item>

          <!-- DIFF_FIRST 配置 -->
          <template v-if="rangeConfig.operation === 'DIFF_FIRST'">
            <a-divider>前N列差值配置</a-divider>
            <a-form-item label="前几列">
              <a-input-number v-model:value="rangeConfig.firstN" :min="2" :max="10" size="large" style="width: 100%" />
              <div class="form-hint">计算第1列值 - 第N列值</div>
            </a-form-item>
            <a-alert message="前N列差值说明" description="计算范围内第1列与第N列的差值。例如: DIFF_FIRST(Detection, 2) = Detection1 - Detection2" type="info" show-icon />
          </template>

          <!-- DIFF_LAST 配置 -->
          <template v-if="rangeConfig.operation === 'DIFF_LAST'">
            <a-divider>后N列差值配置</a-divider>
            <a-form-item label="后几列">
              <a-input-number v-model:value="rangeConfig.lastN" :min="2" :max="10" size="large" style="width: 100%" />
              <div class="form-hint">计算最后一列值 - 倒数第N列值</div>
            </a-form-item>
            <a-alert message="后N列差值说明" description="计算范围内最后N列的差值。例如: 若DetectionColumns=22, DIFF_LAST(Detection, 2) = Detection22 - Detection21" type="info" show-icon />
          </template>

            </div>

            <div class="range-config-side">
              <!-- 公式预览 -->
              <div class="formula-preview-card">
                <div class="preview-header">
                  <Icon icon="ant-design:code-outlined" :size="16" />
                  <span>生成的公式</span>
                </div>
                <div class="formula-code">{{ generatedFormula }}</div>
                <div class="formula-desc">
                  <Icon icon="ant-design:bulb-outlined" :size="14" />
                  <span>{{ getFormulaDescription() }}</span>
                </div>
              </div>

              <div class="simulation-card">
                <div class="preview-header">
                  <Icon icon="ant-design:experiment-outlined" :size="16" />
                  <span>模拟计算</span>
                </div>
                <div class="simulation-result-card">
                  <div class="simulation-result-row">
                    <span class="simulation-result-label">计算结果</span>
                    <span class="simulation-result-value">{{ simulationResultText }}</span>
                  </div>
                  <div class="simulation-result-row">
                    <span class="simulation-result-label">参与范围</span>
                    <span class="simulation-result-text">{{ simulationRangeText }}</span>
                  </div>
                  <div class="simulation-result-row">
                    <span class="simulation-result-label">参与数据</span>
                    <span class="simulation-result-text">{{ simulationValuesText }}</span>
                  </div>
                  <div class="simulation-result-row">
                    <span class="simulation-result-label">计算过程</span>
                    <span class="simulation-result-text">{{ simulationExplainText }}</span>
                  </div>
                </div>
              </div>
            </div>
          </div>

          <a-form-item label="各规格数据范围" style="margin-top: 16px;">
            <div class="spec-range-table">
              <div class="spec-range-table__head">
                <span>产品规格</span>
                <span>列数</span>
                <span>数据范围</span>
                <span>样例数据</span>
              </div>
              <button
                v-for="spec in mockSpecs"
                :key="spec.productSpecId"
                type="button"
                class="spec-range-table__row"
                :class="{ active: spec.productSpecId === selectedMockSpecId }"
                @click="selectedMockSpecId = spec.productSpecId"
              >
                <span class="cell cell-name">{{ spec.productSpecName }}</span>
                <span class="cell cell-columns">{{ getSpecColumnCount(spec) }} 列</span>
                <span class="cell cell-range-input">
                  <span class="range-edit-inline">
                    <a-input-number
                      :value="activeRangePreset === 'CUSTOM' ? getSpecCustomRange(spec).start : getSpecPresetRange(spec).start"
                      :min="1"
                      :max="getSpecColumnCount(spec)"
                      size="small"
                      class="range-text-input"
                      :disabled="activeRangePreset !== 'CUSTOM'"
                      @click.stop
                      @change="updateSpecCustomRange(spec, 'start', $event)"
                    />
                    <span class="range-edit-sep">-</span>
                    <a-input-number
                      :value="activeRangePreset === 'CUSTOM' ? getSpecCustomRange(spec).end : getSpecPresetRange(spec).end"
                      :min="1"
                      :max="getSpecColumnCount(spec)"
                      size="small"
                      class="range-text-input"
                      :disabled="activeRangePreset !== 'CUSTOM'"
                      @click.stop
                      @change="updateSpecCustomRange(spec, 'end', $event)"
                    />
                  </span>
                </span>
                <span class="cell cell-sample">
                  <span class="sample-text">{{ getSpecSampleText(spec) }}</span>
                </span>
              </button>
            </div>
          </a-form-item>
        </a-form>
      </div>
    </template>

    <!-- 普通公式编辑器 (原有逻辑) -->
    <template v-else>
      <div class="formula-builder">
        <!-- 左侧面板 -->
        <div class="panel left-panel">
          <div class="panel-header">
            <span class="step-badge">1</span>
            <span class="panel-title">可用字段</span>
          </div>
          <div class="search-box">
            <a-input v-model:value="searchQuery" placeholder="搜索字段..." allowClear size="small">
              <template #prefix>
                <Icon icon="ant-design:search-outlined" :size="14" />
              </template>
            </a-input>
          </div>
          <div class="fields-list custom-scroll">
            <div v-for="field in filteredFields" :key="field.columnName" class="field-card" draggable="true"
              @dragstart="handleDragStart($event, field)" @click="insertField(field)">
              <div class="field-icon-wrapper">
                <Icon icon="ant-design:database-outlined" :size="16" />
              </div>
              <div class="field-info">
                <span class="field-name">{{ field.displayName }}</span>
                <span class="field-key">{{ field.columnName }}</span>
              </div>
              <div class="add-icon">+</div>
            </div>
          </div>
          <div class="manual-input-section">
            <div class="section-label">手动输入数值</div>
            <div class="input-group">
              <a-input v-model:value="manualNumber" placeholder="100" class="mini-input" @keyup.enter="insertNumber" />
              <a-button class="add-btn" @click="insertNumber">+</a-button>
            </div>
          </div>
        </div>

        <!-- 中间面板 -->
        <div class="panel center-panel">

          <div class="editor-header">
            <div class="title-group">
              <Icon icon="ant-design:edit-outlined" :size="18" />
              <span class="panel-title">公式编辑器</span>
            </div>
            <div class="editor-header-actions">
              <a-tooltip title="占满屏幕编辑；也可双击标题栏或点右上角最大化图标">
                <a-button type="link" size="small" class="fullscreen-entry-btn" @click="enterFormulaFullscreen">
                  <template #icon>
                    <Icon icon="ant-design:fullscreen-outlined" :size="16" />
                  </template>
                  全屏编辑
                </a-button>
              </a-tooltip>
              <a-button type="link" danger size="small" @click="clearFormula">清空全部</a-button>
            </div>
          </div>

          <div class="block-editor" @drop="handleDrop" @dragover.prevent @dragleave="dragOverIndex = null"
            @click="focusEditor">
            <div class="blocks-container">
              <template v-for="(token, index) in tokens" :key="index">
                <span v-if="token.type === 'field'" class="formula-block field-block"
                  :class="{ 'drag-over': dragOverIndex === index }" draggable="true"
                  @dragstart="handleTokenDragStart($event, index)" @drop.stop="handleTokenDrop($event, index)"
                  @dragover.prevent @dragenter="dragOverIndex = index" @dragleave="dragOverIndex = null"
                  @click.stop="removeToken(index)">
                  {{ token.label || token.value }}
                  <span class="remove-x">×</span>
                </span>

                <span v-else-if="token.type === 'operator'" class="formula-block operator-block"
                  :class="{ 'drag-over': dragOverIndex === index }" draggable="true"
                  @dragstart="handleTokenDragStart($event, index)" @drop.stop="handleTokenDrop($event, index)"
                  @dragover.prevent @dragenter="dragOverIndex = index" @dragleave="dragOverIndex = null"
                  @click.stop="removeToken(index)">
                  {{ token.value }}
                </span>

                <span v-else-if="token.type === 'function'" class="formula-block function-block"
                  :class="{ 'drag-over': dragOverIndex === index }" draggable="true"
                  @dragstart="handleTokenDragStart($event, index)" @drop.stop="handleTokenDrop($event, index)"
                  @dragover.prevent @dragenter="dragOverIndex = index" @dragleave="dragOverIndex = null"
                  @click.stop="removeToken(index)">
                  {{ token.label || token.value }}
                  <span class="remove-x">×</span>
                </span>

                <span v-else-if="token.type === 'number'" class="formula-block number-block"
                  :class="{ 'drag-over': dragOverIndex === index }" draggable="true"
                  @dragstart="handleTokenDragStart($event, index)" @drop.stop="handleTokenDrop($event, index)"
                  @dragover.prevent @dragenter="dragOverIndex = index" @dragleave="dragOverIndex = null"
                  @click.stop="removeToken(index)">
                  {{ token.value }}
                  <span class="remove-x">×</span>
                </span>

                <span v-else class="formula-block text-block" :class="{ 'drag-over': dragOverIndex === index }"
                  draggable="true" @dragstart="handleTokenDragStart($event, index)"
                  @drop.stop="handleTokenDrop($event, index)" @dragover.prevent @dragenter="dragOverIndex = index"
                  @dragleave="dragOverIndex = null" @click.stop="removeToken(index)">
                  {{ token.value }}
                </span>
              </template>

              <div v-if="tokens.length === 0" class="placeholder-text">
                请将字段拖拽至此处,或点击右侧运算符构建公式
              </div>
            </div>
          </div>

          <div class="preview-section">
            <div class="section-label">原始公式预览</div>
            <div class="preview-box">
              {{ formulaText || '(空公式)' }}
            </div>
          </div>
        </div>

        <!-- 右侧面板 -->
        <div class="panel right-panel">
          <div class="panel-section">
            <div class="panel-header">
              <span class="step-badge orange">2</span>
              <span class="panel-title">基础运算</span>
            </div>
            <div class="operators-grid">
              <button v-for="op in basicOperators" :key="op" class="op-btn" @click="insertOperator(op)">{{ op
              }}</button>
            </div>
          </div>

          <div class="panel-section">
            <div class="panel-header">
              <span class="step-badge orange">3</span>
              <span class="panel-title">语法结构</span>
            </div>
            <div class="operators-grid syntax-grid">
              <button v-for="op in syntaxOperators" :key="op" class="op-btn" @click="insertOperator(op)">
                {{ op === ',' ? ',' : (op === 'TO' ? '至' : op) }}
              </button>
            </div>
          </div>

          <div class="panel-section">
            <div class="panel-header sm-header">
              <span class="panel-title">比较运算</span>
            </div>
            <div class="operators-grid">
              <button v-for="op in comparisonOperators" :key="op" class="op-btn" @click="insertOperator(op)">{{ op
              }}</button>
            </div>
          </div>

          <div class="panel-section">
            <div class="panel-header">
              <span class="step-badge purple">4</span>
              <span class="panel-title">函数</span>
              <span class="header-tip">使用逗号 (,) 分隔参数</span>
            </div>
            <div class="functions-list-detailed">
              <div v-for="func in functions" :key="func.name" class="func-item" @click="insertFunction(func)">
                <div class="func-name">{{ func.name }}</div>
                <div class="func-desc">{{ func.description }}</div>
              </div>
            </div>
          </div>

        </div>
      </div>
    </template>

    <template #footer>
      <div class="modal-footer">
        <a-button @click="handleCancel" class="footer-btn">取消</a-button>
        <a-button type="primary" class="footer-btn" @click="handleSubmit">保存公式</a-button>
      </div>
    </template>
  </BasicModal>
</template>

<script lang="ts" setup>
import { ref, computed, nextTick, watch } from 'vue';
import { BasicModal, useModalInner } from '/@/components/Modal';
import { Icon } from '/@/components/Icon';
import { getAvailableColumns } from '/@/api/lab/intermediateDataFormula';
import type { IntermediateDataColumnInfo } from '/@/api/lab/types/intermediateDataFormula';
import { getProductSpecList } from '/@/api/lab/productSpec';

// --- 类型定义 ---
type TokenType = 'field' | 'operator' | 'function' | 'number' | 'text';
interface Token {
  type: TokenType;
  value: string;
  label?: string;
}

interface RangeConfig {
  prefix: 'Detection' | 'Thickness';
  start: number;
  end: string;  // 数字字符串或 "$DetectionColumns"
  operation: 'AVG' | 'MAX' | 'MIN' | 'SUM' | 'COUNT' | 'DIFF_FIRST' | 'DIFF_LAST' | 'CONDITIONAL_DIFF' | '';
  firstN: number;
  lastN: number;
  // CONDITIONAL_DIFF 的配置
  middleStart: number;
  middleEnd: number;
  leftStart: number;
  leftEnd: number;
  rightStart: number;
  rightEnd: string;
}

interface MockProductSpec {
  productSpecId: string;
  productSpecName: string;
  productSpecCode?: string;
  detectionColumnCount: number;
  thicknessColumnCount: number;
  detectionValues: number[];
  thicknessValues: number[];
}

type RangePresetKey = 'LEFT' | 'MIDDLE' | 'RIGHT' | 'ALL' | 'CUSTOM';

const emit = defineEmits(['register', 'save']);

// --- 状态 ---
const modalTitle = ref('公式构建器');
const formulaId = ref('');
const isRangeColumn = ref(false);  // 是否为范围列
const editorMode = ref<'range' | 'advanced'>('advanced');
const availableFields = ref<IntermediateDataColumnInfo[]>([]);
const searchQuery = ref('');
const manualNumber = ref('');
const tokens = ref<Token[]>([]);
const dragOverIndex = ref<number | null>(null);  // 拖拽悬停位置
const selectedMockSpecId = ref('120');
const activeRangePreset = ref<RangePresetKey>('ALL');

// 范围列配置
const rangeConfig = ref<RangeConfig>({
  prefix: 'Thickness',
  start: 1,
  end: '$DetectionColumns',
  operation: 'AVG',
  firstN: 2,
  lastN: 2,
  // CONDITIONAL_DIFF 默认值
  middleStart: 9,
  middleEnd: 13,
  leftStart: 4,
  leftEnd: 8,
  rightStart: 14,
  rightEnd: '$DetectionColumns',
});

const mockSpecs = ref<MockProductSpec[]>([]);
const customSpecRanges = ref<Record<string, { start: number; end: number }>>({});
const presetColumnCount = ref('2');

const rangePresets = [
  { key: 'LEFT', label: '左侧区间', description: '从左边开始取 N 列' },
  { key: 'MIDDLE', label: '中间区间', description: '从中间位置取 N 列' },
  { key: 'RIGHT', label: '右侧区间', description: '从右边开始取 N 列' },
  { key: 'ALL', label: '全部列', description: '使用当前规格全部列' },
  { key: 'CUSTOM', label: '自定义区间', description: '手动指定起止列' },
] as const;

// --- 计算属性 ---
const filteredFields = computed(() => {
  if (!searchQuery.value) return availableFields.value;
  const query = searchQuery.value.toLowerCase();
  return availableFields.value.filter(f =>
    f.displayName.toLowerCase().includes(query) ||
    f.columnName.toLowerCase().includes(query)
  );
});

const formulaText = computed(() => {
  return tokens.value.map(t => t.value).join('');
});

const currentSpec = computed(() => {
  return mockSpecs.value.find(spec => spec.productSpecId === selectedMockSpecId.value) || mockSpecs.value[0];
});

const currentColumnCount = computed(() => {
  if (!currentSpec.value) return 22;
  return rangeConfig.value.prefix === 'Detection'
    ? currentSpec.value.detectionColumnCount
    : currentSpec.value.thicknessColumnCount;
});

const sampleLabelPrefix = computed(() => rangeConfig.value.prefix === 'Detection' ? '检' : '厚');
const editableSampleValues = computed(() => {
  if (!currentSpec.value) return [];
  const rawValues = rangeConfig.value.prefix === 'Detection'
    ? currentSpec.value.detectionValues
    : currentSpec.value.thicknessValues;
  return rawValues.slice(0, currentColumnCount.value);
});

const recommendedSegments = computed(() => buildRecommendedSegments(currentColumnCount.value));

function getSpecColumnCount(spec: MockProductSpec): number {
  return rangeConfig.value.prefix === 'Detection' ? spec.detectionColumnCount : spec.thicknessColumnCount;
}

function getSpecPresetRange(spec: MockProductSpec) {
  const columnCount = getSpecColumnCount(spec);
  const parsedCount = Number.parseInt(String(presetColumnCount.value || '2'), 10);
  const edgeCount = Math.min(Math.max(1, Number.isFinite(parsedCount) ? parsedCount : 2), columnCount);
  if (activeRangePreset.value === 'LEFT') return { start: 1, end: edgeCount };
  if (activeRangePreset.value === 'MIDDLE') {
    const start = Math.max(1, Math.floor((columnCount - edgeCount) / 2) + 1);
    return { start, end: Math.min(columnCount, start + edgeCount - 1) };
  }
  if (activeRangePreset.value === 'RIGHT') return { start: Math.max(1, columnCount - edgeCount + 1), end: columnCount };
  return { start: 1, end: columnCount };
}

function getSpecPresetRangeText(spec: MockProductSpec): string {
  const range = getSpecPresetRange(spec);
  return `${range.start}-${range.end}`;
}

function getSpecSampleText(spec: MockProductSpec): string {
  const values = (rangeConfig.value.prefix === 'Detection' ? spec.detectionValues : spec.thicknessValues)
    .slice(0, getSpecColumnCount(spec));
  return values.join(', ');
}

function getSpecCustomRange(spec: MockProductSpec) {
  const key = `${rangeConfig.value.prefix}-${spec.productSpecId}`;
  const cached = customSpecRanges.value[key];
  if (cached) return cached;
  const preset = getSpecPresetRange(spec);
  customSpecRanges.value[key] = { ...preset };
  return customSpecRanges.value[key];
}

function syncCurrentSpecRange() {
  if (!currentSpec.value) return;
  if (activeRangePreset.value === 'CUSTOM') {
    const customRange = getSpecCustomRange(currentSpec.value);
    rangeConfig.value.start = customRange.start;
    rangeConfig.value.end = String(customRange.end);
  } else {
    const presetRange = getSpecPresetRange(currentSpec.value);
    rangeConfig.value.start = presetRange.start;
    rangeConfig.value.end = String(presetRange.end);
  }
}

function updateSpecCustomRange(spec: MockProductSpec, field: 'start' | 'end', value: number | string | null) {
  const raw = typeof value === 'string' ? value : String(value ?? '');
  const numericValue = Number.parseInt(raw.replace(/[^\d]/g, ''), 10);
  if (!Number.isFinite(numericValue)) return;
  const max = getSpecColumnCount(spec);
  const next = { ...getSpecCustomRange(spec) };
  if (field === 'start') {
    next.start = Math.min(Math.max(1, Math.floor(numericValue)), max);
    next.end = Math.max(next.start, next.end);
  } else {
    next.end = Math.min(Math.max(next.start, Math.floor(numericValue)), max);
  }
  customSpecRanges.value[`${rangeConfig.value.prefix}-${spec.productSpecId}`] = next;
  if (spec.productSpecId === selectedMockSpecId.value) {
    rangeConfig.value.start = next.start;
    rangeConfig.value.end = String(next.end);
  }
}

const simulationRanges = computed(() => {
  const maxEnd = currentColumnCount.value;
  const normalizeEnd = (value: string) => value === '$DetectionColumns' ? maxEnd : Number(value);

  if (rangeConfig.value.operation === 'CONDITIONAL_DIFF') {
    return {
      left: { start: rangeConfig.value.leftStart, end: Math.min(rangeConfig.value.leftEnd, maxEnd) },
      middle: { start: rangeConfig.value.middleStart, end: Math.min(rangeConfig.value.middleEnd, maxEnd) },
      right: { start: rangeConfig.value.rightStart, end: Math.min(normalizeEnd(rangeConfig.value.rightEnd), maxEnd) },
    };
  }

  return {
    main: {
      start: rangeConfig.value.start,
      end: Math.min(normalizeEnd(rangeConfig.value.end), maxEnd),
    },
  };
});

const simulationResult = computed(() => {
  const values = editableSampleValues.value;
  if (!values.length) return null;

  const pickRangeValues = (start: number, end: number) => {
    const normalizedStart = Math.max(1, start);
    const normalizedEnd = Math.min(end, values.length);
    if (normalizedStart > normalizedEnd) return [] as number[];
    return values.slice(normalizedStart - 1, normalizedEnd);
  };

  const aggregate = (items: number[], operation: RangeConfig['operation']) => {
    if (!items.length) return null;
    switch (operation) {
      case 'AVG':
        return items.reduce((sum, item) => sum + item, 0) / items.length;
      case 'MAX':
        return Math.max(...items);
      case 'MIN':
        return Math.min(...items);
      case 'SUM':
        return items.reduce((sum, item) => sum + item, 0);
      case 'COUNT':
        return items.length;
      default:
        return null;
    }
  };

  if (rangeConfig.value.operation === 'DIFF_FIRST') {
    const items = pickRangeValues(rangeConfig.value.start, Number(rangeConfig.value.end === '$DetectionColumns' ? values.length : rangeConfig.value.end));
    if (items.length < rangeConfig.value.firstN) return null;
    const left = items[0];
    const right = items[rangeConfig.value.firstN - 1];
    return {
      result: left - right,
      rangeText: `第1列 - 第${rangeConfig.value.firstN}列`,
      valuesText: `${left} - ${right}`,
      explain: `${left} - ${right} = ${(left - right).toFixed(3)}`,
    };
  }

  if (rangeConfig.value.operation === 'DIFF_LAST') {
    const items = pickRangeValues(rangeConfig.value.start, Number(rangeConfig.value.end === '$DetectionColumns' ? values.length : rangeConfig.value.end));
    if (items.length < rangeConfig.value.lastN) return null;
    const left = items[items.length - 1];
    const right = items[items.length - rangeConfig.value.lastN];
    return {
      result: left - right,
      rangeText: `最后1列 - 倒数第${rangeConfig.value.lastN}列`,
      valuesText: `${left} - ${right}`,
      explain: `${left} - ${right} = ${(left - right).toFixed(3)}`,
    };
  }

  if (rangeConfig.value.operation === 'CONDITIONAL_DIFF') {
    const ranges = simulationRanges.value as {
      left: { start: number; end: number };
      middle: { start: number; end: number };
      right: { start: number; end: number };
    };
    const middleValues = pickRangeValues(ranges.middle.start, ranges.middle.end);
    const leftValues = pickRangeValues(ranges.left.start, ranges.left.end);
    const rightValues = pickRangeValues(ranges.right.start, ranges.right.end);
    const sideValues = [...leftValues, ...rightValues];
    const middleAvg = aggregate(middleValues, 'AVG');
    const sideAvg = aggregate(sideValues, 'AVG');
    if (middleAvg === null || sideAvg === null) return null;
    const useMinMinusMax = middleAvg < sideAvg;
    const result = useMinMinusMax
      ? Math.min(...middleValues) - Math.max(...sideValues)
      : Math.max(...middleValues) - Math.min(...sideValues);
    return {
      result,
      rangeText: `中${ranges.middle.start}-${ranges.middle.end} / 左${ranges.left.start}-${ranges.left.end} / 右${ranges.right.start}-${ranges.right.end}`,
      valuesText: `中间[${middleValues.join(', ')}]；两侧[${sideValues.join(', ')}]`,
      explain: useMinMinusMax
        ? `AVG(中间)=${middleAvg.toFixed(3)} < AVG(两侧)=${sideAvg.toFixed(3)}，取 MIN(中间) - MAX(两侧) = ${result.toFixed(3)}`
        : `AVG(中间)=${middleAvg.toFixed(3)} >= AVG(两侧)=${sideAvg.toFixed(3)}，取 MAX(中间) - MIN(两侧) = ${result.toFixed(3)}`,
    };
  }

  const mainRange = simulationRanges.value as { main: { start: number; end: number } };
  const mainValues = pickRangeValues(mainRange.main.start, mainRange.main.end);
  const result = aggregate(mainValues, rangeConfig.value.operation);
  if (result === null) return null;
  const opNameMap: Record<string, string> = { AVG: '平均值', MAX: '最大值', MIN: '最小值', SUM: '求和', COUNT: '非空计数' };
  return {
    result,
    rangeText: `${mainRange.main.start}-${mainRange.main.end}`,
    valuesText: mainValues.join(', '),
    explain: `${opNameMap[rangeConfig.value.operation] || rangeConfig.value.operation} = ${rangeConfig.value.operation === 'COUNT' ? result : result.toFixed(3)} (共${mainValues.length}项)`,
  };
});

const simulationResultText = computed(() => {
  if (!simulationResult.value) return '暂无结果';
  return typeof simulationResult.value.result === 'number' && Number.isFinite(simulationResult.value.result)
    ? simulationResult.value.result.toFixed(rangeConfig.value.operation === 'COUNT' ? 0 : 3)
    : '计算异常';
});

const simulationRangeText = computed(() => simulationResult.value?.rangeText || '-');
const simulationValuesText = computed(() => simulationResult.value?.valuesText || '-');
const simulationExplainText = computed(() => simulationResult.value?.explain || '请先选择模板和计算类型');

// 生成范围公式
const generatedFormula = computed(() => {
  const { operation, prefix, start, end, firstN, lastN, middleStart, middleEnd, leftStart, leftEnd, rightStart, rightEnd } = rangeConfig.value;

  if (!operation) return '';

  if (operation === 'DIFF_FIRST') {
    return `DIFF_FIRST(${prefix}, ${firstN}, ${end})`;
  }

  if (operation === 'DIFF_LAST') {
    return `DIFF_LAST(${prefix}, ${lastN}, ${end})`;
  }

  if (operation === 'CONDITIONAL_DIFF') {
    const middleRange = `RANGE(${prefix}, ${middleStart}, ${middleEnd})`;
    const sidesRange = `RANGE(${prefix}, ${leftStart}, ${leftEnd}), RANGE(${prefix}, ${rightStart}, ${rightEnd})`;
    return `IF(AVG(${middleRange}) < AVG(${sidesRange}), MIN(${middleRange}) - MAX(${sidesRange}), MAX(${middleRange}) - MIN(${sidesRange}))`;
  }

  return `${operation}(RANGE(${prefix}, ${start}, ${end}))`;
});

// 公式描述
function getFormulaDescription(): string {
  const { operation, prefix, start, end, firstN, lastN } = rangeConfig.value;

  if (!operation) return '请选择计算类型';

  const prefixName = prefix === 'Detection' ? '检测数据' : '带厚';
  const endDesc = end === '$DetectionColumns'
    ? 'DetectionColumns字段的值'
    : `第${end}列`;

  if (operation === 'DIFF_FIRST') {
    return `计算${prefixName}前${firstN}列的差值（第1列值 - 第${firstN}列值）`;
  }

  if (operation === 'DIFF_LAST') {
    return `计算${prefixName}后${lastN}列的差值（最后一列值 - 倒数第${lastN}列值）`;
  }

  if (operation === 'CONDITIONAL_DIFF') {
    const { middleStart, middleEnd, leftStart, leftEnd, rightStart, rightEnd } = rangeConfig.value;
    return `条件差值：比较${prefixName}${middleStart}-${middleEnd}列与${leftStart}-${leftEnd}、${rightStart}-${rightEnd}列的平均值，根据大小关系计算极值差`;
  }

  const opName = {
    'AVG': '平均值',
    'MAX': '最大值',
    'MIN': '最小值',
    'SUM': '求和',
    'COUNT': '非空值计数',
  }[operation] || operation;

  return `计算${prefixName}第${start}列到${endDesc}的${opName}`;
}

// 辅助函数：获取中间列范围字符串
function getMiddleRange(): string {
  const { prefix, middleStart, middleEnd } = rangeConfig.value;
  return `RANGE(${prefix}, ${middleStart}, ${middleEnd})`;
}

// 辅助函数：获取两边列范围字符串
function getSidesRange(): string {
  const { prefix, leftStart, leftEnd, rightStart, rightEnd } = rangeConfig.value;
  return `RANGE(${prefix}, ${leftStart}, ${leftEnd}), RANGE(${prefix}, ${rightStart}, ${rightEnd})`;
}

// --- 常量定义 ---
const basicOperators = ['+', '-', '×', '÷'];
const syntaxOperators = ['(', ')', ','];
const comparisonOperators = ['=', '<>', '>', '<'];
const allOperators = [...basicOperators, ...syntaxOperators, ...comparisonOperators];

const functions = [
  { name: 'SUM', value: 'SUM(', type: 'function', description: '统计' },
  { name: 'AVG', value: 'AVG(', type: 'function', description: '统计' },
  { name: 'MAX', value: 'MAX(', type: 'function', description: '统计' },
  { name: 'MIN', value: 'MIN(', type: 'function', description: '统计' },
  { name: 'IF', value: 'IF(', type: 'function', description: '逻辑' },
  { name: 'RANGE', value: 'RANGE(', type: 'function', description: '范围' },
  { name: 'DIFF_FIRST', value: 'DIFF_FIRST(', type: 'function', description: '前N列差' },
  { name: 'DIFF_LAST', value: 'DIFF_LAST(', type: 'function', description: '后N列差' },
];

const templates: any[] = [];
const showRangeModeToggle = computed(() => {
  const formulaType = currentRecord.value?.formulaType;
  return formulaType !== 'JUDGE' && formulaType !== 'NO';
});
const currentRecord = ref<any>(null);

function buildRecommendedSegments(columnCount: number) {
  if (columnCount <= 4) {
    return {
      left: { start: 1, end: 1 },
      middle: { start: 1, end: columnCount },
      right: { start: columnCount, end: columnCount },
    };
  }

  const edgeSize = columnCount <= 7 ? 2 : Math.min(4, Math.max(2, Math.floor(columnCount / 3)));
  const left = { start: 1, end: edgeSize };
  const right = { start: Math.max(edgeSize + 1, columnCount - edgeSize + 1), end: columnCount };
  const middleStart = Math.min(left.end + 1, right.start - 1);
  const middleEnd = Math.max(middleStart, right.start - 1);

  return {
    left,
    middle: { start: middleStart, end: middleEnd },
    right,
  };
}

function applyRecommendedSegments() {
  const recommended = recommendedSegments.value;
  rangeConfig.value.middleStart = recommended.middle.start;
  rangeConfig.value.middleEnd = recommended.middle.end;
  rangeConfig.value.leftStart = recommended.left.start;
  rangeConfig.value.leftEnd = recommended.left.end;
  rangeConfig.value.rightStart = recommended.right.start;
  rangeConfig.value.rightEnd = String(recommended.right.end);
}

function applyRangePreset(preset: RangePresetKey) {
  activeRangePreset.value = preset;
  applyRecommendedSegments();
  syncCurrentSpecRange();
}

function clampRangeConfigToCurrentSpec() {
  const max = currentColumnCount.value;
  rangeConfig.value.start = Math.min(Math.max(1, rangeConfig.value.start), max);
  if (rangeConfig.value.end !== '$DetectionColumns') {
    rangeConfig.value.end = String(Math.min(Math.max(rangeConfig.value.start, Number(rangeConfig.value.end)), max));
  }

  rangeConfig.value.middleStart = Math.min(Math.max(1, rangeConfig.value.middleStart), max);
  rangeConfig.value.middleEnd = Math.min(Math.max(rangeConfig.value.middleStart, rangeConfig.value.middleEnd), max);
  rangeConfig.value.leftStart = Math.min(Math.max(1, rangeConfig.value.leftStart), max);
  rangeConfig.value.leftEnd = Math.min(Math.max(rangeConfig.value.leftStart, rangeConfig.value.leftEnd), max);
  rangeConfig.value.rightStart = Math.min(Math.max(1, rangeConfig.value.rightStart), max);
  if (rangeConfig.value.rightEnd !== '$DetectionColumns') {
    rangeConfig.value.rightEnd = String(Math.min(Math.max(rangeConfig.value.rightStart, Number(rangeConfig.value.rightEnd)), max));
  }
}

// --- Modal Init ---
const [registerModal, { setModalProps, closeModal, redoModalHeight }] = useModalInner(async (data) => {
  setModalProps({ confirmLoading: false, defaultFullscreen: false });
  formulaId.value = data?.record?.id || '';
  currentRecord.value = data?.record || null;

  // 检查是否为范围列
  isRangeColumn.value = data?.record?.isRange === true;
  editorMode.value = resolveInitialEditorMode(data?.record);

  if (data?.record) {
    modalTitle.value = `编辑公式:${data.record.formulaName || ''} (${data.record.columnName || ''})`;
  } else {
    modalTitle.value = '公式构建器';
  }

  await loadAvailableFields();
  await loadRealProductSpecs();

  const initFormula = data?.record?.formula || '';

  if (editorMode.value === 'range' && initFormula) {
    // 解析范围公式
    parseRangeFormula(initFormula);
    applyRecommendedSegments();
  } else if (initFormula) {
    // 解析普通公式
    parseFormulaToTokens(initFormula);
  } else {
    tokens.value = [];

    // 从 record 初始化默认值
    const record = data?.record || {};
    const defaultPrefix = record.rangePrefix || 'Thickness';

    // 如果是 Detection 则默认为 MAX, 否则 AVG (或者如果不确定则保留 AVG)
    // 根据用户需求: "Detection ... 进行MAX"
    const defaultOp = defaultPrefix === 'Detection' ? 'MAX' : 'AVG';

    // 处理 rangeEnd: -1 对应 $DetectionColumns
    let defaultEnd = '$DetectionColumns';
    if (record.rangeEnd !== undefined && record.rangeEnd !== null) {
      if (Number(record.rangeEnd) === -1) {
        defaultEnd = '$DetectionColumns';
      } else {
        defaultEnd = String(record.rangeEnd);
      }
    }

    rangeConfig.value = {
      prefix: defaultPrefix,
      start: record.rangeStart || 1,
      end: defaultEnd,
      operation: defaultOp,
      firstN: 2,
      lastN: 2,
      middleStart: 9,
      middleEnd: 13,
      leftStart: 4,
      leftEnd: 8,
      rightStart: 14,
      rightEnd: '$DetectionColumns',
    };
    applyRangePreset('ALL');
  }
});

async function loadRealProductSpecs() {
  try {
    const res: any = await getProductSpecList({ pageSize: 500, currentPage: 1, enabled: true });
    const list = Array.isArray(res?.list) ? res.list : [];
    const mapped = list
      .map((item: any) => {
        const columnCount = parseDetectionColumnCount(item?.detectionColumns);
        return {
          productSpecId: String(item?.id || item?.productSpecId || ''),
          productSpecCode: item?.code || item?.productSpecCode || '',
          productSpecName: item?.name || item?.productSpecName || item?.code || '未命名规格',
          detectionColumnCount: columnCount,
          thicknessColumnCount: columnCount,
          detectionValues: buildSampleValues(columnCount, 'Detection', item?.code || item?.name || ''),
          thicknessValues: buildSampleValues(columnCount, 'Thickness', item?.code || item?.name || ''),
        } satisfies MockProductSpec;
      })
      .filter((item: MockProductSpec) => item.productSpecId);

    mockSpecs.value = mapped.length ? mapped : buildFallbackSpecs();
    if (!mockSpecs.value.some(item => item.productSpecId === selectedMockSpecId.value)) {
      selectedMockSpecId.value = mockSpecs.value[0]?.productSpecId || 'default';
    }
  } catch {
    mockSpecs.value = buildFallbackSpecs();
    selectedMockSpecId.value = mockSpecs.value[0]?.productSpecId || 'default';
  }
}

function parseDetectionColumnCount(rawValue: unknown): number {
  if (typeof rawValue === 'number' && Number.isFinite(rawValue)) {
    return Math.min(22, Math.max(1, Math.floor(rawValue)));
  }

  if (typeof rawValue !== 'string') return 15;
  const values = rawValue
    .split(',')
    .map(item => Number(item.trim()))
    .filter(item => Number.isFinite(item) && item > 0);

  if (!values.length) return 15;
  return Math.min(22, Math.max(1, Math.max(...values)));
}

function buildSampleValues(count: number, family: 'Detection' | 'Thickness', seedSource: string): number[] {
  const baseSamples = [
    566.9, 509.9, 511.9, 525.7, 507.2, 490.9, 504.4, 514.4, 520.2, 501.9, 489.5,
    474.4, 483.2, 498.5, 491.0, 484.7, 491.1, 485.2, 496.8, 497.4, 503.9, 508.1,
  ];
  const seed = Array.from(seedSource).reduce((sum, char) => sum + char.charCodeAt(0), 0) % 7;
  const offset = family === 'Thickness' ? seed * 0.1 : seed * 0.3;
  return Array.from({ length: count }, (_, index) => {
    if (index < baseSamples.length) {
      return Number((baseSamples[index] + offset).toFixed(1));
    }
    const last = baseSamples[baseSamples.length - 1];
    return Number((last + offset + (index - baseSamples.length + 1) * 1.7).toFixed(1));
  });
}

function buildFallbackSpecs(): MockProductSpec[] {
  const fallbackCounts = [13, 9, 6];
  return fallbackCounts.map((count, index) => ({
    productSpecId: `fallback-${index + 1}`,
    productSpecCode: `S${index + 1}`,
    productSpecName: `默认规格${index + 1}`,
    detectionColumnCount: count,
    thicknessColumnCount: count,
    detectionValues: buildSampleValues(count, 'Detection', `fallback-${index + 1}`),
    thicknessValues: buildSampleValues(count, 'Thickness', `fallback-${index + 1}`),
  }));
}

function resolveInitialEditorMode(record: any): 'range' | 'advanced' {
  if (!record) return 'advanced';
  if (record.isRange === true) return 'range';

  const formula = String(record.formula || '').toUpperCase();
  const columnName = String(record.columnName || '').toUpperCase();
  const formulaName = String(record.formulaName || '').toUpperCase();
  const rangeHints = ['RANGE(', 'DIFF_FIRST(', 'DIFF_LAST(', 'CONDITIONAL_DIFF', 'DETECTION', 'THICKNESS'];
  const columnHints = ['THICK', 'DETECTION', '带厚', '检测'];

  if (rangeHints.some(hint => formula.includes(hint))) return 'range';
  if (columnHints.some(hint => columnName.includes(hint) || formulaName.includes(hint))) return 'range';
  return 'advanced';
}

const loadAvailableFields = async () => {
  try {
    const res: any = await getAvailableColumns(true);
    availableFields.value = res.data || res || [];
  } catch (e) { }
};

// --- 解析范围公式 ---
function parseRangeFormula(formula: string) {
  const defaultConfig = {
    middleStart: 9,
    middleEnd: 13,
    leftStart: 4,
    leftEnd: 8,
    rightStart: 14,
    rightEnd: '$DetectionColumns',
  };

  // 匹配 OPERATION(RANGE(Prefix, Start, End))
  const match = formula.match(/^(\w+)\(RANGE\((\w+),\s*(\d+),\s*([\w$]+)\)\)$/);

  if (match) {
    rangeConfig.value = {
      operation: match[1] as any,
      prefix: match[2] as any,
      start: parseInt(match[3]),
      end: match[4],
      firstN: 2,
      lastN: 2,
      ...defaultConfig,
    };
    return;
  }

  // 匹配 DIFF_FIRST(Prefix, Count, MaxColumns)
  const diffFirstMatch = formula.match(/^DIFF_FIRST\((\w+),\s*(\d+),\s*([\w$]+)\)$/);

  if (diffFirstMatch) {
    rangeConfig.value = {
      operation: 'DIFF_FIRST',
      prefix: diffFirstMatch[1] as any,
      start: 1,
      end: diffFirstMatch[3],
      firstN: parseInt(diffFirstMatch[2]),
      lastN: 2,
      ...defaultConfig,
    };
    return;
  }

  // 匹配 DIFF_LAST(Prefix, Count, MaxColumns)
  const diffLastMatch = formula.match(/^DIFF_LAST\((\w+),\s*(\d+),\s*([\w$]+)\)$/);

  if (diffLastMatch) {
    rangeConfig.value = {
      operation: 'DIFF_LAST',
      prefix: diffLastMatch[1] as any,
      start: 1,
      end: diffLastMatch[3],
      firstN: 2,
      lastN: parseInt(diffLastMatch[2]),
      ...defaultConfig,
    };
    return;
  }

  // 匹配 IF(AVG(RANGE(...)) < AVG(RANGE(...), RANGE(...)), ...)
  const conditionalMatch = formula.match(/^IF\(AVG\(RANGE\((\w+),\s*(\d+),\s*(\d+)\)\)\s*<\s*AVG\(RANGE\(\w+,\s*(\d+),\s*(\d+)\),\s*RANGE\(\w+,\s*(\d+),\s*([\w$]+)\)\)/);

  if (conditionalMatch) {
    rangeConfig.value = {
      operation: 'CONDITIONAL_DIFF',
      prefix: conditionalMatch[1] as any,
      start: 1,
      end: '$DetectionColumns',
      firstN: 2,
      lastN: 2,
      middleStart: parseInt(conditionalMatch[2]),
      middleEnd: parseInt(conditionalMatch[3]),
      leftStart: parseInt(conditionalMatch[4]),
      leftEnd: parseInt(conditionalMatch[5]),
      rightStart: parseInt(conditionalMatch[6]),
      rightEnd: conditionalMatch[7],
    };
    return;
  }

  // 解析失败,使用默认值
  console.warn('无法解析范围公式:', formula);
}

watch([selectedMockSpecId, () => rangeConfig.value.prefix], () => {
  clampRangeConfigToCurrentSpec();
  applyRecommendedSegments();
  syncCurrentSpecRange();
});

watch(presetColumnCount, value => {
  const digitsOnly = String(value ?? '').replace(/[^\d]/g, '');
  const normalized = String(Math.max(1, Number.parseInt(digitsOnly || '2', 10)));
  if (normalized !== value) {
    presetColumnCount.value = normalized;
    return;
  }
  if (activeRangePreset.value === 'LEFT' || activeRangePreset.value === 'MIDDLE' || activeRangePreset.value === 'RIGHT') {
    syncCurrentSpecRange();
  }
});

// --- 表达式模式操作 (保持原有逻辑) ---
const parseFormulaToTokens = (formula: string) => {
  const result: Token[] = [];
  let buffer = formula;

  while (buffer.length > 0) {
    // 匹配完整的 IF(AVG(RANGE...) < AVG(RANGE..., RANGE...), ...) 条件差值公式
    const conditionalDiffMatch = buffer.match(/^IF\s*\(\s*AVG\s*\(\s*RANGE\s*\(\s*(\w+)\s*,\s*(\d+)\s*,\s*(\d+)\s*\)\s*\)\s*<\s*AVG\s*\(\s*RANGE\s*\(\s*\w+\s*,\s*(\d+)\s*,\s*(\d+)\s*\)\s*,\s*RANGE\s*\(\s*\w+\s*,\s*(\d+)\s*,\s*([\w$]+)\s*\)\s*\)\s*,\s*MIN\s*\([^)]+\)\s*-\s*MAX\s*\([^)]+\)\s*,\s*MAX\s*\([^)]+\)\s*-\s*MIN\s*\([^)]+\)\s*\)/i);
    if (conditionalDiffMatch) {
      const prefix = conditionalDiffMatch[1];
      const middleStart = conditionalDiffMatch[2];
      const middleEnd = conditionalDiffMatch[3];
      const leftStart = conditionalDiffMatch[4];
      const leftEnd = conditionalDiffMatch[5];
      const rightStart = conditionalDiffMatch[6];
      const rightEnd = conditionalDiffMatch[7];
      const displayLabel = `条件差值: ${prefix}${middleStart}-${middleEnd} vs ${prefix}${leftStart}-${leftEnd}, ${prefix}${rightStart}-${rightEnd === '$DetectionColumns' ? '动态' : rightEnd}`;
      result.push({ type: 'function', value: conditionalDiffMatch[0], label: displayLabel });
      buffer = buffer.slice(conditionalDiffMatch[0].length);
      continue;
    }

    // 匹配范围前缀名称（Detection, Thickness）作为字段
    const rangePrefixMatch = buffer.match(/^(Detection|Thickness)\b/i);
    if (rangePrefixMatch) {
      const prefix = rangePrefixMatch[1];
      const displayName = prefix.toLowerCase() === 'detection' ? '检测数据' : '带厚';
      result.push({ type: 'field', value: prefix, label: `${displayName} (${prefix})` });
      buffer = buffer.slice(prefix.length);
      continue;
    }

    const fieldMatch = buffer.match(/^\[(.*?)\]/);
    if (fieldMatch) {
      const full = fieldMatch[0];
      const key = fieldMatch[1];
      const field = availableFields.value.find(f => f.columnName === key);
      const label = field ? `${field.displayName} (${field.columnName})` : full;

      result.push({ type: 'field', value: full, label });
      buffer = buffer.slice(full.length);
      continue;
    }

    const numMatch = buffer.match(/^\d+(\.\d+)?/);
    if (numMatch) {
      result.push({ type: 'number', value: numMatch[0] });
      buffer = buffer.slice(numMatch[0].length);
      continue;
    }

    let funcMatched = false;
    for (const func of functions) {
      if (buffer.startsWith(func.name)) {
        result.push({ type: 'function', value: func.name });
        buffer = buffer.slice(func.name.length);
        funcMatched = true;
        break;
      }
    }
    if (funcMatched) continue;

    if (buffer.startsWith('*')) {
      result.push({ type: 'operator', value: ' * ' });
      buffer = buffer.slice(1);
      continue;
    }
    if (buffer.startsWith('/')) {
      result.push({ type: 'operator', value: ' / ' });
      buffer = buffer.slice(1);
      continue;
    }

    if (buffer.startsWith('TO ') || (buffer.startsWith('TO') && (buffer.length === 2 || [' ', '('].includes(buffer[2])))) {
      result.push({ type: 'operator', value: ' TO ' });
      buffer = buffer.slice(2);
      continue;
    }

    let opMatched = false;
    for (const op of allOperators) {
      if (op === 'TO') continue;
      if (op === '×' || op === '÷') continue;
      if (buffer.startsWith(op)) {
        result.push({ type: 'operator', value: ` ${op} ` });
        buffer = buffer.slice(op.length);
        opMatched = true;
        break;
      }
    }
    if (opMatched) continue;

    const char = buffer[0];
    if (char.trim() === '') {
      const spaceMatch = buffer.match(/^\s+/);
      if (spaceMatch) {
        buffer = buffer.slice(spaceMatch[0].length);
      } else {
        buffer = buffer.slice(1);
      }
    } else {
      // 占位符/变量名整段作为一个 token（如 $DetectionColumns），避免逐字拆成多块难看且难拖
      const identOrDollarVar = buffer.match(/^(?:\$[A-Za-z_][\w]*|[A-Za-z_][\w]*)/);
      if (identOrDollarVar) {
        result.push({ type: 'text', value: identOrDollarVar[0] });
        buffer = buffer.slice(identOrDollarVar[0].length);
      } else {
        result.push({ type: 'text', value: char });
        buffer = buffer.slice(1);
      }
    }
  }
  tokens.value = result;
};

function removeToken(index: number) { tokens.value.splice(index, 1); }
function clearFormula() { tokens.value = []; }
function insertField(field: IntermediateDataColumnInfo) {
  // 检查是否为范围列
  if (field.isRange) {
    // 范围列自动生成 RANGE() 语法，分解为多个token
    const prefix = field.rangePrefix || field.columnName;
    const start = field.rangeStart ?? 1;
    // rangeEnd = -1 表示动态列数
    const end = (field.rangeEnd === -1 || field.rangeEnd === null || field.rangeEnd === undefined)
      ? '$DetectionColumns'
      : String(field.rangeEnd);

    // 分解为多个token: RANGE ( prefix , start , end )
    tokens.value.push({ type: 'function', value: 'RANGE' });
    tokens.value.push({ type: 'operator', value: ' ( ' });
    tokens.value.push({ type: 'field', value: prefix, label: `${field.displayName} (${prefix})` });
    tokens.value.push({ type: 'operator', value: ' , ' });
    tokens.value.push({ type: 'number', value: String(start) });
    tokens.value.push({ type: 'operator', value: ' , ' });
    tokens.value.push({ type: 'text', value: end });
    tokens.value.push({ type: 'operator', value: ' ) ' });
  } else {
    tokens.value.push({
      type: 'field',
      value: `[${field.columnName}]`,
      label: `${field.displayName} (${field.columnName})`
    });
  }
}
function insertOperator(op: string) {
  let val = op;
  if (op === '×') val = '*';
  if (op === '÷') val = '/';
  tokens.value.push({ type: 'operator', value: ` ${val} ` });
}
function insertFunction(func: any) { tokens.value.push({ type: 'function', value: func.name }); }
function insertNumber() {
  if (!manualNumber.value) return;
  tokens.value.push({ type: 'number', value: manualNumber.value });
  manualNumber.value = '';
}
function insertTemplate(temp: any) {
  const current = formulaText.value;
  const newFull = current + temp.template;
  parseFormulaToTokens(newFull);
}
function handleDragStart(event: DragEvent, field: IntermediateDataColumnInfo) {
  // 如果是范围列，传递范围信息用于分解
  if (field.isRange) {
    const prefix = field.rangePrefix || field.columnName;
    const start = field.rangeStart ?? 1;
    const end = (field.rangeEnd === -1 || field.rangeEnd === null || field.rangeEnd === undefined)
      ? '$DetectionColumns'
      : String(field.rangeEnd);

    event.dataTransfer?.setData('text/plain', JSON.stringify({
      type: 'range',
      prefix: prefix,
      start: start,
      end: end,
      displayName: field.displayName
    }));
  } else {
    event.dataTransfer?.setData('text/plain', JSON.stringify({
      type: 'field',
      value: `[${field.columnName}]`,
      label: `${field.displayName} (${field.columnName})`
    }));
  }
  event.dataTransfer!.effectAllowed = 'copy';
}
function handleTokenDragStart(event: DragEvent, index: number) {
  event.dataTransfer?.setData('text/plain', JSON.stringify({ type: 'token', index: index }));
  event.dataTransfer!.effectAllowed = 'move';
}
function handleTokenDrop(event: DragEvent, targetIndex: number) {
  event.preventDefault();
  event.stopPropagation();
  try {
    const dataStr = event.dataTransfer?.getData('text/plain');
    if (dataStr) {
      const data = JSON.parse(dataStr);
      if (data.type === 'token') {
        const oldIndex = data.index;
        if (oldIndex === targetIndex) return;
        const token = tokens.value[oldIndex];
        tokens.value.splice(oldIndex, 1);
        let newIndex = targetIndex;
        if (oldIndex < targetIndex) { newIndex -= 1; }
        tokens.value.splice(targetIndex, 0, token);
      } else if (data.type === 'field') {
        tokens.value.splice(targetIndex, 0, {
          type: 'field',
          value: data.value,
          label: data.label || data.value
        });
      } else if (data.type === 'range') {
        // 分解为多个token插入
        const rangeTokens: Token[] = [
          { type: 'function', value: 'RANGE' },
          { type: 'operator', value: ' ( ' },
          { type: 'field', value: data.prefix, label: `${data.displayName} (${data.prefix})` },
          { type: 'operator', value: ' , ' },
          { type: 'number', value: String(data.start) },
          { type: 'operator', value: ' , ' },
          { type: 'text', value: data.end },
          { type: 'operator', value: ' ) ' },
        ];
        tokens.value.splice(targetIndex, 0, ...rangeTokens);
      }
    }
  } catch (e) { }
  dragOverIndex.value = null;
}
function handleDrop(event: DragEvent) {
  event.preventDefault();
  try {
    const dataStr = event.dataTransfer?.getData('text/plain');
    if (dataStr) {
      const data = JSON.parse(dataStr);
      if (data.type === 'field') {
        tokens.value.push({
          type: 'field',
          value: data.value,
          label: data.label || data.value
        });
      } else if (data.type === 'range') {
        // 分解为多个token推入
        tokens.value.push({ type: 'function', value: 'RANGE' });
        tokens.value.push({ type: 'operator', value: ' ( ' });
        tokens.value.push({ type: 'field', value: data.prefix, label: `${data.displayName} (${data.prefix})` });
        tokens.value.push({ type: 'operator', value: ' , ' });
        tokens.value.push({ type: 'number', value: String(data.start) });
        tokens.value.push({ type: 'operator', value: ' , ' });
        tokens.value.push({ type: 'text', value: data.end });
        tokens.value.push({ type: 'operator', value: ' ) ' });
      } else if (data.type === 'token') {
        const oldIndex = data.index;
        const token = tokens.value[oldIndex];
        tokens.value.splice(oldIndex, 1);
        tokens.value.push(token);
      }
    }
  } catch (e) { }
  dragOverIndex.value = null;
}
function focusEditor() { }

function enterFormulaFullscreen() {
  setModalProps({ defaultFullscreen: true });
  nextTick(() => redoModalHeight());
}

// --- 提交 ---
function handleSubmit() {
  let formula = '';

  if (editorMode.value === 'range') {
    formula = generatedFormula.value;
  } else {
    formula = formulaText.value;
  }

  emit('save', {
    id: formulaId.value,
    formula: formula
  });
  closeModal();
}

function handleCancel() { closeModal(); }

</script>

<style lang="less" scoped>
// 变量
@color-primary: #1890ff;
@color-bg-gray: #f7f8fa;
@color-border: #eef0f5;

@block-field-bg: #e6f7ff;
@block-field-text: #1890ff;
@block-field-border: #91d5ff;

@block-func-bg: #f9f0ff;
@block-func-text: #722ed1;
@block-func-border: #d3adf7;

@block-op-bg: #fff7e6;
@block-op-text: #fa8c16;
@block-op-border: #ffd591;

@block-num-bg: #f6ffed;
@block-num-text: #52c41a;
@block-num-border: #b7eb8f;

// ========== 范围列配置模式 ==========
.range-formula-config {
  display: flex;
  flex-direction: column;
  max-height: ~'calc(100vh - 160px)';
  padding: 20px 20px 16px 0;
  background: #fff;

  .range-formula-form {
    flex: 1;
    min-height: 0;
    overflow: visible;
    display: flex;
    flex-direction: column;
    padding: 0 20px 20px 0;
  }

  .range-formula-layout {
    display: flex;
    gap: 20px;
    flex: 1 1 auto;
    min-height: auto;
  }

  .range-config-main,
  .range-config-side {
    min-height: 0;
    overflow: visible;
  }

  .range-config-main {
    flex: 1;
    min-width: 0;
    padding-right: 4px;
  }

  .range-config-side {
    width: 340px;
    flex-shrink: 0;
    padding-left: 20px;
    border-left: 1px solid #eef0f5;
  }

  .range-config-side > * + * {
    margin-top: 12px;
  }

  .form-hint {
    font-size: 12px;
    color: #999;
    margin-top: 8px;
    line-height: 1.6;
  }

  .radio-option {
    display: flex;
    align-items: center;
    gap: 8px;
  }

  .operation-option,
  .dynamic-option {
    display: flex;
    align-items: center;
    gap: 8px;
  }

  .formula-preview-card {
    background: #f9f9f9;
    border: 1px solid #e8e8e8;
    border-radius: 8px;
    padding: 12px;

    .preview-header {
      display: flex;
      align-items: center;
      gap: 8px;
      font-weight: 600;
      color: #333;
      margin-bottom: 12px;
      font-size: 14px;
    }

    .formula-code {
      background: #1e1e1e;
      color: #4ec9b0;
      padding: 12px 16px;
      border-radius: 6px;
      font-family: 'Consolas', monospace;
      font-size: 14px;
      margin-bottom: 12px;
      word-break: break-all;
    }

    .formula-desc {
      display: flex;
      align-items: start;
      gap: 8px;
      padding: 10px 12px;
      background: #e6f7ff;
      border-radius: 6px;
      color: #0969da;
      font-size: 13px;
      line-height: 1.6;
    }
  }
}

// ========== 普通公式编辑器 (保持原样式) ==========
.formula-builder {
  display: flex;
  gap: 12px;
  max-height: ~'calc(100vh - 120px)';
  min-height: 520px;
  background: white;
  padding: 0 0 16px;
}

.panel {
  display: flex;
  flex-direction: column;
  height: 100%;
}

.left-panel {
  width: 200px;
  flex-shrink: 0;
  border-right: 1px solid @color-border;
  padding-right: 10px;
}

.right-panel {
  width: 220px;
  flex-shrink: 0;
  border-left: 1px solid @color-border;
  padding-left: 10px;
}

.center-panel {
  flex: 1;
  min-width: 0;
  padding: 0 4px;
}

.panel-header {
  display: flex;
  align-items: center;
  gap: 8px;
  margin-bottom: 12px;
  font-weight: 600;

  .step-badge {
    display: inline-flex;
    justify-content: center;
    align-items: center;
    width: 20px;
    height: 20px;
    border-radius: 50%;
    background: #e6f7ff;
    color: #1890ff;
    font-size: 12px;
    font-weight: bold;

    &.orange {
      background: #fff7e6;
      color: #fa8c16;
    }

    &.purple {
      background: #f9f0ff;
      color: #722ed1;
    }
  }

  .panel-title {
    font-size: 14px;
    color: #333;
  }

  .header-tip {
    font-size: 11px;
    color: #999;
    font-weight: normal;
    margin-left: auto;
  }

  &.sm-header {
    margin-bottom: 8px;

    .panel-title {
      font-size: 13px;
      color: #666;
    }
  }
}

.fields-list {
  flex: 1;
  overflow-y: auto;
  padding-right: 4px;
}

.search-box {
  padding: 0 4px 12px 0;
}

.field-card {
  display: flex;
  align-items: center;
  gap: 12px;
  padding: 8px 12px;
  background: white;
  border: 1px solid #f0f0f0;
  border-radius: 6px;
  margin-bottom: 6px;
  cursor: grab;
  transition: all 0.2s;

  &:hover {
    border-color: @color-primary;
    box-shadow: 0 2px 8px rgba(0, 0, 0, 0.08);
    transform: translateY(-1px);

    .add-icon {
      color: @color-primary;
      background: #e6f7ff;
    }
  }

  .field-icon-wrapper {
    width: 28px;
    height: 28px;
    background: #e6f7ff;
    border-radius: 6px;
    display: flex;
    align-items: center;
    justify-content: center;
    color: @color-primary;
    font-size: 14px;
  }

  .field-info {
    flex: 1;
    display: flex;
    flex-direction: column;

    .field-name {
      font-size: 12px;
      color: #333;
      font-weight: 500;
    }

    .field-key {
      font-size: 10px;
      color: #999;
    }
  }

  .add-icon {
    width: 18px;
    height: 18px;
    display: flex;
    align-items: center;
    justify-content: center;
    color: #ccc;
    border-radius: 4px;
    font-size: 16px;
    transition: all 0.2s;
  }
}

.manual-input-section {
  margin-top: 16px;
  padding: 12px;
  background: #fcfcfc;
  border: 1px solid #f0f0f0;
  border-radius: 8px;

  .section-label {
    font-size: 12px;
    color: #666;
    margin-bottom: 8px;
  }

  .input-group {
    display: flex;
    gap: 8px;

    .mini-input {
      flex: 1;
    }

    .add-btn {
      background: #f0f0f0;
      border: none;

      &:hover {
        background: #e6f7ff;
        color: @color-primary;
      }
    }
  }
}

.tip-box {
  background: #fffbe6;
  border: 1px solid #ffe58f;
  border-radius: 6px;
  padding: 8px 12px;
  font-size: 12px;
  color: #d46b08;
  display: flex;
  gap: 8px;
  align-items: start;
  margin-bottom: 20px;

  .code {
    font-family: monospace;
    background: rgba(0, 0, 0, 0.05);
    padding: 0 4px;
    border-radius: 3px;
  }
}

.editor-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 12px;

  .title-group {
    display: flex;
    align-items: center;
    gap: 8px;
    font-weight: bold;
    color: #333;
  }

  .editor-header-actions {
    display: flex;
    align-items: center;
    flex-wrap: wrap;
    justify-content: flex-start;
    gap: 4px;
  }

  .fullscreen-entry-btn {
    padding: 0 6px;
  }
}

.block-editor {
  flex: 1;
  border: 1px solid #d9d9d9;
  border-radius: 8px;
  background: #fafafa;
  padding: 16px;
  margin-bottom: 16px;
  overflow-y: auto;
  cursor: text;
  transition: all 0.2s;
  min-height: 360px;

  &:hover,
  &:focus-within {
    border-color: @color-primary;
    background: white;
    box-shadow: 0 0 0 2px rgba(24, 144, 255, 0.1);
  }
}

.blocks-container {
  display: flex;
  flex-wrap: wrap;
  gap: 8px;
  align-items: center;
  align-content: flex-start;
}

.formula-block {
  padding: 4px 10px;
  border-radius: 4px;
  font-size: 13px;
  font-family: 'Consolas', monospace;
  cursor: pointer;
  display: inline-flex;
  align-items: center;
  gap: 6px;
  border: 1px solid transparent;
  transition: all 0.2s;
  user-select: none;

  &:hover {
    transform: scale(1.05);

    .remove-x {
      opacity: 1;
    }
  }

  .remove-x {
    font-size: 14px;
    opacity: 0.5;
    font-weight: bold;
    margin-left: 2px;
  }
}

.field-block {
  background: @block-field-bg;
  color: @block-field-text;
  border-color: @block-field-border;
}

.operator-block {
  background: @block-op-bg;
  color: @block-op-text;
  border-color: @block-op-border;
  font-weight: bold;
}

.function-block {
  background: @block-func-bg;
  color: @block-func-text;
  border-color: @block-func-border;
  font-weight: bold;
}

.number-block {
  background: @block-num-bg;
  color: @block-num-text;
  border-color: @block-num-border;
}

.text-block {
  background: #f5f5f5;
  color: #666;
  border: 1px solid #ddd;
  white-space: nowrap;
}

// 条件差值公式块 - 使用特殊的渐变样式
.conditional-diff-block {
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  color: #fff;
  border: none;
  padding: 8px 14px;
  font-size: 12px;
  font-weight: 500;
  box-shadow: 0 2px 8px rgba(102, 126, 234, 0.3);
  
  .conditional-icon {
    margin-right: 6px;
    font-size: 14px;
  }

  .remove-x {
    color: rgba(255, 255, 255, 0.8);
    &:hover {
      color: #fff;
      background: rgba(255, 255, 255, 0.2);
    }
  }

  &:hover {
    box-shadow: 0 4px 12px rgba(102, 126, 234, 0.5);
    transform: translateY(-1px);
  }
}

// 拖拽悬停样式
.drag-over {
  position: relative;

  &::before {
    content: '';
    position: absolute;
    left: -4px;
    top: 0;
    bottom: 0;
    width: 3px;
    background: @color-primary;
    border-radius: 2px;
    animation: pulse 0.8s ease-in-out infinite;
  }

  transform: scale(1.05);
  box-shadow: 0 2px 8px rgba(24, 144, 255, 0.3);
}

@keyframes pulse {

  0%,
  100% {
    opacity: 1;
  }

  50% {
    opacity: 0.5;
  }
}

.placeholder-text {
  color: #bbb;
  font-style: italic;
  width: 100%;
  text-align: center;
  margin-top: 40px;
  pointer-events: none;
}

.preview-section {
  border: 1px solid #f0f0f0;
  border-radius: 6px;
  overflow: hidden;

  .section-label {
    background: #f9f9f9;
    color: #999;
    font-size: 11px;
    padding: 6px 12px;
    border-bottom: 1px solid #f0f0f0;
  }

  .preview-box {
    padding: 10px 12px;
    font-family: monospace;
    color: #333;
    font-size: 13px;
    background: white;
    min-height: 80px;
    white-space: pre-wrap;
    word-break: break-all;
  }
}

.panel-section {
  margin-bottom: 16px;
}

.operators-grid {
  display: grid;
  grid-template-columns: repeat(4, 1fr);
  gap: 8px;

  &.syntax-grid {
    grid-template-columns: repeat(3, 1fr);
  }

  .op-btn {
    height: 32px;
    border: 1px solid #e8e8e8;
    background: white;
    border-radius: 4px;
    font-size: 14px;
    color: #666;
    cursor: pointer;
    transition: all 0.2s;

    &:hover {
      color: @color-primary;
      border-color: @color-primary;
      background: #e6f7ff;
    }
  }
}

.functions-list-detailed {
  display: flex;
  flex-direction: column;
  gap: 6px;

  .func-item {
    display: flex;
    align-items: center;
    justify-content: space-between;
    padding: 6px 12px;
    background: #f9f0ff;
    border: 1px solid #d3adf7;
    border-radius: 4px;
    cursor: pointer;
    transition: all 0.2s;

    &:hover {
      border-color: #722ed1;
      background: #f0e6fa;
    }

    .func-name {
      font-weight: bold;
      color: #722ed1;
      font-size: 13px;
    }

    .func-desc {
      font-size: 11px;
      color: #b37feb;
    }
  }
}

.template-card {
  border: 1px solid #f0f0f0;
  border-radius: 6px;
  padding: 10px;
  cursor: pointer;
  transition: all 0.2s;
  margin-bottom: 8px;

  &:hover {
    border-color: @color-primary;
    box-shadow: 0 4px 12px rgba(0, 0, 0, 0.05);
  }

  .temp-title {
    font-size: 12px;
    font-weight: 500;
    color: #333;
    margin-bottom: 2px;
  }

  .temp-desc {
    font-size: 10px;
    color: #999;
  }
}

.modal-footer {
  display: flex;
  justify-content: flex-end;
  gap: 12px;
  padding-top: 10px;

  .footer-btn {
    height: 32px;
    border-radius: 4px;
    padding: 0 20px;
  }
}

.custom-scroll {
  &::-webkit-scrollbar {
    width: 4px;
  }

  &::-webkit-scrollbar-thumb {
    background: #ddd;
    border-radius: 4px;
  }

  &::-webkit-scrollbar-track {
    background: transparent;
  }
}

.formula-code-formatted {
  background: #1e1e3f;
  color: #a9b7c6;
  padding: 16px;
  border-radius: 8px;
  font-family: 'Fira Code', 'Consolas', monospace;
  font-size: 13px;
  line-height: 1.6;
  overflow-x: auto;
  margin: 12px 0;
  white-space: pre;
}

.formula-logic-box {
  background: #f0f5ff;
  border: 1px solid #d6e4ff;
  border-radius: 8px;
  padding: 12px 16px;
  margin-top: 12px;

  .logic-item {
    display: flex;
    align-items: center;
    gap: 12px;
    padding: 6px 0;

    &:not(:last-child) {
      border-bottom: 1px dashed #d6e4ff;
    }

    .logic-label {
      background: @color-primary;
      color: #fff;
      padding: 2px 10px;
      border-radius: 4px;
      font-size: 12px;
      font-weight: 500;
      min-width: 60px;
      text-align: center;
    }

    .logic-value {
      color: #333;
      font-size: 13px;
      font-weight: 500;
    }
  }
}

.formula-mode-switch {
  display: flex;
  justify-content: flex-start;
  padding: 0 0 16px;
}

.spec-range-table {
  border: 1px solid #dbe3ef;
  border-radius: 12px;
  overflow: hidden;
  background: #fff;
}

.spec-range-table__head,
.spec-range-table__row {
  display: grid;
  grid-template-columns: 80px 56px 152px minmax(200px, 1fr);
  gap: 12px;
  align-items: start;
  padding: 4px 8px;
}

.spec-range-table__head {
  background: #f5f8fc;
  color: #64748b;
  font-size: 12px;
  font-weight: 600;
  border-bottom: 1px solid #e5edf8;
}

.spec-range-table__head span:nth-child(1),
.spec-range-table__head span:nth-child(2),
.spec-range-table__head span:nth-child(3) {
  text-align: center;
}

.spec-range-table__row {
  width: 100%;
  border: none;
  border-bottom: 1px solid #eef2f7;
  background: #fff;
  text-align: left;
  cursor: pointer;
  transition: background 0.2s ease, box-shadow 0.2s ease;

  &:last-child {
    border-bottom: none;
  }

  &:hover {
    background: #f9fbff;
  }

  &.active {
    background: #f0f7ff;
    box-shadow: inset 3px 0 0 #1677ff;
  }
}

.cell {
  color: #0f172a;
  font-size: 13px;
  line-height: 1.5;
  word-break: break-all;
}

.cell-name {
  font-weight: 600;
  text-align: center;
}

.cell-columns {
  color: #1677ff;
  font-weight: 600;
  white-space: nowrap;
  text-align: center;
}

.range-edit-inline {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  gap: 6px;
  font-family: inherit;
  width: 100%;
}

.cell-range-input,
.cell-sample {
  display: flex;
  align-items: center;
}

.cell-range-input {
  justify-content: center;
}

.range-text-input {
  width: 100%;
}

.range-text-input :deep(.ant-input-number-input) {
  text-align: center;
  padding: 0 6px;
  font-size: 12px;
}

.range-text-input {
  max-width: 68px;
}

.range-text-input :deep(.ant-input-number-disabled .ant-input-number-input) {
  background: #f8fafc;
  color: #0f172a;
  cursor: default;
}

.sample-text {
  display: inline-block;
  color: #334155;
  font-size: 11px;
  line-height: 1.4;
  word-break: break-all;
}

.range-edit-sep {
  color: #64748b;
  font-size: 12px;
}

.range-template-tabs {
  display: flex;
  flex-wrap: wrap;
  gap: 8px;
}

.preset-count-row {
  display: flex;
  align-items: center;
  gap: 12px;
  margin-top: 12px;
}

.preset-count-label {
  color: #64748b;
  font-size: 12px;
  white-space: nowrap;
}

.preset-count-input {
  width: 120px;
}

.simulation-card {
  border: 1px solid #e5edf8;
  border-radius: 12px;
  padding: 16px 18px 18px;
  background: linear-gradient(180deg, #ffffff 0%, #f9fbfe 100%);
}

.simulation-subtitle {
  margin-bottom: 12px;
  color: #0f172a;
  font-size: 13px;
  font-weight: 600;
}

.simulation-values {
  display: flex;
  flex-wrap: nowrap;
  gap: 10px;
  overflow-x: auto;
  overflow-y: hidden;
  padding: 2px 4px 8px 0;
}

.sample-input-item {
  display: flex;
  flex-direction: column;
  gap: 6px;
  flex: 0 0 78px;
}

.sample-label {
  color: #64748b;
  font-size: 12px;
  white-space: nowrap;
}

.simulation-hint {
  margin-top: 12px;
  color: #64748b;
  font-size: 12px;
  line-height: 1.6;
}

.simulation-result-card {
  display: flex;
  flex-direction: column;
  gap: 8px;
  background: #fff;
  border: 1px solid #eef2f7;
  border-radius: 10px;
  padding: 10px;
}

.simulation-result-row {
  display: flex;
  align-items: flex-start;
  gap: 10px;
}

.simulation-result-label {
  flex-shrink: 0;
  width: 64px;
  color: #64748b;
  font-size: 12px;
  font-weight: 500;
  line-height: 1.6;
}

.simulation-result-value {
  flex: 1;
  color: #1677ff;
  font-size: 18px;
  font-weight: 700;
  line-height: 1.4;
}

.simulation-result-text {
  flex: 1;
  color: #334155;
  font-size: 12px;
  line-height: 1.6;
  word-break: break-all;
}

@media screen and (max-width: 1280px) {
  .range-formula-config {
    .range-formula-layout {
      flex-direction: column;
    }

    .range-config-side {
      width: 100%;
      padding-left: 0;
      border-left: none;
      border-top: 1px solid #eef0f5;
      padding-top: 16px;
    }
  }

  .spec-range-table {
    overflow-x: auto;
  }

  .spec-range-table__head,
  .spec-range-table__row {
    min-width: 680px;
  }

}

@media screen and (max-width: 960px) {
  .simulation-values {
    gap: 8px;
  }
}
</style>

<style lang="less">
/* 全屏时拉长三栏编辑区（类名由 BasicModal 挂在 .ant-modal 上，与 wrap 上的 fullscreen-modal 组合使用） */
.fullscreen-modal .ant-modal.formula-builder-modal .formula-builder {
  height: ~'calc(100vh - 200px)';
  min-height: 360px;
}

.fullscreen-modal .ant-modal.formula-builder-modal .range-formula-config {
  height: ~'calc(100vh - 200px)';
  min-height: 360px;
}

.ant-modal.formula-builder-modal {
  max-width: ~'calc(100vw - 16px)';
}
</style>
