<template>
  <Teleport to="body">
    <!-- FAB colapsado -->
    <button
      v-if="!isExpanded"
      class="sw-fab"
      @click="isExpanded = true"
      title="Cronómetro"
      aria-label="Abrir cronómetro"
    >
      <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" class="sw-fab-icon">
        <circle cx="12" cy="13" r="8" />
        <path stroke-linecap="round" d="M12 9v4l2.5 2.5" />
        <path stroke-linecap="round" d="M9.5 2.5h5M12 2.5V5" />
      </svg>
      <span v-if="running" class="pulse-dot"></span>
    </button>

    <!-- Panel expandido: bottom sheet en mobile, panel flotante en desktop -->
    <Transition name="sw-panel">
      <div v-if="isExpanded" class="sw-overlay" @click.self="collapse">
        <div class="sw-sheet">
          <!-- Handle drag (mobile) -->
          <div class="sw-handle"></div>

          <div class="sw-content">
            <!-- Header -->
            <div class="sw-header">
              <div class="sw-header-left">
                <div class="sw-status-dot" :class="running ? 'active' : ''"></div>
                <span class="sw-title">Cronómetro</span>
              </div>
              <button class="sw-close-btn" @click="collapse" aria-label="Cerrar">
                <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" width="18" height="18">
                  <path stroke-linecap="round" d="M6 18L18 6M6 6l12 12" />
                </svg>
              </button>
            </div>

            <!-- Display -->
            <div class="sw-display">
              <span class="sw-time">{{ formatted }}</span>
              <span class="sw-ms">{{ centesimas }}</span>
            </div>

            <!-- Controls -->
            <div class="sw-controls">
              <button class="sw-btn-reset" @click="reset" aria-label="Reiniciar" title="Reiniciar">
                <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" width="20" height="20">
                  <path stroke-linecap="round" stroke-linejoin="round" d="M12 6v6l3.5 2M12 2a10 10 0 100 20A10 10 0 0012 2z" />
                  <path stroke-linecap="round" stroke-linejoin="round" d="M3 3l3.5 3.5M3 3v4m0-4h4" />
                </svg>
              </button>

              <button class="sw-btn-play" @click="toggle" :aria-label="running ? 'Pausar' : 'Iniciar'">
                <svg v-if="!running" viewBox="0 0 24 24" fill="currentColor" width="22" height="22">
                  <path d="M8 5v14l11-7L8 5z"/>
                </svg>
                <svg v-else viewBox="0 0 24 24" fill="currentColor" width="22" height="22">
                  <path d="M6 19h4V5H6v14zm8-14v14h4V5h-4z"/>
                </svg>
              </button>
            </div>
          </div>
        </div>
      </div>
    </Transition>
  </Teleport>
</template>

<script setup>
import { ref, computed, onUnmounted } from 'vue'

const isExpanded = ref(false)
const running = ref(false)
const elapsed = ref(0) // ms
let intervalId = null

const formatted = computed(() => {
  const total = Math.floor(elapsed.value / 1000)
  const h = Math.floor(total / 3600)
  const m = Math.floor((total % 3600) / 60)
  const s = total % 60
  if (h > 0) return `${String(h).padStart(2, '0')}:${String(m).padStart(2, '0')}:${String(s).padStart(2, '0')}`
  return `${String(m).padStart(2, '0')}:${String(s).padStart(2, '0')}`
})

const centesimas = computed(() => {
  return '.' + String(Math.floor((elapsed.value % 1000) / 10)).padStart(2, '0')
})

function toggle() {
  if (running.value) {
    clearInterval(intervalId)
    running.value = false
  } else {
    const start = Date.now() - elapsed.value
    intervalId = setInterval(() => { elapsed.value = Date.now() - start }, 50)
    running.value = true
  }
}

function reset() {
  clearInterval(intervalId)
  running.value = false
  elapsed.value = 0
}

function collapse() {
  isExpanded.value = false
}

onUnmounted(() => clearInterval(intervalId))
</script>

<style scoped>
/* ===== FAB button ===== */
.sw-fab {
  position: fixed;
  bottom: 24px;
  right: 24px;
  z-index: 9998;
  width: 46px;
  height: 46px;
  border-radius: 50%;
  background: rgba(16, 16, 16, 0.88);
  border: 1px solid rgba(255,255,255,0.09);
  backdrop-filter: blur(14px);
  color: rgba(255,255,255,0.5);
  display: flex;
  align-items: center;
  justify-content: center;
  cursor: pointer;
  transition: color 0.2s, background 0.2s, transform 0.15s;
  box-shadow: 0 4px 20px rgba(0,0,0,0.45);
}
.sw-fab:hover {
  color: #fff;
  background: rgba(232, 93, 4, 0.2);
  border-color: rgba(232, 93, 4, 0.4);
  transform: scale(1.07);
}
.sw-fab-icon { width: 22px; height: 22px; }

.pulse-dot {
  position: absolute;
  top: 8px;
  right: 8px;
  width: 8px;
  height: 8px;
  border-radius: 50%;
  background: #e85d04;
  animation: pulse 1.4s infinite;
}
@keyframes pulse {
  0%, 100% { opacity: 1; transform: scale(1); }
  50% { opacity: 0.4; transform: scale(0.65); }
}

/* ===== Overlay ===== */
.sw-overlay {
  position: fixed;
  inset: 0;
  z-index: 9999;
  /* Fondo semitransparente solo en mobile para dar sensación de sheet */
  background: transparent;
  display: flex;
  align-items: flex-end;
  justify-content: flex-end;
  pointer-events: none;
}

/* ===== Sheet / Panel ===== */
.sw-sheet {
  pointer-events: all;
  background: rgba(13, 13, 13, 0.96);
  backdrop-filter: blur(24px);
  border: 1px solid rgba(255,255,255,0.07);
  border-top: 1px solid rgba(232, 93, 4, 0.25);

  /* Desktop: panel flotante abajo a la derecha */
  margin: 24px;
  border-radius: 20px;
  width: 260px;
  box-shadow: 0 16px 48px rgba(0,0,0,0.6), 0 0 0 1px rgba(232,93,4,0.08);
}

/* Mobile: bottom sheet full width */
@media (max-width: 640px) {
  .sw-overlay {
    align-items: flex-end;
    justify-content: center;
    background: rgba(0,0,0,0.45);
    backdrop-filter: blur(4px);
    pointer-events: all;
  }
  .sw-sheet {
    margin: 0;
    width: 100%;
    border-radius: 24px 24px 0 0;
    border-bottom: none;
    padding-bottom: env(safe-area-inset-bottom, 8px);
  }
}

.sw-handle {
  width: 40px;
  height: 4px;
  background: rgba(255,255,255,0.12);
  border-radius: 2px;
  margin: 10px auto 0;
  display: none;
}
@media (max-width: 640px) {
  .sw-handle { display: block; }
}

.sw-content {
  padding: 16px 20px 20px;
}

/* Header */
.sw-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 16px;
}
.sw-header-left {
  display: flex;
  align-items: center;
  gap: 8px;
}
.sw-status-dot {
  width: 7px;
  height: 7px;
  border-radius: 50%;
  background: rgba(255,255,255,0.2);
  transition: background 0.3s;
}
.sw-status-dot.active {
  background: #22c55e;
  animation: pulse 1.4s infinite;
}
.sw-title {
  font-size: 11px;
  font-weight: 700;
  letter-spacing: 0.1em;
  text-transform: uppercase;
  color: rgba(255,255,255,0.35);
}
.sw-close-btn {
  background: none;
  border: none;
  color: rgba(255,255,255,0.3);
  cursor: pointer;
  padding: 4px;
  display: flex;
  border-radius: 6px;
  transition: color 0.15s, background 0.15s;
}
.sw-close-btn:hover {
  color: rgba(255,255,255,0.8);
  background: rgba(255,255,255,0.07);
}

/* Display */
.sw-display {
  display: flex;
  align-items: baseline;
  justify-content: center;
  gap: 2px;
  margin-bottom: 20px;
  padding: 16px 0 12px;
  border-bottom: 1px solid rgba(255,255,255,0.05);
}
.sw-time {
  font-size: 48px;
  font-weight: 700;
  line-height: 1;
  color: #fff;
  font-variant-numeric: tabular-nums;
  font-family: 'SF Mono', 'Fira Code', 'Courier New', monospace;
  letter-spacing: -1px;
}
.sw-ms {
  font-size: 24px;
  font-weight: 500;
  color: rgba(255,255,255,0.35);
  font-variant-numeric: tabular-nums;
  font-family: 'SF Mono', 'Fira Code', 'Courier New', monospace;
  margin-bottom: 2px;
}

/* Controls */
.sw-controls {
  display: flex;
  gap: 10px;
  align-items: center;
  justify-content: center;
}

.sw-btn-reset,
.sw-btn-play {
  width: 42px;
  height: 42px;
  flex: none;
  display: flex;
  align-items: center;
  justify-content: center;
  border-radius: 12px;
  cursor: pointer;
  transition: background 0.15s, color 0.15s, box-shadow 0.15s, transform 0.1s;
}
.sw-btn-reset:active,
.sw-btn-play:active { transform: scale(0.93); }

.sw-btn-reset {
  border: 1px solid rgba(255,255,255,0.08);
  background: rgba(255,255,255,0.05);
  color: rgba(255,255,255,0.5);
}
.sw-btn-reset:hover {
  background: rgba(255,255,255,0.1);
  color: rgba(255,255,255,0.85);
}

.sw-btn-play {
  border: none;
  background: #e85d04;
  box-shadow: 0 6px 20px rgba(232, 93, 4, 0.35);
  color: #fff;
}
.sw-btn-play:hover {
  background: #c94c00;
  box-shadow: 0 6px 22px rgba(201, 76, 0, 0.4);
}

/* ===== Transitions ===== */
.sw-panel-enter-active {
  transition: all 0.22s cubic-bezier(0.34, 1.2, 0.64, 1);
}
.sw-panel-leave-active {
  transition: all 0.18s ease-in;
}
.sw-panel-enter-from,
.sw-panel-leave-to {
  opacity: 0;
  transform: translateY(16px) scale(0.97);
}
@media (max-width: 640px) {
  .sw-panel-enter-from,
  .sw-panel-leave-to {
    opacity: 0;
    transform: translateY(100%);
  }
}
</style>
