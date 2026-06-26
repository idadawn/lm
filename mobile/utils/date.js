export function formatDate(date, format = 'yyyy-MM-dd') {
  const year = date.getFullYear()
  const month = date.getMonth() + 1
  const day = date.getDate()
  return format
    .replace('yyyy', String(year))
    .replace('MM', month < 10 ? '0' + month : String(month))
    .replace('dd', day < 10 ? '0' + day : String(day))
}

export function getStartOfMonth() {
  const now = new Date()
  return new Date(now.getFullYear(), now.getMonth(), 1)
}

export function getToday() {
  return new Date()
}

// ── 快速日期区间：返回 [Date, Date]（起、止；都是包含的边界，止 = 今天为止） ──
// 周以「周一」为起点（行业内最常用：检测中心一周从周一开始算）。

function startOfWeek(d) {
  const x = new Date(d.getFullYear(), d.getMonth(), d.getDate())
  // getDay(): 0=周日 / 1=周一 / ... / 6=周六；周一作为一周第一天
  const dow = x.getDay()
  const diff = dow === 0 ? -6 : 1 - dow
  x.setDate(x.getDate() + diff)
  return x
}

function startOfMonthDate(d) {
  return new Date(d.getFullYear(), d.getMonth(), 1)
}

function endOfMonthDate(d) {
  return new Date(d.getFullYear(), d.getMonth() + 1, 0)
}

function endOfWeekDate(d) {
  const start = startOfWeek(d)
  const end = new Date(start)
  end.setDate(end.getDate() + 6)
  return end
}

/**
 * 给定一个"快速选择" key，返回 [startDate, endDate]（已格式化为 yyyy-MM-dd）。
 * 支持：today / this_week / last_week / current_month / last_month / this_year
 */
export function getQuickRange(key) {
  const now = new Date()
  switch (key) {
    case 'today': {
      const d = formatDate(now)
      return [d, d]
    }
    case 'this_week':
      return [formatDate(startOfWeek(now)), formatDate(now)]
    case 'last_week': {
      const thisMon = startOfWeek(now)
      const lastMon = new Date(thisMon)
      lastMon.setDate(thisMon.getDate() - 7)
      const lastSun = new Date(thisMon)
      lastSun.setDate(thisMon.getDate() - 1)
      return [formatDate(lastMon), formatDate(lastSun)]
    }
    case 'current_month':
      return [formatDate(startOfMonthDate(now)), formatDate(now)]
    case 'last_month': {
      const firstOfThis = startOfMonthDate(now)
      const lastOfLast = new Date(firstOfThis)
      lastOfLast.setDate(0) // 上月最后一天
      const firstOfLast = startOfMonthDate(lastOfLast)
      return [formatDate(firstOfLast), formatDate(lastOfLast)]
    }
    case 'this_year':
      // 今年：1 月 1 日 → 今天
      return [formatDate(new Date(now.getFullYear(), 0, 1)), formatDate(now)]
    default:
      return [formatDate(now), formatDate(now)]
  }
}

/**
 * 反向：给定 [startDate, endDate]（字符串）判断它属于哪个快速 key。
 * 用于打开页面时回显当前选中的 chip。返回 '' 表示自定义。
 */
export function detectQuickRangeKey(start, end) {
  const candidates = ['today', 'this_week', 'last_week', 'current_month', 'last_month', 'this_year']
  for (const k of candidates) {
    const [s, e] = getQuickRange(k)
    if (s === start && e === end) return k
  }
  return ''
}
