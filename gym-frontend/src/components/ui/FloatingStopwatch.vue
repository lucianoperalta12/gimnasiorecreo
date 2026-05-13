<template>
  <Teleport to="body">
    <div class="stopwatch-wrapper" :class="{ expanded: isExpanded }">
      <!-- Collapsed: solo ícono reloj -->
      <button
        v-if="!isExpanded"
        class="stopwatch-icon-btn"
        @click="isExpanded = true"
        title="Cronómetro"
      >
        <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" class="clock-icon">
          <circle cx="12" cy="13" r="8" />
          <path stroke-linecap="round" d="M12 9v4l2.5 2.5" />
          <path stroke-linecap="round" d="M9.5 2.5h5M12 2.5V5" />
        </svg>
        <span v-if="running" class="pulse-dot"></span>
      </button>

      <!-- Expanded panel -->
      <div v-else class="stopwatch-panel">
        <div class="sw-header">
          <span class="sw-title">Cronómetro</span>
          <button class="sw-close" @click="collapse" title="Minimizar">
            <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" width="14" height="14">
              <path stroke-linecap="round" d="M19 9l-7 7-7-7" />
            </svg>
          </button>
        </div>

        <div class="sw-display">
          <span class="sw-time">{{ formatted }}</span>
        </div>

        <div class="sw-controls">
          <!-- Play / Pause -->
          <button class="sw-btn primary" @click="toggle" :title="running ? 'Pausar' : 'Iniciar'">
            <!-- Play -->
            <svg v-if="!running" viewBox="0 0 24 24" fill="currentColor" width="18" height="18">
              <path d="M8 5v14l11-7L8 5z"/>
            </svg>
            <!-- Pause -->
            <svg v-else viewBox="0 0 24 24" fill="currentColor" width="18" height="18">
              <path d="M6 19h4V5H6v14zm8-14v14h4V5h-4z"/>
            </svg>
          </button>

          <!-- Reset -->
          <button class="sw-btn secondary" @click="reset" title="Reiniciar">
            <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" width="16" height="16">
              <path stroke-linecap="round" stroke-linejoin="round" d="M4.5 12a7.5 7.5 0 1115 0M4.5 12V7.5M4.5 12H9"/>
            </svg>
          </button>
        </div>
      </div>
    </div>
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
  if (h > 0) {
    return `${String(h).padStart(2, '0')}:${String(m).padStart(2, '0')}:${String(s).padStart(2, '0')}`
  }
  return `${String(m).padStart(2, '0')}:${String(s).padStart(2, '0')}`
})

function toggle() {
  if (running.value) {
    clearInterval(intervalId)
    running.value = false
  } else {
    const start = Date.now() - elapsed.value
    intervalId = setInterval(() => {
      elapsed.value = Date.now() - start
    }, 100)
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
.stopwatch-wrapper {
  position: fixed;
  bottom: 24px;
  right: 24px;
  z-index: 9999;
  display: flex;
  flex-direction: column;
  align-items: flex-end;
}

/* Collapsed button */
.stopwatch-icon-btn {
  position: relative;
  width: 44px;
  height: 44px;
  border-radius: 50%;
  background: rgba(20, 20, 20, 0.85);
  border: 1px solid rgba(255,255,255,0.08);
  backdrop-filter: blur(12px);
  color: rgba(255,255,255,0.55);
  display: flex;
  align-items: center;
  justify-content: center;
  cursor: pointer;
  transition: color 0.2s, background 0.2s, transform 0.15s;
  box-shadow: 0 4px 20px rgba(0,0,0,0.4);
}
.stopwatch-icon-btn:hover {
  color: rgba(255,255,255,0.9);
  background: rgba(35, 35, 35, 0.95);
  transform: scale(1.05);
}
.clock-icon {
  width: 22px;
  height: 22px;
}

/* Pulsing dot when running */
.pulse-dot {
  position: absolute;
  top: 7px;
  right: 7px;
  width: 7px;
  height: 7px;
  border-radius: 50%;
  background: #22c55e;
  animation: pulse 1.4s infinite;
}
@keyframes pulse {
  0%, 100% { opacity: 1; transform: scale(1); }
  50% { opacity: 0.5; transform: scale(0.7); }
}

/* Expanded panel */
.stopwatch-panel {
  background: rgba(14, 14, 14, 0.92);
  border: 1px solid rgba(255,255,255,0.07);
  backdrop-filter: blur(20px);
  border-radius: 16px;
  padding: 14px 16px 12px;
  width: 180px;
  box-shadow: 0 8px 32px rgba(0,0,0,0.5);
  animation: fadeUp 0.18s ease-out;
}
@keyframes fadeUp {
  from { opacity: 0; transform: translateY(8px) scale(0.97); }
  to   { opacity: 1; transform: translateY(0) scale(1); }
}

.sw-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 10px;
}
.sw-title {
  font-size: 11px;
  font-weight: 600;
  letter-spacing: 0.08em;
  text-transform: uppercase;
  color: rgba(255,255,255,0.35);
}
.sw-close {
  background: none;
  border: none;
  color: rgba(255,255,255,0.3);
  cursor: pointer;
  padding: 2px;
  display: flex;
  transition: color 0.15s;
}
.sw-close:hover { color: rgba(255,255,255,0.7); }

.sw-display {
  text-align: center;
  margin-bottom: 12px;
}
.sw-time {
  font-size: 30px;
  font-weight: 700;
  letter-spacing: 0.02em;
  color: #fff;
  font-variant-numeric: tabular-nums;
  font-family: 'SF Mono', 'Fira Code', monospace;
}

.sw-controls {
  display: flex;
  gap: 8px;
  justify-content: center;
}
.sw-btn {
  border: none;
  cursor: pointer;
  border-radius: 10px;
  display: flex;
  align-items: center;
  justify-content: center;
  transition: background 0.15s, transform 0.1s;
}
.sw-btn:active { transform: scale(0.93); }

.sw-btn.primary {
  width: 48px;
  height: 36px;
  background: rgba(99, 102, 241, 0.85);
  color: #fff;
}
.sw-btn.primary:hover { background: rgba(99, 102, 241, 1); }

.sw-btn.secondary {
  width: 36px;
  height: 36px;
  background: rgba(255,255,255,0.07);
  color: rgba(255,255,255,0.55);
}
.sw-btn.secondary:hover {
  background: rgba(255,255,255,0.12);
  color: rgba(255,255,255,0.9);
}
</style>
