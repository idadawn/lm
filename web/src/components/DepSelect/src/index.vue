<template>
  <div>
    <a-select
      ref="select"
      mode="multiple"
      show-search
      allowClear
      placeholder="请选择"
      v-model:value="targetCheckedArrWeidu"
      :filter-option="filterOption"
      @change="targetWeiduChange"
      :options="targetColumnArrWeidu"
      :fieldNames="{
        label: 'fieldName',
        value: 'field',
      }">
    </a-select>
  </div>
</template>
<script lang="ts" setup>
  import { ref, reactive, watch } from 'vue';
  const emit = defineEmits(['depSelectEmits', 'depSelectItemEmits']);
  const props = defineProps({
    dataArr: { default: [] },
    checkedArr: { default: [] },
  });

  const targetColumnArrWeidu = ref<any[]>([]);
  const targetCheckedArrWeidu = ref<any[]>([]);
  const targetCheckedArrWeiduItem = ref<any[]>([]);
  const targetColumnArr = ref<any[]>([]);

  watch(
    () => props.dataArr,
    val => {
      targetColumnArrWeidu.value = props.dataArr;
      targetColumnArr.value = props.dataArr;
      targetColumnArrWeidu.value = [
        {
          dataType: 'all',
          field: 'all',
          fieldName: '全部',
        },
        ...targetColumnArrWeidu.value,
      ];
      targetCheckedArrWeidu.value = props.checkedArr;
    },
  );

  watch(
    () => props.checkedArr,
    val => {
      targetCheckedArrWeidu.value = props.checkedArr;
    },
  );

  // 下拉框搜索
  const filterOption = (input: string, item: any) => {
    return item.fieldName.toLowerCase().indexOf(input.toLowerCase()) >= 0;
  };

  // 切换维度
  function targetWeiduChange(value, node) {
    // 
    // 
    if (value.includes('all')) {
      const arr = [];
      const arrItem = [];
      targetColumnArr.value.forEach(item => {
        arr.push(item.field);
        arrItem.push(item);
      });
      targetCheckedArrWeidu.value = arr;
      targetCheckedArrWeiduItem.value = arrItem;
    } else {
      targetCheckedArrWeiduItem.value = node;
    }

    emit('depSelectEmits', targetCheckedArrWeidu.value);
    emit('depSelectItemEmits', targetCheckedArrWeiduItem.value);
  }
</script>
