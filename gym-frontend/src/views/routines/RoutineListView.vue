<template>
  <div class="animate-fade-in">
    <div class="flex items-center justify-between mb-6">
      <div>
        <h1 class="page-title">Rutinas</h1>
        <p class="page-subtitle">Gestión de rutinas de entrenamiento</p>
      </div>
      <router-link to="/routines/new">
        <AppButton variant="danger" class="md:px-4 px-2.5 !rounded-xl md:!rounded-lg">
          <svg class="w-5 h-5 md:w-4 md:h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2.5">
            <path stroke-linecap="round" stroke-linejoin="round" d="M12 4.5v15m7.5-7.5h-15" />
          </svg>
          <span class="hidden md:inline ml-1">Nueva Rutina</span>
        </AppButton>
      </router-link>
    </div>

    <LoadingSpinner v-if="store.loading" />

    <template v-else>
      <div v-if="store.routines.length === 0" class="card text-center py-12">
        <p class="text-dark-400">No hay rutinas creadas</p>
        <router-link to="/routines/new" class="text-primary-400 text-sm hover:underline mt-2 inline-block">Crear la primera rutina</router-link>
      </div>

      <div v-else class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
        <div v-for="routine in store.routines" :key="routine.id" class="card-hover group">
          <div class="flex items-start justify-between mb-3">
            <h3 class="font-semibold text-white group-hover:text-primary-300 transition-colors">{{ routine.nombre }}</h3>
            <span :class="routine.activa ? 'badge-success' : 'badge-danger'">
              {{ routine.activa ? 'Activa' : 'Inactiva' }}
            </span>
          </div>
          <p class="text-sm text-dark-400 mb-4 line-clamp-2">{{ routine.descripcion || 'Sin descripción' }}</p>
          <div class="flex items-center justify-between text-xs text-dark-500">
            <div class="flex items-center gap-3">
              <span>{{ routine.cantidadEjercicios }} ejercicios</span>
              <span>{{ routine.profesorNombre }}</span>
            </div>
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
import { ref, onMounted } from 'vue'
import { useRoutineStore } from '@/stores/routine.store'
import { useNotification } from '@/composables/useNotification'
import AppButton from '@/components/ui/AppButton.vue'
import AppModal from '@/components/ui/AppModal.vue'
import LoadingSpinner from '@/components/shared/LoadingSpinner.vue'

const store = useRoutineStore()
const { success, error: showError } = useNotification()

const showDeleteModal = ref(false)
const deletingRoutine = ref(null)
const deleting = ref(false)

function confirmDelete(routine) {
  deletingRoutine.value = routine
  showDeleteModal.value = true
}

async function handleDelete() {
  deleting.value = true
  try {
    await store.deleteRoutine(deletingRoutine.value.id)
    success('Rutina eliminada')
    showDeleteModal.value = false
  } catch (err) {
    showError(err.response?.data?.error || 'Error al eliminar')
  } finally {
    deleting.value = false
  }
}

onMounted(() => store.fetchRoutines())
</script>
