<template>
  <div class="z-temp-feild-wrapper">
    <a-form-item :label="item[labelProps]" :name="formItemName" :rules="state.rules">
      <jnpf-input
        v-if="state.inputTypes.includes(item[typeProps]) && item[displayProps] != 'N'"
        v-model:value="state.result"
        :placeholder="state.placeholder"
        @change="change(item)" />

      <jnpf-input-number
        v-if="state.numberTypes.includes(item[typeProps]) && item[displayProps] != 'N'"
        v-model:value="state.result"
        :placeholder="state.placeholder"
        @change="change(item)" />

      <jnpf-switch
        v-if="state.switchTypes.includes(item[typeProps]) && item[displayProps] != 'N'"
        v-model:value="state.result"
        :placeholder="state.placeholder"
        @change="change(item)" />

      <jnpf-select
        v-if="state.selectTypes.includes(item[typeProps]) && item[displayProps] != 'N'"
        v-model:value="state.result"
        :placeholder="state.placeholder"
        :options="state.options"
        showSearch
        :fieldNames="{ value: 'v' }"
        @change="change(item)" />

      <a-radio-group
        v-model:value="state.result"
        v-if="state.radioTypes.includes(item[typeProps]) && item[displayProps] != 'N'"
        @change="change(item)">
        <a-radio v-for="item in state.options" :key="item.v" :value="item.v">{{ item.fullName }} </a-radio>
      </a-radio-group>
      <a-tooltip>
        <ExclamationCircleOutlined v-if="item.des" class="icon_class" />
        <template #title>{{ item.des }}</template>
      </a-tooltip>
    </a-form-item>
    <template v-for="(element, index) in state.childItems" :key="index">
      <z-temp-field
        :items="state.childItems"
        :item="element"
        :typeProps="typeProps"
        :labelProps="labelProps"
        :keyProps="keyProps"
        :listProps="listProps"
        :listLabelProps="listLabelProps"
        :listValueProps="listValueProps"
        :requiredProps="requiredProps"
        :formItemName="state.childFormItemName.concat([index, keyProps])"
        :params="index" />
    </template>
  </div>
</template>
<script lang="ts" setup>
  import { reactive, watch, computed, toRaw } from 'vue';
  import { props as _props } from './props';
  import { ExclamationCircleOutlined } from '@ant-design/icons-vue';

  defineOptions({
    name: 'ZTempField',
  });
  const emit = defineEmits(['update:value', 'change', 'blur']);
  const props = defineProps(_props);

  const state = reactive<any>({
    selectTypes: ['select', 'radio', 'Option', 'Enum'],
    inputTypes: ['input', 'String'],
    numberTypes: ['number', 'Int32', 'UInt64'],
    passwordTypes: ['password'],
    radioTypes: ['radio', 'Boolean'],
    switchTypes: ['switch'],

    result: props.item[props.keyProps] ? props.item[props.keyProps] : props.item[props.defaultValue],
    ruels: [],
    options:
      props.item[props.listProps]?.map((target: any, index: number) => {
        const v = props.listValueProps == 'index' ? index : target[props.listValueProps];
        return {
          ...target,
          v,
          fullName: target[props.listLabelProps],
        };
      }) || [],
    address: {
      province: '',
      city: '',
      area: '',
    },
    codeType: computed(() => {
      return ['select', 'radio'].indexOf(props.item.dataType) > -1 ? props.item.itemID : '';
    }),
    fieldType: computed(() => {
      return props.fieldType || props.item.fieldType;
    }),
    placeholder: computed(() => {
      return props.item[props.placeholderProps] || `请输入${props.item[props.labelProps]}`;
    }),
    childItems: computed(() => {
      let option: any = {};
      if (state.selectTypes.includes(props.item[props.typeProps])) {
        option = state.options.find((target: any) => {
          return target[props.listValueProps] == state.result && target?.items?.length > 0;
        });
      }
      return option?.items || [];
    }),
    childFormItemName: computed(() => {
      const names = [];
      if (props.formItemName) {
        names.push(...props.formItemName);
        names.splice(props.formItemName.length - 1, 1);
      }
      props.childFormItemName[1] = state.result;
      return names.concat(props.childFormItemName || []);
    }),
  });
  watch(
    () => props.item[props.keyProps],
    // [toRef(props, "item", props.keyProps)],
    () => {
      if (props.item[props.keyProps]) {
        state.result = props.item[props.keyProps];
      }
    },
  );
  watch(
    () => props.item[props.listProps],
    // [toRef(props, "item", props.keyProps)],
    () => {
      state.options =
        props.item[props.listProps]?.map((target: any, index: number) => {
          const v = props.listValueType == 'index' ? index : target[props.listValueProps];
          return {
            ...target,
            v,
            fullName: target[props.listLabelProps],
          };
        }) || [];
      // 如果是boolean类型的radio,则需要特殊处理
      if (props.item[props.typeProps] == 'Boolean') {
        state.options = [
          {
            v: true,
            fullName: '是',
          },
          {
            v: false,
            fullName: '否',
          },
        ];
      } else if (props.item[props.typeProps] == 'Enum' && props.item.enumName == 'BlueBird.Core.Enums.TagValueType') {
        state.options = [
          {
            item: 'Unkown',
            fullName: 'Unkown',
            v: -1,
          },
          {
            item: 'Bit',
            fullName: 'Boolean(Bit)',
            v: 0,
          },
          {
            item: 'Byte',
            fullName: 'Byte',
            v: 1,
          },
          {
            item: 'Short',
            fullName: 'Int16(Short)',
            v: 2,
          },
          {
            item: 'UShort',
            fullName: 'UInt16(UShort)',
            v: 3,
          },
          {
            item: 'Int32',
            fullName: 'Int32',
            v: 4,
          },
          {
            item: 'UInt32',
            fullName: 'UInt32',
            v: 5,
          },
          {
            item: 'Float',
            fullName: 'Single(Float)',
            v: 6,
          },
          {
            item: 'Long',
            fullName: 'Int64(Long)',
            v: 7,
          },
          {
            item: 'ULong',
            fullName: 'UInt64(ULong)',
            v: 8,
          },
          {
            item: 'Double',
            fullName: 'Double',
            v: 9,
          },
          {
            item: 'String',
            fullName: 'String',
            v: 10,
          },
        ];
      }
      // 排序
      if (props.listSortProps) {
        state.options.sort((a: any, b: any) => {
          return a[props.listSortProps] - b[props.listSortProps];
        });
      }
      // 设置默认值
      if (props.item[props.keyProps] == '' && props.listSelectProps) {
        state.options.forEach((item: any) => {
          if (item[props.listSelectProps] == 'Y') {
            state.result = item[props.listValueProps];
            // eslint-disable-next-line vue/no-mutating-props
            props.item[props.keyProps] = state.result;
            // eslint-disable-next-line vue/no-mutating-props
            props.item[props.nameProps] = state.name;
          }
        });
      } else {
        state.result = props.item[props.keyProps];
      }
    },
    { immediate: true },
  );

  const change = (item: any) => {
    // 数字矫正
    if (state.fieldType == 'number') {
      state.result = parseFloat(state.result);
    }
    item[props.keyProps] = state.result;
    item[props.nameProps] = state.name;
    // 地址赋值
    if (item.dataType == 'address') {
      item.area = state.address.area;
      item.city = state.address.city;
      item.province = state.address.province;
    }
    // 其他参数回传
    if (props.params != null) {
      item.params = props.params;
    }
    emit('update:value', state.result);
    emit('change', item);
    console.log(state.result, 'state.result');
  };

  const setRules = () => {
    const res: any = [];
    const item = props.item;
    const labelProps = props.labelProps;
    if (item[props.requiredProps]) {
      res.push({
        required: true,
        message: `请输入${item[labelProps]}`,
      });
    }
    if (item.validType) {
      res.push({
        pattern: new RegExp(item.validType),
        message: `${item[props.labelProps]}格式不正确`,
      });
    }
    state.rules = res;
  };
  setRules();
</script>
<style lang="less" scoped>
  .z-temp-feild-wrapper {
    // height: 80px;
    .icon_class {
      position: absolute;
      right: -20px;
      top: 10px;
      color: #3d7da9;
    }
  }
</style>
