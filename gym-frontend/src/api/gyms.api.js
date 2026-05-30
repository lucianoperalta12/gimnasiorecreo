import api from './axios'

export const gymsApi = {
  getAll: async () => {
    const res = await api.get('/gyms')
    res.data = res.data ? res.data.filter(g => g.activo) : []
    return res
  },
  getAllAdmin: () => api.get('/gyms'),
  create: (payload) => api.post('/gyms', payload),
  update: (id, payload) => api.put(`/gyms/${id}`, payload),
  toggleStatus: (id) => api.patch(`/gyms/${id}/toggle-status`)
}
