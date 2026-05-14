<template>
  <div class="min-h-screen flex">
    <!-- Sidebar -->
    <aside
      class="fixed inset-y-0 left-0 z-50 w-72 bg-[#080808] border-r border-dark-900 transform transition-transform duration-300 lg:translate-x-0 shadow-2xl"
      :class="sidebarOpen ? 'translate-x-0' : '-translate-x-full'"
    >
      <!-- Logo Container (Oculto en móvil) -->
      <div class="hidden lg:flex py-2 flex-col items-center justify-center bg-black">
        <div class="relative w-50 h-24 flex items-center justify-center overflow-hidden cursor-pointer" @click="router.push('/')">
          <img :src="logoUrl" alt="Logo" class="max-w-full max-h-full object-contain" />
        </div>
      </div>

      <!-- Navigation -->
      <nav class="p-1 space-y-2">
        <router-link
          v-for="item in navItems"
          :key="item.to"
          :to="item.to"
          class="flex items-center gap-4 px-5 py-4 rounded-xl text-[16px] font-semibold transition-all duration-300 group"
          :class="$route.path === item.to || $route.path.startsWith(item.to + '/')
            ? 'bg-primary-600/10 text-primary-600'
            : 'text-dark-400 hover:text-dark-200 hover:bg-dark-900/50'"
          @click="sidebarOpen = false"
        >
          <component :is="item.icon" class="w-6 h-6 transition-colors" :class="$route.path === item.to ? 'text-primary-600' : 'text-dark-400 group-hover:text-dark-200'" />
          {{ item.label }}
        </router-link>
      </nav>

      <!-- User info at bottom -->
      <div class="absolute bottom-0 left-0 right-0 p-6 border-t border-dark-900 bg-black/40 backdrop-blur-md">
        <div class="flex items-center justify-between">
          <div class="flex items-center gap-3">
            <div class="relative">
              <div class="w-12 h-12 rounded-full border-2 border-primary-500/30 p-0.5">
                <div class="w-full h-full rounded-full bg-dark-800 flex items-center justify-center text-lg font-black text-white">
                  {{ user?.nombre?.charAt(0)?.toUpperCase() || 'A' }}
                </div>
              </div>
              <div class="absolute bottom-0 right-0 w-3.5 h-3.5 bg-emerald-600 border-2 border-dark-950 rounded-full shadow-glow-sm"></div>
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
        <div class="lg:hidden flex items-center justify-center mr-4 py-1">
          <img :src="logoUrl" alt="Logo" class="h-10 w-auto object-contain opacity-80" />
        </div>

        <!-- Notifications -->
        <div class="ml-auto">
          <Teleport to="body">
            <div class="fixed top-4 right-4 z-50 space-y-2">
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

  <FloatingStopwatch />

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
import { ref, computed, h, watch } from 'vue'
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

const allNavItems = [
  { to: '/', label: 'Panel Principal', icon: IconDashboard, roles: null },
  { to: '/exercises', label: 'Ejercicios', icon: IconDumbbell, roles: ['Profesor', 'Superusuario', 'Administrativo'] },
  { to: '/routines', label: 'Rutinas', icon: IconClipboard, roles: ['Profesor', 'Superusuario', 'Administrativo'] },
  { to: '/assignments', label: 'Asignaciones', icon: IconLink, roles: ['Profesor', 'Superusuario', 'Administrativo'] },
  { to: '/users', label: 'Usuarios', icon: IconUsers, roles: ['Superusuario', 'Administrativo'] },
  { to: '/gyms', label: 'Gimnasios', icon: IconGym, roles: ['Superusuario'] },
]

const navItems = computed(() =>
  allNavItems.filter(item => !item.roles || authStore.hasRole(...item.roles))
)

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
