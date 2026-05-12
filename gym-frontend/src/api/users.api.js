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
  changeRole(id, rol) {
    return api.put(`/users/${id}/role`, { rol })
  },
  changePassword(id, password) {
    return api.put(`/users/${id}/password`, { password })
  },
  toggleStatus(id) {
    return api.patch(`/users/${id}/toggle-status`)
  },
  delete(id) {
    return api.delete(`/users/${id}`)
  }
}
