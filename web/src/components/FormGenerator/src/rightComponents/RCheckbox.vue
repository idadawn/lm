<template>
  <a-form-item label="默认值">
    <jnpf-checkbox v-model:value="activeData.__config__.defaultValue" :options="activeData.options" :fieldNames="activeData.props" />
  </a-form-item>
  <a-form-item label="排列方式">
    <jnpf-radio v-model:value="activeData.direction" :options="directionOptions" optionType="button" buttonStyle="solid" />
  </a-form-item>
  <a-divider>数据选项</a-divider>
  <a-form-item label=" " :labelCol="{ style: { width: '30px' } }">
    <jnpf-radio v-model:value="activeData.__config__.dataType" :options="dataTypeOptions" optionType="button" buttonStyle="solid" @change="onDataTypeChange" />
  </a-form-item>
  <div class="options-list" v-if="activeData.__config__.dataType === 'static'">
    <draggable v-model="activeData.options" :animation="300" group="selectItem" handle=".option-drag" itemKey="uuid">
      <template #item="{ element, index }">
        <div class="select-item">
          <div class="select-line-icon option-drag">
            <i class="icon-ym icon-ym-darg" />
          </div>
          <a-input v-model:value="element.fullName" placeholder="选项名" />
          <a-input v-model:value="element.id" placeholder="选项值" />
          <div class="close-btn select-line-icon" @click="activeData.options.splice(index, 1)">
            <i class="icon-ym icon-ym-btn-clearn" />
          </div>
        </div>
      </template>
    </draggable>
    <div class="add-btn">
      <a-button type="link" preIcon="icon-ym icon-ym-btn-add" @click="addSelectItem" class="!px-0">添加选项</a-button>
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
      <jnpf-select v-model:value="activeData.props.value" placeholder="请选择存储字段" :options="valueOptions" />
    </a-form-item>
  </div>
  <div v-if="activeData.__config__.dataType === 'dynamic'">
    <a-form-item label="远端数据">
      <interface-modal :value="activeData.__config__.propsUrl" :title="activeData.__config__.propsName" popupTitle="远端数据" @change="onPropsUrlChange" />
    </a-form-item>
    <a-form-item label="存储字段">
      <a-input v-model:value="activeData.props.value" placeholder="请输入存储字段" />
    </a-form-item>
    <a-form-item label="显示字段">
      <a-input v-model:value="activeData.props.label" placeholder="请输入显示字段" />
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
  <BatchOperate @register="registerBatchOperate" @confirm="onBatchOperateConfirm" />
</template>
<script lang="ts" setup>
  import draggable from 'vuedraggable';
  import { useDynamic } from '../hooks/useDynamic';
  import { InterfaceModal } from '/@/components/CommonModal';
  import { useModal } from '/@/components/Modal';
  import BatchOperate from './components/BatchOperate.vue';

  defineOptions({ inheritAttrs: false });
  const props = defineProps(['activeData', 'dicOptions']);

  const {
    onDataTypeChange,
    onDictionaryTypeChange,
    onPropsUrlChange,
    dataTypeOptions,
    valueOptions,
    addSelectItem,
    formFieldsOptions,
    onRelationFieldChange,
    onBatchOperateConfirm,
  } = useDynamic(props.activeData);
  const [registerBatchOperate, { openModal }] = useModal();

  const columns = [
    { width: 50, title: '序号', align: 'center', customRender: ({ index }) => index + 1 },
    { title: '参数名称', dataIndex: 'field', key: 'field', width: 135 },
    { title: '表单字段', dataIndex: 'relationField', key: 'relationField', width: 135 },
  ];
  const directionOptions = [
    { id: 'horizontal', fullName: '水平排列' },
    { id: 'vertical', fullName: '垂直排列' },
  ];
</script>
