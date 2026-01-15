<template>
  <BasicModal
    v-bind="$attrs"
    @register="registerModal"
    title="高级查询"
    :width="800"
    okText="查询"
    @ok="handleSubmit"
    destroyOnClose
    class="jnpf-super-query-modal">
    <template #insertFooter>
      <a-space :size="10" class="float-left">
        <a-button @click="addPlan">保存方案</a-button>
        <a-popover placement="bottom" trigger="click" overlayClassName="plan-popover" v-model:visible="popoverVisible">
          <a-button>方案选择<DownOutlined /></a-button>
          <template #content>
            <div class="plan-list" v-if="planList.length">
              <div class="plan-list-item" v-for="(item, i) in planList" :key="i" @click="selectPlan(item)">
                <p class="plan-list-name">{{ item.fullName }} </p>
                <i class="icon-ym icon-ym-nav-close" @click.stop="delPlan(item.id, i)"></i>
              </div>
            </div>
            <div class="noData-txt" v-else>暂无数据</div>
          </template>
        </a-popover>
      </a-space>
    </template>
    <div class="super-query-main">
      <template v-if="conditionList.length">
        <div class="matchLogic">
          <span>匹配逻辑：</span>
          <jnpf-select v-model:value="state.matchLogic" :options="matchLogicOptions" />
        </div>
        <div>
          <a-row class="condition-list" :key="index" :gutter="20" v-for="(item, index) in conditionList">
            <a-col :span="8">
              <jnpf-select
                v-model:value="item.field"
                :options="fieldOptions"
                placeholder="请选择查询字段"
                showSearch
                allowClear
                class="w-full"
                :fieldNames="{ options: 'options1' }"
                @change="(val, data) => onFieldChange(val, data, item)" />
            </a-col>
            <a-col :span="4">
              <jnpf-select v-model:value="item.symbol" :options="symbolOptions" class="w-full" />
            </a-col>
            <a-col :span="8">
              <template v-if="item.jnpfKey === 'inputNumber'">
                <jnpf-input-number v-model:value="item.fieldValue" placeholder="请输入" :precision="item.attr.precision" />
              </template>
              <template v-else-if="item.jnpfKey === 'calculate'">
                <a-input-number v-model:value="item.fieldValue" placeholder="请输入" :precision="2" />
              </template>
              <template v-else-if="['rate', 'slider'].includes(item.jnpfKey)">
                <a-input-number v-model:value="item.fieldValue" placeholder="请输入" />
              </template>
              <div v-else-if="item.jnpfKey === 'switch'" class="pt-3px">
                <jnpf-switch v-model:value="item.fieldValue" />
              </div>
              <template v-else-if="item.jnpfKey === 'timePicker'">
                <jnpf-time-picker v-model:value="item.fieldValue" :format="item.attr.format" allowClear />
              </template>
              <template v-else-if="['datePicker', 'createTime', 'modifyTime'].includes(item.jnpfKey)">
                <jnpf-date-picker v-model:value="item.fieldValue" :format="item.attr.format || 'YYYY-MM-DD HH:mm:ss'" allowClear />
              </template>
              <template v-else-if="['organizeSelect', 'currOrganize'].includes(item.jnpfKey)">
                <jnpf-organize-select v-model:value="item.fieldValue" allowClear />
              </template>
              <template v-else-if="['depSelect'].includes(item.jnpfKey)">
                <jnpf-dep-select v-model:value="item.fieldValue" allowClear :selectType="item.attr.selectType" :ableDepIds="item.attr.ableDepIds" />
              </template>
              <template v-else-if="item.jnpfKey === 'roleSelect'">
                <jnpf-role-select v-model:value="item.fieldValue" allowClear />
              </template>
              <template v-else-if="item.jnpfKey === 'groupSelect'">
                <jnpf-group-select v-model:value="item.fieldValue" allowClear />
              </template>
              <template v-else-if="item.jnpfKey === 'posSelect'">
                <jnpf-pos-select
                  v-model:value="item.fieldValue"
                  allowClear
                  :selectType="item.attr.selectType"
                  :ableDepIds="item.attr.ableDepIds"
                  :ablePosIds="item.attr.ablePosIds" />
              </template>
              <template v-else-if="item.jnpfKey === 'currPosition'">
                <jnpf-pos-select v-model:value="item.fieldValue" allowClear />
              </template>
              <template v-else-if="['createUser', 'modifyUser'].includes(item.jnpfKey)">
                <jnpf-user-select v-model:value="item.fieldValue" allowClear />
              </template>
              <template v-else-if="['userSelect'].includes(item.jnpfKey)">
                <jnpf-user-select
                  v-model:value="item.fieldValue"
                  allowClear
                  :selectType="item.attr.selectType != 'all' && item.attr.selectType != 'custom' ? 'all' : item.attr.selectType"
                  :ableDepIds="item.attr.ableDepIds"
                  :ablePosIds="item.attr.ablePosIds"
                  :ableUserIds="item.attr.ableUserIds"
                  :ableRoleIds="item.attr.ableRoleIds"
                  :ableGroupIds="item.attr.ableGroupIds" />
              </template>
              <template v-else-if="['usersSelect'].includes(item.jnpfKey)">
                <jnpf-users-select v-model:value="item.fieldValue" allowClear :selectType="item.attr.selectType" :ableIds="item.attr.ableIds" />
              </template>
              <template v-else-if="item.jnpfKey === 'areaSelect'">
                <jnpf-area-select v-model:value="item.fieldValue" :level="item.attr.level" allowClear :key="item.cellKey" />
              </template>
              <template v-else-if="['select', 'radio', 'checkbox'].includes(item.jnpfKey)">
                <jnpf-select
                  v-model:value="item.fieldValue"
                  placeholder="请选择"
                  showSearch
                  allowClear
                  :options="item.attr.options"
                  :fieldNames="item.attr.props"
                  class="!w-full" />
              </template>
              <template v-else-if="item.jnpfKey === 'cascader'">
                <jnpf-cascader
                  v-model:value="item.fieldValue"
                  :options="item.attr.options"
                  :fieldNames="item.attr.props"
                  :showAllLevels="item.attr.showAllLevels"
                  showSearch
                  allowClear
                  placeholder="请选择" />
              </template>
              <template v-else-if="item.jnpfKey === 'treeSelect'">
                <jnpf-tree-select
                  v-model:value="item.fieldValue"
                  :options="item.attr.options"
                  :fieldNames="item.attr.props"
                  showSearch
                  allowClear
                  placeholder="请选择" />
              </template>
              <template v-else-if="item.jnpfKey === 'relationForm'">
                <jnpf-relation-form
                  v-model:value="item.fieldValue"
                  placeholder="请选择"
                  :modelId="item.attr.modelId"
                  allowClear
                  :columnOptions="item.attr.columnOptions"
                  :relationField="item.attr.relationField"
                  :hasPage="item.attr.hasPage"
                  :pageSize="item.attr.pageSize"
                  :popupType="item.attr.popupType"
                  :popupTitle="item.attr.popupTitle"
                  :popupWidth="item.attr.popupWidth" />
              </template>
              <template v-else-if="item.jnpfKey === 'popupSelect' || item.jnpfKey === 'popupTableSelect'">
                <jnpf-popup-select
                  v-model:value="item.fieldValue"
                  placeholder="请选择"
                  :interfaceId="item.attr.interfaceId"
                  allowClear
                  :templateJson="item.attr.templateJson"
                  :columnOptions="item.attr.columnOptions"
                  :propsValue="item.attr.propsValue"
                  :relationField="item.attr.relationField"
                  :hasPage="item.attr.hasPage"
                  :pageSize="item.attr.pageSize"
                  :popupType="item.attr.popupType"
                  :popupTitle="item.attr.popupTitle"
                  :popupWidth="item.attr.popupWidth" />
              </template>
              <template v-else-if="item.jnpfKey === 'autoComplete'">
                <jnpf-auto-complete
                  v-model:value="item.fieldValue"
                  placeholder="请输入"
                  allowClear
                  :interfaceId="item.attr.interfaceId"
                  :relationField="item.attr.relationField"
                  :templateJson="item.attr.templateJson"
                  :total="item.attr.total" />
              </template>
              <template v-else>
                <a-input v-model:value="item.fieldValue" placeholder="请输入" allowClear />
              </template>
            </a-col>
            <a-col :span="4" class="!flex justify-between">
              <a-button @click="addCondition"><i class="ym-custom ym-custom-plus"></i></a-button>
              <a-button @click="delCondition(index)"><i class="ym-custom ym-custom-minus"></i></a-button>
            </a-col>
          </a-row>
        </div>
      </template>
      <div class="query-noData" v-show="!conditionList.length">
        <img src="../../../assets/images/query-noData.png" class="noData-img" />
        <div class="noData-txt">
          <span>没有任何查询条件</span>
          <a-divider type="vertical"></a-divider>
          <span class="link-text" @click="addCondition">点击新增</span>
        </div>
      </div>
    </div>
    <BasicModal v-bind="$attrs" @register="registerFormModal" title="保存方案" @ok="handleFormSubmit">
      <BasicForm @register="registerForm" />
    </BasicModal>
  </BasicModal>
</template>
<script lang="ts" setup>
  import { reactive, toRefs, computed, unref, nextTick } from 'vue';
  import { BasicModal, useModal, useModalInner } from '/@/components/Modal';
  import { BasicForm, useForm } from '/@/components/Form';
  import { getAdvancedQueryList, delAdvancedQuery, create, update } from '/@/api/system/advancedQuery';
  import { getDictionaryDataSelector } from '/@/api/systemData/dictionary';
  import { getDataInterfaceRes } from '/@/api/systemData/dataInterface';
  import { useMessage } from '/@/hooks/web/useMessage';
  import { useI18n } from '/@/hooks/web/useI18n';
  import { DownOutlined } from '@ant-design/icons-vue';
  import { cloneDeep } from 'lodash-es';
  import { useRoute } from 'vue-router';
  import { dyOptionsList } from '/@/components/FormGenerator/src/helper/config';
  import { JnpfRelationForm } from '/@/components/Jnpf';
  import { isEmpty } from '/@/utils/is';

  interface State {
    planList: any[];
    conditionList: any[];
    popoverVisible: boolean;
    fieldOptions: any[];
    matchLogic: string;
  }
  const emit = defineEmits(['register', 'superQuery']);
  const [registerModal, { closeModal, changeLoading }] = useModalInner(init);
  const [registerFormModal, { openModal: openFormModal, closeModal: closeFormModal, setModalProps: setFormModalProps }] = useModal();
  const [registerForm, { resetFields, validate }] = useForm({
    labelWidth: 80,
    schemas: [
      {
        field: 'fullName',
        label: '方案名称',
        component: 'Input',
        componentProps: { placeholder: '请输入保存的方案名称', maxlength: 50 },
        rules: [{ required: true, trigger: 'blur', message: '请输入保存的方案名称' }],
      },
    ],
  });
  const { createMessage, createConfirm } = useMessage();
  const { t } = useI18n();
  const route = useRoute();
  const matchLogicOptions = [
    { id: 'AND', fullName: 'AND(所有条件都要求匹配)' },
    { id: 'OR', fullName: 'OR(条件中的任意一个匹配)' },
  ];
  const symbolOptions = [
    { id: '>=', fullName: '大于等于' },
    { id: '>', fullName: '大于' },
    { id: '==', fullName: '等于' },
    { id: '<=', fullName: '小于等于' },
    { id: '<', fullName: '小于' },
    { id: '<>', fullName: '不等于' },
    { id: 'like', fullName: '包含' },
    { id: 'notLike', fullName: '不包含' },
  ];
  const state = reactive<State>({
    planList: [],
    conditionList: [{ field: '', fieldValue: '', symbol: '==', jnpfKey: '', cellKey: +new Date(), attr: {} }],
    popoverVisible: false,
    fieldOptions: [],
    matchLogic: 'AND',
  });
  const { planList, popoverVisible, conditionList, fieldOptions } = toRefs(state);

  const getCurrMenuId = computed(() => (route.meta.modelId as string) || '');

  function init(data) {
    changeLoading(true);
    state.fieldOptions = cloneDeep(data.columnOptions).map(o => ({ ...o, disabled: false }));
    getPlanList();
  }
  function getPlanList() {
    if (!unref(getCurrMenuId)) return;
    getAdvancedQueryList(unref(getCurrMenuId)).then(res => {
      state.planList = res.data.list;
      changeLoading(false);
    });
  }
  function onFieldChange(val, data, item) {
    item.cellKey = +new Date();
    if (!val) {
      item.jnpfKey = '';
      item.fieldValue = undefined;
      item.attr = {};
      return;
    }
    item.jnpfKey = data.__config__?.jnpfKey || '';
    item.attr = data;
    const config = data.__config__;
    if (dyOptionsList.includes(config.jnpfKey)) {
      if (config.dataType === 'dictionary') {
        if (!config.dictionaryType) return;
        getDictionaryDataSelector(config.dictionaryType).then(res => {
          item.attr.options = res.data.list;
        });
      }
      if (config.dataType === 'dynamic') {
        if (!config.propsUrl) return;
        getDataInterfaceRes(config.propsUrl).then(res => {
          item.attr.options = Array.isArray(res.data) ? res.data : [];
        });
      }
    }
    item.fieldValue = undefined;
  }
  function addCondition() {
    const item = { field: '', fieldValue: '', symbol: '==', jnpfKey: '', cellKey: +new Date(), attr: {} };
    state.conditionList.push(item);
  }
  function delCondition(index) {
    state.conditionList.splice(index, 1);
  }
  function exist(type = '') {
    let isOk = true;
    for (let i = 0; i < state.conditionList.length; i++) {
      const e = state.conditionList[i];
      if (!e.field) {
        createMessage.error('请选择查询字段');
        isOk = false;
        break;
      }
      const flag = (!e.fieldValue && e.fieldValue !== 0) || isEmpty(e.fieldValue);
      if (flag) {
        const mes = type == 'query' ? '查询' : '保存';
        createMessage.error(`空条件不能${mes}`);
        isOk = false;
        break;
      }
    }
    return isOk;
  }
  function addPlan() {
    if (!state.conditionList.length) return createMessage.error('请添加条件');
    if (!exist()) return;
    openFormModal();
    nextTick(() => {
      resetFields();
    });
  }
  function delPlan(id, i) {
    delAdvancedQuery(id).then(res => {
      createMessage.success(res.msg);
      state.planList.splice(i, 1);
    });
  }
  function selectPlan(item) {
    state.matchLogic = item.matchLogic;
    state.conditionList = item.conditionJson ? JSON.parse(item.conditionJson) : [];
    state.popoverVisible = false;
  }
  async function handleFormSubmit() {
    const values = await validate();
    if (!values) return;
    const fullName = values.fullName;
    const boo = state.planList.some(o => o.fullName === fullName);
    if (!boo) return savePlan(fullName);
    const list = state.planList.filter(o => o.fullName === fullName);
    createConfirm({
      iconType: 'warning',
      title: t('common.tipTitle'),
      content: `${list[0].fullName}已存在, 是否覆盖方案?`,
      onOk: () => {
        savePlan(fullName, list[0].id);
      },
    });
  }
  function savePlan(fullName, id = '') {
    setFormModalProps({ confirmLoading: true });
    const query = {
      id,
      fullName,
      matchLogic: state.matchLogic,
      moduleId: unref(getCurrMenuId),
      conditionJson: JSON.stringify(state.conditionList),
    };
    const formMethod = query.id ? update : create;
    formMethod(query)
      .then(res => {
        closeFormModal();
        setFormModalProps({ confirmLoading: false });
        createMessage.success(res.msg);
        getPlanList();
      })
      .catch(() => {
        setFormModalProps({ confirmLoading: false });
      });
  }
  function handleSubmit() {
    if (!exist('query')) return;
    const query = {
      matchLogic: state.matchLogic,
      conditionJson: JSON.stringify(state.conditionList),
    };
    let str = JSON.stringify(query);
    if (!state.conditionList.length) str = '';
    emit('superQuery', str);
    closeModal();
  }
</script>
