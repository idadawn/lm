<template>
  <div class="page-content-wrapper">
    <div class="page-content-wrapper-left">
      <BasicLeftTree
        title="指标目录"
        :treeData="treeData"
        :loading="treeLoading"
        @reload="reloadTree"
        @select="handleTreeSelect"
        :dropDownActions="leftDropDownActions" />
    </div>
    <div class="page-content-wrapper-center">
      <div class="page-content-wrapper-content">
        <a-form
          ref="searchForm"
          :model="searchInfo"
          style="background: #fff; padding: 18px 10px 0px 10px; margin-bottom: 10px">
          <a-row :gutter="16">
            <a-col :span="8">
              <a-form-item label="关键字" name="keyword">
                <a-input v-model:value="searchInfo.keyword" allowClear placeholder="请输入"></a-input>
              </a-form-item>
            </a-col>
            <a-col :span="6">
              <a-form-item label="指标类型" name="type">
                <a-select ref="select" v-model:value="searchInfo.type" allowClear placeholder="请选择">
                  <a-select-option value="Basic">基础指标</a-select-option>
                  <a-select-option value="Derive">派生指标</a-select-option>
                  <a-select-option value="Composite">复合指标</a-select-option>
                </a-select>
              </a-form-item>
            </a-col>
            <a-col :span="6">
              <a-form-item label="状态" name="isEnabled">
                <a-select ref="select" v-model:value="searchInfo.isEnabled" allowClear placeholder="请选择">
                  <a-select-option value="true">已发布</a-select-option>
                  <a-select-option value="false">未发布</a-select-option>
                </a-select>
              </a-form-item>
            </a-col>
            <a-col :span="8">
              <a-form-item label="标签" name="tags">
                <depSelectTag
                  :dataArr="metricTagArr"
                  :checkedArr="searchInfo.tags"
                  @depSelectEmitsTag="depSelectEmitsTagFun"></depSelectTag>
              </a-form-item>
            </a-col>
            <a-col :span="6">
              <a-button type="primary" @click="searchFun" style="margin-right: 10px">查询</a-button>
              <a-button @click="resetFun">重置</a-button>
            </a-col>
          </a-row>
        </a-form>

        <BasicTable @register="registerTable" :searchInfo="searchInfo" ref="tableRef">
          <template #tableTitle>
            <a-button type="primary" preIcon="icon-ym icon-ym-btn-add" @click="addHandle">新建</a-button>
            <a-button type="primary" @click="rowCreationFun">创建分级</a-button>
            <a-button type="primary" @click="rowCloneFun">克隆指标</a-button>
            <a-button type="primary" @click="rowOnlineFun">批量上线</a-button>
            <a-button type="primary" @click="rowOfflineFun">批量下线</a-button>
            <a-button type="primary" danger @click="rowDeleteFun">批量删除</a-button>
          </template>
          <template #bodyCell="{ column, record }">
            <template v-if="column.key === 'name'">
              <a @click="jumpCharts(record)"> {{ record.name }}</a>
            </template>
            <template v-if="column.key === 'action' && !record.isAdministrator">
              <TableAction :actions="getTableActions(record)" />
            </template>
          </template>
        </BasicTable>
      </div>
    </div>

    <!-- 新增指标 -->
    <a-modal width="660px" v-model:visible="state.addVisible" title="" :footer="null">
      <TargetAdd />
    </a-modal>
    <!-- 创建分级 -->
    <a-modal width="800px" v-model:visible="state.createVisible" title="创建分级" :footer="null">
      <div style="padding: 20px 30px 30px 30px">
        <a-button type="primary" @click="newGradingFun" style="margin-bottom: 10px">新建分级</a-button>
        <a-table :columns="columnsCreate" :data-source="createTable" :pagination="false">
          <template #bodyCell="{ column, record }">
            <template v-if="column.key === 'action'">
              <span>
                <a @click="create_edit(record)">编辑</a>
                <a-divider type="vertical" />
                <a style="color: #de7673" @click="create_delete(record.id)">删除</a>
              </span>
            </template>
          </template>
        </a-table>
      </div>
    </a-modal>
    <!-- 新建分级 -->
    <a-modal width="600px" v-model:visible="state.newCreateVisible" title="新建分级" @ok="createHandleOk">
      <a-form ref="createAddForm" :model="createAddInfo" :label-col="{ span: 4 }" :wrapper-col="{ span: 18 }">
        <a-tabs v-model:activeKey="activeKey" centered>
          <a-tab-pane key="1" tab="值">
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
          <a-tab-pane key="2" tab="区间" force-render>
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
    <!-- 左侧树 -->
    <OrgTree @register="registerOrgTree" />
  </div>
</template>
<script lang="ts" setup>
  import { reactive, ref, onMounted, watch } from 'vue';
  import { Modal as AModal } from 'ant-design-vue';
  import { BasicLeftTree, TreeItem } from '/@/components/Tree';
  import { getMetriccategoryList } from '/@/api/targetDirectory';
  import {
    postMetrickinshipList,
    deleteMetric,
    putMetricOnline,
    putMetricOffline,
    getMetrictagSelector,
    getMetricCovstatusOptions,
    postMetricCopy,
    getMetricGraded,
    putMetricGraded,
    deleteMetricGraded,
    postMetricGradedList,
    postMetricGraded,
  } from '/@/api/targetDefinition';
  import { BasicTable, useTable, TableAction, BasicColumn, FormProps, ActionItem } from '/@/components/Table';
  import { useMessage } from '/@/hooks/web/useMessage';
  import { usePopup } from '/@/components/Popup';
  import OrgTree from './OrgTree.vue';
  import { useRouter } from 'vue-router';
  import { depSelectTag } from '/@/components/DepSelectTag';
  import TargetAdd from './components/targetAdd.vue';
  const { createMessage, createConfirm } = useMessage();

  defineOptions({ name: 'permission-user' });
  const router = useRouter();

  const [registerOrgTree, { openPopup: openOrgTreePopup }] = usePopup();
  const metricTagArr = ref([]);
  const searchForm = ref();
  const state = reactive({
    addVisible: false, //新建指标
    createVisible: false, //创建分级
    newCreateVisible: false, //新建分级
  });
  const columns: BasicColumn[] = [
    { title: '指标名称', dataIndex: 'name', width: 250 },
    { title: '指标编码', dataIndex: 'code', width: 170 },
    { title: '指标目录', dataIndex: 'metricCategoryName', width: 100 },
    { title: '指标类型', dataIndex: 'typeName', width: 100 },
    {
      title: '标签',
      dataIndex: 'metricTagName',
      width: 100,
      customRender: ({ record }) => {
        const text = record.metricTagName.toString();
        return text;
      },
    },
    {
      title: '状态',
      dataIndex: 'isEnabled',
      width: 100,
      align: 'center',
      customRender: ({ record }) => {
        const text = record.isEnabled ? '已发布' : '未发布';
        return text;
      },
    },
    { title: '创建时间', dataIndex: 'createdTime', width: 150, format: 'date|YYYY-MM-DD HH:mm' },
    { title: '更新时间', dataIndex: 'lastModifiedTime', width: 150, format: 'date|YYYY-MM-DD HH:mm' },
  ];
  const searchInfo = reactive({
    keyword: '', //关键字
    isEnabled: undefined, // 状态
    type: undefined, //指标类型
    tags: [], //标签
    isShowDeleted: 0,
    menuId: '', //左侧菜单指标目录id
  });
  const leftDropDownActions = [
    {
      label: '架构图',
      onClick: openOrgTreePopup.bind(null, true, {}),
    },
  ];
  // 创建分级
  const activeKey = ref('1');
  const metricStatusArr = ref<any[]>([]);
  const createTable = ref<any[]>([]); // 筛选-table
  const columnsCreate = [
    { title: '分级名称', dataIndex: 'name', width: 150 },
    { title: '值', key: 'value', dataIndex: 'value', width: 200 },
    { title: '状态', key: 'status', dataIndex: 'status', width: 130 },
    { title: '区间', key: 'rangType', dataIndex: 'rangType', width: 130 },
    { title: '趋势', key: 'trend', dataIndex: 'trend', width: 130 },
    { title: '操作', key: 'action', width: 150 },
  ];
  const createAddInfo = reactive({
    id: '', //分级id
    metricId: '', //指标的id
    value: '', // 值
    name: '', // 分级名称
    status: undefined, //状态
    status_color: '#000', //颜色
    trend: undefined, //趋势
    rangType: undefined, //区间
  });

  const treeLoading = ref(false);
  const treeData = ref<any[]>([]);
  const [registerTable, { reload, setLoading, getSelectRows, clearSelectedRowKeys }] = useTable({
    api: postMetrickinshipList,
    columns,
    immediate: false,
    actionColumn: {
      width: 200,
      title: '操作',
      dataIndex: 'action',
    },
    rowSelection: { type: 'checkbox' },
    clickToRowSelect: false,
    afterFetch: data => {},
  });

  function getTableActions(record): ActionItem[] {
    return [
      {
        label: '编辑',
        onClick: updateHandle.bind(null, record),
      },
      {
        label: '创建分级',
        onClick: rowCreationFun.bind(null, record, '分级'),
      },
      {
        label: `${record.isEnabled === true ? '下线' : '发布'}`,
        onClick: handleState.bind(null, record),
      },
      {
        label: '删除',
        color: 'error',
        modelConfirm: {
          onOk: handleDelete.bind(null, record.id),
        },
      },
    ];
  }

  function initData(isInit = false) {
    treeLoading.value = true;
    if (isInit) setLoading(true);
    getMetriccategoryList({}).then(res => {
      treeData.value = [{ fullName: '顶级节点', id: '', children: res.data }];
      treeLoading.value = false;
      if (isInit) reload({ page: 1 });
    });
  }

  // 查询
  function searchFun() {
    reload();
  }
  // 重置
  function resetFun() {
    searchForm.value.resetFields();
    reload();
  }
  function reloadTree() {
    treeData.value = [];
    initData();
  }
  function handleTreeSelect(id, _node, nodePath) {
    if (searchInfo.menuId === id) return;
    searchInfo.menuId = id;
    reload();
  }
  // 添加
  function addHandle() {
    state.addVisible = true;
  }

  // 编辑
  function updateHandle(record) {
    // 
    // type: 'Basic', //指标类型,基础指标Basic,派生指标Derive,复合指标Composite
    if (record.type == 'Basic') {
      router.push({
        path: '/kpi/indicatorDefine/formAtomic',
        query: {
          queryId: record.id,
        },
      });
    } else if (record.type == 'Derive') {
      router.push({
        path: '/kpi/indicatorDefine/formDerive',
        query: {
          queryId: record.id,
        },
      });
    } else if (record.type == 'Composite') {
      router.push({
        path: '/kpi/indicatorDefine/formRecombination',
        query: {
          queryId: record.id,
        },
      });
    }
  }

  // 删除
  function handleDelete(data) {
    deleteMetric([data]).then(res => {
      createMessage.success(res.msg);
      reload();
    });
  }
  // 发布下线
  function handleState(record) {
    if (record.isEnabled == true) {
      // 指标下线
      putMetricOffline([record.id]).then(res => {
        if (res.code == 200) {
          reload();
        }
      });
    } else {
      // 指标在线
      putMetricOnline([record.id]).then(res => {
        if (res.code == 200) {
          reload();
        }
      });
    }
  }
  // 复选框-删除
  function rowDeleteFun() {
    const selectData = getSelectRows() || [];
    if (!selectData.length) return createMessage.error('请选择一条数据');
    createConfirm({
      iconType: 'warning',
      title: '提示',
      content: '您确定要删除这些数据吗, 是否继续?',
      onOk: () => {
        const query = {
          list: selectData.map(item => item.id),
        };
        // 
        deleteMetric(query.list).then(res => {
          createMessage.success(res.msg);
          clearSelectedRowKeys();
          reload();
        });
      },
    });
  }
  // 复选框-上线
  function rowOnlineFun() {
    const selectData = getSelectRows() || [];
    if (!selectData.length) return createMessage.error('请选择一条数据');
    createConfirm({
      iconType: 'warning',
      title: '提示',
      content: '您确定要上线这些数据吗, 是否继续?',
      onOk: () => {
        const query = {
          list: selectData.map(item => item.id),
        };
        // 
        putMetricOnline(query.list).then(res => {
          createMessage.success(res.msg);
          clearSelectedRowKeys();
          reload();
        });
      },
    });
  }
  // 复选框-下线
  function rowOfflineFun() {
    const selectData = getSelectRows() || [];
    if (!selectData.length) return createMessage.error('请选择一条数据');
    createConfirm({
      iconType: 'warning',
      title: '提示',
      content: '您确定要下线这些数据吗, 是否继续?',
      onOk: () => {
        const query = {
          list: selectData.map(item => item.id),
        };
        // 
        putMetricOffline(query.list).then(res => {
          createMessage.success(res.msg);
          clearSelectedRowKeys();
          reload();
        });
      },
    });
  }
  // 复选框-创建分级
  function rowCreationFun(record, name) {
    if (name === '分级') {
      state.createVisible = true;
      // 获取指标id
      createAddInfo.metricId = record.id;
      postMetricGradedListFun(record.id);
    } else {
      const selectData = getSelectRows() || [];
      if (!selectData.length || selectData.length != 1) return createMessage.error('请选择一条数据进行创建分级');
      state.createVisible = true;
      // 获取指标id
      createAddInfo.metricId = selectData[0].id;
      postMetricGradedListFun(selectData[0].id);
    }
  }

  function postMetricGradedListFun(metricId) {
    // 获取指标分级列表
    postMetricGradedList(metricId).then(res => {
      createTable.value = res.data;
      createTable.value.forEach(item => {
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
          }
        });
      });
    });
  }

  // 复选框-克隆指标
  function rowCloneFun() {
    const selectData = getSelectRows() || [];
    if (!selectData.length || selectData.length != 1) return createMessage.error('请选择一条数据进行克隆指标');
    postMetricCopy(selectData[0].id).then(res => {
      if (res.code === 200) {
        reload();
      }
    });
  }

  // 创建分级
  function newGradingFun() {
    state.newCreateVisible = true;
    createAddInfo.id = '';
    createAddInfo.value = '';
    createAddInfo.name = '';
    createAddInfo.status = undefined;
    createAddInfo.status_color = '#000';
    createAddInfo.trend = undefined;
    createAddInfo.rangType = undefined;
  }
  // 创建分级-状态
  getMetricCovstatusOptions().then(res => {
    metricStatusArr.value = res.data;
  });
  // 切换状态时更新颜色
  function createStatusChange(value, obj) {
    createAddInfo.status_color = obj.item.color;
  }

  // 提交分级
  function createHandleOk() {
    // 
    if (activeKey.value == '1') {
      // 新建分级当值提交时，趋势和区间要初始化
      createAddInfo.trend = undefined;
    }
    if (createAddInfo.id) {
      putMetricGraded(createAddInfo.id, createAddInfo).then(res => {
        if (res.code === 200) {
          createMessage.success(res.msg);
          state.newCreateVisible = false;
          postMetricGradedListFun(createAddInfo.metricId);
        }
      });
    } else {
      postMetricGraded(createAddInfo).then(res => {
        if (res.code === 200) {
          createMessage.success(res.msg);
          state.newCreateVisible = false;
          postMetricGradedListFun(createAddInfo.metricId);
        }
      });
    }
  }

  //创建分级-编辑
  function create_edit(record) {
    getMetricGraded(record.id).then(res => {
      if (res.data.trend) {
        activeKey.value = '2';
      } else {
        activeKey.value = '1';
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
    createConfirm({
      iconType: 'warning',
      title: '提示',
      content: '您确定要删除数据吗, 是否继续?',
      onOk: () => {
        deleteMetricGraded(id).then(res => {
          if (res.code === 200) {
            createMessage.success(res.msg);
            state.newCreateVisible = false;
            postMetricGradedListFun(createAddInfo.metricId);
          }
        });
      },
    });
  }

  //标签
  getMetrictagSelector().then(res => {
    metricTagArr.value = res.data;
  });
  // 选中的标签id
  function depSelectEmitsTagFun(val) {
    // 
    searchInfo.tags = val;
  }

  //跳转到图表
  function jumpCharts(record) {
    const recordId = record.id;
    const recordType = record.type;

    router.push({
      path: '/kpi/indicatorDefine/chartsTree',
      query: {
        recordId: recordId,
        recordType: recordType,
      },
    });
  }

  onMounted(() => {
    initData(true);
  });
</script>
<style scoped>
  .divColor {
    width: 100px;
    height: 30px;
  }
</style>
