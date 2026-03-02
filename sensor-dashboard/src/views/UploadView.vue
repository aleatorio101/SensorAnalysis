<template>
  <div class="upload-view">
    <div class="upload-header">
      <p class="section-label">INÍCIO</p>
      <h1 class="upload-title">
        Processamento<br />
        <em>Assíncrono</em> de Amostras
      </h1>
      <p class="upload-desc">
        Carregue um ou mais arquivos <code>.json</code> para iniciar análises simultâneas.
        Cada arquivo vira um job independente.
      </p>
    </div>

    <div
      class="drop-zone"
      :class="{ 'drop-zone--active': isDragging, 'drop-zone--ready': selectedFiles.length > 0 }"
      @dragenter.prevent="isDragging = true"
      @dragleave.prevent="isDragging = false"
      @dragover.prevent
      @drop.prevent="onDrop"
      @click="fileInput.click()"
    >
      <input
        ref="fileInput"
        type="file"
        accept=".json"
        multiple
        class="hidden-input"
        @change="onFileChange"
      />

      <div v-if="selectedFiles.length === 0" class="drop-content">
        <div class="drop-icon">⬡</div>
        <p class="drop-main">Arraste os arquivos aqui</p>
        <p class="drop-sub">ou clique para selecionar &mdash; múltiplos <code>.json</code> aceitos</p>
      </div>

      <div v-else class="files-list">
        <div
          v-for="(f, idx) in selectedFiles"
          :key="idx"
          class="file-row"
          @click.stop
        >
          <div class="file-icon">◈</div>
          <div class="file-info">
            <span class="file-name">{{ f.name }}</span>
            <span class="file-size">{{ formatBytes(f.size) }}</span>
          </div>

          <div class="file-upload-progress" v-if="uploadProgressMap[idx] > 0 && uploadProgressMap[idx] < 100">
            <div class="mini-bar">
              <div class="mini-fill" :style="{ width: uploadProgressMap[idx] + '%' }" />
            </div>
            <span class="mini-pct">{{ uploadProgressMap[idx] }}%</span>
          </div>
          <span v-else-if="uploadProgressMap[idx] === 100" class="file-done">✓</span>
          <button class="file-clear" @click.stop="removeFile(idx)">✕</button>
        </div>

        <button class="add-more-btn" @click.stop="fileInput.click()">
          + adicionar mais arquivos
        </button>
      </div>
    </div>

    <div v-if="error" class="error-banner">
      <span class="error-icon">⚠</span>
      {{ error }}
    </div>

    <div v-if="activeJobs.length > 0" class="active-jobs-hint">
      <span class="hint-icon">◎</span>
      <span>{{ activeJobs.length }} job(s) em processamento —</span>
      <router-link :to="{ name: 'jobs' }" class="hint-link">ver painel de jobs →</router-link>
    </div>

    <div class="upload-actions">
      <router-link :to="{ name: 'jobs' }" class="btn btn-ghost" v-if="jobs.length > 0">
        VER JOBS ({{ jobs.length }})
      </router-link>
      <button
        class="btn btn-primary btn-lg"
        :disabled="selectedFiles.length === 0 || isUploading"
        @click="startAnalysis"
      >
        <span v-if="isUploading">ENVIANDO {{ uploadingIndex + 1 }}/{{ selectedFiles.length }}...</span>
        <span v-else>INICIAR {{ selectedFiles.length > 1 ? selectedFiles.length + ' ANÁLISES' : 'ANÁLISE' }} →</span>
      </button>
    </div>
  </div>
</template>

<script setup>
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { analysisApi } from '@/services/api'
import { useJobStore } from '@/composables/useJobStore'

const router = useRouter()
const { jobs, activeJobs, addJob } = useJobStore()

const fileInput        = ref(null)
const selectedFiles    = ref([])
const isDragging       = ref(false)
const isUploading      = ref(false)
const uploadProgressMap = ref({})
const uploadingIndex   = ref(0)
const error            = ref('')

function onFileChange(e) {
  const files = Array.from(e.target.files ?? [])
  addFiles(files)
  e.target.value = ''
}

function onDrop(e) {
  isDragging.value = false
  const files = Array.from(e.dataTransfer.files ?? []).filter(f => f.name.endsWith('.json'))
  if (files.length === 0) {
    error.value = 'Apenas arquivos .json são aceitos.'
    return
  }
  addFiles(files)
}

function addFiles(files) {
  error.value = ''
  for (const f of files) {
    if (!f.name.endsWith('.json')) continue
    if (!selectedFiles.value.find(x => x.name === f.name && x.size === f.size)) {
      selectedFiles.value.push(f)
    }
  }
}

function removeFile(idx) {
  selectedFiles.value.splice(idx, 1)
  delete uploadProgressMap.value[idx]
}

async function startAnalysis() {
  if (selectedFiles.value.length === 0) return

  isUploading.value = true
  error.value = ''

  const createdJobs = []

  for (let i = 0; i < selectedFiles.value.length; i++) {
    uploadingIndex.value = i
    const file = selectedFiles.value[i]

    try {
      const { jobId } = await analysisApi.upload(
        file,
        p => { uploadProgressMap.value = { ...uploadProgressMap.value, [i]: p } }
      )
      uploadProgressMap.value = { ...uploadProgressMap.value, [i]: 100 }

      addJob({ jobId, fileName: file.name, totalSamples: 0 })
      createdJobs.push(jobId)
    } catch (err) {
      error.value = `Erro no arquivo "${file.name}": ${err.message}`
      isUploading.value = false
      return
    }
  }

  isUploading.value = false

  if (createdJobs.length === 1) {
    router.push({ name: 'progress', params: { id: createdJobs[0] } })
  } else {
    router.push({ name: 'jobs' })
  }
}

function formatBytes(bytes) {
  if (bytes < 1024)       return `${bytes} B`
  if (bytes < 1024 ** 2)  return `${(bytes / 1024).toFixed(1)} KB`
  return `${(bytes / 1024 ** 2).toFixed(1)} MB`
}
</script>

<style scoped>
.upload-view {
  max-width: 680px;
  margin: 0 auto;
  display: flex;
  flex-direction: column;
  gap: 2rem;
}

.upload-header { display: flex; flex-direction: column; gap: 1rem; }

.upload-title {
  font-size: clamp(2rem, 5vw, 3.25rem);
  font-weight: 800;
  line-height: 1.1;
  color: var(--text-primary);
}

.upload-title em { font-style: normal; color: var(--amber); }

.upload-desc {
  color: var(--text-secondary);
  font-size: 0.85rem;
  line-height: 1.7;
}

.upload-desc code {
  color: var(--amber);
  background: var(--amber-dim);
  padding: 1px 6px;
  border-radius: 3px;
}

.drop-zone {
  border: 2px dashed var(--border-light);
  border-radius: var(--radius-lg);
  padding: 3rem 2rem;
  cursor: pointer;
  text-align: center;
  transition: all var(--transition);
  background: var(--bg-surface);
  position: relative;
}

.drop-zone:hover,
.drop-zone--active {
  border-color: var(--amber);
  background: var(--amber-dim);
}

.drop-zone--ready {
  border-style: solid;
  border-color: var(--amber);
  background: var(--amber-dim);
  padding: 1.5rem 2rem;
  text-align: left;
  cursor: default;
}

.hidden-input { display: none; }

.drop-icon {
  font-size: 3rem;
  color: var(--text-muted);
  margin-bottom: 1rem;
  display: block;
  transition: color var(--transition);
}

.drop-zone:hover .drop-icon,
.drop-zone--active .drop-icon { color: var(--amber); }

.drop-main {
  font-size: 1.1rem;
  font-weight: 700;
  color: var(--text-primary);
  margin-bottom: 0.5rem;
}

.drop-sub { font-size: 0.75rem; color: var(--text-muted); }
.drop-sub code { color: var(--amber); }

.files-list {
  display: flex;
  flex-direction: column;
  gap: 0.75rem;
}

.file-row {
  display: flex;
  align-items: center;
  gap: 0.875rem;
  background: var(--bg-elevated);
  border: 1px solid var(--border);
  border-radius: var(--radius-md);
  padding: 0.75rem 1rem;
  transition: border-color var(--transition);
}

.file-row:hover { border-color: var(--border-light); }

.file-icon { font-size: 1.25rem; color: var(--amber); flex-shrink: 0; }

.file-info {
  flex: 1;
  display: flex;
  flex-direction: column;
  gap: 0.2rem;
  min-width: 0;
}

.file-name {
  font-size: 0.85rem;
  font-weight: 700;
  color: var(--text-primary);
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.file-size { font-size: 0.68rem; color: var(--text-muted); }

.file-upload-progress {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  flex-shrink: 0;
}

.mini-bar {
  width: 80px;
  height: 3px;
  background: var(--border);
  border-radius: 2px;
  overflow: hidden;
}

.mini-fill {
  height: 100%;
  background: var(--amber);
  border-radius: 2px;
  transition: width 0.3s ease;
}

.mini-pct { font-size: 0.65rem; color: var(--text-muted); }

.file-done {
  color: var(--green);
  font-size: 1rem;
  flex-shrink: 0;
}

.file-clear {
  background: none;
  border: 1px solid var(--border);
  color: var(--text-muted);
  width: 26px;
  height: 26px;
  border-radius: var(--radius-sm);
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 0.65rem;
  transition: all var(--transition);
  flex-shrink: 0;
}

.file-clear:hover {
  border-color: var(--red);
  color: var(--red);
  background: var(--red-dim);
}

.add-more-btn {
  background: transparent;
  border: 1px dashed var(--border-light);
  color: var(--text-muted);
  border-radius: var(--radius-md);
  padding: 0.6rem;
  font-family: var(--font-mono);
  font-size: 0.7rem;
  letter-spacing: 0.08em;
  cursor: pointer;
  transition: all var(--transition);
  width: 100%;
  text-align: center;
}

.add-more-btn:hover {
  border-color: var(--amber);
  color: var(--amber);
  background: var(--amber-dim);
}

.active-jobs-hint {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  font-size: 0.75rem;
  color: var(--text-muted);
  background: var(--bg-surface);
  border: 1px solid var(--border);
  border-radius: var(--radius-md);
  padding: 0.75rem 1rem;
}

.hint-icon { color: var(--amber); animation: spin 2s linear infinite; }

.hint-link {
  color: var(--amber);
  text-decoration: none;
  margin-left: auto;
}

.hint-link:hover { text-decoration: underline; }

.error-banner {
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

.upload-actions {
  display: flex;
  justify-content: flex-end;
  align-items: center;
  gap: 1rem;
}

.btn-lg {
  padding: 1rem 2.5rem;
  font-size: 0.85rem;
  letter-spacing: 0.1em;
}

@keyframes spin { to { transform: rotate(360deg); } }
</style>