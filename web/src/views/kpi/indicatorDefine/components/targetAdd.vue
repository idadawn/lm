<template>
  <div>
    <a-row
      name="basic"
      :label-col="{ span: 4 }"
      :wrapper-col="{ span: 16 }"
      autocomplete="off"
      style="padding: 25px 35px 12px 35px; background: #eceef5">
      <a-row>
        <a-col :span="24" style="height: 70px">
          <a-row>
            <a-col>
              <span class="icon-ym icon-ym-dataApplication" style="color: #317bb2; font-size: 40px"></span>
            </a-col>
            <a-col style="margin-left: 10px">
              <h3 style="font-size: 20px; margin: 0">新增指标</h3>
              <p>请选择需要新增的指标类型</p>
            </a-col>
          </a-row>
        </a-col>
      </a-row>
      <a-row class="targetRow">
        <template v-for="item in targetClassArr">
          <a-col
            :span="12"
            :style="{ background: item.background }"
            class="targetClass"
            @click="targetIndexFun(item.id)">
            <a-row>
              <a-col :span="7">
                <span class="icon-ym" :class="item.icon" :style="{ color: item.color }" style="font-size: 60px"></span>
              </a-col>
              <a-col :span="17">
                <h3 style="margin-bottom: 10px; font-size: 18px">{{ item.title }}</h3>
                <p>{{ item.content }}</p>
              </a-col>
            </a-row>
          </a-col>
        </template>
      </a-row>
    </a-row>
  </div>
</template>
<script lang="ts" setup>
// import { reactive, ref, onMounted, watch } from 'vue';
import { useRouter } from 'vue-router';
const router = useRouter();
const targetClassArr: any[] = [
  {
    id: 'atomic',
    background: '#d2e1fc',
    color: '#367bf6',
    title: '原子指标',
    icon: 'icon-ym-echartsScatter',
    content: '定量衡量产品或业务的表现，例如销售额。',
  },
  {
    id: 'derive',
    background: '#fcf1f6',
    color: '#ef89ad',
    title: '派生指标',
    icon: 'icon-ym-echartsLineArea',
    content: '基于单个指标扩展而来，常用于指标在时间维度上的扩展，如销售额的月环比。',
  },
  {
    id: 'recombination',
    background: '#f9f5eb',
    color: '#f1b442',
    title: '复合指标',
    icon: 'icon-ym-wf-postBatchTab',
    content: '基于多个指标，通过输入表达式定义，例如利润=销售额-成本。',
  },
];

// 新建指标
function targetIndexFun(id) {
  if (id === 'atomic') {
    // 原子指标
    router.push({ path: '/kpi/indicatorDefine/formAtomic' });
  } else if (id === 'derive') {
    // 派生指标
    router.push('/kpi/indicatorDefine/formDerive');
  } else if (id === 'recombination') {
    // 复合指标
    router.push('/kpi/indicatorDefine/formRecombination');
  }
}
</script>
<style lang="less" scoped>
.targetRow {
  display: flex;
  justify-content: space-between;
}

.targetClass {
  flex: none;
  width: 48% !important;
  margin-bottom: 4%;
  padding: 20px;
  cursor: pointer;
  border: 1px solid #e0e7ef;
}
</style>
