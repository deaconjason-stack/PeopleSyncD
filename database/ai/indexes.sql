CREATE INDEX IF NOT EXISTS idx_ai_conversations_org_user_status
    ON ai_conversations (organization_id, user_id, status);

CREATE INDEX IF NOT EXISTS idx_ai_messages_conversation_created
    ON ai_messages (organization_id, conversation_id, created_at);

CREATE INDEX IF NOT EXISTS idx_ai_actions_org_status_expiry
    ON ai_action_requests (organization_id, status, expires_at);

CREATE INDEX IF NOT EXISTS idx_ai_memory_org_subject_class
    ON ai_memory_records (organization_id, subject_user_id, memory_class);
