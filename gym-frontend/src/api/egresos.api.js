import api from './axios'

export const egresosApi = {
  getAll(params = {}) {
    return api.get('/egresos', { params })
  },
  getById(id) {
    return api.get(`/egresos/${id}`)
  },
  create(payload) {
    return api.post('/egresos', payload)
  },
  update(id, payload) {
    return api.put(`/egresos/${id}`, payload)
  },
  delete(id) {
    return api.delete(`/egresos/${id}`)
  }
}
