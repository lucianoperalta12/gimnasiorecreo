<template>
  <div class="animate-fade-in">
    <!-- Header -->
    <div class="flex items-center justify-between mb-6">
      <div>
        <div class="flex flex-wrap items-baseline gap-x-3 gap-y-1">
          <h1 class="page-title">Usuarios</h1>
          <div class="flex gap-2 text-xs font-medium text-dark-400">
            <span class="flex items-center gap-1">
              <span class="w-1.5 h-1.5 rounded-full bg-emerald-500"></span>
              {{ activeCountText }}
            </span>
            <span class="text-dark-600">•</span>
            <span class="flex items-center gap-1">
              <span class="w-1.5 h-1.5 rounded-full bg-dark-500"></span>
              {{ inactiveCountText }}
            </span>
          </div>
        </div>
        <p class="page-subtitle hidden sm:block">Administración de usuarios y roles</p>
      </div>

      <div class="flex items-center gap-3">
        <AppButton variant="secondary" @click="router.push('/dashboard')" class="md:px-4 px-2.5 !rounded-xl md:!rounded-lg">
          <svg class="w-5 h-5 md:w-3 md:h-3" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="3">
            <path stroke-linecap="round" stroke-linejoin="round" d="M10.5 19.5L3 12m0 0l7.5-7.5M3 12h18" />
          </svg>
          <span class="hidden md:inline ml-1 text-[10px] font-black uppercase tracking-[0.2em]">Volver</span>
        </AppButton>

        <AppButton @click="openCreateModal" class="md:px-4 px-2.5 !rounded-xl md:!rounded-lg">
          <svg class="w-5 h-5 md:w-4 md:h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2.5">
            <path stroke-linecap="round" stroke-linejoin="round" d="M12 4v16m8-8H4" />
          </svg>
          <span class="hidden md:inline ml-1">Crear Usuario</span>
        </AppButton>
      </div>
    </div>

    <div class="flex flex-col sm:flex-row gap-3 mb-4">
      <input v-model="search" type="text" placeholder="Buscar por nombre, email o DNI..." class="input flex-1 sm:max-w-sm" />

      <div class="flex flex-col sm:flex-row gap-3">
        <AppSearchSelect v-model="roleFilter" :options="roleOptions" placeholder="Todos los roles" class="w-full sm:w-auto sm:min-w-[160px]" />

        <AppSearchSelect
          v-if="authStore.hasRole('Superusuario')"
          v-model="gymFilter"
          :options="gymOptions"
          placeholder="Todos los gimnasios"
          class="w-full sm:w-auto sm:min-w-[200px]"
        />
      </div>
    </div>

    <LoadingSpinner v-if="userStore.loading" />

    <!-- Mobile: Card Layout -->
    <div v-else-if="filteredUsers.length === 0" class="card text-center py-8">
      <p class="text-dark-400">No hay usuarios para mostrar.</p>
    </div>

    <template v-else>
      <!-- Mobile Cards (visible < md) -->
      <div class="md:hidden space-y-3">
        <div
          v-for="u in paginatedUsers"
          :key="userStore.rowKey(u)"
          class="p-4 rounded-2xl bg-dark-900/40 border border-dark-800/50"
          :class="{ 'opacity-50': !u.activo }"
        >
          <div class="flex items-center justify-between mb-3">
            <div>
              <p class="font-semibold text-white text-sm">{{ fullName(u) }}</p>
              <p class="text-[11px] text-dark-500 mt-0.5">{{ u.email }}</p>
            </div>
            <select
              v-if="canChangeRole(u)"
              :value="u.rol"
              class="bg-dark-800 border border-dark-700 text-[10px] rounded-full px-2 py-0.5 text-dark-200 focus:outline-none focus:ring-1 focus:ring-primary-500/50"
              @change="handleRoleChange(u, $event.target.value)"
            >
              <option v-for="role in getAvailableRoles(u)" :key="role" :value="role">{{ role }}</option>
            </select>
            <span v-else :class="roleBadge(u.rol)" class="text-[10px]">{{ u.rol }}</span>
          </div>
          <div class="flex items-center justify-between text-xs text-dark-400">
            <div class="flex flex-col gap-1">
              <span>DNI: {{ u.dni || '-' }}</span>
              <span v-if="authStore.hasRole('Superusuario')" class="text-primary-400 font-bold uppercase tracking-wider text-[9px]">{{
                u.gymNombre || '-'
              }}</span>
            </div>
            <span :class="u.activo ? 'text-emerald-500' : 'text-red-400'">{{ u.activo ? 'Activo' : 'Inactivo' }}</span>
          </div>
          <div class="flex items-center justify-end gap-2 mt-3 pt-3 border-t border-dark-800/50">
            <button
              v-if="!isSystemAdmin(u)"
              class="relative inline-flex h-5 w-9 shrink-0 items-center rounded-full transition-colors"
              :class="u.activo ? 'bg-emerald-600' : 'bg-dark-700'"
              @click="handleToggleStatus(u)"
              title="Activar o desactivar"
            >
              <span class="inline-block h-3 w-3 rounded-full bg-white transition-transform" :class="u.activo ? 'translate-x-5' : 'translate-x-1'" />
            </button>
            <button
              v-if="authStore.hasRole('Superusuario', 'Administrativo') && !isSystemAdmin(u)"
              class="text-amber-500 hover:text-amber-400 transition-colors p-1"
              title="Editar Usuario"
              @click="openEditModal(u)"
            >
              <svg class="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 12a3 3 0 11-6 0 3 3 0 016 0z" />
                <path
                  stroke-linecap="round"
                  stroke-linejoin="round"
                  stroke-width="2"
                  d="M2.458 12C3.732 7.943 7.523 5 12 5c4.478 0 8.268 2.943 9.542 7-1.274 4.057-5.064 7-9.542 7-4.477 0-8.268-2.943-9.542-7z"
                />
              </svg>
            </button>
            <button
              v-if="authStore.hasRole('Superusuario', 'Administrativo') && !isSystemAdmin(u)"
              class="text-primary-500 hover:text-primary-400 transition-colors p-1"
              @click="openPasswordModal(u)"
            >
              <svg class="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path
                  stroke-linecap="round"
                  stroke-linejoin="round"
                  stroke-width="2"
                  d="M15 7a2 2 0 012 2m4 0a6 6 0 01-7.743 5.743L11 17H9v2H7v2H4a1 1 0 01-1-1v-2.586a1 1 0 01.293-.707l5.964-5.964A6 6 0 1121 9z"
                />
              </svg>
            </button>
            <button
              v-if="authStore.hasRole('Superusuario', 'Administrativo') && !isSystemAdmin(u)"
              class="text-red-500 hover:text-red-400 transition-colors p-1"
              @click="handleDeleteClick(u)"
            >
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
        </div>
      </div>

      <!-- Desktop Table (visible md+) -->
      <div class="hidden md:block overflow-x-auto rounded-xl border border-dark-700/50">
        <table class="w-full text-sm text-left">
          <thead class="bg-dark-800/80 border-b border-dark-700">
            <tr>
              <th class="px-3 py-3 text-xs font-semibold text-dark-400 uppercase tracking-wider">Nombre</th>
              <th class="px-3 py-3 text-xs font-semibold text-dark-400 uppercase tracking-wider hidden lg:table-cell">Email</th>
              <th class="px-3 py-3 text-xs font-semibold text-dark-400 uppercase tracking-wider hidden xl:table-cell">DNI</th>
              <th class="px-3 py-3 text-xs font-semibold text-dark-400 uppercase tracking-wider">Rol</th>
              <th v-if="authStore.hasRole('Superusuario')" class="px-3 py-3 text-xs font-semibold text-dark-400 uppercase tracking-wider hidden md:table-cell">
                Gimnasio
              </th>
              <th class="px-3 py-3 text-xs font-semibold text-dark-400 uppercase tracking-wider">Estado</th>
              <th class="px-3 py-3 text-xs font-semibold text-dark-400 uppercase tracking-wider text-right">Acciones</th>
            </tr>
          </thead>
          <tbody>
            <tr
              v-for="u in paginatedUsers"
              :key="userStore.rowKey(u)"
              class="border-b border-dark-800 hover:bg-dark-800/40 transition-colors"
              :class="{ 'opacity-50': !u.activo }"
            >
              <td class="px-3 py-3 font-medium text-white">
                <div class="max-w-[160px] truncate">{{ fullName(u) }}</div>
                <div class="lg:hidden text-[10px] text-dark-500 mt-0.5 max-w-[160px] truncate">{{ u.email }}</div>
              </td>
              <td class="px-3 py-3 text-dark-400 hidden lg:table-cell">
                <div class="max-w-[200px] truncate">{{ u.email }}</div>
              </td>
              <td class="px-3 py-3 text-dark-400 hidden xl:table-cell">{{ u.dni }}</td>
              <td class="px-3 py-3">
                <select
                  v-if="canChangeRole(u)"
                  :value="u.rol"
                  class="bg-dark-800 border border-dark-700 text-xs rounded-full px-2.5 py-0.5 text-dark-200 focus:outline-none focus:ring-1 focus:ring-primary-500/50 cursor-pointer"
                  @change="handleRoleChange(u, $event.target.value)"
                >
                  <option v-for="role in getAvailableRoles(u)" :key="role" :value="role">{{ role }}</option>
                </select>
                <span v-else :class="roleBadge(u.rol)">{{ u.rol }}</span>
              </td>
              <td v-if="authStore.hasRole('Superusuario')" class="px-3 py-3 text-dark-400 hidden md:table-cell">{{ u.gymNombre || '-' }}</td>
              <td class="px-3 py-3">
                <span :class="u.activo ? 'badge-success' : 'badge-danger'">{{ u.activo ? 'Activo' : 'Inactivo' }}</span>
              </td>
              <td class="px-3 py-3">
                <div class="flex items-center justify-end gap-1.5 flex-nowrap">
                  <button
                    v-if="!isSystemAdmin(u)"
                    class="relative inline-flex h-5 w-9 shrink-0 items-center rounded-full transition-colors"
                    :class="u.activo ? 'bg-emerald-600' : 'bg-dark-700'"
                    @click="handleToggleStatus(u)"
                    title="Activar o desactivar"
                  >
                    <span class="inline-block h-3 w-3 rounded-full bg-white transition-transform" :class="u.activo ? 'translate-x-5' : 'translate-x-1'" />
                  </button>
                  <button
                    v-if="authStore.hasRole('Superusuario', 'Administrativo') && !isSystemAdmin(u)"
                    class="text-primary-500 hover:text-primary-400 transition-colors p-1"
                    title="Editar Usuario"
                    @click="openEditModal(u)"
                  >
                    <svg class="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                      <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 12a3 3 0 11-6 0 3 3 0 016 0z" />
                      <path
                        stroke-linecap="round"
                        stroke-linejoin="round"
                        stroke-width="2"
                        d="M2.458 12C3.732 7.943 7.523 5 12 5c4.478 0 8.268 2.943 9.542 7-1.274 4.057-5.064 7-9.542 7-4.477 0-8.268-2.943-9.542-7z"
                      />
                    </svg>
                  </button>
                  <button
                    v-if="authStore.hasRole('Superusuario', 'Administrativo') && !isSystemAdmin(u)"
                    class="text-primary-500 hover:text-primary-400 transition-colors p-1 shrink-0"
                    title="Cambiar contraseña"
                    @click="openPasswordModal(u)"
                  >
                    <svg class="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                      <path
                        stroke-linecap="round"
                        stroke-linejoin="round"
                        stroke-width="2"
                        d="M15 7a2 2 0 012 2m4 0a6 6 0 01-7.743 5.743L11 17H9v2H7v2H4a1 1 0 01-1-1v-2.586a1 1 0 01.293-.707l5.964-5.964A6 6 0 1121 9z"
                      />
                    </svg>
                  </button>
                  <button
                    v-if="authStore.hasRole('Superusuario', 'Administrativo') && !isSystemAdmin(u)"
                    class="text-red-500 hover:text-red-400 transition-colors p-1 shrink-0"
                    title="Eliminar usuario"
                    @click="handleDeleteClick(u)"
                  >
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

      <!-- Pagination Controls -->
      <div v-if="totalCount > pageSize" class="mt-6 flex items-center justify-between px-2">
        <div class="text-xs text-dark-500 font-medium">
          Mostrando {{ pageStart }} - {{ pageEnd }} de {{ totalCount }} registros
        </div>
        <div class="flex items-center gap-2">
          <button class="btn-secondary !p-2 !rounded-lg" :disabled="currentPage === 1" @click="goToPage(currentPage - 1)">
            <svg class="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M15.75 19.5L8.25 12l7.5-7.5" />
            </svg>
          </button>

          <div class="flex items-center gap-1">
            <button
              v-for="p in totalPages"
              :key="p"
              @click="goToPage(p)"
              class="w-8 h-8 rounded-lg text-xs font-black transition-all"
              :class="currentPage === p ? 'bg-primary-600 text-white' : 'bg-dark-800 text-dark-400 hover:bg-dark-700'"
            >
              {{ p }}
            </button>
          </div>

          <button class="btn-secondary !p-2 !rounded-lg" :disabled="currentPage === totalPages" @click="goToPage(currentPage + 1)">
            <svg class="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M8.25 4.5l7.5 7.5-7.5 7.5" />
            </svg>
          </button>
        </div>
      </div>
    </template>

    <!-- Create or Edit User Modal -->
    <AppModal v-model="showCreateModal" :title="userToEdit ? 'Editar usuario' : 'Crear Usuario'" size="lg">
      <form class="grid grid-cols-1 sm:grid-cols-2 gap-4" @submit.prevent="submitForm">
        <AppInput v-model="createForm.nombre" label="Nombre" required />
        <AppInput v-model="createForm.apellido" label="Apellido" required />
        <AppInput
          v-model="createForm.email"
          label="Correo electrónico"
          type="email"
          :required="!userToEdit"
          :disabled="!!userToEdit"
        />
        <AppInput v-model="createForm.dni" label="DNI" :required="!userToEdit" :disabled="!!userToEdit" />
        <AppInput v-model="createForm.fechaNacimiento" label="Fecha de Nacimiento" type="date" required />
        <AppInput v-model="createForm.domicilio" label="Domicilio" />
        <AppInput v-model="createForm.telefono" label="Teléfono" />
        <div class="min-w-0">
          <AppSearchSelect
            v-model="createForm.rol"
            label="Rol"
            :required="!userToEdit"
            :options="createRoleOptions"
            placeholder="Seleccionar rol"
            :disabled="!!userToEdit"
          />
        </div>
        <div v-if="authStore.hasRole('Superusuario')" class="min-w-0">
          <AppSearchSelect
            v-model.number="createForm.gymId"
            label="Gimnasio"
            :required="!userToEdit"
            :options="createGymOptions"
            placeholder="Seleccionar gimnasio"
            :disabled="!!userToEdit"
          />
        </div>
        <div class="sm:col-span-2 min-w-0">
          <label class="label">Observaciones</label>
          <textarea v-model="createForm.observaciones" class="input w-full min-h-[80px] resize-y" placeholder="Observaciones adicionales..."></textarea>
        </div>
        <p v-if="formError" class="sm:col-span-2 text-sm text-red-400">{{ formError }}</p>
        <div class="sm:col-span-2 flex flex-col-reverse sm:flex-row sm:justify-end gap-3 pt-1">
          <button type="button" class="btn-secondary w-full sm:w-auto" @click="showCreateModal = false">Cancelar</button>
          <AppButton type="submit" class="w-full sm:w-auto" :loading="creating">{{ userToEdit ? 'Guardar' : 'Crear' }}</AppButton>
        </div>
      </form>
    </AppModal>

    <!-- Change Password Modal -->
    <AppModal v-model="showPasswordModal" title="Cambiar contraseña" size="sm">
      <form class="space-y-4" @submit.prevent="confirmPasswordChange">
        <AppInput v-model="newPassword" label="Nueva contraseña" type="password" required />
        <div class="flex justify-end gap-3">
          <button type="button" class="btn-secondary" @click="showPasswordModal = false">Cancelar</button>
          <AppButton type="submit" :loading="isUpdatingPassword" :disabled="!newPassword">Guardar</AppButton>
        </div>
      </form>
    </AppModal>

    <!-- Delete Confirmation Modal -->
    <AppModal v-model="showDeleteModal" :title="deleteModalCopy.title" size="sm">
      <div class="space-y-4">
        <div class="p-4 rounded-xl bg-red-500/10 border border-red-500/20 flex gap-3 items-start">
          <svg class="w-5 h-5 text-red-500 mt-0.5 shrink-0" fill="none" viewBox="0 0 24 24" stroke="currentColor">
            <path
              stroke-linecap="round"
              stroke-linejoin="round"
              stroke-width="2"
              d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z"
            />
          </svg>
          <div>
            <p class="text-sm font-bold text-white">{{ deleteModalCopy.lead }}</p>
            <p class="text-xs text-dark-400 mt-1">{{ deleteModalCopy.detail }}</p>
          </div>
        </div>
        <div class="flex justify-end gap-3 pt-2">
          <button type="button" class="btn-secondary" @click="showDeleteModal = false">Cancelar</button>
          <AppButton class="!bg-red-600 hover:!bg-red-500" :loading="deleting" @click="confirmDelete">
            {{ deleteModalCopy.confirmLabel }}
          </AppButton>
        </div>
      </div>
    </AppModal>
  </div>
</template>

<script setup>
import { computed, onMounted, reactive, ref, watch } from 'vue';
import { useRouter } from 'vue-router';
import { useUserStore } from '@/stores/user.store';
import { useAuthStore } from '@/stores/auth.store';
import { useNotification } from '@/composables/useNotification';
import { gymsApi } from '@/api/gyms.api';
import AppButton from '@/components/ui/AppButton.vue';
import AppInput from '@/components/ui/AppInput.vue';
import AppModal from '@/components/ui/AppModal.vue';
import AppSearchSelect from '@/components/ui/AppSearchSelect.vue';
import LoadingSpinner from '@/components/shared/LoadingSpinner.vue';

const router = useRouter();
const userStore = useUserStore();
const authStore = useAuthStore();
const { success, error: showError } = useNotification();
const search = ref('');
const roleFilter = ref('');
const gymFilter = ref('');
const gyms = ref([]);
const showCreateModal = ref(false);
const showPasswordModal = ref(false);
const creating = ref(false);
const deleting = ref(false);
const isUpdatingPassword = ref(false);
const userToEdit = ref(null);
const userToDelete = ref(null);
const showDeleteModal = ref(false);
const newPassword = ref('');
const formError = ref('');
const pageSize = ref(15);
const currentPage = ref(1);
const createForm = reactive({
  nombre: '',
  apellido: '',
  email: '',
  dni: '',
  rol: 'Alumno',
  gymId: null,
  fechaNacimiento: '',
  domicilio: '',
  telefono: '',
  observaciones: '',
});

// Removí el watcher que limpiaba el DNI para permitir texto alfanumérico (extranjeros, etc) como pidió el usuario.

const allowedCreateRoles = computed(() => (authStore.hasRole('Superusuario') ? ['Administrativo', 'Profesor', 'Alumno', 'Terminal'] : ['Profesor', 'Alumno']));

const roleOptions = computed(() => {
  const list = [
    { id: '', label: 'Todos los roles' },
    { id: 'Administrativo', label: 'Administrativo' },
    { id: 'Profesor', label: 'Profesor' },
    { id: 'Alumno', label: 'Alumno' },
  ];
  if (authStore.hasRole('Superusuario')) {
    list.push({ id: 'Terminal', label: 'Terminal' });
    list.push({ id: 'Superusuario', label: 'Superusuario' });
  }
  return list;
});

const gymOptions = computed(() => {
  const list = [{ id: '', label: 'Todos los gimnasios' }];
  if (gyms.value) {
    gyms.value.forEach((g) => {
      list.push({ id: g.id, label: g.nombre });
    });
  }
  return list;
});

const createRoleOptions = computed(() => {
  return allowedCreateRoles.value.map((rol) => ({ id: rol, label: rol }));
});

const createGymOptions = computed(() => {
  return gyms.value.map((g) => ({ id: g.id, label: g.nombre }));
});

const filteredUsers = computed(() => {
  if (userStore.usersServerPaginationEnabled) {
    return userStore.users;
  }
  const q = search.value.toLowerCase();
  const r = roleFilter.value;
  const g = gymFilter.value;

  return userStore.users.filter((u) => {
    const matchesSearch =
      fullName(u).toLowerCase().includes(q) ||
      u.email.toLowerCase().includes(q) ||
      String(u.dni || '')
        .toLowerCase()
        .includes(q);

    const matchesRole = !r || u.rol === r;
    const matchesGym = !g || u.gymId === Number(g);

    return matchesSearch && matchesRole && matchesGym;
  });
});

const activeCount = computed(() => {
  return userStore.users.filter((u) => {
    const matchesGym = !gymFilter.value || u.gymId === Number(gymFilter.value);
    return u.activo && matchesGym;
  }).length;
});

const inactiveCount = computed(() => {
  return userStore.users.filter((u) => {
    const matchesGym = !gymFilter.value || u.gymId === Number(gymFilter.value);
    return !u.activo && matchesGym;
  }).length;
});

const activeCountText = computed(() => {
  const count = activeCount.value;
  return userStore.usersServerPaginationEnabled ? `${count} activos en esta pág.` : `${count} activos`;
});

const inactiveCountText = computed(() => {
  const count = inactiveCount.value;
  return userStore.usersServerPaginationEnabled ? `${count} inactivos en esta pág.` : `${count} inactivos`;
});

const totalCount = computed(() => userStore.usersServerPaginationEnabled ? userStore.usersTotalCount : filteredUsers.value.length);
const totalPages = computed(() => Math.ceil(totalCount.value / pageSize.value));
const pageStart = computed(() => (userStore.users.length ? (currentPage.value - 1) * pageSize.value + 1 : 0));
const pageEnd = computed(() => (userStore.users.length ? pageStart.value + userStore.users.length - 1 : 0));

const paginatedUsers = computed(() => {
  if (userStore.usersServerPaginationEnabled) {
    return userStore.users;
  }
  const start = (currentPage.value - 1) * pageSize.value;
  const end = start + pageSize.value;
  return filteredUsers.value.slice(start, end);
});

async function loadUsers(targetPage = 1) {
  const params = {
    page: targetPage,
    pageSize: pageSize.value,
  };
  if (search.value.trim()) params.search = search.value.trim();
  if (roleFilter.value) params.rol = roleFilter.value;
  if (gymFilter.value) params.gymId = Number(gymFilter.value);

  await userStore.fetchUsers(params);
  currentPage.value = userStore.usersPage ?? targetPage;
}

async function goToPage(targetPage) {
  if (targetPage < 1 || targetPage > totalPages.value || targetPage === currentPage.value) return;
  await loadUsers(targetPage);
}

// Reset/reload when filters change
let searchDebounce = null;

watch([roleFilter, gymFilter], async () => {
  await loadUsers(1);
});

watch(search, () => {
  if (searchDebounce) clearTimeout(searchDebounce);
  searchDebounce = setTimeout(async () => {
    await loadUsers(1);
  }, 300);
});

const deleteModalCopy = computed(() => {
  const user = userToDelete.value;
  if (!user) {
    return {
      title: 'Confirmar eliminación',
      lead: '¿Estás seguro?',
      detail: '',
      confirmLabel: 'Eliminar',
      removesAssociationOnly: false,
    };
  }

  const name = fullName(user);
  const gym = user.gymNombre || 'este gimnasio';
  const dni = user.dni ? ` (DNI ${user.dni})` : '';
  const removesAssociationOnly = userStore.users.some((u) => u.id === user.id && u.gymId !== user.gymId);

  if (removesAssociationOnly) {
    return {
      title: 'Eliminar del gimnasio',
      lead: `Eliminar a ${name}${dni} ?`,
      detail: 'Esta acción no se puede deshacer.',
      confirmLabel: 'Eliminar de este gimnasio',
      removesAssociationOnly: true,
    };
  }

  return {
    title: 'Confirmar eliminación',
    lead: `¿Eliminar a ${name}${dni}?`,
    detail: 'Esta acción no se puede deshacer.',
    confirmLabel: 'Eliminar',
    removesAssociationOnly: false,
  };
});

function fullName(user) {
  return `${user.nombre || ''} ${user.apellido || ''}`.trim();
}

function roleBadge(rol) {
  const map = { Alumno: 'badge-primary', Profesor: 'badge-warning', Administrativo: 'badge-success', Superusuario: 'badge-danger', Terminal: 'badge-warning' };
  return map[rol] || 'badge-primary';
}

function isSystemAdmin(user) {
  return user.email === 'admin' || user.nombre?.toLowerCase() === 'admin';
}

function canChangeRole(user) {
  if (isSystemAdmin(user)) return false;
  if (authStore.hasRole('Superusuario')) return true;
  if (authStore.hasRole('Administrativo')) {
    return user.rol === 'Profesor' || user.rol === 'Alumno';
  }
  return false;
}

function getAvailableRoles(user) {
  if (authStore.hasRole('Superusuario')) {
    return ['Administrativo', 'Profesor', 'Alumno', 'Terminal'];
  }
  if (authStore.hasRole('Administrativo')) {
    return ['Profesor', 'Alumno'];
  }
  return [];
}

function resetCreateForm() {
  userToEdit.value = null;
  createForm.nombre = '';
  createForm.apellido = '';
  createForm.email = '';
  createForm.dni = '';
  createForm.fechaNacimiento = '';
  createForm.domicilio = '';
  createForm.telefono = '';
  createForm.observaciones = '';
  createForm.rol = allowedCreateRoles.value[allowedCreateRoles.value.length - 1];
  createForm.gymId = null;
  formError.value = '';
}

async function openCreateModal() {
  resetCreateForm();
  if (authStore.hasRole('Superusuario') && (!gyms.value || gyms.value.length === 0)) {
    try {
      const response = await gymsApi.getAll();
      gyms.value = response.data || [];
    } catch (err) {
      showError('No se pudieron cargar los gimnasios');
    }
  }
  showCreateModal.value = true;
}

async function openEditModal(user) {
  userToEdit.value = user;
  createForm.nombre = user.nombre;
  createForm.apellido = user.apellido;
  createForm.email = user.email;
  createForm.dni = user.dni;
  createForm.fechaNacimiento = user.fechaNacimiento ? user.fechaNacimiento.split('T')[0] : '';
  createForm.domicilio = user.domicilio || '';
  createForm.telefono = user.telefono || '';
  createForm.observaciones = user.observaciones || '';
  createForm.rol = user.rol;
  createForm.gymId = user.gymId;
  formError.value = '';
  if (authStore.hasRole('Superusuario') && (!gyms.value || gyms.value.length === 0)) {
    try {
      const response = await gymsApi.getAll();
      gyms.value = response.data || [];
    } catch (err) {
      showError('No se pudieron cargar los gimnasios');
    }
  }
  showCreateModal.value = true;
}

async function submitForm() {
  if (userToEdit.value) {
    await editUser();
  } else {
    await createUser();
  }
}

async function createUser() {
  formError.value = '';
  creating.value = true;
  try {
    await userStore.createUser({
      ...createForm,
      fechaNacimiento: createForm.fechaNacimiento || null,
    });
    success('Usuario creado. La contraseña inicial es el DNI.');
    showCreateModal.value = false;
  } catch (err) {
    formError.value = err.response?.data?.error || 'No se pudo crear el usuario';
  } finally {
    creating.value = false;
  }
}

async function editUser() {
  formError.value = '';
  creating.value = true;
  try {
    await userStore.updateUser(userToEdit.value.id, {
      nombre: createForm.nombre,
      apellido: createForm.apellido,
      fechaNacimiento: createForm.fechaNacimiento || null,
      domicilio: createForm.domicilio,
      telefono: createForm.telefono,
      observaciones: createForm.observaciones,
    });
    success('Usuario actualizado correctamente.');
    showCreateModal.value = false;
  } catch (err) {
    formError.value = err.response?.data?.error || 'No se pudo actualizar el usuario';
  } finally {
    creating.value = false;
  }
}

async function handleRoleChange(user, newRole) {
  try {
    await userStore.changeRole(user, newRole);
    success(`Rol cambiado a ${newRole}`);
  } catch (err) {
    showError(err.response?.data?.error || 'Error al cambiar rol');
  }
}

async function handleToggleStatus(user) {
  try {
    await userStore.toggleUserStatus(user);
    success(user.activo ? 'Usuario desactivado en este gimnasio' : 'Usuario activado en este gimnasio');
  } catch (err) {
    showError(err.response?.data?.error || 'Error al cambiar estado');
  }
}

function openPasswordModal(user) {
  userToEdit.value = user;
  newPassword.value = '';
  showPasswordModal.value = true;
}

function handleDeleteClick(user) {
  userToDelete.value = user;
  showDeleteModal.value = true;
}

async function confirmDelete() {
  if (!userToDelete.value) return;

  deleting.value = true;
  try {
    const removedAssociationOnly = deleteModalCopy.value.removesAssociationOnly;
    await userStore.deleteUser(userToDelete.value);
    success(removedAssociationOnly ? 'Usuario quitado del gimnasio correctamente' : 'Usuario eliminado correctamente');
    showDeleteModal.value = false;
    userToDelete.value = null;
  } catch (err) {
    showError(err.response?.data?.error || 'No se pudo eliminar el usuario');
  } finally {
    deleting.value = false;
  }
}

async function confirmPasswordChange() {
  if (!userToEdit.value || !newPassword.value) return;
  isUpdatingPassword.value = true;
  try {
    await userStore.changePassword(userToEdit.value.id, newPassword.value);
    success('Contraseña actualizada');
    showPasswordModal.value = false;
  } catch (err) {
    showError(err.response?.data?.error || 'Error al actualizar contraseña');
  } finally {
    isUpdatingPassword.value = false;
  }
}

onMounted(async () => {
  try {
    await loadUsers(1);
    if (authStore.hasRole('Superusuario')) {
      const response = await gymsApi.getAll();
      gyms.value = response.data || [];
    }
  } catch (err) {
    showError('No se pudieron cargar los datos');
  }
});
</script>
