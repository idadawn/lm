/**
 * Unit tests for mobile/utils/markdown.js (W1.4).
 *
 * sanitizeHtml needs a DOM, mirroring the H5 production environment. We use
 * happy-dom (already a web devDep, accessible from this test via the workspace's
 * node_modules resolution) to provide window/document.
 */

const path = require('node:path')
const assert = require('node:assert')
const { pathToFileURL } = require('node:url')

const { Window } = require(path.resolve(__dirname, '../../web/node_modules/happy-dom'))

const win = new Window()
globalThis.window = win
globalThis.document = win.document

let pass = 0
let fail = 0

async function test(name, fn) {
  try {
    await fn()
    pass++
    console.log(`  PASS  ${name}`)
  } catch (e) {
    fail++
    console.log(`  FAIL  ${name}: ${e.message}`)
  }
}

;(async () => {
  const m = await import(pathToFileURL(path.resolve(__dirname, '../utils/markdown.js')).href)

  await test('renderMarkdown empty input returns empty string', () => {
    assert.strictEqual(m.renderMarkdown(''), '')
    assert.strictEqual(m.renderMarkdown(null), '')
    assert.strictEqual(m.renderMarkdown(undefined), '')
  })

  await test('renderMarkdown bold + italic', () => {
    const html = m.renderMarkdown('**bold** and *italic*')
    assert.match(html, /<strong>bold<\/strong>/)
    assert.match(html, /<em>italic<\/em>/)
  })

  await test('renderMarkdown code block fenced', () => {
    const html = m.renderMarkdown('```js\nconst x = 1\n```')
    assert.match(html, /<pre>/)
    assert.match(html, /<code/)
  })

  await test('renderMarkdown unordered list', () => {
    const html = m.renderMarkdown('- a\n- b\n- c')
    assert.match(html, /<ul>/)
    assert.ok((html.match(/<li>/g) || []).length === 3)
  })

  await test('renderMarkdown table (gfm)', () => {
    const html = m.renderMarkdown('| h1 | h2 |\n|---|---|\n| a | b |')
    assert.match(html, /<table>/)
    assert.match(html, /<th>h1<\/th>/)
  })

  await test('renderMarkdown chinese UTF-8 preserved', () => {
    const html = m.renderMarkdown('合格率：**95%**')
    assert.match(html, /合格率/)
    assert.match(html, /<strong>95%<\/strong>/)
  })

  await test('sanitizeHtml strips <script>', () => {
    const out = m.sanitizeHtml('<p>ok</p><script>alert(1)</script>')
    assert.match(out, /<p>ok<\/p>/)
    assert.ok(!out.includes('<script>'))
  })

  await test('sanitizeHtml strips <iframe>', () => {
    const out = m.sanitizeHtml('<p>ok</p><iframe src="x"></iframe>')
    assert.ok(!out.includes('<iframe'))
  })

  await test('sanitizeHtml strips event handlers', () => {
    const out = m.sanitizeHtml('<a href="#" onclick="alert(1)">click</a>')
    assert.ok(!out.includes('onclick'))
  })

  await test('sanitizeHtml preserves whitelist tags', () => {
    const out = m.sanitizeHtml('<p>p</p><strong>s</strong><table><tr><td>c</td></tr></table>')
    assert.match(out, /<p>p<\/p>/)
    assert.match(out, /<strong>s<\/strong>/)
    assert.match(out, /<td>c<\/td>/)
  })

  console.log(`\n${pass + fail} tests: ${pass} passed, ${fail} failed`)
  process.exit(fail === 0 ? 0 : 1)
})().catch(err => {
  console.error('TEST RUNNER ERROR:', err)
  process.exit(2)
})
