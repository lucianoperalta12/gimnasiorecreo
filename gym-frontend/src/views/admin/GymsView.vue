<template>
  <div class="animate-fade-in">
    <!-- Header -->
    <div class="flex items-center justify-between mb-6">
      <div>
        <h1 class="page-title">Gimnasios</h1>
        <p class="page-subtitle hidden sm:block">Administración de gimnasios</p>
      </div>
      <button @click="openCreateModal" class="btn-primary flex items-center gap-2">
        <svg class="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2.5">
          <path stroke-linecap="round" stroke-linejoin="round" d="M12 4v16m8-8H4" />
        </svg>
        <span class="hidden sm:inline text-sm">Crear gimnasio</span>
      </button>
    </div>

    <LoadingSpinner v-if="loading" />

    <div v-else-if="gyms.length === 0" class="card text-center py-8">
      <p class="text-dark-400">No hay gimnasios registrados.</p>
    </div>

    <template v-else>
      <!-- Mobile Cards (< md) -->
      <div class="md:hidden space-y-3">
        <div v-for="gym in gyms" :key="gym.id" class="p-4 rounded-2xl bg-dark-900/40 border border-dark-800/50" :class="{ 'opacity-50': !gym.activo }">
          <div class="flex items-center gap-3 mb-3">
            <img v-if="gym.logoUrl" :src="gym.logoUrl" alt="" class="w-10 h-10 rounded object-cover bg-dark-900" />
            <div v-else class="w-10 h-10 rounded bg-dark-900 border border-dark-700 shrink-0" />
            <div class="flex-1 min-w-0">
              <p class="font-semibold text-white text-sm truncate">{{ gym.nombre }}</p>
              <p class="text-[11px] text-dark-500 mt-0.5 truncate">{{ gym.duenoNombreApellido }}</p>
            </div>
            <span :class="gym.activo ? 'badge-success' : 'badge-danger'" class="text-[10px] shrink-0">{{ gym.activo ? 'Activo' : 'Inactivo' }}</span>
          </div>
          <div class="flex items-center justify-between">
            <div class="flex items-center gap-2">
              <span class="w-4 h-4 rounded border border-dark-700" :style="{ backgroundColor: gym.colorPrincipalHex }" />
              <span class="text-xs text-dark-400">{{ gym.colorPrincipalHex }}</span>
            </div>
            <div class="flex items-center gap-2">
              <button class="btn-secondary btn-sm text-xs" @click="openEditModal(gym)">Editar</button>
              <button class="btn-secondary btn-sm text-xs" @click="toggleGym(gym)">
                {{ gym.activo ? 'Desactivar' : 'Activar' }}
              </button>
            </div>
          </div>
        </div>
      </div>

      <!-- Desktop Table (md+) -->
      <div class="hidden md:block overflow-x-auto rounded-xl border border-dark-700/50">
        <table class="w-full text-sm text-left">
          <thead class="bg-dark-800/80 border-b border-dark-700">
            <tr>
              <th class="px-4 py-3 text-xs font-semibold text-dark-400 uppercase tracking-wider">Gimnasio</th>
              <th class="px-4 py-3 text-xs font-semibold text-dark-400 uppercase tracking-wider">Dueño</th>
              <th class="px-4 py-3 text-xs font-semibold text-dark-400 uppercase tracking-wider">Color</th>
              <th class="px-4 py-3 text-xs font-semibold text-dark-400 uppercase tracking-wider">Estado</th>
              <th class="px-4 py-3 text-xs font-semibold text-dark-400 uppercase tracking-wider text-right">Acciones</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="gym in gyms" :key="gym.id" class="border-b border-dark-800 hover:bg-dark-800/40 transition-colors" :class="{ 'opacity-50': !gym.activo }">
              <td class="px-4 py-3">
                <div class="flex items-center gap-3">
                  <img v-if="gym.logoUrl" :src="gym.logoUrl" alt="" class="w-10 h-10 rounded object-cover bg-dark-900" />
                  <div v-else class="w-10 h-10 rounded bg-dark-900 border border-dark-700" />
                  <span class="font-medium text-white">{{ gym.nombre }}</span>
                </div>
              </td>
              <td class="px-4 py-3 text-dark-400">{{ gym.duenoNombreApellido }}</td>
              <td class="px-4 py-3">
                <div class="flex items-center gap-2">
                  <span class="w-5 h-5 rounded border border-dark-700" :style="{ backgroundColor: gym.colorPrincipalHex }" />
                  <span class="text-dark-400">{{ gym.colorPrincipalHex }}</span>
                </div>
              </td>
              <td class="px-4 py-3">
                <span :class="gym.activo ? 'badge-success' : 'badge-danger'">{{ gym.activo ? 'Activo' : 'Inactivo' }}</span>
              </td>
              <td class="px-4 py-3 text-right">
                <div class="flex items-center justify-end gap-2">
                  <button class="btn-secondary btn-sm" @click="openEditModal(gym)">Editar</button>
                  <button class="btn-secondary btn-sm" @click="toggleGym(gym)">
                    {{ gym.activo ? 'Desactivar' : 'Activar' }}
                  </button>
                </div>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </template>

    <AppModal v-model="showModal" :title="editingGym ? 'Editar gimnasio' : 'Crear gimnasio'" size="lg">
      <form class="grid grid-cols-1 sm:grid-cols-2 gap-4" @submit.prevent="saveGym">
        <AppInput v-model="form.nombre" label="Nombre del gimnasio" required />
        <AppInput v-model="form.duenoNombreApellido" label="Nombre y apellido del dueño" required />
        <AppInput v-model="form.logoUrl" label="Imagen/logo del gimnasio" placeholder="https://..." />
        <div>
          <label class="label">Color principal</label>
          <div class="flex gap-3">
            <input v-model="form.colorPrincipalHex" type="color" class="h-11 w-14 rounded bg-dark-800 border border-dark-600" />
            <input v-model="form.colorPrincipalHex" class="input" placeholder="#2563EB" required />
          </div>
        </div>
        <p v-if="formError" class="sm:col-span-2 text-sm text-red-400">{{ formError }}</p>
        <div class="sm:col-span-2 flex justify-end gap-3">
          <button type="button" class="btn-secondary" @click="showModal = false">Cancelar</button>
          <AppButton type="submit" :loading="saving">Guardar</AppButton>
        </div>
      </form>
    </AppModal>
  </div>
</template>

<script setup>
import { onMounted, reactive, ref } from 'vue'
import { gymsApi } from '@/api/gyms.api'
import { useNotification } from '@/composables/useNotification'
import AppButton from '@/components/ui/AppButton.vue'
import AppInput from '@/components/ui/AppInput.vue'
import AppModal from '@/components/ui/AppModal.vue'
import LoadingSpinner from '@/components/shared/LoadingSpinner.vue'

const { success, error: showError } = useNotification()
const gyms = ref([])
const loading = ref(false)
const saving = ref(false)
const showModal = ref(false)
const editingGym = ref(null)
const formError = ref('')
const form = reactive({ nombre: '', duenoNombreApellido: '', logoUrl: '', colorPrincipalHex: '#2563EB' })

async function load() {
  loading.value = true
  try {
    const response = await gymsApi.getAll()
    gyms.value = response.data || []
  } catch (err) {
    showError('No se pudieron cargar los gimnasios')
  } finally {
    loading.value = false
  }
}

function resetForm() {
  form.nombre = ''
  form.duenoNombreApellido = ''
  form.logoUrl = ''
  form.colorPrincipalHex = '#2563EB'
  formError.value = ''
}

function openCreateModal() {
  editingGym.value = null
  resetForm()
  showModal.value = true
}

function openEditModal(gym) {
  editingGym.value = gym
  form.nombre = gym.nombre
  form.duenoNombreApellido = gym.duenoNombreApellido
  form.logoUrl = gym.logoUrl || ''
  form.colorPrincipalHex = gym.colorPrincipalHex
  formError.value = ''
  showModal.value = true
}

async function saveGym() {
  formError.value = ''
  saving.value = true
  try {
    if (editingGym.value) await gymsApi.update(editingGym.value.id, { ...form })
    else await gymsApi.create({ ...form })
    success('Gimnasio guardado')
    showModal.value = false
    await load()
  } catch (err) {
    formError.value = err.response?.data?.error || 'No se pudo guardar el gimnasio'
  } finally {
    saving.value = false
  }
}

async function toggleGym(gym) {
  try {
    await gymsApi.toggleStatus(gym.id)
    success(gym.activo ? 'Gimnasio desactivado' : 'Gimnasio activado')
    await load()
  } catch (err) {
    showError(err.response?.data?.error || 'No se pudo cambiar el estado')
  }
}

onMounted(load)
</script>
