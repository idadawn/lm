<template>
  <a-select v-bind="getSelectBindValue" v-model:value="innerValue" :options="options" @change="onChange" @click="openSelectModal"></a-select>
  <a-modal
    v-model:visible="visible"
    :title="modalTitle"
    :width="800"
    class="transfer-modal"
    @ok="handleSubmit"
    centered
    :maskClosable="false"
    :keyboard="false">
    <template #closeIcon>
      <ModalClose :canFullscreen="false" @cancel="handleCancel" />
    </template>
    <div class="transfer__body">
      <div class="transfer-pane">
        <div class="transfer-pane__tool">
          <a-input-search :placeholder="t('common.enterKeyword')" allowClear v-model:value="keyword" @search="handleSearch" />
        </div>
        <div class="transfer-pane__body">
          <BasicTree :treeData="treeData" @select="handleSelect" ref="treeRef" defaultExpandAll :loading="loading" />
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
              <span :title="item.organize">{{ item.organize }}</span>
              <delete-outlined class="delete-btn" @click="removeData(i)" />
            </div>
            <Empty :image="simpleImage" v-if="!selectedData.length" />
          </ScrollContainer>
        </div>
      </div>
    </div>
  </a-modal>
</template>

<script lang="ts" setup>
  import { getOrganizeSelectorByAuth, getDepartmentSelectorByAuth } from '/@/api/permission/organize';
  import { Form, Empty, Modal as AModal } from 'ant-design-vue';
  import { DeleteOutlined } from '@ant-design/icons-vue';
  import { computed, ref, unref, watch, onMounted, nextTick } from 'vue';
  import { BasicTree, TreeActionType } from '/@/components/Tree';
  import { ScrollContainer } from '/@/components/Container';
  import { organizeSelectProps } from './props';
  import ModalClose from '/@/components/Modal/src/components/ModalClose.vue';
  import { useI18n } from '/@/hooks/web/useI18n';
  import { useAttrs } from '/@/hooks/core/useAttrs';
  import { useOrganizeStore } from '/@/store/modules/organize';
  import { cloneDeep, pick } from 'lodash-es';

  defineOptions({ name: 'JnpfOrganizeSelect', inheritAttrs: false });
  const props = defineProps(organizeSelectProps);
  const emit = defineEmits(['update:value', 'change']);
  const attrs: any = useAttrs({ excludeDefaultKeys: false });
  const { t } = useI18n();
  const organizeStore = useOrganizeStore();
  const visible = ref(false);
  const treeRef = ref<Nullable<TreeActionType>>(null);
  const innerValue = ref<string | any[] | undefined>([]);
  const keyword = ref('');
  const treeData = ref<any[]>([]);
  const allList = ref<any[]>([]);
  const options = ref<any[]>([]);
  const loading = ref(false);
  const selectedIds = ref<any[]>([]);
  const selectedData = ref<any[]>([]);
  const nodePathData = ref<any[]>([]);
  const simpleImage = ref(Empty.PRESENTED_IMAGE_SIMPLE);
  const formItemContext = Form.useInjectFormItemContext();

  const getSelectBindValue = computed(() => ({
    ...pick(props, ['placeholder', 'disabled', 'size', 'allowClear']),
    fieldNames: { label: 'organize', value: 'id' },
    open: false,
    mode: props.multiple ? 'multiple' : '',
    showSearch: false,
    showArrow: true,
    class: unref(attrs).class ? 'w-full ' + unref(attrs).class : 'w-full',
  }));

  watch(
    () => props.value,
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

  function setValue() {
    if (!props.value || !props.value.length) {
      innerValue.value = props.multiple ? [] : undefined;
      options.value = [];
      selectedIds.value = [];
      selectedData.value = [];
      return;
    }
    const ids = props.multiple ? (props.value as any[]) : [props.value];
    if (!Array.isArray(ids[0])) return;
    selectedIds.value = cloneDeep(ids);
    let selectedList: any[] = [];
    for (let i = 0; i < ids.length; i++) {
      inner: for (let j = 0; j < allList.value.length; j++) {
        const organizeIds = allList.value[j].organizeIds;
        if (ids[i].join() === organizeIds.join()) {
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
  function onChange(_val, option) {
    if (!option) {
      selectedData.value = [];
      selectedIds.value = [];
    } else {
      selectedData.value = option;
      selectedIds.value = option.map(o => o.organizeIds);
    }
    handleSubmit();
  }
  function openSelectModal() {
    if (props.disabled) return;
    visible.value = true;
    keyword.value = '';
    treeData.value = [];
    initData();
    setValue();
    nextTick(() => {
      handleSearch('');
    });
  }
  function handleCancel() {
    visible.value = false;
  }
  function handleSearch(value) {
    getTree().setSearchValue(value);
  }
  function getNodePath(node): any[] {
    let fullPath: any[] = [];
    const currNode = { ...node.dataRef };
    fullPath.push(currNode);
    if (node.parent) {
      const nodes = node.parent.nodes;
      fullPath = [...nodes, ...fullPath];
    }
    return fullPath;
  }
  function handleSelect(keys, { node }) {
    if (!keys.length) return;
    const nodePath: any[] = getNodePath(node);
    const data = getTree().getSelectedNode(keys[0]);
    if (data?.disabled) return;
    const currId = data?.organizeIds;
    if (props.multiple) {
      const boo = selectedIds.value.some(o => o.join() === currId.join());
      if (boo) return;
      selectedIds.value.push(currId);
      selectedData.value.push(data);
      nodePathData.value.push(nodePath);
    } else {
      selectedIds.value = [currId];
      selectedData.value = [data];
      nodePathData.value = [nodePath];
    }
  }
  function removeAll() {
    selectedIds.value = [];
    selectedData.value = [];
  }
  function removeData(index: number) {
    selectedIds.value.splice(index, 1);
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
    if (props.multiple) {
      emit('update:value', unref(selectedIds));
      emit('change', unref(selectedIds), unref(nodePathData));
    } else {
      emit('update:value', unref(selectedIds)[0]);
      emit('change', unref(selectedIds)[0], unref(nodePathData)[0]);
    }
    formItemContext.onFieldChange();
    handleCancel();
  }
  async function initData() {
    loading.value = true;
    const orgTreeData = await organizeStore.getOrganizeTree();
    const topItem = {
      fullName: '顶级节点',
      hasChildren: true,
      id: '-1',
      icon: 'icon-ym icon-ym-tree-organization3',
      organize: '顶级节点',
      organizeIds: ['-1'],
    };
    const organizeList = await organizeStore.getOrganizeList();
    allList.value = [...organizeList, topItem];
    if (props.auth) {
      if (props.isOnlyOrg && props.parentId === '-1') {
        treeData.value = [topItem];
        loading.value = false;
        return;
      }
      const method = props.isOnlyOrg ? getOrganizeSelectorByAuth : getDepartmentSelectorByAuth;
      method(props.currOrgId).then(res => {
        treeData.value = res.data.list;
      });
    } else {
      treeData.value = orgTreeData;
    }
    loading.value = false;
  }

  onMounted(() => {
    initData();
  });
</script>
