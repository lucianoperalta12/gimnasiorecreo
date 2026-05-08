<template>
  <div class="animate-fade-in">
    <div class="mb-6">
      <h1 class="page-title">Mis Rutinas</h1>
      <p class="page-subtitle">Rutinas de entrenamiento asignadas</p>
    </div>

    <LoadingSpinner v-if="loading" />

    <div v-else-if="routines.length === 0" class="card text-center py-16">
      <div class="text-5xl mb-4">🏋️</div>
      <p class="text-dark-300 text-lg">No tenés rutinas asignadas</p>
      <p class="text-dark-500 text-sm mt-2">Tu profesor te asignará rutinas pronto.</p>
    </div>

    <div v-else class="space-y-6">
      <div v-for="routine in routines" :key="routine.id" class="card animate-slide-up">
        <div class="flex items-start justify-between mb-4">
          <div>
            <h2 class="text-xl font-bold text-white">{{ routine.nombre }}</h2>
            <p class="text-sm text-dark-400 mt-1">{{ routine.descripcion || 'Sin descripción' }}</p>
            <p class="text-xs text-dark-500 mt-2">Prof. {{ routine.profesorNombre }}</p>
          </div>
          <span class="badge-success">Activa</span>
        </div>

        <div class="space-y-4">
          <section v-for="section in getRoutineSections(routine.ejercicios)" :key="section.value" class="space-y-3">
            <div class="flex items-center justify-between border-b border-dark-800 pb-1">
              <h3 class="text-[10px] font-bold uppercase tracking-widest text-dark-400 flex items-center gap-2">
                <span v-if="section.value === 'calentamientoInicial'">🔥</span>
                <span v-else-if="section.value === 'parteMedia'">⚙️</span>
                <span v-else-if="section.value === 'fuerza'">💪</span>
                {{ section.label }}
              </h3>
              <span class="text-[10px] font-bold text-dark-600 bg-dark-900 px-1.5 py-0.5 rounded">{{ section.ejercicios.length }}</span>
            </div>

            <div v-if="section.ejercicios.length === 0" class="rounded-lg border border-dashed border-dark-700/50 px-4 py-3 text-xs text-dark-500 italic text-center">
              Sin ejercicios en esta sección
            </div>

            <div
              v-for="ej in section.ejercicios"
              :key="ej.id"
              class="flex items-center gap-3 p-2.5 rounded-xl bg-dark-900/40 border border-dark-800/50 hover:border-dark-700 transition-colors"
            >
              <span class="w-7 h-7 rounded-lg bg-dark-800 text-dark-300 flex items-center justify-center text-[10px] font-bold border border-dark-700/50">{{ ej.orden }}</span>
              <div class="flex-1 min-w-0">
                <p class="text-sm font-semibold text-white truncate">{{ ej.ejercicioNombre }}</p>
                <p class="text-[10px] text-dark-500 uppercase font-medium">{{ ej.grupoMuscular }}</p>
              </div>
              <div class="flex items-center gap-2 text-[11px]">
                <div class="flex items-center gap-1 bg-dark-800 px-2 py-1 rounded-md text-dark-200">
                  <span class="font-bold">{{ ej.series }}</span>
                  <span class="text-dark-500 text-[9px]">x</span>
                  <span class="font-bold">{{ ej.repeticiones }}</span>
                </div>
                <div v-if="ej.peso" class="bg-primary-600/10 text-primary-400 px-2 py-1 rounded-md font-bold">
                  {{ ej.peso }}<span class="text-[9px] ml-0.5">kg</span>
                </div>
                <a
                  v-if="ej.videoUrl"
                  :href="ej.videoUrl"
                  target="_blank"
                  class="p-1.5 rounded-lg bg-dark-800 text-dark-400 hover:text-white transition-colors"
                >
                  <svg class="w-3.5 h-3.5" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2.5">
                    <path stroke-linecap="round" stroke-linejoin="round" d="M5.25 5.653c0-.856.917-1.398 1.667-.986l11.54 6.348a1.125 1.125 0 010 1.971l-11.54 6.347c-.75.412-1.667-.13-1.667-.986V5.653z" />
                  </svg>
                </a>
              </div>
            </div>
          </section>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { useRoutineStore } from '@/stores/routine.store'
import LoadingSpinner from '@/components/shared/LoadingSpinner.vue'
import { groupRoutineExercises } from '@/constants/routineSections'

const store = useRoutineStore()
const loading = ref(true)
const routines = computed(() => store.myRoutines)
const getRoutineSections = groupRoutineExercises

onMounted(async () => {
  try {
    await store.fetchMyRoutines()
  } finally {
    loading.value = false
  }
})
</script>
