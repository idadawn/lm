<style lang="less">
  .date-filter-wrapper {
    // background: #fff;
  }
</style>
<template>
  <div class="date-filter-wrapperr" :style="{ width, height }">
    <BasicForm @register="registerForm" @field-value-change="valueChange" @reset="handleReset" />
  </div>
</template>
<script lang="ts" setup>
  import { reactive,computed } from 'vue';
  import { BasicForm, useForm } from '/@/components/Form';
  import { useChartStore } from '/@/store/modules/chart';
  const props = defineProps({
    width: {
      type: String as PropType<string>,
      default: '100%',
    },
    height: {
      type: String as PropType<string>,
      default: '300px',
    },
    currentLayout: {
      type: Number as PropType<number>,
      default: 0,
    },
  });
  const chartStore = useChartStore();

  const defaultValue = computed({
    get: () => chartStore.getLayout.canvas.layouts[0].layout[props.currentLayout].filter?.values,
    set: value => {
      console.log('filter change')
      chartStore.setLayout({type:'update'});
      chartStore.getLayout.canvas.layouts[0].layout[props.currentLayout].filter.values = value
    },
  });

  const [registerForm, { resetFields }] = useForm({
    baseColProps: { span: 16 },
    // actionColOptions: { span: 24 },
    showActionButtonGroup: false,
    showAdvancedButton: false,
    compact: true,
    labelAlign: 'left',
    labelWidth: 60,
    schemas: [
      {
        field: 'keyword',
        label: '',
        defaultValue: defaultValue,
        component: 'DateRange',
        componentProps: {
          placeholder: ['开始日期', '结束日期'],
          submitOnPressEnter: true,
        },
      },
    ],
  });
  
  const state = reactive({
    keyword: '',
  });
  function valueChange(key, value) {
    console.log(key, value, 'values');
    chartStore.setLayout({type:'update'});
  }
  function handleReset() {
    state.keyword = '';
  }
</script>
