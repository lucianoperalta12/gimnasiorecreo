<template>
  <div class="animate-fade-in space-y-6">
    <!-- Header -->
    <div class="flex flex-col md:flex-row md:items-center md:justify-between gap-4">
      <div>
        <h1 class="page-title">Estadísticas de Asistencia</h1>
        <p class="page-subtitle">Monitoreo de flujo de ingresos y horarios pico</p>
      </div>

      <!-- Período Selector -->
      <div class="flex flex-wrap items-center gap-2 sm:gap-3 self-start md:self-auto w-full md:w-auto">
        <!-- Gym Selector (Solo Superusuario) -->
        <div v-if="authStore.hasRole('Superusuario')" class="w-full md:w-64">
          <AppSearchSelect v-model="gymId" :options="gymOptions" placeholder="Todos los gimnasios" />
        </div>

        <button
          v-for="p in periodos"
          :key="p.id"
          @click="cambiarPeriodo(p.id)"
          class="px-4 py-2 rounded-xl text-xs font-black uppercase tracking-wider transition-all duration-300 border"
          :class="
            periodoActivo === p.id
              ? 'bg-primary-600 border-primary-500 text-white shadow-[0_0_15px_rgba(255,102,0,0.2)]'
              : 'bg-dark-900/40 border-dark-800/80 text-dark-400 hover:bg-dark-800/60 hover:text-dark-200'
          "
        >
          {{ p.label }}
        </button>
      </div>
    </div>

    <!-- Spinner -->
    <div v-if="loading" class="flex items-center justify-center py-24">
      <div class="w-10 h-10 border-4 border-primary-500/20 border-t-primary-500 rounded-full animate-spin"></div>
    </div>

    <template v-else>
      <!-- KPI Cards -->
      <div class="grid grid-cols-1 sm:grid-cols-3 gap-3 sm:gap-4">
        <!-- Card 1: Total -->
        <div class="p-4 sm:p-6 rounded-2xl sm:rounded-[2rem] bg-dark-900/40 border border-dark-800/50 backdrop-blur-md relative overflow-hidden group">
          <div
            class="absolute -right-4 -bottom-4 w-12 sm:w-24 h-12 sm:h-24 bg-primary-500/5 rounded-full blur-xl group-hover:bg-primary-500/10 transition-all duration-500"
          ></div>
          <p class="text-[10px] sm:text-[10px] text-dark-500 uppercase tracking-[0.15em] sm:tracking-[0.2em] font-black">Total Accesos</p>
          <p class="text-xl sm:text-4xl font-black text-white mt-1.5 sm:mt-3 tracking-tight leading-none">{{ totalVisitas }}</p>
          <p class="hidden sm:block text-xs text-dark-400 mt-2">Ingresos registrados en el período</p>
        </div>

        <!-- Card 2: Promedio diario -->
        <div class="p-4 sm:p-6 rounded-2xl sm:rounded-[2rem] bg-dark-900/40 border border-dark-800/50 backdrop-blur-md relative overflow-hidden group">
          <div
            class="absolute -right-4 -bottom-4 w-12 sm:w-24 h-12 sm:h-24 bg-emerald-500/5 rounded-full blur-xl group-hover:bg-emerald-500/10 transition-all duration-500"
          ></div>
          <p class="text-[10px] sm:text-[10px] text-dark-500 uppercase tracking-[0.15em] sm:tracking-[0.2em] font-black">Promedio Diario</p>
          <p class="text-xl sm:text-4xl font-black text-white mt-1.5 sm:mt-3 tracking-tight leading-none">{{ promedioDiario.toFixed(1) }}</p>
          <p class="hidden sm:block text-xs text-dark-400 mt-2">Visitas promedio por día</p>
        </div>

        <!-- Card 3: Hora Pico -->
        <div class="p-4 sm:p-6 rounded-2xl sm:rounded-[2rem] bg-dark-900/40 border border-dark-800/50 backdrop-blur-md relative overflow-hidden group">
          <div
            class="absolute -right-4 -bottom-4 w-12 sm:w-24 h-12 sm:h-24 bg-blue-500/5 rounded-full blur-xl group-hover:bg-blue-500/10 transition-all duration-500"
          ></div>
          <p class="text-[10px] sm:text-[10px] text-dark-500 uppercase tracking-[0.15em] sm:tracking-[0.2em] font-black">Hora Pico</p>
          <p class="text-xl sm:text-4xl font-black text-white mt-1.5 sm:mt-3 tracking-tight leading-none">
            {{ horaPico }}<span class="text-[10px] sm:text-base font-bold text-dark-400 ml-0.5">hs</span>
          </p>
          <p class="hidden sm:block text-xs text-dark-400 mt-2">Mayor volumen de ingresos registrado</p>
        </div>
      </div>

      <!-- Charts Section -->
      <div class="grid grid-cols-1 lg:grid-cols-2 gap-6">
        <!-- Chart 1: Distribución por Hora -->
        <div class="card p-6 flex flex-col h-[400px]">
          <div class="mb-4">
            <h3 class="text-sm font-bold text-white uppercase tracking-wider">Flujo Horario</h3>
            <p class="text-xs text-dark-500">Distribución de ingresos por rango horario (24 hs)</p>
          </div>

          <div class="flex-1 flex items-end gap-2 pt-4 pb-2 border-b border-dark-800/50 overflow-x-auto custom-scrollbar">
            <div v-for="h in histogramaHoras" :key="h.hora" class="flex-1 flex flex-col items-center group h-full justify-end min-w-[18px] sm:min-w-0">
              <!-- Tooltip flotante -->
              <div
                class="absolute mb-2 opacity-0 group-hover:opacity-100 transition-opacity bg-black border border-dark-800 text-[10px] text-white px-2 py-1 rounded-md pointer-events-none transform -translate-y-12 z-20 whitespace-nowrap shadow-xl"
              >
                {{ h.cantidad }} visitas ({{ h.porcentaje }}%)
              </div>
              <!-- Barra de Gráfico -->
              <div
                class="w-full rounded-t-md bg-gradient-to-t from-primary-600 to-primary-400 transition-all duration-500 group-hover:from-primary-500 group-hover:to-primary-300 group-hover:shadow-[0_0_15px_rgba(255,102,0,0.3)]"
                :style="{ height: `${Math.max(4, h.porcentaje)}%` }"
              ></div>
              <!-- Etiqueta eje X -->
              <span class="text-[9px] font-bold text-dark-500 mt-2 group-hover:text-dark-300">
                {{ h.hora.toString().padStart(2, '0') }}
              </span>
            </div>
          </div>
        </div>

        <!-- Chart 2: Distribución por Día de la Semana -->
        <div class="card p-6 flex flex-col h-[400px]">
          <div class="mb-4">
            <h3 class="text-sm font-bold text-white uppercase tracking-wider">Asistencia por Día</h3>
            <p class="text-xs text-dark-500">Distribución de ingresos según el día de la semana</p>
          </div>

          <div class="flex-1 flex flex-col justify-around pt-2">
            <div v-for="d in histogramaDias" :key="d.nombre" class="flex items-center gap-2 sm:gap-4 group">
              <!-- Día -->
              <span class="w-10 sm:w-20 text-[10px] sm:text-xs font-black text-dark-400 uppercase tracking-wide group-hover:text-dark-200 transition-colors">
                <span class="sm:hidden">{{ d.nombre.slice(0, 3) }}</span>
                <span class="hidden sm:inline">{{ d.nombre }}</span>
              </span>
              <!-- Barra Horizontal -->
              <div class="flex-1 h-5 bg-dark-950 rounded-lg overflow-hidden relative border border-dark-900/40">
                <!-- Porcentaje Barra -->
                <div
                  class="h-full bg-gradient-to-r from-primary-600 to-primary-400 rounded-lg transition-all duration-500 group-hover:from-primary-500 group-hover:to-primary-300"
                  :style="{ width: `${d.porcentaje}%` }"
                ></div>
                <!-- Cantidad flotante -->
                <span class="absolute inset-y-0 right-3 flex items-center text-[10px] font-black text-white drop-shadow-md">
                  {{ d.cantidad }}
                </span>
              </div>
              <!-- Porcentaje Eje -->
              <span class="w-8 sm:w-10 text-right text-[10px] sm:text-xs font-bold text-dark-500 group-hover:text-dark-300">
                {{ d.porcentaje.toFixed(0) }}%
              </span>
            </div>
          </div>
        </div>
      </div>

      <!-- Tabla de Detalles / Historial Rápido de este Rango -->
      <div class="card p-6">
        <div class="mb-4 flex items-center justify-between">
          <div>
            <h3 class="text-sm font-bold text-white uppercase tracking-wider">Detalles de Ingresos</h3>
            <p class="text-xs text-dark-500">Historial completo del rango seleccionado (últimos 100 registros)</p>
          </div>
          <span class="badge-primary">{{ ingresosFiltrados.length }} registros</span>
        </div>

        <div class="table-container max-h-[300px] overflow-y-auto custom-scrollbar">
          <table class="table text-xs">
            <thead>
              <tr>
                <th>Alumno</th>
                <th class="hidden sm:table-cell">DNI</th>
                <th>Día y Hora</th>
                <th class="hidden sm:table-cell">Terminal</th>
                <th>Membresía</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="item in ingresosFiltrados.slice(0, 100)" :key="item.id" class="hover:bg-dark-900/20">
                <td class="font-medium text-white">{{ item.alumno }}</td>
                <td class="hidden sm:table-cell">{{ item.dni }}</td>
                <td>{{ formatDateTime(item.fechaHora) }}</td>
                <td class="hidden sm:table-cell">{{ item.terminal }}</td>
                <td>
                  <span class="badge-primary !py-0.5 !px-2">{{ item.tipoMembresia || '—' }}</span>
                </td>
              </tr>
              <tr v-if="!ingresosFiltrados.length">
                <td colspan="5" class="text-center py-6 text-dark-500">No hay registros de ingresos en este período.</td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>
    </template>
  </div>
</template>

<script setup>
import { computed, onMounted, ref, watch } from 'vue';
import { ingresosApi } from '@/api/ingresos.api';
import { gymsApi } from '@/api/gyms.api';
import { useAuthStore } from '@/stores/auth.store';
import { useNotification } from '@/composables/useNotification';
import AppSearchSelect from '@/components/ui/AppSearchSelect.vue';
import { formatLocalDate } from '@/utils/date';

const { error: showError } = useNotification();
const authStore = useAuthStore();
const loading = ref(false);
const periodoActivo = ref('7d');
const ingresosRaw = ref([]);
const gymId = ref(null);
const gyms = ref([]);

const gymOptions = computed(() => [{ id: null, label: 'Todos los gimnasios' }, ...gyms.value.map((g) => ({ id: g.id, label: g.nombre }))]);

watch(gymId, async () => {
  await cargarEstadisticas();
});

const periodos = [
  { id: '7d', label: 'Últimos 7 días' },
  { id: 'este-mes', label: 'Este Mes' },
  { id: 'mes-anterior', label: 'Mes Anterior' },
];

function formatDateTime(val) {
  if (!val) return '—';
  return new Date(val).toLocaleString('es-AR', {
    day: '2-digit',
    month: 'short',
    year: 'numeric',
    hour: '2-digit',
    minute: '2-digit',
    hour12: false
  });
}

// Cambiar de período y fetch
async function cambiarPeriodo(id) {
  periodoActivo.value = id;
  await cargarEstadisticas();
}

// Calcular rango de fechas
function obtenerRangoFechas(id) {
  const hoy = new Date();
  let desde, hasta;

  if (id === '7d') {
    desde = new Date();
    desde.setDate(hoy.getDate() - 6);
    hasta = hoy;
  } else if (id === 'este-mes') {
    desde = new Date(hoy.getFullYear(), hoy.getMonth(), 1);
    hasta = hoy;
  } else if (id === 'mes-anterior') {
    desde = new Date(hoy.getFullYear(), hoy.getMonth() - 1, 1);
    hasta = new Date(hoy.getFullYear(), hoy.getMonth(), 0); // Último día del mes anterior
  }

  return { desde: formatLocalDate(desde), hasta: formatLocalDate(hasta) };
}

async function cargarEstadisticas() {
  loading.value = true;
  try {
    const { desde, hasta } = obtenerRangoFechas(periodoActivo.value);
    const params = { fechaDesde: desde, fechaHasta: hasta };
    if (authStore.hasRole('Superusuario') && gymId.value) {
      params.gymId = gymId.value;
    }
    const { data } = await ingresosApi.getAll(params);
    ingresosRaw.value = data || [];
  } catch (err) {
    showError(err.response?.data?.error || 'No se pudieron cargar los datos de asistencia');
  } finally {
    loading.value = false;
  }
}

// Procesamiento de datos local
const ingresosFiltrados = computed(() => {
  return [...ingresosRaw.value].sort((a, b) => new Date(b.fechaHora) - new Date(a.fechaHora));
});

const totalVisitas = computed(() => ingresosFiltrados.value.length);

const promedioDiario = computed(() => {
  if (totalVisitas.value === 0) return 0;
  const fechas = ingresosFiltrados.value.map((x) => new Date(x.fechaHora).toDateString());
  const diasUnicos = new Set(fechas).size || 1;
  return totalVisitas.value / diasUnicos;
});

// Horario pico (horas más repetidas)
const horaPico = computed(() => {
  if (totalVisitas.value === 0) return '—';
  const horasCount = {};
  ingresosFiltrados.value.forEach((x) => {
    const hora = new Date(x.fechaHora).getHours();
    horasCount[hora] = (horasCount[hora] || 0) + 1;
  });

  let maxHora = '—';
  let maxCant = 0;
  Object.keys(horasCount).forEach((h) => {
    if (horasCount[h] > maxCant) {
      maxCant = horasCount[h];
      maxHora = h;
    }
  });
  return maxHora;
});

// Histograma de Horas (de 7 a 22)
const histogramaHoras = computed(() => {
  const horasFrecuentes = [7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22];
  const counts = {};
  horasFrecuentes.forEach((h) => {
    counts[h] = 0;
  });

  ingresosFiltrados.value.forEach((x) => {
    const h = new Date(x.fechaHora).getHours();
    if (h in counts) {
      counts[h]++;
    }
  });

  const maxVal = Math.max(...Object.values(counts)) || 1;

  return horasFrecuentes.map((hora) => {
    const cant = counts[hora];
    return {
      hora,
      cantidad: cant,
      porcentaje: (cant / maxVal) * 100,
    };
  });
});

// Histograma de Días
const histogramaDias = computed(() => {
  const diasNombres = ['Lunes', 'Martes', 'Miércoles', 'Jueves', 'Viernes', 'Sábado', 'Domingo'];
  const indexMap = [6, 0, 1, 2, 3, 4, 5]; // Ajuste para Domingo=0 de JS -> Domingo es el índice 6
  const counts = Array(7).fill(0);

  ingresosFiltrados.value.forEach((x) => {
    const dayIndex = new Date(x.fechaHora).getDay(); // 0 = Domingo, 1 = Lunes...
    const adjusted = indexMap[dayIndex];
    counts[adjusted]++;
  });

  const total = totalVisitas.value || 1;
  return diasNombres.map((nombre, i) => {
    return {
      nombre,
      cantidad: counts[i],
      porcentaje: (counts[i] / total) * 100,
    };
  });
});

onMounted(async () => {
  const promises = [];
  if (authStore.hasRole('Superusuario')) {
    promises.push(
      gymsApi.getAll().then((res) => {
        gyms.value = res.data || [];
      })
    );
  }
  promises.push(cargarEstadisticas());
  await Promise.allSettled(promises);
});
</script>
