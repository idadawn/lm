<template>
  <div class="page-content-wrapper">
    <div class="bg-white page-content-wrapper-center">
      <div class="page-content-wrapper-content">
        <a-tabs v-model:activeKey="state.activeKey" type="card">
          <a-tab-pane key="1" tab="曲线分析">
            <div class="pl-2">
              <a-select
                v-model:value="state.fileType"
                placeholder="文件类型"
                style="width: 200px"
                :options="state.options"
                :filter-option="filterOption"
                @focus="handleFocus"
                @blur="handleBlur"></a-select>

              <a-upload
                v-model:file-list="state.fileList"
                list-type="picture"
                :max-count="1"
                @change="handleChange"
                :action="state.uploadUrlMap[state.fileType]">
                <a-button class="ml-4" type="primary">上传 </a-button>
              </a-upload>
              <div v-show="!state.isUploading && state.imageUrl">
                <img :src="state.imageUrl" alt="avatar" style="width: 700px" />
                <div class="answer-text">{{ state.answer }}</div>
              </div>
            </div>
          </a-tab-pane>
          <a-tab-pane key="2" tab="一键分析">
            <div class="pl-2">
              <a-button class="ml-4" type="primary" @click="handleAnalysisFast">打开AI助手一键分析</a-button>

              <!-- <a-button class="ml-4" type="primary" @click="handleAnalysis" :loading="state.isFastgptPending"
                >一键分析</a-button
              > -->
              <div v-if="!state.isFastgptPending" class="m-4 answer-text">
                {{ state.fastgptAnswer }}
              </div>
            </div>
          </a-tab-pane>
        </a-tabs>
      </div>
    </div>
  </div>
</template>
<script lang="ts" setup>
  import { reactive } from 'vue';
  import type { SelectProps } from 'ant-design-vue';
  import { ref } from 'vue';
  import { completions } from '/@/api/dataAnalysis';
  import { Spin } from 'ant-design-vue';

  defineOptions({
    name: 'base',
  });

  const handleChange = (info: string) => {
    const status = info.file.status;
    if (status !== 'uploading') {
      state.isUploading = true;
    }
    if (status === 'done') {
      state.isUploading = false;

      // 文件上传成功后的处理
       // 这里可以获取到接口返回的数据
      state.answer = info.file.response;
      state.imageUrl = URL.createObjectURL(info.file.originFileObj);
    }
  };
  const handleBlur = () => {
  };
  const handleFocus = () => {
  };
  const filterOption = (input: string, option: any) => {
    return option.value.toLowerCase().indexOf(input.toLowerCase()) >= 0;
  };

  const value = ref<string | undefined>(undefined);

  const state = reactive<any>({
    activeKey: '1',
    options: [
      { value: '1', label: '拉速曲线' },
      { value: '2', label: '称重曲线' },
    ],
    fileType: '1',
    answer: '',
    fileList: [],
    uploadUrlMap: {
      '1': 'http://61.174.171.60:18091/chat/speed',
      '2': 'http://61.174.171.60:18091/chat/weight',
    },
    imageUrl: '',
    isUploading: false,
    fastgptAnswer: '',
    isFastgptPending: false,
  });

  // 一键分析
  const handleAnalysisFast = async () => {
    const btn = document.getElementById('fastgpt-chatbot-button');

    const iframe = document.getElementById('fastgpt-chatbot-window');
    const childWindow = document.getElementById('fastgpt-chatbot-window').contentWindow;
    const style = window.getComputedStyle(iframe);
    const visibility = style.getPropertyValue('visibility');
    if (visibility === 'hidden') {
      btn.click();
    }
    childWindow.postMessage(
      {
        type: 'sendPrompt',
        text: '标准钢坏重量W=2532kg、标准钢坏长度L=11821mm，上一次估计的真实重量West=2533kg，以及上一次切割时的切割长度Length=11827，以及称所称出的对应重量Weight=2534kg',
      },
      '*',
    );
  };

  const handleAnalysis = async () => {
    try {
      state.isFastgptPending = true;
      const prompt = `标准钢坏重量W=7587kg、标准钢坏长度L=2100mm，上一次估计的真实重量West=7580kg，以及上一次切割时的切割长度Length=2091，以及称所称出的对应重量Weight=7582kg`;
      const res = await completions({ prompt });
      state.fastgptAnswer = res.choices[0].message.content;
      state.isFastgptPending = false;
    } catch (err) {
      state.isFastgptPending = false;
    }
  };
</script>
<style lang="less" scoped>
  .answer-text {
    font-size: 16px;
    color: #616161;
  }
</style>
