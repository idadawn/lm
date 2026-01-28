<template>
  <div class="page-content-wrapper page-content-wrapper-form">
    <div class="page-content-wrapper-form-body px-10px">
      <ScrollContainer>
        <!-- 动态表单 -->
        {{ formData.items }}
        <a-form ref="formRef" :model="formData.items" :colon="false" v-bind="formTailLayout" @finish="onFinish">
          <template v-for="(element, index) in formData.items">
            <z-temp-field
              :items="formData.items"
              :item="element"
              typeProps="t"
              labelProps="title"
              keyProps="v"
              listProps="options"
              listLabelProps="title"
              listValueProps="key"
              requiredProps="isReq"
              @change="changeItem"
              :params="index" />
          </template>
          <a-form-item>
            <a-button type="primary" html-type="submit">Submit</a-button>
          </a-form-item>
        </a-form>
        <!-- 公式输入框 -->
        {{ state.formulaValue }}
        <a-form-item label="公式输入框">
          <z-formula-input
            placeholder="输入「@」后选择指标"
            :options="state.formularOptions"
            :initValue="state.initValue"
            v-model:value="state.formulaValue"
            @change="afterChange" />
        </a-form-item>
        <div class="my-10px">
          <a-alert message="下拉框、多选框、单选框、树形选择" type="warning" :show-icon="false" />
        </div>
        <a-form ref="formRef" :colon="false" :model="dataForm" :labelCol="{ style: { width: '110px' } }">
          <a-form-item label="当前状态">
            <a-switch v-model:checked="dataForm.EnabledMark" />
          </a-form-item>
          <a-form-item label="请假类别">
            <a-radio-group v-model:value="dataForm.LeaveType">
              <a-radio v-for="item in radioOptions" :key="item" :value="item">{{ item }} </a-radio>
            </a-radio-group>
          </a-form-item>
          <a-form-item label="运输工具">
            <a-radio-group v-model:value="dataForm.Conveyance">
              <a-radio v-for="item in radioOptions1" :key="item" :value="item">
                <i :class="'icon-ym icon-ym-extend-' + item" style="font-size: 18px"></i>
              </a-radio>
            </a-radio-group>
          </a-form-item>
          <a-form-item label="传统节日">
            <a-checkbox-group v-model:value="dataForm.Festival">
              <a-checkbox v-for="item in checkboxOptions" :value="item">{{ item }}</a-checkbox>
            </a-checkbox-group>
          </a-form-item>
          <a-form-item label="角色类型">
            <jnpf-select v-model:value="dataForm.RoleType" :options="options" />
          </a-form-item>
          <a-form-item label="树形下拉框">
            <jnpf-tree-select
              mode="multiple"
              v-model:value="dataForm.parentId"
              :options="treeData"
              allowClear
              @change="getValue" />
          </a-form-item>
          <a-form-item label="级联选择器">
            <jnpf-cascader v-model:value="dataForm.cascader" placeholder="请选择" :options="options1" allowClear />
          </a-form-item>
          <a-form-item label="全选下拉框">
            <a-select
              v-model:value="value"
              mode="multiple"
              style="width: 100%"
              placeholder="select one country"
              @change="selectAll"
              option-label-prop="children">
              <a-select-option value="all" label="China">
                <span role="img" aria-label="全选">🇨</span>
                &nbsp;&nbsp;全选
              </a-select-option>
              <a-select-option value="usa" label="USA">
                <span role="img" aria-label="USA">🇺🇸</span>
                &nbsp;&nbsp;USA (美国)
              </a-select-option>
              <a-select-option value="japan" label="Japan">
                <span role="img" aria-label="Japan">🇯🇵</span>
                &nbsp;&nbsp;Japan (日本)
              </a-select-option>
              <a-select-option value="korea" label="Korea">
                <span role="img" aria-label="Korea">🇰🇷</span>
                &nbsp;&nbsp;Korea (韩国)
              </a-select-option>
            </a-select>
          </a-form-item>
        </a-form>
      </ScrollContainer>
    </div>
  </div>
</template>
<script lang="ts" setup>
  import { reactive, toRefs, ref, onMounted, watch } from 'vue';
  import { ScrollContainer } from '/@/components/Container';
  import type { FormInstance } from 'ant-design-vue';
  import { useBaseStore } from '/@/store/modules/base';
  import { getPositionSelector } from '/@/api/permission/position';
  import { ZTempField } from '/@/components/ZTempField';
  import { ZFormulaInput } from '/@/components/ZFormulaInput';

  defineOptions({ name: 'extend-formDemo-examples' });

  interface State {
    elements: any[];
    dataForm: any;
    radioOptions: any[];
    radioOptions1: any[];
    checkboxOptions: any[];
    options: any[];
    treeData: any[];
    options1: any[];
    inputVal: string;
    formulaValue: Object;
    initValue: Object;
  }

  const value = ref([]);
  const selectAll = () => {
    if (value.value.includes('all')) {
      value.value = ['usa', 'japan', 'korea'];
    }
  };
  watch(value, val => {
    // if (val.includes('all')) {
    //   value.value = ['usa', 'japan', 'korea'];
    // }
  });

  const onFinish = values => {
  };

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
    items: [
      {
        key: 'name',
        title: '姓名',
        des: '姓名描述',
        v: null,
        isReq: true,
        t: 'input',
        enumName: 'string',
        options: [{}],
        range: {
          min: null,
          max: null,
        },
      },
      {
        key: 'dataSource',
        title: '数据来源',
        des: '数据来源描述',
        v: null,
        isReq: true,
        t: 'select',
        enumName: 'string',
        // 下拉选项
        options: [
          {
            key: 'dictionary',
            title: '数据字典',
            // 动态表单
            items: [
              {
                key: 'a1',
                title: '数据字典属性1',
                des: '数据字典属性1描述',
                v: null,
                isReq: true,
                t: 'input',
                enumName: 'string',
                options: [{}],
                range: {
                  min: null,
                  max: null,
                },
              },
            ],
          },
          { key: 'api', title: '远端数据', items: [] },
          { key: 'organize', title: '组织数据', items: [] },
          { key: 'department', title: '部门数据', items: [] },
        ],
        range: {
          min: null,
          max: null,
        },
      },
    ],
  });
  const state = reactive<State>({
    initValue: {
      formula: '1+superman+2-batman+3*aquaman+4/wonderwoman',
      vars: {
        superman: '指标1',
        batman: '指标2',
        aquaman: '指标3',
        wonderwoman: '指标4',
      },
    },
    formulaValue: {
      formula: '',
      vars: {},
    },
    model: {
      formula: '1+superman+2-batman+3*aquaman+4/wonderwoman',
      vars: {
        superman: 'ClarkKent',
        batman: 'BruceWayne',
        aquaman: 'ArthurCurry',
        wonderwoman: 'DianaPrince',
      },
    },
    formularOptions: [
      {
        field: 'superman',
        name: '指标1',
      },
      {
        field: 'batman',
        name: '指标2',
      },
      {
        field: 'theflash',
        name: '指标3',
      },
      {
        field: 'wonderwoman',
        name: '指标4',
      },
      {
        field: 'aquaman',
        name: '指标5',
      },
      {
        field: 'cyborg',
        name: '指标6',
      },
      {
        field: 'greenlantern',
        name: '指标7',
      },
    ],
    elements: [
      {
        id: 'B0000000000000002350',
        factorTypeName: '保额',
        factorType: 'name',
        factorValue: '',
        isDisplay: 'Y',
        isMustInput: 'N',
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
    inputVal: '',
    dataForm: {
      EnabledMark: true,
      LeaveType: '年假',
      Conveyance: 'car',
      Festival: ['春节', '清明节'],
      RoleType: '',
      Position: '',
      cascader: [],
      options1: [],
    },
    radioOptions: ['事假', '病假', '婚假', '产假', '丧假', '年假', '调休', '其他'],
    radioOptions1: ['bicycle', 'motorcycle', 'plane', 'truck', 'subway', 'car', 'bus', 'rocket', 'train', 'ambulance'],
    checkboxOptions: ['春节', '清明节', '七夕节', '五一节', '端午节', '中秋节', '重阳节', '除夕', '元旦'],
    options: [],
    treeData: [
      {
        fullName: 'ZC',
        icon: 'icon-ym icon-ym-tree-organization3',
        enabledMark: 1,
        type: 'company',
        sortCode: 0,
        organizeIdTree: '96240625-934F-490B-8AA6-0BC775B18468',
        organize: 'ZC',
        id: '96240625-934F-490B-8AA6-0BC775B18468',
        parentId: '-1',
        hasChildren: true,
        children: [
          {
            fullName: '市场部',
            icon: 'icon-ym icon-ym-tree-department1',
            enabledMark: 1,
            type: 'department',
            sortCode: 0,
            organizeIdTree: '96240625-934F-490B-8AA6-0BC775B18468,EBA8E097-971B-47F7-9892-9A81F74EADE7',
            organize: 'ZC/市场部',
            id: 'EBA8E097-971B-47F7-9892-9A81F74EADE7',
            parentId: '96240625-934F-490B-8AA6-0BC775B18468',
            hasChildren: true,
            children: [
              {
                fullName: '市场拓展经理',
                icon: 'icon-ym icon-ym-tree-position1',
                enabledMark: 1,
                type: 'position',
                sortCode: -2,
                organizeIdTree: null,
                organize: 'ZC/市场部',
                id: '0AE8DE21-23F4-4BC4-8BB7-3999EED977E5',
                parentId: 'EBA8E097-971B-47F7-9892-9A81F74EADE7',
                hasChildren: false,
                children: null,
                num: 1,
                isLeaf: true,
              },
            ],
            num: 1,
            isLeaf: false,
          },
        ],
        num: 9,
        isLeaf: false,
      },
    ],
    options1: [
      {
        id: 'zhinan',
        fullName: '指南',
        children: [
          {
            id: 'shejiyuanze',
            fullName: '设计原则',
            children: [
              {
                id: 'yizhi',
                fullName: '一致',
              },
              {
                id: 'fankui',
                fullName: '反馈',
              },
              {
                id: 'xiaolv',
                fullName: '效率',
              },
              {
                id: 'kekong',
                fullName: '可控',
              },
            ],
          },
          {
            id: 'daohang',
            fullName: '导航',
            children: [
              {
                id: 'cexiangdaohang',
                fullName: '侧向导航',
              },
              {
                id: 'dingbudaohang',
                fullName: '顶部导航',
              },
            ],
          },
        ],
      },
      {
        id: 'zujian',
        fullName: '组件',
        children: [
          {
            id: 'basic',
            fullName: 'Basic',
            children: [
              {
                id: 'layout',
                fullName: 'Layout 布局',
              },
              {
                id: 'color',
                fullName: 'Color 色彩',
              },
              {
                id: 'typography',
                fullName: 'Typography 字体',
              },
              {
                id: 'icon',
                fullName: 'Icon 图标',
              },
              {
                id: 'button',
                fullName: 'Button 按钮',
              },
            ],
          },
          {
            id: 'form',
            fullName: 'Form',
            children: [
              {
                id: 'radio',
                fullName: 'Radio 单选框',
              },
              {
                id: 'checkbox',
                fullName: 'Checkbox 多选框',
              },
              {
                id: 'input',
                fullName: 'Input 输入框',
              },
              {
                id: 'input-number',
                fullName: 'InputNumber 计数器',
              },
              {
                id: 'select',
                fullName: 'Select 选择器',
              },
              {
                id: 'cascader',
                fullName: 'Cascader 级联选择器',
              },
              {
                id: 'switch',
                fullName: 'Switch 开关',
              },
              {
                id: 'slider',
                fullName: 'Slider 滑块',
              },
              {
                id: 'time-picker',
                fullName: 'TimePicker 时间选择器',
              },
              {
                id: 'date-picker',
                fullName: 'DatePicker 日期选择器',
              },
              {
                id: 'datetime-picker',
                fullName: 'DateTimePicker 日期时间选择器',
              },
              {
                id: 'upload',
                fullName: 'Upload 上传',
              },
              {
                id: 'rate',
                fullName: 'Rate 评分',
              },
              {
                id: 'form',
                fullName: 'Form 表单',
              },
            ],
          },
          {
            id: 'data',
            fullName: 'Data',
            children: [
              {
                id: 'table',
                fullName: 'Table 表格',
              },
              {
                id: 'tag',
                fullName: 'Tag 标签',
              },
              {
                id: 'progress',
                fullName: 'Progress 进度条',
              },
              {
                id: 'tree',
                fullName: 'Tree 树形控件',
              },
              {
                id: 'pagination',
                fullName: 'Pagination 分页',
              },
              {
                id: 'badge',
                fullName: 'Badge 标记',
              },
            ],
          },
          {
            id: 'notice',
            fullName: 'Notice',
            children: [
              {
                id: 'alert',
                fullName: 'Alert 警告',
              },
              {
                id: 'loading',
                fullName: 'Loading 加载',
              },
              {
                id: 'message',
                fullName: 'Message 消息提示',
              },
              {
                id: 'message-box',
                fullName: 'MessageBox 弹框',
              },
              {
                id: 'notification',
                fullName: 'Notification 通知',
              },
            ],
          },
          {
            id: 'navigation',
            fullName: 'Navigation',
            children: [
              {
                id: 'menu',
                fullName: 'NavMenu 导航菜单',
              },
              {
                id: 'tabs',
                fullName: 'Tabs 标签页',
              },
              {
                id: 'breadcrumb',
                fullName: 'Breadcrumb 面包屑',
              },
              {
                id: 'dropdown',
                fullName: 'Dropdown 下拉菜单',
              },
              {
                id: 'steps',
                fullName: 'Steps 步骤条',
              },
            ],
          },
          {
            id: 'others',
            fullName: 'Others',
            children: [
              {
                id: 'dialog',
                fullName: 'Dialog 对话框',
              },
              {
                id: 'tooltip',
                fullName: 'Tooltip 文字提示',
              },
              {
                id: 'popover',
                fullName: 'Popover 弹出框',
              },
              {
                id: 'card',
                fullName: 'Card 卡片',
              },
              {
                id: 'carousel',
                fullName: 'Carousel 走马灯',
              },
              {
                id: 'collapse',
                fullName: 'Collapse 折叠面板',
              },
            ],
          },
        ],
      },
      {
        id: 'ziyuan',
        fullName: '资源',
        children: [
          {
            id: 'axure',
            fullName: 'Axure Components',
          },
          {
            id: 'sketch',
            fullName: 'Sketch Templates',
          },
          {
            id: 'jiaohu',
            fullName: '组件交互文档',
          },
        ],
      },
    ],
  });

  const formRef = ref<FormInstance>();

  const changeItem = e => {
  };
  const afterChange = v => {
  };
  const { dataForm, radioOptions, radioOptions1, checkboxOptions, options, treeData, options1 } = toRefs(state);
  const baseStore = useBaseStore();

  async function init() {
    state.options = (await baseStore.getDictionaryData('RoleType')) as any[];
    getPositionSelector().then(res => {
      // state.treeData = res.data.list;
    });
  }
  function getValue(value) {
    state.dataForm.Position = value;
  }

  onMounted(() => {
    init();
  });
</script>
