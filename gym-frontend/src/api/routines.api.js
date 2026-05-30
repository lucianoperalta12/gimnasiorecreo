import api from './axios'

export const routinesApi = {
  getAll(params = {}) {
    return api.get('/routines', { params })
  },
  getById(id) {
    return api.get(`/routines/${id}`)
  },
  create(data) {
    return api.post('/routines', data)
  },
  update(id, data) {
    return api.put(`/routines/${id}`, data)
  },
  delete(id) {
    return api.delete(`/routines/${id}`)
  }
}
