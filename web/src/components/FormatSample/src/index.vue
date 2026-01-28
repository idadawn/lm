<template>
  <div>
    <!-- 编辑指标格式 -->
    <a-modal
      width="400px"
      v-model:visible="state.visible_format"
      title="编辑指标格式"
      @ok="handleFormat"
      @cancel="cancelFormat">
      <div style="padding: 20px">
        <a-radio-group v-model:value="radio_group" @change="handleRadioGroup" style="margin-bottom: 30px">
          <a-radio-button value="None">无格式</a-radio-button>
          <a-radio-button value="Number">数值</a-radio-button>
          <a-radio-button value="Currency">货币</a-radio-button>
          <a-radio-button value="Percentage">百分比</a-radio-button>
        </a-radio-group>
        <a-form :model="format" :label-col="{ span: 5 }" :wrapper-col="{ span: 16 }">
          <a-form-item label="样例">
            <a-input v-model:value="formatValue" :defaultValue="formatDefaultValue" disabled></a-input>
          </a-form-item>
          <a-form-item label="小数位数" v-if="radio_group != 'None'">
            <a-input-number
              v-model:value="format.decimal_place"
              :min="0"
              :max="10"
              @change="handleChange_decimal"></a-input-number>
          </a-form-item>
          <a-form-item label="单位" v-if="radio_group == 'Number' || radio_group == 'Currency'">
            <a-select ref="select" v-model:value="format.unit" @change="handleChange_unit">
              <a-select-option value="None">无</a-select-option>
              <a-select-option value="Default">自动</a-select-option>
              <a-select-option value="Wan">万(10,000)</a-select-option>
              <a-select-option value="Yi">亿(100,000,000)</a-select-option>
            </a-select>
          </a-form-item>
          <a-form-item label="货币标志" v-if="radio_group == 'Currency'">
            <a-select ref="select" v-model:value="format.currency_symbol" @change="handleChange_symbol">
              <a-select-option value="None">无</a-select-option>
              <a-select-option value="CNY1">¥</a-select-option>
              <a-select-option value="USD1">$</a-select-option>
            </a-select>
          </a-form-item>
          <a-form-item label="分隔符" v-if="radio_group != 'None'">
            <a-checkbox v-model:checked="format.use_thousand_separator" @change="handleChange_separator"
              >使用千分位分隔符</a-checkbox
            >
          </a-form-item>
        </a-form>
      </div>
    </a-modal>
  </div>
</template>
<script lang="ts" setup>
  import { reactive, watch, ref } from 'vue';
  import { Modal as AModal } from 'ant-design-vue';
  import { props as _props } from './props';

  defineOptions({
    name: 'FormatSample',
  });

  const emit = defineEmits(['visible_format', 'format_obj', 'format_value_format']);
  const props = defineProps(_props);

  // 
  const state = reactive({
    visible_format: false,
  });
  const radio_group = ref('Number'); //指标格式默认值
  const formatValue = ref<any>('1234');
  const formatDefaultValue = ref<any>(1234);
  const formatDecimal = ref(''); //小数位.000
  const formatUnit = ref(''); //单位K,M,B
  const format = reactive({
    type: '',
    decimal_place: 0,
    unit: 'None',
    currency_symbol: 'None',
    use_thousand_separator: true,
  });

  // 小数位数
  function handleChange_decimal() {
    formatValue.value = formatDefaultValue.value;
    formatDecimal.value = '';

    //判断单位
    if (format.decimal_place > 0) {
      formatDecimal.value = '.' + '0'.repeat(format.decimal_place);
    }

    ifComma();
    let currency_symbol_val = '';
    if (format.currency_symbol === 'CNY1') {
      currency_symbol_val = '¥';
    } else if (format.currency_symbol === 'USD1') {
      currency_symbol_val = '$';
    } else {
      currency_symbol_val = '';
    }
    formatValue.value = currency_symbol_val + formatValue.value + formatDecimal.value + formatUnit.value;
  }

  // 单位
  function handleChange_unit() {
    formatValue.value = formatDefaultValue.value;
    let currency_symbol_val = '';
    if (format.currency_symbol === 'CNY1') {
      currency_symbol_val = '¥';
    } else if (format.currency_symbol === 'USD1') {
      currency_symbol_val = '$';
    } else {
      currency_symbol_val = '';
    }
    if (format.unit == 'Wan') {
      formatUnit.value = '万';
      ifComma();
      formatValue.value = currency_symbol_val + formatValue.value + formatDecimal.value + formatUnit.value;
    } else if (format.unit == 'Yi') {
      formatUnit.value = '亿';
      ifComma();
      formatValue.value = currency_symbol_val + formatValue.value + formatDecimal.value + formatUnit.value;
    } else if (format.unit == 'Default' || format.unit == 'None') {
      formatUnit.value = '';
      ifComma();
      formatValue.value = currency_symbol_val + formatValue.value + formatDecimal.value + formatUnit.value;
    }
  }
  // 货币标志
  function handleChange_symbol() {
    formatValue.value = formatDefaultValue.value;
    let currency_symbol_val = '';
    if (format.currency_symbol === 'CNY1') {
      currency_symbol_val = '¥';
    } else if (format.currency_symbol === 'USD1') {
      currency_symbol_val = '$';
    } else {
      currency_symbol_val = '';
    }

    ifComma();
    formatValue.value = currency_symbol_val + formatValue.value + formatDecimal.value + formatUnit.value;
  }
  // 分隔符
  function handleChange_separator() {
    formatValue.value = formatDefaultValue.value;
    let currency_symbol_val = '';
    if (format.currency_symbol === 'CNY1') {
      currency_symbol_val = '¥';
    } else if (format.currency_symbol === 'USD1') {
      currency_symbol_val = '$';
    } else {
      currency_symbol_val = '';
    }

    // 判断是否选中
    if (format.use_thousand_separator) {
      let formatValue2 = (formatValue.value || 0).toString().replace(/(\d)(?=(?:\d{3})+$)/g, '$1,');
      formatValue.value = currency_symbol_val + formatValue2 + formatDecimal.value + formatUnit.value;
    } else {
      let formatValue2 = formatValue.value.toString().replace(/,/gi, '');
      formatValue.value = currency_symbol_val + formatValue2 + formatDecimal.value + formatUnit.value;
    }
  }
  // 判断是否有逗号1,234,000
  function ifComma() {
    // 选中时，带有逗号时处理
    if (format.use_thousand_separator) {
      formatValue.value = (formatValue.value || 0).toString().replace(/(\d)(?=(?:\d{3})+$)/g, '$1,');
    }
  }
  // 切换指标格式
  handleRadioGroup();
  function handleRadioGroup() {
    if (radio_group.value == 'None') {
      // 无格式
      format.decimal_place = 0;
      format.unit = 'None';
      format.currency_symbol = 'None';
      format.use_thousand_separator = false;
      handleChange_decimal(); //小数位数
      handleChange_unit(); //单位
      handleChange_symbol(); //货币
      handleChange_separator(); //分隔符
    } else if (radio_group.value == 'Number') {
      // 数值
      format.decimal_place = 2;
      format.unit = 'Default';
      format.currency_symbol = 'None';
      format.use_thousand_separator = true;
      handleChange_decimal(); //小数位数
      handleChange_unit(); //单位
      handleChange_symbol(); //货币
      handleChange_separator(); //分隔符
    } else if (radio_group.value == 'Currency') {
      // 货币
      format.decimal_place = 2;
      format.unit = 'Default';
      format.currency_symbol = 'None';
      format.use_thousand_separator = true;
      handleChange_decimal(); //小数位数
      handleChange_unit(); //单位
      handleChange_symbol(); //货币
      handleChange_separator(); //分隔符
    } else if (radio_group.value == 'Percentage') {
      // 百分比
      format.decimal_place = 2;
      format.unit = 'None';
      format.currency_symbol = 'None';
      format.use_thousand_separator = true;
      handleChange_decimal(); //小数位数
      handleChange_unit(); //单位
      handleChange_symbol(); //货币
      handleChange_separator(); //分隔符
      radioWatch();
    }
  }
  // 小数位数监听
  function radioWatch() {
    if (
      formatValue.value.indexOf('万') != -1 ||
      formatValue.value.indexOf('亿') != -1 ||
      formatValue.value.indexOf('%') != -1
    ) {
      formatValue.value = formatValue.value.slice(0, formatValue.value.length - 1) + '%';
    } else {
      formatValue.value = formatValue.value + '%';
    }
  }
  // 监听百分比中的小数位数，把K，M，B改为%
  // 监听百分比中的分隔符，添加单位%
  watch([() => format.decimal_place, () => format.use_thousand_separator], () => {
    if (radio_group.value == 'Percentage') {
      radioWatch();
    }
  });

  // 监听弹层显示隐藏
  watch(
    () => props.visible,
    () => {
      if (props.visible) {
        state.visible_format = true;
      } else {
        state.visible_format = false;
      }
    },
  );

  // 当pradioType 为percentage百分比格式时，默认显示百分比样例
  // if (props.radioType === 'Percentage') {
  //   radio_group.value = 'Percentage';
  //   handleFormat();
  // } else if (props.radioType === 'Number') {
  //   radio_group.value = 'Number';
  //   handleFormat();
  // }

  // 格式提交
  function handleFormat() {
    format.type = radio_group.value;
    let format_value_format;
    if (format.type == 'None') {
      format_value_format = '无格式' + formatValue.value;
    } else if (format.type == 'Number') {
      format_value_format = '数值' + formatValue.value;
    } else if (format.type == 'Currency') {
      format_value_format = '货币' + formatValue.value;
    } else if (format.type == 'Percentage') {
      format_value_format = '百分比' + formatValue.value;
    }

    emit('format_obj', format);
    emit('format_value_format', format_value_format);
    emit('visible_format', false);
  }
  // 取消
  function cancelFormat() {
    // 
    emit('visible_format', false);
  }
</script>
<style lang="less" scoped></style>
