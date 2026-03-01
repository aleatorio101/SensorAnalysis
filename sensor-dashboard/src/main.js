import { createApp } from 'vue'
import { createRouter, createWebHistory } from 'vue-router'
import App from './App.vue'
import './assets/main.css'

import UploadView   from '@/views/UploadView.vue'
import ProgressView from '@/views/ProgressView.vue'
import DashboardView from '@/views/DashboardView.vue'

const router = createRouter({
  history: createWebHistory(),
  routes: [
    { path: '/',              name: 'upload',    component: UploadView },
    { path: '/progress/:id',  name: 'progress',  component: ProgressView },
    { path: '/dashboard/:id', name: 'dashboard', component: DashboardView }
  ]
})

createApp(App).use(router).mount('#app')
