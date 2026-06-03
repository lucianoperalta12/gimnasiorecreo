<template>
  <div class="animate-fade-in">
    <div class="flex items-center justify-between mb-6">
      <div>
        <div class="flex flex-wrap items-baseline gap-x-3 gap-y-1">
          <h1 class="page-title">Egresos</h1>
          <div class="flex gap-2 text-sm font-medium text-dark-400">
            <span class="flex items-center gap-1">
              <span class="w-1.5 h-1.5 rounded-full bg-rose-500"></span>
              {{ egresos.length }} registros
            </span>
            <span class="flex items-center gap-1 text-rose-400">
              <span class="w-1.5 h-1.5 rounded-full bg-rose-500"></span>
              {{ formatCurrency(totalAmount) }} </span
            >, filtrados
          </div>
        </div>
        <p class="page-subtitle">Registro de egresos del gimnasio</p>
      </div>
      <div class="flex items-center gap-3">
        <AppButton variant="secondary" @click="router.push('/dashboard')" class="md:px-4 px-2.5 !rounded-xl md:!rounded-lg">
          <svg class="w-5 h-5 md:w-3 md:h-3" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="3">
            <path stroke-linecap="round" stroke-linejoin="round" d="M10.5 19.5L3 12m0 0l7.5-7.5M3 12h18" />
          </svg>
          <span class="hidden md:inline ml-1 text-[10px] font-black uppercase tracking-[0.2em]">Volver</span>
        </AppButton>
        <AppButton variant="primary" @click="openCreateModal" class="md:px-4 px-2.5 !rounded-xl md:!rounded-lg">
          <svg class="w-5 h-5 md:w-4 md:h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2.5">
            <path stroke-linecap="round" stroke-linejoin="round" d="M12 4.5v15m7.5-7.5h-15" />
          </svg>
          <span class="hidden md:inline ml-1">Registrar Egreso</span>
        </AppButton>
      </div>
    </div>

    <div class="mb-4">
      <div class="grid grid-cols-1 gap-3 md:grid-cols-12 md:items-center">
        <div class="md:col-span-3">
          <input v-model="search" type="text" placeholder="Buscar descripción..." class="input w-full" />
        </div>
        <div class="md:col-span-3">
          <AppSearchSelect v-model="selectedCategoria" :options="categoriaFilterOptions" placeholder="Categoría" class="w-full" />
        </div>
        <div class="md:col-span-2">
          <input v-model="dateFrom" type="date" class="input w-full text-sm" />
        </div>
        <div class="md:col-span-2">
          <input v-model="dateTo" type="date" class="input w-full text-sm" />
        </div>
        <div class="md:col-span-2">
          <AppButton variant="secondary" class="w-full !rounded-xl" @click="resetFilters">Limpiar</AppButton>
        </div>
      </div>

      <div v-if="authStore.hasRole('Superusuario')" class="mt-3">
        <AppSearchSelect
          v-model="selectedGymId"
          :options="gymOptions"
          placeholder="Todos los gimnasios"
          class="w-full md:max-w-xs"
          @update:model-value="loadData"
        />
      </div>
    </div>

    <LoadingSpinner v-if="loading" />

    <template v-else>
      <div>
        <!-- Mobile cards -->
        <div v-if="filteredEgresos.length && isMobile" class="space-y-2">
          <div v-for="e in paginatedEgresos" :key="e.id" class="p-3.5 rounded-xl bg-dark-900/30 border border-dark-800/40 flex flex-col gap-2">
            <div class="flex items-start justify-between gap-2">
              <div class="min-w-0">
                <h3 class="font-bold text-sm text-white truncate">{{ e.descripcion }}</h3>
                <div class="flex flex-wrap items-center gap-x-2 gap-y-0.5 mt-1 text-[11px] text-dark-500">
                  <span>{{ formatDateTime(e.fecha) }}</span>
                  <span class="text-dark-700">•</span>
                  <span>{{ e.categoria }}</span>
                </div>
              </div>
              <span class="text-sm font-black text-rose-400 shrink-0">{{ formatCurrency(e.monto) }}</span>
            </div>
            <div class="border-t border-dark-900/50 pt-2 flex items-center justify-end gap-4">
              <button class="text-xs font-bold text-dark-400 hover:text-white transition-colors" @click="openEditModal(e)">Editar</button>
              <button class="text-xs font-bold text-red-500/80 hover:text-red-400 transition-colors" @click="confirmDelete(e)">Eliminar</button>
            </div>
          </div>
        </div>

        <!-- Desktop table -->
        <div v-else-if="filteredEgresos.length && !isMobile" class="table-container">
          <table class="table">
            <thead>
              <tr>
                <th>Descripción</th>
                <th>Categoría</th>
                <th>Monto</th>
                <th>Fecha</th>
                <th class="text-right">Acciones</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="e in paginatedEgresos" :key="e.id">
                <td class="text-white">{{ e.descripcion }}</td>
                <td>{{ e.categoria }}</td>
                <td class="font-black text-rose-400">{{ formatCurrency(e.monto) }}</td>
                <td>{{ formatDateTime(e.fecha) }}</td>
                <td class="text-right">
                  <div class="flex items-center justify-end gap-2">
                    <button class="btn-ghost btn-sm" @click="openEditModal(e)">Editar</button>
                    <button class="btn-ghost btn-sm text-red-400 p-1.5" title="Eliminar Egreso" @click="confirmDelete(e)">
                      <svg class="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                        <path
                          stroke-linecap="round"
                          stroke-linejoin="round"
                          stroke-width="2"
                          d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16"
                        />
                      </svg>
                    </button>
                  </div>
                </td>
              </tr>
            </tbody>
          </table>
        </div>

        <!-- Pagination -->
        <div v-if="filteredEgresos.length > pageSize" class="mt-6 flex items-center justify-between px-2">
          <div class="text-xs text-dark-500 font-medium">
            Mostrando {{ (currentPage - 1) * pageSize + 1 }} - {{ Math.min(currentPage * pageSize, filteredEgresos.length) }} de
            {{ filteredEgresos.length }} registros
          </div>
          <div class="flex items-center gap-2">
            <button class="btn-secondary !p-2 !rounded-lg" :disabled="currentPage === 1" @click="currentPage--">
              <svg class="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M15.75 19.5L8.25 12l7.5-7.5" />
              </svg>
            </button>
            <div class="flex items-center gap-1">
              <button
                v-for="p in totalPages"
                :key="p"
                @click="currentPage = p"
                class="w-8 h-8 rounded-lg text-xs font-black transition-all"
                :class="currentPage === p ? 'bg-primary-600 text-white' : 'bg-dark-800 text-dark-400 hover:bg-dark-700'"
              >
                {{ p }}
              </button>
            </div>
            <button class="btn-secondary !p-2 !rounded-lg" :disabled="currentPage === totalPages" @click="currentPage++">
              <svg class="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M8.25 4.5l7.5 7.5-7.5 7.5" />
              </svg>
            </button>
          </div>
        </div>

        <div v-else-if="!filteredEgresos.length" class="card text-center py-12">
          <p class="text-dark-400">No hay egresos registrados</p>
        </div>
      </div>
    </template>

    <!-- Create/Edit Modal -->
    <AppModal v-model="showModal" :title="editingEgreso ? 'Editar egreso' : 'Registrar egreso'">
      <form class="space-y-4" @submit.prevent="handleSubmit">
        <AppInput v-model="form.descripcion" label="Descripción" type="text" placeholder="Descripción del egreso" />

        <div class="grid grid-cols-2 gap-4">
          <div>
            <label class="label">Categoría</label>
            <AppSearchSelect v-model="form.categoria" :options="categoriaOptions" placeholder="Categoría" />
          </div>
          <AppInput v-model="form.monto" label="Monto" type="text" @input="onMontoInput" />
        </div>

        <AppInput v-model="form.fecha" label="Fecha" type="datetime-local" />

        <textarea v-model="form.observaciones" rows="2" class="input" placeholder="Observaciones (opcional)" />
      </form>
      <template #footer>
        <AppButton variant="secondary" @click="showModal = false">Cancelar</AppButton>
        <AppButton :loading="saving" @click="handleSubmit">Guardar</AppButton>
      </template>
    </AppModal>

    <!-- Delete Modal -->
    <AppModal v-model="showDeleteModal" title="Eliminar egreso" size="sm">
      <p class="text-dark-300">¿Eliminar este egreso?</p>
      <template #footer>
        <AppButton variant="secondary" @click="showDeleteModal = false">Cancelar</AppButton>
        <AppButton variant="danger" :loading="saving" @click="handleDelete">Eliminar</AppButton>
      </template>
    </AppModal>
  </div>
</template>

<script setup>
import { ref, reactive, computed, watch, onMounted, nextTick } from 'vue';
import { useIsMobile } from '@/composables/useIsMobile';
import { useRouter } from 'vue-router';
import { useAuthStore } from '@/stores/auth.store';
import { useNotification } from '@/composables/useNotification';
import { gymsApi } from '@/api/gyms.api';
import { egresosApi } from '@/api/egresos.api';
import { formatCurrency, formatDate, formatDateTime } from '@/constants/membershipStatus';
import AppButton from '@/components/ui/AppButton.vue';
import AppInput from '@/components/ui/AppInput.vue';
import AppModal from '@/components/ui/AppModal.vue';
import AppSearchSelect from '@/components/ui/AppSearchSelect.vue';
import LoadingSpinner from '@/components/shared/LoadingSpinner.vue';
import { formatLocalDate, localDateTimeString } from '@/utils/date';

const router = useRouter();
const authStore = useAuthStore();
const { success, error: showError } = useNotification();
const { isMobile } = useIsMobile();

const CATEGORIAS = ['Sueldos', 'Alquiler', 'Servicios', 'Impuestos', 'Mantenimiento', 'Equipamiento', 'Otros'];

const categoriaOptions = CATEGORIAS.map((c) => ({ id: c, label: c }));
const categoriaFilterOptions = [{ id: 'Todas', label: 'Todas' }, ...categoriaOptions];

const egresos = ref([]);
const loading = ref(false);
const selectedGymId = ref(null);
const gyms = ref([]);
const showModal = ref(false);
const showDeleteModal = ref(false);
const saving = ref(false);
const editingEgreso = ref(null);
const deletingEgreso = ref(null);

const defaultDateFrom = () => {
  const date = new Date();
  date.setMonth(date.getMonth() - 1);
  return formatLocalDate(date);
};
const defaultDateTo = () => formatLocalDate(new Date());

const search = ref('');
const dateFrom = ref(defaultDateFrom());
const dateTo = ref(defaultDateTo());
const selectedCategoria = ref('Todas');

const pageSize = ref(15);
const currentPage = ref(1);

const form = reactive({
  descripcion: '',
  categoria: 'Sueldos',
  monto: '',
  fecha: localDateTimeString(),
  observaciones: '',
});

const filteredEgresos = computed(() => {
  const q = search.value.toLowerCase();
  const cat = selectedCategoria.value;

  let from = 0;
  if (dateFrom.value) {
    const [y, m, d] = dateFrom.value.split('-');
    from = new Date(y, m - 1, d, 0, 0, 0).getTime();
  }

  let to = Infinity;
  if (dateTo.value) {
    const [y, m, d] = dateTo.value.split('-');
    to = new Date(y, m - 1, d, 23, 59, 59, 999).getTime();
  }

  return egresos.value.filter((e) => {
    const t = new Date(e.fecha).getTime();
    const matchesCat = cat === 'Todas' || e.categoria === cat;
    return t >= from && t <= to && matchesCat && e.descripcion.toLowerCase().includes(q);
  });
});

const totalAmount = computed(() => filteredEgresos.value.reduce((sum, e) => sum + (e.monto || 0), 0));
const totalPages = computed(() => Math.ceil(filteredEgresos.value.length / pageSize.value) || 1);
const paginatedEgresos = computed(() => {
  const start = (currentPage.value - 1) * pageSize.value;
  return filteredEgresos.value.slice(start, start + pageSize.value);
});

watch([search, dateFrom, dateTo, selectedGymId, selectedCategoria], () => {
  currentPage.value = 1;
});

const gymOptions = computed(() => {
  const list = [{ id: null, label: 'Todos los gimnasios' }];
  gyms.value.forEach((g) => list.push({ id: g.id, label: g.nombre }));
  return list;
});

function openCreateModal() {
  editingEgreso.value = null;
  Object.assign(form, {
    descripcion: '',
    categoria: 'Sueldos',
    monto: '',
    fecha: localDateTimeString(),
    observaciones: '',
  });
  showModal.value = true;
}

function openEditModal(e) {
  editingEgreso.value = e;
  Object.assign(form, {
    descripcion: e.descripcion,
    categoria: e.categoria,
    monto: formatNumberWithDots(e.monto),
    fecha: e.fecha ? localDateTimeString(new Date(e.fecha)) : '',
    observaciones: e.observaciones || '',
  });
  showModal.value = true;
}

function confirmDelete(e) {
  deletingEgreso.value = e;
  showDeleteModal.value = true;
}

async function handleSubmit() {
  saving.value = true;
  try {
    // Convertir fecha-hora local a UTC antes de enviar
    const fechaLocal = new Date(form.fecha);
    const fechaUTC = new Date(fechaLocal.getTime() - fechaLocal.getTimezoneOffset() * 60000);

    const payload = {
      ...form,
      monto: parseFormattedNumber(form.monto),
      fecha: fechaUTC.toISOString(),
    };
    if (editingEgreso.value) {
      await egresosApi.update(editingEgreso.value.id, payload);
      success('Egreso actualizado');
    } else {
      await egresosApi.create(payload);
      success('Egreso registrado');
    }
    showModal.value = false;
    await loadData();
  } catch (err) {
    showError(err.response?.data?.error || 'Error al guardar');
  } finally {
    saving.value = false;
  }
}

async function handleDelete() {
  saving.value = true;
  try {
    await egresosApi.delete(deletingEgreso.value.id);
    success('Egreso eliminado');
    showDeleteModal.value = false;
    await loadData();
  } catch (err) {
    showError(err.response?.data?.error || 'Error al eliminar');
  } finally {
    saving.value = false;
  }
}

async function loadData() {
  loading.value = true;
  try {
    const params = selectedGymId.value ? { gymId: selectedGymId.value } : {};
    const { data } = await egresosApi.getAll(params);
    egresos.value = data || [];
  } catch (err) {
    showError('No se pudieron cargar los egresos');
  } finally {
    loading.value = false;
  }
}

onMounted(async () => {
  if (authStore.hasRole('Superusuario')) {
    try {
      const { data } = await gymsApi.getAll();
      gyms.value = data;
    } catch {
      /* noop */
    }
  }
  await loadData();
});

function formatNumberWithDots(val) {
  if (val === null || val === undefined || val === '') return '';
  let str = String(val).replace(/\./g, '');
  str = str.replace(/[^0-9,]/g, '');
  const parts = str.split(',');
  parts[0] = parts[0].replace(/\B(?=(\d{3})+(?!\d))/g, '.');
  if (parts.length > 2) return parts[0] + ',' + parts.slice(1).join('');
  return parts.join(',');
}

function parseFormattedNumber(val) {
  if (val === null || val === undefined || val === '') return 0;
  const clean = String(val).replace(/\./g, '').replace(/,/g, '.');
  const num = Number(clean);
  return isNaN(num) ? 0 : num;
}

function onMontoInput(event) {
  const input = event.target;
  const value = input.value;
  const formatted = formatNumberWithDots(value);
  const selectionStart = input.selectionStart;
  const oldLength = value.length;
  form.monto = formatted;
  input.value = formatted;
  nextTick(() => {
    const diff = formatted.length - oldLength;
    const newPos = selectionStart + diff;
    input.setSelectionRange(newPos, newPos);
  });
}

function resetFilters() {
  search.value = '';
  selectedCategoria.value = 'Todas';
  dateFrom.value = defaultDateFrom();
  dateTo.value = defaultDateTo();
}
</script>
