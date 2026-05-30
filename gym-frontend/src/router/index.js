import { createRouter, createWebHistory } from 'vue-router'
import { useAuthStore } from '@/stores/auth.store'

const routes = [
  {
    path: '/select-gym',
    name: 'Seleccionar Gimnasio',
    component: () => import('@/views/SelectGymView.vue'),
    meta: { requiresAuth: true, requiresGymSelection: true }
  },
  {
    path: '/login',
    name: 'Login',
    component: () => import('@/views/LoginView.vue'),
    meta: { requiresAuth: false }
  },
  {
    path: '/',
    component: () => import('@/layouts/MainLayout.vue'),
    meta: { requiresAuth: true },
    children: [
      {
        path: '',
        name: 'Panel Principal',
        component: () => import('@/views/DashboardView.vue')
      },
      {
        path: 'ingresos',
        name: 'Asistencia',
        component: () => import('@/views/admin/IngresosView.vue'),
        meta: { roles: ['Superusuario', 'Administrativo'] }
      },
      {
        path: 'terminal',
        name: 'Terminal',
        component: () => import('@/views/DashboardView.vue'),
        meta: { roles: ['Superusuario', 'Administrativo', 'Terminal'] }
      },
      // Exercises
      {
        path: 'exercises',
        name: 'Ejercicios',
        component: () => import('@/views/exercises/ExerciseListView.vue'),
        meta: { roles: ['Profesor', 'Superusuario', 'Administrativo'] }
      },
      // Routines
      {
        path: 'routines',
        name: 'Rutinas',
        component: () => import('@/views/routines/RoutineListView.vue'),
        meta: { roles: ['Profesor', 'Superusuario', 'Administrativo'] }
      },
      {
        path: 'routines/new',
        name: 'Crear una Rutina',
        component: () => import('@/views/routines/RoutineFormView.vue'),
        meta: { roles: ['Profesor', 'Superusuario', 'Administrativo'] }
      },
      {
        path: 'routines/:id/edit',
        name: 'Edición de Rutina',
        component: () => import('@/views/routines/RoutineFormView.vue'),
        meta: { roles: ['Profesor', 'Superusuario', 'Administrativo'] }
      },
      {
        path: 'routines/:id',
        name: 'Detalle de la Rutina',
        component: () => import('@/views/routines/RoutineDetailView.vue')
      },
      // Assignments
      {
        path: 'assignments',
        name: 'Asignaciones',
        component: () => import('@/views/assignments/AssignmentView.vue'),
        meta: { roles: ['Profesor', 'Superusuario', 'Administrativo'] }
      },
      // Student
      {
        path: 'my-routines',
        name: 'Mis Rutinas',
        component: () => import('@/views/students/MyRoutinesView.vue'),
        meta: { roles: ['Alumno', 'Administrativo'] }
      },
      // Admin
      {
        path: 'users',
        name: 'Usuarios',
        component: () => import('@/views/admin/UserManagementView.vue'),
        meta: { roles: ['Superusuario', 'Administrativo'] }
      },
      {
        path: 'gyms',
        name: 'Gimnasios',
        component: () => import('@/views/admin/GymsView.vue'),
        meta: { roles: ['Superusuario'] }
      },
      {
        path: 'membership-plans',
        name: 'Planes',
        component: () => import('@/views/admin/MembershipPlansView.vue'),
        meta: { roles: ['Superusuario', 'Administrativo'] }
      },
      {
        path: 'memberships',
        name: 'Membresías',
        component: () => import('@/views/admin/MembershipsView.vue'),
        meta: { roles: ['Superusuario', 'Administrativo'] }
      },
      {
        path: 'payments',
        name: 'Pagos',
        component: () => import('@/views/admin/PaymentsView.vue'),
        meta: { roles: ['Superusuario', 'Administrativo'] }
      },
      {
        path: 'my-membership',
        name: 'Mi Membresía',
        component: () => import('@/views/students/MyMembershipView.vue'),
        meta: { roles: ['Alumno'] }
      },
      {
        path: 'estadisticas/asistencia',
        name: 'Flujo de Asistencia',
        component: () => import('@/views/admin/estadisticas/AsistenciaEstadisticasView.vue'),
        meta: { roles: ['Superusuario', 'Administrativo'] }
      },
      {
        path: 'estadisticas/ventas',
        name: 'Ventas y Membresías',
        component: () => import('@/views/admin/estadisticas/VentasEstadisticasView.vue'),
        meta: { roles: ['Superusuario', 'Administrativo'] }
      },
      {
        path: 'estadisticas/retencion',
        name: 'Retención y Deserción',
        component: () => import('@/views/admin/estadisticas/RetencionEstadisticasView.vue'),
        meta: { roles: ['Superusuario', 'Administrativo'] }
      }
    ]
  },
  {
    path: '/:pathMatch(.*)*',
    redirect: '/'
  }
]

const router = createRouter({
  history: createWebHistory(),
  routes
})

router.beforeEach((to, from, next) => {
  const authStore = useAuthStore()
  const needsGymSelection = authStore.pendingGymSelection || (authStore.isAuthenticated && !authStore.hasSelectedGym)
  
  // Force logout if requested by URL
  if (to.query.forceLogout === 'true') {
    authStore.logout()
    return next({ path: '/login', query: {}, replace: true })
  }

  if (to.path === '/select-gym') {
    if (!authStore.isAuthenticated) {
      return next('/login')
    }

    if (!needsGymSelection) {
      return next('/')
    }

    return next()
  }

  // Public routes
  if (to.meta.requiresAuth === false) {
    if (authStore.isAuthenticated) {
      return needsGymSelection ? next('/select-gym') : next('/')
    }
    return next()
  }

  // Auth required
  if (to.meta.requiresAuth !== false && !authStore.isAuthenticated) {
    return next('/login')
  }

  if (needsGymSelection) {
    return next('/select-gym')
  }

  // Role check
  if (to.meta.roles && to.meta.roles.length > 0) {
    if (!authStore.hasRole(...to.meta.roles)) {
      return next('/')
    }
  }

  if (authStore.hasRole('Terminal') && to.path !== '/') {
    return next('/')
  }

  // Check if routines/exercises/assignments routes are requested but veRutinas is disabled
  if (['exercises', 'routines', 'assignments'].some(path => to.path.includes(path))) {
    if (authStore.user?.rol !== 'Superusuario' && authStore.user?.gymVeRutinas === false) {
      return next('/')
    }
  }

  next()
})

export default router
