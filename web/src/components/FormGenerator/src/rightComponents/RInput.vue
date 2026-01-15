<template>
  <a-form-item label="默认值">
    <a-input v-model:value="activeData.__config__.defaultValue" placeholder="请输入默认值" />
  </a-form-item>
  <a-form-item label="前缀" v-show="showType === 'pc'">
    <a-input v-model:value="activeData.addonBefore" placeholder="请输入前缀" />
  </a-form-item>
  <a-form-item label="后缀" v-show="showType === 'pc'">
    <a-input v-model:value="activeData.addonAfter" placeholder="请输入后缀" />
  </a-form-item>
  <a-form-item label="前图标" v-show="showType === 'pc'">
    <jnpf-icon-picker v-model:value="activeData.prefixIcon" placeholder="请选择前图标" />
  </a-form-item>
  <a-form-item label="后图标" v-show="showType === 'pc'">
    <jnpf-icon-picker v-model:value="activeData.suffixIcon" placeholder="请选择后图标" />
  </a-form-item>
  <a-form-item label="最多输入">
    <a-input-number v-model:value="activeData.maxlength" placeholder="请输入字符长度" :min="0" addonAfter="个字符" />
  </a-form-item>
  <a-form-item label="能否清空">
    <a-switch v-model:checked="activeData.clearable" />
  </a-form-item>
  <a-form-item label="是否密码">
    <a-switch v-model:checked="activeData.showPassword" @change="activeData.__config__.renderKey = +new Date()" />
  </a-form-item>
  <a-form-item label="是否只读" v-show="showType === 'pc'">
    <a-switch v-model:checked="activeData.readonly" />
  </a-form-item>
  <a-form-item label="是否禁用">
    <a-switch v-model:checked="activeData.disabled" />
  </a-form-item>
  <a-form-item label="是否隐藏">
    <a-switch v-model:checked="activeData.__config__.noShow" />
  </a-form-item>
  <a-divider>校验规则</a-divider>
  <a-form-item label="是否必填">
    <a-switch v-model:checked="activeData.__config__.required" />
  </a-form-item>
  <a-form-item>
    <template #label>是否唯一<BasicHelp text="输入值唯一性校验" /></template>
    <a-switch v-model:checked="activeData.__config__.unique" />
  </a-form-item>
  <div v-for="(item, index) in activeData.__config__.regList" :key="index" class="reg-item">
    <span class="close-btn" @click="activeData.__config__.regList.splice(index, 1)">
      <i class="icon-ym icon-ym-nav-close" />
    </span>
    <a-form-item label="表达式">
      <a-input v-model:value="item.pattern" placeholder="请输入正则" @change="onPatternChange" />
    </a-form-item>
    <a-form-item label="错误提示" style="margin-bottom: 0">
      <a-input v-model:value="item.message" placeholder="请输入错误提示" />
    </a-form-item>
  </div>
  <div class="mt-10px">
    <a-dropdown>
      <template #overlay>
        <a-menu>
          <a-menu-item v-for="(item, i) in ruleList" :key="i" @click="addReg(item)">{{ item.label }}</a-menu-item>
        </a-menu>
      </template>
      <a-button type="primary">添加常用校验<DownOutlined /></a-button>
    </a-dropdown>
    <a-button type="primary" style="margin-left: 10px" @click="addReg()">自定义规则</a-button>
  </div>
</template>
<script lang="ts" setup>
  import { inject, computed } from 'vue';
  import { DownOutlined } from '@ant-design/icons-vue';
  import { useMessage } from '/@/hooks/web/useMessage';

  const ruleList = [
    {
      pattern: '/^\\d+$/',
      message: '请输入正确的数字',
      label: '数字',
    },
    {
      pattern: '/^0\\d{2,3}-?\\d{7,8}$/',
      message: '请输入正确的电话号码',
      label: '电话',
    },
    {
      pattern: '/^1[3456789]\\d{9}$/',
      message: '请输入正确的手机号码',
      label: '手机',
    },
    {
      pattern: '/^1[3456789]\\d{9}$|^0\\d{2,3}-?\\d{7,8}$/',
      message: '请输入正确的联系方式',
      label: '电话/手机',
    },
    {
      pattern: '/^[a-z0-9]+([._\\-]*[a-z0-9])*@([a-z0-9]+[-a-z0-9]*[a-z0-9]+.){1,63}[a-z0-9]+$/',
      message: '请输入正确的邮箱',
      label: '邮箱',
    },
    {
      pattern: '/^[1-9]\\d{5}(18|19|([23]\\d))\\d{2}((0[1-9])|(10|11|12))(([0-2][1-9])|10|20|30|31)\\d{3}[0-9Xx]$/',
      message: '请输入正确的身份证号码',
      label: '身份证',
    },
  ];

  defineOptions({ inheritAttrs: false });
  const props = defineProps(['activeData']);
  const { createMessage } = useMessage();
  const getShowType: (() => string | undefined) | undefined = inject('getShowType');
  const showType = computed(() => (getShowType as () => string | undefined)());

  function addReg(row?) {
    const item = {
      pattern: row?.pattern ? row?.pattern : '',
      message: row?.message ? row?.message : '',
    };
    props.activeData.__config__.regList.push(item);
  }
  function onPatternChange(e) {
    try {
      const val = e.target.value;
      const isRegExp = Object.prototype.toString.call(eval(val)) === '[object RegExp]';
      if (!isRegExp) createMessage.error('请输入正确的正则表达式');
    } catch {
      createMessage.error('请输入正确的正则表达式');
    }
  }
</script>
