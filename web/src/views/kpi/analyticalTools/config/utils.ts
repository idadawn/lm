import type { DataItem, MarkLineData } from './types';
import { DataSet } from './types';
import { MarkLineDataLineStyle, Reservedfixed, SpcA2, SpcA3, SpcB3, SpcB4, SpcD3, SpcD4, SpcE2 } from './const';
import {
  abs,
  add,
  ceil,
  divide,
  exp,
  max,
  mean,
  median,
  min,
  multiply,
  number,
  pow,
  sqrt,
  std,
  subtract,
  sum,
  variance,
} from 'mathjs';

/**
 * 正态分布公式计算
 * @param source
 * @param [isSample=true] 是否为样本 | 总体， 默认样本
 */
export function normalDistributionMethod(source, isSample = true) {
  const dataSet: DataSet = {
    source,
    // 数据出现坐标
    dataAfterX: [] as any[],
    dataAfterSpacing: [] as any[],
    // 数据出现频率
    dataAfterY: [] as number[],
    normalDistribution: [] as number[],
    // 偏差
    deviation: [] as number[],
    length: source.length,
    sum: 0,
    average: 0,
    max: 0,
    min: 0,
    // 方差
    variance: 0,
    // 标准差
    standardDeviation: 0,
    group: 0,
    groupSpacing: 0,
    range: 0,
    // 一倍标准差范围
    standarDevRangeOfOne: '',
    // 二倍标准差范围
    standarDevRangeOfTwo: '',
    // 三倍标准差范围
    standarDevRangeOfThree: '',
  };

  if (source.length) {
    // 计算数据之和
    dataSet.sum = sum(source);

    // 计算平均值
    dataSet.average = mean(source);

    // 计算最大值
    dataSet.max = max(source);

    // 计算最小值
    dataSet.min = min(source);

    // 计算样本方差 | 总体方差
    dataSet.variance = isSample ? (variance(source) as number) : (variance(source, 'uncorrected') as number);

    // 计算标准差
    dataSet.standardDeviation = isSample ? std(source) : std(source, 'uncorrected');

    // 一倍标准差范围
    dataSet.standarDevRangeOfOne = `${dataSet.average - dataSet.standardDeviation}~${
      dataSet.average + dataSet.standardDeviation
    }`;
    // 二倍标准差范围
    dataSet.standarDevRangeOfTwo = `${dataSet.average - 2 * dataSet.standardDeviation}~${
      dataSet.average + 2 * dataSet.standardDeviation
    }`;
    // 三倍标准差范围
    dataSet.standarDevRangeOfThree = `${dataSet.average - 3 * dataSet.standardDeviation}~${
      dataSet.average + 3 * dataSet.standardDeviation
    }`;

    // 将数据分成若干组，并做好记号。分组的数量在5－12之间较为适宜,
    // 组数：其中组数是这组数组被分成组的个数，是对数据个数开方然后向上取整求出
    // 计算组距的宽度。用最大值和最小值之差去除组数，求出组距的宽度
    // 组距：组距是每一组数两个端点的差，用极差除以组数求得
    // 计算各组的界限位
    // 各组的界限位可以从第一组开始依次计算，第一组的下界为最小值减去最小测定单位的一半，第一组的上界为其下界值加上组距
    // 第二组的下界限位为第一组的上界限值，第二组的下界限值加上组距，就是第二组的上界限位，依此类推
    // [0.15, 0.25]  大于等于0.15，小于0.25
    dataSet.group = ceil(sqrt(dataSet.length)) as number;
    dataSet.range = subtract(max(source), min(source));
    dataSet.groupSpacing = divide(dataSet.range, dataSet.group);

    let cacheMin = dataSet.min;
    let cacheMax = 0;
    for (let i = 0; i < dataSet.group; i++) {
      cacheMax = preservedDecimalPlacesNumber(add(cacheMin, dataSet.groupSpacing));
      const afterX = number(divide(add(cacheMin, cacheMax), 2));
      dataSet.dataAfterX.push(preservedDecimalPlacesNumber(afterX));
      const spacing = `${cacheMin} - ${cacheMax}`;
      dataSet.dataAfterSpacing.push(spacing);

      const countSpacing = source.filter(res => {
        return number(res) >= cacheMin && number(res) < cacheMax;
      }).length;
      dataSet.dataAfterY.push(countSpacing);
      cacheMin = cacheMax;
    }

    // // 计算正态分布 Y | 右侧
    for (let i = 0; i < dataSet.dataAfterX.length; i++) {
      const x = Number(dataSet.dataAfterX[i]);
      const a = dataSet.standardDeviation;
      const u = dataSet.average;

      // const y = (1 / (Math.sqrt(2 * Math.PI) * a)) * Math.exp((-1 * ((x - u) * (x - u))) / (2 * a * a));
      const o = divide(1, multiply(sqrt(multiply(2, Math.PI)), a)) as number;

      const w = multiply(-1, pow(subtract(x, u), 2)) as number;
      const e = multiply(2, pow(a, 2)) as number;
      const t = exp(divide(w, e));

      const y = multiply(o, t);

      dataSet.normalDistribution.push(y);
    }

    // // 数据整理。原数据整理为：{数据值 : 数据频率}
    // const copySource = source.toSorted();
    // const dataAfter = {};
    // for (let i = 0; i < copySource.length; i++) {
    //   // 这里保留 1 位小数
    //   let key = parseFloat(copySource[i]).toFixed(1);
    //   //这个判断用来处理保留小数位后 -0.0 和 0.0 判定为不同 key 的 bug
    //   if (key === 'NaN' || parseFloat(key) === 0) key = '0.0';
    //   if (dataAfter[key]) dataAfter[key] += 1;
    //   else dataAfter[key] = 1;
    // }
    //
    // // 计算坐标 X
    // dataSet.dataAfterX = Object.keys(dataAfter)
    //   .sort((a, b) => Number(a) - Number(b))
    //   .map(t => parseFloat(t).toFixed(1));
    //
    // // 计算频率 Y | 频率集合 | 左侧
    // for (let i = 0; i < dataSet.dataAfterX.length; i++) {
    //   dataSet.dataAfterY.push(dataAfter[dataSet.dataAfterX[i]]);
    // }
    //
    // // 计算正态分布 Y | 右侧
    // for (let i = 0; i < dataSet.dataAfterX.length; i++) {
    //   const x = Number(dataSet.dataAfterX[i]);
    //   const a = dataSet.standardDeviation;
    //   const u = dataSet.average;
    //   const y = (1 / (Math.sqrt(2 * Math.PI) * a)) * Math.exp((-1 * ((x - u) * (x - u))) / (2 * a * a));
    //   dataSet.normalDistribution.push(y);
    //   // 正态分布峰值，用于验证 ，目前没有用到,，随便写的
    //   if (x == 11.8) console.log(y);
    // }
  }
  return dataSet;
}

/**
 * @description 保留小数位数
 * */

export function preservedDecimalPlacesNumber(x): number {
  return divide(ceil(multiply(x, 1000)), 1000) as number;
}

/**
 * 正态分布图表展示
 */
export function normalDistributionOption(dataSet) {
  // let [lowOne, upOne] = dataSet.standarDevRangeOfOne.split('~');
  // lowOne = parseFloat(lowOne).toFixed(1);
  // upOne = parseFloat(upOne).toFixed(1);
  // let [lowTwo, upTwo] = dataSet.standarDevRangeOfTwo.split('~');
  // lowTwo = parseFloat(lowTwo).toFixed(1);
  // upTwo = parseFloat(upTwo).toFixed(1);
  // let [lowThree, upThree] = dataSet.standarDevRangeOfThree.split('~');
  // lowThree = parseFloat(lowThree).toFixed(1);
  // upThree = parseFloat(upThree).toFixed(1);

  return {
    title: {
      text: '正态分布',
    },
    grid: {
      left: 50,
      right: 50,
    },
    tooltip: {
      trigger: 'item',
      // axisPointer: {
      //   type: 'cross',
      // },
      // formatter: params => {
      //   if (params.componentType === 'series') {
      //     return params.name + '<br>' + params.marker + params.seriesName + ' ' + params.data;
      //   } else if (params.componentType === 'markLine') {
      //     return '正态分布' + '<br>' + params.name + ' ' + params.value;
      //   }
      // },
    },
    toolbox: {
      show: true,
      feature: {
        dataView: { readOnly: false },
        saveAsImage: {},
      },
    },
    legend: {},
    // dataZoom: [
    //   {
    //     show: true,
    //     realtime: true,
    //     start: 30,
    //     end: 50,
    //     xAxisIndex: [0, 1]
    //   },
    //   {
    //     type: 'inside',
    //     realtime: true,
    //     start: 30,
    //     end: 50,
    //     xAxisIndex: [0, 1]
    //   }
    // ],
    xAxis: [
      {
        // name: '频数',
        axisTick: {
          alignWithLabel: false,
        },
        data: dataSet.xAxisHistogram,
      },
      {
        // name: '概率',
        show: false,
        // axisTick: {
        //   alignWithLabel: true,
        // },
        // position: 'bottom',
        // offset: 25,
        boundaryGap: false,
        data: dataSet.xAxis,
      },
    ],
    yAxis: [
      {
        type: 'value',
        name: '频数',
        position: 'left',
        // 网格线
        splitLine: {
          show: false,
        },
        axisLine: {
          show: true,
          lineStyle: {
            // color: 'orange',
          },
        },
        axisLabel: {
          formatter: '{value}',
        },
      },
      {
        type: 'value',
        name: '概率',
        position: 'right',
        // 网格线
        splitLine: {
          show: false,
        },
        axisLine: {
          show: true,
          lineStyle: {
            // color: 'black',
          },
        },
        axisLabel: {
          formatter: '{value}',
        },
      },
    ],
    series: [
      {
        name: '频率', // y 轴名称
        type: 'bar', // y 轴类型
        xAxisIndex: 0,
        yAxisIndex: 0,
        barGap: 0,
        barWidth: 27,
        itemStyle: {
          show: true,
          // color: 'rgba(255, 204, 0,.3)', //柱子颜色
          // borderColor: '#FF7F50', //边框颜色
        },
        data: dataSet.yAxisHistogram, // y 轴数据 -- 源数据
      },
      {
        name: '正态分布', // y 轴名称
        type: 'line', // y 轴类型
        // symbol: 'circle', //去掉折线图中的节点
        smooth: true, //true 为平滑曲线
        xAxisIndex: 1,
        yAxisIndex: 1,
        data: dataSet.yAxis, // y 轴数据 -- 正态分布
        itemStyle: {
          show: true,
          // color: 'rgba(255, 204, 0,.3)',
          // borderColor: '#FF7F50',
        },
        // 警示线
        markLine: {
          symbol: ['none'], // 箭头方向
          lineStyle: {
            // type: 'silent',
            // color: 'green',
          },
          itemStyle: {
            // show: true,
            // color: 'black',
          },
          label: {
            show: true,
            // position: 'insideEndTop',
          },
          // data: [
          //   {
          //     name: '一倍标准差-Min',
          //     xAxis: lowOne,
          //     lineStyle: {
          //       opacity: dataSet.min > Number(lowOne) ? 0 : 1,
          //       // color: 'red',
          //     },
          //     label: {
          //       show: !(dataSet.min > Number(lowOne)),
          //     },
          //   },
          //   {
          //     name: '一倍标准差-Max',
          //     xAxis: upOne,
          //     lineStyle: {
          //       opacity: dataSet.max < Number(upOne) ? 0 : 1,
          //     },
          //     label: {
          //       show: !(dataSet.max < Number(upOne)),
          //     },
          //   },
          //   {
          //     name: '二倍标准差-Min',
          //     xAxis: lowTwo,
          //     lineStyle: {
          //       opacity: dataSet.min > Number(lowTwo) ? 0 : 1,
          //     },
          //     label: {
          //       show: !(dataSet.min > Number(lowTwo)),
          //     },
          //   },
          //   {
          //     name: '二倍标准差-Max',
          //     xAxis: upTwo,
          //     lineStyle: {
          //       opacity: dataSet.max < Number(upTwo) ? 0 : 1,
          //     },
          //     label: {
          //       show: !(dataSet.max < Number(upTwo)),
          //     },
          //   },
          //   {
          //     name: '三倍标准差-Min',
          //     xAxis: lowThree,
          //     lineStyle: {
          //       opacity: dataSet.min > Number(lowThree) ? 0 : 1,
          //     },
          //     label: {
          //       show: !(dataSet.min > Number(lowThree)),
          //     },
          //   },
          //   {
          //     name: '三倍标准差-Max',
          //     xAxis: upThree,
          //     lineStyle: {
          //       opacity: dataSet.max < Number(upThree) ? 0 : 1,
          //     },
          //     label: {
          //       show: !(dataSet.max < Number(upThree)),
          //     },
          //   },
          //   // {
          //   //   name: '平均值',
          //   //   type: 'average',
          //   //   yAxis: dataSet.average,
          //   //   lineStyle: {
          //   //     color: 'red',
          //   //   },
          //   // },
          // ],
        },
      },
    ],
  };
}

/**
 * @description 生成[m, n]之间的随机数
 * @param {Number} m 最小值
 * @param {Number} n 最大值
 * @returns {Number}
 */
export function randomNum(m, n) {
  let result = Number((Math.random() * (n - m + 1) + m).toFixed(Reservedfixed));
  while (result > n) {
    result = Math.floor(Math.random() * (n - m + 1) + m);
  }
  return result;
}

/**
 * @description 生成日期
 */
export function createTime(timestamp) {
  const date = new Date(timestamp);
  var year = date.getFullYear();
  var month = date.getMonth() + 1 > 9 ? date.getMonth() + 1 : '0' + (date.getMonth() + 1);
  var day = date.getDate() > 9 ? date.getDate() : '0' + date.getDate();
  var hour = date.getHours() > 9 ? date.getHours() : '0' + date.getHours();
  var minute = date.getMinutes() > 9 ? date.getMinutes() : '0' + date.getMinutes();
  var second = date.getSeconds() > 9 ? date.getSeconds() : '0' + date.getSeconds();
  return `${year}-${month}-${day} ${hour}:${minute}:${second}`;
}

/**
 * @function createSampleData
 * @description 创建样本数据
 * @param {number} groupCount 样本组数量
 * @param {number} itemCount 每组数量
 * @param {number} min 数值最小值
 * @param {number} max 数值最大值
 * @returns {Array<any>} 样本数据
 */
export function createSampleData(groupCount = 100, itemCount = 5, min = 0, max = 100) {
  const arr: DataItem[] = [];
  let date = Date.now();
  createTime(date);
  for (let i = 0; i < groupCount; i++) {
    const sample: number[] = [];
    for (let j = 0; j < itemCount; j++) {
      sample.push(randomNum(min, max));
    }
    arr.push({
      key: crypto.randomUUID(),
      group: i + 1,
      name: `样本${i + 1}`,
      data: sample,
      time: createTime(date),
    });
    date += 60000;
  }

  return arr;
}

/**
 * @function xRControl
 * @description X-R 均值-极差控制图
 * @description 平均值、极差、CL、UCL、LCL等
 * @param {Array} titles X-R标题
 * @param {Array} tableData 数据来源，与表格数据保持一致
 * @returns {Array<any>} 样本数据
 */
export function xRControl(titles, tableData: DataItem[]) {
  const topxAxis: string[] = [],
    bottomxAxis: string[] = [],
    topData: number[] = [],
    bottomData: number[] = [],
    topMarkLine: MarkLineData = {},
    bottomMarkLine: MarkLineData = {};

  // 每组样本数量
  let itemCount = 0;

  // 计算平均值、极差值、每组样本数量
  for (let i = 0; i < tableData.length; i++) {
    itemCount = tableData[i].data.length;

    tableData[i].mean = Number(mean(tableData[i].data).toFixed(Reservedfixed));
    tableData[i].range = Number((max(tableData[i].data) - min(tableData[i].data)).toFixed(Reservedfixed));
    topxAxis.push(tableData[i].time);
    bottomxAxis.push(tableData[i].time);
    topData.push(tableData[i].mean || 0);
    bottomData.push(tableData[i].range || 0);
  }

  // 平均值的平均值
  const aAverage = Number(mean(topData).toFixed(Reservedfixed));
  // 极差值的平均值
  const rangeAverage = Number(mean(bottomData).toFixed(Reservedfixed));

  let A2 = itemCount ? SpcA2[itemCount - 2] : SpcA2[0];

  topMarkLine.CL = aAverage;
  topMarkLine.UCL = Number(add(aAverage, multiply(A2, rangeAverage)).toFixed(Reservedfixed));
  topMarkLine.LCL = Number(subtract(aAverage, multiply(A2, rangeAverage)).toFixed(Reservedfixed));

  let D4 = itemCount ? SpcD4[itemCount - 2] : SpcA2[0];
  let D3 = itemCount ? SpcD3[itemCount - 2] : SpcA2[0];

  bottomMarkLine.CL = rangeAverage;
  bottomMarkLine.UCL = Number(multiply(D4, rangeAverage).toFixed(Reservedfixed));
  bottomMarkLine.LCL = Number(multiply(D3, rangeAverage).toFixed(Reservedfixed));

  return {
    title: titles.data,
    xAxis: [topxAxis, bottomxAxis],
    data: [topData, bottomData],
    markLine: [topMarkLine, bottomMarkLine],
  };
}

/**
 * 均值-标准差控制图
 * @description 平均值、标准差、CL、UCL、LCL等
 * @param {Array} titles X-S标题
 * @param {Array} tableData 数据来源，与表格数据保持一致
 * @returns {Array<any>} 样本数据
 */
export function xSControl(titles, tableData: DataItem[]) {
  const topxAxis: string[] = [],
    bottomxAxis: string[] = [],
    topData: number[] = [],
    bottomData: number[] = [],
    topMarkLine: MarkLineData = {},
    bottomMarkLine: MarkLineData = {};

  // 每组样本数量
  let itemCount = 0;

  // 计算平均值、标准差值、每组样本数量
  for (let i = 0; i < tableData.length; i++) {
    itemCount = tableData[i].data.length;

    tableData[i].mean = Number(mean(tableData[i].data).toFixed(Reservedfixed));
    tableData[i].standard = Number((std(tableData[i].data) as unknown as number).toFixed(Reservedfixed));

    topxAxis.push(tableData[i].time);
    bottomxAxis.push(tableData[i].time);
    topData.push(tableData[i].mean || 0);
    bottomData.push(tableData[i].standard || 0);
  }

  // 平均值的平均值
  const aAverage = Number(mean(topData).toFixed(Reservedfixed));
  // 标准差值的平均值
  const standardAverage = Number(mean(bottomData).toFixed(Reservedfixed));

  // X图控制界限
  // CLx = aAverage 各组样本平均值的平均值
  // UCLx = aAverage + A3standardAverage
  // LCLx = aAverage - A3standardAverage
  let A3 = itemCount ? SpcA3[itemCount - 2] : SpcA3[0];
  topMarkLine.CL = aAverage;
  topMarkLine.UCL = Number(add(aAverage, multiply(A3, standardAverage)).toFixed(Reservedfixed));
  topMarkLine.LCL = Number(subtract(aAverage, multiply(A3, standardAverage)).toFixed(Reservedfixed));

  // S图控制界限
  // CLs = standardAverage 各组样本标准差的平均值
  // UCLs = B4standardAverage
  // LCLs = B3standardAverage
  let B4 = itemCount ? SpcB4[itemCount - 2] : SpcB4[0];
  let B3 = itemCount ? SpcB3[itemCount - 2] : SpcB3[0];
  bottomMarkLine.CL = standardAverage;
  bottomMarkLine.UCL = Number(multiply(B4, standardAverage).toFixed(Reservedfixed));
  bottomMarkLine.LCL = Number(multiply(B3, standardAverage).toFixed(Reservedfixed));

  return {
    title: titles.data,
    xAxis: [topxAxis, bottomxAxis],
    data: [topData, bottomData],
    markLine: [topMarkLine, bottomMarkLine],
  };
}

/**
 * 中位数-极差值控制图
 * @description 中位数、极差值、CL、UCL、LCL等
 * @param {Array} titles  X~ ,R标题
 * @param {Array} tableData 数据来源，与表格数据保持一致
 * @returns {Array<any>} 样本数据
 */
export function axRControl(titles, tableData: DataItem[]) {
  const topxAxis: string[] = [],
    bottomxAxis: string[] = [],
    topData: number[] = [],
    bottomData: number[] = [],
    topMarkLine: MarkLineData = {},
    bottomMarkLine: MarkLineData = {};

  // 每组样本数量
  let itemCount = 0;

  // 计算中位数、极差值、每组样本数量
  for (let i = 0; i < tableData.length; i++) {
    itemCount = tableData[i].data.length;

    tableData[i].median = Number(median(tableData[i].data).toFixed(Reservedfixed));
    tableData[i].range = Number((max(tableData[i].data) - min(tableData[i].data)).toFixed(Reservedfixed));
    topxAxis.push(tableData[i].time);
    bottomxAxis.push(tableData[i].time);
    topData.push(tableData[i].mean || 0);
    bottomData.push(tableData[i].range || 0);
  }

  // 中位数的平均值
  const aAverage = Number(mean(topData).toFixed(Reservedfixed));
  // 极差值的平均值
  const rangeAverage = Number(mean(bottomData).toFixed(Reservedfixed));

  let A2 = itemCount ? SpcA2[itemCount - 2] : SpcA2[0];
  topMarkLine.CL = aAverage;
  topMarkLine.UCL = Number(add(aAverage, multiply(A2, rangeAverage)).toFixed(Reservedfixed));
  topMarkLine.LCL = Number(subtract(aAverage, multiply(A2, rangeAverage)).toFixed(Reservedfixed));

  let D4 = itemCount ? SpcD4[itemCount - 2] : SpcA2[0];
  let D3 = itemCount ? SpcD3[itemCount - 2] : SpcA2[0];
  bottomMarkLine.CL = rangeAverage;
  bottomMarkLine.UCL = Number(multiply(D4, rangeAverage).toFixed(Reservedfixed));
  bottomMarkLine.LCL = Number(multiply(D3, rangeAverage).toFixed(Reservedfixed));

  return {
    title: titles.data,
    xAxis: [topxAxis, bottomxAxis],
    data: [topData, bottomData],
    markLine: [topMarkLine, bottomMarkLine],
  };
}

/**
 * 单值-移动极差值控制图
 * @description 单值、移动极差值、CL、UCL、LCL等
 * @param {Array} titles X-MR标题
 * @param {Array} tableData 数据来源，与表格数据保持一致
 * @returns {Array<any>} 样本数据
 */
export function xmRControl(titles, tableData: DataItem[]) {
  const topxAxis: string[] = [],
    bottomxAxis: string[] = [],
    topData: number[] = [],
    bottomData: number[] = [],
    topMarkLine: MarkLineData = {},
    bottomMarkLine: MarkLineData = {};

  // 每组样本数量
  let itemCount = 0;

  // 计算单值、移动极差值、每组样本数量
  for (let i = 0; i < tableData.length; i++) {
    itemCount = tableData[i].data.length;
    // tableData[i].individual = tableData[i].individual;
    if (i === 0) {
      tableData[i].movingRange = null;
    } else {
      // 取绝对值
      tableData[i].movingRange = abs(
        subtract(tableData[i].individual, tableData[i - 1].individual).toFixed(Reservedfixed),
      );
    }

    topxAxis.push(tableData[i].time);
    bottomxAxis.push(tableData[i].time);
    topData.push(tableData[i].individual || 0);
    bottomData.push(tableData[i].movingRange || 0);
  }

  // 单值的平均值
  const aAverage = Number(mean(topData).toFixed(Reservedfixed));
  // 移动极差值的平均值
  const movingRange = Number(mean(bottomData.slice(1)).toFixed(Reservedfixed));

  let E2 = itemCount ? SpcE2[itemCount - 2] : SpcA2[0];
  topMarkLine.CL = aAverage;
  topMarkLine.UCL = Number(add(aAverage, multiply(E2, movingRange)).toFixed(Reservedfixed));
  topMarkLine.LCL = Number(subtract(aAverage, multiply(E2, movingRange)).toFixed(Reservedfixed));

  let D4 = itemCount ? SpcD4[itemCount - 2] : SpcA2[0];
  let D3 = itemCount ? SpcD3[itemCount - 2] : SpcA2[0];
  bottomMarkLine.CL = movingRange;
  bottomMarkLine.UCL = Number(multiply(D4, movingRange).toFixed(Reservedfixed));
  bottomMarkLine.LCL = Number(multiply(D3, movingRange).toFixed(Reservedfixed));

  return {
    title: titles.data,
    xAxis: [topxAxis, bottomxAxis],
    data: [topData, bottomData],
    markLine: [topMarkLine, bottomMarkLine],
  };
}

export function spcOption(dataSet) {
  const {
    title: [topTitle, bottomTitle],
    xAxis: [topxAxis, bottomxAxis],
    data: [topData, bottomData],
    markLine: [topMarkLine, bottomMarkLine],
  } = dataSet;
  // 标记线数据
  const topMarkLineData = [] as any[],
    bottomMarkLineData = [] as any[];
  for (let [key, value] of Object.entries(topMarkLine)) {
    topMarkLineData.push({
      name: key,
      yAxis: value,
      lineStyle: MarkLineDataLineStyle[key],
    });
  }
  for (let [key, value] of Object.entries(bottomMarkLine)) {
    bottomMarkLineData.push({
      name: key,
      yAxis: value,
      lineStyle: MarkLineDataLineStyle[key],
    });
  }

  const getYAxisData = (min, max) => {
    // 控制分隔条数，
    const diff = subtract(max, min);
    return {
      max: add(max, diff),
      min: subtract(min, diff),
      // 分割成5等份
      interval: divide(subtract(add(max, diff), subtract(min, diff)), 5),
    };
  };

  const topYAxis = getYAxisData(min(topData), max(topData));

  const bottomYAxis = getYAxisData(min(bottomData), max(bottomData));

  return {
    title: [
      {
        // top: '2%',
        left: 'center',
        text: topTitle,
      },
      {
        top: '44%',
        left: 'center',
        text: bottomTitle,
      },
    ],
    grid: [
      {
        left: 60,
        right: 40,
        height: '32%',
      },
      {
        left: 60,
        right: 40,
        top: '50%',
        height: '32%',
      },
    ],
    tooltip: {
      trigger: 'axis',
      axisPointer: {
        animation: false,
      },
    },
    axisPointer: {
      link: [
        {
          xAxisIndex: 'all',
        },
      ],
    },
    // legend: {
    //   data: [topTitle, bottomTitle],
    //   left: 10,
    // },
    toolbox: {
      feature: {
        dataZoom: {
          yAxisIndex: 'none',
        },
        restore: {},
        saveAsImage: {},
      },
    },
    // 折线渐变
    // visualMap: [
    //   {
    //     show: false,
    //     type: 'continuous',
    //     seriesIndex: 0,
    //     min: 0,
    //     max: 200,
    //   },
    //   {
    //     show: false,
    //     type: 'continuous',
    //     seriesIndex: 1,
    //     dimension: 0,
    //     min: 0,
    //     max: 100,
    //   },
    // ],
    xAxis: [
      {
        type: 'category',
        boundaryGap: false,
        axisLine: { onZero: true },
        data: topxAxis,
      },
      {
        gridIndex: 1,
        type: 'category',
        boundaryGap: false,
        axisLine: { onZero: true },
        data: bottomxAxis,
        // position: 'top'
      },
    ],
    yAxis: [
      {
        // name: topTitle,
        type: 'value',
        interval: topYAxis.interval,
        // 设置y轴最大值
        min: topYAxis.min,
        // 设置y轴最小值
        max: topYAxis.max,
        axisLabel: {
          formatter: function (value) {
            return value.toFixed(4);
          },
        },
        // 网格线
        splitLine: {
          show: false,
        },
      },
      {
        // name: bottomTitle,
        gridIndex: 1,
        type: 'value',
        // inverse: true,
        interval: bottomYAxis.interval,
        // 设置y轴最大值
        min: bottomYAxis.min,
        // 设置y轴最小值
        max: bottomYAxis.max,
        axisLabel: {
          formatter: function (value) {
            return value.toFixed(4);
          },
        },
        // 网格线
        splitLine: {
          show: false,
        },
      },
    ],
    // dataZoom: [
    //   {
    //     gridIndex: 0,
    //     xAxisIndex: [0, 1], // 对应网格的索引
    //     top: '43%',
    //     type: 'slider',
    //     start: 0,
    //     end: 20,
    //     // zoomLock: true
    //   },
    //   {
    //     gridIndex: 1,
    //     xAxisIndex: [1], // 对应网格的索引
    //     bottom: 20,
    //     type: 'slider',
    //     start: 0,
    //     end: 20,
    //     // zoomLock: true
    //   },
    // ],
    series: [
      {
        name: topTitle,
        type: 'line',
        data: topData,
        markLine: {
          symbol: ['none', 'none'],
          label: {
            formatter: '{b} : {c}',
            position: 'insideEndTop',
          },
          precision: 3,
          data: topMarkLineData,
        },
      },
      {
        name: bottomTitle,
        type: 'line',
        xAxisIndex: 1,
        yAxisIndex: 1,
        data: bottomData,
        markLine: {
          symbol: ['none', 'none'],
          label: {
            formatter: '{b} : {c}',
            position: 'insideEndTop',
          },
          precision: 3,
          data: bottomMarkLineData,
        },
      },
    ],
  };
}
