<template>
  <div class="animate-fade-in">
    <LoadingSpinner v-if="loading" />

    <template v-else-if="routine">
      <div class="space-y-4 mb-6">
        <div class="flex items-start justify-between gap-4">
          <div class="flex-1">
            <h1 class="text-2xl font-black text-white leading-tight">{{ routine.nombre }}</h1>
            <div class="flex items-center gap-3 mt-1">
              <span :class="routine.activa ? 'badge-success' : 'badge-danger'" class="shrink-0">
                {{ routine.activa ? 'Activa' : 'Inactiva' }}
              </span>
            </div>
          </div>
          <div class="flex items-center gap-2 shrink-0">
            <AppButton
              v-if="authStore.hasRole('Superusuario', 'Administrativo', 'Profesor')"
              variant="secondary"
              @click="downloadWord"
              class="md:px-4 px-2.5 !rounded-xl md:!rounded-lg"
              title="Descargar en Word"
            >
              <svg class="w-5 h-5 md:w-3 md:h-3" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2.5">
                <path stroke-linecap="round" stroke-linejoin="round" d="M6.72 13.829c-.24.03-.48.062-.72.096m.72-.096a42.415 42.415 0 0110.56 0m-10.56 0L6.34 18m10.94-4.171c.24.03.48.062.72.096m-.72-.096L17.66 18m0 0a2.25 2.25 0 01-2.24 2.156H8.58a2.25 2.25 0 01-2.24-2.156M15 11.25v5.25M9 11.25v5.25M4.785 14c-.621 0-1.098-.57-1.002-1.185.019-.121.039-.242.06-.362.245-1.464 1.341-2.656 2.766-3.003A48.293 48.293 0 0112 9c2.197 0 4.298.145 6.353.424 1.425.347 2.52 1.54 2.766 3.003.02.12.04.24.06.362.096.614-.38 1.185-1.002 1.185H4.785z" />
                <path stroke-linecap="round" stroke-linejoin="round" d="M9 13.5V9c0-.621.504-1.125 1.125-1.125h3.75c.621 0 1.125.504 1.125 1.125v4.5m-3-1.125h.008v.008H12v-.008z" />
              </svg>
              <span class="hidden md:inline ml-1 text-[10px] font-black uppercase tracking-[0.2em]">Imprimir</span>
            </AppButton>
            <AppButton
              variant="secondary"
              @click="goBack"
              class="md:px-4 px-2.5 !rounded-xl md:!rounded-lg"
            >
              <svg class="w-5 h-5 md:w-3 md:h-3" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="3">
                <path stroke-linecap="round" stroke-linejoin="round" d="M10.5 19.5L3 12m0 0l7.5-7.5M3 12h18" />
              </svg>
              <span class="hidden md:inline ml-1 text-[10px] font-black uppercase tracking-[0.2em]">Volver</span>
            </AppButton>
          </div>
        </div>
        <p class="text-dark-400 text-sm leading-relaxed">{{ routine.descripcion || 'Sin descripción' }}</p>
      </div>

      <div class="grid grid-cols-2 sm:grid-cols-3 gap-3 mb-8">
        <div class="p-3 rounded-2xl bg-dark-900/50 border border-dark-800/50 flex flex-col gap-1">
          <span class="text-[9px] text-dark-500 uppercase tracking-widest font-black">Profesor</span>
          <span class="text-xs font-bold text-dark-200 truncate">{{ routine.profesorNombre }}</span>
        </div>
        <div v-if="routine.fechaAsignacion" class="p-3 rounded-2xl bg-dark-900/50 border border-dark-800/50 flex flex-col gap-1">
          <span class="text-[9px] text-dark-500 uppercase tracking-widest font-black">Asignada</span>
          <span class="text-xs font-bold text-dark-200">{{ formatDate(routine.fechaAsignacion) }}</span>
        </div>
        <div class="p-3 rounded-2xl bg-dark-900/50 border border-dark-800/50 flex flex-col gap-1">
          <span class="text-[9px] text-dark-500 uppercase tracking-widest font-black">Ejercicios</span>
          <span class="text-xs font-bold text-dark-200">{{ routine.ejercicios.length }} total</span>
        </div>
      </div>

      <div v-if="routine.isByDays" class="flex p-1.5 gap-1.5 bg-dark-950/80 backdrop-blur-md border border-dark-800 rounded-2xl mb-4 overflow-x-auto no-scrollbar">
        <button
          v-for="day in routine.daysCount"
          :key="day"
          @click="currentDay = day"
          class="flex-1 min-w-[80px] py-2 text-xs font-black rounded-xl transition-all duration-300"
          :class="currentDay === day
            ? 'bg-primary-600/20 text-primary-400 border border-primary-500/30'
            : 'text-dark-400 hover:text-dark-200 hover:bg-dark-900'"
        >
          Día {{ day }}
        </button>
      </div>

      <div class="flex p-1.5 gap-1.5 bg-dark-950/80 backdrop-blur-md border border-dark-800 rounded-2xl mb-8 sticky top-[72px] z-20 shadow-xl overflow-x-auto no-scrollbar">
        <button
          v-for="section in groupedSections"
          :key="section.value"
          @click="activeTab = section.value"
          class="flex-1 min-w-[100px] py-3 text-xs font-black rounded-xl transition-all duration-300 flex flex-col items-center justify-center gap-1.5 relative group"
          :class="activeTab === section.value
            ? 'bg-primary-600 text-white'
            : 'text-dark-400 hover:text-dark-200 hover:bg-dark-900'"
        >
          <span class="text-lg transition-transform duration-300 group-active:scale-125">
            <span v-if="section.value === 'calentamientoInicial'">🔥</span>
            <span v-else-if="section.value === 'parteMedia'">⚙️</span>
            <span v-else-if="section.value === 'fuerza'">🤸</span>
          </span>
          <span class="uppercase tracking-tighter">{{ section.label }}</span>
          <span
            v-if="section.ejercicios.length > 0"
            class="absolute -top-1 -right-1 w-5 h-5 rounded-full flex items-center justify-center text-[9px] font-black border-2 border-dark-950"
            :class="activeTab === section.value ? 'bg-white text-primary-600' : 'bg-dark-700 text-dark-300'"
          >
            {{ section.ejercicios.length }}
          </span>
        </button>
      </div>

      <div v-if="currentSection" class="animate-scale-in">
        <div class="card space-y-4">
          <div class="flex items-center justify-between mb-2">
            <h2 class="text-lg font-black text-white uppercase tracking-widest flex items-center gap-3">
              <span class="w-2 h-6 bg-primary-600 rounded-full"></span>
              {{ currentSection.label }}
            </h2>
          </div>

          <div v-if="currentSection.ejercicios.length === 0" class="rounded-xl border border-dashed border-dark-700 px-4 py-12 text-center text-dark-500">
            <p>Todavía no hay ejercicios cargados en esta parte de la rutina.</p>
          </div>

          <div v-else>
            <div class="hidden md:block table-container">
              <table class="table">
                <thead>
                  <tr>
                    <th>Ejercicio</th>
                    <th>Grupo</th>
                    <th>Series</th>
                    <th>Reps</th>
                    <th>Peso</th>
                    <th>Descanso</th>
                    <th>Observaciones</th>
                  </tr>
                </thead>
                <tbody>
                  <tr v-for="ej in currentSection.ejercicios" :key="ej.id" class="group">
                    <td class="font-medium text-white">
                      <div class="flex items-center gap-2">
                        {{ ej.ejercicioNombre }}
                        <a
                          v-if="ej.videoUrl"
                          :href="ej.videoUrl"
                          target="_blank"
                          class="p-1 rounded-md bg-primary-600/10 text-primary-400 hover:bg-primary-600/20 transition-all"
                          title="Ver video"
                        >
                          <svg class="w-3.5 h-3.5" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2.5">
                            <path stroke-linecap="round" stroke-linejoin="round" d="M5.25 5.653c0-.856.917-1.398 1.667-.986l11.54 6.348a1.125 1.125 0 010 1.971l-11.54 6.347c-.75.412-1.667-.13-1.667-.986V5.653z" />
                          </svg>
                        </a>
                      </div>
                    </td>
                    <td><span class="badge-primary">{{ ej.grupoMuscular }}</span></td>
                    <td>{{ ej.series }}</td>
                    <td>{{ ej.repeticiones }}</td>
                    <td>
                      <span v-if="ej.peso" class="text-white font-semibold">{{ ej.peso }} <small class="text-dark-500 font-normal">kg</small></span>
                      <span v-else class="text-dark-600">—</span>
                    </td>
                    <td>
                      <span v-if="ej.descansoSegundos" class="text-dark-300">{{ ej.descansoSegundos }}<small class="text-dark-500">s</small></span>
                      <span v-else class="text-dark-600">—</span>
                    </td>
                    <td class="text-dark-400 text-sm max-w-xs truncate" :title="ej.ejercicioDescripcion">{{ ej.ejercicioDescripcion || '—' }}</td>
                  </tr>
                </tbody>
              </table>
            </div>

            <div class="md:hidden space-y-2.5">
              <div v-for="ej in currentSection.ejercicios" :key="ej.id" class="p-3.5 rounded-2xl bg-dark-900/60 border border-dark-800/50 shadow-lg space-y-3.5 relative overflow-hidden group">
                <div class="absolute -right-4 -top-4 w-24 h-24 bg-primary-600/5 rounded-full blur-2xl group-hover:bg-primary-600/10 transition-colors" />

                <div class="flex items-start justify-between relative z-10">
                  <div class="flex items-center gap-2.5">
                    <div>
                      <h3 class="font-black text-sm text-white leading-tight">{{ ej.ejercicioNombre }}</h3>
                      <div class="flex items-center gap-2 mt-1.5">
                        <span class="px-2 py-0.5 rounded-md bg-primary-600/10 text-primary-400 text-[9px] font-black uppercase tracking-wider">{{ ej.grupoMuscular }}</span>
                        <span v-if="ej.videoUrl" class="w-1 h-1 rounded-full bg-dark-600"></span>
                        <span v-if="ej.videoUrl" class="text-[9px] text-dark-500 font-bold">Video disponible</span>
                      </div>
                    </div>
                  </div>
                  <a
                    v-if="ej.videoUrl"
                    :href="ej.videoUrl"
                    target="_blank"
                    class="w-8 h-8 rounded-xl bg-primary-600 text-white flex items-center justify-center active:scale-90 transition-transform"
                  >
                    <svg class="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="3">
                      <path stroke-linecap="round" stroke-linejoin="round" d="M5.25 5.653c0-.856.917-1.398 1.667-.986l11.54 6.348a1.125 1.125 0 010 1.971l-11.54 6.347c-.75.412-1.667-.13-1.667-.986V5.653z" />
                    </svg>
                  </a>
                </div>

                <div class="grid grid-cols-3 gap-2 relative z-10">
                  <div class="bg-dark-950/50 p-2 rounded-xl border border-dark-800/50 flex flex-col items-center justify-center gap-0.5">
                    <span class="text-[8px] uppercase font-black text-dark-500 tracking-tighter">Series</span>
                    <span class="text-sm font-black text-white">{{ ej.series }}</span>
                  </div>
                  <div class="bg-dark-950/50 p-2 rounded-xl border border-dark-800/50 flex flex-col items-center justify-center gap-0.5">
                    <span class="text-[8px] uppercase font-black text-dark-500 tracking-tighter">Reps</span>
                    <span class="text-sm font-black text-white">{{ ej.repeticiones }}</span>
                  </div>
                  <div class="bg-dark-950/50 p-2 rounded-xl border border-dark-800/50 flex flex-col items-center justify-center gap-0.5">
                    <span class="text-[8px] uppercase font-black text-dark-500 tracking-tighter">Peso</span>
                    <span class="text-sm font-black text-primary-400">{{ ej.peso || '—' }}<small v-if="ej.peso" class="ml-0.5 font-bold text-[10px]">kg</small></span>
                  </div>
                </div>

                <div v-if="ej.descansoSegundos || ej.ejercicioDescripcion" class="pt-2.5 border-t border-dark-800/50 flex flex-col gap-2 relative z-10">
                  <div v-if="ej.descansoSegundos" class="flex items-center justify-between">
                    <div class="flex items-center gap-2 text-[10px] font-bold text-dark-400 uppercase tracking-wide">
                      <svg class="w-4 h-4 text-primary-500" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2.5">
                        <path stroke-linecap="round" stroke-linejoin="round" d="M12 6v6h4.5m4.5 0a9 9 0 11-18 0 9 9 0 0118 0z" />
                      </svg>
                      Tiempo Descanso
                    </div>
                    <span class="text-sm font-black text-dark-200 bg-dark-800 px-3 py-1 rounded-full border border-dark-700">{{ ej.descansoSegundos }}s</span>
                  </div>
                  <div v-if="ej.ejercicioDescripcion" class="bg-primary-600/5 p-2.5 rounded-xl border border-primary-600/10">
                    <div class="flex items-center gap-2 mb-2">
                      <svg class="w-3.5 h-3.5 text-primary-400" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="3">
                        <path stroke-linecap="round" stroke-linejoin="round" d="M7.5 8.25h9m-9 3H12m-9.75 1.51c0 1.6 1.123 2.994 2.707 3.227 1.129.166 2.27.293 3.423.379.35.026.67.21.865.501L12 21l2.755-4.133a1.14 1.14 0 01.865-.501 48.172 48.172 0 003.423-.379c1.584-.233 2.707-1.626 2.707-3.228V6.741c0-1.602-1.123-2.995-2.707-3.228A48.394 48.394 0 0012 3c-2.392 0-4.744.175-7.043.513C3.373 3.746 2.25 5.14 2.25 6.741v6.018z" />
                      </svg>
                      <span class="text-[10px] uppercase font-black text-primary-400 tracking-wider">Observaciones</span>
                    </div>
                    <p class="text-xs text-dark-300 leading-relaxed italic break-words">{{ ej.ejercicioDescripcion }}</p>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </template>
  </div>
</template>

<script setup>
import { ref, onMounted, computed } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useRoutineStore } from '@/stores/routine.store'
import { useAuthStore } from '@/stores/auth.store'
import AppButton from '@/components/ui/AppButton.vue'
import LoadingSpinner from '@/components/shared/LoadingSpinner.vue'
import { groupRoutineExercises } from '@/constants/routineSections'

const route = useRoute()
const router = useRouter()
const store = useRoutineStore()
const authStore = useAuthStore()
const loading = ref(true)
const routine = ref(null)
const activeTab = ref('calentamientoInicial')
const currentDay = ref(1)

function goBack() {
  if (authStore.hasRole('Alumno')) {
    router.push('/dashboard')
  } else {
    router.push('/routines')
  }
}

const filteredExercises = computed(() => {
  if (!routine.value) return []
  if (!routine.value.isByDays) return routine.value.ejercicios
  return routine.value.ejercicios.filter(ej => ej.dayNumber === currentDay.value)
})

const groupedSections = computed(() => groupRoutineExercises(filteredExercises.value))
const currentSection = computed(() => groupedSections.value.find(s => s.value === activeTab.value))

function formatDate(dateStr) {
  return new Date(dateStr).toLocaleDateString('es-AR', { day: '2-digit', month: 'short', year: 'numeric' })
}

function downloadWord() {
  if (!routine.value) return

  let html = `<html xmlns:o="urn:schemas-microsoft-com:office:office" xmlns:w="urn:schemas-microsoft-com:office:word" xmlns="http://www.w3.org/TR/REC-html40">`
  html += `<head><meta charset="utf-8"><title>${routine.value.nombre}</title>`
  html += `<style>
    body { font-family: 'Segoe UI', Helvetica, Arial, sans-serif; color: #1a202c; line-height: 1.5; padding: 20px; }
    .header-title { font-size: 28px; font-weight: bold; margin: 0 0 5px 0; color: #1a202c; }
    .header-desc { font-size: 14px; color: #4a5568; margin: 0 0 20px 0; font-style: italic; }
    .meta-box { margin-bottom: 25px; font-size: 13px; color: #4a5568; border-bottom: 2px solid #e2e8f0; padding-bottom: 12px; }
    .meta-item { margin-bottom: 4px; }
    .meta-label { font-weight: bold; color: #2d3748; }
    .day-title { font-size: 22px; color: #2b6cb0; font-weight: bold; margin-top: 35px; margin-bottom: 15px; border-bottom: 2px solid #ebf8ff; padding-bottom: 6px; text-transform: uppercase; }
    .section-title { font-size: 16px; color: #2b6cb0; font-weight: bold; text-transform: uppercase; margin-top: 30px; margin-bottom: 10px; border-bottom: 2px solid #2b6cb0; padding-bottom: 4px; }
    table.exercise-table { width: 100%; border-collapse: collapse; margin-bottom: 30px; }
    table.exercise-table th { background-color: #f7fafc; color: #2d3748; font-weight: bold; text-align: left; padding: 10px 12px; border-bottom: 2px solid #cbd5e0; font-size: 12px; text-transform: uppercase; }
    table.exercise-table td { padding: 12px; border-bottom: 1px solid #e2e8f0; font-size: 13px; vertical-align: top; }
    .exercise-name { font-weight: bold; color: #1a202c; font-size: 14px; }
    .muscle-badge { font-size: 10px; font-weight: bold; color: #4a5568; display: inline-block; margin-top: 3px; }
    .obs-text { font-size: 12px; color: #4a5568; }
    .num-value { font-weight: bold; color: #1a202c; text-align: center; font-size: 14px; }
  </style>`
  html += `</head><body>`

  html += `<h1 class="header-title">${routine.value.nombre}</h1>`
  html += `<p class="header-desc">${routine.value.descripcion || 'Sin descripción'}</p>`

  html += `<div class="meta-box">`
  html += `<div class="meta-item"><span class="meta-label">Profesor:</span> ${routine.value.profesorNombre}</div>`
  if (routine.value.fechaAsignacion) {
    html += `<div class="meta-item"><span class="meta-label">Fecha de Asignación:</span> ${formatDate(routine.value.fechaAsignacion)}</div>`
  }
  html += `<div class="meta-item"><span class="meta-label">Ejercicios:</span> ${routine.value.ejercicios.length} total</div>`
  html += `</div>`

  const renderTable = (exercises) => {
    let tableHtml = `<table class="exercise-table"><thead><tr>`
    tableHtml += `<th>Ejercicio</th>`
    tableHtml += `<th style="width: 65px; text-align: center;">Series</th>`
    tableHtml += `<th style="width: 65px; text-align: center;">Reps</th>`
    tableHtml += `<th style="width: 80px; text-align: center;">Peso</th>`
    tableHtml += `<th style="width: 85px; text-align: center;">Descanso</th>`
    tableHtml += `<th>Observaciones</th>`
    tableHtml += `</tr></thead><tbody>`

    exercises.forEach(ej => {
      tableHtml += `<tr>`
      tableHtml += `<td>`
      tableHtml += `<div class="exercise-name">${ej.ejercicioNombre}</div>`
      tableHtml += `<div class="muscle-badge">${ej.grupoMuscular}</div>`
      tableHtml += `</td>`
      tableHtml += `<td class="num-value">${ej.series}</td>`
      tableHtml += `<td class="num-value">${ej.repeticiones}</td>`
      tableHtml += `<td class="num-value">${ej.peso ? ej.peso + ' kg' : '—'}</td>`
      tableHtml += `<td class="num-value">${ej.descansoSegundos ? ej.descansoSegundos + 's' : '—'}</td>`
      tableHtml += `<td class="obs-text">${ej.ejercicioDescripcion || '—'}</td>`
      tableHtml += `</tr>`
    })

    tableHtml += `</tbody></table>`
    return tableHtml
  }

  if (routine.value.isByDays) {
    for (let day = 1; day <= routine.value.daysCount; day++) {
      const dayExercises = routine.value.ejercicios.filter(e => e.dayNumber === day)
      if (dayExercises.length === 0) continue

      html += `<div class="day-title">Día ${day}</div>`
      
      const sections = groupRoutineExercises(dayExercises)
      sections.forEach(sec => {
        if (sec.ejercicios.length === 0) return
        html += `<div class="section-title">${sec.label}</div>`
        html += renderTable(sec.ejercicios)
      })
    }
  } else {
    const sections = groupRoutineExercises(routine.value.ejercicios)
    sections.forEach(sec => {
      if (sec.ejercicios.length === 0) return
      html += `<div class="section-title">${sec.label}</div>`
      html += renderTable(sec.ejercicios)
    })
  }

  html += `</body></html>`

  const blob = new Blob(['\ufeff' + html], { type: 'application/msword' })
  const url = URL.createObjectURL(blob)
  const a = document.createElement('a')
  a.href = url
  a.download = `${routine.value.nombre.replace(/\s+/g, '_')}.doc`
  document.body.appendChild(a)
  a.click()
  document.body.removeChild(a)
  URL.revokeObjectURL(url)
}

onMounted(async () => {
  try {
    routine.value = await store.fetchRoutine(route.params.id)

    const firstWithExercises = groupedSections.value.find(s => s.ejercicios.length > 0)
    if (firstWithExercises) {
      activeTab.value = firstWithExercises.value
    }
  } finally {
    loading.value = false
  }
})
</script>
