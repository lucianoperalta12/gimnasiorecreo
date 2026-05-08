<template>
  <div>
    <label v-if="label" :for="id" class="label">{{ label }}</label>
    <input
      :id="id"
      :type="type"
      :value="modelValue"
      :placeholder="placeholder"
      :disabled="disabled"
      class="input"
      :class="{ 'border-red-500/50 focus:ring-red-500/50': error }"
      @input="$emit('update:modelValue', $event.target.value)"
      v-bind="$attrs"
    />
    <p v-if="error" class="mt-1 text-xs text-red-400">{{ error }}</p>
  </div>
</template>

<script setup>
defineProps({
  modelValue: { type: [String, Number], default: '' },
  label: { type: String, default: '' },
  type: { type: String, default: 'text' },
  placeholder: { type: String, default: '' },
  id: { type: String, default: () => `input-${Math.random().toString(36).substr(2, 9)}` },
  error: { type: String, default: '' },
  disabled: { type: Boolean, default: false }
})

defineEmits(['update:modelValue'])
</script>
