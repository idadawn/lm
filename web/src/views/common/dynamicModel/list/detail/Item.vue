<template>
  <a-col
    :class="[...getConfig.className, getConfig.layout === 'colFormItem' ? 'ant-col-item' : '']"
    :span="getConfig.span"
    v-if="!getConfig.noShow && (!getConfig.visibility || (Array.isArray(getConfig.visibility) && getConfig.visibility.includes('pc')))">
    <template v-if="getConfig.layout === 'colFormItem'">
      <template v-if="getConfig.jnpfKey === 'divider'">
        <jnpf-divider :contentPosition="item.contentPosition" :content="item.content" />
      </template>
      <template v-else>
        <a-form-item :name="item.__vModel__" :labelCol="getLabelCol">
          <template #label v-if="getConfig.showLabel">
            {{ getConfig.label || '' }}<BasicHelp :text="getConfig.tipLabel" v-if="getConfig.label && getConfig.tipLabel" />
          </template>
          <template v-if="getConfig.jnpfKey === 'text'">
            <jnpf-text :content="item.content" :textStyle="item.textStyle" />
          </template>
          <template v-else-if="getConfig.jnpfKey === 'link'">
            <jnpf-link :content="item.content" :href="item.href" :target="item.target" :textStyle="item.textStyle" />
          </template>
          <template v-else-if="getConfig.jnpfKey === 'alert'">
            <jnpf-alert
              :title="item.title"
              :type="item.type"
              :closable="item.closable"
              :showIcon="item.showIcon"
              :description="item.description"
              :closeText="item.closeText" />
          </template>
          <template v-else-if="getConfig.jnpfKey === 'groupTitle'">
            <jnpf-group-title :content="item.content" :contentPosition="item.contentPosition" :helpMessage="item.helpMessage" />
          </template>
          <template v-else-if="getConfig.jnpfKey === 'button'">
            <jnpf-button :align="item.align" :buttonText="item.buttonText" :type="item.type" :disabled="item.disabled" />
          </template>
          <template v-else-if="getConfig.jnpfKey === 'uploadFile'">
            <jnpf-upload-file v-model:value="getConfig.defaultValue" detailed disabled />
          </template>
          <template v-else-if="getConfig.jnpfKey === 'uploadImg'">
            <jnpf-upload-img v-model:value="getConfig.defaultValue" detailed disabled />
          </template>
          <template v-else-if="getConfig.jnpfKey === 'colorPicker'">
            <jnpf-color-picker v-model:value="getConfig.defaultValue" :showAlpha="item.showAlpha" :colorFormat="item.colorFormat" disabled />
          </template>
          <template v-else-if="getConfig.jnpfKey === 'rate'">
            <jnpf-rate v-model:value="getConfig.defaultValue" :count="item.count" :allowHalf="item.allowHalf" disabled />
          </template>
          <template v-else-if="getConfig.jnpfKey === 'slider'">
            <jnpf-slider v-model:value="getConfig.defaultValue" :min="item.min" :max="item.max" :step="item.step" disabled />
          </template>
          <template v-else-if="getConfig.jnpfKey === 'editor'">
            <div v-html="getConfig.defaultValue"></div>
          </template>
          <template v-else-if="getConfig.jnpfKey === 'relationForm'">
            <p class="link-text" @click="toDetail(item)">{{ item.name }}</p>
          </template>
          <template v-else-if="getConfig.jnpfKey === 'popupSelect'">
            <p>{{ item.name }}</p>
          </template>
          <template v-else-if="['relationFormAttr', 'popupAttr'].includes(getConfig.jnpfKey)">
            <p v-if="!item.__vModel__">
              {{ relationData[item.relationField] && relationData[item.relationField][item.showField] ? relationData[item.relationField][item.showField] : '' }}
            </p>
            <p v-else>{{ item.__config__.defaultValue }}</p>
          </template>
          <template v-else-if="getConfig.jnpfKey === 'barcode'">
            <jnpf-barcode
              :format="item.format"
              :lineColor="item.lineColor"
              :background="item.background"
              :width="item.width"
              :height="item.height"
              :staticText="item.staticText"
              :dataType="item.dataType"
              :relationField="item.relationField + '_id'"
              :formData="formData" />
          </template>
          <template v-else-if="getConfig.jnpfKey === 'qrcode'">
            <jnpf-qrcode
              :format="item.format"
              :colorLight="item.colorLight"
              :colorDark="item.colorDark"
              :size="item.size"
              :staticText="item.staticText"
              :dataType="item.dataType"
              :relationField="item.relationField + '_id'"
              :formData="formData" />
          </template>
          <template v-else-if="getConfig.jnpfKey === 'inputNumber'">
            <jnpf-input-number
              v-model:value="getConfig.defaultValue"
              :precision="item.precision"
              :addonBefore="item.addonBefore"
              :addonAfter="item.addonAfter"
              :thousands="item.thousands"
              :isAmountChinese="item.isAmountChinese"
              disabled
              detailed />
          </template>
          <template v-else-if="getConfig.jnpfKey === 'calculate'">
            <jnpf-calculate
              :expression="item.expression"
              :isStorage="item.isStorage"
              :formData="formData"
              :precision="item.precision"
              :thousands="item.thousands"
              :isAmountChinese="item.isAmountChinese"
              detailed />
          </template>
          <template v-else>
            <p>
              <span v-if="item.addonBefore">{{ item.addonBefore }}</span>
              {{ getValue(item.__config__?.defaultValue || '') }}
              <span v-if="item.addonAfter">{{ item.addonAfter }}</span>
            </p>
          </template>
        </a-form-item>
      </template>
    </template>
    <template v-else>
      <template v-if="getConfig.jnpfKey === 'card'">
        <a-card :hoverable="item.shadow === 'hover'" :size="formConf.size">
          <template #title v-if="item.header">{{ item.header }}<BasicHelp :text="getConfig.tipLabel" v-if="getConfig.tipLabel" /></template>
          <a-row>
            <Item v-for="(childItem, childIndex) in getConfig.children" v-bind="getBindValue" :key="childIndex" :item="childItem" @toDetail="toDetail" />
          </a-row>
        </a-card>
      </template>
      <a-row v-if="getConfig.jnpfKey === 'row'">
        <Item v-for="(childItem, childIndex) in getConfig.children" v-bind="getBindValue" :key="childIndex" :item="childItem" @toDetail="toDetail" />
      </a-row>
      <template v-if="getConfig.jnpfKey === 'tab'">
        <a-tabs :type="item.type" :tabPosition="item.tabPosition" :size="formConf.size" v-model:activeKey="getConfig.active">
          <a-tab-pane v-for="pane in getConfig.children" :key="pane.name" :tab="pane.title">
            <Item v-for="(childItem, childIndex) in pane.__config__.children" v-bind="getBindValue" :key="childIndex" :item="childItem" @toDetail="toDetail" />
          </a-tab-pane>
        </a-tabs>
      </template>
      <template v-if="getConfig.jnpfKey === 'collapse'">
        <a-collapse :ghost="item.ghost" :expandIconPosition="item.expandIconPosition" :accordion="item.accordion" v-model:activeKey="getConfig.active">
          <a-collapse-panel v-for="pane in getConfig.children" :key="pane.name" :header="pane.title">
            <Item v-for="(childItem, childIndex) in pane.__config__.children" v-bind="getBindValue" :key="childIndex" :item="childItem" @toDetail="toDetail" />
          </a-collapse-panel>
        </a-collapse>
      </template>
      <template v-if="getConfig.jnpfKey === 'table'">
        <a-form-item v-if="!getConfig.noShow">
          <JnpfGroupTitle :content="getConfig.label" v-if="getConfig.showTitle && getConfig.label" :helpMessage="getConfig.tipLabel" :bordered="false" />
          <a-table
            :data-source="item.__config__.defaultValue"
            :columns="getColumns"
            size="small"
            :pagination="false"
            :scroll="{ x: 'max-content' }"
            :bordered="formConf.formStyle === 'word-form'">
            <template #headerCell="{ column }">
              {{ column.title }}
              <BasicHelp v-if="column.title && column.__config__?.tipLabel" :text="column.__config__.tipLabel" />
            </template>
            <template #bodyCell="{ column, record, index }">
              <template v-if="column.key === 'index'">{{ index + 1 }}</template>
              <template v-else-if="column.__config__?.jnpfKey === 'uploadFile'">
                <jnpf-upload-file v-model:value="record[column.dataIndex]" detailed disabled />
              </template>
              <template v-else-if="column.__config__?.jnpfKey === 'uploadImg'">
                <jnpf-upload-img v-model:value="record[column.dataIndex]" detailed disabled />
              </template>
              <template v-else-if="column.__config__?.jnpfKey === 'colorPicker'">
                <jnpf-color-picker v-model:value="record[column.dataIndex]" :showAlpha="column.showAlpha" :colorFormat="column.colorFormat" disabled />
              </template>
              <template v-else-if="column.__config__?.jnpfKey === 'rate'">
                <jnpf-rate v-model:value="record[column.dataIndex]" :count="column.count" :allowHalf="column.allowHalf" disabled />
              </template>
              <template v-else-if="column.__config__?.jnpfKey === 'slider'">
                <jnpf-slider v-model:value="record[column.dataIndex]" :min="column.min" :max="column.max" :step="column.step" disabled />
              </template>
              <template v-else-if="column.__config__?.jnpfKey === 'relationForm'">
                <p class="link-text" @click="toTableDetail(column, record[column.dataIndex + '_id'])">{{ record[column.dataIndex] }}</p>
              </template>
              <template v-else-if="['relationFormAttr', 'popupAttr'].includes(column.__config__?.jnpfKey)">
                <p v-if="!record[column.dataIndex]">{{ record[column.relationField.split('_jnpfTable_')[0] + '_' + column.showField] }}</p>
                <p v-else>{{ record[column.dataIndex] }}</p>
              </template>
              <template v-else-if="column.__config__?.jnpfKey === 'inputNumber'">
                <jnpf-input-number
                  v-model:value="record[column.dataIndex]"
                  :precision="column.precision"
                  :addonBefore="column.addonBefore"
                  :addonAfter="column.addonAfter"
                  :thousands="column.thousands"
                  :isAmountChinese="column.isAmountChinese"
                  disabled
                  detailed />
              </template>
              <template v-else-if="column.__config__?.jnpfKey === 'calculate'">
                <jnpf-calculate
                  :rowIndex="index"
                  :expression="column.expression"
                  :isStorage="column.isStorage"
                  :formData="formData"
                  :precision="column.precision"
                  :thousands="column.thousands"
                  :isAmountChinese="column.isAmountChinese"
                  detailed />
              </template>
              <template v-else>
                <p>
                  <span v-if="column.addonBefore">{{ column.addonBefore }}</span>
                  {{ getValue(record[column.dataIndex]) }}
                  <span v-if="column.addonAfter">{{ column.addonAfter }}</span>
                </p>
              </template>
            </template>
            <template #summary v-if="item.__config__.defaultValue.length && item.showSummary">
              <a-table-summary fixed>
                <a-table-summary-row>
                  <a-table-summary-cell :index="0">合计</a-table-summary-cell>
                  <a-table-summary-cell v-for="(item, index) in getColumnSum" :key="index" :index="index + 1">{{ item }}</a-table-summary-cell>
                </a-table-summary-row>
              </a-table-summary>
            </template>
          </a-table>
        </a-form-item>
      </template>
      <template v-if="getConfig.jnpfKey === 'tableGrid'">
        <table
          class="table-grid-box"
          :style="{
            '--borderType': item.__config__.borderType,
            '--borderColor': item.__config__.borderColor,
            '--borderWidth': item.__config__.borderWidth + 'px',
          }">
          <tbody>
            <tr v-for="(tr, index) in getConfig.children" :key="index">
              <td
                v-for="(td, i) in tr.__config__.children"
                :key="i"
                :colspan="td.__config__.colspan"
                :rowspan="td.__config__.rowspan"
                v-show="!td.__config__.merged">
                <Item
                  v-for="(childItem, childIndex) in td.__config__.children"
                  v-bind="getBindValue"
                  :key="childIndex"
                  :item="childItem"
                  @toDetail="toDetail" />
              </td>
            </tr>
          </tbody>
        </table>
      </template>
    </template>
  </a-col>
</template>

<script lang="ts" setup>
  import { computed, unref } from 'vue';
  import { omit } from 'lodash-es';
  import { thousandsFormat } from '/@/utils/jnpf';

  defineOptions({ name: 'Item' });
  const props = defineProps({
    item: { type: Object, required: true },
    formConf: { type: Object, required: true },
    relationData: { type: Object, default: () => {} },
    formData: { type: Object },
    loading: { type: Boolean, default: false },
  });
  const emit = defineEmits(['toDetail']);

  const getBindValue = computed(() => ({ ...omit(props, ['item']) }));
  const getConfig = computed(() => props.item.__config__);
  const getLabelCol = computed(() => {
    const globalLabelWidth = props.formConf.labelWidth;
    let labelCol = {};
    if (props.formConf.labelPosition !== 'top' && unref(getConfig).showLabel) {
      let labelWidth = (unref(getConfig).labelWidth || globalLabelWidth) + 'px';
      if (!unref(getConfig).showLabel) labelWidth = '0px';
      labelCol = { style: { width: labelWidth } };
    }
    return labelCol;
  });
  const getColumns = computed(() => {
    if (unref(getConfig).jnpfKey !== 'table') return [];
    const noColumn = { width: 50, title: '序号', dataIndex: 'index', key: 'index', align: 'center', customRender: ({ index }) => index + 1, fixed: 'left' };
    const list = unref(getConfig)
      .children.filter(
        o => !o.__config__.noShow && (!o.__config__.visibility || (Array.isArray(o.__config__.visibility) && o.__config__.visibility.includes('pc'))),
      )
      .map(o => ({
        ...o,
        title: o.__config__?.label,
        dataIndex: o.__vModel__,
        width: o.__config__?.columnWidth || undefined,
      }));
    return [noColumn, ...list];
  });
  const getColumnSum = computed(() => {
    if (unref(getConfig).jnpfKey !== 'table') return [];
    const sums: any[] = [];
    const isSummary = key => props.item.summaryField.includes(key);
    const useThousands = key => unref(getConfig).children.some(o => o.__vModel__ === key && o.thousands);
    const tableData = unref(getConfig).children.filter(o => !o.__config__.noShow && (!o.__config__.visibility || o.__config__.visibility.includes('pc')));
    tableData.forEach((column, index) => {
      let sumVal = unref(getConfig).defaultValue.reduce((sum, d) => sum + getCmpValOfRow(d, column.__vModel__), 0);
      if (!isSummary(column.__vModel__)) sumVal = '';
      sumVal = Number.isNaN(sumVal) ? '' : sumVal;
      const realVal = sumVal && !Number.isInteger(sumVal) ? Number(sumVal).toFixed(2) : sumVal;
      sums[index] = useThousands(column.__vModel__) ? thousandsFormat(realVal) : realVal;
    });
    return sums;
  });

  function toDetail(item) {
    emit('toDetail', item);
  }
  function toTableDetail(item, value) {
    item.__config__.defaultValue = value;
    emit('toDetail', item);
  }
  function getValue(value) {
    return Array.isArray(value) ? value.join() : value;
  }
  function getCmpValOfRow(row, key) {
    const isSummary = key => props.item.summaryField.includes(key);
    if (!props.item.summaryField.length || !isSummary(key)) return 0;
    const target = row[key];
    if (!target) return 0;
    const data = isNaN(target) ? 0 : Number(target);
    return data;
  }
</script>
