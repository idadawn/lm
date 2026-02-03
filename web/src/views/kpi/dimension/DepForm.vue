<template>
  <BasicModal v-bind="$attrs" @register="registerModal" :title="getTitle" @ok="handleSubmit">
    <!-- <template> -->
    <div class="page-content-wrapper">
      <div class="page-content-wrapper-left">
        <BasicLeftTree :treeData="treeData" :loading="treeLoading" :showSearch="true" searchPlaceholder="搜索数据源"
          @reload="reloadTree" @select="handleTreeSelect" v-model:selectKeysId="selectKeysId"
          :dropDownActions="leftDropDownActions" :fieldNames="{ key: 'id', title: 'name' }" :defaultExpandAll="false" />
      </div>
      <div class="page-content-wrapper-center">
        <div class="page-content-wrapper-content">
          <a-form layout="vertical" :model="formState" :label-col="{ span: 8 }" :wrapper-col="{ span: 24 }">
            <a-form-item label="维度名">
              <jnpf-input v-model:value="state.addInfo.Name" placeholder="请输入" disabled />
            </a-form-item>
            <a-form-item label="维度" name="popupWidth">
              <a-select ref="select" show-search allowClear placeholder="请选择" v-model:value="targetCheckedArrWeidu"
                :filter-option="filterOption" @change="depSelectItemEmitsFun" :options="targetColumnArrWeidu"
                :fieldNames="{
                  label: 'fieldName',
                  value: 'field',
                }">
              </a-select>
            </a-form-item>
          </a-form>
        </div>
      </div>

      <!-- 左侧树 -->
      <OrgTree @register="registerOrgTree" />
    </div>
    <!-- </template> -->
  </BasicModal>
</template>
<script lang="ts" setup>
import {
  addDimension,
  updateDimension,
  getMetricSchema,
  getDimensionDetail,
} from '/@/api/dimension/model';
import { getMetricLinkIdSchemaSchemaName } from '/@/api/targetDefinition';
import { reactive, ref, unref, computed, onMounted } from 'vue';
import { BasicModal, useModalInner } from '/@/components/Modal';
import OrgTree from '/@/views/kpi/indicatorDefine/OrgTree.vue';
import { BasicLeftTree, TreeItem } from '/@/components/Tree';
import { useMessage } from '/@/hooks/web/useMessage';

const emit = defineEmits(['register', 'reload']);
const [registerModal, { closeModal, changeOkLoading }] = useModalInner(init);
const [registerOrgTree] = useModalInner();
const leftDropDownActions = ref([]);
const filterOption = (input: string, option: any) => {
  return option.fieldName.toLowerCase().indexOf(input.toLowerCase()) >= 0;
};
const id = ref('');
let selectKeysId = ref<any[]>([]);

const getTitle = computed(() => (!unref(id) ? '新建公共维度' : '编辑公共维度'));

const state = reactive({
  formRules: {
    popupWidth: [{ required: true, message: '类型不能为空', trigger: 'change' }],
  },
  addInfo: {
    DateModelType: 'Db',
    DataModelId: {},
    Name: '',
    DataType: 'string',
    Column: {},
  },
});
const { createMessage } = useMessage();
// const { dataForm, formRules } = toRefs(state);
// ---------数据源---------------
const treeLoading = ref(false);
const treeData = ref<TreeItem[]>([]);
// const emit = defineEmits(['cancel', 'reload']);
const formState = reactive({
  selectedModel: '', //选中模型
  targetColumn: undefined, //指标的列
  polymerization: undefined, //聚合方式
  format: '', //格式
});
// const formRef = ref<FormInstance>();
// 数据源信息-提交时用
const dataModelIdInfo = ref({});
const linkId = ref('');
const schemaName = ref('');
// ----------维度---------------------
// 维度（模型中表示细分维度的列）
const targetColumnArrWeidu = ref([]);
const targetCheckedArrWeidu = ref<any[]>([]);
const targetCheckedArrWeiduItem = ref<any[]>([]);
onMounted(() => { });

reloadTree();
function reloadTree() {
  treeLoading.value = true;
  getMetricSchema().then(res => {
    treeData.value = res.data;
    treeLoading.value = false;
  });
}
function init(data) {
  // 
  id.value = data.id;
  //根据ID获取详细信息
  if (id.value) {
    // 
    getDimensionDetail(data.id).then(res => {
      state.addInfo = {
        ...res.data,
        Column: res.data.column,
        DataModelId: res.data.dataModelId,
        Name: res.data.name,
      };
      //回显树形数据
      const selectKeysIdArr = ref<any[]>([]);
      selectKeysIdArr.value.push(res.data.dataModelId.id);
      selectKeysId.value = selectKeysIdArr.value;

      //回显depselect数据
      const targetCheckedArrWeiduArr = ref<any[]>([]);
      targetCheckedArrWeiduArr.value.push(res.data.column.fieldName);
      targetCheckedArrWeidu.value = targetCheckedArrWeiduArr.value;
    });
  } else {
    // 
    state.addInfo = {
      DateModelType: 'Db',
      DataModelId: {},
      Name: '',
      DataType: 'string',
      Column: {},
    };
    targetCheckedArrWeidu.value = [];
    selectKeysId.value = [];
  }
}
function handleTreeSelect(_id, node, _nodePath) {
  // 
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
  };
  getMetricLinkIdSchemaSchemaName(data).then(res => {
    // 
    const { tableFieldList } = res.data;
    // // 表结构列表
    // tableList.value = tableFieldList;
    // // 选中模型
    // formState.selectedModel = tableInfo.tableName;
    // // 指标的列
    // targetColumnArr.value = tableFieldList;
    // 维度
    targetColumnArrWeidu.value = tableFieldList;
  });
}
function depSelectItemEmitsFun(_val, node) {
  targetCheckedArrWeiduItem.value = node;
  // const nameArr = ref([]);
  // val.map(item => {
  //   nameArr.value.push(item.fieldName);
  // });
  state.addInfo.Name = node.fieldName;
}
async function handleSubmit() {
  state.addInfo.Column = targetCheckedArrWeiduItem.value;
  state.addInfo.DataModelId = dataModelIdInfo.value;
  // const values = await addDimension(addInfo);
  if (!state.addInfo) return;
  changeOkLoading(true);
  const query = {
    ...state.addInfo,
    id: id.value,
  };
  const formMethod = id.value ? updateDimension : addDimension;
  formMethod(query)
    .then(res => {
      createMessage.success(res.msg);
      changeOkLoading(false);
      // organizeStore.resetState();
      closeModal();
      setTimeout(() => {
        emit('reload');
      }, 300);
    })
    .catch(() => {
      changeOkLoading(false);
    });
}
</script>
<style lang="less" scoped>
:deep(.scrollbar__view) {
  padding: 0 !important;
}

.page-content-wrapper-left {
  margin-right: 0px !important;
  border-right: 1px solid #d9d9d9;
  height: 500px;
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
  height: 500px;
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
  // border: 1px solid red;
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
}

.spanClass::after {
  content: ',';
}

.spanClass:last-child::after {
  content: '';
}
</style>
