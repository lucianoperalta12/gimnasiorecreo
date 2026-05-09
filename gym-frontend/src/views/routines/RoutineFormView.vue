<template>
  <div class="animate-fade-in pb-20">
    <!-- Sticky Header -->
    <div class="sticky top-[70px] z-30 -mx-6 px-6 py-4 bg-dark-950/80 backdrop-blur-md border-b border-dark-800 flex items-center justify-between mb-8">
      <div>
        <router-link to="/routines" class="text-[10px] uppercase font-black text-dark-500 hover:text-primary-400 transition-colors tracking-widest flex items-center gap-1">
          <svg class="w-3 h-3" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" d="M15.75 19.5L8.25 12l7.5-7.5" /></svg>
          Volver
        </router-link>
        <h1 class="text-xl font-black text-white mt-0.5">{{ isEditing ? 'Editar Rutina' : 'Nueva Rutina' }}</h1>
      </div>
      <div class="flex items-center gap-3">
        <button type="button" @click="router.push('/routines')" class="hidden sm:block btn-secondary btn-sm">Cancelar</button>
        <AppButton @click="handleSubmit" :loading="saving" size="sm" class="shadow-glow-sm">
          {{ isEditing ? 'Guardar Cambios' : 'Crear Rutina' }}
        </AppButton>
      </div>
    </div>

    <LoadingSpinner v-if="loading" />

    <form v-else @submit.prevent="handleSubmit" class="max-w-5xl mx-auto grid grid-cols-1 lg:grid-cols-12 gap-8">
      <!-- Sidebar: General Info -->
      <div class="lg:col-span-4 space-y-6">
        <div class="card p-5 space-y-5 sticky top-[160px]">
          <h2 class="text-xs font-black text-dark-400 uppercase tracking-widest flex items-center gap-2">
            <span class="w-1.5 h-4 bg-primary-600 rounded-full"></span>
            Información General
          </h2>
          
          <div class="space-y-4">
            <AppInput v-model="form.nombre" label="Nombre" placeholder="Ej: Full Body A" />
            <div>
              <label class="label">Descripción</label>
              <textarea v-model="form.descripcion" rows="4" class="input text-sm" placeholder="Objetivos de la rutina..." />
            </div>
            
            <div class="flex items-center justify-between p-3 rounded-xl bg-dark-900/50 border border-dark-800">
              <span class="text-xs font-bold text-dark-300">Estado de la rutina</span>
              <button
                type="button"
                class="relative w-10 h-5 rounded-full transition-colors duration-200"
                :class="form.activa ? 'bg-primary-600' : 'bg-dark-700'"
                @click="form.activa = !form.activa"
              >
                <span
                  class="absolute top-0.5 left-0.5 w-4 h-4 bg-white rounded-full shadow transition-transform duration-200"
                  :class="form.activa ? 'translate-x-5' : ''"
                />
              </button>
            </div>
          </div>
        </div>
      </div>

      <!-- Main: Exercises List -->
      <div class="lg:col-span-8 space-y-8">
        <div v-for="section in routineSections" :key="section.value" class="space-y-4">
          <div class="flex items-center justify-between">
            <h3 class="text-sm font-black text-white uppercase tracking-widest flex items-center gap-3">
              <span class="text-lg">
                <span v-if="section.value === 'calentamientoInicial'">🔥</span>
                <span v-else-if="section.value === 'parteMedia'">⚙️</span>
                <span v-else-if="section.value === 'fuerza'">💪</span>
              </span>
              {{ section.label }}
            </h3>
            <button 
              type="button" 
              @click="addExercise(section.value)"
              class="text-[10px] font-black uppercase text-primary-400 hover:text-primary-300 transition-colors flex items-center gap-1.5"
            >
              <svg class="w-3.5 h-3.5" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="3"><path stroke-linecap="round" stroke-linejoin="round" d="M12 4.5v15m7.5-7.5h-15" /></svg>
              Añadir Ejercicio
            </button>
          </div>

          <div v-if="getExercisesBySection(section.value).length === 0" class="p-6 rounded-2xl border border-dashed border-dark-800 text-center">
            <p class="text-xs text-dark-500 italic font-medium">Sin ejercicios en este bloque</p>
          </div>

          <div class="space-y-3">
            <div
              v-for="(ej, idx) in getExercisesBySection(section.value)"
              :key="idx"
              class="p-4 rounded-2xl bg-dark-900/40 border border-dark-800/50 hover:border-dark-700 transition-all group animate-slide-up"
            >
              <div class="flex flex-col sm:flex-row gap-4">
                <!-- Select Exercise -->
                <div class="flex-1 min-w-0">
                  <div class="flex items-center justify-between mb-2">
                    <span class="text-[10px] font-black text-dark-500 uppercase">Ejercicio #{{ ej.ordenGlobal }}</span>
                    <button type="button" @click="removeExercise(ej.originalIndex)" class="text-red-500/50 hover:text-red-500 transition-colors">
                      <svg class="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" d="M6 18L18 6M6 6l12 12" /></svg>
                    </button>
                  </div>
                  <select v-model="form.ejercicios[ej.originalIndex].ejercicioId" class="input !py-2 text-sm font-bold">
                    <option :value="0" disabled>Seleccionar...</option>
                    <option v-for="ex in exercises" :key="ex.id" :value="ex.id">{{ ex.nombre }} ({{ ex.grupoMuscular }})</option>
                  </select>
                </div>

                <!-- Specs Grid -->
                <div class="grid grid-cols-4 gap-2 sm:w-[320px]">
                  <div class="flex flex-col gap-1">
                    <span class="text-[8px] font-black text-dark-500 uppercase text-center">Series</span>
                    <input v-model.number="form.ejercicios[ej.originalIndex].series" type="number" class="input !px-2 !py-2 text-center text-sm font-black" />
                  </div>
                  <div class="flex flex-col gap-1">
                    <span class="text-[8px] font-black text-dark-500 uppercase text-center">Reps</span>
                    <input v-model.number="form.ejercicios[ej.originalIndex].repeticiones" type="number" class="input !px-2 !py-2 text-center text-sm font-black" />
                  </div>
                  <div class="flex flex-col gap-1">
                    <span class="text-[8px] font-black text-dark-500 uppercase text-center">Peso</span>
                    <input v-model.number="form.ejercicios[ej.originalIndex].peso" type="number" step="0.5" class="input !px-2 !py-2 text-center text-sm font-black text-primary-400" placeholder="0" />
                  </div>
                  <div class="flex flex-col gap-1">
                    <span class="text-[8px] font-black text-dark-500 uppercase text-center">Desc</span>
                    <input v-model.number="form.ejercicios[ej.originalIndex].descansoSegundos" type="number" class="input !px-2 !py-2 text-center text-sm font-black" placeholder="90" />
                  </div>
                </div>
              </div>

              <!-- Observations (Compact) -->
              <div class="mt-3 pt-3 border-t border-dark-800/50">
                <input v-model="form.ejercicios[ej.originalIndex].observaciones" class="bg-transparent border-none text-[11px] text-dark-400 w-full focus:ring-0 placeholder-dark-600 font-medium" placeholder="Añadir observaciones..." />
              </div>
            </div>
          </div>
        </div>
      </div>
    </form>
  </div>
</template>

<script setup>
import { ref, reactive, computed, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useRoutineStore } from '@/stores/routine.store'
import { useNotification } from '@/composables/useNotification'
import AppInput from '@/components/ui/AppInput.vue'
import AppButton from '@/components/ui/AppButton.vue'
import LoadingSpinner from '@/components/shared/LoadingSpinner.vue'
import { ROUTINE_SECTIONS } from '@/constants/routineSections'

const route = useRoute()
const router = useRouter()
const store = useRoutineStore()
const { success, error: showError } = useNotification()

const isEditing = computed(() => !!route.params.id)
const loading = ref(false)
const saving = ref(false)
const exercises = computed(() => store.exercises)
const routineSections = ROUTINE_SECTIONS

const form = reactive({
  nombre: '',
  descripcion: '',
  activa: true,
  ejercicios: []
})

// Helper to get exercises for a specific section with tracking for original index
function getExercisesBySection(sectionValue) {
  return form.ejercicios
    .map((ej, index) => ({ ...ej, originalIndex: index }))
    .filter(ej => ej.bloque === sectionValue)
    .map((ej, idx) => ({ ...ej, ordenGlobal: idx + 1 }))
}

function addExercise(sectionValue = 'parteMedia') {
  form.ejercicios.push({
    ejercicioId: 0,
    bloque: sectionValue,
    series: 3,
    repeticiones: 10,
    peso: null,
    descansoSegundos: 90,
    orden: form.ejercicios.length + 1,
    observaciones: ''
  })
}

function removeExercise(index) {
  form.ejercicios.splice(index, 1)
  form.ejercicios.forEach((ej, i) => {
    ej.orden = i + 1
  })
}

async function handleSubmit() {
  if (!form.nombre.trim()) return showError('El nombre es obligatorio')
  if (form.ejercicios.length === 0) return showError('Agregá al menos un ejercicio')
  if (form.ejercicios.some((ejercicio) => !ejercicio.ejercicioId)) return showError('Seleccioná todos los ejercicios')
  if (form.ejercicios.some((ejercicio) => !ejercicio.bloque)) return showError('Seleccioná el bloque de cada ejercicio')

  form.ejercicios.forEach((ej, i) => {
    ej.orden = i + 1
  })

  saving.value = true
  try {
    if (isEditing.value) {
      await store.updateRoutine(route.params.id, form)
      success('Rutina actualizada')
    } else {
      await store.createRoutine(form)
      success('Rutina creada')
    }
    router.push('/routines')
  } catch (err) {
    showError(err.response?.data?.error || 'Error al guardar')
  } finally {
    saving.value = false
  }
}

onMounted(async () => {
  loading.value = true
  try {
    await store.fetchExercises()
    if (isEditing.value) {
      const routine = await store.fetchRoutine(route.params.id)
      if (routine) {
        form.nombre = routine.nombre
        form.descripcion = routine.descripcion || ''
        form.activa = routine.activa
        form.ejercicios = routine.ejercicios.map((ejercicio) => ({
          ejercicioId: ejercicio.ejercicioId,
          bloque: ejercicio.bloque || 'parteMedia',
          series: ejercicio.series,
          repeticiones: ejercicio.repeticiones,
          peso: ejercicio.peso,
          descansoSegundos: ejercicio.descansoSegundos,
          orden: ejercicio.orden,
          observaciones: ejercicio.observaciones || ''
        }))
      }
    }
  } finally {
    loading.value = false
  }
})
</script>
