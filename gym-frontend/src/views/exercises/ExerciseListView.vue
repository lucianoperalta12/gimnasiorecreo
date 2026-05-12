<template>
  <div class="animate-fade-in">
    <div class="flex items-center justify-between mb-6">
      <div>
        <h1 class="page-title">Ejercicios</h1>
        <p class="page-subtitle">Gestión del catálogo global de ejercicios</p>
      </div>
      <div class="flex items-center gap-3">
        <button 
          @click="router.push('/dashboard')" 
          class="text-[10px] font-black text-dark-400 hover:text-white transition-all uppercase tracking-[0.2em] py-2 px-4 rounded-xl border border-dark-800 hover:border-primary-500/50 bg-dark-900/50 hover:bg-dark-800 flex items-center gap-2 shadow-sm"
        >
          <svg class="w-3 h-3" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="3">
            <path stroke-linecap="round" stroke-linejoin="round" d="M10.5 19.5L3 12m0 0l7.5-7.5M3 12h18" />
          </svg>
          Volver
        </button>
        <AppButton variant="danger" @click="openCreateModal" class="md:px-4 px-2.5 !rounded-xl md:!rounded-lg">
          <svg class="w-5 h-5 md:w-4 md:h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2.5">
            <path stroke-linecap="round" stroke-linejoin="round" d="M12 4.5v15m7.5-7.5h-15" />
          </svg>
          <span class="hidden md:inline ml-1">Nuevo Ejercicio</span>
        </AppButton>
      </div>
    </div>

    <LoadingSpinner v-if="store.loading" />

    <template v-else>
      <!-- Filter -->
      <div class="mb-4">
        <input v-model="search" type="text" placeholder="Buscar ejercicio..." class="input max-w-sm" />
      </div>

      <!-- Responsive List -->
      <div v-if="filteredExercises.length">
        <!-- Desktop Table -->
        <div class="hidden md:block table-container">
          <table class="table">
            <thead>
              <tr>
                <th>Nombre</th>
                <th>Grupo Muscular</th>
                <th>Descripción</th>
                <th class="text-right">Acciones</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="exercise in filteredExercises" :key="exercise.id" class="animate-fade-in">
                <td class="font-medium text-white">{{ exercise.nombre }}</td>
                <td><span class="badge-primary">{{ exercise.grupoMuscular }}</span></td>
                <td class="text-dark-400 max-w-xs truncate">{{ exercise.descripcion || '—' }}</td>
                <td class="text-right">
                  <div class="flex items-center justify-end gap-2">
                    <button @click="openEditModal(exercise)" class="btn-ghost btn-sm">Editar</button>
                    <button @click="confirmDelete(exercise)" class="btn-ghost btn-sm text-red-400 hover:text-red-300">
                      <svg class="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="1.5">
                <path stroke-linecap="round" stroke-linejoin="round" d="M14.74 9l-.346 9m-4.788 0L9.26 9m9.968-3.21c.342.052.682.107 1.022.166m-1.022-.165L18.16 19.673a2.25 2.25 0 01-2.244 2.077H8.084a2.25 2.25 0 01-2.244-2.077L4.772 5.79m14.456 0a48.108 48.108 0 00-3.478-.397m-12 .562c.34-.059.68-.114 1.022-.165m0 0a48.11 48.11 0 013.478-.397m7.5 0v-.916c0-1.18-.91-2.164-2.09-2.201a51.964 51.964 0 00-3.32 0c-1.18.037-2.09 1.022-2.09 2.201v.916m7.5 0a48.667 48.667 0 00-7.5 0" />
              </svg>
                    </button>
                  </div>
                </td>
              </tr>
            </tbody>
          </table>
        </div>

        <!-- Mobile Cards -->
        <div class="md:hidden space-y-4">
          <div v-for="exercise in filteredExercises" :key="exercise.id" class="card p-4 space-y-3">
            <div class="flex items-start justify-between">
              <div>
                <h3 class="font-bold text-white">{{ exercise.nombre }}</h3>
                <span class="badge-primary mt-1">{{ exercise.grupoMuscular }}</span>
              </div>
              <div class="flex gap-1">
                <button @click="openEditModal(exercise)" class="p-2 text-dark-400 hover:text-white">
                  <svg class="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" d="M16.862 4.487l1.687-1.688a1.875 1.875 0 112.652 2.652L10.582 16.07a4.5 4.5 0 01-1.897 1.13L6 18l.8-2.685a4.5 4.5 0 011.13-1.897l8.932-8.931zm0 0L19.5 7.125M18 14v4.75A2.25 2.25 0 0115.75 21H5.25A2.25 2.25 0 013 18.75V8.25A2.25 2.25 0 015.25 6H10" /></svg>
                </button>
                <button @click="confirmDelete(exercise)" class="p-2 text-red-400 hover:text-red-300">
                  <svg class="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" d="M14.74 9l-.346 9m-4.788 0L9.26 9m9.968-3.21c.342.052.682.107 1.022.166m-1.022-.165L18.16 19.673a2.25 2.25 0 01-2.244 2.077H8.084a2.25 2.25 0 01-2.244-2.077L4.772 5.79m14.456 0a48.108 48.108 0 00-3.478-.397m-12 .562c.34-.059.68-.114 1.022-.165m0 0a48.11 48.11 0 013.478-.397m7.5 0v-.916c0-1.18-.91-2.164-2.09-2.201a51.964 51.964 0 00-3.32 0c-1.18.037-2.09 1.022-2.09 2.201v.916m7.5 0a48.667 48.667 0 00-7.5 0" /></svg>
                </button>
              </div>
            </div>
            <p class="text-sm text-dark-400">{{ exercise.descripcion || 'Sin descripción' }}</p>
          </div>
        </div>
      </div>

      <div v-else class="card text-center py-12">
        <p class="text-dark-400">No se encontraron ejercicios</p>
      </div>
    </template>

    <!-- Create/Edit Modal -->
    <AppModal v-model="showModal" :title="editingExercise ? 'Editar Ejercicio' : 'Nuevo Ejercicio'">
      <form @submit.prevent="handleSubmit" class="space-y-4">
        <AppInput v-model="form.nombre" label="Nombre" placeholder="Ej: Press de banca" />
        <AppInput v-model="form.grupoMuscular" label="Grupo Muscular" placeholder="Ej: Pecho" />
        <div>
          <label class="label">Descripción</label>
          <textarea v-model="form.descripcion" rows="3" class="input" placeholder="Descripción del ejercicio..." />
        </div>
        <AppInput v-model="form.videoUrl" label="URL del Video (opcional)" placeholder="https://..." />
      </form>
      <template #footer>
        <AppButton variant="secondary" @click="showModal = false">Cancelar</AppButton>
        <AppButton :loading="saving" @click="handleSubmit">{{ editingExercise ? 'Guardar' : 'Crear' }}</AppButton>
      </template>
    </AppModal>

    <!-- Delete Confirmation -->
    <AppModal v-model="showDeleteModal" title="Eliminar Ejercicio" size="sm">
      <p class="text-dark-300">¿Estás seguro de que querés eliminar <strong class="text-white">{{ deletingExercise?.nombre }}</strong>?</p>
      <p class="text-sm text-dark-500 mt-2">Esta acción no se puede deshacer.</p>
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
import { useRoutineStore } from '@/stores/routine.store'
import { useNotification } from '@/composables/useNotification'
import AppButton from '@/components/ui/AppButton.vue'
import AppInput from '@/components/ui/AppInput.vue'
import AppModal from '@/components/ui/AppModal.vue'
import LoadingSpinner from '@/components/shared/LoadingSpinner.vue'

const router = useRouter()
const store = useRoutineStore()
const { success, error: showError } = useNotification()

const search = ref('')
const showModal = ref(false)
const showDeleteModal = ref(false)
const saving = ref(false)
const editingExercise = ref(null)
const deletingExercise = ref(null)

const form = reactive({ nombre: '', descripcion: '', grupoMuscular: '', videoUrl: '' })

const filteredExercises = computed(() => {
  const q = search.value.toLowerCase()
  return store.exercises.filter(e =>
    e.nombre.toLowerCase().includes(q) || e.grupoMuscular.toLowerCase().includes(q)
  )
})

function openCreateModal() {
  editingExercise.value = null
  Object.assign(form, { nombre: '', descripcion: '', grupoMuscular: '', videoUrl: '' })
  showModal.value = true
}

function openEditModal(exercise) {
  editingExercise.value = exercise
  Object.assign(form, { nombre: exercise.nombre, descripcion: exercise.descripcion || '', grupoMuscular: exercise.grupoMuscular, videoUrl: exercise.videoUrl || '' })
  showModal.value = true
}

function confirmDelete(exercise) {
  deletingExercise.value = exercise
  showDeleteModal.value = true
}

async function handleSubmit() {
  saving.value = true
  try {
    if (editingExercise.value) {
      await store.updateExercise(editingExercise.value.id, form)
      success('Ejercicio actualizado')
    } else {
      await store.createExercise(form)
      success('Ejercicio creado')
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
    await store.deleteExercise(deletingExercise.value.id)
    success('Ejercicio eliminado')
    showDeleteModal.value = false
  } catch (err) {
    showError(err.response?.data?.error || 'Error al eliminar')
  } finally {
    saving.value = false
  }
}

onMounted(() => store.fetchExercises())
</script>
