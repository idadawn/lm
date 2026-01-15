<template>
  <a-collapse-panel>
    <template #header>日程设置</template>
    <a-form-item label="默认视图">
      <jnpf-radio v-model:value="activeData.defaultView" :options="defaultViewOptions" optionType="button" button-style="solid" />
    </a-form-item>
    <a-form-item label="显示农历">
      <a-switch v-model:checked="activeData.showLunarCalendar" />
    </a-form-item>
    <a-form-item label="周第一天">
      <jnpf-select v-model:value="activeData.firstDay" :options="firstDayOptions" placeholder="请选择" />
    </a-form-item>
    <Refresh :refresh="activeData.refresh" />
  </a-collapse-panel>
</template>
<script lang="ts" setup>
  import Refresh from './RRefresh.vue';

  defineProps(['activeData', 'showType']);
  const defaultViewOptions = [
    { id: 'timeGridDay', fullName: '日' },
    { id: 'timeGridWeek', fullName: '周' },
    { id: 'dayGridMonth', fullName: '月' },
  ];
  const firstDayOptions = [
    { id: 1, fullName: '一' },
    { id: 2, fullName: '二' },
    { id: 3, fullName: '三' },
    { id: 4, fullName: '四' },
    { id: 5, fullName: '五' },
    { id: 6, fullName: '六' },
    { id: 0, fullName: '日' },
  ];
</script>
