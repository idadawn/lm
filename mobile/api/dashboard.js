import { get } from '@/utils/http.js'

export function getMonthlyReport(params) {
  return get('/api/lab/monthly-quality-report', params)
}

export function getLaminationTrend(params) {
  return get('/api/lab/dashboard/lamination-trend', params)
}

export function getThicknessCorrelation(params) {
  return get('/api/lab/dashboard/thickness-correlation', params)
}

export function getDailyProduction() {
  return get('/api/lab/dashboard/daily-production')
}
