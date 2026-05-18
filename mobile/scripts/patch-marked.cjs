#!/usr/bin/env node
// 给 marked / mp-html 自带的 marked.min.js 打补丁：替换 \p{L}\p{N} / \p{P}\p{S} Unicode
// property escapes（APP-PLUS 的 V8 太老不支持）→ ASCII + 中文范围。
//
// 用法：node scripts/patch-marked.cjs
// 建议放到 package.json 的 postinstall hook 里。

const fs = require('fs');
const path = require('path');

const ROOT = path.resolve(__dirname, '..');
const TARGETS = [
  'node_modules/marked/lib/marked.cjs',
  'node_modules/marked/lib/marked.esm.js',
  'node_modules/marked/lib/marked.umd.js',
  'node_modules/marked/src/Tokenizer.js',
  'node_modules/mp-html/plugins/markdown/marked.min.js',
];

function patchOne(rel) {
  const f = path.resolve(ROOT, rel);
  if (!fs.existsSync(f)) {
    console.log('  SKIP (missing):', rel);
    return;
  }
  const bak = f + '.bak';
  if (fs.existsSync(bak)) {
    // 重置回原始版本，再 patch（多次运行幂等）
    fs.copyFileSync(bak, f);
  } else {
    fs.copyFileSync(f, bak);
  }
  let src = fs.readFileSync(f, 'utf8');
  const before = src;

  // ── 1) \p{L}\p{N} → ASCII + CJK ─────────────────────
  // 这两个 escape 在源码里出现的形态有两种：
  //   1. /[\p{L}\p{N}]/u   — 直接作为 character class 内容
  //   2. [\p{L}\p{N}]      — 作为字符类内部
  src = src.split('\\p{L}\\p{N}').join('a-zA-Z0-9_\\u4e00-\\u9fa5');
  // 同时去掉这类 regex 末尾的 /u 标志（ASCII 字符类不需要）
  src = src.split('/[a-zA-Z0-9_\\u4e00-\\u9fa5]/u').join('/[a-zA-Z0-9_\\u4e00-\\u9fa5]/');

  // ── 2) \p{P}\p{S} → ASCII 标点符号范围 ──────────────
  // mp-html 自带的 marked.min.js 使用了 \p{P}\p{S}（标点和符号），
  // 老 V8 同样不支持。用 ASCII 标点范围 [!-/:-@[-`{-~] 替代。
  const punctSymbol = '!-/:-@[-`{-~';
  src = src.split('\\p{P}\\p{S}').join(punctSymbol);
  // 移除替换后 regex 的 /u 标志（按具体模式精确匹配，避免误伤）
  const uPatterns = [
    `/(?:[^\\s${punctSymbol}]|~)/u`,
    `/(?!~)[\\s${punctSymbol}]/u`,
    `/(?!~)[${punctSymbol}]/u`,
    `/[^\\s${punctSymbol}]/u`,
    `/[\\s${punctSymbol}]/u`,
    `/[${punctSymbol}]/u`,
  ];
  uPatterns.forEach(pattern => {
    const plain = pattern.replace('/u', '/');
    src = src.split(pattern).join(plain);
  });

  if (src !== before) {
    fs.writeFileSync(f, src);
    console.log('  PATCHED:', rel);
  } else {
    console.log('  NO MATCH:', rel);
  }
}

console.log('Patching marked Unicode property escapes...');
TARGETS.forEach(patchOne);

// Verify
console.log('---verify---');
TARGETS.forEach((rel) => {
  const f = path.resolve(ROOT, rel);
  if (!fs.existsSync(f)) return;
  const src = fs.readFileSync(f, 'utf8');
  const re = /\\p\{[LNPS]\}/g;   // L=Letter, N=Number, P=Punctuation, S=Symbol
  const matches = src.match(re) || [];
  console.log(`  ${rel}: remaining \\p{} = ${matches.length}`);
});
