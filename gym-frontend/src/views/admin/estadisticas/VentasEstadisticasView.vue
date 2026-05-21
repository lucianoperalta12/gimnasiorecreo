<template>
  <div class="animate-fade-in space-y-6">
    <!-- Header -->
    <div class="flex flex-col md:flex-row md:items-center md:justify-between gap-4">
      <div>
        <h1 class="page-title">Ventas y Membresías</h1>
        <p class="page-subtitle">Métricas comerciales, recaudación y popularidad de planes</p>
      </div>
      <div class="flex items-center gap-3 self-start md:self-auto w-full md:w-auto">
        <!-- Gym Selector (Solo Superusuario) -->
        <div v-if="authStore.hasRole('Superusuario')" class="w-64">
          <AppSearchSelect
            v-model="gymId"
            :options="gymOptions"
            placeholder="Todos los gimnasios"
          />
        </div>

        <button
          @click="cargarDatos"
          class="btn-secondary flex items-center gap-2"
          :disabled="loading"
        >
          <svg
            class="w-4 h-4"
            :class="{ 'animate-spin': loading }"
            fill="none"
            viewBox="0 0 24 24"
            stroke="currentColor"
            stroke-width="2"
          >
            <path stroke-linecap="round" stroke-linejoin="round" d="M16.023 9.348h4.992v-.001M2.985 19.644v-4.992m0 0h4.992m-4.993 0 3.181 3.183a8.25 8.25 0 0 0 13.803-3.7M4.031 9.865a8.25 8.25 0 0 1 13.803-3.7l3.181 3.182m0-4.991v4.99" />
          </svg>
          Actualizar
        </button>
      </div>
    </div>

    <!-- Spinner -->
    <div v-if="loading" class="flex items-center justify-center py-24">
      <div class="w-10 h-10 border-4 border-primary-500/20 border-t-primary-500 rounded-full animate-spin"></div>
    </div>

    <template v-else>
      <!-- KPI Cards -->
      <div class="grid grid-cols-1 sm:grid-cols-3 gap-4">
        <!-- KPI 1: Recaudación Mes Actual -->
        <div class="p-6 rounded-[2rem] bg-dark-900/40 border border-dark-800/50 backdrop-blur-md relative overflow-hidden group">
          <div class="absolute -right-4 -bottom-4 w-24 h-24 bg-emerald-500/5 rounded-full blur-xl group-hover:bg-emerald-500/10 transition-all duration-500"></div>
          <p class="text-[10px] text-dark-500 uppercase tracking-[0.2em] font-black">Recaudación este Mes</p>
          <p class="text-4xl font-black text-white mt-3 tracking-tight leading-none">
            {{ formatMoneda(recaudacionMesActual) }}
          </p>
          <div class="flex items-center gap-1.5 mt-2.5">
            <span
              class="text-[10px] font-black px-1.5 py-0.5 rounded-md"
              :class="tendenciaVentas >= 0 ? 'bg-emerald-500/10 text-emerald-400' : 'bg-rose-500/10 text-rose-400'"
            >
              {{ tendenciaVentas >= 0 ? '+' : '' }}{{ tendenciaVentas.toFixed(1) }}%
            </span>
            <span class="text-[11px] text-dark-500">vs. mes anterior</span>
          </div>
        </div>

        <!-- KPI 2: Membresías Activas -->
        <div class="p-6 rounded-[2rem] bg-dark-900/40 border border-dark-800/50 backdrop-blur-md relative overflow-hidden group">
          <div class="absolute -right-4 -bottom-4 w-24 h-24 bg-primary-500/5 rounded-full blur-xl group-hover:bg-primary-500/10 transition-all duration-500"></div>
          <p class="text-[10px] text-dark-500 uppercase tracking-[0.2em] font-black">Membresías Activas</p>
          <p class="text-4xl font-black text-white mt-3 tracking-tight leading-none">
            {{ membershipsActivasCount }}
          </p>
          <p class="text-xs text-dark-400 mt-2">Alumnos habilitados para ingresar</p>
        </div>

        <!-- KPI 3: Ticket Promedio -->
        <div class="p-6 rounded-[2rem] bg-dark-900/40 border border-dark-800/50 backdrop-blur-md relative overflow-hidden group">
          <div class="absolute -right-4 -bottom-4 w-24 h-24 bg-blue-500/5 rounded-full blur-xl group-hover:bg-blue-500/10 transition-all duration-500"></div>
          <p class="text-[10px] text-dark-500 uppercase tracking-[0.2em] font-black">Cobro Promedio</p>
          <p class="text-4xl font-black text-white mt-3 tracking-tight leading-none">
            {{ formatMoneda(ticketPromedio) }}
          </p>
          <p class="text-xs text-dark-400 mt-2">Valor medio por pago procesado</p>
        </div>
      </div>

      <!-- Charts Section -->
      <div class="grid grid-cols-1 lg:grid-cols-3 gap-6">
        <!-- Distribución de Membresías por Plan -->
        <div class="card p-6 lg:col-span-2 flex flex-col min-h-[350px]">
          <div class="mb-4">
            <h3 class="text-sm font-bold text-white uppercase tracking-wider">Membresías por Plan</h3>
            <p class="text-xs text-dark-500">Distribución de membresías activas por plan de entrenamiento</p>
          </div>
          
          <div class="flex-1 flex flex-col justify-center space-y-4">
            <div v-for="plan in planesDistribucion" :key="plan.nombre" class="group">
              <div class="flex items-center justify-between text-xs font-bold mb-1">
                <span class="text-dark-200 uppercase tracking-wide">{{ plan.nombre }}</span>
                <span class="text-white">{{ plan.cantidad }} alumnos ({{ plan.porcentaje.toFixed(0) }}%)</span>
              </div>
              <div class="w-full h-3 bg-dark-950 rounded-full overflow-hidden border border-dark-900/50">
                <div
                  class="h-full bg-gradient-to-r from-primary-600 to-primary-400 rounded-full transition-all duration-500 group-hover:from-primary-500 group-hover:to-primary-300"
                  :style="{ width: `${plan.porcentaje}%` }"
                ></div>
              </div>
            </div>
            <div v-if="!planesDistribucion.length" class="text-center py-12 text-dark-500 text-xs">
              No hay membresías activas registradas.
            </div>
          </div>
        </div>

        <!-- Alumnos Más Activos -->
        <div class="card p-6 flex flex-col min-h-[350px]">
          <div class="mb-4">
            <h3 class="text-sm font-bold text-white uppercase tracking-wider">Top Alumnos Activos</h3>
            <p class="text-xs text-dark-500">Alumnos con mayor asistencia registrada este mes</p>
          </div>

          <div class="flex-1 overflow-y-auto custom-scrollbar space-y-3">
            <div
              v-for="(alumno, index) in alumnosMasActivos"
              :key="alumno.dni"
              class="flex items-center gap-3 p-3 rounded-2xl bg-dark-950/40 border border-dark-900/60 hover:border-primary-500/20 hover:bg-dark-900/60 transition-all duration-300"
            >
              <!-- Puesto -->
              <div
                class="w-7 h-7 rounded-lg flex items-center justify-center text-xs font-black"
                :class="
                  index === 0
                    ? 'bg-amber-500/10 text-amber-500 border border-amber-500/20'
                    : index === 1
                    ? 'bg-slate-300/10 text-slate-300 border border-slate-300/20'
                    : index === 2
                    ? 'bg-amber-700/10 text-amber-700 border border-amber-700/20'
                    : 'bg-dark-900 text-dark-400'
                "
              >
                {{ index + 1 }}
              </div>
              
              <!-- Detalles Alumno -->
              <div class="flex-1 min-w-0">
                <p class="text-xs font-black text-white truncate leading-none mb-1">{{ alumno.nombre }}</p>
                <p class="text-[10px] text-dark-500">DNI: {{ alumno.dni }}</p>
              </div>

              <!-- Contador de Ingresos -->
              <div class="text-right">
                <span class="text-xs font-black text-primary-500">{{ alumno.cantidad }}</span>
                <p class="text-[9px] text-dark-500 uppercase tracking-widest leading-none mt-0.5">visitas</p>
              </div>
            </div>
            
            <div v-if="!alumnosMasActivos.length" class="text-center py-12 text-dark-500 text-xs">
              No hay registros de ingresos este mes.
            </div>
          </div>
        </div>
      </div>

      <!-- Tabla de Pagos Recientes -->
      <div class="card p-6">
        <div class="mb-4 flex items-center justify-between">
          <div>
            <h3 class="text-sm font-bold text-white uppercase tracking-wider">Historial de Cobros Recientes</h3>
            <p class="text-xs text-dark-500">Últimos cobros registrados en el sistema comercial</p>
          </div>
          <span class="badge-primary">{{ pagosRecientes.length }} cobros este mes</span>
        </div>

        <div class="table-container max-h-[300px] overflow-y-auto custom-scrollbar">
          <table class="table text-xs">
            <thead>
              <tr>
                <th>Alumno</th>
                <th>Fecha de Pago</th>
                <th>Concepto / Plan</th>
                <th>Monto</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="item in pagosRecientes.slice(0, 50)" :key="item.id" class="hover:bg-dark-900/20">
                <td class="font-medium text-white">{{ item.alumnoNombreCompleto }}</td>
                <td>{{ formatDateTime(item.fechaPago) }}</td>
                <td>{{ item.planNombre }}</td>
                <td class="font-black text-emerald-400">{{ formatMoneda(item.monto) }}</td>
              </tr>
              <tr v-if="!pagosRecientes.length">
                <td colspan="4" class="text-center py-6 text-dark-500">No hay pagos registrados en este período.</td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>
    </template>
  </div>
</template>

<script setup>
import { computed, onMounted, ref, watch } from 'vue'
import { paymentsApi } from '@/api/payments.api'
import { membershipsApi } from '@/api/memberships.api'
import { ingresosApi } from '@/api/ingresos.api'
import { gymsApi } from '@/api/gyms.api'
import { useAuthStore } from '@/stores/auth.store'
import { useNotification } from '@/composables/useNotification'
import AppSearchSelect from '@/components/ui/AppSearchSelect.vue'

const { error: showError } = useNotification()
const authStore = useAuthStore()
const loading = ref(false)

const memberships = ref([])
const payments = ref([])
const ingresosMes = ref([])
const gymId = ref(null)
const gyms = ref([])

const gymOptions = computed(() => [
  { id: null, label: 'Todos los gimnasios' },
  ...gyms.value.map(g => ({ id: g.id, label: g.nombre }))
])

watch(gymId, async () => {
  await cargarDatos()
})

function formatMoneda(val) {
  return new Intl.NumberFormat('es-AR', { style: 'currency', currency: 'ARS', minimumFractionDigits: 0 }).format(val)
}

function formatDateTime(val) {
  return new Date(val).toLocaleDateString('es-AR')
}

async function cargarDatos() {
  loading.value = true
  try {
    const hoy = new Date()
    const inicioMes = new Date(hoy.getFullYear(), hoy.getMonth(), 1).toISOString().slice(0, 10)
    const finMes = new Date(hoy.getFullYear(), hoy.getMonth() + 1, 0).toISOString().slice(0, 10)
    
    const params = {}
    if (authStore.hasRole('Superusuario') && gymId.value) {
      params.gymId = gymId.value
    }
    
    // Obtener rangos para pagos y memberships
    const [membershipsRes, paymentsRes, ingresosRes] = await Promise.all([
      membershipsApi.getAll(params),
      paymentsApi.getAll(params),
      ingresosApi.getAll({ fechaDesde: inicioMes, fechaHasta: finMes, ...params })
    ])
    
    memberships.value = membershipsRes.data || []
    payments.value = paymentsRes.data || []
    ingresosMes.value = ingresosRes.data || []
  } catch (err) {
    showError(err.response?.data?.error || 'No se pudieron cargar los datos de facturación')
  } finally {
    loading.value = false
  }
}

// Membresías Activas
const membershipsActivasCount = computed(() => {
  return memberships.value.filter(x => x.estado === 'Activa').length
})

// Pagos de este Mes
const pagosRecientes = computed(() => {
  const hoy = new Date()
  const mesActual = hoy.getMonth()
  const añoActual = hoy.getFullYear()
  
  return payments.value
    .filter(x => {
      const fecha = new Date(x.fechaPago)
      return fecha.getMonth() === mesActual && fecha.getFullYear() === añoActual
    })
    .sort((a, b) => new Date(b.fechaPago) - new Date(a.fechaPago))
})

const recaudacionMesActual = computed(() => {
  return pagosRecientes.value.reduce((acc, p) => acc + p.monto, 0)
})

// Recaudación Mes Anterior (para tendencia)
const recaudacionMesAnterior = computed(() => {
  const hoy = new Date()
  let mesAnterior = hoy.getMonth() - 1
  let añoAnterior = hoy.getFullYear()
  if (mesAnterior < 0) {
    mesAnterior = 11
    añoAnterior--
  }
  
  return payments.value
    .filter(x => {
      const fecha = new Date(x.fechaPago)
      return fecha.getMonth() === mesAnterior && fecha.getFullYear() === añoAnterior
    })
    .reduce((acc, p) => acc + p.monto, 0)
})

// Tendencia Ventas
const tendenciaVentas = computed(() => {
  const anterior = recaudacionMesAnterior.value
  if (anterior === 0) return recaudacionMesActual.value > 0 ? 100 : 0
  return ((recaudacionMesActual.value - anterior) / anterior) * 100
})

// Ticket Promedio (Mes Actual)
const ticketPromedio = computed(() => {
  if (pagosRecientes.value.length === 0) return 0
  return recaudacionMesActual.value / pagosRecientes.value.length
})

// Distribución de Membresías Activas por Plan
const planesDistribucion = computed(() => {
  const activas = memberships.value.filter(x => x.estado === 'Activa')
  if (activas.length === 0) return []
  
  const counts = {}
  activas.forEach(m => {
    const nombrePlan = m.planNombre || 'Sin Plan'
    counts[nombrePlan] = (counts[nombrePlan] || 0) + 1
  })
  
  const total = activas.length
  return Object.keys(counts).map(nombre => {
    return {
      nombre,
      cantidad: counts[nombre],
      porcentaje: (counts[nombre] / total) * 100
    }
  }).sort((a, b) => b.cantidad - a.cantidad)
})

// Alumnos Más Activos de este Mes
const alumnosMasActivos = computed(() => {
  if (ingresosMes.value.length === 0) return []
  
  const counts = {}
  ingresosMes.value.forEach(x => {
    const key = x.dni
    if (!counts[key]) {
      counts[key] = { nombre: x.alumno, dni: x.dni, cantidad: 0 }
    }
    counts[key].cantidad++
  })
  
  return Object.values(counts)
    .sort((a, b) => b.cantidad - a.cantidad)
    .slice(0, 5)
})

onMounted(async () => {
  const promises = []
  if (authStore.hasRole('Superusuario')) {
    promises.push(gymsApi.getAll().then(res => {
      gyms.value = res.data || []
    }))
  }
  promises.push(cargarDatos())
  await Promise.allSettled(promises)
})
</script>
