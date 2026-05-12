<template>
  <div class="animate-fade-in">
    <div class="flex items-center justify-between mb-6">
      <div>
        <h1 class="page-title">Usuarios</h1>
        <p class="page-subtitle">Administración de usuarios y roles</p>
      </div>
      <AppButton 
        variant="secondary" 
        @click="router.push('/dashboard')"
        class="md:px-4 px-2.5 !rounded-xl md:!rounded-lg"
      >
        <svg class="w-5 h-5 md:w-3 md:h-3" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="3">
          <path stroke-linecap="round" stroke-linejoin="round" d="M10.5 19.5L3 12m0 0l7.5-7.5M3 12h18" />
        </svg>
        <span class="hidden md:inline ml-1 text-[10px] font-black uppercase tracking-[0.2em]">Volver</span>
      </AppButton>
    </div>

    <LoadingSpinner v-if="userStore.loading" />

    <template v-else>
      <div class="mb-4">
        <input v-model="search" type="text" placeholder="Buscar por nombre o email..." class="input max-w-sm" />
      </div>

      <!-- Responsive List -->
      <div v-if="filteredUsers.length">
        <!-- Desktop Table -->
        <div class="hidden lg:block table-container">
          <table class="table">
            <thead>
              <tr>
                <th>Nombre</th>
                <th>Email</th>
                <th>Rol</th>
                <th>Estado</th>
                <th>Fecha Registro</th>
                <th class="text-right">Acciones</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="u in filteredUsers" :key="u.id" :class="{ 'opacity-50': !u.activo }">
                <td class="font-medium text-white">{{ u.nombre }}</td>
                <td class="text-dark-400">{{ u.email }}</td>
                <td>
                  <span :class="roleBadge(u.rol)">{{ u.rol }}</span>
                </td>
                <td>
                  <span :class="u.activo ? 'badge-success' : 'badge-danger'">
                    {{ u.activo ? 'Activo' : 'Inactivo' }}
                  </span>
                </td>
                <td class="text-dark-400 text-sm">{{ formatDate(u.fechaCreacion) }}</td>
                <td class="text-right">
                  <div class="flex items-center justify-end gap-4">
                    <select
                      :value="u.rol"
                      @change="handleRoleChange(u.id, ($event.target).value)"
                      class="input py-1 px-2 text-xs w-auto inline-block disabled:opacity-50 disabled:cursor-not-allowed"
                      :disabled="isAdmin(u)"
                    >
                      <option value="Alumno">Alumno</option>
                      <option value="Profesor">Profesor</option>
                      <option value="Superusuario">Superusuario</option>
                    </select>
                    
                    <button
                      v-if="!isAdmin(u)"
                      @click.stop="handleToggleStatus(u.id, u.activo)"
                      class="relative inline-flex h-5 w-10 items-center rounded-full transition-colors focus:outline-none"
                      :class="u.activo ? 'bg-emerald-600' : 'bg-dark-700'"
                    >
                      <span
                        class="inline-block h-3 w-3 transform rounded-full bg-white transition-transform"
                        :class="u.activo ? 'translate-x-6' : 'translate-x-1'"
                      />
                    </button>
                    <div v-else class="w-10 h-5" />

                    <button
                      v-if="authStore.hasRole('Superusuario') && !isAdmin(u)"
                      @click.stop="openPasswordModal(u)"
                      class="text-primary-500 hover:text-primary-400 transition-colors p-1"
                      title="Cambiar contraseña"
                    >
                      <svg class="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2">
                        <path stroke-linecap="round" stroke-linejoin="round" d="M16.5 10.5V6.75a4.5 4.5 0 10-9 0v3.75m-.75 11.25h10.5a2.25 2.25 0 002.25-2.25v-6.75a2.25 2.25 0 00-2.25-2.25H6.75a2.25 2.25 0 00-2.25 2.25v6.75a2.25 2.25 0 002.25 2.25z" />
                      </svg>
                    </button>

                    <button
                      v-if="!isAdmin(u)"
                      @click.stop="openDeleteModal(u)"
                      class="text-red-500 hover:text-red-400 transition-colors p-1"
                      title="Eliminar usuario definitivamente"
                    >
                   <svg class="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="1.5">
                <path stroke-linecap="round" stroke-linejoin="round" d="M14.74 9l-.346 9m-4.788 0L9.26 9m9.968-3.21c.342.052.682.107 1.022.166m-1.022-.165L18.16 19.673a2.25 2.25 0 01-2.244 2.077H8.084a2.25 2.25 0 01-2.244-2.077L4.772 5.79m14.456 0a48.108 48.108 0 00-3.478-.397m-12 .562c.34-.059.68-.114 1.022-.165m0 0a48.11 48.11 0 013.478-.397m7.5 0v-.916c0-1.18-.91-2.164-2.09-2.201a51.964 51.964 0 00-3.32 0c-1.18.037-2.09 1.022-2.09 2.201v.916m7.5 0a48.667 48.667 0 00-7.5 0" />
              </svg>
                    </button>
                  </div>
                </td>
              </tr>
            </tbody>
          </table>
        </div>

        <!-- Mobile Cards -->
        <div class="lg:hidden space-y-4">
          <div v-for="u in filteredUsers" :key="u.id" class="card p-4 space-y-4" :class="{ 'opacity-50': !u.activo }">
            <div class="flex items-start justify-between">
              <div>
                <h3 class="font-bold text-white">{{ u.nombre }}</h3>
                <p class="text-xs text-dark-500">{{ u.email }}</p>
              </div>
              <span :class="u.activo ? 'badge-success' : 'badge-danger'">
                {{ u.activo ? 'Activo' : 'Inactivo' }}
              </span>
            </div>
            
            <div class="flex items-center justify-between pt-2 border-t border-dark-700/50">
              <div class="flex flex-col gap-1">
                <span class="text-[10px] uppercase font-bold text-dark-500">Rol</span>
                <span :class="roleBadge(u.rol)">{{ u.rol }}</span>
              </div>
              <div class="flex flex-col gap-1 items-end">
                <span class="text-[10px] uppercase font-bold text-dark-500">Acciones</span>
                <div class="flex items-center gap-3">
                  <select
                    :value="u.rol"
                    @change="handleRoleChange(u.id, ($event.target).value)"
                    class="input py-1 px-2 text-xs w-auto disabled:opacity-50"
                    :disabled="isAdmin(u)"
                  >
                    <option value="Alumno">Alumno</option>
                    <option value="Profesor">Profesor</option>
                    <option value="Superusuario">Superusuario</option>
                  </select>
                  <button
                    v-if="!isAdmin(u)"
                    @click.stop="handleToggleStatus(u.id, u.activo)"
                    class="relative inline-flex h-5 w-10 items-center rounded-full transition-colors"
                    :class="u.activo ? 'bg-emerald-600' : 'bg-dark-700'"
                  >
                    <span
                      class="inline-block h-3 w-3 transform rounded-full bg-white transition-transform"
                      :class="u.activo ? 'translate-x-6' : 'translate-x-1'"
                    />
                  </button>
                  <button
                    v-if="authStore.hasRole('Superusuario') && !isAdmin(u)"
                    @click.stop="openPasswordModal(u)"
                    class="text-primary-500 hover:text-primary-400 transition-colors p-1"
                  >
                    <svg class="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2">
                      <path stroke-linecap="round" stroke-linejoin="round" d="M16.5 10.5V6.75a4.5 4.5 0 10-9 0v3.75m-.75 11.25h10.5a2.25 2.25 0 002.25-2.25v-6.75a2.25 2.25 0 00-2.25-2.25H6.75a2.25 2.25 0 00-2.25 2.25v6.75a2.25 2.25 0 002.25 2.25z" />
                    </svg>
                  </button>
                  <button
                    v-if="!isAdmin(u)"
                    @click.stop="openDeleteModal(u)"
                    class="text-red-500 hover:text-red-400 transition-colors p-1"
                  >
                      <svg class="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="1.5">
                <path stroke-linecap="round" stroke-linejoin="round" d="M14.74 9l-.346 9m-4.788 0L9.26 9m9.968-3.21c.342.052.682.107 1.022.166m-1.022-.165L18.16 19.673a2.25 2.25 0 01-2.244 2.077H8.084a2.25 2.25 0 01-2.244-2.077L4.772 5.79m14.456 0a48.108 48.108 0 00-3.478-.397m-12 .562c.34-.059.68-.114 1.022-.165m0 0a48.11 48.11 0 013.478-.397m7.5 0v-.916c0-1.18-.91-2.164-2.09-2.201a51.964 51.964 0 00-3.32 0c-1.18.037-2.09 1.022-2.09 2.201v.916m7.5 0a48.667 48.667 0 00-7.5 0" />
              </svg>
                  </button>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </template>

    <!-- Delete Confirmation Modal -->
    <div v-if="showDeleteModal" class="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/60 backdrop-blur-sm">
      <div class="bg-dark-900 border border-dark-800 rounded-2xl p-6 max-w-sm w-full shadow-2xl">
        <h3 class="text-xl font-bold text-white mb-2">Eliminar Usuario</h3>
        <p class="text-dark-400 text-sm mb-6">
          ¿Estás seguro que deseas eliminar definitivamente a <strong class="text-white">{{ userToDelete?.nombre }}</strong>? Esta acción no se puede deshacer.
        </p>
        <div class="flex justify-end gap-3">
          <button @click="showDeleteModal = false" class="btn bg-dark-700 hover:bg-dark-600 text-white border-none">
            Cancelar
          </button>
          <button @click="confirmDelete" class="btn bg-red-600 hover:bg-red-700 text-white border-none" :disabled="isDeleting">
            {{ isDeleting ? 'Eliminando...' : 'Eliminar' }}
          </button>
        </div>
      </div>
    </div>

    <!-- Password Change Modal -->
    <div v-if="showPasswordModal" class="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/60 backdrop-blur-sm">
      <div class="bg-dark-900 border border-dark-800 rounded-2xl p-6 max-w-sm w-full shadow-2xl animate-scale-in">
        <h3 class="text-xl font-bold text-white mb-2">Cambiar Contraseña</h3>
        <p class="text-dark-400 text-sm mb-4">
          Nueva contraseña para <strong class="text-white">{{ userToEdit?.nombre }}</strong>
        </p>
        
        <div class="space-y-4 mb-6">
          <div>
            <label class="text-[10px] uppercase font-black text-dark-500 mb-1.5 block tracking-widest">Nueva Contraseña</label>
            <input 
              v-model="newPassword" 
              type="password" 
              placeholder="••••••••" 
              class="input w-full"
              @keyup.enter="confirmPasswordChange"
            />
          </div>
        </div>

        <div class="flex justify-end gap-3">
          <button @click="showPasswordModal = false" class="btn bg-dark-700 hover:bg-dark-600 text-white border-none">
            Cancelar
          </button>
          <button 
            @click="confirmPasswordChange" 
            class="btn bg-primary-600 hover:bg-primary-700 text-white border-none" 
            :disabled="isUpdatingPassword || !newPassword"
          >
            {{ isUpdatingPassword ? 'Actualizando...' : 'Aceptar' }}
          </button>
        </div>
      </div>
    </div>

  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useUserStore } from '@/stores/user.store'
import { useAuthStore } from '@/stores/auth.store'
import { useNotification } from '@/composables/useNotification'
import AppButton from '@/components/ui/AppButton.vue'
import LoadingSpinner from '@/components/shared/LoadingSpinner.vue'

const router = useRouter()
const userStore = useUserStore()
const authStore = useAuthStore()
const { success, error: showError } = useNotification()
const search = ref('')
const showDeleteModal = ref(false)
const showPasswordModal = ref(false)
const userToDelete = ref(null)
const userToEdit = ref(null)
const newPassword = ref('')
const isDeleting = ref(false)
const isUpdatingPassword = ref(false)

const filteredUsers = computed(() => {
  const q = search.value.toLowerCase()
  return userStore.users.filter(u =>
    u.nombre.toLowerCase().includes(q) || u.email.toLowerCase().includes(q)
  )
})

function roleBadge(rol) {
  const map = { Alumno: 'badge-primary', Profesor: 'badge-warning', Superusuario: 'badge-danger' }
  return map[rol] || 'badge-primary'
}

function formatDate(d) {
  return new Date(d).toLocaleDateString('es-AR', { day: '2-digit', month: 'short', year: 'numeric' })
}

function isAdmin(user) {
  return user.email === 'admin' || user.nombre.toLowerCase() === 'admin'
}

async function handleRoleChange(userId, newRole) {
  try {
    await userStore.changeRole(userId, newRole)
    success(`Rol cambiado a ${newRole}`)
  } catch (err) {
    showError(err.response?.data?.error || 'Error al cambiar rol')
  }
}

async function handleToggleStatus(userId, currentStatus) {
  try {
    await userStore.toggleUserStatus(userId)
    success(currentStatus ? 'Usuario desactivado' : 'Usuario activado')
  } catch (err) {
    showError(err.response?.data?.error || 'Error al cambiar estado')
  }
}

function openDeleteModal(user) {
  userToDelete.value = user
  showDeleteModal.value = true
}

async function confirmDelete() {
  if (!userToDelete.value) return
  isDeleting.value = true
  try {
    await userStore.deleteUser(userToDelete.value.id)
    success('Usuario eliminado exitosamente')
    showDeleteModal.value = false
  } catch (err) {
    showError(err.response?.data?.error || 'Error al eliminar usuario')
  } finally {
    isDeleting.value = false
    userToDelete.value = null
  }
}

function openPasswordModal(user) {
  userToEdit.value = user
  newPassword.value = ''
  showPasswordModal.value = true
}

async function confirmPasswordChange() {
  if (!userToEdit.value || !newPassword.value) return
  isUpdatingPassword.value = true
  try {
    await userStore.changePassword(userToEdit.value.id, newPassword.value)
    success('Contraseña actualizada exitosamente')
    showPasswordModal.value = false
  } catch (err) {
    showError(err.response?.data?.error || 'Error al actualizar contraseña')
  } finally {
    isUpdatingPassword.value = false
    userToEdit.value = null
    newPassword.value = ''
  }
}

onMounted(() => userStore.fetchUsers())
</script>
