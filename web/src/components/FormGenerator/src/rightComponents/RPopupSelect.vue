<template>
  <a-form-item label="弹窗标题" v-if="jnpfKey === 'popupSelect'">
    <a-input v-model:value="activeData.popupTitle" placeholder="请输入弹窗标题" />
  </a-form-item>
  <a-form-item label="弹窗类型" v-if="jnpfKey === 'popupSelect' && showType === 'pc'">
    <jnpf-select v-model:value="activeData.popupType" placeholder="请选择弹窗类型" :options="popupTypeOptions" />
  </a-form-item>
  <a-form-item label="弹窗宽度" v-if="jnpfKey === 'popupSelect' && showType === 'pc'">
    <a-select v-model:value="activeData.popupWidth" placeholder="请选择弹窗宽度">
      <a-select-option v-for="item in popupWidthOptions" :key="item" :value="item">{{ item }}</a-select-option>
    </a-select>
  </a-form-item>
  <a-form-item label="远端数据">
    <interface-modal :value="activeData.interfaceId" :title="activeData.interfaceName" :hasPage="1" popupTitle="远端数据" @change="onInterfaceIdChange" />
  </a-form-item>
  <a-form-item label="存储字段">
    <a-input v-model:value="activeData.propsValue" placeholder="请输入存储字段" />
  </a-form-item>
  <a-form-item label="显示字段">
    <a-input v-model:value="activeData.relationField" placeholder="请输入显示字段" />
  </a-form-item>
  <a-table :data-source="activeData.templateJson" :columns="columns" size="small" :pagination="false" v-if="activeData.templateJson.length">
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
  <a-divider>列表字段</a-divider>
  <div class="options-list">
    <draggable v-model="activeData.columnOptions" :animation="300" group="selectItem" handle=".option-drag" itemKey="uuid">
      <template #item="{ element, index }">
        <div class="select-item">
          <div class="select-line-icon option-drag">
            <i class="icon-ym icon-ym-darg" />
          </div>
          <a-input v-model:value="element.label" placeholder="列名" />
          <a-input v-model:value="element.value" placeholder="字段" />
          <div class="close-btn select-line-icon" @click="activeData.columnOptions.splice(index, 1)">
            <i class="icon-ym icon-ym-btn-clearn" />
          </div>
        </div>
      </template>
    </draggable>
    <div class="add-btn">
      <a-button type="link" preIcon="icon-ym icon-ym-btn-add" @click="addSelectItem">添加字段</a-button>
    </div>
  </div>
  <a-divider>列表分页</a-divider>
  <a-form-item label="分页设置">
    <a-switch v-model:checked="activeData.hasPage" />
  </a-form-item>
  <a-form-item label="分页条数" v-if="activeData.hasPage">
    <jnpf-radio v-model:value="activeData.pageSize" :options="pageSizeOptions" optionType="button" button-style="solid" class="right-radio" />
  </a-form-item>
  <a-divider></a-divider>
  <a-form-item label="能否清空">
    <a-switch v-model:checked="activeData.clearable" />
  </a-form-item>
  <a-form-item label="能否多选" v-if="jnpfKey === 'popupTableSelect'">
    <a-switch v-model:checked="activeData.multiple" @change="onMultipleChange" />
  </a-form-item>
</template>
<script lang="ts" setup>
  import { computed, unref } from 'vue';
  import draggable from 'vuedraggable';
  import { useDynamic } from '../hooks/useDynamic';
  import { InterfaceModal } from '/@/components/CommonModal';

  defineOptions({ inheritAttrs: false });
  const props = defineProps(['activeData', 'dicOptions']);
  const jnpfKey = computed(() => unref(props.activeData).__config__?.jnpfKey);
  const { showType, formFieldsOptions, onRelationFieldChange, onMultipleChange } = useDynamic(props.activeData);
  const columns = [
    { width: 50, title: '序号', align: 'center', customRender: ({ index }) => index + 1 },
    { title: '参数名称', dataIndex: 'field', key: 'field', width: 135 },
    { title: '表单字段', dataIndex: 'relationField', key: 'relationField', width: 135 },
  ];
  const popupTypeOptions = [
    { id: 'dialog', fullName: '居中弹窗' },
    { id: 'drawer', fullName: '右侧弹窗' },
  ];
  const popupWidthOptions = ['600px', '800px', '1000px', '40%', '50%', '60%', '70%', '80%'];
  const pageSizeOptions = [
    { id: 20, fullName: '20条' },
    { id: 50, fullName: '50条' },
    { id: 80, fullName: '80条' },
    { id: 100, fullName: '100条' },
  ];

  function onInterfaceIdChange(val, row) {
    if (!val) {
      props.activeData.interfaceId = '';
      props.activeData.interfaceName = '';
      props.activeData.templateJson = [];
      props.activeData.__config__.defaultValue = '';
      return;
    }
    if (props.activeData.__config__.propsUrl === val) return;
    props.activeData.interfaceId = val;
    props.activeData.interfaceName = row.fullName;
    props.activeData.templateJson = row.requestParameters ? JSON.parse(row.requestParameters) : [];
    props.activeData.__config__.defaultValue = '';
  }
  function addSelectItem() {
    props.activeData.columnOptions.push({
      value: '',
      label: '',
    });
  }
</script>
