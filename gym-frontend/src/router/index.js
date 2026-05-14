import { createRouter, createWebHistory } from 'vue-router'
import { useAuthStore } from '@/stores/auth.store'

const routes = [
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
        name: 'Detalle de tu Rutina',
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
  
  // Force logout if requested by URL
  if (to.query.forceLogout === 'true') {
    authStore.logout()
    return next({ path: '/login', query: {}, replace: true })
  }

  // Public routes
  if (to.meta.requiresAuth === false) {
    if (authStore.isAuthenticated) {
      return next('/')
    }
    return next()
  }

  // Auth required
  if (to.meta.requiresAuth !== false && !authStore.isAuthenticated) {
    return next('/login')
  }

  // Role check
  if (to.meta.roles && to.meta.roles.length > 0) {
    if (!authStore.hasRole(...to.meta.roles)) {
      return next('/')
    }
  }

  next()
})

export default router
