import { ref, computed } from 'vue'

const jobs = ref(JSON.parse(localStorage.getItem('sensor_jobs') ?? '[]'))

function persist() {
  localStorage.setItem('sensor_jobs', JSON.stringify(jobs.value))
}

export function useJobStore() {
  function addJob({ jobId, fileName, totalSamples }) {
    jobs.value.unshift({
      jobId,
      fileName,
      totalSamples,
      status: 'Queued',
      processedSamples: 0,
      progressPercent: 0,
      createdAt: new Date().toISOString(),
      completedAt: null,
      errorMessage: null,
    })
    persist()
  }

  function updateJob(jobId, patch) {
    const idx = jobs.value.findIndex(j => j.jobId === jobId)
    if (idx === -1) return
    jobs.value[idx] = { ...jobs.value[idx], ...patch }
    persist()
  }

  function removeJob(jobId) {
    jobs.value = jobs.value.filter(j => j.jobId !== jobId)
    persist()
  }

  function clearAll() {
    jobs.value = []
    persist()
  }

  const activeJobs = computed(() =>
    jobs.value.filter(j => j.status === 'Queued' || j.status === 'Processing')
  )

  const completedJobs = computed(() =>
    jobs.value.filter(j => j.status === 'Completed')
  )

  const failedJobs = computed(() =>
    jobs.value.filter(j => j.status === 'Failed')
  )

  return { jobs, activeJobs, completedJobs, failedJobs, addJob, updateJob, removeJob, clearAll }
}