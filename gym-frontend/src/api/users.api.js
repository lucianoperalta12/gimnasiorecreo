import api from './axios'

export const usersApi = {
  getAll() {
    return api.get('/users')
  },
  getById(id) {
    return api.get(`/users/${id}`)
  },
  getStudents() {
    return api.get('/users/students')
  },
  create(payload) {
    return api.post('/users', payload)
  },
  update(id, payload) {
    return api.put(`/users/${id}`, payload)
  },
  changeRole(id, rol) {
    return api.put(`/users/${id}/role`, { rol })
  },
  changePassword(id, password) {
    return api.put(`/users/${id}/password`, { password })
  },
  changeInitialPassword(password) {
    return api.put('/users/me/change-initial-password', { password })
  },
  toggleStatus(id) {
    return api.patch(`/users/${id}/toggle-status`)
  },
  delete(id) {
    return api.delete(`/users/${id}`)
  }
}
