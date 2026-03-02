<template>
  <div class="job-card card" :class="`job-card--${statusClass}`">

    <div class="job-card-main">
      <div class="job-file">
        <span class="job-file-icon">◈</span>
        <div class="job-file-info">
          <span class="job-file-name">{{ job.fileName }}</span>
          <code class="job-id">{{ job.jobId }}</code>
        </div>
      </div>

      <div class="job-progress-area">
        <div class="job-status-row">
          <span class="status-dot" :class="`dot--${statusClass}`" />
          <span class="status-label">{{ statusText }}</span>
          <span class="job-samples">
            {{ job.processedSamples.toLocaleString('pt-BR') }}
            <span v-if="job.totalSamples > 0">/ {{ job.totalSamples.toLocaleString('pt-BR') }}</span>
            amostras
          </span>
        </div>

        <div v-if="isActive" class="job-track">
          <div class="job-fill" :style="{ width: job.progressPercent + '%' }" />
        </div>
      </div>

      <div class="job-meta">
        <span>{{ formatTime(job.createdAt) }}</span>
        <span v-if="job.completedAt"> → {{ formatTime(job.completedAt) }}</span>
      </div>
    </div>

    <div class="job-card-actions">
      <button
        v-if="job.status === 'Completed'"
        class="btn btn-primary btn-sm"
        @click="$emit('open')"
      >
        Dashboard →
      </button>
      <button
        v-else-if="isActive"
        class="btn btn-ghost btn-sm"
        @click="$emit('progress')"
      >
        Ver progresso
      </button>
      <button class="btn btn-ghost btn-sm btn-icon" @click="$emit('remove')" title="Remover">
        ✕
      </button>
    </div>

  </div>
</template>

<script setup>
import { computed } from 'vue'

const props = defineProps({
  job: { type: Object, required: true }
})

defineEmits(['open', 'progress', 'remove'])

const isActive = computed(() =>
  props.job.status === 'Queued' || props.job.status === 'Processing'
)

const statusClass = computed(() => {
  if (props.job.status === 'Completed')  return 'green'
  if (props.job.status === 'Failed')     return 'red'
  if (props.job.status === 'Processing') return 'amber'
  return 'muted'
})

const statusText = computed(() => ({
  Queued:     'NA FILA',
  Processing: 'PROCESSANDO',
  Completed:  'CONCLUÍDO',
  Failed:     'FALHOU',
}[props.job.status] ?? props.job.status.toUpperCase()))

function formatTime(iso) {
  if (!iso) return ''
  return new Date(iso).toLocaleString('pt-BR', { hour: '2-digit', minute: '2-digit', second: '2-digit' })
}
</script>

<style scoped>
.job-card {
  display: flex;
  align-items: center;
  gap: 1.5rem;
  padding: 1.25rem 1.5rem;
  border-left: 3px solid var(--border);
  transition: all var(--transition);
}

.job-card--amber { border-left-color: var(--amber); }
.job-card--green { border-left-color: var(--green); }
.job-card--red   { border-left-color: var(--red); }
.job-card--muted { border-left-color: var(--text-muted); }

.job-card-main {
  flex: 1;
  display: flex;
  flex-direction: column;
  gap: 0.6rem;
  min-width: 0;
}

.job-file {
  display: flex;
  align-items: center;
  gap: 0.75rem;
}

.job-file-icon { color: var(--amber); font-size: 1rem; flex-shrink: 0; }

.job-file-info {
  display: flex;
  flex-direction: column;
  gap: 0.15rem;
  min-width: 0;
}

.job-file-name {
  font-size: 0.9rem;
  font-weight: 700;
  color: var(--text-primary);
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.job-id {
  font-size: 0.62rem;
  color: var(--text-muted);
}

.job-progress-area { display: flex; flex-direction: column; gap: 0.4rem; }

.job-status-row {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  font-size: 0.7rem;
}

.status-dot {
  width: 7px;
  height: 7px;
  border-radius: 50%;
  flex-shrink: 0;
}

.dot--amber { background: var(--amber); animation: blink 1s ease infinite; }
.dot--green { background: var(--green); }
.dot--red   { background: var(--red); }
.dot--muted { background: var(--text-muted); }

.status-label {
  letter-spacing: 0.12em;
  color: var(--text-secondary);
}

.job-samples {
  margin-left: auto;
  color: var(--text-muted);
}

.job-track {
  height: 3px;
  background: var(--border);
  border-radius: 2px;
  overflow: hidden;
}

.job-fill {
  height: 100%;
  background: var(--amber);
  border-radius: 2px;
  transition: width 0.6s ease;
  box-shadow: 0 0 8px var(--amber-glow);
}

.job-meta {
  font-size: 0.65rem;
  color: var(--text-muted);
}

.job-card-actions {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  flex-shrink: 0;
}

.btn-sm {
  padding: 0.5rem 1rem;
  font-size: 0.72rem;
}

.btn-icon {
  padding: 0.5rem 0.625rem;
  color: var(--text-muted);
}

.btn-icon:hover {
  color: var(--red);
  border-color: var(--red);
  background: var(--red-dim);
}

@keyframes blink {
  0%, 100% { opacity: 1; }
  50%       { opacity: 0.3; }
}
</style>