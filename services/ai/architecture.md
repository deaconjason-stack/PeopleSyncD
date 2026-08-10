# Domonique 2.0 AI Architecture

## Request flow

Authenticate → resolve organization → select assistant mode → classify intent and risk → retrieve permitted context → enforce policy → construct prompt → generate or select tool → validate output → require approval where needed → execute through service contract → record sources and audit → return result.

## Components

Conversation Engine, Prompt Builder, Retrieval Gateway, Policy Engine, Tool Registry, Approval Coordinator, Memory Vault, Model Router, Safety Filter, Voice Adapter, Evaluation Harness, and Audit Adapter.

## Trust boundaries

Models, retrieved files, web content, user messages, tool results, and external providers are untrusted inputs. Service authorization and policy decisions remain deterministic platform functions.
