<template>
  <div class="page-content-wrapper">
    <div class="page-content-wrapper-center">
      <div style="height: 50px">
        <a-space wrap style="float: left; margin: 10px 0 10px 10px">
          <a-button type="primary" @click="gongchengFun" :disabled="displayGoBack"> 工程 </a-button>
          <a-button type="primary" @click="xigtongFun" :disabled="displayGoBack"> 对接MRP系统 </a-button>
          <a-button type="primary" @click="dingdanFun" :disabled="displayGoBack"> 订单 </a-button>
          <!-- <a-button type="primary" @click="hetongFun" :disabled="displayGoBack"> 合同 </a-button> -->
        </a-space>
        <!-- <a-button style="float: right; margin: 10px 10px 10px 0" @click="goBack" v-if="displayGoBack"
          >返回上一级</a-button
        > -->
      </div>
      <a-card>
        <div>
          <div v-for="(item, index) in state.currentItem.arr" class="wrapper-div">
            <span>{{ item.name }}</span>
            <a-space wrap>
              <a-button v-for="(itm, index) in item.items" :style="{ background: itm.color }" @click="itemClick">{{
                itm.name
              }}</a-button>
            </a-space>
          </div>
        </div>
        <div class="stepsClass">
          <a-steps
            size="small"
            progress-dot
            :percent="state.currentItem.percent"
            :current="state.currentIndex"
            @change="changeTab">
            <a-step v-for="(item, index) in data" :title="item.date" :key="index" />
          </a-steps>
        </div>
      </a-card>
    </div>
  </div>
</template>
<script setup lang="ts">
  import { ref, reactive, onMounted } from 'vue';
  import { dingdan_data } from './data/dingdan_data.js';
  import { hetong_data } from './data/hetong_data.js';
  import { xitong_data } from './data/xitong_data.js';
  import { gongcheng_data } from './data/gongcheng_data.js';

  import { PlusOutlined, MinusOutlined } from '@ant-design/icons-vue';
  defineOptions({ name: 'matchingRate' });

  const state = reactive<any>({
    currentIndex: 0,
    currentItem: {
      percent: 0,
    },
  });
  const data: any = ref([]);
  const displayGoBack = ref(false);
  // sleep函数
  const sleep = (ms: number) => {
    return new Promise(resolve => setTimeout(resolve, ms));
  };
  onMounted(() => {
    data.value = xitong_data;
    // data.value = dingdan_data;
    startProcess();
  });
  const startProcess = async () => {
    displayGoBack.value = true;
    state.currentIndex = 0;

    for (let i = 0; i < data.value.length; i++) {
      const item = data.value[i];
      item.percent = 0;
      await new Promise(resolve => {
        // 频率
        const fre = item.pending / 100;
        const timer = setInterval(async () => {
          if (item.percent < 100) {
            item.percent += 1;
            state.currentItem = item;
          } else {
            clearInterval(timer);
            state.currentItem = item;
            await sleep(1000);
            resolve(1);
            state.currentIndex = i + 1;
          }
        }, fre);
      });
    }
    displayGoBack.value = false;
  };
  const changeTab = current => {
    state.currentItem = data.value[current];
    state.currentIndex = current;
  };
  // 点击合同
  const itemClick = () => {
    // data.value = hetong_data;
    // data.value = dingdan_data;
    // startProcess();
    // displayGoBack.value = true;
  };
  // 返回上一级
  const goBack = () => {
    // data.value = dingdan_data;
    data.value = xitong_data;
    startProcess();
    displayGoBack.value = false;
  };
  const gongchengFun = () => {
    data.value = gongcheng_data;
    startProcess();
  };
  const xigtongFun = () => {
    data.value = xitong_data;
    startProcess();
  };
  const hetongFun = () => {
    data.value = hetong_data;
    startProcess();
  };
  const dingdanFun = () => {
    data.value = dingdan_data;
    startProcess();
  };
</script>
<style scoped lang="less">
  .page-content-wrapper-center {
    background: #fff;
  }
  .wrapper-div {
    margin-bottom: 15px;

    :deep(.ant-space) {
      gap: 2px !important;
    }
    span {
      margin-right: 5px;
      font-size: 12px;
      color: #555;
      display: inline-block;
      width: auto;
    }
    .ant-btn {
      width: auto;
      height: 30px;
      color: #fff;
      font-size: 10px;
    }
  }
  :deep(.ant-progress-inner) {
    width: 10px !important;
    height: 10px !important;
    top: -5px !important;
  }
  :deep(.ant-steps-icon-dot) {
    top: -2px;
  }

  .stepsClass {
    width: 100%;
    overflow-x: auto;
  }
</style>
