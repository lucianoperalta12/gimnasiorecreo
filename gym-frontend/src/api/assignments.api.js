import api from './axios'

export const assignmentsApi = {
  getMyRoutines() {
    return api.get('/assignments/my-routines')
  },
  getSummary() {
    return api.get('/assignments/summary')
  },
  getByStudent(studentId) {
    return api.get(`/assignments/student/${studentId}`)
  },
  assign(data) {
    return api.post('/assignments', data)
  },
  unassign(id) {
    return api.delete(`/assignments/${id}`)
  }
}
