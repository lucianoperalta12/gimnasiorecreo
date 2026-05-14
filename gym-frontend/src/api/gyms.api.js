import api from './axios'

export const gymsApi = {
  getAll: () => api.get('/gyms'),
  create: (payload) => api.post('/gyms', payload),
  update: (id, payload) => api.put(`/gyms/${id}`, payload),
  toggleStatus: (id) => api.patch(`/gyms/${id}/toggle-status`)
}
