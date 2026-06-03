import { defineStore } from 'pinia'
import { ref } from 'vue'
import { usersApi } from '@/api/users.api'
import { parsePaginatedResponse } from '@/utils/pagination'

function rowKey(user) {
  return `${user.id}-${user.gymId}`
}

function findUserRowIndex(users, user) {
  return users.findIndex((u) => u.id === user.id && u.gymId === user.gymId)
}

export const useUserStore = defineStore('user', () => {
  const users = ref([])
  const students = ref([])
  const usersTotalCount = ref(0)
  const usersPage = ref(null)
  const usersPageSize = ref(null)
  const usersServerPaginationEnabled = ref(false)
  const usersActiveCount = ref(0)
  const usersInactiveCount = ref(0)
  const studentsTotalCount = ref(0)
  const studentsPage = ref(null)
  const studentsPageSize = ref(null)
  const studentsServerPaginationEnabled = ref(false)
  const loading = ref(false)

  const lastUsersParams = ref({})
  const lastStudentsParams = ref({})

  async function fetchUsers(params = {}) {
    loading.value = true
    lastUsersParams.value = params
    try {
      const response = await usersApi.getAll(params)
      const pagination = parsePaginatedResponse(response)
      users.value = pagination.items
      usersTotalCount.value = pagination.totalCount
      usersPage.value = pagination.page
      usersPageSize.value = pagination.pageSize
      usersServerPaginationEnabled.value = pagination.serverPaginationEnabled
      usersActiveCount.value = pagination.activeCount ?? pagination.items.filter((u) => u.activo).length
      usersInactiveCount.value = pagination.inactiveCount ?? pagination.items.filter((u) => !u.activo).length
    } finally {
      loading.value = false
    }
  }

  async function fetchStudents(params = {}) {
    loading.value = true
    lastStudentsParams.value = params
    try {
      const response = await usersApi.getStudents(params)
      const pagination = parsePaginatedResponse(response)
      students.value = pagination.items
      studentsTotalCount.value = pagination.totalCount
      studentsPage.value = pagination.page
      studentsPageSize.value = pagination.pageSize
      studentsServerPaginationEnabled.value = pagination.serverPaginationEnabled
    } finally {
      loading.value = false
    }
  }

  async function changeRole(user, rol) {
    const { data } = await usersApi.changeRole(user.id, rol, user.gymId)
    const index = findUserRowIndex(users.value, user)
    if (index !== -1) users.value[index] = data
    return data
  }

  async function createUser(payload) {
    const { data } = await usersApi.create(payload)
    await fetchUsers(lastUsersParams.value)
    return data
  }

  async function updateUser(userId, payload) {
    const { data } = await usersApi.update(userId, payload)
    users.value = users.value.map((u) =>
      u.id === userId
        ? {
            ...u,
            nombre: data.nombre,
            apellido: data.apellido,
            email: data.email,
            dni: data.dni,
            debeCambiarPassword: data.debeCambiarPassword,
            fechaNacimiento: data.fechaNacimiento,
            domicilio: data.domicilio,
            telefono: data.telefono,
            observaciones: data.observaciones
          }
        : u
    )
    return data
  }

  async function changePassword(userId, password) {
    await usersApi.changePassword(userId, password)
  }

  async function toggleUserStatus(user) {
    const { data } = await usersApi.toggleStatus(user.id, user.gymId)
    const index = findUserRowIndex(users.value, user)
    if (index !== -1) users.value[index] = data
    return data
  }

  async function deleteUser(user) {
    await usersApi.delete(user.id, user.gymId)
    users.value = users.value.filter((u) => rowKey(u) !== rowKey(user))
  }

  return {
    users,
    students,
    usersTotalCount,
    usersPage,
    usersPageSize,
    usersServerPaginationEnabled,
    usersActiveCount,
    usersInactiveCount,
    studentsTotalCount,
    studentsPage,
    studentsPageSize,
    studentsServerPaginationEnabled,
    loading,
    fetchUsers,
    fetchStudents,
    createUser,
    updateUser,
    changeRole,
    changePassword,
    toggleUserStatus,
    deleteUser,
    rowKey
  }
})
