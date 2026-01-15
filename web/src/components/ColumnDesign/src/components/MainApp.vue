<template>
  <div class="column-design-container">
    <div class="main-board">
      <jnpf-group-title content="查询字段" :bordered="false" />
      <a-table :data-source="columnData.searchList" :columns="searchColumns" size="small" :pagination="false" rowKey="id" class="search-table-app">
        <template #bodyCell="{ column, record }">
          <template v-if="column.key === 'drag'">
            <i class="drag-handler icon-ym icon-ym-darg" title="点击拖动" />
          </template>
          <template v-if="column.key === 'label' && webType == 4">
            <a-input v-model:value="record.label" placeholder="列名" allowClear />
          </template>
          <template v-if="column.key === 'searchType'">
            <jnpf-select v-model:value="record.searchType" :options="searchTypeOptions" :disabled="!['input', 'textarea'].includes(record.jnpfKey)" />
          </template>
          <template v-if="column.key === 'searchMultiple'">
            <a-checkbox v-model:checked="record.searchMultiple" :disabled="!multipleList.includes(record.jnpfKey)" />
          </template>
        </template>
      </a-table>
      <jnpf-group-title content="排序字段" :bordered="false" class="mt-20px" />
      <a-table :data-source="columnData.sortList" :columns="columnColumns" size="small" :pagination="false" rowKey="id" class="sort-table-app">
        <template #bodyCell="{ column, record }">
          <template v-if="column.key === 'drag'">
            <i class="drag-handler icon-ym icon-ym-darg" title="点击拖动" />
          </template>
          <template v-if="column.key === 'label' && webType == 4">
            <a-input v-model:value="record.label" placeholder="列名" allowClear />
          </template>
        </template>
      </a-table>
      <jnpf-group-title content="列表字段" :bordered="false" class="mt-20px" />
      <a-table :data-source="columnData.columnList" :columns="columnColumns" size="small" :pagination="false" rowKey="id" class="column-table-app">
        <template #bodyCell="{ column, record }">
          <template v-if="column.key === 'drag'">
            <i class="drag-handler icon-ym icon-ym-darg" title="点击拖动" />
          </template>
          <template v-if="column.key === 'label' && webType == 4">
            <a-input v-model:value="record.label" placeholder="列名" allowClear />
          </template>
        </template>
      </a-table>
    </div>
    <div class="right-board">
      <a-tabs v-model:activeKey="activeKey" :tabBarGutter="5" class="average-tabs">
        <a-tab-pane key="search" tab="查询字段"></a-tab-pane>
        <a-tab-pane key="sort" tab="排序字段"></a-tab-pane>
        <a-tab-pane key="field" tab="列表字段"></a-tab-pane>
        <a-tab-pane key="column" tab="列表属性"></a-tab-pane>
      </a-tabs>
      <div class="right-main">
        <div class="h-full" v-show="activeKey === 'search'">
          <a-table
            :data-source="state.searchOptions"
            :columns="rightColumns"
            size="small"
            :pagination="false"
            :scroll="{ y: 'calc(100vh - 161px)' }"
            rowKey="id"
            :row-selection="{ columnWidth: 50, selectedRowKeys: searchSelectedRowKeys, onChange: onSearchSelectChange }">
            <template #headerCell>查询字段</template>
          </a-table>
        </div>
        <div class="h-full" v-show="activeKey === 'sort'">
          <a-table
            :data-source="state.sortOptions"
            :columns="rightColumns"
            size="small"
            :pagination="false"
            :scroll="{ y: 'calc(100vh - 161px)' }"
            rowKey="id"
            :row-selection="{ columnWidth: 50, selectedRowKeys: sortSelectedRowKeys, onChange: onSortSelectChange }">
            <template #headerCell>排序字段</template>
          </a-table>
        </div>
        <div class="h-full" v-show="activeKey === 'field'">
          <a-table
            :data-source="state.columnOptions"
            :columns="rightColumns"
            size="small"
            :pagination="false"
            :scroll="{ y: 'calc(100vh - 161px)' }"
            rowKey="id"
            :row-selection="{ columnWidth: 50, selectedRowKeys: columnSelectedRowKeys, onChange: onColumnSelectChange }">
            <template #headerCell>列表字段</template>
          </a-table>
        </div>
        <ScrollContainer v-show="activeKey === 'column'">
          <a-form :colon="false" labelAlign="left" :labelCol="{ style: { width: '90px' } }" class="right-board-form !-mt-10px">
            <a-divider>表格配置</a-divider>
            <a-form-item label="数据过滤" v-if="webType != 4">
              <a-button block @click="editRuleList">{{ getRuleBtnText }}</a-button>
            </a-form-item>
            <a-form-item label="排序类型">
              <jnpf-select v-model:value="columnData.sort" :options="sortTypeOptions" placeholder="请选择排序类型" />
            </a-form-item>
            <a-form-item label="排序字段">
              <jnpf-select
                v-model:value="columnData.defaultSidx"
                :options="state.groupFieldOptions"
                placeholder="请选择排序字段"
                :fieldNames="{ options: 'options1' }"
                showSearch
                allowClear />
            </a-form-item>
            <a-form-item label="分页设置">
              <a-switch v-model:checked="columnData.hasPage" />
            </a-form-item>
            <a-form-item label="分页条数" v-if="columnData.hasPage">
              <jnpf-radio v-model:value="columnData.pageSize" :options="pageSizeOptions" optionType="button" button-style="solid" class="right-radio" />
            </a-form-item>
            <a-divider>按钮配置</a-divider>
            <a-checkbox-group v-model:value="state.btnsList" class="btnsList" v-if="webType != 4">
              <div v-for="item in btnsOption" :key="item.value">
                <a-checkbox :value="item.value">
                  <span class="btn-label">{{ getBtnText(item.value) }}</span>
                  <a-input v-model:value="item.label" placeholder="按钮名称" />
                </a-checkbox>
              </div>
            </a-checkbox-group>
            <a-checkbox-group v-model:value="state.columnBtnsList" class="btnsList" v-if="webType != 4">
              <div v-for="item in columnBtnsOption" :key="item.value">
                <a-checkbox :value="item.value">
                  <span class="btn-label">{{ getBtnText(item.value) }}</span>
                  <a-input v-model:value="item.label" placeholder="按钮名称" />
                </a-checkbox>
              </div>
            </a-checkbox-group>
            <div v-if="modelType == 1">
              <p class="btn-cap">自定义按钮区</p>
              <div class="custom-btns-list">
                <draggable v-model="columnData.customBtnsList" :animation="300" group="selectItem" handle=".option-drag" itemKey="value">
                  <template #item="{ element, index }">
                    <div class="custom-item">
                      <div class="custom-line-icon option-drag">
                        <i class="icon-ym icon-ym-darg" />
                      </div>
                      <p class="custom-line-value">{{ element.value }}</p>
                      <a-input v-model:value="element.label" placeholder="按钮名称">
                        <template #addonAfter>
                          <span class="cursor-pointer" @click="editBtnEvent(element, index)">事件</span>
                        </template>
                      </a-input>
                      <div class="close-btn custom-line-icon" @click="columnData.customBtnsList.splice(index, 1)">
                        <i class="icon-ym icon-ym-btn-clearn" />
                      </div>
                    </div>
                  </template>
                </draggable>
                <div class="add-btn">
                  <a-button type="link" preIcon="icon-ym icon-ym-btn-add" @click="addCustomBtn">添加选项</a-button>
                </div>
              </div>
            </div>
            <div v-if="webType != 4">
              <a-divider>权限设置</a-divider>
              <a-form-item label="按钮权限">
                <a-switch v-model:checked="columnData.useBtnPermission" />
              </a-form-item>
              <a-form-item label="列表权限">
                <a-switch v-model:checked="columnData.useColumnPermission" />
              </a-form-item>
              <a-form-item label="表单权限">
                <a-switch v-model:checked="columnData.useFormPermission" />
              </a-form-item>
              <a-form-item label="数据权限">
                <a-switch v-model:checked="columnData.useDataPermission" />
              </a-form-item>
            </div>
            <div v-if="modelType == 1">
              <a-divider>脚本事件</a-divider>
              <a-form-item :label="getFuncText(key)" v-for="(_value, key) in columnData.funcs" :key="key">
                <a-button block @click="editFunc(key)">脚本编写</a-button>
              </a-form-item>
            </div>
          </a-form>
        </ScrollContainer>
      </div>
    </div>
    <FormScript @register="registerScriptModal" @confirm="updateScript" />
    <BtnEvent @register="registerBtnEventModal" @confirm="updateBtnEvent" />
    <ConditionModal @register="registerConditionModal" @confirm="updateRuleList" />
  </div>
</template>
<script lang="ts" setup>
  import { ref, reactive, onMounted, computed, toRefs, unref, nextTick, watch } from 'vue';
  import { ScrollContainer } from '/@/components/Container';
  import { getDrawingList } from '/@/components/FormGenerator/src/helper/db';
  import { noColumnShowList, noSearchList, getSearchType, getSearchMultiple, defaultFuncsData, defaultAppColumnData } from '../helper/config';
  import { cloneDeep } from 'lodash-es';
  import draggable from 'vuedraggable';
  import { buildBitUUID } from '/@/utils/uuid';
  import { useModal } from '/@/components/Modal';
  import FormScript from './FormScript.vue';
  import BtnEvent from './BtnEvent.vue';
  import ConditionModal from './ConditionModal.vue';
  import Sortablejs from 'sortablejs';

  interface State {
    columnData: any;
    groupFieldOptions: any[];
    columnOptions: any[];
    searchOptions: any[];
    sortOptions: any[];
    btnsList: any[];
    columnBtnsList: any[];
    activeFunc: string;
    activeBtn: string;
    searchSelectedRowKeys: string[];
    columnSelectedRowKeys: string[];
    sortSelectedRowKeys: string[];
  }

  const props = defineProps(['conf', 'formInfo', 'viewFields']);
  defineExpose({ getData });

  const sortTypeOptions = [
    { id: 'asc', fullName: '升序' },
    { id: 'desc', fullName: '降序' },
  ];
  const pageSizeOptions = [
    { id: 20, fullName: '20条' },
    { id: 50, fullName: '50条' },
    { id: 80, fullName: '80条' },
    { id: 100, fullName: '100条' },
  ];
  const btnsOption = ref([{ value: 'add', icon: 'icon-ym icon-ym-btn-add', label: '新增' }]);
  const columnBtnsOption = ref([
    { value: 'edit', icon: 'icon-ym icon-ym-btn-edit', label: '编辑' },
    { value: 'remove', icon: 'icon-ym icon-ym-btn-clearn', label: '删除' },
    { value: 'detail', icon: 'icon-ym icon-ym-generator-menu', label: '详情' },
  ]);
  const rightColumns = [{ title: '字段', dataIndex: 'fullName', key: 'fullName' }];
  const searchColumns = [
    { title: '拖动', dataIndex: 'drag', key: 'drag', align: 'center', width: 50 },
    { title: '列名', dataIndex: 'label', key: 'label', width: 200 },
    { title: '字段', dataIndex: 'prop', key: 'prop' },
    { title: '类型', dataIndex: 'searchType', key: 'searchType', width: 200 },
    { title: '是否多选', dataIndex: 'searchMultiple', key: 'searchMultiple', width: 100, align: 'center' },
  ];
  const columnColumns = [
    { title: '拖动', dataIndex: 'drag', key: 'drag', align: 'center', width: 50 },
    { title: '列名', dataIndex: 'label', key: 'label', width: '40%' },
    { title: '字段', dataIndex: 'prop', key: 'prop' },
  ];

  const multipleList = ['select', 'depSelect', 'roleSelect', 'userSelect', 'usersSelect', 'organizeSelect', 'posSelect', 'groupSelect'];
  const searchTypeOptions = [
    { id: 1, fullName: '等于查询' },
    { id: 2, fullName: '模糊查询' },
    { id: 3, fullName: '范围查询' },
  ];

  const activeKey = ref('column');
  const state = reactive<State>({
    columnData: cloneDeep(defaultAppColumnData),
    groupFieldOptions: [],
    columnOptions: [],
    searchOptions: [],
    sortOptions: [],
    btnsList: [],
    columnBtnsList: [],
    activeFunc: '',
    activeBtn: '',
    searchSelectedRowKeys: [],
    columnSelectedRowKeys: [],
    sortSelectedRowKeys: [],
  });
  const { columnData, searchSelectedRowKeys, columnSelectedRowKeys, sortSelectedRowKeys } = toRefs(state);
  const [registerScriptModal, { openModal: openScriptModal }] = useModal();
  const [registerBtnEventModal, { openModal: openBtnEventModal }] = useModal();
  const [registerConditionModal, { openModal: openConditionModal }] = useModal();

  const webType = computed(() => props.formInfo?.webType);
  const modelType = computed(() => props.formInfo?.type);
  const getRuleBtnText = computed(() => (state.columnData?.ruleListApp.length ? '编辑过滤条件' : '添加过滤条件'));
  const formFieldsOptions = computed(() => {
    let list: any[] = [];
    const loop = (data, parent?) => {
      if (!data) return;
      if (data.__config__ && data.__config__.children && Array.isArray(data.__config__.children)) {
        loop(data.__config__.children, data);
      }
      if (Array.isArray(data)) data.forEach(d => loop(d, parent));
      if (data.__config__ && data.__config__.jnpfKey) {
        const visibility = !data.__config__.visibility || (Array.isArray(data.__config__.visibility) && data.__config__.visibility.includes('app'));
        if (data.__config__.layout === 'colFormItem' && data.__vModel__ && visibility) {
          const isTableChild = parent && parent.__config__ && parent.__config__.jnpfKey === 'table';
          list.push({
            id: isTableChild ? parent.__vModel__ + '-' + data.__vModel__ : data.__vModel__,
            fullName: isTableChild ? parent.__config__.label + '-' + data.__config__.label : data.__config__.label,
            ...data,
          });
        }
      }
    };
    loop(getDrawingList());
    return list;
  });
  const viewFieldOptions = computed(() => props.viewFields.map(o => ({ id: o, fullName: o, __vModel__: o, __config__: { jnpfKey: 'input' } })));

  watch(
    () => unref(viewFieldOptions),
    () => {
      if (unref(webType) == 4) init(unref(viewFieldOptions));
    },
  );

  // 供父组件使用 获取表单JSON
  function getData() {
    updateBtnList();
    state.columnData.defaultColumnList = state.columnOptions.map(o => ({
      ...o,
      checked: state.columnData.columnList.some(i => i.prop === o.prop),
    }));
    return state.columnData;
  }
  function updateBtnList() {
    const list: any[] = [];
    for (let i = 0; i < unref(btnsOption).length; i++) {
      if (state.btnsList.includes(unref(btnsOption)[i].value)) {
        list.push(unref(btnsOption)[i]);
      }
    }
    state.columnData.btnsList = list;
    const columnBtns: any[] = [];
    for (let i = 0; i < unref(columnBtnsOption).length; i++) {
      if (state.columnBtnsList.includes(unref(columnBtnsOption)[i].value)) {
        columnBtns.push(unref(columnBtnsOption)[i]);
      }
    }
    state.columnData.columnBtnsList = columnBtns;
  }
  function getBtnText(key) {
    let text = '';
    switch (key) {
      case 'download':
        text = '导出';
        break;
      case 'batchRemove':
        text = '批量删除';
        break;
      case 'edit':
        text = '编辑';
        break;
      case 'remove':
        text = '删除';
        break;
      case 'detail':
        text = '详情';
        break;
      case 'upload':
        text = '导入';
        break;
      default:
        text = '新增';
        break;
    }
    return text;
  }
  function getFuncText(key) {
    let text = '';
    switch (key) {
      case 'afterOnload':
        text = '表格事件';
        break;
      case 'rowStyle':
        text = '表格行样式';
        break;
      case 'cellStyle':
        text = '单元格样式';
        break;
      default:
        text = '';
        break;
    }
    return text;
  }
  function addCustomBtn() {
    const id = buildBitUUID();
    state.columnData.customBtnsList.push({
      value: 'btn_' + id,
      label: '按钮' + id,
      event: {},
    });
  }
  function editBtnEvent(item, index) {
    state.activeBtn = index;
    openBtnEventModal(true, {
      showType: 'app',
      formFieldsOptions: unref(webType) == 4 ? state.columnOptions : unref(formFieldsOptions),
      dataForm: item.event,
    });
  }
  function updateBtnEvent(data) {
    state.columnData.customBtnsList[state.activeBtn].event = data;
  }
  function editRuleList() {
    openConditionModal(true, {
      ruleList: state.columnData.ruleListApp,
      formFieldsOptions: unref(webType) == 4 ? state.columnOptions : unref(formFieldsOptions),
    });
  }
  function updateRuleList(data) {
    state.columnData.ruleListApp = data;
  }
  function editFunc(funcName) {
    state.activeFunc = funcName;
    if (!state.columnData.funcs[state.activeFunc]) state.columnData.funcs[state.activeFunc] = defaultFuncsData[state.activeFunc];
    openScriptModal(true, { text: state.columnData.funcs[state.activeFunc], funcName });
  }
  function updateScript(data) {
    state.columnData.funcs[state.activeFunc] = data;
  }
  function setBtnValue(replacedData, data, key?) {
    key = key ? key : 'value';
    outer: for (let i = 0; i < replacedData.length; i++) {
      inter: for (let ii = 0; ii < data.length; ii++) {
        if (replacedData[i][key] === data[ii][key]) {
          data[ii] = replacedData[i];
          break inter;
        }
      }
    }
  }
  function setListValue(data, defaultData, type) {
    data = data.filter(o =>
      defaultData.some(e => {
        if (o.prop == e.prop) {
          o.label = e.label;
          o.fullName = e.fullName;
        }
        return o.prop == e.prop;
      }),
    );
    state[type + 'SelectedRowKeys'] = data.map(o => o.prop);
  }
  function updateListValue(selectedRowKeys, selectedRows, type) {
    state[type + 'SelectedRowKeys'] = selectedRowKeys;
    if (!selectedRowKeys.length) return (state.columnData[type + 'List'] = []);
    state.columnData[type + 'List'] = state.columnData[type + 'List'].filter(o => selectedRowKeys.some(e => o.prop == e));
    for (let i = 0; i < selectedRows.length; i++) {
      if (!state.columnData[type + 'List'].some(o => o.prop === selectedRows[i].prop)) {
        state.columnData[type + 'List'].push(selectedRows[i]);
      }
    }
  }
  function onSearchSelectChange(selectedRowKeys, selectedRows) {
    updateListValue(selectedRowKeys, selectedRows, 'search');
  }
  function onColumnSelectChange(selectedRowKeys, selectedRows) {
    updateListValue(selectedRowKeys, selectedRows, 'column');
  }
  function onSortSelectChange(selectedRowKeys, selectedRows) {
    updateListValue(selectedRowKeys, selectedRows, 'sort');
  }
  function initSort() {
    const searchTable: any = document.querySelector(`.search-table-app .ant-table-tbody`);
    Sortablejs.create(searchTable, {
      handle: '.drag-handler',
      animation: 150,
      easing: 'cubic-bezier(1, 0, 0, 1)',
      onStart: () => {},
      onEnd: ({ newIndex, oldIndex }: any) => {
        const currRow = state.columnData.searchList.splice(oldIndex, 1)[0];
        state.columnData.searchList.splice(newIndex, 0, currRow);
      },
    });
    const sortTable: any = document.querySelector(`.sort-table-app .ant-table-tbody`);
    Sortablejs.create(sortTable, {
      handle: '.drag-handler',
      animation: 150,
      easing: 'cubic-bezier(1, 0, 0, 1)',
      onStart: () => {},
      onEnd: ({ newIndex, oldIndex }: any) => {
        const currRow = state.columnData.sortList.splice(oldIndex, 1)[0];
        state.columnData.sortList.splice(newIndex, 0, currRow);
      },
    });
    const columnTable: any = document.querySelector(`.column-table-app .ant-table-tbody`);
    Sortablejs.create(columnTable, {
      handle: '.drag-handler',
      animation: 150,
      easing: 'cubic-bezier(1, 0, 0, 1)',
      onStart: () => {},
      onEnd: ({ newIndex, oldIndex }: any) => {
        const currRow = state.columnData.columnList.splice(oldIndex, 1)[0];
        state.columnData.columnList.splice(newIndex, 0, currRow);
      },
    });
  }
  function init(list) {
    const columnOptions = list.filter(o => !noColumnShowList.includes(o.__config__.jnpfKey) || o.isStorage);
    const searchOptions = list.filter(o => !noSearchList.includes(o.__config__.jnpfKey));
    const sortOptions = columnOptions.filter(o => o.id.indexOf('_jnpf_') < 0 && o.id.indexOf('-') < 0);
    state.groupFieldOptions = list.filter(o => o.id.indexOf('-') < 0).map(o => ({ ...o, disabled: false }));
    state.columnOptions = columnOptions.map(o => ({
      label: o.fullName,
      prop: o.id,
      fixed: 'none',
      align: 'left',
      jnpfKey: o.__config__.jnpfKey,
      sortable: false,
      width: null,
      ...o,
    }));
    state.searchOptions = searchOptions.map(o => ({
      label: o.fullName,
      prop: o.id,
      jnpfKey: o.__config__.jnpfKey,
      value: '',
      searchType: getSearchType(o),
      searchMultiple: getSearchMultiple(o.__config__.jnpfKey),
      ...o,
    }));
    state.sortOptions = sortOptions.map(o => ({
      label: o.fullName,
      prop: o.id,
      jnpfKey: o.__config__.jnpfKey,
      ...o,
    }));
    state.columnData.columnOptions = columnOptions;
    if (!state.columnOptions.length) state.columnData.columnList = [];
    if (!state.searchOptions.length) state.columnData.searchList = [];
    if (!state.sortOptions.length) state.columnData.sortList = [];
    setBtnValue(state.columnData.btnsList, btnsOption.value);
    setBtnValue(state.columnData.columnBtnsList, columnBtnsOption.value);
    state.btnsList = state.columnData.btnsList.map(o => o.value);
    state.columnBtnsList = state.columnData.columnBtnsList.map(o => o.value);
    nextTick(() => {
      setListValue(state.columnData.searchList, state.searchOptions, 'search');
      setListValue(state.columnData.columnList, state.columnOptions, 'column');
      setListValue(state.columnData.sortList, state.sortOptions, 'sort');
      initSort();
    });
  }

  onMounted(() => {
    if (typeof props.conf === 'object' && props.conf !== null) {
      state.columnData = Object.assign({}, defaultAppColumnData, props.conf);
    }
    if (unref(webType) != 4) {
      init(unref(formFieldsOptions));
    }
  });
</script>
