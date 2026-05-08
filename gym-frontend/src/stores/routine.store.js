import { defineStore } from 'pinia'
import { ref } from 'vue'
import { routinesApi } from '@/api/routines.api'
import { exercisesApi } from '@/api/exercises.api'
import { assignmentsApi } from '@/api/assignments.api'

export const useRoutineStore = defineStore('routine', () => {
  const routines = ref([])
  const exercises = ref([])
  const myRoutines = ref([])
  const assignmentSummary = ref({
    ejerciciosCount: 0,
    rutinasCount: 0,
    alumnosCount: 0,
    profesoresCount: 0,
    asignacionesCount: 0
  })
  const currentRoutine = ref(null)
  const loading = ref(false)

  // Exercises
  async function fetchExercises() {
    loading.value = true
    try {
      const { data } = await exercisesApi.getAll()
      exercises.value = data
    } finally {
      loading.value = false
    }
  }

  async function createExercise(exerciseData) {
    const { data } = await exercisesApi.create(exerciseData)
    exercises.value.push(data)
    return data
  }

  async function updateExercise(id, exerciseData) {
    const { data } = await exercisesApi.update(id, exerciseData)
    const index = exercises.value.findIndex(e => e.id === id)
    if (index !== -1) exercises.value[index] = data
    return data
  }

  async function deleteExercise(id) {
    await exercisesApi.delete(id)
    exercises.value = exercises.value.filter(e => e.id !== id)
  }

  // Routines
  async function fetchRoutines() {
    loading.value = true
    try {
      const { data } = await routinesApi.getAll()
      routines.value = data
    } finally {
      loading.value = false
    }
  }

  async function fetchRoutine(id) {
    loading.value = true
    try {
      const { data } = await routinesApi.getById(id)
      currentRoutine.value = data
      return data
    } finally {
      loading.value = false
    }
  }

  async function createRoutine(routineData) {
    const { data } = await routinesApi.create(routineData)
    routines.value.unshift({
      id: data.id,
      nombre: data.nombre,
      descripcion: data.descripcion,
      profesorNombre: data.profesorNombre,
      fechaCreacion: data.fechaCreacion,
      activa: data.activa,
      cantidadEjercicios: data.ejercicios?.length || 0
    })
    return data
  }

  async function updateRoutine(id, routineData) {
    const { data } = await routinesApi.update(id, routineData)
    const index = routines.value.findIndex(r => r.id === id)
    if (index !== -1) {
      routines.value[index] = {
        ...routines.value[index],
        nombre: data.nombre,
        descripcion: data.descripcion,
        activa: data.activa,
        cantidadEjercicios: data.ejercicios?.length || 0
      }
    }
    return data
  }

  async function deleteRoutine(id) {
    await routinesApi.delete(id)
    routines.value = routines.value.filter(r => r.id !== id)
  }

  // Assignments (student)
  async function fetchMyRoutines() {
    loading.value = true
    try {
      const { data } = await assignmentsApi.getMyRoutines()
      myRoutines.value = data
    } finally {
      loading.value = false
    }
  }

  async function assignRoutine(alumnoId, rutinaId) {
    const { data } = await assignmentsApi.assign({ alumnoId, rutinaId })
    return data
  }

  async function unassignRoutine(assignmentId) {
    await assignmentsApi.unassign(assignmentId)
  }

  async function getStudentAssignments(studentId) {
    const { data } = await assignmentsApi.getByStudent(studentId)
    return data
  }

  async function fetchAssignmentSummary() {
    const { data } = await assignmentsApi.getSummary()
    assignmentSummary.value = data
    return data
  }

  return {
    routines,
    exercises,
    myRoutines,
    assignmentSummary,
    currentRoutine,
    loading,
    fetchExercises,
    createExercise,
    updateExercise,
    deleteExercise,
    fetchRoutines,
    fetchRoutine,
    createRoutine,
    updateRoutine,
    deleteRoutine,
    fetchMyRoutines,
    assignRoutine,
    unassignRoutine,
    getStudentAssignments,
    fetchAssignmentSummary
  }
})
