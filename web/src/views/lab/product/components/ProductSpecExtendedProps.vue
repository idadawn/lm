<template>
  <div class="extended-props-container">
    <a-divider orientation="left">扩展属性</a-divider>

    <div class="props-list">
      <div
        v-for="(prop, index) in localProperties"
        :key="index"
        class="prop-item"
      >
        <div class="prop-inputs">
          <a-input
            v-model:value="prop.name"
            placeholder="属性名称"
            class="prop-name"
            @change="updateProperties"
          />
          <a-select
            v-model:value="prop.type"
            placeholder="类型"
            class="prop-type"
            @change="updateProperties"
          >
            <a-select-option value="string">文本</a-select-option>
            <a-select-option value="number">数字</a-select-option>
            <a-select-option value="boolean">布尔值</a-select-option>
          </a-select>
          <a-input
            v-if="prop.type === 'string'"
            v-model:value="prop.value"
            placeholder="属性值"
            class="prop-value"
            @change="updateProperties"
          />
          <a-input-number
            v-if="prop.type === 'number'"
            v-model:value="prop.value"
            placeholder="属性值"
            class="prop-value"
            @change="updateProperties"
          />
          <a-switch
            v-if="prop.type === 'boolean'"
            v-model:checked="prop.value"
            class="prop-value"
            @change="updateProperties"
          />
          <a-button
            type="link"
            danger
            @click="removeProperty(index)"
            class="remove-btn"
          >
            <DeleteOutlined />
          </a-button>
        </div>
      </div>

      <a-button
        type="dashed"
        block
        @click="addProperty"
        class="add-prop-btn"
      >
        <PlusOutlined />
        添加扩展属性
      </a-button>
    </div>
  </div>
</template>

<script lang="ts" setup>
import { ref, watch, nextTick } from 'vue';
import { DeleteOutlined, PlusOutlined } from '@ant-design/icons-vue';

interface ExtendedProperty {
  name: string;
  type: 'string' | 'number' | 'boolean';
  value: any;
}

interface Props {
  modelValue?: Record<string, any>;
}

interface Emits {
  (e: 'update:modelValue', value: Record<string, any>): void;
}

const props = withDefaults(defineProps<Props>(), {
  modelValue: () => ({}),
});

const emit = defineEmits<Emits>();

const localProperties = ref<ExtendedProperty[]>([]);

// 初始化本地属性列表
const initProperties = () => {
  if (props.modelValue && Object.keys(props.modelValue).length > 0) {
    localProperties.value = Object.entries(props.modelValue).map(([name, value]) => {
      let type: ExtendedProperty['type'] = 'string';
      let propValue: any = value;

      if (typeof value === 'number') {
        type = 'number';
      } else if (typeof value === 'boolean') {
        type = 'boolean';
      }

      return { name, type, value: propValue };
    });
  } else {
    localProperties.value = [];
  }
};

// 添加属性
const addProperty = () => {
  localProperties.value.push({
    name: '',
    type: 'string',
    value: '',
  });

  nextTick(() => {
    updateProperties();
  });
};

// 删除属性
const removeProperty = (index: number) => {
  localProperties.value.splice(index, 1);
  updateProperties();
};

// 更新属性值
const updateProperties = () => {
  const validProperties: Record<string, any> = {};

  localProperties.value.forEach(prop => {
    if (prop.name.trim()) {
      validProperties[prop.name.trim()] = prop.value;
    }
  });

  emit('update:modelValue', validProperties);
};

// 监听外部值变化
watch(
  () => props.modelValue,
  () => {
    initProperties();
  },
  { deep: true, immediate: true }
);

// 监听本地属性变化
watch(
  localProperties,
  () => {
    updateProperties();
  },
  { deep: true }
);
</script>

<style lang="less" scoped>
.extended-props-container {
  margin-top: 16px;
}

.props-list {
  margin-top: 12px;
}

.prop-item {
  margin-bottom: 12px;

  &:last-child {
    margin-bottom: 0;
  }
}

.prop-inputs {
  display: flex;
  gap: 8px;
  align-items: center;
}

.prop-name {
  flex: 0 0 120px;
}

.prop-type {
  flex: 0 0 100px;
}

.prop-value {
  flex: 1;
}

.remove-btn {
  flex: 0 0 32px;
  padding: 0;
}

.add-prop-btn {
  margin-top: 8px;
}

@media (max-width: 768px) {
  .prop-inputs {
    flex-wrap: wrap;
  }

  .prop-name,
  .prop-type,
  .prop-value {
    flex: 1 1 100%;
  }

  .remove-btn {
    flex: 0 0 auto;
    margin-left: auto;
  }
}
</style>