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
          class="modelFormClass"
          layout="vertical"
          :model="formState"
          :label-col="{ span: 8 }"
          :wrapper-col="{ span: 14 }">
          <a-form-item label="派生自" :rules="[{ required: true, message: '请选择派生自' }]">
            <a-select
              ref="select"
              v-model:value="formState.derivedFromValue"
              show-search
              :filter-option="filterOption"
              :options="derivedFromArr"
              :fieldNames="{
                label: 'name',
                value: 'id',
              }"
              placeholder="请选择"
              @change="derivedFromChange">
            </a-select>
          </a-form-item>
          <a-form-item label="派生类型" :rules="[{ required: true, message: '请选择派生类型' }]">
            <a-select
              ref="select"
              v-model:value="formState.derivedTypeValue"
              show-search
              :options="derivedTypeArr"
              :fieldNames="{
                label: 'fullName',
                value: 'enCode',
              }"
              placeholder="请选择">
            </a-select>
          </a-form-item>
          <template v-if="formState.derivedTypeValue === 'PTD'">
            <a-form-item label="计算区间">
              <a-select ref="select" v-model:value="formState.calculationIntervalValue" placeholder="请选择">
                <a-select-option value="Year">年</a-select-option>
                <a-select-option value="Quarter">季</a-select-option>
                <a-select-option value="Month">月</a-select-option>
                <a-select-option value="Week">周</a-select-option>
              </a-select>
            </a-form-item>
            <a-form-item label="聚合方式（值的计算方式）">
              <a-select ref="select" v-model:value="formState.polymerization" placeholder="请选择">
                <a-select-option value="SUM">求和</a-select-option>
                <a-select-option value="MAX">最大值</a-select-option>
                <a-select-option value="MIN">最小值</a-select-option>
                <a-select-option value="AVG">平均值</a-select-option>
              </a-select>
            </a-form-item>
          </template>
          <template v-if="formState.derivedTypeValue === 'POP'">
            <a-form-item label="计算区间">
              <a-select
                ref="select"
                v-model:value="formState.calculationIntervalValue"
                @change="calculationIntervalChange"
                placeholder="请选择">
                <a-select-option value="Year">年</a-select-option>
                <a-select-option value="Quarter">季</a-select-option>
                <a-select-option value="Month">月</a-select-option>
                <a-select-option value="Week">周</a-select-option>
                <a-select-option value="Day">日</a-select-option>
              </a-select>
            </a-form-item>
            <a-form-item label="时间粒度">
              <a-select ref="select" v-model:value="formState.timeGranularity" show-search placeholder="请选择">
                <template v-if="formState.calculationIntervalValue === 'Year'">
                  <a-select-option value="Year">年</a-select-option>
                  <a-select-option value="Quarter">季</a-select-option>
                  <a-select-option value="Month">月</a-select-option>
                </template>
                <template v-if="formState.calculationIntervalValue === 'Quarter'">
                  <a-select-option value="Quarter">季</a-select-option>
                  <a-select-option value="Month">月</a-select-option>
                </template>
                <template v-if="formState.calculationIntervalValue === 'Month'">
                  <a-select-option value="Month">月</a-select-option>
                </template>
                <template v-if="formState.calculationIntervalValue === 'Week'">
                  <a-select-option value="Week">周</a-select-option>
                  <a-select-option value="Day">日</a-select-option>
                </template>
                <template v-if="formState.calculationIntervalValue === 'Day'">
                  <a-select-option value="Day">日</a-select-option>
                </template>
              </a-select>
            </a-form-item>
          </template>
          <template v-if="formState.derivedTypeValue === 'Cumulative'">
            <a-form-item label="聚合方式（值的计算方式）">
              <a-select ref="select" v-model:value="formState.polymerization" placeholder="请选择">
                <a-select-option value="SUM">求和</a-select-option>
                <a-select-option value="MAX">最大值</a-select-option>
                <a-select-option value="MIN">最小值</a-select-option>
                <a-select-option value="AVG">平均值</a-select-option>
              </a-select>
            </a-form-item>
          </template>
          <template v-if="formState.derivedTypeValue === 'Moving'">
            <a-form-item label="聚合方式（值的计算方式）">
              <a-select ref="select" v-model:value="formState.polymerization" placeholder="请选择">
                <a-select-option value="SUM">求和</a-select-option>
                <a-select-option value="MAX">最大值</a-select-option>
                <a-select-option value="MIN">最小值</a-select-option>
                <a-select-option value="AVG">平均值</a-select-option>
              </a-select>
            </a-form-item>
            <a-form-item label="计算区间开始于">
              <a-radio-group v-model:value="formState.firstLine" name="radioGroup">
                <a-radio value="first">第一行</a-radio>
                <a-radio value="last"
                  >相对于当前行的第
                  <a-input-number style="width: 80px !important" v-model:value="formState.startLine" />
                  行</a-radio
                >
              </a-radio-group>
            </a-form-item>
            <a-form-item label="计算区间结束于">
              <a-radio-group v-model:value="formState.lastLine" name="radioGroup">
                <a-radio value="first">最后一行</a-radio>
                <a-radio value="last"
                  >相对于当前行的第<a-input-number style="width: 80px !important" v-model:value="formState.endLine" />
                  行</a-radio
                >
              </a-radio-group>
            </a-form-item>
          </template>
          <template
            v-if="formState.derivedTypeValue === 'Difference' || formState.derivedTypeValue === 'DifferenceRatio'">
            <a-form-item label="相对于">
              <a-radio-group v-model:value="formState.isRange" name="radioGroup">
                <a-radio value="first">第一行</a-radio>
                <a-radio value="last">最后一行</a-radio>
                <a-radio value="range"
                  >相对于当前行的第
                  <a-input-number style="width: 80px !important" v-model:value="formState.rangeLine" />
                  行</a-radio
                >
              </a-radio-group>
            </a-form-item>
          </template>
          <template v-if="formState.derivedTypeValue === 'Ranking'">
            <a-form-item label="排名类型">
              <a-select
                ref="select"
                v-model:value="formState.rankingType"
                :options="rankingTypeArr"
                :fieldNames="{
                  label: 'fullName',
                  value: 'enCode',
                }"
                placeholder="请选择">
              </a-select>
            </a-form-item>
            <a-form-item label="排序">
              <a-select ref="select" v-model:value="formState.sortType" placeholder="请选择">
                <a-select-option value="ASC">升序</a-select-option>
                <a-select-option value="DESC">降序</a-select-option>
              </a-select>
            </a-form-item>
          </template>
          <template v-else></template>

          <a-form-item label="格式">
            <a-input v-model:value="formState.format" placeholder="请选择格式" disabled>
              <template #suffix>
                <a-tooltip>
                  <EditOutlined @click="FormatEdit" />
                </a-tooltip>
              </template>
            </a-input>
          </a-form-item>
          <a-form-item label="维度">
            <p style="color: #8b949e; margin-bottom: 10px">数据中表示细分维度的列</p>
            <p>
              <a-tag v-for="item in dimensions"> {{ item.fieldName }}</a-tag>
            </p>
          </a-form-item>
        </a-form>

        <div>
          <h2 style="font-size: 20px">时间设置</h2>
          <a-form class="modelFormClass" layout="vertical" :label-col="{ span: 8 }" :wrapper-col="{ span: 14 }">
            <a-form-item label="时间维度">
              <p style="color: #8b949e"> {{ dimensionsTime }} </p>
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
  </div>
</template>
<script lang="ts" setup>
  import { useBaseStore } from '/@/store/modules/base';
  import { ref, reactive, watch } from 'vue';
  import { EditOutlined, SettingOutlined } from '@ant-design/icons-vue';
  import { FormatSample } from '/@/components/FormatSample';
  import { useMessage } from '/@/hooks/web/useMessage';
  import {
    getMetricAllDerive,
    getMetric,
    postMetricDerive,
    putMetricDerive,
    getMetricDerive,
    getMetrictagSelector,
  } from '/@/api/targetDefinition';
  import { getMetriccategoryList } from '/@/api/targetDirectory';
  import { depSelectTag } from '/@/components/DepSelectTag';
  import { useRouter, useRoute } from 'vue-router';
  const { createMessage, createConfirm } = useMessage();
  const router = useRouter();
  const route = useRoute();
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
  // -----------------------
  const emit = defineEmits(['cancel', 'reload']);
  const baseStore = useBaseStore();

  const dateModelType = ref(''); //数据类别,数据库Db,建模Model
  // 数据源信息-提交时用
  const dataModelIdInfo = ref({});
  // 选择模型中表示指标的列-提交时用
  const targetColumnInfo = ref({});
  // 时间设置
  const dateSetting = ref({});
  // 选中的的维度-多选
  const targetCheckedArrWeidu = ref([]);
  const targetCheckedArrWeiduItem = ref<any[]>([]);
  const shaixuanTable = ref<any[]>([]); // 筛选-table

  const derivedFromArr = ref<any[]>([]); //派生自
  const derivedTypeArr = ref<any[]>([]); //派生类型
  const rankingTypeArr = ref<any[]>([]); //排名类型
  const dimensions = ref<any[]>([]); //维度
  const dimensionsTime = ref(''); //时间维度
  const formState = reactive({
    derivedFromValue: undefined, //派生自
    derivedTypeValue: undefined, //派生指标类别
    calculationIntervalValue: undefined, //计算区间
    timeGranularity: undefined, //时间粒度
    polymerization: 'SUM',
    rankingType: undefined,
    sortType: undefined, //排序
    format: '', //格式
    // ----------------
    firstLine: 'last', //第一行first,last
    lastLine: 'last', //最后一行first,last
    isRange: 'range', //区间first,last,range
    startLine: -2, //开始值
    endLine: 0, //结束值
    rangeLine: -1, //区间值
  });

  // 获取下拉选择框
  getOptions();
  async function getOptions() {
    const derivedType = (await baseStore.getDictionaryData('derivedType')) as any;
    const rankingType = (await baseStore.getDictionaryData('rankingType')) as any;
    // 派生类型
    derivedTypeArr.value = derivedType as any[];
    // 排名类型
    rankingTypeArr.value = rankingType as any[];
  }

  // ------------------------------
  const formatInfo = ref({}); //格式的信息
  const state = reactive({
    visible_format: false,
  });
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
  // ----------------------

  // 派生自下拉框搜索
  const filterOption = (input: string, item: any) => {
    return item.name.toLowerCase().indexOf(input.toLowerCase()) >= 0;
  };
  // 派生自
  getMetricAllDerive().then(res => {
    derivedFromArr.value = res.data;
  });

  //标签
  getMetrictagSelector().then(res => {
    metricTagArr.value = res.data;
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

  // 切换派生自
  function derivedFromChange(value) {
    // 获取详情
    getMetric(value).then(res => {
      // 样例格式和名称描述(所有字段)需要重新填写
      //维度
      dimensions.value = res.data.dimensions;
      // 时间维度
      const obj = res.data.dimensions.filter(item => {
        return item.field === res.data.timeDimensions.field;
      });
      dimensionsTime.value = obj[0].fieldName;

      dateModelType.value = res.data.dateModelType; //数据类别,数据库Db,建模Model
      dataModelIdInfo.value = res.data.dataModelId; //数据源信息
      targetColumnInfo.value = res.data.column; //选择模型中表示指标的列的信息
      targetCheckedArrWeidu.value = res.data.dimensionsItem; //维度
      targetCheckedArrWeiduItem.value = res.data.dimensions; //维度
      shaixuanTable.value = res.data.filters; //筛选
      dateSetting.value = res.data.timeDimensions; // 时间设置
    });
  }
  // 切换计算区间
  function calculationIntervalChange() {
    // 时间粒度为空
    formState.timeGranularity = undefined;
  }

  // 取消
  function cancelFun() {
    router.push('/kpi/indicatorDefine');
  }

  // 提交
  async function createFun() {
    // 
    // 
    // 
    formRefDescribe.value.validate().then(async () => {
      let data = {
        type: 'Derive', //指标类型,基础指标Basic,派生指标Derive,复合指标Composite
        name: nameDescribe.name, //指标名称
        code: nameDescribe.code, //指标编码
        frequency: nameDescribe.frequency, //频率
        description: nameDescribe.description, //指标描述
        metricCategory: nameDescribe.metricCategory, //指标目录
        metricTag: nameDescribe.metricTag, //指标标签
        parentId: formState.derivedFromValue, //派生自
        deriveType: formState.derivedTypeValue, //派生指标类别
        aggType: formState.polymerization, //聚合类别
        rankingType: formState.rankingType, //排名类型
        sortType: formState.sortType, //排序方式
        format: formatInfo.value, //格式的信息
        formatValue: formState.format, //格式样例
        caGranularity: formState.calculationIntervalValue, //计算区间
        dateGranularity: formState.timeGranularity, //时间粒度
        granularityStr: {
          //计算区间开始于结束于、相当于
          firstLine: formState.firstLine,
          lastLine: formState.lastLine,
          isRange: formState.isRange,
          startLine: formState.startLine,
          endLine: formState.endLine,
          rangeLine: formState.rangeLine,
        },

        dateModelType: dateModelType.value, //数据类别,数据库Db,建模Model
        dataModelId: dataModelIdInfo.value, //数据源信息
        column: targetColumnInfo.value, //选择模型中表示指标的列的信息
        dimensions: targetCheckedArrWeiduItem.value, //维度
        dimensionsItem: targetCheckedArrWeidu.value, //维度
        filters: shaixuanTable.value, //筛选
        timeDimensions: dateSetting.value, //时间设置
      };
      // 
      if (route.query && route.query.queryId) {
        putMetricDerive(route.query.queryId, data).then(res => {
          if (res.code == 200) {
            createMessage.success('更新成功');
            router.push('/kpi/indicatorDefine');
          }
        });
      } else {
        postMetricDerive(data).then(res => {
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
    getMetricDerive(route.query.queryId).then(res => {
      // 
      formState.derivedFromValue = res.data.parentId; //派生自
      formState.derivedTypeValue = res.data.deriveType; //派生类型
      formState.polymerization = res.data.aggType; //聚合方式
      formState.calculationIntervalValue = res.data.caGranularity; //计算区间
      formState.timeGranularity = res.data.dateGranularity; //时间粒度
      formState.rankingType = res.data.rankingType; //排名类型
      formState.sortType = res.data.sortType; //排序方式
      //计算区间开始于结束于、相当于
      formState.firstLine = res.data.granularityStr.firstLine;
      formState.lastLine = res.data.granularityStr.lastLine;
      formState.isRange = res.data.granularityStr.isRange;
      formState.startLine = res.data.granularityStr.startLine;
      formState.endLine = res.data.granularityStr.endLine;
      formState.rangeLine = res.data.granularityStr.rangeLine;
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
      dimensions.value = res.data.dimensions;
      // 时间维度
      const obj2 = res.data.dimensions.filter(item => {
        return item.field === res.data.timeDimensions.field;
      });
      dimensionsTime.value = obj2[0].fieldName;

      dateModelType.value = res.data.dateModelType; //数据类别,数据库Db,建模Model
      dataModelIdInfo.value = res.data.dataModelId; //数据源信息
      targetColumnInfo.value = res.data.column; //选择模型中表示指标的列的信息
      targetCheckedArrWeiduItem.value = res.data.dimensions; //维度
      targetCheckedArrWeidu.value = res.data.dimensionsItem; //维度
      shaixuanTable.value = res.data.filters; //筛选
      dateSetting.value = res.data.timeDimensions; // 时间设置
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
</script>
<style lang="less" scoped>
  :deep(.scrollbar__view) {
    padding: 0px !important;
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

  .modelFormClass {
    padding: 10px;
  }
</style>
