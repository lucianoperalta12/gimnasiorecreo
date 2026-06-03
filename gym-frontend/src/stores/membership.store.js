import { defineStore } from 'pinia'
import { ref } from 'vue'
import { membershipPlansApi } from '@/api/membership-plans.api'
import { membershipsApi } from '@/api/memberships.api'
import { paymentsApi } from '@/api/payments.api'
import { parsePaginatedResponse } from '@/utils/pagination'

export const useMembershipStore = defineStore('membership', () => {
  const plans = ref([])
  const memberships = ref([])
  const payments = ref([])
  const membershipsTotalCount = ref(0)
  const membershipsPage = ref(null)
  const membershipsPageSize = ref(null)
  const membershipsServerPaginationEnabled = ref(false)
  const paymentsTotalCount = ref(0)
  const paymentsPage = ref(null)
  const paymentsPageSize = ref(null)
  const paymentsServerPaginationEnabled = ref(false)
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
      const response = await membershipsApi.getAll(params)
      const pagination = parsePaginatedResponse(response)
      memberships.value = pagination.items
      membershipsTotalCount.value = pagination.totalCount
      membershipsPage.value = pagination.page
      membershipsPageSize.value = pagination.pageSize
      membershipsServerPaginationEnabled.value = pagination.serverPaginationEnabled
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

  async function renewMembership(studentId, payload, options = {}) {
    const { refreshMemberships = true } = options
    const { data } = await membershipsApi.renew(studentId, payload)
    if (refreshMemberships) {
      await fetchMemberships()
    }
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
      const response = await paymentsApi.getAll(params)
      const pagination = parsePaginatedResponse(response)
      payments.value = pagination.items
      paymentsTotalCount.value = pagination.totalCount
      paymentsPage.value = pagination.page
      paymentsPageSize.value = pagination.pageSize
      paymentsServerPaginationEnabled.value = pagination.serverPaginationEnabled
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
    membershipsTotalCount,
    membershipsPage,
    membershipsPageSize,
    membershipsServerPaginationEnabled,
    paymentsTotalCount,
    paymentsPage,
    paymentsPageSize,
    paymentsServerPaginationEnabled,
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
