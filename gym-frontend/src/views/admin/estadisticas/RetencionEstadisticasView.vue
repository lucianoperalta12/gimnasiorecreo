<template>
  <div class="animate-fade-in space-y-6">
    <!-- Header -->
    <div class="flex flex-col md:flex-row md:items-center md:justify-between gap-4">
      <div>
        <h1 class="page-title">Retención y Deserción</h1>
        <p class="page-subtitle">Análisis de churn rate, retención y evolución de alumnos</p>
      </div>

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
      <div class="grid grid-cols-2 sm:grid-cols-4 gap-3 sm:gap-4">
        <!-- Retención Actual -->
        <div class="p-4 sm:p-6 rounded-2xl sm:rounded-[2rem] bg-dark-900/40 border border-dark-800/50 backdrop-blur-md relative overflow-hidden group">
          <div class="absolute -right-4 -bottom-4 w-12 sm:w-24 h-12 sm:h-24 bg-emerald-500/5 rounded-full blur-xl group-hover:bg-emerald-500/10 transition-all duration-500"></div>
          <p class="text-[10px] text-dark-500 uppercase tracking-[0.15em] sm:tracking-[0.2em] font-black">Retención</p>
          <p class="text-xl sm:text-4xl font-black mt-1.5 sm:mt-3 tracking-tight leading-none" :class="kpiRetencion >= 70 ? 'text-emerald-400' : kpiRetencion >= 50 ? 'text-amber-400' : 'text-rose-400'">
            {{ kpiRetencion.toFixed(1) }}<span class="text-[10px] sm:text-base font-bold text-dark-400 ml-0.5">%</span>
          </p>
          <div class="flex items-center gap-1 sm:gap-1.5 mt-1.5 sm:mt-2.5">
            <span
              class="text-[9px] sm:text-[10px] font-black px-1.5 py-0.5 rounded-md"
              :class="kpiRetencionTendencia >= 0 ? 'bg-emerald-500/10 text-emerald-400' : 'bg-rose-500/10 text-rose-400'"
            >
              {{ kpiRetencionTendencia >= 0 ? '+' : '' }}{{ kpiRetencionTendencia.toFixed(1) }}%
            </span>
            <span class="text-[9px] sm:text-[11px] text-dark-500">vs. mes ant.</span>
          </div>
        </div>

        <!-- Churn Rate -->
        <div class="p-4 sm:p-6 rounded-2xl sm:rounded-[2rem] bg-dark-900/40 border border-dark-800/50 backdrop-blur-md relative overflow-hidden group">
          <div class="absolute -right-4 -bottom-4 w-12 sm:w-24 h-12 sm:h-24 bg-rose-500/5 rounded-full blur-xl group-hover:bg-rose-500/10 transition-all duration-500"></div>
          <p class="text-[10px] text-dark-500 uppercase tracking-[0.15em] sm:tracking-[0.2em] font-black">Churn Rate</p>
          <p class="text-xl sm:text-4xl font-black mt-1.5 sm:mt-3 tracking-tight leading-none" :class="kpiChurn <= 10 ? 'text-emerald-400' : kpiChurn <= 25 ? 'text-amber-400' : 'text-rose-400'">
            {{ kpiChurn.toFixed(1) }}<span class="text-[10px] sm:text-base font-bold text-dark-400 ml-0.5">%</span>
          </p>
          <div class="flex items-center gap-1 sm:gap-1.5 mt-1.5 sm:mt-2.5">
            <span
              class="text-[9px] sm:text-[10px] font-black px-1.5 py-0.5 rounded-md"
              :class="kpiChurnTendencia <= 0 ? 'bg-emerald-500/10 text-emerald-400' : 'bg-rose-500/10 text-rose-400'"
            >
              {{ kpiChurnTendencia >= 0 ? '+' : '' }}{{ kpiChurnTendencia.toFixed(1) }}%
            </span>
            <span class="text-[9px] sm:text-[11px] text-dark-500">vs. mes ant.</span>
          </div>
        </div>

        <!-- Total Activos -->
        <div class="p-4 sm:p-6 rounded-2xl sm:rounded-[2rem] bg-dark-900/40 border border-dark-800/50 backdrop-blur-md relative overflow-hidden group">
          <div class="absolute -right-4 -bottom-4 w-12 sm:w-24 h-12 sm:h-24 bg-primary-500/5 rounded-full blur-xl group-hover:bg-primary-500/10 transition-all duration-500"></div>
          <p class="text-[10px] text-dark-500 uppercase tracking-[0.15em] sm:tracking-[0.2em] font-black">Activos</p>
          <p class="text-xl sm:text-4xl font-black text-white mt-1.5 sm:mt-3 tracking-tight leading-none">{{ kpiActivos }}</p>
          <p class="hidden sm:block text-xs text-dark-400 mt-2">Alumnos con membresía vigente</p>
        </div>

        <!-- Nuevos este mes -->
        <div class="p-4 sm:p-6 rounded-2xl sm:rounded-[2rem] bg-dark-900/40 border border-dark-800/50 backdrop-blur-md relative overflow-hidden group">
          <div class="absolute -right-4 -bottom-4 w-12 sm:w-24 h-12 sm:h-24 bg-blue-500/5 rounded-full blur-xl group-hover:bg-blue-500/10 transition-all duration-500"></div>
          <p class="text-[10px] text-dark-500 uppercase tracking-[0.15em] sm:tracking-[0.2em] font-black">Nuevos</p>
          <p class="text-xl sm:text-4xl font-black text-white mt-1.5 sm:mt-3 tracking-tight leading-none">{{ kpiNuevos }}</p>
          <p class="hidden sm:block text-xs text-dark-400 mt-2">Altas en el último mes</p>
        </div>
      </div>

      <!-- Charts Section -->
      <div class="grid grid-cols-1 lg:grid-cols-2 gap-6">
        <!-- Chart 1: Evolución Mensual (Activos vs Bajas) -->
        <div class="card p-6 flex flex-col min-h-[380px]">
          <div class="mb-4">
            <h3 class="text-sm font-bold text-white uppercase tracking-wider">Evolución Mensual</h3>
            <p class="text-xs text-dark-500">Alumnos activos vs bajas por mes</p>
          </div>

          <div class="flex-1 flex flex-col justify-end" v-if="dataMensual.length">
            <!-- Leyenda -->
            <div class="flex items-center gap-4 mb-4">
              <div class="flex items-center gap-1.5">
                <div class="w-3 h-3 rounded-sm bg-emerald-500"></div>
                <span class="text-[10px] font-bold text-dark-400 uppercase tracking-wider">Activos</span>
              </div>
              <div class="flex items-center gap-1.5">
                <div class="w-3 h-3 rounded-sm bg-rose-500"></div>
                <span class="text-[10px] font-bold text-dark-400 uppercase tracking-wider">Bajas</span>
              </div>
              <div class="flex items-center gap-1.5">
                <div class="w-3 h-3 rounded-sm bg-blue-400"></div>
                <span class="text-[10px] font-bold text-dark-400 uppercase tracking-wider">Nuevos</span>
              </div>
            </div>

            <!-- Barras -->
            <div class="flex items-end gap-1.5 sm:gap-3 h-[220px] border-b border-dark-800/50 pb-2">
              <div v-for="mes in dataMensual" :key="mes.label" class="flex-1 flex flex-col items-center gap-1 group h-full justify-end min-w-[30px]">
                <!-- Tooltip -->
                <div class="absolute mb-2 opacity-0 group-hover:opacity-100 transition-opacity bg-black border border-dark-800 text-[10px] text-white px-2.5 py-1.5 rounded-lg pointer-events-none transform -translate-y-[220px] z-20 whitespace-nowrap shadow-xl">
                  <p class="font-black text-emerald-400">{{ mes.activosFin }} activos</p>
                  <p class="text-blue-400">+{{ mes.nuevos }} nuevos</p>
                  <p class="text-rose-400">-{{ mes.bajas }} bajas</p>
                  <p class="text-dark-300 mt-1 border-t border-dark-800 pt-1">Ret: {{ mes.retencion.toFixed(1) }}% · Churn: {{ mes.churn.toFixed(1) }}%</p>
                </div>

                <!-- Barras apiladas -->
                <div class="w-full flex flex-col items-center gap-0.5">
                  <!-- Barra activos -->
                  <div
                    class="w-full rounded-t-md bg-gradient-to-t from-emerald-600 to-emerald-400 transition-all duration-500 group-hover:from-emerald-500 group-hover:to-emerald-300 group-hover:shadow-[0_0_10px_rgba(16,185,129,0.3)]"
                    :style="{ height: `${barHeight(mes.activosFin)}px` }"
                  ></div>
                  <!-- Barra bajas -->
                  <div
                    v-if="mes.bajas > 0"
                    class="w-full rounded-t-md bg-gradient-to-t from-rose-600 to-rose-400 transition-all duration-500 group-hover:from-rose-500 group-hover:to-rose-300"
                    :style="{ height: `${barHeight(mes.bajas)}px` }"
                  ></div>
                </div>

                <!-- Label mes -->
                <span class="text-[9px] font-bold text-dark-500 mt-1 group-hover:text-dark-300 uppercase">
                  {{ mes.label }}
                </span>
              </div>
            </div>
          </div>
          <div v-else class="flex-1 flex items-center justify-center text-dark-500 text-xs">No hay datos para el período seleccionado.</div>
        </div>

        <!-- Chart 2: Retención vs Churn mensual -->
        <div class="card p-6 flex flex-col min-h-[380px]">
          <div class="mb-4">
            <h3 class="text-sm font-bold text-white uppercase tracking-wider">Retención vs Churn</h3>
            <p class="text-xs text-dark-500">Porcentajes mensuales de retención y deserción</p>
          </div>

          <div class="flex-1 flex flex-col justify-end" v-if="dataMensual.length">
            <!-- Leyenda -->
            <div class="flex items-center gap-4 mb-4">
              <div class="flex items-center gap-1.5">
                <div class="w-3 h-3 rounded-sm bg-emerald-500"></div>
                <span class="text-[10px] font-bold text-dark-400 uppercase tracking-wider">Retención %</span>
              </div>
              <div class="flex items-center gap-1.5">
                <div class="w-3 h-3 rounded-sm bg-rose-500"></div>
                <span class="text-[10px] font-bold text-dark-400 uppercase tracking-wider">Churn %</span>
              </div>
            </div>

            <!-- Barras horizontales por mes -->
            <div class="flex-1 flex flex-col justify-around pt-2 space-y-1">
              <div v-for="mes in dataMensual" :key="'rate-' + mes.label" class="group">
                <div class="flex items-center gap-2 sm:gap-3">
                  <span class="w-12 sm:w-16 text-[10px] sm:text-xs font-black text-dark-400 uppercase tracking-wide group-hover:text-dark-200 transition-colors text-right">
                    {{ mes.label }}
                  </span>
                  <div class="flex-1 flex flex-col gap-1">
                    <!-- Retención -->
                    <div class="h-3 bg-dark-950 rounded-full overflow-hidden border border-dark-900/40 relative">
                      <div
                        class="h-full bg-gradient-to-r from-emerald-600 to-emerald-400 rounded-full transition-all duration-500 group-hover:from-emerald-500 group-hover:to-emerald-300"
                        :style="{ width: `${Math.min(mes.retencion, 100)}%` }"
                      ></div>
                    </div>
                    <!-- Churn -->
                    <div class="h-3 bg-dark-950 rounded-full overflow-hidden border border-dark-900/40 relative">
                      <div
                        class="h-full bg-gradient-to-r from-rose-600 to-rose-400 rounded-full transition-all duration-500 group-hover:from-rose-500 group-hover:to-rose-300"
                        :style="{ width: `${Math.min(mes.churn * 2, 100)}%` }"
                      ></div>
                    </div>
                  </div>
                  <div class="w-20 sm:w-24 text-right">
                    <span class="text-[10px] font-black text-emerald-400">{{ mes.retencion.toFixed(0) }}%</span>
                    <span class="text-[10px] text-dark-600 mx-0.5">/</span>
                    <span class="text-[10px] font-black text-rose-400">{{ mes.churn.toFixed(0) }}%</span>
                  </div>
                </div>
              </div>
            </div>
          </div>
          <div v-else class="flex-1 flex items-center justify-center text-dark-500 text-xs">No hay datos para el período seleccionado.</div>
        </div>
      </div>

      <!-- Tabla Detallada -->
      <div class="card p-6">
        <div class="mb-4 flex items-center justify-between">
          <div>
            <h3 class="text-sm font-bold text-white uppercase tracking-wider">Detalle por Mes</h3>
            <p class="text-xs text-dark-500">Movimientos de alumnos y métricas de retención por período</p>
          </div>
          <span class="badge-primary">{{ dataMensual.length }} meses</span>
        </div>

        <div class="table-container overflow-x-auto custom-scrollbar">
          <table class="table text-xs">
            <thead>
              <tr>
                <th>Período</th>
                <th class="text-center">Inicio</th>
                <th class="text-center">Nuevos</th>
                <th class="text-center">Bajas</th>
                <th class="text-center">Fin</th>
                <th class="text-center">Retención</th>
                <th class="text-center">Churn</th>
                <th class="text-center hidden sm:table-cell">Tendencia</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="(mes, idx) in dataMensual" :key="'table-' + mes.label" class="hover:bg-dark-900/20">
                <td class="font-black text-white uppercase">{{ mes.labelLargo }}</td>
                <td class="text-center">{{ mes.activosInicio }}</td>
                <td class="text-center">
                  <span class="text-blue-400 font-bold">+{{ mes.nuevos }}</span>
                </td>
                <td class="text-center">
                  <span class="text-rose-400 font-bold" v-if="mes.bajas > 0">-{{ mes.bajas }}</span>
                  <span class="text-dark-600" v-else>0</span>
                </td>
                <td class="text-center font-bold text-white">{{ mes.activosFin }}</td>
                <td class="text-center">
                  <span
                    class="inline-block px-2 py-0.5 rounded-md text-[10px] font-black"
                    :class="mes.retencion >= 70 ? 'bg-emerald-500/10 text-emerald-400' : mes.retencion >= 50 ? 'bg-amber-500/10 text-amber-400' : 'bg-rose-500/10 text-rose-400'"
                  >
                    {{ mes.retencion.toFixed(1) }}%
                  </span>
                </td>
                <td class="text-center">
                  <span
                    class="inline-block px-2 py-0.5 rounded-md text-[10px] font-black"
                    :class="mes.churn <= 10 ? 'bg-emerald-500/10 text-emerald-400' : mes.churn <= 25 ? 'bg-amber-500/10 text-amber-400' : 'bg-rose-500/10 text-rose-400'"
                  >
                    {{ mes.churn.toFixed(1) }}%
                  </span>
                </td>
                <td class="text-center hidden sm:table-cell">
                  <template v-if="idx > 0">
                    <span v-if="mes.activosFin > dataMensual[idx - 1].activosFin" class="text-emerald-400 font-black text-[10px]">▲ Crecimiento</span>
                    <span v-else-if="mes.activosFin < dataMensual[idx - 1].activosFin" class="text-rose-400 font-black text-[10px]">▼ Caída</span>
                    <span v-else class="text-dark-500 font-bold text-[10px]">— Estable</span>
                  </template>
                  <span v-else class="text-dark-600 text-[10px]">—</span>
                </td>
              </tr>
              <tr v-if="!dataMensual.length">
                <td colspan="8" class="text-center py-6 text-dark-500">No hay datos de membresías para analizar.</td>
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
import { membershipsApi } from '@/api/memberships.api';
import { gymsApi } from '@/api/gyms.api';
import { useAuthStore } from '@/stores/auth.store';
import { useNotification } from '@/composables/useNotification';
import AppSearchSelect from '@/components/ui/AppSearchSelect.vue';

const { error: showError } = useNotification();
const authStore = useAuthStore();
const loading = ref(false);
const periodoActivo = ref('6m');
const membershipsRaw = ref([]);
const gymId = ref(null);
const gyms = ref([]);

const gymOptions = computed(() => [{ id: null, label: 'Todos los gimnasios' }, ...gyms.value.map((g) => ({ id: g.id, label: g.nombre }))]);

watch(gymId, async () => {
  await cargarDatos();
});

const periodos = [
  { id: '6m', label: '6 Meses' },
  { id: '12m', label: '12 Meses' },
];

function cambiarPeriodo(id) {
  periodoActivo.value = id;
}

// Generar lista de meses para el rango
function generarMeses(cantidadMeses) {
  const meses = [];
  const hoy = new Date();
  for (let i = cantidadMeses - 1; i >= 0; i--) {
    const fecha = new Date(hoy.getFullYear(), hoy.getMonth() - i, 1);
    meses.push({
      año: fecha.getFullYear(),
      mes: fecha.getMonth(),
      inicio: new Date(fecha.getFullYear(), fecha.getMonth(), 1),
      fin: new Date(fecha.getFullYear(), fecha.getMonth() + 1, 0, 23, 59, 59, 999),
    });
  }
  return meses;
}

const nombresMeses = ['Ene', 'Feb', 'Mar', 'Abr', 'May', 'Jun', 'Jul', 'Ago', 'Sep', 'Oct', 'Nov', 'Dic'];
const nombresMesesLargo = ['Enero', 'Febrero', 'Marzo', 'Abril', 'Mayo', 'Junio', 'Julio', 'Agosto', 'Septiembre', 'Octubre', 'Noviembre', 'Diciembre'];

async function cargarDatos() {
  loading.value = true;
  try {
    const params = {};
    if (authStore.hasRole('Superusuario') && gymId.value) {
      params.gymId = gymId.value;
    }
    const { data } = await membershipsApi.getAll(params);
    membershipsRaw.value = data || [];
  } catch (err) {
    showError(err.response?.data?.error || 'No se pudieron cargar los datos de membresías');
  } finally {
    loading.value = false;
  }
}

// Calcular datos mensuales de retención/churn
const dataMensual = computed(() => {
  if (!membershipsRaw.value.length) return [];

  const cantMeses = periodoActivo.value === '12m' ? 12 : 6;
  const meses = generarMeses(cantMeses);
  const memberships = membershipsRaw.value;

  return meses.map((mesInfo) => {
    const inicioMes = mesInfo.inicio;
    const finMes = mesInfo.fin;

    // Activos al inicio del mes: membresías cuya fechaInicio <= inicioMes y fechaVencimiento > inicioMes
    // y que no estuvieran canceladas antes del inicio del mes
    const activosInicio = new Set();
    memberships.forEach((m) => {
      const fi = new Date(m.fechaInicio);
      const fv = new Date(m.fechaVencimiento);
      // La membresía estaba activa al inicio del mes si empezó antes y vence después
      if (fi < inicioMes && fv >= inicioMes) {
        // Excluir canceladas que se cancelaron antes del mes (si estado es Cancelada y venció antes)
        if (m.estado === 'Cancelada' && fv < inicioMes) return;
        activosInicio.add(m.alumnoId);
      }
    });

    // Nuevos: alumnos cuya primera membresía comienza en este mes
    const nuevosSet = new Set();
    memberships.forEach((m) => {
      const fi = new Date(m.fechaInicio);
      if (fi >= inicioMes && fi <= finMes) {
        // Verificar si es su primera membresía
        const tieneAnterior = memberships.some((other) => {
          return other.alumnoId === m.alumnoId && new Date(other.fechaInicio) < inicioMes;
        });
        if (!tieneAnterior) {
          nuevosSet.add(m.alumnoId);
        }
      }
    });

    // Bajas: alumnos que tenían membresía activa al inicio o durante el mes,
    // y su membresía venció o se canceló durante el mes, sin renovación posterior
    const bajasSet = new Set();
    memberships.forEach((m) => {
      const fv = new Date(m.fechaVencimiento);
      const fi = new Date(m.fechaInicio);

      // La membresía venció o fue cancelada durante este mes
      const vencioDuranteMes = fv >= inicioMes && fv <= finMes && (m.estado === 'Vencida' || m.estado === 'Cancelada');

      if (vencioDuranteMes && fi < finMes) {
        // Verificar si el alumno tiene otra membresía activa después
        const tieneRenovacion = memberships.some((other) => {
          return other.alumnoId === m.alumnoId && other.id !== m.id && new Date(other.fechaInicio) >= inicioMes && new Date(other.fechaInicio) <= finMes && (other.estado === 'Activa' || new Date(other.fechaVencimiento) > finMes);
        });
        if (!tieneRenovacion) {
          bajasSet.add(m.alumnoId);
        }
      }
    });

    // Remover de bajas los que son nuevos y se dieron de baja en el mismo mes
    // para no contar doble
    const bajas = bajasSet.size;

    const activosInicioCount = activosInicio.size;
    const nuevos = nuevosSet.size;
    const activosFinCount = Math.max(0, activosInicioCount + nuevos - bajas);

    const retencion = activosInicioCount > 0 ? ((activosInicioCount - bajas) / activosInicioCount) * 100 : nuevos > 0 ? 100 : 0;
    const churn = activosInicioCount > 0 ? (bajas / activosInicioCount) * 100 : 0;

    return {
      label: nombresMeses[mesInfo.mes],
      labelLargo: `${nombresMesesLargo[mesInfo.mes]} ${mesInfo.año}`,
      activosInicio: activosInicioCount,
      nuevos,
      bajas,
      activosFin: activosFinCount,
      retencion: Math.max(0, Math.min(100, retencion)),
      churn: Math.max(0, Math.min(100, churn)),
    };
  });
});

// Altura de barra proporcional
const maxBarValue = computed(() => {
  if (!dataMensual.value.length) return 1;
  return Math.max(...dataMensual.value.map((m) => Math.max(m.activosFin, m.bajas)), 1);
});

function barHeight(value) {
  return Math.max(4, (value / maxBarValue.value) * 180);
}

// KPIs del último mes
const ultimoMes = computed(() => dataMensual.value.length ? dataMensual.value[dataMensual.value.length - 1] : null);
const penultimoMes = computed(() => dataMensual.value.length > 1 ? dataMensual.value[dataMensual.value.length - 2] : null);

const kpiRetencion = computed(() => ultimoMes.value?.retencion ?? 0);
const kpiChurn = computed(() => ultimoMes.value?.churn ?? 0);
const kpiActivos = computed(() => ultimoMes.value?.activosFin ?? 0);
const kpiNuevos = computed(() => ultimoMes.value?.nuevos ?? 0);

const kpiRetencionTendencia = computed(() => {
  if (!penultimoMes.value) return 0;
  return kpiRetencion.value - penultimoMes.value.retencion;
});

const kpiChurnTendencia = computed(() => {
  if (!penultimoMes.value) return 0;
  return kpiChurn.value - penultimoMes.value.churn;
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
  promises.push(cargarDatos());
  await Promise.allSettled(promises);
});
</script>
