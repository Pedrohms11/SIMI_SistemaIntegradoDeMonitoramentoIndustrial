
# Documentação Técnica do Sistema Integrado de Monitoramento Industrial


## 1. Introdução

O Sistema Integrado de Monitoramento Industrial foi desenvolvido para atender à necessidade de coleta, validação e visualização de dados provenientes de sensores industriais, especificamente monitorando variáveis de temperatura e ângulo. A arquitetura distribuída permite o monitoramento em tempo real, com validações configuráveis e interface gráfica intuitiva.

### 1.1 Objetivos do Sistema

- Coletar dados de sensores via requisições HTTP
- Validar dados segundo parâmetros configuráveis e limites físicos
- Armazenar temporalmente as leituras realizadas
- Disponibilizar interface gráfica para visualização dos dados históricos
- Simular o comportamento de sensores reais para testes e demonstração

## 2. Arquitetura do Sistema

### 2.1 Visão Geral da Arquitetura

```
┌─────────────────────┐         ┌──────────────────────┐
│   Simulador de      │  HTTP   │   API de Processamento│
│   Sensores (Console)│ ──────> │   (ASP.NET Core)      │
│                     │   POST  │                       │
└─────────────────────┘         └──────────┬───────────┘
                                            │
                                            │ HTTP GET
                                            ▼
                                 ┌──────────────────────┐
                                 │  Interface WPF       │
                                 │  (SensorInterface)   │
                                 └──────────────────────┘
```

### 2.2 Componentes do Sistema

| Componente | Tecnologia | Função |
|------------|------------|--------|
| API de Processamento | .NET 8 / ASP.NET Core | Receber, validar e armazenar dados dos sensores |
| Interface WPF | .NET 8 / WPF / MVVM | Visualizar dados históricos através de gráficos |
| Simulador de Sensores | .NET 8 / Console | Simular envio periódico de leituras de sensores |
| Biblioteca Compartilhada | .NET 8 / Class Library | Compartilhar modelos de dados entre componentes |

### 2.3 Modelo de Dados (SensorData)

```csharp
public class SensorData
{
    public int Id { get; set; }
    public double Temperatura { get; set; }
    public int Angulo { get; set; }
    public DateTime Timestamp { get; set; }
}
```

## 3. Configuração e Execução

### 3.1 Pré-requisitos

- **.NET 8 SDK** ou superior
- **Visual Studio 2022** 
- **Git** (opcional, para clonagem do repositório)

### 3.2 Obtenção do Código Fonte

```bash
git clone [url-do-repositorio]
cd SistemaMonitoramentoIndustrial
```

### 3.3 Estrutura de Diretórios

```
SistemaMonitoramentoIndustrial/
├── ApiProcessamento/           # API RESTful
│   ├── Controllers/
│   ├── Config/
│   └── appsettings.json
├── SensorInterface/            # Interface WPF
│   ├── ViewModel/
│   ├── View/
│   └── Command/
├── SensorSimulator/            # Simulador Console
│   └── Program.cs
└── Shared/                     # Biblioteca Compartilhada
    └── SensorData.cs
```

### 3.4 Configuração da API

O arquivo `appsettings.json` da API contém as configurações de validação:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=app.db"
  },
  "ApiConfig": {
    "MaxTemperatura": 80,
    "MaxAngulo": 180
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}

```

### 3.5 Instruções de Execução

#### Passo 1: Executar a API de Processamento

```bash
cd ApiProcessamento
dotnet run
```

**Verificação:** A API estará disponível em `https://localhost:7157`

#### Passo 2: Executar o Simulador de Sensores

```bash
cd SensorSimulator
dotnet run
```

**Comportamento esperado:** 
- Envia dados a cada 2 segundos
- Temperatura: valores aleatórios entre 20-100°C
- Ângulo: valores aleatórios entre 0-359 graus

#### Passo 3: Executar a Interface WPF

```bash
cd SensorInterface
dotnet run
```

**Ou:** Abrir a solução no Visual Studio e executar o projeto `SensorInterface`

## 4. Endpoints da API

### 4.1 POST /api/v1/sensores

**Descrição:** Recebe e valida dados de um sensor

**Corpo da Requisição:**
```json
{
  "id": 1,
  "temperatura": 85.5,
  "angulo": 180,
  "timestamp": "2024-01-15T10:30:00Z"
}
```

**Regras de Validação:**
| Campo | Regra |
|-------|-------|
| Temperatura | ≤ MaxTemperatura (80) e ≥ -50 |
| Ângulo | ≤ MaxAngulo (180) e ≥ 0 |
| Timestamp | Não pode ser futuro (tolerância 5 min) |
| Timestamp | Não pode ser anterior a 1 ano |

**Respostas Possíveis:**
- `200 OK`: Dados válidos e armazenados
- `400 Bad Request`: Dados inválidos (com detalhes do erro)
- `500 Internal Server Error`: Erro interno no servidor

### 4.2 GET /api/v1/sensores

**Descrição:** Lista todas as leituras de sensores armazenadas

**Parâmetros (Query String):**
| Parâmetro | Tipo | Obrigatório | Descrição |
|-----------|------|-------------|-----------|
| dataInicio | DateTime | Não | Filtro de data inicial |
| dataFim | DateTime | Não | Filtro de data final |

**Exemplo de Uso:**
```
GET /api/v1/sensores?dataInicio=2024-01-01&dataFim=2024-01-31
```

## 5. Interface WPF - Funcionalidades

### 5.1 Componentes da Interface

- **Botão "Carregar Sensores"** → Dispara requisição GET para a API
- **Lista de Temperaturas** → Exibe todas as temperaturas registradas
- **Lista de Ângulos** → Exibe todos os ângulos registrados

### 5.2 Padrão MVVM Implementado

```
View (MainWindow.xaml)
         ↓
ViewModel (MainViewModel.cs)
         ↓
Model (SensorData - Shared)
```

### 5.3 Comandos Disponíveis

| Comando | Método | Descrição |
|---------|--------|-----------|
| CarregarSensoresCommand | CarregarSensores() | Obtém todos os dados da API e atualiza as coleções |

## 6. Fluxo de Dados do Sistema

```
1. Simulador → Gera dados aleatórios (temp, ângulo, timestamp)
              ↓
2. Simulador → Envia POST para API (localhost:7157/api/v1/sensores)
              ↓
3. API → Valida dados (limites configuráveis e físicos)
              ↓
4. API → Armazena em memória (List<SensorData>)
              ↓
5. API → Retorna resposta de sucesso/erro
              ↓
6. Interface WPF → Botão "Carregar" → GET /api/v1/sensores
              ↓
7. Interface WPF → Exibe dados em ObservableCollections
```

## 7. Tratamento de Erros

### 7.1 Tipos de Erros Tratados

| Componente | Tipo de Erro | Tratamento |
|------------|--------------|-------------|
| API | Dados inválidos | BadRequest com mensagem específica |
| API | Exceção interna | StatusCode 500 com detalhes |
| Simulador | HTTP não sucedido | Console.WriteLine com status |
| Interface WPF | Requisição falha | Exceção não tratada (a ser implementado) |

### 8.2 Melhorias Recomendadas

1. **Interface WPF:** Implementar tratamento de exceções no método `CarregarSensores()`
2. **Persistência:** Substituir armazenamento em memória por banco de dados
3. **Autenticação:** Adicionar JWT ou API Keys para segurança
4. **Logging:** Implementar ILogger para rastreamento
5. **Gráficos:** Substituir ListBox por controles gráficos (OxyPlot, LiveCharts)

## 9. Considerações Finais

O sistema atende aos requisitos propostos de monitoramento industrial, demonstrando conceitos fundamentais de desenvolvimento de software:

- **Arquitetura distribuída** com separação clara de responsabilidades
- **Comunicação HTTP** entre componentes via API RESTful
- **Validação robusta** de dados com configuração externa
- **Padrão MVVM** na interface gráfica WPF

A documentação apresentada permite a reprodução do ambiente por outros desenvolvedores, contribuindo para a disseminação do conhecimento em sistemas de monitoramento industrial.

## 10. Referências

- Microsoft. (2024). *ASP.NET Core Documentation*. Microsoft Learn.
- Microsoft. (2024). *WPF Documentation*. Microsoft Learn.
- Microsoft. (2024). *.NET HTTP Client Documentation*. Microsoft Learn.
