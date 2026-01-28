<template>
  <div class="page-content-wrapper">
    <div class="page-content-wrapper-center">
      <div class="page-content-wrapper-content">
        <BasicTable @register="registerTable">
          <template #tableTitle>
            <a-button type="primary" preIcon="icon-ym icon-ym-btn-add" @click="addOrUpdateHandle()">添加</a-button>
            <a-button type="primary" preIcon="icon-ym icon-ym-extend-save" @click="saveHandle()">保存配置</a-button>
            <a-button type="primary" preIcon="icon-ym icon-ym-btn-export" @click="exportHandle()">导出配置</a-button>
          </template>
          <template #bodyCell="{ column, record }">
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
      :label-col="{ span: 6 }"
      :wrapper-col="{ span: 14 }"
      autocomplete="off"
      style="margin-top: 20px"
      :rules="rules"
      ref="formRef">
      <a-form-item label="类型" name="type">
        <a-select ref="select" v-model:value="formState.type" @select="handleChange">
          <template v-for="item in type_options">
            <a-select-option :value="item">{{ item }}</a-select-option>
          </template>
        </a-select>
      </a-form-item>
      <a-form-item label="名称" name="name">
        <a-input v-model:value="formState.name" placeholder="请输入名称" />
      </a-form-item>
      <a-form-item label="描述" name="description">
        <a-input v-model:value="formState.description" placeholder="请输入描述" />
      </a-form-item>
      <template v-for="(element, index) in formState.elements">
        <z-temp-field
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
          @change="changeItem"
          :formItemName="['elements', index, 'v']"
          :childFormItemName="['options', 'childIndex', 'items']"
          :params="index" />
      </template>
    </a-form>
  </a-modal>
</template>

<script lang="ts" setup>
  import { ref, reactive } from 'vue';
  import { BasicTable, useTable, TableAction, BasicColumn, ActionItem } from '/@/components/Table';
  import {
    serviceDropDown,
    servicePage,
    serviceConfigTemplate,
    serviceAdd,
    serviceUpdate,
    serviceRemove,
    serviceDetail,
    serviceEnable,
    serviceDisable,
    systemSaveConfig,
    systemDownloadServiceConfig,
  } from '/@/api/service';
  import { Modal as AModal } from 'ant-design-vue';
  import type { SelectProps } from 'ant-design-vue';
  import { useMessage } from '/@/hooks/web/useMessage';
  import { ZTempField } from '/@/components/ZTempField';
  import { useRouter } from 'vue-router';

  defineOptions({
    name: 'service',
  });
  interface FormState {
    id?: number | string;
    type: string;
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
  });
  const type_options = ref<SelectProps['options']>([]);

  const formState = reactive<FormState>({
    id: '',
    type: '',
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

  // 切换类型
  const handleChange = (value: string) => {
    serviceConfigTemplateFun(value);
  };

  // 获取类型
  serviceDropDownFun();
  async function serviceDropDownFun() {
    const { data: res } = await serviceDropDown();
    type_options.value = res;
  }

  // 获取配置模板
  async function serviceConfigTemplateFun(data) {
    const { data: res } = await serviceConfigTemplate({ type: data });
    formState.elements = res.schema.items;
    formState.props.key = res.schema.key;
    formState.props.title = res.schema.title;
    formState.props.des = res.schema.des;
    formState.props.groups = res.schema.groups;
  }

  const changeItem = () => {
  };

  // -------------------
  const columns: BasicColumn[] = [
    { title: 'ID', dataIndex: 'id', width: 80 },
    { title: '名称', dataIndex: 'name', width: 80 },
    { title: '描述', dataIndex: 'description', width: 90 },
    {
      title: '状态',
      dataIndex: 'state',
      width: 80,
      customRender: ({ record }) => {
        if (record.state === -1) {
          return '禁用';
        } else if (record.state === 0) {
          return '正常';
        } else if (record.state === 1) {
          return '错误';
        }
      },
    },
    { title: '类型', dataIndex: 'type', width: 80 },
  ];
  const [registerTable, { reload }] = useTable({
    api: servicePage,
    columns,
    useSearchForm: false, //是否开启搜索
    actionColumn: {
      width: 100,
      title: '操作',
      dataIndex: 'action',
    },
    // afterFetch: function (data) {
    // 
    // },
  });
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
        label: `${record.state === 0 ? '禁用' : '启用'}`,
        onClick: handleState.bind(null, record),
      },
      {
        label: `${record.hasWatchList === true ? '观察列表' : ''}`,
        onClick: handleWatch.bind(null, record.id),
      },
    ];
  }
  // 添加or编辑
  function addOrUpdateHandle(id = '') {
    state.add_visible = true;
    // 重置
    formState.id = '';
    formState.type = '';
    formState.name = '';
    formState.description = '';
    formState.elements = [];

    if (id) {
      addEditTitle.value = '编辑';
      getDetail(id);
    } else {
      addEditTitle.value = '添加';
    }
  }
  // 获取详情
  const getDetail = async id => {
    let { data: res } = await serviceDetail({ id: id });
    if (res) {
      formState.id = res.id;
      formState.type = res.type;
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
        id: formState.id,
        type: formState.type,
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
        delete itemObj.id;
        let { success: res } = await serviceAdd(itemObj);
        if (res) {
          state.add_visible = false;
          createMessage.success('添加成功');
          reload();
        }
      } else if (addEditTitle.value === '编辑') {
        let { success: res } = await serviceUpdate(itemObj);
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
    serviceRemove(id).then(res => {
      if (res) {
        createMessage.success('删除成功');
        reload();
      }
    });
  }

  // 保存配置
  function saveHandle() {
    systemSaveConfig().then(res => {
      if (res) {
        createMessage.success('保存成功');
      }
    });
  }

  // 导出配置
  function exportHandle() {
    systemDownloadServiceConfig().then(res => {
      if (res) {
        // 下载导出json文件
        var a = document.createElement('a');
        a.download = '数据服务配置.json';
        a.style.display = 'none';
        var dat = JSON.stringify(res, null, 4);
        var blob = new Blob([dat], { type: 'Application/json' });
        a.href = URL.createObjectURL(blob);
        document.body.appendChild(a);
        a.click();
        document.body.removeChild(a);
      }
    });
  }

  // 禁用-1|启用0
  function handleState(record) {
    if (record.state === 0) {
      serviceDisable(record.id).then(res => {
        if (res) {
          createMessage.success('操作成功');
          reload();
        }
      });
    } else {
      serviceEnable(record.id).then(res => {
        if (res) {
          createMessage.success('操作成功');
          reload();
        }
      });
    }
  }
  // 观察列表
  function handleWatch(id) {
    router.push('/service/watch/' + id);
  }
</script>
<style lang="less" scoped></style>
