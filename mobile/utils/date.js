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
