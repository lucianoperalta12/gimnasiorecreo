import api from './axios'

export const membershipsApi = {
  getAll(params = {}) {
    return api.get('/memberships', { params })
  },
  getDashboardSummary(params = {}) {
    return api.get('/memberships/dashboard-summary', { params })
  },
  getById(id) {
    return api.get(`/memberships/${id}`)
  },
  getByStudent(studentId) {
    return api.get(`/memberships/student/${studentId}`)
  },
  getStudentAccess(studentId) {
    return api.get(`/memberships/student/${studentId}/access`)
  },
  getMyAccess() {
    return api.get('/memberships/me/access')
  },
  create(payload) {
    return api.post('/memberships', payload)
  },
  renew(studentId, payload) {
    return api.post(`/memberships/student/${studentId}/renew`, payload)
  },
  cancel(id, payload) {
    return api.post(`/memberships/${id}/cancel`, payload)
  },
  sendExpirationEmail(id) {
    return api.post(`/memberships/${id}/send-expiration-email`)
  },
  getRenovationsReport(params = {}) {
    return api.get('/memberships/renovations-report', { params })
  }
}
