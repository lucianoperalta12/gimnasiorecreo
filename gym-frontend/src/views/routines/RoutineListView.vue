<template>
  <div class="animate-fade-in">
    <div class="flex items-center justify-between mb-6">
      <div>
        <h1 class="page-title">Rutinas</h1>
        <p class="page-subtitle">Gestión de rutinas</p>
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
        <router-link to="/routines/new">
          <AppButton variant="primary" class="md:px-4 px-2.5 !rounded-xl md:!rounded-lg">
            <svg class="w-5 h-5 md:w-4 md:h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2.5">
              <path stroke-linecap="round" stroke-linejoin="round" d="M12 4.5v15m7.5-7.5h-15" />
            </svg>
            <span class="hidden md:inline ml-1">Nueva Rutina</span>
          </AppButton>
        </router-link>
      </div>
    </div>

    <div v-if="authStore.hasRole('Superusuario')" class="mb-6 flex justify-end">
      <select 
        v-model="gymFilter" 
        class="input w-full sm:w-auto sm:min-w-[200px]"
      >
        <option :value="0">Todos los gimnasios</option>
        <option v-for="gym in gyms" :key="gym.id" :value="gym.id">{{ gym.nombre }}</option>
      </select>
    </div>

    <LoadingSpinner v-if="store.loading" />

    <template v-else>
      <div v-if="filteredRoutines.length === 0" class="card text-center py-12">
        <p class="text-dark-400">No hay rutinas que coincidan</p>
      </div>

      <div v-else class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
        <div v-for="routine in filteredRoutines" :key="routine.id" class="card-hover group">
          <div class="flex items-start justify-between mb-3">
            <h3 class="font-semibold text-white group-hover:text-primary-300 transition-colors">{{ routine.nombre }}</h3>
            <span :class="routine.activa ? 'badge-success' : 'badge-danger'">
              {{ routine.activa ? 'Activa' : 'Inactiva' }}
            </span>
          </div>
          <p class="text-sm text-dark-400 mb-4 line-clamp-2">{{ routine.descripcion || 'Sin descripción' }}</p>
            <div class="flex flex-col gap-1">
              <div class="flex items-center gap-3">
                <span>{{ routine.cantidadEjercicios }} ejercicios</span>
                <span>{{ routine.profesorNombre }}</span>
              </div>
              <span v-if="authStore.hasRole('Superusuario')" class="text-[10px] text-primary-400 font-bold uppercase tracking-wider">{{ routine.gymNombre }}</span>
            </div>
          <div class="flex items-center gap-2 mt-4 pt-4 border-t border-dark-700/50">
            <router-link :to="`/routines/${routine.id}`" class="btn-ghost btn-sm flex-1 text-center">Ver</router-link>
            <router-link :to="`/routines/${routine.id}/edit`" class="btn-ghost btn-sm flex-1 text-center">Editar</router-link>
            <button @click="confirmDelete(routine)" class="btn-ghost btn-sm text-red-400 hover:text-red-300">
              <svg class="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="1.5">
                <path stroke-linecap="round" stroke-linejoin="round" d="M14.74 9l-.346 9m-4.788 0L9.26 9m9.968-3.21c.342.052.682.107 1.022.166m-1.022-.165L18.16 19.673a2.25 2.25 0 01-2.244 2.077H8.084a2.25 2.25 0 01-2.244-2.077L4.772 5.79m14.456 0a48.108 48.108 0 00-3.478-.397m-12 .562c.34-.059.68-.114 1.022-.165m0 0a48.11 48.11 0 013.478-.397m7.5 0v-.916c0-1.18-.91-2.164-2.09-2.201a51.964 51.964 0 00-3.32 0c-1.18.037-2.09 1.022-2.09 2.201v.916m7.5 0a48.667 48.667 0 00-7.5 0" />
              </svg>
            </button>
          </div>
        </div>
      </div>

      <div v-if="showPagination" class="mt-6 flex items-center justify-between px-2">
        <div class="text-xs text-dark-500 font-medium">
          Mostrando {{ pageStart }} - {{ pageEnd }}
          de {{ effectiveTotalCount }} rutinas
        </div>
        <div class="flex items-center gap-2">
          <button
            class="btn-secondary !p-2 !rounded-lg"
            :disabled="currentPage === 1"
            @click="goToPage(currentPage - 1)"
          >
            <svg class="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M15.75 19.5L8.25 12l7.5-7.5" /></svg>
          </button>

          <div class="flex items-center gap-1">
            <button
              v-for="p in totalPages"
              :key="p"
              @click="goToPage(p)"
              class="w-8 h-8 rounded-lg text-xs font-black transition-all"
              :class="currentPage === p ? 'bg-primary-600 text-white' : 'bg-dark-800 text-dark-400 hover:bg-dark-700'"
            >
              {{ p }}
            </button>
          </div>

          <button
            class="btn-secondary !p-2 !rounded-lg"
            :disabled="currentPage === totalPages"
            @click="goToPage(currentPage + 1)"
          >
            <svg class="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M8.25 4.5l7.5 7.5-7.5 7.5" /></svg>
          </button>
        </div>
      </div>
    </template>

    <!-- Delete -->
    <AppModal v-model="showDeleteModal" title="Eliminar Rutina" size="sm">
      <p class="text-dark-300">¿Eliminar <strong class="text-white">{{ deletingRoutine?.nombre }}</strong>?</p>
      <template #footer>
        <AppButton variant="secondary" @click="showDeleteModal = false">Cancelar</AppButton>
        <AppButton variant="danger" :loading="deleting" @click="handleDelete">Eliminar</AppButton>
      </template>
    </AppModal>
  </div>
</template>

<script setup>
import { ref, computed, onMounted, watch } from 'vue'
import { useRouter } from 'vue-router'
import { useRoutineStore } from '@/stores/routine.store'
import { useAuthStore } from '@/stores/auth.store'
import { useNotification } from '@/composables/useNotification'
import AppButton from '@/components/ui/AppButton.vue'
import AppModal from '@/components/ui/AppModal.vue'
import LoadingSpinner from '@/components/shared/LoadingSpinner.vue'

const router = useRouter()
const store = useRoutineStore()
const authStore = useAuthStore()
const { success, error: showError } = useNotification()

const gymFilter = ref(0)
const gyms = ref([])
const showDeleteModal = ref(false)
const deletingRoutine = ref(null)
const deleting = ref(false)
const currentPage = ref(1)
const pageSize = 15

const filteredRoutines = computed(() => {
  if (!authStore.hasRole('Superusuario') || gymFilter.value === 0) {
    return store.routines
  }
  return store.routines.filter(r => r.gymId === gymFilter.value)
})

const usingLegacyFullFetch = computed(() => authStore.hasRole('Superusuario') && gymFilter.value !== 0)
const effectiveTotalCount = computed(() => {
  if (usingLegacyFullFetch.value || !store.routinesServerPaginationEnabled) {
    return filteredRoutines.value.length
  }

  return store.routinesTotalCount
})
const totalPages = computed(() => Math.max(1, Math.ceil(effectiveTotalCount.value / pageSize)))
const pageStart = computed(() => (filteredRoutines.value.length ? (currentPage.value - 1) * pageSize + 1 : 0))
const pageEnd = computed(() => (filteredRoutines.value.length ? pageStart.value + filteredRoutines.value.length - 1 : 0))
const showPagination = computed(() => !usingLegacyFullFetch.value && store.routinesServerPaginationEnabled && effectiveTotalCount.value > pageSize)

function confirmDelete(routine) {
  deletingRoutine.value = routine
  showDeleteModal.value = true
}

async function handleDelete() {
  deleting.value = true
  try {
    await store.deleteRoutine(deletingRoutine.value.id)
    if (usingLegacyFullFetch.value) {
      await loadRoutinesLegacy()
    } else {
      const fallbackPage = filteredRoutines.value.length === 0 && currentPage.value > 1 ? currentPage.value - 1 : currentPage.value
      await loadRoutinesPaged(fallbackPage)
    }
    success('Rutina eliminada')
    showDeleteModal.value = false
  } catch (err) {
    showError(err.response?.data?.error || 'Error al eliminar')
  } finally {
    deleting.value = false
  }
}

async function loadRoutinesPaged(targetPage = 1) {
  await store.fetchRoutines({ page: targetPage, pageSize })
  currentPage.value = store.routinesPage ?? targetPage
}

async function loadRoutinesLegacy() {
  await store.fetchRoutines()
  currentPage.value = 1
}

async function goToPage(targetPage) {
  if (usingLegacyFullFetch.value || !store.routinesServerPaginationEnabled) return
  if (targetPage < 1 || targetPage > totalPages.value || targetPage === currentPage.value) return
  await loadRoutinesPaged(targetPage)
}

onMounted(async () => {
  await loadRoutinesPaged()
  if (authStore.hasRole('Superusuario')) {
    try {
      const { gymsApi } = await import('@/api/gyms.api')
      const { data } = await gymsApi.getAll()
      gyms.value = data
    } catch (err) {
      console.error('Error fetching gyms:', err)
    }
  }
})

watch(gymFilter, async (value) => {
  if (!authStore.hasRole('Superusuario')) return

  if (value === 0) {
    await loadRoutinesPaged(1)
    return
  }

  await loadRoutinesLegacy()
})
</script>
