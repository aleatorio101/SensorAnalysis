<template>
  <div class="donut-wrapper">
    <Doughnut :data="chartData" :options="chartOptions" />
  </div>
</template>

<script setup>
import { computed } from 'vue'
import { Doughnut } from 'vue-chartjs'
import { Chart as ChartJS, ArcElement, Tooltip, Legend } from 'chart.js'

ChartJS.register(ArcElement, Tooltip, Legend)

const props = defineProps({
  summary: { type: Object, required: true }
})

const chartData = computed(() => ({
  labels: ['Normal', 'Alerta', 'Crítico', 'Anomalia', 'Inválido'],
  datasets: [{
    data: [
      props.summary.totalNormal,
      (props.summary.tempAlertMaxCount  + props.summary.humidityAlertMaxCount  +
       props.summary.tempAlertMinCount  + props.summary.humidityAlertMinCount  +
       props.summary.dewPointAlertMaxCount),
      (props.summary.tempCriticalMaxCount  + props.summary.humidityCriticalMaxCount  +
       props.summary.tempCriticalMinCount  + props.summary.humidityCriticalMinCount  +
       props.summary.dewPointCriticalMaxCount),
      props.summary.totalAnomaly,
      props.summary.totalInvalid
    ],
    backgroundColor: ['#3dd68c33', '#f0a50033', '#f0525233', '#f0785033', '#ffffff11'],
    borderColor:     ['#3dd68c',   '#f0a500',   '#f05252',   '#f07850',   '#555a68'],
    borderWidth: 2,
    hoverBorderWidth: 3
  }]
}))

const chartOptions = {
  responsive: true,
  maintainAspectRatio: false,
  cutout: '72%',
  plugins: {
    legend: {
      position: 'bottom',
      labels: {
        color: '#8b909e',
        font: {size: 10 },
        padding: 16,
        boxWidth: 10
      }
    },
    tooltip: {
      backgroundColor: '#1e2128',
      borderColor: '#2a2d35',
      borderWidth: 1,
      titleColor: '#e8eaf0',
      bodyColor: '#8b909e',
      titleFont: { size: 11 },
      bodyFont: { size: 11 }
    }
  }
}
</script>

<style scoped>
.donut-wrapper {
  height: 280px;
  position: relative;
}
</style>
