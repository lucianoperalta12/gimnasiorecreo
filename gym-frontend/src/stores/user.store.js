import { defineStore } from 'pinia'
import { ref } from 'vue'
import { usersApi } from '@/api/users.api'

function rowKey(user) {
  return `${user.id}-${user.gymId}`
}

function findUserRowIndex(users, user) {
  return users.findIndex((u) => u.id === user.id && u.gymId === user.gymId)
}

export const useUserStore = defineStore('user', () => {
  const users = ref([])
  const students = ref([])
  const loading = ref(false)

  async function fetchUsers() {
    loading.value = true
    try {
      const { data } = await usersApi.getAll()
      users.value = data
    } finally {
      loading.value = false
    }
  }

  async function fetchStudents() {
    loading.value = true
    try {
      const { data } = await usersApi.getStudents()
      students.value = data
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
    await fetchUsers()
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
