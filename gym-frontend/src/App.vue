<script setup>
import { onBeforeMount } from 'vue'
import { useAuthStore } from '@/stores/auth.store'

onBeforeMount(() => {
  const params = new URLSearchParams(window.location.search)
  if (params.get('forceLogout') === 'true') {
    const authStore = useAuthStore()
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
