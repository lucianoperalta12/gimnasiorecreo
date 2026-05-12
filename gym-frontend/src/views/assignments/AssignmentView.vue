<template>
  <div class="animate-fade-in">
    <div class="flex items-center justify-between mb-6">
      <div>
        <h1 class="page-title">Asignación de Rutinas</h1>
        <p class="page-subtitle">Vincular rutinas a alumnos</p>
      </div>
      <button 
        @click="router.push('/dashboard')" 
        class="text-[10px] font-black text-dark-400 hover:text-white transition-all uppercase tracking-[0.2em] py-2 px-4 rounded-xl border border-dark-800 hover:border-primary-500/50 bg-dark-900/50 hover:bg-dark-800 flex items-center gap-2 shadow-sm"
      >
        <svg class="w-3 h-3" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="3">
          <path stroke-linecap="round" stroke-linejoin="round" d="M10.5 19.5L3 12m0 0l7.5-7.5M3 12h18" />
        </svg>
        Volver
      </button>
    </div>

    <div class="grid grid-cols-1 lg:grid-cols-2 gap-6">
      <!-- Assign Form -->
      <div class="card space-y-6">
        <h2 class="text-lg font-semibold text-white">Nueva Asignación</h2>

        <AppSearchSelect
          v-model="selectedStudent"
          :options="studentOptions"
          label="Alumno"
          placeholder="Buscar alumno por nombre..."
        />

        <AppSearchSelect
          v-model="selectedRoutine"
          :options="routineOptions"
          label="Rutina"
          placeholder="Buscar rutina..."
        />

        <AppButton 
          :loading="assigning" 
          @click="handleAssign" 
          :disabled="!selectedStudent || !selectedRoutine"
          class="w-full py-4 text-base shadow-glow-sm"
        >
          Asignar Rutina
        </AppButton>
      </div>

      <!-- Current Assignments -->
      <div class="card space-y-4">
        <div class="flex items-start justify-between gap-3">
          <h2 class="text-lg font-semibold text-white">Asignaciones del alumno</h2>
          <span v-if="selectedStudent && !loadingAssignments" class="text-xs text-dark-400">
            {{ studentAssignments.length }} rutina<span v-if="studentAssignments.length !== 1">s</span>
          </span>
        </div>

        <div v-if="!selectedStudent" class="text-center py-8 text-dark-400 text-sm">
          Seleccioná un alumno para ver sus asignaciones
        </div>

        <LoadingSpinner v-else-if="loadingAssignments" text="Cargando..." />

        <div v-else-if="studentAssignments.length === 0" class="text-center py-8 text-dark-400 text-sm">
          Este alumno no tiene rutinas asignadas
        </div>

        <div v-else class="space-y-2">
          <div v-for="a in studentAssignments" :key="a.id"
            class="flex items-center justify-between p-3 rounded-lg bg-dark-900/50 border border-dark-700/50">
            <div>
              <p class="text-sm font-medium text-dark-200">{{ a.rutinaNombre }}</p>
              <p class="text-xs text-dark-500">Asignada el: {{ formatDate(a.fechaAsignacion) }}</p>
            </div>
            <div class="flex items-center gap-2">
              <button @click="router.push(`/routines/${a.rutinaId}`)" class="btn-ghost btn-sm flex-1">
                <span class="text-xs">Ver</span>
              </button>
              
              <button @click="handleUnassign(a.id)" class="btn-ghost btn-sm text-red-400 hover:text-red-300">
                <svg class="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="1.5">
                  <path stroke-linecap="round" stroke-linejoin="round" d="M14.74 9l-.346 9m-4.788 0L9.26 9m9.968-3.21c.342.052.682.107 1.022.166m-1.022-.165L18.16 19.673a2.25 2.25 0 01-2.244 2.077H8.084a2.25 2.25 0 01-2.244-2.077L4.772 5.79m14.456 0a48.108 48.108 0 00-3.478-.397m-12 .562c.34-.059.68-.114 1.022-.165m0 0a48.11 48.11 0 013.478-.397m7.5 0v-.916c0-1.18-.91-2.164-2.09-2.201a51.964 51.964 0 00-3.32 0c-1.18.037-2.09 1.022-2.09 2.201v.916m7.5 0a48.667 48.667 0 00-7.5 0" />
                </svg>
              </button>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, watch, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useRoutineStore } from '@/stores/routine.store'
import { useUserStore } from '@/stores/user.store'
import { useNotification } from '@/composables/useNotification'
import AppButton from '@/components/ui/AppButton.vue'
import AppSearchSelect from '@/components/ui/AppSearchSelect.vue'
import LoadingSpinner from '@/components/shared/LoadingSpinner.vue'

const router = useRouter()
const routineStore = useRoutineStore()
const userStore = useUserStore()
const { success, error: showError } = useNotification()

const students = ref([])
const routines = ref([])
const selectedStudent = ref(null)
const selectedRoutine = ref(null)
const studentAssignments = ref([])
const loadingAssignments = ref(false)
const assigning = ref(false)

// Options for search select (sorted alphabetically)
const studentOptions = computed(() => {
  return [...students.value]
    .filter(s => s.activo)
    .sort((a, b) => a.nombre.localeCompare(b.nombre))
    .map(s => ({
      id: s.id,
      label: s.nombre,
      subLabel: s.email
    }))
})

const routineOptions = computed(() => {
  return [...routines.value]
    .sort((a, b) => a.nombre.localeCompare(b.nombre))
    .map(r => ({
      id: r.id,
      label: r.nombre
    }))
})

function formatDate(d) {
  return new Date(d).toLocaleDateString('es-AR', { day: '2-digit', month: 'short', year: 'numeric' })
}

async function loadStudentAssignments() {
  if (!selectedStudent.value) return
  loadingAssignments.value = true
  try {
    studentAssignments.value = await routineStore.getStudentAssignments(selectedStudent.value)
  } finally {
    loadingAssignments.value = false
  }
}

watch(selectedStudent, loadStudentAssignments)

async function handleAssign() {
  assigning.value = true
  try {
    await routineStore.assignRoutine(selectedStudent.value, selectedRoutine.value)
    success('Rutina asignada')
    selectedRoutine.value = null
    await loadStudentAssignments()
  } catch (err) {
    showError(err.response?.data?.error || 'Error al asignar')
  } finally {
    assigning.value = false
  }
}

async function handleUnassign(id) {
  try {
    await routineStore.unassignRoutine(id)
    success('Asignación eliminada')
    await loadStudentAssignments()
  } catch (err) {
    showError(err.response?.data?.error || 'Error al quitar')
  }
}

onMounted(async () => {
  await Promise.all([
    userStore.fetchStudents(),
    routineStore.fetchRoutines()
  ])
  students.value = userStore.students
  routines.value = routineStore.routines
})
</script>
