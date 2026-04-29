# 🚀 Modern Orchestrator Engine (MOE)

> **A lightweight, portable plugin orchestration runtime for .NET 10**  
> *Modernize legacy infrastructures without rewriting them.*

[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?style=flat-square&logo=dotnet)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/badge/license-MIT-green?style=flat-square)](LICENSE)
[![Status](https://img.shields.io/badge/status-In%20Development-orange?style=flat-square)]()
[![Blog](https://img.shields.io/badge/blog-sviluppoignorante.altervista.org-blue?style=flat-square)](https://sviluppoignorante.altervista.org)

---

## 🇬🇧 English

### What is MOE?

**Modern Orchestrator Engine** is a lightweight, self-contained runtime that runs alongside legacy applications (such as .NET Framework 4.8 websites) as a **sidecar process**, offloading heavy business logic into a modern, isolated, and easily scalable environment.

It transforms classic monolithic Windows Services into a **dynamic Micro-Plugin architecture** — eliminating the complexity of large-scale installation and maintenance.

### The Problem It Solves

Many companies are trapped in a **"technology limbo"**: their legacy systems cannot be rewritten due to budget, risk, or complexity constraints, yet they desperately need modern performance and tooling.

MOE provides a **gradual escape route**:
- No need to rewrite existing applications
- New features run on .NET 10 with full access to modern APIs
- Legacy app and modern engine coexist safely

### Architecture Overview

```
┌─────────────────────────────────────────┐
│             MOE.Host                    │  ← Windows Service / standalone exe
│   (Lifetime, DI root, configuration)   │
├─────────────┬───────────────────────────┤
│  MOE.Api    │        MOE.Core           │  ← Engine loop, scheduler
│  (REST API) │  PluginRunner, ALC Mgr    │
├─────────────┴──────────┬────────────────┤
│     MOE.Security       │ MOE.Persistence│  ← Infrastructure layer
│  (SHA256, HMAC, Scan)  │ (SQLite+LiteDB)│
├────────────────────────┴────────────────┤
│              MOE.Sdk                    │  ← Public contract (NuGet)
│     IPlugin · IPluginContext · PluginResult  │
└─────────────────────────────────────────┘
```

### Key Features

| Feature | Description |
|---------|-------------|
| 🔌 **Plug & Play Isolation** | Each plugin runs in its own `AssemblyLoadContext` — isolated, unloadable |
| 💾 **Zero-Config Storage** | SQLite for state, LiteDB for structured logs — no external DB server required |
| 🛡️ **Security Pipeline** | SHA-256 verification + HMAC signing + static DLL analysis via Mono.Cecil |
| ⏱️ **Flexible Scheduling** | Cron expressions, event triggers, and manual execution — all in one queue |
| 📦 **Portable Deployment** | Self-contained Windows Service — single folder, no install prerequisites |
| 🔁 **Resilient Execution** | Configurable retry policies with exponential backoff per plugin |

### Plugin Contract (SDK)

Developers reference only `MOE.Sdk` — a minimal NuGet package:

```csharp
public interface IPlugin : IAsyncDisposable
{
    string Name { get; }
    string Version { get; }
    Task InitializeAsync(IPluginContext context);
    Task<PluginResult> ExecuteAsync(CancellationToken ct);
}
```

Each plugin is distributed as a **ZIP package** containing:
- The compiled plugin DLL
- `service.config.json` — the plugin's "passport" for the orchestrator
- `appsettings.json` *(optional)* — plugin-specific configuration

### Plugin Lifecycle

```
ZIP dropped in /plugins/incoming/
        │
        ▼
[Security] SHA-256 + HMAC verification
        │
        ▼
[Security] Static DLL analysis (namespace whitelist + pattern scan)
        │
        ▼
[Core] Unzip → register in SQLite
        │
        ▼
[Scheduler] Plan execution (cron / trigger / manual)
        │
        ▼
[Runner] Load ALC → Initialize → Execute → Dispose → Unload
        │
        ▼
[Persistence] Update state + write structured log to LiteDB
```

### Roadmap

- [x] Architecture & Security Design
- [ ] **Phase 0** — SDK (IPlugin, IPluginContext, PluginResult, Manifest)
- [ ] **Phase 1** — Core Engine (ALC, PluginRunner, Timeout)
- [ ] **Phase 2** — Persistence (SQLite + LiteDB, Dapper)
- [ ] **Phase 3** — Security Pipeline (SHA256, HMAC, Mono.Cecil analyzer)
- [ ] **Phase 4** — Scheduler (Cron + Trigger + Manual queue)
- [ ] **Phase 5** — REST API (status, trigger, deploy endpoints)
- [ ] **Phase 6** — Windows Service Host (graceful shutdown, self-contained publish)

### Project Structure

```
MOE.sln
├── src/
│   ├── MOE.Sdk/            # NuGet package — plugin developer contract
│   ├── MOE.Core/           # Engine, ALC manager, plugin runner
│   ├── MOE.Persistence/    # SQLite (state) + LiteDB (logs)
│   ├── MOE.Security/       # Hash verification + static analyzer
│   ├── MOE.Api/            # Minimal REST API
│   └── MOE.Host/           # Windows Service + exe entry point
├── plugins/
│   ├── incoming/           # Drop zone for new ZIP packages
│   ├── active/             # Validated and registered plugins
│   └── failed/             # Rejected packages with reason
├── tests/
└── samples/
    └── MOE.SamplePlugin/   # Reference implementation for plugin developers
```

### Tech Stack

- **.NET 10** — Runtime
- **Dapper** — SQLite data access
- **LiteDB** — Structured logging
- **Cronos** — Cron expression parsing
- **Mono.Cecil** — Static DLL analysis (without loading assemblies)
- **ASP.NET Core Minimal API** — REST interface

### Contributing

The project is in early development. Contributions, issues, and discussions are welcome.  
Please read [CONTRIBUTING.md](CONTRIBUTING.md) *(coming soon)* before submitting a PR.

### Author

**Danilo Garro** — Software Developer  
📖 Blog: [sviluppoignorante.altervista.org](https://sviluppoignorante.altervista.org)  
*Follow the development journey on the blog — each phase is documented as a technical article.*

---

## 🇮🇹 Italiano

### Cos'è MOE?

**Modern Orchestrator Engine** è un runtime leggero e self-contained che opera accanto alle applicazioni legacy (come siti .NET Framework 4.8) come **processo sidecar**, spostando la logica di business corposa in un ambiente moderno, isolato e facilmente scalabile.

Trasforma i classici Windows Service monolitici in un'architettura a **Micro-Plugin dinamici** — eliminando la complessità di installazione e manutenzione su larga scala.

### Il Problema che Risolve

Molte aziende sono intrappolate in un **"limbo tecnologico"**: i sistemi legacy non possono essere riscritti per ragioni di budget, rischio o complessità, ma hanno bisogno di performance e strumenti moderni.

MOE offre una **via di uscita graduale**:
- Nessuna riscrittura delle applicazioni esistenti
- Le nuove funzionalità girano su .NET 10 con accesso completo alle API moderne
- L'app legacy e l'engine moderno coesistono in sicurezza

### Caratteristiche Principali

| Caratteristica | Descrizione |
|----------------|-------------|
| 🔌 **Isolamento Plug & Play** | Ogni plugin gira nel proprio `AssemblyLoadContext` — isolato e scaricabile dinamicamente |
| 💾 **Storage Zero-Config** | SQLite per lo stato, LiteDB per i log — nessun server database esterno richiesto |
| 🛡️ **Pipeline di Sicurezza** | Verifica SHA-256 + firma HMAC + analisi statica DLL via Mono.Cecil |
| ⏱️ **Scheduling Flessibile** | Espressioni cron, trigger su evento, esecuzione manuale — tutto su un'unica coda prioritaria |
| 📦 **Deploy Portabile** | Windows Service self-contained — cartella singola, nessun prerequisito da installare |
| 🔁 **Esecuzione Resiliente** | Retry policy configurabile con backoff esponenziale, definita per singolo plugin |

### Contratto Plugin (SDK)

I developer referenziano solo `MOE.Sdk` — un pacchetto NuGet minimale:

```csharp
public interface IPlugin : IAsyncDisposable
{
    string Name { get; }
    string Version { get; }
    Task InitializeAsync(IPluginContext context);
    Task<PluginResult> ExecuteAsync(CancellationToken ct);
}
```

Ogni plugin viene distribuito come pacchetto **ZIP** contenente:
- La DLL compilata del plugin
- `service.config.json` — il "passaporto" del servizio per l'orchestratore
- `appsettings.json` *(opzionale)* — configurazione specifica del plugin

### Roadmap

- [x] Design architetturale e piano di sicurezza
- [ ] **Fase 0** — SDK (IPlugin, IPluginContext, PluginResult, Manifest)
- [ ] **Fase 1** — Core Engine (ALC, PluginRunner, Timeout)
- [ ] **Fase 2** — Persistenza (SQLite + LiteDB, Dapper)
- [ ] **Fase 3** — Pipeline di Sicurezza (SHA256, HMAC, analizzatore Mono.Cecil)
- [ ] **Fase 4** — Scheduler (Cron + Trigger + coda Manuale)
- [ ] **Fase 5** — REST API (endpoint status, trigger, deploy)
- [ ] **Fase 6** — Windows Service Host (graceful shutdown, publish self-contained)

### Stack Tecnologico

- **.NET 10** — Runtime
- **Dapper** — Accesso dati SQLite
- **LiteDB** — Log strutturati
- **Cronos** — Parsing espressioni cron
- **Mono.Cecil** — Analisi statica DLL (senza caricare gli assembly)
- **ASP.NET Core Minimal API** — Interfaccia REST

### Autore

**Danilo Garro** — Software Developer  
📖 Blog: [sviluppoignorante.altervista.org](https://sviluppoignorante.altervista.org)  
*Segui lo sviluppo sul blog — ogni fase è documentata come articolo tecnico.*

---

<p align="center">
  <i>Built with ❤️ and a lot of ☕ — turning legacy constraints into modern opportunities.</i>
</p>
