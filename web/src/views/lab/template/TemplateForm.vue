<template>
  <BasicModal v-bind="$attrs" @register="registerModal" :title="getTitle" @ok="handleSubmit" @cancel="handleCancel"
    :width="1000">
    <div class="p-4" v-loading="loading">
      <!-- 基本信息 -->
      <div class="mb-6">
        <h3 class="text-lg font-bold mb-4 border-b pb-2">基本信息</h3>
        <a-form layout="vertical" :model="formData">
          <a-row :gutter="16">
            <a-col :span="12">
              <a-form-item label="模板名称" required>
                <a-input v-model:value="formData.templateName" placeholder="如：标准产品导入模板" />
              </a-form-item>
            </a-col>
            <a-col :span="12">
              <a-form-item label="模板编码">
                <a-input v-model:value="formData.templateCode" placeholder="系统自动生成" disabled />
              </a-form-item>
            </a-col>
          </a-row>
        </a-form>
      </div>

      <!-- 字段映射配置 -->
      <div class="mb-6">
        <div class="flex justify-between items-center mb-4 border-b pb-2">
          <h3 class="text-lg font-bold">字段映射配置</h3>
          <div class="flex gap-2">
            <a-upload name="file" :showUploadList="false" :customRequest="handleUpload" accept=".xlsx,.xls">
              <a-button type="default" size="small" :loading="uploadLoading">
                从Excel上传
              </a-button>
            </a-upload>
            <!-- 移除加载默认字段按钮 -->
          </div>
        </div>

        <a-table :dataSource="fieldMappings" :columns="columns" :pagination="false" size="small" rowKey="key">
          <template #bodyCell="{ column, record, index }">
            <!-- 字段名 (只读，显示中文) -->
            <template v-if="column.key === 'field'">
              <span>{{ record.label || record.field }}</span>
            </template>

            <!-- Excel列名 -->
            <template v-if="column.key === 'excelColumnNames'">
              <a-select v-model:value="record.excelColumnIndex" style="width: 100%" placeholder="选择列"
                :options="getOptionsForRecord(record)" allowClear
                @change="(val, opt) => handleColumnChange(val, opt, record)" />
            </template>

            <!-- 数据类型 -->
            <template v-if="column.key === 'dataType'">
              <a-select v-model:value="record.dataType" style="width: 100%">
                <a-select-option value="string">文本</a-select-option>
                <a-select-option value="decimal">数值</a-select-option>
                <a-select-option value="int">整数</a-select-option>
                <a-select-option value="datetime">日期时间</a-select-option>
              </a-select>
            </template>

            <!-- 单位选择 -->
            <template v-if="column.key === 'unitId'">
              <a-select v-model:value="record.unitId" style="width: 100%" allowClear placeholder="选择单位">
                <a-select-option v-for="unit in unitList" :key="unit.id" :value="unit.id">
                  {{ unit.name }} ({{ unit.symbol }})
                </a-select-option>
              </a-select>
            </template>

            <!-- 小数点保留位数 -->
            <template v-if="column.key === 'decimalPlaces'">
              <a-input-number v-if="record.dataType === 'decimal' || record.dataType === 'int'"
                v-model:value="record.decimalPlaces" :min="0" :max="10" :precision="0" style="width: 100%"
                placeholder="0" />
              <span v-else class="text-gray-400">-</span>
            </template>

            <!-- 是否必填 -->
            <template v-if="column.key === 'required'">
              <a-checkbox v-model:checked="record.required"></a-checkbox>
            </template>

            <!-- 移除操作列 -->
          </template>
        </a-table>
      </div>
    </div>
  </BasicModal>
</template>

<script lang="ts" setup>
import { ref, unref, computed, reactive, onMounted } from 'vue';

console.log('TemplateForm setup script start');

import { BasicModal, useModalInner } from '/@/components/Modal';
import { useMessage } from '/@/hooks/web/useMessage';
import { updateExcelTemplate, getExcelTemplateById, validateTemplateConfig, parseExcelHeaders, getSystemFields } from '/@/api/lab/excelTemplate';
import { getUnitDefinitionList } from '/@/api/lab/unit';

const emit = defineEmits(['register', 'reload']);

const [registerModal, { closeModal, changeLoading, changeOkLoading }] = useModalInner(init);
const { createMessage } = useMessage(); // removed createConfirm

const loading = ref(false);
const uploadLoading = ref(false);
const systemFieldLoading = ref(false);
const id = ref('');
const unitList = ref<any[]>([]);
const excelHeaderOptions = ref<any[]>([]); // Excel表头选项

// ... (省略部分代码)

// 移除 handleLoadSystemFields

async function doLoadSystemFields() {
  console.log('doLoadSystemFields start', formData.templateCode);
  systemFieldLoading.value = true;
  try {
    const res: any = await getSystemFields(formData.templateCode);
    const fields = res.data?.fields || res.fields || [];
    const headers = res.data?.excelHeaders || res.excelHeaders || [];

    // 恢复ExcelDropdownOptions
    if (headers && headers.length > 0) {
      excelHeaderOptions.value = headers.map((h: any) => ({
        label: `${h.index} - ${h.name}`,
        value: h.index,
        name: h.name
      }));
    }

    fieldMappings.value = fields.map((f: any) => ({
      key: Date.now() + Math.random(),
      field: f.field,
      label: f.label, // 添加label字段，保存时需要
      excelColumnName: f.excelColumnNames && f.excelColumnNames.length > 0 ? f.excelColumnNames[0] : f.label,
      excelColumnIndex: f.excelColumnIndex, // 后端可能返回 null
      dataType: f.dataType,
      decimalPlaces: f.decimalPlaces ?? 2, // 默认保留2位小数
      unitId: f.unitId,
      required: f.required === true,
    }));

    createMessage.success(`已加载 ${fields.length} 个系统字段`);
  } catch (error) {
    console.error('加载系统字段失败', error);
    createMessage.error('加载系统字段失败');
  } finally {
    systemFieldLoading.value = false;
  }
}

// 处理上传
const handleUpload = async ({ file }: any) => {
  uploadLoading.value = true;
  try {
    // 转换文件为Base64
    const reader = new FileReader();
    reader.readAsDataURL(file);
    reader.onload = async () => {
      try {
        const base64Str = (reader.result as string).split(',')[1];
        const res: any = await parseExcelHeaders({
          fileName: file.name,
          fileData: base64Str
        });

        const headers: any[] = res.data || res || [];

        // 更新选项: Value=Index(A,B...), Label="A - Name"
        excelHeaderOptions.value = headers.map((h: any) => {
          return {
            label: `${h.index} - ${h.name}`,
            value: h.index,
            name: h.name
          };
        });

        createMessage.success(`成功解析 ${headers.length} 个表头列`);

        // 自动匹配逻辑
        let matchCount = 0;
        fieldMappings.value.forEach(mapping => {
          // 如果还没有设置Excel列
          if (!mapping.excelColumnIndex) {
            // 1. 尝试完全匹配字段名 (Case Insensitive)
            const exactMatch = headers.find(h => h.name.toLowerCase() === mapping.field.toLowerCase());
            if (exactMatch) {
              mapping.excelColumnIndex = exactMatch.index;
              mapping.excelColumnName = exactMatch.name;
              matchCount++;
              return;
            }

            // 2. 尝试匹配 Label (如果存在) - 优先匹配 Name
            if (mapping.label) {
              const labelMatch = headers.find(h => h.name.includes(mapping.label) || mapping.label.includes(h.name));
              if (labelMatch) {
                mapping.excelColumnIndex = labelMatch.index;
                mapping.excelColumnName = labelMatch.name;
                matchCount++;
                return;
              }
            }

            // 3. 兼容逻辑：如果没有匹配到，但系统字段预设了 Name (excelColumnName)，尝试倒推 Index
            if (mapping.excelColumnName) {
              const nameMatch = headers.find(h => h.name === mapping.excelColumnName);
              if (nameMatch) {
                mapping.excelColumnIndex = nameMatch.index;
                // Name is already set
                matchCount++;
              }
            }
          }
        });

        if (matchCount > 0) {
          createMessage.success(`自动匹配了 ${matchCount} 个字段`);
        }

      } catch (err: any) {
        console.error('API调用失败', err);
        createMessage.error('解析Excel表头失败');
      } finally {
        uploadLoading.value = false;
      }
    };
    reader.onerror = () => {
      createMessage.error('文件读取失败');
      uploadLoading.value = false;
    };
  } catch (error: any) {
    console.error('处理上传失败', error);
    uploadLoading.value = false;
  }
};

// 表单数据
const formData = reactive({
  templateName: '',
  templateCode: '',
});

// 字段映射数据
const fieldMappings = ref<any[]>([]);

// 表格列定义
const columns = [
  { title: '字段名称', key: 'field', width: '18%' },
  { title: 'Excel列名 (选择)', key: 'excelColumnNames', width: '25%' },
  { title: '数据类型', key: 'dataType', width: '12%' },
  { title: '小数位数', key: 'decimalPlaces', width: '10%', align: 'center' },
  { title: '单位', key: 'unitId', width: '20%' },
  { title: '必填', key: 'required', width: '8%', align: 'center' },
  // 移除操作列
];

const getTitle = computed(() => '编辑导入模板');

// 辅助函数：根据保存的配置恢复字段映射
function restoreFieldMappingsFromConfig(config: any) {
  if (!config.fieldMappings || !Array.isArray(config.fieldMappings)) {
    return [];
  }

  // 创建标题列索引到名称的映射
  const headerMap = new Map();
  if (config.excelHeaders && Array.isArray(config.excelHeaders)) {
    config.excelHeaders.forEach((h: any) => {
      headerMap.set(h.index, h.name);
    });
  }

  return config.fieldMappings.map((item: any, index: number) => {
    // 获取Excel列索引和名称
    let idx = item.excelColumnIndex || '';
    let name = '';

    // 1. 如果保存了索引，从headerMap中获取名称
    if (idx && headerMap.has(idx)) {
      name = headerMap.get(idx);
    }

    // 2. 如果没找到名称，尝试从excelColumnNames获取
    if (!name && item.excelColumnNames && Array.isArray(item.excelColumnNames) && item.excelColumnNames.length > 0) {
      name = item.excelColumnNames[0];
    } else if (!name && item.excelColumnName) {
      name = item.excelColumnName;
    }

    // 3. 如果保存了名称但没索引，尝试从headerMap中查找
    if (!idx && name) {
      for (const [index, headerName] of headerMap.entries()) {
        if (headerName === name) {
          idx = index;
          break;
        }
      }
    }

    return {
      key: Date.now() + index,
      field: item.field,
      label: item.label || item.field,
      excelColumnIndex: idx,
      excelColumnName: name,
      dataType: item.dataType || 'string',
      decimalPlaces: item.decimalPlaces ?? 2, // 默认保留2位小数
      required: item.required !== undefined ? item.required : false,
      unitId: item.unitId || undefined,
      defaultValue: item.defaultValue || null
    };
  });
}

// 初始化
async function init(data: any) {
  console.log('TemplateForm init', data);
  loading.value = true;
  requestAnimationFrame(async () => {
    id.value = data.id || '';

    // 重置数据
    formData.templateName = '';
    formData.templateCode = '';
    fieldMappings.value = [];
    excelHeaderOptions.value = [];

    // 加载单位列表
    await loadUnitList();

    if (id.value) {
      try {
        // 1. 获取模板详情
        const response = await getExcelTemplateById(id.value);
        const templateData = response.data || response;

        formData.templateName = templateData.templateName;
        formData.templateCode = templateData.templateCode;

        // 2. 解析并恢复保存的配置
        if (templateData.configJson) {
          try {
            let config = JSON.parse(templateData.configJson);
            // 防止双重序列化导致的问题 (String containing JSON)
            if (typeof config === 'string') {
              try {
                config = JSON.parse(config);
              } catch (innerE) {
                console.warn('Config is string but failed inner parse', innerE);
              }
            }

            // 2.1 恢复Excel表头选项 (Dropdown Options)
            if (config.excelHeaders && Array.isArray(config.excelHeaders) && config.excelHeaders.length > 0) {
              excelHeaderOptions.value = config.excelHeaders.map((h: any) => ({
                label: `${h.index} - ${h.name}`,
                value: h.index,
                name: h.name
              }));
            } else if (config.fieldMappings) {
              // 兼容旧数据：从已保存的映射中提取表头信息
              const savedHeaders = new Map<string, string>(); // Index -> Name
              config.fieldMappings.forEach((m: any) => {
                const idx = m.excelColumnIndex;
                const name = (m.excelColumnNames && m.excelColumnNames.length > 0) ? m.excelColumnNames[0] : m.excelColumnName;
                if (idx && name) {
                  savedHeaders.set(idx, name);
                }
              });

              if (savedHeaders.size > 0) {
                excelHeaderOptions.value = Array.from(savedHeaders.entries()).map(([idx, name]) => ({
                  label: `${idx} - ${name}`,
                  value: idx,
                  name: name
                }));
              }
            }

            // 2.2 使用保存的字段映射配置
            if (config.fieldMappings && config.fieldMappings.length > 0) {
              // 使用辅助函数恢复字段映射
              fieldMappings.value = restoreFieldMappingsFromConfig(config);

            } else {
              // 配置为空但有JSON，回退到系统字段加载
              await loadFieldsFromSystemWithCode(formData.templateCode || 'raw-data-import');
            }
          } catch (e) {
            console.error('解析ConfigJson失败', e);
            createMessage.error('解析配置失败');
            // 2. 获取系统字段 (用于获取最新Label和字段定义)
            // 确保templateCode存在，否则默认raw-data-import
            const code = formData.templateCode || 'raw-data-import';
            let systemFields: any[] = [];
            try {
              // 这里的res已经是SystemFieldResult
              const sysRes: any = await getSystemFields(code);
              // 提取字段列表
              systemFields = sysRes.data?.fields || sysRes.fields || [];
              console.log('Loader System Fields:', systemFields);
            } catch (err) {
              console.error('Failed to load system fields', err);
            }
            const systemFieldMap = new Map(systemFields.map(f => [f.field, f]));
          }
        } else {
          // 无配置，加载系统字段
          await loadFieldsFromSystemWithCode(formData.templateCode || 'raw-data-import');
        }
      } catch (error) {
        console.error('加载模板数据失败:', error);
        createMessage.error('加载模板数据失败');
      }
    }

    loading.value = false;
  });
}

// 辅助：直接从系统字段加载
function loadFieldsFromSystem(fields: any[]) {
  if (!fields || fields.length === 0) return;
  fieldMappings.value = fields.map((f: any, index: number) => ({
    key: Date.now() + index,
    field: f.field,
    label: f.label,
    excelColumnName: f.excelColumnNames && f.excelColumnNames.length > 0 ? f.excelColumnNames[0] : f.label,
    excelColumnIndex: f.excelColumnIndex,
    dataType: f.dataType,
    decimalPlaces: f.decimalPlaces ?? 2, // 默认保留2位小数
    unitId: f.unitId,
    required: f.required === true,
  }));
  // 此时没有预设的Excel Index/Name绑定，excelHeaderOptions为空，等待上传
}

// 根据模板编码加载系统字段
async function loadFieldsFromSystemWithCode(templateCode: string) {
  // 使用现有的doLoadSystemFields函数，但需要先设置templateCode
  const originalCode = formData.templateCode;
  formData.templateCode = templateCode;
  try {
    await doLoadSystemFields();
  } finally {
    formData.templateCode = originalCode;
  }
}



// 选中列名改变时
function handleColumnChange(val: string, option: any, record: any) {
  if (option) {
    record.excelColumnName = option.name;
    // record.excelColumnIndex is bound via v-model
  } else {
    record.excelColumnName = undefined;
  }
}

// 获取当前行可用的选项 (排除已被其他行选中的)
function getOptionsForRecord(record: any) {
  // 获取所有已被选中的值 (排除当前行)
  const selectedValues = fieldMappings.value
    .filter(item => item !== record && item.excelColumnIndex)
    .map(item => item.excelColumnIndex);

  // 过滤选项: 保留(未被选中) 或 (当前行选中的)
  return excelHeaderOptions.value.filter(opt =>
    !selectedValues.includes(opt.value) || opt.value === record.excelColumnIndex
  );
}

// 加载单位列表
async function loadUnitList() {
  try {
    const res: any = await getUnitDefinitionList();
    unitList.value = res.data || res || [];
  } catch (error) {
    console.error('加载单位列表失败', error);
  }
}


// 提交表单
async function handleSubmit() {
  if (!formData.templateName) {
    createMessage.error('请输入模板名称');
    return;
  }

  changeOkLoading(true);
  try {
    // 构造ConfigJson
    const config = {
      version: "1.0",
      description: formData.templateName,
      fieldMappings: fieldMappings.value.map(item => ({
        field: item.field,
        label: item.label, // 包含中文名
        excelColumnNames: item.excelColumnName ? [item.excelColumnName] : [],
        excelColumnIndex: item.excelColumnIndex, // 保存索引
        dataType: item.dataType,
        decimalPlaces: item.decimalPlaces, // 小数点保留位数
        unitId: item.unitId,
        required: item.required,
        defaultValue: null
      })),
      detectionColumns: { // 默认检测列配置，后续可由界面配置
        minColumn: 1,
        maxColumn: 100,
        patterns: ["{col}", "检测{col}", "列{col}", "第{col}列", "检测列{col}"]
      },
      // 保存Excel表头信息，用于编辑时恢复下拉选项
      excelHeaders: excelHeaderOptions.value.map(opt => ({
        index: opt.value,
        name: opt.name
      }))
    };

    const configJson = JSON.stringify(config);

    // 验证配置
    await validateTemplateConfig(configJson);

    const submitValues = {
      templateName: formData.templateName,
      configJson: configJson,
      ownerUserId: null // or current user
    };

    await updateExcelTemplate(id.value, submitValues);
    createMessage.success('更新成功');
    closeModal();
    emit('reload');
  } catch (error: any) {
    const errorMsg = error?.response?.data?.msg || error?.message || '保存失败';
    createMessage.error(errorMsg);
  } finally {
    changeOkLoading(false);
  }
}

function handleCancel() {
  closeModal();
}
</script>

<style scoped>
/* Add any specific styles here */
</style>