import api from './client'

export const AuthApi = {
  login: async (email: string, password: string, role: string) => {
    const res = await api.post('/auth/login', { email, password, role })
    return res.data.data
  },

  register: async (payload: { email: string; password: string; fullName: string; role: string }) => {
    const res = await api.post('/auth/register', payload)
    return res.data.data
  },

  logout: async () => {
    await api.post('/auth/logout')
  },

  me: async () => {
    const res = await api.get('/auth/me')
    return res.data.data
  }
,

  changePassword: async (payload: { currentPassword: string; newPassword: string }) => {
    const res = await api.post('/auth/change-password', payload)
    return res.data
  }
}
