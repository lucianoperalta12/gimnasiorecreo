import api from './axios'

export const ingresosApi = {
  getAll(params = {}) {
    return api.get('/ingresos', { params })
  },
  getToday(params = {}) {
    return api.get('/ingresos/today', { params })
  },
  registrar(dni) {
    return api.post('/ingresos/registrar', { dni })
  }
}
