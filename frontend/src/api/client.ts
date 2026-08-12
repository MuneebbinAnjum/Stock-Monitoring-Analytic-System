import axios from 'axios'

const api = axios.create({
  baseURL: import.meta.env.VITE_API_BASE ?? '/api',
  timeout: 15000,
  withCredentials: true
})

// Request interceptor to attach token
api.interceptors.request.use((config) => {
  const token = sessionStorage.getItem('token')
  if (token && config.headers) {
    config.headers['Authorization'] = `Bearer ${token}`
  }
  return config
})

// Response interceptor to handle 401 and refresh
let isRefreshing = false
let refreshQueue: Array<(token: string | null) => void> = []

// Normalize server-side error shape: if API returns success=false with 200/4xx, reject so callers handle uniformly
api.interceptors.response.use(
  (r) => {
    try {
      if (r?.data && typeof r.data === 'object' && 'success' in r.data && r.data.success === false) {
        return Promise.reject({ response: r })
      }
    } catch (ex) { }
    return r
  },
  async (error) => {
    const originalRequest = error.config
    if (error.response?.status === 401 && !originalRequest._retry && !originalRequest.url?.includes('/auth/login') && !originalRequest.url?.includes('/auth/refresh')) {
      originalRequest._retry = true
      if (!isRefreshing) {
        isRefreshing = true
        const refreshToken = sessionStorage.getItem('refreshToken')
        try {
          const resp = await axios.post(`${import.meta.env.VITE_API_BASE ?? '/api'}/auth/refresh`, null, { withCredentials: true })
          const data = resp.data.data
          sessionStorage.setItem('token', data.token)
          sessionStorage.setItem('refreshToken', data.refreshToken)
          refreshQueue.forEach(cb => cb(data.token))
          refreshQueue = []
          originalRequest.headers['Authorization'] = `Bearer ${data.token}`
          return api(originalRequest)
        } catch (ex) {
          refreshQueue.forEach(cb => cb(null))
          refreshQueue = []
          sessionStorage.removeItem('token')
          sessionStorage.removeItem('refreshToken')
          localStorage.removeItem('token')
          localStorage.removeItem('refreshToken')
          throw ex
        } finally {
          isRefreshing = false
        }
      } else {
        return new Promise((resolve, reject) => {
          refreshQueue.push((token: string | null) => {
            if (token) {
              originalRequest.headers['Authorization'] = `Bearer ${token}`
              resolve(api(originalRequest))
            } else {
              reject(error)
            }
          })
        })
      }
    }
    throw error
  }
)

export default api
