# Workflow Domain

Entities: Workflow Definition, Version, Instance, State, Transition, Task, Approval, Deadline, Reminder, Exception, Compensation Action, and History Entry.

Definition states: `Draft → Review → Approved → Active → Retired`.

Instance states are definition-specific and immutable definition versions remain linked to history.
