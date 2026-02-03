<template>
  <div class="page-content-wrapper">
    <div class="page-content-wrapper-left">
      <BasicLeftTree title="指标目录" :treeData="treeData" :loading="treeLoading" @reload="reloadTree"
        @select="handleTreeSelect" :dropDownActions="leftDropDownActions" />
    </div>
    <div class="page-content-wrapper-center">
      <div class="page-content-wrapper-content">
        <a-form ref="searchForm" :model="searchInfo"
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
                <depSelectTag :dataArr="metricTagArr" :checkedArr="searchInfo.tags"
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
            <a-button type="primary" @click="openDimensionModal">公共维度</a-button>
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

    <!-- 公共维度列表模态框 -->
    <a-modal v-model:open="dimensionModalVisible" title="公共维度列表" width="900px" :footer="null">
      <a-table
        :columns="dimensionColumns"
        :data-source="dimensionList"
        :loading="dimensionLoading"
        :pagination="{ pageSize: 10 }"
        row-key="id"
        size="small"
      >
        <template #bodyCell="{ column, record }">
          <template v-if="column.key === 'action'">
            <a-button type="link" size="small" @click="editDimension(record)">编辑</a-button>
            <a-popconfirm title="确定要删除这个公共维度吗？" @confirm="deleteDimension(record.id)">
              <a-button type="link" size="small" danger>删除</a-button>
            </a-popconfirm>
          </template>
        </template>
      </a-table>
      <div style="text-align: right; margin-top: 16px;">
        <a-button type="primary" @click="openDimensionForm">新建公共维度</a-button>
        <a-button style="margin-left: 8px" @click="dimensionModalVisible = false">关闭</a-button>
      </div>
    </a-modal>

    <!-- 公共维度表单 -->
    <DepForm @register="registerDepForm" @reload="reloadDimensionList" />

    <!-- 左侧树 -->
    <OrgTree @register="registerOrgTree" />
  </div>
</template>
<script lang="ts" setup>
import { reactive, ref, onMounted } from 'vue';
import { Modal as AModal } from 'ant-design-vue';
import { BasicLeftTree } from '/@/components/Tree';
import { getMetriccategoryList } from '/@/api/targetDirectory';
import {
  postMetrickinshipList,
  deleteMetric,
  putMetricOnline,
  putMetricOffline,
  getMetrictagSelector,
  postMetricCopy,
} from '/@/api/targetDefinition';
import { BasicTable, useTable, TableAction, BasicColumn, ActionItem } from '/@/components/Table';
import { useMessage } from '/@/hooks/web/useMessage';
import { usePopup } from '/@/components/Popup';
import { useModal } from '/@/components/Modal';
import OrgTree from './OrgTree.vue';
import { useRouter } from 'vue-router';
import { depSelectTag } from '/@/components/DepSelectTag';
import TargetAdd from './components/targetAdd.vue';
import DepForm from '/@/views/kpi/dimension/DepForm.vue';
import { getDimensionList, deleteDimension as deleteDimensionApi } from '/@/api/dimension/model';
const { createMessage, createConfirm } = useMessage();

defineOptions({ name: 'permission-user' });
const router = useRouter();

const [registerOrgTree, { openPopup: openOrgTreePopup }] = usePopup();
const [registerDepForm, { openModal: openDepFormModal }] = useModal();
const metricTagArr = ref([]);
const searchForm = ref();
const state = reactive({
  addVisible: false, //新建指标
});

// 公共维度相关状态
const dimensionModalVisible = ref(false);
const dimensionList = ref<any[]>([]);
const dimensionLoading = ref(false);
const dimensionColumns = [
  { title: '公共维度名', dataIndex: 'name', key: 'name', width: 200 },
  { title: '数据类型', dataIndex: 'dataType', key: 'dataType', width: 120 },
  { title: '字段名', dataIndex: ['column', 'fieldName'], key: 'fieldName', width: 150 },
  { title: '最后更新时间', dataIndex: 'createdTime', key: 'createdTime', width: 180, format: 'date|YYYY-MM-DD HH:mm' },
  { title: '操作', key: 'action', width: 150, fixed: 'right' },
];
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
  afterFetch: () => { },
});

function getTableActions(record): ActionItem[] {
  return [
    {
      label: '编辑',
      onClick: updateHandle.bind(null, record),
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
function handleTreeSelect(id, _node) {
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

// ============ 公共维度相关方法 ============
// 打开公共维度列表模态框
function openDimensionModal() {
  dimensionModalVisible.value = true;
  reloadDimensionList();
}

// 加载公共维度列表
function reloadDimensionList() {
  dimensionLoading.value = true;
  getDimensionList({})
    .then(res => {
      dimensionList.value = res.data || [];
    })
    .catch(() => {
      createMessage.error('加载公共维度列表失败');
    })
    .finally(() => {
      dimensionLoading.value = false;
    });
}

// 打开公共维度表单（新建/编辑）
function openDimensionForm(id = '') {
  openDepFormModal(true, { id });
}

// 编辑公共维度
function editDimension(record) {
  dimensionModalVisible.value = false;
  openDimensionForm(record.id);
}

// 删除公共维度
function deleteDimension(id: string) {
  deleteDimensionApi(id)
    .then(res => {
      createMessage.success(res.msg || '删除成功');
      reloadDimensionList();
    })
    .catch(() => {
      createMessage.error('删除失败');
    });
}

onMounted(() => {
  initData(true);
});
</script>
<style scoped></style>
