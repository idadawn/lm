<template>
  <div class="edit-form-wrapper">
    <div class="bg-white edit-form-header">
      <a-tabs v-model:activeKey="state.currTab" @change="changeTab">
        <a-tab-pane :key="item.key" :tab="item.name" v-for="item in state.tabs"></a-tab-pane>
      </a-tabs>
    </div>
    <div class="edit-form-content">
      <a-form
        layout="vertical"
        ref="formRef"
        :model="state.form"
        :colon="false"
        v-bind="formTailLayout"
        @finish="onFinish">
        <!-- 图表数据 -->
        <div v-show="state.currTab == 1 && currentChartId">
          <!-- chart -->
          <div v-if="state.chartTypes.includes(chartType)">
            <div class="flex flex-row items-center justify-between pb-4">
              <span class="font-bold">图表类型</span>
              <a-radio-group v-model:value="chartType">
                <a-radio v-for="item in state.chartTypeOptions" :key="item.id" :value="item.id">
                  <i class="iconfont" :class="item.class"></i>
                </a-radio>
              </a-radio-group>
            </div>
            <a-divider dashed />

            <!-- Y轴指标 -->
            <a-form-item label="指标" class="font-bold">
              <a-select
                v-model:value="yAxisMetrics"
                :options="metrics"
                mode="multiple"
                :fieldNames="{ value: 'id', label: 'name' }" />
            </a-form-item>
            <a-divider dashed />
            <!-- X轴维度 -->
            <a-form-item label="维度" class="font-bold">
              <a-select v-model:value="dimension" :options="dimensions" :fieldNames="{ value: 'id', label: 'name' }" />
            </a-form-item>
            <a-divider dashed />

            <!-- 筛选条件 -->
            <div class="filter-wrapper">
              <div class="flex flex-row items-center justify-between pb-4">
                <span class="font-bold">筛选条件</span>
                <div class="cursor-pointer filter-add" @click="addCondition"><PlusOutlined /></div>
              </div>
              <div
                class="flex flex-row items-center justify-between pt-1 pb-1 pl-1 pr-1 mb-2 bg-gray-200 group"
                v-for="(item, index) in conditions"
                :key="index">
                <div class="cursor-pointer" @click="editCondition(item, index)">{{ item.fieldName }}</div>
                <div class="hidden group-hover:block">
                  <CloseOutlined @click="delCondition(index)" />
                </div>
              </div>
            </div>
            <a-divider dashed />

            <!-- 标题-common -->
            <div class="flex flex-row items-center justify-between pb-4">
              <span class="font-bold">标题</span>
              <a-switch v-model:checked="titleShow" />
            </div>
            <a-form-item v-show="titleShow" label="" name="settingTitle">
              <jnpf-textarea v-model:value="settingTitle" placeholder="请输入" />
            </a-form-item>
            <a-divider dashed />

            <!-- Y轴标题 -->
            <div class="flex flex-row items-center justify-between pb-4">
              <span class="font-bold">Y轴标题</span>
              <a-switch v-model:checked="yAxisTitleShow" />
            </div>
            <a-form-item v-show="yAxisTitleShow" label="" name="yAxisTitle">
              <jnpf-textarea v-model:value="yAxisTitle" placeholder="请输入" />
            </a-form-item>
            <a-divider dashed />

            <!-- X轴标题 -->
            <div class="flex flex-row items-center justify-between pb-4">
              <span class="font-bold">X轴标题</span>
              <a-switch v-model:checked="xAxisTitleShow" />
            </div>
            <a-form-item v-show="xAxisTitleShow" label="" name="xAxisTitle">
              <jnpf-textarea v-model:value="xAxisTitle" placeholder="请输入" />
            </a-form-item>
            <a-divider dashed />
          </div>

          <!-- filter -->
          <template v-if="state.filterTypes.includes(chartType)">
            <a-form-item label="筛选方式" class="font-bold">
              <jnpf-select v-model:value="filterType" :options="state.filterOptions" />
            </a-form-item>
          </template>
        </div>

        <!-- 图表设置 -->
        <div v-show="state.currTab == 2">
          <a-form-item label="主题色" class="font-bold">
            <a-select v-model:value="chartTheme" :options="chartThemes" :fieldNames="{ value: 'name', label: 'name' }">
              <template #option="{ color, name }">
                <div class="flex flex-col">
                  <div>{{ name }}</div>
                  <div class="flex flex-row items-center">
                    <div
                      class="w-4 h-4"
                      :style="{ background: item }"
                      v-for="(item, index) in color"
                      :key="index"></div>
                  </div>
                </div>
              </template>
            </a-select>
          </a-form-item>
          <a-divider dashed />
          <!-- 拖拽吸附 -->
          <div class="flex flex-row items-center justify-between pb-4">
            <span class="font-bold">拖拽吸附</span>
            <a-switch v-model:checked="snapToGrid" />
          </div>
          <!-- 吸附大小 -->
          <a-form-item label="吸附距离" class="font-bold align-center" v-if="snapToGrid">
            <a-row>
              <a-col :span="14">
                <a-slider v-model:value="snapToGridSize" :min="1" :max="100" :step="1" />
              </a-col>
              <a-col :span="10">
                <a-input-number
                  v-model:value="snapToGridSize"
                  :min="0"
                  :max="100"
                  :step="1"
                  style="margin-left: 16px" />
              </a-col>
            </a-row>
          </a-form-item>
          <a-divider dashed />

          <!-- 缩放 -->
          <a-form-item label="缩放" class="font-bold align-center">
            <a-row :gutter="10">
              <a-col :span="18">
                <a-slider v-model:value="scaleX" :min="0" :max="1" :step="0.01" />
              </a-col>
              <a-col :span="6">
                <a-input-number v-model:value="scaleX" :min="0" :max="1" :step="0.01" />
              </a-col>
            </a-row>
          </a-form-item>
          <a-divider dashed />

          <!-- 视图 -->
          <a-row :gutter="10">
            <a-col span="12">
              <a-form-item class="font-bold" label="宽" name="viewportWidth">
                <a-input-number v-model:value="viewportWidth" :min="500" :max="5000" :step="100" placeholder="请输入" />
              </a-form-item>
            </a-col>
            <a-col span="12">
              <a-form-item class="font-bold" label="高" name="viewportHeight">
                <a-input-number
                  v-model:value="viewportHeight"
                  :min="500"
                  :max="5000"
                  :step="100"
                  placeholder="请输入" />
              </a-form-item>
            </a-col>
          </a-row>
          <a-divider dashed />

          <!-- 背景 -->
          <a-form-item class="font-bold" label="画布背景" name="background">
            <a-input v-model:value="background" placeholder="请输入" type="color" />
          </a-form-item>
          <a-divider dashed />

          <!-- 图表边框 -->
          <div class="flex flex-row items-center justify-between pb-4">
            <span class="font-bold">是否展示边框</span>
            <a-switch v-model:checked="layoutsBorderShow" />
          </div>
          <a-form-item class="font-bold" v-show="layoutsBorderShow" label="图表边框颜色" name="layoutsBorderColor">
            <a-input v-model:value="layoutsBorderColor" placeholder="请输入" type="color" />
          </a-form-item>
          <a-divider dashed />
        </div>
      </a-form>
    </div>
  </div>

  <Filter ref="filterModalRef" @ok="filterConfirm" />
</template>
<script lang="ts" setup>
  import { reactive, computed, ref } from 'vue';
  import { props as _props } from './props';
  import { useChartStore } from '/@/store/modules/chart';
  import Filter from './Filter.vue';
  import { BarChartOutlined, AreaChartOutlined, CloseOutlined, PlusOutlined } from '@ant-design/icons-vue';
  import { themes as chartThemes } from '/@/utils/helper/themes';
  import { ChartTypeEnum } from '/@/enums/chartEnum';

  defineOptions({
    name: 'zEditorElements',
  });

  const emit = defineEmits(['delete', 'update']);
  const chartStore = useChartStore();

  const state = reactive<any>({
    form: {},
    currTab: 1,
    tabs: [
      { key: 1, name: '图表数据' },
      { key: 2, name: '图表设置' },
    ],
    chartTypes: ['bar', 'line', 'markArea'],
    chartTypeOptions: [
      {
        id: 'bar',
        name: '柱状图',
        class: 'icon-barchart',
      },
      {
        id: 'line',
        name: '折线图',
        class: 'icon-linechart',
      },
      {
        id: 'markArea',
        name: '区域折线图',
        class: 'icon-bgchart',
      },
    ],
    filterTypes: ['select', 'date'],
    filterOptions: [
      {
        id: 'ByRange',
        fullName: '值范围筛选',
      },
      {
        id: 'ByDateRang',
        fullName: '时间范围筛选',
      },
      {
        id: 'ByValue',
        fullName: '值筛选',
      },
    ],
  });
  const filterModalRef = ref();

  const currentChartId = computed(() => {
    return chartStore.getCurrentChartId;
  });

  const metrics = computed(() => {
    return chartStore.getMetrics;
  });

  const dimensions = computed(() => {
    return chartStore.getDimensions;
  });

  const viewportWidth = computed({
    get: () => chartStore.getLayout.canvas.viewport?.width,
    set: value => (chartStore.getLayout.canvas.viewport.width = value),
  });
  const layoutsBorderColor = computed({
    get: () => chartStore.getLayout.canvas.layoutsStyle?.borderColor,
    set: value => (chartStore.getLayout.canvas.layoutsStyle.borderColor = value),
  });
  const layoutsBorderShow = computed({
    get: () => chartStore.getLayout.canvas.layoutsStyle?.borderShow,
    set: value => {
      chartStore.getLayout.canvas.layoutsStyle.borderShow = value;
    },
  });
  const viewportHeight = computed({
    get: () => chartStore.getLayout.canvas.viewport?.height,
    set: value => (chartStore.getLayout.canvas.viewport.height = value),
  });

  const scaleX = computed({
    get: () => parseFloat(chartStore.scale.x),
    set: value => (chartStore.scale.x = value),
  });
  const background = computed({
    get: () => chartStore.getLayout.canvas.layouts[0].style?.background,
    set: value => (chartStore.getLayout.canvas.layouts[0].style.background = value),
  });

  const chartTheme = computed({
    get: () => chartStore.getLayout.canvas.layoutsStyle?.chartTheme,
    set: value => {
      chartStore.getLayout.canvas.layoutsStyle.chartTheme = value;
      chartStore.setLayout({ type: 'update' });
    },
  });

  const snapToGrid = computed({
    get: () => chartStore.snapToGrid,
    set: value => (chartStore.snapToGrid = value),
  });
  const snapToGridSize = computed({
    get: () => chartStore.snapToGridSize,
    set: value => (chartStore.snapToGridSize = value),
  });

  const currentItem = computed(() => chartStore.getLayout.canvas.layouts[0].layout[chartStore.currentIndex]);

  const chartType = computed({
    get: () => currentItem.value?.type,
    set: value => {
      currentItem.value.type = value;
      chartStore.changeChartType(currentChartId.value, value, currentItem.value);
    },
  });

  const filterType = computed({
    get: () => currentItem.value?.filter?.conditions[0]?.filterType,
    set: value => {
      currentItem.value.filter.conditions[0].filterType = value;
      // chartStore.setLayout({ type: 'update' });
    },
  });
  const dimension = computed({
    get: () => currentItem.value?.query?.dimension,
    set: value => {
      currentItem.value.query.dimension = value;
      chartStore.updateLayout();
    },
  });

  const titleShow = computed({
    get: () => currentItem.value?.setting?.titleShow,
    set: value => (currentItem.value.setting.titleShow = value),
  });

  const settingTitle = computed({
    get: () => currentItem.value?.setting?.title,
    set: value => (currentItem.value.setting.title = value),
  });

  const yAxisTitle = computed({
    get: () => currentItem.value?.setting?.yAxis?.title,
    set: value => (currentItem.value.setting.yAxis.title = value),
  });
  const yAxisTitleShow = computed({
    get: () => currentItem.value?.setting?.yAxis?.titleShow,
    set: value => (currentItem.value.setting.yAxis.titleShow = value),
  });

  const yAxisMetrics = computed({
    get: () => currentItem.value?.query?.metrics,
    set: value => {
      currentItem.value.query.metrics = value;
      chartStore.updateLayout();
    },
  });

  const xAxisTitle = computed({
    get: () => currentItem.value?.setting?.xAxis?.title,
    set: value => (currentItem.value.setting.xAxis.title = value),
  });
  const xAxisTitleShow = computed({
    get: () => currentItem.value?.setting?.xAxis?.titleShow,
    set: value => (currentItem.value.setting.xAxis.titleShow = value),
  });

  // 图表筛选条件
  const conditions = computed({
    get: () => currentItem.value?.filter?.conditions,
    set: value => (currentItem.value.filter.conditions = value),
  });

  const formTailLayout = {
    labelCol: { style: { width: '110px' } },
    // wrapperCol: { offset: 4 },
  };

  const onFinish = values => {
    // emit('update', values);
  };

  // 添加筛选条件
  const addCondition = () => {
    state.isEdit = false;
    filterModalRef.value?.openModal();
  };
  // 编辑筛选条件
  const editCondition = (item, index) => {
    state.isEdit = true;
    state.editIndex = index;
    filterModalRef.value?.openModal(item);
  };
  // 筛选条件确认
  const filterConfirm = item => {
    const {
      operator,
      field,
      dataType,
      fieldName,
      checkedList,
      filterType,
      minValue,
      maxValue,
      minValueChecked,
      maxValueChecked,
    } = item;
    const condition = {
      operator,
      field,
      dataType,
      fieldName,
      checkedList,
      filterType,
      minValue,
      maxValue,
      minValueChecked,
      maxValueChecked,
    };
    // 如果是编辑
    if (state.isEdit) {
      conditions.value.splice(state.editIndex, 1, condition);
    } else {
      conditions.value.push(condition);
    }
  };
  const delCondition = index => {
    conditions.value.splice(index, 1);
  };

  const updateItem = () => {
    emit('update', state.form);
  };

  const changeTab = () => {};
</script>
<style lang="less" scoped>
  .edit-form-wrapper {
    display: flex;
    flex-direction: column;
    height: 100%;

    ::v-global(.ant-divider-dashed) {
      margin: 0 0 8px;
    }

    .edit-form-content {
      flex: 1;
      overflow: auto;

      .filter-wrapper {
      }
    }

    .edit-form-header {
    }
  }

  .overflow-auto {
    overflow: auto;
  }
</style>
