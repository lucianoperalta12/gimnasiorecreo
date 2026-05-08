export const ROUTINE_SECTIONS = [
  { value: 'calentamientoInicial', label: 'Calentamiento inicial' },
  { value: 'parteMedia', label: 'Parte media' },
  { value: 'fuerza', label: 'Fuerza' }
]

export function groupRoutineExercises(ejercicios = []) {
  return ROUTINE_SECTIONS.map((section) => ({
    ...section,
    ejercicios: ejercicios.filter((ejercicio) => ejercicio.bloque === section.value)
  }))
}
