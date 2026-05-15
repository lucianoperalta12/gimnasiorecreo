import api from './axios'

export const paymentsApi = {
  getAll(params = {}) {
    return api.get('/payments', { params })
  },
  getById(id) {
    return api.get(`/payments/${id}`)
  },
  getByMembership(membresiaId) {
    return api.get(`/payments/membership/${membresiaId}`)
  },
  create(payload) {
    return api.post('/payments', payload)
  },
  update(id, payload) {
    return api.put(`/payments/${id}`, payload)
  },
  delete(id) {
    return api.delete(`/payments/${id}`)
  }
}
