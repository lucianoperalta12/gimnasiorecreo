<template>
  <div class="min-h-screen relative overflow-hidden bg-dark-950 px-4 py-10 sm:px-6 lg:px-8">
    <div class="absolute inset-0 bg-[radial-gradient(circle_at_top,_rgba(255,102,0,0.18),_transparent_35%),radial-gradient(circle_at_bottom_right,_rgba(14,165,233,0.16),_transparent_30%)]"></div>
    <div class="relative mx-auto w-full max-w-6xl">
      <div class="text-center mb-10">
        <p class="text-[11px] font-black uppercase tracking-[0.35em] text-primary-400">Cambio de contexto</p>
        <h1 class="mt-4 text-3xl sm:text-5xl font-black text-white tracking-tight">Elegí tu gimnasio</h1>
        <p class="mt-4 text-sm sm:text-base text-dark-400 max-w-2xl mx-auto">
          Tu cuenta tiene accesos activos en más de una sede. Seleccioná dónde querés continuar para cargar el panel correcto, los colores y el logo de ese gimnasio.
        </p>
      </div>

      <div v-if="error" class="mx-auto max-w-lg rounded-2xl border border-red-500/20 bg-red-500/10 p-5 text-red-300 text-sm text-center">
        {{ error }}
      </div>

      <div v-else-if="!gyms.length" class="mx-auto max-w-xl rounded-2xl border border-white/10 bg-dark-900/70 p-6 text-center text-dark-300">
        No hay gimnasios activos para seleccionar.
      </div>

      <div v-else class="grid gap-5 sm:grid-cols-2 xl:grid-cols-3">
        <button
          v-for="gym in gyms"
          :key="gym.gymId"
          type="button"
          class="group relative overflow-hidden rounded-[2rem] border border-white/10 bg-dark-900/70 p-5 text-left shadow-2xl backdrop-blur-xl transition duration-300 hover:-translate-y-1 hover:border-white/20"
          :disabled="submitting"
          @click="handleSelect(gym.gymId)"
        >
          <div class="absolute inset-0 opacity-90" :style="{ background: `linear-gradient(135deg, ${gym.colorPrincipalHex}22 0%, rgba(8,8,8,0.88) 58%)` }"></div>
          <div class="relative flex items-start gap-4">
            <div
              class="flex h-16 w-16 items-center justify-center rounded-2xl border border-white/10 bg-black/30 p-2 shadow-inner"
              :style="{ boxShadow: `0 0 0 1px ${gym.colorPrincipalHex}55, 0 20px 40px rgba(0,0,0,.35)` }"
            >
              <img
                v-if="gym.logoUrl"
                :src="gym.logoUrl"
                :alt="gym.gymNombre"
                class="max-h-full max-w-full object-contain"
              />
              <span v-else class="text-lg font-black" :style="{ color: gym.colorPrincipalHex }">
                {{ gym.gymNombre?.charAt(0)?.toUpperCase() || 'G' }}
              </span>
            </div>

            <div class="min-w-0 flex-1">
              <div class="flex items-center justify-between gap-3">
                <h2 class="truncate text-lg font-black text-white">{{ gym.gymNombre }}</h2>
                <span class="shrink-0 rounded-full px-2.5 py-1 text-[10px] font-black uppercase tracking-widest text-white/90" :style="{ backgroundColor: gym.colorPrincipalHex }">
                  {{ gym.rol }}
                </span>
              </div>
              <p class="mt-2 text-sm text-dark-400">
                Acceso contextual activo para esta sede.
              </p>
            </div>
          </div>

          <div class="relative mt-6 flex items-center justify-between">
            <div class="h-1.5 flex-1 rounded-full bg-white/5">
              <div class="h-1.5 rounded-full transition-all duration-300 group-hover:w-full" :style="{ width: '48%', backgroundColor: gym.colorPrincipalHex }"></div>
            </div>
            <span class="ml-3 text-[11px] font-black uppercase tracking-[0.3em]" :style="{ color: gym.colorPrincipalHex }">Entrar</span>
          </div>
        </button>
      </div>
    </div>
  </div>
</template>

<script setup>
import { computed, ref } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth.store'

const router = useRouter()
const authStore = useAuthStore()
const submitting = ref(false)
const error = ref('')

const gyms = computed(() => authStore.gyms || [])

async function handleSelect(gymId) {
  error.value = ''
  submitting.value = true
  try {
    await authStore.selectGym(gymId)
    await router.replace('/')
  } catch (err) {
    error.value = err.response?.data?.error || 'No se pudo seleccionar el gimnasio'
  } finally {
    submitting.value = false
  }
}
</script>
