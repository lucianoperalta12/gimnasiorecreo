import api from './axios'

export const usersApi = {
  getAll(params = {}) {
    return api.get('/users', { params })
  },
  getById(id) {
    return api.get(`/users/${id}`)
  },
  getStudents(params = {}) {
    return api.get('/users/students', { params })
  },
  create(payload) {
    return api.post('/users', payload)
  },
  update(id, payload) {
    return api.put(`/users/${id}`, payload)
  },
  changeRole(id, rol, gymId) {
    return api.put(`/users/${id}/role`, { rol, gymId: gymId ?? undefined })
  },
  changePassword(id, password) {
    return api.put(`/users/${id}/password`, { password })
  },
  changeInitialPassword(password) {
    return api.put('/users/me/change-initial-password', { password })
  },
  toggleStatus(id, gymId) {
    return api.patch(`/users/${id}/toggle-status`, null, {
      params: gymId != null ? { gymId } : undefined
    })
  },
  delete(id, gymId) {
    return api.delete(`/users/${id}`, {
      params: gymId != null ? { gymId } : undefined
    })
  }
}
