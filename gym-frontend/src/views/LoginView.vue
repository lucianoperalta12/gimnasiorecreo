<template>
  <div class="min-h-screen flex items-center justify-center p-4 bg-dark-950 relative overflow-hidden">
    <!-- Background Accents -->
    <div class="absolute top-[-10%] left-[-10%] w-[40%] h-[40%] bg-primary-600/10 rounded-full blur-[120px] animate-pulse"></div>
    <div class="absolute bottom-[-10%] right-[-10%] w-[40%] h-[40%] bg-primary-900/10 rounded-full blur-[120px] animate-pulse" style="animation-delay: 2s"></div>

    <div class="relative w-full max-w-md animate-fade-in">
      <!-- Logo Container -->
      <div class="text-center mb-10 flex flex-col items-center">
        <div class="relative group">
          <div class="absolute -inset-1 bg-gradient-to-r from-primary-600 to-primary-400 rounded-2xl blur opacity-20 group-hover:opacity-40 transition duration-1000 group-hover:duration-200"></div>
          <img 
            src="/logo.png" 
            alt="Logo" 
            class="relative h-30 w-auto object-contain transition-transform duration-500 group-hover:scale-105" 
          />
        </div>
      </div>

      <!-- Login Card -->
      <div class="card p-8 sm:p-10 border border-dark-800 shadow-2xl bg-dark-900/60 backdrop-blur-xl rounded-[2.5rem] relative overflow-hidden group">
        <!-- Subtle inner glow -->
        <div class="absolute top-0 left-0 w-full h-1 bg-gradient-to-r from-transparent via-primary-500/20 to-transparent"></div>
        
        <form @submit.prevent="handleLogin" class="space-y-6 relative z-10">
          <div class="space-y-1">
            <AppInput
              id="login-user"
              v-model="form.username"
              label="Acceso al sistema"
              placeholder="Email, DNI o Nombre"
              :error="errors.username"
              class="!bg-dark-950/50 !border-dark-800 focus:!border-primary-500/50 transition-all"
            />
          </div>
          
          <div class="space-y-1">
            <AppInput
              id="login-password"
              v-model="form.password"
              label="Contraseña"
              type="password"
              placeholder="••••••••"
              :error="errors.password"
              class="!bg-dark-950/50 !border-dark-800 focus:!border-primary-500/50 transition-all"
            />
          </div>

          <div v-if="error" class="p-4 rounded-xl bg-red-500/10 border border-red-500/20 text-red-400 text-sm font-medium animate-shake text-center">
            {{ error }}
          </div>

          <button
            type="submit"
            :disabled="loading"
            class="w-full py-4 px-6 bg-gradient-to-r from-primary-600 to-primary-500 hover:from-primary-500 hover:to-primary-400 text-white font-black uppercase tracking-widest rounded-2xl shadow-lg shadow-primary-600/20 active:scale-[0.98] transition-all duration-300 disabled:opacity-50 disabled:cursor-not-allowed group"
          >
            <div class="flex items-center justify-center gap-3">
              <span v-if="loading" class="w-5 h-5 border-2 border-white/30 border-t-white rounded-full animate-spin"></span>
              <span v-else>Entrar</span>
              <svg v-if="!loading" class="w-5 h-5 group-hover:translate-x-1 transition-transform" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2.5">
                <path stroke-linecap="round" stroke-linejoin="round" d="M13 7l5 5m0 0l-5 5m5-5H6" />
              </svg>
            </div>
          </button>
        </form>
      </div>
      
      <!-- Footer Info -->
      <p class="text-center mt-8 text-dark-600 text-xs font-bold uppercase tracking-[0.3em]">
        &copy; {{ new Date().getFullYear() }} Fit Center System
      </p>
    </div>
  </div>
</template>

<script setup>
import { reactive, ref } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth.store'
import AppInput from '@/components/ui/AppInput.vue'

const router = useRouter()
const authStore = useAuthStore()

const loading = ref(false)
const error = ref('')
const form = reactive({
  username: '',
  password: ''
})

const errors = reactive({
  username: '',
  password: ''
})

async function handleLogin() {
  error.value = ''
  errors.username = ''
  errors.password = ''

  if (!form.username) {
    errors.username = 'El usuario es requerido'
    return
  }
  if (!form.password) {
    errors.password = 'La contraseña es requerida'
    return
  }

  loading.value = true
  try {
    await authStore.login(form.username, form.password)
    router.push('/')
  } catch (err) {
    console.error('Login error:', err)
    error.value = err.response?.data?.error || 'Credenciales inválidas o error de conexión'
  } finally {
    loading.value = false
  }
}
</script>

<style scoped>
.animate-shake {
  animation: shake 0.5s cubic-bezier(.36,.07,.19,.97) both;
}

@keyframes shake {
  10%, 90% { transform: translate3d(-1px, 0, 0); }
  20%, 80% { transform: translate3d(2px, 0, 0); }
  30%, 50%, 70% { transform: translate3d(-4px, 0, 0); }
  40%, 60% { transform: translate3d(4px, 0, 0); }
}
</style>
