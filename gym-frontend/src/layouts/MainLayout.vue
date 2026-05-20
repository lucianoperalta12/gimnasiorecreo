<template>
  <div v-if="!authStore.hasRole('Terminal')" class="min-h-screen flex">
    <!-- Sidebar -->
    <aside
      class="fixed inset-y-0 left-0 z-50 w-72 bg-[#080808] border-r border-dark-900 transform transition-transform duration-300 lg:translate-x-0 shadow-2xl flex flex-col"
      :class="sidebarOpen ? 'translate-x-0' : '-translate-x-full'"
    >
      <!-- Logo Container (Oculto en móvil) -->
      <div class="flex-none hidden lg:flex py-2 flex-col items-center justify-center bg-black">
        <div class="relative w-50 h-24 flex items-center justify-center overflow-hidden cursor-pointer" @click="router.push('/')">
          <img :src="logoUrl" alt="Logo" class="max-w-full max-h-full object-contain" />
        </div>
      </div>

      <!-- Navigation -->
      <nav class="flex-1 overflow-y-auto p-3 space-y-4 custom-scrollbar">
        <div v-for="(group, idx) in menuGroups" :key="group.id" class="space-y-1.5">
          <!-- Separator for groups after the first one -->
          <div v-if="idx > 0" class="border-t border-dark-900/40 my-3 mx-2" />
          
          <!-- Group Title -->
          <div v-if="group.title" class="px-4 pb-1 text-[10px] font-black uppercase tracking-[0.2em] text-dark-500">
            {{ group.title }}
          </div>

          <!-- Group Items -->
          <router-link
            v-for="item in group.items"
            :key="item.to"
            :to="item.to"
            class="flex items-center gap-4 px-4 py-3 rounded-xl text-[15px] font-semibold transition-all duration-300 group"
            :class="$route.path === item.to || $route.path.startsWith(item.to + '/')
              ? 'bg-primary-600/10 text-primary-600'
              : 'text-dark-400 hover:text-dark-200 hover:bg-dark-900/50'"
            @click="sidebarOpen = false"
          >
            <component :is="item.icon" class="w-6 h-6 transition-colors" :class="$route.path === item.to ? 'text-primary-600' : 'text-dark-400 group-hover:text-dark-200'" />
            {{ item.label }}
          </router-link>
        </div>
      </nav>

      <!-- User info at bottom -->
      <div class="flex-none p-6 border-t border-dark-900 bg-black/40 backdrop-blur-md">
        <div class="flex items-center justify-between">
          <div class="flex items-center gap-3">
            <div class="relative">
              <div class="w-12 h-12 rounded-full border-2 border-primary-500/30 p-0.5">
                <div class="w-full h-full rounded-full bg-dark-800 flex items-center justify-center text-lg font-black text-white">
                  {{ user?.nombre?.charAt(0)?.toUpperCase() || 'A' }}
                </div>
              </div>
              <div class="absolute bottom-0 right-0 w-3.5 h-3.5 bg-emerald-600 border-2 border-dark-950 rounded-full "></div>
            </div>
            <div class="flex-1 min-w-0">
              <p class="text-sm font-black text-white truncate leading-none mb-1">{{ user?.nombre || 'Admin' }}</p>
              <p class="text-[11px] font-bold text-dark-600 uppercase tracking-wider">{{ user?.rol || 'Superusuario' }}</p>
            </div>
          </div>
          
          <button @click="handleLogout" class="p-2 rounded-lg text-primary-600 hover:bg-primary-600/10 hover:text-primary-600 transition-all active:scale-90" title="Cerrar sesión">
            <svg class="w-6 h-6" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2">
              <path stroke-linecap="round" stroke-linejoin="round" d="M15.75 9V5.25A2.25 2.25 0 0013.5 3h-6a2.25 2.25 0 00-2.25 2.25v13.5A2.25 2.25 0 007.5 21h6a2.25 2.25 0 002.25-2.25V15m3 0l3-3m0 0l-3-3m3 3H9" />
            </svg>
          </button>
        </div>
      </div>
    </aside>

    <!-- Overlay -->
    <div
      v-if="sidebarOpen"
      class="fixed inset-0 bg-black/60 z-40 lg:hidden backdrop-blur-sm"
      @click="sidebarOpen = false"
    />

    <!-- Main content -->
    <div class="flex-1 lg:ml-72">
      <!-- Top bar (Optimized for Mobile) -->
      <header class="h-[70px] bg-dark-950/80 backdrop-blur-xl border-b border-dark-800/50 flex items-center px-4 sm:px-6 sticky top-0 z-40">
        <button @click="sidebarOpen = !sidebarOpen" class="lg:hidden w-10 h-10 flex items-center justify-center rounded-xl bg-dark-900 border border-dark-800 text-dark-300 active:scale-95 transition-transform mr-3">
          <svg class="w-6 h-6" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2">
            <path stroke-linecap="round" stroke-linejoin="round" d="M3.75 6.75h16.5M3.75 12h16.5m-16.5 5.25h16.5" />
          </svg>
        </button>
        
    

        <div class="flex-1 min-w-0">
          <p class="text-base font-bold text-white truncate">{{ $route.name || 'Dashboard' }}</p>
        </div>
        <!-- Mobile Logo (Sutil) -->
        <div class="lg:hidden flex items-center justify-center mr-4 py-1 cursor-pointer" @click="router.push('/')">
          <img :src="logoUrl" alt="Logo" class="h-10 w-auto object-contain opacity-80" />
        </div>

        <!-- Notifications -->
        <div class="ml-auto">
          <Teleport to="body">
            <div class="fixed top-4 right-4 z-[100] space-y-2">
              <TransitionGroup name="notification">
                <div
                  v-for="n in notifications"
                  :key="n.id"
                  class="px-4 py-3 rounded-lg shadow-xl text-sm font-medium animate-slide-in-right backdrop-blur-sm max-w-sm"
                  :class="{
                    'bg-emerald-500/90 text-white': n.type === 'success',
                    'bg-red-500/90 text-white': n.type === 'error',
                    'bg-amber-500/90 text-white': n.type === 'warning'
                  }"
                >
                  {{ n.message }}
                </div>
              </TransitionGroup>
            </div>
          </Teleport>
        </div>
      </header>

      <!-- Page content -->
      <main class="p-6">
        <router-view />
      </main>
    </div>
  </div>

  <div v-else class="h-[100dvh] w-full overflow-hidden bg-dark-950 flex flex-col">
    <header class="h-[65px] sm:h-[75px] bg-[#0c0c0c] border-b border-dark-900/60 flex items-center justify-between px-3 sm:px-8 shrink-0">
      <div class="flex items-center gap-2 sm:gap-4 min-w-0">
        <img v-if="logoUrl" :src="logoUrl" alt="Logo" class="h-8 sm:h-10 w-auto object-contain opacity-90 flex-shrink-0" />
        <div class="min-w-0">
          <p class="text-sm sm:text-lg font-black text-white uppercase tracking-widest leading-none mb-1 truncate">{{ user?.gymNombre || 'Terminal' }}</p>
          <p class="text-[9px] sm:text-[11px] text-dark-500 uppercase tracking-[0.2em] font-bold truncate">Terminal de asistencia</p>
        </div>
      </div>

      <div class="flex items-center gap-3 sm:gap-6 flex-shrink-0">
        <div class="flex items-center gap-2 text-right">
          <svg class="w-4 h-4 sm:w-5 sm:h-5 text-dark-500" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2.5">
            <path stroke-linecap="round" stroke-linejoin="round" d="M12 6v6h4.5m4.5 0a9 9 0 1 1-18 0 9 9 0 0 1 18 0Z" />
          </svg>
          <div>
            <p class="text-sm sm:text-base font-black text-white leading-none mb-0.5">{{ timeString }}</p>
            <p class="text-[9px] sm:text-[10px] font-bold text-dark-500 tracking-widest">{{ dateString }}</p>
          </div>
        </div>
        
        <button @click="handleLogout" class="p-1.5 sm:p-2 rounded-lg text-primary-600 hover:bg-primary-600/10 transition-all active:scale-90" title="Cerrar sesión">
          <svg class="w-5 h-5 sm:w-6 sm:h-6" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2">
            <path stroke-linecap="round" stroke-linejoin="round" d="M15.75 9V5.25A2.25 2.25 0 0013.5 3h-6a2.25 2.25 0 00-2.25 2.25v13.5A2.25 2.25 0 007.5 21h6a2.25 2.25 0 002.25-2.25V15m3 0l3-3m0 0l-3-3m3 3H9" />
          </svg>
        </button>
      </div>
    </header>

    <main class="flex-1 flex flex-col p-4 sm:p-6 overflow-hidden">
      <router-view />
    </main>
  </div>

  <FloatingStopwatch v-if="!authStore.hasRole('Terminal')" />

  <AppModal v-model="showInitialPasswordModal" title="Cambiar contraseña inicial" size="sm">
    <form class="space-y-4" @submit.prevent="changeInitialPassword">
      <AppInput
        v-model="initialPassword"
        id="initial-password"
        label="Nueva contraseña"
        type="password"
        placeholder="Nueva contraseña"
        required
      />
      <p v-if="initialPasswordError" class="text-sm text-red-400">{{ initialPasswordError }}</p>
      <div class="flex justify-end">
        <button class="btn-primary" type="submit" :disabled="changingInitialPassword || !initialPassword">
          {{ changingInitialPassword ? 'Guardando...' : 'Guardar' }}
        </button>
      </div>
    </form>
  </AppModal>
</template>

<script setup>
import { ref, computed, h, watch, onMounted, onUnmounted } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth.store'
import { useNotification } from '@/composables/useNotification'
import FloatingStopwatch from '@/components/ui/FloatingStopwatch.vue'
import AppModal from '@/components/ui/AppModal.vue'
import AppInput from '@/components/ui/AppInput.vue'
import { usersApi } from '@/api/users.api'

const router = useRouter()
const authStore = useAuthStore()
const { notifications } = useNotification()
const sidebarOpen = ref(false)
const logoUrl = computed(() => {
  if (authStore.hasRole('Superusuario')) return '/logo.png'
  return authStore.user?.gymLogoUrl || '/logo.png'
})
const showInitialPasswordModal = ref(false)
const initialPassword = ref('')
const initialPasswordError = ref('')
const changingInitialPassword = ref(false)

const user = computed(() => authStore.user)

const timeString = ref('')
const dateString = ref('')

function updateClock() {
  const now = new Date()
  timeString.value = now.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })
  dateString.value = now.toLocaleDateString('es-AR', { day: '2-digit', month: '2-digit', year: 'numeric' })
}

let clockInterval = null
onMounted(() => {
  updateClock()
  clockInterval = setInterval(updateClock, 1000)
})

onUnmounted(() => {
  if (clockInterval) clearInterval(clockInterval)
})

// SVG icon components
const IconDashboard = (_, { attrs }) => h('svg', { ...attrs, fill: 'none', viewBox: '0 0 24 24', stroke: 'currentColor', 'stroke-width': '2' }, [
  h('path', { 'stroke-linecap': 'round', 'stroke-linejoin': 'round', d: 'M4 6a2 2 0 012-2h2a2 2 0 012 2v2a2 2 0 01-2 2H6a2 2 0 01-2-2V6zM14 6a2 2 0 012-2h2a2 2 0 012 2v2a2 2 0 01-2 2h-2a2 2 0 01-2-2V6zM4 16a2 2 0 012-2h2a2 2 0 012 2v2a2 2 0 01-2 2H6a2 2 0 01-2-2v-2zM14 16a2 2 0 012-2h2a2 2 0 012 2v2a2 2 0 01-2 2h-2a2 2 0 01-2-2v-2z' })
])
const IconDumbbell = (_, { attrs }) => h('svg', { ...attrs, fill: 'none', viewBox: '0 0 24 24', stroke: 'currentColor', 'stroke-width': '2' }, [
  h('path', { 'stroke-linecap': 'round', 'stroke-linejoin': 'round', d: 'M8 9l3 3-3 3m5 0h3M5 20h14a2 2 0 002-2V6a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z' })
])
const IconClipboard = (_, { attrs }) => h('svg', { ...attrs, fill: 'none', viewBox: '0 0 24 24', stroke: 'currentColor', 'stroke-width': '2' }, [
  h('path', { 'stroke-linecap': 'round', 'stroke-linejoin': 'round', d: 'M9 5H7a2 2 0 00-2 2v12a2 2 0 002 2h10a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2m-6 9h6m-6 4h6' })
])
const IconLink = (_, { attrs }) => h('svg', { ...attrs, fill: 'none', viewBox: '0 0 24 24', stroke: 'currentColor', 'stroke-width': '2' }, [
  h('path', { 'stroke-linecap': 'round', 'stroke-linejoin': 'round', d: 'M13.828 10.172a4 4 0 00-5.656 0l-4 4a4 4 0 105.656 5.656l1.102-1.101m-.758-4.899a4 4 0 005.656 0l4-4a4 4 0 00-5.656-5.656l-1.1 1.1' })
])
const IconUsers = (_, { attrs }) => h('svg', { ...attrs, fill: 'none', viewBox: '0 0 24 24', stroke: 'currentColor', 'stroke-width': '2' }, [
  h('path', { 'stroke-linecap': 'round', 'stroke-linejoin': 'round', d: 'M12 4.354a4 4 0 110 5.292M15 21H3v-1a6 6 0 0112 0v1zm0 0h6v-1a6 6 0 00-9-5.197M13 7a4 4 0 11-8 0 4 4 0 018 0z' })
])
const IconGym = IconDumbbell
const IconMembership = (_, { attrs }) => h('svg', { ...attrs, fill: 'none', viewBox: '0 0 24 24', stroke: 'currentColor', 'stroke-width': '2' }, [
  h('path', { 'stroke-linecap': 'round', 'stroke-linejoin': 'round', d: 'M15 9h3.75M15 12h3.75M15 15h3.75M4.5 19.5h15a2.25 2.25 0 002.25-2.25V6.75A2.25 2.25 0 0019.5 4.5h-15a2.25 2.25 0 00-2.25 2.25v10.5A2.25 2.25 0 004.5 19.5zm6-10.125a1.875 1.875 0 11-3.75 0 1.875 1.875 0 013.75 0zm1.294 6.336a6.721 6.721 0 01-3.17.789 6.721 6.721 0 01-3.168-.789 3.376 3.376 0 016.338 0z' })
])
const IconPlan = (_, { attrs }) => h('svg', { ...attrs, fill: 'none', viewBox: '0 0 24 24', stroke: 'currentColor', 'stroke-width': '2' }, [
  h('path', { 'stroke-linecap': 'round', 'stroke-linejoin': 'round', d: 'M9 12h3.75M9 15h3.75M9 18h3.75m.75-12h3.75M9 9h3.75M4.5 12H6m-1.5 3H6m-1.5 3H6m-1.5-9H6m-1.5-3H6m3-3h10.5a2.25 2.25 0 012.25 2.25v13.5a2.25 2.25 0 01-2.25 2.25H6a2.25 2.25 0 01-2.25-2.25V5.25A2.25 2.25 0 016 3h3z' })
])
const IconPayment = (_, { attrs }) => h('svg', { ...attrs, fill: 'none', viewBox: '0 0 24 24', stroke: 'currentColor', 'stroke-width': '2' }, [
  h('path', { 'stroke-linecap': 'round', 'stroke-linejoin': 'round', d: 'M2.25 18.75a60.07 60.07 0 0115.797 2.101c.727.198 1.453-.342 1.453-1.096V18.75M3.75 4.5v.75m0 1.5v.75m0 1.5v.75m0 1.5V15m1.5 1.5h15m-15-1.5v-1.5m0-1.5v-1.5m0-1.5v-.75m0-1.5v-.75m0-1.5V4.5m15 0v1.5m0 1.5v.75m0 1.5v.75m0 1.5V15m0 1.5h-15m15-1.5v-1.5m0-1.5v-1.5m0-1.5v-.75m0-1.5v-.75m0-1.5V4.5m-13.5 0h12m-12 0v1.5m0 1.5v.75m0 1.5v.75m0 1.5v1.5m12-1.5V4.5m0 1.5v.75m0 1.5v.75m0 1.5v1.5m-6 4.5a3 3 0 110-6 3 3 0 010 6z' })
])
const IconClock = (_, { attrs }) => h('svg', { ...attrs, fill: 'none', viewBox: '0 0 24 24', stroke: 'currentColor', 'stroke-width': '2' }, [
  h('path', { 'stroke-linecap': 'round', 'stroke-linejoin': 'round', d: 'M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z' })
])

const menuGroups = computed(() => {
  const isSuper = authStore.hasRole('Superusuario')
  const veRutinas = authStore.user?.gymVeRutinas !== false

  const groups = []

  // 1. General/Inicio
  const general = {
    id: 'general',
    title: null,
    items: [
      { to: '/', label: 'Panel Principal', icon: IconDashboard, roles: null },
      { to: '/my-membership', label: 'Mi Membresía', icon: IconMembership, roles: ['Alumno'] }
    ].filter(item => !item.roles || authStore.hasRole(...item.roles))
  }
  if (general.items.length > 0) groups.push(general)

  // 2. Gestión Comercial
  const comercial = {
    id: 'comercial',
    title: 'Gestión Comercial',
    items: [
      { to: '/memberships', label: 'Membresías', icon: IconMembership, roles: ['Superusuario', 'Administrativo'] },
      { to: '/membership-plans', label: 'Planes', icon: IconPlan, roles: ['Superusuario', 'Administrativo'] },
      { to: '/payments', label: 'Pagos', icon: IconPayment, roles: ['Superusuario', 'Administrativo'] },
      { to: '/ingresos', label: 'Ingresos', icon: IconClock, roles: ['Superusuario', 'Administrativo'] }
    ].filter(item => !item.roles || authStore.hasRole(...item.roles))
  }
  if (comercial.items.length > 0) groups.push(comercial)

  // 3. Entrenamiento
  if (isSuper || veRutinas) {
    const entrenamiento = {
      id: 'entrenamiento',
      title: 'Entrenamiento',
      items: [
        { to: '/exercises', label: 'Ejercicios', icon: IconDumbbell, roles: ['Profesor', 'Superusuario', 'Administrativo'] },
        { to: '/routines', label: 'Rutinas', icon: IconClipboard, roles: ['Profesor', 'Superusuario', 'Administrativo'] },
        { to: '/assignments', label: 'Asignaciones', icon: IconLink, roles: ['Profesor', 'Superusuario', 'Administrativo'] }
      ].filter(item => !item.roles || authStore.hasRole(...item.roles))
    }
    if (entrenamiento.items.length > 0) groups.push(entrenamiento)
  }

  // 4. Usuarios
  const usuarios = {
    id: 'usuarios',
    title: 'Usuarios',
    items: [
      { to: '/users', label: 'Usuarios', icon: IconUsers, roles: ['Superusuario', 'Administrativo'] }
    ].filter(item => !item.roles || authStore.hasRole(...item.roles))
  }
  if (usuarios.items.length > 0) groups.push(usuarios)

  // 5. Configuración
  const config = {
    id: 'config',
    title: 'Configuración',
    items: [
      { to: '/gyms', label: 'Gimnasios', icon: IconGym, roles: ['Superusuario'] }
    ].filter(item => !item.roles || authStore.hasRole(...item.roles))
  }
  if (config.items.length > 0) groups.push(config)

  return groups
})

watch(
  () => authStore.user?.debeCambiarPassword,
  value => { showInitialPasswordModal.value = !!value },
  { immediate: true }
)

async function changeInitialPassword() {
  initialPasswordError.value = ''
  changingInitialPassword.value = true
  try {
    await usersApi.changeInitialPassword(initialPassword.value)
    authStore.setUser({ ...authStore.user, debeCambiarPassword: false })
    showInitialPasswordModal.value = false
    initialPassword.value = ''
  } catch (err) {
    initialPasswordError.value = err.response?.data?.error || 'No se pudo actualizar la contraseña'
  } finally {
    changingInitialPassword.value = false
  }
}

async function handleLogout() {
  await authStore.logout()
  router.push('/login')
}
</script>

<style scoped>
.notification-enter-active {
  transition: all 0.3s ease-out;
}
.notification-leave-active {
  transition: all 0.2s ease-in;
}
.notification-enter-from {
  opacity: 0;
  transform: translateX(40px);
}
.notification-leave-to {
  opacity: 0;
  transform: translateX(40px);
}
</style>
