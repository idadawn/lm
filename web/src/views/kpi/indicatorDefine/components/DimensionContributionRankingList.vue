<template>
  <div class="dcr-container-list">
    <a-tabs v-model:activeKey="activeKey" :tab-position="mode" :style="{ minHeight: '200px' }" @tabScroll="callback">
      <a-tab-pane v-for="(item, index) in tabData" :key="index" :tab="item.name">
        <div class="flex justify-between mb-2">
          <div class="dcr-title">门店城市</div>
          <div class="dcr-toggle">
            <BarChartOutlined
              :class="['px-2', 'py-1', activeType === DimensionContributionTypeEnum.Chart ? 'bg-white' : '']"
              @click="showChartTable(DimensionContributionTypeEnum.Chart)" />
            <TableOutlined
              :class="['px-2', 'py-1', activeType === DimensionContributionTypeEnum.Table ? 'bg-white' : '']"
              @click="showChartTable(DimensionContributionTypeEnum.Table)" />
          </div>
        </div>
        <div>
          <template v-if="activeType === DimensionContributionTypeEnum.Chart">
            <div class="flex">
              <div class="w-1/2">
                <DimensionContributionRankingChart :type="ContributionTypeEnum.Down" :chart-data="item.left" />
              </div>
              <div class="w-1/2">
                <DimensionContributionRankingChart :type="ContributionTypeEnum.Up" :chart-data="item.right" />
              </div>
            </div>
          </template>
          <template v-else>
            <BasicTable @register="registerTable">
              <template #bodyCell="{ column, record }">
                <template v-if="column.key === 'name'">
                  <div class="font-semibold cursor-pointer text-rose-600">
                    <ArrowDownOutlined />{{ record?.name }}
                    <ArrowUpOutlined />
                  </div>
                </template>
              </template>
            </BasicTable>
          </template>
        </div>
      </a-tab-pane>
    </a-tabs>
  </div>
</template>

<script setup lang="ts">
  import { ref, toRef, toRefs, watch, reactive } from 'vue';
  // import { , ref, toRefs, , computed, nextTick, onMounted } from 'vue';
  import { BarChartOutlined, TableOutlined, ArrowDownOutlined, ArrowUpOutlined } from '@ant-design/icons-vue';
  import type { TabsProps } from 'ant-design-vue';
  import { DimensionContributionTypeEnum, ContributionTypeEnum } from '/@/enums/publicEnum';
  import DimensionContributionRankingChart from './DimensionContributionRankingChart.vue';
  import { BasicTable, useTable, BasicColumn } from '/@/components/Table';

  defineOptions({
    name: 'DimensionContributionRankingList',
  });

  const props = defineProps<{
    tableData: any;
  }>();

  const { tableData } = toRefs(props);

  const mode = ref<TabsProps['tabPosition']>('left');
  const activeKey = ref();
  const activeType = ref(DimensionContributionTypeEnum.Chart);

  const callback: TabsProps['onTabScroll'] = val => {
    console.log(val);
  };

  const columns: BasicColumn[] = [
    { title: '维度值', dataIndex: 'dimension' },

    { title: '贡献值', dataIndex: 'coefficient' },
    { title: '变化值', dataIndex: 'createdTime', format: 'date|YYYY-MM-DD HH:mm' },
  ];

  const tabData = ref<any>([]);

  watch(
    () => props,
    (newValue, oldValue) => {
      // console.log('========', newValue);
      // state.tableDataMsg = newValue.tableDataMsg;
      // state.leftMenu = newValue.leftMenu;
      // activeKey.value = leftMenu[0];
      newValue.tableData.map(v => {
        const tab = {
          name: v.dimension,
          coefficient: v.coefficient,
          left: {
            xAxis: [] as any[],
            yAxis: [] as any[],
          },
          right: {
            xAxis: [] as any[],
            yAxis: [] as any[],
          },
        };

        v.attribution_list.forEach((res: any) => {
          if (res.dimension_value < 0) {
            //小于0，反向贡献
            tab.right.xAxis.push(res.dimension_value);
            tab.right.yAxis.push(res.base_period_value);
          } else {
            //正向贡献，添加在左边
            tab.left.xAxis.push(res.dimension_value);
            tab.left.yAxis.push(res.base_period_value);
          }
        });

        tabData.value.push(tab);
      });
    },
    { deep: true },
  );
  const [registerTable, { reload }] = useTable({
    api: () => {
      return Promise.resolve({
        code: 200,
        data: [],
        message: 'xxxx',
      });
    },
    columns,
    showIndexColumn: false,
    showTableSetting: false,
    useSearchForm: false,
    afterFetch: data => {
      console.log(data);
    },
  });

  const showChartTable = type => {
    if (type === DimensionContributionTypeEnum.Chart) {
      activeType.value = DimensionContributionTypeEnum.Chart;
    }

    if (type === DimensionContributionTypeEnum.Table) {
      activeType.value = DimensionContributionTypeEnum.Table;
    }
  };
</script>

<style lang="less" scoped>
  .dcr-container-list {
    .dcr-title {
      font-weight: 600;
      font-size: 14px;
      line-height: 22px;
      color: #546174;
    }
    .dcr-toggle {
      font-size: 14px;
      background: #ecf0f8;
      padding: 4px;
      border-radius: 6px;

      span {
        color: #8b99ae;
        border-radius: 6px;
      }
    }
  }
</style>
