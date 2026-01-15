<template>
  <BasicModal v-bind="$attrs" @register="registerModal" :width="800" class="transfer-modal" title="选择角色" showOkBtn @ok="handleSubmit" destroyOnClose>
    <div class="transfer__body">
      <div class="transfer-pane">
        <div class="transfer-pane__tool">
          <a-input-search :placeholder="t('common.enterKeyword')" allowClear v-model:value="keyword" @search="handleSearch" />
        </div>
        <div class="transfer-pane__body transfer-pane__body-tab">
          <a-tabs v-model:activeKey="activeKey" :tabBarGutter="10" size="small" @tabClick="onTabClick" class="flex-shrink-0">
            <a-tab-pane key="1" tab="组织"></a-tab-pane>
            <a-tab-pane key="2" tab="全局"></a-tab-pane>
          </a-tabs>
          <BasicTree class="tree-main" :treeData="treeData" :fieldNames="fieldNames" @select="handleSelect" ref="treeRef" defaultExpandAll :loading="loading" />
        </div>
      </div>
      <div class="transfer-pane right-pane">
        <div class="transfer-pane__tool">
          <span>已选</span>
          <span class="remove-all-btn" @click="removeAll">清空列表</span>
        </div>
        <div class="transfer-pane__body">
          <ScrollContainer>
            <div v-for="(item, i) in selectedData" :key="i" class="selected-item">
              <span :title="item.fullName">{{ item.fullName }}</span>
              <delete-outlined class="delete-btn" @click="removeData(i)" />
            </div>
            <Empty :image="simpleImage" v-if="!selectedData.length" />
          </ScrollContainer>
        </div>
      </div>
    </div>
  </BasicModal>
</template>
<script lang="ts" setup>
  import { BasicModal, useModalInner } from '/@/components/Modal';
  import { getModelData, setModelData } from '/@/api/permission/authorize';
  // import { useMessage } from '/@/hooks/web/useMessage';
  import { Form, Empty } from 'ant-design-vue';
  import { DeleteOutlined } from '@ant-design/icons-vue';
  import { ref, unref, watch, nextTick, reactive } from 'vue';
  import { BasicTree, TreeActionType } from '/@/components/Tree';
  import { ScrollContainer } from '/@/components/Container';
  import { organizeSelectProps } from '/@/components/Jnpf/Organize/src/props';
  import { useI18n } from '/@/hooks/web/useI18n';
  import { useOrganizeStore } from '/@/store/modules/organize';
  import { cloneDeep } from 'lodash-es';
  import { useMessage } from '/@/hooks/web/useMessage';

  defineOptions({ name: 'JnpfOrganizeSelect', inheritAttrs: false });
  const props = defineProps(organizeSelectProps);
  const { createMessage } = useMessage();
  const { t } = useI18n();
  const organizeStore = useOrganizeStore();
  const treeRef = ref<Nullable<TreeActionType>>(null);
  const innerValue = ref<string | any[] | undefined>([]);
  const keyword = ref('');
  const id = ref('');
  const systemId = ref('');
  const activeKey = ref('1');
  const treeData = ref<any[]>([]);
  const treeData1 = ref<any[]>([]);
  const treeData2 = ref<any[]>([]);
  const allList = ref<any[]>([]);
  const options = ref<any[]>([]);
  const loading = ref(false);
  const selectedData = ref<any[]>([]);
  const selectedIds = ref<any[]>([]);
  const simpleImage = ref(Empty.PRESENTED_IMAGE_SIMPLE);
  const fieldNames = reactive({ key: 'onlyId', title: 'fullName' });
  const formItemContext = Form.useInjectFormItemContext();
  const [registerModal, { closeModal }] = useModalInner(init);

  watch(
    () => selectedIds.value,
    () => {
      setValue();
    },
    { immediate: true },
  );
  watch(
    () => allList.value,
    () => {
      setValue();
    },
    { deep: true },
  );

  function init(data) {
    id.value = data.id;
    systemId.value = data.systemId;
    if (id.value) {
      loading.value = true;
      getModelData(id.value, 'Role').then(res => {
        selectedIds.value = res.data.ids;
        nextTick(() => initData());
        loading.value = false;
      });
    }
  }
  function setValue() {
    if (!selectedIds.value || !selectedIds.value.length) {
      innerValue.value = props.multiple ? [] : undefined;
      options.value = [];
      selectedData.value = [];
      return;
    }
    const ids = props.multiple ? (selectedIds.value as any[]) : [selectedIds.value];
    let selectedList: any[] = [];
    for (let i = 0; i < ids.length; i++) {
      inner: for (let j = 0; j < allList.value.length; j++) {
        if (ids[i] === allList.value[j].id) {
          selectedList.push(allList.value[j]);
          break inner;
        }
      }
    }
    const innerIds = selectedList.map(o => o.id);
    innerValue.value = props.multiple ? innerIds : innerIds[0];
    options.value = cloneDeep(selectedList);
    selectedData.value = cloneDeep(selectedList);
  }
  function onTabClick(val) {
    keyword.value = '';
    treeData.value = val == '1' ? treeData1.value : treeData2.value;
    nextTick(() => {
      handleSearch('');
    });
  }
  function handleSearch(value) {
    getTree().setSearchValue(value);
  }
  function handleSelect(keys) {
    if (!keys.length) return;
    const data = getTree().getSelectedNode(keys[0]);
    if (data?.disabled || data?.type !== 'role') return;
    const boo = selectedData.value.some(o => o.id === data.id);
    if (boo) return;
    props.multiple ? selectedData.value.push(data) : (selectedData.value = [data]);
  }
  function removeAll() {
    selectedData.value = [];
  }
  function removeData(index: number) {
    selectedData.value.splice(index, 1);
  }
  function getTree() {
    const tree = unref(treeRef);
    if (!tree) {
      throw new Error('tree is null!');
    }
    return tree;
  }
  function handleSubmit() {
    const ids = unref(selectedData).map(o => o.id);
    options.value = unref(selectedData);
    innerValue.value = props.multiple ? ids : ids[0];
    let query = { itemType: 'portalManage', objectType: 'Role', systemId: systemId.value, objectId: ids };
    setModelData(id.value, query).then(res => {
      createMessage.success(res.msg);
      formItemContext.onFieldChange();
      closeModal();
    });
  }
  async function initData() {
    loading.value = true;
    allList.value = await organizeStore.getRoleList();
    const res = await organizeStore.getRoleTree();
    treeData1.value = res.filter(o => o.id !== '1');
    treeData2.value = res.filter(o => o.id === '1');
    treeData.value = activeKey.value == '1' ? treeData1.value : treeData2.value;
    loading.value = false;
  }
</script>
