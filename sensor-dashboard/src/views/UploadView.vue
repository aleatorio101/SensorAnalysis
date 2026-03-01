<template>
  <div class="upload-view">
    <div class="upload-header">
      <p class="section-label">INÍCIO</p>
      <h1 class="upload-title">
        Processamento<br />
        <em>Assíncrono</em> de Amostras
      </h1>
      <p class="upload-desc">
        Carregue um arquivo <code>.json</code> contendo as leituras de sensores
        ambientais para iniciar a análise de limiares e detecção de anomalias.
      </p>
    </div>

    <div
      class="drop-zone"
      :class="{ 'drop-zone--active': isDragging, 'drop-zone--ready': !!selectedFile }"
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
        class="hidden-input"
        @change="onFileChange"
      />

      <div v-if="!selectedFile" class="drop-content">
        <div class="drop-icon">⬡</div>
        <p class="drop-main">Arraste o arquivo aqui</p>
        <p class="drop-sub">ou clique para selecionar &mdash; apenas <code>.json</code></p>
      </div>

      <div v-else class="file-preview">
        <div class="file-icon">◈</div>
        <div class="file-info">
          <span class="file-name">{{ selectedFile.name }}</span>
          <span class="file-size">{{ formatBytes(selectedFile.size) }}</span>
        </div>
        <button class="file-clear" @click.stop="clearFile">✕</button>
      </div>
    </div>

    <div v-if="uploadProgress > 0 && uploadProgress < 100" class="upload-progress">
      <div class="progress-label">
        <span>ENVIANDO</span>
        <span>{{ uploadProgress }}%</span>
      </div>
      <div class="progress-bar">
        <div class="progress-fill" :style="{ width: uploadProgress + '%' }" />
      </div>
    </div>

    <div v-if="error" class="error-banner">
      <span class="error-icon">⚠</span>
      {{ error }}
    </div>

    <div class="upload-actions">
      <button
        class="btn btn-primary btn-lg"
        :disabled="!selectedFile || isUploading"
        @click="startAnalysis"
      >
        <span v-if="isUploading">PROCESSANDO...</span>
        <span v-else>INICIAR ANÁLISE →</span>
      </button>
    </div>
  </div>
</template>

<script setup>
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { analysisApi } from '@/services/api'

const router = useRouter()

const fileInput     = ref(null)
const selectedFile  = ref(null)
const isDragging    = ref(false)
const isUploading   = ref(false)
const uploadProgress = ref(0)
const error         = ref('')

function onFileChange(e) {
  const file = e.target.files?.[0]
  if (file) setFile(file)
}

function onDrop(e) {
  isDragging.value = false
  const file = e.dataTransfer.files?.[0]
  if (file?.name.endsWith('.json')) setFile(file)
  else error.value = 'Apenas arquivos .json são aceitos.'
}

function setFile(file) {
  selectedFile.value = file
  error.value = ''
  uploadProgress.value = 0
}

function clearFile() {
  selectedFile.value = null
  uploadProgress.value = 0
  error.value = ''
  if (fileInput.value) fileInput.value.value = ''
}

async function startAnalysis() {
  if (!selectedFile.value) return

  isUploading.value   = true
  uploadProgress.value = 0
  error.value          = ''

  try {
    const { jobId } = await analysisApi.upload(
      selectedFile.value,
      p => { uploadProgress.value = p }
    )
    router.push({ name: 'progress', params: { id: jobId } })
  } catch (err) {
    error.value = err.message
    isUploading.value = false
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

.upload-title em {
  font-style: normal;
  color: var(--amber);
}

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

.drop-sub {
  font-size: 0.75rem;
  color: var(--text-muted);
}

.drop-sub code {
  color: var(--amber);
}

.file-preview {
  display: flex;
  align-items: center;
  gap: 1rem;
}

.file-icon {
  font-size: 2rem;
  color: var(--amber);
}

.file-info {
  flex: 1;
  display: flex;
  flex-direction: column;
  gap: 0.25rem;
  text-align: left;
}

.file-name {
  font-size: 0.9rem;
  font-weight: 700;
  color: var(--text-primary);
  word-break: break-all;
}

.file-size {
  font-size: 0.7rem;
  color: var(--text-muted);
}

.file-clear {
  background: none;
  border: 1px solid var(--border);
  color: var(--text-muted);
  width: 28px;
  height: 28px;
  border-radius: var(--radius-sm);
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 0.7rem;
  transition: all var(--transition);
  flex-shrink: 0;
}

.file-clear:hover {
  border-color: var(--red);
  color: var(--red);
  background: var(--red-dim);
}

.upload-progress { display: flex; flex-direction: column; gap: 0.5rem; }

.progress-label {
  display: flex;
  justify-content: space-between;
  font-size: 0.65rem;
  letter-spacing: 0.1em;
  color: var(--text-muted);
}

.progress-bar {
  height: 3px;
  background: var(--border);
  border-radius: 2px;
  overflow: hidden;
}

.progress-fill {
  height: 100%;
  background: var(--amber);
  border-radius: 2px;
  transition: width 0.3s ease;
  box-shadow: 0 0 8px var(--amber-glow);
}

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

.error-icon { font-size: 1rem; }

.upload-actions { display: flex; justify-content: flex-end; }

.btn-lg {
  padding: 1rem 2.5rem;
  font-size: 0.85rem;
  letter-spacing: 0.1em;
}
</style>
