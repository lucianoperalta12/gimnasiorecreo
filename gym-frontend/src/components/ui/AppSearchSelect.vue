<template>
  <div class="relative" ref="containerRef">
    <label v-if="label" class="label">{{ label }}</label>
    
    <div class="relative group">
      <input
        type="text"
        v-model="searchTerm"
        :placeholder="selectedOption ? selectedOption.label : placeholder"
        class="input pr-10 focus:ring-primary-500/50 disabled:opacity-50 disabled:cursor-not-allowed"
        :disabled="disabled"
        @focus="handleFocus"
        @input="isOpen = true"
      />
      
      <div class="absolute right-3 top-1/2 -translate-y-1/2 flex items-center gap-2 pointer-events-none text-dark-500">
        <svg v-if="!disabled && (searchTerm || selectedOption)" @click.stop="clear" class="w-4 h-4 pointer-events-auto hover:text-white cursor-pointer" fill="none" viewBox="0 0 24 24" stroke="currentColor">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" />
        </svg>
        <svg class="w-4 h-4 transition-transform duration-200" :class="{ 'rotate-180': isOpen }" fill="none" viewBox="0 0 24 24" stroke="currentColor">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 9l-7 7-7-7" />
        </svg>
      </div>
    </div>

    <!-- Dropdown -->
    <transition
      enter-active-class="transition duration-100 ease-out"
      enter-from-class="transform scale-95 opacity-0"
      enter-to-class="transform scale-100 opacity-100"
      leave-active-class="transition duration-75 ease-in"
      leave-from-class="transform scale-100 opacity-100"
      leave-to-class="transform scale-95 opacity-0"
    >
      <div v-if="isOpen" class="absolute z-50 w-full mt-1 bg-dark-900 border border-dark-700 rounded-lg shadow-2xl max-h-60 overflow-y-auto no-scrollbar backdrop-blur-xl bg-dark-900/90">
        <div v-if="filteredOptions.length === 0" class="p-4 text-center text-dark-500 text-sm italic">
          No se encontraron resultados
        </div>
        <ul v-else class="py-1">
          <li
            v-for="option in filteredOptions"
            :key="option.id"
            @click="selectOption(option)"
            class="px-4 py-2 text-sm cursor-pointer hover:bg-primary-600/20 hover:text-white transition-colors"
            :class="{ 'bg-primary-600/10 text-primary-400 font-medium': modelValue === option.id }"
          >
            <div class="flex flex-col">
              <span>{{ option.label }}</span>
              <span v-if="option.subLabel" class="text-xs text-dark-500">{{ option.subLabel }}</span>
            </div>
          </li>
        </ul>
      </div>
    </transition>
  </div>
</template>

<script setup>
import { ref, computed, onMounted, onUnmounted } from 'vue'

const props = defineProps({
  modelValue: { type: [String, Number, null], default: null },
  options: { type: Array, required: true }, // [{ id, label, subLabel }]
  label: { type: String, default: '' },
  placeholder: { type: String, default: 'Buscar...' },
  disabled: { type: Boolean, default: false }
})

const emit = defineEmits(['update:modelValue'])

const isOpen = ref(false)
const searchTerm = ref('')
const containerRef = ref(null)

const selectedOption = computed(() => {
  return props.options.find(o => o.id === props.modelValue)
})

const filteredOptions = computed(() => {
  const term = searchTerm.value.toLowerCase().trim()
  if (!term) return props.options
  return props.options.filter(o => 
    o.label.toLowerCase().includes(term) || 
    (o.subLabel && o.subLabel.toLowerCase().includes(term))
  )
})

function handleFocus() {
  if (props.disabled) return
  isOpen.value = true
  // If we have a selection, we don't clear searchTerm immediately so user can see what they have
}

function selectOption(option) {
  emit('update:modelValue', option.id)
  searchTerm.value = '' // Clear search to show the new selection as placeholder
  isOpen.value = false
}

function clear() {
  emit('update:modelValue', null)
  searchTerm.value = ''
}

// Close on click outside
function handleClickOutside(event) {
  if (containerRef.value && !containerRef.value.contains(event.target)) {
    isOpen.value = false
    searchTerm.value = ''
  }
}

onMounted(() => {
  document.addEventListener('click', handleClickOutside)
})

onUnmounted(() => {
  document.removeEventListener('click', handleClickOutside)
})
</script>
