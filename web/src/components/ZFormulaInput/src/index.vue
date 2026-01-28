<template>
  <div class="formula-input-wrapper">
    <div
      ref="formulaRef"
      class="input"
      :class="{ error: state.errorMsg }"
      :contenteditable="disabled ? 'false' : 'plaintext-only'"
      :placeholder="placeholder"
      style="ime-mode: disabled"
      @keydown.stop="onKeydown"
      @keyup="onKeyup"
      @blur="setValue"></div>
    <div class="hint" v-if="state.errorMsg">{{ state.errorMsg }}</div>
    <div class="formula-input-selection" v-if="state.showSelection" ref="selectionRef" @click.stop>
      <a-input v-model:value="state.filter" ref="inputRef" placeholder="输入关键字筛选"></a-input>
      <div class="options" v-if="displayOptions.length">
        <span class="option" v-for="(item, i) in displayOptions" :key="i" @click="optionClick(item)">
          {{ item.name }}
        </span>
      </div>
      <div class="empty" v-else>暂无数据</div>
    </div>
  </div>
</template>
<script lang="ts" setup>
  import { reactive, onMounted, watch, ref, computed, nextTick } from 'vue';
  import { props as _props } from './props';
  import { getHTMLList, str2dom, dom2str, isHTML, validKeys, setFocus, getDiffIndex, getParentNode } from './utils';
  import { throttle } from 'lodash-es';
  defineOptions({
    name: 'ZFormulaInput',
  });
  const emit = defineEmits(['input', 'change', 'update:value']);

  const props = defineProps(_props);
  const formulaRef = ref<any>(null);
  const selectionRef = ref<any>(null);
  const inputRef = ref<any>(null);
  const initDefaultInnerModol = () => ({
    formula: '',
    vars: {},
  });
  const state = reactive<any>({
    filter: '',
    showSelection: false,
    errorMsg: '',
    throttleSetSelectionStyle: null,
    innerModel: props.initValue || initDefaultInnerModol(),
  });

  const displayOptions = computed(() => {
    const { filter } = state;
    const options = props.options || [];
    return options.filter(({ name }) => name.includes(filter));
  });

  const addEventListener = () => {
    // 添加全局click监听
    window.addEventListener('click', removeSelection);
    // 监听滚定事件 定位下拉框
    if (props.scrollWrapperClassName) {
      const el = getParentNode(selectionRef.value, props.scrollWrapperClassName);
      el && el.addEventListener('scroll', state.throttleSetSelectionStyle);
    }
  };
  const removeEventListener = () => {
    // 解绑全局click监听关闭选项弹框
    window.removeEventListener('click', removeSelection);
    // 解绑全局滚动监听
    if (props.scrollWrapperClassName) {
      const el = getParentNode(formulaRef.value, props.scrollWrapperClassName);
      el && el.addEventListener('scroll', state.throttleSetSelectionStyle);
    }
  };
  const onKeyup = () => {
    const target = formulaRef.value;
    const originStr = target.innerHTML;
    let list = str2dom(originStr);
    list = list.map(v =>
      isHTML(v)
        ? dom2str(v)
        : v.data
            .split('')
            .filter(v => validKeys.includes(v))
            .join(''),
    );
    const filteredStr = list.join('');
    if (originStr !== filteredStr) {
      const index = getDiffIndex(originStr, filteredStr);
      formulaRef.value.innerHTML = filteredStr;
      setFocus(formulaRef.value, index);
    }
  };
  const initDisplay = () => {

    const { vars, formula } = state.innerModel;
    let result = formula;
    for (let key in vars) {
      const rule = new RegExp(key, 'g');
      const name = `<div contenteditable="false">${vars[key]}<span>${key}</span></div>`;
      result = result.replace(rule, (v, index, string) => {
        const length = v.length;
        const str = string.slice(index - 1, length + index + 1);
        if (str.startsWith('_') || str.endsWith('_')) {
          return key;
        } else {
          return name;
        }
      });
    }
    formulaRef.value.innerHTML = result;
    setValue();
  };
  const removeSelection = e => {
    state.showSelection = false;
    e && resetDisplay('@');
  };
  const onKeydown = e => {
    const { key } = e;
    switch (key) {
      // 禁止回车
      case 'Enter':
        e.preventDefault();
        break;
      case '@':
      case 'Process': // 中文输入法的 @
        openSelection();
        break;
      default:
    }
  };
  const isValid = v => {
    return validKeys.indexOf(v) >= 0;
  };
  const openSelection = () => {
    state.filter = '';
    state.showSelection = true;
    setTimeout(() => {
      // append to body
      document.body.appendChild(selectionRef.value);
      setSelectionStyle();
      // 绑定监听
      addEventListener();
      // 焦点到下拉框filter input中
      inputRef.value.focus();
    }, 0);
  };
  const setSelectionStyle = () => {
    const formula = formulaRef.value;
    const selection = selectionRef.value;
    if (!formulaRef.value) return;
    const { top, left, height, width } = formula.getBoundingClientRect();

    selection &&
      selection.setAttribute('style', `left: ${left}px; top: ${top + height}px; width: ${width > 300 ? width : 300}px`);
  };
  const optionClick = item => {
    const { name, field } = item;
    state.showSelection = false;
    const res = `<div contenteditable="false">${name}<span>${field}</span></div>`;
    resetDisplay('@', res);
    nextTick(() => {
      setFocus(formulaRef.value, formulaRef.value.innerHTML.length);
    });
  };
  const resetDisplay = (from, to = '') => {
    let text = formulaRef.value.innerHTML;
    if (text.includes(from)) {
      text = text.replace(new RegExp(from, 'g'), to);
      formulaRef.value.innerHTML = text;
    }
    setValue();
  };
  // 更新 v-model
  const setValue = () => {
    state.errorMsg = '';
    let formula = '';
    const vars = {};
    const text = formulaRef.value.innerHTML.replace(/\sdata-spm-anchor-id=".*?"/g, '');
    const list = getHTMLList({
      text,
      prefix: '<div contenteditable="false">',
      suffix: '</div>',
    });
    list.forEach(item => {
      const [v1, v2] = getHTMLList({
        text: item,
        prefix: '<span>',
        suffix: '</span>',
      });
      if (v2) {
        formula += v2;
        vars[v2] = v1;
      } else {
        formula += v1;
      }
    });
    const res = {
      formula,
      vars,
    };
    emit('input', res);
    emit('change', res);
    emit('update:value', res);
  };
  const validate = () => {
    state.errorMsg = '';
    const { formula } = props.value || {};
    if (!formula) {
      state.errorMsg = '公式不能为空';
      return false;
    }
    return true;
  };

  watch(
    () => props.initValue,
    v => {
      state.innerModel = v || initDefaultInnerModol();
      initDisplay();
    },
    () => state.showSelection,
    v => {
      if (!v) {
        removeEventListener();
      }
    },
    { immediate: true, deep: true },
  );

  onMounted(() => {
    state.throttleSetSelectionStyle = throttle(setSelectionStyle, 100);
    initDisplay();
  });
</script>
<style lang="less">
  @import url('./index.less');
</style>
