<template>
  <div class="animate-fade-in space-y-6">
    <!-- Header -->
    <div class="flex flex-col md:flex-row md:items-center md:justify-between gap-4">
      <div>
        <h1 class="page-title">Balance</h1>
        <p class="page-subtitle">Ingresos vs. egresos mes a mes</p>
      </div>
      <div class="flex items-center gap-3 self-start md:self-auto w-full md:w-auto">
        <div v-if="authStore.hasRole('Superusuario')" class="flex-1 md:w-64 md:flex-initial">
          <AppSearchSelect v-model="gymId" :options="gymOptions" placeholder="Todos los gimnasios" />
        </div>
        <button @click="cargarDatos" class="btn-secondary flex items-center gap-2" :disabled="loading">
          <svg class="w-4 h-4" :class="{ 'animate-spin': loading }" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2">
            <path
              stroke-linecap="round"
              stroke-linejoin="round"
              d="M16.023 9.348h4.992v-.001M2.985 19.644v-4.992m0 0h4.992m-4.993 0 3.181 3.183a8.25 8.25 0 0 0 13.803-3.7M4.031 9.865a8.25 8.25 0 0 1 13.803-3.7l3.181 3.182m0-4.991v4.99"
            />
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
      <!-- KPI Cards del mes actual -->
      <div class="grid grid-cols-1 sm:grid-cols-3 gap-3 sm:gap-4">
        <!-- Ingresos del mes -->
        <div class="p-4 sm:p-6 rounded-2xl sm:rounded-[2rem] bg-dark-900/40 border border-dark-800/50 backdrop-blur-md relative overflow-hidden group">
          <div
            class="absolute -right-4 -bottom-4 w-12 sm:w-24 h-12 sm:h-24 bg-emerald-500/5 rounded-full blur-xl group-hover:bg-emerald-500/10 transition-all duration-500"
          ></div>
          <p class="text-[10px] text-dark-500 uppercase tracking-[0.15em] sm:tracking-[0.2em] font-black">Ingresos del Mes</p>
          <p class="text-xl sm:text-2xl font-black text-emerald-400 mt-1.5 sm:mt-2 tracking-tight leading-none">
            {{ formatMoneda(kpiMesActual.ingresos) }}
          </p>
          <p class="text-[10px] text-dark-500 mt-1.5">Pagos de membresías</p>
        </div>

        <!-- Egresos del mes -->
        <div class="p-4 sm:p-6 rounded-2xl sm:rounded-[2rem] bg-dark-900/40 border border-dark-800/50 backdrop-blur-md relative overflow-hidden group">
          <div
            class="absolute -right-4 -bottom-4 w-12 sm:w-24 h-12 sm:h-24 bg-rose-500/5 rounded-full blur-xl group-hover:bg-rose-500/10 transition-all duration-500"
          ></div>
          <p class="text-[10px] text-dark-500 uppercase tracking-[0.15em] sm:tracking-[0.2em] font-black">Egresos del Mes</p>
          <p class="text-xl sm:text-2xl font-black text-rose-400 mt-1.5 sm:mt-2 tracking-tight leading-none">
            {{ formatMoneda(kpiMesActual.egresos) }}
          </p>
          <p class="text-[10px] text-dark-500 mt-1.5">Gastos registrados</p>
        </div>

        <!-- Balance neto -->
        <div class="p-4 sm:p-6 rounded-2xl sm:rounded-[2rem] bg-dark-900/40 border border-dark-800/50 backdrop-blur-md relative overflow-hidden group">
          <div
            class="absolute -right-4 -bottom-4 w-12 sm:w-24 h-12 sm:h-24 rounded-full blur-xl transition-all duration-500"
            :class="kpiMesActual.balance >= 0 ? 'bg-emerald-500/5 group-hover:bg-emerald-500/10' : 'bg-rose-500/5 group-hover:bg-rose-500/10'"
          ></div>
          <p class="text-[10px] text-dark-500 uppercase tracking-[0.15em] sm:tracking-[0.2em] font-black">Balance Neto</p>
          <p
            class="text-xl sm:text-2xl font-black mt-1.5 sm:mt-2 tracking-tight leading-none"
            :class="kpiMesActual.balance >= 0 ? 'text-emerald-400' : 'text-rose-400'"
          >
            {{ formatMoneda(kpiMesActual.balance) }}
          </p>
          <p class="text-[10px] text-dark-500 mt-1.5">Ingresos − Egresos</p>
        </div>
      </div>

      <!-- Tabla histórica -->
      <div class="card p-6">
        <div class="mb-4">
          <h3 class="text-sm font-bold text-white uppercase tracking-wider">Historial por Mes</h3>
          <p class="text-xs text-dark-500 mt-1">Resumen agrupado de los últimos meses</p>
        </div>

        <div v-if="historial.length" class="table-container">
          <table class="table text-xs">
            <thead>
              <tr>
                <th>Mes</th>
                <th>Ingresos</th>
                <th>Egresos</th>
                <th>Balance</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="row in historial" :key="row.key">
                <td class="text-white font-bold">{{ row.mesLabel }}</td>
                <td class="font-black text-emerald-400">{{ formatMoneda(row.ingresos) }}</td>
                <td class="font-black text-rose-400">{{ formatMoneda(row.egresos) }}</td>
                <td>
                  <span class="font-black" :class="row.balance >= 0 ? 'text-emerald-400' : 'text-rose-400'">{{ formatMoneda(row.balance) }}</span>
                </td>
              </tr>
            </tbody>
          </table>
        </div>

        <div v-else class="text-center py-12 text-dark-500 text-xs">No hay datos registrados aún.</div>
      </div>
      <!-- Gráfico de barras por mes -->
      <div class="card p-6">
        <div class="mb-5">
          <h3 class="text-sm font-bold text-white uppercase tracking-wider">Evolución Mensual</h3>
          <p class="text-xs text-dark-500 mt-1">Ingresos y egresos de los últimos 12 meses</p>
        </div>

        <div v-if="historial.length" class="space-y-5">
          <div v-for="row in historialGrafico" :key="row.mesLabel" class="group">
            <div class="flex items-center justify-between text-[11px] font-bold mb-2">
              <span class="text-dark-300 uppercase tracking-wide w-20 shrink-0">{{ row.mesLabel }}</span>
              <div class="flex items-center gap-4 text-[10px]">
                <span class="text-emerald-400">+{{ formatMoneda(row.ingresos) }}</span>
                <span class="text-rose-400">-{{ formatMoneda(row.egresos) }}</span>
                <span :class="row.balance >= 0 ? 'text-emerald-300' : 'text-rose-300'" class="font-black"> = {{ formatMoneda(row.balance) }} </span>
              </div>
            </div>
            <!-- Barra ingresos -->
            <div class="w-full h-2.5 bg-dark-950 rounded-full overflow-hidden border border-dark-900/50 mb-1">
              <div
                class="h-full bg-gradient-to-r from-emerald-600 to-emerald-400 rounded-full transition-all duration-500"
                :style="{ width: `${row.pctIngresos}%` }"
              ></div>
            </div>
            <!-- Barra egresos -->
            <div class="w-full h-2.5 bg-dark-950 rounded-full overflow-hidden border border-dark-900/50">
              <div
                class="h-full bg-gradient-to-r from-rose-700 to-rose-500 rounded-full transition-all duration-500"
                :style="{ width: `${row.pctEgresos}%` }"
              ></div>
            </div>
          </div>

          <!-- Leyenda -->
          <div class="flex items-center gap-6 pt-2 border-t border-dark-900/40">
            <div class="flex items-center gap-2 text-[11px] text-dark-400">
              <div class="w-3 h-2 rounded-sm bg-gradient-to-r from-emerald-600 to-emerald-400"></div>
              <span>Ingresos</span>
            </div>
            <div class="flex items-center gap-2 text-[11px] text-dark-400">
              <div class="w-3 h-2 rounded-sm bg-gradient-to-r from-rose-700 to-rose-500"></div>
              <span>Egresos</span>
            </div>
          </div>
        </div>

        <div v-else class="text-center py-12 text-dark-500 text-xs">No hay datos registrados aún.</div>
      </div>
    </template>
  </div>
</template>

<script setup>
import { computed, onMounted, ref, watch } from 'vue';
import { paymentsApi } from '@/api/payments.api';
import { egresosApi } from '@/api/egresos.api';
import { gymsApi } from '@/api/gyms.api';
import { useAuthStore } from '@/stores/auth.store';
import { useNotification } from '@/composables/useNotification';
import AppSearchSelect from '@/components/ui/AppSearchSelect.vue';

const { error: showError } = useNotification();
const authStore = useAuthStore();
const loading = ref(false);

const payments = ref([]);
const egresos = ref([]);
const gymId = ref(null);
const gyms = ref([]);

const gymOptions = computed(() => [{ id: null, label: 'Todos los gimnasios' }, ...gyms.value.map((g) => ({ id: g.id, label: g.nombre }))]);

watch(gymId, () => cargarDatos());

function formatMoneda(val) {
  return new Intl.NumberFormat('es-AR', { style: 'currency', currency: 'ARS', minimumFractionDigits: 0 }).format(val || 0);
}

async function cargarDatos() {
  loading.value = true;
  try {
    const params = {};
    if (authStore.hasRole('Superusuario') && gymId.value) params.gymId = gymId.value;

    const [paymentsRes, egresosRes] = await Promise.all([paymentsApi.getAll(params), egresosApi.getAll(params)]);

    payments.value = paymentsRes.data || [];
    egresos.value = egresosRes.data || [];
  } catch (err) {
    showError(err.response?.data?.error || 'No se pudieron cargar los datos');
  } finally {
    loading.value = false;
  }
}

// Agrupa pagos y egresos por mes (últimos 12 meses, más reciente primero)
const historial = computed(() => {
  const map = {};

  payments.value.forEach((p) => {
    const d = new Date(p.fechaPago);
    const key = `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}`;
    if (!map[key]) map[key] = { key, year: d.getFullYear(), month: d.getMonth(), ingresos: 0, egresos: 0 };
    map[key].ingresos += p.monto || 0;
  });

  egresos.value.forEach((e) => {
    const d = new Date(e.fecha);
    const key = `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}`;
    if (!map[key]) map[key] = { key, year: d.getFullYear(), month: d.getMonth(), ingresos: 0, egresos: 0 };
    map[key].egresos += e.monto || 0;
  });

  const MESES = ['Ene', 'Feb', 'Mar', 'Abr', 'May', 'Jun', 'Jul', 'Ago', 'Sep', 'Oct', 'Nov', 'Dic'];

  return Object.values(map)
    .sort((a, b) => b.key.localeCompare(a.key))
    .slice(0, 12)
    .map((row) => ({
      ...row,
      balance: row.ingresos - row.egresos,
      mesLabel: `${MESES[row.month]} ${row.year}`,
    }));
});

// Para el gráfico: porcentajes relativos al máximo de ingresos/egresos
const historialGrafico = computed(() => {
  if (!historial.value.length) return [];
  const maxVal = Math.max(...historial.value.map((r) => Math.max(r.ingresos, r.egresos)), 1);
  return historial.value.map((row) => ({
    ...row,
    pctIngresos: Math.round((row.ingresos / maxVal) * 100),
    pctEgresos: Math.round((row.egresos / maxVal) * 100),
  }));
});

// KPI mes actual
const kpiMesActual = computed(() => {
  const hoy = new Date();
  const mesKey = `${hoy.getFullYear()}-${String(hoy.getMonth() + 1).padStart(2, '0')}`;
  const row = historial.value.find((r) => r.key === mesKey);
  if (!row) return { ingresos: 0, egresos: 0, balance: 0 };
  return row;
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
