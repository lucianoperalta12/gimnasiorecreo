import { defineStore } from 'pinia'
import { computed, ref } from 'vue'
import { authApi } from '@/api/auth.api'

const STORAGE_KEYS = {
  accessToken: 'accessToken',
  refreshToken: 'refreshToken',
  user: 'user',
  gyms: 'auth.gyms',
  pendingGymSelection: 'auth.pendingGymSelection'
}

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

function readStoredGyms() {
  return safeParse(localStorage.getItem(STORAGE_KEYS.gyms)) || []
}

function readPendingSelection() {
  return localStorage.getItem(STORAGE_KEYS.pendingGymSelection) === 'true'
}

export const useAuthStore = defineStore('auth', () => {
  const accessToken = ref(localStorage.getItem(STORAGE_KEYS.accessToken) || '')
  const refreshToken = ref(localStorage.getItem(STORAGE_KEYS.refreshToken) || '')
  const user = ref(safeParse(localStorage.getItem(STORAGE_KEYS.user)))
  const gyms = ref(readStoredGyms())
  const pendingGymSelection = ref(readPendingSelection())

  const isAuthenticated = computed(() => !!accessToken.value)
  const userRole = computed(() => user.value?.rol || null)
  const hasSelectedGym = computed(() => !!user.value?.gymId && !pendingGymSelection.value)
  const hasMultipleGyms = computed(() => (gyms.value?.length || 0) > 1)
  const selectedGym = computed(() => gyms.value.find((gym) => gym.gymId === user.value?.gymId) || null)

  function persistGyms(gymList) {
    gyms.value = Array.isArray(gymList) ? gymList : []
    localStorage.setItem(STORAGE_KEYS.gyms, JSON.stringify(gyms.value))
  }

  function clearGyms() {
    gyms.value = []
    localStorage.removeItem(STORAGE_KEYS.gyms)
  }

  function buildSingleGymFromUser(userData) {
    if (!userData?.gymId) {
      return []
    }

    return [{
      gymId: userData.gymId,
      gymNombre: userData.gymNombre || 'Gimnasio',
      logoUrl: userData.gymLogoUrl || null,
      colorPrincipalHex: userData.gymColorPrincipalHex || '#ff6600',
      rol: userData.rol || 'Alumno'
    }]
  }

  function setUser(userData) {
    user.value = userData

    if (!userData) {
      localStorage.removeItem(STORAGE_KEYS.user)
      return
    }

    localStorage.setItem(STORAGE_KEYS.user, JSON.stringify(userData))
  }

  function setPendingSelection(isPending) {
    pendingGymSelection.value = !!isPending
    localStorage.setItem(STORAGE_KEYS.pendingGymSelection, pendingGymSelection.value ? 'true' : 'false')
  }

  function saveFinalAuthData(data) {
    if (!data || !data.accessToken || !data.refreshToken || !data.user) {
      throw new Error('Invalid auth response')
    }

    accessToken.value = data.accessToken
    refreshToken.value = data.refreshToken
    localStorage.setItem(STORAGE_KEYS.accessToken, data.accessToken)
    localStorage.setItem(STORAGE_KEYS.refreshToken, data.refreshToken)
    setUser(data.user)
    persistGyms(data.gyms?.length ? data.gyms : buildSingleGymFromUser(data.user))
    setPendingSelection(false)
  }

  function savePendingAuthData(data) {
    if (!data?.accessToken) {
      throw new Error('Pending selection requires a temporary access token')
    }

    accessToken.value = data.accessToken
    refreshToken.value = ''
    localStorage.setItem(STORAGE_KEYS.accessToken, data.accessToken)
    localStorage.removeItem(STORAGE_KEYS.refreshToken)
    localStorage.removeItem(STORAGE_KEYS.user)
    user.value = null
    persistGyms(data.gyms || [])
    setPendingSelection(true)
  }

  function saveAuthData(data) {
    if (!data) {
      throw new Error('Auth data is required')
    }

    const needsGymSelection = !!(data.requiresGymSelection ?? data.RequiresGymSelection)

    if (needsGymSelection) {
      savePendingAuthData(data)
      return { ...data, requiresGymSelection: true }
    }

    saveFinalAuthData(data)
    return data
  }

  async function loginWithGoogle(credential) {
    const { data } = await authApi.loginGoogle(credential)
    return saveAuthData(data)
  }

  async function login(username, password) {
    const { data } = await authApi.loginAdmin(username, password)
    return saveAuthData(data)
  }

  async function register(data) {
    const response = await authApi.register(data.nombre, data.email, data.password)
    return saveAuthData(response.data)
  }

  async function selectGym(gymId) {
    const { data } = await authApi.selectGym(gymId)
    return saveAuthData(data)
  }

  function clearSession() {
    user.value = null
    accessToken.value = ''
    refreshToken.value = ''
    clearGyms()
    setPendingSelection(false)

    localStorage.removeItem(STORAGE_KEYS.accessToken)
    localStorage.removeItem(STORAGE_KEYS.refreshToken)
    localStorage.removeItem(STORAGE_KEYS.user)
  }

  async function logout() {
    try {
      await authApi.logout()
    } catch (error) {
      console.error('Logout error:', error)
    } finally {
      clearSession()
    }
  }

  function hasRole(...roles) {
    return roles.includes(user.value?.rol)
  }

  return {
    user,
    gyms,
    pendingGymSelection,
    isAuthenticated,
    userRole,
    hasSelectedGym,
    hasMultipleGyms,
    selectedGym,
    setUser,
    setPendingSelection,
    saveAuthData,
    loginWithGoogle,
    login,
    register,
    selectGym,
    logout,
    hasRole
  }
})
