CREATE OR REPLACE VIEW pending_ai_approvals AS
SELECT
    id,
    organization_id,
    conversation_id,
    tool_id,
    tool_version,
    risk_class,
    requested_by,
    expires_at,
    row_version,
    created_at
FROM ai_action_requests
WHERE status = 'approval_required';
