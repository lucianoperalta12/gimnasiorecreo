<template>
  <Teleport to="body">
    <Transition name="modal">
      <div v-if="modelValue" class="fixed inset-0 z-50 flex items-center justify-center p-3 sm:p-4">
        <div class="absolute inset-0 bg-black/60 backdrop-blur-sm" @click="close" />
        <div
          class="relative bg-dark-800 border border-dark-700 rounded-xl sm:rounded-2xl shadow-2xl w-full max-h-[calc(100dvh-1.5rem)] flex flex-col overflow-hidden animate-slide-up"
          :class="sizeClass"
        >
          <!-- Header -->
          <div v-if="title" class="flex items-center justify-between gap-3 px-4 py-3 sm:px-6 sm:py-4 border-b border-dark-700 shrink-0">
            <h3 class="text-base sm:text-lg font-semibold text-white min-w-0 pr-2">{{ title }}</h3>
            <button @click="close" class="shrink-0 p-1 rounded-lg hover:bg-dark-700 text-dark-400 hover:text-dark-200 transition-colors" aria-label="Cerrar">
              <svg class="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2">
                <path stroke-linecap="round" stroke-linejoin="round" d="M6 18L18 6M6 6l12 12" />
              </svg>
            </button>
          </div>
          <!-- Body -->
          <div class="p-4 sm:p-6 overflow-y-auto overscroll-contain flex-1 min-h-0">
            <slot />
          </div>
          <!-- Footer -->
          <div v-if="$slots.footer" class="px-4 py-3 sm:px-6 sm:py-4 border-t border-dark-700 flex flex-col-reverse sm:flex-row sm:justify-end gap-3 shrink-0">
            <slot name="footer" />
          </div>
        </div>
      </div>
    </Transition>
  </Teleport>
</template>

<script setup>
import { computed } from 'vue'

const props = defineProps({
  modelValue: { type: Boolean, default: false },
  title: { type: String, default: '' },
  size: { type: String, default: 'md' }
})

const emit = defineEmits(['update:modelValue'])

const sizeClass = computed(() => ({
  sm: 'max-w-md',
  md: 'max-w-lg',
  lg: 'max-w-2xl',
  xl: 'max-w-4xl'
}[props.size] || 'max-w-lg'))

function close() {
  emit('update:modelValue', false)
}
</script>

<style scoped>
.modal-enter-active { transition: all 0.25s ease-out; }
.modal-leave-active { transition: all 0.2s ease-in; }
.modal-enter-from, .modal-leave-to { opacity: 0; }
</style>
