<template>
  <div class="animate-fade-in">
    <div v-if="authStore.hasRole('Terminal')" class="min-h-[calc(100vh-16rem)] flex flex-col items-center justify-center p-2 sm:p-4">
      <div class="w-full max-w-3xl text-center mb-6 sm:mb-8">
        <h2 class="text-xs sm:text-base font-black tracking-[0.25em] text-dark-400 uppercase">
          Ingresá el DNI del Alumno
        </h2>
        <div class="w-12 h-1 bg-primary-500 mx-auto mt-3 sm:mt-4 rounded-full"></div>
      </div>

      <div class="w-full max-w-3xl">
        <form @submit.prevent="registrarIngreso" class="space-y-4">
          <div 
            class="flex items-center gap-3 sm:gap-4 px-4 py-3.5 sm:px-6 sm:py-6 rounded-[1.5rem] sm:rounded-[2rem] border-2 bg-[#090909]/45 backdrop-blur-md transition-all duration-300"
            :class="inputFocused ? 'border-primary-500/80 shadow-[0_0_25px_rgba(255,102,0,0.15)]' : 'border-dark-800/80'"
          >
            <!-- ID Card Icon -->
            <svg class="w-7 h-7 sm:w-10 sm:h-10 text-dark-600 flex-shrink-0" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="1.5">
              <path stroke-linecap="round" stroke-linejoin="round" d="M15 9h3.75M15 12h3.75M15 15h3.75M4.5 19.5h15a2.25 2.25 0 002.25-2.25V6.75A2.25 2.25 0 0019.5 4.5h-15a2.25 2.25 0 00-2.25 2.25v10.5A2.25 2.25 0 004.5 19.5zm6-10.125a1.875 1.875 0 11-3.75 0 1.875 1.875 0 013.75 0zm1.294 6.336a6.721 6.721 0 01-3.17.789 6.721 6.721 0 01-3.168-.789 3.376 3.376 0 016.338 0z" />
            </svg>
            
            <!-- Huge Input Field -->
            <input
              ref="dniInput"
              v-model="terminalDni"
              type="text"
              class="w-full bg-transparent border-none text-white text-3xl sm:text-6xl font-black text-center tracking-[0.15em] focus:ring-0 focus:outline-none placeholder-dark-800 p-0"
              placeholder="00000000"
              autocomplete="off"
              @focus="inputFocused = true"
              @blur="inputFocused = false"
            />
          </div>

          <!-- Hidden Submit Button (keeps form submission working) -->
          <button type="submit" class="hidden" :disabled="terminalLoading || !terminalDni.trim()"></button>
        </form>

        <!-- Under Input Message / Tip -->
        <div class="flex items-center justify-center gap-2 text-[10px] sm:text-xs text-dark-500 mt-4 sm:mt-6 tracking-wide uppercase">
          <svg class="w-3.5 h-3.5 sm:w-4 sm:h-4 text-dark-600" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2">
            <path stroke-linecap="round" stroke-linejoin="round" d="M3 10h18M7 15h1m4 0h1m-7 4h12a3 3 0 003-3V8a3 3 0 00-3-3H6a3 3 0 00-3 3v8a3 3 0 003 3z" />
          </svg>
          Presioná <span class="font-black text-primary-500">ENTER</span> para validar
        </div>

        <!-- Alert messages in matching minimalist style -->
        <Transition name="fade-slide">
          <div 
            v-if="terminalMessage" 
            class="mt-6 sm:mt-8 rounded-[1.5rem] sm:rounded-[2rem] border p-4 sm:p-6 text-center backdrop-blur-md shadow-lg animate-fade-in" 
            :class="terminalSuccess 
              ? 'border-emerald-500/25 bg-emerald-500/5 text-emerald-400' 
              : 'border-red-500/25 bg-red-500/5 text-red-400'"
          >
            <p class="text-sm sm:text-lg font-black tracking-wide uppercase leading-snug">{{ terminalMessage }}</p>
            <p v-if="terminalDetail" class="text-xs sm:text-sm mt-1.5 sm:mt-2 text-dark-400 font-medium leading-relaxed">{{ terminalDetail }}</p>
          </div>
        </Transition>
      </div>
    </div>

    <template v-else>
      <div class="mb-8">
        <h1 class="page-title">Bienvenido, {{ user?.nombre }}</h1>
        <p class="page-subtitle">{{ greetingMessage }}</p>
      </div>

      <div v-if="authStore.hasRole('Profesor', 'Superusuario', 'Administrativo')" class="grid grid-cols-2 lg:grid-cols-4 gap-3 mb-8">
        <router-link
          v-for="stat in stats"
          :key="stat.label"
          :to="stat.path || '/'"
          class="p-5 rounded-[2rem] bg-dark-900/40 border border-dark-800/50 backdrop-blur-md group hover:border-primary-500/30 hover:bg-dark-800/60 active:scale-[0.98] transition-all duration-300 relative overflow-hidden"
        >
          <div class="flex flex-col gap-4 relative z-10">
            <div class="w-12 h-12 rounded-2xl flex items-center justify-center text-xl transition-transform duration-500 group-hover:scale-110" :class="stat.bgColor">
              {{ stat.icon }}
            </div>
            <div>
              <div class="flex items-baseline gap-2">
                <p class="text-3xl font-black text-white leading-none tracking-tight">{{ stat.value }}</p>
              </div>
              <p class="text-[11px] text-dark-400 uppercase tracking-[0.2em] font-bold mt-2 group-hover:text-dark-200 transition-colors">{{ stat.label }}</p>
            </div>
          </div>
        </router-link>
      </div>

      <div v-if="authStore.hasRole('Profesor', 'Superusuario', 'Administrativo')" class="mb-8">
        <h2 class="text-sm font-bold text-dark-500 uppercase tracking-[0.2em] mb-4 ml-1">Accesos Rápidos</h2>
        <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
          <router-link to="/assignments" class="group flex items-center p-4 rounded-2xl bg-dark-900/40 border border-dark-800/50 hover:bg-dark-800/60 hover:border-primary-500/30 transition-all duration-300">
            <div class="w-10 h-10 rounded-xl bg-emerald-500/10 text-emerald-500 flex items-center justify-center mr-4">
              <svg class="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 4v16m8-8H4" /></svg>
            </div>
            <div><h3 class="text-sm font-bold text-white">Asignar Rutina</h3><p class="text-xs text-dark-500">Vincular ejercicios a alumnos</p></div>
          </router-link>

          <router-link to="/routines/new" class="group flex items-center p-4 rounded-2xl bg-dark-900/40 border border-dark-800/50 hover:bg-dark-800/60 hover:border-primary-500/30 transition-all duration-300">
            <div class="w-10 h-10 rounded-xl bg-blue-500/10 text-blue-500 flex items-center justify-center mr-4">
              <svg class="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 13h6m-3-3v6m5 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" /></svg>
            </div>
            <div><h3 class="text-sm font-bold text-white">Nueva Rutina</h3><p class="text-xs text-dark-500">Crear plan de entrenamiento</p></div>
          </router-link>

          <router-link v-if="authStore.hasRole('Superusuario', 'Administrativo')" to="/memberships" class="group flex items-center p-4 rounded-2xl bg-dark-900/40 border border-dark-800/50 hover:bg-dark-800/60 hover:border-primary-500/30 transition-all duration-300">
            <div class="w-10 h-10 rounded-xl bg-amber-500/10 text-amber-500 flex items-center justify-center mr-4">
              <svg class="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2"><path stroke-linecap="round" stroke-linejoin="round" d="M15 9h3.75M15 12h3.75M15 15h3.75M4.5 19.5h15a2.25 2.25 0 002.25-2.25V6.75A2.25 2.25 0 0019.5 4.5h-15a2.25 2.25 0 00-2.25 2.25v10.5A2.25 2.25 0 004.5 19.5zm6-10.125a1.875 1.875 0 11-3.75 0 1.875 1.875 0 013.75 0zm1.294 6.336a6.721 6.721 0 01-3.17.789 6.721 6.721 0 01-3.168-.789 3.376 3.376 0 016.338 0z" /></svg>
            </div>
            <div><h3 class="text-sm font-bold text-white">Membresías</h3><p class="text-xs text-dark-500">Alumnos y pagos</p></div>
          </router-link>

          <router-link v-if="authStore.hasRole('Superusuario', 'Administrativo')" to="/ingresos" class="group flex items-center p-4 rounded-2xl bg-dark-900/40 border border-dark-800/50 hover:bg-dark-800/60 hover:border-primary-500/30 transition-all duration-300">
            <div class="w-10 h-10 rounded-xl bg-rose-500/10 text-rose-400 flex items-center justify-center mr-4">
              <svg class="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8v4l3 3M12 3a9 9 0 100 18 9 9 0 000-18z" /></svg>
            </div>
            <div><h3 class="text-sm font-bold text-white">Ingresos</h3><p class="text-xs text-dark-500">Accesos registrados</p></div>
          </router-link>
        </div>
      </div>

      <div v-if="authStore.hasRole('Alumno')" class="space-y-8">
        <router-link v-if="myAccess" to="/my-membership" class="card p-5 max-w-2xl block hover:border-primary-500/30 transition-colors">
          <div class="flex items-start justify-between gap-4">
            <div>
              <p class="text-xs uppercase tracking-widest text-dark-500 font-bold mb-2">Tu membresía</p>
              <span :class="accessStatusBadgeClass(myAccess.estadoAcceso)" class="text-sm px-3 py-1">{{ myAccess.estadoAcceso }}</span>
              <p v-if="myAccess.planNombre" class="text-sm text-dark-400 mt-3">{{ myAccess.planNombre }} · vence {{ formatDate(myAccess.fechaVencimiento) }}</p>
            </div>
          </div>
        </router-link>
      </div>
    </template>
  </div>
</template>

<script setup>
import { computed, nextTick, onMounted, ref } from 'vue'
import { useAuthStore } from '@/stores/auth.store'
import { useRoutineStore } from '@/stores/routine.store'
import { useUserStore } from '@/stores/user.store'
import { useMembershipStore } from '@/stores/membership.store'
import { accessStatusBadgeClass, formatDate } from '@/constants/membershipStatus'
import { ingresosApi } from '@/api/ingresos.api'

const authStore = useAuthStore()
const routineStore = useRoutineStore()
const userStore = useUserStore()
const membershipStore = useMembershipStore()
const myAccess = computed(() => membershipStore.myAccess)
const user = computed(() => authStore.user)
const assignmentSummary = computed(() => routineStore.assignmentSummary)
const dniInput = ref(null)
const terminalDni = ref('')
const terminalLoading = ref(false)
const terminalMessage = ref('')
const terminalDetail = ref('')
const terminalSuccess = ref(false)
const inputFocused = ref(false)

const greetingMessage = computed(() => {
  const role = user.value?.rol
  if (role === 'Superusuario') return 'Panel de administracion del gimnasio'
  if (role === 'Administrativo') return 'Gestion de gimnasio, profesores y alumnos'
  if (role === 'Profesor') return 'Gestion de rutinas y alumnos'
  if (role === 'Terminal') return 'Registro de asistencia del gimnasio'
  return 'Revisa tus rutinas de entrenamiento'
})

const stats = ref([
  { icon: '🏋️', label: 'Ejercicios', value: '-', bgColor: 'bg-primary-500/10 text-primary-400', path: '/exercises' },
  { icon: '📋', label: 'Rutinas', value: '-', bgColor: 'bg-primary-600/10 text-primary-400', path: '/routines' }
])

if (authStore.hasRole('Superusuario', 'Administrativo')) {
  stats.value.push(
    { icon: '👥', label: 'Alumnos', value: '-', bgColor: 'bg-primary-700/10 text-primary-400', path: '/users' },
    { icon: '🪪', label: 'Membresías activas', value: '-', bgColor: 'bg-amber-500/10 text-amber-400', path: '/memberships' }
  )
}

function updateDashboardStats() {
  stats.value[0].value = assignmentSummary.value.ejerciciosCount || routineStore.exercises.length
  stats.value[1].value = assignmentSummary.value.rutinasCount || routineStore.routines.length
  if (authStore.hasRole('Superusuario', 'Administrativo') && stats.value.length > 2) {
    stats.value[2].value = userStore.users.filter(x => x.rol === 'Alumno' && x.activo).length
    stats.value[3].value = membershipStore.memberships.length
  }
}

async function registrarIngreso() {
  terminalLoading.value = true
  terminalMessage.value = ''
  terminalDetail.value = ''
  try {
    const { data } = await ingresosApi.registrar(terminalDni.value)
    terminalSuccess.value = true
    terminalMessage.value = `Ingreso registrado para ${data.alumnoNombreCompleto}`
    terminalDetail.value = `${data.tipoMembresia} · ${new Date(data.fechaHora).toLocaleString()}`
    terminalDni.value = ''
  } catch (err) {
    terminalSuccess.value = false
    terminalMessage.value = err.response?.data?.error || 'No se pudo registrar el ingreso'
  } finally {
    terminalLoading.value = false
    await nextTick()
    dniInput.value?.focus()
  }
}

onMounted(async () => {
  if (authStore.hasRole('Terminal')) {
    await nextTick()
    dniInput.value?.focus()
    return
  }

  if (authStore.hasRole('Superusuario', 'Administrativo')) {
    await Promise.allSettled([
      routineStore.fetchExercises(),
      routineStore.fetchRoutines(),
      routineStore.fetchAssignmentSummary(),
      userStore.fetchUsers(),
      membershipStore.fetchMemberships({ estado: 'Activa' })
    ])
    updateDashboardStats()
  }
})
</script>
