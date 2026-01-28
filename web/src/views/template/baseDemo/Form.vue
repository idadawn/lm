<template>
  <BasicPopup
    class="popup-wrapper"
    v-bind="$attrs"
    @register="registerPopup"
    :title="getTitle"
    showOkBtn
    @ok="handleSubmit">
    <div>content</div>
  </BasicPopup>
</template>
<script lang="ts" setup>
  import { create, update } from '/@/api/permission/organize';
  import { unref, computed, ref } from 'vue';
  import { BasicPopup, usePopupInner } from '/@/components/Popup';

  const emit = defineEmits(['register', 'reload']);
  function init(data) {
    changeLoading(false);
    id.value = data.id
  }
  const [registerPopup, { closePopup, changeLoading, changeOkLoading }] = usePopupInner(init);
  const id = ref('');
  const getTitle = computed(() => (!unref(id) ? '新建问题树' : '编辑问题树'));
  async function handleSubmit() {
    changeOkLoading(true);
    const query = {
      id: 1,
    };
    const formMethod = id.value ? update : create;
    formMethod(query)
      .then(res => {
        changeOkLoading(false);
        closePopup();
        emit('reload');
      })
      .catch(() => {
        changeOkLoading(false);
      });
  }
</script>

<style lang="less" scoped>
  .popup-wrapper {
    height: 100%;
    position: relative;

    :deep(.scrollbar__view) {
      height: 100%;

      .popup-body-warapper {
        height: 100%;
      }
    }
  }

  .editor-wrapper {
    background: grey;
    height: 100%;
  }
</style>
