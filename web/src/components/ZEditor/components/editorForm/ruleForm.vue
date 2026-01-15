<template>
  <BasicModal
    v-bind="$attrs"
    @register="registerModal"
    :title="getTitle"
    showOkBtn
    :okButtonProps="{ disabled: okButtonDisabled }"
    @ok="handleSubmit"
    @cancel="handleCancel">
    <div class="pb-4">
      <a-tabs class="custom-tabs" v-model:activeKey="dataForm.operators" @change="operatorsChange">
        <a-tab-pane :key="AddRuleTypeEnum.GreaterThan" tab="大于某值">
          <a-form :colon="false" :labelCol="{ style: { width: '40px' } }">
            <a-form-item label="如果">
              <div class="flex items-center">
                <jnpf-select v-model:value="dataForm.type" :options="valueOptions" showSearch placeholder="值" />
                <span class="px-2"> &gt;&#61;</span>
                <a-input v-model:value="dataForm.value" placeholder="" />
              </div>
            </a-form-item>
          </a-form>
        </a-tab-pane>
        <a-tab-pane :key="AddRuleTypeEnum.Between" tab="两值之间">
          <a-form :colon="false" :labelCol="{ style: { width: '40px' } }">
            <a-form-item label="如果">
              <div class="flex items-center">
                <a-input v-model:value="dataForm.minValue" placeholder="" />
                <span class="px-2"> &lt;&#61;</span>
                <jnpf-select v-model:value="dataForm.type" :options="valueOptions" showSearch placeholder="值" />
                <span class="px-2"> &lt;&#61;</span>
                <a-input v-model:value="dataForm.maxValue" placeholder="" />
              </div>
            </a-form-item>
          </a-form>
        </a-tab-pane>
        <a-tab-pane :key="AddRuleTypeEnum.LessThan" tab="小于某值">
          <a-form :colon="false" :labelCol="{ style: { width: '40px' } }">
            <a-form-item label="如果">
              <div class="flex items-center">
                <jnpf-select v-model:value="dataForm.type" :options="valueOptions" showSearch placeholder="值" />
                <span class="px-2"> &lt;&#61;</span>
                <a-input v-model:value="dataForm.value" placeholder="" />
              </div>
            </a-form-item>
          </a-form>
        </a-tab-pane>
      </a-tabs>
      <div class="update-status">
        更改状态为：
        <a-space>
          <a-select v-model:value="dataForm.status" @change="handleStatusChange">
            <a-select-option v-for="item in stateOptions" :key="item.id" :value="item.id">
              {{ item.name }}
            </a-select-option>
          </a-select>
        </a-space>
      </div>
    </div>
  </BasicModal>
</template>
<script lang="ts" setup>
  import { ref, computed } from 'vue';
  import { BasicModal, useModalInner } from '/@/components/Modal';
  import { AddRuleTypeEnum, AddRuleThresholdList, OptTypeEnum } from './const';
  import { RuleType, MetricCovStatusOptionType } from '../../types/type';
  import { postMetriccovrule, putMetriccovrule } from '/@/api/createModel/model';
  import { ResultEnum } from '/@/enums/httpEnum';
  import { message } from 'ant-design-vue';

  /**
   * @description 标题名称
   */
  const getTitle = computed(() => (dataForm.value.id ? '编辑规则' : '添加新规则'));

  /**
   * @description 计算按钮是否禁用
   */
  const okButtonDisabled = computed(() => {
    if (dataForm.value.operators === AddRuleTypeEnum.Between) {
      return !(
        String(dataForm.value.minValue) &&
        String(dataForm.value.maxValue) &&
        dataForm.value.type &&
        dataForm.value.status
      );
    } else {
      return !(String(dataForm.value.value) && dataForm.value.type && dataForm.value.status);
    }
  });

  const emits = defineEmits(['register', 'reload']);

  const [registerModal, { closeModal }] = useModalInner(init);

  const initForm: RuleType = {
    id: '',
    covId: '',
    level: '',
    operators: AddRuleTypeEnum.GreaterThan,
    type: '',
    value: '',
    minValue: '',
    maxValue: '',
    status: '',
  };

  const dataForm = ref<RuleType>({ ...initForm });

  const stateOptions = ref<MetricCovStatusOptionType[]>([]);

  const valueOptions = ref([...AddRuleThresholdList]);

  function init(data) {
    dataForm.value = { ...dataForm.value, ...data };
    stateOptions.value = data.statusOptions;
  }

  /**
   * @description 状态改变
   */
  const handleStatusChange = () => {
    // emits('update:dataForm', dataForm);
  };

  /**
   * @description 切换的时候清空输入值
   */
  const operatorsChange = () => {
    dataForm.value.value = '';
    dataForm.value.minValue = '';
    dataForm.value.maxValue = '';
    dataForm.value.type = '';
  };

  const handleCancel = () => {
    dataForm.value = { ...initForm };
  };

  /**
   * @description 新增、编辑
   * @description 处理提交新规则
   */
  async function handleSubmit() {
    const { id, covId, level, operators, type, value, minValue, maxValue, status } = dataForm.value;
    const params = {
      id,
      covId,
      level,
      operators,
      type,
      value,
      minValue,
      maxValue,
      status,
    };
    const fetchMethod = dataForm.value.id ? putMetriccovrule : postMetriccovrule;
    fetchMethod(params).then(res => {
      if (res.code === ResultEnum.SUCCESS) {
        message.success(res.msg);
        emits('reload', id ? OptTypeEnum.Edit : OptTypeEnum.Add);
        closeModal();
        dataForm.value = { ...initForm };
      } else {
        message.error(res.msg);
      }
    });
  }
</script>
<style lang="less" scoped>
  .custom-tabs {
    .ant-select,
    .ant-input,
    .ant-input-group-wrapper {
      width: 100px !important;
    }
  }
  .update-status {
    .ant-select {
      min-width: 100px !important;
    }
  }
</style>
