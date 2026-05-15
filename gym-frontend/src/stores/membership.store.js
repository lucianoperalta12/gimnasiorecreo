import { defineStore } from 'pinia'
import { ref } from 'vue'
import { membershipPlansApi } from '@/api/membership-plans.api'
import { membershipsApi } from '@/api/memberships.api'
import { paymentsApi } from '@/api/payments.api'

export const useMembershipStore = defineStore('membership', () => {
  const plans = ref([])
  const memberships = ref([])
  const payments = ref([])
  const myAccess = ref(null)
  const loading = ref(false)

  async function fetchPlans(gymId) {
    loading.value = true
    try {
      const { data } = await membershipPlansApi.getAll(gymId)
      plans.value = data
    } finally {
      loading.value = false
    }
  }

  async function createPlan(payload) {
    const { data } = await membershipPlansApi.create(payload)
    plans.value.unshift(data)
    return data
  }

  async function updatePlan(id, payload) {
    const { data } = await membershipPlansApi.update(id, payload)
    const index = plans.value.findIndex(p => p.id === id)
    if (index !== -1) plans.value[index] = data
    return data
  }

  async function deletePlan(id) {
    await membershipPlansApi.delete(id)
    plans.value = plans.value.filter(p => p.id !== id)
  }

  async function fetchMemberships(params = {}) {
    loading.value = true
    try {
      const { data } = await membershipsApi.getAll(params)
      memberships.value = data
    } finally {
      loading.value = false
    }
  }

  async function fetchMembershipsByStudent(studentId) {
    loading.value = true
    try {
      const { data } = await membershipsApi.getByStudent(studentId)
      memberships.value = data
      return data
    } finally {
      loading.value = false
    }
  }

  async function createMembership(payload) {
    const { data } = await membershipsApi.create(payload)
    memberships.value.unshift(data)
    return data
  }

  async function renewMembership(studentId, payload) {
    const { data } = await membershipsApi.renew(studentId, payload)
    await fetchMemberships()
    return data
  }

  async function cancelMembership(id, payload) {
    const { data } = await membershipsApi.cancel(id, payload)
    const index = memberships.value.findIndex(m => m.id === id)
    if (index !== -1) memberships.value[index] = data
    return data
  }

  async function fetchMyAccess() {
    loading.value = true
    try {
      const { data } = await membershipsApi.getMyAccess()
      myAccess.value = data
      return data
    } finally {
      loading.value = false
    }
  }

  async function fetchPayments(params = {}) {
    loading.value = true
    try {
      const { data } = await paymentsApi.getAll(params)
      payments.value = data
    } finally {
      loading.value = false
    }
  }

  async function createPayment(payload) {
    const { data } = await paymentsApi.create(payload)
    payments.value.unshift(data)
    return data
  }

  async function updatePayment(id, payload) {
    const { data } = await paymentsApi.update(id, payload)
    const index = payments.value.findIndex(p => p.id === id)
    if (index !== -1) payments.value[index] = data
    return data
  }

  async function deletePayment(id) {
    await paymentsApi.delete(id)
    payments.value = payments.value.filter(p => p.id !== id)
  }

  return {
    plans,
    memberships,
    payments,
    myAccess,
    loading,
    fetchPlans,
    createPlan,
    updatePlan,
    deletePlan,
    fetchMemberships,
    fetchMembershipsByStudent,
    createMembership,
    renewMembership,
    cancelMembership,
    fetchMyAccess,
    fetchPayments,
    createPayment,
    updatePayment,
    deletePayment
  }
})
