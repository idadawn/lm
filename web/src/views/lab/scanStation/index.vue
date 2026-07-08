<template>
  <div class="scan-station-page">
    <a-tabs v-model:activeKey="activeTab" type="card" @change="handleTabChange">
      <!-- 页签1：扫码追溯 -->
      <a-tab-pane key="trace" tab="扫码追溯">
        <div class="trace-panel">
          <div class="trace-panel-main">
            <div class="scan-input-wrapper">
              <a-input
                ref="scanInputRef"
                v-model:value="scanCode"
                size="large"
                class="scan-input"
                placeholder="将光标置于此处，用扫码枪扫描样品二维码"
                :disabled="traceLoading"
                allow-clear
                @pressEnter="handleScanEnter"
              />
            </div>

            <a-spin :spinning="traceLoading">
              <div v-if="!traceResult" class="trace-empty-hint">
                <a-empty description="请扫描样品二维码，或点击右侧扫描历史查看追溯数据" />
              </div>
              <div v-else-if="!traceResult.matched" class="trace-empty-hint">
                <a-empty :description="`未查询到炉号「${traceResult.normalizedFurnaceNo || traceResult.inputCode || ''}」的追溯数据`" />
              </div>
              <div v-else class="trace-result">
                <a-card title="基本信息" size="small" class="trace-card">
                  <a-descriptions :column="2" size="small" bordered>
                    <a-descriptions-item label="归一化炉号">{{ basicInfo.furnaceNo }}</a-descriptions-item>
                    <a-descriptions-item label="产线">{{ basicInfo.lineNo }}</a-descriptions-item>
                    <a-descriptions-item label="班次">{{ basicInfo.shift }}</a-descriptions-item>
                    <a-descriptions-item label="生产日期">{{ basicInfo.prodDate }}</a-descriptions-item>
                  </a-descriptions>
                </a-card>

                <a-card title="叠片检测数据" size="small" class="trace-card">
                  <a-descriptions :column="2" size="small" bordered>
                    <a-descriptions-item label="检测日期">{{ sheetInfo.detectionDate }}</a-descriptions-item>
                    <a-descriptions-item label="规格">{{ sheetInfo.productSpecName }}</a-descriptions-item>
                    <a-descriptions-item label="宽度">{{ sheetInfo.width }}</a-descriptions-item>
                    <a-descriptions-item label="带材重量">{{ sheetInfo.coilWeight }}</a-descriptions-item>
                    <a-descriptions-item label="单卷重量">{{ sheetInfo.singleCoilWeight }}</a-descriptions-item>
                  </a-descriptions>
                </a-card>

                <a-card title="综合判定" size="small" class="trace-card">
                  <a-descriptions :column="2" size="small" bordered>
                    <a-descriptions-item label="贴标">
                      <a-tag :color="judgmentColor(judgmentInfo.labeling)">{{ display(judgmentInfo.labeling) }}</a-tag>
                    </a-descriptions-item>
                    <a-descriptions-item label="一次检验">
                      <a-tag :color="judgmentColor(judgmentInfo.firstInspection)">{{ display(judgmentInfo.firstInspection) }}</a-tag>
                    </a-descriptions-item>
                    <a-descriptions-item label="磁性能判定">
                      <a-tag :color="judgmentColor(judgmentInfo.magneticResult)">{{ display(judgmentInfo.magneticResult) }}</a-tag>
                    </a-descriptions-item>
                    <a-descriptions-item label="厚度判定">
                      <a-tag :color="judgmentColor(judgmentInfo.thicknessResult)">{{ display(judgmentInfo.thicknessResult) }}</a-tag>
                    </a-descriptions-item>
                    <a-descriptions-item label="叠片系数判定">
                      <a-tag :color="judgmentColor(judgmentInfo.laminationResult)">{{ display(judgmentInfo.laminationResult) }}</a-tag>
                    </a-descriptions-item>
                    <a-descriptions-item label="计算状态">
                      <a-tag :color="judgmentColor(judgmentInfo.calcStatus)">{{ display(judgmentInfo.calcStatus) }}</a-tag>
                    </a-descriptions-item>
                    <a-descriptions-item label="判定状态">
                      <a-tag :color="judgmentColor(judgmentInfo.judgeStatus)">{{ display(judgmentInfo.judgeStatus) }}</a-tag>
                    </a-descriptions-item>
                  </a-descriptions>
                </a-card>

                <a-card title="环样性能" size="small" class="trace-card">
                  <a-table
                    :columns="perfColumns"
                    :data-source="normalizedMagneticRecords"
                    :pagination="false"
                    size="small"
                    bordered
                    :locale="{ emptyText: '暂无' }"
                    row-key="detectionTime"
                  >
                    <template #bodyCell="{ column, record }">
                      <template v-if="column.dataIndex === 'isScratched'">
                        <a-tag :color="scratchedColor(record.isScratched)">{{ scratchedLabel(record.isScratched) }}</a-tag>
                      </template>
                      <template v-else>
                        {{ record[column.dataIndex] ?? '暂无' }}
                      </template>
                    </template>
                  </a-table>
                </a-card>

                <a-card title="单片性能" size="small" class="trace-card">
                  <a-table
                    :columns="perfColumns"
                    :data-source="normalizedSingleSheetRecords"
                    :pagination="false"
                    size="small"
                    bordered
                    :locale="{ emptyText: '暂无' }"
                    row-key="detectionTime"
                  >
                    <template #bodyCell="{ column, record }">
                      <template v-if="column.dataIndex === 'isScratched'">
                        <a-tag :color="scratchedColor(record.isScratched)">{{ scratchedLabel(record.isScratched) }}</a-tag>
                      </template>
                      <template v-else>
                        {{ record[column.dataIndex] ?? '暂无' }}
                      </template>
                    </template>
                  </a-table>
                </a-card>
              </div>
            </a-spin>
          </div>

          <div class="trace-panel-history">
            <div class="trace-history-title">扫描历史（最近 20 条）</div>
            <div class="trace-history-list">
              <div v-if="scanHistory.length === 0" class="trace-history-empty">暂无扫描记录</div>
              <div
                v-for="(item, index) in scanHistory"
                :key="`${item.time}-${index}`"
                class="trace-history-item"
                @click="handleHistoryClick(item)"
              >
                <div class="trace-history-item-code">{{ item.code }}</div>
                <div class="trace-history-item-meta">
                  <span>{{ item.time }}</span>
                  <a-tag :color="item.matched ? 'success' : 'error'">{{ item.matched ? '命中' : '未命中' }}</a-tag>
                </div>
              </div>
            </div>
          </div>
        </div>
      </a-tab-pane>

      <!-- 页签2：标签打印 -->
      <a-tab-pane key="print" tab="标签打印">
        <div class="print-panel">
          <a-form layout="inline" class="print-search-form">
            <a-form-item label="日期范围">
              <a-range-picker v-model:value="printDateRange" value-format="YYYY-MM-DD" />
            </a-form-item>
            <a-form-item label="关键字">
              <a-input v-model:value="printKeyword" placeholder="炉号、产线等" allow-clear @press-enter="handlePrintSearch" />
            </a-form-item>
            <a-form-item>
              <a-button type="primary" :loading="printTableLoading" @click="handlePrintSearch">查询</a-button>
            </a-form-item>
          </a-form>

          <a-table
            :columns="printColumns"
            :data-source="printTableData"
            :row-selection="printRowSelection"
            :loading="printTableLoading"
            :pagination="printPagination"
            row-key="id"
            size="small"
            bordered
            class="print-table"
            @change="handlePrintTableChange"
          />

          <div class="print-toolbar">
            <span class="print-selected-count">已选 {{ selectedRowKeys.length }} 条</span>
            <a-button type="primary" :disabled="selectedRowKeys.length === 0" @click="handleGeneratePreview">生成标签预览</a-button>
            <a-button :disabled="labelList.length === 0" @click="handlePrint">打印</a-button>
          </div>

          <div v-if="labelList.length > 0" class="label-print-area">
            <div v-for="label in labelList" :key="label.id" class="label-item">
              <QrCode :value="label.qrValue" :width="90" :options="{ margin: 1 }" tag="img" />
              <div class="label-info">
                <div class="label-furnace-no">{{ label.furnaceNo }}</div>
                <div class="label-spec">{{ label.productSpecName }}</div>
                <div class="label-date">{{ label.prodDate }}</div>
              </div>
            </div>
          </div>
        </div>
      </a-tab-pane>
    </a-tabs>
  </div>
</template>

<script lang="ts" setup>
import { ref, computed, onMounted, onUnmounted, nextTick } from 'vue';
import { getTraceByCode } from '/@/api/lab/trace';
import { getRawDataList } from '/@/api/lab/rawData';
import { QrCode } from '/@/components/Qrcode/index';
import { useMessage } from '/@/hooks/web/useMessage';
import { formatToDate, formatToDateTime } from '/@/utils/dateUtil';

defineOptions({ name: 'scanStation' });

const { createMessage } = useMessage();

// 取值助手：后端返回字段可能是 camelCase 也可能是 PascalCase，双兼容取值
function pick(obj: any, ...keys: string[]): any {
  if (!obj) return undefined;
  for (const key of keys) {
    const val = obj[key];
    if (val !== undefined && val !== null) return val;
  }
  return undefined;
}

// 空值统一显示“暂无”
function display(value: any): string {
  if (value === undefined || value === null || value === '') return '暂无';
  return String(value);
}

function formatDateSafe(value: any): string {
  if (!value) return '暂无';
  return formatToDate(value);
}

function formatDateTimeSafe(value: any): string {
  if (!value) return '暂无';
  return formatToDateTime(value);
}

// 判定类标签颜色：A=绿色，含“不”=红色，其它=默认
function judgmentColor(value: any): string {
  const text = value === undefined || value === null ? '' : String(value);
  if (!text) return 'default';
  if (text.includes('不')) return 'red';
  if (text === 'A') return 'green';
  return 'default';
}

function scratchedLabel(value: any): string {
  if (value === 1 || value === '1') return '刻痕后';
  if (value === 0 || value === '0') return '刻痕前';
  return '暂无';
}

function scratchedColor(value: any): string {
  if (value === 1 || value === '1') return 'warning';
  if (value === 0 || value === '0') return 'default';
  return 'default';
}

const activeTab = ref('trace');

// ========== 页签1：扫码追溯 ==========
const scanInputRef = ref<any>(null);
const scanCode = ref('');
const traceLoading = ref(false);
const traceResult = ref<any>(null);

interface ScanHistoryItem {
  time: string;
  code: string;
  matched: boolean;
}
const scanHistory = ref<ScanHistoryItem[]>([]);

function focusScanInput() {
  nextTick(() => {
    scanInputRef.value?.focus?.();
  });
}

async function handleScanEnter() {
  const code = scanCode.value.trim();
  scanCode.value = '';
  if (!code) return;
  await runTrace(code);
}

async function runTrace(code: string) {
  traceLoading.value = true;
  try {
    const res: any = await getTraceByCode(code);
    const data = res?.data || res;
    traceResult.value = data;
    scanHistory.value.unshift({
      time: formatToDateTime(new Date()),
      code,
      matched: !!data?.matched,
    });
    if (scanHistory.value.length > 20) {
      scanHistory.value = scanHistory.value.slice(0, 20);
    }
    if (!data?.matched) {
      createMessage.warning('未查询到该炉号的追溯数据');
    }
  } catch (error) {
    createMessage.error('查询失败，请重试');
  } finally {
    traceLoading.value = false;
    focusScanInput();
  }
}

function handleHistoryClick(item: ScanHistoryItem) {
  runTrace(item.code);
}

function isEditableTarget(target: EventTarget | null): boolean {
  if (!(target instanceof HTMLElement)) return false;
  const tag = target.tagName;
  return tag === 'INPUT' || tag === 'TEXTAREA' || tag === 'SELECT' || target.isContentEditable;
}

// 扫码枪本质是击键输入：若焦点被操作员误移到别处，捕获到可见字符键时把焦点拉回扫码输入框
function handleGlobalKeydown(event: KeyboardEvent) {
  if (activeTab.value !== 'trace') return;
  if (isEditableTarget(event.target)) return;
  if (event.key.length === 1 && !event.ctrlKey && !event.altKey && !event.metaKey) {
    focusScanInput();
  }
}

onMounted(() => {
  focusScanInput();
  document.addEventListener('keydown', handleGlobalKeydown);
});

onUnmounted(() => {
  document.removeEventListener('keydown', handleGlobalKeydown);
});

const basicInfo = computed(() => {
  const data = traceResult.value;
  const rawData = data?.rawData;
  const intermediate = data?.intermediate;
  return {
    furnaceNo: display(data?.normalizedFurnaceNo),
    lineNo: display(pick(rawData, 'lineNo', 'LineNo') ?? pick(intermediate, 'lineNo', 'LineNo')),
    shift: display(pick(rawData, 'shift', 'Shift') ?? pick(intermediate, 'shift', 'Shift')),
    prodDate: formatDateSafe(pick(rawData, 'prodDate', 'ProdDate') ?? pick(intermediate, 'prodDate', 'ProdDate')),
  };
});

const sheetInfo = computed(() => {
  const data = traceResult.value;
  const rawData = data?.rawData;
  const intermediate = data?.intermediate;
  return {
    detectionDate: formatDateSafe(pick(rawData, 'detectionDate', 'DetectionDate') ?? pick(intermediate, 'detectionDate', 'DetectionDate')),
    productSpecName: display(pick(rawData, 'productSpecName', 'ProductSpecName') ?? pick(intermediate, 'productSpecName', 'ProductSpecName')),
    width: display(pick(rawData, 'width', 'Width') ?? pick(intermediate, 'width', 'Width')),
    coilWeight: display(pick(rawData, 'coilWeight', 'CoilWeight') ?? pick(intermediate, 'coilWeight', 'CoilWeight')),
    singleCoilWeight: display(pick(rawData, 'singleCoilWeight', 'SingleCoilWeight') ?? pick(intermediate, 'singleCoilWeight', 'SingleCoilWeight')),
  };
});

const judgmentInfo = computed(() => {
  const intermediate = traceResult.value?.intermediate;
  return {
    labeling: pick(intermediate, 'labeling', 'Labeling'),
    firstInspection: pick(intermediate, 'firstInspection', 'FirstInspection'),
    magneticResult: pick(intermediate, 'magneticResult', 'MagneticResult'),
    thicknessResult: pick(intermediate, 'thicknessResult', 'ThicknessResult'),
    laminationResult: pick(intermediate, 'laminationResult', 'LaminationResult'),
    calcStatus: pick(intermediate, 'calcStatus', 'CalcStatus'),
    judgeStatus: pick(intermediate, 'judgeStatus', 'JudgeStatus'),
  };
});

const perfColumns = [
  { title: '刻痕', dataIndex: 'isScratched', key: 'isScratched', align: 'center', width: 90 },
  { title: 'Ps铁损', dataIndex: 'psLoss', key: 'psLoss', align: 'right', width: 100 },
  { title: 'Ss激磁功率', dataIndex: 'ssPower', key: 'ssPower', align: 'right', width: 110 },
  { title: 'Hc', dataIndex: 'hc', key: 'hc', align: 'right', width: 90 },
  { title: '检测时间', dataIndex: 'detectionTime', key: 'detectionTime', align: 'center', width: 160 },
];

function normalizeRecord(row: any) {
  return {
    originalFurnaceNo: pick(row, 'originalFurnaceNo', 'OriginalFurnaceNo'),
    furnaceNo: pick(row, 'furnaceNo', 'FurnaceNo'),
    isScratched: pick(row, 'isScratched', 'IsScratched'),
    psLoss: pick(row, 'psLoss', 'PsLoss') ?? '暂无',
    ssPower: pick(row, 'ssPower', 'SsPower') ?? '暂无',
    hc: pick(row, 'hc', 'Hc') ?? '暂无',
    detectionTime: formatDateTimeSafe(pick(row, 'detectionTime', 'DetectionTime')),
  };
}

const normalizedMagneticRecords = computed(() => (traceResult.value?.magneticRecords || []).map(normalizeRecord));
const normalizedSingleSheetRecords = computed(() => (traceResult.value?.singleSheetRecords || []).map(normalizeRecord));

// ========== 页签2：标签打印 ==========
const printDateRange = ref<string[]>([]);
const printKeyword = ref('');
const printTableLoading = ref(false);
const printTableData = ref<any[]>([]);
const printTotal = ref(0);
const printCurrentPage = ref(1);
const printPageSize = ref(50);
const printLoaded = ref(false);
const selectedRowKeys = ref<string[]>([]);
const selectedRows = ref<any[]>([]);

const printColumns = [
  { title: '炉号', dataIndex: 'furnaceNo', key: 'furnaceNo', width: 200 },
  { title: '规格', dataIndex: 'productSpecName', key: 'productSpecName', width: 140 },
  { title: '生产日期', dataIndex: 'prodDateStr', key: 'prodDateStr', width: 120 },
  { title: '检测日期', dataIndex: 'detectionDateStr', key: 'detectionDateStr', width: 120 },
];

const printPagination = computed(() => ({
  current: printCurrentPage.value,
  pageSize: printPageSize.value,
  total: printTotal.value,
  showSizeChanger: false,
  showTotal: (total: number) => `共 ${total} 条`,
}));

const printRowSelection = computed(() => ({
  type: 'checkbox' as const,
  selectedRowKeys: selectedRowKeys.value,
  onChange: (keys: string[], rows: any[]) => {
    selectedRowKeys.value = keys;
    selectedRows.value = rows;
  },
}));

async function fetchPrintTable() {
  printTableLoading.value = true;
  try {
    const params: any = {
      currentPage: printCurrentPage.value,
      pageSize: printPageSize.value,
      isValidData: 1,
    };
    if (printKeyword.value) params.keyword = printKeyword.value;
    if (printDateRange.value && printDateRange.value.length === 2) {
      params.startDate = printDateRange.value[0];
      params.endDate = printDateRange.value[1];
    }
    const res: any = await getRawDataList(params);
    const payload = res?.data || res;
    printTableData.value = payload?.list || [];
    printTotal.value = payload?.pagination?.total || 0;
  } catch (error) {
    createMessage.error('查询失败，请重试');
  } finally {
    printTableLoading.value = false;
  }
}

function handlePrintSearch() {
  printCurrentPage.value = 1;
  fetchPrintTable();
}

function handlePrintTableChange(pagination: any) {
  printCurrentPage.value = pagination.current;
  printPageSize.value = pagination.pageSize;
  fetchPrintTable();
}

function handleTabChange(key: string) {
  if (key === 'print' && !printLoaded.value) {
    printLoaded.value = true;
    fetchPrintTable();
  } else if (key === 'trace') {
    focusScanInput();
  }
}

const labelList = ref<any[]>([]);

function handleGeneratePreview() {
  if (selectedRows.value.length === 0) {
    createMessage.warning('请先勾选需要打印的炉号');
    return;
  }
  labelList.value = selectedRows.value.map((row) => {
    const furnaceNo = row.furnaceNo || '';
    const furnaceNoFormatted = row.furnaceNoFormatted || '';
    return {
      id: row.id,
      qrValue: furnaceNoFormatted || furnaceNo,
      furnaceNo: furnaceNoFormatted || furnaceNo,
      productSpecName: display(row.productSpecName),
      prodDate: display(row.prodDateStr || row.prodDate),
    };
  });
}

function handlePrint() {
  if (labelList.value.length === 0) {
    createMessage.warning('请先生成标签预览');
    return;
  }
  document.body.classList.add('label-printing');
  const cleanup = () => {
    document.body.classList.remove('label-printing');
    window.removeEventListener('afterprint', cleanup);
  };
  window.addEventListener('afterprint', cleanup);
  window.print();
}
</script>

<style scoped>
.scan-station-page {
  padding: 16px;
  background: #fff;
}

.trace-panel {
  display: flex;
  gap: 16px;
  align-items: flex-start;
}

.trace-panel-main {
  flex: 1;
  min-width: 0;
}

.trace-panel-history {
  width: 280px;
  flex-shrink: 0;
  border-left: 1px solid #f0f0f0;
  padding-left: 16px;
}

.scan-input-wrapper {
  margin-bottom: 16px;
}

.scan-input :deep(input) {
  font-size: 22px;
  height: 52px;
}

.trace-empty-hint {
  padding: 60px 0;
}

.trace-result {
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.trace-card {
  width: 100%;
}

.trace-history-title {
  font-weight: 500;
  margin-bottom: 12px;
}

.trace-history-list {
  max-height: 640px;
  overflow-y: auto;
}

.trace-history-empty {
  color: #999;
  text-align: center;
  padding: 24px 0;
}

.trace-history-item {
  padding: 8px;
  border-radius: 4px;
  cursor: pointer;
  margin-bottom: 4px;
}

.trace-history-item:hover {
  background: #f5f5f5;
}

.trace-history-item-code {
  font-weight: 500;
  word-break: break-all;
}

.trace-history-item-meta {
  display: flex;
  justify-content: space-between;
  align-items: center;
  color: #999;
  font-size: 12px;
  margin-top: 4px;
}

.print-panel {
  display: flex;
  flex-direction: column;
  gap: 16px;
}

.print-toolbar {
  display: flex;
  align-items: center;
  gap: 12px;
}

.print-selected-count {
  color: #666;
}

.label-print-area {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(230px, 1fr));
  gap: 12px;
  padding: 16px;
  border: 1px solid #f0f0f0;
}

.label-item {
  width: 60mm;
  height: 40mm;
  border: 1px dashed #d9d9d9;
  padding: 4mm;
  box-sizing: border-box;
  display: flex;
  align-items: center;
  gap: 3mm;
  break-inside: avoid;
}

.label-info {
  flex: 1;
  min-width: 0;
  overflow-wrap: break-word;
}

.label-furnace-no {
  font-weight: 700;
  font-size: 13px;
  line-height: 1.3;
  word-break: break-all;
}

.label-spec,
.label-date {
  font-size: 11px;
  color: #333;
  line-height: 1.3;
}
</style>

<!-- 打印样式需要覆盖整个页面（脱离本组件的布局层级），使用非 scoped 全局样式 -->
<style>
@media print {
  body.label-printing * {
    visibility: hidden;
  }

  body.label-printing .label-print-area,
  body.label-printing .label-print-area * {
    visibility: visible;
  }

  body.label-printing .label-print-area {
    position: fixed;
    inset: 0;
    z-index: 9999;
    background: #fff;
    margin: 0;
    padding: 0;
    border: none;
    overflow: visible;
    display: grid;
    grid-template-columns: repeat(auto-fill, 60mm);
    gap: 2mm;
  }

  body.label-printing .label-item {
    border: none;
    break-inside: avoid;
  }
}
</style>
