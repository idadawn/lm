<template>
  <div class="page-content-wrapper">
    <div class="page-content-wrapper-center">
      <div class="page-content-wrapper-search-box">
        <a-row :gutter="16">
          <a-col :span="8">
            <a-input-search
              v-model:value="state.queryInfo.keywords"
              placeholder="请输入关键字进行查询"
              enter-button
              @search="onSearch" />
          </a-col>
          <a-col :span="8">
            <a-button type="primary" @click="addDash">新建</a-button>
          </a-col>
        </a-row>
      </div>
      <div class="page-content-wrapper-content" ref="contentRef">
        <a-card class="card-item" hoverable v-for="item in state.items" @click="goChartEdtor('edit', item)">
          <template #cover>
            <img alt="example" src="/img/card.png" />
          </template>
          <template #actions @click.stop>
            <PlayCircleOutlined key="setting" @click.stop="goChartEdtor('preview', item)" />
            <edit-outlined key="edit" @click.stop="editDash(item)" />
            <DeleteOutlined key="delete" @click.stop="delDash(item)" />
          </template>
          <a-card-meta class="w-full card-meta">
            <template #avatar>
              <div class="h-[70px] w-[200px]">
                <div class="font-bold h-[24px] overflow-ellipsis overflow-hidden whitespace-nowrap">{{
                  item.name
                }}</div>
                <div class="h-[40px] card-des">{{ item.description }}</div>
              </div>
              <div class="h-[23px] overflow-hidden">
                <a-tag v-for="(tag, index) in item.metricTag" :key="index" color="blue" :bordered="false">{{
                  codeToName(tag)
                }}</a-tag>
              </div>
            </template>
          </a-card-meta>
        </a-card>
      </div>
    </div>
    <Form @register="registerForm" />
    <DepForm @register="registerDepForm" @reload="onSearch" />
  </div>
</template>
<script lang="ts" setup>
  import { usePopup } from '/@/components/Popup';
  import { useModal } from '/@/components/Modal';
  import { useMessage } from '/@/hooks/web/useMessage';
  import Form from './Form.vue';
  import DepForm from './DepForm.vue';
  import { getDashTreeList, deleteDash } from '/@/api/chart';
  import { reactive } from 'vue';
  import { PlayCircleOutlined, EditOutlined, DeleteOutlined } from '@ant-design/icons-vue';
  import { postMetrictagList } from '/@/api/labelManagement';
  import { onMounted, ref } from 'vue';

  defineOptions({ name: 'permission-organize' });

  const [registerForm, { openPopup: openFormPopup }] = usePopup();
  const [registerDepForm, { openModal: openDepFormModal }] = useModal();
  const { createMessage, createConfirm } = useMessage();

  const state: any = reactive({
    items: [],
    queryInfo: {
      keywords: '',
      currentPage: 1,
      pageSize: 999,
    },
    tags: [],
  });
  const goChartEdtor = (type, item) => {
    openFormPopup(true, { type, id: item.id });
  };

  // 获取列表
  const onSearch = async () => {
    //标签
    const res = await getDashTreeList(state.queryInfo);
    state.items = res.data?.list.map(item => {
      return {
        ...item,
        metricTag: item.metricTag ? item.metricTag?.split(',') : [],
      };
    });
  };
  // 新建仪表盘
  const addDash = () => {
    openDepFormModal(true, { type: 'add' });
  };
  // 编辑仪表盘
  const editDash = item => {
    openDepFormModal(true, { type: 'edit', item });
  };
  // 删除仪表盘
  const delDash = item => {
    // 二次确认是否需要删除
    createConfirm({
      iconType: 'warning',
      title: '确定要删除此数据吗？',
      content: '删除后无法恢复，请谨慎操作。',
      onOk: async () => {
        try {
          await deleteDash(item.id);
          createMessage.success('删除成功');
          onSearch();
        } catch (error: any) {
          const errorMsg = error?.response?.data?.msg || error?.message || '删除失败，请稍后重试';
          createMessage.error(errorMsg);
          throw error; // 重新抛出错误，阻止 Modal 自动关闭
        }
      },
    });
  };

  // 将code展示成name,入参为code字符串，和对象数组，返回name字符串
  const codeToName = code => {
    const arr = state.tags;
    let name = '';
    arr.forEach(item => {
      if (item.id === code) {
        name = item.name;
      }
    });
    return name;
  };

  const init = async () => {
    const res = await postMetrictagList({});
    state.tags = res.data.list;
    onSearch();
  };
  init();

  const contentRef = ref<HTMLDivElement | null>(null);
  const limit = ref(20);
  const onResizeChange = () => {
    const rect: DOMRect = contentRef.value?.getBoundingClientRect()!;
    const width = Math.floor(rect.width - 10);
    let col = Math.floor(width / 250);
    let comWidth = Math.floor(col * 250 + (col + 1) * limit.value);
    if (width < comWidth) {
      col = col - 1;
    }
    const remaining = Math.floor(width - col * 250);
    const gap = Math.floor(remaining / (col + 1));

    if (gap > limit.value) {
      contentRef.value!.style.padding = `${gap}px`;
      contentRef.value!.style.gap = `${gap}px`;
    } else {
      contentRef.value!.style.padding = `${limit.value}px`;
      contentRef.value!.style.gap = `${limit.value}px`;
    }
  };

  onMounted(() => {
    onResizeChange();
    window.onresize = () => {
      onResizeChange();
    };
  });
</script>
<style lang="less" scoped>
  .page-content-wrapper .page-content-wrapper-center .page-content-wrapper-content {
    /* flex: 1;
    overflow: hidden; */
    background: #fff;
    display: flex;
    flex-flow: row wrap;
    overflow: auto;

    ::v-global(.ant-card-body) {
      padding: 12px;
    }

    .card-item {
      width: 250px;
      height: 310px;
      // margin: 10px;
    }

    .card-meta {
      .card-des {
        display: -webkit-box;
        -webkit-line-clamp: 2;
        -webkit-box-orient: vertical;
        overflow: hidden;
        color: #888;
      }
    }
  }

  .page-content-wrapper-search-box {
    padding: 10px;
  }

  .itemBox {
    width: 100%;
    background: #fff;
    border-radius: 4px;
    box-shadow: inset 0 -2px 0 0 #cdcde6, inset 0 0 1px 1px #fff, 0 1px 2px 1px rgb(30 35 90 / 40%);
    margin-top: 26px;
  }

  .contentBox,
  .buttonBox {
    display: flex;
    flex-direction: row;
  }

  .contentBox {
    padding: 10px 15px;
    box-sizing: border-box;
    color: #888;
  }

  .buttonBox {
    padding: 10px 15px;
    box-sizing: border-box;
    background: #ccc;
    color: #888;

    /* border: 1px solid red; */
  }

  .logoBox {
    width: 80px;
    height: 80px;
    border-radius: 40px;
    flex-shrink: 0;
    margin-right: 10px;
  }

  .Title {
    color: #000;
    font-size: 16px;
  }

  .ContentMsg {
    width: 100%;
    height: auto;
    line-height: 16px;
    overflow: hidden;
    text-overflow: ellipsis;
    display: -webkit-box;
    -webkit-line-clamp: 3 !important;
    line-clamp: 3 !important;
    -webkit-box-orient: vertical;
    word-wrap: break-word;
  }

  .buttonLeft,
  .buttonRight {
    width: 50%;
    text-align: center;
    cursor: pointer;
  }

  .buttonLeft {
    border-right: 1px solid #fff;
  }

  .buttonLeft:hover {
    color: #fff;
  }

  .buttonRight:hover {
    color: #fff;
  }
</style>
