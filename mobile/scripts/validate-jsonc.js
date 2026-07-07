// 校验 uni-app 的 jsonc 配置文件（manifest.json / pages.json）。
// uni-app 允许 BOM、// 与 /* */ 注释、尾逗号——不能用裸 JSON.parse 校验。
// 用法: node scripts/validate-jsonc.js <file...>
const fs = require('fs');

function stripJsonc(text) {
  let out = '';
  let i = 0;
  let inString = false;
  while (i < text.length) {
    const ch = text[i];
    const next = text[i + 1];
    if (inString) {
      out += ch;
      if (ch === '\\') {
        out += next === undefined ? '' : next;
        i += 2;
        continue;
      }
      if (ch === '"') inString = false;
      i++;
      continue;
    }
    if (ch === '"') {
      inString = true;
      out += ch;
      i++;
      continue;
    }
    if (ch === '/' && next === '/') {
      while (i < text.length && text[i] !== '\n') i++;
      continue;
    }
    if (ch === '/' && next === '*') {
      i += 2;
      while (i < text.length && !(text[i] === '*' && text[i + 1] === '/')) i++;
      i += 2;
      continue;
    }
    out += ch;
    i++;
  }
  // 尾逗号：,} 或 ,]
  return out.replace(/,\s*([}\]])/g, '$1');
}

let failed = false;
for (const file of process.argv.slice(2)) {
  try {
    const raw = fs.readFileSync(file, 'utf8').replace(/^﻿/, '');
    JSON.parse(stripJsonc(raw));
    console.log(`[OK] ${file}`);
  } catch (err) {
    console.error(`[FAIL] ${file}: ${err.message}`);
    failed = true;
  }
}
process.exit(failed ? 1 : 0);
