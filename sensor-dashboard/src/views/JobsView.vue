<template>
  <div class="jobs-view">

    <div class="jobs-header">
      <div>
        <p class="section-label">JOBS</p>
        <h1 class="jobs-title">Painel de <em>Análises</em></h1>
      </div>
      <div class="jobs-actions">
        <button class="btn btn-ghost" @click="router.push({ name: 'upload' })">
          + Novo Upload
        </button>
        <button v-if="jobs.length > 0" class="btn btn-ghost btn-danger" @click="confirmClear">
          Limpar histórico
        </button>
      </div>
    </div>

    <div v-if="jobs.length === 0" class="empty-state">
      <div class="empty-icon">⬡</div>
      <p class="empty-title">Nenhuma análise ainda</p>
      <p class="empty-sub">Faça upload de um arquivo .json para começar.</p>
      <button class="btn btn-primary" @click="router.push({ name: 'upload' })">
        Ir para Upload →
      </button>
    </div>

    <div v-else class="jobs-list">
      <template v-if="activeJobs.length > 0">
        <p class="section-label">EM PROCESSAMENTO</p>
        <div class="job-cards">
          <JobCard
            v-for="job in activeJobs"
            :key="job.jobId"
            :job="job"
            @remove="removeJob(job.jobId)"
            @open="router.push({ name: 'dashboard', params: { id: job.jobId } })"
            @progress="router.push({ name: 'progress', params: { id: job.jobId } })"
          />
        </div>
      </template>

      <template v-if="completedJobs.length > 0">
        <p class="section-label">CONCLUÍDOS</p>
        <div class="job-cards">
          <JobCard
            v-for="job in completedJobs"
            :key="job.jobId"
            :job="job"
            @remove="removeJob(job.jobId)"
            @open="router.push({ name: 'dashboard', params: { id: job.jobId } })"
            @progress="router.push({ name: 'progress', params: { id: job.jobId } })"
          />
        </div>
      </template>

      <template v-if="failedJobs.length > 0">
        <p class="section-label">FALHOS</p>
        <div class="job-cards">
          <JobCard
            v-for="job in failedJobs"
            :key="job.jobId"
            :job="job"
            @remove="removeJob(job.jobId)"
            @open="router.push({ name: 'dashboard', params: { id: job.jobId } })"
            @progress="router.push({ name: 'progress', params: { id: job.jobId } })"
          />
        </div>
      </template>
    </div>

  </div>
</template>

<script setup>
import { onMounted, onUnmounted } from 'vue'
import { useRouter } from 'vue-router'
import { analysisApi } from '@/services/api'
import { useJobStore } from '@/composables/useJobStore'
import JobCard from '@/components/JobCard.vue'

const router = useRouter()
const { jobs, activeJobs, completedJobs, failedJobs, updateJob, removeJob, clearAll } = useJobStore()

let pollTimer = null

async function pollActive() {
  for (const job of activeJobs.value) {
    try {
      const progress = await analysisApi.getProgress(job.jobId)
      updateJob(job.jobId, {
        status:           progress.status,
        processedSamples: progress.processedSamples,
        totalSamples:     progress.totalSamples,
        progressPercent:  progress.progressPercent,
        completedAt:      progress.completedAt,
        errorMessage:     progress.errorMessage,
      })
    } catch {
    }
  }

  if (activeJobs.value.length > 0) {
    pollTimer = setTimeout(pollActive, 1200)
  }
}

onMounted(pollActive)
onUnmounted(() => clearTimeout(pollTimer))

function confirmClear() {
  if (confirm('Remover todos os jobs do histórico?')) clearAll()
}
</script>

<style scoped>
.jobs-view { display: flex; flex-direction: column; gap: 2.5rem; }

.jobs-header {
  display: flex;
  align-items: flex-end;
  justify-content: space-between;
  flex-wrap: wrap;
  gap: 1rem;
}

.jobs-title {
  font-size: clamp(1.5rem, 3vw, 2.25rem);
  font-weight: 800;
}

.jobs-title em { font-style: normal; color: var(--amber); }

.jobs-actions { display: flex; align-items: center; gap: 0.75rem; }

.btn-danger {
  color: var(--red);
  border-color: #f0525233;
}

.btn-danger:hover {
  background: var(--red-dim);
  border-color: var(--red);
}

.empty-state {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 1rem;
  padding: 5rem 2rem;
  text-align: center;
}

.empty-icon {
  font-size: 4rem;
  color: var(--text-muted);
}

.empty-title {
  font-size: 1.1rem;
  font-weight: 700;
  color: var(--text-primary);
}

.empty-sub {
  font-size: 0.8rem;
  color: var(--text-muted);
}

.jobs-list { display: flex; flex-direction: column; gap: 2rem; }

.job-cards { display: flex; flex-direction: column; gap: 0.75rem; }
</style>