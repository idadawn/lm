import { onMounted } from 'vue';
import { getMetricsDimensions } from '/@/api/chart';
import { useChartStore } from '/@/store/modules/chart';
import { guid } from '/@/utils/helper/toolHelper';

export const useEditor = () => {
  const chartStore = useChartStore();

  const addNode = async (data, isOnChart, currentKey) => {
    console.log('addNode', data);
    // 获取鼠标位置
    const { e, item } = data;
    const x = e?.offsetX;
    const y = e?.offsetY;
    let key = currentKey ? currentKey : item.type + guid();

    // 获取指标维度
    let dimension = '';
    if (item.class == 'chart') {
      const dimensionRes = await getMetricsDimensions({ metrics: [item.id] });
      const dimensions = dimensionRes.data;
      dimension = dimensions[0]?.id;
      chartStore.setDimensionMap(key, dimensions);
    }
    const node: any = {
      key,
      id: item.id || key,
      type: item.type || 'line',
      query: {
        metrics: [item.id],
        dimension: dimension,
      },
      setting: {
        title: item.name,
        titleShow: true,
        xAxis: {
          title: '',
          titleShow: true,
          unit: '月',
        },
        yAxis: {
          title: '',
          titleShow: true,
          unit: '月',
        },
      },

      style: {
        width: item.class == 'chart' ? 400 : 200,
        height: 300,
        x,
        y,
        zIndex: 18,
      },
      filter: {
        operator: item.class == 'filter' ? 'AND' : 'IN',
        conditions: [
          {
            operator: 'AND',
            field: item.id,
            dataType: '',
            fieldName: '',
            checkedList: [],
            filterType: 'ByValue', //筛选方式
            minValue: undefined, //最小值
            maxValue: undefined, //最大值
            minValueChecked: true, //最小值包含
            maxValueChecked: true, //最大值包含
          },
        ],
      },
    };
    if (item.class == 'chart') {
      node.filter.conditions = [];
    }
    if (isOnChart) {
      chartStore.updateLayout(key, node);
    } else {
      chartStore.addLayout(key, node);
    }
  };
  const delNode = (key?: string) => {
    chartStore.deleteLayout(key);
  };

  onMounted(() => {});

  return {
    addNode,
    delNode,
  };
};
