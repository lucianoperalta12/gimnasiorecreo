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
      <router-link
        v-for="stat in stats"
        :key="stat.label"
        :to="stat.path || '/'"
        class="p-5 rounded-[2rem] bg-dark-900/40 border border-dark-800/50 backdrop-blur-md group hover:border-primary-500/30 hover:bg-dark-800/60 active:scale-[0.98] transition-all duration-300 relative overflow-hidden"
      >
        <!-- Decorative gradient -->
        <div class="absolute -right-4 -top-4 w-24 h-24 bg-primary-500/5 rounded-full blur-2xl group-hover:bg-primary-500/10 transition-colors"></div>
        
        <div class="flex flex-col gap-4 relative z-10">
          <div
            class="w-12 h-12 rounded-2xl flex items-center justify-center text-xl shadow-glow-sm transition-transform duration-500 group-hover:scale-110"
            :class="stat.bgColor"
          >
            {{ stat.icon }}
          </div>
          <div>
            <div class="flex items-baseline gap-2">
              <p class="text-3xl font-black text-white leading-none tracking-tight">{{ stat.value }}</p>
              <svg class="w-4 h-4 text-dark-500 group-hover:text-primary-400 transform group-hover:translate-x-1 transition-all" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5l7 7-7 7" />
              </svg>
            </div>
            <p class="text-[11px] text-dark-400 uppercase tracking-[0.2em] font-bold mt-2 group-hover:text-dark-200 transition-colors">{{ stat.label }}</p>
          </div>
        </div>
      </router-link>
    </div>

    <!-- Quick Actions for Professor/Admin -->
    <div v-if="authStore.hasRole('Profesor', 'Superusuario')" class="mb-8">
      <h2 class="text-sm font-bold text-dark-500 uppercase tracking-[0.2em] mb-4 ml-1">Accesos Rápidos</h2>
      <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4">
        <router-link to="/assignments" class="group flex items-center p-4 rounded-2xl bg-dark-900/40 border border-dark-800/50 hover:bg-dark-800/60 hover:border-primary-500/30 transition-all duration-300">
          <div class="w-10 h-10 rounded-xl bg-emerald-500/10 text-emerald-500 flex items-center justify-center mr-4 group-hover:scale-110 transition-transform">
            <svg class="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 4v16m8-8H4" />
            </svg>
          </div>
          <div>
            <h3 class="text-sm font-bold text-white">Asignar Rutina</h3>
            <p class="text-xs text-dark-500">Vincular ejercicios a alumnos</p>
          </div>
        </router-link>

        <router-link to="/routines/new" class="group flex items-center p-4 rounded-2xl bg-dark-900/40 border border-dark-800/50 hover:bg-dark-800/60 hover:border-primary-500/30 transition-all duration-300">
          <div class="w-10 h-10 rounded-xl bg-blue-500/10 text-blue-500 flex items-center justify-center mr-4 group-hover:scale-110 transition-transform">
            <svg class="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 13h6m-3-3v6m5 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
            </svg>
          </div>
          <div>
            <h3 class="text-sm font-bold text-white">Nueva Rutina</h3>
            <p class="text-xs text-dark-500">Crear plan de entrenamiento</p>
          </div>
        </router-link>

      </div>
    </div>

   
    <div v-if="authStore.hasRole('Alumno')" class="space-y-4">
  <h2 class="text-lg font-semibold text-white">Tus Rutinas Activas</h2>
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
  { icon: '🏋️', label: 'Ejercicios', value: '-', bgColor: 'bg-primary-500/10 text-primary-400', path: '/exercises' },
  { icon: '📋', label: 'Rutinas', value: '-', bgColor: 'bg-primary-600/10 text-primary-400', path: '/routines' }
])

if (authStore.hasRole('Superusuario')) {
  stats.value.push(
    { icon: '👥', label: 'Alumnos', value: '-', bgColor: 'bg-primary-700/10 text-primary-400', path: '/users' },
    { icon: '👨‍🏫', label: 'Profesores', value: '-', bgColor: 'bg-primary-800/10 text-primary-400', path: '/users' }
  )
}

function getActiveStudentsCount() {
  return userStore.users.filter(currentUser => currentUser.rol === 'Alumno' && currentUser.activo).length
}

function getActiveProfessorsCount() {
  return userStore.users.filter(currentUser => currentUser.rol === 'Profesor' && currentUser.activo).length
}

function updateDashboardStats() {
  stats.value[0].value = assignmentSummary.value.ejerciciosCount || routineStore.exercises.length
  stats.value[1].value = assignmentSummary.value.rutinasCount || routineStore.routines.length

  if (authStore.hasRole('Superusuario') && stats.value.length > 2) {
    if (userStore.users.length > 0) {
      stats.value[2].value = getActiveStudentsCount()
      stats.value[3].value = getActiveProfessorsCount()
    } else {
      stats.value[2].value = assignmentSummary.value.alumnosCount || 0
      stats.value[3].value = assignmentSummary.value.profesoresCount || 0
    }
  }
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
