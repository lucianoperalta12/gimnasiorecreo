<template>
  <div class="animate-fade-in">
    <div class="mb-6">
      <h1 class="page-title">Usuarios</h1>
      <p class="page-subtitle">Administración de usuarios y roles</p>
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
                      class="input py-1 px-2 text-xs w-auto inline-block"
                    >
                      <option value="Alumno">Alumno</option>
                      <option value="Profesor">Profesor</option>
                      <option value="Superusuario">Superusuario</option>
                    </select>
                    
                    <button
                      @click="handleToggleStatus(u.id, u.activo)"
                      class="relative inline-flex h-5 w-10 items-center rounded-full transition-colors focus:outline-none"
                      :class="u.activo ? 'bg-emerald-600' : 'bg-dark-700'"
                    >
                      <span
                        class="inline-block h-3 w-3 transform rounded-full bg-white transition-transform"
                        :class="u.activo ? 'translate-x-6' : 'translate-x-1'"
                      />
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
                    class="input py-1 px-2 text-xs w-auto"
                  >
                    <option value="Alumno">Alumno</option>
                    <option value="Profesor">Profesor</option>
                    <option value="Superusuario">Superusuario</option>
                  </select>
                  <button
                    @click="handleToggleStatus(u.id, u.activo)"
                    class="relative inline-flex h-5 w-10 items-center rounded-full transition-colors"
                    :class="u.activo ? 'bg-emerald-600' : 'bg-dark-700'"
                  >
                    <span
                      class="inline-block h-3 w-3 transform rounded-full bg-white transition-transform"
                      :class="u.activo ? 'translate-x-6' : 'translate-x-1'"
                    />
                  </button>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </template>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { useUserStore } from '@/stores/user.store'
import { useNotification } from '@/composables/useNotification'
import LoadingSpinner from '@/components/shared/LoadingSpinner.vue'

const userStore = useUserStore()
const { success, error: showError } = useNotification()
const search = ref('')

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

onMounted(() => userStore.fetchUsers())
</script>
