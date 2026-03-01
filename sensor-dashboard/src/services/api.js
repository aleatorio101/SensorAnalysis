import axios from 'axios'

const http = axios.create({
  baseURL: '/api',
  timeout: 30_000
})

http.interceptors.response.use(
  res => res,
  err => {
    const message = err.response?.data?.error ?? err.message ?? 'Erro desconhecido'
    return Promise.reject(new Error(message))
  }
)

export const analysisApi = {

  upload(file, onUploadProgress) {
    const form = new FormData()
    form.append('file', file)

    return http.post('/analysis/upload', form, {
      headers: { 'Content-Type': 'multipart/form-data' },
      onUploadProgress: e => {
        if (e.total) onUploadProgress?.(Math.round((e.loaded / e.total) * 100))
      }
    }).then(r => r.data)
  },


  getProgress(jobId) {
    return http.get(`/analysis/${jobId}/progress`).then(r => r.data)
  },

  getSummary(jobId) {
    return http.get(`/analysis/${jobId}/summary`).then(r => r.data)
  },

  getResults(jobId, type = null) {
    const params = type ? { type } : {}
    return http.get(`/analysis/${jobId}/results`, { params }).then(r => r.data)
  }
}
