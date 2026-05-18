<template>
  <transition name="slide">
    <div class="kg-panel" v-if="panel">
      <div class="panel-head">
        <span class="panel-type-badge" :style="{ background: panel.color }">{{ panel.typeLabel }}</span>
        <span class="panel-label" :title="panel.label">{{ panel.label }}</span>
        <a-button type="text" size="small" @click="$emit('close')" class="panel-close">
          <template #icon><CloseOutlined /></template>
        </a-button>
      </div>
      <a-divider style="margin: 8px 0" />

      <!-- 炉号解析规则详情 -->
      <template v-if="panel.type === 'furnaceNo'">
        <div class="prop-row"><b>名称:</b> {{ panel.label }}</div>
        <div class="prop-row"><b>说明:</b> {{ panel.raw?.description || '炉号解析' }}</div>

        <!-- 原始炉号：显示格式规则、正则、示例 -->
        <template v-if="panel.typeLabel === 'FurnaceNoInput'">
          <div class="section-title">格式规则</div>
          <div class="prop-row" style="font-size:12px;color:#475569;background:#f8fafc;padding:8px;border-radius:6px">
            <div><b>标准格式：</b></div>
            <div>[产线数字][班次汉字][8位日期]-[炉次号]-[卷号]-[分卷号][特殊标记W/w][特性描述]</div>
            <div style="margin-top:4px"><b>示例：</b>1甲20251101-1-4-1W脆</div>
          </div>

          <div class="section-title">正则表达式</div>
          <div class="prop-row" style="font-size:11px;color:#64748b;background:#f1f5f9;padding:8px;border-radius:6px;word-break:break-all">
            ^\s*(\d+)(.*?)(\d{8})\s*-\s*(\d+)\s*-\s*(\d+(?:\.\d+)?)\s*-\s*(\d+(?:\.\d+)?)\s*([Ww]?)\s*([...]*)\s*$
          </div>

          <div class="section-title">解析字段（正则分组）</div>
          <div class="attr-grid">
            <div class="attr-row"><span class="attr-key">Group 1</span><span class="attr-val">产线号，如：1, 2, 3</span></div>
            <div class="attr-row"><span class="attr-key">Group 2</span><span class="attr-val">班次，甲=1, 乙=2, 丙=3</span></div>
            <div class="attr-row"><span class="attr-key">Group 3</span><span class="attr-val">生产日期，格式：yyyyMMdd</span></div>
            <div class="attr-row"><span class="attr-key">Group 4</span><span class="attr-val">炉次号，如：1, 2...</span></div>
            <div class="attr-row"><span class="attr-key">Group 5</span><span class="attr-val">卷号，支持小数</span></div>
            <div class="attr-row"><span class="attr-key">Group 6</span><span class="attr-val">分卷号，支持小数</span></div>
            <div class="attr-row"><span class="attr-key">Group 7</span><span class="attr-val">特殊标记，W/w</span></div>
            <div class="attr-row"><span class="attr-key">Group 8</span><span class="attr-val">特性描述，如：脆, 硬</span></div>
          </div>

          <div class="section-title">忽略后缀</div>
          <div class="prop-row" style="font-size:12px;color:#475569">
            复测、（复测）、(复测) — 解析时自动去除
          </div>

          <div class="section-title">磁性数据炉号</div>
          <div class="prop-row" style="font-size:12px;color:#475569;background:#f8fafc;padding:8px;border-radius:6px">
            <div><b>格式：</b>[产线数字][班次汉字][8位日期]-[炉号][是否刻痕K]</div>
            <div style="margin-top:4px"><b>示例：</b>1甲20251101-1, 1甲20251101-1K</div>
            <div style="margin-top:4px"><b>说明：</b>K 后缀表示刻痕标记，解析后 IsScratched=true</div>
          </div>

          <div class="section-title">数据库字段</div>
          <div class="attr-grid">
            <div class="attr-row"><span class="attr-key">原始炉号</span><span class="attr-val" style="font-size:11px;color:#94a3b8">F_FURNACE_NO</span></div>
            <div class="attr-row"><span class="attr-key">原始炉号</span><span class="attr-val" style="font-size:11px;color:#94a3b8">F_ORIGINAL_FURNACE_NO</span></div>
          </div>
        </template>

        <!-- 炉号：显示组成部分和字符含义 -->
        <template v-if="panel.typeLabel === 'FurnaceNoParsed'">
          <div class="section-title">炉号组成部分</div>
          <div class="prop-row" style="font-size:12px;color:#475569;background:#f8fafc;padding:8px;border-radius:6px">
            <div><b>完整示例：</b>1甲20251101-1-4-1W脆</div>
            <div style="margin-top:4px"><b>标准格式（无特性）：</b>1甲20251101-1-4-1</div>
            <div style="margin-top:4px"><b>标准炉号（批次号）：</b>1甲20251101-1</div>
          </div>

          <div class="section-title">字符含义</div>
          <div class="attr-grid">
            <div class="attr-row"><span class="attr-key">1</span><span class="attr-val">产线号（第1条生产线）</span></div>
            <div class="attr-row"><span class="attr-key">甲</span><span class="attr-val">班次（甲班=1，乙班=2，丙班=3）</span></div>
            <div class="attr-row"><span class="attr-key">20251101</span><span class="attr-val">生产日期（2025年11月1日）</span></div>
            <div class="attr-row"><span class="attr-key">1</span><span class="attr-val">炉次号（当日第1炉）</span></div>
            <div class="attr-row"><span class="attr-key">4</span><span class="attr-val">卷号（该炉第4卷）</span></div>
            <div class="attr-row"><span class="attr-key">1</span><span class="attr-val">分卷号（该卷第1分卷）</span></div>
            <div class="attr-row"><span class="attr-key">W</span><span class="attr-val">特殊标记（W或w）</span></div>
            <div class="attr-row"><span class="attr-key">脆</span><span class="attr-val">特性描述（材质特性，如脆、硬）</span></div>
            <div class="attr-row"><span class="attr-key">-</span><span class="attr-val">分隔符（固定格式）</span></div>
          </div>

          <div class="section-title">辅助方法</div>
          <div class="prop-row" style="font-size:12px;color:#475569">
            <div>• <b>GetFullFurnaceNo()</b> — 完整炉号（含特性描述）</div>
            <div>• <b>GetFurnaceNo()</b> — 基础炉号（不含特性，含W标记）</div>
            <div>• <b>GetStandardFurnaceNo()</b> — 标准炉号/批次号（不含W和特性）</div>
            <div>• <b>GetBatchNo()</b> — 批次：[产线][班次][日期]-[炉次号]</div>
            <div>• <b>GetSprayNo()</b> — 喷次：[日期]-[炉次号]</div>
          </div>

          <div class="section-title">数据库字段</div>
          <div class="attr-grid">
            <div class="attr-row"><span class="attr-key">炉号（格式化）</span><span class="attr-val" style="font-size:11px;color:#94a3b8">F_FURNACE_NO_FORMATTED</span></div>
            <div class="attr-row"><span class="attr-key">炉号（解析后）</span><span class="attr-val" style="font-size:11px;color:#94a3b8">F_FURNACE_NO_PARSED</span></div>
          </div>
        </template>

        <!-- 炉号解析字段：显示具体字段详情 -->
        <template v-if="panel.typeLabel === 'FurnaceNoField'">
          <div class="section-title">字段信息</div>
          <div class="attr-grid">
            <div class="attr-row"><span class="attr-key">中文名称</span><span class="attr-val">{{ panel.label }}</span></div>
            <div class="attr-row"><span class="attr-key">C# 属性</span><span class="attr-val">{{ panel.raw?.field || '-' }}</span></div>
            <div class="attr-row"><span class="attr-key">示例值</span><span class="attr-val">{{ panel.raw?.example || '-' }}</span></div>
            <div class="attr-row"><span class="attr-key">正则分组</span><span class="attr-val">{{ furnaceFieldGroup(String(panel.raw?.field || '')) }}</span></div>
          </div>

          <div class="section-title">数据库字段映射</div>
          <div class="attr-grid">
            <div class="attr-row" v-for="dbField in furnaceDbFieldMap(String(panel.raw?.field || ''))" :key="dbField.column">
              <span class="attr-key">{{ dbField.comment }}</span>
              <span class="attr-val" style="font-size:11px;color:#94a3b8">{{ dbField.column }}</span>
            </div>
          </div>
        </template>
      </template>

      <!-- 规格详情 / 导入模板详情 -->
      <template v-if="panel.type === 'spec'">
        <!-- ProductSpec 单条规格：明确字段标注 -->
        <template v-if="panel.typeLabel === 'ProductSpec'">
          <div class="prop-row">
            <b>规格代码:</b> {{ panel.raw?.code || '-' }}
            <small class="f-col-hint">F_CODE</small>
          </div>
          <div class="prop-row">
            <b>规格名称:</b> {{ panel.raw?.name || '-' }}
            <small class="f-col-hint">F_NAME</small>
          </div>
          <div class="prop-row">
            <b>叠片检测数据列:</b> {{ panel.raw?.detection_columns || panel.raw?.description || '-' }}
            <small class="f-col-hint">F_DETECTION_COLUMNS</small>
          </div>
          <div class="prop-row" v-if="panel.raw?.version">
            <b>当前版本:</b> <a-tag>{{ panel.raw.version }}</a-tag>
            <small class="f-col-hint">F_VERSION</small>
          </div>
        </template>
        <template v-else>
          <div class="prop-row"><b>编码:</b> {{ panel.raw?.template_code || panel.raw?.code || '-' }}</div>
          <div class="prop-row"><b>名称:</b> {{ panel.raw?.template_name || panel.raw?.name || '-' }}</div>
          <div class="prop-row" v-if="panel.raw?.description"><b>节点说明:</b> {{ panel.raw.description }}</div>
        </template>
        <div class="prop-row" v-if="panel.raw?.targetTable"><b>目标表:</b> {{ panel.raw.targetTable }}</div>
        <div class="prop-row" v-if="panel.ruleCount !== undefined && panel.ruleCount !== null">
          <b>规则数:</b> <a-tag color="blue">{{ panel.ruleCount }}</a-tag>
        </div>
        <div class="prop-row" v-if="panel.formulaCount !== undefined && panel.formulaCount !== null">
          <b>公式数:</b> <a-tag color="purple">{{ panel.formulaCount }}</a-tag>
        </div>

        <!-- 导入模板 / 中间数据：表字段列表 -->
        <template v-if="panel.typeLabel === 'RawDataImport' || panel.typeLabel === 'MagneticDataImport' || panel.typeLabel === 'IntermediateData'">
          <div class="section-title">表字段 ({{ (panel.specs || []).length }})</div>
          <div class="attr-grid">
            <div class="attr-row" v-for="s in (panel.specs || [])" :key="s.id">
              <span class="attr-key">{{ s.name }}</span>
              <span class="attr-val">{{ s.code }} <small style="color:#94a3b8">{{ s.description }}</small></span>
            </div>
          </div>
          <div v-if="!(panel.specs || []).length" class="prop-row" style="color:#94a3b8">暂无表字段</div>

          <!-- 导入模板字段映射 -->
          <div class="section-title" v-if="(panel.products || []).length > 0">导入模板字段映射 ({{ (panel.products || []).length }})</div>
          <div class="attr-grid" v-if="(panel.products || []).length > 0">
            <div class="attr-row" v-for="p in (panel.products || [])" :key="p.id">
              <span class="attr-key">{{ p.name }}</span>
              <span class="attr-val">{{ p.code || '-' }} <small style="color:#94a3b8">{{ p.description }}</small></span>
            </div>
          </div>
        </template>

        <!-- 普通规格：扩展信息（来自 lab_product_spec_attribute，按 F_PRODUCT_SPEC_ID + F_VERSION 过滤）-->
        <template v-else>
          <div class="section-title">扩展信息 ({{ (panel.specs || []).length }})</div>
          <div class="attr-grid">
            <div class="attr-row" v-for="s in (panel.specs || [])" :key="s.id">
              <span class="attr-key">{{ s.name }}</span>
              <span class="attr-val">
                {{ s.code || '-' }}
                <small v-if="s.description" style="color:#94a3b8;margin-left:4px">{{ s.description }}</small>
              </span>
            </div>
          </div>
          <div v-if="!(panel.specs || []).length" class="prop-row" style="color:#94a3b8">暂无扩展信息</div>
        </template>

        <!-- 动作 -->
        <div class="section-title">动作</div>
        <a-space direction="vertical" style="width:100%">
          <a-button type="primary" size="small" block @click="$emit('action', { type: 'explore', target: panel.raw?.id })">
            在图谱中展开
          </a-button>
          <a-button size="small" block @click="$emit('action', { type: 'records', specCode: panel.raw?.code })">
            查看检测记录
          </a-button>
        </a-space>
      </template>

      <!-- 规则聚合详情 -->
      <template v-if="panel.type === 'ruleCombo'">
        <div class="prop-row"><b>规格:</b> {{ panel.specName || '通用' }}</div>
        <div class="prop-row"><b>状态:</b>
          <a-tag :color="panel.qualityStatus === '合格' ? 'success' : 'error'">{{ panel.qualityStatus }}</a-tag>
        </div>
        <div class="prop-row"><b>规则数:</b> {{ panel.rules?.length || 0 }}</div>
        <div class="section-title">规则列表</div>
        <div class="rule-list">
          <div class="rule-item" v-for="r in panel.rules" :key="r.id"
               :class="{ 'rule-item-active': selectedRuleId === r.id }"
               @click="selectRule(r)">
            <span class="rule-name">{{ r.name }}</span>
            <span class="rule-priority">P{{ r.priority }}</span>
          </div>
        </div>
      </template>

      <!-- 单条规则详情 -->
      <template v-if="panel.type === 'rule'">
        <a-button type="link" size="small" @click="$emit('back')" style="padding:0;margin-bottom:4px">
          ← 返回列表
        </a-button>
        <div class="prop-row"><b>等级:</b> {{ panel.raw?.name }}</div>
        <div class="prop-row"><b>代码:</b> {{ panel.raw?.code || '-' }}</div>
        <div class="prop-row"><b>质量状态:</b>
          <a-tag :color="panel.raw?.quality_status === '合格' ? 'success' : 'error'">{{ panel.raw?.quality_status }}</a-tag>
        </div>
        <div class="prop-row"><b>优先级:</b> {{ panel.raw?.priority }}</div>
        <div class="prop-row" v-if="panel.raw?.product_spec_name"><b>规格:</b> {{ panel.raw.product_spec_name }}</div>
        <div class="prop-row" v-if="panel.raw?.formula_name"><b>公式:</b> {{ panel.raw.formula_name }}</div>
        <div class="prop-row" v-if="panel.raw?.description"><b>说明:</b> {{ panel.raw.description }}</div>

        <!-- 条件表格（新增） -->
        <template v-if="conditions.length > 0">
          <div class="section-title">判定条件 ({{ conditions.length }})</div>
          <div class="condition-table">
            <div class="cond-header">
              <span>字段</span>
              <span>期望</span>
              <span>实际</span>
              <span>结果</span>
            </div>
            <div class="cond-row" v-for="(c, i) in conditions" :key="i">
              <span :title="c.field">{{ c.label || c.field }}</span>
              <span>{{ c.expected }}</span>
              <span>{{ c.actual ?? '-' }}</span>
              <span>
                <a-tag v-if="c.satisfied === true" color="success" size="small">满足</a-tag>
                <a-tag v-else-if="c.satisfied === false" color="error" size="small">不满足</a-tag>
                <a-tag v-else size="small">未评估</a-tag>
              </span>
            </div>
          </div>
        </template>
      </template>

      <!-- 公式详情 -->
      <template v-if="panel.type === 'formula'">
        <div class="prop-row"><b>名称:</b> {{ panel.raw?.formula_name }}</div>
        <div class="prop-row"><b>类型:</b>
          <a-tag>{{ formulaTypeLabel }}</a-tag>
        </div>
        <div class="prop-row"><b>列名:</b> {{ panel.raw?.column_name }}</div>
        <div class="prop-row"><b>单位:</b> {{ panel.raw?.unit_name || '-' }}</div>
        <div class="section-title" v-if="panel.raw?.formula">公式</div>
        <pre class="formula-code" v-if="panel.raw?.formula">{{ panel.raw.formula }}</pre>
        <div class="section-title">关联规则 ({{ panel.ruleCount }})</div>
        <div class="rule-list">
          <div class="rule-item" v-for="r in panel.linkedRules" :key="r.id">
            <span class="rule-name">{{ r.name }}</span>
            <span class="rule-priority">P{{ r.priority }}</span>
          </div>
        </div>
      </template>

      <!-- 产品规格聚合节点：展示所有规格条目 -->
      <template v-if="panel.type === 'productSpecList'">
        <div class="prop-row"><b>节点:</b> 产品规格分类</div>
        <div class="prop-row"><b>说明:</b> {{ panel.raw?.description || '带材所属的产品规格分类' }}</div>
        <div class="prop-row"><b>规格数量:</b> <a-tag color="blue">{{ (panel.specs || []).length }}</a-tag></div>

        <div class="section-title">产品规格列表 ({{ (panel.specs || []).length }})</div>
        <div class="rule-list spec-list">
          <div
            class="rule-item spec-item"
            v-for="s in (panel.specs || [])"
            :key="s.id"
            @click="$emit('action', { type: 'selectSpec', specId: s.id, spec: s })"
          >
            <span class="rule-name">
              {{ s.name }}
              <small v-if="s.description" style="color:#94a3b8;margin-left:6px">{{ s.description }}</small>
            </span>
            <span class="rule-priority" v-if="s.code">{{ s.code }}</span>
          </div>
        </div>
        <div v-if="!(panel.specs || []).length" class="prop-row" style="color:#94a3b8">暂无产品规格</div>
      </template>

      <!-- 产品扩展信息聚合节点：展示去重后的扩展属性 -->
      <template v-if="panel.type === 'specAttributeList'">
        <div class="prop-row"><b>节点:</b> 扩展信息分类</div>
        <div class="prop-row"><b>说明:</b> {{ panel.raw?.description || '跨产品规格的扩展属性（按名称去重）' }}</div>
        <div class="prop-row"><b>属性数量:</b> <a-tag color="magenta">{{ (panel.specs || []).length }}</a-tag></div>

        <div class="section-title">扩展信息列表 ({{ (panel.specs || []).length }})</div>
        <div class="attr-grid">
          <div class="attr-row" v-for="s in (panel.specs || [])" :key="s.id">
            <span class="attr-key">{{ s.name }}</span>
            <span class="attr-val">
              <span v-if="s.code">{{ s.code }}</span>
              <small v-if="s.description" style="color:#94a3b8;margin-left:6px">{{ s.description }}</small>
            </span>
          </div>
        </div>
        <div v-if="!(panel.specs || []).length" class="prop-row" style="color:#94a3b8">暂无扩展信息</div>
      </template>

      <!-- 单条扩展属性条目（聚合节点的子节点） -->
      <template v-if="panel.type === 'specAttributeItem'">
        <div class="prop-row"><b>名称:</b> {{ panel.raw?.name || panel.raw?.attr_key || '-' }}</div>
        <div class="prop-row" v-if="panel.raw?.attr_key && panel.raw?.attr_key !== panel.raw?.name">
          <b>键:</b> <code style="background:#F1F5F9;padding:1px 4px;border-radius:3px">{{ panel.raw.attr_key }}</code>
        </div>
        <div class="prop-row" v-if="panel.raw?.value_type"><b>数据类型:</b> <a-tag>{{ panel.raw.value_type }}</a-tag></div>
        <div class="prop-row" v-if="panel.raw?.unit"><b>单位:</b> {{ panel.raw.unit }}</div>
        <div class="prop-row" v-if="panel.raw?.value !== undefined && panel.raw?.value !== null && panel.raw?.value !== ''">
          <b>示例值:</b> {{ panel.raw.value }}
        </div>
        <div class="prop-row" v-if="panel.raw?.precision_val !== undefined && panel.raw?.precision_val !== null && panel.raw?.precision_val !== ''">
          <b>精度:</b> {{ panel.raw.precision_val }}
        </div>
        <div class="prop-row" v-if="panel.raw?.specCount">
          <b>关联规格:</b> <a-tag color="blue">{{ panel.raw.specCount }} 个</a-tag>
        </div>
        <div class="prop-row" v-if="panel.raw?.version"><b>版本:</b> {{ panel.raw.version }}</div>
      </template>

      <!-- 外观特性聚合根 -->
      <template v-if="panel.type === 'appearanceFeatureRoot'">
        <div class="prop-row"><b>节点:</b> 外观特性聚合根</div>
        <div class="prop-row"><b>说明:</b> {{ panel.raw?.description }}</div>
        <div class="section-title">数据统计</div>
        <div class="attr-grid">
          <div class="attr-row">
            <span class="attr-key">特性大类</span>
            <span class="attr-val"><a-tag color="orange">{{ panel.raw?.categoryCount ?? 0 }}</a-tag></span>
          </div>
          <div class="attr-row">
            <span class="attr-key">特性条目</span>
            <span class="attr-val"><a-tag color="red">{{ panel.raw?.featureCount ?? 0 }}</a-tag></span>
          </div>
          <div class="attr-row">
            <span class="attr-key">特性等级</span>
            <span class="attr-val"><a-tag color="purple">{{ panel.raw?.levelCount ?? 0 }}</a-tag></span>
          </div>
        </div>
        <div class="section-title">来源表</div>
        <div class="attr-grid">
          <div class="attr-row"><span class="attr-key">大类</span><span class="attr-val"><small class="f-col-hint">LAB_APPEARANCE_FEATURE_CATEGORY</small></span></div>
          <div class="attr-row"><span class="attr-key">特性</span><span class="attr-val"><small class="f-col-hint">LAB_APPEARANCE_FEATURE</small></span></div>
          <div class="attr-row"><span class="attr-key">等级</span><span class="attr-val"><small class="f-col-hint">LAB_APPEARANCE_FEATURE_LEVEL</small></span></div>
        </div>
      </template>

      <!-- 单个特性大类：显示大类信息 + 旗下特性 -->
      <template v-if="panel.type === 'appearanceCategoryItem'">
        <div class="prop-row">
          <b>大类名称:</b> {{ panel.raw?.name || '-' }}
          <small class="f-col-hint">F_NAME</small>
        </div>
        <div class="prop-row" v-if="panel.raw?.description">
          <b>描述:</b> {{ panel.raw.description }}
          <small class="f-col-hint">F_DESCRIPTION</small>
        </div>
        <div class="prop-row" v-if="panel.raw?.parent_id">
          <b>父级ID:</b> {{ panel.raw.parent_id }}
          <small class="f-col-hint">F_PARENTID</small>
        </div>
        <div class="prop-row" v-if="panel.raw?.root_id">
          <b>根分类ID:</b> {{ panel.raw.root_id }}
          <small class="f-col-hint">F_ROOTID</small>
        </div>
        <div class="prop-row" v-if="panel.raw?.path">
          <b>分类路径:</b> <code style="font-size:11px">{{ panel.raw.path }}</code>
          <small class="f-col-hint">F_PATH</small>
        </div>
        <div class="prop-row" v-if="panel.raw?.sort_code !== undefined && panel.raw?.sort_code !== null">
          <b>排序码:</b> {{ panel.raw.sort_code }}
          <small class="f-col-hint">F_SORTCODE</small>
        </div>

        <div class="section-title">该大类下的特性 ({{ (panel.specs || []).length }})</div>
        <div class="attr-grid">
          <div class="attr-row" v-for="s in (panel.specs || [])" :key="s.id">
            <span class="attr-key">{{ s.name }}</span>
            <span class="attr-val">
              <a-tag size="small" color="orange">{{ s.code }}</a-tag>
              <small v-if="s.description" style="color:#94a3b8;margin-left:4px">{{ s.description }}</small>
            </span>
          </div>
        </div>
        <div v-if="!(panel.specs || []).length" class="prop-row" style="color:#94a3b8">该大类下暂无特性</div>
      </template>

      <!-- 单条外观特性 -->
      <template v-if="panel.type === 'appearanceFeatureItem'">
        <div class="prop-row">
          <b>特性名称:</b> {{ panel.raw?.name || '-' }}
          <small class="f-col-hint">F_NAME</small>
        </div>
        <div class="prop-row" v-if="panel.raw?.level_name">
          <b>特性等级:</b>
          <a-tag color="orange">{{ panel.raw.level_name }}</a-tag>
          <a-tag v-if="panel.raw?.level_is_default" color="default">默认</a-tag>
          <small class="f-col-hint">F_SEVERITY_LEVEL_ID</small>
        </div>
        <div class="prop-row" v-if="panel.raw?.category_id">
          <b>所属大类ID:</b> {{ panel.raw.category_id }}
          <small class="f-col-hint">F_CATEGORY_ID</small>
        </div>
        <div class="prop-row" v-if="panel.raw?.description">
          <b>描述:</b> {{ panel.raw.description }}
          <small class="f-col-hint">F_DESCRIPTION</small>
        </div>
        <div class="prop-row" v-if="panel.raw?.sort_code !== undefined && panel.raw?.sort_code !== null">
          <b>排序码:</b> {{ panel.raw.sort_code }}
          <small class="f-col-hint">F_SORTCODE</small>
        </div>
        <div class="section-title" v-if="(panel.raw?.keywords || []).length > 0">
          关键词 ({{ (panel.raw.keywords || []).length }})
          <small class="f-col-hint">F_KEYWORDS</small>
        </div>
        <div v-if="(panel.raw?.keywords || []).length > 0" style="display:flex;flex-wrap:wrap;gap:4px">
          <a-tag v-for="kw in panel.raw.keywords" :key="kw" color="red">{{ kw }}</a-tag>
        </div>
        <!-- NLQ 同义词（aliases.json 维护，补充口语化说法） -->
        <div class="section-title" v-if="(panel.raw?.aliases || []).length > 0">
          AI 同义词 ({{ (panel.raw.aliases || []).length }})
          <small style="color:#64748B;font-weight:normal;margin-left:6px">问数召回辅助</small>
        </div>
        <div v-if="(panel.raw?.aliases || []).length > 0" style="display:flex;flex-wrap:wrap;gap:4px">
          <a-tag v-for="alias in panel.raw.aliases" :key="alias" color="cyan">{{ alias }}</a-tag>
        </div>
      </template>

      <!-- 特性等级聚合：展示所有等级 -->
      <template v-if="panel.type === 'appearanceLevelList'">
        <div class="prop-row"><b>节点:</b> 特性等级聚合</div>
        <div class="prop-row"><b>说明:</b> {{ panel.raw?.description }}</div>
        <div class="prop-row"><b>等级数量:</b> <a-tag color="purple">{{ (panel.specs || []).length }}</a-tag></div>
        <div class="section-title">等级列表 ({{ (panel.specs || []).length }})</div>
        <div class="attr-grid">
          <div class="attr-row" v-for="s in (panel.specs || [])" :key="s.id">
            <span class="attr-key">{{ s.name }}</span>
            <span class="attr-val">
              <a-tag size="small" :color="s.code === '默认' ? 'green' : s.code === '停用' ? 'default' : 'purple'">{{ s.code }}</a-tag>
              <small v-if="s.description" style="color:#94a3b8;margin-left:4px">{{ s.description }}</small>
            </span>
          </div>
        </div>
        <div v-if="!(panel.specs || []).length" class="prop-row" style="color:#94a3b8">暂无等级</div>
      </template>

      <!-- 单个特性等级：显示等级信息 + 该等级下的特性 -->
      <template v-if="panel.type === 'appearanceLevelItem'">
        <div class="prop-row">
          <b>等级名称:</b> {{ panel.raw?.name || '-' }}
          <small class="f-col-hint">F_NAME</small>
        </div>
        <div class="prop-row" v-if="panel.raw?.description">
          <b>等级描述:</b> {{ panel.raw.description }}
          <small class="f-col-hint">F_DESCRIPTION</small>
        </div>
        <div class="prop-row">
          <b>是否默认:</b>
          <a-tag :color="panel.raw?.is_default ? 'green' : 'default'">
            {{ panel.raw?.is_default ? '是' : '否' }}
          </a-tag>
          <small class="f-col-hint">F_ISDEFAULT</small>
        </div>
        <div class="prop-row">
          <b>是否启用:</b>
          <a-tag :color="panel.raw?.enabled ? 'green' : 'red'">
            {{ panel.raw?.enabled ? '启用' : '停用' }}
          </a-tag>
          <small class="f-col-hint">F_ENABLED</small>
        </div>
        <div class="prop-row" v-if="panel.raw?.sort_code !== undefined && panel.raw?.sort_code !== null">
          <b>排序码:</b> {{ panel.raw.sort_code }}
          <small class="f-col-hint">F_SORTCODE</small>
        </div>

        <div class="section-title">该等级下的特性 ({{ (panel.specs || []).length }})</div>
        <div class="attr-grid">
          <div class="attr-row" v-for="s in (panel.specs || [])" :key="s.id">
            <span class="attr-key">{{ s.name }}</span>
            <span class="attr-val">
              <a-tag size="small" color="red">{{ s.code }}</a-tag>
              <small v-if="s.description" style="color:#94a3b8;margin-left:4px">{{ s.description }}</small>
            </span>
          </div>
        </div>
        <div v-if="!(panel.specs || []).length" class="prop-row" style="color:#94a3b8">该等级下暂无特性</div>
      </template>

      <!-- 指标聚合根 -->
      <template v-if="panel.type === 'metricRoot'">
        <div class="prop-row"><b>节点:</b> 指标聚合根</div>
        <div class="prop-row">
          <b>来源表:</b> <code style="background:#F1F5F9;padding:1px 4px;border-radius:3px">lab_report_config</code>
        </div>
        <div class="prop-row">
          <b>页面入口:</b> <code style="background:#ECFDF5;padding:1px 4px;border-radius:3px">/lab/reportConfig</code>
        </div>
        <div class="prop-row"><b>说明:</b> {{ panel.raw?.description }}</div>
        <div class="section-title">指标列表 ({{ (panel.specs || []).length }})</div>
        <div class="attr-grid">
          <div class="attr-row" v-for="s in (panel.specs || [])" :key="s.id">
            <span class="attr-key">{{ s.name }}</span>
            <span class="attr-val">
              <a-tag size="small" :color="s.code === '占比' ? 'gold' : 'cyan'">{{ s.code }}</a-tag>
              <small v-if="s.description" style="color:#94a3b8;margin-left:4px">{{ s.description }}</small>
            </span>
          </div>
        </div>
        <div v-if="!(panel.specs || []).length" class="prop-row" style="color:#94a3b8">暂无指标</div>
        <div class="prop-row" style="margin-top:8px;color:#64748B;font-size:12px">
          每个指标 = 1 个 JUDGE 公式 + N 个等级名称；按公式分组聚合（占比 / 汇总 / 占比+汇总）。
        </div>

        <!-- NLQ SQL 模板库（通用） -->
        <div class="section-title" v-if="(panel.raw?.sql_templates || []).length > 0">
          NLQ SQL 模板 ({{ (panel.raw.sql_templates || []).length }})
          <small style="color:#64748B;font-weight:normal;margin-left:6px">智能问数骨架</small>
        </div>
        <a-collapse v-if="(panel.raw?.sql_templates || []).length > 0" :bordered="false" ghost size="small">
          <a-collapse-panel v-for="tpl in panel.raw.sql_templates" :key="tpl.id" :header="tpl.name">
            <div class="prop-row" v-if="tpl.description"><b>说明:</b> <small style="color:#64748B">{{ tpl.description }}</small></div>
            <div class="prop-row" v-if="(tpl.sample_questions || []).length > 0">
              <b>示例问题:</b>
              <div v-for="q in tpl.sample_questions" :key="q" style="color:#64748B;font-size:12px;margin-left:8px">· {{ q }}</div>
            </div>
            <pre class="formula-code" style="margin-top:6px">{{ tpl.sql_template }}</pre>
          </a-collapse-panel>
        </a-collapse>
      </template>

      <!-- 单条指标 -->
      <template v-if="panel.type === 'metricItem'">
        <div class="prop-row">
          <b>指标名称:</b> {{ panel.raw?.name || '-' }}
          <small class="f-col-hint">F_NAME</small>
        </div>
        <div class="prop-row" v-if="panel.raw?.formula_name_resolved || panel.raw?.formula_id">
          <b>判定列:</b>
          <a-tag color="cyan">{{ panel.raw.formula_name_resolved || panel.raw.formula_id }}</a-tag>
          <small v-if="panel.raw?.formula_column_name" style="color:#94a3b8;margin-left:4px">{{ panel.raw.formula_column_name }}</small>
          <small class="f-col-hint">F_FORMULA_ID</small>
        </div>
        <div class="section-title">
          包含等级 ({{ (panel.raw?.level_names || []).length }})
          <small class="f-col-hint">F_LEVEL_NAMES</small>
        </div>
        <div v-if="(panel.raw?.level_names || []).length > 0" style="display:flex;flex-wrap:wrap;gap:4px">
          <a-tag v-for="n in panel.raw.level_names" :key="n" color="purple">{{ n }}</a-tag>
        </div>
        <div v-else class="prop-row" style="color:#94a3b8">未配置等级</div>

        <div class="section-title">统计设置</div>
        <div class="attr-grid">
          <div class="attr-row">
            <span class="attr-key">仅占比</span>
            <span class="attr-val">
              <a-tag :color="panel.raw?.is_percentage ? 'gold' : 'default'">{{ panel.raw?.is_percentage ? '是' : '否' }}</a-tag>
              <small class="f-col-hint">F_IS_PERCENTAGE</small>
            </span>
          </div>
          <div class="attr-row">
            <span class="attr-key">显示占比</span>
            <span class="attr-val">
              <a-tag :color="panel.raw?.is_show_ratio ? 'green' : 'default'">{{ panel.raw?.is_show_ratio ? '是' : '否' }}</a-tag>
              <small class="f-col-hint">F_IS_SHOW_RATIO</small>
            </span>
          </div>
          <div class="attr-row">
            <span class="attr-key">头部展示</span>
            <span class="attr-val">
              <a-tag :color="panel.raw?.is_header ? 'blue' : 'default'">{{ panel.raw?.is_header ? '是' : '否' }}</a-tag>
              <small class="f-col-hint">F_IS_HEADER</small>
            </span>
          </div>
          <div class="attr-row">
            <span class="attr-key">报表展示</span>
            <span class="attr-val">
              <a-tag :color="panel.raw?.is_show_in_report ? 'green' : 'default'">{{ panel.raw?.is_show_in_report ? '是' : '否' }}</a-tag>
              <small class="f-col-hint">F_IS_SHOW_IN_REPORT</small>
            </span>
          </div>
          <div class="attr-row">
            <span class="attr-key">系统内置</span>
            <span class="attr-val">
              <a-tag :color="panel.raw?.is_system ? 'blue' : 'default'">{{ panel.raw?.is_system ? '是' : '否' }}</a-tag>
              <small class="f-col-hint">F_IS_SYSTEM</small>
            </span>
          </div>
          <div class="attr-row" v-if="panel.raw?.sort_order !== undefined && panel.raw?.sort_order !== null">
            <span class="attr-key">排序</span>
            <span class="attr-val">{{ panel.raw.sort_order }} <small class="f-col-hint">F_SORT_ORDER</small></span>
          </div>
        </div>
        <div class="prop-row" v-if="panel.raw?.description" style="margin-top:8px">
          <b>说明:</b> {{ panel.raw.description }}
          <small class="f-col-hint">F_DESCRIPTION</small>
        </div>
        <!-- NLQ 同义词 -->
        <div class="section-title" v-if="(panel.raw?.aliases || []).length > 0">
          AI 同义词 ({{ (panel.raw.aliases || []).length }})
          <small style="color:#64748B;font-weight:normal;margin-left:6px">问数召回辅助</small>
        </div>
        <div v-if="(panel.raw?.aliases || []).length > 0" style="display:flex;flex-wrap:wrap;gap:4px">
          <a-tag v-for="alias in panel.raw.aliases" :key="alias" color="cyan">{{ alias }}</a-tag>
        </div>

        <!-- 适用 SQL 模板 -->
        <div class="section-title" v-if="(panel.raw?.sql_templates || []).length > 0">
          适用 SQL 模板 ({{ (panel.raw.sql_templates || []).length }})
          <small style="color:#64748B;font-weight:normal;margin-left:6px">智能问数骨架</small>
        </div>
        <a-collapse v-if="(panel.raw?.sql_templates || []).length > 0" :bordered="false" ghost size="small">
          <a-collapse-panel v-for="tpl in panel.raw.sql_templates" :key="tpl.id" :header="tpl.name">
            <div class="prop-row" v-if="tpl.description"><b>说明:</b> <small style="color:#64748B">{{ tpl.description }}</small></div>
            <div class="prop-row" v-if="(tpl.sample_questions || []).length > 0">
              <b>示例问题:</b>
              <div v-for="q in tpl.sample_questions" :key="q" style="color:#64748B;font-size:12px;margin-left:8px">· {{ q }}</div>
            </div>
            <div class="prop-row" v-if="(tpl.parameters || []).length > 0">
              <b>参数:</b>
              <div v-for="p in tpl.parameters" :key="p.name" style="color:#64748B;font-size:12px;margin-left:8px">
                · <code style="background:#F1F5F9;padding:1px 4px;border-radius:3px">{{ p.name }}</code>
                <span style="color:#94A3B8">({{ p.type }}{{ p.required ? ', 必填' : '' }})</span>
                <small v-if="p.desc" style="margin-left:4px">{{ p.desc }}</small>
              </div>
            </div>
            <pre class="formula-code" style="margin-top:6px">{{ tpl.sql_template }}</pre>
          </a-collapse-panel>
        </a-collapse>
      </template>

      <!-- 判定等级聚合根 -->
      <template v-if="panel.type === 'judgmentLevelRoot'">
        <div class="prop-row"><b>节点:</b> 判定等级聚合根</div>
        <div class="prop-row">
          <b>来源表:</b> <code style="background:#F1F5F9;padding:1px 4px;border-radius:3px">lab_intermediate_data_judgment_level</code>
        </div>
        <div class="prop-row">
          <b>页面入口:</b> <code style="background:#ECFDF5;padding:1px 4px;border-radius:3px">/lab/intermediateDataJudgmentLevel</code>
        </div>
        <div class="prop-row"><b>说明:</b> {{ panel.raw?.description }}</div>
        <div class="section-title">数据统计</div>
        <div class="attr-grid">
          <div class="attr-row">
            <span class="attr-key">等级总数</span>
            <span class="attr-val"><a-tag color="purple">{{ panel.raw?.totalCount ?? 0 }}</a-tag></span>
          </div>
          <div class="attr-row">
            <span class="attr-key">规格分组</span>
            <span class="attr-val"><a-tag color="blue">{{ panel.raw?.specGroupCount ?? 0 }}</a-tag></span>
          </div>
        </div>
        <div class="prop-row" style="margin-top:8px;color:#64748B;font-size:12px">
          每条等级 = (产品规格, JUDGE 公式) 二元组下的具象判定，含质量状态/优先级/条件 JSON。
        </div>
      </template>

      <!-- 按产品规格分组的判定集 -->
      <template v-if="panel.type === 'judgmentLevelSpecGroup'">
        <div class="prop-row"><b>节点:</b> 规格判定集</div>
        <div class="prop-row">
          <b>规格:</b>
          <a-tag color="blue">{{ panel.raw?.specName || '-' }}</a-tag>
          <small class="f-col-hint" v-if="panel.raw?.specCode">{{ panel.raw.specCode }}</small>
        </div>
        <div class="prop-row"><b>等级数量:</b> <a-tag color="purple">{{ (panel.specs || []).length }}</a-tag></div>
        <div class="section-title">等级列表 ({{ (panel.specs || []).length }})</div>
        <div class="attr-grid">
          <div class="attr-row" v-for="s in (panel.specs || [])" :key="s.id">
            <span class="attr-key">{{ s.name }}</span>
            <span class="attr-val">
              <a-tag size="small" :color="s.code === '合格' ? 'success' : s.code === '不合格' ? 'error' : 'default'">
                {{ s.code || '-' }}
              </a-tag>
              <small v-if="s.description" style="color:#94a3b8;margin-left:4px">{{ s.description }}</small>
            </span>
          </div>
        </div>
        <div v-if="!(panel.specs || []).length" class="prop-row" style="color:#94a3b8">该规格暂无判定等级</div>
      </template>

      <!-- 单条判定等级 -->
      <template v-if="panel.type === 'judgmentLevelItem'">
        <div class="prop-row">
          <b>等级名称:</b> {{ panel.raw?.name || '-' }}
          <small class="f-col-hint">F_NAME</small>
        </div>
        <div class="prop-row" v-if="panel.raw?.code">
          <b>等级代码:</b> <code style="font-size:11px;background:#F1F5F9;padding:1px 4px;border-radius:3px">{{ panel.raw.code }}</code>
          <small class="f-col-hint">F_CODE</small>
        </div>
        <div class="prop-row">
          <b>质量状态:</b>
          <a-tag :color="panel.raw?.quality_status === '合格' ? 'success' : panel.raw?.quality_status === '不合格' ? 'error' : 'default'">
            {{ panel.raw?.quality_status || '-' }}
          </a-tag>
          <small class="f-col-hint">F_QUALITY_STATUS</small>
        </div>
        <div class="prop-row">
          <b>优先级:</b> <a-tag>P{{ panel.raw?.priority ?? '-' }}</a-tag>
          <small class="f-col-hint">F_PRIORITY</small>
        </div>
        <div class="prop-row" v-if="panel.raw?.formula_name || panel.raw?.formula_id">
          <b>判定公式:</b> {{ panel.raw.formula_name || panel.raw.formula_id }}
          <small class="f-col-hint">F_FORMULA_ID</small>
        </div>
        <div class="prop-row" v-if="panel.raw?.product_spec_name">
          <b>所属规格:</b>
          <a-tag color="blue">{{ panel.raw.product_spec_name }}</a-tag>
          <small v-if="panel.raw?.product_spec_code" style="color:#94a3b8;margin-left:4px">{{ panel.raw.product_spec_code }}</small>
          <small class="f-col-hint">F_PRODUCT_SPEC_ID</small>
        </div>
        <div class="prop-row" v-if="panel.raw?.color">
          <b>展示颜色:</b>
          <span style="display:inline-flex;align-items:center;gap:6px">
            <span :style="{ display: 'inline-block', width: '14px', height: '14px', borderRadius: '3px', border: '1px solid #E2E8F0', background: panel.raw.color }"></span>
            <code style="font-size:11px">{{ panel.raw.color }}</code>
          </span>
          <small class="f-col-hint">F_COLOR</small>
        </div>
        <div class="prop-row">
          <b>是否统计:</b>
          <a-tag :color="panel.raw?.is_statistic ? 'green' : 'default'">{{ panel.raw?.is_statistic ? '是' : '否' }}</a-tag>
          <small class="f-col-hint">F_IS_STATISTIC</small>
        </div>
        <div class="prop-row">
          <b>是否兜底:</b>
          <a-tag :color="panel.raw?.is_default ? 'blue' : 'default'">{{ panel.raw?.is_default ? '是' : '否' }}</a-tag>
          <small class="f-col-hint">F_IS_DEFAULT</small>
        </div>
        <div class="prop-row" v-if="panel.raw?.description">
          <b>业务说明:</b> {{ panel.raw.description }}
          <small class="f-col-hint">F_DESCRIPTION</small>
        </div>
        <div class="section-title" v-if="panel.raw?.conditionJson">
          判定条件
          <small class="f-col-hint">F_CONDITION</small>
        </div>
        <div class="condition-tree" v-if="conditionTree.length">
          <div
            v-for="(line, i) in conditionTree"
            :key="i"
            :class="['cond-line', `cond-line--${line.kind}`, `cond-depth-${line.depth}`]"
          >{{ line.text }}</div>
        </div>
      </template>

      <!-- 叠片数据（业务成品视图）：lab_intermediate_data -->
      <template v-if="panel.type === 'laminationDataView'">
        <div class="prop-row"><b>节点:</b> 业务成品视图</div>
        <div class="prop-row">
          <b>来源表:</b> <code style="background:#F1F5F9;padding:1px 4px;border-radius:3px">{{ panel.raw?.targetTable || 'lab_intermediate_data' }}</code>
        </div>
        <div class="prop-row" v-if="panel.raw?.route">
          <b>页面入口:</b> <code style="background:#ECFDF5;padding:1px 4px;border-radius:3px">{{ panel.raw.route }}</code>
        </div>
        <div class="prop-row"><b>说明:</b> {{ panel.raw?.description }}</div>

        <div class="section-title">数据源（sourcedFrom）</div>
        <div class="attr-grid">
          <div class="attr-row">
            <span class="attr-key">原始叠片导入</span>
            <span class="attr-val">
              <a-tag color="green" size="small">lab_raw_data</a-tag>
              <small style="color:#94a3b8;margin-left:4px">{{ panel.raw?.rawSourceName || '-' }}</small>
            </span>
          </div>
          <div class="attr-row">
            <span class="attr-key">单片性能</span>
            <span class="attr-val">
              <a-tag color="orange" size="small">lab_magnetic_raw_data</a-tag>
              <small style="color:#94a3b8;margin-left:4px">{{ panel.raw?.magneticSourceName || '-' }}</small>
            </span>
          </div>
        </div>

        <div class="section-title">列定义（computedBy → 公式聚合根）</div>
        <div class="attr-grid">
          <div class="attr-row">
            <span class="attr-key">计算公式 CALC</span>
            <span class="attr-val"><a-tag color="blue">{{ panel.raw?.formulaCounts?.calc ?? 0 }}</a-tag></span>
          </div>
          <div class="attr-row">
            <span class="attr-key">判定公式 JUDGE</span>
            <span class="attr-val"><a-tag color="orange">{{ panel.raw?.formulaCounts?.judge ?? 0 }}</a-tag></span>
          </div>
          <div class="attr-row">
            <span class="attr-key">只展示 NO</span>
            <span class="attr-val"><a-tag color="default">{{ panel.raw?.formulaCounts?.no ?? 0 }}</a-tag></span>
          </div>
        </div>
        <div class="prop-row" style="margin-top:8px;color:#64748B;font-size:12px">
          每一列的定义见「公式」聚合根；判定结果列的等级规则见 JUDGE 类型组。
        </div>

        <!-- NLQ 维度元数据 -->
        <template v-if="panel.raw?.dimensions">
          <div class="section-title">
            NLQ 维度元数据
            <small style="color:#64748B;font-weight:normal;margin-left:6px">智能问数解析依据</small>
          </div>

          <!-- 时间维度 -->
          <div class="prop-row" v-if="panel.raw.dimensions.time">
            <b>时间字段:</b> <code style="background:#F1F5F9;padding:1px 4px;border-radius:3px">{{ panel.raw.dimensions.time.field }}</code>
          </div>
          <div class="prop-row" v-if="panel.raw.dimensions.time?.min_date || panel.raw.dimensions.time?.max_date">
            <b>数据范围:</b>
            <small style="color:#64748B">
              {{ (panel.raw.dimensions.time.min_date || '').slice(0,10) }} ~
              {{ (panel.raw.dimensions.time.max_date || '').slice(0,10) }}
              <a-tag size="small" color="cyan">{{ panel.raw.dimensions.time.row_count }} 行</a-tag>
            </small>
          </div>
          <div class="prop-row" v-if="(panel.raw.dimensions.time?.common_ranges || []).length > 0">
            <b>常用时间表达 ({{ panel.raw.dimensions.time.common_ranges.length }}):</b>
            <div style="display:flex;flex-wrap:wrap;gap:4px;margin-top:4px">
              <a-tooltip v-for="r in panel.raw.dimensions.time.common_ranges" :key="r.expr"
                :title="`${r.start} → ${r.end}` + (r.aliases?.length ? '（也叫: ' + r.aliases.join('/') + '）' : '')">
                <a-tag color="blue" size="small">{{ r.expr }}</a-tag>
              </a-tooltip>
            </div>
          </div>

          <!-- 班次维度 -->
          <div class="prop-row" v-if="panel.raw.dimensions.shift" style="margin-top:8px">
            <b>班次:</b> <code style="background:#F1F5F9;padding:1px 4px;border-radius:3px">{{ panel.raw.dimensions.shift.field }}</code>
            <span v-if="panel.raw.dimensions.shift.secondary_field">
              / <code style="background:#F1F5F9;padding:1px 4px;border-radius:3px">{{ panel.raw.dimensions.shift.secondary_field }}</code>
            </span>
            <small v-if="(panel.raw.dimensions.shift.values_from_data || []).length > 0" style="color:#64748B;margin-left:6px">
              DB 实际值: [{{ (panel.raw.dimensions.shift.values_from_data || []).join('/') }}]
            </small>
          </div>
          <div v-if="(panel.raw.dimensions.shift?.values || []).length > 0" class="attr-grid">
            <div class="attr-row" v-for="sv in panel.raw.dimensions.shift.values" :key="sv.code">
              <span class="attr-key">{{ sv.code }} {{ sv.numeric !== undefined ? `(${sv.numeric})` : '' }}</span>
              <span class="attr-val">
                <a-tag v-for="alias in (sv.aliases || [])" :key="alias" size="small" color="cyan">{{ alias }}</a-tag>
              </span>
            </div>
          </div>

          <!-- 产线维度 -->
          <div class="prop-row" v-if="panel.raw.dimensions.line" style="margin-top:8px">
            <b>产线:</b> <code style="background:#F1F5F9;padding:1px 4px;border-radius:3px">{{ panel.raw.dimensions.line.field }}</code>
            <small v-if="(panel.raw.dimensions.line.values_from_data || []).length > 0" style="color:#64748B;margin-left:6px">
              DB 实际值: [{{ (panel.raw.dimensions.line.values_from_data || []).join('/') }}]
            </small>
          </div>

          <!-- 规格维度 -->
          <div class="prop-row" v-if="panel.raw.dimensions.product_spec" style="margin-top:8px">
            <b>规格:</b> <code style="background:#F1F5F9;padding:1px 4px;border-radius:3px">{{ panel.raw.dimensions.product_spec.field }}</code>
            <small v-if="(panel.raw.dimensions.product_spec.values_from_data || []).length > 0" style="color:#64748B;margin-left:6px">
              DB 实际值: [{{ (panel.raw.dimensions.product_spec.values_from_data || []).join('/') }}]
            </small>
          </div>

          <!-- 炉号维度 -->
          <div class="prop-row" v-if="panel.raw.dimensions.furnace_no" style="margin-top:8px">
            <b>炉号:</b>
            <code style="background:#F1F5F9;padding:1px 4px;border-radius:3px">{{ panel.raw.dimensions.furnace_no.field }}</code>
            <span v-if="panel.raw.dimensions.furnace_no.raw_field">
              / <code style="background:#F1F5F9;padding:1px 4px;border-radius:3px">{{ panel.raw.dimensions.furnace_no.raw_field }}</code>
            </span>
          </div>
        </template>
      </template>

      <!-- 公式聚合根 -->
      <template v-if="panel.type === 'formulaRoot'">
        <div class="prop-row"><b>节点:</b> 公式聚合根</div>
        <div class="prop-row"><b>说明:</b> {{ panel.raw?.description }}</div>
        <div class="section-title">数据统计</div>
        <div class="attr-grid">
          <div class="attr-row">
            <span class="attr-key">公式总数</span>
            <span class="attr-val"><a-tag color="cyan">{{ panel.raw?.totalCount ?? 0 }}</a-tag></span>
          </div>
          <div class="attr-row">
            <span class="attr-key">计算公式</span>
            <span class="attr-val"><a-tag color="blue">{{ panel.raw?.calcCount ?? 0 }}</a-tag>F_FORMULA_TYPE = CALC</span>
          </div>
          <div class="attr-row">
            <span class="attr-key">判定公式</span>
            <span class="attr-val"><a-tag color="orange">{{ panel.raw?.judgeCount ?? 0 }}</a-tag>F_FORMULA_TYPE = JUDGE</span>
          </div>
          <div class="attr-row">
            <span class="attr-key">只展示</span>
            <span class="attr-val"><a-tag color="default">{{ panel.raw?.noCount ?? 0 }}</a-tag>F_FORMULA_TYPE = NO</span>
          </div>
        </div>
        <div class="section-title">来源表</div>
        <div class="attr-grid">
          <div class="attr-row"><span class="attr-key">公式</span><span class="attr-val"><small class="f-col-hint">lab_intermediate_data_formula</small></span></div>
        </div>
      </template>

      <!-- 公式类型组（CALC / JUDGE / NO） -->
      <template v-if="panel.type === 'formulaTypeGroup'">
        <div class="prop-row"><b>类型:</b>
          <a-tag :color="panel.raw?.formulaType === 'CALC' ? 'blue' : panel.raw?.formulaType === 'JUDGE' ? 'orange' : 'default'">
            {{ panel.raw?.label }}
          </a-tag>
          <small class="f-col-hint">F_FORMULA_TYPE = {{ panel.raw?.formulaType }}</small>
        </div>
        <div class="prop-row"><b>说明:</b> {{ panel.raw?.description }}</div>
        <div class="prop-row"><b>数量:</b> <a-tag color="cyan">{{ (panel.specs || []).length }}</a-tag></div>

        <div class="section-title">公式列表 ({{ (panel.specs || []).length }})</div>
        <div class="attr-grid">
          <div class="attr-row" v-for="s in (panel.specs || [])" :key="s.id">
            <span class="attr-key">{{ s.name }}</span>
            <span class="attr-val">
              <a-tag v-if="s.code" size="small">{{ s.code }}</a-tag>
              <small v-if="s.description" style="color:#94a3b8;margin-left:4px">{{ s.description }}</small>
            </span>
          </div>
        </div>
        <div v-if="!(panel.specs || []).length" class="prop-row" style="color:#94a3b8">该类型暂无公式</div>
      </template>

      <!-- 单条公式：展示完整字段 -->
      <template v-if="panel.type === 'formulaItem'">
        <div class="prop-row">
          <b>公式名称:</b> {{ panel.raw?.formula_name || '-' }}
          <small class="f-col-hint">F_FORMULA_NAME</small>
        </div>
        <div class="prop-row">
          <b>列名:</b> <code style="font-size:11px;background:#F1F5F9;padding:1px 4px;border-radius:3px">{{ panel.raw?.column_name || '-' }}</code>
          <small class="f-col-hint">F_COLUMN_NAME</small>
        </div>
        <div class="prop-row">
          <b>类型:</b>
          <a-tag :color="panel.raw?.formula_type === 'CALC' ? 'blue' : panel.raw?.formula_type === 'JUDGE' ? 'orange' : 'default'">
            {{ panel.raw?.formula_type === 'CALC' ? '计算' : panel.raw?.formula_type === 'JUDGE' ? '判定' : panel.raw?.formula_type === 'NO' ? '只展示' : panel.raw?.formula_type || '-' }}
          </a-tag>
          <small class="f-col-hint">F_FORMULA_TYPE</small>
        </div>
        <div class="prop-row">
          <b>来源:</b>
          <a-tag :color="panel.raw?.source_type === 'CUSTOM' ? 'purple' : 'default'">
            {{ panel.raw?.source_type === 'CUSTOM' ? '自定义' : '系统' }}
          </a-tag>
          <small class="f-col-hint">F_SOURCE_TYPE</small>
        </div>
        <div class="prop-row" v-if="panel.raw?.table_name">
          <b>来源表:</b> {{ panel.raw.table_name }}
          <small class="f-col-hint">F_TABLE_NAME</small>
        </div>
        <div class="prop-row" v-if="panel.raw?.unit_name">
          <b>单位:</b> {{ panel.raw.unit_name }}
          <small class="f-col-hint">F_UNIT_NAME</small>
        </div>
        <div class="prop-row" v-if="panel.raw?.precision_val !== null && panel.raw?.precision_val !== undefined">
          <b>精度:</b> {{ panel.raw.precision_val }} 位小数
          <small class="f-col-hint">F_PRECISION</small>
        </div>
        <div class="prop-row" v-if="panel.raw?.formula_language">
          <b>公式语言:</b> <a-tag>{{ panel.raw.formula_language }}</a-tag>
          <small class="f-col-hint">F_FORMULA_LANGUAGE</small>
        </div>
        <div class="prop-row">
          <b>状态:</b>
          <a-tag :color="panel.raw?.is_enabled ? 'green' : 'red'">
            {{ panel.raw?.is_enabled ? '启用' : '禁用' }}
          </a-tag>
          <small class="f-col-hint">F_IS_ENABLED</small>
        </div>
        <div class="prop-row" v-if="panel.raw?.sort_order !== undefined && panel.raw?.sort_order !== null">
          <b>排序:</b> {{ panel.raw.sort_order }}
          <small class="f-col-hint">F_SORT_ORDER</small>
        </div>
        <div class="prop-row" v-if="panel.raw?.default_value">
          <b>默认值:</b> {{ panel.raw.default_value }}
          <small class="f-col-hint">F_DEFAULT_VALUE</small>
        </div>
        <div class="section-title" v-if="panel.raw?.formula && panel.raw?.formula_type !== 'NO'">
          公式表达式
          <small class="f-col-hint">F_FORMULA</small>
        </div>
        <pre class="formula-code" v-if="panel.raw?.formula && panel.raw?.formula_type !== 'NO'">{{ panel.raw.formula }}</pre>
        <div class="prop-row" v-if="panel.raw?.remark">
          <b>备注:</b> <span style="color:#64748B">{{ panel.raw.remark }}</span>
          <small class="f-col-hint">F_REMARK</small>
        </div>
        <!-- NLQ 同义词 -->
        <div class="section-title" v-if="(panel.raw?.aliases || []).length > 0">
          AI 同义词 ({{ (panel.raw.aliases || []).length }})
          <small style="color:#64748B;font-weight:normal;margin-left:6px">问数召回辅助</small>
        </div>
        <div v-if="(panel.raw?.aliases || []).length > 0" style="display:flex;flex-wrap:wrap;gap:4px">
          <a-tag v-for="alias in panel.raw.aliases" :key="alias" color="cyan">{{ alias }}</a-tag>
        </div>
      </template>

      <!-- 带材根节点 → 产品规格 + 扩展属性 + 叠片数据 + 单片性能（折叠面板） -->
      <template v-if="panel.type === 'ribbonRoot'">
        <div class="prop-row"><b>节点:</b> 业务根节点</div>
        <div class="prop-row"><b>描述:</b> 以炉号为业务入口，连接规格、检测数据、公式和判定规则。</div>

        <a-collapse :bordered="false" style="margin-top:8px">
          <!-- 产品规格 -->
          <a-collapse-panel key="products" header="产品规格">
            <div class="rule-list spec-list">
              <div class="rule-item spec-item" v-for="p in (panel.products || [])" :key="p.id"
                   @click="$emit('action', { type: 'selectSpec', specId: p.id, spec: p })">
                <span class="rule-name">{{ p.name }}</span>
                <span class="rule-priority" v-if="p.code">{{ p.code }}</span>
              </div>
            </div>
            <div v-if="!(panel.products || []).length" class="prop-row" style="color:#94a3b8">暂无产品规格</div>
          </a-collapse-panel>

          <!-- 产品扩展信息 -->
          <a-collapse-panel key="attributes" header="产品扩展信息">
            <div class="attr-grid">
              <div class="attr-row" v-for="s in (panel.specs || [])" :key="s.id">
                <span class="attr-key">{{ s.name }}</span>
              </div>
            </div>
            <div v-if="!(panel.specs || []).length" class="prop-row" style="color:#94a3b8">暂无扩展信息</div>
          </a-collapse-panel>

          <!-- 叠片数据 -->
          <a-collapse-panel key="rawData" header="叠片数据">
            <div class="attr-grid">
              <div class="attr-row" v-for="f in (panel.raw?.rawDataFields || [])" :key="f.id">
                <span class="attr-key">{{ f.name }}</span>
                <span class="attr-val" style="font-size:11px;color:#94a3b8">{{ f.code }}</span>
              </div>
            </div>
            <div v-if="!(panel.raw?.rawDataFields || []).length" class="prop-row" style="color:#94a3b8">暂无叠片数据字段</div>
          </a-collapse-panel>

          <!-- 单片性能 -->
          <a-collapse-panel key="magneticData" header="单片性能">
            <div class="attr-grid">
              <div class="attr-row" v-for="f in (panel.raw?.magneticDataFields || [])" :key="f.id">
                <span class="attr-key">{{ f.name }}</span>
                <span class="attr-val" style="font-size:11px;color:#94a3b8">{{ f.code }}</span>
              </div>
            </div>
            <div v-if="!(panel.raw?.magneticDataFields || []).length" class="prop-row" style="color:#94a3b8">暂无单片性能字段</div>
          </a-collapse-panel>
        </a-collapse>
      </template>

      <!-- 带材详情 -->
      <template v-if="panel.type === 'ribbon'">
        <div class="prop-row"><b>炉号:</b> {{ panel.raw?.furnace_no }}</div>
        <div class="prop-row" v-if="panel.raw?.furnace_no_formatted"><b>格式化:</b> {{ panel.raw.furnace_no_formatted }}</div>
        <div class="prop-row"><b>规格:</b> {{ panel.raw?.spec_name || panel.raw?.spec_code || '-' }}</div>
        <div class="prop-row"><b>检测日期:</b> {{ panel.raw?.detection_date || '-' }}</div>
        <div class="prop-row"><b>等级:</b>
          <a-tag :color="panel.raw?.labeling === 'A' ? 'success' : panel.raw?.labeling === 'C' ? 'error' : 'processing'">
            {{ panel.raw?.labeling || '未判定' }}
          </a-tag>
        </div>
        <div class="section-title">性能数据</div>
        <div class="attr-grid">
          <div class="attr-row"><span class="attr-key">Ps铁损</span><span class="attr-val">{{ panel.raw?.ps_loss ?? '-' }}</span></div>
          <div class="attr-row"><span class="attr-key">Ss激磁功率</span><span class="attr-val">{{ panel.raw?.ss_power ?? '-' }}</span></div>
          <div class="attr-row"><span class="attr-key">Hc</span><span class="attr-val">{{ panel.raw?.hc ?? '-' }}</span></div>
          <div class="attr-row"><span class="attr-key">宽度</span><span class="attr-val">{{ panel.raw?.width ?? '-' }}</span></div>
          <div class="attr-row"><span class="attr-key">平均厚度</span><span class="attr-val">{{ panel.raw?.avg_thickness ?? '-' }}</span></div>
        </div>
        <div class="section-title">判定结果</div>
        <div class="attr-grid">
          <div class="attr-row"><span class="attr-key">磁性能</span><span class="attr-val">{{ panel.raw?.magnetic_res || '-' }}</span></div>
          <div class="attr-row"><span class="attr-key">厚度</span><span class="attr-val">{{ panel.raw?.thick_res || '-' }}</span></div>
          <div class="attr-row"><span class="attr-key">叠片系数</span><span class="attr-val">{{ panel.raw?.lam_factor_res || '-' }}</span></div>
        </div>
      </template>

      <!-- 叠片数据详情 -->
      <template v-if="panel.type === 'lamination'">
        <div class="prop-row"><b>宽度:</b> {{ panel.raw?.width ?? '-' }}</div>
        <div class="prop-row"><b>卷重:</b> {{ panel.raw?.coil_weight ?? '-' }}</div>
        <div class="prop-row"><b>断头数:</b> {{ panel.raw?.break_count ?? '-' }}</div>
        <div class="prop-row"><b>单卷重量:</b> {{ panel.raw?.single_coil_weight ?? '-' }}</div>
      </template>

      <!-- 单片性能详情 -->
      <template v-if="panel.type === 'singleSheet'">
        <div class="prop-row"><b>Ps铁损:</b> {{ panel.raw?.ps_loss ?? '-' }}</div>
        <div class="prop-row"><b>Ss激磁功率:</b> {{ panel.raw?.ss_power ?? '-' }}</div>
        <div class="prop-row"><b>Hc:</b> {{ panel.raw?.hc ?? '-' }}</div>
      </template>

      <!-- 外观特性详情 -->
      <template v-if="panel.type === 'appearance'">
        <div class="prop-row"><b>特性名称:</b> {{ panel.raw?.name }}</div>
        <div class="prop-row"><b>大类:</b> {{ panel.raw?.category || '-' }}</div>
        <div class="prop-row"><b>等级:</b> {{ panel.raw?.level || '-' }}</div>
      </template>
    </div>
  </transition>
</template>

<script lang="ts" setup>
import { computed, ref } from 'vue';
import { CloseOutlined } from '@ant-design/icons-vue';
import type { PanelData, OntologyRule } from '../types/ontology';

const props = defineProps<{
  panel: PanelData | null;
}>();

const emit = defineEmits<{
  (e: 'close'): void;
  (e: 'back'): void;
  (e: 'selectRule', rule: OntologyRule): void;
  (e: 'action', payload: Record<string, unknown>): void;
}>();

const selectedRuleId = ref('');

// ────────────────────────────────────────────────────────────────────────────
// conditionJson → 中文嵌套缩进渲染（与后端 query_agent._render_condition_tree_md 对齐）
// 业务人员视角，不展示 JSON 原文 / 英文列名 / 操作符符号
// ────────────────────────────────────────────────────────────────────────────
const COND_LEFT_CN: Record<string, string> = {
  AppearanceFeatureCategoryIds: '外观大类',
  AppearanceFeatureLevelIds: '外观严重等级',
  Width: '带宽', Thickness: '厚度', ThicknessRange: '厚度极差',
  MaxThickness: '最大厚度', MinThickness: '最小厚度',
  LaminationFactor: '叠片系数', LaminationFactorRes: '叠片判定值',
  PerfHc: '矫顽力 Hc', PerfPsLoss: 'Ps 铁损', PerfSsPower: 'Ss 比功率',
  AfterHc: '退火后矫顽力', AfterPsLoss: '退火后铁损', AfterSsPower: '退火后比功率',
  PerfAfterPsLoss: '刻痕后 Ps 铁损', PerfAfterHc: '刻痕后矫顽力',
  PerfAfterSsPower: '刻痕后 Ss 比功率',
  NewBandLeft: '新带型左侧', NewBandRight: '新带型右侧',
  BreakCount: '断头数', BreakHead: '断头',
  AvgThickness: '平均厚度', Density: '密度', DensityRes: '密度判定值',
  AppearanceAll: '所有外观特性', AppearanceFeatures: '外观特性',
  FeatureSuffix: '外观特性',
  FirstInspection: '一次交检', Labeling: '等级标签',
  MagneticResult: '磁性能判定', ThickRes: '厚度判定',
  Shift: '班次', LineNo: '产线', FurnaceNo: '炉号',
};
const COND_OP_CN: Record<string, string> = {
  EQ: '等于', EQUALS: '等于', NEQ: '不等于', NE: '不等于',
  GT: '大于', GTE: '大于等于', GE: '大于等于',
  LT: '小于', LTE: '小于等于', LE: '小于等于',
  IN: '属于', NOT_IN: '不属于',
  CONTAINS_ANY: '包含任一', CONTAINS_ALL: '全部包含', NOT_CONTAINS: '不包含',
  BETWEEN: '在区间', NOT_BETWEEN: '不在区间',
  IS_NULL: '为空', IS_NOT_NULL: '非空', NOT_NULL: '非空', NULL: '为空',
  LIKE: '匹配', NOT_LIKE: '不匹配', EMPTY: '为空', NOT_EMPTY: '非空',
  '>': '大于', '>=': '大于等于', '<': '小于', '<=': '小于等于',
  '=': '等于', '==': '等于', '!=': '不等于', '<>': '不等于',
};
function humanizeLeftExpr(le: any, groupName = ''): string {
  if (typeof le !== 'string' || !le) return '?';
  // $VAR_<id> 没有前端 var_map（接口里 conditionJson 是原始数据）→ 兜底用组名
  if (le.startsWith('$VAR_')) return groupName || '扩展属性';
  return COND_LEFT_CN[le] || le;
}
function humanizeOp(op: any): string {
  if (typeof op !== 'string' || !op) return '?';
  return COND_OP_CN[op.toUpperCase()] || COND_OP_CN[op] || op;
}
function humanizeRight(v: any): string {
  if (v === null || v === undefined || v === '') return '（空）';
  if (typeof v === 'string') {
    try {
      const arr = JSON.parse(v);
      if (Array.isArray(arr)) return arr.join('、') || '（空集）';
    } catch { /* not JSON, fall through */ }
  }
  return String(v);
}
function formatCondition(c: any, groupName = ''): string {
  const left = humanizeLeftExpr(c?.leftExpr, groupName);
  const opStr = (c?.operator || '').toString().toUpperCase();
  const op = humanizeOp(c?.operator);
  if (['IS_NULL', 'IS_NOT_NULL', 'NOT_NULL', 'NULL', 'EMPTY', 'NOT_EMPTY'].includes(opStr)) {
    return `${left}${op}`;
  }
  const right = humanizeRight(c?.rightValue);
  return `${left} ${op}【${right}】`;
}
interface CondLine { kind: 'group' | 'subgroup' | 'cond'; depth: number; text: string; }
function renderConditionTree(rawJson: any): CondLine[] {
  if (!rawJson || typeof rawJson !== 'string') return [];
  let data: any;
  try {
    data = JSON.parse(rawJson);
  } catch {
    return [{ kind: 'cond', depth: 0, text: '（条件 JSON 解析失败）' }];
  }
  const groups: any[] = Array.isArray(data?.groups) ? data.groups : [];
  if (!groups.length) return [{ kind: 'cond', depth: 0, text: '（未配置任何条件）' }];

  const lines: CondLine[] = [];
  groups.forEach((g, gi) => {
    const label = String.fromCharCode(65 + gi);
    const gname = (g?.name || '').toString().trim();
    const prefix = gi > 0 ? '且 ' : '';
    lines.push({ kind: 'group', depth: 0, text: `${prefix}条件组 ${label}${gname ? ' · ' + gname : ''}` });

    const conds: any[] = Array.isArray(g?.conditions) ? g.conditions : [];
    const subs: any[] = Array.isArray(g?.subGroups) ? g.subGroups : [];
    const gLogic = (g?.logic || 'AND').toString().toUpperCase();

    if (subs.length) {
      const subWord = gLogic === 'AND' ? '且' : '或';
      subs.forEach((sub, si) => {
        const sLabel = String.fromCharCode(65 + si);
        const head = si === 0 ? '' : `${subWord} `;
        lines.push({ kind: 'subgroup', depth: 1, text: `${head}子组 ${sLabel}：` });
        const subConds: any[] = Array.isArray(sub?.conditions) ? sub.conditions : [];
        const sLogic = (sub?.logic || 'AND').toString().toUpperCase();
        const inner = sLogic === 'AND' ? '且' : '或';
        subConds.forEach((c, ci) => {
          const t = formatCondition(c, gname);
          lines.push({ kind: 'cond', depth: 2, text: ci === 0 ? t : `${inner} ${t}` });
        });
      });
    }
    if (conds.length) {
      const inner = gLogic === 'AND' ? '且' : '或';
      conds.forEach((c, ci) => {
        const t = formatCondition(c, gname);
        lines.push({ kind: 'cond', depth: 1, text: ci === 0 ? t : `${inner} ${t}` });
      });
    }
  });
  return lines;
}

const conditionTree = computed<CondLine[]>(() => {
  const raw = (props.panel?.raw as any)?.conditionJson;
  return renderConditionTree(raw);
});

const formulaTypeLabel = computed(() => {
  const raw = props.panel?.raw as any;
  if (!raw) return '-';
  return raw.formula_type === 'CALC'
    ? '计算公式'
    : raw.formula_type === 'JUDGE' || raw.formula_type === '2'
      ? '判定公式'
      : raw.formula_type || '-';
});

interface ParsedCondition {
  field: string;
  label?: string;
  expected: string;
  actual?: string | number;
  satisfied?: boolean | null;
}

const conditions = computed<ParsedCondition[]>(() => {
  const raw = props.panel?.raw as any;
  if (!raw?.conditionJson) return [];
  try {
    const parsed = JSON.parse(raw.conditionJson);
    if (!Array.isArray(parsed)) return [];
    return parsed
      .filter((c: any) => c && typeof c === 'object')
      .map((c: any) => ({
        field: c.field || c.column || c.column_name || c.metric || '',
        label: c.label || c.field || '',
        expected: c.operator && c.value !== undefined
          ? `${c.operator} ${c.value}`
          : c.min !== undefined && c.max !== undefined
            ? `${c.min} ~ ${c.max}`
            : '-',
        actual: undefined,
        satisfied: undefined,
      }));
  } catch {
    return [];
  }
});

function selectRule(r: OntologyRule) {
  selectedRuleId.value = r.id;
  emit('selectRule', r);
}

// 炉号字段 → 正则分组号
function furnaceFieldGroup(csharpField?: string): string {
  const map: Record<string, string> = {
    'LineNo': 'Group 1',
    'Shift': 'Group 2',
    'ProdDate': 'Group 3',
    'FurnaceBatchNo': 'Group 4',
    'CoilNo': 'Group 5',
    'SubcoilNo': 'Group 6',
    'SpecialMarker': 'Group 7',
    'FeatureSuffix': 'Group 8',
  };
  return map[csharpField || ''] || '-';
}

// 炉号字段 → 数据库字段映射
function furnaceDbFieldMap(csharpField?: string): Array<{ column: string; comment: string }> {
  const map: Record<string, Array<{ column: string; comment: string }>> = {
    'LineNo': [{ column: 'F_LINE_NO', comment: '产线' }],
    'Shift': [
      { column: 'F_SHIFT', comment: '班次（汉字）' },
      { column: 'F_SHIFT_NUMERIC', comment: '班次数字' },
    ],
    'ProdDate': [{ column: 'F_PROD_DATE', comment: '生产日期' }],
    'FurnaceBatchNo': [{ column: 'F_FURNACE_BATCH_NO', comment: '炉次号' }],
    'CoilNo': [{ column: 'F_COIL_NO', comment: '卷号' }],
    'SubcoilNo': [{ column: 'F_SUBCOIL_NO', comment: '分卷号' }],
    'SpecialMarker': [{ column: 'F_SPECIAL_MARKER', comment: '特殊标记' }],
    'FeatureSuffix': [{ column: 'F_FEATURE_SUFFIX', comment: '特性描述' }],
  };
  return map[csharpField || ''] || [];
}
</script>

<style lang="less" scoped>
.kg-panel {
  width: 340px;
  background: #fff;
  border-left: 1px solid #F1F5F9;
  padding: 16px;
  overflow-y: auto;
  flex-shrink: 0;
}

.panel-head {
  display: flex;
  align-items: center;
  gap: 8px;
}

.panel-type-badge {
  display: inline-block;
  padding: 2px 8px;
  border-radius: 4px;
  color: #fff;
  font-size: 11px;
  font-weight: 600;
  flex-shrink: 0;
}

.panel-label {
  font-size: 15px;
  font-weight: 600;
  color: #1E293B;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
  flex: 1;
}

.panel-close { flex-shrink: 0; }

.prop-row {
  font-size: 13px;
  color: #475569;
  margin-bottom: 5px;
  line-height: 1.6;
  b { color: #334155; }
}

// 数据库字段名提示（F_CODE 等）
.f-col-hint {
  color: #94A3B8;
  font-family: 'SFMono-Regular', Consolas, monospace;
  font-size: 11px;
  margin-left: 6px;
  background: #F1F5F9;
  padding: 1px 4px;
  border-radius: 3px;
}

.section-title {
  font-size: 12px;
  font-weight: 600;
  color: #64748B;
  text-transform: uppercase;
  letter-spacing: 0.5px;
  margin: 14px 0 8px;
  padding-bottom: 4px;
  border-bottom: 1px solid #F1F5F9;
}

.attr-grid {
  display: flex;
  flex-direction: column;
  gap: 3px;
}

.attr-row {
  display: flex;
  justify-content: space-between;
  align-items: center;
  background: #F8FAFC;
  border-radius: 4px;
  padding: 4px 8px;
  font-size: 12px;
}

.attr-key { color: #64748B; }
.attr-val { color: #1E293B; font-weight: 500; }

.rule-list {
  display: flex;
  flex-direction: column;
  gap: 2px;
  max-height: 260px;
  overflow-y: auto;
}

.rule-item {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 5px 8px;
  border-radius: 4px;
  cursor: pointer;
  font-size: 12px;
  transition: background 0.15s;

  &:hover { background: #F1F5F9; }
  &-active { background: #EFF6FF; }
}

.rule-name { color: #334155; }
.rule-priority { color: #94A3B8; font-size: 11px; }

.formula-code {
  background: #F8FAFC;
  border: 1px solid #E2E8F0;
  border-radius: 6px;
  padding: 8px;
  font-size: 12px;
  font-family: 'SFMono-Regular', Consolas, monospace;
  color: #334155;
  white-space: pre-wrap;
  word-break: break-all;
  max-height: 160px;
  overflow-y: auto;
  margin: 4px 0 0;
}

.condition-table {
  font-size: 12px;
  border: 1px solid #F1F5F9;
  border-radius: 6px;
  overflow: hidden;

  .cond-header, .cond-row {
    display: grid;
    grid-template-columns: 1.4fr 1fr 1fr 60px;
    gap: 4px;
    padding: 5px 8px;
    align-items: center;
  }

  .cond-header {
    background: #F8FAFC;
    font-weight: 600;
    color: #64748B;
  }

  .cond-row {
    border-top: 1px solid #F1F5F9;
    color: #475569;
  }
}

.condition-tree {
  background: #F8FAFC;
  border: 1px solid #E2E8F0;
  border-radius: 6px;
  padding: 8px 12px;
  margin: 4px 0 0;
  font-size: 12.5px;
  line-height: 1.7;
  color: #334155;
  max-height: 280px;
  overflow-y: auto;

  .cond-line {
    word-break: break-word;
  }
  .cond-line--group {
    font-weight: 600;
    color: #6366f1;
    margin-top: 6px;
    &:first-child { margin-top: 0; }
  }
  .cond-line--subgroup {
    color: #0891b2;
    margin-top: 2px;
  }
  .cond-line--cond {
    color: #475569;
  }
  .cond-depth-1 { padding-left: 14px; }
  .cond-depth-2 { padding-left: 28px; }
}

.slide-enter-active, .slide-leave-active {
  transition: transform 0.2s ease, opacity 0.2s ease;
}
.slide-enter-from { transform: translateX(100%); opacity: 0; }
.slide-leave-to { transform: translateX(100%); opacity: 0; }
</style>
