<template>
  <div>
    <div class="text-center">
      <a-button type="primary" :disabled="state.disabledGrading" @click="openCreationFun">创建分级</a-button>
    </div>
    <a-list :loading="initLoading" item-layout="horizontal" :data-source="createTable">
      <template #renderItem="{ item }">
        <a-list-item>
          <template #actions>
            <span class="text-xs text-blue-500 cursor-pointer" @click="create_edit(item)">编辑</span>
            <span class="text-xs text-red-600 cursor-pointer" @click="create_delete(item.id)">删除</span>
          </template>
          <a-list-item-meta :description="item.status">
            <template #title>
              <span> {{ item.name }}</span>
            </template>
          </a-list-item-meta>
          <div :style="{ color: item.status_color }">{{ item.value }}</div>
        </a-list-item>
      </template>
    </a-list>
  </div>
  <!-- 新建分级 -->
  <a-modal width="600px" v-model:visible="state.newCreateVisible" title="新建分级" @ok="createHandleOk">
    <a-form ref="createAddForm" :model="createAddInfo" :label-col="{ span: 4 }" :wrapper-col="{ span: 18 }">
      <a-tabs v-model:activeKey="activeKey" centered>
        <a-tab-pane :key="GradingTypeEnum.Value" tab="值">
          <a-form-item label="值" name="value">
            <a-row :gutter="16">
              <a-col :span="6">
                <a-select ref="select" v-model:value="createAddInfo.rangType" placeholder="请选择">
                  <a-select-option value="Value">数值</a-select-option>
                  <a-select-option value="Percent">百分比</a-select-option>
                </a-select>
              </a-col>
              <a-col :span="18">
                <a-form-item-rest>
                  <a-input v-model:value="createAddInfo.value"></a-input>
                </a-form-item-rest>
              </a-col>
            </a-row>
          </a-form-item>
        </a-tab-pane>
        <a-tab-pane :key="GradingTypeEnum.Trend" tab="区间" force-render>
          <a-form-item label="趋势" name="trend">
            <a-select ref="select" v-model:value="createAddInfo.trend" placeholder="请选择">
              <a-select-option value="Up">提升/增加</a-select-option>
              <a-select-option value="Down">下降/减少</a-select-option>
            </a-select>
          </a-form-item>
          <a-form-item label="区间" name="rangType">
            <a-row :gutter="16">
              <a-col :span="6">
                <a-select ref="select" v-model:value="createAddInfo.rangType" placeholder="请选择">
                  <a-select-option value="Value">数值</a-select-option>
                  <a-select-option value="Percent">百分比</a-select-option>
                </a-select>
              </a-col>
              <a-col :span="18">
                <a-form-item-rest>
                  <a-input v-model:value="createAddInfo.value"></a-input>
                </a-form-item-rest>
              </a-col>
            </a-row>
          </a-form-item>
        </a-tab-pane>
      </a-tabs>
      <a-form-item label="分级名称" name="name">
        <a-input v-model:value="createAddInfo.name" allowClear placeholder="请输入"></a-input>
      </a-form-item>
      <a-form-item label="状态" name="status">
        <a-row :gutter="16">
          <a-col :span="18">
            <a-select
              ref="select"
              v-model:value="createAddInfo.status"
              placeholder="请选择"
              @change="createStatusChange">
              <a-select-option v-for="item in metricStatusArr" :value="item.id" :item="item">{{
                item.name
              }}</a-select-option>
            </a-select>
          </a-col>
          <a-col :span="6">
            <div :style="{ background: createAddInfo.status_color }" class="divColor"></div>
          </a-col>
        </a-row>
      </a-form-item>
    </a-form>
  </a-modal>
</template>
<script setup lang="ts">
  import { reactive, ref, toRefs, watch } from 'vue';
  import {
    List as AList,
    ListItem as AListItem,
    ListItemMeta as AListItemMeta,
    message,
    Modal as AModal,
  } from 'ant-design-vue';
  import {
    deleteMetricGraded,
    getMetricCovstatusOptions,
    getMetricGraded,
    postMetricGraded,
    postMetricGradedList,
    putMetricGraded,
  } from '/@/api/targetDefinition';
  import { ResultEnum } from '/@/enums/httpEnum';
  import { GradingTypeEnum } from './const';

  defineOptions({
    name: 'Grading',
  });

  const props = defineProps<{
    nodeForm: any;
  }>();

  const initLoading = ref(false);

  const { nodeForm } = toRefs(props);

  /**
   * 分级Model状态
   * */
  const state = reactive({
    // 分级按钮是否可用
    disabledGrading: true,
    //打开分级
    createVisible: false,
    //新建分级
    newCreateVisible: false,
  });

  // 状态列表
  const metricStatusArr = ref<{ id: string | number; name: string; color: string }[]>([]);

  // 分级列表data
  const createTable = ref([]);

  const initCreateTable = {
    id: '', //分级id
    metricId: '', //指标的id
    value: '', // 值
    name: '', // 分级名称
    status: undefined, //状态
    status_color: '#000', //颜色
    trend: undefined, //趋势
    rangType: undefined, //区间
  };

  // 创建分级-表单
  const createAddInfo = reactive({
    ...initCreateTable,
  });

  // 创建分级类型
  const activeKey = ref(GradingTypeEnum.Value);

  watch(
    () => nodeForm.value.metricId,
    async newVal => {
      // 创建分级-状态
      if (metricStatusArr.value.length === 0) {
        const result = await getMetricCovstatusOptions();
        metricStatusArr.value = result.data;
      }

      if (newVal) {
        createAddInfo.metricId = newVal;
        state.disabledGrading = false;
        postMetricGradedListFun();
      } else {
        state.disabledGrading = true;
        createTable.value = [];
      }
    },
    {
      immediate: true,
    },
  );

  const openCreationFun = () => {
    state.newCreateVisible = true;
    newGradingClearFun();
  };

  function postMetricGradedListFun() {
    // 获取指标分级列表
    postMetricGradedList(nodeForm.value.metricId).then(res => {
      if (res.data && res.data.length > 0) {
        res.data.forEach(item => {
          // 趋势
          if (item.trend === 'Up') {
            item.trend = '提升/增加';
          } else if (item.trend === 'Down') {
            item.trend = '下降/减少';
          }
          // 区间
          if (item.rangType === 'Value') {
            item.rangType = '数值';
          } else if (item.rangType === 'Percent') {
            item.rangType = '百分比';
          }
          // 状态
          metricStatusArr.value.forEach(obj => {
            if (item.status === obj.id) {
              item.status = obj.name;
              item.status_color = obj.color;
            }
          });
        });
      }
      createTable.value = res.data;
    });
  }

  // 创建分级
  function newGradingClearFun() {
    createAddInfo.id = initCreateTable.id;
    createAddInfo.value = initCreateTable.value;
    createAddInfo.name = initCreateTable.name;
    createAddInfo.status = initCreateTable.status;
    createAddInfo.status_color = initCreateTable.status_color;
    createAddInfo.trend = initCreateTable.trend;
    createAddInfo.rangType = initCreateTable.rangType;
  }

  // 切换状态时更新颜色
  function createStatusChange(_value, obj) {
    createAddInfo.status_color = obj.item.color;
  }

  // 提交分级
  function createHandleOk() {
    if (createAddInfo.id) {
      putMetricGraded(createAddInfo.id, createAddInfo).then(res => {
        if (res.code === ResultEnum.SUCCESS) {
          message.success(res.msg);
          state.newCreateVisible = false;
          newGradingClearFun();
          postMetricGradedListFun();
        }
      });
    } else {
      postMetricGraded(createAddInfo).then(res => {
        if (res.code === ResultEnum.SUCCESS) {
          message.success(res.msg);
          state.newCreateVisible = false;
          newGradingClearFun();
          postMetricGradedListFun();
        }
      });
    }
  }

  //创建分级-编辑
  function create_edit(record) {
    getMetricGraded(record.id).then(res => {
      if (res.data.trend) {
        activeKey.value = GradingTypeEnum.Trend;
      } else {
        activeKey.value = GradingTypeEnum.Value;
      }
      state.newCreateVisible = true;
      createAddInfo.metricId = res.data.metricId;
      createAddInfo.id = res.data.id;
      createAddInfo.value = res.data.value;
      createAddInfo.name = res.data.name;
      createAddInfo.status = res.data.status;
      createAddInfo.status_color = res.data.status_color;
      createAddInfo.trend = res.data.trend;
      createAddInfo.rangType = res.data.rangType;
    });
  }

  //创建分级-删除
  function create_delete(id) {
    AModal.confirm({
      iconType: 'warning',
      title: '提示',
      content: '您确定要删除数据吗, 是否继续?',
      onOk() {
        return deleteMetricGraded(id).then(res => {
          if (res.code === 200) {
            message.success(res.msg);
            state.newCreateVisible = false;
            postMetricGradedListFun();
          }
        });
      },
      onCancel() {},
    });
  }
</script>
<style scoped>
  .divColor {
    width: 100px;
    height: 30px;
  }
</style>
