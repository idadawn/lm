<template>
  <BasicModal v-bind="$attrs" @register="registerModal" :title="modalTitle" @ok="handleSubmit" @cancel="handleCancel"
    :width="'min(1420px, calc(100vw - 32px))'" :minHeight="380" :canFullscreen="true" class="formula-builder-modal">

    <!-- 检测是否为范围列,显示不同的编辑器 -->
    <template v-if="isRangeColumn">
      <!-- 范围列配置模式 -->
      <div class="range-formula-config">
        <a-alert message="范围计算列配置" description="此列将对多个检测列或带厚列进行统计计算,自动适配不同产品的实际列数" type="info" show-icon
          style="margin-bottom: 20px" />

        <a-form :model="rangeConfig" layout="vertical" style="padding: 0 20px">
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
            <div class="form-hint">
              检测数据列: F_DETECTION_1 ~ F_DETECTION_22<br>
              带厚列: F_THICK_1 ~ F_THICK_22
            </div>
          </a-form-item>

          <!-- 范围配置 -->
          <a-form-item label="数据范围" required>
            <a-row :gutter="16">
              <a-col :span="12">
                <a-input-number v-model:value="rangeConfig.start" :min="1" :max="22" size="large" style="width: 100%">
                  <template #addonBefore>从第</template>
                  <template #addonAfter>列</template>
                </a-input-number>
              </a-col>
              <a-col :span="12">
                <a-select v-model:value="rangeConfig.end" size="large" placeholder="选择结束列" style="width: 100%">
                  <a-select-opt-group label="🔢 固定列号">
                    <a-select-option v-for="i in 22" :key="i" :value="String(i)">
                      到第{{ i }}列
                    </a-select-option>
                  </a-select-opt-group>

                  <a-select-opt-group label="⚡ 动态引用">
                    <a-select-option value="$DetectionColumns">
                      <div class="dynamic-option">
                        <Icon icon="ant-design:thunderbolt-filled" :size="14" style="color: #faad14" />
                        <span>到检测列字段的值</span>
                        <a-tag color="blue" size="small">推荐</a-tag>
                      </div>
                    </a-select-option>
                  </a-select-opt-group>
                </a-select>
              </a-col>
            </a-row>

            <a-alert v-if="rangeConfig.end === '$DetectionColumns'" message="动态范围说明"
              description="系统会根据每条数据的 DetectionColumns 字段值自动确定范围。例如: DetectionColumns=15 则计算第1-15列; DetectionColumns=22 则计算第1-22列"
              type="success" show-icon style="margin-top: 12px" />
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
              <a-select-option value="CONDITIONAL_DIFF">
                <div class="operation-option">
                  <Icon icon="ant-design:branch-outlined" :size="16" />
                  <span>条件差值 (IF判断)</span>
                </div>
              </a-select-option>
            </a-select>
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

          <!-- CONDITIONAL_DIFF 配置 -->
          <template v-if="rangeConfig.operation === 'CONDITIONAL_DIFF'">
            <a-divider>条件差值配置</a-divider>
            <a-alert message="计算逻辑" type="info" show-icon style="margin-bottom: 16px">
              <template #description>
                <div>如果中间列平均值 &lt; 两边列平均值，则返回：中间列最小值 - 两边列最大值</div>
                <div>否则返回：中间列最大值 - 两边列最小值</div>
              </template>
            </a-alert>
            <a-row :gutter="16">
              <a-col :span="12">
                <a-form-item label="中间列起始">
                  <a-input-number v-model:value="rangeConfig.middleStart" :min="1" :max="22" size="large" style="width: 100%" />
                </a-form-item>
              </a-col>
              <a-col :span="12">
                <a-form-item label="中间列结束">
                  <a-input-number v-model:value="rangeConfig.middleEnd" :min="1" :max="22" size="large" style="width: 100%" />
                </a-form-item>
              </a-col>
            </a-row>
            <a-row :gutter="16">
              <a-col :span="12">
                <a-form-item label="左边列起始">
                  <a-input-number v-model:value="rangeConfig.leftStart" :min="1" :max="22" size="large" style="width: 100%" />
                </a-form-item>
              </a-col>
              <a-col :span="12">
                <a-form-item label="左边列结束">
                  <a-input-number v-model:value="rangeConfig.leftEnd" :min="1" :max="22" size="large" style="width: 100%" />
                </a-form-item>
              </a-col>
            </a-row>
            <a-row :gutter="16">
              <a-col :span="12">
                <a-form-item label="右边列起始">
                  <a-input-number v-model:value="rangeConfig.rightStart" :min="1" :max="22" size="large" style="width: 100%" />
                </a-form-item>
              </a-col>
              <a-col :span="12">
                <a-form-item label="右边列结束">
                  <a-select v-model:value="rangeConfig.rightEnd" size="large" style="width: 100%">
                    <a-select-option value="$DetectionColumns">动态 ($DetectionColumns)</a-select-option>
                    <a-select-option v-for="i in 22" :key="i" :value="String(i)">{{ i }}</a-select-option>
                  </a-select>
                </a-form-item>
              </a-col>
            </a-row>
          </template>

          <!-- 公式预览 -->
          <a-divider />
          <div class="formula-preview-card">
            <div class="preview-header">
              <Icon icon="ant-design:code-outlined" :size="16" />
              <span>生成的公式</span>
            </div>
            <!-- 条件差值使用格式化显示 -->
            <template v-if="rangeConfig.operation === 'CONDITIONAL_DIFF'">
              <pre class="formula-code-formatted">IF(
  AVG({{ getMiddleRange() }}) &lt; AVG({{ getSidesRange() }}),
  MIN({{ getMiddleRange() }}) - MAX({{ getSidesRange() }}),
  MAX({{ getMiddleRange() }}) - MIN({{ getSidesRange() }})
)</pre>
              <div class="formula-logic-box">
                <div class="logic-item">
                  <span class="logic-label">中间列</span>
                  <span class="logic-value">{{ rangeConfig.prefix }}{{ rangeConfig.middleStart }} ~ {{ rangeConfig.prefix }}{{ rangeConfig.middleEnd }}</span>
                </div>
                <div class="logic-item">
                  <span class="logic-label">两边列</span>
                  <span class="logic-value">{{ rangeConfig.prefix }}{{ rangeConfig.leftStart }} ~ {{ rangeConfig.prefix }}{{ rangeConfig.leftEnd }} 和 {{ rangeConfig.prefix }}{{ rangeConfig.rightStart }} ~ {{ rangeConfig.rightEnd === '$DetectionColumns' ? '动态列数' : rangeConfig.prefix + rangeConfig.rightEnd }}</span>
                </div>
              </div>
            </template>
            <template v-else>
              <div class="formula-code">{{ generatedFormula }}</div>
            </template>
            <div class="formula-desc">
              <Icon icon="ant-design:bulb-outlined" :size="14" />
              <span>{{ getFormulaDescription() }}</span>
            </div>
          </div>
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
          <div class="tip-box">
            <Icon icon="ant-design:info-circle-outlined" />
            <span>除法提示: 您可以使用标准的 <span class="code">IF(分母 &lt;&gt; 0, ...)</span> 模式。如果数据库支持,使用 <span
                class="code">SAFE_DIVIDE</span> 或将默认值设为 1 会更简洁。</span>
          </div>

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

                <!-- 条件差值公式使用特殊样式 -->
                <span v-else-if="token.type === 'function' && token.label?.startsWith('条件差值')" 
                  class="formula-block conditional-diff-block"
                  :class="{ 'drag-over': dragOverIndex === index }" 
                  draggable="true"
                  @dragstart="handleTokenDragStart($event, index)" 
                  @drop.stop="handleTokenDrop($event, index)"
                  @dragover.prevent 
                  @dragenter="dragOverIndex = index" 
                  @dragleave="dragOverIndex = null"
                  @click.stop="removeToken(index)">
                  <span class="conditional-icon">📊</span>
                  {{ token.label }}
                  <span class="remove-x">×</span>
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

          <div class="panel-section mt-auto">
            <div class="panel-header">
              <span class="panel-title">常用逻辑模板</span>
            </div>
            <div class="template-card" v-for="temp in templates" :key="temp.name" @click="insertTemplate(temp)">
              <div class="temp-title">{{ temp.name }}</div>
              <div class="temp-desc">{{ temp.description }}</div>
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
import { ref, computed, nextTick } from 'vue';
import { BasicModal, useModalInner } from '/@/components/Modal';
import { Icon } from '/@/components/Icon';
import { getAvailableColumns } from '/@/api/lab/intermediateDataFormula';
import type { IntermediateDataColumnInfo } from '/@/api/lab/types/intermediateDataFormula';

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

const emit = defineEmits(['register', 'save']);

// --- 状态 ---
const modalTitle = ref('公式构建器');
const formulaId = ref('');
const isRangeColumn = ref(false);  // 是否为范围列
const availableFields = ref<IntermediateDataColumnInfo[]>([]);
const searchQuery = ref('');
const manualNumber = ref('');
const tokens = ref<Token[]>([]);
const dragOverIndex = ref<number | null>(null);  // 拖拽悬停位置

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

// 生成范围公式
const generatedFormula = computed(() => {
  const { operation, prefix, end, firstN, lastN, middleStart, middleEnd, leftStart, leftEnd, rightStart, rightEnd } = rangeConfig.value;

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

  return `${operation}(RANGE(${prefix}, 1, ${end}))`;
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

// --- Modal Init ---
const [registerModal, { setModalProps, closeModal, redoModalHeight }] = useModalInner(async (data) => {
  setModalProps({ confirmLoading: false, defaultFullscreen: false });
  formulaId.value = data?.record?.id || '';

  // 检查是否为范围列
  isRangeColumn.value = data?.record?.isRange === true;

  if (data?.record) {
    modalTitle.value = `编辑公式:${data.record.formulaName || ''} (${data.record.columnName || ''})`;
  } else {
    modalTitle.value = '公式构建器';
  }

  await loadAvailableFields();

  const initFormula = data?.record?.formula || '';

  if (isRangeColumn.value && initFormula) {
    // 解析范围公式
    parseRangeFormula(initFormula);
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
  }
});

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

  if (isRangeColumn.value) {
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
  padding: 20px;
  max-height: ~'min(520px, calc(100vh - 240px))';
  overflow-y: auto;

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
    padding: 16px;

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
  height: ~'min(520px, calc(100vh - 240px))';
  min-height: 300px;
  background: white;
  padding: 0;
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
    justify-content: flex-end;
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
  min-height: 200px;

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
    min-height: 40px;
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
</style>

<style lang="less">
/* 全屏时拉长三栏编辑区（类名由 BasicModal 挂在 .ant-modal 上，与 wrap 上的 fullscreen-modal 组合使用） */
.fullscreen-modal .ant-modal.formula-builder-modal .formula-builder {
  height: ~'calc(100vh - 200px)';
  min-height: 360px;
}

.ant-modal.formula-builder-modal {
  max-width: ~'calc(100vw - 16px)';
}
</style>