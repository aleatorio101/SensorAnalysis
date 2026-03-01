<template>
  <div class="app-shell">
    <header class="app-header">
      <div class="header-inner">
        <div class="logo">
          <span class="logo-bracket">[</span>
          <span class="logo-text">SENSOR<em>ANALYSIS</em></span>
          <span class="logo-bracket">]</span>
        </div>
        <div class="header-meta">
          <span class="meta-tag">v1.0.0</span>
          <span class="meta-divider">|</span>
          <span class="meta-tag">AMBIENTAL</span>
        </div>
      </div>
    </header>

    <main class="app-main">
      <RouterView v-slot="{ Component }">
        <Transition name="fade" mode="out-in">
          <component :is="Component" />
        </Transition>
      </RouterView>
    </main>

    <footer class="app-footer">
      <span>SENSOR ANALYSIS SYSTEM</span>
      <span class="footer-dot">◆</span>
      <span>PROCESSAMENTO ASSÍNCRONO DE AMOSTRAS</span>
    </footer>
  </div>
</template>

<script setup>
import { RouterView } from 'vue-router'
</script>
