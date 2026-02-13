import { PluginOption } from 'vite';
import vue from '@vitejs/plugin-vue';
import vueJsx from '@vitejs/plugin-vue-jsx';
import legacy from '@vitejs/plugin-legacy';
import purgeIcons from 'vite-plugin-purge-icons';
import windiCSS from 'vite-plugin-windicss';
import VitePluginCertificate from 'vite-plugin-mkcert';
// import vueSetupExtend from 'vite-plugin-vue-setup-extend';
import { configHtmlPlugin } from './html';
import { configPwaConfig } from './pwa';
import { configCompressPlugin } from './compress';
import { configStyleImportPlugin } from './styleImport';
import { configVisualizerConfig } from './visualizer';
import { configThemePlugin } from './theme';
import { configImageminPlugin } from './imagemin';
import { configSvgIconsPlugin } from './svgSprite';
import { configCdnPlugin } from './cdn';

/**
 * 修复 ant-design-vue 3.x eagerComputed 使用 flush:'sync' 导致的无限递归更新。
 * 当大表格（80+ 列）的数据源更新时，sync flush 使多个 eagerComputed 实例之间
 * 形成同步循环依赖，触发 "Maximum recursive updates exceeded"。
 * 将 flush 改为 'pre' 让 Vue 在同一 tick 内批量处理，打破递归链。
 *
 * 需要同时处理两个场景：
 * 1. Vite transform（构建模式 & 未预打包的模块）
 * 2. esbuild plugin（开发模式下依赖预打包 optimizeDeps）
 */
function fixEagerComputedPlugin(): PluginOption {
  const FLUSH_SYNC_RE = /flush:\s*['"]sync['"]/g;
  return {
    name: 'fix-antdv-eager-computed',
    enforce: 'pre',
    // 在 esbuild 预打包阶段也替换，确保开发模式生效
    config() {
      return {
        optimizeDeps: {
          esbuildOptions: {
            plugins: [
              {
                name: 'fix-eager-computed-esbuild',
                setup(build: any) {
                  build.onLoad(
                    { filter: /eagerComputed\.(js|mjs|ts)$/ },
                    async (args: any) => {
                      if (!args.path.includes('ant-design-vue')) return undefined;
                      const fs = await import('fs');
                      const contents = fs.readFileSync(args.path, 'utf8');
                      if (!FLUSH_SYNC_RE.test(contents)) return undefined;
                      FLUSH_SYNC_RE.lastIndex = 0;
                      return {
                        contents: contents.replace(FLUSH_SYNC_RE, "flush: 'pre'"),
                        loader: 'js',
                      };
                    },
                  );
                },
              },
            ],
          },
        },
      };
    },
    // Vite transform（构建模式 & 非预打包模块）
    transform(code, id) {
      if (id.includes('ant-design-vue') && id.includes('eagerComputed')) {
        return code.replace(FLUSH_SYNC_RE, "flush: 'pre'");
      }
    },
  };
}

export function createVitePlugins(viteEnv: ViteEnv, isBuild: boolean) {
  const { VITE_USE_IMAGEMIN, VITE_LEGACY, VITE_BUILD_COMPRESS, VITE_BUILD_COMPRESS_DELETE_ORIGIN_FILE, VITE_CDN } = viteEnv;

  const vitePlugins: (PluginOption | PluginOption[])[] = [
    // 修复 ant-design-vue eagerComputed 无限递归（必须在 vue 插件之前）
    fixEagerComputedPlugin(),
    // have to
    vue({
      template: {
        compilerOptions: {
          // 解决vue3上marquee标签报错
          isCustomElement: tag => tag === 'marquee',
        },
      },
    }),
    // have to
    vueJsx(),
    // support name
    // vueSetupExtend(),
    VitePluginCertificate({
      source: 'coding',
    }),
  ];

  // vite-plugin-windicss
  vitePlugins.push(windiCSS());

  // @vitejs/plugin-legacy
  VITE_LEGACY && isBuild && vitePlugins.push(legacy());

  // vite-plugin-html
  vitePlugins.push(configHtmlPlugin(viteEnv, isBuild));

  // vite-plugin-svg-icons
  vitePlugins.push(configSvgIconsPlugin(isBuild));

  // vite-plugin-purge-icons
  vitePlugins.push(purgeIcons());

  // vite-plugin-style-import
  vitePlugins.push(configStyleImportPlugin(isBuild));

  // rollup-plugin-visualizer
  vitePlugins.push(configVisualizerConfig());

  // vite-plugin-theme
  vitePlugins.push(configThemePlugin(isBuild));

  // The following plugins only work in the production environment
  if (isBuild) {
    // vite-plugin-cdn-import
    if (VITE_CDN) vitePlugins.push(configCdnPlugin);

    // vite-plugin-imagemin
    VITE_USE_IMAGEMIN && vitePlugins.push(configImageminPlugin());

    // rollup-plugin-gzip
    vitePlugins.push(configCompressPlugin(VITE_BUILD_COMPRESS, VITE_BUILD_COMPRESS_DELETE_ORIGIN_FILE));

    // vite-plugin-pwa
    vitePlugins.push(configPwaConfig(viteEnv));
  }

  return vitePlugins;
}
