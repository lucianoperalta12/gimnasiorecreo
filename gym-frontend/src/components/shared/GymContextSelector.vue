<template>
  <div v-if="authStore.hasMultipleGyms && authStore.hasSelectedGym" class="flex items-center gap-3">
    <div class="hidden xl:flex items-center gap-2 rounded-full border border-white/10 bg-dark-900/80 px-3 py-2 backdrop-blur-md">
      <span class="h-2.5 w-2.5 rounded-full" :style="{ backgroundColor: currentGym?.colorPrincipalHex || '#ff6600' }"></span>
      <span class="max-w-[150px] truncate text-[11px] font-black uppercase tracking-[0.22em] text-white">
        {{ currentGym?.gymNombre || authStore.user?.gymNombre || 'Gimnasio' }}
      </span>
    </div>

    <select
      class="input !min-w-[180px] !rounded-full !bg-dark-900/80 !border-dark-800 text-sm"
      :disabled="submitting"
      :value="authStore.user?.gymId"
      @change="handleChange"
    >
      <option v-for="gym in authStore.gyms" :key="gym.gymId" :value="gym.gymId">
        {{ gym.gymNombre }}
      </option>
    </select>
  </div>
</template>

<script setup>
import { computed, ref } from 'vue'
import { useAuthStore } from '@/stores/auth.store'

const authStore = useAuthStore()
const submitting = ref(false)

const currentGym = computed(() => authStore.selectedGym)

async function handleChange(event) {
  const gymId = Number(event.target.value)
  if (!gymId || gymId === authStore.user?.gymId) return

  submitting.value = true
  try {
    await authStore.selectGym(gymId)
    window.location.reload()
  } finally {
    submitting.value = false
  }
}
</script>
