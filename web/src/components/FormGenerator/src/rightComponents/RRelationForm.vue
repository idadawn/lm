<template>
  <a-form-item label="弹窗标题">
    <a-input v-model:value="activeData.popupTitle" placeholder="请输入弹窗标题" />
  </a-form-item>
  <a-form-item label="弹窗类型" v-if="showType === 'pc'">
    <jnpf-select v-model:value="activeData.popupType" placeholder="请选择弹窗类型" :options="popupTypeOptions" />
  </a-form-item>
  <a-form-item label="弹窗宽度" v-if="showType === 'pc'">
    <a-select v-model:value="activeData.popupWidth" placeholder="请选择弹窗宽度">
      <a-select-option v-for="item in popupWidthOptions" :key="item" :value="item">{{ item }}</a-select-option>
    </a-select>
  </a-form-item>
  <a-form-item label="关联功能">
    <JnpfTreeSelect v-model:value="activeData.modelId" :options="treeData" placeholder="请选择关联功能" lastLevel allowClear @change="onModeIdChange" />
  </a-form-item>
  <a-form-item label="显示字段">
    <jnpf-select
      v-model:value="activeData.relationField"
      placeholder="请选择显示字段"
      :options="fieldOptions"
      showSearch
      @dropdownVisibleChange="visibleChange" />
  </a-form-item>
  <a-divider>列表字段</a-divider>
  <div class="options-list">
    <draggable v-model="activeData.columnOptions" :animation="300" group="selectItem" handle=".option-drag" itemKey="uuid">
      <template #item="{ element, index }">
        <div class="select-item">
          <div class="select-line-icon option-drag">
            <i class="icon-ym icon-ym-darg" />
          </div>
          <jnpf-select
            v-model:value="element.value"
            placeholder="请选择显示字段"
            :options="fieldOptions"
            showSearch
            @dropdownVisibleChange="visibleChange"
            @change="onChange($event, element)" />
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
</template>
<script lang="ts" setup>
  import { getVisualDevSelector, getFormDataFields } from '/@/api/onlineDev/visualDev';
  import { computed, ref, inject, onMounted } from 'vue';
  import draggable from 'vuedraggable';
  import { useMessage } from '/@/hooks/web/useMessage';

  defineOptions({ inheritAttrs: false });
  const props = defineProps(['activeData']);
  const emit = defineEmits(['relationChange']);
  const { createMessage } = useMessage();

  const getShowType: (() => string | undefined) | undefined = inject('getShowType');
  const showType = computed(() => (getShowType as () => string | undefined)());

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
  const treeData = ref([]);
  const fieldOptions = ref<any[]>([]);

  function onModeIdChange(val) {
    props.activeData.relationField = '';
    emit('relationChange', props.activeData.__vModel__);
    if (!val) {
      fieldOptions.value = [];
      return;
    }
    getFieldOptions();
  }
  function getFieldOptions() {
    if (!props.activeData.modelId) return;
    getFormDataFields(props.activeData.modelId).then(res => {
      fieldOptions.value = res.data.list.map(o => ({
        ...o,
        id: o.vmodel,
        fullName: o.label,
      }));
    });
  }
  function visibleChange(val) {
    if (!val) return;
    if (!props.activeData.modelId) createMessage.warning('请先选择关联功能');
  }
  function onChange(val, item) {
    const list = fieldOptions.value.filter(o => o.vmodel === val) || [];
    if (!list.length) return;
    const active = list[0];
    item.label = active.label;
  }
  function addSelectItem() {
    props.activeData.columnOptions.push({
      value: '',
      label: '',
    });
  }

  function getFeatureList() {
    getVisualDevSelector({ type: 1, webType: '1,2' }).then(res => {
      treeData.value = res.data.list;
    });
  }

  onMounted(() => {
    getFeatureList();
    getFieldOptions();
  });
</script>
