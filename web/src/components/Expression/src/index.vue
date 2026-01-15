<template>
  <div>
    <!-- 编辑表达式 -->
    <a-modal
      width="800px"
      v-model:visible="state.visible_expression"
      title="编辑表达式"
      @ok="handleExpression"
      @cancel="cancelExpression">
      <div style="padding: 10px 20px">
        <p style="color: #8e99ac"><info-circle-filled style="margin-right: 5px" />表达式仅能使用数值类型的指标。</p>

        <a-row :gutter="16" style="min-height: 200px">
          <a-col :span="8" class="expressionCol">
            <a-input
              v-model:value="state.expressionSearch"
              placeholder="搜索展示名称"
              @change="inputChange"
              style="margin-top: 10px">
              <template #prefix>
                <search-outlined style="color: #8e99ac" />
              </template>
            </a-input>
            <div class="expressionList">
              <p v-for="item in state.formularOptions">
                <span @click="expressionListClick(item.name)">{{ item.name }}</span>
              </p>
            </div>
          </a-col>
          <a-col :span="15" class="expressionCol" style="padding: 0px">
            <div class="expressionBtn">
              <a-button size="small" @click="expressionBtn('+')">+</a-button>
              <a-button size="small" @click="expressionBtn('-')">-</a-button>
              <a-button size="small" @click="expressionBtn('*')">*</a-button>
              <a-button size="small" @click="expressionBtn('/')">/</a-button>
              <a-button size="small" @click="expressionBtn('(')">(</a-button>
              <a-button size="small" @click="expressionBtn(')')">)</a-button>
            </div>

            <a-textarea
              v-model:value="state.expressionValue"
              placeholder="可以使用左侧列表点击输入指标"
              :auto-size="{ minRows: 10, maxRows: 10 }"
              @blur="expressionBlur"
              style="border: none">
            </a-textarea>
            <a style="position: absolute; bottom: 15px; right: 60px" @click="clearChange">清空</a>
            <a style="position: absolute; bottom: 15px; right: 15px" @click="inspectCheck">检查</a>
          </a-col>
        </a-row>
      </div>
    </a-modal>
  </div>
</template>
<script lang="ts" setup>
  import { reactive, watch, ref, onMounted } from 'vue';
  import { Modal as AModal } from 'ant-design-vue';
  import { props as _props } from './props';
  import { SearchOutlined, InfoCircleFilled } from '@ant-design/icons-vue';
  import { getMetricAll, postMetricCompositeFormulaCheck } from '/@/api/targetDefinition';
  import { useMessage } from '/@/hooks/web/useMessage';
  const { createMessage, createConfirm } = useMessage();

  defineOptions({
    name: 'ExpressionSample',
  });

  const emit = defineEmits(['visible_expression', 'expression_dimensions', 'expression_value', 'expression_id']);
  const props = defineProps(_props);

  // console.log('props---', props);
  const state = reactive({
    visible_expression: false,
    formularOptions: [],
    formularOptionsNew: [],
    expressionSearch: '',
    expressionValue: '',
    expressionIds: [],
    cursorPosition: '', //保存光标的位置
  });

  // 监听弹层显示隐藏
  watch(
    () => props.visible,
    () => {
      if (props.visible) {
        state.visible_expression = true;
        state.expressionValue = props.expressionValue;
      } else {
        state.visible_expression = false;
      }
    },
  );
  // 搜索
  function inputChange() {
    let filteredArray = state.formularOptionsNew.filter(item => {
      const regex = new RegExp(state.expressionSearch, 'i');
      return regex.test(item.name);
    });
    state.formularOptions = filteredArray;
  }

  // 获取所有的指标信息
  getMetricAll().then(res => {
    state.formularOptions = res.data;
    state.formularOptionsNew = res.data;
  });

  // 失去焦点时
  function expressionBlur(e) {
    state.cursorPosition = e.srcElement.selectionStart;
  }

  // 点击左侧的指标
  function expressionListClick(value) {
    // console.log(' state.cursorPosition---', state.cursorPosition);
    if (typeof state.cursorPosition == 'number') {
      //插入到指定光标处
      let right = state.expressionValue.slice(0, state.cursorPosition);
      let left = state.expressionValue.slice(state.cursorPosition);
      state.expressionValue = right + '${' + value + '}' + left;
    } else {
      //没有指定插入直接添加到最后
      state.expressionValue += '${' + value + '}';
    }
  }

  // 加减乘除
  function expressionBtn(value) {
    if (typeof state.cursorPosition == 'number') {
      //插入到指定光标处
      let right = state.expressionValue.slice(0, state.cursorPosition);
      let left = state.expressionValue.slice(state.cursorPosition);
      state.expressionValue = right + value + left;
    } else {
      //没有指定插入直接添加到最后
      state.expressionValue += value;
    }
  }
  // 清空
  function clearChange() {
    state.expressionValue = '';
    state.cursorPosition = '';
    state.expressionIds = [];
  }

  // 封装：将表达式${}中的name中替换成id
  function replaceNameWithValue(arr, val) {
    let result = val;
    arr.forEach(item => {
      const reg = new RegExp('\\${' + item.name + '}', 'g');
      result = result.replace(reg, '${' + item.id + '}');
    });
    return result;
  }

  // 公式检查
  function inspectCheck() {
    // console.log(' state.expressionValue-----', state.expressionValue);
    var result = replaceNameWithValue(state.formularOptionsNew, state.expressionValue);
    // console.log('result----', result);
    postMetricCompositeFormulaCheck({ formulaData: result }).then(res => {
      if (res.data) {
        createMessage.success('检查成功');
      }
    });
  }

  // 提交
  function handleExpression() {
    let matchArray = state.expressionValue.match(/\$\{([^}]+)\}/g);
    let extractedValues = matchArray.map(match => match.match(/\$\{([^}]+)\}/)[1]);

    let extractedArrId = [];
    state.formularOptionsNew.forEach(item => {
      extractedValues.forEach(value => {
        if (item.name === value) {
          extractedArrId.push(item.id);
        }
      });
    });

    let uniqueArray = Array.from(new Set(extractedArrId));

    var result = replaceNameWithValue(state.formularOptionsNew, state.expressionValue);
    // console.log('extractedArrId-----', extractedArrId);
    emit('expression_id', result);
    emit('expression_value', state.expressionValue);
    emit('expression_dimensions', uniqueArray);
    emit('visible_expression', false);
  }
  // 取消
  function cancelExpression() {
    // console.log('取消-----');
    emit('visible_expression', false);
  }
</script>
<style lang="less" scoped>
  .expressionList {
    margin: 10px 0px;
    height: 260px;
    overflow: auto;
    p {
      span {
        cursor: pointer;
      }
    }
  }
  .expressionCol {
    border: 1px solid #e7ebf3;
    border-radius: 4px;
    margin: 10px 7px;
  }
  .expressionBtn {
    border-bottom: 1px solid #e7ebf3;
    padding: 10px;
    button {
      margin-right: 10px;
      border-radius: 4px;
    }
  }
</style>
