<template>
  <div class="page-content-wrapper">
    <div class="page-content-wrapper-footer">
      <a-button preIcon="icon-ym" @click="cancelFun">取消</a-button>
      <a-button type="primary" preIcon="icon-ym icon-ym-btn-add" @click="createFun" style="margin-right: 100px"
        >创建</a-button
      >
    </div>
    <div class="page-content-wrapper-center">
      <div class="page-content-wrapper-content">
        <h2 style="font-size: 20px">指标定义</h2>
        <a-form
          layout="vertical"
          class="modelFormClass"
          :model="formState"
          :label-col="{ span: 8 }"
          :wrapper-col="{ span: 24 }">
          <a-row :gutter="16">
            <a-col :span="14">
              <a-form-item label="表达式" :rules="[{ required: true, message: '表达式' }]">
                <a-input v-model:value="state.formulaValue" placeholder="请输入表达式" disabled>
                  <template #suffix>
                    <a-tooltip>
                      <EditOutlined @click="FormulaDataEdit" />
                    </a-tooltip>
                  </template>
                </a-input>
              </a-form-item>
            </a-col>
          </a-row>

          <a-row :gutter="16">
            <a-col :span="14">
              <a-form-item label="格式">
                <a-input v-model:value="formState.format" placeholder="请选择格式" disabled>
                  <template #suffix>
                    <a-tooltip>
                      <EditOutlined @click="FormatEdit" />
                    </a-tooltip>
                  </template>
                </a-input> </a-form-item
            ></a-col>
          </a-row>

          <a-form-item label="维度">
            <p style="color: #8b949e; margin-bottom: 10px">数据中表示细分维度的列</p>
            <p>
              <a-tag v-for="item in dimensions"> {{ item.fieldName }}</a-tag>
            </p>
          </a-form-item>

          <a-form-item label="筛选">
            <a-button style="background: #edf0f8; margin-bottom: 10px" @click="shaixuan_add">添加</a-button>
            <a-table :columns="columns_shaixuan" :data-source="shaixuanTable" :pagination="false">
              <template #bodyCell="{ column, record }">
                <template v-if="column.key === 'fieldValue'">
                  <template v-if="record.type !== 'ByValue'">
                    <span v-if="record.minValueChecked">[</span>
                    <span v-else>(</span>
                    <span>{{ record.minValue }}</span>
                    <span>~</span>
                    <span>{{ record.maxValue }}</span>
                    <span v-if="record.maxValueChecked">]</span>
                    <span v-else>)</span>
                  </template>
                  <template v-else>
                    <a class="spanClass" v-for="item in record.fieldValue"> {{ item }} </a>
                  </template>
                </template>
                <template v-if="column.key === 'action'">
                  <span>
                    <a @click="shaixuan_edit(record)">编辑</a>
                    <a-divider type="vertical" />
                    <a @click="shaixuan_delete(record.field)">删除</a>
                  </span>
                </template>
              </template>
            </a-table>
          </a-form-item>
        </a-form>
        <div style="margin-top: 40px" v-if="timeSetting">
          <h2 style="font-size: 20px">时间设置</h2>
          <a-form
            class="modelFormClass"
            layout="vertical"
            :model="dateSetting"
            :label-col="{ span: 8 }"
            :wrapper-col="{ span: 14 }">
            <a-form-item label="时间维度">
              <p style="color: #8b949e">数据中表示如何展示历史数据时间的列</p>
              <a-select
                ref="select"
                allowClear
                placeholder="请选择"
                v-model:value="dateSetting.date_weidu"
                :options="dateSettingOptions"
                :fieldNames="{
                  label: 'fieldName',
                  value: 'field',
                }">
              </a-select>
            </a-form-item>
            <a-form-item label="时间粒度">
              <p style="color: #8b949e">细分指标值的时间粒度</p>
              <a-select ref="select" v-model:value="dateSetting.date_lidu" placeholder="请选择">
                <a-select-option value="Year">年</a-select-option>
                <a-select-option value="Quarter">季</a-select-option>
                <a-select-option value="Month">月</a-select-option>
                <a-select-option value="Week">周</a-select-option>
                <a-select-option value="Day">日</a-select-option>
              </a-select>
            </a-form-item>
            <a-form-item label="指标值的展示方式">
              <a-select ref="select" v-model:value="dateSetting.indicator_value" placeholder="请选择">
                <a-select-option value="Latest">最新值</a-select-option>
                <a-select-option value="All">基于全部数据的值</a-select-option>
              </a-select>
            </a-form-item>
          </a-form>
        </div>
        <div>
          <h2 style="font-size: 20px">名称和描述</h2>
          <a-form
            class="modelFormClass"
            layout="vertical"
            :model="nameDescribe"
            ref="formRefDescribe"
            :rules="rulesDescribe"
            :label-col="{ span: 8 }"
            :wrapper-col="{ span: 14 }">
            <a-form-item label="指标名称" name="name">
              <a-input v-model:value="nameDescribe.name" placeholder="请输入"></a-input>
            </a-form-item>
            <a-form-item label="指标编码" name="code">
              <p style="color: #8b949e">指标编码唯一标识，指标创建后无法修改</p>
              <a-input v-model:value="nameDescribe.code" placeholder="请输入"></a-input>
            </a-form-item>
            <a-form-item label="频率" name="frequency">
              <a-select ref="select" v-model:value="nameDescribe.frequency" allowClear placeholder="请选择">
                <a-select-option value="Week">周</a-select-option>
                <a-select-option value="Day">日</a-select-option>
                <a-select-option value="Hour">时</a-select-option>
                <a-select-option value="Minute">分</a-select-option>
                <a-select-option value="Second">秒</a-select-option>
              </a-select>
            </a-form-item>
            <a-form-item label="描述">
              <a-textarea
                v-model:value="nameDescribe.description"
                placeholder="请输入"
                :auto-size="{ minRows: 3, maxRows: 5 }"></a-textarea>
            </a-form-item>
            <a-form-item label="指标目录" name="metricCategory">
              <jnpf-tree-select v-model:value="nameDescribe.metricCategory" :options="metricCategoryArr" allowClear />
            </a-form-item>
            <a-form-item label="标签" name="metricTag">
              <depSelectTag
                :dataArr="metricTagArr"
                :checkedArr="checkedMetricTag"
                @depSelectEmitsTag="depSelectEmitsTagFun"
                @depSelectItemEmitsTag="depSelectItemEmitsTagFun"></depSelectTag>
            </a-form-item>
          </a-form>
        </div>
      </div>
    </div>

    <!-- 格式 -->
    <FormatSample
      :visible="state.visible_format"
      @visible_format="visible_format_change"
      @format_obj="format_obj_change"
      @format_value_format="format_value_format_change" />
    <!-- 表达式 -->
    <Expression
      :visible="state.visible_expression"
      :expressionValue="state.formulaValue"
      @visible_expression="visible_expression_change"
      @expression_value="expression_value_change"
      @expression_dimensions="expression_dimensions_change"
      @expression_id="expression_id_change" />

    <a-modal width="500px" v-model:visible="shaixuanVisible" :title="shaixuanTitle" @ok="shaixuanSelectionFun">
      <a-form
        layout="vertical"
        :model="formState_shaixuan"
        :label-col="{ span: 8 }"
        :wrapper-col="{ span: 24 }"
        style="padding: 20px">
        <a-form-item label="选择维度">
          <a-select
            ref="select"
            v-model:value="formState_shaixuan.select_weidu"
            @change="shaixuanChange"
            :disabled="shaixuanDisable"
            placeholder="请选择">
            <template v-for="item in weiduTagArr">
              <a-select-option :value="item.field" :dataType="item.dataType" :field="item.field">{{
                item.fieldName
              }}</a-select-option>
            </template>
          </a-select>
        </a-form-item>
        <a-form-item label="筛选方式" v-if="formState_shaixuan.select_weidu">
          <a-radio-group v-model:value="formState_shaixuan.shaixuanValue">
            <template v-for="modelValue in formState_shaixuan.filterModel">
              <a-radio-button value="ByRange" v-if="modelValue == 'ByRange'">值范围筛选</a-radio-button>
              <a-radio-button value="ByDateRang" v-if="modelValue == 'ByDateRang'">时间范围筛选</a-radio-button>
              <a-radio-button value="ByValue" v-if="modelValue == 'ByValue'">值筛选</a-radio-button>
            </template>
          </a-radio-group>
          <div style="margin-top: 20px">
            <div v-if="formState_shaixuan.shaixuanValue == 'ByRange'">
              <a-row :gutter="16">
                <a-col :span="11">
                  <a-form-item label="最小值">
                    <a-input-number v-model:value="formState_shaixuan.minValue" />
                  </a-form-item>
                </a-col>
                <a-col :span="2" style="display: flex; align-items: center; justify-content: center">-</a-col>
                <a-col :span="11">
                  <a-form-item label="最大值">
                    <a-input-number v-model:value="formState_shaixuan.maxValue" />
                  </a-form-item>
                </a-col>
              </a-row>
              <a-row :gutter="16">
                <a-col :span="11">
                  <a-checkbox v-model:checked="formState_shaixuan.minValueChecked">包含</a-checkbox>
                </a-col>
                <a-col :span="2"></a-col>
                <a-col :span="11"
                  ><a-checkbox v-model:checked="formState_shaixuan.maxValueChecked">包含</a-checkbox></a-col
                >
              </a-row>
            </div>

            <div class="shaixuan_box" v-if="formState_shaixuan.shaixuanValue == 'ByValue'">
              <template v-if="formState_shaixuan.plainOptions.length > 0">
                <a-checkbox
                  v-model:checked="formState_shaixuan.checkAll"
                  :indeterminate="formState_shaixuan.indeterminate"
                  @change="onCheckAllChange_shaixuan">
                  全选
                </a-checkbox>

                <a-checkbox-group
                  v-model:value="formState_shaixuan.checkedList"
                  :options="formState_shaixuan.plainOptions" />
              </template>
            </div>

            <div v-if="formState_shaixuan.shaixuanValue == 'ByDateRang'">
              <a-row :gutter="16">
                <a-col :span="24">
                  <a-form-item label="开始和结束时间">
                    <jnpf-date-range
                      v-model:value="formState_shaixuan.dateValue"
                      format="YYYY/MM/DD"
                      valueFormat="YYYY/MM/DD HH:mm:ss"
                      allowClear />
                  </a-form-item>
                </a-col>
              </a-row>
              <a-row :gutter="16">
                <a-col :span="11">
                  <a-checkbox v-model:checked="formState_shaixuan.minValueChecked">包含</a-checkbox>
                </a-col>
                <a-col :span="2"></a-col>
                <a-col :span="11"
                  ><a-checkbox v-model:checked="formState_shaixuan.maxValueChecked">包含</a-checkbox></a-col
                >
              </a-row>
            </div>
          </div>
        </a-form-item>
      </a-form>
    </a-modal>
  </div>
</template>
<script lang="ts" setup>
  import { Modal as AModal } from 'ant-design-vue';
  import { ref, reactive, watch } from 'vue';
  import {
    getMetricAll,
    postMetricComposite,
    putMetricComposite,
    getMetrictagSelector,
    postMetricDims,
    postFilterMetricData,
    getMetricComposite,
  } from '/@/api/targetDefinition';
  import { getMetriccategoryList } from '/@/api/targetDirectory';
  import { BasicTable, useTable, TableAction, BasicColumn, ActionItem } from '/@/components/Table';
  import { EditOutlined, SettingOutlined } from '@ant-design/icons-vue';
  import { FormatSample } from '/@/components/FormatSample';
  import { Expression } from '/@/components/Expression';
  import { depSelectTag } from '/@/components/DepSelectTag';
  import { useMessage } from '/@/hooks/web/useMessage';
  import { useRouter, useRoute } from 'vue-router';
  import dayjs from 'dayjs';
  const { createMessage, createConfirm } = useMessage();

  const router = useRouter();
  const route = useRoute();

  // 指标id
  const parentIds = ref([]);
  // 是否显示时间设置
  const timeSetting = ref(false);
  // -----------筛选-------------------
  const shaixuanDisable = ref(false);
  const shaixuanVisible = ref(false);
  const weiduTagArr = ref<any[]>([]); // 筛选-维度标签
  const shaixuanTitle = ref('添加维度');
  const formState_shaixuan: any = reactive({
    select_weidu: undefined, //筛选选择维度
    shaixuanValue: 'ByValue', //筛选方式
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
  // -------------时间设置---------------
  const dateSettingOptions = ref<any[]>([]); //时间维度数组
  const dateSetting: any = reactive({
    date_weidu: undefined, //时间维度
    date_lidu: undefined, //时间粒度
    indicator_value: undefined, //指标值的展示方式
  });
  //--------------名称和描述-------------
  const formRefDescribe = ref();
  const rulesDescribe = {
    name: [{ required: true, message: '请输入名称' }],
    code: [{ required: true, message: '请输入编码' }],
  };
  const metricTagArr = ref([]); //标签的数组
  const metricCategoryArr = ref([]); //指标目录的数组
  const checkedMetricTag = ref([]); //标签回显id
  const nameDescribe: any = reactive({
    name: '', //名称
    code: '', //编码
    frequency: undefined, //频率
    description: '', //描述
    metricTag: [], //标签
    metricCategory: '', //指标目录
  });

  const emit = defineEmits(['cancel', 'reload']);

  const dimensions = ref<any[]>([]); //维度
  const targetCheckedArrWeiduItem = ref<any[]>([]);
  const shaixuanTable = ref<any[]>([]); // 筛选-table

  // 公式检查
  const state = reactive({
    visible_format: false, //格式
    visible_expression: false, //表达式

    formulaValue: '', //表达式的值 例子：${1#-甲班组生产数量}+${1#-甲班组合格数量}
    formulaValueId: '', //表达式的值 例子：${509273244998565829}+${509955652131487685}
    formularOptions: [
      // {
      //   id: '502327017564274629',
      //   name: '001',
      // },
    ],
  });
  const formState = reactive({
    selectedModel: '', //选中模型
    targetColumn: undefined, //指标的列
    polymerization: undefined, //聚合方式
    format: '', //格式
  });

  // 筛选
  const columns_shaixuan: BasicColumn[] = [
    { title: '维度名', dataIndex: 'fieldName', width: 100 },
    { title: '筛选值', key: 'fieldValue', dataIndex: 'fieldValue', width: 300 },
    { title: '操作', key: 'action', width: 150 },
  ];
  // 筛选-添加
  function shaixuan_add() {
    shaixuanTitle.value = '添加维度';
    shaixuanVisible.value = true;
    shaixuanDisable.value = false;
    // 选择维度
    weiduTagArr.value = targetCheckedArrWeiduItem.value;
    // 初始化数据
    formState_shaixuan.checkedList = [];
    formState_shaixuan.plainOptions = [];
    formState_shaixuan.select_weidu = undefined;
    formState_shaixuan.checkAll = false;
    formState_shaixuan.indeterminate = false;
  }
  // 筛选-编辑
  function shaixuan_edit(record) {
    shaixuanTitle.value = '编辑维度';
    shaixuanVisible.value = true;
    shaixuanDisable.value = true;
    formState_shaixuan.checkedList = [];
    formState_shaixuan.plainOptions = [];
    formState_shaixuan.shaixuanValue = 'ByValue';

    // 选择维度
    weiduTagArr.value = targetCheckedArrWeiduItem.value;

    // 筛选方式
    shaixuanChange(null, { field: record.field, dataType: record.dataType }, record);
  }
  // 筛选-删除
  function shaixuan_delete(field) {
    // 判断对象数组中是否存在该对象
    let index = shaixuanTable.value.findIndex(item => item.field === field);
    // 如果有就替换，没有就添加
    if (index !== -1) {
      shaixuanTable.value.splice(index, 1);
    }
  }
  // 筛选选择维度的change事件
  function shaixuanChange(value, item, record) {
    // 初始化数据
    // 
    formState_shaixuan.checkAll = false;
    formState_shaixuan.indeterminate = false;

    const data = {
      metrics: parentIds.value,
      columnField: {
        field: item.field,
        dataType: item.dataType,
      },
      orderByField: {
        field: item.field,
        dataType: item.dataType,
      },
    };
    postFilterMetricData(data).then(res => {
      // 筛选方式
      formState_shaixuan.filterModel = res.data.filterModel;
      // 最小值最大值
      formState_shaixuan.minValue = res.data.data[0];
      const dataLength = res.data.data.length;
      formState_shaixuan.maxValue = res.data.data[dataLength - 1];
      // 选中的维度信息
      formState_shaixuan.metas = res.data.metas;

      // 判断选择维度时是否有日期范围选择
      let dateFalg = formState_shaixuan.filterModel.some(function (value) {
        return value === 'ByDateRang';
      });
      if (dateFalg) {
        // 获取日期
        formState_shaixuan.dateValue = [formState_shaixuan.minValue, formState_shaixuan.maxValue];
        formState_shaixuan.plainOptions = [];
        formState_shaixuan.filterModel = ['ByDateRang'];
        formState_shaixuan.shaixuanValue = 'ByDateRang';
      } else {
        formState_shaixuan.shaixuanValue = 'ByValue';
        // 筛选选择维度
        formState_shaixuan.plainOptions = res.data.data;
      }

      // 如果record有值，就执行赋值（编辑详情）
      if (record) {
        formState_shaixuan.select_weidu = record.field;
        formState_shaixuan.checkedList = record.fieldValue;
        formState_shaixuan.shaixuanValue = record.type;
        formState_shaixuan.minValue = record.minValue;
        formState_shaixuan.maxValue = record.maxValue;
        formState_shaixuan.minValueChecked = record.minValueChecked;
        formState_shaixuan.maxValueChecked = record.maxValueChecked;
      }
    });
  }
  // 全选
  function onCheckAllChange_shaixuan(e: any) {
    Object.assign(formState_shaixuan, {
      checkedList: e.target.checked ? formState_shaixuan.plainOptions : [],
      indeterminate: false,
    });
  }
  // 监听全选
  watch(
    () => formState_shaixuan.checkedList,
    val => {
      if (formState_shaixuan.checkedList.length > 0) {
        formState_shaixuan.indeterminate = !!val.length && val.length < formState_shaixuan.plainOptions.length;
        formState_shaixuan.checkAll = val.length === formState_shaixuan.plainOptions.length;
      } else if (formState_shaixuan.checkedList.length == 0) {
        formState_shaixuan.indeterminate = false;
        formState_shaixuan.checkAll = false;
      }
    },
  );
  // 监听订单日期判断是否显示时间设置
  watch(
    () => targetCheckedArrWeiduItem.value,
    val => {
      let index = targetCheckedArrWeiduItem.value.findIndex(
        item => item.dataType === 'date' || item.dataType === 'datetime',
      );
      // 如果有就显示时间设置，没有就隐藏时间设置
      if (index !== -1) {
        // 有
        timeSetting.value = true;

        // 时间设置
        dateSettingOptions.value = [];
        targetCheckedArrWeiduItem.value.filter(item => {
          if (item.dataType === 'date' || item.dataType === 'datetime') {
            dateSettingOptions.value.push(item);
          }
        });
        // 
      } else {
        timeSetting.value = false;
      }
    },
  );

  // 添加筛选提交
  function shaixuanSelectionFun() {
    shaixuanVisible.value = false;
    // ByRange值范围筛选，ByDateRang时间范围筛选，ByValue值筛选
    if (formState_shaixuan.shaixuanValue !== 'ByValue') {
      formState_shaixuan.checkedList = [];
    }
    // 处理日期
    if (formState_shaixuan.shaixuanValue === 'ByDateRang') {
      formState_shaixuan.minValue = dayjs(formState_shaixuan.dateValue[0]).format('YYYY/MM/DD');
      formState_shaixuan.maxValue = dayjs(formState_shaixuan.dateValue[1]).format('YYYY/MM/DD');
    }

    let obj = {
      whereType: 'and',
      type: formState_shaixuan.shaixuanValue,
      fieldValue: formState_shaixuan.checkedList,
      fieldName: formState_shaixuan.metas[0].fieldName,
      field: formState_shaixuan.metas[0].field,
      dataType: formState_shaixuan.metas[0].dataType,
      maxValue: formState_shaixuan.maxValue,
      minValue: formState_shaixuan.minValue,
      maxValueChecked: formState_shaixuan.maxValueChecked,
      minValueChecked: formState_shaixuan.minValueChecked,
    };

    // 筛选的table数据
    // 判断对象数组中是否存在该对象
    let index = shaixuanTable.value.findIndex(item => item.field === obj.field);
    // 如果有就替换，没有就添加
    if (index !== -1) {
      shaixuanTable.value.splice(index, 1, obj);
    } else {
      shaixuanTable.value.push(obj);
    }
    // 
  }

  // ----------------

  // 创建提交
  async function createFun() {
    // 

    formRefDescribe.value.validate().then(async () => {
      let date_name;
      if (dateSetting.date_lidu === 'Day') {
        date_name = '日';
      } else if (dateSetting.date_lidu === 'Week') {
        date_name = '周';
      } else if (dateSetting.date_lidu === 'Month') {
        date_name = '月';
      } else if (dateSetting.date_lidu === 'Quarter') {
        date_name = '季';
      } else if (dateSetting.date_lidu === 'Year') {
        date_name = '年';
      }

      let data = {
        parentIds: parentIds.value,
        dataModelId: '', //数据源信息
        expression: state.formulaValueId, //表达式
        column: state.formulaValue, //表达式
        format: formatInfo.value, //格式的信息
        formatValue: formState.format, //格式样例
        dimensions: targetCheckedArrWeiduItem.value, //维度
        filters: shaixuanTable.value, //筛选

        // -------时间设置---------
        timeDimensions: {
          field: dateSetting.date_weidu,
          fieldName: date_name,
          granularity: dateSetting.date_lidu,
          displayOption: dateSetting.indicator_value,
        },
        // -------名称和描述---------
        type: 'Composite', //指标类型,基础指标Basic,派生指标Derive,复合指标Composite
        name: nameDescribe.name, //指标名称
        code: nameDescribe.code, //指标编码
        frequency: nameDescribe.frequency, //频率
        description: nameDescribe.description, //指标描述
        metricCategory: nameDescribe.metricCategory, //指标目录
        metricTag: nameDescribe.metricTag, //指标标签
      };

      // 
      if (route.query && route.query.queryId) {
        putMetricComposite(route.query.queryId, data).then(res => {
          if (res.code == 200) {
            createMessage.success('更新成功');
            router.push('/kpi/indicatorDefine');
          }
        });
      } else {
        postMetricComposite(data).then(res => {
          if (res.code == 200) {
            createMessage.success('添加成功');
            router.push('/kpi/indicatorDefine');
          }
        });
      }
    });
  }

  // --------获取详情------------
  if (route.query && route.query.queryId) {
    getMetricComposite(route.query.queryId).then(res => {
      // 数据源信息
      targetCheckedArrWeiduItem.value = res.data.dimensions; //选中的维度
      // 选择维度
      weiduTagArr.value = targetCheckedArrWeiduItem.value;
      dimensions.value = targetCheckedArrWeiduItem.value;

      // 表达式
      state.formulaValueId = res.data.expression;
      state.formulaValue = res.data.column;
      // 筛选获取值
      parentIds.value = res.data.parentIds;
      // 格式
      formState.format = res.data.formatValue;
      const obj = res.data.format;
      formatInfo.value = {
        type: obj.Type,
        decimalPlaces: obj.DecimalPlaces,
        unit: obj.Unit,
        isThousandsSeparator: obj.IsThousandsSeparator,
        currencySymbol: obj.CurrencySymbol,
      };
      // 维度
      shaixuanTable.value = res.data.filters;

      // 时间设置
      dateSetting.date_weidu = res.data.timeDimensions.field;
      dateSetting.date_lidu = res.data.timeDimensions.granularity;
      dateSetting.indicator_value = res.data.timeDimensions.displayOption;
      // 名称和描述
      nameDescribe.name = res.data.name;
      nameDescribe.code = res.data.code;
      nameDescribe.frequency = res.data.frequency;
      nameDescribe.description = res.data.description;
      nameDescribe.metricCategory = res.data.metricCategory;
      if (res.data.metricTag) {
        nameDescribe.metricTag = res.data.metricTag.split(',');
        checkedMetricTag.value = res.data.metricTag.split(',');
      }
    });
  }

  // 取消
  function cancelFun() {
    // 
    router.push('/kpi/indicatorDefine');
  }

  // 获取所有的指标信息
  getMetricAll().then(res => {
    state.formularOptions = res.data;
  });

  //标签
  getMetrictagSelector().then(res => {
    metricTagArr.value = res.data;
    // 
  });
  // 标签回显
  function depSelectEmitsTagFun(val) {
    // 
    nameDescribe.metricTag = val;
  }
  function depSelectItemEmitsTagFun(val) {
    // 
  }
  // 指标目录
  getMetriccategoryList({}).then(res => {
    metricCategoryArr.value = res.data;
  });

  // -------------表达式-----------------
  function FormulaDataEdit() {
    state.visible_expression = true;
  }
  function visible_expression_change() {
    state.visible_expression = false;
  }
  function expression_value_change(value) {
    state.formulaValue = value;
  }
  function expression_id_change(value) {
    state.formulaValueId = value;
  }
  function expression_dimensions_change(uniqueArray) {
    parentIds.value = uniqueArray;

    postMetricDims({ metricIds: uniqueArray }).then(res => {
      if (res.data) {
        createMessage.success('确认成功');
        dimensions.value = res.data.dimensions;

        targetCheckedArrWeiduItem.value = dimensions.value; //维度
        shaixuanTable.value = []; //筛选表格清空
      }
    });
  }

  // -------------格式-----------------
  const formatInfo = ref({}); //格式的信息

  // 格式编辑
  function FormatEdit() {
    state.visible_format = true;
  }
  // 格式-回传的弹层
  function visible_format_change() {
    state.visible_format = false;
  }
  // 格式-回传格式参数
  function format_obj_change(obj) {
    // 
    formatInfo.value = {
      type: obj.type,
      decimalPlaces: obj.decimal_place,
      unit: obj.unit,
      isThousandsSeparator: obj.use_thousand_separator,
      currencySymbol: obj.currency_symbol,
    };
  }
  // 格式-回传的样例
  function format_value_format_change(value) {
    // 
    formState.format = value;
  }
  // --------------
</script>
<style lang="less" scoped>
  :deep(.scrollbar__view) {
    padding: 0 !important;
  }
  .page-content-wrapper-footer {
    height: 60px;
    width: 100%;
    background: #fff;
    border-top: 1px solid #f5f5f5;
    z-index: 9;
    position: absolute;
    bottom: 0;
    text-align: right;

    .ant-btn {
      margin-right: 20px;
      margin-top: 10px;
    }
  }

  .page-content-wrapper-center {
    background: #fff;
    padding: 15px 20px;
    width: 60%;
    margin: 0 auto 70px;
    flex: none;
  }

  .page-content-wrapper {
    height: auto;
    min-height: 100%;
  }
  .weidu_box {
    width: 100%;
    height: 100px;
    padding: 10px;
    border: 1px solid #ccc;
    border-radius: 5px;
  }
  :deep(.ant-tag-close-icon) {
    color: #1f3fbd;
  }

  :deep(.ant-checkbox-group) {
    display: flex;
    flex-direction: column;
  }
  .shaixuan_box {
    margin-top: 10px;
    padding: 10px;
    border-radius: 5px;
    height: 200px;
    overflow-y: auto;
    border: 1px solid #ccc;
  }

  .modelFormClass {
    padding: 10px;
  }
  .spanClass {
    color: #000;
    cursor: auto;
    display: inline-block;
  }

  .spanClass::after {
    content: ',';
  }

  .spanClass:last-child::after {
    content: '';
  }
</style>
