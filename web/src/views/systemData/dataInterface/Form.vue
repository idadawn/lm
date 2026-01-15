<template>
  <BasicPopup
    v-bind="$attrs"
    @register="registerPopup"
    :title="getTitle"
    showOkBtn
    @ok="handleSubmit"
    class="full-popup jnpf-dataInterface-modal"
    :continueLoading="activeStep < getStepList.length - 1">
    <template #title>
      <div class="ml-10px steps">
        <a-steps v-model:current="activeStep" type="navigation" size="small" :key="key">
          <a-step v-for="item in getStepList" :key="item" :title="item" disabled />
        </a-steps>
      </div>
    </template>
    <template #insertToolbar>
      <a-space :size="10">
        <a-button @click="handlePrev" :disabled="activeStep <= 0">{{ t('common.prev') }}</a-button>
        <a-button @click="handleNext" :disabled="activeStep >= getStepList.length - 1">{{ t('common.next') }} </a-button>
      </a-space>
    </template>
    <!-- 基本信息 -->
    <a-row class="mt-20px overflow-auto h-full" v-show="activeStep === 0">
      <a-col :span="14" :offset="5">
        <BasicForm @register="registerForm">
          <template #checkType="{ model, field }">
            <a-switch v-model:checked="model[field]" :checkedValue="1" :unCheckedValue="0" @change="onCheckTypeChange" />
            <span class="page-explain" @click="handleShowPageExplain">分页使用说明</span>
          </template>
        </BasicForm>
      </a-col>
    </a-row>
    <!-- sql语句 -->
    <div class="config h-full" v-if="getShowSqlBox()">
      <div class="left-pane">
        <jnpf-select
          class="!w-full"
          v-model:value="dataForm.dbLinkId"
          showSearch
          :options="dbOptions"
          placeholder="选择数据库"
          @change="handleSelectTable"
          :fieldNames="{ options: 'children' }" />
        <div class="box">
          <InputSearch class="search-box" :placeholder="t('common.enterKeyword')" allowClear v-model:value="keyword" @search="handleSearchTable" />
          <BasicTree
            class="tree-box remove-active-tree"
            ref="leftTreeRef"
            :defaultExpandAll="false"
            :treeData="treeData"
            :loading="treeLoading"
            :load-data="onLoadData"
            @select="handleTreeSelect" />
        </div>
      </div>
      <div class="middle-pane">
        <div class="cap">
          <span>
            SQL语句
            <BasicHelp text="支持SQL语句&存储过程语句" />
          </span>
          <a-dropdown>
            <span class="cursor-pointer">系统变量<DownOutlined /></span>
            <template #overlay>
              <a-menu>
                <a-menu-item disabled>当前系统变量仅支持内部接口引用</a-menu-item>
                <a-menu-divider />
                <a-menu-item v-for="(item, index) in getSysVariableList" :key="index" @click="handleSysNodeClick(item.value)">
                  <span>{{ item.value }}</span>
                  <span style="float: right; color: #8492a6; padding-left: 10px">{{ item.tips }}</span>
                </a-menu-item>
              </a-menu>
            </template>
          </a-dropdown>
        </div>
        <MonacoEditor v-if="activeStep === 1" class="h-full" ref="sqlEditorRef" language="sql" v-model="dataForm.query" />
        <MonacoEditor v-if="activeStep === 2" class="h-full" ref="sqlEditorRef" language="sql" v-model="dataForm.propertyJson.countSql" />
        <MonacoEditor v-if="activeStep === 3" class="h-full" ref="sqlEditorRef" language="sql" v-model="dataForm.propertyJson.echoSql" />
      </div>
      <div class="right-pane">
        <div class="right-pane-list">
          <div class="cap">
            <span>
              接口参数
              <BasicHelp text="接收方式:Body/json" />
            </span>
          </div>
          <a-table
            :data-source="requestParameters"
            :columns="parametersColumns"
            size="small"
            :scroll="{ x: undefined, y: 'calc(100vh - 310px)' }"
            :pagination="false"
            rowKey="id"
            class="drag-table list">
            <template #bodyCell="{ column, record, index }">
              <template v-if="column.key === 'drag'">
                <i class="drag-handler icon-ym icon-ym-darg" title="点击拖动" />
              </template>
              <template v-if="column.key === 'field'">
                <p @click="handleItemClick(record)" class="cursor-pointer">
                  <span class="required-sign">{{ record.required ? '*' : '' }}</span>
                  {{ record.field }}{{ record.fieldName ? '(' + record.fieldName + ')' : '' }}
                </p>
              </template>
              <template v-if="column.key === 'dataType'">
                <span>{{ getDataTypeText(record.dataType) }}</span>
              </template>
              <template v-if="column.key === 'action'">
                <a-space :size="10">
                  <i class="icon-ym icon-ym-btn-edit" @click="addOrUpdateHandle(record)"></i>
                  <i class="icon-ym icon-ym-delete" @click="removeParameter(index)"></i>
                </a-space>
              </template>
            </template>
          </a-table>
          <div class="table-actions" @click="addOrUpdateHandle()">
            <a-button type="link" preIcon="icon-ym icon-ym-btn-add">添加参数</a-button>
          </div>
        </div>
      </div>
    </div>
    <!-- api操作 -->
    <div class="overflow-auto h-full" v-if="activeStep === 1 && dataForm.dataType === 3">
      <a-row class="mt-20px">
        <a-col :span="14" :offset="5">
          <a-form :colon="false" :labelCol="{ style: { width: '90px' } }" :model="dataForm" ref="formElRef">
            <a-form-item label="接口类型" name="requestMethod">
              <jnpf-radio v-model:value="dataForm.requestMethod" :options="requestMethodOptions" @change="onMethodChange($event, 'api')" />
            </a-form-item>
            <a-form-item label="接口路径" name="path" :rules="pathRules">
              <a-input-search v-model:value="dataForm.path" placeholder="输入接口路径" @search="addHeaders">
                <template #enterButton>
                  <Button>
                    <template #icon> <i class="icon-ym icon-ym-btn-add"></i> </template>
                    添加headers
                  </Button>
                </template>
              </a-input-search>
            </a-form-item>
            <a-form-item label=" " v-if="requestHeaders.length" class="!-mt-10px">
              <a-form-item-rest>
                <a-row v-for="(item, index) in requestHeaders" :key="item.index" class="mt-10px">
                  <a-col :span="10">
                    <a-auto-complete class="w-200px" v-model:value="item.field" :options="restaurants" placeholder="key" :filter-option="filterOption" />
                  </a-col>
                  <a-col :span="10" :offset="1">
                    <a-input v-model:value="item.defaultValue" placeholder="value" clearable />
                  </a-col>
                  <a-col :span="2" :offset="1">
                    <a-button type="danger" preIcon="icon-ym icon-ym-nav-close" @click="removeHeaders(index)"> </a-button>
                  </a-col>
                </a-row>
              </a-form-item-rest>
            </a-form-item>
            <a-form-item name="requestParameters">
              <template #label>接口参数<BasicHelp text="接收方式:Body/json" /></template>
              <a-button @click="addOrUpdateHandle()" preIcon="icon-ym icon-ym-btn-add">添加参数 </a-button>
              <a-form-item-rest>
                <a-table
                  :data-source="requestParameters"
                  :columns="parametersColumns_"
                  size="small"
                  :pagination="false"
                  rowKey="id"
                  class="drag-table list mt-20px">
                  <template #bodyCell="{ column, record, index }">
                    <template v-if="column.key === 'drag'">
                      <i class="drag-handler icon-ym icon-ym-darg" title="点击拖动" />
                    </template>
                    <template v-if="column.key === 'field'">
                      <p>
                        <span class="required-sign">{{ record.required ? '*' : '' }}</span>
                        {{ record.field }}{{ record.fieldName ? '(' + record.fieldName + ')' : '' }}
                      </p>
                    </template>
                    <template v-if="column.key === 'dataType'">
                      <span>{{ getDataTypeText(record.dataType) }}</span>
                    </template>
                    <template v-if="column.key === 'action'">
                      <a-space :size="10">
                        <i class="icon-ym icon-ym-btn-edit" @click="addOrUpdateHandle(record)"></i>
                        <i class="icon-ym icon-ym-delete" @click="removeParameter(index)"></i>
                      </a-space>
                    </template>
                  </template>
                </a-table>
              </a-form-item-rest>
            </a-form-item>
            <a-form-item name="pageParameters" label="分页参数" v-if="dataForm.checkType">
              <a-form-item-rest>
                <a-table :data-source="pageParameters" :columns="pageParametersColumns" size="small" :pagination="false" rowKey="id">
                  <template #bodyCell="{ column, record }">
                    <template v-if="column.key === 'fieldName'">
                      <p>{{ record.fieldName }}</p>
                    </template>
                    <template v-if="column.key === 'field'">
                      <a-input v-model:value="record.field" :placeholder="record.fieldName" clearable />
                    </template>
                  </template>
                </a-table>
              </a-form-item-rest>
            </a-form-item>
          </a-form>
        </a-col>
      </a-row>
    </div>
    <div class="overflow-auto h-full" v-if="activeStep === 2 && dataForm.dataType === 3 && dataForm.checkType">
      <a-row class="mt-20px">
        <a-col :span="14" :offset="5">
          <a-form :colon="false" :labelCol="{ style: { width: '90px' } }" :model="dataForm" ref="formElRef">
            <a-form-item label="接口类型" name="propertyJson.echoReqMethod">
              <jnpf-radio v-model:value="dataForm.propertyJson.echoReqMethod" :options="requestMethodOptions" />
            </a-form-item>
            <a-form-item label="接口路径" name="echoPath" :rules="echoPathRules">
              <a-input-search v-model:value="dataForm.echoPath" placeholder="输入接口路径" @search="addHeaders(1)">
                <template #enterButton>
                  <Button>
                    <template #icon> <i class="icon-ym icon-ym-btn-add"></i> </template>
                    添加headers
                  </Button>
                </template>
              </a-input-search>
            </a-form-item>
            <a-form-item label=" " v-if="echoReqHeaders.length" class="!-mt-10px">
              <a-form-item-rest>
                <a-row v-for="(item, index) in echoReqHeaders" :key="item.index" class="mt-10px">
                  <a-col :span="10">
                    <a-auto-complete class="w-200px" v-model:value="item.field" placeholder="key" :options="restaurants" :filter-option="filterOption" />
                  </a-col>
                  <a-col :span="10" :offset="1">
                    <a-input v-model:value="item.defaultValue" placeholder="value" clearable />
                  </a-col>
                  <a-col :span="2" :offset="1">
                    <a-button type="danger" preIcon="icon-ym icon-ym-nav-close" @click="removeHeaders(index, 1)"> </a-button>
                  </a-col>
                </a-row>
              </a-form-item-rest>
            </a-form-item>
            <a-form-item name="echoReqParameters">
              <template #label>接口参数<BasicHelp text="接收方式:Body/json" /></template>
              <a-button @click="addOrUpdateHandle(null, 1)" preIcon="icon-ym icon-ym-btn-add">添加参数 </a-button>
              <a-form-item-rest>
                <a-table
                  :data-source="echoReqParameters"
                  :columns="parametersColumns_"
                  size="small"
                  :pagination="false"
                  rowKey="id"
                  class="drag-table list mt-20px">
                  <template #bodyCell="{ column, record, index }">
                    <template v-if="column.key === 'drag'">
                      <i class="drag-handler icon-ym icon-ym-darg" title="点击拖动" />
                    </template>
                    <template v-if="column.key === 'field'">
                      <p>
                        <span class="required-sign">{{ record.required ? '*' : '' }}</span>
                        {{ record.field }}{{ record.fieldName ? '(' + record.fieldName + ')' : '' }}
                      </p>
                    </template>
                    <template v-if="column.key === 'dataType'">
                      <span>{{ getDataTypeText(record.dataType) }}</span>
                    </template>
                    <template v-if="column.key === 'action'">
                      <a-space :size="10">
                        <i class="icon-ym icon-ym-btn-edit" @click="addOrUpdateHandle(record, 1)"></i>
                        <i class="icon-ym icon-ym-delete" @click="removeParameter(index, 1)"></i>
                      </a-space>
                    </template>
                  </template>
                </a-table>
              </a-form-item-rest>
            </a-form-item>
            <a-form-item name="echoParameters" label="回显参数">
              <a-form-item-rest>
                <a-table :data-source="echoParameters" :columns="echoParametersColumns" size="small" :pagination="false" rowKey="id">
                  <template #bodyCell="{ column, record }">
                    <template v-if="column.key === 'fieldName'">
                      <p>{{ record.fieldName }}</p>
                    </template>
                    <template v-if="column.key === 'field'">
                      <a-input v-model:value="record.field" :placeholder="record.fieldName" clearable />
                    </template>
                  </template>
                </a-table>
              </a-form-item-rest>
            </a-form-item>
          </a-form>
        </a-col>
      </a-row>
    </div>
    <!-- 静态数据数据处理 -->
    <MonacoEditor v-if="activeStep === 1 && dataForm.dataType == 2" class="h-full" ref="editorRef" v-model="dataForm.query" language="json" />
    <!-- api\sql数据处理 -->
    <div class="jsStaticData" v-if="activeStep === getStepList.length - 1 && dataForm.dataType !== 2">
      <div class="json-box">
        <MonacoEditor class="h-full" ref="codeEditorRef" v-model="dataForm.dataProcessing" language="javascript" />
      </div>
      <div class="jsTips">
        <p>1、支持JavaScript的脚本</p>
        <p>2、小程序不支持在线JS脚本</p>
      </div>
    </div>
    <FieldForm @register="registerFiledForm" @reload="reloadRequestParameters" />
    <PageExplainModal @register="registerPageExplain" />
  </BasicPopup>
</template>
<script lang="ts" setup>
  import { ref, computed, reactive, toRefs, unref, nextTick } from 'vue';
  import { BasicPopup, usePopupInner } from '/@/components/Popup';
  import { BasicForm, useForm, FormSchema } from '/@/components/Form';
  import { getDataInterfaceInfo, createDataInterface, updateDataInterface } from '/@/api/systemData/dataInterface';
  import { getDataModelListAll, getDataModelFieldList } from '/@/api/systemData/dataModel';
  import { getDataSourceSelector } from '/@/api/systemData/dataSource';
  import { BasicTree } from '/@/components/Tree';
  import { useMessage } from '/@/hooks/web/useMessage';
  import { useBaseStore } from '/@/store/modules/base';
  import { useI18n } from '/@/hooks/web/useI18n';
  import { MonacoEditor } from '/@/components/CodeEditor';
  import { BasicColumn } from '/@/components/Table';
  import { useModal } from '/@/components/Modal';
  import { DownOutlined } from '@ant-design/icons-vue';
  import { InputSearch } from 'ant-design-vue';
  import { Button } from 'ant-design-vue';
  import FieldForm from './components/FieldForm.vue';
  import PageExplainModal from './components/PageExplainModal.vue';
  import Sortablejs from 'sortablejs';
  import { getDataTypeText } from '/@/utils/jnpf';

  const defaultDataHandler = '(data) => {\r\n    // 处理数据逻辑\r\n\r\n    // 返回所需的数据\r\n    return data\r\n}';
  const defaultPageParameters: any[] = [
    { fieldName: 'currentPage', field: 'currentPage' },
    { fieldName: 'pageSize', field: 'pageSize' },
    { fieldName: 'keyword', field: 'keyword' },
  ];
  const defaultEchoParameters: any[] = [
    { fieldName: 'showKey', field: '' },
    { fieldName: 'showValue', field: '' },
  ];
  const defaultJson = {
    countSql: '',
    echoSql: '',
    echoPath: '',
    echoReqMethod: '6',
    echoReqParameters: [],
    echoReqHeaders: [],
    pageParameters: defaultPageParameters,
    echoParameters: defaultEchoParameters,
  };

  interface State {
    activeStep: number;
    keyword: string;
    treeData: any[];
    dbOptions: any[];
    requestParameters: any[];
    requestHeaders: any[];
    treeLoading: boolean;
    sqlRequestMethod: string;
    apiRequestMethod: string;
    dataForm: any;
    requestMethodOptions: any[];
    restaurants: any[];
    pathRules: any;
    echoPathRules: any;
    key: any;
    echoReqHeaders: any[];
    echoReqParameters: any[];
    pageParameters: any[];
    echoParameters: any[];
    sqlType: number;
  }

  const { t } = useI18n();
  const baseStore = useBaseStore();
  const sqlEditorRef = ref(null);
  const leftTreeRef = ref(null);
  const state = reactive<State>({
    activeStep: 0,
    keyword: '',
    treeData: [],
    dbOptions: [],
    requestParameters: [],
    requestHeaders: [],
    treeLoading: false,
    sqlRequestMethod: '3',
    apiRequestMethod: '6',
    dataForm: {
      fullName: '',
      enCode: '',
      categoryId: '',
      dbLinkId: '0',
      dataType: 2,
      checkType: 0,
      ipAddress: '',
      requestHeaders: '',
      requestMethod: '1',
      responseType: 'json',
      sortCode: 0,
      enabledMark: 1,
      description: '',
      dataProcessing: '',
      requestParameters: '',
      query: '',
      propertyJson: JSON.parse(JSON.stringify(defaultJson)),
    },
    requestMethodOptions: [
      { fullName: 'GET', id: '6' },
      { fullName: 'POST', id: '7' },
    ],
    restaurants: [
      { value: 'Postman-Token' },
      { value: 'Host' },
      { value: 'User-Agent' },
      { value: 'Accept' },
      { value: 'Accept-Encoding' },
      { value: 'Connection' },
    ],
    pathRules: [{ required: true, message: '请填写接口路径', trigger: 'blur' }],
    echoPathRules: [{ required: true, message: '请填写接口路径', trigger: 'blur' }],
    key: +new Date(),
    echoReqHeaders: [],
    echoReqParameters: [],
    pageParameters: [],
    echoParameters: [],
    sqlType: 0,
  });
  const {
    activeStep,
    dataForm,
    requestParameters,
    dbOptions,
    treeLoading,
    treeData,
    keyword,
    requestMethodOptions,
    requestHeaders,
    restaurants,
    pathRules,
    echoPathRules,
    key,
    echoReqHeaders,
    echoReqParameters,
    pageParameters,
    echoParameters,
  } = toRefs(state);
  const schemas: FormSchema[] = [
    {
      field: 'fullName',
      label: '名称',
      component: 'Input',
      componentProps: { placeholder: '输入名称', maxlength: 50 },
      rules: [{ required: true, trigger: 'blur', message: '名称不能为空' }],
    },
    {
      field: 'enCode',
      label: '编码',
      component: 'Input',
      componentProps: { placeholder: '输入编码', maxlength: 50 },
      rules: [{ required: true, trigger: 'blur', message: '编码不能为空' }],
    },
    {
      field: 'categoryId',
      label: '分类',
      component: 'TreeSelect',
      componentProps: { placeholder: '选择分类' },
      rules: [{ required: true, trigger: 'blur', message: '请选择分类' }],
    },
    {
      field: 'dataType',
      label: '类型',
      component: 'Radio',
      defaultValue: 2,
      componentProps: {
        options: [
          { fullName: '静态数据', id: 2 },
          { fullName: 'SQL操作', id: 1 },
          { fullName: 'API操作', id: 3 },
        ],
        onChange: onDataTypeChange,
      },
    },
    {
      ifShow: ({ values }) => values.dataType === 1,
      field: 'requestMethod',
      label: '动作',
      component: 'Radio',
      defaultValue: '3',
      componentProps: {
        options: [
          { fullName: '查询', id: '3' },
          { fullName: '增加', id: '1' },
          { fullName: '修改', id: '2' },
          { fullName: '删除', id: '4' },
        ],
        onChange: onMethodChange,
      },
    },
    {
      field: 'sortCode',
      label: '排序',
      component: 'InputNumber',
      defaultValue: 0,
      componentProps: { min: 0, max: 999999 },
    },
    {
      field: 'enabledMark',
      label: '状态',
      component: 'Switch',
      defaultValue: 1,
    },
    {
      ifShow: ({ values }) => values.dataType === 3 || (values.dataType === 1 && values.requestMethod === '3'),
      field: 'checkType',
      label: '分页',
      component: 'Switch',
      defaultValue: 0,
      slot: 'checkType',
    },
    {
      field: 'description',
      label: '说明',
      component: 'Textarea',
      componentProps: { row: 3 },
    },
  ];
  const parametersColumns: BasicColumn[] = [
    { title: '拖动', dataIndex: 'drag', key: 'drag', align: 'center', width: 50 },
    { title: '参数名称', dataIndex: 'field', key: 'field' },
    { title: '参数类型', dataIndex: 'dataType', key: 'dataType', width: 80 },
    { title: '操作', dataIndex: 'action', key: 'action', width: 70 },
  ];
  const pageParametersColumns: BasicColumn[] = [
    { title: '分页字段', dataIndex: 'fieldName', key: 'fieldName', width: 300 },
    { title: '接口参数', dataIndex: 'field', key: 'field', width: 300 },
  ];
  const echoParametersColumns: BasicColumn[] = [
    { title: '回显字段', dataIndex: 'fieldName', key: 'fieldName', width: 300 },
    { title: '接口参数', dataIndex: 'field', key: 'field', width: 300 },
  ];
  const parametersColumns_: BasicColumn[] = JSON.parse(JSON.stringify(parametersColumns));
  const row = { title: '默认值', dataIndex: 'defaultValue', key: 'defaultValue' };
  parametersColumns_.splice(3, 0, row);
  const emit = defineEmits(['register', 'reload']);
  const { createMessage, createConfirm } = useMessage();
  const [registerForm, { setFieldsValue, validate, resetFields, updateSchema }] = useForm({ labelWidth: 60, schemas: schemas });
  const [registerPopup, { closePopup, changeLoading, changeOkLoading }] = usePopupInner(init);
  const [registerFiledForm, { openModal: openFiledFormModal }] = useModal();
  const [registerPageExplain, { openModal: openPageExplainModal }] = useModal();
  const formElRef = ref();

  const getTitle = computed(() => (state.dataForm.id ? '新建' : '编辑'));
  const getStepList = computed(() => {
    state.key = +new Date();
    let base = ['基本信息', '数据配置'];
    if (state.dataForm.dataType === 2) return base;
    if (state.dataForm.dataType === 1 && state.dataForm.checkType === 1) return [...base, '数量统计', '数据回显', '数据处理'];
    if (state.dataForm.dataType === 3 && state.dataForm.checkType === 1) return [...base, '数据回显', '数据处理'];
    return [...base, '数据处理'];
  });
  const getSysVariableList = computed(() => {
    const list = [
      { value: '@user', tips: '当前用户' },
      { value: '@currentUsersAndSubordinates', tips: '当前用户及下属' },
      { value: '@organization', tips: '当前组织' },
      { value: '@currentOrganizationAndSuborganization', tips: '当前组织及子组织' },
      { value: '@chargeorganization', tips: '当前分管组织' },
      { value: '@currentChargeorganizationAndSuborganization', tips: '当前分管组织及子组织' },
    ];
    const dataConfigList = [
      { value: '@offsetSize', tips: '开始数据条数' },
      { value: '@pageSize', tips: '返回数据条数' },
    ];
    const dataEchoList = [
      { value: '@showKey', tips: '回显字段查询key' },
      { value: '@showValue', tips: '回显字段值' },
    ];
    const keyword = { value: '@keyword', tips: '关键词搜索' };
    if (!state.dataForm.checkType) return list;
    if (state.activeStep === 2) return [...list, keyword];
    if (state.activeStep === 3) return [...list, ...dataEchoList];
    return [...list, ...dataConfigList, keyword];
  });

  async function init(data) {
    state.activeStep = 0;
    resetData();
    resetFields();
    state.dataForm.id = data.id;
    const options = (await baseStore.getDictionaryData('DataInterfaceType')) as any[];
    updateSchema([{ field: 'categoryId', componentProps: { options } }]);
    getDataSourceSelector().then(res => {
      let list = res.data.list || [];
      list = list.filter(o => o.children && o.children.length);
      if (list[0] && list[0].children && list[0].children.length) list[0] = list[0].children[0];
      delete list[0].children;
      state.dbOptions = list;
      if (state.dataForm.id) {
        changeLoading(true);
        getDataInterfaceInfo(state.dataForm.id).then(res => {
          if (!res.data.propertyJson) res.data.propertyJson = JSON.stringify(defaultJson);
          res.data.propertyJson = JSON.parse(res.data.propertyJson);
          res.data.echoPath = res.data.propertyJson.echoPath || '';
          state.dataForm = res.data;
          setFieldsValue({
            fullName: state.dataForm.fullName,
            enCode: state.dataForm.enCode,
            categoryId: state.dataForm.categoryId,
            dataType: state.dataForm.dataType,
            requestMethod: state.dataForm.requestMethod,
            sortCode: state.dataForm.sortCode,
            enabledMark: state.dataForm.enabledMark,
            description: state.dataForm.description,
            checkType: state.dataForm.checkType,
            id: state.dataForm.id,
          });
          if (res.data.requestParameters) state.requestParameters = JSON.parse(res.data.requestParameters) || [];
          if (res.data.requestHeaders) state.requestHeaders = JSON.parse(res.data.requestHeaders) || [];
          if (res.data.propertyJson) {
            const propertyJson = res.data.propertyJson;
            state.echoReqHeaders = propertyJson.echoReqHeaders || [];
            state.echoReqParameters = propertyJson.echoReqParameters || [];
            state.pageParameters = propertyJson.pageParameters && propertyJson.pageParameters.length ? propertyJson.pageParameters : defaultPageParameters;
            state.echoParameters = propertyJson.echoParameters && propertyJson.echoParameters.length ? propertyJson.echoParameters : defaultEchoParameters;
          }
          if (res.data.dataType === 1) state.sqlRequestMethod = state.dataForm.requestMethod;
          if (res.data.dataType === 3) state.apiRequestMethod = state.dataForm.requestMethod;
          getTableList();
          changeLoading(false);
        });
      } else {
        updateSchema([{ field: 'categoryId', defaultValue: data.categoryId }]);
        getTableList();
      }
    });
  }
  function getTableList() {
    state.treeLoading = true;
    const query = {
      linkId: state.dataForm.dbLinkId,
      keyword: state.keyword,
      pageSize: 1000000,
    };
    getDataModelListAll(query).then(res => {
      state.treeData = res.data.list.map(o => ({
        ...o,
        fullName: o.tableName ? o.table + '(' + o.tableName + ')' : o.table,
        isLeaf: false,
        id: o.table,
        icon: o.type == 1 ? 'icon-ym icon-ym-view' : 'icon-ym icon-ym-generator-tableGrid',
      }));
      state.treeLoading = false;
    });
  }
  async function handleSubmit() {
    const values = await validate();
    if (!values) return;
    if (!state.dataForm.query && state.dataForm.dataType == 2) return createMessage.error('请输入静态数据');
    changeOkLoading(true);
    state.dataForm.requestHeaders = JSON.stringify(state.requestHeaders);
    state.dataForm.requestParameters = JSON.stringify(state.requestParameters);
    state.dataForm.propertyJson.echoPath = state.dataForm.echoPath;
    const query = {
      ...state.dataForm,
      ...values,
      id: state.dataForm.id,
      propertyJson: JSON.stringify(state.dataForm.propertyJson),
    };
    const formMethod = state.dataForm.id ? updateDataInterface : createDataInterface;
    formMethod(query)
      .then(res => {
        createMessage.success(res.msg);
        changeOkLoading(false);
        closePopup();
        emit('reload');
      })
      .catch(() => {
        changeOkLoading(false);
      });
  }
  function onDataTypeChange(val) {
    state.dataForm.dataType = val;
    state.dataForm.requestMethod = val == 1 ? state.sqlRequestMethod : val == 3 ? state.apiRequestMethod : '';
    setFieldsValue({ requestMethod: state.dataForm.requestMethod });
    state.requestParameters = [];
  }
  function onCheckTypeChange(val) {
    state.dataForm.checkType = val;
  }
  function handlePrev() {
    state.activeStep -= 1;
  }
  async function handleNext() {
    const values = await validate();
    if (!values) return;
    if (state.dataForm.dataType === 1) {
      if (state.activeStep === 1 && !state.dataForm.query) return createMessage.warning('请输入SQL查询语');
      if (state.dataForm.checkType) {
        if (state.activeStep === 2 && !state.dataForm.propertyJson.countSql) return createMessage.warning('请输入SQL语句');
        if (state.activeStep === 3 && !state.dataForm.propertyJson.echoSql) return createMessage.warning('请输入SQL语句');
      }
      handleNextFun();
    }
    if (state.dataForm.dataType === 3) {
      if (state.activeStep === 1 || (state.activeStep === 2 && state.dataForm.checkType)) {
        try {
          const values_ = await formElRef.value?.validate();
          if (!values_) return;
          handleNextFun();
        } catch (_) {}
      } else {
        handleNextFun();
      }
    }
    if (state.dataForm.dataType === 2) handleNextFun();
  }
  function handleNextFun() {
    state.activeStep += 1;
    // SQL操作
    if (state.dataForm.dataType === 1 && state.activeStep === unref(getStepList).length - 1) setDataProcessing();
    // API操作
    if (state.dataForm.dataType === 3) {
      if (state.activeStep === 1 || (state.activeStep === 2 && state.dataForm.checkType)) nextTick(() => initSort());
      if (state.activeStep === unref(getStepList).length - 1) setDataProcessing();
    }
  }
  function setDataProcessing() {
    if (!state.dataForm.dataProcessing) state.dataForm.dataProcessing = defaultDataHandler;
  }
  function removeParameter(index, type?) {
    createConfirm({
      iconType: 'warning',
      title: t('common.tipTitle'),
      content: '此操作删除该参数, 是否继续?',
      onOk: () => {
        state[type === 1 ? 'echoReqParameters' : 'requestParameters'].splice(index, 1);
      },
    });
  }
  function addOrUpdateHandle(item = null, type?) {
    state.sqlType = type || 0;
    item = item ? JSON.parse(JSON.stringify(item)) : null;
    const list = type == 1 ? state.echoReqParameters : state.requestParameters;
    openFiledFormModal(true, { item, list: list });
  }
  function reloadRequestParameters(type, item) {
    const data = JSON.parse(JSON.stringify(item));
    const key = state.sqlType == 1 ? 'echoReqParameters' : 'requestParameters';
    if (type == 'add') {
      state[key].push(data);
    } else {
      const index = state[key].findIndex(res => res.id == data.id);
      if (index != -1) state[key][index] = data;
    }
  }
  function handleSysNodeClick(data) {
    (sqlEditorRef.value as any)?.insert(data);
  }
  function handleItemClick(item) {
    item.field && (sqlEditorRef.value as any)?.insert('{' + item.field + '}');
  }
  function getTree() {
    const tree = unref(leftTreeRef);
    if (!tree) {
      throw new Error('tree is null!');
    }
    return tree as any;
  }
  function onLoadData(node) {
    return new Promise((resolve: (value?: unknown) => void) => {
      getDataModelFieldList(state.dataForm.dbLinkId, node.dataRef.table).then(res => {
        const data = res.data.list.map(o => ({
          ...o,
          isLeaf: true,
          fullName: o.fieldName ? o.field + '(' + o.fieldName + ')' : o.field,
          id: node.dataRef.table + '-' + o.field,
        }));
        getTree().updateNodeByKey(node.eventKey, { children: data, isLeaf: !data.length });
        resolve();
      });
    });
  }
  function handleSelectTable(val) {
    state.dataForm.dbLinkId = val;
    getTableList();
  }
  function handleTreeSelect(keys) {
    const selectedNode: any = getTree()?.getSelectedNode(keys[0]);
    const content = selectedNode.isLeaf ? selectedNode.field : selectedNode.table;
    (sqlEditorRef.value as any)?.insert(content);
  }
  function handleSearchTable() {
    treeData.value = [];
    getTableList();
  }
  function initSort() {
    const table: any = document.querySelector(`.drag-table .ant-table-tbody`);
    Sortablejs.create(table, {
      handle: '.drag-handler',
      animation: 150,
      easing: 'cubic-bezier(1, 0, 0, 1)',
      onStart: () => {},
      onEnd: ({ newIndex, oldIndex }: any) => {
        if (state.dataForm.dataType === 1) {
          oldIndex = oldIndex - 1;
          newIndex = newIndex - 1;
        }
        setNodeSort(state.requestParameters, oldIndex, newIndex);
      },
    });
  }
  function setNodeSort(data: any, oldIndex: any, newIndex: any) {
    const currRow = data.splice(oldIndex, 1)[0];
    data.splice(newIndex, 0, currRow);
  }
  function addHeaders(type?) {
    state[type === 1 ? 'echoReqHeaders' : 'requestHeaders'].push({ field: '', defaultValue: '' });
  }
  function removeHeaders(index, type?) {
    state[type === 1 ? 'echoReqHeaders' : 'requestHeaders'].splice(index, 1);
  }
  function filterOption(input: string, option: any) {
    return option.value.toUpperCase().indexOf(input.toUpperCase()) >= 0;
  }
  function onMethodChange(val, key = 'sql') {
    if (key == 'sql') state.dataForm.requestMethod = val;
    state[key + 'RequestMethod'] = val;
    let data = { action: val };
    if (val !== 3) {
      data['checkType'] = false;
      state.dataForm.checkType = false;
    }
    setFieldsValue(data);
  }
  function resetData() {
    state.keyword = '';
    state.dataForm = {
      fullName: '',
      enCode: '',
      categoryId: '',
      dbLinkId: '0',
      dataType: 2,
      checkType: 0,
      ipAddress: '',
      requestHeaders: '',
      requestMethod: '1',
      responseType: 'json',
      sortCode: 0,
      enabledMark: 1,
      description: '',
      dataProcessing: '',
      requestParameters: '',
      query: '',
      propertyJson: JSON.parse(JSON.stringify(defaultJson)),
    };
    state.echoReqHeaders = [];
    state.requestHeaders = [];
    state.sqlRequestMethod = '3';
    state.apiRequestMethod = '6';
    state.pageParameters = JSON.parse(JSON.stringify(defaultPageParameters));
    state.echoParameters = JSON.parse(JSON.stringify(defaultEchoParameters));
  }
  function handleShowPageExplain() {
    openPageExplainModal(true);
  }
  function getShowSqlBox() {
    if (state.dataForm.dataType !== 1) return false;
    if (state.activeStep === 1) return true;
    if (state.dataForm.checkType && (state.activeStep === 2 || state.activeStep === 3)) return true;
  }
</script>
<style lang="less" scoped>
  :deep(.scrollbar) {
    padding: 0;
    height: 100%;
    .scrollbar__view {
      height: 100%;

      & > div {
        height: 100% !important;
      }
    }
  }
  .steps {
    overflow: auto;
    .ant-steps-item {
      width: 150px;
    }
  }
  .page-explain {
    cursor: pointer;
    float: right;
    color: @text-color-label;
    &:hover {
      color: @primary-color;
    }
  }
  .config {
    flex: 1;
    padding: 10px;
    display: flex;
    justify-content: space-between;
    overflow: hidden;
    .left-pane {
      flex-shrink: 0;
      width: 350px;
      .box {
        margin-top: 8px;
        border-radius: 4px;
        height: calc(100% - 40px);
        border: 1px solid @border-color-base;
        overflow: hidden;
        .search-box {
          padding: 10px;
        }
        .tree-box {
          height: calc(100% - 52px);
          overflow: auto;
          overflow-x: hidden;
        }
      }
    }
    .middle-pane {
      flex: 1;
      margin: 0 10px;
      overflow: hidden;
      display: flex;
      flex-direction: column;
      border: 1px solid @border-color-base;
      border-radius: 4px;
      flex: 1;
      display: flex;
      flex-direction: column;
      overflow: hidden;
      .cap {
        height: 36px;
        line-height: 36px;
        display: flex;
        justify-content: space-between;
        color: @text-color-label;
        font-size: 14px;
        padding: 0 10px;
        flex-shrink: 0;
        border-bottom: 1px solid @border-color-base;
      }
      .top-box {
        display: flex;
        .main-box {
          flex: 1;
          margin-right: 18px;
        }
      }
    }
    .right-pane {
      width: 350px;
      flex-shrink: 0;
      display: flex;
      flex-direction: column;
      height: calc(100% + 9px);
      overflow: hidden;
      .right-pane-list {
        border: 1px solid @border-color-base;
        border-radius: 4px;
        flex: 1;
        display: flex;
        flex-direction: column;
        margin-bottom: 10px;
        overflow: hidden;
        .cap {
          height: 38px;
          line-height: 38px;
          display: flex;
          color: @text-color-label;
          font-size: 14px;
          padding: 0 10px;
          flex-shrink: 0;
          justify-content: space-between;
          align-items: center;
        }
        .table-actions {
          flex-shrink: 0;
          border-top: 1px dashed @border-color-base;
          text-align: center;
        }
        .list {
          flex: 1;
          display: flex;
          flex-direction: column;
          overflow: hidden;
        }
      }
      .right-pane-btn {
        flex-shrink: 0;
      }
    }
  }
  .jsStaticData {
    flex: 1;
    display: flex;
    overflow: hidden;
    flex-direction: column;
    padding: 10px;
    height: 100%;
    .json-box {
      flex: 1;
    }
    .jsTips {
      flex-shrink: 0;
      padding: 8px 16px;
      background-color: @primary-1;
      border-radius: 4px;
      border-left: 5px solid @primary-color;
      margin-top: 10px;

      p {
        line-height: 24px;
        color: @text-color-help-dark;
      }
    }
  }
  .icon-ym-btn-edit {
    color: @primary-color;
    cursor: pointer;
    font-size: 16px;
  }
  .icon-ym-delete {
    color: @error-color;
    cursor: pointer;
    font-size: 16px;
  }
  .ant-select {
    width: 100% !important;
  }
</style>
