<template>
  <div class="kpi-card" :class="`kpi-card--${variant}`">
    <div class="kpi-label">{{ label }}</div>
    <div class="kpi-value">
      <span class="kpi-number">{{ formattedValue }}</span>
      <span v-if="suffix" class="kpi-suffix">{{ suffix }}</span>
    </div>
    <div v-if="sub" class="kpi-sub">{{ sub }}</div>
  </div>
</template>

<script setup>
import { computed } from 'vue'

const props = defineProps({
  label:   { type: String,  required: true },
  value:   { type: Number,  required: true },
  suffix:  { type: String,  default: '' },
  sub:     { type: String,  default: '' },
  variant: {
    type: String,
    default: 'default',
    validator: v => ['default', 'amber', 'red', 'green', 'orange'].includes(v)
  }
})

const formattedValue = computed(() =>
  Number.isInteger(props.value)
    ? props.value.toLocaleString('pt-BR')
    : props.value.toFixed(1)
)
</script>

<style scoped>
.kpi-card {
  background: var(--bg-surface);
  border: 1px solid var(--border);
  border-radius: var(--radius-lg);
  padding: 1.25rem 1.5rem;
  display: flex;
  flex-direction: column;
  gap: 0.4rem;
  transition: border-color var(--transition), box-shadow var(--transition);
  position: relative;
  overflow: hidden;
}

.kpi-card::before {
  content: '';
  position: absolute;
  top: 0; left: 0; right: 0;
  height: 2px;
}

.kpi-card--amber::before  { background: var(--amber); }
.kpi-card--red::before    { background: var(--red); }
.kpi-card--green::before  { background: var(--green); }
.kpi-card--orange::before { background: var(--orange); }
.kpi-card--default::before { background: var(--border-light); }

.kpi-card:hover {
  border-color: var(--border-light);
  box-shadow: 0 4px 24px #00000033;
}

.kpi-label {
  font-size: 0.65rem;
  letter-spacing: 0.15em;
  color: var(--text-muted);
  text-transform: uppercase;
}

.kpi-value {
  display: flex;
  flex-direction: column;
  gap: 0.2rem;
}

.kpi-number {
  font-size: clamp(1.4rem, 1.8vw, 2rem);
  font-weight: 800;
  line-height: 1;
  color: var(--text-primary);
}

.kpi-card--amber  .kpi-number { color: var(--amber); }
.kpi-card--red    .kpi-number { color: var(--red); }
.kpi-card--green  .kpi-number { color: var(--green); }
.kpi-card--orange .kpi-number { color: var(--orange); }

.kpi-suffix {
  font-size: 0.7rem;
  color: var(--text-muted);
}

.kpi-sub {
  font-size: 0.7rem;
  color: var(--text-muted);
}
</style>
