# PEP-1160: Audit Domain

- Status: Accepted

Audit Events are append-only evidence of successful, denied, failed, canceled, and approval-pending actions. Events record actor, tenant, session, action, target, result, reason, before/after references where permitted, AI involvement, and time. Audit storage must resist modification and must not become a secondary source of unrestricted sensitive data.
