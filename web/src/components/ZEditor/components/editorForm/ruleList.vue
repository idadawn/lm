<template>
  <div class="rulesTitleBox">
    <a-select
      class="ruleStatus"
      v-model:value="ruleStatus"
      allow-clear
      show-search
      @change="handleStatusChange"
      placeholder="状态">
      <a-select-option v-for="item in statusOptions" :key="item.id" :value="item.id">
        {{ item.name }}
      </a-select-option>
    </a-select>
    <div class="rulesTitle">规则列表</div>
    <div class="ruleItem" v-for="item in currentList" :key="item.id">
      <div>
        <span v-if="item.operators === AddRuleTypeEnum.GreaterThan">
          {{ typeToValue(item.type) }} &gt;&#61;{{ item.value }}
        </span>
        <span v-if="item.operators === AddRuleTypeEnum.Between">
          {{ item.minValue }}&lt;&#61;
          <span>{{ typeToValue(item.type) }}</span>
          &lt;&#61;{{ item.maxValue }}
        </span>
        <span v-if="item.operators === AddRuleTypeEnum.LessThan">
          {{ typeToValue(item.type) }} &lt;&#61;{{ item.value }}
        </span>
        <span>
          状态:
          <span :style="{ color: item.statusColor }">{{ item.statusName }}</span>
        </span>
      </div>
      <div>
        <MessageOutlined class="cursor-pointer text-base operationBox" @click="createNotice(item)" />
        <EditOutlined class="cursor-pointer text-base pl-1 operationBox" @click="editRule(item)" />
        <DeleteOutlined class="cursor-pointer text-base pl-1 deleteRed" @click="showDeleteRuleConfirm(item)" />
      </div>
    </div>
    <div class="buttonBox">
      <a-button type="primary" @click="addRules">添加规则</a-button>
    </div>
  </div>
  <ruleForm @register="registerForm" @reload="$emit('reload', $event)" />
</template>
<script lang="ts" setup>
  import { toRefs, ref, watch, createVNode } from 'vue';
  import { EditOutlined, DeleteOutlined, ExclamationCircleOutlined, MessageOutlined } from '@ant-design/icons-vue';
  import { useModal } from '/@/components/Modal';
  import ruleForm from './ruleForm.vue';
  import { AddRuleTypeEnum, AddRuleThresholdEnum, OptTypeEnum } from './const';
  import { message, Modal } from 'ant-design-vue';
  import { deleteMetriccovrule } from '/@/api/createModel/model';
  import { find } from 'lodash-es';
  import { RuleType, RuleListType, MetricCovStatusOptionType } from '../../types/type';
  import { ResultEnum } from '/@/enums/httpEnum';
  import { useI18n } from '/@/hooks/web/useI18n';

  const { t } = useI18n();

  const [registerForm, { openModal: openFormModal }] = useModal();

  defineOptions({
    name: 'rulesForm',
  });

  const emits = defineEmits<{
    (e: 'reload', type: string): void;
    (e: 'noticeRule', item: RuleType): void;
  }>();

  const props = defineProps<{
    items: RuleListType;
    // 当前节点信息
    nodeForm: any;
    statusOptions: MetricCovStatusOptionType[];
  }>();

  /**
   * @description 所有的规则列表
   * */
  const { items: ruleList, statusOptions } = toRefs(props);

  /**
   * @description 筛选后的规则列表
   * */
  const currentList = ref<RuleListType>([]);

  /**
   * @description 创建规则筛选条件变量
   */
  const ruleStatus = ref();

  /**
   * @description 根据状态获取状态名称
   */
  const setStatusName = () => {
    if (statusOptions.value.length > 0) {
      (currentList.value || []).forEach(item => {
        const statusItem = find(statusOptions.value, { id: item.status }) as MetricCovStatusOptionType;
        if (statusItem) {
          item.statusName = statusItem.name;
          item.statusColor = statusItem.color;
        }
      });
    }
  };

  watch(
    () => ruleList.value,
    () => {
      handleStatusChange(ruleStatus.value);
      setStatusName();
    },
    {
      deep: true,
    },
  );

  watch(
    () => statusOptions.value,
    () => {
      handleStatusChange(ruleStatus.value);
      setStatusName();
    },
  );

  /**
   * @description 根据key获取value
   * @param {string | number} key
   */
  const typeToValue = (key: string) => {
    return AddRuleThresholdEnum[key];
  };

  /**
   * @description 打开规则表单
   */
  function addRules() {
    if (props.nodeForm.id) {
      openFormModal(true, { covId: props.nodeForm.id, id: '', statusOptions: statusOptions.value });
    } else {
      message.warning('请先锁定目标！');
    }
  }

  /**
   * @description 编辑规则
   * @param {RuleType} item
   */
  const editRule = (item: RuleType) => {
    openFormModal(true, { ...item, covId: props.nodeForm.id, statusOptions: statusOptions.value });
  };

  /**
   * type: string
   */
  const createNotice = (item: RuleType) => {
    emits('noticeRule', item);
  };

  /**
   * @description 显示删除规则确认框
   * @param {RuleType} item
   */
  const showDeleteRuleConfirm = (item: RuleType) => {
    Modal.confirm({
      title: t('common.tipTitle'),
      icon: createVNode(ExclamationCircleOutlined),
      centered: true,
      content: t('common.delTip'),
      onOk() {
        return deleteMetriccovrule(item.id).then(res => {
          if (res.code === ResultEnum.SUCCESS) {
            message.success(res.msg);
            emits('reload', OptTypeEnum.Delete);
          } else {
            message.error(res.msg);
          }
        });
      },
      onCancel() {},
    });
  };

  /**
   * @description 过滤
   */
  const handleStatusChange = (value?) => {
    if (value) currentList.value = ruleList.value.filter(item => item.status === value);
    else currentList.value = ruleList.value;
  };
</script>
<style lang="less" scoped>
  .rulesTitleBox {
    padding: 0 0 25px 0;
    box-sizing: border-box;
    .ruleStatus {
      width: 100%;
    }
    .rulesTitle {
      color: #000;
      font-size: 16px;
      padding-top: 15px;
      box-sizing: border-box;
    }
    .ruleItem {
      padding: 10px 0 0 8px;
      box-sizing: border-box;
      display: flex;
      flex-direction: row;
      justify-content: space-between;
    }
    .buttonBox {
      cursor: pointer;
      margin-top: 30px;
      display: flex;
      justify-content: center;
    }
    .operationBox {
      color: #ccc;
    }
    .deleteRed {
      color: #f5222d;
    }
  }
</style>
