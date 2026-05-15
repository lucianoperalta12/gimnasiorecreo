const fs = require('fs')
const path = require('path')

const d = 'div'
const adminDir = path.join(__dirname, '../src/views/admin')
const studentsDir = path.join(__dirname, '../src/views/students')

const membershipsView = `<template>
  <${d} class="animate-fade-in">
    <${d} class="flex items-center justify-between mb-6">
      <${d}>
        <h1 class="page-title">Membresías</h1>
        <p class="page-subtitle">Alumnos, vencimientos y estado de acceso</p>
      </${d}>
      <${d} class="flex items-center gap-3">
        <AppButton variant="secondary" @click="router.push('/dashboard')">
          <span class="hidden md:inline ml-1 text-[10px] font-black uppercase tracking-[0.2em]">Volver</span>
        </AppButton>
        <AppButton variant="primary" @click="openCreateModal">
          <span class="hidden md:inline ml-1">Nueva membresía</span>
        </AppButton>
      </${d}>
    </${d}>

    <${d} class="mb-4 flex flex-wrap gap-3">
      <input v-model="search" type="text" placeholder="Buscar alumno o plan..." class="input max-w-sm" />
      <select v-model="filterEstado" class="input max-w-[180px]" @change="loadMemberships">
        <option value="">Todos los estados</option>
        <option v-for="e in MEMBERSHIP_ESTADOS" :key="e" :value="e">{{ e }}</option>
      </select>
      <select v-if="authStore.hasRole('Superusuario')" v-model="selectedGymId" class="input max-w-xs" @change="loadMemberships">
        <option :value="null">Todos los gimnasios</option>
        <option v-for="gym in gyms" :key="gym.id" :value="gym.id">{{ gym.nombre }}</option>
      </select>
    </${d}>

    <LoadingSpinner v-if="store.loading" />

    <template v-else>
      <${d} v-if="filteredMemberships.length" class="hidden md:block table-container">
        <table class="table">
          <thead>
            <tr>
              <th>Alumno</th>
              <th>Plan</th>
              <th>Inicio</th>
              <th>Vencimiento</th>
              <th>Estado</th>
              <th>Acceso</th>
              <th class="text-right">Acciones</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="m in filteredMemberships" :key="m.id">
              <td class="font-medium text-white">{{ m.alumnoNombreCompleto }}</td>
              <td>{{ m.planNombre }}</td>
              <td>{{ formatDate(m.fechaInicio) }}</td>
              <td>{{ formatDate(m.fechaVencimiento) }}</td>
              <td><span :class="membershipEstadoBadgeClass(m.estado)">{{ m.estado }}</span></td>
              <td><span :class="accessStatusBadgeClass(m.estadoAcceso)">{{ m.estadoAcceso }}</span></td>
              <td class="text-right">
                <button v-if="m.estado === 'Activa'" class="btn-ghost btn-sm" @click="openRenewModal(m)">Renovar</button>
                <button v-if="m.estado === 'Activa'" class="btn-ghost btn-sm text-red-400" @click="openCancelModal(m)">Cancelar</button>
              </td>
            </tr>
          </tbody>
        </table>
      </${d}>

      <${d} v-else class="card text-center py-12">
        <p class="text-dark-400">No hay membresías registradas</p>
      </${d}>
    </template>

    <AppModal v-model="showCreateModal" title="Nueva membresía" size="lg">
      <form class="space-y-4" @submit.prevent="handleCreate">
        <AppSearchSelect v-model="createForm.alumnoId" label="Alumno" placeholder="Buscar alumno..." :options="studentOptions" />
        <label class="label">Plan</label>
        <select v-model.number="createForm.planId" class="input" required>
          <option :value="0" disabled>Seleccionar plan</option>
          <option v-for="p in activePlans" :key="p.id" :value="p.id">{{ p.nombre }} ({{ p.duracionDias }} días - {{ formatCurrency(p.precio) }})</option>
        </select>
        <AppInput v-model="createForm.fechaInicio" label="Fecha de inicio" type="date" />
        <textarea v-model="createForm.notas" rows="2" class="input" placeholder="Notas opcionales" />
      </form>
      <template #footer>
        <AppButton variant="secondary" @click="showCreateModal = false">Cancelar</AppButton>
        <AppButton :loading="saving" @click="handleCreate">Crear</AppButton>
      </template>
    </AppModal>

    <AppModal v-model="showRenewModal" title="Renovar membresía">
      <form class="space-y-4" @submit.prevent="handleRenew">
        <p class="text-sm text-dark-400">Alumno: <strong class="text-white">{{ renewingMembership?.alumnoNombreCompleto }}</strong></p>
        <label class="label">Plan</label>
        <select v-model.number="renewForm.planId" class="input" required>
          <option v-for="p in activePlans" :key="p.id" :value="p.id">{{ p.nombre }}</option>
        </select>
        <AppInput v-model="renewForm.fechaInicio" label="Fecha de inicio (opcional)" type="date" />
        <textarea v-model="renewForm.notas" rows="2" class="input" placeholder="Notas" />
      </form>
      <template #footer>
        <AppButton variant="secondary" @click="showRenewModal = false">Cancelar</AppButton>
        <AppButton :loading="saving" @click="handleRenew">Renovar</AppButton>
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
  </${d}>
</template>

<script setup>
import { ref, reactive, computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useMembershipStore } from '@/stores/membership.store'
import { useUserStore } from '@/stores/user.store'
import { useAuthStore } from '@/stores/auth.store'
import { useNotification } from '@/composables/useNotification'
import { gymsApi } from '@/api/gyms.api'
import {
  MEMBERSHIP_ESTADOS,
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
const filterEstado = ref('')
const selectedGymId = ref(null)
const gyms = ref([])
const saving = ref(false)
const showCreateModal = ref(false)
const showRenewModal = ref(false)
const showCancelModal = ref(false)
const renewingMembership = ref(null)
const cancellingMembership = ref(null)
const cancelMotivo = ref('')

const createForm = reactive({ alumnoId: null, planId: 0, fechaInicio: new Date().toISOString().slice(0, 10), notas: '' })
const renewForm = reactive({ planId: 0, fechaInicio: '', notas: '' })

const studentOptions = computed(() =>
  userStore.students.map(s => ({
    id: s.id,
    label: \`\${s.nombre} \${s.apellido}\`.trim(),
    subLabel: s.email
  }))
)

const activePlans = computed(() => store.plans.filter(p => p.activo))

const filteredMemberships = computed(() => {
  const q = search.value.toLowerCase()
  return store.memberships.filter(m =>
    m.alumnoNombreCompleto.toLowerCase().includes(q) ||
    m.planNombre.toLowerCase().includes(q)
  )
})

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
  renewForm.fechaInicio = ''
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
}

async function handleCreate() {
  if (!createForm.alumnoId) return showError('Seleccioná un alumno')
  saving.value = true
  try {
    await store.createMembership({
      alumnoId: createForm.alumnoId,
      planId: createForm.planId,
      fechaInicio: createForm.fechaInicio,
      notas: createForm.notas || null
    })
    success('Membresía creada')
    showCreateModal.value = false
    await loadMemberships()
  } catch (err) {
    showError(err.response?.data?.error || 'Error al crear')
  } finally {
    saving.value = false
  }
}

async function handleRenew() {
  saving.value = true
  try {
    await store.renewMembership(renewingMembership.value.alumnoId, {
      planId: renewForm.planId,
      fechaInicio: renewForm.fechaInicio || null,
      notas: renewForm.notas || null
    })
    success('Membresía renovada')
    showRenewModal.value = false
    await loadMemberships()
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
</script>
`

const paymentsView = `<template>
  <${d} class="animate-fade-in">
    <${d} class="flex items-center justify-between mb-6">
      <${d}>
        <h1 class="page-title">Pagos</h1>
        <p class="page-subtitle">Registro de pagos de membresías</p>
      </${d}>
      <${d} class="flex items-center gap-3">
        <AppButton variant="secondary" @click="router.push('/dashboard')">
          <span class="hidden md:inline ml-1 text-[10px] font-black uppercase tracking-[0.2em]">Volver</span>
        </AppButton>
        <AppButton variant="primary" @click="openCreateModal">
          <span class="hidden md:inline ml-1">Registrar pago</span>
        </AppButton>
      </${d}>
    </${d}>

    <LoadingSpinner v-if="store.loading" />

    <template v-else>
      <${d} v-if="store.payments.length" class="hidden md:block table-container">
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
            <tr v-for="p in store.payments" :key="p.id">
              <td class="text-white">{{ p.alumnoNombreCompleto }}</td>
              <td>{{ formatCurrency(p.monto) }}</td>
              <td>{{ formatDate(p.fechaPago) }}</td>
              <td><span :class="membershipEstadoBadgeClass(p.estado)">{{ p.estado }}</span></td>
              <td>{{ p.metodoPago || '—' }}</td>
              <td class="text-right">
                <button class="btn-ghost btn-sm" @click="openEditModal(p)">Editar</button>
                <button class="btn-ghost btn-sm text-red-400" @click="confirmDelete(p)">Eliminar</button>
              </td>
            </tr>
          </tbody>
        </table>
      </${d}>
      <${d} v-else class="card text-center py-12">
        <p class="text-dark-400">No hay pagos registrados</p>
      </${d}>
    </template>

    <AppModal v-model="showModal" :title="editingPayment ? 'Editar pago' : 'Registrar pago'">
      <form class="space-y-4" @submit.prevent="handleSubmit">
        <label class="label">Membresía ID</label>
        <input v-model.number="form.membresiaId" type="number" class="input" required />
        <AppInput v-model.number="form.monto" label="Monto" type="number" min="0" step="0.01" />
        <AppInput v-model="form.fechaPago" label="Fecha de pago" type="date" />
        <AppInput v-model="form.metodoPago" label="Método de pago" placeholder="Efectivo, transferencia..." />
        <label class="label">Estado</label>
        <select v-model="form.estado" class="input">
          <option v-for="e in PAYMENT_ESTADOS" :key="e" :value="e">{{ e }}</option>
        </select>
        <AppInput v-model="form.referencia" label="Referencia (opcional)" />
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
  </${d}>
</template>

<script setup>
import { ref, reactive, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useMembershipStore } from '@/stores/membership.store'
import { useNotification } from '@/composables/useNotification'
import { PAYMENT_ESTADOS, formatCurrency, formatDate, membershipEstadoBadgeClass } from '@/constants/membershipStatus'
import AppButton from '@/components/ui/AppButton.vue'
import AppInput from '@/components/ui/AppInput.vue'
import AppModal from '@/components/ui/AppModal.vue'
import LoadingSpinner from '@/components/shared/LoadingSpinner.vue'

const router = useRouter()
const store = useMembershipStore()
const { success, error: showError } = useNotification()

const showModal = ref(false)
const showDeleteModal = ref(false)
const saving = ref(false)
const editingPayment = ref(null)
const deletingPayment = ref(null)

const form = reactive({
  membresiaId: 0,
  monto: 0,
  fechaPago: new Date().toISOString().slice(0, 10),
  metodoPago: '',
  estado: 'Completado',
  referencia: '',
  notas: ''
})

function openCreateModal() {
  editingPayment.value = null
  Object.assign(form, {
    membresiaId: 0,
    monto: 0,
    fechaPago: new Date().toISOString().slice(0, 10),
    metodoPago: '',
    estado: 'Completado',
    referencia: '',
    notas: ''
  })
  showModal.value = true
}

function openEditModal(p) {
  editingPayment.value = p
  Object.assign(form, {
    membresiaId: p.membresiaId,
    monto: p.monto,
    fechaPago: p.fechaPago?.slice(0, 10),
    metodoPago: p.metodoPago || '',
    estado: p.estado,
    referencia: p.referencia || '',
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
    await store.fetchPayments()
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

onMounted(() => store.fetchPayments())
</script>
`

const myMembershipView = `<template>
  <${d} class="animate-fade-in">
    <${d} class="mb-6">
      <h1 class="page-title">Mi membresía</h1>
      <p class="page-subtitle">Estado de acceso al gimnasio</p>
    </${d}>

    <LoadingSpinner v-if="store.loading" />

    <template v-else-if="access">
      <${d} class="card p-6 md:p-8 max-w-2xl">
        <${d} class="flex items-start justify-between gap-4 mb-6">
          <${d}>
            <p class="text-xs uppercase tracking-widest text-dark-500 font-bold mb-1">Estado de acceso</p>
            <span :class="accessStatusBadgeClass(access.estadoAcceso)" class="text-base px-3 py-1">{{ access.estadoAcceso }}</span>
          </${d}>
          <${d} class="text-4xl">💳</${d}>
        </${d}>

        <${d} class="grid grid-cols-1 sm:grid-cols-2 gap-4">
          <${d} class="rounded-xl bg-dark-900/60 border border-dark-800 p-4">
            <p class="text-[10px] uppercase tracking-wider text-dark-500 font-bold">Plan</p>
            <p class="text-lg font-bold text-white mt-1">{{ access.planNombre || 'Sin plan activo' }}</p>
          </${d}>
          <${d} class="rounded-xl bg-dark-900/60 border border-dark-800 p-4">
            <p class="text-[10px] uppercase tracking-wider text-dark-500 font-bold">Vencimiento</p>
            <p class="text-lg font-bold text-white mt-1">{{ formatDate(access.fechaVencimiento) }}</p>
          </${d}>
          <${d} v-if="access.diasRestantes != null" class="rounded-xl bg-dark-900/60 border border-dark-800 p-4 sm:col-span-2">
            <p class="text-[10px] uppercase tracking-wider text-dark-500 font-bold">Días restantes</p>
            <p class="text-2xl font-black text-primary-400 mt-1">{{ access.diasRestantes }}</p>
          </${d}>
        </${d}>

        <p v-if="access.estadoAcceso === 'Sin membresía'" class="text-sm text-dark-400 mt-6">
          Contactá a la administración del gimnasio para activar tu membresía.
        </p>
        <p v-else-if="access.estadoAcceso === 'Moroso'" class="text-sm text-amber-400/90 mt-6">
          Tenés pagos pendientes. Regularizá tu situación en recepción.
        </p>
      </${d}>
    </template>
  </${d}>
</template>

<script setup>
import { computed, onMounted } from 'vue'
import { useMembershipStore } from '@/stores/membership.store'
import { accessStatusBadgeClass, formatDate } from '@/constants/membershipStatus'
import LoadingSpinner from '@/components/shared/LoadingSpinner.vue'

const store = useMembershipStore()
const access = computed(() => store.myAccess)

onMounted(() => store.fetchMyAccess())
</script>
`

fs.writeFileSync(path.join(adminDir, 'MembershipsView.vue'), membershipsView, 'utf8')
fs.writeFileSync(path.join(adminDir, 'PaymentsView.vue'), paymentsView, 'utf8')
fs.writeFileSync(path.join(studentsDir, 'MyMembershipView.vue'), myMembershipView, 'utf8')

const dashboardPath = path.join(__dirname, '../src/views/DashboardView.vue')
let dashboard = fs.readFileSync(dashboardPath, 'utf8')

const alumnoBlock = `    <${d} v-if="authStore.hasRole('Alumno')" class="space-y-8">
      <router-link v-if="myAccess" to="/my-membership" class="card p-5 max-w-2xl block hover:border-primary-500/30 transition-colors">
        <${d} class="flex items-start justify-between gap-4">
          <${d}>
            <p class="text-xs uppercase tracking-widest text-dark-500 font-bold mb-2">Tu membresía</p>
            <span :class="accessStatusBadgeClass(myAccess.estadoAcceso)" class="text-sm px-3 py-1">{{ myAccess.estadoAcceso }}</span>
            <p v-if="myAccess.planNombre" class="text-sm text-dark-400 mt-3">{{ myAccess.planNombre }} · vence {{ formatDate(myAccess.fechaVencimiento) }}</p>
          </${d}>
          <span class="text-3xl">💳</span>
        </${d}>
      </router-link>

      <${d}>
        <h2 class="text-lg font-semibold text-white mb-4">Tus rutinas activas</h2>
        <LoadingSpinner v-if="loadingRoutines" />
        <${d} v-else-if="myRoutines.length === 0" class="card text-center py-12">
          <p class="text-dark-400">No tenés rutinas asignadas todavía.</p>
          <p class="text-sm text-dark-500 mt-1">Tu profesor te asignará rutinas pronto.</p>
        </${d}>
        <${d} v-else class="grid grid-cols-1 md:grid-cols-2 gap-4">
          <router-link
            v-for="routine in myRoutines"
            :key="routine.id"
            :to="\`/routines/\${routine.id}\`"
            class="card-hover group"
          >
            <${d} class="flex items-start justify-between">
              <${d}>
                <h3 class="font-semibold text-white group-hover:text-primary-300 transition-colors">{{ routine.nombre }}</h3>
                <p class="text-sm text-dark-400 mt-1">{{ routine.descripcion || 'Sin descripción' }}</p>
              </${d}>
              <span class="badge-success">Activa</span>
            </${d}>
          </router-link>
        </${d}>
      </${d}>
    </${d}>
`

const alumnoStart = dashboard.indexOf(`    <${d} v-if="authStore.hasRole('Alumno')`)
const templateEnd = dashboard.indexOf('</template>')
if (alumnoStart >= 0 && templateEnd > alumnoStart) {
  dashboard = dashboard.slice(0, alumnoStart) + alumnoBlock + '\n' + dashboard.slice(templateEnd)
}

if (!dashboard.includes('useMembershipStore')) {
  dashboard = dashboard.replace(
    "import LoadingSpinner from '@/components/shared/LoadingSpinner.vue'",
    `import LoadingSpinner from '@/components/shared/LoadingSpinner.vue'
import { useMembershipStore } from '@/stores/membership.store'
import { accessStatusBadgeClass, formatDate } from '@/constants/membershipStatus'`
  )
  dashboard = dashboard.replace(
    'const userStore = useUserStore()',
    `const userStore = useUserStore()
const membershipStore = useMembershipStore()
const myAccess = computed(() => membershipStore.myAccess)`
  )
  dashboard = dashboard.replace(
    'await routineStore.fetchMyRoutines()',
    `await Promise.all([
        routineStore.fetchMyRoutines(),
        membershipStore.fetchMyAccess()
      ])`
  )
}

if (!dashboard.includes("to=\"/memberships\"")) {
  dashboard = dashboard.replace(
    '      </div>\n    </motion.div>\n\n   \n    <',
    '        <router-link\n          v-if="authStore.hasRole(\'Superusuario\', \'Administrativo\')"\n          to="/memberships"\n          class="group flex items-center p-4 rounded-2xl bg-dark-900/40 border border-dark-800/50 hover:bg-dark-800/60 hover:border-primary-500/30 transition-all duration-300"\n        >\n          <div class="w-10 h-10 rounded-xl bg-amber-500/10 text-amber-500 flex items-center justify-center mr-4 text-lg">💳</div>\n          <div>\n            <h3 class="text-sm font-bold text-white">Membresías</h3>\n            <p class="text-xs text-dark-500">Alumnos y pagos</p>\n          </div>\n        </router-link>\n      </div>\n    </div>\n\n    <'
  )
  // fallback without motion
  dashboard = dashboard.replace(
    `        </router-link>\n\n      </div>\n    </div>\n\n   \n    <${d} v-if="authStore.hasRole('Alumno')"`,
    `        </router-link>\n\n        <router-link\n          v-if="authStore.hasRole('Superusuario', 'Administrativo')"\n          to="/memberships"\n          class="group flex items-center p-4 rounded-2xl bg-dark-900/40 border border-dark-800/50 hover:bg-dark-800/60 hover:border-primary-500/30 transition-all duration-300"\n        >\n          <div class="w-10 h-10 rounded-xl bg-amber-500/10 text-amber-500 flex items-center justify-center mr-4 text-lg">💳</div>\n          <div>\n            <h3 class="text-sm font-bold text-white">Membresías</h3>\n            <p class="text-xs text-dark-500">Alumnos y pagos</p>\n          </div>\n        </router-link>\n      </div>\n    </div>\n\n    <${d} v-if="authStore.hasRole('Alumno')"`
  )
}

dashboard = dashboard.replace(
  `if (authStore.hasRole('Superusuario', 'Administrativo')) {
  stats.value.push(
    { icon: '👥', label: 'Alumnos', value: '-', bgColor: 'bg-primary-700/10 text-primary-400', path: '/users' },
    { icon: '👨‍🏫', label: 'Profesores', value: '-', bgColor: 'bg-primary-800/10 text-primary-400', path: '/users' }
  )
}`,
  `if (authStore.hasRole('Superusuario', 'Administrativo')) {
  stats.value.push(
    { icon: '👥', label: 'Alumnos', value: '-', bgColor: 'bg-primary-700/10 text-primary-400', path: '/users' },
    { icon: '💳', label: 'Membresías activas', value: '-', bgColor: 'bg-amber-500/10 text-amber-400', path: '/memberships' }
  )
}`
)

dashboard = dashboard.replace(
  `if (authStore.hasRole('Superusuario', 'Administrativo')) {
      requests.push(userStore.fetchUsers())
    }`,
  `if (authStore.hasRole('Superusuario', 'Administrativo')) {
      requests.push(userStore.fetchUsers(), membershipStore.fetchMemberships({ estado: 'Activa' }))
    }`
)

dashboard = dashboard.replace(
  `stats.value[3].value = assignmentSummary.value.profesoresCount || 0
    }
  }
}`,
  `stats.value[3].value = membershipStore.memberships.length
    }
  }
}`
)

fs.writeFileSync(dashboardPath, dashboard, 'utf8')
console.log('Views generated')
