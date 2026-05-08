import api from './axios'

export const exercisesApi = {
  getAll() {
    return api.get('/exercises')
  },
  getById(id) {
    return api.get(`/exercises/${id}`)
  },
  create(data) {
    return api.post('/exercises', data)
  },
  update(id, data) {
    return api.put(`/exercises/${id}`, data)
  },
  delete(id) {
    return api.delete(`/exercises/${id}`)
  }
}
