<script setup lang="ts">
  import { ref, createVNode, watch } from 'vue';
  import { props as _props } from './props';
  import { useMindMap, useMindMapResult } from '../hooks/useMindMap';
  import { useI18n } from '/@/hooks/web/useI18n';
  import { GotTypeEnum } from '/@/enums/publicEnum';
  import { addIndicatorValueChain, deleteIndicatorValueChain } from '/@/api/createModel/model';
  import { ResultEnum } from '/@/enums/httpEnum';
  import { message, Modal } from 'ant-design-vue';
  import { ExclamationCircleOutlined } from '@ant-design/icons-vue';
  import { isEqual } from 'lodash-es';

  const { t } = useI18n();

  defineOptions({
    name: 'MindMap',
  });

  const props = defineProps(_props);

  const graphRef: any = ref(null);

  /**
   * @description 删除节点
   * @param { String } nodeId
   * @param { Object } model
   */
  const deleteItem = (nodeId?, model?) => {
    if (props.onDeleteItem) {
      props.onDeleteItem(nodeId, model);
    } else {
      Modal.confirm({
        title: t('common.tipTitle'),
        icon: createVNode(ExclamationCircleOutlined),
        centered: true,
        content: t('common.delTip'),
        onOk() {
          return deleteIndicatorValueChain(nodeId).then(async res => {
            if (res.code === ResultEnum.SUCCESS) {
              message.success(res.msg);
              if (model?.is_root && model?.parentId === '-1') {
                await graph.value.clear();
                const params = {
                  name: '初始节点',
                  gotType: GotTypeEnum.cov,
                  gotId: props.source.gotId,
                  parentId: '-1',
                  is_root: true,
                };
                const res = await addIndicatorValueChain(params);
                if (res.code === ResultEnum.SUCCESS) {
                  graph.value.data(res.data);
                  graph.value.render();
                  graph.value.moveTo(100, 100, true, {
                    duration: 100,
                  });
                }
              } else {
                // TreeGraph 删除
                graph.value.removeChild(nodeId);
                // Graph 删除
                // graph.value.removeItem(nodeId);
              }
            } else {
              message.error(res.msg);
            }
          });
        },
        onCancel() {},
      });
    }
  };

  /**
   * @description 新增节点
   * @param {String} nodeId
   * @param { Object } model
   */
  const addItem = async model => {
    if (props.onAddItem) {
      return props.onAddItem(model);
    } else {
      const params = {
        name: `指标节点${Math.ceil(Math.random() * 100000)}`,
        gotType: GotTypeEnum.cov,
        gotId: props.source.gotId,
        metricId: '',
        currentValue: '',
        level: '',
        iv: '',
        tv: '',
        parentId: model.id,
        covTreeId: '',
        status: '',
        is_root: false,
      };
      const res = await addIndicatorValueChain(params);
      if (res.code === ResultEnum.SUCCESS) {
        const { id, name, gotId, parentId } = res.data;
        return { id, name, gotId, parentId };
      }
      return null;
    }
  };

  useMindMap({
    props,
    graphRef,
    deleteItem,
    addItem,
  });

  const { graph, updateItem } = useMindMapResult();

  const loop = (newVal, _oldVal) => {
    const isChanged = (newSource, oldSource) => {
      if (!isEqual(newSource.id, oldSource.id) && (newSource.id || oldSource.id)) {
        return true;
      }
      if (!isEqual(newSource.name, oldSource.name && (newSource.name || oldSource.name))) {
        return true;
      }
      if (!isEqual(newSource.metricId, oldSource.metricId) && (newSource.metricId || oldSource.metricId)) {
        return true;
      }
      if (!isEqual(newSource.metricName, oldSource.metricName) && (newSource.metricName || oldSource.metricName)) {
        return true;
      }
      if (
        !isEqual(newSource.currentValue, oldSource.currentValue) &&
        (newSource.currentValue || oldSource.currentValue)
      ) {
        return true;
      }
      if (!isEqual(newSource.trendData, oldSource.trendData) && (newSource.trendData || oldSource.trendData)) {
        return true;
      }
      if (!isEqual(newSource.metricGrade, oldSource.metricGrade) && (newSource.metricGrade || oldSource.metricGrade)) {
        return true;
      }
      if (!isEqual(newSource.statusColor, oldSource.statusColor) && (newSource.statusColor || oldSource.statusColor)) {
        return true;
      }
      return false;
    };

    const updateView = source => {
      updateItem({
        id: source.id,
        name: source.name ?? '',
        metricId: source.metricId ?? '',
        metricName: source.metricName ?? '',
        currentValue: source.currentValue ?? '',
        trendData: source.trendData ?? [],
        metricGrade: source.metricGrade ?? [],
        statusColor: source.statusColor ?? '',
      });
    };

    const task = source => {
      if (Object.prototype.toString.call(source) === '[object Array]') {
        source.forEach(item => task(item));
      }

      if (Object.prototype.toString.call(source) === '[object Object]') {
        // 判断节点数据是否变化
        const oldSource = graph.value.findById(source.id).getModel();
        if (oldSource) {
          if (isChanged(source, oldSource)) {
            updateView(source);
          }
        } else {
          updateView(source);
        }

        if (source.children) {
          task(source.children);
        }
      }
    };

    task(newVal);
  };

  watch(
    () => props.source,
    (newVal, oldVal) => {
      if (graph.value) {
        loop(newVal, oldVal);
      }
    },
    {
      deep: true,
    },
  );
</script>

<template>
  <div class="mindMapGraph" ref="graphRef"></div>
</template>

<style scoped lang="less">
  .mindMapGraph {
    width: 100%;
    height: 100%;
  }
</style>
