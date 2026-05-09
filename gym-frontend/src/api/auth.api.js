import axios from './axios'

export const authApi = {
  loginGoogle: (credential) => axios.post('/auth/google', { credential }),
  loginAdmin: (username, password) => axios.post('/auth/login-admin', { username, password }),
  register: (nombre, email, password) => axios.post('/auth/register', { nombre, email, password }),
  refresh: (refreshToken) => axios.post('/auth/refresh', { refreshToken }),
  logout: () => axios.post('/auth/logout')
}
