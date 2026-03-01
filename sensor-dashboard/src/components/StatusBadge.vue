<template>
  <span class="badge" :class="`badge--${status}`">
    <span class="badge-dot" />
    {{ label }}
  </span>
</template>

<script setup>
import { computed } from 'vue'

const props = defineProps({
  status: {
    type: String,
    required: true,
    validator: v => ['normal', 'alert', 'critical', 'anomaly', 'invalid'].includes(v)
  }
})

const LABELS = {
  normal:   'NORMAL',
  alert:    'ALERTA',
  critical: 'CRÍTICO',
  anomaly:  'ANOMALIA',
  invalid:  'INVÁLIDO'
}

const label = computed(() => LABELS[props.status] ?? props.status.toUpperCase())
</script>

<style scoped>
.badge {
  display: inline-flex;
  align-items: center;
  gap: 6px;
  font-size: 0.65rem;
  letter-spacing: 0.1em;
  padding: 3px 10px;
  border-radius: 2px;
  font-weight: 700;
  border: 1px solid transparent;
}

.badge-dot {
  width: 5px;
  height: 5px;
  border-radius: 50%;
  flex-shrink: 0;
}

.badge--normal {
  background: var(--green-dim);
  color: var(--green);
  border-color: #3dd68c33;
}
.badge--normal .badge-dot { background: var(--green); }

.badge--alert {
  background: var(--amber-dim);
  color: var(--amber);
  border-color: var(--amber-glow);
}
.badge--alert .badge-dot {
  background: var(--amber);
  box-shadow: 0 0 4px var(--amber);
  animation: pulse 1.5s ease infinite;
}

.badge--critical {
  background: var(--red-dim);
  color: var(--red);
  border-color: #f0525233;
}
.badge--critical .badge-dot {
  background: var(--red);
  box-shadow: 0 0 4px var(--red);
  animation: pulse 0.8s ease infinite;
}

.badge--anomaly {
  background: var(--orange-dim);
  color: var(--orange);
  border-color: #f0785033;
}
.badge--anomaly .badge-dot { background: var(--orange); }

.badge--invalid {
  background: #ffffff08;
  color: var(--text-muted);
  border-color: var(--border);
}
.badge--invalid .badge-dot { background: var(--text-muted); }

@keyframes pulse {
  0%, 100% { opacity: 1; }
  50%       { opacity: 0.3; }
}
</style>
