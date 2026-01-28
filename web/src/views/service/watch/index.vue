<template>
  <div class="page-content-wrapper">
    <div class="page-content-wrapper-center">
      <div class="page-content-wrapper-content" style="background: #fff">
        <div class="gutter-div">
          <div class="gutter-box">
            <!-- 获取所有采集器及隶属标签 -->
            <h3>采集器及隶属标签</h3>
            <div class="gutter-tree">
              <a-tree
                checkable
                defaultExpandAll
                :selectable="false"
                v-model:checkedKeys="allCollectorTags_checkedKeys"
                v-if="allCollectorTags_treeData.length"
                :tree-data="allCollectorTags_treeData"
                @check="allCollectorTags_onCheck"
                :fieldNames="{ children: 'tagInfos', title: 'tagName', key: 'tagId' }">
              </a-tree>
            </div>
            <a-button class="gutter-btn" type="primary" @click="addTags">添加</a-button>
          </div>
          <div class="gutter-box">
            <!-- 获取服务观察列表 -->
            <h3>服务观察列表</h3>
            <div class="gutter-tree">
              <a-tree
                checkable
                defaultExpandAll
                :selectable="false"
                v-model:checkedKeys="watchlist_checkedKeys"
                v-if="watchlist_treeData.length"
                :tree-data="watchlist_treeData"
                @check="watchlist_onCheck"
                :fieldNames="{ children: 'tagInfos', title: 'tagName', key: 'tagId' }">
              </a-tree>
            </div>
            <a-button class="gutter-btn" type="primary" @click="removeTags">移除</a-button>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script lang="ts" setup>
  import { ref, reactive, watch } from 'vue';
  import { BasicTable, useTable, TableAction, BasicColumn, ActionItem } from '/@/components/Table';
  import { serviceAllCollectorTags, serviceWatchlist, serviceWatch, serviceUnwatch } from '/@/api/service';
  import { Modal as AModal } from 'ant-design-vue';
  import { useMessage } from '/@/hooks/web/useMessage';
  import { useRouter } from 'vue-router';
  import type { TreeProps } from 'ant-design-vue';

  defineOptions({
    name: 'watch',
  });

  const { createMessage } = useMessage();
  const router = useRouter();
  const state = reactive<any>({
    parentId: router.currentRoute.value.params.id,
  });

  //-----start--- 公共js ------------
  // 分组函数
  const groupByTagIndex = data => {
    return data.reduce((result, item) => {
      const tagIndex = item.tagIndex;
      if (!result[tagIndex]) {
        result[tagIndex] = [];
      }
      result[tagIndex].push(item);
      return result;
    }, {});
  };

  // 动态提取函数
  const extractTagIndexes = groupedData => {
    const watchList = {};
    for (const tagIndex in groupedData) {
      if (Object.hasOwnProperty.call(groupedData, tagIndex)) {
        const group = groupedData[tagIndex];
        const tagIds = group.map(item => item.tagId);
        watchList[tagIndex.replace('tagIndex_', '')] = tagIds;
      }
    }
    return watchList;
  };
  //-----end--- 公共js ------------

  // -------------采集器及隶属标签---------------------
  const allCollectorTags_checkedKeys = ref([]); //选中
  const allCollectorTags_treeData: TreeProps['treeData'] = ref([]);
  const allCollectorTags_checkedNodes = ref([]); //所有选中的数据

  // 获取所有采集器及隶属标签
  serviceAllCollectorTagsFun();
  async function serviceAllCollectorTagsFun() {
    const { data: res } = await serviceAllCollectorTags();
    if (res.length > 0) {
      allCollectorTags_treeData.value = res;
      allCollectorTags_treeData.value.forEach((item, index) => {
        item.tagName = item.name;
        item.tagId = item.id;
        item.tagInfos.forEach((itm, idx) => {
          itm.tagIndex = `tagIndex_${item.id}`;
        });
      });
      // 
    }
  }

  function allCollectorTags_onCheck(checked, e) {
    // 
    // 将数据进行整理成自己想要的
    e.checkedNodes.forEach((item, index) => {
      if (item.name) {
        delete e.checkedNodes[index];
      }
    });
    // 
    allCollectorTags_checkedNodes.value = e.checkedNodes;
  }

  //新增
  function addTags() {
    if (allCollectorTags_checkedNodes.value && allCollectorTags_checkedNodes.value.length > 0) {
      // 将数据拆解成接口想要的数据
      const groupedData = groupByTagIndex(allCollectorTags_checkedNodes.value);
      const watchListArr = extractTagIndexes(groupedData);
      // 
      let data = {
        id: state.parentId,
        watchList: watchListArr,
      };
      serviceWatchFun();
      async function serviceWatchFun() {
        const res = await serviceWatch(data);
        if (res.code === 200) {
          serviceWatchlistFun();
          allCollectorTags_checkedKeys.value = []; //去掉选中复选框的状态
          allCollectorTags_checkedNodes.value = []; //清空数据
          createMessage.success('添加成功');
        }
      }
    } else {
      createMessage.warning('请选择采集器及隶属标签');
    }
  }

  // -------------服务观察列表---------------------
  const watchlist_checkedKeys = ref([]); //选中
  const watchlist_treeData: TreeProps['treeData'] = ref([]);
  const watchlist_checkedNodes = ref([]); //所有选中的数据

  // 获取服务观察列表
  serviceWatchlistFun();
  async function serviceWatchlistFun() {
    const { data: res } = await serviceWatchlist({ id: state.parentId });
    if (res.length > 0) {
      watchlist_treeData.value = res;
      watchlist_treeData.value.forEach((item, index) => {
        item.tagName = item.name;
        item.tagId = item.id;
        item.tagInfos.forEach((itm, idx) => {
          itm.tagIndex = `tagIndex_${item.id}`;
        });
      });
      // 
    }
  }

  function watchlist_onCheck(checked, e) {
    e.checkedNodes.forEach((item, index) => {
      if (item.name) {
        delete e.checkedNodes[index];
      }
    });
    watchlist_checkedNodes.value = e.checkedNodes;
  }
  //移除
  function removeTags() {
    if (watchlist_checkedNodes.value && watchlist_checkedNodes.value.length > 0) {
      const groupedData = groupByTagIndex(watchlist_checkedNodes.value);
      const watchListArr = extractTagIndexes(groupedData);
      // 

      let data = {
        id: state.parentId,
        watchList: watchListArr,
      };

      serviceUnwatchFun();
      async function serviceUnwatchFun() {
        const res = await serviceUnwatch(data);
        if (res.code === 200) {
          serviceWatchlistFun();
          watchlist_checkedKeys.value = []; //去掉选中复选框的状态
          watchlist_checkedNodes.value = []; //清空数据
          createMessage.success('移除成功');
        }
      }
    } else {
      createMessage.warning('请选择服务观察列表');
    }
  }
</script>
<style lang="less" scoped>
  .gutter-div {
    padding: 20px;
    display: flex;
    justify-content: space-around;
    .gutter-box {
      height: 450px;
      width: 49%;
      position: relative;
      .gutter-tree {
        height: 450px;
        padding: 10px;
        border: 1px solid #c9d1d9;
        border-radius: 6px;
        overflow: auto;
      }
      .gutter-btn {
        position: absolute;
        bottom: -70px;
        right: 0px;
      }
    }
  }
</style>
