<template>
  <div class="flex items-center justify-center py-12">
    <div class="flex flex-col items-center gap-3">
      <div class="w-10 h-10 border-3 border-primary-500 border-t-transparent rounded-full animate-spin" />
      <p v-if="text" class="text-sm text-dark-400">{{ text }}</p>
    </div>
  </div>
</template>

<script setup>
defineProps({
  text: { type: String, default: 'Cargando...' }
})
</script>
