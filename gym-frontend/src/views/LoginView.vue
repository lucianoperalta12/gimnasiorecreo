<template>
  <div class="min-h-screen flex items-center justify-center p-4 bg-dark-950">
    <!-- Background effects -->
    <div class="absolute inset-0 overflow-hidden">
      <div class="absolute -top-40 -right-40 w-80 h-80 bg-primary-900/20 rounded-full blur-3xl" />
      <div class="absolute -bottom-40 -left-40 w-80 h-80 bg-primary-600/10 rounded-full blur-3xl" />
    </div>

    <div class="relative w-full max-w-md">
      <!-- Logo -->
      <div class="text-center mb-8">
        <div class="inline-flex items-center justify-center w-24 h-24 mb-4">
          <img :src="logoUrl" alt="Recreo Logo" class="w-full h-full object-contain shadow-glow-lg rounded-2xl" />
        </div>
        <p class="text-dark-400 mt-2 font-medium">Espacio Fitness</p>
      </div>

      <!-- Card -->
      <div class="card p-6 sm:p-8 border border-dark-800 shadow-2xl backdrop-blur-md bg-dark-900/80">
        <!-- Google Login -->
        <!--
        <div class="mb-8">
          <div id="googleButton" class="flex justify-center"></div>
          <div class="relative mt-8">
            <div class="absolute inset-0 flex items-center"><div class="w-full border-t border-dark-700"></div></div>
            <div class="relative flex justify-center text-xs uppercase"><span class="bg-dark-900 px-2 text-dark-500">O acceso administrador</span></div>
          </div>
        </div>
        -->

        <!-- Main Login Form -->
        <form @submit.prevent="handleAdminLogin" class="space-y-4">
          <AppInput
            id="admin-user"
            v-model="adminForm.username"
            label="Usuario o Email"
            placeholder="tu@email.com"
            :error="errors.username"
          />
          <AppInput
            id="admin-pass"
            v-model="adminForm.password"
            label="Contraseña"
            type="password"
            placeholder="••••••••"
            :error="errors.password"
          />
          <AppButton type="submit" :loading="loading" class="w-full bg-primary-600 hover:bg-primary-700 text-white font-bold py-3">
            Entrar
          </AppButton>
        </form>

        <div class="mt-6 text-center">
          <p class="text-sm text-dark-400">
            ¿No tenés cuenta?
            <button @click.prevent="showRegisterModal = true" class="text-primary-400 hover:text-primary-300 font-medium transition-colors">Crear cuenta</button>
          </p>
        </div>

        <!-- Error message -->
        <div v-if="errorMessage" class="mt-4 p-3 rounded-lg bg-red-500/10 border border-red-500/20 text-red-400 text-sm text-center animate-fade-in">
          {{ errorMessage }}
        </div>
      </div>
    </div>

    <!-- Modal Registro -->
    <AppModal v-model="showRegisterModal" title="Crear Cuenta" size="sm">
      <form @submit.prevent="handleRegister" class="space-y-4">
        <AppInput id="reg-name" v-model="registerForm.nombre" label="Nombre completo" placeholder="Ej: Juan Pérez" required />
        <AppInput id="reg-email" v-model="registerForm.email" label="Correo electrónico" type="email" placeholder="tu@email.com" required />
        <AppInput id="reg-password" v-model="registerForm.password" label="Contraseña" type="password" placeholder="••••••••" required />
        <div v-if="registerError" class="p-2 rounded bg-red-500/10 border border-red-500/20 text-red-400 text-xs text-center">{{ registerError }}</div>
        <AppButton type="submit" :loading="registerLoading" class="w-full">Registrarme</AppButton>
      </form>
    </AppModal>
  </div>
</template>

<script setup>
import { ref, reactive, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth.store'
import AppInput from '@/components/ui/AppInput.vue'
import AppButton from '@/components/ui/AppButton.vue'
import AppModal from '@/components/ui/AppModal.vue'

const router = useRouter()
const authStore = useAuthStore()

const logoUrl = ref('/logo-recreo.png')
const loading = ref(false)
const errorMessage = ref('')
const errors = reactive({})
const adminForm = reactive({ username: '', password: '' })

const showRegisterModal = ref(false)
const registerForm = reactive({ nombre: '', email: '', password: '' })
const registerLoading = ref(false)
const registerError = ref('')

// Google Identity Services Setup
onMounted(() => {
  const script = document.createElement('script')
  script.src = 'https://accounts.google.com/gsi/client'
  script.async = true
  script.defer = true
  script.onload = () => {
    /* global google */
    google.accounts.id.initialize({
      client_id: '839586478940-dd8mcgbta9jnte01be3lavr2qrsmsmol.apps.googleusercontent.com',
      callback: handleGoogleResponse
    })
    google.accounts.id.renderButton(
      document.getElementById('googleButton'),
      { theme: 'outline', size: 'large', width: '100%', text: 'continue_with', shape: 'pill' }
    )
  }
  document.head.appendChild(script)
})

async function handleGoogleResponse(response) {
  loading.value = true
  errorMessage.value = ''
  try {
    await authStore.loginWithGoogle(response.credential)
    router.push('/')
  } catch (err) {
    errorMessage.value = err.response?.data?.error || 'Error en autenticación con Google'
  } finally {
    loading.value = false
  }
}

async function handleAdminLogin() {
  errorMessage.value = ''
  if (!adminForm.username || !adminForm.password) {
    errorMessage.value = 'Complete todos los campos'
    return
  }

  loading.value = true
  try {
    await authStore.loginAdmin(adminForm)
    router.push('/')
  } catch (err) {
    errorMessage.value = err.response?.data?.error || 'Credenciales inválidas'
  } finally {
    loading.value = false
  }
}

async function handleRegister() {
  registerError.value = ''
  if (!registerForm.nombre || !registerForm.email || !registerForm.password) {
    registerError.value = 'Complete todos los campos'
    return
  }
  
  registerLoading.value = true
  try {
    await authStore.register(registerForm)
    showRegisterModal.value = false
    router.push('/')
  } catch (err) {
    registerError.value = err.response?.data?.error || 'Error al crear la cuenta'
  } finally {
    registerLoading.value = false
  }
}
</script>
