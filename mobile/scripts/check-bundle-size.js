#!/usr/bin/env node
/**
 * Mobile bundle-size gate (W2.4).
 *
 * uni-app build:mp-weixin 输出到 mobile/unpackage/dist/build/mp-weixin/。
 * 微信小程序硬限制:主包 ≤ 2MB。本脚本设双阈值:
 *   warn at 1.5MB(≥80% 限制 — 该开始减肥)
 *   fail at 1.9MB(留 100KB+ runway)
 *
 * Usage:
 *   node scripts/check-bundle-size.js              (当前 build,默认 mp-weixin)
 *   node scripts/check-bundle-size.js --target h5  (后续可扩展)
 */

const fs = require('node:fs')
const path = require('node:path')

const WARN_BYTES = 1.5 * 1024 * 1024  // 1.5MB
const FAIL_BYTES = 1.9 * 1024 * 1024  // 1.9MB
const HARD_LIMIT_BYTES = 2 * 1024 * 1024  // 2MB(微信硬限制,仅用于报告)

function getDirSize(dir) {
  let total = 0
  for (const entry of fs.readdirSync(dir, { withFileTypes: true })) {
    const full = path.join(dir, entry.name)
    if (entry.isDirectory()) total += getDirSize(full)
    else total += fs.statSync(full).size
  }
  return total
}

function fmtMB(bytes) {
  return (bytes / 1024 / 1024).toFixed(3) + ' MB'
}

const target = process.argv.includes('--target')
  ? process.argv[process.argv.indexOf('--target') + 1]
  : 'mp-weixin'
const buildDir = path.resolve(__dirname, '..', 'unpackage', 'dist', 'build', target)

if (!fs.existsSync(buildDir)) {
  console.log(`[skip] no ${target} build at ${buildDir}; run uni-app build first`)
  process.exit(0)
}

const size = getDirSize(buildDir)
console.log(`Bundle size (${target}): ${fmtMB(size)}`)
console.log(`  WeChat mp 主包硬限制: ${fmtMB(HARD_LIMIT_BYTES)}`)

if (size >= FAIL_BYTES) {
  console.log(`[FAIL] Bundle ${fmtMB(size)} ≥ fail threshold ${fmtMB(FAIL_BYTES)}`)
  process.exit(1)
}
if (size >= WARN_BYTES) {
  console.log(`[WARN] Bundle ${fmtMB(size)} ≥ warn threshold ${fmtMB(WARN_BYTES)}`)
}
console.log(`[OK] Bundle size within budget`)
process.exit(0)
