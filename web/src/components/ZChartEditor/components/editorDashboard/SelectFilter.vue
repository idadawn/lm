<template>
  <!-- 添加筛选 -->
  <div class="select-wrapper" :style="{ width, height }">
    <spin v-if="!options.type" class="loading" />
    <div>
      <div v-if="filterType == 'ByRange'">
        <a-row :gutter="16">
          <a-col :span="11">
            <a-form-item label="最小值">
              <a-input-number v-model:value="minValue" />
            </a-form-item>
          </a-col>
          <a-col :span="2" style="display: flex; align-items: center; justify-content: center">-</a-col>
          <a-col :span="11">
            <a-form-item label="最大值">
              <a-input-number v-model:value="maxValue" />
            </a-form-item>
          </a-col>
        </a-row>
        <a-row :gutter="16">
          <a-col :span="11">
            <a-checkbox v-model:checked="minValueChecked">包含</a-checkbox>
          </a-col>
          <a-col :span="2"></a-col>
          <a-col :span="11"><a-checkbox v-model:checked="maxValueChecked">包含</a-checkbox></a-col>
        </a-row>
      </div>

      <div class="filter-box" v-if="filterType == 'ByValue'">
        <a-checkbox v-model:checked="filterState.checkAll" @change="checkAll"> 全选 </a-checkbox>
        <a-checkbox-group v-model:value="checkedKeys" :options="checkOptions" />
      </div>

      <div v-if="filterType == 'ByDateRang'">
        <a-row :gutter="16">
          <a-col :span="24">
            <a-form-item label="开始和结束时间">
              <jnpf-date-range
                v-model:value="dateRange"
                format="YYYY/MM/DD"
                valueFormat="YYYY/MM/DD HH:mm:ss"
                allowClear />
            </a-form-item>
          </a-col>
        </a-row>
        <a-row :gutter="16">
          <a-col :span="11">
            <a-checkbox v-model:checked="minValueChecked">包含</a-checkbox>
          </a-col>
          <a-col :span="2"></a-col>
          <a-col :span="11"><a-checkbox v-model:checked="maxValueChecked">包含</a-checkbox></a-col>
        </a-row>
      </div>
    </div>
  </div>
</template>
<script lang="ts" setup>
  import { reactive, computed } from 'vue';
  import { props as _props } from './props';
  import { Spin } from 'ant-design-vue';

  const chartStore = useChartStore();
  import { useChartStore } from '/@/store/modules/chart';

  defineOptions({
    name: 'Filter',
  });
  const props = defineProps({
    options: {
      type: Object as PropType<any>,
      default: () => {
        return {
          data: [
            // {
            //   title: 'parent 1-0',
            //   key: '0-0-0',
            // },
          ],
        };
      },
    },
    item: {
      type: Object as PropType<any>,
      default: () => {
        return {};
      },
    },
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
  const filterState: any = reactive({
    fieldName: '',
    field: '',
    dataType: '',
    select_weidu: undefined, //筛选选择维度
    filterType: 'ByRange', //筛选方式
    filterModel: [], //过滤的值
    checkAll: false,
    indeterminate: false,
    checkedList: [],
    plainOptions: computed(() => {
      return props.options.data;
    }),
    sqlValue: '', //sql语句
    dateValue: [], //日期
    minValue: undefined, //最小值
    maxValue: undefined, //最大值
    minValueChecked: true, //最小值包含
    maxValueChecked: true, //最大值包含
    metas: [], //当前维度的信息
  });

  const currentItem: any = computed(() => chartStore.getLayout.canvas.layouts[0].layout[props.currentLayout]);

  const checkedKeys = computed({
    get: () => currentItem.value?.filter?.conditions[0].checkedList,
    set: value => {
      filterState.checkAll = value.length === checkOptions.value.length;
      currentItem.value.filter.conditions[0].checkedList = value;
      chartStore.setLayout({ type: 'update' });
    },
  });
  const filterType = computed({
    get: () => currentItem.value?.filter?.conditions[0]?.filterType,
    set: value => {
      currentItem.value.filter.conditions[0].filterType = value;
      chartStore.setLayout({ type: 'update' });
    },
  });
  const minValue = computed({
    get: () => currentItem.value?.filter?.conditions[0].minValue,
    set: value => {
      currentItem.value.filter.conditions[0].minValue = value;
      chartStore.setLayout({ type: 'update' });
    },
  });
  const maxValue = computed({
    get: () => currentItem.value?.filter?.conditions[0].maxValue,
    set: value => {
      currentItem.value.filter.conditions[0].maxValue = value;
      chartStore.setLayout({ type: 'update' });
    },
  });
  const dateRange = computed({
    get: () => currentItem.value?.filter?.conditions[0].dateRange,
    set: value => {
      currentItem.value.filter.conditions[0].dateRange = value;
      chartStore.setLayout({ type: 'update' });
    },
  });

  const minValueChecked = computed({
    get: () => currentItem.value?.filter?.conditions[0].minValueChecked,
    set: value => {
      currentItem.value.filter.conditions[0].minValueChecked = value;
      chartStore.setLayout({ type: 'update' });
    },
  });
  const maxValueChecked = computed({
    get: () => currentItem.value?.filter?.conditions[0].maxValueChecked,
    set: value => {
      currentItem.value.filter.conditions[0].maxValueChecked = value;
      chartStore.setLayout({ type: 'update' });
    },
  });

  const checkOptions = computed(() => {
    return props.options.data;
  });

  // 全选
  const checkAll = (e: any) => {
    checkedKeys.value = e.target.checked ? checkOptions.value : [];
  }
</script>
<style lang="less" scoped>
  .select-wrapper {
    position: relative;

    .loading {
      position: absolute;
      top: 50%;
      left: 50%;
      transform: translate(-50%, -50%);
    }
  }

  .filter-box {
    padding: 10px;
    border-radius: 5px;
    height: 200px;
    overflow-y: auto;
  }

  :deep(.ant-tag-close-icon) {
    color: #1f3fbd;
  }

  :deep(.ant-checkbox-group) {
    display: flex;
    flex-direction: column;
  }
</style>
