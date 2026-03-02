# Sensor Analysis

Sistema completo para processamento assíncrono de amostras de sensores ambientais com detecção de anomalias, fila de notificações via RabbitMQ e dashboard interativo com suporte a múltiplas análises simultâneas.

---

## Tecnologias e Bibliotecas

| Camada | Tecnologia |
|---|---|
| Backend | .NET 8 / ASP.NET Core 8 |
| Frontend | Vue 3 + Vite + Chart.js |
| Fila de mensagens | RabbitMQ 3.13 + RabbitMQ.Client 6.8.1 |
| Testes | xUnit + FluentAssertions + NSubstitute |
| Documentação | Swagger / Swashbuckle |
| Detecção de anomalias | Implementação própria: Z-Score + IQR + heurística de sensor travado |
| Infraestrutura | Docker + docker-compose |

---

## Como executar

### Pré-requisitos
- [Docker Desktop](https://www.docker.com/products/docker-desktop)

### Rodando tudo junto
```bash
docker-compose up --build
```

| Serviço | URL |
|---|---|
| Frontend | http://localhost:3000 |
| API | http://localhost:5000 |
| Swagger | http://localhost:5000 |
| RabbitMQ UI | http://localhost:15672 (guest/guest) |

### Rodando localmente (sem Docker)

**API:**
```bash
cd SensorAnalysis.API
dotnet run
```

**Frontend:**
```bash
cd sensor-dashboard
npm install
npm run dev
```

> O Vite faz proxy de `/api` para `http://localhost:5000` automaticamente.

### Rodando os testes
```bash
cd SensorAnalysis.Tests
dotnet test
```

---

## Estrutura do Projeto

```
SensorAnalysis/
├── docker-compose.yml
├── README.md
├── SensorAnalysis.sln
├── SensorAnalysis.API/
│   ├── Controllers/
│   │   └── AnalysisController.cs          # Endpoints REST
│   ├── Models/
│   │   ├── SensorReading.cs               # Entidade de entrada
│   │   ├── AnalysisResult.cs              # Resultado por amostra
│   │   ├── ThresholdConfig.cs             # Limites configuráveis
│   │   ├── ProcessingJob.cs               # Estado do job + DashboardSummary
│   │   └── NotificationMessage.cs         # Mensagem de fila
│   ├── Services/
│   │   ├── IThresholdAnalysisService      # Análise de limites por variável
│   │   ├── IAnomalyDetectionService       # Detecção de anomalias (Z-Score + IQR)
│   │   ├── ISampleAnalyzerService         # Fit estatístico + análise por amostra
│   │   ├── IAnalysisOrchestrator          # Enfileira jobs no Channel
│   │   ├── IAnalysisJobProcessor          # Processa um job (testável isoladamente)
│   │   ├── AnalysisBackgroundService      # IHostedService — consome o Channel
│   │   └── IJobRepository                 # Persistência em memória dos jobs
│   ├── Queue/
│   │   ├── RabbitMqNotificationQueue      # Publicação na fila RabbitMQ
│   │   └── InMemoryNotificationQueue      # Fallback para ambiente sem RabbitMQ
│   ├── Middleware/
│   │   └── GlobalExceptionMiddleware      # Tratamento centralizado de erros
│   └── Extensions/
│       ├── ServiceCollectionExtensions.cs # Registro de DI + Channel
│       └── RabbitMqExtensions.cs          # Registro da conexão RabbitMQ
├── SensorAnalysis.Tests/
│   ├── ThresholdAnalysisServiceTests.cs
│   └── AnomalyDetectionServiceTests.cs
└── sensor-dashboard/
    ├── Dockerfile
    ├── nginx.conf
    └── src/
        ├── main.js                        # App + roteador
        ├── views/
        │   ├── UploadView.vue             # Upload de múltiplos arquivos simultâneos
        │   ├── JobsView.vue               # Painel de todos os jobs com polling
        │   ├── ProgressView.vue           # Progresso individual com timeline
        │   └── DashboardView.vue          # Dashboard com KPIs e gráficos
        ├── components/
        │   ├── JobCard.vue                # Card de job com progresso em tempo real
        │   ├── KpiCard.vue                # Card reutilizável de métrica
        │   ├── StatusBadge.vue            # Badge animado de status
        │   ├── TimeSeriesChart.vue        # Gráfico temporal paginado com limites
        │   └── SummaryDonut.vue           # Donut chart de distribuição
        ├── composables/
        │   ├── usePolling.js              # Polling genérico com stop automático
        │   └── useJobStore.js             # Estado global de jobs + localStorage
        └── services/
            └── api.js                     # Camada HTTP com Axios
```

**Princípios aplicados:** SOLID, Dependency Injection, Interface Segregation, Clean Architecture por camadas, processamento assíncrono via `BackgroundService` + `Channel<T>`, paralelismo com `Parallel.ForEachAsync`.

---

## Endpoints

| Método | Rota | Descrição |
|---|---|---|
| `POST` | `/api/analysis/upload` | Upload do JSON e inicia análise |
| `GET` | `/api/analysis/{jobId}/progress` | Progresso do job (polling) |
| `GET` | `/api/analysis/{jobId}/results?type=FM5308` | Resultados completos (opcional: filtra por tipo) |
| `GET` | `/api/analysis/{jobId}/summary` | Apenas o resumo / KPIs do dashboard |

---

## Arquitetura de Processamento Assíncrono

O processamento de jobs segue um modelo produtor/consumidor desacoplado:

1. O `AnalysisController` recebe o upload e chama o `AnalysisOrchestrator`
2. O `AnalysisOrchestrator` cria o registro do job e publica um `AnalysisWorkItem` no `Channel<T>`
3. O `AnalysisBackgroundService` (`IHostedService`) consome o Channel e delega para o `AnalysisJobProcessor`
4. O `AnalysisJobProcessor` executa o `Fit` estatístico sequencialmente e depois processa as amostras em paralelo via `Parallel.ForEachAsync`

Esse modelo garante que o processamento sobrevive ao ciclo de vida da requisição HTTP e é gerenciado corretamente pelo host do ASP.NET Core.

---

## Algoritmos de Detecção de Anomalia

### Z-Score (3σ)
Valores que desviam mais de 3 desvios padrão da média são considerados anomalias.

### IQR (Intervalo Interquartil)
Valores fora de `[Q1 - 1.5*IQR, Q3 + 1.5*IQR]` são considerados anomalias.

### Heurística de Sensor Travado
Sensores em que mais de 80% das leituras de umidade possuem exatamente o mesmo valor são sinalizados como potencialmente travados/com falha.

O `Fit` estatístico é calculado uma vez sobre o conjunto completo antes da análise paralela, garantindo que o contexto (médias, desvios, quartis) seja consistente para todas as amostras do job.

---

## Limites de Referência (configuráveis em `appsettings.json`)

| Variável | Alert Min | Alert Max | Critical Min | Critical Max |
|---|---|---|---|---|
| Temperatura (°C) | 15 | 30 | 10 | 35 |
| Umidade (%) | 40 | 70 | 30 | 80 |
| Ponto de Orvalho (°C) | — | 22 | — | 25 |

---

## Fila de Notificações

Amostras com status **critical** em qualquer variável, ou classificadas como **anomalia**, são publicadas na fila `log_notifications` via **RabbitMQ**.

A fila é declarada como **durable** — mensagens sobrevivem a restarts do broker. A conexão possui **reconexão automática** configurada com intervalo de 10 segundos.

O consumidor da fila **não foi implementado** conforme especificação — apenas a publicação ocorre.

Formato da mensagem:
```json
{
  "sensorId": "sensor_002",
  "timestamp": "2024-11-10T14:23:22Z",
  "motivo": "critical"
}
```

---

## Upload de Múltiplos Arquivos

A interface suporta o envio de múltiplos arquivos `.json` simultaneamente. Cada arquivo gera um job independente, visível no painel de jobs (`/jobs`) com progresso em tempo real e acesso direto ao dashboard de cada análise. O histórico de jobs é mantido no `localStorage` do navegador.