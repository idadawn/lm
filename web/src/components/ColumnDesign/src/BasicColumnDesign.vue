<template>
  <div :class="`${prefixCls}`">
    <div class="h-full" v-show="formInfo.webType == 2 || formInfo.webType == 4">
      <column-main ref="columnMain" v-bind="getBindValue" v-show="showType === 'pc'" />
      <column-main-app ref="columnMainApp" v-bind="getAppBindValue" v-show="showType === 'app'" />
      <div class="head-tabs">
        <a-button pre-icon="icon-ym icon-ym-pc" type="link" :class="{ 'unActive-btn': showType != 'pc' }" @click="showType = 'pc'">桌面端</a-button>
        <a-button pre-icon="icon-ym icon-ym-mobile" type="link" :class="{ 'unActive-btn': showType != 'app' }" @click="showType = 'app'">移动端</a-button>
      </div>
    </div>
    <div class="column-empty-box" v-show="formInfo.webType == 1">
      <img :src="emptyImg" class="empty-img" />
      <p>开启后，表单带有数据列表</p>
      <a-button type="primary" class="w-150px" size="large" @click="toggleWebType">开启列表</a-button>
    </div>
  </div>
</template>
<script lang="ts" setup>
  import { ref, unref, computed, onMounted } from 'vue';
  import { useDesign } from '/@/hooks/web/useDesign';
  import ColumnMain from './components/Main.vue';
  import ColumnMainApp from './components/MainApp.vue';
  import emptyImg from '/@/assets/images/generator/columnType1.png';
  import { omit } from 'lodash-es';
  import { getDataInterfaceFields } from '/@/api/systemData/dataInterface';

  const props = defineProps(['columnData', 'appColumnData', 'formInfo']);
  const emit = defineEmits(['toggleWebType']);
  defineExpose({ getData });
  const { prefixCls } = useDesign('basic-column-design');

  const columnMain = ref<any>(null);
  const columnMainApp = ref<any>(null);
  const showType = ref('pc');
  const viewFields = ref<any[]>([]);

  const getBindValue = computed(() => ({ ...omit(props, ['columnData', 'appColumnData']), conf: props.columnData, viewFields: unref(viewFields) }));
  const getAppBindValue = computed(() => ({ ...omit(props, ['columnData', 'appColumnData']), conf: props.appColumnData, viewFields: unref(viewFields) }));

  // 供父组件使用 获取表单JSON
  function getData() {
    return new Promise((resolve, reject) => {
      const columnData = unref(columnMain).getData();
      if (!columnData) reject({ msg: '', target: 2 });
      const appColumnData = unref(columnMainApp).getData();
      if (!appColumnData.columnList || !appColumnData.columnList.length) {
        appColumnData.columnList = columnData.columnList;
      }
      resolve({ columnData: columnData, appColumnData: appColumnData, target: 2 });
    });
  }
  function toggleWebType() {
    emit('toggleWebType', 2);
  }
  function getFields() {
    if (!props.formInfo.interfaceId) return;
    const query = { paramList: props.formInfo?.interfaceParam ? JSON.parse(props.formInfo.interfaceParam) : [] };
    getDataInterfaceFields(props.formInfo.interfaceId, query).then(res => {
      viewFields.value = res.data || [];
    });
  }
  onMounted(() => {
    getFields();
  });
</script>
<style lang="less">
  @import '../style/index.less';
</style>
