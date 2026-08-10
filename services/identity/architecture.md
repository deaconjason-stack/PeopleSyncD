# Identity Architecture

Components: authentication API, credential verifier, MFA adapter, federation adapter, session manager, recovery workflow, risk evaluator, event outbox, and audit adapter.

Authorization decisions remain in the Permissions service. Organization context is established only after verified membership. Ambiguous identity, tenant, or federation state fails closed.
