<template>
  <div class="animate-fade-in">
    <div class="mb-8">
      <h1 class="page-title">Bienvenido, {{ user?.nombre }}</h1>
      <p class="page-subtitle">{{ greetingMessage }}</p>
    </div>

    <div
      v-if="authStore.hasRole('Profesor', 'Superusuario')"
      class="grid grid-cols-2 lg:grid-cols-4 gap-3 mb-8"
    >
      <div
        v-for="stat in stats"
        :key="stat.label"
        class="p-4 rounded-3xl bg-dark-900/40 border border-dark-800/50 backdrop-blur-sm group active:scale-95 transition-all"
      >
        <div class="flex flex-col gap-3">
          <div
            class="w-10 h-10 rounded-2xl flex items-center justify-center text-base font-black tracking-wider shadow-glow-sm text-primary-100"
            :class="stat.bgColor"
          >
            {{ stat.icon }}
          </div>
          <div>
            <p class="text-2xl font-black text-white leading-none">{{ stat.value }}</p>
            <p class="text-[10px] text-dark-500 uppercase tracking-widest font-black mt-1">{{ stat.label }}</p>
          </div>
        </div>
      </div>
    </div>

    <div v-if="authStore.hasRole('Alumno')" class="space-y-4">
      <h2 class="text-lg font-semibold text-white">Tus rutinas activas</h2>
      <LoadingSpinner v-if="loadingRoutines" />
      <div v-else-if="myRoutines.length === 0" class="card text-center py-12">
        <p class="text-dark-400">No tenes rutinas asignadas todavia.</p>
        <p class="text-sm text-dark-500 mt-1">Tu profesor te asignara rutinas pronto.</p>
      </div>
      <div v-else class="grid grid-cols-1 md:grid-cols-2 gap-4">
        <router-link
          v-for="routine in myRoutines"
          :key="routine.id"
          :to="`/routines/${routine.id}`"
          class="card-hover group"
        >
          <div class="flex items-start justify-between">
            <div>
              <h3 class="font-semibold text-white group-hover:text-primary-300 transition-colors">{{ routine.nombre }}</h3>
              <p class="text-sm text-dark-400 mt-1">{{ routine.descripcion || 'Sin descripcion' }}</p>
            </div>
            <span class="badge-success">Activa</span>
          </div>
        </router-link>
      </div>
    </div>
  </div>
</template>

<script setup>
import { computed, onMounted, ref } from 'vue'
import { useAuthStore } from '@/stores/auth.store'
import { useRoutineStore } from '@/stores/routine.store'
import { useUserStore } from '@/stores/user.store'
import LoadingSpinner from '@/components/shared/LoadingSpinner.vue'

const authStore = useAuthStore()
const routineStore = useRoutineStore()
const userStore = useUserStore()

const user = computed(() => authStore.user)
const myRoutines = computed(() => routineStore.myRoutines)
const assignmentSummary = computed(() => routineStore.assignmentSummary)
const loadingRoutines = ref(false)

const greetingMessage = computed(() => {
  const role = user.value?.rol
  if (role === 'Superusuario') return 'Panel de administracion del gimnasio'
  if (role === 'Profesor') return 'Gestion de rutinas y alumnos'
  return 'Revisa tus rutinas de entrenamiento'
})

const stats = ref([
  { icon: '\u{1F3CB}\u{FE0F}', label: 'Ejercicios', value: '-', bgColor: 'bg-primary-500/20' },
  { icon: '\u{1F4CB}', label: 'Rutinas', value: '-', bgColor: 'bg-primary-600/20' },
  { icon: '\u{1F465}', label: 'Alumnos', value: '-', bgColor: 'bg-primary-700/20' },
  { icon: '\u{1F468}\u{200D}\u{1F3EB}', label: 'Profesores', value: '-', bgColor: 'bg-primary-800/20' }
])

function getActiveStudentsCount() {
  return userStore.users.filter(currentUser => currentUser.rol === 'Alumno' && currentUser.activo).length
}

function getActiveProfessorsCount() {
  return userStore.users.filter(currentUser => currentUser.rol === 'Profesor' && currentUser.activo).length
}

function updateDashboardStats() {
  stats.value[0].value = assignmentSummary.value.ejerciciosCount || routineStore.exercises.length
  stats.value[1].value = assignmentSummary.value.rutinasCount || routineStore.routines.length

  if (authStore.hasRole('Superusuario') && userStore.users.length > 0) {
    stats.value[2].value = getActiveStudentsCount()
    stats.value[3].value = getActiveProfessorsCount()
    return
  }

  stats.value[2].value = assignmentSummary.value.alumnosCount || 0
  stats.value[3].value = assignmentSummary.value.profesoresCount || 0
}

onMounted(async () => {
  if (authStore.hasRole('Alumno')) {
    loadingRoutines.value = true
    try {
      await routineStore.fetchMyRoutines()
    } finally {
      loadingRoutines.value = false
    }
  }

  if (authStore.hasRole('Profesor', 'Superusuario')) {
    const requests = [
      routineStore.fetchExercises(),
      routineStore.fetchRoutines(),
      routineStore.fetchAssignmentSummary()
    ]

    if (authStore.hasRole('Superusuario')) {
      requests.push(userStore.fetchUsers())
    }

    await Promise.allSettled(requests)
    updateDashboardStats()
  }
})
</script>
