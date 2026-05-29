<template>
  <div class="animate-fade-in flex-1 flex flex-col h-full">
    <div v-if="authStore.hasRole('Terminal') || isTerminalRoute" class="flex-1 flex flex-col items-center w-full relative">
      <!-- Header -->
      <div class="w-full max-w-3xl text-center mb-6 sm:mb-8">
        <div class="w-12 h-12 sm:w-14 sm:h-14 mx-auto mb-4 rounded-2xl bg-dark-900/60 border border-dark-800/50 flex items-center justify-center">
          <svg class="w-6 h-6 sm:w-7 sm:h-7 text-primary-500" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="1.5">
            <path
              stroke-linecap="round"
              stroke-linejoin="round"
              d="M15 9h3.75M15 12h3.75M15 15h3.75M4.5 19.5h15a2.25 2.25 0 002.25-2.25V6.75A2.25 2.25 0 0019.5 4.5h-15a2.25 2.25 0 00-2.25 2.25v10.5A2.25 2.25 0 004.5 19.5zm6-10.125a1.875 1.875 0 11-3.75 0 1.875 1.875 0 013.75 0zm1.294 6.336a6.721 6.721 0 01-3.17.789 6.721 6.721 0 01-3.168-.789 3.376 3.376 0 016.338 0z"
            />
          </svg>
        </div>
        <h2 class="text-base sm:text-lg font-black tracking-wide text-white">Ingresá tu DNI</h2>
        <p class="text-xs sm:text-sm text-dark-500 mt-1.5">Escaneá o escribí y presioná <span class="font-black text-white">ENTER</span></p>
      </div>

      <!-- Input -->
      <div class="w-full max-w-3xl">
        <form @submit.prevent="registrarIngreso" class="space-y-4">
          <div
            class="flex items-center gap-3 sm:gap-4 px-4 py-3.5 sm:px-6 sm:py-5 rounded-2xl border-2 bg-[#0a0a0a]/70 backdrop-blur-md transition-all duration-300"
            :class="inputFocused ? 'border-primary-500/80 shadow-[0_0_30px_rgba(255,102,0,0.12)]' : 'border-dark-800/60'"
          >
            <svg class="w-7 h-7 sm:w-9 sm:h-9 text-dark-600 flex-shrink-0" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="1.5">
              <path
                stroke-linecap="round"
                stroke-linejoin="round"
                d="M15 9h3.75M15 12h3.75M15 15h3.75M4.5 19.5h15a2.25 2.25 0 002.25-2.25V6.75A2.25 2.25 0 0019.5 4.5h-15a2.25 2.25 0 00-2.25 2.25v10.5A2.25 2.25 0 004.5 19.5zm6-10.125a1.875 1.875 0 11-3.75 0 1.875 1.875 0 013.75 0zm1.294 6.336a6.721 6.721 0 01-3.17.789 6.721 6.721 0 01-3.168-.789 3.376 3.376 0 016.338 0z"
              />
            </svg>
            <input
              ref="dniInput"
              v-model="terminalDni"
              type="text"
              class="w-full bg-transparent border-none text-white text-3xl sm:text-6xl font-black text-center tracking-[0.15em] focus:ring-0 focus:outline-none placeholder-dark-800 p-0"
              placeholder="00000000"
              autocomplete="off"
              @focus="inputFocused = true"
              @blur="inputFocused = false"
            />
          </div>
          <button type="submit" class="hidden" :disabled="terminalLoading || !terminalDni.trim()"></button>
        </form>

        <!-- Tip -->
        <div class="flex items-center justify-center gap-1.5 text-[10px] sm:text-xs text-dark-500 mt-4 sm:mt-5 tracking-wide">
          <svg class="w-3.5 h-3.5 sm:w-4 sm:h-4 text-dark-600" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2">
            <path stroke-linecap="round" stroke-linejoin="round" d="M3 10h18M7 15h1m4 0h1m-7 4h12a3 3 0 003-3V8a3 3 0 00-3-3H6a3 3 0 00-3 3v8a3 3 0 003 3z" />
          </svg>
          Presioná <span class="font-black text-primary-500 mx-0.5">ENTER</span> para validar
        </div>
      </div>
      <div class="flex-1 w-full max-w-3xl">
        <!-- Result Card -->
        <Transition name="fade-slide">
          <div v-if="terminalMessage" :key="terminalMessage" class="mt-6 sm:mt-8 animate-fade-in">
            <!-- Success Card -->
            <div
              v-if="terminalSuccess && terminalData"
              class="rounded-2xl overflow-hidden border backdrop-blur-md"
              :class="terminalData.diasRestantes <= 7 ? 'border-amber-500/25 bg-[#1a1400]/60' : 'border-emerald-500/20 bg-[#0a1a0f]/60'"
            >
              <div class="p-5 sm:p-6">
                <div class="flex flex-col sm:flex-row sm:items-start sm:justify-between gap-4">
                  <!-- Left: Status + Name + Plan -->
                  <div class="flex-1 min-w-0">
                    <div class="flex items-center gap-2.5 mb-3">
                      <div class="w-7 h-7 rounded-full bg-emerald-500/20 flex items-center justify-center flex-shrink-0">
                        <svg class="w-4 h-4 text-emerald-400" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2.5">
                          <path stroke-linecap="round" stroke-linejoin="round" d="M5 13l4 4L19 7" />
                        </svg>
                      </div>
                      <span class="text-xs sm:text-sm font-black uppercase tracking-[0.15em] text-emerald-400">ACCESO AUTORIZADO</span>
                    </div>
                    <h3 class="text-lg sm:text-xl font-black text-white mb-2.5 ml-0.5 truncate">{{ terminalData.nombre }}</h3>
                    <div class="space-y-1.5 ml-0.5">
                      <div class="flex items-center gap-2 text-xs text-dark-400">
                        <span>{{ terminalData.tipoMembresia }}</span>
                      </div>
                    </div>
                  </div>
                  <!-- Right: Time / Date / Days -->
                  <div class="flex flex-row sm:flex-col gap-5 sm:gap-3 sm:text-right sm:items-end flex-shrink-0">
                    <div>
                      <p class="text-[10px] text-dark-500 uppercase tracking-wider font-bold mb-0.5">Ingreso</p>
                      <p class="text-sm sm:text-base font-black text-primary-500">{{ terminalData.hora }}</p>
                    </div>
                    <div>
                      <p class="text-[10px] text-dark-500 uppercase tracking-wider font-bold mb-0.5">Fecha</p>
                      <p class="text-sm sm:text-base font-black text-white">{{ terminalData.fecha }}</p>
                    </div>
                    <div>
                      <p class="text-[10px] text-dark-500 uppercase tracking-wider font-bold mb-0.5">Vence en</p>
                      <p class="text-sm sm:text-base font-black" :class="terminalData.diasRestantes <= 7 ? 'text-amber-400' : 'text-emerald-400'">
                        {{ terminalData.diasRestantes }} días
                      </p>
                    </div>
                  </div>
                </div>
              </div>
            </div>

            <!-- Error Card -->
            <div v-else class="rounded-2xl border border-red-500/25 bg-red-500/5 backdrop-blur-md p-5 sm:p-6">
              <div class="flex items-center gap-2.5">
                <div class="w-7 h-7 rounded-full bg-red-500/20 flex items-center justify-center flex-shrink-0">
                  <svg class="w-4 h-4 text-red-400" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2.5">
                    <path stroke-linecap="round" stroke-linejoin="round" d="M6 18L18 6M6 6l12 12" />
                  </svg>
                </div>
                <span class="text-sm font-black text-red-400 uppercase tracking-wide">{{ terminalMessage }}</span>
              </div>
            </div>
          </div>
        </Transition>
      </div>

      <!-- Footer Status Bar -->
      <div class="absolute bottom-4 left-0 right-0 flex items-center justify-center gap-2 text-[10px] text-dark-600 tracking-wider">
        <span class="w-1.5 h-1.5 rounded-full bg-emerald-500 animate-pulse"></span>
        Sistema en línea &bull; {{ terminalName }}
      </div>
    </div>

    <template v-else>
      <div class="mb-8 flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
        <div>
          <h1 class="page-title">Bienvenido, {{ user?.nombre }}</h1>
          <p class="page-subtitle">{{ greetingMessage }}</p>
        </div>
        <div v-if="authStore.hasRole('Administrativo', 'Superusuario')">
          <a
            href="/terminal"
            target="_blank"
            class="px-4 py-2 rounded-xl bg-primary-500 hover:bg-primary-600 text-xs font-black text-white uppercase transition-colors flex items-center gap-2"
          >
            <svg class="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path
                stroke-linecap="round"
                stroke-linejoin="round"
                stroke-width="2"
                d="M10 6H6a2 2 0 00-2 2v10a2 2 0 002 2h10a2 2 0 002-2v-4M14 4h6m0 0v6m0-6L10 14"
              />
            </svg>
            Abrir Terminal
          </a>
        </div>
      </div>
      <!-- 
      <div v-if="authStore.hasRole('Profesor', 'Superusuario', 'Administrativo')" class="grid grid-cols-2 lg:grid-cols-4 gap-3 mb-8">
        <router-link
          v-for="stat in stats"
          :key="stat.label"
          :to="stat.path || '/'"
          class="p-5 rounded-[2rem] bg-dark-900/40 border border-dark-800/50 backdrop-blur-md group hover:border-primary-500/30 hover:bg-dark-800/60 active:scale-[0.98] transition-all duration-300 relative overflow-hidden"
        >
          <div class="flex flex-col gap-4 relative z-10">
            <div
              class="w-12 h-12 rounded-2xl flex items-center justify-center text-xl transition-transform duration-500 group-hover:scale-110"
              :class="stat.bgColor"
            >
              {{ stat.icon }}
            </div>
            <div>
              <div class="flex items-baseline gap-2">
                <p class="text-3xl font-black text-white leading-none tracking-tight">{{ stat.value }}</p>
              </div>
              <p class="text-[11px] text-dark-400 uppercase tracking-[0.2em] font-bold mt-2 group-hover:text-dark-200 transition-colors">{{ stat.label }}</p>
            </div>
          </div>
        </router-link>
      </div> -->

      <div v-if="authStore.hasRole('Profesor', 'Superusuario', 'Administrativo')" class="mb-8">
        <h2 class="text-sm font-bold text-dark-500 uppercase tracking-[0.2em] mb-4 ml-1">Accesos Rápidos</h2>
        <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
          <router-link
            to="/assignments"
            class="group flex items-center p-4 rounded-2xl bg-dark-900/40 border border-dark-800/50 hover:bg-dark-800/60 hover:border-primary-500/30 transition-all duration-300"
          >
            <div class="w-10 h-10 rounded-xl bg-emerald-500/10 text-emerald-500 flex items-center justify-center mr-4">
              <svg class="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 4v16m8-8H4" />
              </svg>
            </div>
            <div>
              <h3 class="text-sm font-bold text-white">Asignar Rutina</h3>
              <p class="text-xs text-dark-500">Vincular ejercicios a alumnos</p>
            </div>
          </router-link>

          <router-link
            to="/routines/new"
            class="group flex items-center p-4 rounded-2xl bg-dark-900/40 border border-dark-800/50 hover:bg-dark-800/60 hover:border-primary-500/30 transition-all duration-300"
          >
            <div class="w-10 h-10 rounded-xl bg-blue-500/10 text-blue-500 flex items-center justify-center mr-4">
              <svg class="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path
                  stroke-linecap="round"
                  stroke-linejoin="round"
                  stroke-width="2"
                  d="M9 13h6m-3-3v6m5 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z"
                />
              </svg>
            </div>
            <div>
              <h3 class="text-sm font-bold text-white">Nueva Rutina</h3>
              <p class="text-xs text-dark-500">Crear plan de entrenamiento</p>
            </div>
          </router-link>

          <router-link
            v-if="authStore.hasRole('Superusuario', 'Administrativo')"
            to="/memberships"
            class="group flex items-center p-4 rounded-2xl bg-dark-900/40 border border-dark-800/50 hover:bg-dark-800/60 hover:border-primary-500/30 transition-all duration-300"
          >
            <div class="w-10 h-10 rounded-xl bg-amber-500/10 text-amber-500 flex items-center justify-center mr-4">
              <svg class="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2">
                <path
                  stroke-linecap="round"
                  stroke-linejoin="round"
                  d="M15 9h3.75M15 12h3.75M15 15h3.75M4.5 19.5h15a2.25 2.25 0 002.25-2.25V6.75A2.25 2.25 0 0019.5 4.5h-15a2.25 2.25 0 00-2.25 2.25v10.5A2.25 2.25 0 004.5 19.5zm6-10.125a1.875 1.875 0 11-3.75 0 1.875 1.875 0 013.75 0zm1.294 6.336a6.721 6.721 0 01-3.17.789 6.721 6.721 0 01-3.168-.789 3.376 3.376 0 016.338 0z"
                />
              </svg>
            </div>
            <div>
              <h3 class="text-sm font-bold text-white">Membresías</h3>
              <p class="text-xs text-dark-500">Alumnos y pagos</p>
            </div>
          </router-link>

          <router-link
            v-if="authStore.hasRole('Superusuario', 'Administrativo')"
            to="/ingresos"
            class="group flex items-center p-4 rounded-2xl bg-dark-900/40 border border-dark-800/50 hover:bg-dark-800/60 hover:border-primary-500/30 transition-all duration-300"
          >
            <div class="w-10 h-10 rounded-xl bg-rose-500/10 text-rose-400 flex items-center justify-center mr-4">
              <svg class="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8v4l3 3M12 3a9 9 0 100 18 9 9 0 000-18z" />
              </svg>
            </div>
            <div>
              <h3 class="text-sm font-bold text-white">Ingresos</h3>
              <p class="text-xs text-dark-500">Accesos registrados</p>
            </div>
          </router-link>
        </div>
      </div>

      <!-- Monitoreo en Tiempo Real (Solo Administrador y Superusuario) -->
      <div v-if="authStore.hasRole('Superusuario', 'Administrativo')" class="grid grid-cols-1 lg:grid-cols-3 gap-6 mb-8">
        <!-- Col 1 y 2: Ingresos Recientes -->
        <div class="card p-6 lg:col-span-2 flex flex-col h-[320px]">
          <div class="flex items-center justify-between mb-4">
            <div>
              <h2 class="text-sm font-bold text-white uppercase tracking-wider">Asistencia de Hoy</h2>
              <p class="text-xs text-dark-500">Ingresos recientes a través de la terminal</p>
            </div>
            <button @click="fetchIngresosHoy" class="p-2 rounded-lg text-dark-400 hover:text-white hover:bg-dark-900/60 transition-colors">
              <svg class="w-4 h-4" :class="{ 'animate-spin': loadingIngresos }" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2">
                <path
                  stroke-linecap="round"
                  stroke-linejoin="round"
                  d="M16.023 9.348h4.992v-.001M2.985 19.644v-4.992m0 0h4.992m-4.993 0 3.181 3.183a8.25 8.25 0 0 0 13.803-3.7M4.031 9.865a8.25 8.25 0 0 1 13.803-3.7l3.181 3.182m0-4.991v4.99"
                />
              </svg>
            </button>
          </div>

          <div class="flex-1 overflow-y-auto custom-scrollbar space-y-2.5">
            <div
              v-for="ingreso in ingresosHoy.slice(0, 5)"
              :key="ingreso.id"
              class="flex items-center justify-between p-3.5 rounded-2xl bg-dark-950/40 border border-dark-900/60 hover:border-primary-500/10 transition-all duration-300"
            >
              <div class="flex items-center gap-3">
                <div class="w-8 h-8 rounded-full bg-primary-500/10 text-primary-500 flex items-center justify-center text-xs font-black">
                  {{ ingreso.alumno?.charAt(0)?.toUpperCase() }}
                </div>
                <div>
                  <p class="text-xs font-black text-white leading-none mb-1">{{ ingreso.alumno }}</p>
                  <p class="text-[10px] text-dark-500">Plan: {{ ingreso.tipoMembresia }}</p>
                </div>
              </div>
              <div class="text-right">
                <p class="text-xs font-black text-white">{{ formatTime(ingreso.fechaHora) }}</p>
                <p class="text-[9px] text-dark-500 uppercase tracking-widest leading-none mt-0.5">{{ ingreso.terminal }}</p>
              </div>
            </div>

            <div v-if="!ingresosHoy.length" class="h-full flex flex-col items-center justify-center text-center py-6">
              <svg class="w-8 h-8 text-dark-700 mb-2" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="1.5">
                <path
                  stroke-linecap="round"
                  stroke-linejoin="round"
                  d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z"
                />
              </svg>
              <p class="text-xs text-dark-500 font-bold">Sin ingresos registrados hoy aún.</p>
            </div>
          </div>
        </div>

        <!-- Col 3: Panel Comercial Rápido (Recaudación + Alertas por Vencer) -->
        <div class="flex flex-col gap-4 h-[320px]">
          <!-- Card Recaudación del Mes -->
          <!-- <div class="p-5 rounded-[2rem] bg-dark-900/40 border border-dark-800/50 backdrop-blur-md relative overflow-hidden group shrink-0">
            <div
              class="absolute -right-4 -bottom-4 w-20 h-20 bg-emerald-500/5 rounded-full blur-xl group-hover:bg-emerald-500/10 transition-all duration-500"
            ></div>
            <p class="text-[9px] text-dark-500 uppercase tracking-[0.2em] font-black">Recaudación este Mes</p>
            <p class="text-2xl font-black text-emerald-400 mt-2 tracking-tight leading-none">
              {{ formatMoneda(pagosMesVal) }}
            </p>
          </div> -->

          <!-- Card Membresías por Vencer -->
          <div class="flex-1 card p-5 flex flex-col overflow-hidden min-h-[170px]">
            <h3 class="text-xs font-bold text-white uppercase tracking-wider mb-3">Expira en <span class="text-primary-500 font-black">7 días</span></h3>

            <div class="flex-1 overflow-y-auto custom-scrollbar space-y-2">
              <div
                v-for="membresia in membresiasPorVencer.slice(0, 3)"
                :key="membresia.id"
                class="flex items-center justify-between p-2 rounded-xl bg-dark-950/40 border border-dark-900/60"
              >
                <div class="min-w-0">
                  <p class="text-[11px] font-black text-white truncate leading-none mb-1">{{ membresia.alumnoNombreCompleto }}</p>
                  <p class="text-[9px] text-dark-500">Vence: {{ formatDate(membresia.fechaVencimiento) }}</p>
                </div>
                <button @click="openRenewModal(membresia)" class="text-[9px] font-black text-primary-500 hover:text-primary-400 uppercase tracking-widest flex-shrink-0">
                  Renovar
                </button>
              </div>

              <div v-if="!membresiasPorVencer.length" class="h-full flex flex-col items-center justify-center text-center py-4">
                <p class="text-[10px] text-dark-500 font-bold">No hay membresías por vencer próximamente.</p>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- Alumnos con membresía vencida (último mes, sin membresía activa) -->
      <div v-if="authStore.hasRole('Superusuario', 'Administrativo')" class="card p-4 sm:p-6 mb-8">
        <div class="flex items-center justify-between mb-4">
          <div>
            <h2 class="text-sm font-bold text-white uppercase tracking-wider">Membresías vencidas</h2>
            <p class="text-xs text-dark-500">Alumnos sin membresía activa que vencieron en el último mes</p>
          </div>
          <button
            type="button"
            class="p-2 rounded-lg text-dark-400 hover:text-white hover:bg-dark-900/60 transition-colors"
            @click="fetchMembresiasVencidas"
          >
            <svg class="w-4 h-4" :class="{ 'animate-spin': loadingVencidas }" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2">
              <path
                stroke-linecap="round"
                stroke-linejoin="round"
                d="M16.023 9.348h4.992v-.001M2.985 19.644v-4.992m0 0h4.992m-4.993 0 3.181 3.183a8.25 8.25 0 0 0 13.803-3.7M4.031 9.865a8.25 8.25 0 0 1 13.803-3.7l3.181 3.182m0-4.991v4.99"
              />
            </svg>
          </button>
        </div>

        <LoadingSpinner v-if="loadingVencidas" />

        <template v-else>
          <!-- Móvil: tarjetas apiladas, sin scroll horizontal -->
          <div
            v-if="alumnosMembresiaVencida.length"
            class="md:hidden max-h-[360px] overflow-y-auto custom-scrollbar space-y-2.5"
          >
            <div
              v-for="m in alumnosMembresiaVencida"
              :key="m.id"
              class="flex items-center gap-3 p-3.5 rounded-2xl bg-dark-950/40 border border-dark-900/60"
            >
              <div class="w-9 h-9 rounded-full bg-red-500/10 text-red-400 flex items-center justify-center text-xs font-black flex-shrink-0">
                {{ m.alumnoNombreCompleto?.charAt(0)?.toUpperCase() }}
              </div>
              <div class="min-w-0 flex-1">
                <p class="text-xs font-black text-white truncate leading-none mb-1">{{ m.alumnoNombreCompleto }}</p>
                <p class="text-[10px] text-dark-500 truncate">
                  {{ m.planNombre }} · venció {{ formatDate(m.fechaVencimiento) }}
                </p>
                <p class="text-[9px] text-dark-600 mt-0.5">Hace {{ diasDesdeVencimiento(m.fechaVencimiento) }} días</p>
              </div>
              <div class="flex items-center gap-1 flex-shrink-0">
                <a
                  v-if="m.whatsappUrl"
                  :href="m.whatsappUrl"
                  target="_blank"
                  rel="noopener noreferrer"
                  class="p-1.5 rounded-lg text-[#25D366] hover:bg-[#25D366]/10 transition-colors"
                  title="Contactar por WhatsApp"
                  aria-label="Contactar por WhatsApp"
                >
                  <svg class="w-4 h-4" viewBox="0 0 24 24" fill="currentColor" aria-hidden="true">
                    <path
                      d="M17.472 14.382c-.297-.149-1.758-.867-2.03-.967-.273-.099-.471-.148-.67.15-.197.297-.767.966-.94 1.164-.173.199-.347.223-.644.075-.297-.15-1.255-.463-2.39-1.475-.883-.788-1.48-1.761-1.653-2.059-.173-.297-.018-.458.13-.606.134-.133.298-.347.446-.52.149-.174.198-.298.298-.497.099-.198.05-.371-.025-.52-.075-.149-.669-1.612-.916-2.207-.242-.579-.487-.5-.669-.51-.173-.008-.371-.01-.57-.01-.198 0-.52.074-.792.372-.272.297-1.04 1.016-1.04 2.479 0 1.462 1.065 2.875 1.213 3.074.149.198 2.096 3.2 5.077 4.487.709.306 1.262.489 1.694.625.712.227 1.36.195 1.871.118.571-.085 1.758-.719 2.006-1.413.248-.694.248-1.289.173-1.413-.074-.124-.272-.198-.57-.347m-5.421 7.403h-.004a9.87 9.87 0 01-5.031-1.378l-.361-.214-3.741.982.998-3.648-.235-.374a9.86 9.86 0 01-1.51-5.26c.001-5.45 4.436-9.884 9.888-9.884 2.64 0 5.122 1.03 6.988 2.898a9.825 9.825 0 012.893 6.994c-.003 5.45-4.435 9.884-9.885 9.884m8.413-18.297A11.815 11.815 0 0012.05 0C5.495 0 .16 5.335.157 11.892c0 2.096.547 4.142 1.588 5.945L.057 24l6.305-1.654a11.882 11.882 0 005.683 1.448h.005c6.554 0 11.89-5.335 11.893-11.893a11.821 11.821 0 00-3.48-8.413z"
                    />
                  </svg>
                </a>
                <button
                  type="button"
                  class="text-[9px] font-black text-primary-500 hover:text-primary-400 uppercase tracking-widest px-2 py-1"
                  @click="openRenewModal(m)"
                >
                  Renovar
                </button>
              </div>
            </div>
          </div>

          <!-- Escritorio: tabla -->
          <div
            v-if="alumnosMembresiaVencida.length"
            class="hidden md:block table-container max-h-[360px] overflow-y-auto custom-scrollbar"
          >
            <table class="table">
              <thead class="sticky top-0 z-10 bg-dark-950">
                <tr>
                  <th>Alumno</th>
                  <th>Plan</th>
                  <th>Vencimiento</th>
                  <th>Hace</th>
                  <th class="text-right">Acciones</th>
                </tr>
              </thead>
              <tbody>
                <tr v-for="m in alumnosMembresiaVencida" :key="m.id">
                  <td class="font-medium text-white">{{ m.alumnoNombreCompleto }}</td>
                  <td>{{ m.planNombre }}</td>
                  <td>{{ formatDate(m.fechaVencimiento) }}</td>
                  <td class="text-dark-400">{{ diasDesdeVencimiento(m.fechaVencimiento) }} días</td>
                  <td class="text-right">
                    <div class="flex items-center justify-end gap-2">
                      <a
                        v-if="m.whatsappUrl"
                        :href="m.whatsappUrl"
                        target="_blank"
                        rel="noopener noreferrer"
                        class="p-1.5 rounded-lg text-[#25D366] hover:bg-[#25D366]/10 transition-colors inline-flex"
                        title="Contactar por WhatsApp"
                        aria-label="Contactar por WhatsApp"
                      >
                        <svg class="w-4 h-4" viewBox="0 0 24 24" fill="currentColor" aria-hidden="true">
                          <path
                            d="M17.472 14.382c-.297-.149-1.758-.867-2.03-.967-.273-.099-.471-.148-.67.15-.197.297-.767.966-.94 1.164-.173.199-.347.223-.644.075-.297-.15-1.255-.463-2.39-1.475-.883-.788-1.48-1.761-1.653-2.059-.173-.297-.018-.458.13-.606.134-.133.298-.347.446-.52.149-.174.198-.298.298-.497.099-.198.05-.371-.025-.52-.075-.149-.669-1.612-.916-2.207-.242-.579-.487-.5-.669-.51-.173-.008-.371-.01-.57-.01-.198 0-.52.074-.792.372-.272.297-1.04 1.016-1.04 2.479 0 1.462 1.065 2.875 1.213 3.074.149.198 2.096 3.2 5.077 4.487.709.306 1.262.489 1.694.625.712.227 1.36.195 1.871.118.571-.085 1.758-.719 2.006-1.413.248-.694.248-1.289.173-1.413-.074-.124-.272-.198-.57-.347m-5.421 7.403h-.004a9.87 9.87 0 01-5.031-1.378l-.361-.214-3.741.982.998-3.648-.235-.374a9.86 9.86 0 01-1.51-5.26c.001-5.45 4.436-9.884 9.888-9.884 2.64 0 5.122 1.03 6.988 2.898a9.825 9.825 0 012.893 6.994c-.003 5.45-4.435 9.884-9.885 9.884m8.413-18.297A11.815 11.815 0 0012.05 0C5.495 0 .16 5.335.157 11.892c0 2.096.547 4.142 1.588 5.945L.057 24l6.305-1.654a11.882 11.882 0 005.683 1.448h.005c6.554 0 11.89-5.335 11.893-11.893a11.821 11.821 0 00-3.48-8.413z"
                          />
                        </svg>
                      </a>
                      <button
                        type="button"
                        class="text-[10px] font-black text-primary-500 hover:text-primary-400 uppercase tracking-widest"
                        @click="openRenewModal(m)"
                      >
                        Renovar
                      </button>
                    </div>
                  </td>
                </tr>
              </tbody>
            </table>
          </div>

          <div v-else class="py-8 text-center">
            <p class="text-xs text-dark-500 font-bold">No hay alumnos con membresía vencida en el último mes.</p>
          </div>
        </template>
      </div>

      <div v-if="authStore.hasRole('Alumno')" class="space-y-8">
        <router-link v-if="myAccess" to="/my-membership" class="card p-5 max-w-2xl block hover:border-primary-500/30 transition-colors">
          <div class="flex items-start justify-between gap-4">
            <div>
              <p class="text-xs uppercase tracking-widest text-dark-500 font-bold mb-2">Tu membresía</p>
              <span :class="accessStatusBadgeClass(myAccess.estadoAcceso)" class="text-sm px-3 py-1">{{ myAccess.estadoAcceso }}</span>
              <p v-if="myAccess.planNombre" class="text-sm text-dark-400 mt-3">{{ myAccess.planNombre }} · vence {{ formatDate(myAccess.fechaVencimiento) }}</p>
            </div>
          </div>
        </router-link>

        <!-- Sección de Rutinas (Solo si el gimnasio lo permite) -->
        <div v-if="veRutinas" class="max-w-2xl space-y-4">
          <h2 class="text-sm font-bold text-dark-500 uppercase tracking-[0.2em] ml-1">Tus Rutinas de Entrenamiento</h2>

          <!-- Si tiene membresía activa y tiene rutinas asignadas -->
          <div v-if="myAccess?.estadoAcceso === 'Activo' && myRoutines.length > 0" class="grid grid-cols-1 gap-3">
            <router-link
              v-for="rutina in myRoutines"
              :key="rutina.id"
              to="/my-routines"
              class="group flex items-center justify-between p-5 rounded-2xl bg-dark-900/40 border border-dark-800/50 hover:bg-dark-800/60 hover:border-primary-500/30 transition-all duration-300"
            >
              <div class="flex items-center gap-4">
                <div class="w-12 h-12 rounded-xl bg-primary-500/10 text-primary-500 flex items-center justify-center text-xl">🏋️</div>
                <div>
                  <h3 class="text-sm font-bold text-white group-hover:text-primary-500 transition-colors">{{ rutina.nombre }}</h3>
                  <p class="text-xs text-dark-500 mt-1">Prof: {{ rutina.profesorNombre }}</p>
                </div>
              </div>
              <svg
                class="w-5 h-5 text-dark-500 group-hover:text-primary-500 transition-colors"
                fill="none"
                viewBox="0 0 24 24"
                stroke="currentColor"
                stroke-width="2"
              >
                <path stroke-linecap="round" stroke-linejoin="round" d="M9 5l7 7-7 7" />
              </svg>
            </router-link>
          </div>

          <!-- Si tiene membresía activa pero no tiene rutinas asignadas -->
          <div
            v-else-if="myAccess?.estadoAcceso === 'Activo' && myRoutines.length === 0"
            class="p-5 rounded-2xl border border-amber-500/20 bg-amber-500/5 text-amber-400 flex flex-col sm:flex-row items-center gap-4"
          >
            <div class="w-10 h-10 rounded-full bg-amber-500/10 text-amber-500 flex items-center justify-center text-lg flex-shrink-0">⚠️</div>
            <div>
              <p class="text-sm font-bold">No tenés rutinas de entrenamiento asignadas</p>
              <p class="text-xs text-dark-400 mt-0.5">Pedile a tu profesor que te asigne una rutina para comenzar.</p>
            </div>
          </div>
        </div>
      </div>
      <!-- Modales de Renovación y Pago -->
      <AppModal v-model="showRenewModal" title="Renovar membresía">
        <form class="space-y-4" @submit.prevent="handleRenew">
          <p class="text-sm text-dark-400">Alumno: <strong class="text-white">{{ renewingMembership?.alumnoNombreCompleto }}</strong></p>
          <label class="label">Plan</label>
          <AppSearchSelect
            v-model.number="renewForm.planId"
            :options="planOptions"
            placeholder="Seleccionar plan"
          />
          <AppInput v-model="renewForm.fechaInicio" label="Fecha de inicio (opcional)" type="date" />
          <textarea v-model="renewForm.notas" rows="2" class="input" placeholder="Notas" />
        </form>
        <template #footer>
          <AppButton variant="secondary" @click="showRenewModal = false">Cancelar</AppButton>
          <div class="flex gap-2">
            <AppButton :loading="saving" @click="handleRenew(false)">Renovar</AppButton>
            <AppButton :loading="saving" variant="primary" @click="handleRenew(true)">Renovar y Pagar</AppButton>
          </div>
        </template>
      </AppModal>

      <AppModal v-model="showPaymentModal" title="Registrar pago">
        <form class="space-y-4" @submit.prevent="handlePaymentSubmit">
          <div class="grid grid-cols-2 gap-4">
            <AppInput v-model="paymentForm.alumnoNombre" label="Alumno" disabled />
            <AppInput v-model="paymentForm.planNombre" label="Plan" disabled />
          </div>
          <div class="grid grid-cols-2 gap-4">
            <AppInput v-model="paymentForm.monto" label="Precio" type="text" @input="onMontoInput" />
            <AppInput v-model="paymentForm.fechaPago" label="Fecha de pago" type="datetime-local" />
          </div>
          <div class="grid grid-cols-2 gap-4">
            <div>
              <label class="label">Método de pago</label>
              <AppSearchSelect
                v-model="paymentForm.metodoPago"
                :options="metodoPagoOptions"
                placeholder="Método de pago"
              />
            </div>
            <div>
              <label class="label">Estado</label>
              <AppSearchSelect
                v-model="paymentForm.estado"
                :options="paymentEstadoOptions"
                placeholder="Estado"
              />
            </div>
          </div>
          <textarea v-model="paymentForm.notas" rows="2" class="input" placeholder="Notas opcionales" />
        </form>
        <template #footer>
          <AppButton variant="secondary" @click="showPaymentModal = false">Cancelar</AppButton>
          <AppButton :loading="saving" @click="handlePaymentSubmit">Guardar Pago</AppButton>
        </template>
      </AppModal>
    </template>
  </div>
</template>

<script setup>
import { computed, nextTick, onMounted, ref, reactive } from 'vue';
import { useRoute } from 'vue-router';
import { useAuthStore } from '@/stores/auth.store';
import { useRoutineStore } from '@/stores/routine.store';
import { useUserStore } from '@/stores/user.store';
import { useMembershipStore } from '@/stores/membership.store';
import { accessStatusBadgeClass, formatDate, formatCurrency, PAYMENT_ESTADOS } from '@/constants/membershipStatus';
import { ingresosApi } from '@/api/ingresos.api';
import { membershipsApi } from '@/api/memberships.api';
import { paymentsApi } from '@/api/payments.api';
import AppButton from '@/components/ui/AppButton.vue';
import LoadingSpinner from '@/components/shared/LoadingSpinner.vue';
import AppInput from '@/components/ui/AppInput.vue';
import AppModal from '@/components/ui/AppModal.vue';
import AppSearchSelect from '@/components/ui/AppSearchSelect.vue';
import { useNotification } from '@/composables/useNotification';
import { buildMembresiaVencidaWhatsAppMessage, buildWhatsAppUrl } from '@/utils/whatsapp';

const route = useRoute();
const isTerminalRoute = computed(() => route.path === '/terminal');
const authStore = useAuthStore();
const routineStore = useRoutineStore();
const userStore = useUserStore();
const membershipStore = useMembershipStore();
const myAccess = computed(() => membershipStore.myAccess);
const user = computed(() => authStore.user);
const assignmentSummary = computed(() => routineStore.assignmentSummary);
const myRoutines = computed(() => routineStore.myRoutines);
const veRutinas = computed(() => authStore.user?.gymVeRutinas !== false);
const dniInput = ref(null);
const terminalDni = ref('');
const terminalLoading = ref(false);
const terminalMessage = ref('');
const terminalSuccess = ref(false);
const terminalData = ref(null);
let terminalTimeout = null;
const inputFocused = ref(false);
const terminalName = computed(() => {
  if (authStore.hasRole('Terminal')) return `Terminal · ${authStore.user?.nombre || 'Terminal'}`;
  return 'Terminal 01';
});

const ingresosHoy = ref([]);
const loadingIngresos = ref(false);
const pagosMesVal = ref(0);
const membershipsVencidasRaw = ref([]);
const loadingVencidas = ref(false);

const greetingMessage = computed(() => {
  const role = user.value?.rol;
  if (role === 'Superusuario') return 'Panel de administracion del gimnasio';
  if (role === 'Administrativo') return 'Gestion de gimnasio, profesores y alumnos';
  if (role === 'Profesor') return 'Gestion de rutinas y alumnos';
  if (role === 'Terminal') return 'Registro de asistencia del gimnasio';
  return 'Revisa tus rutinas de entrenamiento';
});

const stats = ref([
  { icon: '🏋️', label: 'Ejercicios', value: '-', bgColor: 'bg-primary-500/10 text-primary-400', path: '/exercises' },
  { icon: '📋', label: 'Rutinas', value: '-', bgColor: 'bg-primary-600/10 text-primary-400', path: '/routines' },
]);

if (authStore.hasRole('Superusuario', 'Administrativo')) {
  stats.value.push(
    { icon: '👥', label: 'Alumnos', value: '-', bgColor: 'bg-primary-700/10 text-primary-400', path: '/users' },
    { icon: '🪪', label: 'Membresías activas', value: '-', bgColor: 'bg-amber-500/10 text-amber-400', path: '/memberships' }
  );
}

function updateDashboardStats() {
  stats.value[0].value = assignmentSummary.value.ejerciciosCount || routineStore.exercises.length;
  stats.value[1].value = assignmentSummary.value.rutinasCount || routineStore.routines.length;
  if (authStore.hasRole('Superusuario', 'Administrativo') && stats.value.length > 2) {
    stats.value[2].value = userStore.users.filter((x) => x.rol === 'Alumno' && x.activo).length;
    stats.value[3].value = membershipStore.memberships.length;
  }
}

async function registrarIngreso() {
  terminalLoading.value = true;
  terminalMessage.value = '';
  terminalData.value = null;
  if (terminalTimeout) clearTimeout(terminalTimeout);

  try {
    const { data } = await ingresosApi.registrar(terminalDni.value);
    terminalSuccess.value = true;

    const fechaHora = new Date(data.fechaHora);
    const vencimiento = new Date(data.fechaVencimiento);
    const diasRestantes = Math.max(0, Math.ceil((vencimiento - new Date()) / (1000 * 3600 * 24)));

    terminalData.value = {
      nombre: data.alumnoNombreCompleto,
      tipoMembresia: data.tipoMembresia,
      paseLibre: data.paseLibre,
      hora: fechaHora.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit', second: '2-digit' }),
      fecha: fechaHora.toLocaleDateString('es-AR', { day: '2-digit', month: '2-digit', year: 'numeric' }),
      diasRestantes,
    };

    terminalMessage.value = 'ACCESO AUTORIZADO';
    terminalDni.value = '';

    // Refrescar ingresos recientes tras ingreso exitoso en terminal
    if (authStore.hasRole('Superusuario', 'Administrativo')) {
      fetchIngresosHoy();
    }
  } catch (err) {
    terminalSuccess.value = false;
    terminalData.value = null;
    terminalMessage.value = err.response?.data?.error || 'No se pudo registrar el ingreso';
  } finally {
    terminalLoading.value = false;
    await nextTick();
    dniInput.value?.focus();

    terminalTimeout = setTimeout(() => {
      terminalMessage.value = '';
      terminalData.value = null;
    }, 10000);
  }
}

async function fetchIngresosHoy() {
  if (!authStore.hasRole('Superusuario', 'Administrativo')) return;
  loadingIngresos.value = true;
  try {
    const hoy = new Date().toISOString().slice(0, 10);
    const { data } = await ingresosApi.getAll({ fechaDesde: hoy, fechaHasta: hoy });
    ingresosHoy.value = data || [];
  } catch (err) {
    console.error(err);
  } finally {
    loadingIngresos.value = false;
  }
}

async function fetchPagosMes() {
  if (!authStore.hasRole('Superusuario', 'Administrativo')) return;
  try {
    const { data } = await paymentsApi.getAll();
    const hoy = new Date();
    const mes = hoy.getMonth();
    const año = hoy.getFullYear();
    const esteMes = (data || []).filter((p) => {
      const d = new Date(p.fechaPago);
      return d.getMonth() === mes && d.getFullYear() === año;
    });
    pagosMesVal.value = esteMes.reduce((acc, p) => acc + p.monto, 0);
  } catch (err) {
    console.error(err);
  }
}

const membresiasPorVencer = computed(() => {
  if (!authStore.hasRole('Superusuario', 'Administrativo')) return [];
  const hoy = new Date();
  const limite = new Date();
  limite.setDate(hoy.getDate() + 7);
  return membershipStore.memberships
    .filter((m) => {
      if (!m.fechaVencimiento || m.estado !== 'Activa') return false;
      const v = new Date(m.fechaVencimiento);
      return v >= hoy && v <= limite;
    })
    .sort((a, b) => new Date(a.fechaVencimiento) - new Date(b.fechaVencimiento));
});

const gymNombreWhatsApp = computed(
  () => authStore.user?.gymNombre || authStore.selectedGym?.gymNombre || 'Gimnasio'
);

function getAlumnoTelefono(alumnoId) {
  const gymId = authStore.user?.gymId;
  const alumno = userStore.users.find(
    (u) => u.id === alumnoId && u.rol === 'Alumno' && (!gymId || u.gymId === gymId)
  );
  return alumno?.telefono ?? null;
}

function buildWhatsAppUrlForMembresia(m) {
  const telefono = getAlumnoTelefono(m.alumnoId);
  if (!telefono) return null;
  const message = buildMembresiaVencidaWhatsAppMessage(m.alumnoNombreCompleto, gymNombreWhatsApp.value);
  return buildWhatsAppUrl(telefono, message);
}

const alumnosMembresiaVencida = computed(() => {
  if (!authStore.hasRole('Superusuario', 'Administrativo')) return [];

  const hoy = new Date();
  hoy.setHours(23, 59, 59, 999);
  const haceUnMes = new Date();
  haceUnMes.setMonth(haceUnMes.getMonth() - 1);
  haceUnMes.setHours(0, 0, 0, 0);

  const activosPorAlumno = new Set(
    membershipStore.memberships.filter((m) => m.estado === 'Activa').map((m) => m.alumnoId)
  );

  const porAlumno = new Map();
  for (const m of membershipsVencidasRaw.value) {
    if (m.estado !== 'Vencida' || activosPorAlumno.has(m.alumnoId)) continue;
    const v = new Date(m.fechaVencimiento);
    if (v < haceUnMes || v > hoy) continue;

    const prev = porAlumno.get(m.alumnoId);
    if (!prev || new Date(m.fechaVencimiento) > new Date(prev.fechaVencimiento)) {
      porAlumno.set(m.alumnoId, m);
    }
  }

  return Array.from(porAlumno.values())
    .sort((a, b) => new Date(b.fechaVencimiento) - new Date(a.fechaVencimiento))
    .map((m) => ({
      ...m,
      whatsappUrl: buildWhatsAppUrlForMembresia(m),
    }));
});

async function fetchMembresiasVencidas() {
  if (!authStore.hasRole('Superusuario', 'Administrativo')) return;
  loadingVencidas.value = true;
  try {
    const params = { estado: 'Vencida' };
    if (authStore.user?.gymId) params.gymId = authStore.user.gymId;
    const { data } = await membershipsApi.getAll(params);
    membershipsVencidasRaw.value = data || [];
  } catch (err) {
    console.error(err);
  } finally {
    loadingVencidas.value = false;
  }
}

function diasDesdeVencimiento(fechaVencimiento) {
  const v = new Date(fechaVencimiento);
  v.setHours(0, 0, 0, 0);
  const hoy = new Date();
  hoy.setHours(0, 0, 0, 0);
  return Math.max(0, Math.floor((hoy - v) / (1000 * 60 * 60 * 24)));
}

function formatMoneda(val) {
  return new Intl.NumberFormat('es-AR', { style: 'currency', currency: 'ARS', minimumFractionDigits: 0 }).format(val);
}

function formatTime(val) {
  return new Date(val).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
}

onMounted(async () => {
  if (authStore.hasRole('Terminal') || isTerminalRoute.value) {
    await nextTick();
    dniInput.value?.focus();
    return;
  }

  if (authStore.hasRole('Superusuario', 'Administrativo')) {
    await Promise.allSettled([
      routineStore.fetchExercises(),
      routineStore.fetchRoutines(),
      routineStore.fetchAssignmentSummary(),
      userStore.fetchUsers(),
      membershipStore.fetchMemberships({ estado: 'Activa' }),
      membershipStore.fetchPlans(),
      fetchIngresosHoy(),
      fetchPagosMes(),
      fetchMembresiasVencidas(),
    ]);
    updateDashboardStats();
  } else if (authStore.hasRole('Profesor')) {
    await Promise.allSettled([routineStore.fetchExercises(), routineStore.fetchRoutines(), routineStore.fetchAssignmentSummary()]);
    updateDashboardStats();
  } else if (authStore.hasRole('Alumno')) {
    await membershipStore.fetchMyAccess();
    if (veRutinas.value) {
      await routineStore.fetchMyRoutines();
    }
  }
});

const { success, error: showError } = useNotification();
const showRenewModal = ref(false);
const showPaymentModal = ref(false);
const renewingMembership = ref(null);
const saving = ref(false);

const renewForm = reactive({ planId: 0, fechaInicio: '', notas: '' });

const paymentForm = reactive({
  membresiaId: 0,
  alumnoNombre: '',
  planNombre: '',
  monto: '',
  fechaPago: '',
  metodoPago: 'Efectivo',
  estado: 'Completado',
  notas: ''
});

const activePlans = computed(() => membershipStore.plans.filter(p => p.activo));
const planOptions = computed(() => {
  return activePlans.value.map(p => ({
    id: p.id,
    label: `${p.nombre} (${p.duracionDias} días - ${formatCurrency(p.precio, p.moneda)})`
  }));
});
const metodoPagoOptions = [
  { id: 'Efectivo', label: 'Efectivo' },
  { id: 'Transferencia', label: 'Transferencia' }
];
const paymentEstadoOptions = computed(() => {
  return PAYMENT_ESTADOS.map(e => ({ id: e, label: e }));
});

async function openRenewModal(m) {
  renewingMembership.value = m;
  if (membershipStore.plans.length === 0) {
    await membershipStore.fetchPlans();
  }
  renewForm.planId = activePlans.value[0]?.id || 0;
  renewForm.fechaInicio = new Date().toISOString().slice(0, 10);
  renewForm.notas = '';
  showRenewModal.value = true;
}

async function handleRenew(andPay = false) {
  saving.value = true;
  try {
    const renewed = await membershipStore.renewMembership(renewingMembership.value.alumnoId, {
      planId: renewForm.planId,
      fechaInicio: renewForm.fechaInicio || null,
      notas: renewForm.notas || null
    });
    success('Membresía renovada');
    showRenewModal.value = false;
    
    await Promise.all([
      membershipStore.fetchMemberships({ estado: 'Activa' }),
      fetchMembresiasVencidas(),
    ]);

    if (andPay && renewed) {
      paymentForm.membresiaId = renewed.id;
      paymentForm.alumnoNombre = `${renewed.alumnoNombre} ${renewed.alumnoApellido}`.trim();
      paymentForm.planNombre = renewed.planNombre;
      paymentForm.monto = formatNumberWithDots(renewed.planPrecio);
      paymentForm.fechaPago = new Date(Date.now() - new Date().getTimezoneOffset() * 60000).toISOString().slice(0, 16);
      paymentForm.metodoPago = 'Efectivo';
      paymentForm.estado = 'Completado';
      paymentForm.notas = '';
      showPaymentModal.value = true;
    }
  } catch (err) {
    showError(err.response?.data?.error || 'Error al renovar');
  } finally {
    saving.value = false;
  }
}

async function handlePaymentSubmit() {
  saving.value = true;
  try {
    await membershipStore.createPayment({
      membresiaId: paymentForm.membresiaId,
      monto: parseFormattedNumber(paymentForm.monto),
      fechaPago: paymentForm.fechaPago,
      metodoPago: paymentForm.metodoPago,
      estado: paymentForm.estado,
      notas: paymentForm.notas || null
    });
    success('Pago registrado');
    showPaymentModal.value = false;
  } catch (err) {
    showError(err.response?.data?.error || 'Error al guardar pago')
  } finally {
    saving.value = false;
  }
}

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
  
  paymentForm.monto = formatted;
  input.value = formatted;
  
  nextTick(() => {
    const newLength = formatted.length;
    const diff = newLength - oldLength;
    const newCursorPos = selectionStart + diff;
    input.setSelectionRange(newCursorPos, newCursorPos);
  });
}
</script>
