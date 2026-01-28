<template>
  <Card :tab-list="tabListTitle" v-bind="$attrs" :active-tab-key="activeKey" @tab-change="onTabChange">
   
    <p v-if="activeKey === 'tab1'">
       <!-- 时间筛选框   -->
    <div class="searchBox">
      <BasicForm @register="registerForm" @submit="handleSubmit" @reset="handleReset" />
    </div>
      <div class="md:flex enter-y middleBox">
        <chartsModel  :chartsFlag="state.chartsFlag" :chartsData="state.ChartsObj"/>
      </div>
    </p>
    <p v-if="activeKey === 'tab2'">
      <!-- 归因分析模块 -->
      <AttributionAnalysis />
    </p>
    <p v-if="activeKey === 'tab3'">
      <VisitAnalysisBar />
    </p>
  </Card>
  <MessageDrawer @register="registerMessageDrawer" @readMsg="readMsg" />
</template>
<script lang="ts" setup>
  import { reactive, ref, toRefs, watch, computed, nextTick ,onMounted} from 'vue';
  import { Card, Button } from 'ant-design-vue';
  import { useRouter } from 'vue-router';


  import VisitAnalysisBar from '/@/views/basic/home/components/VisitAnalysisBar.vue';
  import  chartsModel  from './components/chartsModel.vue';
  import  AttributionAnalysis  from './components/AttributionAnalysis.vue';
  

  import { BasicForm, useForm } from '/@/components/Form';
  import { useI18n } from '/@/hooks/web/useI18n';
  import { getChartsFormatData } from '/@/api/basic/charts';
import { isArray } from '@vue/shared';

  const { t } = useI18n();
  const activeKey = ref('tab1');

  const state = reactive<State>({
    activeKey: '0',
    keyword: '',
    chartsFlag:'axis',
    rowLimit:'',
    ChartsObj:{
      // legendData: ['直接', '最优值'],
      xAxisData:[],
      seriesData: [
        {
          name: '',
          type: 'pie',
          stack: 'Total',
          data: [],
        },
      ],
      valueData:[],
    },
    

  });

  const router = useRouter();
  const Dates = [];
  const onChange = (time: '', timeString: string) => {
    // 
  };

  const tabListTitle = [
    {
      key: 'tab1',
      tab: '图表',
    },
    {
      key: 'tab2',
      tab: '归因分析',
    },
    {
      key: 'tab3',
      tab: '定义',
    },
  ];
  const [registerForm, { resetFields }] = useForm({
    baseColProps: { span: 4 },
    // actionColOptions: { span: 24 },
    showActionButtonGroup: true,
    showAdvancedButton: true,
    compact: true,
    labelAlign: 'left',
    labelWidth: 60,
    schemas: [
      {
        field: 'timeRange',
        label: t('common.DateRange'),
        component: 'DateRange',
        componentProps: {
          placeholder: t('common.enterKeyword'),
          submitOnPressEnter: true,
        },
      },
      {
          field: 'chartsFlag',
          label: '图表类型',
          component: 'Select',
          componentProps: {
            placeholder: '请选择类型',
            options: [
              { fullName: '折线图', id: 'axis' },
              { fullName: '饼状图', id: 'pie' },
              { fullName: '柱状图', id: 'histogram' },
              { fullName: '图表', id: 'gudge' },
            ],
          },
        },
      {
          field: 'rowLimit',
          label: '行数限制',
          component: 'Select',
          componentProps: {
            placeholder: '请选择行数限制',
            options: [
              { fullName: '1000', id: 1 },
              { fullName: '2000', id: 2 },
            ],
          },
        },
       
    ],

  });
  onMounted(() => {
    
    getChartsFormatDataList();
  });
  function onTabChange(key) {
    if(key=='tab3'){
      alert('功能暂未开放')
    }else {
      activeKey.value = key;
      getChartsFormatDataList();
      
    }
  }
  function handleSubmit(values) {
    state.timeRange = values?.timeRange || '';
    state.rowLimit = values?.rowLimit || '';
    state.chartsFlag = values?.chartsFlag || '';
   
    handleSearch();
  }
  function handleReset() {
    state.keyword = '';
    handleSearch();
  }
  function handleSearch() {
    
    nextTick(() => {
      getChartsFormatDataList({ page: 1 });
    });
  }
  async function getChartsFormatDataList() {
    try {
      const res = await getChartsFormatData({ nodeId: '1', userId: '1' });
      
      state.elements=[];//置空

      state.ChartsObj.seriesData[0].name=res.data.metric_names[0];

      state.elements = res.data?.data;
      if(state.chartsFlag=='pie'||state.chartsFlag=='gudge'){
        let valueData=[];
        state.elements.map(item=>{
          let valueDataObj={}
          valueDataObj['value']=item[0];
          valueDataObj['name']=item[1];
          valueData.push(valueDataObj)
      })
      state.ChartsObj.valueData=valueData;
      }else{
        //初始化XY轴数据
        state.ChartsObj.xAxisData=[];
        state.ChartsObj.seriesData[0].data=[];

        state.elements.map(item=>{
        state.ChartsObj.xAxisData.push(item[0]) 
        state.ChartsObj.seriesData[0].data.push(item[1])
      })
      if(state.chartsFlag=='axis'){
        state.ChartsObj.seriesData[0].type='line'
        }else if(state.chartsFlag=='histogram'){
          state.ChartsObj.seriesData[0].type='bar'
        }else if(state.chartsFlag=='pie'){
          state.ChartsObj.seriesData[0].type='pie'
        }else{
          state.ChartsObj.seriesData[0].type='line'
        }
      }
      
      } catch (_) {
        
    }
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
  .middleBox {
    margin-top: 40px;
    /* border: 1px solid red; */
  }
  /* .searchBox {
    display: flex;
    align-items: center;
  } */
</style>
