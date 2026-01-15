<template>
  <BasicPopup v-bind="$attrs" @register="registerPopup" :title="title" destroyOnClose :closeFunc="onClose">
    <template #insertToolbar>
      <a-button type="primary" @click="handlePrint" v-if="formConf.hasPrintBtn && formConf.printId">{{ formConf.printButtonText || '打印' }}</a-button>
    </template>
    <div class="p-10px" :style="{ margin: '0 auto', width: formConf.fullScreenWidth || '100%' }">
      <Parser ref="parserRef" :formConf="formConf" :formData="formData" :relationData="relationData" @toDetail="toDetail" :key="key" v-if="!loading" />
    </div>
  </BasicPopup>
  <BasicModal v-bind="$attrs" @register="registerModal" :title="title" :width="formConf.generalWidth" :minHeight="100" :showOkBtn="false" :closeFunc="onClose">
    <template #insertFooter>
      <a-button type="primary" @click="handlePrint" v-if="formConf.hasPrintBtn && formConf.printId">{{ formConf.printButtonText || '打印' }}</a-button>
    </template>
    <Parser ref="parserRef" :formConf="formConf" :formData="formData" :relationData="relationData" @toDetail="toDetail" :key="key" v-if="!loading" />
  </BasicModal>
  <BasicDrawer v-bind="$attrs" @register="registerDrawer" :title="title" :width="formConf.drawerWidth" showFooter :showOkBtn="false" :closeFunc="onClose">
    <template #insertFooter>
      <a-button type="primary" @click="handlePrint" v-if="formConf.hasPrintBtn && formConf.printId">{{ formConf.printButtonText || '打印' }}</a-button>
    </template>
    <div class="p-10px">
      <Parser ref="parserRef" :formConf="formConf" :formData="formData" :relationData="relationData" @toDetail="toDetail" :key="key" v-if="!loading" />
    </div>
  </BasicDrawer>
  <Detail v-if="detailVisible" ref="detailRef" @close="state.detailVisible = false" />
  <PrintSelect @register="registerPrintSelect" @change="handleShowBrowse" />
  <PrintBrowse @register="registerPrintBrowse" />
</template>
<script lang="ts" setup>
  import { getDataChange, getConfigData } from '/@/api/onlineDev/visualDev';
  import { getDataInterfaceDataInfoByIds } from '/@/api/systemData/dataInterface';
  import { reactive, toRefs, nextTick, ref } from 'vue';
  import { BasicPopup, usePopup } from '/@/components/Popup';
  import { BasicModal, useModal } from '/@/components/Modal';
  import { BasicDrawer, useDrawer } from '/@/components/Drawer';
  import { useMessage } from '/@/hooks/web/useMessage';
  import { useUserStore } from '/@/store/modules/user';
  import { useGeneratorStore } from '/@/store/modules/generator';
  import Parser from './Parser.vue';
  import PrintSelect from '/@/components/PrintDesign/printSelect/index.vue';
  import PrintBrowse from '/@/components/PrintDesign/printBrowse/index.vue';
  import { cloneDeep } from 'lodash-es';

  interface State {
    formConf: any;
    formData: any;
    config: any;
    loading: boolean;
    key: number;
    dataForm: any;
    formOperates: any[];
    title: string;
    relationData: any;
    detailVisible: boolean;
  }

  defineOptions({ name: 'Detail' });
  const emit = defineEmits(['close']);
  const userStore = useUserStore();
  const generatorStore = useGeneratorStore();
  const { createMessage } = useMessage();
  const [registerPopup, { openPopup, closePopup, setPopupProps }] = usePopup();
  const [registerModal, { openModal, closeModal, setModalProps }] = useModal();
  const [registerDrawer, { openDrawer, closeDrawer, setDrawerProps }] = useDrawer();
  const [registerPrintSelect, { openModal: openPrintSelect }] = useModal();
  const [registerPrintBrowse, { openModal: openPrintBrowse }] = useModal();
  const parserRef = ref<any>(null);
  const detailRef = ref<any>(null);
  const state = reactive<State>({
    formConf: {},
    formData: {},
    config: {},
    loading: false,
    key: +new Date(),
    dataForm: {
      id: '',
      data: '',
    },
    formOperates: [],
    title: '详情',
    relationData: {},
    detailVisible: false,
  });
  const { title, formConf, formData, relationData, key, loading, detailVisible } = toRefs(state);

  defineExpose({ init });

  function fillFormData(form, data) {
    let relationFormAttrList: any[] = [];
    const loop = (list, parent?) => {
      for (let i = 0; i < list.length; i++) {
        let item = list[i];
        if (item.__vModel__) {
          if (item.__config__.jnpfKey === 'relationForm' || item.__config__.jnpfKey === 'popupSelect') {
            item.__config__.defaultValue = data[item.__vModel__ + '_id'];
            item.name = data[item.__vModel__] || '';
          } else {
            const val = data.hasOwnProperty(item.__vModel__) ? data[item.__vModel__] : item.__config__.defaultValue;
            item.__config__.defaultValue = val;
          }
          if (state.config.useFormPermission) {
            let id = item.__config__.isSubTable ? parent.__vModel__ + '-' + item.__vModel__ : item.__vModel__;
            let noShow = true;
            if (state.formOperates && state.formOperates.length) {
              noShow = !state.formOperates.some(o => o.enCode === id);
            }
            noShow = item.__config__.noShow ? item.__config__.noShow : noShow;
            item.__config__.noShow = noShow;
          }
        }
        if (['relationFormAttr', 'popupAttr'].includes(item.__config__.jnpfKey)) relationFormAttrList.push(item);
        if (item.__config__ && item.__config__.children && Array.isArray(item.__config__.children)) {
          loop(item.__config__.children, item);
        }
      }
    };
    loop(form.fields);
    handleAttrList(relationFormAttrList);
  }
  function handleAttrList(list) {
    let realList = unique(list, 'relationField');
    for (let i = 0; i < realList.length; i++) {
      const item = realList[i];
      let modelId = '',
        id = '',
        field = '',
        jnpfKey = '',
        activeItem: any = {};
      let prop = item.relationField.split('_jnpfTable_')[0];
      const loop = list => {
        for (let i = 0; i < list.length; i++) {
          if (prop === list[i].__vModel__) {
            jnpfKey = list[i].__config__.jnpfKey;
            modelId = list[i].__config__.jnpfKey === 'relationForm' ? list[i].modelId : list[i].interfaceId;
            id = list[i].__config__.defaultValue;
            field = list[i].__config__.tableName
              ? list[i].__vModel__ + '_jnpfTable_' + list[i].__config__.tableName + (list[i].__config__.isSubTable ? '0' : '1')
              : list[i].__vModel__;
            activeItem = list[i];
            break;
          }
          if (list[i].__config__ && list[i].__config__.jnpfKey !== 'table' && list[i].__config__.children && Array.isArray(list[i].__config__.children)) {
            loop(list[i].__config__.children);
          }
        }
      };
      loop(state.formConf.fields);
      if (!id) {
        state.relationData[field] = '';
        continue;
      }
      if (jnpfKey === 'relationForm') {
        getDataChange(modelId, id)
          .then(res => {
            if (!res.data || !res.data.data) return (state.relationData[field] = '');
            const data = JSON.parse(res.data.data);
            state.relationData[field] = data;
          })
          .catch(() => {
            state.relationData[field] = '';
          });
      }
      if (jnpfKey === 'popupSelect') {
        const paramList = getParamList(activeItem);
        let query = {
          ids: [id],
          interfaceId: modelId,
          propsValue: activeItem.propsValue,
          relationField: activeItem.relationField,
          paramList,
        };
        getDataInterfaceDataInfoByIds(modelId, query)
          .then(res => {
            if (!res.data) return (state.relationData[field] = '');
            state.relationData[field] = res.data && res.data.length ? res.data[0] : {};
          })
          .catch(() => {
            state.relationData[field] = '';
          });
      }
    }
  }
  function unique(arr, attrName) {
    const res = new Map();
    // 根据对象的某个属性值去重
    return arr.filter(o => !res.has(o[attrName]) && res.set(o[attrName], 1));
  }
  function getParamList(activeItem) {
    let templateJson = activeItem.templateJson;
    if (!state.formData) return templateJson;
    for (let i = 0; i < templateJson.length; i++) {
      if (templateJson[i].relationField) {
        if (templateJson[i].relationField.includes('-')) {
          let tableVModel = templateJson[i].relationField.split('-')[0];
          let childVModel = templateJson[i].relationField.split('-')[1];
          templateJson[i].defaultValue = (state.formData[tableVModel] && state.formData[tableVModel][0] && state.formData[tableVModel][0][childVModel]) || '';
        } else {
          templateJson[i].defaultValue = state.formData[templateJson[i].relationField] || '';
        }
      }
    }
    return templateJson;
  }
  function init(data) {
    state.loading = true;
    state.config = data;
    state.formConf = cloneDeep(data.formConf);
    state.dataForm.id = data.id;
    getFormOperates();
    openForm();
    nextTick(() => {
      setTimeout(initData, 0);
    });
  }
  function initData() {
    changeLoading(true);
    state.loading = true;
    if (state.config.id) {
      const extra = { modelId: state.config.modelId, id: state.config.id, type: 2 };
      generatorStore.setDynamicModelExtra(extra);
      getInfo(state.config.id);
    } else {
      closeForm();
    }
  }
  function getInfo(id) {
    getDataChange(state.config.modelId, id).then(res => {
      state.dataForm = res.data || {};
      if (!state.dataForm.data) return;
      state.formData = JSON.parse(state.dataForm.data);
      fillFormData(state.formConf, state.formData);
      initRelationForm(state.formConf.fields);
      nextTick(() => {
        state.loading = false;
        state.key = +new Date();
        changeLoading(false);
      });
    });
  }
  function initRelationForm(componentList) {
    componentList.forEach(cur => {
      const config = cur.__config__;
      if (config.jnpfKey == 'relationFormAttr' || config.jnpfKey == 'popupAttr') {
        const relationKey = cur.relationField.split('_jnpfTable_')[0];
        componentList.forEach(item => {
          const noVisibility = Array.isArray(item.__config__.visibility) && !item.__config__.visibility.includes('pc');
          if (relationKey == item.__vModel__ && (noVisibility || !!item.__config__.noShow) && !cur.__vModel__) {
            cur.__config__.noShow = true;
          }
        });
      }
      if (cur.__config__.children && cur.__config__.children.length) initRelationForm(cur.__config__.children);
    });
  }
  function getFormOperates() {
    if (state.config.isPreview || !state.config.useFormPermission) return;
    const permissionList = userStore.getPermissionList;
    const modelId = state.config.menuId;
    const list = permissionList.filter(o => o.modelId === modelId);
    state.formOperates = list[0] && list[0].form ? list[0].form : [];
  }
  function toDetail(item) {
    if (!item.__config__.defaultValue) return;
    getConfigData(item.modelId).then(res => {
      if (!res.data) return;
      if (!res.data.formData) return;
      const formConf = JSON.parse(res.data.formData);
      formConf.popupType = state.formData.popupType;
      const data = {
        id: item.__config__.defaultValue,
        formConf,
        modelId: item.modelId,
      };
      state.detailVisible = true;
      nextTick(() => {
        detailRef.value?.init(data);
      });
    });
  }
  function handlePrint() {
    if (state.config.isPreview) return createMessage.warning('功能预览不支持打印');
    if (!state.formConf.printId?.length) return createMessage.error('未配置打印模板');
    if (state.formConf.printId?.length === 1) return handleShowBrowse(state.formConf.printId[0]);
    openPrintSelect(true, state.formConf.printId);
  }
  function handleShowBrowse(id) {
    openPrintBrowse(true, { id, formId: state.dataForm.id });
  }
  function openForm() {
    if (state.formConf.popupType === 'fullScreen') return openPopup();
    if (state.formConf.popupType === 'drawer') return openDrawer();
    openModal();
  }
  function closeForm() {
    if (state.formConf.popupType === 'fullScreen') return closePopup();
    if (state.formConf.popupType === 'drawer') return closeDrawer();
    closeModal();
  }
  function setFormProps(data) {
    if (state.formConf.popupType === 'fullScreen') return setPopupProps(data);
    if (state.formConf.popupType === 'drawer') return setDrawerProps(data);
    setModalProps(data);
  }
  function changeLoading(loading) {
    setFormProps({ loading });
  }
  async function onClose() {
    emit('close');
    return true;
  }
</script>
