# Workflow Architecture

Components: definition registry, command API, state engine, task manager, approval manager, scheduler, retry and compensation engine, outbox, notification adapter, policy adapter, and audit adapter.

The engine evaluates current state and version under concurrency control before committing a transition.
