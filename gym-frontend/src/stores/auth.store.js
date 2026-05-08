import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import { authApi } from '@/api/auth.api'

export const useAuthStore = defineStore('auth', () => {
  const user = ref(JSON.parse(localStorage.getItem('user') || 'null'))
  const isAuthenticated = computed(() => !!user.value)
  const userRole = computed(() => user.value?.rol || null)

  function setUser(userData) {
    user.value = userData
    localStorage.setItem('user', JSON.stringify(userData))
  }

  async function loginWithGoogle(credential) {
    const { data } = await authApi.loginGoogle(credential)
    saveAuthData(data)
    return data
  }

  async function loginAdmin(credentials) {
    const { data } = await authApi.loginAdmin(credentials.username, credentials.password)
    saveAuthData(data)
    return data
  }

  function saveAuthData(data) {
    localStorage.setItem('accessToken', data.accessToken)
    localStorage.setItem('refreshToken', data.refreshToken)
    setUser(data.user)
  }

  async function logout() {
    // Clear local state first to prevent infinite loops if the API call fails
    user.value = null
    localStorage.removeItem('accessToken')
    localStorage.removeItem('refreshToken')
    localStorage.removeItem('user')

    try {
      await authApi.logout()
    } catch {
      // Ignore errors on logout
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
    logout,
    hasRole
  }
})
