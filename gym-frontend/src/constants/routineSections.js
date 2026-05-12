export const ROUTINE_SECTIONS = [
  { value: 'calentamientoInicial', label: 'Calentamiento' },
  { value: 'parteMedia', label: 'Bloque Central' },
  { value: 'fuerza', label: 'Movilidad' }
]

export function groupRoutineExercises(ejercicios = []) {
  return ROUTINE_SECTIONS.map((section) => ({
    ...section,
    ejercicios: ejercicios.filter((ejercicio) => ejercicio.bloque === section.value)
  }))
}
