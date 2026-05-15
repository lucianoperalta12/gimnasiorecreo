<template>
  <div class="animate-fade-in">
    <div class="mb-6">
      <h1 class="page-title">Mi membresía</h1>
      <p class="page-subtitle">Estado de acceso al gimnasio</p>
    </div>

    <LoadingSpinner v-if="store.loading" />

    <template v-else-if="access">
      <div class="card p-6 md:p-8 max-w-2xl">
        <div class="flex items-start justify-between gap-4 mb-6">
          <div>
            <p class="text-xs uppercase tracking-widest text-dark-500 font-bold mb-1">Estado de acceso</p>
            <span :class="accessStatusBadgeClass(access.estadoAcceso)" class="text-base px-3 py-1">{{ access.estadoAcceso }}</span>
          </div>
          <div class="w-12 h-12 rounded-2xl bg-primary-500/10 text-primary-400 flex items-center justify-center">
            <svg class="w-8 h-8" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2">
              <path stroke-linecap="round" stroke-linejoin="round" d="M15 9h3.75M15 12h3.75M15 15h3.75M4.5 19.5h15a2.25 2.25 0 002.25-2.25V6.75A2.25 2.25 0 0019.5 4.5h-15a2.25 2.25 0 00-2.25 2.25v10.5A2.25 2.25 0 004.5 19.5zm6-10.125a1.875 1.875 0 11-3.75 0 1.875 1.875 0 013.75 0zm1.294 6.336a6.721 6.721 0 01-3.17.789 6.721 6.721 0 01-3.168-.789 3.376 3.376 0 016.338 0z" />
            </svg>
          </div>
        </div>

        <div class="grid grid-cols-1 sm:grid-cols-2 gap-4">
          <div class="rounded-xl bg-dark-900/60 border border-dark-800 p-4">
            <p class="text-[10px] uppercase tracking-wider text-dark-500 font-bold">Plan</p>
            <p class="text-lg font-bold text-white mt-1">{{ access.planNombre || 'Sin plan activo' }}</p>
          </div>
          <div class="rounded-xl bg-dark-900/60 border border-dark-800 p-4">
            <p class="text-[10px] uppercase tracking-wider text-dark-500 font-bold">Vencimiento</p>
            <p class="text-lg font-bold text-white mt-1">{{ formatDate(access.fechaVencimiento) }}</p>
          </div>
          <div v-if="access.diasRestantes != null" class="rounded-xl bg-dark-900/60 border border-dark-800 p-4 sm:col-span-2">
            <p class="text-[10px] uppercase tracking-wider text-dark-500 font-bold">Días restantes</p>
            <p class="text-2xl font-black text-primary-400 mt-1">{{ access.diasRestantes }}</p>
          </div>
        </div>

        <p v-if="access.estadoAcceso === 'Sin membresía'" class="text-sm text-dark-400 mt-6">
          Contactá a la administración del gimnasio para activar tu membresía.
        </p>
        <p v-else-if="access.estadoAcceso === 'Moroso'" class="text-sm text-amber-400/90 mt-6">
          Tenés pagos pendientes. Regularizá tu situación en recepción.
        </p>
      </div>
    </template>
  </div>
</template>

<script setup>
import { computed, onMounted } from 'vue'
import { useMembershipStore } from '@/stores/membership.store'
import { accessStatusBadgeClass, formatDate } from '@/constants/membershipStatus'
import LoadingSpinner from '@/components/shared/LoadingSpinner.vue'

const store = useMembershipStore()
const access = computed(() => store.myAccess)

onMounted(() => store.fetchMyAccess())
</script>
