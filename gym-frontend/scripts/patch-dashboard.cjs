const fs = require('fs')
const path = require('path')
const d = 'div'
const dashboardPath = path.join(__dirname, '../src/views/DashboardView.vue')
let dashboard = fs.readFileSync(dashboardPath, 'utf8')

const alumnoBlock = `    <${d} v-if="authStore.hasRole('Alumno')" class="space-y-8">
      <router-link v-if="myAccess" to="/my-membership" class="card p-5 max-w-2xl block hover:border-primary-500/30 transition-colors">
        <${d} class="flex items-start justify-between gap-4">
          <${d}>
            <p class="text-xs uppercase tracking-widest text-dark-500 font-bold mb-2">Tu membresía</p>
            <span :class="accessStatusBadgeClass(myAccess.estadoAcceso)" class="text-sm px-3 py-1">{{ myAccess.estadoAcceso }}</span>
            <p v-if="myAccess.planNombre" class="text-sm text-dark-400 mt-3">{{ myAccess.planNombre }} · vence {{ formatDate(myAccess.fechaVencimiento) }}</p>
          </${d}>
          <span class="text-3xl">💳</span>
        </${d}>
      </router-link>
      <${d}>
        <h2 class="text-lg font-semibold text-white mb-4">Tus rutinas activas</h2>
        <LoadingSpinner v-if="loadingRoutines" />
        <${d} v-else-if="myRoutines.length === 0" class="card text-center py-12">
          <p class="text-dark-400">No tenés rutinas asignadas todavía.</p>
        </${d}>
        <${d} v-else class="grid grid-cols-1 md:grid-cols-2 gap-4">
          <router-link v-for="routine in myRoutines" :key="routine.id" :to="\`/routines/\${routine.id}\`" class="card-hover group">
            <${d} class="flex items-start justify-between">
              <${d}>
                <h3 class="font-semibold text-white">{{ routine.nombre }}</h3>
                <p class="text-sm text-dark-400 mt-1">{{ routine.descripcion || 'Sin descripción' }}</p>
              </${d}>
              <span class="badge-success">Activa</span>
            </${d}>
          </router-link>
        </${d}>
      </${d}>
    </${d}>
`

const start = dashboard.indexOf("authStore.hasRole('Alumno')")
const alumnoLine = dashboard.lastIndexOf('<', start)
const alumnoStart = dashboard.lastIndexOf('\n', alumnoLine - 5) + 1
const templateEnd = dashboard.indexOf('</template>')
dashboard = dashboard.slice(0, alumnoStart) + alumnoBlock + '\n' + dashboard.slice(templateEnd)

if (!dashboard.includes('useMembershipStore')) {
  dashboard = dashboard.replace(
    "import LoadingSpinner from '@/components/shared/LoadingSpinner.vue'",
    "import LoadingSpinner from '@/components/shared/LoadingSpinner.vue'\nimport { useMembershipStore } from '@/stores/membership.store'\nimport { accessStatusBadgeClass, formatDate } from '@/constants/membershipStatus'"
  )
  dashboard = dashboard.replace(
    'const userStore = useUserStore()',
    "const userStore = useUserStore()\nconst membershipStore = useMembershipStore()\nconst myAccess = computed(() => membershipStore.myAccess)"
  )
  dashboard = dashboard.replace(
    'await routineStore.fetchMyRoutines()',
    'await Promise.all([routineStore.fetchMyRoutines(), membershipStore.fetchMyAccess()])'
  )
}

if (!dashboard.includes('to="/memberships"')) {
  dashboard = dashboard.replace(
    '</router-link>\n\n      </div>\n    </div>',
    `</router-link>\n\n        <router-link v-if="authStore.hasRole('Superusuario', 'Administrativo')" to="/memberships" class="group flex items-center p-4 rounded-2xl bg-dark-900/40 border border-dark-800/50 hover:bg-dark-800/60 hover:border-primary-500/30 transition-all duration-300"><${d} class="w-10 h-10 rounded-xl bg-amber-500/10 text-amber-500 flex items-center justify-center mr-4 text-lg">💳</${d}><${d}><h3 class="text-sm font-bold text-white">Membresías</h3><p class="text-xs text-dark-500">Alumnos y pagos</p></${d}></router-link>\n\n      </div>\n    </div>`
  )
}

dashboard = dashboard.replace(
  "icon: '👨‍🏫', label: 'Profesores'",
  "icon: '💳', label: 'Membresías activas'"
)
dashboard = dashboard.replace(
  'requests.push(userStore.fetchUsers())',
  "requests.push(userStore.fetchUsers(), membershipStore.fetchMemberships({ estado: 'Activa' }))"
)
dashboard = dashboard.replace(
  'stats.value[3].value = assignmentSummary.value.profesoresCount || 0',
  'stats.value[3].value = membershipStore.memberships.length'
)

fs.writeFileSync(dashboardPath, dashboard, 'utf8')
console.log('Dashboard patched')
