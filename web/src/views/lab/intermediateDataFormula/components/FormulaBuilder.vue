<template>
  <BasicModal
    v-bind="$attrs"
    @register="registerModal"
    :title="modalTitle"
    @ok="handleSubmit"
    @cancel="handleCancel"
    :width="1200"
    :minHeight="650"
    class="formula-builder-modal">
    <div class="formula-builder">
      <!-- ... -->
      <div class="panel left-panel">
        <div class="panel-header">
          <span class="step-badge">1</span>
          <span class="panel-title">可用字段</span>
        </div>
        <div class="search-box">
           <a-input v-model:value="searchQuery" placeholder="搜索字段..." allowClear size="small">
              <template #prefix>
                <span class="search-icon">🔍</span>
              </template>
           </a-input>
        </div>
        <div class="fields-list custom-scroll">
          <div
            v-for="field in filteredFields"
            :key="field.columnName"
            class="field-card"
            draggable="true"
            @dragstart="handleDragStart($event, field)"
            @click="insertField(field)">
            <div class="field-icon-wrapper">
              <span class="icon">📄</span>
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

      <!-- 中间：公式编辑器 -->
      <div class="panel center-panel">
        <div class="tip-box">
          <span class="tip-icon">ℹ️</span>
          <span>除法提示: 您可以使用标准的 <span class="code">IF(分母 &lt;&gt; 0, ...)</span> 模式。如果数据库支持，使用 <span class="code">SAFE_DIVIDE</span> 或将默认值设为 1 会更简洁。</span>
        </div>
        
        <div class="editor-header">
          <div class="title-group">
            <span class="editor-icon">📝</span>
            <span class="panel-title">公式编辑器</span>
          </div>
          <a-button type="link" danger size="small" @click="clearFormula">清空全部</a-button>
        </div>

        <div
          class="block-editor"
          @drop="handleDrop"
          @dragover.prevent
          @click="focusEditor">
          
          <div class="blocks-container">
            <template v-for="(token, index) in tokens" :key="index">
              <!-- 字段块 (蓝色) -->
              <span 
                v-if="token.type === 'field'" 
                class="formula-block field-block" 
                draggable="true"
                @dragstart="handleTokenDragStart($event, index)"
                @drop.stop="handleTokenDrop($event, index)"
                @dragover.prevent
                @click.stop="removeToken(index)">
                {{ token.label || token.value }}
                <span class="remove-x">×</span>
              </span>

              <!-- 运算符块 (橙色) -->
              <span 
                v-else-if="token.type === 'operator'" 
                class="formula-block operator-block" 
                draggable="true"
                @dragstart="handleTokenDragStart($event, index)"
                @drop.stop="handleTokenDrop($event, index)"
                @dragover.prevent
                @click.stop="removeToken(index)">
                 {{ token.value }} 
              </span>

              <!-- 函数块 (紫色) -->
              <span 
                v-else-if="token.type === 'function'" 
                class="formula-block function-block" 
                draggable="true"
                @dragstart="handleTokenDragStart($event, index)"
                @drop.stop="handleTokenDrop($event, index)"
                @dragover.prevent
                @click.stop="removeToken(index)">
                {{ token.value }}
              </span>

              <!-- 数字块 (绿色) -->
              <span 
                v-else-if="token.type === 'number'" 
                class="formula-block number-block" 
                draggable="true"
                @dragstart="handleTokenDragStart($event, index)"
                @drop.stop="handleTokenDrop($event, index)"
                @dragover.prevent
                @click.stop="removeToken(index)">
                {{ token.value }}
                <span class="remove-x">×</span>
              </span>

               <!-- 其他文本 (灰色) -->
              <span 
                v-else 
                class="formula-block text-block" 
                draggable="true"
                @dragstart="handleTokenDragStart($event, index)"
                @drop.stop="handleTokenDrop($event, index)"
                @dragover.prevent
                @click.stop="removeToken(index)">
                {{ token.value }}
              </span>
            </template>
            
            <div v-if="tokens.length === 0" class="placeholder-text">
              请将字段拖拽至此处，或点击右侧运算符构建公式
            </div>
          </div>

        </div>

        <div class="preview-section">
          <div class="section-label">原始公式预览</div>
          <div class="preview-box">
            {{ formulaText }}
          </div>
        </div>
      </div>

      <!-- 右侧：运算符和函数 -->
      <div class="panel right-panel">
        <div class="panel-section">
          <div class="panel-header">
            <span class="step-badge orange">2</span>
            <span class="panel-title">基础运算</span>
          </div>
          <div class="operators-grid">
            <button v-for="op in basicOperators" :key="op" class="op-btn" @click="insertOperator(op)">{{ op }}</button>
          </div>
        </div>

        <div class="panel-section">
          <div class="panel-header">
            <span class="step-badge orange">3</span>
            <span class="panel-title">语法结构</span>
          </div>
          <div class="operators-grid syntax-grid">
            <button v-for="op in syntaxOperators" :key="op" class="op-btn" @click="insertOperator(op)">
                {{ op === ',' ? '，' : (op === 'TO' ? '至' : op) }}
            </button>
          </div>
        </div>
        
         <div class="panel-section">
          <div class="panel-header sm-header">
            <span class="panel-title">比较运算</span>
          </div>
          <div class="operators-grid">
            <button v-for="op in comparisonOperators" :key="op" class="op-btn" @click="insertOperator(op)">{{ op }}</button>
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

    <template #footer>
      <div class="modal-footer">
        <a-button @click="handleCancel" class="footer-btn">取消</a-button>
        <a-button type="primary" class="footer-btn" @click="handleSubmit">保存单据</a-button>
      </div>
    </template>
  </BasicModal>
</template>

<script lang="ts" setup>
import { ref, computed } from 'vue';
import { BasicModal, useModalInner } from '/@/components/Modal';
// import { useMessage } from '/@/hooks/web/useMessage'; // Unused
import { getAvailableColumns } from '/@/api/lab/intermediateDataFormula';
import type { IntermediateDataColumnInfo } from '/@/api/lab/types/intermediateDataFormula';

// --- 类型定义 ---
type TokenType = 'field' | 'operator' | 'function' | 'number' | 'text';
interface Token {
  type: TokenType;
  value: string;
  label?: string; // 用于显示（如：列卖价 (L3)）
}

const emit = defineEmits(['register', 'save']);

// --- 状态 ---
const modalTitle = ref('公式构建器');
const formulaId = ref('');
const availableFields = ref<IntermediateDataColumnInfo[]>([]);
const filteredFields = computed(() => {
    if (!searchQuery.value) return availableFields.value;
    const query = searchQuery.value.toLowerCase();
    return availableFields.value.filter(f => 
        f.displayName.toLowerCase().includes(query) || 
        f.columnName.toLowerCase().includes(query)
    );
});
const searchQuery = ref('');
const manualNumber = ref('');
const tokens = ref<Token[]>([]);

// --- 计算属性：还原公式文本 ---
const formulaText = computed(() => {
  return tokens.value.map(t => t.value).join(''); 
});

// --- 常量定义 ---
const basicOperators = ['+', '-', '×', '÷'];
const syntaxOperators = ['(', ')', ','];
const comparisonOperators = ['=', '<>', '>', '<'];

// Combine for parsing
const allOperators = [...basicOperators, ...syntaxOperators, ...comparisonOperators];


const functions = [
  { name: 'SUM', value: 'SUM(', type: 'function', description: '统计' },
  { name: 'AVG', value: 'AVG(', type: 'function', description: '统计' },
  { name: 'MAX', value: 'MAX(', type: 'function', description: '统计' },
  { name: 'MIN', value: 'MIN(', type: 'function', description: '统计' },
  { name: 'IF', value: 'IF(', type: 'function', description: '逻辑' },
];
const templates = [
  { name: '多字段求和 (SUM)', template: 'SUM([字段1], [字段2])', description: '计算多个字段的总和' },
  { name: '动态范围统计 (TO)', template: 'SUM([起始列] TO [检测列])', description: '从起始列统计至目标列(如检测列)' },
  { name: '安全除法 (IF)', template: 'IF([分母] <> 0, [分子] / [分母], 0)', description: '防止除以零的错误' },
];

// --- 核心逻辑：Tokenizer ---
const parseFormulaToTokens = (formula: string) => {
  const result: Token[] = [];
  let buffer = formula;
  
  while (buffer.length > 0) {
    // 匹配字段 [xxx]
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

    // 匹配数字
    const numMatch = buffer.match(/^\d+(\.\d+)?/);
    if (numMatch) {
      result.push({ type: 'number', value: numMatch[0] });
      buffer = buffer.slice(numMatch[0].length);
      continue;
    }

    // 匹配函数
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

    // 匹配运算符
    let opMatched = false;
    // 检查实际运算符 * /
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
    
    // 特殊处理 TO (需要前后空格，或者作为单词匹配)
    // 简单起见，作为 operator 匹配，只要 startsWith
    // 注意 TO 也是 text. 最好识别 'TO ' 或 strict match if we have spaces
    // formula usually has spaces around TO: " [A] TO [B] "
    
    // Check TO specifically to ensure it's parsed as operator not text
    if (buffer.startsWith('TO ') || (buffer.startsWith('TO') && (buffer.length === 2 || [' ', '('].includes(buffer[2])))) {
        result.push({ type: 'operator', value: ' TO ' }); // Normalize
        buffer = buffer.slice(2);
        continue;
    }

    // 其他常规运算符
    for (const op of allOperators) {
        if (op === 'TO') continue; // Handled above roughly, or let loop handle it?
        // Loop handle is fine if order is correct. 'TO' length 2.
        // If we have variable starting with TO... but variables are [TO].
        
        if (op === '×' || op === '÷') continue; 
        if (buffer.startsWith(op)) {
            result.push({ type: 'operator', value: ` ${op} ` }); 
            buffer = buffer.slice(op.length);
            opMatched = true;
            break;
        }
    }
    if (opMatched) continue;

    // 其他
    const char = buffer[0];
    if (char.trim() === '') {
        const spaceMatch = buffer.match(/^\s+/);
        if (spaceMatch) {
             buffer = buffer.slice(spaceMatch[0].length);
        } else {
             buffer = buffer.slice(1);
        }
    } else {
         result.push({ type: 'text', value: char });
         buffer = buffer.slice(1);
    }
  }
  tokens.value = result;
};


// --- Modal Init ---
const [registerModal, { setModalProps, closeModal }] = useModalInner(async (data) => {
  setModalProps({ confirmLoading: false });
  formulaId.value = data?.record?.id || '';
  
  if (data?.record) {
      modalTitle.value = `编辑公式：${data.record.formulaName || ''} (${data.record.columnName || ''})`;
  } else {
      modalTitle.value = '公式构建器';
  }
  
  // 初始化 tokens
  const initFormula = data?.record?.formula || '';
  // 先加载字段，再解析，为了能正确显示 label
  await loadAvailableFields();
  
  if (initFormula) {
      parseFormulaToTokens(initFormula);
  } else {
      tokens.value = [];
  }
});

const loadAvailableFields = async () => {
    try {
        const res: any = await getAvailableColumns(true);
        availableFields.value = res.data || res || [];
    } catch (e) {}
};


// --- 操作 ---
const removeToken = (index: number) => {
    tokens.value.splice(index, 1);
};

const clearFormula = () => {
    tokens.value = [];
};

const insertField = (field: IntermediateDataColumnInfo) => {
    tokens.value.push({ 
        type: 'field', 
        value: `[${field.columnName}]`,
        label: `${field.displayName} (${field.columnName})`
    });
};

const insertOperator = (op: string) => {
    let val = op;
    if (op === '×') val = '*';
    if (op === '÷') val = '/';
    // 逗号特殊处理 display? No, just space
    
    tokens.value.push({ type: 'operator', value: ` ${val} ` });
};

const insertFunction = (func: any) => {
    tokens.value.push({ type: 'function', value: func.name });
};

const insertNumber = () => {
    if (!manualNumber.value) return;
    tokens.value.push({ type: 'number', value: manualNumber.value });
    manualNumber.value = '';
};

const insertTemplate = (temp: any) => {
    // Append template logic
    const current = formulaText.value;
    const newFull = current + temp.template;
    parseFormulaToTokens(newFull);
};

const handleDragStart = (event: DragEvent, field: IntermediateDataColumnInfo) => {
  event.dataTransfer?.setData('text/plain', JSON.stringify({
      type: 'field',
      value: `[${field.columnName}]`,
      label: `${field.displayName} (${field.columnName})` 
  }));
  event.dataTransfer!.effectAllowed = 'copy';
};

const handleTokenDragStart = (event: DragEvent, index: number) => {
    event.dataTransfer?.setData('text/plain', JSON.stringify({
        type: 'token',
        index: index
    }));
    event.dataTransfer!.effectAllowed = 'move';
};

const handleTokenDrop = (event: DragEvent, targetIndex: number) => {
    event.preventDefault();
    event.stopPropagation(); // Prevent container drop
    
    try {
        const dataStr = event.dataTransfer?.getData('text/plain');
        if (dataStr) {
            const data = JSON.parse(dataStr);
            
            // Case 1: Reordering existing token
            if (data.type === 'token') {
                const oldIndex = data.index;
                if (oldIndex === targetIndex) return;
                
                const token = tokens.value[oldIndex];
                tokens.value.splice(oldIndex, 1);
                // Adjust target index if we removed an item before it
                let newIndex = targetIndex;
                if (oldIndex < targetIndex) {
                    newIndex -= 1;
                }
                // Insert after the target (or before? usually "drop on" means "replace" or "insert before")
                // Let's assume "insert before" makes most sense for a list
                tokens.value.splice(targetIndex, 0, token);
            }
            // Case 2: Dropping new field
            else if (data.type === 'field') {
               tokens.value.splice(targetIndex, 0, { 
                   type: 'field', 
                   value: data.value,
                   label: data.label || data.value
               });
            }
        }
    } catch(e) {}
};

const handleDrop = (event: DragEvent) => {
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
          } else if (data.type === 'token') {
               // Move to end
               const oldIndex = data.index;
               const token = tokens.value[oldIndex];
               tokens.value.splice(oldIndex, 1);
               tokens.value.push(token);
          }
      }
  } catch(e) {}
};

const focusEditor = () => {
};

// Removed handleValidate

const handleSubmit = () => {
    emit('save', {
        id: formulaId.value,
        formula: formulaText.value
    });
    closeModal();
};

const handleCancel = () => closeModal();

</script>

<style lang="less" scoped>
// 变量
@color-primary: #1890ff;
@color-bg-gray: #f7f8fa;
@color-border: #eef0f5;
@card-shadow: 0 4px 12px rgba(0, 0, 0, 0.05);

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

.formula-builder {
  display: flex;
  gap: 20px;
  height: 580px;
  background: white;
  padding: 0;
}

.panel {
  display: flex;
  flex-direction: column;
  height: 100%;
}

.left-panel {
  width: 260px;
  border-right: 1px solid @color-border;
  padding-right: 16px;
}

.right-panel {
  width: 300px;
  border-left: 1px solid @color-border;
  padding-left: 16px;
}

.center-panel {
  flex: 1;
  padding: 0 8px;
}

// 通用 Header
.panel-header {
  display: flex;
  align-items: center;
  gap: 8px;
  margin-bottom: 12px; // Reduced margin
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
    
    &.orange { background: #fff7e6; color: #fa8c16; }
    &.purple { background: #f9f0ff; color: #722ed1; }
  }
  
  .panel-title {
    font-size: 14px; // Slightly smaller
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
      .panel-title { font-size: 13px; color: #666; }
  }
}

// 左侧：字段列表
.fields-list {
  flex: 1;
  overflow-y: auto;
  padding-right: 4px;
}

.search-box {
    padding: 0 4px 12px 0;
    
    .search-icon {
        color: #999;
        margin-right: 4px;
        font-size: 12px;
    }
}

.field-card {
  display: flex;
  align-items: center;
  gap: 12px;
  padding: 8px 12px; // Reduced padding
  background: white;
  border: 1px solid #f0f0f0;
  border-radius: 6px;
  margin-bottom: 6px;
  cursor: grab;
  transition: all 0.2s;
  
  &:hover {
    border-color: @color-primary;
    box-shadow: 0 2px 8px rgba(0,0,0,0.08);
    transform: translateY(-1px);
    
    .add-icon {
        color: @color-primary;
        background: #e6f7ff;
    }
  }
  
  .field-icon-wrapper {
    width: 28px; // Smaller icon
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

// 手动输入
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
            &:hover { background: #e6f7ff; color: @color-primary; }
        }
    }
}


// 中间：编辑器
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
        background: rgba(0,0,0,0.05);
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
    
    &:hover, &:focus-within {
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


// 右侧面板
.panel-section {
    margin-bottom: 16px; // Reduced spacing
}

.operators-grid {
    display: grid;
    grid-template-columns: repeat(4, 1fr);
    gap: 8px;
    
    &.syntax-grid {
        grid-template-columns: repeat(3, 1fr);
    }
    
    .op-btn {
        height: 32px; // Smaller buttons
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
        border: 1px solid #d3adf7; // Transparent border initially
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
        box-shadow: 0 4px 12px rgba(0,0,0,0.05);
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
    
    // Validate button removed
}

/* 滚动条美化 */
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
</style>
