/**
 * Markdown rendering utilities for mobile chat assistant.
 *
 * Plan v3 W1.4: assistant messages render as markdown across mp-weixin /
 * app-plus / h5. mp-weixin and app-plus use the mp-html component (which
 * accepts an HTML string and handles its own platform-safe rendering); H5
 * pipes the HTML through DOMPurify before v-html.
 *
 * sanitizeHtml is only invoked from the H5 template branch (#ifdef H5).
 * On mp-weixin / app-plus the function is imported but never called at
 * runtime; vite/uni-app tree-shaking can elide DOMPurify from those builds
 * if needed.
 */

import { marked } from 'marked'
import DOMPurify from 'dompurify'

marked.setOptions({
  breaks: true, // single line break → <br>
  gfm: true,    // GitHub-flavored markdown (tables, fenced code, autolinks)
})

/**
 * Convert markdown text to HTML.
 * Returns '' for empty/null input so consumers can bind directly.
 */
export function renderMarkdown(content) {
  if (!content) return ''
  try {
    return marked.parse(content)
  } catch (_) {
    // marked throws on malformed input; surface raw text rather than crash UI
    return String(content)
  }
}

const ALLOWED_TAGS = [
  'p', 'br', 'hr',
  'strong', 'em', 'b', 'i', 'u', 's', 'del', 'ins',
  'code', 'pre',
  'ul', 'ol', 'li',
  'h1', 'h2', 'h3', 'h4', 'h5', 'h6',
  'blockquote',
  'a',
  'table', 'thead', 'tbody', 'tr', 'td', 'th',
  'span', 'div',
]

const ALLOWED_ATTR = ['href', 'title', 'class', 'src', 'alt', 'colspan', 'rowspan', 'align']

/**
 * Sanitize HTML for v-html on H5. Strips scripts, event handlers, iframes,
 * and any tag not in the markdown-output whitelist.
 *
 * Only invoked from the H5 template branch — DOMPurify needs a DOM, which
 * mp-weixin / app-plus environments do not provide. Importing here is safe
 * (DOMPurify defers DOM access until sanitize() is called).
 */
export function sanitizeHtml(html) {
  if (!html) return ''
  return DOMPurify.sanitize(html, { ALLOWED_TAGS, ALLOWED_ATTR })
}
