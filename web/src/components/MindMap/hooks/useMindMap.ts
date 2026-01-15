import Chart from '@antv/chart-node-g6';
import G6, { ComboConfig, IEdge, IG6GraphEvent, IGraph, INode, Item, ModelConfig } from '@antv/g6';
import { message } from 'ant-design-vue';
import { computed, h, onMounted, reactive, ref, render, toRaw } from 'vue';
import CustomToolbar from './CustomToolbar';
import { cloneDeep, find } from 'lodash-es';

interface useMindMapCallback {
  nodeClick?: Function;
  nodeChange?: Function;
}

interface CustomModelConfig extends ModelConfig, ComboConfig {
  children?: CustomModelConfig[];
  collapsed?: boolean;
  statusColor?: string;
  trendData?: [];
  metricGrade?: any[];
}

const animateCfg = { duration: 200, easing: 'easeCubic' };

// 画布实例
let graph: any = reactive({});

let graphContainerRef = ref(null);

let clickCallback: any = <T>(_item: T) => {};
let changeCallback: any = <T>(_item: T) => {};

// 当前选中的节点
const currentItem: any = ref({});

const isScreenFull = ref(false);

const widthGraph = ref();
const heightGraph = ref();

const toolbar = new G6.ToolBar({
  position: { x: 10, y: 10 },
  getContent: (_graph?: IGraph) => {
    const content = document.createElement('div');
    render(h(CustomToolbar), content);
    return content;
  },
  handleClick: (code, graph) => {
    switch (code) {
      case 'zoomOut':
        graph.zoom(1.2, undefined, true, animateCfg);
        break;
      case 'zoomIn':
        graph.zoom(0.8, undefined, true, animateCfg);
        break;
      case 'realZoom':
        graph.zoomTo(1, undefined, true, animateCfg);
        break;
      case 'autoZoom':
        graph.fitView(20, undefined, true, animateCfg);
        break;
      case 'fullScreen':
        const container = graphContainerRef.value! as HTMLElement;
        if (isScreenFull.value) {
          if (document.exitFullscreen) {
            document.exitFullscreen();
          } else if (document.mozCancelFullScreen) {
            document.mozCancelFullScreen();
          } else if (document.webkitExitFullscreen) {
            document.webkitExitFullscreen();
          }
        } else {
          if (container.requestFullscreen) {
            container.requestFullscreen();
          } else if (container.mozRequestFullScreen) {
            container.mozRequestFullScreen();
          } else if (container.webkitRequestFullscreen) {
            container.webkitRequestFullscreen();
          } else if (container.msRequestFullscreen) {
            container.msRequestFullscreen();
          }
        }
        window.onresize = () => {
          isScreenFull.value = !isScreenFull.value;
          if (isScreenFull.value) {
            const w = container.scrollWidth;
            const h = container.scrollHeight || 500;
            graph.changeSize(w, h);
          } else {
            graph.changeSize(widthGraph.value, heightGraph.value);
          }
        };
        break;
    }
  },
});

const nodeConfig = {
  size: [300, 120],
};

const nodeStyle = {
  pointX: -nodeConfig.size[0] / 2,
  pointY: -nodeConfig.size[1] / 2,
  paddingTop: 20,
  paddingBottom: 10,
  paddingLeft: 10,
  paddingRight: 10,
  baseFontSize: 14,
  baseColor: '#787878',
  valueFontSize: 18,
  valueColor: '#000',
  bg: '#fff',
  borderColor: '#83c85c',
  radius: 2,
  leftLineBg: '#83c85c',
  // 标题文字颜色
  titleColor: '#fff',
  // 标题背景颜色
  titleBoxBg: '#1890ff',
  // 节点触摸时边框颜色
  nodeBorderStroke: '#1890ff',
  // 节点触摸时边框颜色
  nodeBorderHoverStroke: '#1890ff',
  // 边线触摸时颜色
  edgeHoverStroke: '#1890ff',
  // 节点点击时边框颜色
  nodeClickActive: '#1890ff',
  // 标题背景圆角
  nodeRadius: [2, 2, 0, 0],
  // 节点下面line的圆角
  nodeBottomLineRadius: [2, 2, 2, 2],
  colors: {
    S: '#E0DFE3',
    P: '#F46649',
    E: '#83c85c',
  },
};

const defaultNodeStyle = {
  stroke: '#40a9ff',
  radius: 5,
};

const defaultEdgeStyle = {
  stroke: '#ccc',
  endArrow: {
    path: 'M 0,0 L 12, 6 L 9,0 L 12, -6 Z',
    fill: '#ccc',
  },
};

const defaultLayout = {
  type: 'indented',
  direction: 'LR',
  dropCap: false,
  indent: nodeConfig.size[0] * 1.5,
  getHeight: () => {
    return nodeConfig.size[1];
  },
};

const defaultLabelCfg = {
  style: {
    fill: '#000',
    fontSize: 12,
  },
};

const defaultStateStyles = {
  hover: {
    stroke: nodeStyle.nodeBorderHoverStroke,
    lineWidth: 2,
    opacity: 1,
    shadowColor: nodeStyle.nodeBorderHoverStroke,
    shadowBlur: 6,
    // shadowOffsetX: 4,
    // shadowOffsetY: 4,
  },
  active: {
    stroke: nodeStyle.nodeClickActive,
    lineWidth: 2,
    opacity: 1,
    shadowColor: nodeStyle.nodeClickActive,
    shadowBlur: 6,
    // shadowOffsetX: 4,
    // shadowOffsetY: 4,
    endArrow: {
      path: 'M 0,0 L 12, 6 L 9,0 L 12, -6 Z',
      fill: nodeStyle.edgeHoverStroke,
    },
  },
  edgeHover: {
    stroke: nodeStyle.edgeHoverStroke,
    lineWidth: 2,
    opacity: 1,
    endArrow: {
      path: 'M 0,0 L 12, 6 L 9,0 L 12, -6 Z',
      fill: nodeStyle.edgeHoverStroke,
    },
  },
};

export function useMindMap({ props, graphRef, deleteItem, addItem }) {
  const { source, authAdd, authDelete } = toRaw(props);
  const ADD_ICON = (x, y, r) => {
    return [
      ['M', x - r, y],
      ['a', r, r, 0, 1, 0, r * 2, 0],
      ['a', r, r, 0, 1, 0, -r * 2, 0],
      ['M', x + 2 - r, y],
      ['L', x + r - 2, y],
      ['M', x, y - r + 2],
      ['L', x, y + r - 2],
    ];
  };
  const DELETE_ICON = (x, y, r) => {
    return [
      ['M', x - r, y],
      ['a', r, r, 0, 1, 0, r * 2, 0],
      ['a', r, r, 0, 1, 0, -r * 2, 0],
      ['M', x + 2 - r, y],
      ['L', x + r - 2, y],
    ];
  };

  G6.registerNode(
    'node-with-line',
    {
      options: {
        size: nodeConfig.size,
        stroke: '#91d5ff',
        fill: '#f00',
        draggable: true,
      },
      draw(cfg: CustomModelConfig, group) {
        const self = this as any;
        const styles = self.getShapeStyle(cfg);
        const w = styles.width;
        const h = styles.height;
        const startX = nodeStyle.pointX + nodeStyle.paddingLeft;
        const startY = nodeStyle.pointY + nodeStyle.paddingTop;

        const { name, collapsed, currentValue, metricName, trendData, statusColor, metricGrade } = cfg;

        let referenceValue = 0;

        const keyShape = group.addShape('rect', {
          attrs: {
            x: startX,
            y: startY,
            width: w,
            height: h,
            fill: nodeStyle.bg,
            radius: nodeStyle.radius,
            stroke: nodeStyle.nodeBorderStroke,
            cursor: 'pointer',
          },
          draggable: true,
        });

        // 标题 box
        group.addShape('rect', {
          attrs: {
            x: startX,
            y: startY,
            width: w,
            height: startY + 64,
            fill: nodeStyle.titleBoxBg,
            radius: nodeStyle.nodeRadius,
            cursor: 'pointer',
          },
          name: 'title-box',
          draggable: true,
        });

        // 标题
        group.addShape('text', {
          attrs: {
            text: name,
            x: startX + 10,
            y: startY + 20,
            fontSize: nodeStyle.baseFontSize,
            fill: nodeStyle.titleColor,
            cursor: 'pointer',
          },
          draggable: true,
        });

        // 值名称
        group.addShape('text', {
          attrs: {
            text: metricName,
            x: startX + 10,
            y: startY + 45,
            fontSize: nodeStyle.baseFontSize,
            fill: nodeStyle.baseColor,
            cursor: 'pointer',
          },
          draggable: true,
        });

        if (currentValue) {
          group.addShape('text', {
            attrs: {
              text: `指标值 ${currentValue}`,
              x: startX + 10,
              y: startY + 70,
              fontSize: nodeStyle.baseFontSize,
              fill: statusColor ? statusColor : nodeStyle.valueColor,
              fontWeight: 'bold',
              cursor: 'pointer',
            },
            draggable: true,
          });
        }

        if (metricGrade && metricGrade.length > 0) {
          const referenceObj = find(metricGrade, { name: '基准' });
          if (referenceObj) {
            referenceValue = referenceObj.value;
          }

          let xs = startX + 10;
          const allx = metricGrade.reduce((pre, cur, index) => {
            if (cur.is_show) {
              const names = cur.name.split('');
              if (index + 1 === metricGrade.length) {
                return pre + names.length * 12;
              } else {
                return pre + names.length * 12 + 30;
              }
            } else {
              return pre;
            }
          }, 0);

          if (w > allx) {
            xs = Math.floor(startX + (w - allx) / 2 - 5);
          }
          metricGrade.forEach(resGrade => {
            if (resGrade.is_show) {
              group.addShape('text', {
                attrs: {
                  text: resGrade.name,
                  x: xs,
                  y: startY + 90,
                  fontSize: nodeStyle.baseFontSize,
                  fill: nodeStyle.valueColor,
                  cursor: 'pointer',
                },
                draggable: true,
              });

              group.addShape('text', {
                attrs: {
                  text: resGrade.value,
                  x: xs,
                  y: startY + 110,
                  fontSize: nodeStyle.baseFontSize,
                  fill: resGrade.status_color ? resGrade.status_color : nodeStyle.valueColor,
                  fontWeight: 'bold',
                  cursor: 'pointer',
                },
                draggable: true,
              });
              const nameLen = resGrade.name.split('').length;
              xs = xs + 30 + nameLen * 12;
            }
          });
        }

        // 底部进度条进度
        if (statusColor && referenceValue) {
          // 底部进度条初始
          group.addShape('rect', {
            attrs: {
              x: startX + 3.2,
              y: startY + h - 8,
              width: w - 6,
              height: 6,
              radius: nodeStyle.nodeBottomLineRadius,
              fill: nodeStyle.colors['S'],
              cursor: 'pointer',
            },
          });

          const cw = w - 6;
          // 当前值/目标值
          const bottomLineFillPercent = Number(currentValue) / Number(referenceValue);
          let bottomLineWidth = 0;
          if (Number(currentValue) === 0 || bottomLineFillPercent === 0) {
            bottomLineWidth = 0;
          } else if (bottomLineFillPercent > 0 && bottomLineFillPercent < 1) {
            bottomLineWidth = cw * bottomLineFillPercent;
          } else {
            bottomLineWidth = cw;
          }
          group.addShape('rect', {
            attrs: {
              x: startX + 3.2,
              y: startY + h - 8,
              width: bottomLineWidth,
              height: 6,
              radius: nodeStyle.nodeBottomLineRadius,
              fill: statusColor,
              cursor: 'pointer',
            },
          });
        }

        // add icon
        if (authAdd) {
          group.addShape('marker', {
            attrs: {
              x: nodeStyle.pointX / 4,
              y: -nodeStyle.pointY + 29,
              r: 6,
              stroke: '#73d13d',
              cursor: 'pointer',
              symbol: ADD_ICON,
            },
            name: 'add-item',
          });
        }

        // delete icon
        if (authDelete) {
          group.addShape('marker', {
            attrs: {
              x: nodeStyle.pointX / 4 + 50,
              y: -nodeStyle.pointY + 29,
              r: 6,
              stroke: '#ff4d4f',
              cursor: 'pointer',
              symbol: DELETE_ICON,
            },
            name: 'remove-item',
          });
        }

        // collapse icon
        if (cfg.children && cfg.children.length) {
          group.addShape('marker', {
            attrs: {
              x: -nodeStyle.pointX + 20,
              y: -10,
              r: 6,
              stroke: '#83c85c',
              cursor: 'pointer',
              fill: '#fff',
            },
            name: 'collapse-item',
            modelId: cfg.id,
          });

          // collpase text
          group.addShape('text', {
            attrs: {
              x: -nodeStyle.pointX + 20,
              y: -9,
              textAlign: 'center',
              textBaseline: 'middle',
              text: collapsed ? cfg.children.length : cfg.children.length,
              fontSize: 10,
              cursor: 'pointer',
              fill: 'rgba(0, 0, 0, 0.25)',
            },
            name: 'collapse-text',
            modelId: cfg.id,
          });
        }

        if (trendData && trendData.length > 0) {
          const view = new Chart({
            group,
            padding: 5,
            width: 100,
            height: 40,
            x: -startX - 90,
            y: startY + 26,
          });
          view.data(trendData);
          // area
          view.area().position('item*value').style({
            fill: 'l(90) 0:#366bf1 1:#fff',
            cursor: 'pointer',
          });
          // line
          view.line().position('item*value').color('#366bf1').style({
            lineWidth: 1,
            cursor: 'pointer',
          });
          view.axis(false);
          view.legend(false);
          view.render();
        }

        return keyShape;
      },
      update: undefined,
    },
    'rect',
  );

  // 没有用到的，所以先注释掉
  // G6.registerEdge('flow-line', {
  //   draw(cfg, group) {
  //     const startPoint = cfg.startPoint!;
  //     const endPoint = cfg.endPoint!;

  //     const style = cfg.style!;
  //     return group.addShape('path', {
  //       attrs: {
  //         stroke: style.stroke,
  //         endArrow: style.endArrow,
  //         path: [
  //           ['M', startPoint.x, startPoint.y],
  //           ['L', startPoint.x, (startPoint.y + endPoint.y) / 2],
  //           ['L', endPoint.x, (startPoint.y + endPoint.y) / 2],
  //           ['L', endPoint.x, endPoint.y],
  //         ],
  //       },
  //     });
  //   },
  // });

  /**
   * @description 初始化画布
   * */
  const initGraph = () => {
    const container = graphRef.value;
    graphContainerRef.value = graphRef.value;
    if (!container) return;

    const width = container.scrollWidth;
    const height = container.scrollHeight || 500;

    widthGraph.value = width;
    heightGraph.value = height;

    const _graph = new G6.TreeGraph({
      container,
      width,
      height,
      linkCenter: false,
      plugins: [toolbar],
      enabledStack: false,
      minZoom: 0.5,
      modes: {
        default: ['drag-canvas', 'zoom-canvas', 'drag-node'],
      },
      defaultNode: {
        type: 'node-with-line',
        size: nodeConfig.size,
        style: defaultNodeStyle,
        labelCfg: defaultLabelCfg,
      },
      defaultEdge: {
        type: 'cubic-horizontal',
        // type: 'flow-line',
        style: defaultEdgeStyle,
        // style: {
        //   stroke: '#ccc',
        //   endArrow: true,
        // },
      },
      nodeStateStyles: defaultStateStyles,
      edgeStateStyles: defaultStateStyles,
      layout: defaultLayout,
    });
    graph = _graph;
    _graph.data(cloneDeep(source));
    _graph.render();
    // _graph.fitView();
    _graph.moveTo(100, 100, true, {
      duration: 100,
    });

    // 节点触摸事件进入
    _graph.on('node:mouseenter', (evt: IG6GraphEvent) => {
      const item = evt.item! as Item as INode;

      _graph.setItemState(item, 'hover', true);

      const edges = item.getEdges();
      edges.forEach(edge => _graph.setItemState(edge, 'edgeHover', true));
    });

    // 节点触摸事件离开
    _graph.on('node:mouseleave', evt => {
      const item = evt.item! as Item as INode;

      _graph.setItemState(item, 'hover', false);

      const edges = item.getEdges();
      edges.forEach(edge => _graph.setItemState(edge, 'edgeHover', false));
    });

    // 边触摸事件进入
    _graph.on('edge:mouseenter', evt => {
      const item = evt.item! as Item as IEdge;

      _graph.setItemState(item, 'edgeHover', true);

      const sourceNode = item.getSource();
      const targetNode = item.getTarget();
      _graph.setItemState(sourceNode, 'hover', true);
      _graph.setItemState(targetNode, 'hover', true);
    });

    // 边触摸事件离开
    _graph.on('edge:mouseleave', evt => {
      const item = evt.item! as Item as IEdge;
      _graph.setItemState(item, 'edgeHover', false);

      const sourceNode = item.getSource();
      const targetNode = item.getTarget();
      _graph.setItemState(sourceNode, 'hover', false);
      _graph.setItemState(targetNode, 'hover', false);
    });

    _graph.on('node:click', async (evt: any) => {
      const { item, target } = evt;
      const name = target.get('name');
      const model = item!.getModel();
      switch (name) {
        case 'collapse-item':
        case 'collapse-text':
          const collapsed = model.collapsed;
          model.collapsed = !collapsed;
          _graph.layout();
          _graph.setItemState(item, 'collapse', !collapsed);
          break;
        case 'add-item':
          const child = await addItem(model);
          if (!child) return;
          _graph.updateChild(child, model.id);
          setNodesState(model.id);
          break;
        case 'remove-item':
          deleteItem(model.id, model);
          break;
        default:
          const id = item.getModel().id;
          if (currentItem?.value.id != id) {
            changeCallback(item.getModel());
          }
          setNodesState(id);
          clickCallback(item.getModel());
          break;
      }

      // 设置消除边选状态
      _graph.getEdges().forEach(edge => {
        _graph.setItemState(edge, 'active', false);
      });
    });

    _graph.on('edge:click', evt => {
      const item = evt.item! as Item as IEdge;
      _graph.getEdges().forEach(edge => {
        if (edge._cfg!.id === item._cfg!.id) {
          _graph.setItemState(edge, 'active', true);
        } else {
          _graph.setItemState(edge, 'active', false);
        }
      });
    });

    const setNodesState = (id?) => {
      // 将其他节点的active状态去掉
      graph.getNodes().forEach(node => {
        if (node._cfg.id == id) {
          graph.setItemState(node, 'active', true);
          currentItem.value = node.getModel();
        } else {
          graph.setItemState(node, 'active', false);
        }
      });
    };

    if (typeof window !== 'undefined') {
      window.onresize = () => {
        if (!_graph || _graph.get('destroyed')) return;
        if (!graphRef.value || !graphRef.value.scrollWidth || !graphRef.value.scrollHeight) return;
        _graph.changeSize(graphRef.value.scrollWidth, graphRef.value.scrollHeight - 20);
      };
    }
  };

  onMounted(() => {
    initGraph();
  });
}

export function useMindMapCallback(params?: useMindMapCallback) {
  const { nodeClick, nodeChange } = params || {};
  clickCallback = nodeClick || function () {};
  changeCallback = nodeChange || function () {};
}

export function useMindMapResult() {
  /**
   * @description 获取数据
   */
  const getData = () => {
    return {
      nodes: graph.getNodes(),
      edges: graph.getEdges(),
    };
  };

  /**
   * @description 更新节点内容
   */
  const updateItem = target => {
    const item = graph.findById(target.id);
    if (item) {
      graph.updateItem(item, {
        label: target.name,
        metricId: target.metricId,
        metricName: target.metricName,
        currentValue: target.currentValue,
        trendData: target.trendData,
        metricGrade: target.metricGrade,
        statusColor: target.statusColor,
      });

      if (target.isRunChildren && target.children.length > 0) {
        graph.updateChildren(target.children, target.id);
      }
    } else {
      message.warning('请先锁定目标！');
    }
  };

  return {
    graph: computed(() => graph),
    getData,
    currentItem,
    updateItem,
  };
}
