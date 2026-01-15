<template>
  <div class="page-content-wrapper">
    <div class="page-content-wrapper-center">
      <div class="page-content-wrapper-content">
        <div style="padding: 20px">
          <h3>录音上传</h3>
          <div style="font-size: 14px">
            <h3>录音时长：{{ recorder && recorder.duration.toFixed(4) }}</h3>

            <br />

            <a-button type="primary" @click="handleStart">开始录音</a-button>

            <a-button type="info" @click="handlePause">暂停录音</a-button>

            <a-button type="success" @click="handleResume">继续录音</a-button>

            <a-button type="warning" @click="handleStop">停止录音</a-button>

            <br />

            <br />

            <h3>
              播放时长：{{
                recorder && (playTime > recorder.duration ? recorder.duration.toFixed(4) : playTime.toFixed(4))
              }}
            </h3>

            <br />

            <a-button type="primary" @click="handlePlay">播放录音</a-button>

            <a-button type="info" @click="handlePausePlay">暂停播放</a-button>

            <a-button type="success" @click="handleResumePlay">继续播放</a-button>

            <a-button type="warning" @click="handleStopPlay">停止播放</a-button>

            <a-button type="error" @click="handleDestroy">销毁录音</a-button>

            <a-button type="primary" @click="uploadRecord">上传</a-button>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script lang="ts" setup>
  import { ref, reactive } from 'vue';
  import Recorder from 'js-audio-recorder';
  import { useMessage } from '/@/hooks/web/useMessage';
  const { createMessage } = useMessage();

  let recorder = ref<any>(null);
  let playTime = ref<number>(0);
  let timer = ref<any>(null);
  let src = ref<any>(null);

  recorder.value = new Recorder();

  // 开始录音

  function handleStart() {
    // 获取麦克风权限
    Recorder.getPermission().then(
      () => {
        console.log('开始录音');

        recorder.value.start(); // 开始录音
      },
      error => {
        createMessage.info('请先允许该网页使用麦克风');

        console.log(`${error.name} : ${error.message}`);
      },
    );
  }
  function handlePause() {
    console.log('暂停录音');

    recorder.value.pause(); // 暂停录音
  }

  function handleResume() {
    console.log('恢复录音');

    recorder.value.resume(); // 恢复录音
  }

  function handleStop() {
    console.log('停止录音');

    recorder.value.stop(); // 停止录音
  }

  function handlePlay() {
    console.log('播放录音');

    recorder.value.play(); // 播放录音 // 播放时长

    timer.value = setInterval(() => {
      try {
        playTime.value = recorder.value.getPlayTime();
      } catch (error) {
        timer.value = null;
      }
    }, 100);
  }

  function handlePausePlay() {
    console.log('暂停播放');

    recorder.value.pausePlay(); // 暂停播放 // 播放时长

    playTime.value = recorder.value.getPlayTime();

    timer.value = null;
  }

  function handleResumePlay() {
    console.log('恢复播放');

    recorder.value.resumePlay(); // 恢复播放 // 播放时长

    timer.value = setInterval(() => {
      try {
        playTime.value = recorder.value.getPlayTime();
      } catch (error) {
        timer.value = null;
      }
    }, 100);
  }

  function handleStopPlay() {
    console.log('停止播放');

    recorder.value.stopPlay(); // 停止播放 // 播放时长

    playTime.value = recorder.value.getPlayTime();

    timer.value = null;
  }

  function handleDestroy() {
    console.log('销毁实例');

    recorder.value.destroy(); // 毁实例

    timer.value = null;
  }

  function uploadRecord() {
    if (recorder.value == null || recorder.value.duration === 0) {
      createMessage.error('请先录音');

      return false;
    }

    recorder.value.pause(); // 暂停录音

    timer.value = null;

    console.log('上传录音'); // 上传录音

    const formData = new FormData();

    const blob = recorder.value.getWAVBlob(); // 获取wav格式音频数据 // 此处获取到blob对象后需要设置fileName满足当前项目上传需求，其它项目可直接传把blob作为file塞入formData

    const newbolb = new Blob([blob], { type: 'audio/wav' });

    const fileOfBlob = new File([newbolb], new Date().getTime() + '.wav');

    formData.append('file', fileOfBlob);

    const url = window.URL.createObjectURL(fileOfBlob);

    src.value = url;
    console.log(src.value);
    console.log(formData);
    // const axios = require('axios');
    // axios.post(url, formData).then(res => {
    //   console.log(res.data.data[0].url);
    // });
  }
</script>
