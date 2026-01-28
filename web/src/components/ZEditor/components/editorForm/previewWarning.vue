<template>
  <div class="page-content-wrapper">
    <div class="page-content-wrapper-center bg-white">
      <a-form
        layout="vertical"
        ref="formRef"
        :model="state.elements"
        :colon="false"
        v-bind="formTailLayout"
        @finish="onFinish">
        <template v-for="(element, index) in state.elements">
          <z-temp-field
            :items="state.elements"
            :item="element"
            typeProps="dataType"
            labelProps="factorTypeName"
            keyProps="factorValue"
            listProps="elements"
            listLabelProps="factorDisplayValue"
            listValueProps="factorValue"
            :params="index" />
        </template>
        <a-form-item>
          <a-button type="primary" html-type="submit">Submit</a-button>
        </a-form-item>
      </a-form>
      <a-form layout="vertical" :model="state.form">
        <a-form-item label="名称" name="label" @change="updateItem">
          <a-input v-model:value="state.form.label" placeholder="请输入" />
        </a-form-item>
        <a-form-item label="计算公式" name="formula">
          <a-input v-model:value="state.form.formula" placeholder="请输入" />
        </a-form-item>
        <a-form-item>
          <a-row>
            <a-col span="12">
              <a-button type="primary" @click="updateItem">保存</a-button>
            </a-col>
            <a-col span="12">
              <a-button type="danger" @click="delItem">删除</a-button>
            </a-col>
          </a-row>
        </a-form-item>
      </a-form>
    </div>
  </div>
</template>
<script lang="ts" setup>
  import { reactive, watch } from 'vue';
  import { props as _props } from './props';
  import { ZTempField } from '/@/components/ZTempField';
  import { getOptimalNodeElements } from '/@/api/createModel/model';

  defineOptions({
    name: 'ZEditorForm',
  });

  const emit = defineEmits(['delete', 'update']);
  const props = defineProps(_props);

  const formTailLayout = {
    labelCol: { style: { width: '110px' } },
    // wrapperCol: { offset: 4 },
  };

  const formData = reactive({
    elements: [
      {
        id: 'B0000000000000002350',
        factorTypeName: '保额',
        factorType: 'name',
        factorValue: '',
        isDisplay: 'Y',
        isMustInput: 'Y',
        isPremCalFacotor: 'N',
        showOrder: 8,
        dataType: 'input',
        riskFactorList: [],
        isValueCalFacotor: 'N',
        isValueComparison: 'N',
      },
      {
        id: 'B0000000000000002351',
        factorTypeName: '保额1',
        factorType: 'age',
        factorValue: '222',
        isDisplay: 'Y',
        isMustInput: 'N',
        isPremCalFacotor: 'N',
        showOrder: 8,
        dataType: 'input',
        riskFactorList: [],
        isValueCalFacotor: 'N',
        isValueComparison: 'N',
      },
    ],
  });
  const state = reactive<any>({
    form: {},
    elements: [],
  });

  const onFinish = values => {
    // emit('update', values);
  };

  const delItem = () => {
    emit('delete');
  };
  const updateItem = () => {
    emit('update', state.form);
  };

  watch(
    () => props.form,
    val => {
      state.form = val;
      state.form.type = val.type;
    },
  );

  watch(
    () => props.form.id,
    async val => {
      try {
        const res = await getOptimalNodeElements({ nodeId: val, userId: '1' });
        state.elements = res.data.elements;
      } catch (e) {
      }
    },
  );
</script>
<style lang="less" scoped>
  .page-content-wrapper-center {
    overflow: auto;
  }
</style>
