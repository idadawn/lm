<template>
  <div class="page-content-wrapper">
    <div class="page-content-wrapper-center">
      <div class="page-content-wrapper-content">
        <BasicTable @register="registerTable">
          <template #tableTitle>
            <a-button type="primary" preIcon="icon-ym icon-ym-btn-add" @click="addOrUpdateHandle()">添加</a-button>
          </template>
          <template #bodyCell="{ column, record }">
            <template v-if="column.key === 'valueType'">
              <div>{{ valueTypeFun(record.valueType) }}</div>
            </template>
            <template v-if="column.key === 'action'">
              <TableAction :actions="getTableActions(record)" />
            </template>
          </template>
        </BasicTable>
      </div>
    </div>
  </div>

  <a-modal width="600px" v-model:visible="state.add_visible" :title="addEditTitle" @ok="handleOk">
    <a-form
      :model="formState"
      name="basic"
      :label-col="{ span: 4 }"
      :wrapper-col="{ span: 16 }"
      autocomplete="off"
      style="margin-top: 20px"
      :rules="rules"
      ref="formRef">
      <a-form-item label="名称" name="name">
        <a-input v-model:value="formState.name" placeholder="请输入名称" />
      </a-form-item>
      <a-form-item label="描述" name="description">
        <a-input v-model:value="formState.description" placeholder="请输入描述" />
      </a-form-item>

      <template v-for="(element, index) in formState.elements">
        <z-temp-field
          itemsProps="elements"
          :items="formState.elements"
          :item="element"
          typeProps="t"
          labelProps="title"
          keyProps="v"
          listProps="options"
          listLabelProps="title"
          listValueProps="v"
          listValueType="index"
          requiredProps="isReq"
          :formItemName="['elements', index, 'v']"
          :childFormItemName="['options', 'childIndex', 'items']"
          @change="changeItem"
          :params="index" />
      </template>
    </a-form>
  </a-modal>
  <a-modal width="600px" v-model:visible="state.tag_visible" title="修改值" @ok="handleOk_tag">
    <a-form
      :model="formState_tag"
      name="basic"
      :label-col="{ span: 4 }"
      :wrapper-col="{ span: 16 }"
      autocomplete="off"
      style="margin-top: 20px"
      :rules="rules_tag">
      <a-form-item label="值" name="val">
        <a-input v-model:value="formState_tag.value" placeholder="请输入值" />
      </a-form-item>
    </a-form>
  </a-modal>
  <a-modal width="1000px" v-model:visible="state.history_visible" title="标签历史曲线值" :footer="null">
    <echartsLine :dataEchart="dataEchart"></echartsLine>
  </a-modal>
  <a-modal width="1000px" v-model:visible="state.history_visible_table" title="标签历史表格值" :footer="null">
    <historyTable :dataTable="dataTable"></historyTable>
  </a-modal>
</template>

<script lang="ts" setup>
  import { ref, reactive } from 'vue';
  import { BasicTable, useTable, TableAction, BasicColumn, ActionItem } from '/@/components/Table';
  import {
    tagPage,
    tagConfigTemplate,
    tagAdd,
    tagUpdate,
    tagRemove,
    tagDetail,
    tagSetValue,
    tagHistory,
  } from '/@/api/collector';
  import { Modal as AModal } from 'ant-design-vue';
  import { useMessage } from '/@/hooks/web/useMessage';
  import { ZTempField } from '/@/components/ZTempField';
  import { useRouter } from 'vue-router';
  import echartsLine from './components/echartsLineArea/index.vue';
  import historyTable from './components/historyTable/index.vue';
  import * as dayjs from 'dayjs';

  defineOptions({
    name: 'tag',
  });
  interface FormState {
    tagId?: number | string;
    name: string;
    description: string;
    elements: any[];
    props: {
      key: null;
      title: null;
      des: null;
      groups: null;
    };
  }

  const { createMessage } = useMessage();
  const router = useRouter();
  const formRef = ref();
  const addEditTitle = ref('添加');
  const state = reactive<any>({
    add_visible: false,
    tag_visible: false,
    history_visible: false,
    history_visible_table: false,
    parentId: router.currentRoute.value.params.id,
  });
  const dataEchart = reactive<any>({
    id: '',
    date: [],
    title: '',
    xAxisData: [],
    seriesData: [],
  });
  const dataTable = reactive<any>({
    id: '',
  });
  const formState = reactive<FormState>({
    tagId: '',
    name: '',
    description: '',
    elements: [],
    props: {
      key: null,
      title: null,
      des: null,
      groups: null,
    },
  });
  const rules = {
    name: [{ required: true, message: '请输入名称' }],
  };
  const formState_tag = reactive({
    tagId: '',
    value: '',
  });
  const rules_tag = {
    value: [{ required: true, message: '请输入值' }],
  };

  // 获取配置模板
  async function tagConfigTemplateFun() {
    const { data: res } = await tagConfigTemplate({ id: state.parentId });
    formState.elements = res.schema.items;
    formState.props.key = res.schema.key;
    formState.props.title = res.schema.title;
    formState.props.des = res.schema.des;
    formState.props.groups = res.schema.groups;
  }

  // const changeItem = () => {
  //   console.log('changeItem');
  // };

  // -------------------
  const columns: BasicColumn[] = [
    { title: 'ID', dataIndex: 'id', width: 80 },
    { title: '名称', dataIndex: 'name', width: 80 },
    { title: '描述', dataIndex: 'description', width: 90 },
    {
      title: '状态',
      dataIndex: 'state',
      width: 60,
      customRender: ({ record }) => {
        if (record.state === -1) {
          return '禁用';
        } else if (record.state === 0) {
          return '正常';
        } else if (record.state === 1) {
          return '断开';
        } else if (record.state === 2) {
          return '错误';
        }
      },
    },
    {
      title: '可写',
      dataIndex: 'writeable',
      width: 60,
      customRender: ({ record }) => {
        if (record.writeable) {
          return '√';
        } else {
          return '×';
        }
      },
    },
    {
      title: '值',
      dataIndex: 'value',
      width: 100,
    },
    {
      title: '值类型',
      dataIndex: 'valueType',
      width: 80,
    },
  ];
  const [registerTable, { reload }] = useTable({
    api: tagPage,
    columns,
    useSearchForm: false, //是否开启搜索
    searchInfo: {
      id: state.parentId,
    },
    actionColumn: {
      width: 120,
      title: '操作',
      dataIndex: 'action',
    },
    afterFetch: function (data) {},
  });
  // 处理值类型
  function valueTypeFun(type) {
    if (type === -1) {
      return 'Unkown';
    } else if (type === 0) {
      return 'Boolean(Bit)';
    } else if (type === 1) {
      return 'Byte';
    } else if (type === 2) {
      return 'Int16(Short)';
    } else if (type === 3) {
      return 'UInt16(UShort)';
    } else if (type === 4) {
      return 'Int32';
    } else if (type === 5) {
      return 'UInt32';
    } else if (type === 6) {
      return 'Single(Float)';
    } else if (type === 7) {
      return 'Int64(Long)';
    } else if (type === 8) {
      return 'UInt64(ULong)';
    } else if (type === 9) {
      return 'Double';
    } else if (type === 10) {
      return 'String';
    }
  }

  // 显示标签历史操作
  function valueTypeLabelFun(type) {
    const validValues = [-1, 0, 1, 10];
    const str = validValues.includes(type) ? '标签历史表格' : '标签历史曲线&表格';
    return str;
  }
  // 表格操作
  function getTableActions(record): ActionItem[] {
    return [
      {
        label: '编辑',
        onClick: addOrUpdateHandle.bind(null, record.id),
      },
      {
        label: '删除',
        color: 'error',
        modelConfirm: {
          onOk: handleDelete.bind(null, record.id),
        },
      },
      {
        label: `${valueTypeLabelFun(record.valueType)}`,
        onClick: handleHistory.bind(null, record.id, record),
      },
      {
        label: `${record.writeable ? '修改值' : ''}`,
        onClick: modifiedHandle.bind(null, record.id, record.value),
      },
    ];
  }
  // 添加or编辑
  function addOrUpdateHandle(id = '') {
    state.add_visible = true;
    // 重置
    formState.name = '';
    formState.description = '';
    formState.elements = [];
    if (id) {
      addEditTitle.value = '编辑';
      getDetail(id);
    } else {
      addEditTitle.value = '添加';
      // 获取配置模板
      tagConfigTemplateFun();
    }
  }
  // 获取详情
  const getDetail = async id => {
    let { data: res } = await tagDetail({ id: state.parentId, tagId: id });
    if (res) {
      formState.tagId = res.id;
      formState.name = res.name;
      formState.description = res.description;
      formState.elements = res.propConfigBuilder.schema.items;
    }
  };
  // 添加or编辑-提交
  const handleOk = async () => {
    formRef.value.validate().then(async () => {
      formState.elements.forEach(item => {
        if (item.options && item.options.length > 0) {
          item.vData = item.options[item.v];
          delete item.options;
        }
      });
      const itemObj = {
        id: Number(state.parentId),
        tagId: formState.tagId,
        name: formState.name,
        description: formState.description,
        props: {
          key: formState.props.key,
          title: formState.props.title,
          des: formState.props.des,
          groups: formState.props.groups,
          items: formState.elements,
        },
      };

      if (addEditTitle.value === '添加') {
        delete itemObj.tagId;
        let { success: res } = await tagAdd(itemObj);
        if (res) {
          state.add_visible = false;
          createMessage.success('添加成功');
          reload();
        }
      } else if (addEditTitle.value === '编辑') {
        let { success: res } = await tagUpdate(itemObj);
        if (res) {
          state.add_visible = false;
          createMessage.success('修改成功');
          reload();
        }
      }
    });
  };
  // 删除
  function handleDelete(id) {
    tagRemove(state.parentId, id).then(res => {
      if (res) {
        createMessage.success('删除成功');
        reload();
      }
    });
  }
  // 修改测试值
  function modifiedHandle(id, value) {
    state.tag_visible = true;
    formState_tag.tagId = id;
    formState_tag.value = value;
  }
  function handleOk_tag() {
    tagSetValue({
      id: state.parentId,
      tagId: formState_tag.tagId,
      value: formState_tag.value,
    }).then(res => {
      if (res) {
        state.tag_visible = false;
        reload();
      }
    });
  }
  // 标签历史
  function handleHistory(id, record) {
    console.log('record.valueType--', record.valueType);
    if (record.valueType != -1 && record.valueType != 0 && record.valueType != 1 && record.valueType != 10) {
      // console.log('曲线');
      state.history_visible = true;

      let start = dayjs().subtract(2, 'day').format('YYYY/MM/DD HH:mm:ss');
      let end = dayjs().format('YYYY/MM/DD HH:mm:ss');

      // console.log(start);
      // console.log(end);
      let data = {
        tagId: id,
        start: start,
        end: end,
      };
      tagHistory(data).then(res => {
        if (res.code === 200 && res.data.length > 0) {
          dataEchart.date = [start, end];
          dataEchart.id = id;
          dataEchart.title = res.data[0].name;
          dataEchart.xAxisData = Array.from(res.data, ({ timeStamp }) => dayjs(timeStamp).format('MM/DD HH:mm:ss'));
          dataEchart.seriesData = Array.from(res.data, ({ value }) => value);
        } else {
          dataEchart.date = [start, end];
          dataEchart.id = id;
          dataEchart.title = '';
          dataEchart.xAxisData = [];
          dataEchart.seriesData = [];
        }
      });
    } else {
      // console.log('表格');
      state.history_visible_table = true;
      dataTable.id = id;
      let start = dayjs().subtract(2, 'day').format('YYYY/MM/DD HH:mm:ss');
      let end = dayjs().format('YYYY/MM/DD HH:mm:ss');
      dataTable.date = [start, end];
    }
  }
</script>
<style lang="less" scoped></style>
