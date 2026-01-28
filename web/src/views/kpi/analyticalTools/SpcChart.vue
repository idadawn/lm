<template>
  <ScrollContainer>
    <spin :spinning="spinning">
      <a-form class="w-full" ref="formRef" :model="formState" :label-col="labelCol" :rules="rules" name="SpcForm">
        <a-row :gutter="16" class="custion-mx">
          <a-col :span="6">
            <a-form-item label="日期" name="dateTime">
              <range-picker
                v-model:value="formState.dateTime"
                show-time
                :format="dateFormat"
                :placeholder="['开始时间', '结束时间']"
                @change="onRangeChange" />
            </a-form-item>
          </a-col>
          <a-col :span="6">
            <a-form-item label="作业" name="material">
              <a-select v-model:value="formState.material" placeholder="材料">
                <a-select-option :value="item.value" v-for="item in materialOptions" :key="item.value">
                  {{ item.label }}
                </a-select-option>
              </a-select>
            </a-form-item>
          </a-col>
          <a-col :span="6">
            <a-form-item label="温度" name="temperature">
              <a-select v-model:value="formState.temperature" placeholder="温度">
                <a-select-option :value="item.value" v-for="item in temperatureOptions" :key="item.value">
                  {{ item.label }}
                </a-select-option>
              </a-select>
            </a-form-item>
          </a-col>
          <a-col :span="6">
            <a-form-item label="时间" name="refiningTime">
              <a-select v-model:value="formState.refiningTime" placeholder="时间">
                <a-select-option :value="item.value" v-for="item in refiningTimeOptions" :key="item.value">
                  {{ item.label }}
                </a-select-option>
              </a-select>
            </a-form-item>
          </a-col>
          <a-col :span="6">
            <a-form-item label="分析图形" name="chartType">
              <a-select v-model:value="formState.chartType" placeholder="分析图形">
                <a-select-option :value="item.value" v-for="item in chartTypeOptions" :key="item.value">
                  {{ item.label }}
                </a-select-option>
              </a-select>
            </a-form-item>
          </a-col>
          <a-col :span="6">
            <a-form-item label=" " :colon="false" name="operation">
              <a-button type="primary" @click="onSearch"> 查询图表</a-button>
            </a-form-item>
          </a-col>
        </a-row>
      </a-form>
      <!-- <a-table
        class="custom-table"
        :columns="columns"
        :row-key="record => record.name"
        :loading="loading"
        :data-source="tableData"
        @change="handleTableChange">
        <template #bodyCell="{ column, text, record }">
          <template v-if="['x1', 'x2', 'x3', 'x4', 'x5', 'x6'].includes(column.dataIndex)">
            <div>
              {{ text }}
            </div>
          </template>
          <template v-else-if="column.dataIndex === 'operation'">
            <div class="editable-row-operations">
              <span v-if="editableData[record.key]">
                <a-tag color="#87d068" @click="save(record.key)">Save</a-tag>
                <a-tag @click="cancel(record.key)">Cancel</a-tag>
              </span>
              <span v-else>
                <a-tag color="orange" @click="edit(record.key)">Edit</a-tag>
              </span>
            </div>
          </template>
        </template>
      </a-table> -->
      <div ref="spcChart" class="spc-chart"></div>
    </spin>
  </ScrollContainer>
</template>

<script setup lang="ts">
  import { ref, Ref, reactive, watch, h } from 'vue';
  import type { UnwrapRef } from 'vue';
  import { useECharts } from '/@/hooks/web/useECharts';
  import { MlOptions, TtOptions, RtOptions, CtOptions } from './config/mock';
  import { ChartTypeEnum, SpcColumn } from './config/const';
  import type { SpcFormState, DataItem, SpcLineType } from './config/types';
  import type { Rule } from 'ant-design-vue/es/form';
  import { message, RangePicker, Spin } from 'ant-design-vue';
  import type { Dayjs } from 'dayjs';
  import dayjs, { unix } from 'dayjs';
  import { cloneDeep } from 'lodash-es';
  import { ScrollContainer } from '/@/components/Container';
  import { find } from 'lodash-es';
  import { spcOption, createSampleData, xRControl, xSControl, axRControl, xmRControl } from './config/utils';
  import { defHttp } from '/@/utils/http/axios';
  import { ResultEnum } from '/@/enums/httpEnum';
  import { bignumber, number } from 'mathjs';
  import { LoadingOutlined } from '@ant-design/icons-vue';

  const spinning = ref(false);

  const spcChart = ref<HTMLDivElement | null>(null);

  const formRef = ref();

  const labelCol = { style: { width: '100px' } };

  const dateFormat = ref('YYYY-MM-DD HH:mm:ss');

  const formState = reactive<SpcFormState>({
    dateTime: ['', ''],
    material: '',
    temperature: '1000',
    refiningTime: '200',
    chartType: ChartTypeEnum.meanExtreme, // 图表类型
  });

  // 接口获取初始数据
  const spcSource = ref();
  const rules: Record<string, Rule[]> = {
    material: [{ message: '请选择材料', trigger: 'change' }],
    temperature: [{ message: '请选择温度', trigger: 'change' }],
    refiningTime: [{ message: '请选择炼制时间', trigger: 'change' }],
    chartType: [{ message: '请选择分析图形', trigger: 'change' }],
  };

  // 材料选项
  const materialOptions = ref(MlOptions);

  // 温度选项
  const temperatureOptions = ref(TtOptions);

  // 炼制时间选项
  const refiningTimeOptions = ref(RtOptions);

  // 分析图形选项
  const chartTypeOptions = ref(CtOptions);

  const columns = ref<{ title: string; dataIndex: string; width?: string }[]>([
    {
      title: '样本',
      dataIndex: 'sampleName',
      width: '10%',
    },
    {
      title: '时间',
      dataIndex: 'time',
      width: '15%',
    },
    {
      title: 'operation',
      dataIndex: 'operation',
      width: '15%',
    },
  ]);

  const { setOptions } = useECharts(spcChart as Ref<HTMLDivElement>);

  const apiInfo = () => {
    spinning.value = true;
    const params = {
      type: formState.material,
      temperature: formState.temperature,
      startTime: formState.dateTime[0],
      endTime: formState.dateTime[1],
      makeMin: formState.refiningTime,
      count: 5,
    };
    return defHttp.get({ url: '/api/kpi/v1/analysisdata/rb' });
  };

  const tableData = ref<DataItem[]>();

  /**
   * @description 判断是否存在Spc计算的列
   */
  const isExistSpcColumn = () => {
    const keys = Object.keys(SpcColumn);
    const exist = columns.value.find(item => keys.includes(item.dataIndex));
    return exist ? true : false;
  };

  /**
   * @function initSpcComputeData
   * @description 初始化Spc计算数据
   * @description 表格column头部
   * @description 表格数据
   * @returns
   */
  const initSpcComputeData = () => {
    // Charts的标题
    const titles = find(CtOptions, { value: formState.chartType }) as SpcLineType;
    // 是否已经插入过计算的列
    if (isExistSpcColumn()) {
      // 删除样本列
      columns.value.splice(2, columns.value.length - 3);
    }

    switch (formState.chartType) {
      case ChartTypeEnum.meanExtreme:
        // 重新计算数据
        // tableDataFormat();
        // 表格标题计算列
        // columns.value.splice(-1, 0, SpcColumn.mean, SpcColumn.range);
        // 执行图表
        return xRControl(titles, tableData.value!);
      case ChartTypeEnum.meanStandard:
        // tableDataFormat();
        columns.value.splice(-1, 0, SpcColumn.mean, SpcColumn.standard);
        return xSControl(titles, tableData.value!);
      case ChartTypeEnum.medianExtreme:
        // tableDataFormat();
        columns.value.splice(-1, 0, SpcColumn.median, SpcColumn.range);
        return axRControl(titles, tableData.value!);
      case ChartTypeEnum.individualmovingRange:
        // tableDataIndividualmovingRange();
        columns.value.splice(-1, 0, SpcColumn.individual, SpcColumn.movingRange);
        return xmRControl(titles, tableData.value!);
      default:
    }
  };

  /**
   * @function initSpc
   * @description 初始化Spc图表
   * @return
   * @Date：2023-11-28
   * @param source
   * */
  const initSpcChart = source => {
    const titles = find(CtOptions, { value: formState.chartType }) as SpcLineType;
    const topxAxis = source.average.axis.map((_res, index) => index + 1);
    const bottomxAxis = source.average.axis.map((_res, index) => index + 1);
    const dataSet = {
      title: titles.data,
      xAxis: [topxAxis, bottomxAxis],
      data: [source.average.axis, source.range.axis],
      markLine: [
        {
          UCL: source.average.uCL,
          CL: source.average.cL,
          LCL: source.average.lCL,
        },
        {
          UCL: source.range.uCL,
          CL: source.range.cL,
          LCL: source.range.lCL,
        },
      ],
    };
    const option = spcOption(dataSet) as any;
    setOptions(option);
  };

  /**
   * @description 数据格式化转换
   * @return
   * @date: 2023-12-06
   * @author：leixus
   */
  const tableDataFormat = () => {
    const customTableData: DataItem[] = [];
    for (let i = 0; spcSource.value.length > i; i++) {
      const sampleItem = {};
      for (let [index, value] of spcSource.value[i].data.entries()) {
        if (i === 0) {
          const addColumn = {
            title: `x${index + 1}`,
            dataIndex: `x${index + 1}`,
          };
          columns.value.splice(-1, 0, addColumn);
        }
        sampleItem[`x${index + 1}`] = value;
      }
      const timer = number(`${spcSource.value[i].time}000`);
      spcSource.value[i].time = dayjs(number(timer)).format('YYYY-MM-DD HH:mm');
      customTableData.push({
        ...spcSource.value[i],
        ...sampleItem,
      });
    }
    tableData.value = customTableData;
  };

  /**
   * @description 数据格式化转换
   * @description 单值-移动极差值
   * @return
   * @date: 2023-12-06
   * @author: leixus
   */
  const tableDataIndividualmovingRange = () => {
    const customTableData: DataItem[] = [];
    let count = 1;
    for (let i = 0; spcSource.value.length > i; i++) {
      for (let value of spcSource.value[i].data.values()) {
        customTableData.push({
          ...spcSource.value[i],
          key: crypto.randomUUID(),
          individual: value,
          name: `样本${count++}`,
        });
      }
      const timer = number(`${spcSource.value[i].time}000`);
      spcSource.value[i].time = dayjs(timer).format('YYYY-MM-DD HH:mm');
    }
    tableData.value = customTableData;
  };

  const editableData: UnwrapRef<Record<string, DataItem>> = reactive({});

  const onRangeChange = (value: [Dayjs, Dayjs], dateString: [string, string]) => {
    // const [startDate, endDate] = dateString;
    //
    // formState.dateTime = [dayjs(startDate).unix(), dayjs(endDate).unix()];
  };

  // const handleTableChange = () => {};

  // const onRangeOk = (value: [Dayjs, Dayjs]) => {
  //   
  // };

  /**
   * @description 不同条件下效果图
   */
  const onSearch = () => {
    setTimeout(() => {
      message.destroy();
    }, 3000);
    formRef.value
      .validate()
      .then(() => {
        apiInfo()
          .then(res => {
            if (res.code === ResultEnum.SUCCESS) {
              // spcSource.value = res.data;
              // const result = initSpcComputeData();
              initSpcChart(res.data);
            }
          })
          .finally(() => {
            spinning.value = false;
          });
      })
      .catch(error => {
      });
  };

  const edit = (key: string) => {
    editableData[key] = cloneDeep(tableData.value!.filter(item => key === item.key)[0]);
  };
  const save = (key: string) => {
    Object.assign(tableData.value!.filter(item => key === item.key)[0], editableData[key]);
    delete editableData[key];
  };
  const cancel = (key: string) => {
    delete editableData[key];
  };
</script>

<style scoped lang="less">
  .custion-mx {
    margin-right: 0 !important;
    margin-left: 0 !important;
  }

  .custom-table {
    :deep(.ant-table-thead > tr > th),
    :deep(.ant-table-tbody > tr > td),
    :deep(.ant-table tfoot > tr > th),
    :deep(.ant-table tfoot > tr > td) {
      padding: 8px 8px !important;
    }
  }

  .spc-chart {
    width: 100%;
    height: 1000px;
    margin-bottom: 4px;
  }
</style>
