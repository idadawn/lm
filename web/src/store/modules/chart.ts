import { union } from 'lodash-es';
import { defineStore } from 'pinia';
import { getChartData, getDimensions, getFilterData, getLayout, saveLayout } from '/@/api/chart';
import { store } from '/@/store';
import {
  changeTypeAdapter,
  getChartParams,
  getDimensionMap,
  getFilters,
  getOptionMap,
  optionAdapter,
} from '/@/utils/helper/toolHelper';
interface ChartEditorState {
  layout: any;
  currentChartId: string;
  currentIndex: number;
  optionMap: any;
  dimensionMap: any;
  scale: any;
  editType: string;
  dimensions: any;
  metrics: any;
  snapToGrid: boolean;
  snapToGridSize: number;
}
const initLayout = {
  id: '',
  name: '',
  description: '',
  canvas: {
    viewport: {
      width: 1920,
      height: 1080,
    },
    layoutsStyle: {
      borderColor: '#eeeeee',
      chartTheme: '明亮',
    },
    layouts: [
      {
        name: '',
        value: 0,
        style: {
          background: '#fff',
        },
        layout: [],
      },
    ],
    config: {
      autoRefresh: true,
    },
    contentSize: {
      width: 1398,
      height: 1056,
    },
  },
};

export const useChartStore = defineStore({
  id: 'app-chart',
  state: (): ChartEditorState => ({
    layout: {
      id: '',
      name: '',
      description: '',
      canvas: {
        type: 'responsive',
        viewport: {
          width: 1920,
          height: 1080,
        },
        layoutsStyle: {
          borderColor: '#eeeeee',
        },
        layouts: [
          {
            name: '',
            value: 0,
            style: {
              background: '#fff',
            },
            layout: [],
          },
        ],
        config: {
          autoRefresh: true,
        },
        contentSize: {
          width: 1398,
          height: 1056,
        },
      },
    },
    currentChartId: '',
    currentIndex: 0,
    optionMap: {},
    dimensionMap: {},
    scale: {
      x: 1,
      y: 1,
    },
    editType: 'preview',
    dimensions: [
      // { id: 'area', fullName: '区域' },
    ],
    // 指标
    metrics: [],
    // 吸附
    snapToGrid: false,
    snapToGridSize: 10,
  }),
  getters: {
    getLayout(): any {
      return this.layout;
    },
    getOptionMap(): any {
      return this.optionMap;
    },
    getCurrentChartId(): any {
      return this.currentChartId;
    },
    getDimensions(): any {
      return this.dimensions;
    },
    getMetrics(): any {
      return this.metrics;
    },
  },
  actions: {
    /**
     * 初始化或更新画布
     * @param params
     */
    async setLayout(params: any) {
      try {
        const type = params.type;
        if (type == 'init') {
          // reset
          this.layout = JSON.parse(JSON.stringify(initLayout));
          this.currentChartId = '';
          this.currentIndex = 0;
          this.optionMap = {};
          this.dimensionMap = {};

          const res = await getLayout({ id: params.id });
          if (res.data?.formJson) {
            this.layout = JSON.parse(res.data.formJson);
          }
          // 获取指标维度
          const dimensions = await getDimensions();
          this.dimensions = dimensions.data || [];
        }
        const res = await getOptionMap(this.layout, type);
        // 合并optionMap和res
        this.optionMap = Object.assign(this.optionMap, res);
        // 维度信息
        this.dimensionMap = await getDimensionMap(this.layout);
      } catch (e) {
        console.error(e);
      }
    },
    /**
     * 设置当前可操作元素id,index
     * @param key
     * @param index
     */
    setCurrentChartId(key: string, index: number) {
      this.currentChartId = key;
      this.currentIndex = index;
    },
    /**
     * 设置画布模式：预览｜编辑
     * @param type edit|preview
     */
    setLayoutEditType(type: string) {
      this.editType = type;
    },
    /**
     * 添加layout
     * @param key
     * @param node
     */
    async addLayout(key: string, node: any) {
      let optionData: any = {
        data: [],
      };
      this.layout.canvas.layouts[0].layout.push(node);
      const type = node.type;
      let chartTheme = '';

      switch (type) {
        case 'select':
          node.class = 'filter';
          const filterData = await getFilterData(node.id);
          optionData = filterData.data;
          break;
        case 'line':
        case 'bar':
        case 'pie':
          node.class = 'chart';
          chartTheme = this.layout.canvas.layoutsStyle.chartTheme;
          const commonFilters = getFilters(this.layout.canvas.layouts[0].layout);
          const chartQueryInfo = getChartParams(commonFilters, node);
          optionData = await getChartData(chartQueryInfo);
          break;
        default:
          break;
      }
      if (optionData.data) {
        this.optionMap[key] = await optionAdapter({ type, chartTheme }, optionData.data);
      }
    },

    /**
     * 更新layout
     * @param key
     * @param node
     */
    async updateLayout(key?: string, node?: any) {
      key = key ? key : this.currentChartId;
      node = node ? node : this.layout.canvas.layouts[0].layout.find(item => item.key == key);
      const commonFilters = getFilters(this.layout.canvas.layouts[0].layout);
      const currentLayout = this.layout.canvas.layouts[0].layout.find(item => item.key == key);
      // 更新当前layout，合并去重
      currentLayout.query.metrics = union(currentLayout.query.metrics, node.query.metrics);
      const chartQueryInfo = getChartParams(commonFilters, currentLayout);
      const optionData = await getChartData(chartQueryInfo);
      const type = currentLayout.type;
      const chartTheme = this.layout.canvas.layoutsStyle.chartTheme;
      this.optionMap[key] = await optionAdapter({ type, chartTheme }, optionData.data);
    },

    /**
     * 存储画布所有数据
     * @param id
     * @returns
     */
    async saveLayout(id) {
      const params = {
        gotId: id,
        gotType: 'Dash',
        formJson: JSON.stringify(this.layout),
      };
      return await saveLayout(params);
    },

    /**
     * 删除layout
     * @param key
     */
    deleteLayout(key?: string) {
      const _key = key ? key : this.currentChartId;
      delete this.optionMap[_key];
      const ids = this.layout.canvas.layouts[0].layout;
      ids.forEach((element, index) => {
        if (element.key == key) {
          ids.splice(index, 1);
        }
      });
    },

    /**
     * 切换chart展示类型
     * @param key
     * @param type
     * @param layout
     */
    async changeChartType(key, type, layout) {
      this.optionMap[key].type = type;
      this.optionMap[key] = await changeTypeAdapter(this.optionMap[key], type, layout);
    },

    /**
     * 缩放
     * @param scale
     */
    setScale(scale: any) {
      this.scale = scale;
    },

    /**
     * 设置指标列表
     * @param metrics
     */
    setMetrics(metrics: any) {
      this.metrics = metrics;
    },
    /**
     * 设置所有指标对应的维度数据
     * @param key
     * @param dimensions
     */
    setDimensionMap(key, dimensions) {
      this.dimensionMap[key] = { dimensions };
    },
  },
});

// Need to be used outside the setup
export function useChartStoreWithOut() {
  return useChartStore(store);
}
