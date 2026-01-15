<template>
  <div class="edit-form-wrapper">
    <div class="bg-white edit-form-header">
      <a-tabs v-model:activeKey="state.currTab" @change="changeTab">
        <a-tab-pane :key="item.key" :tab="item.name" v-for="item in state.tabs"></a-tab-pane>
      </a-tabs>
    </div>
    <div class="edit-form-content">
      <a-form layout="vertical" ref="formRef" :model="state.form" :colon="false" v-bind="formTailLayout">
        <!-- 指标数据 -->
        <template v-if="state.currTab === NodeTabsEnum.Data">
          <a-form-item label="父节点" v-if="state.form.parentId === '-1'">
            <a-tree-select
              v-model:value="state.form.gotParentId"
              show-search
              style="width: 100%"
              :dropdown-style="{ maxHeight: '400px', overflow: 'auto' }"
              placeholder="请选择父节点"
              allow-clear
              tree-default-expand-all
              :tree-data="state.parentOptions"
              :field-names="{
                children: 'children',
                label: 'name',
                value: 'id',
              }"
              tree-node-filter-prop="name"
              @change="handleGotParentChange">
              <template #title="{ name }">
                {{ name }}
              </template>
            </a-tree-select>
          </a-form-item>
          <a-form-item label="名称">
            <a-input v-model:value="state.form.name" placeholder="请输入名称" @blur="updateItem" />
          </a-form-item>
          <a-form-item label="指标">
            <jnpf-select
              v-model:value="state.form.metricId"
              :options="state.metrics"
              showSearch
              allowClear
              :fieldNames="{ label: 'name', value: 'id' }"
              @change="metricChange" />
          </a-form-item>
        </template>
        <!-- 创建分级 -->
        <Grading v-if="state.currTab == NodeTabsEnum.Grading" :node-form="state.form" />
        <!-- 创建通知 -->
        <template v-if="state.currTab == NodeTabsEnum.Notice">
          <a-form-item label="消息模版">
            <jnpf-select
              v-model:value="state.noticeTemplate.templateId"
              :options="state.metricsNoticeTemplateOptions"
              showSearch
              allowClear
              :fieldNames="{ label: 'name', value: 'id' }"
              @change="handleNoticeChange" />
          </a-form-item>
          <a-form-item label="消息列表" v-if="state.metricNoticeList.length > 0">
            <a-list item-layout="horizontal" :data-source="state.metricNoticeList">
              <template #renderItem="{ item }">
                <a-list-item>
                  <template #actions>
                    <DeleteOutlined class="cursor-pointer deleteRed" @click="delItem(item.id)" />
                  </template>
                  <div class="flex items-center">
                    {{ getMetricNoticeName(item.templateId) }}
                  </div>
                </a-list-item>
              </template>
            </a-list>
          </a-form-item>
        </template>
      </a-form>
    </div>
  </div>
</template>
<script lang="ts" setup>
  import { reactive, watch, createVNode } from 'vue';
  import { props as _props } from './props';
  import Grading from './Grading.vue';
  import {
    updateIndicatorValueChain,
    getAllIndicatorList,
    getMetriccovruleList,
    getMetricData,
    getMetricNoticeTemplateList,
    postMetricNotice,
    deleteMetricNotice,
    getmetricNotice,
    postMetricData,
    getMetriccovSelector,
  } from '/@/api/createModel/model';
  import { GotTypeEnum } from '/@/enums/publicEnum';
  import { ResultEnum } from '/@/enums/httpEnum';
  import { message, Modal, List as AList, ListItem as AListItem, TreeSelect as ATreeSelect } from 'ant-design-vue';
  import { NodeTabsEnum, NoticeSourceEnum } from './const';
  import { DeleteOutlined, ExclamationCircleOutlined } from '@ant-design/icons-vue';
  import { useI18n } from '/@/hooks/web/useI18n';

  const { t } = useI18n();

  defineOptions({
    name: 'ZEditorForm',
  });

  const emits = defineEmits(['delete', 'update']);
  const props = defineProps(_props);

  const state = reactive<any>({
    form: {},
    ruleList: [],
    elements: [],
    currTab: 4,
    tabs: [
      // { key: 1, name: '建模' },
      // { key: 2, name: '预警' },
      // { key: 3, name: '基线' },
      { key: NodeTabsEnum.Data, name: '指标数据' },
      { key: NodeTabsEnum.Grading, name: '创建分级' },
      { key: NodeTabsEnum.Notice, name: '创建通知' },
    ],
    metricsNoticeTemplateOptions: [],
    statusOptions: props.statusOptions || [],
    // 通知模版item
    noticeTemplate: {
      // 消息模版ID
      templateId: '',
      // 来源
      source: '',
    },
    // 规则Id
    ruleId: '',
    // 当前规则
    currentRule: {},
    metricsNoticeSourceOptions: [
      {
        label: '节点',
        value: NoticeSourceEnum.Node,
      },
      {
        label: '规则',
        value: NoticeSourceEnum.Rule,
      },
    ],
    metricNoticeList: [],
    parentOptions: [],
  });

  /**
   * @description 是否继续执行
   * */
  const isRun = () => {
    if (!state.form.id) {
      message.warning('请先锁定目标！');
      throw new Error('请先锁定目标！');
    }
  };

  /**
   * @deascription 初始化加载数据
   */
  const initFetch = () => {
    // 指标列表
    getAllIndicatorList().then(res => {
      state.metrics = res.data;
    });

    // 通知模版
    getMetricNoticeTemplateList().then(res => {
      if (res.code === ResultEnum.SUCCESS) {
        state.metricsNoticeTemplateOptions = res.data;
      }
    });

    // 获取父节点
    getMetriccovSelector().then(res => {
      if (res.code === ResultEnum.SUCCESS) {
        state.parentOptions = res.data;
      }
    });
  };

  initFetch();

  /**
   * @description 表单布局
   * */
  const formTailLayout = {
    labelCol: { style: { width: '110px' } },
    // wrapperCol: { offset: 4 },
  };

  /**
   * @description 指标变化
   * @param {string} _value
   * @param {object} data 当前指标数据
   */
  const metricChange = async (_value, data) => {
    if (!data || !data.id) return;

    state.form.metricId = data.id;
    state.form.metricName = data.name;

    const loadingChildren = [];
    // 查询当前数据
    try {
      const metricInfo = await getMetricData(data.id);
      if (metricInfo.code === ResultEnum.SUCCESS) {
        state.form.currentValue = metricInfo.data.data.data;
      } else {
        throw new Error(metricInfo.msg);
      }
    } catch (err: any) {
      message.error(err.message);
    }

    // 查询折线数据
    try {
      const metricData = await postMetricData({ metricId: data.id, limit: 20 });
      if (metricData.code === ResultEnum.SUCCESS) {
        const list = metricData.data.data.data;
        const trendData: { item: string; value: number | string }[] = [];
        for (let i = 0; i < list.length; i++) {
          const [item, value] = list[i];
          trendData.push({ item, value });
        }
        state.form.trendData = trendData;
        state.form.metricGrade = metricData.data.metric_grade;
      } else {
        throw new Error(metricData.msg);
      }
    } catch (err: any) {
      message.error(err.message);
    }

    if (loadingChildren.length > 0) {
      Modal.confirm({
        title: t('common.tipTitle'),
        icon: createVNode(ExclamationCircleOutlined),
        centered: true,
        content: '是否加载子节点',
        onOk() {
          // 查询节点数据
          // fetch...
          state.form.isRunChildren = true;
          state.form.loadingChildren = loadingChildren;
          updateItem();
        },
        onCancel() {
          state.form.isRunChildren = false;
          state.form.loadingChildren = null;
          updateItem();
        },
      });
    } else {
      state.form.isRunChildren = false;
      state.form.loadingChildren = null;
      updateItem();
    }
  };

  /**
   * @description 选择父节点变化
   * @param {String} value
   * */
  const handleGotParentChange = () => {
    updateItem();
  };

  /**
   * @description: 删除
   */
  const delItem = (id?: string) => {
    isRun();

    if (state.currTab === NodeTabsEnum.Data) {
      emits('delete');
    }

    if (state.currTab === NodeTabsEnum.Notice) {
      Modal.confirm({
        title: t('common.tipTitle'),
        icon: createVNode(ExclamationCircleOutlined),
        centered: true,
        content: t('common.delTip'),
        onOk() {
          return deleteMetricNotice(id).then(res => {
            if (res.code === ResultEnum.SUCCESS) {
              message.success(res.msg);
              getMetricNoticeList(state.currentRule?.id);
              state.noticeTemplate.templateId = '';
            } else {
              message.error(res.msg);
            }
          });
        },
        onCancel() {},
      });
    }
  };

  /**
   * @description 更新指标数据
   */
  const updateItem = () => {
    isRun();

    if (state.currTab === NodeTabsEnum.Data) {
      const params = {
        name: state.form.name,
        gotType: GotTypeEnum.cov,
        gotId: state.form.gotId,
        metricId: state.form.metricId,
        currentValue: state.form.currentValue,
        is_root: state.form.is_root,
        level: '',
        iv: 0,
        tv: state.form.tv,
        parentId: state.form.parentId,
        covTreeId: '',
        status: state.form.status,
        id: state.form.id,
        gotParentId: state.form.gotParentId,
      };
      updateIndicatorValueChain(params).then(res => {
        if (res.code === ResultEnum.SUCCESS) {
          emits('update', state.form);
          message.success(res.msg);
        } else {
          message.error(res.msg);
        }
      });
    }

    if (state.currTab === NodeTabsEnum.Notice) {
      const params = {
        type: state.currentRule?.id ? NoticeSourceEnum.Rule : NoticeSourceEnum.Node,
        nodeId: state.form.id,
        ruleId: state.currentRule?.id,
        templateId: state.noticeTemplate.templateId,
      };
      postMetricNotice(params).then(res => {
        if (res.code === ResultEnum.SUCCESS) {
          message.success(res.msg);
          state.noticeTemplate.templateId = '';
          getMetricNoticeList(state.currentRule?.id);
        } else {
          message.error(res.msg);
        }
      });
    }
  };

  /**
   * @description 切换tab
   * @param {number} val 5 === 创建规则
   * @param {number} val 6 === 创建通知
   */
  const changeTab = val => {
    isRun();

    if (val === NodeTabsEnum.Grading) {
      getMetricCovRuleListData();
    }

    if (val === NodeTabsEnum.Notice) {
      state.noticeTemplate = {
        templateId: '',
        source: NoticeSourceEnum.Node,
      };
      state.currentRule = {};
      getMetricNoticeList();
    }
  };

  /**
   * @description 查询节点规则列表
   */
  const getMetricCovRuleListData = async () => {
    try {
      const res = await getMetriccovruleList(state.form.id);
      if (res.code === ResultEnum.SUCCESS) {
        state.ruleList = res.data;
      } else {
        throw new Error(res.msg);
      }
    } catch (err: any) {
      message.error(err.message);
    }
  };

  watch(
    () => props.form,
    val => {
      state.form = val;
      changeTab(state.currTab);
    },
    {
      immediate: true,
    },
  );

  /**
   * @description 查询节点或者规则通知模版列表
   */
  const getMetricNoticeList = (ruleId?) => {
    const params = {
      nodeId: state.form.id,
      ruleId,
      currentPage: 1,
    };
    getmetricNotice(params).then(res => {
      if (res.code === ResultEnum.SUCCESS) {
        state.metricNoticeList = res.data.list;
      }
    });
  };

  /**
   * @description 查询通知模版名称
   * */
  const getMetricNoticeName = templateId => {
    const item = state.metricsNoticeTemplateOptions.find(res => res.id === templateId);
    let name = '';
    if (item) {
      name = item.name;
    }
    return name;
  };

  /**
   * @description 是否进行通知
   */
  const handleNoticeChange = () => {
    Modal.confirm({
      title: '是否通知',
      icon: '',
      centered: true,
      content: '是否为状态变化设置通知?',
      onOk() {
        return updateItem();
      },
      onCancel() {},
    });
  };
</script>
<style lang="less" scoped>
  .edit-form-wrapper {
    display: flex;
    flex-direction: column;
    height: 100%;

    .edit-form-content {
      flex: 1;
      overflow: auto;
    }
  }

  .overflow-auto {
    overflow: auto;
  }

  .buttonContainer {
    display: flex;
    justify-content: space-between;
    padding: 0 10px;
    box-sizing: border-box;
  }

  .switchBox {
    display: flex;
    flex-direction: row;
    padding: 0 !important;
    align-items: center;
  }

  .deleteRed {
    color: #f5222d;
  }
</style>
