import axios from './axios'

export const authApi = {
  loginGoogle: (credential) => axios.post('/auth/google', { credential }),
  loginAdmin: (username, password) => axios.post('/auth/login-admin', { username, password }),
  refresh: (refreshToken) => axios.post('/auth/refresh', { refreshToken }),
  logout: () => axios.post('/auth/logout')
}
