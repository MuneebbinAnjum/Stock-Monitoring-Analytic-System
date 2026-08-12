import api from './client'

type CacheEntry = {
  ts: number
  ttl: number
  value: any
}

const cache = new Map<string, CacheEntry>()

const makeKey = (url: string, params?: any) => {
  if (!params) return url
  try {
    return `${url}|${JSON.stringify(params)}`
  } catch {
    return url
  }
}

export const cachedGet = async (url: string, params?: any, ttl = 1000 * 60 * 5) => {
  const key = makeKey(url, params)
  const now = Date.now()
  const existing = cache.get(key)
  if (existing && (now - existing.ts) < existing.ttl) {
    return existing.value
  }

  const resp = await api.get(url, params ? { params } : undefined)
  const entry: CacheEntry = { ts: now, ttl, value: resp }
  cache.set(key, entry)
  return resp
}

// Helper to fetch time-series data in smaller chunks and merge results. Assumes
// endpoint returns either an array of points OR an object with keys mapping to values
export const fetchTimeSeriesInChunks = async (url: string, startIso: string, endIso: string, chunkDays = 90, params?: any, ttl?: number) => {
  const start = new Date(startIso)
  const end = new Date(endIso)
  if (isNaN(start.getTime()) || isNaN(end.getTime()) || start >= end) {
    const r = await cachedGet(url, params, ttl)
    return r.data?.data ?? r.data
  }

  const results: any[] = []
  const mergedObj: Record<string, any> = {}

  let chunkStart = new Date(start)
  while (chunkStart < end) {
    const chunkEnd = new Date(Math.min(end.getTime(), new Date(chunkStart.getTime() + chunkDays * 24 * 3600 * 1000).getTime()))
    const p = { ...(params || {}), startDate: chunkStart.toISOString(), endDate: chunkEnd.toISOString() }
    const resp = await cachedGet(url, p, ttl ?? 1000 * 60 * 5)
    const data = resp.data?.data ?? resp.data
    if (Array.isArray(data)) {
      results.push(...data)
    } else if (data && typeof data === 'object') {
      // merge by keys (e.g., dailyRevenue). If a key contains a nested object (like dailyRevenue),
      // merge its nested keys and sum numeric values.
      Object.keys(data).forEach(k => {
        const val = data[k]
        if (typeof val === 'number') {
          mergedObj[k] = (mergedObj[k] || 0) + val
        } else if (val && typeof val === 'object' && !Array.isArray(val)) {
          mergedObj[k] = mergedObj[k] || {}
          Object.keys(val).forEach(sub => {
            const subVal = val[sub]
            if (typeof subVal === 'number') {
              mergedObj[k][sub] = (mergedObj[k][sub] || 0) + subVal
            } else {
              mergedObj[k][sub] = subVal
            }
          })
        } else {
          mergedObj[k] = val
        }
      })
    }
    // next chunk
    chunkStart = new Date(chunkEnd.getTime() + 1)
  }

  if (results.length > 0) return results
  return mergedObj
}

export const clearCache = () => cache.clear()
