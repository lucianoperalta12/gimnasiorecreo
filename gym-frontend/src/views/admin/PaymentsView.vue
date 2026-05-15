<template>
  <div class="animate-fade-in">
    <div class="flex items-center justify-between mb-6">
      <div>
        <h1 class="page-title">Pagos</h1>
        <p class="page-subtitle">Registro de pagos de membresías</p>
      </div>
      <div class="flex items-center gap-3">
        <AppButton 
          variant="secondary" 
          @click="router.push('/dashboard')"
          class="md:px-4 px-2.5 !rounded-xl md:!rounded-lg"
        >
          <svg class="w-5 h-5 md:w-3 md:h-3" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="3">
            <path stroke-linecap="round" stroke-linejoin="round" d="M10.5 19.5L3 12m0 0l7.5-7.5M3 12h18" />
          </svg>
          <span class="hidden md:inline ml-1 text-[10px] font-black uppercase tracking-[0.2em]">Volver</span>
        </AppButton>
        <AppButton variant="primary" @click="openCreateModal" class="md:px-4 px-2.5 !rounded-xl md:!rounded-lg">
          <svg class="w-5 h-5 md:w-4 md:h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2.5">
            <path stroke-linecap="round" stroke-linejoin="round" d="M12 4.5v15m7.5-7.5h-15" />
          </svg>
          <span class="hidden md:inline ml-1">Registrar Pago</span>
        </AppButton>
      </div>
    </div>

    <div class="mb-4 flex flex-wrap gap-3 items-center">
      <input v-model="search" type="text" placeholder="Buscar alumno..." class="input max-w-sm" />
      <div class="flex items-center gap-2">
        <label class="text-xs text-dark-400 font-medium">Desde:</label>
        <input v-model="dateFrom" type="date" class="input max-w-[150px] text-sm" />
      </div>
      <div class="flex items-center gap-2">
        <label class="text-xs text-dark-400 font-medium">Hasta:</label>
        <input v-model="dateTo" type="date" class="input max-w-[150px] text-sm" />
      </div>
      <select v-if="authStore.hasRole('Superusuario')" v-model="selectedGymId" class="input max-w-xs" @change="loadData">
        <option :value="null">Todos los gimnasios</option>
        <option v-for="gym in gyms" :key="gym.id" :value="gym.id">{{ gym.nombre }}</option>
      </select>
    </div>

    <LoadingSpinner v-if="store.loading" />

    <template v-else>
      <!-- Mobile List (< md) -->
      <div v-if="filteredPayments.length" class="md:hidden space-y-3">
        <div v-for="p in paginatedPayments" :key="p.id" class="p-4 rounded-2xl bg-dark-900/40 border border-dark-800/50 flex flex-col gap-3">
          <div class="flex items-center justify-between">
            <h3 class="font-semibold text-white">{{ p.alumnoNombreCompleto }}</h3>
            <span :class="membershipEstadoBadgeClass(p.estado)" class="text-[10px]">{{ p.estado }}</span>
          </div>
          <div class="space-y-1.5">
            <div class="flex items-center justify-between text-xs">
              <span class="text-dark-400">Monto:</span>
              <span class="text-primary-400 font-bold">{{ formatCurrency(p.monto) }}</span>
            </div>
            <div class="flex items-center justify-between text-xs">
              <span class="text-dark-400">Fecha:</span>
              <span class="text-dark-300">{{ formatDateTime(p.fechaPago) }}</span>
            </div>
            <div class="flex items-center justify-between text-xs">
              <span class="text-dark-400">Método:</span>
              <span class="text-dark-300">{{ p.metodoPago || '—' }}</span>
            </div>
          </div>
          <div class="flex items-center gap-2 mt-1">
            <button class="btn-secondary btn-sm flex-1 text-xs" @click="openEditModal(p)">Editar</button>
            <button class="btn-secondary btn-sm flex-1 text-xs text-red-400 flex items-center justify-center gap-1" @click="confirmDelete(p)">
              <svg class="w-3.5 h-3.5" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16" /></svg>
              Eliminar
            </button>
          </div>
        </div>
      </div>

      <!-- Desktop Table (md+) -->
      <div v-if="filteredPayments.length" class="hidden md:block table-container">
        <table class="table">
          <thead>
            <tr>
              <th>Alumno</th>
              <th>Monto</th>
              <th>Fecha</th>
              <th>Estado</th>
              <th>Método</th>
              <th class="text-right">Acciones</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="p in paginatedPayments" :key="p.id">
              <td class="text-white">{{ p.alumnoNombreCompleto }}</td>
              <td>{{ formatCurrency(p.monto) }}</td>
              <td>{{ formatDateTime(p.fechaPago) }}</td>
              <td><span :class="membershipEstadoBadgeClass(p.estado)">{{ p.estado }}</span></td>
              <td>{{ p.metodoPago || '—' }}</td>
              <td class="text-right">
                <div class="flex items-center justify-end gap-2">
                  <button class="btn-ghost btn-sm" @click="openEditModal(p)">Editar</button>
                  <button class="btn-ghost btn-sm text-red-400 p-1.5" title="Eliminar Pago" @click="confirmDelete(p)">
                    <svg class="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16" /></svg>
                  </button>
                </div>
              </td>
            </tr>
          </tbody>
        </table>
      </div>

      <!-- Pagination Controls -->
      <div v-if="filteredPayments.length > pageSize" class="mt-6 flex items-center justify-between px-2">
        <div class="text-xs text-dark-500 font-medium">
          Mostrando {{ (currentPage - 1) * pageSize + 1 }} - {{ Math.min(currentPage * pageSize, filteredPayments.length) }} 
          de {{ filteredPayments.length }} registros
        </div>
        <div class="flex items-center gap-2">
          <button 
            class="btn-secondary !p-2 !rounded-lg" 
            :disabled="currentPage === 1"
            @click="currentPage--"
          >
            <svg class="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M15.75 19.5L8.25 12l7.5-7.5" /></svg>
          </button>
          
          <div class="flex items-center gap-1">
            <button 
              v-for="p in totalPages" 
              :key="p"
              @click="currentPage = p"
              class="w-8 h-8 rounded-lg text-xs font-black transition-all"
              :class="currentPage === p ? 'bg-primary-600 text-white' : 'bg-dark-800 text-dark-400 hover:bg-dark-700'"
            >
              {{ p }}
            </button>
          </div>

          <button 
            class="btn-secondary !p-2 !rounded-lg" 
            :disabled="currentPage === totalPages"
            @click="currentPage++"
          >
            <svg class="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M8.25 4.5l7.5 7.5-7.5 7.5" /></svg>
          </button>
        </div>
      </div>

      <div v-else-if="!filteredPayments.length" class="card text-center py-12">
        <p class="text-dark-400">No hay pagos registrados</p>
      </div>
    </template>

    <AppModal v-model="showModal" :title="editingPayment ? 'Editar pago' : 'Registrar pago'">
      <form class="space-y-4" @submit.prevent="handleSubmit">
        <AppSearchSelect v-model="form.membresiaId" label="Alumno (Membresía Activa)" placeholder="Buscar alumno..." :options="studentOptions" />
        
        <div class="grid grid-cols-2 gap-4">
          <AppInput v-model.number="form.monto" label="Monto" type="number" min="0" step="0.01" />
          <AppInput v-model="form.fechaPago" label="Fecha de pago" type="datetime-local" />
        </div>
        
        <div class="grid grid-cols-2 gap-4">
          <div>
            <label class="label">Método de pago</label>
            <select v-model="form.metodoPago" class="input">
              <option value="Efectivo">Efectivo</option>
              <option value="Transferencia">Transferencia</option>
            </select>
          </div>
          <div>
            <label class="label">Estado</label>
            <select v-model="form.estado" class="input">
              <option v-for="e in PAYMENT_ESTADOS" :key="e" :value="e">{{ e }}</option>
            </select>
          </div>
        </div>
        
        <textarea v-model="form.notas" rows="2" class="input" placeholder="Notas" />
      </form>
      <template #footer>
        <AppButton variant="secondary" @click="showModal = false">Cancelar</AppButton>
        <AppButton :loading="saving" @click="handleSubmit">Guardar</AppButton>
      </template>
    </AppModal>

    <AppModal v-model="showDeleteModal" title="Eliminar pago" size="sm">
      <p class="text-dark-300">¿Eliminar este pago?</p>
      <template #footer>
        <AppButton variant="secondary" @click="showDeleteModal = false">Cancelar</AppButton>
        <AppButton variant="danger" :loading="saving" @click="handleDelete">Eliminar</AppButton>
      </template>
    </AppModal>
  </div>
</template>

<script setup>
import { ref, reactive, computed, watch, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useMembershipStore } from '@/stores/membership.store'
import { useAuthStore } from '@/stores/auth.store'
import { useNotification } from '@/composables/useNotification'
import { gymsApi } from '@/api/gyms.api'
import { PAYMENT_ESTADOS, formatCurrency, formatDateTime, formatDate, membershipEstadoBadgeClass } from '@/constants/membershipStatus'
import AppButton from '@/components/ui/AppButton.vue'
import AppInput from '@/components/ui/AppInput.vue'
import AppModal from '@/components/ui/AppModal.vue'
import AppSearchSelect from '@/components/ui/AppSearchSelect.vue'
import LoadingSpinner from '@/components/shared/LoadingSpinner.vue'

const router = useRouter()
const store = useMembershipStore()
const authStore = useAuthStore()
const { success, error: showError } = useNotification()

const selectedGymId = ref(null)
const gyms = ref([])
const showModal = ref(false)
const showDeleteModal = ref(false)
const saving = ref(false)
const editingPayment = ref(null)
const deletingPayment = ref(null)

const search = ref('')
const dateFrom = ref(new Date(new Date().setMonth(new Date().getMonth() - 1)).toISOString().slice(0, 10))
const dateTo = ref(new Date().toISOString().slice(0, 10))

const pageSize = ref(15)
const currentPage = ref(1)

const form = reactive({
  membresiaId: null,
  monto: 0,
  fechaPago: new Date(Date.now() - new Date().getTimezoneOffset() * 60000).toISOString().slice(0, 16),
  metodoPago: 'Efectivo',
  estado: 'Completado',
  notas: ''
})

const filteredPayments = computed(() => {
  const q = search.value.toLowerCase()
  
  let from = 0
  if (dateFrom.value) {
    const [y, m, d] = dateFrom.value.split('-')
    from = new Date(y, m - 1, d, 0, 0, 0).getTime()
  }
  
  let to = Infinity
  if (dateTo.value) {
    const [y, m, d] = dateTo.value.split('-')
    to = new Date(y, m - 1, d, 23, 59, 59, 999).getTime()
  }

  return store.payments.filter(p => {
    const d = new Date(p.fechaPago).getTime()
    return d >= from && d <= to && p.alumnoNombreCompleto.toLowerCase().includes(q)
  })
})

const totalPages = computed(() => Math.ceil(filteredPayments.value.length / pageSize.value) || 1)

const paginatedPayments = computed(() => {
  const start = (currentPage.value - 1) * pageSize.value
  return filteredPayments.value.slice(start, start + pageSize.value)
})

watch([search, dateFrom, dateTo, selectedGymId], () => {
  currentPage.value = 1
})

const studentOptions = computed(() => {
  return store.memberships
    .filter(m => m.estado === 'Activa')
    .map(m => ({
      id: m.id,
      label: m.alumnoNombreCompleto,
      subLabel: m.planNombre
    }))
})

watch(() => form.membresiaId, (newId) => {
  if (newId && !editingPayment.value) {
    const membership = store.memberships.find(m => m.id === newId)
    if (membership) {
      const plan = store.plans.find(p => p.nombre === membership.planNombre)
      if (plan) {
        form.monto = plan.precio
      }
    }
  }
})

function openCreateModal() {
  editingPayment.value = null
  Object.assign(form, {
    membresiaId: null,
    monto: 0,
    fechaPago: new Date(Date.now() - new Date().getTimezoneOffset() * 60000).toISOString().slice(0, 16),
    metodoPago: 'Efectivo',
    estado: 'Completado',
    notas: ''
  })
  showModal.value = true
}

function openEditModal(p) {
  editingPayment.value = p
  Object.assign(form, {
    membresiaId: p.membresiaId,
    monto: p.monto,
    fechaPago: p.fechaPago ? new Date(new Date(p.fechaPago).getTime() - new Date().getTimezoneOffset() * 60000).toISOString().slice(0, 16) : '',
    metodoPago: p.metodoPago || 'Efectivo',
    estado: p.estado,
    notas: p.notas || ''
  })
  showModal.value = true
}

function confirmDelete(p) {
  deletingPayment.value = p
  showDeleteModal.value = true
}

async function handleSubmit() {
  saving.value = true
  try {
    const payload = { ...form, monto: Number(form.monto), membresiaId: Number(form.membresiaId) }
    if (editingPayment.value) {
      await store.updatePayment(editingPayment.value.id, payload)
      success('Pago actualizado')
    } else {
      await store.createPayment(payload)
      success('Pago registrado')
    }
    showModal.value = false
    await loadData()
  } catch (err) {
    showError(err.response?.data?.error || 'Error al guardar')
  } finally {
    saving.value = false
  }
}

async function handleDelete() {
  saving.value = true
  try {
    await store.deletePayment(deletingPayment.value.id)
    success('Pago eliminado')
    showDeleteModal.value = false
  } catch (err) {
    showError(err.response?.data?.error || 'Error al eliminar')
  } finally {
    saving.value = false
  }
}

async function loadData() {
  const params = selectedGymId.value ? { gymId: selectedGymId.value } : {}
  await Promise.all([
    store.fetchPayments(params),
    store.fetchMemberships({ estado: 'Activa', ...params })
  ])
}

onMounted(async () => {
  if (authStore.hasRole('Superusuario')) {
    try { const { data } = await gymsApi.getAll(); gyms.value = data } catch { /* noop */ }
  }
  await store.fetchPlans()
  await loadData()
})
</script>
