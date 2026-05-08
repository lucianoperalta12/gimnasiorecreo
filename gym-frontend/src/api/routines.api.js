import api from './axios'

export const routinesApi = {
  getAll() {
    return api.get('/routines')
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
