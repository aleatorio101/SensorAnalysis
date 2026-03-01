<template>
  <div class="progress-view">
    <p class="section-label">PROCESSAMENTO</p>

    <div class="progress-card card">

      <div class="progress-header">
        <div class="job-id">
          <span class="job-label">JOB ID</span>
          <code class="job-value">{{ jobId }}</code>
        </div>
        <div class="job-status">
          <span class="status-dot" :class="`dot--${statusClass}`" />
          <span class="status-text">{{ statusLabel }}</span>
        </div>
      </div>

      <div class="main-progress">
        <div class="progress-percent">
          <span class="percent-number">{{ job?.progressPercent?.toFixed(1) ?? '0.0' }}</span>
          <span class="percent-sign">%</span>
        </div>

        <div class="progress-track">
          <div
            class="progress-fill"
            :style="{ width: (job?.progressPercent ?? 0) + '%' }"
            :class="{ 'fill--complete': job?.status === 'Completed' }"
          />
        </div>

        <div class="progress-counts">
          <span>
            <strong>{{ job?.processedSamples ?? 0 }}</strong>
            amostras analisadas
          </span>
          <span>
            de <strong>{{ job?.totalSamples ?? '—' }}</strong>
          </span>
        </div>
      </div>

<div class="timeline">
  <div class="timeline-item" :class="{ done: job }">
    <span class="tl-dot" />
    <span class="tl-label">Job criado</span>
    <span v-if="job?.createdAt" class="tl-time">{{ formatTime(job.createdAt) }}</span>
  </div>
  <div class="timeline-item" :class="{ done: job?.status !== 'Queued' }">
    <span class="tl-dot" />
    <span class="tl-label">Processamento iniciado</span>
  </div>
  <div class="timeline-item" :class="{ done: job?.status === 'Completed' || job?.status === 'Failed' }">
    <span class="tl-dot" />
    <span class="tl-label">Análise concluída</span>
    <span v-if="job?.completedAt" class="tl-time">{{ formatTime(job.completedAt) }}</span>
  </div>
</div>

      <div v-if="job?.status === 'Failed'" class="error-block">
        <span class="error-icon">⚠</span>
        {{ job.errorMessage ?? 'Erro desconhecido durante o processamento.' }}
      </div>
    </div>

    <div class="progress-actions">
      <button class="btn btn-ghost" @click="router.push({ name: 'upload' })">
        ← Novo upload
      </button>
      <button
        v-if="job?.status === 'Completed'"
        class="btn btn-primary"
        @click="router.push({ name: 'dashboard', params: { id: jobId } })"
      >
        VER DASHBOARD →
      </button>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { analysisApi } from '@/services/api'
import { usePolling } from '@/composables/usePolling'

const route  = useRoute()
const router = useRouter()

const jobId = route.params.id
const job   = ref(null)
const error = ref('')

const STATUS_LABELS = {
  Queued:     'NA FILA',
  Processing: 'PROCESSANDO',
  Completed:  'CONCLUÍDO',
  Failed:     'FALHOU'
}

const statusLabel = computed(() => STATUS_LABELS[job.value?.status] ?? '—')

const statusClass = computed(() => {
  const s = job.value?.status
  if (s === 'Completed') return 'green'
  if (s === 'Failed')    return 'red'
  if (s === 'Processing') return 'amber'
  return 'muted'
})

async function fetchProgress() {
  try {
    job.value = await analysisApi.getProgress(jobId)
  } catch (err) {
    error.value = err.message
  }
}

const { start } = usePolling(fetchProgress, {
  interval: 1200,
  shouldStop: () => ['Completed', 'Failed'].includes(job.value?.status)
})

onMounted(start)

function formatTime(iso) {
  if (!iso) return ''
  return new Date(iso).toLocaleTimeString('pt-BR')
}
</script>

<style scoped>
.progress-view {
  max-width: 680px;
  margin: 0 auto;
  display: flex;
  flex-direction: column;
  gap: 2rem;
}

.progress-card { display: flex; flex-direction: column; gap: 2rem; }

.progress-header {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  flex-wrap: wrap;
  gap: 1rem;
}

.job-id { display: flex; flex-direction: column; gap: 0.25rem; }

.job-label {
  font-size: 0.6rem;
  letter-spacing: 0.15em;
  color: var(--text-muted);
}

.job-value {
  font-size: 0.75rem;
  color: var(--text-secondary);
  word-break: break-all;
}

.job-status {
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.status-dot {
  width: 8px;
  height: 8px;
  border-radius: 50%;
  flex-shrink: 0;
}

.dot--amber  { background: var(--amber); box-shadow: 0 0 6px var(--amber); animation: blink 1s ease infinite; }
.dot--green  { background: var(--green); box-shadow: 0 0 6px var(--green); }
.dot--red    { background: var(--red); }
.dot--muted  { background: var(--text-muted); }

.status-text {
  font-size: 0.7rem;
  letter-spacing: 0.15em;
  color: var(--text-secondary);
}

.main-progress { display: flex; flex-direction: column; gap: 1rem; }

.progress-percent {
  display: flex;
  align-items: baseline;
  gap: 4px;
}

.percent-number {
  font-size: 4rem;
  font-weight: 800;
  line-height: 1;
  color: var(--amber);
}

.percent-sign {
  font-size: 1.5rem;
  font-weight: 800;
  color: var(--amber);
  opacity: 0.6;
}

.progress-track {
  height: 4px;
  background: var(--border);
  border-radius: 2px;
  overflow: hidden;
}

.progress-fill {
  height: 100%;
  background: var(--amber);
  border-radius: 2px;
  transition: width 0.6s cubic-bezier(0.4, 0, 0.2, 1);
  box-shadow: 0 0 10px var(--amber-glow);
}

.fill--complete { background: var(--green); box-shadow: 0 0 10px #3dd68c44; }

.progress-counts {
  display: flex;
  justify-content: space-between;
  font-size: 0.75rem;
  color: var(--text-muted);
}

.progress-counts strong { color: var(--text-primary); }

.timeline { display: flex; flex-direction: column; gap: 0; }

.timeline-item {
  display: flex;
  align-items: flex-start;
  gap: 0.75rem;
  padding: 0.6rem 0 0.6rem 1.25rem;
  border-left: 1px solid var(--border);
  margin-left: 7px;
  position: relative;
  opacity: 0.35;
  transition: opacity var(--transition);
}

.timeline-item.done { opacity: 1; }

.tl-dot {
  position: absolute;
  left: -5px;
  top: 50%;
  transform: translateY(-50%);
  width: 9px;
  height: 9px;
  border-radius: 50%;
  border: 2px solid var(--border);
  background: var(--bg-surface);
  transition: all var(--transition);
  flex-shrink: 0;
}

.timeline-item.done .tl-dot {
  border-color: var(--amber);
  background: var(--amber);
}

.tl-content {
  display: flex;
  flex-direction: column;
  gap: 0.15rem;
}

.tl-label {
  font-size: 0.75rem;
  color: var(--text-secondary);
}

.tl-time {
  font-size: 0.65rem;
  color: var(--text-muted);
}

.error-block {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  background: var(--red-dim);
  border: 1px solid #f0525233;
  border-radius: var(--radius-md);
  padding: 0.875rem 1rem;
  font-size: 0.8rem;
  color: var(--red);
}

.progress-actions {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

@keyframes blink {
  0%, 100% { opacity: 1; }
  50%       { opacity: 0.3; }
}
</style>
