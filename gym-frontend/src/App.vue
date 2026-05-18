<script setup>
import { onBeforeMount, watchEffect } from 'vue'
import { useAuthStore } from '@/stores/auth.store'

const authStore = useAuthStore()

watchEffect(() => {
  const color = authStore.user?.gymColorPrincipalHex || '#ff6600'
  document.documentElement.style.setProperty('--gym-primary', color)
  document.documentElement.style.setProperty('--gym-primary-hover', color)
  document.documentElement.style.setProperty('--gym-primary-soft', `${color}33`)
})

onBeforeMount(() => {
  const params = new URLSearchParams(window.location.search)
  if (params.get('forceLogout') === 'true') {
    authStore.logout()
    // Limpiar la URL para que no vuelva a desloguear al recargar
    const newUrl = window.location.origin + window.location.pathname
    window.history.replaceState({}, document.title, newUrl)
  }
})
</script>

<template>
  <router-view />
</template>
