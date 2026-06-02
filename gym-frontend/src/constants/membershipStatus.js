export function accessStatusBadgeClass(estadoAcceso) {
  switch (estadoAcceso) {
    case 'Activo':
      return 'badge-success'
    case 'Moroso':
      return 'badge-warning'
    case 'Suspendido':
      return 'badge-warning'
    case 'Vencido':
    case 'Sin membresía':
      return 'badge-danger'
    default:
      return 'badge-primary'
  }
}

export function paymentEstadoBadgeClass(estado) {
  switch (estado) {
    case 'Completado':
      return 'badge-success'
    case 'Pendiente':
      return 'badge-warning'
    case 'Rechazado':
    case 'Reembolsado':
      return 'badge-danger'
    default:
      return 'badge-primary'
  }
}

export function membershipEstadoBadgeClass(estado) {
  switch (estado) {
    case 'Activa':
      return 'badge-success'
    case 'Vencida':
      return 'badge-danger'
    case 'Cancelada':
      return 'badge-warning'
    case 'Suspendida':
      return 'badge-warning'
    default:
      return 'badge-primary'
  }
}

export const MEMBERSHIP_ESTADOS = ['Activa', 'Vencida', 'Cancelada']
export const PAYMENT_ESTADOS = ['Pendiente', 'Completado', 'Rechazado', 'Reembolsado']

export function formatCurrency(value, currencyCode = 'ARS') {
  const code = currencyCode || 'ARS'
  return new Intl.NumberFormat(code === 'ARS' ? 'es-AR' : 'en-US', {
    style: 'currency',
    currency: code,
    maximumFractionDigits: 0
  }).format(value ?? 0)
}

export function formatDate(value) {
  if (!value) return '—'
  return new Date(value).toLocaleDateString('es-AR', {
    day: '2-digit',
    month: 'short',
    year: 'numeric'
  })
}

export function formatDateTime(value) {
  if (!value) return '—'
  return new Date(value).toLocaleString('es-AR', {
    day: '2-digit',
    month: 'short',
    year: 'numeric',
    hour: '2-digit',
    minute: '2-digit',
    hour12: false
  })
}
