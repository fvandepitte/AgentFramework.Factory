# Chain of Responsibility Visual Guide

## Architecture Overview

```
┌─────────────────────────────────────────────────────────────┐
│                     ProviderFactory                         │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│  CreateChatClient(modelName)                                │
│         │                                                   │
│         ▼                                                   │
│  ┌──────────────────────────────────────────────────┐      │
│  │         Provider Chain (Built at startup)        │      │
│  │                                                  │      │
│  │   [Azure] → [OpenAI] → [GitHub]                 │      │
│  │                                                  │      │
│  └──────────────────────────────────────────────────┘      │
│         │                                                   │
│         ▼                                                   │
│  TryCreateChatClient(modelName)                             │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

## Request Flow

```
User Request: "Create client for 'llama-3.2'"
    │
    ▼
┌────────────────────────────────────────┐
│   ProviderFactory                      │
│   CreateChatClient("llama-3.2")        │
└────────────────────────────────────────┘
    │
    ├─► Log: "🔍 Looking for provider to handle model: llama-3.2"
    │
    ▼
┌────────────────────────────────────────┐
│  Handler 1: AzureOpenAIProviderHandler │
│                                        │
│  CanHandleModel("llama-3.2")?          │
│    ├─► Check: Is endpoint configured?  │
│    │     ✓ Yes                          │
│    ├─► Check: Can deployment handle it?│
│    │     ✓ Yes (accepts all models)    │
│    │                                    │
│    └─► CreateChatClient()               │
│          ├─► Success! Return client     │
│          └─► OR fail → Pass to next     │
└────────────────────────────────────────┘
    │
    │ (If Azure fails/can't handle)
    ▼
┌────────────────────────────────────────┐
│  Handler 2: OpenAIProviderHandler      │
│                                        │
│  CanHandleModel("llama-3.2")?          │
│    ├─► Check: Is API key configured?   │
│    │     ✓ Yes                          │
│    ├─► Check: Is model in known list?  │
│    │     ✗ No (llama not in OpenAI)    │
│    │                                    │
│    └─► Cannot handle → Pass to next    │
└────────────────────────────────────────┘
    │
    ▼
┌────────────────────────────────────────┐
│  Handler 3: GitHubModelsProviderHandler│
│                                        │
│  CanHandleModel("llama-3.2")?          │
│    ├─► Check: Is token configured?     │
│    │     ✓ Yes                          │
│    ├─► Check: Is model supported?      │
│    │     ✓ Yes (llama-3.2 in list)     │
│    │                                    │
│    └─► CreateChatClient()               │
│          └─► ✓ Success! Return client   │
└────────────────────────────────────────┘
    │
    ▼
┌────────────────────────────────────────┐
│  Return IChatClient to User            │
│  Log: "✓ Provider 'GitHubModels'       │
│        handling model: llama-3.2"      │
└────────────────────────────────────────┘
```

## Handler Decision Tree

```
                    CanHandleModel(model)?
                            │
                ┌───────────┴───────────┐
                │                       │
              YES                      NO
                │                       │
                ▼                       ▼
        CreateChatClient()      Pass to Next Handler
                │                       │
        ┌───────┴───────┐               │
        │               │               │
     Success         Exception          │
        │               │               │
        ▼               ▼               ▼
   Return Client  Try Next Handler  Next.TryCreate()
                        │               │
                        │          (Recursive)
                        │               │
                        └───────┬───────┘
                                │
                                ▼
                        All Failed?
                                │
                    ┌───────────┴──────────┐
                    │                      │
                   YES                     NO
                    │                      │
                    ▼                      ▼
            Throw Exception         Return Client
```

## Provider Model Support Matrix

```
┌─────────────────┬──────────────┬──────────┬───────────────┐
│ Model           │ Azure OpenAI │  OpenAI  │ GitHub Models │
├─────────────────┼──────────────┼──────────┼───────────────┤
│ gpt-4o          │      ✓       │    ✓     │      ✓        │
│ gpt-4o-mini     │      ✓       │    ✓     │      ✓        │
│ gpt-3.5-turbo   │      ✓       │    ✓     │      ✓        │
│ o1-preview      │      ✓       │    ✓     │      ✓        │
├─────────────────┼──────────────┼──────────┼───────────────┤
│ llama-3.2       │      ✓*      │    ✗     │      ✓        │
│ phi-3.5         │      ✓*      │    ✗     │      ✓        │
│ mistral-large   │      ✓*      │    ✗     │      ✓        │
└─────────────────┴──────────────┴──────────┴───────────────┘

* Azure OpenAI accepts all models if configured (uses deployment)
✓ = Can handle if configured
✗ = Cannot handle (not in supported models list)
```

## Configuration to Chain Mapping

```
Configuration:
┌────────────────────────────────────────┐
│ "providerChain": [                     │
│    "azureOpenAI",    ← First in chain  │
│    "openAI",         ← Second in chain │
│    "githubModels"    ← Last in chain   │
│ ]                                      │
└────────────────────────────────────────┘
         │
         ▼
Runtime Chain:
┌─────────────┐     ┌─────────┐     ┌──────────────┐
│ Azure       │────►│ OpenAI  │────►│ GitHub       │────► null
│ OpenAI      │     │         │     │ Models       │
│ Handler     │     │ Handler │     │ Handler      │
└─────────────┘     └─────────┘     └──────────────┘
  _nextHandler       _nextHandler      _nextHandler
```

## Logging Output Example

```
Console Output:

🔍 Looking for provider to handle model: llama-3.2

  Trying: AzureOpenAIProviderHandler
    ├─ Configuration check: ✓ Endpoint configured
    ├─ Configuration check: ✓ Deployment configured  
    └─ Model support: ✓ Accepts all models
    └─► ✓ Provider 'AzureOpenAI' handling model: llama-3.2

✅ Chat client created successfully!
```

```
Console Output (Fallback Scenario):

🔍 Looking for provider to handle model: llama-3.2

  Trying: AzureOpenAIProviderHandler
    ├─ Configuration check: ✗ Endpoint not configured
    └─► Passing to next handler...

  Trying: OpenAIProviderHandler
    ├─ Configuration check: ✓ API key configured
    ├─ Model support: ✗ llama-3.2 not in OpenAI catalog
    └─► Passing to next handler...

  Trying: GitHubModelsProviderHandler
    ├─ Configuration check: ✓ Token configured
    ├─ Model support: ✓ llama-3.2 supported
    └─► ✓ Provider 'GitHubModels' handling model: llama-3.2

✅ Chat client created successfully!
```

## Class Relationship Diagram

```
                  IProviderHandler (Interface)
                         │
                         │ implements
                         ▼
               BaseProviderHandler (Abstract)
                         │
                         │ extends
        ┌────────────────┼────────────────┐
        │                │                │
        ▼                ▼                ▼
┌─────────────┐  ┌────────────┐  ┌──────────────┐
│ Azure       │  │  OpenAI    │  │  GitHub      │
│ OpenAI      │  │  Provider  │  │  Models      │
│ Provider    │  │  Handler   │  │  Provider    │
│ Handler     │  │            │  │  Handler     │
└─────────────┘  └────────────┘  └──────────────┘

Each handler:
- Knows if it can handle a model
- Creates chat clients for supported models
- Passes requests to next handler if unable
```

---

**Visual Guide Created**: 2026-01-30  
**Pattern**: Chain of Responsibility

