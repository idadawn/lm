<template>
  <BasicDrawer
    class="full-drawer propPanel-drawer"
    @register="registerDrawer"
    :width="value && isConditionNode() ? 750 : 600"
    @visible-change="onDrawerVisibleChange"
    showFooter
    :maskClosable="false"
    @ok="handleOk">
    <template #title>
      <template v-if="value && (value.type == 'condition' || value.type == 'approver' || value.type == 'subFlow' || value.type == 'start')">
        <a-input v-model:value="properties.title" placeholder="请输入" class="!w-200px" />
      </template>
      <span v-else>{{ properties.title }}</span>
    </template>
    <!-- 发起节点配置 -->
    <StartNode ref="startRef" v-bind="getBindValue" :formConf="startForm" @updateFormFieldList="updateFormFieldList" v-if="value && isStartNode()" />
    <!-- 定时器节点配置 -->
    <TimerNode ref="timerRef" v-bind="getBindValue" :formConf="state.timerForm" v-if="value && isTimerNode()" />
    <!-- 条件节点配置 -->
    <ConditionNode ref="conditionRef" v-bind="getBindValue" :formConf="state.conditions" v-if="value && isConditionNode()" />
    <!-- 审批节点配置 -->
    <ApproverNode ref="approverRef" v-bind="getBindValue" :formConf="state.approverForm" v-if="value && isApproverNode()" />
    <!-- 子流程节点配置 -->
    <SubFlowNode ref="subFlowRef" v-bind="getBindValue" :formConf="state.subFlowForm" v-if="value && isSubFlowNode()" />
  </BasicDrawer>
</template>
<script lang="ts" setup>
  import { computed, reactive, ref, toRefs, watch, unref } from 'vue';
  import { NodeUtils } from '../helper/util';
  import nodeConfig from '../helper/config';
  import { cloneDeep, omit } from 'lodash-es';
  import { useMessage } from '/@/hooks/web/useMessage';
  import { BasicDrawer, useDrawer } from '/@/components/Drawer';
  import StartNode from './StartNode.vue';
  import TimerNode from './TimerNode.vue';
  import ConditionNode from './ConditionNode.vue';
  import ApproverNode from './ApproverNode.vue';
  import SubFlowNode from './SubFlowNode.vue';
  import { getInfo as getFormInfo } from '/@/api/workFlow/formDesign';
  import { getPrintDevSelector } from '/@/api/system/printDev';
  import { systemFieldOptions } from '../helper/define';

  interface State {
    properties: any;
    activeKey: string;
    startForm: any;
    timerForm: any;
    conditions: any;
    approverForm: any;
    subFlowForm: any;
    formFieldList: any[];
    printTplOptions: any[];
    prevNodeList: any[];
    isPrevNodeWithSubForm: boolean;
    nodeOptions: any[];
    beforeNodeOptions: any[];
  }
  interface ComType {
    getContent: () => any;
  }
  interface OperateNodeType extends ComType {
    updateCheckStatus: () => any;
  }

  const props = defineProps([/* 当前节点数据 */ 'value', /* 整个节点数据 */ 'processData', 'formInfo']);
  const emit = defineEmits(['cancel', 'confirm']);
  const { createMessage } = useMessage();
  const [registerDrawer, { openDrawer, closeDrawer }] = useDrawer();
  const state = reactive<State>({
    properties: {}, // 当前节点数据
    activeKey: '1',
    startForm: cloneDeep(nodeConfig.defaultStartForm),
    timerForm: {},
    conditions: [],
    approverForm: cloneDeep(nodeConfig.defaultApproverForm),
    subFlowForm: cloneDeep(nodeConfig.defaultSubFlowForm),
    formFieldList: [],
    printTplOptions: [],
    prevNodeList: [],
    isPrevNodeWithSubForm: false,
    nodeOptions: [],
    beforeNodeOptions: [],
  });
  const { properties, startForm } = toRefs(state);
  const startRef = ref<Nullable<OperateNodeType>>(null);
  const timerRef = ref<Nullable<ComType>>(null);
  const conditionRef = ref<Nullable<ComType>>(null);
  const approverRef = ref<Nullable<OperateNodeType>>(null);
  const subFlowRef = ref<Nullable<ComType>>(null);

  const flowType = computed(() => props.formInfo?.type || 0);
  // 所有表单字段
  const formFieldsOptions = computed(() => state.formFieldList.filter(o => o.__config__.jnpfKey !== 'table'));
  // 不包含子表的表单字段
  const usedFormItems = computed(() => unref(formFieldsOptions).filter(o => o.id.indexOf('-') < 0));
  // 流程时间及通知等用到表单字段
  const funcOptions = computed(() => {
    const options = [...systemFieldOptions, ...unref(formFieldsOptions)];
    return options.map(o => ({ ...o, label: o.fullName ? o.id + '(' + o.fullName + ')' : o.id }));
  });
  // 必填字段
  const funcRequiredOptions = computed(() => {
    const options = [...systemFieldOptions, ...unref(formFieldsOptions).filter(o => o.__config__ && o.__config__.required)];
    return options.map(o => ({ ...o, label: o.fullName ? o.id + '(' + o.fullName + ')' : o.id }));
  });
  const getBindValue = computed(() => ({
    ...props,
    formFieldList: state.formFieldList,
    printTplOptions: state.printTplOptions,
    flowType: unref(flowType),
    formFieldsOptions: unref(formFieldsOptions),
    usedFormItems: unref(usedFormItems),
    funcOptions: unref(funcOptions),
    funcRequiredOptions: unref(funcRequiredOptions),
    initFormOperates,
    updateAllNodeFormOperates,
    getFormFieldList,
    transformFormJson,
    transformFieldList,
    nodeOptions: state.nodeOptions,
    beforeNodeOptions: state.beforeNodeOptions,
    prevNodeList: state.prevNodeList,
  }));

  watch(
    () => props.value,
    val => {
      if (val && val.properties) {
        state.properties = cloneDeep(val.properties);
        openDrawer();
      }
    },
  );

  function getPrintTplList() {
    getPrintDevSelector('1').then(res => {
      state.printTplOptions = res.data.list.filter(o => o.children && o.children.length).map(o => ({ ...o, hasChildren: true }));
    });
  }
  // 初始化发起人节点数据
  function initStartNodeData() {
    let properties = cloneDeep(props.value.properties);
    Object.assign(state.startForm, properties);
    // 兼容旧数据
    state.formFieldList = state.startForm.formFieldList.map(o => (o.id ? o : { ...o, id: o.__vModel__, fullName: o.__config__.label }));
    unref(startRef)?.updateCheckStatus();
  }
  // 发起人节点确认保存
  function startNodeConfirm() {
    if (state.startForm.errorRule == 2 && !state.startForm.errorRuleUser.length) return createMessage.error('请选择异常处理人');
    const titleObj = { title: state.properties.title };
    Object.assign(state.properties, state.startForm, titleObj);
    const content = unref(startRef)?.getContent();
    emit('confirm', state.properties, content);
    closeDrawer();
  }
  // 初始化定时器节点数据
  function initTimerNodeData() {
    let properties = cloneDeep(props.value.properties);
    Object.assign(state.timerForm, properties);
  }
  // 定时器节点确认保存
  function timerNodeConfirm() {
    Object.assign(state.properties, state.timerForm);
    const content = unref(timerRef)?.getContent();
    emit('confirm', state.properties, content);
    closeDrawer();
  }
  // 初始化条件节点数据
  function initConditionNodeData() {
    getConditionNodeFieldList();
    // 初始化条件表单数据
    let nodeConditions = props.value.properties && props.value.properties.conditions;
    for (let i = 0; i < nodeConditions.length; i++) {
      for (let j = 0; j < unref(usedFormItems).length; j++) {
        if (nodeConditions[i].id === unref(usedFormItems)[j].id) {
          nodeConditions[i] = { ...nodeConditions[i], ...unref(usedFormItems)[j] };
        }
      }
    }
    state.conditions = cloneDeep(nodeConditions);
  }
  // 条件节点确认保存
  function conditionNodeConfirm() {
    if (!conditionExist()) return;
    state.properties.conditions = state.conditions;
    const content = unref(conditionRef)?.getContent();
    emit('confirm', state.properties, content || '请设置条件');
    closeDrawer();
  }
  // 初始化审批节点数据
  function initApproverNodeData() {
    state.isPrevNodeWithSubForm = false;
    let properties = cloneDeep(props.value.properties);
    if (!properties.formId) properties.formFieldList = props.processData.properties.formFieldList;
    // 兼容旧数据
    properties.formFieldList = properties.formFieldList.map(o => (o.id ? o : { ...o, id: o.__vModel__, fullName: o.__config__.label }));
    state.formFieldList = properties.formFieldList;
    state.approverForm.formOperates = initFormOperates(props.value);
    Object.assign(state.approverForm, properties);
    unref(approverRef)?.updateCheckStatus();
    getNodeOption();
    getPrevNodeOption();
  }
  // 审批节点确认保存
  function approverNodeConfirm() {
    if (!state.properties.title) return createMessage.error('请输入节点名称');
    const assigneeType = state.approverForm.assigneeType;
    if (
      assigneeType == 6 &&
      !state.approverForm.approverOrg.length &&
      !state.approverForm.approverRole.length &&
      !state.approverForm.approverPos.length &&
      !state.approverForm.approverGroup.length &&
      !state.approverForm.approvers.length
    ) {
      createMessage.error('请设置审批人');
      return;
    }
    if (assigneeType == 4 && !state.approverForm.formField) return createMessage.error('请选择表单字段');
    if (assigneeType == 5 && !state.approverForm.nodeId) return createMessage.error('请选择节点');
    if (assigneeType == 9 && !state.approverForm.getUserUrl) return createMessage.error('请输入接口路径');
    if (state.approverForm.hasAgreeRule && !state.approverForm.agreeRules.length) return createMessage.error('请选择同意规则配置');
    const titleObj = { title: state.properties.title };
    Object.assign(state.properties, state.approverForm, titleObj);
    const content = unref(approverRef)?.getContent();
    emit('confirm', state.properties, content || '请设置审批人');
    closeDrawer();
  }
  // 子流程审批节点数据
  function initSubFlowNodeData() {
    getNodeOption();
    getPrevNodeOption();
    let properties = cloneDeep(props.value.properties);
    Object.assign(state.subFlowForm, properties);
    const prevNode = state.prevNodeList[0];
    state.formFieldList = prevNode.properties.formId ? prevNode.properties.formFieldList : props.processData.properties.formFieldList;
    // 兼容旧数据
    state.formFieldList = state.formFieldList.map(o => (o.id ? o : { ...o, id: o.__vModel__, fullName: o.__config__.label }));
  }
  // 子流程节点确认保存
  function subFlowNodeConfirm() {
    if (!state.properties.title) return createMessage.error('请输入子流程名称');
    const initiateType = state.subFlowForm.initiateType;
    if (
      initiateType == 6 &&
      !state.subFlowForm.initiateOrg.length &&
      !state.subFlowForm.initiateRole.length &&
      !state.subFlowForm.initiatePos.length &&
      !state.subFlowForm.initiateGroup.length &&
      !state.subFlowForm.initiator.length
    ) {
      createMessage.error('请设置发起人');
      return;
    }
    if (initiateType == 4 && !state.subFlowForm.formField) return createMessage.error('请选择表单字段');
    if (initiateType == 5 && !state.subFlowForm.nodeId) return createMessage.error('请选择节点');
    if (initiateType == 9 && !state.subFlowForm.getUserUrl) return createMessage.error('请输入接口路径');
    if (!state.subFlowForm.flowId) return createMessage.error('请选择子流程表单');
    if (state.subFlowForm.errorRule == 2 && !state.subFlowForm.errorRuleUser.length) return createMessage.error('请选择异常处理人');
    const titleObj = { title: state.properties.title };
    Object.assign(state.properties, state.subFlowForm, titleObj);
    const content = unref(subFlowRef)?.getContent();
    emit('confirm', state.properties, content || '请设置发起人');
    closeDrawer();
  }
  const requiredDisabled = jnpfKey => {
    return ['billRule', 'createUser', 'createTime', 'modifyTime', 'modifyUser', 'currPosition', 'currOrganize', 'table'].includes(jnpfKey);
  };
  const getDataType = data => {
    if (!data.__config__ || !data.__config__.jnpfKey) return '';
    const jnpfKey = data.__config__.jnpfKey;
    if (['inputNumber', 'datePicker', 'rate', 'slider'].includes(jnpfKey)) {
      return 'number';
    } else if (['checkbox', 'uploadFile', 'uploadImg', 'cascader', 'organizeSelect', 'areaSelect'].includes(jnpfKey)) {
      return 'array';
    } else if (['select', 'treeSelect', 'depSelect', 'posSelect', 'userSelect', 'usersSelect', 'roleSelect', 'groupSelect'].includes(jnpfKey)) {
      if (data.multiple) return 'array';
    }
    return '';
  };
  function initFormOperates(target, isUpdate = false, isSameForm = false) {
    const formOperates = (target.properties && target.properties.formOperates) || [];
    let res: any[] = [];
    const getWriteById = id => {
      const arr = formOperates.filter(o => o.id === id);
      return arr.length ? arr[0].write : NodeUtils.isStartNode(target);
    };
    const getReadById = id => {
      const arr = formOperates.filter(o => o.id === id);
      return arr.length ? arr[0].read : true;
    };
    const getRequiredById = id => {
      const arr = formOperates.filter(o => o.id === id);
      return arr.length ? arr[0].required : false;
    };
    if (!formOperates.length || isUpdate) {
      for (let i = 0; i < state.formFieldList.length; i++) {
        const data = state.formFieldList[i];
        res.push({
          id: data.id,
          name: data.fullName,
          required: !isSameForm ? data.__config__.required : data.__config__.required || getRequiredById(data.id),
          requiredDisabled: requiredDisabled(data.__config__.jnpfKey) || data.__config__.required,
          jnpfKey: data.__config__.jnpfKey,
          dataType: getDataType(data),
          read: !isSameForm ? true : getReadById(data.id),
          write: !isSameForm ? NodeUtils.isStartNode(target) : getWriteById(data.id),
        });
      }
    } else {
      res = formOperates;
    }
    return res;
  }
  function updateAllNodeFormOperates(formFieldList, isSameForm = false) {
    const loop = data => {
      if (Array.isArray(data)) data.forEach(d => loop(d));
      if (data.type === 'approver' && !data.properties.formId) {
        data.properties.formOperates = initFormOperates(data, true, isSameForm);
        data.properties.formFieldList = formFieldList;
      }
      if (data.conditionNodes && Array.isArray(data.conditionNodes)) loop(data.conditionNodes);
      if (data.childNode) loop(data.childNode);
    };
    loop(props.processData);
  }
  function getFormFieldList(id, form, isSameForm = false) {
    getFormInfo(id).then(res => {
      const { formType = 1, propertyJson } = res.data;
      let formJson: any = {},
        fieldList: any = [];
      if (propertyJson) formJson = JSON.parse(propertyJson);
      if (formType == 1) {
        fieldList = transformFormJson(formJson);
      } else {
        fieldList = formJson.fields;
      }
      let list: any[] = transformFieldList(fieldList);
      state.formFieldList = list;
      state[form].formFieldList = list;
      state[form].formOperates = initFormOperates(props.value, true, isSameForm);
      // 更新所有没设置表单的节点的表单权限
      if (form === 'startForm') updateAllNodeFormOperates(list, isSameForm);
    });
  }
  function transformFormJson(list) {
    const fieldList = list.map(o => ({
      __config__: {
        label: o.filedName,
        jnpfKey: o.jnpfKey || '',
        required: o.required || false,
      },
      __vModel__: o.filedId,
      multiple: o.multiple || false,
    }));
    return fieldList;
  }
  function transformFieldList(formFieldList) {
    let list: any[] = [];
    const loop = (data, parent?) => {
      if (!data) return;
      if (data.__vModel__) {
        const isTableChild = parent && parent.__config__ && parent.__config__.jnpfKey === 'table';
        list.push({
          id: isTableChild ? parent.__vModel__ + '-' + data.__vModel__ : data.__vModel__,
          fullName: isTableChild ? parent.__config__.label + '-' + data.__config__.label : data.__config__.label,
          ...omit(data, ['__config__', 'on', 'style', 'options', 'props', 'templateJson', 'columnOptions', 'addTableConf', 'tableConf']),
          __config__: {
            label: data.__config__.label,
            jnpfKey: data.__config__.jnpfKey,
            required: data.__config__.required,
          },
        });
      }
      if (Array.isArray(data)) data.forEach(d => loop(d, parent));
      if (data.__config__ && data.__config__.children && Array.isArray(data.__config__.children)) {
        loop(data.__config__.children, data);
      }
    };
    loop(formFieldList);
    return list;
  }
  function updateFormFieldList(data) {
    state.formFieldList = data;
  }
  function getConditionNodeFieldList() {
    getPrevNodeOption();
    if (!state.prevNodeList.length) {
      state.formFieldList = [];
    } else {
      let prevNode = state.prevNodeList[0];
      let formFieldList =
        prevNode.properties.formFieldList && prevNode.properties.formFieldList.length
          ? prevNode.properties.formFieldList
          : props.processData.properties.formFieldList;
      state.formFieldList = formFieldList.filter(o => o.__config__.jnpfKey !== 'table');
      // 兼容旧数据
      state.formFieldList = state.formFieldList.map(o => (o.id ? o : { ...o, id: o.__vModel__, fullName: o.__config__.label }));
    }
  }
  // 获取上一节数据
  function getPrevNodeOption() {
    let prevNode = NodeUtils.getPreviousNode(props.value.prevId, props.processData);
    let node = cloneDeep(prevNode);
    delete node.childNode;
    let prevNodeList: any[] = [];
    const loop = nodeData => {
      if (nodeData.conditionNodes) {
        let hasCondition = nodeData.conditionNodes.some(o => o.nodeId === props.value.nodeId);
        if (hasCondition) return prevNodeList.push(nodeData);
      }
      if (nodeData.childNode) {
        loop(nodeData.childNode);
      } else if (nodeData.conditionNodes && !nodeData.childNode) {
        for (let c of nodeData.conditionNodes) {
          loop(c);
        }
      } else {
        prevNodeList.push(nodeData);
      }
    };
    loop(node);
    state.prevNodeList = prevNodeList;
    getPrevNodeRealList();
  }
  function getPrevNodeRealList() {
    const loop = data => {
      inner: for (let i = 0; i < data.length; i++) {
        if (['condition', 'subFlow', 'timer'].includes(data[i].type)) {
          if (data[i].type === 'subFlow') state.isPrevNodeWithSubForm = true;
          let prevNode = NodeUtils.getPreviousNode(data[i].prevId, props.processData);
          let node = cloneDeep(prevNode);
          delete node.childNode;
          let prevNodeList: any[] = [];
          const getPrevNodeAllList = nodeData => {
            if (nodeData.conditionNodes) {
              let hasCondition = nodeData.conditionNodes.some(o => o.nodeId === data[i].nodeId);
              if (hasCondition) return prevNodeList.push(nodeData);
            }
            if (nodeData.childNode) {
              getPrevNodeAllList(nodeData.childNode);
            } else if (nodeData.conditionNodes && !nodeData.childNode) {
              for (let c of nodeData.conditionNodes) {
                getPrevNodeAllList(c);
              }
            } else {
              prevNodeList.push(nodeData);
            }
          };
          getPrevNodeAllList(node);
          data.splice(i, 1, ...prevNodeList);
          loop(data);
          break inner;
        }
      }
    };
    loop(state.prevNodeList);
    state.prevNodeList = unique(state.prevNodeList, 'nodeId');
  }
  // 去重
  function unique(arr, attrName) {
    const res = new Map();
    // 根据对象的某个属性值去重
    return arr.filter(o => !res.has(o[attrName]) && res.set(o[attrName], 1));
  }
  // 条件字段验证
  function conditionExist() {
    let isOk = true;
    for (let i = 0; i < state.conditions.length; i++) {
      const e = state.conditions[i];
      if (!e.field) {
        createMessage.error('条件字段不能为空');
        isOk = false;
        break;
      }
      if (!e.symbol) {
        createMessage.error('比较不能为空');
        isOk = false;
        break;
      }
    }
    return isOk;
  }
  // 获取节点选项
  function getNodeOption() {
    let list: any[] = [];
    const loop = data => {
      if (Array.isArray(data)) data.forEach(d => loop(d));
      if (data.type === 'approver') list.push(data);
      if (data.conditionNodes && Array.isArray(data.conditionNodes)) loop(data.conditionNodes);
      if (data.childNode) loop(data.childNode);
    };
    loop(props.processData);
    let beforeNodeList: any[] = [];
    for (let i = 0; i < list.length; i++) {
      if (list[i].nodeId === props.value.nodeId) break;
      beforeNodeList.push(list[i]);
    }
    state.beforeNodeOptions = beforeNodeList.map(o => ({ id: o.nodeId, fullName: o.properties.title }));
    state.nodeOptions = list.filter(o => o.nodeId !== props.value.nodeId).map(o => ({ id: o.nodeId, fullName: o.properties.title }));
  }
  // 判断是否是发起节点
  function isStartNode() {
    return props.value ? NodeUtils.isStartNode(props.value) : false;
  }
  // 判断是否是定时器节点
  function isTimerNode() {
    return props.value ? NodeUtils.isTimerNode(props.value) : false;
  }
  // 判断是否是条件节点
  function isConditionNode() {
    return props.value ? NodeUtils.isConditionNode(props.value) : false;
  }
  // 判断是否是审批节点
  function isApproverNode() {
    return props.value ? NodeUtils.isApproverNode(props.value) : false;
  }
  //  判断是否是子流程节点
  function isSubFlowNode() {
    return props.value ? NodeUtils.isSubFlowNode(props.value) : false;
  }
  function onDrawerVisibleChange(val) {
    if (!val) {
      // 重置数据为默认状态
      state.startForm = cloneDeep(nodeConfig.defaultStartForm);
      state.approverForm = cloneDeep(nodeConfig.defaultApproverForm);
      state.subFlowForm = cloneDeep(nodeConfig.defaultSubFlowForm);
      emit('cancel');
      return;
    }
    isStartNode() && initStartNodeData();
    isTimerNode() && initTimerNodeData();
    isConditionNode() && initConditionNodeData();
    isApproverNode() && initApproverNodeData();
    isSubFlowNode() && initSubFlowNodeData();
    getPrintTplList();
  }
  function handleOk() {
    isStartNode() && startNodeConfirm();
    isTimerNode() && timerNodeConfirm();
    isConditionNode() && conditionNodeConfirm();
    isApproverNode() && approverNodeConfirm();
    isSubFlowNode() && subFlowNodeConfirm();
  }
</script>
