<template>
  <div class="chart-wrapper">
    <Line :data="chartData" :options="chartOptions" />
    <div class="chart-nav">
      <button class="nav-btn" :disabled="page === 0" @click="page--">‹</button>
      <span class="nav-info">{{ page + 1 }} / {{ totalPages }}</span>
      <button class="nav-btn" :disabled="page >= totalPages - 1" @click="page++">›</button>
    </div>
  </div>
</template>

<script setup>
import { computed, ref } from 'vue'
import { Line } from 'vue-chartjs'
import {
  Chart as ChartJS,
  CategoryScale, LinearScale, PointElement, LineElement,
  Title, Tooltip, Legend, Filler
} from 'chart.js'

ChartJS.register(
  CategoryScale, LinearScale, PointElement, LineElement,
  Title, Tooltip, Legend, Filler
)

const props = defineProps({
  results:   { type: Array,  required: true },
  variable:  { type: String, required: true },
  label:     { type: String, required: true },
  color:     { type: String, default: '#f0a500' },
  alertMax:  { type: Number, default: null },
  alertMin:  { type: Number, default: null },
  critMax:   { type: Number, default: null },
  critMin:   { type: Number, default: null }
})

const PAGE_SIZE = 100
const page = ref(0)

const sorted = computed(() =>
  [...props.results]
    .filter(r => r[props.variable]?.value != null)
    .sort((a, b) => new Date(a.timestamp) - new Date(b.timestamp))
)

const totalPages = computed(() => Math.ceil(sorted.value.length / PAGE_SIZE))

const paged = computed(() => {
  const start = page.value * PAGE_SIZE
  return sorted.value.slice(start, start + PAGE_SIZE)
})

const labels = computed(() =>
  paged.value.map(r =>
    new Date(r.timestamp).toLocaleString('pt-BR', {
      day: '2-digit', month: '2-digit', hour: '2-digit', minute: '2-digit'
    })
  )
)

const limitLine = (value, label, color) => value != null ? {
  label,
  data: paged.value.map(() => value),
  borderColor: color,
  borderWidth: 2,
  borderDash: [8, 4],
  pointRadius: 0,
  fill: false,
  tension: 0,
  order: 0
} : null

const chartData = computed(() => ({
  labels: labels.value,
  datasets: [
    {
      label: props.label,
      data: paged.value.map(r => r[props.variable]?.value),
      borderColor: props.color,
      backgroundColor: props.color + '18',
      borderWidth: 1.5,
      pointRadius: 2,
      pointHoverRadius: 5,
      fill: true,
      tension: 0.3,
      order: 1
    },
    limitLine(props.alertMax, 'Alerta Máx',  '#f0a500aa'),
    limitLine(props.alertMin, 'Alerta Mín',  '#f0a500aa'),
    limitLine(props.critMax,  'Crítico Máx', '#f05252cc'),
    limitLine(props.critMin,  'Crítico Mín', '#f05252cc'),
  ].filter(Boolean)
}))

const chartOptions = {
  responsive: true,
  maintainAspectRatio: false,
  animation: false,
  interaction: { mode: 'index', intersect: false },
  plugins: {
    legend: {
      labels: {
        color: '#8b909e',
        font: {size: 10 },
        boxWidth: 12
      }
    },
    tooltip: {
      backgroundColor: '#1e2128',
      borderColor: '#2a2d35',
      borderWidth: 1,
      titleColor: '#e8eaf0',
      bodyColor: '#8b909e',
      titleFont: {size: 11 },
      bodyFont: { size: 11 }
    }
  },
  scales: {
    x: {
      ticks: {
        color: '#555a68',
        font: {size: 9 },
        maxTicksLimit: 10,
        maxRotation: 0
      },
      grid: { color: '#2a2d3533' }
    },
    y: {
      ticks: { color: '#555a68', font: {size: 10 } },
      grid: { color: '#2a2d3533' }
    }
  }
}
</script>

<style scoped>
.chart-wrapper {
  display: flex;
  flex-direction: column;
  gap: 0.75rem;
  height: 300px;
}

.chart-wrapper canvas {
  flex: 1;
  min-height: 0;
}

.chart-nav {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 1rem;
  padding-top: 0.25rem;
  border-top: 1px solid var(--border);
  padding-bottom: 0.25rem;
}

.nav-btn {
  background: var(--bg-elevated);
  border: 1px solid var(--border);
  color: var(--text-secondary);
  width: 28px;
  height: 28px;
  border-radius: var(--radius-sm);
  cursor: pointer;
  font-size: 1rem;
  display: flex;
  align-items: center;
  justify-content: center;
  transition: all var(--transition);
  line-height: 1;
}

.nav-btn:hover:not(:disabled) {
  border-color: var(--amber);
  color: var(--amber);
}

.nav-btn:disabled {
  opacity: 0.3;
  cursor: not-allowed;
}

.nav-info {
  font-size: 0.65rem;
  letter-spacing: 0.1em;
  color: var(--text-muted);
  min-width: 50px;
  text-align: center;
}
</style>