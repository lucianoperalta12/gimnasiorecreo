import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import { authApi } from '@/api/auth.api'

export const useAuthStore = defineStore('auth', () => {

  function safeParse(value) {
    try {
      if (!value || value === 'undefined') {
        return null
      }

      return JSON.parse(value)
    } catch {
      return null
    }
  }

  const user = ref(safeParse(localStorage.getItem('user')))

  const isAuthenticated = computed(() => !!user.value)

  const userRole = computed(() => user.value?.rol || null)

  function setUser(userData) {
    user.value = userData

    if (!userData) {
      localStorage.removeItem('user')
      return
    }

    localStorage.setItem('user', JSON.stringify(userData))
  }

  async function loginWithGoogle(credential) {
    const { data } = await authApi.loginGoogle(credential)

    saveAuthData(data)

    return data
  }

  async function loginAdmin(credentials) {
    const { data } = await authApi.loginAdmin(
      credentials.username,
      credentials.password
    )

    saveAuthData(data)

    return data
  }

  async function register(data) {
    const response = await authApi.register(data.nombre, data.email, data.password)
    saveAuthData(response.data)
    return response.data
  }

  function saveAuthData(data) {
    if (!data) {
      throw new Error('Auth data is required')
    }

    if (!data.accessToken || !data.refreshToken || !data.user) {
      throw new Error('Invalid auth response')
    }

    localStorage.setItem('accessToken', data.accessToken)
    localStorage.setItem('refreshToken', data.refreshToken)

    setUser(data.user)
  }

  async function logout() {
    user.value = null

    localStorage.removeItem('accessToken')
    localStorage.removeItem('refreshToken')
    localStorage.removeItem('user')

    try {
      await authApi.logout()
    } catch (error) {
      console.error('Logout error:', error)
    }
  }

  function hasRole(...roles) {
    return roles.includes(user.value?.rol)
  }

  return {
    user,
    isAuthenticated,
    userRole,
    setUser,
    loginWithGoogle,
    loginAdmin,
    register,
    logout,
    hasRole
  }
})