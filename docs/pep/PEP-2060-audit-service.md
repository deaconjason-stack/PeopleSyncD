# PEP-2060: Audit Service

- Status: Accepted

The Audit service accepts append-only evidence from all services, protects integrity, assigns correlation identifiers, supports authorized search and export, and prevents update or deletion through ordinary APIs. It records successful, denied, failed, and approval-pending actions without becoming an unrestricted copy of sensitive records. Initial requirement namespace: `REQ-AUDIT-*`.
