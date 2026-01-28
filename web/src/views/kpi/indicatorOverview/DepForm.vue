<template>
  <BasicModal v-bind="$attrs" @register="registerModal" :title="getTitle" @ok="handleSubmit">
    <a-form
      class="modelFormClass"
      layout="vertical"
      :model="state.form"
      ref="formRefDescribe"
      :label-col="{ span: 8 }"
      :wrapper-col="{ span: 24 }">
      <a-form-item label="价值链名称" name="name" required>
        <a-input v-model:value="state.form.name" placeholder="请输入" allow-clear></a-input>
      </a-form-item>
      <a-form-item label="描述" name="description">
        <a-textarea
          v-model:value="state.form.description"
          placeholder="请输入"
          allow-clear
          :auto-size="{ minRows: 3, maxRows: 5 }"></a-textarea>
      </a-form-item>
      <a-form-item label="标签" name="metricTag">
        <depSelectTag :dataArr="metricTagArr" v-model:checkedArr="state.form.metricTag"></depSelectTag>
      </a-form-item>
    </a-form>
  </BasicModal>
</template>
<script lang="ts" setup>
  import { ref, computed, reactive } from 'vue';
  import { BasicModal, useModalInner } from '/@/components/Modal';
  import { useMessage } from '/@/hooks/web/useMessage';
  import { depSelectTag } from '/@/components/DepSelectTag';
  import { postMetrictagList } from '/@/api/labelManagement';
  import { createDash, editDash } from '/@/api/chart';

  const emit = defineEmits(['register', 'reload']);

  const [registerModal, { closeModal, changeLoading, changeOkLoading }] = useModalInner(init);
  const { createMessage } = useMessage();

  const getTitle = computed(() => (state.form.id ? '编辑仪表盘' : '新建仪表盘'));
  const formRefDescribe = ref(null);
  const metricTagArr = ref([]); //标签的数组
  const state = reactive({
    form: {
      id: '',
      type: 'Dash',
      name: '', //名称
      description: '', //描述
      metricTag: [], //标签
      imgName: '',
    },
  });

  function init(data) {
    formRefDescribe.value.resetFields();
    const { item } = data;
    state.form.id = item?.id;
    // 编辑
    if (state.form.id) {
      state.form = {
        id: item.id,
        type: 'Dash',
        name: item.name, //名称
        description: item.description, //描述
        metricTag: item.metricTag, //标签
        imgName: '',
      };
    }

    //标签
    postMetrictagList({}).then(res => {
      metricTagArr.value = res.data.list;
    });
  }

  async function handleSubmit() {
    formRefDescribe.value
      .validate()
      .then(() => {
        changeOkLoading(true);
        const query = state.form;
        const formMethod = state.form.id ? editDash : createDash;
        formMethod(query)
          .then(res => {
            createMessage.success(res.msg);
            changeOkLoading(false);
            closeModal();
            setTimeout(() => {
              emit('reload');
            }, 300);
          })
          .catch(() => {
            changeOkLoading(false);
          });
      })
      .catch(error => {
      });
  }
</script>
