import { ref, onUnmounted } from 'vue'


export function usePolling(fetchFn, { interval = 1500, shouldStop = () => false } = {}) {
  const isPolling = ref(false)
  let timerId = null

  async function tick() {
    try {
      await fetchFn()
    } catch {
      
    }

    if (shouldStop()) {
      stop()
      return
    }

    timerId = setTimeout(tick, interval)
  }

  function start() {
    if (isPolling.value) return
    isPolling.value = true
    tick()
  }

  function stop() {
    isPolling.value = false
    clearTimeout(timerId)
    timerId = null
  }

  onUnmounted(stop)

  return { isPolling, start, stop }
}
