<template>
  <div class="animate-fade-in">
    <!-- Header -->
    <div class="flex items-center justify-between mb-6">
      <div>
        <h1 class="page-title">Asistencia</h1>
        <p class="page-subtitle">Registros de accesos desde terminales</p>
      </div>
      <div>
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
      </div>
    </div>

    <!-- Filters -->
    <div class="flex flex-col sm:flex-row gap-3 mb-4 items-end">
      <div class="w-full sm:max-w-[180px]">
        <label class="label">Fecha</label>
        <input 
          v-model="fecha" 
          type="date" 
          class="input w-full text-sm" 
          :disabled="!!alumnoId" 
          :class="{ 'opacity-50 cursor-not-allowed': !!alumnoId }" 
        />
      </div>
      <div class="w-full sm:max-w-md">
        <label class="label">Alumno</label>
        <AppSearchSelect v-model="alumnoId" :options="studentOptions" placeholder="Buscar alumno..." class="w-full" />
      </div>
      <div v-if="authStore.hasRole('Superusuario')" class="w-full sm:max-w-xs">
        <label class="label">Gimnasio</label>
        <AppSearchSelect v-model="gymId" :options="gymOptions" placeholder="Filtrar por gimnasio..." class="w-full" />
      </div>
      <div class="flex gap-2 w-full sm:w-auto shrink-0">
        <AppButton variant="primary" class="flex-grow sm:flex-grow-0" @click="buscar">Aplicar</AppButton>
      </div>
    </div>

    <LoadingSpinner v-if="loading" />

    <template v-else>
      <!-- Mobile Cards -->
      <div v-if="items.length && isMobile" class="space-y-3">
        <div v-for="item in items" :key="item.id" class="p-4 rounded-2xl bg-dark-900/40 border border-dark-800/50 flex flex-col gap-2">
          <div class="flex items-center justify-between">
            <h3 class="font-semibold text-white text-sm">{{ item.alumno }}</h3>
            <span class="badge-primary">{{ item.tipoMembresia || '—' }}</span>
          </div>
          <div class="space-y-1 text-xs text-dark-400">
            <div class="flex justify-between">
              <span>DNI:</span>
              <span class="text-dark-200">{{ item.dni }}</span>
            </div>
            <div class="flex justify-between">
              <span>Gimnasio:</span>
              <span class="text-dark-200">{{ item.gimnasio }}</span>
            </div>
            <div class="flex justify-between">
              <span>Fecha y hora:</span>
              <span class="text-dark-200">{{ formatDateTime(item.fechaHora) }}</span>
            </div>
            <div class="flex justify-between">
              <span>Terminal:</span>
              <span class="text-dark-200">{{ item.terminal }}</span>
            </div>
          </div>
        </div>
      </div>

      <!-- Desktop Table -->
      <div v-else-if="items.length && !isMobile" class="table-container">
        <table class="table">
          <thead>
            <tr>
              <th>Alumno</th>
              <th>DNI</th>
              <th>Gimnasio</th>
              <th>Fecha y hora</th>
              <th>Terminal</th>
              <th>Membresía</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="item in items" :key="item.id">
              <td class="font-medium text-white">{{ item.alumno }}</td>
              <td>{{ item.dni }}</td>
              <td>{{ item.gimnasio }}</td>
              <td>{{ formatDateTime(item.fechaHora) }}</td>
              <td>{{ item.terminal }}</td>
              <td>
                <span class="badge-primary">{{ item.tipoMembresia || '—' }}</span>
              </td>
            </tr>
          </tbody>
        </table>
      </div>

      <!-- Pagination Controls -->
      <div v-if="serverPaginationEnabled && totalCount > pageSize" class="mt-6 flex items-center justify-between px-2">
        <div class="text-xs text-dark-500 font-medium">
          Mostrando {{ pageStart }} - {{ pageEnd }}
          de {{ totalCount }} registros
        </div>
        <div class="flex items-center gap-2">
          <button 
            class="btn-secondary !p-2 !rounded-lg" 
            :disabled="page === 1"
            @click="goToPage(page - 1)"
          >
            <svg class="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M15.75 19.5L8.25 12l7.5-7.5" /></svg>
          </button>
          
          <div class="flex items-center gap-1">
            <button 
              v-for="p in totalPages" 
              :key="p"
              @click="goToPage(p)"
              class="w-8 h-8 rounded-lg text-xs font-black transition-all"
              :class="page === p ? 'bg-primary-600 text-white' : 'bg-dark-800 text-dark-400 hover:bg-dark-700'"
            >
              {{ p }}
            </button>
          </div>

          <button 
            class="btn-secondary !p-2 !rounded-lg" 
            :disabled="page === totalPages"
            @click="goToPage(page + 1)"
          >
            <svg class="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M8.25 4.5l7.5 7.5-7.5 7.5" /></svg>
          </button>
        </div>
      </div>

      <div v-else-if="!items.length" class="card text-center py-12">
        <p class="text-dark-400">No hay asistencias para los filtros seleccionados.</p>
      </div>
    </template>
  </div>
</template>

<script setup>
import { computed, onMounted, ref } from 'vue'
import { useIsMobile } from '@/composables/useIsMobile'
import { useRouter } from 'vue-router'
import AppSearchSelect from '@/components/ui/AppSearchSelect.vue'
import AppButton from '@/components/ui/AppButton.vue'
import LoadingSpinner from '@/components/shared/LoadingSpinner.vue'
import { ingresosApi } from '@/api/ingresos.api'
import { gymsApi } from '@/api/gyms.api'
import { useUserStore } from '@/stores/user.store'
import { useAuthStore } from '@/stores/auth.store'
import { useNotification } from '@/composables/useNotification'
import { parsePaginatedResponse } from '@/utils/pagination'
import { formatLocalDate } from '@/utils/date'

const router = useRouter()
const { error: showError } = useNotification()
const { isMobile } = useIsMobile()
const userStore = useUserStore()
const authStore = useAuthStore()
const loading = ref(false)
const items = ref([])
const totalCount = ref(0)
const serverPaginationEnabled = ref(false)
const alumnoId = ref('')
const gymId = ref('')
const gyms = ref([])
const fecha = ref(formatLocalDate(new Date()))
const page = ref(1)
const pageSize = 15

const studentOptions = computed(() => [
  { id: '', label: 'Todos los alumnos' },
  ...userStore.students.map(s => ({ id: s.id, label: `${s.nombre} ${s.apellido}`.trim(), subLabel: s.dni }))
])

const gymOptions = computed(() => [
  { id: '', label: 'Todos los gimnasios' },
  ...gyms.value.map(g => ({ id: g.id, label: g.nombre }))
])

const totalPages = computed(() => Math.max(1, Math.ceil(totalCount.value / pageSize)))
const pageStart = computed(() => (items.value.length ? (page.value - 1) * pageSize + 1 : 0))
const pageEnd = computed(() => (items.value.length ? pageStart.value + items.value.length - 1 : 0))

function formatDateTime(value) {
  return new Date(value).toLocaleString()
}

async function buscar(targetPage = 1) {
  const pageNum = typeof targetPage === 'number' ? targetPage : 1
  loading.value = true
  try {
    const params = {
      page: pageNum,
      pageSize
    }
    if (alumnoId.value) {
      params.alumnoId = alumnoId.value
    } else {
      params.fechaDesde = fecha.value
      params.fechaHasta = fecha.value
    }
    
    if (authStore.hasRole('Superusuario') && gymId.value) {
      params.gymId = gymId.value
    }
    
    const response = await ingresosApi.getAll(params)
    const pagination = parsePaginatedResponse(response)
    items.value = pagination.items
    totalCount.value = pagination.totalCount
    serverPaginationEnabled.value = pagination.serverPaginationEnabled
    page.value = pagination.page ?? targetPage
  } catch (err) {
    showError(err.response?.data?.error || 'No se pudieron cargar los registros de asistencia')
  } finally {
    loading.value = false
  }
}

onMounted(async () => {
  const promises = [userStore.fetchStudents()]
  if (authStore.hasRole('Superusuario')) {
    promises.push(gymsApi.getAll().then(res => {
      gyms.value = res.data || []
    }))
  }
  promises.push(buscar())
  await Promise.allSettled(promises)
})

async function goToPage(targetPage) {
  if (!serverPaginationEnabled.value) return
  if (targetPage < 1 || targetPage > totalPages.value || targetPage === page.value) return
  await buscar(targetPage)
}
</script>
