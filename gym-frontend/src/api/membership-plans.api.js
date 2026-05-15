import api from './axios'

export const membershipPlansApi = {
  getAll(gymId) {
    return api.get('/membershipplans', { params: gymId ? { gymId } : {} })
  },
  getById(id) {
    return api.get(`/membershipplans/${id}`)
  },
  create(payload) {
    return api.post('/membershipplans', payload)
  },
  update(id, payload) {
    return api.put(`/membershipplans/${id}`, payload)
  },
  delete(id) {
    return api.delete(`/membershipplans/${id}`)
  }
}
