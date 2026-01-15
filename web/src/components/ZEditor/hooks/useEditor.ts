import G6 from '@antv/g6';
import { computed, onMounted, reactive, ref } from 'vue';
import ZBackground from '../plugins/ZBackground';
import { guid } from '/@/utils/helper/toolHelper';

const state = reactive<any>({
  source:{},
});
let graph: any = reactive({}); // 画布实例
export function useEditor({ source, graphRef }) {
  state.source = source;
  const currentItem: any = ref({}); // 当前选中的节点
  
  // 添加节点
  const addNode = ({ item, e }) => {
    // 将屏幕/页面坐标转换为渲染坐标
    const point = graph.getPointByClient(e.x, e.y);
    // 新创建的节点信息
    const model = {
      id: 'node' + guid(),
      label: item.name || 'node', // 文本标签
      type: item.type || 'rect', // 图片类型的节点
      x: point.x,
      y: point.y,
      // FIXME 定义锚点坐标
      anchorPoints: item.anchorPoints || [
        [0, 0.5],
        [0.33, 0],
        [0.66, 0],
        [1, 0.5],
        [0.33, 1],
        [0.66, 1],
      ],
    };
    graph.addItem('node', model, false);
    setNodesState(model.id);
  };
  // 删除节点
  const delItem = () => {
    const nodes: any[] = [];
    graph.getNodes().forEach(node => {
      if (node.hasState('active')) {
        nodes.push(node);
      }
    });
    nodes.forEach(node => {
      graph.removeItem(node);
    });
    graph.getEdges().forEach(edge => {
      if (edge.hasState('active')) {
        graph.removeItem(edge);
      }
    });
    // 更新currentItem
    currentItem.value = {};
  };
  // 添加拖拽节点
  const processParallelEdgesOnAnchorPoint = (
    edges,
    offsetDiff = 15,
    multiEdgeType = 'extra-shape-edge',
    singleEdgeType = undefined,
    loopEdgeType = undefined,
  ) => {
    const len = edges.length;
    const cod = offsetDiff * 2;
    const loopPosition = ['top', 'top-right', 'right', 'bottom-right', 'bottom', 'bottom-left', 'left', 'top-left'];
    const edgeMap = {};
    const tags = [];
    const reverses = {};
    for (let i = 0; i < len; i++) {
      const edge = edges[i];
      const { source, target, sourceAnchor, targetAnchor } = edge;
      const sourceTarget = `${source}|${sourceAnchor}-${target}|${targetAnchor}`;

      if (tags[i]) continue;
      if (!edgeMap[sourceTarget]) {
        edgeMap[sourceTarget] = [];
      }
      tags[i] = true;
      edgeMap[sourceTarget].push(edge);
      for (let j = 0; j < len; j++) {
        if (i === j) continue;
        const sedge = edges[j];
        const { source: src, target: dst, sourceAnchor: srcAnchor, targetAnchor: dstAnchor } = sedge;

        // 两个节点之间共同的边
        // 第一条的source = 第二条的target
        // 第一条的target = 第二条的source
        if (!tags[j]) {
          if (source === dst && sourceAnchor === dstAnchor && target === src && targetAnchor === srcAnchor) {
            edgeMap[sourceTarget].push(sedge);
            tags[j] = true;
            reverses[`${src}|${srcAnchor}|${dst}|${dstAnchor}|${edgeMap[sourceTarget].length - 1}`] = true;
          } else if (source === src && sourceAnchor === srcAnchor && target === dst && targetAnchor === dstAnchor) {
            edgeMap[sourceTarget].push(sedge);
            tags[j] = true;
          }
        }
      }
    }

    for (const key in edgeMap) {
      const arcEdges = edgeMap[key];
      const { length } = arcEdges;
      for (let k = 0; k < length; k++) {
        const current = arcEdges[k];
        if (current.source === current.target) {
          if (loopEdgeType) current.type = loopEdgeType;
          // 超过8条自环边，则需要重新处理
          current.loopCfg = {
            position: loopPosition[k % 8],
            dist: Math.floor(k / 8) * 20 + 50,
          };
          continue;
        }
        if (length === 1 && singleEdgeType && (current.source !== current.target || current.sourceAnchor !== current.targetAnchor)) {
          current.type = singleEdgeType;
          continue;
        }
        current.type = multiEdgeType;
        const sign = (k % 2 === 0 ? 1 : -1) * (reverses[`${current.source}|${current.sourceAnchor}|${current.target}|${current.targetAnchor}|${k}`] ? -1 : 1);
        if (length % 2 === 1) {
          current.curveOffset = sign * Math.ceil(k / 2) * cod;
        } else {
          current.curveOffset = sign * (Math.floor(k / 2) * cod + offsetDiff);
        }
      }
    }
    return edges;
  };

  G6.registerNode(
    'rect-node',
    {
      // draw anchor-point circles according to the anchorPoints in afterDraw
      afterDraw(cfg, group: any) {
        const bbox = group.getBBox();
        const anchorPoints = this.getAnchorPoints(cfg);
        anchorPoints.forEach((anchorPos, i) => {
          group.addShape('circle', {
            attrs: {
              r: 5,
              x: bbox.x + bbox.width * anchorPos[0],
              y: bbox.y + bbox.height * anchorPos[1],
              fill: '#fff',
              stroke: '#5F95FF',
            },
            // must be assigned in G6 3.3 and later versions. it can be any string you want, but should be unique in a custom item type
            name: `anchor-point`, // the name, for searching by group.find(ele => ele.get('name') === 'anchor-point')
            anchorPointIdx: i, // flag the idx of the anchor-point circle
            links: 0, // cache the number of edges connected to this shape
            visible: false, // invisible by default, shows up when links > 1 or the node is in showAnchors state
            draggable: true, // allow to catch the drag events on this shape
          });
        });
      },
      getAnchorPoints(cfg: any) {
        return (
          cfg.anchorPoints || [
            [0, 0.5],
            [0.33, 0],
            [0.66, 0],
            [1, 0.5],
            [0.33, 1],
            [0.66, 1],
          ]
        );
      },
      // response the state changes and show/hide the link-point circles
      setState(name, value, item: any) {
        if (name === 'showAnchors') {
          const anchorPoints = item.getContainer().findAll(ele => ele.get('name') === 'anchor-point');
          anchorPoints.forEach(point => {
            if (value || point.get('links') > 0) point.show();
            else point.hide();
          });
        }
      },
    },
    'rect',
  );
  // custom the edge with an extra rect
  G6.registerEdge(
    'extra-shape-edge',
    {
      afterDraw(cfg: any, group: any) {
        // get the first shape in the graphics group of this edge, it is the path of the edge here
        // 获取图形组中的第一个图形，在这里就是边的路径图形
        const shape = group.get('children')[0];
        // get the coordinate of the mid point on the path
        // 获取路径图形的中点坐标
        const midPoint = shape.getPoint(0.5);
        const rectColor = cfg.midPointColor || '#333';
        // add a rect on the mid point of the path. note that the origin of a rect shape is on its lefttop
        // 在中点增加一个矩形，注意矩形的原点在其左上角
        // group.addShape('rect', {
        //   attrs: {
        //     width: 10,
        //     height: 10,
        //     fill: rectColor || '#333',
        //     // x and y should be minus width / 2 and height / 2 respectively to translate the center of the rect to the midPoint
        //     // x 和 y 分别减去 width / 2 与 height / 2，使矩形中心在 midPoint 上
        //     x: midPoint.x - 5,
        //     y: midPoint.y - 5,
        //   },
        //   name: 'rect-shape',
        // });

        // get the coordinate of the quatile on the path
        // 获取路径上的四分位点坐标
        const quatile = shape.getPoint(0.25);
        const quatileColor = cfg.quatileColor || '#333';
        // add a circle on the quatile of the path
        // 在四分位点上放置一个圆形
        // group.addShape('circle', {
        //   attrs: {
        //     r: 5,
        //     fill: quatileColor || '#333',
        //     x: quatile.x,
        //     y: quatile.y,
        //   },
        //   name: 'close-shape',
        // });
      },
      update: undefined,
    },
    'line',
  );
  let sourceAnchorIdx, targetAnchorIdx;
  const setNodesState = (id?) => {
    // 将其他节点的active状态去掉
    graph.getNodes().forEach(node => {
      if (node._cfg.id == id) {
        graph.setItemState(node, 'active', true);
        currentItem.value = node.getModel();
        graph.updateItem(node, {
          style: {
            stroke: '#5F95FF',
            // 边框加粗
            lineWidth: 2,
          },
        });
      } else {
        graph.setItemState(node, 'active', false);
        graph.updateItem(node, {
          style: {
            stroke: '#5F95FF',
            lineWidth: 1,
          },
        });
      }
    });
  };

  const updateItem = target => {
    const item = graph.findById(target.id);
    graph.updateItem(item, {
      label: target.label,
    });
  };
  // 初始化画布
  const initGraph = () => {
    const plugins: any[] = [];
    const background = new ZBackground();
    const grid = new G6.Grid();
    plugins.push(grid);
    // plugins.push(background)
    const _graph = new G6.Graph({
      plugins,
      container: 'sketchpad',
      width: graphRef.value.clientWidth,
      height: graphRef.value.clientHeight,
      modes: {
        default: [
          // config the shouldBegin for drag-node to avoid node moving while dragging on the anchor-point circles
          {
            type: 'drag-node',
            shouldBegin: e => {
              if (e.target.get('name') === 'anchor-point') return false;
              return true;
            },
          },
          // config the shouldBegin and shouldEnd to make sure the create-edge is began and ended at anchor-point circles
          {
            type: 'create-edge',
            trigger: 'drag', // set the trigger to be drag to make the create-edge triggered by drag
            shouldBegin: e => {
              // avoid beginning at other shapes on the node
              if (e.target && e.target.get('name') !== 'anchor-point') return false;
              sourceAnchorIdx = e.target.get('anchorPointIdx');
              e.target.set('links', e.target.get('links') + 1); // cache the number of edge connected to this anchor-point circle
              return true;
            },
            shouldEnd: e => {
              // avoid ending at other shapes on the node
              if (e.target && e.target.get('name') !== 'anchor-point') return false;
              if (e.target) {
                targetAnchorIdx = e.target.get('anchorPointIdx');
                e.target.set('links', e.target.get('links') + 1); // cache the number of edge connected to this anchor-point circle
                console.log('shouldEnd')
                return true;
              }
              targetAnchorIdx = undefined;
              return true;
            },
          },
          'lasso-select',
        ],
        dragLasso: [
          {
            type: 'lasso-select',
            delegateStyle: {
              fill: '#f00',
              fillOpacity: 0.05,
              stroke: '#f00',
              lineWidth: 1,
            },
            onSelect: (nodes, edges) => {
              console.log('onSelect', nodes, edges);
            },
            trigger: 'drag',
          },
          'drag-node',
        ],
      },
      nodeStateStyles: {
        selected: {
          stroke: '#f00',
          lineWidth: 3,
        },
      },
      defaultNode: {
        type: 'rect-node',
        // style: {
        //   fill: '#eee',
        //   stroke: '#ccc',
        // },
        // linkPoints: {
        //   top: true,
        //   right: true,
        //   bottom: true,
        //   left: true,
        //   /* linkPoints' size, 8 by default */
        //   //   size: 5,
        //   /* linkPoints' style */
        //   //   fill: '#ccc',
        //   //   stroke: '#333',
        //   //   lineWidth: 2,
        // },
      },

      defaultEdge: {
        type: 'extra-shape-edge',
        style: {
          stroke: '#F6BD16',
          endArrow: true,
        },
        // size: 1,
        // color: '#e2e2e2',
        // style: {
        //   endArrow: {
        //     path: 'M 0,0 L 8,4 L 8,-4 Z',
        //     fill: '#e2e2e2',
        //   },
        //   radius: 20,
        // },
      },
    });
    graph = _graph;

    _graph.data(state.source);
    _graph.render();

    _graph.on('aftercreateedge', (e: any) => {
      console.log('aftercreateedge')
      // update the sourceAnchor and targetAnchor for the newly added edge
      _graph.updateItem(e.edge, {
        sourceAnchor: sourceAnchorIdx,
        targetAnchor: targetAnchorIdx,
      });

      // update the curveOffset for parallel edges
      const edges: any = _graph.save().edges;
      processParallelEdgesOnAnchorPoint(edges);
      _graph.getEdges().forEach((edge, i) => {
        _graph.updateItem(edge, {
          curveOffset: edges[i].curveOffset,
          curvePosition: edges[i].curvePosition,
        });
      });
    });

    // after drag from the first node, the edge is created, update the sourceAnchor
    _graph.on('afteradditem', e => {
      console.log('afteradditem')
      if (e.item && e.item.getType() === 'node') {
        _graph.updateItem(e.item, {
          type: 'rect-node',
        });
      } else if (e.item && e.item.getType() === 'edge') {
        _graph.updateItem(e.item, {
          sourceAnchor: sourceAnchorIdx,
          type: 'extra-shape-edge',
        });
      }
    });

    // if create-edge is canceled before ending, update the 'links' on the anchor-point circles
    _graph.on('afterremoveitem', (e: any) => {
      if (e.item && e.item.source && e.item.target) {
        const sourceNode = _graph.findById(e.item.source);
        const targetNode = _graph.findById(e.item.target);
        const { sourceAnchor, targetAnchor } = e.item;
        if (sourceNode && !isNaN(sourceAnchor)) {
          const sourceAnchorShape = sourceNode.getContainer().find(ele => ele.get('name') === 'anchor-point' && ele.get('anchorPointIdx') === sourceAnchor);
          sourceAnchorShape.set('links', sourceAnchorShape.get('links') - 1);
        }
        if (targetNode && !isNaN(targetAnchor)) {
          const targetAnchorShape = targetNode.getContainer().find(ele => ele.get('name') === 'anchor-point' && ele.get('anchorPointIdx') === targetAnchor);
          targetAnchorShape.set('links', targetAnchorShape.get('links') - 1);
        }
      }
    });

    // some listeners to control the state of nodes to show and hide anchor-point circles
    _graph.on('node:mouseenter', (e: any) => {
      console.log('hover');

      _graph.setItemState(e.item, 'showAnchors', true);
    });
    _graph.on('node:mouseleave', (e: any) => {
      console.log('leave');

      _graph.setItemState(e.item, 'showAnchors', false);
    });
    _graph.on('node:dragenter', (e: any) => {
      _graph.setItemState(e.item, 'showAnchors', true);
    });
    _graph.on('node:dragleave', (e: any) => {
      // _graph.setItemState(e.item, 'showAnchors', false);
    });
    _graph.on('node:dragstart', (e: any) => {
      _graph.setItemState(e.item, 'showAnchors', true);
      setNodesState(e.item.getModel().id);
    });
    _graph.on('node:dragout', (e: any) => {
      _graph.setItemState(e.item, 'showAnchors', false);
    });

    _graph.on('edge:click', (e: any) => {
      const id = e.item.getModel().id;
      currentItem.value = e.item.getModel();

      _graph.setItemState(e.item, 'active', true);
      // 将其他节点的active状态去掉
      _graph.getEdges().forEach(edge => {
        if (edge.hasState('active') && edge.getModel().id !== id) {
          _graph.setItemState(edge, 'active', false);
        }
      });
      // 清空所有node节点的active状态
      setNodesState();
    });
    _graph.on('node:click', (e: any) => {
      console.log(e.item.getModel(), 'node');
      setNodesState(e.item.getModel().id);
      // 清空所有edge节点的active状态
      _graph.getEdges().forEach(edge => {
        if (edge.hasState('active')) {
          _graph.setItemState(edge, 'active', false);
        }
      });
    });

    _graph.on('close-shape:click', (e: any) => {
      console.log('rect-shape:click');
      console.log(e.item);
      _graph.removeItem(e.item);
      setNodesState();
    });

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

  return {
    addNode,
    delItem,
    updateItem,
    currentItem,
    graph: computed(() => graph),
  };
};


export function useEditorOuter(){
  const getData = ()=>{
   return {
      nodes:graph.getNodes(),
      edges:graph.getEdges(),
   }
  }
  return {
    getData,
  }
}
