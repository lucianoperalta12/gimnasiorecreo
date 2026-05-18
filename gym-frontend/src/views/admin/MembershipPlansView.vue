<template>
  <div class="animate-fade-in">
    <div class="flex items-center justify-between mb-6">
      <div>
        <h1 class="page-title">Planes de membresía</h1>
        <p class="page-subtitle">Planes y precios del gimnasio</p>
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
          <span class="hidden md:inline ml-1">Nuevo Plan</span>
        </AppButton>
      </div>
    </div>

    <div v-if="authStore.hasRole('Superusuario')" class="mb-4">
      <select v-model="selectedGymId" class="input max-w-xs" @change="loadPlans">
        <option :value="null">Todos los gimnasios</option>
        <option v-for="gym in gyms" :key="gym.id" :value="gym.id">{{ gym.nombre }}</option>
      </select>
    </div>

    <LoadingSpinner v-if="store.loading" />

    <template v-else>
      <div class="mb-4">
        <input v-model="search" type="text" placeholder="Buscar plan..." class="input max-w-sm" />
      </div>

      <!-- Mobile List (< md) -->
      <div class="md:hidden space-y-3">
        <div v-for="plan in filteredPlans" :key="plan.id" class="p-4 rounded-2xl bg-dark-900/40 border border-dark-800/50 flex flex-col gap-3">
          <div class="flex items-center justify-between">
            <h3 class="font-semibold text-white">{{ plan.nombre }}</h3>
            <span :class="plan.activo ? 'badge-success' : 'badge-danger'" class="text-[10px]">{{ plan.activo ? 'Activo' : 'Inactivo' }}</span>
          </div>
          <div class="flex items-center justify-between text-xs text-dark-400">
            <span>{{ plan.duracionDias }} días ({{ plan.paseLibre ? 'Pase Libre' : plan.diasPorSemana + ' días/sem' }})</span>
            <span class="font-bold text-primary-400">{{ formatCurrency(plan.precio, plan.moneda) }}</span>
          </div>
          <div v-if="authStore.hasRole('Superusuario')" class="text-[10px] text-dark-500 uppercase font-black tracking-wider border-t border-dark-800 pt-2 mt-1">
            Gimnasio: {{ plan.gymNombre || '—' }}
          </div>
          <div class="flex items-center gap-2 mt-1">
            <button class="btn-secondary btn-sm flex-1 text-xs" @click="openEditModal(plan)">Editar</button>
            <button class="btn-secondary btn-sm flex-1 text-xs text-red-400 flex items-center justify-center gap-1" @click="confirmDelete(plan)">
              <svg class="w-3.5 h-3.5" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16" /></svg>
              Eliminar
            </button>
          </div>
        </div>
      </div>

      <!-- Desktop Table (md+) -->
      <div v-if="filteredPlans.length" class="hidden md:block table-container">
        <table class="table">
          <thead>
            <tr>
              <th>Nombre</th>
              <th>Duración</th>
              <th>Precio</th>
              <th>Estado</th>
              <th v-if="authStore.hasRole('Superusuario')">Gimnasio</th>
              <th class="text-right">Acciones</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="plan in filteredPlans" :key="plan.id">
              <td class="font-medium text-white">{{ plan.nombre }}</td>
              <td>
                <div class="flex flex-col">
                  <span>{{ plan.duracionDias }} días</span>
                  <span class="text-xs text-dark-400">{{ plan.paseLibre ? 'Pase Libre' : plan.diasPorSemana + ' días/sem' }}</span>
                </div>
              </td>
              <td>{{ formatCurrency(plan.precio, plan.moneda) }}</td>
              <td><span :class="plan.activo ? 'badge-success' : 'badge-danger'">{{ plan.activo ? 'Activo' : 'Inactivo' }}</span></td>
              <td v-if="authStore.hasRole('Superusuario')">{{ plan.gymNombre || '—' }}</td>
              <td class="text-right">
                <button class="btn-ghost btn-sm" @click="openEditModal(plan)">Editar</button>
                <button class="btn-ghost btn-sm text-red-400 p-1.5" title="Eliminar" @click="confirmDelete(plan)">
                  <svg class="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16" /></svg>
                </button>
              </td>
            </tr>
          </tbody>
        </table>
      </div>

      <div v-if="filteredPlans.length === 0" class="card text-center py-12">
        <p class="text-dark-400">No hay planes configurados</p>
      </div>
    </template>

    <AppModal v-model="showModal" :title="editingPlan ? 'Editar plan' : 'Nuevo plan'">
      <form class="space-y-4" @submit.prevent="handleSubmit">
        <AppInput v-model="form.nombre" label="Nombre" />
        <textarea v-model="form.descripcion" rows="2" class="input" placeholder="Descripción opcional" />
        <div class="grid grid-cols-2 gap-4">
          <AppInput v-model.number="form.duracionDias" label="Duración (días)" type="number" min="1" />
          <AppInput v-model.number="form.precio" label="Precio" type="number" min="0" step="0.01" />
        </div>

        <div class="flex items-center justify-between p-3 rounded-xl bg-dark-900/50 border border-dark-800">
          <span class="text-sm font-medium text-dark-300">Pase libre</span>
          <button
            type="button"
            class="relative inline-flex h-6 w-11 shrink-0 items-center rounded-full transition-colors focus:outline-none"
            :class="form.paseLibre ? 'bg-emerald-600' : 'bg-dark-700'"
            @click="form.paseLibre = !form.paseLibre"
          >
            <span class="inline-block h-4 w-4 rounded-full bg-white transition-transform" :class="form.paseLibre ? 'translate-x-6' : 'translate-x-1'" />
          </button>
        </div>

        <div v-if="!form.paseLibre" class="animate-fade-in">
          <AppInput v-model.number="form.diasPorSemana" label="Días por semana" type="number" min="1" max="5" />
        </div>

        <div class="flex items-center justify-between p-3 rounded-xl bg-dark-900/50 border border-dark-800">
          <span class="text-sm font-medium text-dark-300">Plan activo</span>
          <button
            type="button"
            class="relative inline-flex h-6 w-11 shrink-0 items-center rounded-full transition-colors focus:outline-none"
            :class="form.activo ? 'bg-emerald-600' : 'bg-dark-700'"
            @click="form.activo = !form.activo"
          >
            <span class="inline-block h-4 w-4 rounded-full bg-white transition-transform" :class="form.activo ? 'translate-x-6' : 'translate-x-1'" />
          </button>
        </div>
        <div v-if="authStore.hasRole('Superusuario') && !editingPlan">
          <label class="label">Gimnasio</label>
          <select v-model.number="form.gymId" class="input" required>
            <option :value="0" disabled>Seleccionar</option>
            <option v-for="gym in gyms" :key="gym.id" :value="gym.id">{{ gym.nombre }}</option>
          </select>
        </div>
      </form>
      <template #footer>
        <AppButton variant="secondary" @click="showModal = false">Cancelar</AppButton>
        <AppButton :loading="saving" @click="handleSubmit">Guardar</AppButton>
      </template>
    </AppModal>

    <AppModal v-model="showDeleteModal" title="Eliminar plan" size="sm">
      <p class="text-dark-300">¿Eliminar <strong class="text-white">{{ deletingPlan?.nombre }}</strong>?</p>
      <template #footer>
        <AppButton variant="secondary" @click="showDeleteModal = false">Cancelar</AppButton>
        <AppButton variant="danger" :loading="saving" @click="handleDelete">Eliminar</AppButton>
      </template>
    </AppModal>
  </div>
</template>

<script setup>
import { ref, reactive, computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useMembershipStore } from '@/stores/membership.store'
import { useAuthStore } from '@/stores/auth.store'
import { useNotification } from '@/composables/useNotification'
import { gymsApi } from '@/api/gyms.api'
import { formatCurrency } from '@/constants/membershipStatus'
import AppButton from '@/components/ui/AppButton.vue'
import AppInput from '@/components/ui/AppInput.vue'
import AppModal from '@/components/ui/AppModal.vue'
import LoadingSpinner from '@/components/shared/LoadingSpinner.vue'

const router = useRouter()
const store = useMembershipStore()
const authStore = useAuthStore()
const { success, error: showError } = useNotification()

const search = ref('')
const gyms = ref([])
const selectedGymId = ref(null)
const showModal = ref(false)
const showDeleteModal = ref(false)
const saving = ref(false)
const editingPlan = ref(null)
const deletingPlan = ref(null)

const form = reactive({ nombre: '', descripcion: '', duracionDias: 30, precio: 0, paseLibre: true, diasPorSemana: 3, activo: true, gymId: 0 })

const filteredPlans = computed(() => {
  const q = search.value.toLowerCase()
  return store.plans.filter(p => p.nombre.toLowerCase().includes(q))
})

function openCreateModal() {
  editingPlan.value = null
  Object.assign(form, { nombre: '', descripcion: '', duracionDias: 30, precio: 0, paseLibre: true, diasPorSemana: 3, activo: true, gymId: authStore.user?.gymId || 0 })
  showModal.value = true
}

function openEditModal(plan) {
  editingPlan.value = plan
  Object.assign(form, { nombre: plan.nombre, descripcion: plan.descripcion || '', duracionDias: plan.duracionDias, precio: plan.precio, paseLibre: plan.paseLibre, diasPorSemana: plan.diasPorSemana || 3, activo: plan.activo, gymId: plan.gymId })
  showModal.value = true
}

function confirmDelete(plan) {
  deletingPlan.value = plan
  showDeleteModal.value = true
}

async function loadPlans() {
  await store.fetchPlans(selectedGymId.value || undefined)
}

async function handleSubmit() {
  saving.value = true
  try {
    const payload = { 
      nombre: form.nombre, 
      descripcion: form.descripcion || null, 
      duracionDias: Number(form.duracionDias), 
      precio: Number(form.precio), 
      paseLibre: form.paseLibre,
      diasPorSemana: form.paseLibre ? null : Number(form.diasPorSemana),
      activo: form.activo 
    }
    if (authStore.hasRole('Superusuario') && !editingPlan.value) payload.gymId = form.gymId || null
    if (editingPlan.value) {
      await store.updatePlan(editingPlan.value.id, payload)
      success('Plan actualizado')
    } else {
      await store.createPlan(payload)
      success('Plan creado')
    }
    showModal.value = false
  } catch (err) {
    showError(err.response?.data?.error || 'Error al guardar')
  } finally {
    saving.value = false
  }
}

async function handleDelete() {
  saving.value = true
  try {
    await store.deletePlan(deletingPlan.value.id)
    success('Plan eliminado')
    showDeleteModal.value = false
  } catch (err) {
    showError(err.response?.data?.error || 'Error al eliminar')
  } finally {
    saving.value = false
  }
}

onMounted(async () => {
  if (authStore.hasRole('Superusuario')) {
    try { const { data } = await gymsApi.getAll(); gyms.value = data } catch { /* noop */ }
  }
  await loadPlans()
})
</script>
