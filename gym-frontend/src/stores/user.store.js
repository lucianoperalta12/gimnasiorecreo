import { defineStore } from 'pinia'
import { ref } from 'vue'
import { usersApi } from '@/api/users.api'

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

  async function changeRole(userId, rol) {
    const { data } = await usersApi.changeRole(userId, rol)
    const index = users.value.findIndex(u => u.id === userId)
    if (index !== -1) users.value[index] = data
    return data
  }

  async function createUser(payload) {
    const { data } = await usersApi.create(payload)
    users.value.unshift(data)
    return data
  }

  async function updateUser(userId, payload) {
    const { data } = await usersApi.update(userId, payload)
    const index = users.value.findIndex(u => u.id === userId)
    if (index !== -1) users.value[index] = data
    return data
  }

  async function changePassword(userId, password) {
    await usersApi.changePassword(userId, password)
  }

  async function toggleUserStatus(userId) {
    const { data } = await usersApi.toggleStatus(userId)
    const index = users.value.findIndex(u => u.id === userId)
    if (index !== -1) users.value[index] = data
    return data
  }

  async function deleteUser(userId) {
    await usersApi.delete(userId)
    users.value = users.value.filter(u => u.id !== userId)
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
    deleteUser
  }
})
