<template>
  <BasicModal v-bind="$attrs" @register="registerModal" :title="getTitle" @ok="handleSubmit">
    <a-form
      layout="vertical"
      ref="formRef"
      :model="state.elements"
      :colon="false"
      v-bind="formTailLayout"
      @finish="onFinish">
      <a-form-item label="状态名称">
        <jnpf-input v-model:value="state.form.name" allowClear />
        <!-- <jnpf-select
          v-model:value="state.form.name"
          :options="state.statusOptions"
          showSearch
          allowClear
          :fieldNames="{ label: 'name', value: 'id' }"
          @change="getNode" /> -->
      </a-form-item>
      <a-form-item label="颜色选择">
        <input type="color" :value="state.form.color" id="myColor" @change="myFunction" />
        {{ state.form.color }}
      </a-form-item>
    </a-form>
  </BasicModal>
</template>
<script lang="ts" setup>
  import { addStatus, updateStatus, getStatusOptionsList, getStatusDetail } from '/@/api/status/model';
  import { reactive, ref, unref, computed } from 'vue';
  import { BasicModal, useModalInner } from '/@/components/Modal';
  import { useMessage } from '/@/hooks/web/useMessage';

  const emit = defineEmits(['register', 'reload']);
  const [registerModal, { closeModal, changeLoading, changeOkLoading }] = useModalInner(init);
  const id = ref('');
  const statusColor = ref('');
  const treeData = ref([{}]);
  const state = reactive<any>({
    form: {},
    statusOptions: [],
    metrics: [
      {
        id: '正常',
        name: '正常',
      },
      {
        id: '有风险',
        name: '有风险',
      },
      {
        id: '无风险',
        name: '无风险',
      },
      {
        id: '完成',
        name: '完成',
      },
    ],
  });

  const { createMessage } = useMessage();

  const getTitle = computed(() => (!unref(id) ? '新建状态' : '编辑状态'));
  function init(data) {
    // resetFields();
    // getStatusOptionsList().then(res => {
    //   console.log('00000', res);
    //   state.statusOptions.value = res.data.map(({ id, name }) => ({ id, fullName: name }));
    // });
    id.value = data.id;

    if (id.value) {
      changeLoading(true);
      //回显数据
      getStatusDetail(data.id).then(res => {
        state.form = {
          ...res.data,
          name: res.data.name,
        };
        // setFieldsValue(values);
        changeLoading(false);
      });
    } else {
      state.form = {
        color: '#000000',
      };
    }
  }
  async function handleSubmit() {
    // const values = ; //获取详情当前所有的字段
    console.log('values---', state.form.id);
    if (!state.form) return;
    changeOkLoading(true);
    const query = {
      ...state.form,
    };

    const formMethod = state.form.id ? updateStatus : addStatus;
    console.log('values---', formMethod);
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
  }
  function myFunction() {
    var x = document.getElementById('myColor').value;
    state.form.color = x;
  }
</script>
<style scoped></style>
