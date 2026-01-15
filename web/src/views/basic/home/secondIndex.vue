<template>
  <Card :tab-list="tabListTitle" v-bind="$attrs" :active-tab-key="activeKey" @tab-change="onTabChange">
  <!-- 时间筛选框   -->
    <div class="searchBox">
      <BasicForm @register="registerForm" @submit="handleSubmit" @reset="handleReset" />
        <div class="blink" @click="openMessageDrawer(true, {})">
         <WarningOutlined style="color: red;font-size: 25px"/>
    </div>
    </div>
    <p v-if="activeKey === 'tab1'">
      <div class="md:flex enter-y middleBox"  @click="jumpSecond">
      <VisitdoubleCategory class="md:w-1/3 w-full" :loading="loading" />
      <VisitCategory class="md:w-1/3 w-full" :loading="loading" />
      </div>
    </p>
    <p v-if="activeKey === 'tab2'">
      <VisitAnalysisBar />
    </p>
   
  </Card>
  <MessageDrawer @register="registerMessageDrawer" @readMsg="readMsg" />
  
</template>
<script lang="ts" setup>
  import { reactive, ref,toRefs, watch, computed, nextTick } from 'vue';
  import { Card,Button } from 'ant-design-vue';
  import { useRouter } from 'vue-router';

  import VisitdoubleCategory from './components/VisitdoubleCategory.vue';
  import VisitCategory from './components/VisitCategory.vue';


  import {useDrawer} from '/@/components/Drawer';
  import MessageDrawer from './components/MessageDrawer.vue'


  import { WarningOutlined } from '@ant-design/icons-vue';
  import { BasicForm, useForm } from '/@/components/Form';
  import { useI18n } from '/@/hooks/web/useI18n';

  const { t } = useI18n();
  const activeKey = ref('tab1');

  const state = reactive<State>({
    activeKey: '0',
    keyword: '',
  });
  
  const router = useRouter();

  const [registerMessageDrawer, { openDrawer: openMessageDrawer }] = useDrawer();
  const Dates = []
  const onChange = (time: '', timeString: string) => {
  console.log(time, timeString);
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
      tab: '**区域',
    },
    {
      key: 'tab4',
      tab: '炼**区域',
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
