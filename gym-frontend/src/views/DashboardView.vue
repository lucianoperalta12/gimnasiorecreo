<template>
  <div class="animate-fade-in flex-1 flex flex-col h-full">
    <div v-if="authStore.hasRole('Terminal')" class="flex-1 flex flex-col items-center justify-center w-full">
      <div class="w-full max-w-3xl text-center mb-6 sm:mb-8">
        <h2 class="text-xs sm:text-base font-black tracking-[0.25em] text-dark-400 uppercase">Ingresá el DNI del Alumno</h2>
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
              <path
                stroke-linecap="round"
                stroke-linejoin="round"
                d="M15 9h3.75M15 12h3.75M15 15h3.75M4.5 19.5h15a2.25 2.25 0 002.25-2.25V6.75A2.25 2.25 0 0019.5 4.5h-15a2.25 2.25 0 00-2.25 2.25v10.5A2.25 2.25 0 004.5 19.5zm6-10.125a1.875 1.875 0 11-3.75 0 1.875 1.875 0 013.75 0zm1.294 6.336a6.721 6.721 0 01-3.17.789 6.721 6.721 0 01-3.168-.789 3.376 3.376 0 016.338 0z"
              />
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
            :class="terminalSuccess ? 'border-emerald-500/25 bg-emerald-500/5 text-emerald-400' : 'border-red-500/25 bg-red-500/5 text-red-400'"
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
      <!-- 
      <div v-if="authStore.hasRole('Profesor', 'Superusuario', 'Administrativo')" class="grid grid-cols-2 lg:grid-cols-4 gap-3 mb-8">
        <router-link
          v-for="stat in stats"
          :key="stat.label"
          :to="stat.path || '/'"
          class="p-5 rounded-[2rem] bg-dark-900/40 border border-dark-800/50 backdrop-blur-md group hover:border-primary-500/30 hover:bg-dark-800/60 active:scale-[0.98] transition-all duration-300 relative overflow-hidden"
        >
          <div class="flex flex-col gap-4 relative z-10">
            <div
              class="w-12 h-12 rounded-2xl flex items-center justify-center text-xl transition-transform duration-500 group-hover:scale-110"
              :class="stat.bgColor"
            >
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
      </div> -->

      <div v-if="authStore.hasRole('Profesor', 'Superusuario', 'Administrativo')" class="mb-8">
        <h2 class="text-sm font-bold text-dark-500 uppercase tracking-[0.2em] mb-4 ml-1">Accesos Rápidos</h2>
        <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
          <router-link
            to="/assignments"
            class="group flex items-center p-4 rounded-2xl bg-dark-900/40 border border-dark-800/50 hover:bg-dark-800/60 hover:border-primary-500/30 transition-all duration-300"
          >
            <div class="w-10 h-10 rounded-xl bg-emerald-500/10 text-emerald-500 flex items-center justify-center mr-4">
              <svg class="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 4v16m8-8H4" />
              </svg>
            </div>
            <div>
              <h3 class="text-sm font-bold text-white">Asignar Rutina</h3>
              <p class="text-xs text-dark-500">Vincular ejercicios a alumnos</p>
            </div>
          </router-link>

          <router-link
            to="/routines/new"
            class="group flex items-center p-4 rounded-2xl bg-dark-900/40 border border-dark-800/50 hover:bg-dark-800/60 hover:border-primary-500/30 transition-all duration-300"
          >
            <div class="w-10 h-10 rounded-xl bg-blue-500/10 text-blue-500 flex items-center justify-center mr-4">
              <svg class="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path
                  stroke-linecap="round"
                  stroke-linejoin="round"
                  stroke-width="2"
                  d="M9 13h6m-3-3v6m5 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z"
                />
              </svg>
            </div>
            <div>
              <h3 class="text-sm font-bold text-white">Nueva Rutina</h3>
              <p class="text-xs text-dark-500">Crear plan de entrenamiento</p>
            </div>
          </router-link>

          <router-link
            v-if="authStore.hasRole('Superusuario', 'Administrativo')"
            to="/memberships"
            class="group flex items-center p-4 rounded-2xl bg-dark-900/40 border border-dark-800/50 hover:bg-dark-800/60 hover:border-primary-500/30 transition-all duration-300"
          >
            <div class="w-10 h-10 rounded-xl bg-amber-500/10 text-amber-500 flex items-center justify-center mr-4">
              <svg class="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2">
                <path
                  stroke-linecap="round"
                  stroke-linejoin="round"
                  d="M15 9h3.75M15 12h3.75M15 15h3.75M4.5 19.5h15a2.25 2.25 0 002.25-2.25V6.75A2.25 2.25 0 0019.5 4.5h-15a2.25 2.25 0 00-2.25 2.25v10.5A2.25 2.25 0 004.5 19.5zm6-10.125a1.875 1.875 0 11-3.75 0 1.875 1.875 0 013.75 0zm1.294 6.336a6.721 6.721 0 01-3.17.789 6.721 6.721 0 01-3.168-.789 3.376 3.376 0 016.338 0z"
                />
              </svg>
            </div>
            <div>
              <h3 class="text-sm font-bold text-white">Membresías</h3>
              <p class="text-xs text-dark-500">Alumnos y pagos</p>
            </div>
          </router-link>

          <router-link
            v-if="authStore.hasRole('Superusuario', 'Administrativo')"
            to="/ingresos"
            class="group flex items-center p-4 rounded-2xl bg-dark-900/40 border border-dark-800/50 hover:bg-dark-800/60 hover:border-primary-500/30 transition-all duration-300"
          >
            <div class="w-10 h-10 rounded-xl bg-rose-500/10 text-rose-400 flex items-center justify-center mr-4">
              <svg class="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8v4l3 3M12 3a9 9 0 100 18 9 9 0 000-18z" />
              </svg>
            </div>
            <div>
              <h3 class="text-sm font-bold text-white">Ingresos</h3>
              <p class="text-xs text-dark-500">Accesos registrados</p>
            </div>
          </router-link>
        </div>
      </div>

      <!-- Monitoreo en Tiempo Real (Solo Administrador y Superusuario) -->
      <div v-if="authStore.hasRole('Superusuario', 'Administrativo')" class="grid grid-cols-1 lg:grid-cols-3 gap-6 mb-8">
        <!-- Col 1 y 2: Ingresos Recientes -->
        <div class="card p-6 lg:col-span-2 flex flex-col h-[320px]">
          <div class="flex items-center justify-between mb-4">
            <div>
              <h2 class="text-sm font-bold text-white uppercase tracking-wider">Asistencia de Hoy</h2>
              <p class="text-xs text-dark-500">Ingresos recientes a través de la terminal</p>
            </div>
            <button @click="fetchIngresosHoy" class="p-2 rounded-lg text-dark-400 hover:text-white hover:bg-dark-900/60 transition-colors">
              <svg class="w-4 h-4" :class="{ 'animate-spin': loadingIngresos }" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2">
                <path
                  stroke-linecap="round"
                  stroke-linejoin="round"
                  d="M16.023 9.348h4.992v-.001M2.985 19.644v-4.992m0 0h4.992m-4.993 0 3.181 3.183a8.25 8.25 0 0 0 13.803-3.7M4.031 9.865a8.25 8.25 0 0 1 13.803-3.7l3.181 3.182m0-4.991v4.99"
                />
              </svg>
            </button>
          </div>

          <div class="flex-1 overflow-y-auto custom-scrollbar space-y-2.5">
            <div
              v-for="ingreso in ingresosHoy.slice(0, 5)"
              :key="ingreso.id"
              class="flex items-center justify-between p-3.5 rounded-2xl bg-dark-950/40 border border-dark-900/60 hover:border-primary-500/10 transition-all duration-300"
            >
              <div class="flex items-center gap-3">
                <div class="w-8 h-8 rounded-full bg-primary-500/10 text-primary-500 flex items-center justify-center text-xs font-black">
                  {{ ingreso.alumno?.charAt(0)?.toUpperCase() }}
                </div>
                <div>
                  <p class="text-xs font-black text-white leading-none mb-1">{{ ingreso.alumno }}</p>
                  <p class="text-[10px] text-dark-500">Plan: {{ ingreso.tipoMembresia }}</p>
                </div>
              </div>
              <div class="text-right">
                <p class="text-xs font-black text-white">{{ formatTime(ingreso.fechaHora) }}</p>
                <p class="text-[9px] text-dark-500 uppercase tracking-widest leading-none mt-0.5">{{ ingreso.terminal }}</p>
              </div>
            </div>

            <div v-if="!ingresosHoy.length" class="h-full flex flex-col items-center justify-center text-center py-6">
              <svg class="w-8 h-8 text-dark-700 mb-2" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="1.5">
                <path
                  stroke-linecap="round"
                  stroke-linejoin="round"
                  d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z"
                />
              </svg>
              <p class="text-xs text-dark-500 font-bold">Sin ingresos registrados hoy aún.</p>
            </div>
          </div>
        </div>

        <!-- Col 3: Panel Comercial Rápido (Recaudación + Alertas por Vencer) -->
        <div class="flex flex-col gap-4 h-[320px]">
          <!-- Card Recaudación del Mes -->
          <!-- <div class="p-5 rounded-[2rem] bg-dark-900/40 border border-dark-800/50 backdrop-blur-md relative overflow-hidden group shrink-0">
            <div
              class="absolute -right-4 -bottom-4 w-20 h-20 bg-emerald-500/5 rounded-full blur-xl group-hover:bg-emerald-500/10 transition-all duration-500"
            ></div>
            <p class="text-[9px] text-dark-500 uppercase tracking-[0.2em] font-black">Recaudación este Mes</p>
            <p class="text-2xl font-black text-emerald-400 mt-2 tracking-tight leading-none">
              {{ formatMoneda(pagosMesVal) }}
            </p>
          </div> -->

          <!-- Card Membresías por Vencer -->
          <div class="flex-1 card p-5 flex flex-col overflow-hidden min-h-[170px]">
            <h3 class="text-xs font-bold text-white uppercase tracking-wider mb-3">Expira en <span class="text-primary-500 font-black">7 días</span></h3>

            <div class="flex-1 overflow-y-auto custom-scrollbar space-y-2">
              <div
                v-for="membresia in membresiasPorVencer.slice(0, 3)"
                :key="membresia.id"
                class="flex items-center justify-between p-2 rounded-xl bg-dark-950/40 border border-dark-900/60"
              >
                <div class="min-w-0">
                  <p class="text-[11px] font-black text-white truncate leading-none mb-1">{{ membresia.alumnoNombre }}</p>
                  <p class="text-[9px] text-dark-500">Vence: {{ formatDate(membresia.fechaVencimiento) }}</p>
                </div>
                <router-link to="/memberships" class="text-[9px] font-black text-primary-500 hover:text-primary-400 uppercase tracking-widest flex-shrink-0">
                  Renovar
                </router-link>
              </div>

              <div v-if="!membresiasPorVencer.length" class="h-full flex flex-col items-center justify-center text-center py-4">
                <p class="text-[10px] text-dark-500 font-bold">No hay membresías por vencer próximamente.</p>
              </div>
            </div>
          </div>
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
import { computed, nextTick, onMounted, ref } from 'vue';
import { useAuthStore } from '@/stores/auth.store';
import { useRoutineStore } from '@/stores/routine.store';
import { useUserStore } from '@/stores/user.store';
import { useMembershipStore } from '@/stores/membership.store';
import { accessStatusBadgeClass, formatDate } from '@/constants/membershipStatus';
import { ingresosApi } from '@/api/ingresos.api';
import { paymentsApi } from '@/api/payments.api';

const authStore = useAuthStore();
const routineStore = useRoutineStore();
const userStore = useUserStore();
const membershipStore = useMembershipStore();
const myAccess = computed(() => membershipStore.myAccess);
const user = computed(() => authStore.user);
const assignmentSummary = computed(() => routineStore.assignmentSummary);
const dniInput = ref(null);
const terminalDni = ref('');
const terminalLoading = ref(false);
const terminalMessage = ref('');
const terminalDetail = ref('');
const terminalSuccess = ref(false);
const inputFocused = ref(false);

const ingresosHoy = ref([]);
const loadingIngresos = ref(false);
const pagosMesVal = ref(0);

const greetingMessage = computed(() => {
  const role = user.value?.rol;
  if (role === 'Superusuario') return 'Panel de administracion del gimnasio';
  if (role === 'Administrativo') return 'Gestion de gimnasio, profesores y alumnos';
  if (role === 'Profesor') return 'Gestion de rutinas y alumnos';
  if (role === 'Terminal') return 'Registro de asistencia del gimnasio';
  return 'Revisa tus rutinas de entrenamiento';
});

const stats = ref([
  { icon: '🏋️', label: 'Ejercicios', value: '-', bgColor: 'bg-primary-500/10 text-primary-400', path: '/exercises' },
  { icon: '📋', label: 'Rutinas', value: '-', bgColor: 'bg-primary-600/10 text-primary-400', path: '/routines' },
]);

if (authStore.hasRole('Superusuario', 'Administrativo')) {
  stats.value.push(
    { icon: '👥', label: 'Alumnos', value: '-', bgColor: 'bg-primary-700/10 text-primary-400', path: '/users' },
    { icon: '🪪', label: 'Membresías activas', value: '-', bgColor: 'bg-amber-500/10 text-amber-400', path: '/memberships' }
  );
}

function updateDashboardStats() {
  stats.value[0].value = assignmentSummary.value.ejerciciosCount || routineStore.exercises.length;
  stats.value[1].value = assignmentSummary.value.rutinasCount || routineStore.routines.length;
  if (authStore.hasRole('Superusuario', 'Administrativo') && stats.value.length > 2) {
    stats.value[2].value = userStore.users.filter((x) => x.rol === 'Alumno' && x.activo).length;
    stats.value[3].value = membershipStore.memberships.length;
  }
}

async function registrarIngreso() {
  terminalLoading.value = true;
  terminalMessage.value = '';
  terminalDetail.value = '';
  try {
    const { data } = await ingresosApi.registrar(terminalDni.value);
    terminalSuccess.value = true;
    terminalMessage.value = `Ingreso registrado para ${data.alumnoNombreCompleto}`;
    terminalDetail.value = `${data.tipoMembresia} · ${new Date(data.fechaHora).toLocaleString()}`;
    terminalDni.value = '';
    // Refrescar ingresos recientes tras ingreso exitoso en terminal
    if (authStore.hasRole('Superusuario', 'Administrativo')) {
      fetchIngresosHoy();
    }
  } catch (err) {
    terminalSuccess.value = false;
    terminalMessage.value = err.response?.data?.error || 'No se pudo registrar el ingreso';
  } finally {
    terminalLoading.value = false;
    await nextTick();
    dniInput.value?.focus();
  }
}

async function fetchIngresosHoy() {
  if (!authStore.hasRole('Superusuario', 'Administrativo')) return;
  loadingIngresos.value = true;
  try {
    const hoy = new Date().toISOString().slice(0, 10);
    const { data } = await ingresosApi.getAll({ fechaDesde: hoy, fechaHasta: hoy });
    ingresosHoy.value = data || [];
  } catch (err) {
    console.error(err);
  } finally {
    loadingIngresos.value = false;
  }
}

async function fetchPagosMes() {
  if (!authStore.hasRole('Superusuario', 'Administrativo')) return;
  try {
    const { data } = await paymentsApi.getAll();
    const hoy = new Date();
    const mes = hoy.getMonth();
    const año = hoy.getFullYear();
    const esteMes = (data || []).filter((p) => {
      const d = new Date(p.fechaPago);
      return d.getMonth() === mes && d.getFullYear() === año;
    });
    pagosMesVal.value = esteMes.reduce((acc, p) => acc + p.monto, 0);
  } catch (err) {
    console.error(err);
  }
}

const membresiasPorVencer = computed(() => {
  if (!authStore.hasRole('Superusuario', 'Administrativo')) return [];
  const hoy = new Date();
  const limite = new Date();
  limite.setDate(hoy.getDate() + 7);
  return membershipStore.memberships
    .filter((m) => {
      if (!m.fechaVencimiento || m.estado !== 'Activa') return false;
      const v = new Date(m.fechaVencimiento);
      return v >= hoy && v <= limite;
    })
    .sort((a, b) => new Date(a.fechaVencimiento) - new Date(b.fechaVencimiento));
});

function formatMoneda(val) {
  return new Intl.NumberFormat('es-AR', { style: 'currency', currency: 'ARS', minimumFractionDigits: 0 }).format(val);
}

function formatTime(val) {
  return new Date(val).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
}

onMounted(async () => {
  if (authStore.hasRole('Terminal')) {
    await nextTick();
    dniInput.value?.focus();
    return;
  }

  if (authStore.hasRole('Superusuario', 'Administrativo')) {
    await Promise.allSettled([
      routineStore.fetchExercises(),
      routineStore.fetchRoutines(),
      routineStore.fetchAssignmentSummary(),
      userStore.fetchUsers(),
      membershipStore.fetchMemberships({ estado: 'Activa' }),
      fetchIngresosHoy(),
      fetchPagosMes(),
    ]);
    updateDashboardStats();
  } else if (authStore.hasRole('Profesor')) {
    await Promise.allSettled([routineStore.fetchExercises(), routineStore.fetchRoutines(), routineStore.fetchAssignmentSummary()]);
    updateDashboardStats();
  }
});
</script>
