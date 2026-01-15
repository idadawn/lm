<template>
  <a-form-item label="默认值">
    <jnpf-cascader
      placeholder="请选择"
      v-model:value="activeData.__config__.defaultValue"
      :options="activeData.options"
      :fieldNames="activeData.props"
      :multiple="activeData.multiple"
      allowClear
      showSearch />
  </a-form-item>
  <a-divider>数据选项</a-divider>
  <a-form-item label=" " :labelCol="{ style: { width: '30px' } }">
    <jnpf-radio v-model:value="activeData.__config__.dataType" :options="dataTypeOptions" optionType="button" buttonStyle="solid" @change="onDataTypeChange" />
  </a-form-item>
  <div class="options-list" v-if="activeData.__config__.dataType === 'static'">
    <BasicTree ref="treeRef" :treeData="activeData.options" :actionList="actionList" />
    <div class="add-btn">
      <a-button type="link" preIcon="icon-ym icon-ym-btn-add" @click="addTreeItem" class="!px-0">添加父级</a-button>
      <a-divider type="vertical"></a-divider>
      <a-button type="link" @click="openModal(true, { options: activeData.options })" class="!px-0">批量编辑</a-button>
    </div>
  </div>
  <div v-if="activeData.__config__.dataType === 'dictionary'">
    <a-form-item label="数据字典">
      <jnpf-tree-select
        :options="dicOptions"
        v-model:value="activeData.__config__.dictionaryType"
        placeholder="请选择数据字典"
        lastLevel
        allowClear
        @change="onDictionaryTypeChange" />
    </a-form-item>
    <a-form-item label="存储字段">
      <jnpf-select v-model:value="activeData.props.value" placeholder="请选择存储字段" :options="valueOptions" @change="onChange" />
    </a-form-item>
  </div>
  <div v-if="activeData.__config__.dataType === 'dynamic'">
    <a-form-item label="远端数据">
      <interface-modal :value="activeData.__config__.propsUrl" :title="activeData.__config__.propsName" popupTitle="远端数据" @change="onPropsUrlChange" />
    </a-form-item>
    <a-form-item label="存储字段">
      <a-input v-model:value="activeData.props.value" placeholder="请输入存储字段" @change="onChange" />
    </a-form-item>
    <a-form-item label="显示字段">
      <a-input v-model:value="activeData.props.label" placeholder="请输入显示字段" @change="onChange" />
    </a-form-item>
    <a-form-item label="子级字段">
      <a-input v-model:value="activeData.props.children" placeholder="请输入子级字段" @change="onChange" />
    </a-form-item>
    <a-table
      :data-source="activeData.__config__.templateJson"
      :columns="columns"
      size="small"
      :pagination="false"
      v-if="activeData.__config__.templateJson.length">
      <template #bodyCell="{ column, record }">
        <template v-if="column.key === 'field'">
          <span class="required-sign">{{ record.required ? '*' : '' }}</span>
          {{ record.field }}{{ record.fieldName ? '(' + record.fieldName + ')' : '' }}
        </template>
        <template v-if="column.key === 'relationField'">
          <jnpf-select
            v-model:value="record.relationField"
            placeholder="请选择表单字段"
            :options="formFieldsOptions"
            allowClear
            showSearch
            :fieldNames="{ options: 'options1' }"
            class="!w-135px"
            @change="onRelationFieldChange($event, record)" />
        </template>
      </template>
    </a-table>
  </div>
  <a-divider></a-divider>
  <a-form-item label="能否清空">
    <a-switch v-model:checked="activeData.clearable" />
  </a-form-item>
  <a-form-item label="能否搜索" v-show="showType === 'pc'">
    <a-switch v-model:checked="activeData.filterable" />
  </a-form-item>
  <a-form-item label="能否多选">
    <a-switch v-model:checked="activeData.multiple" @change="onChange" />
  </a-form-item>
  <TreeNodeModal ref="treeNodeRef" @confirm="onTreeNodeConfirm" />
  <TreeBatchOperate @register="registerTreeBatchOperate" @confirm="onBatchOperateConfirm" />
</template>
<script lang="ts" setup>
  import { ref, unref, h } from 'vue';
  import { PlusOutlined, FormOutlined, DeleteOutlined } from '@ant-design/icons-vue';
  import { BasicTree, TreeActionItem, TreeActionType } from '/@/components/Tree/index';
  import TreeNodeModal from './components/TreeNodeModal.vue';
  import { useDynamic } from '../hooks/useDynamic';
  import { InterfaceModal } from '/@/components/CommonModal';
  import { useModal } from '/@/components/Modal';
  import TreeBatchOperate from './components/TreeBatchOperate.vue';

  defineOptions({ inheritAttrs: false });
  const props = defineProps(['activeData', 'dicOptions']);
  const cascaderKey = ref(+new Date());
  const treeRef = ref<Nullable<TreeActionType>>(null);
  const treeNodeRef = ref(null);
  const currentNode = ref<any>(null);

  const {
    showType,
    onDataTypeChange,
    onDictionaryTypeChange,
    onPropsUrlChange,
    dataTypeOptions,
    valueOptions,
    formFieldsOptions,
    onRelationFieldChange,
    onBatchOperateConfirm,
  } = useDynamic(props.activeData);
  const [registerTreeBatchOperate, { openModal }] = useModal();
  const columns = [
    { width: 50, title: '序号', align: 'center', customRender: ({ index }) => index + 1 },
    { title: '参数名称', dataIndex: 'field', key: 'field', width: 135 },
    { title: '表单字段', dataIndex: 'relationField', key: 'relationField', width: 135 },
  ];
  const actionList: TreeActionItem[] = [
    {
      render: node => {
        return h(PlusOutlined, {
          class: 'ml-4px',
          title: '添加',
          onClick: () => {
            handleAdd(node);
          },
        });
      },
    },
    {
      render: node => {
        return h(FormOutlined, {
          class: 'ml-4px',
          title: '编辑',
          onClick: () => {
            handleEdit(node);
          },
        });
      },
    },
    {
      render: node => {
        return h(DeleteOutlined, {
          class: 'ml-4px',
          title: '删除',
          onClick: () => {
            handleDelete(node);
          },
        });
      },
    },
  ];

  function getTree() {
    const tree = unref(treeRef);
    if (!tree) {
      throw new Error('tree is null!');
    }
    return tree;
  }
  function handleAdd(node: any) {
    getTree().getSelectedNode(node.id);
    const data = getTree().getSelectedNode(node.id) as any;
    if (!Reflect.has(data, 'children')) data.children = [];
    currentNode.value = data.children;
    (unref(treeNodeRef) as any).openModal();
  }
  function handleEdit(node: any) {
    getTree().getSelectedNode(node.id);
    const data = getTree().getSelectedNode(node.id) as any;
    currentNode.value = data;
    (unref(treeNodeRef) as any).openModal(data);
  }
  function addTreeItem() {
    (unref(treeNodeRef) as any).openModal();
    currentNode.value = props.activeData.options;
  }
  function onTreeNodeConfirm(data, isEdit) {
    if (!isEdit) return currentNode.value.push(data);
    getTree().updateNodeByKey(currentNode.value.id, data);
  }
  function handleDelete(node: any) {
    props.activeData.__config__.defaultValue = [];
    getTree().deleteNodeByKey(node.id);
  }
  function onChange() {
    cascaderKey.value = +new Date();
    props.activeData.__config__.renderKey = +new Date();
    props.activeData.__config__.defaultValue = [];
  }
</script>
