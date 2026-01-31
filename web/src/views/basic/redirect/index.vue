<template>
  <div></div>
</template>
<script lang="ts" setup>
  import { unref } from 'vue';
  import { useRouter } from 'vue-router';

  const { currentRoute, replace } = useRouter();

  const { params, query } = unref(currentRoute);
  const { path, _redirect_type = 'path' } = params;

  // Clone params to avoid mutating the original route object (which is read-only)
  const _params = { ...params };
  Reflect.deleteProperty(_params, '_redirect_type');
  Reflect.deleteProperty(_params, 'path');

  const _path = Array.isArray(path) ? path.join('/') : path;

  if (_redirect_type === 'name') {
    replace({
      name: _path,
      query,
      params: JSON.parse((_params._origin_params as string) ?? '{}'),
    });
  } else {
    replace({
      path: _path.startsWith('/') ? _path : '/' + _path,
      query,
    });
  }
</script>
