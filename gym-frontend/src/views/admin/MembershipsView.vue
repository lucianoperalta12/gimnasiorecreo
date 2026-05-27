<template>
  <div class="animate-fade-in">
    <div class="flex items-center justify-between mb-6">
      <div>
        <h1 class="page-title">Membresías</h1>
        <p class="page-subtitle">Alumnos, vencimientos y estado de acceso</p>
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
          <span class="hidden md:inline ml-1">Nueva Membresía</span>
        </AppButton>
      </div>
    </div>

    <div class="mb-4 flex flex-wrap gap-3">
      <input v-model="search" type="text" placeholder="Buscar alumno o plan..." class="input max-w-sm" />
      <AppSearchSelect
        v-model="filterEstado"
        :options="estadoOptions"
        placeholder="Todos los estados"
        class="w-full max-w-[180px]"
        @update:model-value="loadMemberships"
      />
      <AppSearchSelect
        v-if="authStore.hasRole('Superusuario')"
        v-model="selectedGymId"
        :options="gymOptions"
        placeholder="Todos los gimnasios"
        class="w-full max-w-xs"
        @update:model-value="loadMemberships"
      />
    </div>

    <LoadingSpinner v-if="store.loading" />

    <template v-else>
      <!-- Mobile List (< md) -->
      <div class="md:hidden space-y-3">
        <div v-for="m in paginatedMemberships" :key="m.id" class="p-4 rounded-2xl bg-dark-900/40 border border-dark-800/50 flex flex-col gap-3">
          <div class="flex items-center justify-between">
            <h3 class="font-semibold text-white">{{ m.alumnoNombreCompleto }}</h3>
            <span :class="membershipEstadoBadgeClass(m.estado)" class="text-[10px]">{{ m.estado }}</span>
          </div>
          <div class="space-y-1.5">
            <div class="flex items-center justify-between text-xs">
              <span class="text-dark-400">Plan:</span>
              <div class="text-right">
                <span class="text-primary-400 font-bold block">{{ m.planNombre }}</span>
                <span class="text-[10px] text-dark-400 font-medium block">
                  {{ m.paseLibre ? 'Pase Libre' : `${m.diasPorSemana} días/sem` }}
                </span>
              </div>
            </div>
            <div class="flex items-center justify-between text-xs">
              <span class="text-dark-400">Vencimiento:</span>
              <span class="text-dark-300">{{ formatDate(m.fechaVencimiento) }}</span>
            </div>
            <div class="flex items-center justify-between text-xs">
              <span class="text-dark-400">Acceso:</span>
              <span :class="accessStatusBadgeClass(m.estadoAcceso)">{{ m.estadoAcceso }}</span>
            </div>
          </div>
          <div class="flex items-center gap-2 mt-1">
            <button v-if="canRenew(m)" class="btn-secondary btn-sm flex-1 text-xs" @click="openRenewModal(m)">Renovar</button>
            <button v-if="m.estado === 'Activa'" class="btn-secondary btn-sm flex-1 text-xs text-red-400 flex items-center justify-center gap-1" @click="openCancelModal(m)">
              <svg class="w-3.5 h-3.5" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16" /></svg>
              Cancelar
            </button>
          </div>
        </div>
      </div>
      <!-- Desktop Table (md+) -->
      <div v-if="paginatedMemberships.length" class="hidden md:block table-container">
        <table class="table">
          <thead>
            <tr>
              <th @click="toggleSort('alumnoNombreCompleto')" class="cursor-pointer hover:text-white select-none transition-colors">
                Alumno
                <span v-if="sortBy === 'alumnoNombreCompleto'">{{ sortDesc ? '↓' : '↑' }}</span>
              </th>
              <th>Plan</th>
              <th @click="toggleSort('fechaInicio')" class="cursor-pointer hover:text-white select-none transition-colors">
                Inicio
                <span v-if="sortBy === 'fechaInicio'">{{ sortDesc ? '↓' : '↑' }}</span>
              </th>
              <th @click="toggleSort('fechaVencimiento')" class="cursor-pointer hover:text-white select-none transition-colors">
                Vencimiento
                <span v-if="sortBy === 'fechaVencimiento'">{{ sortDesc ? '↓' : '↑' }}</span>
              </th>
              <th>Estado</th>
              <th>Acceso</th>
              <th class="text-right">Acciones</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="m in paginatedMemberships" :key="m.id">
              <td class="font-medium text-white">{{ m.alumnoNombreCompleto }}</td>
              <td>
                <div class="flex flex-col">
                  <span>{{ m.planNombre }}</span>
                  <span class="text-[10px] text-dark-400 font-medium">
                    {{ m.paseLibre ? 'Pase Libre' : `${m.diasPorSemana} días/sem` }}
                  </span>
                </div>
              </td>
              <td>{{ formatDate(m.fechaInicio) }}</td>
              <td>{{ formatDate(m.fechaVencimiento) }}</td>
              <td><span :class="membershipEstadoBadgeClass(m.estado)">{{ m.estado }}</span></td>
              <td><span :class="accessStatusBadgeClass(m.estadoAcceso)">{{ m.estadoAcceso }}</span></td>
              <td class="text-right">
                <div class="flex items-center justify-end gap-2">
                  <button v-if="canRenew(m)" class="btn-ghost btn-sm" @click="openRenewModal(m)">Renovar</button>
                  <button v-if="m.estado === 'Activa'" class="btn-ghost btn-sm text-red-400 p-1.5" title="Cancelar Membresía" @click="openCancelModal(m)">
                    <svg class="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16" /></svg>
                  </button>
                </div>
              </td>
            </tr>
          </tbody>
        </table>
      </div>

      <!-- Pagination Controls -->
      <div v-if="filteredMemberships.length > pageSize" class="mt-6 flex items-center justify-between px-2">
        <div class="text-xs text-dark-500 font-medium">
          Mostrando {{ (currentPage - 1) * pageSize + 1 }} - {{ Math.min(currentPage * pageSize, filteredMemberships.length) }} 
          de {{ filteredMemberships.length }} registros
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

      <div v-if="filteredMemberships.length === 0" class="card text-center py-12">
        <p class="text-dark-400">No hay membresías que coincidan con los filtros</p>
      </div>
    </template>

    <AppModal v-model="showCreateModal" title="Nueva membresía" size="lg">
      <form class="space-y-4" @submit.prevent="handleCreate">
        <AppSearchSelect v-model="createForm.alumnoId" label="Alumno" placeholder="Buscar alumno..." :options="studentOptions" />
        <label class="label">Plan</label>
        <AppSearchSelect
          v-model.number="createForm.planId"
          :options="planOptions"
          placeholder="Seleccionar plan"
        />
        <AppInput v-model="createForm.fechaInicio" label="Fecha de inicio" type="date" />
        <textarea v-model="createForm.notas" rows="2" class="input" placeholder="Notas opcionales" />
      </form>
      <template #footer>
        <AppButton variant="secondary" @click="showCreateModal = false">Cancelar</AppButton>
        <div class="flex gap-2">
          <AppButton :loading="saving" @click="handleCreate(false)">Crear</AppButton>
          <AppButton :loading="saving" variant="primary" @click="handleCreate(true)">Crear y Pagar</AppButton>
        </div>
      </template>
    </AppModal>

    <AppModal v-model="showPaymentModal" title="Registrar pago">
      <form class="space-y-4" @submit.prevent="handlePaymentSubmit">
        <div class="grid grid-cols-2 gap-4">
          <AppInput v-model="paymentForm.alumnoNombre" label="Alumno" disabled />
          <AppInput v-model="paymentForm.planNombre" label="Plan" disabled />
        </div>
        <div class="grid grid-cols-2 gap-4">
          <AppInput v-model="paymentForm.monto" label="Precio" type="text" @input="onMontoInput" />
          <AppInput v-model="paymentForm.fechaPago" label="Fecha de pago" type="datetime-local" />
        </div>
        <div class="grid grid-cols-2 gap-4">
          <div>
            <label class="label">Método de pago</label>
            <AppSearchSelect
              v-model="paymentForm.metodoPago"
              :options="metodoPagoOptions"
              placeholder="Método de pago"
            />
          </div>
          <div>
            <label class="label">Estado</label>
            <AppSearchSelect
              v-model="paymentForm.estado"
              :options="paymentEstadoOptions"
              placeholder="Estado"
            />
          </div>
        </div>
        <textarea v-model="paymentForm.notas" rows="2" class="input" placeholder="Notas opcionales" />
      </form>
      <template #footer>
        <AppButton variant="secondary" @click="showPaymentModal = false">Cancelar</AppButton>
        <AppButton :loading="saving" @click="handlePaymentSubmit">Guardar Pago</AppButton>
      </template>
    </AppModal>

    <AppModal v-model="showRenewModal" title="Renovar membresía">
      <form class="space-y-4" @submit.prevent="handleRenew">
        <p class="text-sm text-dark-400">Alumno: <strong class="text-white">{{ renewingMembership?.alumnoNombreCompleto }}</strong></p>
        <label class="label">Plan</label>
        <AppSearchSelect
          v-model.number="renewForm.planId"
          :options="planOptions"
          placeholder="Seleccionar plan"
        />
        <AppInput v-model="renewForm.fechaInicio" label="Fecha de inicio (opcional)" type="date" />
        <textarea v-model="renewForm.notas" rows="2" class="input" placeholder="Notas" />
      </form>
      <template #footer>
        <AppButton variant="secondary" @click="showRenewModal = false">Cancelar</AppButton>
        <div class="flex gap-2">
          <AppButton :loading="saving" @click="handleRenew(false)">Renovar</AppButton>
          <AppButton :loading="saving" variant="primary" @click="handleRenew(true)">Renovar y Pagar</AppButton>
        </div>
      </template>
    </AppModal>

    <AppModal v-model="showCancelModal" title="Cancelar membresía" size="sm">
      <p class="text-dark-300 mb-3">Cancelar membresía de <strong class="text-white">{{ cancellingMembership?.alumnoNombreCompleto }}</strong></p>
      <textarea v-model="cancelMotivo" rows="2" class="input" placeholder="Motivo (opcional)" />
      <template #footer>
        <AppButton variant="secondary" @click="showCancelModal = false">Cerrar</AppButton>
        <AppButton variant="danger" :loading="saving" @click="handleCancel">Cancelar membresía</AppButton>
      </template>
    </AppModal>
  </div>
</template>

<script setup>
import { ref, reactive, computed, onMounted, nextTick } from 'vue'
import { useRouter } from 'vue-router'
import { useMembershipStore } from '@/stores/membership.store'
import { useUserStore } from '@/stores/user.store'
import { useAuthStore } from '@/stores/auth.store'
import { useNotification } from '@/composables/useNotification'
import { gymsApi } from '@/api/gyms.api'
import {
  MEMBERSHIP_ESTADOS,
  PAYMENT_ESTADOS,
  formatCurrency,
  formatDate,
  accessStatusBadgeClass,
  membershipEstadoBadgeClass
} from '@/constants/membershipStatus'
import AppButton from '@/components/ui/AppButton.vue'
import AppInput from '@/components/ui/AppInput.vue'
import AppModal from '@/components/ui/AppModal.vue'
import AppSearchSelect from '@/components/ui/AppSearchSelect.vue'
import LoadingSpinner from '@/components/shared/LoadingSpinner.vue'

const router = useRouter()
const store = useMembershipStore()
const userStore = useUserStore()
const authStore = useAuthStore()
const { success, error: showError } = useNotification()

const search = ref('')
const filterEstado = ref('Activa')
const selectedGymId = ref(null)
const gyms = ref([])
const saving = ref(false)
const showCreateModal = ref(false)
const showRenewModal = ref(false)
const showCancelModal = ref(false)
const showPaymentModal = ref(false)
const renewingMembership = ref(null)
const cancellingMembership = ref(null)
const cancelMotivo = ref('')

const sortBy = ref('fechaVencimiento')
const sortDesc = ref(true)

const pageSize = ref(15)
const currentPage = ref(1)

const createForm = reactive({ alumnoId: null, planId: 0, fechaInicio: new Date().toISOString().slice(0, 10), notas: '' })
const renewForm = reactive({ planId: 0, fechaInicio: '', notas: '' })

const paymentForm = reactive({
  membresiaId: 0,
  alumnoNombre: '',
  planNombre: '',
  monto: '',
  fechaPago: new Date(Date.now() - new Date().getTimezoneOffset() * 60000).toISOString().slice(0, 16),
  metodoPago: 'Efectivo',
  estado: 'Completado',
  notas: ''
})

const studentOptions = computed(() =>
  userStore.students.map(s => ({
    id: s.id,
    label: `${s.nombre} ${s.apellido}`.trim(),
    subLabel: s.email
  }))
)

const activePlans = computed(() => store.plans.filter(p => p.activo))

const estadoOptions = computed(() => {
  const list = [{ id: '', label: 'Todos los estados' }]
  MEMBERSHIP_ESTADOS.forEach(e => {
    list.push({ id: e, label: e })
  })
  return list
})

const gymOptions = computed(() => {
  const list = [{ id: null, label: 'Todos los gimnasios' }]
  if (gyms.value) {
    gyms.value.forEach(g => {
      list.push({ id: g.id, label: g.nombre })
    })
  }
  return list
})

const planOptions = computed(() => {
  return activePlans.value.map(p => ({
    id: p.id,
    label: `${p.nombre} (${p.duracionDias} días - ${formatCurrency(p.precio, p.moneda)})`
  }))
})

const metodoPagoOptions = [
  { id: 'Efectivo', label: 'Efectivo' },
  { id: 'Transferencia', label: 'Transferencia' }
]

const paymentEstadoOptions = computed(() => {
  return PAYMENT_ESTADOS.map(e => ({ id: e, label: e }))
})

const filteredMemberships = computed(() => {
  const q = search.value.toLowerCase()
  let result = store.memberships.filter(m =>
    m.alumnoNombreCompleto.toLowerCase().includes(q) ||
    m.planNombre.toLowerCase().includes(q)
  )

  result.sort((a, b) => {
    let valA = a[sortBy.value]
    let valB = b[sortBy.value]

    if (valA == null) valA = ''
    if (valB == null) valB = ''

    if (valA < valB) return sortDesc.value ? 1 : -1
    if (valA > valB) return sortDesc.value ? -1 : 1
    return 0
  })

  return result
})

function toggleSort(column) {
  if (sortBy.value === column) {
    sortDesc.value = !sortDesc.value
  } else {
    sortBy.value = column
    sortDesc.value = false
  }
}

const totalPages = computed(() => Math.ceil(filteredMemberships.value.length / pageSize.value))

const paginatedMemberships = computed(() => {
  const start = (currentPage.value - 1) * pageSize.value
  const end = start + pageSize.value
  return filteredMemberships.value.slice(start, end)
})

function canRenew(m) {
  if (m.estado === 'Activa') return true
  if (m.estado !== 'Vencida') return false

  // Solo permitir renovar la última vencida si NO hay ninguna activa para este alumno
  const studentMemberships = store.memberships.filter(ms => ms.alumnoId === m.alumnoId)
  const hasActive = studentMemberships.some(ms => ms.estado === 'Activa')
  if (hasActive) return false

  const latestVencida = studentMemberships
    .filter(ms => ms.estado === 'Vencida')
    .sort((a, b) => new Date(b.fechaVencimiento) - new Date(a.fechaVencimiento))[0]

  return latestVencida && latestVencida.id === m.id
}

function openCreateModal() {
  createForm.alumnoId = null
  createForm.planId = activePlans.value[0]?.id || 0
  createForm.fechaInicio = new Date().toISOString().slice(0, 10)
  createForm.notas = ''
  showCreateModal.value = true
}

function openRenewModal(m) {
  renewingMembership.value = m
  renewForm.planId = activePlans.value[0]?.id || 0
  renewForm.fechaInicio = new Date().toISOString().slice(0, 10)
  renewForm.notas = ''
  showRenewModal.value = true
}

function openCancelModal(m) {
  cancellingMembership.value = m
  cancelMotivo.value = ''
  showCancelModal.value = true
}

async function loadMemberships() {
  const params = {}
  if (filterEstado.value) params.estado = filterEstado.value
  if (selectedGymId.value) params.gymId = selectedGymId.value
  await store.fetchMemberships(params)
  currentPage.value = 1 // Reset to first page on reload
}

async function handleCreate(andPay = false) {
  if (!createForm.alumnoId) return showError('Seleccioná un alumno')
  saving.value = true
  try {
    const created = await store.createMembership({
      alumnoId: createForm.alumnoId,
      planId: createForm.planId,
      fechaInicio: createForm.fechaInicio,
      notas: createForm.notas || null
    })
    success('Membresía creada')
    showCreateModal.value = false
    await loadMemberships()

    if (andPay && created) {
      paymentForm.membresiaId = created.id
      paymentForm.alumnoNombre = `${created.alumnoNombre} ${created.alumnoApellido}`.trim()
      paymentForm.planNombre = created.planNombre
      paymentForm.monto = formatNumberWithDots(created.planPrecio)
      paymentForm.fechaPago = new Date(Date.now() - new Date().getTimezoneOffset() * 60000).toISOString().slice(0, 16)
      paymentForm.metodoPago = 'Efectivo'
      paymentForm.estado = 'Completado'
      paymentForm.notas = ''
      showPaymentModal.value = true
    }
  } catch (err) {
    showError(err.response?.data?.error || 'Error al crear')
  } finally {
    saving.value = false
  }
}

async function handlePaymentSubmit() {
  saving.value = true
  try {
    await store.createPayment({
      membresiaId: paymentForm.membresiaId,
      monto: parseFormattedNumber(paymentForm.monto),
      fechaPago: paymentForm.fechaPago,
      metodoPago: paymentForm.metodoPago,
      estado: paymentForm.estado,
      notas: paymentForm.notas || null
    })
    success('Pago registrado')
    showPaymentModal.value = false
  } catch (err) {
    showError(err.response?.data?.error || 'Error al guardar pago')
  } finally {
    saving.value = false
  }
}

async function handleRenew(andPay = false) {
  saving.value = true
  try {
    const renewed = await store.renewMembership(renewingMembership.value.alumnoId, {
      planId: renewForm.planId,
      fechaInicio: renewForm.fechaInicio || null,
      notas: renewForm.notas || null
    })
    success('Membresía renovada')
    showRenewModal.value = false
    await loadMemberships()

    if (andPay && renewed) {
      paymentForm.membresiaId = renewed.id
      paymentForm.alumnoNombre = `${renewed.alumnoNombre} ${renewed.alumnoApellido}`.trim()
      paymentForm.planNombre = renewed.planNombre
      paymentForm.monto = formatNumberWithDots(renewed.planPrecio)
      paymentForm.fechaPago = new Date(Date.now() - new Date().getTimezoneOffset() * 60000).toISOString().slice(0, 16)
      paymentForm.metodoPago = 'Efectivo'
      paymentForm.estado = 'Completado'
      paymentForm.notas = ''
      showPaymentModal.value = true
    }
  } catch (err) {
    showError(err.response?.data?.error || 'Error al renovar')
  } finally {
    saving.value = false
  }
}

async function handleCancel() {
  saving.value = true
  try {
    await store.cancelMembership(cancellingMembership.value.id, { motivo: cancelMotivo.value || null })
    success('Membresía cancelada')
    showCancelModal.value = false
    await loadMemberships()
  } catch (err) {
    showError(err.response?.data?.error || 'Error al cancelar')
  } finally {
    saving.value = false
  }
}

onMounted(async () => {
  if (authStore.hasRole('Superusuario')) {
    try { const { data } = await gymsApi.getAll(); gyms.value = data } catch { /* noop */ }
  }
  await Promise.all([
    userStore.fetchStudents(),
    store.fetchPlans(selectedGymId.value || undefined),
    loadMemberships()
  ])
})

function formatNumberWithDots(val) {
  if (val === null || val === undefined || val === '') return ''
  let str = String(val).replace(/\./g, '')
  str = str.replace(/[^0-9,]/g, '')
  const parts = str.split(',')
  parts[0] = parts[0].replace(/\B(?=(\d{3})+(?!\d))/g, '.')
  if (parts.length > 2) {
    return parts[0] + ',' + parts.slice(1).join('')
  }
  return parts.join(',')
}

function parseFormattedNumber(val) {
  if (val === null || val === undefined || val === '') return 0
  const clean = String(val).replace(/\./g, '').replace(/,/g, '.')
  const num = Number(clean)
  return isNaN(num) ? 0 : num
}

function onMontoInput(event) {
  const input = event.target
  const value = input.value
  const formatted = formatNumberWithDots(value)
  
  const selectionStart = input.selectionStart
  const oldLength = value.length
  
  paymentForm.monto = formatted
  input.value = formatted
  
  nextTick(() => {
    const newLength = formatted.length
    const diff = newLength - oldLength
    const newCursorPos = selectionStart + diff
    input.setSelectionRange(newCursorPos, newCursorPos)
  })
}
</script>
