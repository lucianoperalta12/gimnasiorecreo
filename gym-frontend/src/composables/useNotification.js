import { ref } from 'vue'

const notifications = ref([])
let nextId = 0

export function useNotification() {
  function notify(message, type = 'success', duration = 4000) {
    const id = nextId++
    notifications.value.push({ id, message, type })
    setTimeout(() => {
      notifications.value = notifications.value.filter(n => n.id !== id)
    }, duration)
  }

  function success(message) {
    notify(message, 'success')
  }

  function error(message) {
    notify(message, 'error')
  }

  function warning(message) {
    notify(message, 'warning')
  }

  return {
    notifications,
    notify,
    success,
    error,
    warning
  }
}
