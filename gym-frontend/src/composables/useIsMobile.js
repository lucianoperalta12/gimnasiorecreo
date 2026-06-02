import { ref, onMounted, onUnmounted } from 'vue'

const MOBILE_BREAKPOINT = '(max-width: 767px)'

export function useIsMobile() {
  const isMobile = ref(
    typeof window !== 'undefined'
      ? window.matchMedia(MOBILE_BREAKPOINT).matches
      : false
  )

  let mq = null

  function onBreakpointChange(e) {
    isMobile.value = e.matches
  }

  onMounted(() => {
    mq = window.matchMedia(MOBILE_BREAKPOINT)
    mq.addEventListener('change', onBreakpointChange)
  })

  onUnmounted(() => {
    mq?.removeEventListener('change', onBreakpointChange)
  })

  return { isMobile }
}
