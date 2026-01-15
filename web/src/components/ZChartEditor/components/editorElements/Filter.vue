<template>
  <!-- 添加筛选 -->
  <a-modal width="500px" v-model:visible="visible" title="维度" @ok="handleSubmit">
    <a-form
      layout="vertical"
      :model="filterState"
      :label-col="{ span: 8 }"
      :wrapper-col="{ span: 24 }"
      style="padding: 20px">
      <a-form-item label="选择维度">
        <a-select
          ref="select"
          v-model:value="filterState.select_weidu"
          @change="dimensionChange"
          :disabled="shaixuanDisable"
          placeholder="请选择">
          <template v-for="item in weiduTagArr">
            <a-select-option :value="item.field" :fieldName="item.fieldName" :dataType="item.dataType">{{
              item.fieldName
            }}</a-select-option>
          </template>
        </a-select>
      </a-form-item>
      <a-form-item label="筛选方式" v-if="filterState.select_weidu">
        <a-radio-group v-model:value="filterState.filterType">
          <template v-for="modelValue in filterState.filterModel">
            <a-radio-button value="ByRange" v-if="modelValue == 'ByRange'">值范围筛选</a-radio-button>
            <a-radio-button value="ByDateRang" v-if="modelValue == 'ByDateRang'">时间范围筛选</a-radio-button>
            <a-radio-button value="ByValue" v-if="modelValue == 'ByValue'">值筛选</a-radio-button>
            <!-- <a-radio-button value="expression">表达式</a-radio-button> -->
          </template>
        </a-radio-group>
        <div style="margin-top: 20px">
          <div v-if="filterState.filterType == 'ByRange'">
            <a-row :gutter="16">
              <a-col :span="11">
                <a-form-item label="最小值">
                  <a-input-number v-model:value="filterState.minValue" />
                </a-form-item>
              </a-col>
              <a-col :span="2" style="display: flex; align-items: center; justify-content: center">-</a-col>
              <a-col :span="11">
                <a-form-item label="最大值">
                  <a-input-number v-model:value="filterState.maxValue" />
                </a-form-item>
              </a-col>
            </a-row>
            <a-row :gutter="16">
              <a-col :span="11">
                <a-checkbox v-model:checked="filterState.minValueChecked">包含</a-checkbox>
              </a-col>
              <a-col :span="2"></a-col>
              <a-col :span="11"><a-checkbox v-model:checked="filterState.maxValueChecked">包含</a-checkbox></a-col>
            </a-row>
          </div>

          <div class="shaixuan_box" v-if="filterState.filterType == 'ByValue'">
            <a-checkbox
              v-model:checked="filterState.checkAll"
              :indeterminate="filterState.indeterminate"
              @change="onCheckAllChange_shaixuan">
              全选
            </a-checkbox>

            <a-checkbox-group v-model:value="filterState.checkedList" :options="filterState.plainOptions" />
          </div>

          <div v-if="filterState.filterType == 'ByDateRang'">
            <a-row :gutter="16">
              <a-col :span="24">
                <a-form-item label="开始和结束时间">
                  <jnpf-date-range
                    v-model:value="filterState.dateValue"
                    format="YYYY/MM/DD"
                    valueFormat="YYYY/MM/DD HH:mm:ss"
                    allowClear />
                </a-form-item>
              </a-col>
            </a-row>
            <a-row :gutter="16">
              <a-col :span="11">
                <a-checkbox v-model:checked="filterState.minValueChecked">包含</a-checkbox>
              </a-col>
              <a-col :span="2"></a-col>
              <a-col :span="11"><a-checkbox v-model:checked="filterState.maxValueChecked">包含</a-checkbox></a-col>
            </a-row>
          </div>
          <!-- <div v-if="filterState.filterType == 'expression'">
          <p style="margin: 10px 0 5px; color: #999">表达式使用 SQL 语法。</p>
          <a-textarea
            v-model:value="filterState.sqlValue"
            placeholder="请输入"
            :auto-size="{ minRows: 5, maxRows: 6 }" />
        </div> -->
        </div>
      </a-form-item>
    </a-form>
  </a-modal>
</template>
<script lang="ts" setup>
  import { reactive, ref, onMounted } from 'vue';
  import { props as _props } from './props';
  import { postMetricFilter_model_data } from '/@/api/targetDefinition';
  import { Modal as AModal } from 'ant-design-vue';

  defineOptions({
    name: 'Filter',
  });
  defineExpose({ openModal });
  const visible = ref(false);
  const props = defineProps({
    visible: {
      type: Boolean,
      default: false,
    },
  });
  const emit = defineEmits(['ok']);
  const state = reactive({
    isEdit: false,
  });
  const resetForm = () => {
    filterState.fieldName = '';
    filterState.field = '';
    filterState.dataType = '';
    filterState.filterType = 'ByValue'; //筛选方式
    filterState.filterModel = []; //过滤的值
    filterState.checkAll = false;
    filterState.indeterminate = false;
    filterState.checkedList = [];
    filterState.plainOptions = [];
    filterState.sqlValue = ''; //sql语句
    filterState.dateValue = []; //日期
    filterState.minValue = undefined; //最小值
    filterState.maxValue = undefined; //最大值
    filterState.minValueChecked = true; //最小值包含
    filterState.maxValueChecked = true; //最大值包含
    filterState.metas = []; //当前维度的信息
  };

  const filterState: any = reactive({
    fieldName: '',
    field: '',
    dataType: '',
    select_weidu: undefined, //筛选选择维度
    filterType: 'ByValue', //筛选方式
    filterModel: [], //过滤的值
    checkAll: false,
    indeterminate: false,
    checkedList: [],
    plainOptions: [],
    sqlValue: '', //sql语句
    dateValue: [], //日期
    minValue: undefined, //最小值
    maxValue: undefined, //最大值
    minValueChecked: true, //最小值包含
    maxValueChecked: true, //最大值包含
    metas: [], //当前维度的信息
  });
  const shaixuanDisable = ref(false);
  const weiduTagArr = ref([
    { primaryKey: 0, allowNull: 1, dataLength: '0', dataType: 'date', field: 'order_time', fieldName: '订单日期' },
    { primaryKey: 0, allowNull: 1, dataLength: '0', dataType: 'int', field: 'order_year', fieldName: '订单年份' },
    { primaryKey: 0, allowNull: 1, dataLength: '0', dataType: 'int', field: 'order_month', fieldName: '订单月份' },
    { primaryKey: 0, allowNull: 1, dataLength: '255', dataType: 'varchar', field: 'region', fieldName: '店铺区域' },
  ]);

  function openModal(item) {
    visible.value = true;
    state.isEdit = item ? true : false;
    // 编辑
    if (state.isEdit) {
      dimensionChange(
        '',
        {
          value: item?.field,
          dataType: item?.dataType,
        },
        item,
      );
    } else {
      resetForm();
    }
  }
  function handleCancel() {
    visible.value = false;
  }
  function handleSubmit() {
    handleCancel();
    emit('ok', filterState);
  }

  // 全选
  function onCheckAllChange_shaixuan(e: any) {
    Object.assign(filterState, {
      checkedList: e.target.checked ? filterState.plainOptions : [],
      indeterminate: false,
    });
  }

  // 筛选选择维度的change事件
  function dimensionChange(value, item, editData) {
    // 初始化数据
    console.log('dimensionChange----', value, item);
    resetForm();
    filterState.fieldName = item.fieldName;
    filterState.field = item.value;
    filterState.dataType = item.dataType;

    const data = {
      linkId: '482106329557630917',
      schemaName: 'retail_stores',
      columnField: {
        field: item.value,
        dataType: item.dataType,
      },
      orderByField: {
        field: item.value,
        dataType: item.dataType,
      },
    };
    postMetricFilter_model_data(data).then(res => {
      const isEdit = editData ? true : false;
      // 筛选选择维度
      filterState.plainOptions = res.data.data;

      if (isEdit) {
        // 筛选选择维度
        filterState.select_weidu = editData.field;
        // 筛选方式
        filterState.filterType = editData.filterType;
        // 最小值最大值
        filterState.minValue = editData.minValue;
        filterState.maxValue = editData.maxValue;
        // 选中的维度信息
        filterState.checkedList = editData.checkedList;

        // 判断选择维度时是否有日期范围选择
        let dateFalg = filterState.filterModel.findIndex(function (value) {
          return value === 'ByDateRang';
        });
        if (dateFalg) {
          // 获取日期
          filterState.dateValue = [filterState.minValue, filterState.maxValue];
        }
        return;
      }

      // 筛选方式
      filterState.filterModel = res.data.filterModel;
      // 最小值最大值
      filterState.minValue = res.data.data[0];
      const dataLength = res.data.data.length;
      filterState.maxValue = res.data.data[dataLength - 1];
      // 选中的维度信息
      filterState.metas = res.data.metas;

      // 判断选择维度时是否有日期范围选择
      let dateFalg = filterState.filterModel.findIndex(function (value) {
        return value === 'ByDateRang';
      });
      if (dateFalg) {
        // 获取日期
        filterState.dateValue = [filterState.minValue, filterState.maxValue];
      }
    });
  }
</script>
<style lang="less" scoped>
  .shaixuan_box {
    margin-top: 10px;
    padding: 10px;
    border-radius: 5px;
    height: 200px;
    overflow-y: auto;
    border: 1px solid #ccc;
  }

  :deep(.ant-tag-close-icon) {
    color: #1f3fbd;
  }

  :deep(.ant-checkbox-group) {
    display: flex;
    flex-direction: column;
  }
</style>
