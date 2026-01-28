<template>
  <Card :tab-list="tabListTitle" v-bind="$attrs" :active-tab-key="activeKey" @tab-change="onTabChange">
  <!-- 时间筛选框 -->
    <div class="searchBox">
      <BasicForm @register="registerForm" @submit="handleSubmit" @reset="handleReset" />
        <div class="blink" @click="openMessageDrawer(true, {})">
         <WarningOutlined style="color: red;font-size: 25px"/>
    </div>
    </div>
    <p v-if="activeKey === 'tab1'">
      <!-- =====组件传参======= -->
      <div @click="jumpSecond" v-if="disableShow">
        <homeChartsManageMent :loading="loading"  />
      </div>
      
      
      <!-- ==================== -->

      <div class="md:flex enter-y middleBox" @click="jumpSecond">
        <VisitAnalysis />
      </div>
      <div class="md:flex enter-y middleBox">
        <VisitAxis class="w-full" :loading="loading"/>
      </div>
      <div class="md:flex enter-y middleBox">
      <VisitGauge class="md:w-1/3 w-full" :loading="loading" />
      <VisitSource class="md:w-1/3 !md:mx-10px !md:my-0 !my-10px w-full" :loading="loading" />
      <visitPie class="md:w-1/3 w-full" :loading="loading" />
      </div>
      <div class="md:flex enter-y middleBox">
        <visitThreePie class="md:w-1/2 w-full" :loading="loading" />
        <visitThreePie class="md:w-1/2 w-full" :loading="loading" />
      </div>
    </p>
    <p v-if="activeKey === 'tab2'">
      <VisitAnalysisBar />
    </p>
    <p v-if="activeKey === 'tab3'">
      <optimalManagement />
    </p>
    <p v-if="activeKey === 'tab4'">
      <MonitorManagement />
    </p>
   
  </Card>
  <MessageDrawer @register="registerMessageDrawer" @readMsg="readMsg" />
  
</template>
<script lang="ts" setup>
  import { reactive, ref,toRefs, watch, computed, nextTick } from 'vue';
  import { Card,Button } from 'ant-design-vue';
  import { useRouter } from 'vue-router';
  import VisitAnalysis from './VisitAnalysis.vue';
  import VisitAxis from './VisitAxis.vue';
  import VisitAnalysisBar from './VisitAnalysisBar.vue';
  import VisitSource from './VisitSource.vue';
  import VisitGauge from './VisitGauge.vue';
  import visitThreePie from './visitThreePie.vue';
  import visitPie from './VisitPie.vue';
  import optimalManagement from './optimalManagement/index.vue'
  import MonitorManagement from './monitorManagement.vue'
  import homeChartsManageMent from './homeComponents/index.vue'


  import {useDrawer} from '/@/components/Drawer';
  import MessageDrawer from './MessageDrawer.vue'


  import { WarningOutlined } from '@ant-design/icons-vue';
  import { BasicForm, useForm } from '/@/components/Form';
  import { useI18n } from '/@/hooks/web/useI18n';
  const { t } = useI18n();
  const activeKey = ref('tab1');

  const state = reactive<State>({
    activeKey: '0',
    keyword: '',
    disableShow:false,
  });
  
  const router = useRouter();

  const [registerMessageDrawer, { openDrawer: openMessageDrawer }] = useDrawer();
  const Dates = []
  const onChange = (time: '', timeString: string) => {
};

  const tabListTitle = [
    {
      key: 'tab1',
      tab: '所有',
    },
    {
      key: 'tab2',
      tab: '炼钢区域',
    },
    {
      key: 'tab3',
      tab: '基线区域示例',
    },
    {
      key: 'tab4',
      tab: '监测区域示例',
    },
  ];
  const [registerForm, { resetFields }] = useForm({
    baseColProps: { span: 16 },
    // actionColOptions: { span: 24 },
    showActionButtonGroup: true,
    showAdvancedButton: true,
    compact: true,
    labelAlign: 'left',
    labelWidth: 60,
    schemas: [
      {
        field: 'keyword',
        label: t('common.DateRange'),
        component: 'DateRange',
        componentProps: {
          placeholder: t('common.enterKeyword'),
          submitOnPressEnter: true,
        },
      },
    ],
  });
 



    function onTabChange(key) {
    activeKey.value = key;
    // router.push('/warning/maintenance?config=')
  }
  function handleSubmit(values) {
    state.keyword = values?.keyword || '';
    handleSearch();
  }
  function handleReset() {
    state.keyword = '';
    handleSearch();
  }
  function handleSearch() {
    nextTick(() => {
      // reload({ page: 1 });
    });
  }
  //跳到二级分析页面
  function jumpSecond(){
    // router.push('/predictionManagement/prediction')
  }
  
</script>
<style>
  .blink {
    animation: blink 0.3s linear infinite alternate;
  }

  @keyframes blink {
    0% {
      opacity: 1;
    }

    100% {
      opacity: 0;
    }
  }

  .middleBox{
    margin-top: 40px;

    /* border: 1px solid red; */
  }

  .searchBox{
    display: flex;
    align-items: center;
  }
</style>
./optimalManagement/optimalData/props