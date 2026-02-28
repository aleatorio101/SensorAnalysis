# Sensor Analysis API

API REST em **ASP.NET Core 8** para processamento assíncrono de amostras de sensores ambientais com detecção de anomalias.

---

## Tecnologias e Bibliotecas

| Camada | Tecnologia |
|---|---|
| Runtime | .NET 8 / ASP.NET Core 8 |
| Testes | xUnit + FluentAssertions + NSubstitute |
| Documentação | Swagger / Swashbuckle |
| Detecção de anomalias | Implementação própria: Z-Score + IQR + heurística de sensor travado |

---

## Como executar

### Pré-requisitos
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)

### Rodando a API

```bash
cd SensorAnalysis.API
dotnet run
```

A API ficará disponível em `https://localhost:5001` (ou `http://localhost:5000`).  
O Swagger UI estará acessível em `http://localhost:5000` (raiz).

### Rodando os testes

```bash
cd SensorAnalysis.Tests
dotnet test
```

---

## Arquitetura

```
SensorAnalysis.API/
├── Controllers/
│   └── AnalysisController.cs     # Endpoints REST
├── Models/
│   ├── SensorReading.cs          # Entidade de entrada
│   ├── AnalysisResult.cs         # Resultado por amostra
│   ├── ThresholdConfig.cs        # Limites configuráveis
│   ├── ProcessingJob.cs          # Estado do job assíncrono
│   └── NotificationMessage.cs    # Mensagem de fila
├── Services/
│   ├── IThresholdAnalysisService # Análise de limites por variável
│   ├── IAnomalyDetectionService  # Detecção de anomalias (Z-Score + IQR)
│   ├── ISampleAnalyzerService    # Orquestra threshold + anomalia por amostra
│   ├── IAnalysisOrchestrator     # Gerencia o job assíncrono
│   └── IJobRepository            # Persistência em memória dos jobs
├── Queue/
│   └── INotificationQueue        # Fila de mensagens (ConcurrentQueue)
├── Middleware/
│   └── GlobalExceptionMiddleware # Tratamento centralizado de erros
└── Extensions/
    └── ServiceCollectionExtensions.cs  # Registro de DI
```

**Princípios aplicados:** SOLID, Dependency Injection, Interface Segregation, Clean Architecture por camadas, async/await correto (fire-and-forget com `Task.Run`).

---

## Endpoints

| Método | Rota | Descrição |
|---|---|---|
| `POST` | `/api/analysis/upload` | Upload do JSON e inicia análise |
| `GET` | `/api/analysis/{jobId}/progress` | Progresso do job (polling) |
| `GET` | `/api/analysis/{jobId}/results?type=FM5308` | Resultados completos (opcional: filtra por tipo) |
| `GET` | `/api/analysis/{jobId}/summary` | Apenas o resumo / KPIs do dashboard |

---

## Algoritmos de Detecção de Anomalia

### Z-Score (3σ)
Valores que desviam mais de 3 desvios padrão da média são considerados anomalias.

### IQR (Intervalo Interquartil)
Valores fora de `[Q1 - 1.5*IQR, Q3 + 1.5*IQR]` são considerados anomalias.

### Heurística de Sensor Travado
Sensores em que mais de 80% das leituras de umidade possuem exatamente o mesmo valor são sinalizados como potencialmente travados/com falha.

---

## Limites de Referência (configuráveis em `appsettings.json`)

| Variável | Alert Min | Alert Max | Critical Min | Critical Max |
|---|---|---|---|---|
| Temperatura (°C) | 15 | 30 | 10 | 35 |
| Umidade (%) | 40 | 70 | 30 | 80 |
| Ponto de Orvalho (°C) | — | 22 | — | 25 |

---

## Fila de Notificações

Amostras com status **critical** em qualquer variável, ou classificadas como **anomalia**, são publicadas na fila `log_notifications` (implementada como `ConcurrentQueue<T>` in-memory).

O consumidor da fila **não foi implementado** conforme especificação — apenas a publicação ocorre.

Formato da mensagem:
```json
{
  "sensorId": "sensor_002",
  "timestamp": "2024-11-10T14:23:22Z",
  "motivo": "critical"
}
```
