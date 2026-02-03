<template>
  <div class="page-content-wrapper">
    <div class="page-content-wrapper-footer">
      <a-button preIcon="icon-ym" @click="cancelFun">取消</a-button>
      <a-button type="primary" preIcon="icon-ym icon-ym-btn-add" @click="createFun"
        style="margin-right: 100px">创建</a-button>
    </div>
    <div class="page-content-wrapper-left" style="height: auto">
      <a-tabs v-model:activeKey="activeKey_left" type="card">

        <a-tab-pane key="2" tab="数据源">
          <BasicLeftTree title="数据源" :treeData="treeData" :loading="treeLoading" @reload="reloadTree"
            @select="handleTreeSelect" :dropDownActions="leftDropDownActions" :fieldNames="{ key: 'id', title: 'name' }"
            :defaultExpandAll="false" style="overflow: auto; max-height: 750px" />
        </a-tab-pane>
      </a-tabs>
    </div>
    <div class="page-content-wrapper-center">
      <div class="page-content-wrapper-content">
        <a-form layout="vertical" :model="formState" :label-col="{ span: 8 }" :wrapper-col="{ span: 24 }">
          <a-form-item label="选中模型">
            <a-input v-model:value="formState.selectedModel" disabled></a-input>
          </a-form-item>

          <a-form-item label="选择模型中表示指标的列">
            <a-select ref="select" v-model:value="formState.targetColumn" show-search :filter-option="filterOption"
              placeholder="请选择" @change="targetColumnChange">
              <template v-for="item in targetColumnArr">
                <a-select-option :value="item.field" :dataType="item.dataType" :allowNull="item.allowNull"
                  :dataLength="item.dataLength" :field="item.field" :fieldName="item.fieldName"
                  :primaryKey="item.primaryKey">
                  <div>
                    <span>{{ item.fieldName }}</span>
                    <span style="float: right; color: #777">{{ item.dataType }}</span>
                  </div>
                </a-select-option>
              </template>
            </a-select>
          </a-form-item>
          <a-row :gutter="16">
            <a-col :span="12"><a-form-item label="聚合方式">
                <a-select ref="select" v-model:value="formState.polymerization" placeholder="请选择">
                  <template v-for="item in polymerizationArr">
                    <a-select-option :value="item.aggType" :disabled="!item.isDisable">{{
                      item.displayName
                    }}</a-select-option>
                  </template>
                </a-select>
              </a-form-item>
            </a-col>
            <a-col :span="12">
              <a-form-item label="格式">
                <a-input v-model:value="formState.format" placeholder="请选择格式" disabled>
                  <template #suffix>
                    <a-tooltip>
                      <EditOutlined @click="FormatEdit" />
                    </a-tooltip>
                  </template>
                </a-input> </a-form-item></a-col>
          </a-row>

          <a-form-item label="维度（模型中表示细分维度的列）">
            <depSelect :dataArr="targetColumnArrWeidu" :checkedArr="targetCheckedArrWeidu"
              @depSelectEmits="depSelectEmitsFun" @depSelectItemEmits="depSelectItemEmitsFun"></depSelect>
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
          <a-form class="modelFormClass" layout="vertical" :model="dateSetting" :label-col="{ span: 8 }"
            :wrapper-col="{ span: 24 }">
            <a-form-item label="时间维度">
              <p style="color: #8b949e">数据中表示如何展示历史数据时间的列</p>
              <a-select ref="select" allowClear placeholder="请选择" v-model:value="dateSetting.date_weidu"
                :options="dateSettingOptions" :fieldNames="{
                  label: 'fieldName',
                  value: 'field',
                }">
              </a-select>
            </a-form-item>
            <a-form-item label="时间粒度">
              <p style="color: #8b949e">细分指标值的时间粒度</p>
              <a-select ref="select" v-if="formState.data_type !== 'RealTime'" v-model:value="dateSetting.date_lidu"
                placeholder="请选择">
                <a-select-option value="Year">年</a-select-option>
                <a-select-option value="Quarter">季</a-select-option>
                <a-select-option value="Month">月</a-select-option>
                <a-select-option value="Week">周</a-select-option>
                <a-select-option value="Day">日</a-select-option>
              </a-select>
              <a-select ref="select" v-else v-model:value="dateSetting.date_lidu" placeholder="请选择">
                <a-select-option value="Hour">时</a-select-option>
                <a-select-option value="Minute">分</a-select-option>
                <a-select-option value="Second">秒</a-select-option>
              </a-select>
            </a-form-item>
            <a-form-item label="指标值的展示方式">
              <div style="color: #8b949e; line-height: 1.5; margin-bottom: 5px">
                最新值：当前时间维度下，最近一个时间周期的聚合数据。<br />
                基于全部数据的值：忽略时间维度，对历史所有筛选数据的汇总计算。
              </div>
              <a-select ref="select" v-model:value="dateSetting.indicator_value" placeholder="请选择">
                <a-select-option value="Latest">最新值</a-select-option>
                <a-select-option value="All">基于全部数据的值</a-select-option>
              </a-select>
            </a-form-item>
          </a-form>
        </div>
        <div>
          <h2 style="font-size: 20px">名称和描述</h2>
          <a-form class="modelFormClass" layout="vertical" :model="nameDescribe" ref="formRefDescribe"
            :rules="rulesDescribe" :label-col="{ span: 8 }" :wrapper-col="{ span: 24 }">
            <a-form-item label="指标名称" name="name">
              <a-input v-model:value="nameDescribe.name" placeholder="请输入"></a-input>
            </a-form-item>
            <a-form-item label="指标编码" name="code">
              <p style="color: #8b949e">指标编码唯一标识，指标创建后无法修改</p>
              <a-input v-model:value="nameDescribe.code" placeholder="请输入"></a-input>
            </a-form-item>

            <a-form-item label="描述">
              <a-textarea v-model:value="nameDescribe.description" placeholder="请输入"
                :auto-size="{ minRows: 3, maxRows: 5 }"></a-textarea>
            </a-form-item>
            <a-form-item label="指标目录" name="metricCategory">
              <jnpf-tree-select v-model:value="nameDescribe.metricCategory" :options="metricCategoryArr" allowClear />
            </a-form-item>
            <a-form-item label="标签" name="metricTag">
              <depSelectTag :dataArr="metricTagArr" :checkedArr="checkedMetricTag"
                @depSelectEmitsTag="depSelectEmitsTagFun" @depSelectItemEmitsTag="depSelectItemEmitsTagFun">
              </depSelectTag>
            </a-form-item>
          </a-form>
        </div>
      </div>
    </div>
    <div class="page-content-wrapper-right">
      <a-tabs v-model:activeKey="activeKey_right" type="card">
        <a-tab-pane key="1" tab="表结构">
          <BasicTable @register="registerTable_biaojiegou" ref="tableRef_biaojiegou" :data-source="tableList">
          </BasicTable>
        </a-tab-pane>
        <a-tab-pane key="2" tab="SQL信息"></a-tab-pane>
        <a-tab-pane key="3" tab="版本变更"></a-tab-pane>
      </a-tabs>
    </div>

    <!-- 左侧树 -->
    <OrgTree @register="registerOrgTree" />
    <!-- 格式 -->
    <FormatSample :visible="state.visible_format" @visible_format="visible_format_change"
      @format_obj="format_obj_change" @format_value_format="format_value_format_change" />
    <!-- 添加筛选 -->
    <a-modal width="500px" v-model:visible="shaixuanVisible" :title="shaixuanTitle" @ok="shaixuanSelectionFun">
      <a-form layout="vertical" :model="formState_shaixuan" :label-col="{ span: 8 }" :wrapper-col="{ span: 24 }"
        style="padding: 20px">
        <a-form-item label="选择维度">
          <a-select ref="select" v-model:value="formState_shaixuan.select_weidu" @change="shaixuanChange"
            :disabled="shaixuanDisable" placeholder="请选择">
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
              <!-- <a-radio-button value="expression">表达式</a-radio-button> -->
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
                <a-col :span="11"><a-checkbox
                    v-model:checked="formState_shaixuan.maxValueChecked">包含</a-checkbox></a-col>
              </a-row>
            </div>

            <div class="shaixuan_box" v-if="formState_shaixuan.shaixuanValue == 'ByValue'">
              <template v-if="formState_shaixuan.plainOptions.length > 0">
                <a-checkbox v-model:checked="formState_shaixuan.checkAll"
                  :indeterminate="formState_shaixuan.indeterminate" @change="onCheckAllChange_shaixuan">
                  全选
                </a-checkbox>

                <a-checkbox-group v-model:value="formState_shaixuan.checkedList"
                  :options="formState_shaixuan.plainOptions" />
              </template>
            </div>

            <div v-if="formState_shaixuan.shaixuanValue == 'ByDateRang'">
              <a-row :gutter="16">
                <a-col :span="24">
                  <a-form-item label="开始和结束时间">
                    <jnpf-date-range v-model:value="formState_shaixuan.dateValue" format="YYYY/MM/DD"
                      valueFormat="YYYY/MM/DD HH:mm:ss" allowClear />
                  </a-form-item>
                </a-col>
              </a-row>
              <a-row :gutter="16">
                <a-col :span="11">
                  <a-checkbox v-model:checked="formState_shaixuan.minValueChecked">包含</a-checkbox>
                </a-col>
                <a-col :span="2"></a-col>
                <a-col :span="11"><a-checkbox
                    v-model:checked="formState_shaixuan.maxValueChecked">包含</a-checkbox></a-col>
              </a-row>
            </div>
            <!-- <div v-if="formState_shaixuan.shaixuanValue == 'expression'">
              <p style="margin: 10px 0 5px; color: #999">表达式使用 SQL 语法。</p>
              <a-textarea
                v-model:value="formState_shaixuan.sqlValue"
                placeholder="请输入"
                :auto-size="{ minRows: 5, maxRows: 6 }" />
            </div> -->
          </div>
        </a-form-item>
      </a-form>
    </a-modal>
  </div>
</template>
<script lang="ts" setup>
import { BasicLeftTree, TreeItem } from '/@/components/Tree';
import { Modal as AModal } from 'ant-design-vue';
import { ref, reactive, watch } from 'vue';
import {
  getMetricSchema,
  getMetricLinkIdSchemaSchemaName,
  postMetricAgg_type,
  postMetricFilter_model_data,
  postMetric,
  getMetric,
  putMetric,
  getMetrictagSelector,
  getMetriRcreal_time,
} from '/@/api/targetDefinition';
import { getMetriccategoryList } from '/@/api/targetDirectory';
import { BasicTable, useTable, BasicColumn } from '/@/components/Table';
import { usePopup } from '/@/components/Popup';
import OrgTree from './OrgTree.vue';
import { EditOutlined } from '@ant-design/icons-vue';
import { FormatSample } from '/@/components/FormatSample';
import { depSelect } from '/@/components/DepSelect';
import { depSelectTag } from '/@/components/DepSelectTag';
import { useMessage } from '/@/hooks/web/useMessage';
import { useRouter, useRoute } from 'vue-router';
import dayjs from 'dayjs';
const { createMessage } = useMessage();

const router = useRouter();
const route = useRoute();

const [registerOrgTree, { openPopup: openOrgTreePopup }] = usePopup();
const leftDropDownActions = [
  {
    label: '架构图',
    onClick: openOrgTreePopup.bind(null, true, {}),
  },
];
const activeKey_left = ref('2');
const activeKey_right = ref('1');
const disabled_dataType = ref(false); //数据类型的禁用
const linkId = ref(''); //482106329557630917
const schemaName = ref(''); //retail_stores
// 数据源信息-提交时用
const dataModelIdInfo = ref({});
// 选择模型中表示指标的列-提交时用
const targetColumnInfo = ref({});
// 是否显示时间设置
const timeSetting = ref(false);
// 聚合方式的数组
const polymerizationArr = ref<any[]>([]);
// ------选择模型中表示指标的列--------
const targetColumnArr = ref<any[]>([]);
// -----------筛选-------------------
const shaixuanDisable = ref(false);
const shaixuanVisible = ref(false);
const weiduTagArr = ref<any[]>([]); // 筛选-维度标签
const shaixuanTable = ref<any[]>([]); // 筛选-table
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

// ---------表结构-------------
const tableList = ref([]);

// ----------维度---------------------
// 维度（模型中表示细分维度的列）
const targetColumnArrWeidu = ref([]);
// 选中的的维度-多选
const targetCheckedArrWeidu = ref([]);
const targetCheckedArrWeiduItem = ref<any[]>([]);
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
  code: `KPI_${dayjs().format('YYYYMMDDHHmmss')}`, //编码

  description: '', //描述
  metricTag: [], //标签
  metricCategory: '', //指标目录
});
// ---------数据源---------------
const treeLoading = ref(false);
const treeData = ref<TreeItem[]>([]);
const emit = defineEmits(['cancel', 'reload']);
const formState: any = reactive({
  selectedModel: '', //选中模型
  data_type: 'Static', //数据类型
  targetColumn: undefined, //指标的列
  polymerization: undefined, //聚合方式
  format: '', //格式
});
// -----------------
function handleTreeSelect(_, node) {
  // 
  // 
  if (node.schemaStorageType === 'RealTime') {
    formState.data_type = 'RealTime';
    disabled_dataType.value = true;
  } else {
    formState.data_type = 'Static';
    disabled_dataType.value = false;
  }
  // 数据源信息
  dataModelIdInfo.value = {
    // children: node.children,
    dbType: node.dbType,
    hasChildren: node.hasChildren,
    host: node.host,
    id: node.id,
    isLeaf: node.isLeaf,
    name: node.name,
    num: node.num,
    parentId: node.parentId,
    schemaStorageType: node.schemaStorageType,
    sortCode: node.sortCode,
    type: node.type,
  };
  linkId.value = node.parentId;
  schemaName.value = node.id;

  const data = {
    linkId: node.parentId,
    schemaName: node.id,
    // linkId: '482106329557630917',
    // schemaName: 'retail_stores',
  };
  getMetricLinkIdSchemaSchemaName(data).then(res => {
    // 
    const { tableFieldList, tableInfo } = res.data;
    // 表结构列表
    tableList.value = tableFieldList;
    // 选中模型
    formState.selectedModel = tableInfo.tableName;
    // 指标的列
    targetColumnArr.value = tableFieldList;
    // 维度
    targetColumnArrWeidu.value = tableFieldList;

    // 如果数据类型为实时数据，则调用这个接口展示列
    if (formState.data_type === 'RealTime') {
      getMetriRcreal_time(tableInfo.table).then(res => {
        // 指标的列
        targetColumnArr.value = res.data;
      });
    }
  });
}
// 切换指标的列
function targetColumnChange(_, item) {
  // 
  // 
  polymerizationArr.value = [];
  formState.polymerization = undefined;
  const obj = {
    allowNull: item.allowNull,
    dataLength: item.dataLength,
    dataType: item.dataType,
    field: item.field,
    fieldName: item.fieldName,
    primaryKey: item.primaryKey,
  };
  // 选择模型中表示指标的列的信息
  targetColumnInfo.value = obj;

  postMetricAgg_type(obj).then(res => {
    // 聚合方式
    // 
    polymerizationArr.value = res.data as any[];
  });
}

function initData() {
  treeLoading.value = true;
  getMetricSchema().then(res => {
    // 
    treeData.value = res.data;
    treeLoading.value = false;
  });
}

// 获取维度的选择值
function depSelectEmitsFun(val) {
  targetCheckedArrWeidu.value = val;
}
function depSelectItemEmitsFun(val) {
  targetCheckedArrWeiduItem.value = val;
}
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
function shaixuanChange(_, item, record) {
  // 初始化数据
  // 
  formState_shaixuan.checkAll = false;
  formState_shaixuan.indeterminate = false;

  const data = {
    linkId: linkId.value,
    schemaName: schemaName.value,
    columnField: {
      field: item.field,
      dataType: item.dataType,
    },
    orderByField: {
      field: item.field,
      dataType: item.dataType,
    },
  };
  postMetricFilter_model_data(data).then(res => {
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
    // 
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
  _ => {
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
    fieldName: formState_shaixuan.metas?.[0]?.fieldName,
    field: formState_shaixuan.metas?.[0]?.field,
    dataType: formState_shaixuan.metas?.[0]?.dataType,
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

// 选择模型中表示指标的列下拉框搜索
const filterOption = (input: string, item: any) => {
  return item.fieldName.toLowerCase().indexOf(input.toLowerCase()) >= 0;
};

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
    } else if (dateSetting.date_lidu === 'Hour') {
      date_name = '时';
    } else if (dateSetting.date_lidu === 'Minute') {
      date_name = '分';
    } else if (dateSetting.date_lidu === 'Second') {
      date_name = '秒';
    }

    // 

    // 筛选
    // filtersArrNew = [
    //   {
    //     whereType: 'and',
    //     dataType: 'int',
    //     field: 'order_year',
    //     fieldName: 'order_year', //选择维度
    //     type: 'ByRange', //ByRange值范围筛选，ByDateRang时间范围筛选，ByValue值筛选
    //     fieldValue: '2015,2016,2017', //值
    //     minValue: '2017', //最小值
    //     maxValue: '2015', //最大值
    //     minValueChecked: false, //最小值true包含false不包含
    //     maxValueChecked: false, //最大值true包含false不包含
    //   },
    // ];

    // 处理fieldValue，改为字符串
    // const filtersNew = shaixuanTable.value.map(item => ({
    //   ...item,
    //   fieldValue: item.fieldValue.toString(),
    // }));

    let data = {
      type: 'Basic', //指标类型,基础指标Basic,派生指标Derive,复合指标Composite
      name: nameDescribe.name, //指标名称
      code: nameDescribe.code, //指标编码
      frequency: nameDescribe.frequency, //频率
      description: nameDescribe.description, //指标描述
      metricCategory: nameDescribe.metricCategory, //指标目录
      metricTag: nameDescribe.metricTag, //指标标签
      dateModelType: 'Db', //数据类别,数据库Db,建模Model
      dataModelId: dataModelIdInfo.value, //数据源信息
      column: targetColumnInfo.value, //选择模型中表示指标的列的信息
      data_type: formState.data_type, //数据类型
      aggType: formState.polymerization, //聚合方式
      format: formatInfo.value, //格式的信息
      dimensions: targetCheckedArrWeiduItem.value, //维度
      dimensionsItem: targetCheckedArrWeidu.value, //维度
      formatValue: formState.format, //格式样例
      filters: shaixuanTable.value, //筛选

      // -------时间设置---------
      timeDimensions: {
        field: dateSetting.date_weidu,
        fieldName: date_name,
        granularity: dateSetting.date_lidu,
        displayOption: dateSetting.indicator_value,
      },
    };

    // 
    if (route.query && route.query.queryId) {
      putMetric(route.query.queryId, data).then(res => {
        if (res.code == 200) {
          createMessage.success('更新成功');
          router.push('/kpi/indicatorDefine');
        }
      });
    } else {
      postMetric(data).then(res => {
        if (res.code == 200) {
          createMessage.success('添加成功');
          router.push('/kpi/indicatorDefine');
        }
      });
    }
  });
}
// 取消
function cancelFun() {
  // 
  router.push('/kpi/indicatorDefine');
}

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
function depSelectItemEmitsTagFun(_val) {
  // 
}
// 指标目录
getMetriccategoryList({}).then(res => {
  metricCategoryArr.value = res.data;
});

// --------------格式----------------
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

// ---------表结构-------------

const columns_biaojiegou: BasicColumn[] = [
  { title: '字段', dataIndex: 'field', width: 100 },
  { title: '说明', dataIndex: 'fieldName', width: 100 },
];
const [registerTable_biaojiegou, { }] = useTable({
  columns: columns_biaojiegou,
  showTableSetting: false,
  pagination: false,
  afterFetch: _ => {
    // 指标中的列
    return targetColumnArr.value;
  },
});

// --------获取详情------------
if (route.query && route.query.queryId) {
  getMetric(route.query.queryId).then(res => {
    // 数据源信息
    handleTreeSelect(null, res.data.dataModelId);
    targetCheckedArrWeiduItem.value = res.data.dimensions; //选中的维度
    //数据类型
    formState.data_type = res.data.data_type;
    if (formState.data_type === 'RealTime') {
      disabled_dataType.value = true;
    } else {
      disabled_dataType.value = false;
    }
    //指标的列
    formState.targetColumn = res.data.column.field;
    // 聚合方式
    targetColumnChange(null, res.data.column);
    formState.polymerization = res.data.aggType;
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
    if (res.data.dimensionsItem) {
      targetCheckedArrWeidu.value = res.data.dimensionsItem;
    }
    // 筛选
    // const filtersNew = res.data.filters).map(item => ({
    //   ...item,
    //   fieldValue: item.fieldValue.split(','),
    // }));
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

reloadTree();
function reloadTree() {
  treeData.value = [];
  initData();
}
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
  padding: 10px;
  margin-bottom: 70px;
}

.page-content-wrapper {
  height: auto;
  min-height: 100%;
}

.page-content-wrapper-right {
  width: 500px;
  margin-left: 10px;
  background: #fff;
  padding: 10px;
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
  background: #eceef5;
  border: 1px solid #d9d9d9;
  padding: 10px;
  border-radius: 5px;
  margin-bottom: 20px;
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
