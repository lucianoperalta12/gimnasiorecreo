<template>
  <div class="animate-fade-in">
    <div class="flex items-center justify-between mb-6">
      <div>
        <div class="flex flex-wrap items-baseline gap-x-3 gap-y-1">
          <h1 class="page-title">Pagos</h1>
          <div class="flex gap-2 text-sm font-medium text-dark-400">
            <span class="flex items-center gap-1">
              <span class="w-1.5 h-1.5 rounded-full bg-emerald-500"></span>
              {{ store.payments.length }} registros
            </span>
            <span class="flex items-center gap-1 text-emerald-400">
              <span class="w-1.5 h-1.5 rounded-full bg-emerald-500"></span>
              {{ formatCurrency(totalAmount) }}
            </span>
            filtrado
          </div>
        </div>
        <p class="page-subtitle">Registro de pagos de membresías</p>
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
          <span class="hidden md:inline ml-1">Registrar Pago</span>
        </AppButton>
      </div>
    </div>

    <div class="mb-4">
      <div class="grid grid-cols-1 gap-3 md:grid-cols-12 md:items-center">
        <div class="md:col-span-3">
          <input v-model="search" type="text" placeholder="Buscar alumno..." class="input w-full" />
        </div>
        <div class="md:col-span-3">
          <AppSearchSelect v-model="selectedPaymentMethod" :options="paymentMethodFilterOptions" placeholder="Método" class="w-full" />
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

    <LoadingSpinner v-if="store.loading" />

    <template v-else>
      <div>
        <div v-if="filteredPayments.length && isMobile" class="space-y-2">
          <div v-for="p in paginatedPayments" :key="p.id" class="p-3.5 rounded-xl bg-dark-900/30 border border-dark-800/40 flex flex-col gap-2">
            <div class="flex items-start justify-between gap-2">
              <div class="min-w-0">
                <h3 class="font-bold text-sm text-white truncate">{{ p.alumnoNombreCompleto }}</h3>
                <div class="flex flex-wrap items-center gap-x-2 gap-y-0.5 mt-1 text-[11px] text-dark-500">
                  <span>DNI: {{ p.alumnoDni || '—' }}</span>
                  <span class="text-dark-700">•</span>
                  <span>{{ formatDate(p.fechaPago) }}</span>
                  <span class="text-dark-700">•</span>
                  <span>{{ p.metodoPago || '—' }}</span>
                </div>
              </div>
              <div class="flex flex-col items-end gap-1 shrink-0">
                <span class="text-sm font-black text-emerald-400">{{ formatCurrency(p.monto) }}</span>
                <span :class="membershipEstadoBadgeClass(p.estado)" class="text-[9px] px-1.5 py-0.5 font-black uppercase tracking-wider rounded">{{
                  p.estado
                }}</span>
              </div>
            </div>
            <div class="border-t border-dark-900/50 pt-2 flex items-center justify-end gap-4">
              <button class="text-xs font-bold text-dark-400 hover:text-white transition-colors" @click="openEditModal(p)">Editar</button>
              <button class="text-xs font-bold text-red-500/80 hover:text-red-400 transition-colors" @click="confirmDelete(p)">Eliminar</button>
            </div>
          </div>
        </div>

        <div v-else-if="filteredPayments.length && !isMobile" class="table-container">
          <table class="table">
            <thead>
              <tr>
                <th>Alumno</th>
                <th>Monto</th>
                <th @click="toggleSort('fechaPago')" class="cursor-pointer hover:text-white select-none transition-colors">
                  Fecha
                  <span v-if="sortBy === 'fechaPago'">{{ sortDesc ? '↓' : '↑' }}</span>
                </th>
                <th>Estado</th>
                <th>Método</th>
                <th>DNI</th>
                <th class="text-right">Acciones</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="p in paginatedPayments" :key="p.id">
                <td class="text-white">{{ p.alumnoNombreCompleto }}</td>
                <td>
                  {{ formatCurrency(p.monto) }}
                </td>
                <td>{{ formatDateTime(p.fechaPago) }}</td>
                <td>
                  <span :class="membershipEstadoBadgeClass(p.estado)">{{ p.estado }}</span>
                </td>

                <td>{{ p.metodoPago || '—' }}</td>
                <td>{{ p.alumnoDni || '—' }}</td>
                <td class="text-right">
                  <div class="flex items-center justify-end gap-2">
                    <button class="btn-ghost btn-sm" @click="openEditModal(p)">Editar</button>
                    <button class="btn-ghost btn-sm text-red-400 p-1.5" title="Eliminar Pago" @click="confirmDelete(p)">
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

        <div v-if="filteredPayments.length > pageSize" class="mt-6 flex items-center justify-between px-2">
          <div class="text-xs text-dark-500 font-medium">
            Mostrando {{ (currentPage - 1) * pageSize + 1 }} - {{ Math.min(currentPage * pageSize, filteredPayments.length) }} de
            {{ filteredPayments.length }} registros
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

        <div v-else-if="!filteredPayments.length" class="card text-center py-12">
          <p class="text-dark-400">No hay pagos registrados</p>
        </div>
      </div>
    </template>

    <AppModal v-model="showModal" :title="editingPayment ? 'Editar pago' : 'Registrar pago'">
      <form class="space-y-4" @submit.prevent="handleSubmit">
        <AppSearchSelect v-model="form.membresiaId" label="Alumno (Membresía Activa)" placeholder="Buscar alumno..." :options="studentOptions" />

        <div class="grid grid-cols-2 gap-4">
          <AppInput v-model="form.monto" label="Precio" type="text" @input="onMontoInput" />
          <AppInput v-model="form.fechaPago" label="Fecha de pago" type="datetime-local" />
        </div>

        <div class="grid grid-cols-2 gap-4">
          <div>
            <label class="label">Método de pago</label>
            <AppSearchSelect v-model="form.metodoPago" :options="metodoPagoOptions" placeholder="Método de pago" />
          </div>
          <div>
            <label class="label">Estado</label>
            <AppSearchSelect v-model="form.estado" :options="paymentEstadoOptions" placeholder="Estado" />
          </div>
        </div>

        <textarea v-model="form.notas" rows="2" class="input" placeholder="Notas" />
      </form>
      <template #footer>
        <AppButton variant="secondary" @click="showModal = false">Cancelar</AppButton>
        <AppButton :loading="saving" @click="handleSubmit">Guardar</AppButton>
      </template>
    </AppModal>

    <AppModal v-model="showDeleteModal" title="Eliminar pago" size="sm">
      <p class="text-dark-300">¿Eliminar este pago?</p>
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
import { useMembershipStore } from '@/stores/membership.store';
import { useAuthStore } from '@/stores/auth.store';
import { useNotification } from '@/composables/useNotification';
import { gymsApi } from '@/api/gyms.api';
import { PAYMENT_ESTADOS, formatCurrency, formatDateTime, formatDate, membershipEstadoBadgeClass } from '@/constants/membershipStatus';
import AppButton from '@/components/ui/AppButton.vue';
import AppInput from '@/components/ui/AppInput.vue';
import AppModal from '@/components/ui/AppModal.vue';
import AppSearchSelect from '@/components/ui/AppSearchSelect.vue';
import LoadingSpinner from '@/components/shared/LoadingSpinner.vue';
import { formatLocalDate, localDateTimeString } from '@/utils/date';

const router = useRouter();
const store = useMembershipStore();
const authStore = useAuthStore();
const { success, error: showError } = useNotification();
const { isMobile } = useIsMobile();

const selectedGymId = ref(null);
const gyms = ref([]);
const showModal = ref(false);
const showDeleteModal = ref(false);
const saving = ref(false);
const editingPayment = ref(null);
const deletingPayment = ref(null);

const defaultDateFrom = () => {
  const date = new Date();
  date.setMonth(date.getMonth() - 1);
  return formatLocalDate(date);
};
const defaultDateTo = () => formatLocalDate(new Date());

const search = ref('');
const dateFrom = ref(defaultDateFrom());
const dateTo = ref(defaultDateTo());
const selectedPaymentMethod = ref('Todos');

const pageSize = ref(15);
const currentPage = ref(1);

const sortBy = ref('fechaPago');
const sortDesc = ref(true);

function toggleSort(field) {
  if (sortBy.value === field) {
    sortDesc.value = !sortDesc.value;
  } else {
    sortBy.value = field;
    sortDesc.value = true;
  }
}

function resetFilters() {
  search.value = '';
  dateFrom.value = defaultDateFrom();
  dateTo.value = defaultDateTo();
  selectedPaymentMethod.value = 'Todos';
}

const form = reactive({
  membresiaId: null,
  monto: '',
  fechaPago: localDateTimeString(),
  metodoPago: 'Efectivo',
  estado: 'Completado',
  notas: '',
});

const filteredPayments = computed(() => {
  const q = search.value.toLowerCase();
  const paymentMethod = selectedPaymentMethod.value;

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

  const items = store.payments.filter((p) => {
    const d = new Date(p.fechaPago).getTime();
    const matchesMethod = paymentMethod === 'Todos' || (p.metodoPago || '') === paymentMethod;
    return d >= from && d <= to && matchesMethod && (p.alumnoNombreCompleto.toLowerCase().includes(q) || (p.alumnoDni || '').toLowerCase().includes(q));
  });

  if (sortBy.value === 'fechaPago') {
    items.sort((a, b) => {
      const dateA = new Date(a.fechaPago).getTime();
      const dateB = new Date(b.fechaPago).getTime();
      return sortDesc.value ? dateB - dateA : dateA - dateB;
    });
  }

  return items;
});

const totalAmount = computed(() => {
  return filteredPayments.value.reduce((sum, p) => sum + (p.monto || 0), 0);
});

const totalPages = computed(() => Math.ceil(filteredPayments.value.length / pageSize.value) || 1);

const paginatedPayments = computed(() => {
  const start = (currentPage.value - 1) * pageSize.value;
  return filteredPayments.value.slice(start, start + pageSize.value);
});

watch([search, dateFrom, dateTo, selectedGymId, selectedPaymentMethod, sortBy, sortDesc], () => {
  currentPage.value = 1;
});

const gymOptions = computed(() => {
  const list = [{ id: null, label: 'Todos los gimnasios' }];
  if (gyms.value) {
    gyms.value.forEach((g) => {
      list.push({ id: g.id, label: g.nombre });
    });
  }
  return list;
});

const metodoPagoOptions = computed(() => {
  const knownMethods = new Set(['Efectivo', 'Transferencia']);

  store.payments.forEach((payment) => {
    if (payment?.metodoPago) knownMethods.add(payment.metodoPago);
  });

  if (editingPayment.value?.metodoPago) {
    knownMethods.add(editingPayment.value.metodoPago);
  }

  return Array.from(knownMethods).map((method) => ({
    id: method,
    label: method,
  }));
});

const paymentMethodFilterOptions = computed(() => [{ id: 'Todos', label: 'Todos' }, ...metodoPagoOptions.value]);

const paymentEstadoOptions = computed(() => {
  return PAYMENT_ESTADOS.map((e) => ({ id: e, label: e }));
});

const studentOptions = computed(() => {
  return store.memberships
    .filter((m) => m.estado === 'Activa')
    .map((m) => ({
      id: m.id,
      label: m.alumnoNombreCompleto,
      subLabel: m.planNombre,
    }));
});

watch(
  () => form.membresiaId,
  (newId) => {
    if (newId && !editingPayment.value) {
      const membership = store.memberships.find((m) => m.id === newId);
      if (membership) {
        const plan = store.plans.find((p) => p.nombre === membership.planNombre);
        if (plan) {
          form.monto = formatNumberWithDots(plan.precio);
        }
      }
    }
  }
);

function openCreateModal() {
  editingPayment.value = null;
  Object.assign(form, {
    membresiaId: null,
    monto: '',
    fechaPago: localDateTimeString(),
    metodoPago: 'Efectivo',
    estado: 'Completado',
    notas: '',
  });
  showModal.value = true;
}

function openEditModal(p) {
  editingPayment.value = p;
  Object.assign(form, {
    membresiaId: p.membresiaId,
    monto: formatNumberWithDots(p.monto),
    fechaPago: p.fechaPago ? localDateTimeString(new Date(p.fechaPago)) : '',
    metodoPago: p.metodoPago || 'Efectivo',
    estado: p.estado,
    notas: p.notas || '',
  });
  showModal.value = true;
}

function confirmDelete(p) {
  deletingPayment.value = p;
  showDeleteModal.value = true;
}

async function handleSubmit() {
  saving.value = true;
  try {
    const payload = {
      ...form,
      fechaPago: form.fechaPago + ':00',
      monto: parseFormattedNumber(form.monto),
      membresiaId: Number(form.membresiaId),
    };
    if (editingPayment.value) {
      await store.updatePayment(editingPayment.value.id, payload);
      success('Pago actualizado');
    } else {
      await store.createPayment(payload);
      success('Pago registrado');
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
    await store.deletePayment(deletingPayment.value.id);
    success('Pago eliminado');
    showDeleteModal.value = false;
  } catch (err) {
    showError(err.response?.data?.error || 'Error al eliminar');
  } finally {
    saving.value = false;
  }
}

async function loadData() {
  const params = selectedGymId.value ? { gymId: selectedGymId.value } : {};
  await Promise.all([store.fetchPayments(params), store.fetchMemberships({ estado: 'Activa', ...params })]);
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
  await store.fetchPlans();
  await loadData();
});

function formatNumberWithDots(val) {
  if (val === null || val === undefined || val === '') return '';
  let str = String(val).replace(/\./g, '');
  str = str.replace(/[^0-9,]/g, '');
  const parts = str.split(',');
  parts[0] = parts[0].replace(/\B(?=(\d{3})+(?!\d))/g, '.');
  if (parts.length > 2) {
    return parts[0] + ',' + parts.slice(1).join('');
  }
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
    const newLength = formatted.length;
    const diff = newLength - oldLength;
    const newCursorPos = selectionStart + diff;
    input.setSelectionRange(newCursorPos, newCursorPos);
  });
}
</script>
