<template>
  <div class="login-view min-h-screen flex items-center justify-center p-4 relative overflow-hidden">
    <!-- Fondo -->
    <div class="login-view__backdrop" aria-hidden="true">
      <div class="login-view__base"></div>
      <div class="login-view__glows"></div>
      <div class="login-view__lines"></div>
      <div class="login-view__vignette"></div>
      <div class="login-view__noise"></div>
    </div>

    <div class="relative z-10 w-full max-w-md animate-fade-in login-wrapper">
      <!-- Logo -->
      <div class="text-center mb-6 flex flex-col items-center">
        <div class="relative group overflow-hidden rounded-[2.2rem]">
          <div class="absolute -inset-1 bg-gradient-to-r from-primary-600 to-primary-400 rounded-[2.2rem] blur opacity-10 transition duration-700"></div>

          <img
            src="/logo.png"
            alt="Logo"
            class="relative block h-26 w-auto object-contain rounded-[2.2rem] transition-transform duration-500 group-hover:scale-[1.02]"
          />
        </div>
      </div>

      <!-- Card -->
      <div class="login-card card p-8 sm:p-10 border border-white/[0.08] bg-[#14181f]/72 backdrop-blur-2xl rounded-[2.5rem] relative overflow-hidden">
        <!-- Glow superior -->
        <div class="absolute top-0 left-0 w-full h-[1px] bg-gradient-to-r from-transparent via-primary-500/25 to-transparent"></div>

        <form @submit.prevent="handleLogin" class="login-form space-y-6 relative z-10">
          <div class="space-y-1">
            <AppInput id="login-user" v-model="form.username" label="Acceso al sistema" placeholder="Email, DNI o Nombre" :error="errors.username" />
          </div>

          <div class="space-y-1">
            <AppInput id="login-password" v-model="form.password" label="Contraseña" type="password" placeholder="••••••••" :error="errors.password" />
          </div>

          <div v-if="error" class="p-4 rounded-xl bg-red-500/10 border border-red-500/20 text-red-400 text-sm font-medium animate-shake text-center">
            {{ error }}
          </div>

          <button
            type="submit"
            :disabled="loading"
            class="login-button w-full py-4 px-6 text-white font-black uppercase tracking-widest rounded-2xl active:scale-[0.985] transition-all duration-300 disabled:opacity-50 disabled:cursor-not-allowed"
          >
            <div class="flex items-center justify-center gap-3">
              <span v-if="loading" class="w-5 h-5 border-2 border-white/30 border-t-white rounded-full animate-spin"></span>

              <template v-else>
                <span>Entrar</span>

                <svg
                  class="w-5 h-5 transition-transform duration-300 group-hover:translate-x-1"
                  fill="none"
                  viewBox="0 0 24 24"
                  stroke="currentColor"
                  stroke-width="2.5"
                >
                  <path stroke-linecap="round" stroke-linejoin="round" d="M13 7l5 5m0 0l-5 5m5-5H6" />
                </svg>
              </template>
            </div>
          </button>
        </form>
      </div>

      <!-- Footer -->
      <p class="text-center mt-8 text-[#5c6478]/35 text-[11px] font-semibold uppercase tracking-[0.25em]">
        &copy; {{ new Date().getFullYear() }} Fit Center System
      </p>
    </div>
  </div>
</template>

<script setup>
import { onMounted, reactive, ref } from 'vue';
import { useRouter } from 'vue-router';
import { useAuthStore } from '@/stores/auth.store';
import AppInput from '@/components/ui/AppInput.vue';

const router = useRouter();
const authStore = useAuthStore();

const loading = ref(false);
const error = ref('');

const form = reactive({
  username: '',
  password: '',
});

const errors = reactive({
  username: '',
  password: '',
});

onMounted(() => {
  if (!authStore.isAuthenticated) return;

  if (authStore.pendingGymSelection) {
    router.replace('/select-gym');
  } else if (authStore.hasSelectedGym) {
    router.replace('/');
  }
});

async function handleLogin() {
  error.value = '';

  errors.username = '';
  errors.password = '';

  if (!form.username) {
    errors.username = 'El usuario es requerido';
    return;
  }

  if (!form.password) {
    errors.password = 'La contraseña es requerida';
    return;
  }

  loading.value = true;

  try {
    await authStore.login(form.username, form.password);

    const target = authStore.pendingGymSelection ? '/select-gym' : '/';

    await router.replace(target);
  } catch (err) {
    console.error('Login error:', err);

    error.value = err.response?.data?.error || 'Credenciales inválidas o error de conexión';
  } finally {
    loading.value = false;
  }
}
</script>

<style scoped>
.login-view {
  background-color: #090b10;
}

/* =========================
   BACKDROP
========================= */

.login-view__backdrop {
  position: absolute;
  inset: 0;
  overflow: hidden;
  pointer-events: none;
  z-index: 0;
}

.login-view__base {
  position: absolute;
  inset: 0;
  background: linear-gradient(180deg, #0d1016 0%, #121722 45%, #070809 100%);
}

.login-view__glows {
  position: absolute;
  inset: 0;
  background:
    radial-gradient(circle at 10% 10%, rgba(255, 115, 0, 0.1) 0%, transparent 30%),
    radial-gradient(circle at 100% 85%, rgba(0, 90, 180, 0.035) 0%, transparent 32%),
    radial-gradient(ellipse 70% 45% at 50% 0%, rgba(18, 22, 30, 0.85) 0%, transparent 60%);
}

.login-view__lines {
  position: absolute;
  inset: 0;
  opacity: 0.025;

  background: repeating-linear-gradient(-45deg, rgba(255, 255, 255, 0.08) 0px, rgba(255, 255, 255, 0.08) 1px, transparent 1px, transparent 22px);
}

.login-view__vignette {
  position: absolute;
  inset: 0;

  background: radial-gradient(circle at center, transparent 45%, rgba(0, 0, 0, 0.45) 100%);
}

.login-view__noise {
  position: absolute;
  inset: 0;

  opacity: 0.02;
  mix-blend-mode: overlay;

  background-image: url("data:image/svg+xml,%3Csvg viewBox='0 0 256 256' xmlns='http://www.w3.org/2000/svg'%3E%3Cfilter id='n'%3E%3CfeTurbulence type='fractalNoise' baseFrequency='0.9' numOctaves='4' stitchTiles='stitch'/%3E%3C/filter%3E%3Crect width='100%25' height='100%25' filter='url(%23n)'/%3E%3C/svg%3E");

  background-size: 180px 180px;
}

/* =========================
   WRAPPER
========================= */

.login-wrapper {
  transform: translateY(-18px);
}

/* =========================
   CARD
========================= */

.login-card {
  box-shadow:
    0 20px 60px rgba(0, 0, 0, 0.45),
    inset 0 1px 0 rgba(255, 255, 255, 0.03);
}

/* =========================
   INPUTS
========================= */

.login-form :deep(.label) {
  color: rgba(145, 153, 172, 0.78);
  font-weight: 400;
  font-size: 0.82rem;
  letter-spacing: 0.03em;
  margin-bottom: 0.55rem;
}

.login-form :deep(.input) {
  background-color: rgba(10, 12, 16, 0.42) !important;

  border-color: rgba(255, 255, 255, 0.06) !important;

  color: rgba(225, 230, 238, 0.92) !important;

  box-shadow: inset 0 1px 0 rgba(255, 255, 255, 0.03);

  transition:
    background-color 0.25s ease,
    border-color 0.25s ease,
    transform 0.25s ease;
}

.login-form :deep(.input::placeholder) {
  color: rgba(107, 116, 136, 0.48) !important;
}

.login-form :deep(.input:focus) {
  background-color: rgba(15, 18, 24, 0.72) !important;

  border-color: rgba(255, 115, 0, 0.26) !important;

  --tw-ring-color: rgba(255, 115, 0, 0.1) !important;

  transform: translateY(-1px);
}

/* =========================
   BUTTON
========================= */

.login-button {
  background: linear-gradient(90deg, #ff6a00 0%, #ff7b00 100%);

  box-shadow: 0 10px 30px rgba(255, 106, 0, 0.1);

  transition:
    transform 0.25s ease,
    filter 0.25s ease,
    box-shadow 0.25s ease;
}

.login-button:hover {
  filter: brightness(1.03);

  box-shadow: 0 12px 35px rgba(255, 106, 0, 0.14);
}

/* =========================
   SHAKE
========================= */

.animate-shake {
  animation: shake 0.5s cubic-bezier(0.36, 0.07, 0.19, 0.97) both;
}

@keyframes shake {
  10%,
  90% {
    transform: translate3d(-1px, 0, 0);
  }

  20%,
  80% {
    transform: translate3d(2px, 0, 0);
  }

  30%,
  50%,
  70% {
    transform: translate3d(-4px, 0, 0);
  }

  40%,
  60% {
    transform: translate3d(4px, 0, 0);
  }
}
</style>
