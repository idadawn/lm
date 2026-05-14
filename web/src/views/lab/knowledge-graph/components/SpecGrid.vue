<template>
  <div class="spec-grid">
    <a-empty v-if="!specs.length" description="暂无产品规格数据" />
    <div class="grid" v-else>
      <div
        class="spec-card"
        v-for="spec in specs"
        :key="spec.id"
        @click="$emit('select', spec.id)"
      >
        <div class="card-header">
          <span class="spec-code">{{ spec.code }}</span>
          <a-tag color="blue" size="small">{{ spec.ruleCount }} 规则</a-tag>
        </div>
        <div class="spec-name" :title="spec.name">{{ spec.name }}</div>
        <div class="spec-meta">
          <span class="meta-item">
            <i class="dot" style="background:#7C3AED"></i>
            {{ spec.formulaCount }} 公式
          </span>
          <span class="meta-item">
            <i class="dot" style="background:#16A34A"></i>
            {{ spec.qualifiedCount }} 合格
          </span>
          <span class="meta-item">
            <i class="dot" style="background:#DC2626"></i>
            {{ spec.unqualifiedCount }} 不合格
          </span>
        </div>
      </div>
    </div>
    <div class="view-all" v-if="specs.length">
      <a-button type="link" @click="$emit('viewAll')">查看完整知识图谱 →</a-button>
    </div>
  </div>
</template>

<script lang="ts" setup>
import type { OntologySpec, OntologyRule } from '../types/ontology';

interface SpecCardItem extends OntologySpec {
  ruleCount: number;
  formulaCount: number;
  qualifiedCount: number;
  unqualifiedCount: number;
}

const props = defineProps<{
  specs: SpecCardItem[];
}>();

const emit = defineEmits<{
  (e: 'select', specId: string): void;
  (e: 'viewAll'): void;
}>();
</script>

<style lang="less" scoped>
.spec-grid {
  padding: 20px;
  overflow-y: auto;
  flex: 1;
  width: 100%;
}

.grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(240px, 1fr));
  gap: 16px;
}

.spec-card {
  background: #fff;
  border: 1px solid #E2E8F0;
  border-radius: 10px;
  padding: 16px;
  cursor: pointer;
  transition: all 0.2s ease;
  box-shadow: 0 1px 2px rgba(0,0,0,0.04);

  &:hover {
    border-color: #93C5FD;
    box-shadow: 0 4px 12px rgba(37, 99, 235, 0.08);
    transform: translateY(-1px);
  }
}

.card-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 8px;
}

.spec-code {
  font-size: 13px;
  font-weight: 700;
  color: #2563EB;
  background: #EFF6FF;
  padding: 2px 8px;
  border-radius: 4px;
}

.spec-name {
  font-size: 15px;
  font-weight: 600;
  color: #1E293B;
  margin-bottom: 12px;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.spec-meta {
  display: flex;
  gap: 12px;
  font-size: 12px;
  color: #64748B;
}

.meta-item {
  display: flex;
  align-items: center;
  gap: 4px;
}

.dot {
  width: 6px;
  height: 6px;
  border-radius: 50%;
  display: inline-block;
}

.view-all {
  margin-top: 20px;
  text-align: center;
}
</style>
