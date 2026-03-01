<template>
  <div class="dashboard-view">

    <div class="dash-topbar">
      <div>
        <p class="section-label">DASHBOARD</p>
        <h1 class="dash-title">Resultados da <em>Análise</em></h1>
      </div>
      <div class="dash-actions">
        <div class="filter-group">
          <span class="filter-label">TIPO</span>
          <button
            v-for="t in typeFilters"
            :key="t.value"
            class="filter-btn"
            :class="{ 'filter-btn--active': activeType === t.value }"
            @click="activeType = t.value"
          >
            {{ t.label }}
          </button>
        </div>
        <button class="btn btn-ghost" @click="router.push({ name: 'upload' })">
          ← Novo upload
        </button>
      </div>
    </div>

    <div v-if="loading" class="loading-state">
      <span class="loading-spinner" />
      Carregando resultados...
    </div>

    <div v-else-if="error" class="error-banner">
      <span>⚠</span> {{ error }}
    </div>

    <template v-else-if="summary">

      <section>
        <p class="section-label">VISÃO GERAL</p>
        <div class="kpi-grid">
          <KpiCard label="Total Analisadas" :value="summary.totalAnalyzed" variant="default" />
          <KpiCard label="Inválidas"         :value="summary.totalInvalid"  variant="red"
            :sub="`${pct(summary.totalInvalid)}% do total`" />
          <KpiCard label="Anomalias"         :value="summary.totalAnomaly"  variant="orange"
            :sub="`${pct(summary.totalAnomaly)}% do total`" />
          <KpiCard label="Normais"           :value="summary.totalNormal"   variant="green"
            :sub="`${pct(summary.totalNormal)}% do total`" />
          <KpiCard label="Notificações"      :value="summary.notifications?.length ?? 0" variant="amber"
            sub="críticos + anomalias" />
        </div>
      </section>

      <section>
        <p class="section-label">TEMPERATURA</p>
        <div class="threshold-grid">
          <KpiCard label="Alerta Máximo"   :value="summary.tempAlertMaxCount"    suffix="amostras" variant="amber" />
          <KpiCard label="Alerta Mínimo"   :value="summary.tempAlertMinCount"    suffix="amostras" variant="amber" />
          <KpiCard label="Crítico Máximo"  :value="summary.tempCriticalMaxCount" suffix="amostras" variant="red" />
          <KpiCard label="Crítico Mínimo"  :value="summary.tempCriticalMinCount" suffix="amostras" variant="red" />
        </div>
      </section>

      <section>
        <p class="section-label">UMIDADE</p>
        <div class="threshold-grid">
          <KpiCard label="Alerta Máximo"   :value="summary.humidityAlertMaxCount"    suffix="amostras" variant="amber" />
          <KpiCard label="Alerta Mínimo"   :value="summary.humidityAlertMinCount"    suffix="amostras" variant="amber" />
          <KpiCard label="Crítico Máximo"  :value="summary.humidityCriticalMaxCount" suffix="amostras" variant="red" />
          <KpiCard label="Crítico Mínimo"  :value="summary.humidityCriticalMinCount" suffix="amostras" variant="red" />
        </div>
      </section>

      <section>
        <p class="section-label">PONTO DE ORVALHO</p>
        <div class="threshold-grid">
          <KpiCard label="Alerta Máximo"  :value="summary.dewPointAlertMaxCount"    suffix="amostras" variant="amber" />
          <KpiCard label="Crítico Máximo" :value="summary.dewPointCriticalMaxCount" suffix="amostras" variant="red" />
        </div>
      </section>

      <section>
        <p class="section-label">DISTRIBUIÇÃO & SÉRIES TEMPORAIS</p>
        <div class="charts-row">
          <div class="card chart-card">
            <p class="chart-title">Distribuição por Status</p>
            <SummaryDonut :summary="summary" />
          </div>
          <div class="card chart-card chart-card--wide">
            <p class="chart-title">Temperatura ao Longo do Tempo</p>
            <TimeSeriesChart
              :results="filteredResults"
              variable="temperature"
              label="Temperatura (°C)"
              color="#f0a500"
              :alertMax="30" :alertMin="15"
              :critMax="35"  :critMin="10"
            />
          </div>
        </div>
      </section>

      <section>
        <div class="charts-row">
          <div class="card chart-card">
            <p class="chart-title">Umidade ao Longo do Tempo</p>
            <TimeSeriesChart
              :results="filteredResults"
              variable="humidity"
              label="Umidade (%)"
              color="#5b8def"
              :alertMax="70" :alertMin="40"
              :critMax="80"  :critMin="30"
            />
          </div>
          <div class="card chart-card">
            <p class="chart-title">Ponto de Orvalho ao Longo do Tempo</p>
            <TimeSeriesChart
              :results="filteredResults"
              variable="dewPoint"
              label="Ponto de Orvalho (°C)"
              color="#3dd68c"
              :alertMax="22"
              :critMax="25"
            />
          </div>
        </div>
      </section>

<section v-if="summary.notifications?.length">
  <p class="section-label">NOTIFICAÇÕES PUBLICADAS NA FILA</p>
  <div class="card">
    <table class="notif-table">
      <thead>
        <tr>
          <th>SENSOR ID</th>
          <th>TIMESTAMP</th>
          <th>MOTIVO</th>
        </tr>
      </thead>
      <tbody>
        <tr v-for="n in pagedNotifications" :key="`${n.sensorId}-${n.timestamp}`">
          <td><code>{{ n.sensorId }}</code></td>
          <td>{{ formatDate(n.timestamp) }}</td>
          <td><StatusBadge :status="n.motivo === 'critical' ? 'critical' : 'anomaly'" /></td>
        </tr>
      </tbody>
    </table>

    <div class="notif-nav">
      <span class="notif-total">{{ summary.notifications.length }} notificações</span>
      <div class="nav-controls">
        <button class="nav-btn" :disabled="notifPage === 0" @click="notifPage--">‹</button>
        <span class="nav-info">{{ notifPage + 1 }} / {{ totalNotifPages }}</span>
        <button class="nav-btn" :disabled="notifPage >= totalNotifPages - 1" @click="notifPage++">›</button>
      </div>
    </div>
  </div>
</section>

    </template>
  </div>
</template>

<script setup>
import { ref, computed, onMounted, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { analysisApi } from '@/services/api'
import KpiCard         from '@/components/KpiCard.vue'
import StatusBadge     from '@/components/StatusBadge.vue'
import TimeSeriesChart from '@/components/TimeSeriesChart.vue'
import SummaryDonut    from '@/components/SummaryDonut.vue'

const route  = useRoute()
const router = useRouter()

const jobId   = route.params.id
const allResults = ref([])
const loading = ref(true)
const error   = ref('')

const activeType = ref('all')

const typeFilters = [
  { value: 'all',    label: 'TODOS' },
  { value: 'FM5308', label: 'FM5308' },
  { value: 'AM2302', label: 'AM2302' }
]

const filteredResults = computed(() =>
  activeType.value === 'all'
    ? allResults.value
    : allResults.value.filter(r => r.type === activeType.value)
)


const summary = computed(() => {
  const results = filteredResults.value
  if (!results.length) return null

  const valid = results.filter(r => r.anomaly.status !== 'invalid')

  const notifications = results
    .filter(r => {
      const isCritical = r.temperature.status === 'critical'
                      || r.humidity.status    === 'critical'
                      || r.dewPoint.status    === 'critical'
      const isAnomaly  = r.anomaly.status === 'anomaly'
      return isCritical || isAnomaly
    })
    .map(r => ({
      sensorId:  r.sensorId,
      timestamp: r.timestamp,
      motivo: (r.temperature.status === 'critical' ||
               r.humidity.status    === 'critical' ||
               r.dewPoint.status    === 'critical')
               ? 'critical'
               : 'anomaly'
    }))

  return {
    totalAnalyzed: results.length,
    totalInvalid:  results.filter(r => r.anomaly.status === 'invalid').length,
    totalAnomaly:  results.filter(r => r.anomaly.status === 'anomaly').length,
    totalNormal:   results.filter(r => r.anomaly.status === 'normal').length,
    notifications,

    tempAlertMaxCount:    valid.filter(r => r.temperature.status === 'alert'    && r.temperature.limitType === 'max').length,
    tempAlertMinCount:    valid.filter(r => r.temperature.status === 'alert'    && r.temperature.limitType === 'min').length,
    tempCriticalMaxCount: valid.filter(r => r.temperature.status === 'critical' && r.temperature.limitType === 'max').length,
    tempCriticalMinCount: valid.filter(r => r.temperature.status === 'critical' && r.temperature.limitType === 'min').length,

    humidityAlertMaxCount:    valid.filter(r => r.humidity.status === 'alert'    && r.humidity.limitType === 'max').length,
    humidityAlertMinCount:    valid.filter(r => r.humidity.status === 'alert'    && r.humidity.limitType === 'min').length,
    humidityCriticalMaxCount: valid.filter(r => r.humidity.status === 'critical' && r.humidity.limitType === 'max').length,
    humidityCriticalMinCount: valid.filter(r => r.humidity.status === 'critical' && r.humidity.limitType === 'min').length,

    dewPointAlertMaxCount:    valid.filter(r => r.dewPoint.status === 'alert'    && r.dewPoint.limitType === 'max').length,
    dewPointCriticalMaxCount: valid.filter(r => r.dewPoint.status === 'critical' && r.dewPoint.limitType === 'max').length,
  }
})

onMounted(async () => {
  try {
    const data = await analysisApi.getResults(jobId)
    allResults.value = data.results ?? []
  } catch (err) {
    error.value = err.message
  } finally {
    loading.value = false
  }
})

function pct(value) {
  if (!summary.value?.totalAnalyzed) return '0.0'
  return ((value / summary.value.totalAnalyzed) * 100).toFixed(1)
}

function formatDate(iso) {
  return new Date(iso).toLocaleString('pt-BR')
}

const notifPage = ref(0)
const NOTIF_PAGE_SIZE = 20

const pagedNotifications = computed(() => {
  const notifs = summary.value?.notifications ?? []
  const start = notifPage.value * NOTIF_PAGE_SIZE
  return notifs.slice(start, start + NOTIF_PAGE_SIZE)
})

const totalNotifPages = computed(() =>
  Math.ceil((summary.value?.notifications?.length ?? 0) / NOTIF_PAGE_SIZE)
)
</script>

<style scoped>
.dashboard-view { display: flex; flex-direction: column; gap: 2.5rem; }

.dash-topbar {
  display: flex;
  align-items: flex-end;
  justify-content: space-between;
  flex-wrap: wrap;
  gap: 1rem;
}

.dash-title {
  font-size: clamp(1.5rem, 3vw, 2.25rem);
  font-weight: 800;
}

.dash-title em { font-style: normal; color: var(--amber); }

.dash-actions {
  display: flex;
  align-items: center;
  gap: 1rem;
  flex-wrap: wrap;
}

.filter-group {
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.filter-label {
  font-size: 0.6rem;
  letter-spacing: 0.15em;
  color: var(--text-muted);
}

.filter-btn {
  font-family: var(--font-mono);
  font-size: 0.65rem;
  letter-spacing: 0.1em;
  padding: 0.4rem 0.875rem;
  border-radius: var(--radius-sm);
  border: 1px solid var(--border);
  background: transparent;
  color: var(--text-muted);
  cursor: pointer;
  transition: all var(--transition);
}

.filter-btn:hover { border-color: var(--border-light); color: var(--text-primary); }

.filter-btn--active {
  border-color: var(--amber);
  color: var(--amber);
  background: var(--amber-dim);
}

.kpi-grid {
  display: grid;
  grid-template-columns: repeat(5, minmax(0, 1fr));
  gap: 1rem;
}

.threshold-grid {
  display: grid;
  grid-template-columns: repeat(4, minmax(0, 1fr));
  gap: 1rem;
}

.charts-row {
  display: grid;
  grid-template-columns: 320px 1fr;
  gap: 1rem;
}

@media (max-width: 900px) {
  .charts-row { grid-template-columns: 1fr; }
}

.chart-card {
  display: flex;
  flex-direction: column;
  gap: 1rem;
  padding-bottom: 0.75rem;
}

.chart-card--wide { grid-column: auto; }

.chart-title {
  font-size: 0.7rem;
  letter-spacing: 0.1em;
  color: var(--text-muted);
  text-transform: uppercase;
}

.notif-table {
  width: 100%;
  border-collapse: collapse;
  font-size: 0.78rem;
}

.notif-table th {
  font-size: 0.6rem;
  letter-spacing: 0.15em;
  color: var(--text-muted);
  text-align: left;
  padding: 0.5rem 0.75rem;
  border-bottom: 1px solid var(--border);
}

.notif-table td {
  padding: 0.75rem;
  border-bottom: 1px solid var(--border);
  color: var(--text-secondary);
  vertical-align: middle;
}

.notif-table tr:last-child td { border-bottom: none; }

.notif-table tr:hover td { background: var(--bg-elevated); }

.notif-table code {
  color: var(--amber);
  font-size: 0.75rem;
}

.loading-state {
  display: flex;
  align-items: center;
  gap: 1rem;
  color: var(--text-muted);
  font-size: 0.85rem;
  padding: 4rem 0;
  justify-content: center;
}

.loading-spinner {
  width: 18px;
  height: 18px;
  border: 2px solid var(--border);
  border-top-color: var(--amber);
  border-radius: 50%;
  animation: spin 0.7s linear infinite;
}

.error-banner {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  background: var(--red-dim);
  border: 1px solid #f0525233;
  border-radius: var(--radius-md);
  padding: 1rem;
  color: var(--red);
  font-size: 0.85rem;
}

@keyframes spin {
  to { transform: rotate(360deg); }
}

.notif-nav {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding-top: 1rem;
  margin-top: 0.5rem;
  border-top: 1px solid var(--border);
}

.notif-total {
  font-size: 0.65rem;
  letter-spacing: 0.1em;
  color: var(--text-muted);
}

.nav-controls {
  display: flex;
  align-items: center;
  gap: 0.75rem;
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
</style>
