ALTER TABLE ai_conversations
    ADD CONSTRAINT ai_conversation_status_check
    CHECK (status IN ('active','closed','archived'));

ALTER TABLE ai_action_requests
    ADD CONSTRAINT ai_action_status_check
    CHECK (status IN ('drafted','approval_required','approved','executing','executed','refused','canceled','expired','failed','rolled_back'));

ALTER TABLE ai_source_references
    ADD CONSTRAINT ai_source_confidence_check
    CHECK (confidence IS NULL OR confidence BETWEEN 0 AND 1);

ALTER TABLE ai_memory_records
    ADD CONSTRAINT ai_memory_confidence_check
    CHECK (confidence IS NULL OR confidence BETWEEN 0 AND 1);

CREATE POLICY ai_conversations_tenant_policy ON ai_conversations
    USING (organization_id = current_setting('app.organization_id', true)::uuid)
    WITH CHECK (organization_id = current_setting('app.organization_id', true)::uuid);

CREATE POLICY ai_messages_tenant_policy ON ai_messages
    USING (organization_id = current_setting('app.organization_id', true)::uuid)
    WITH CHECK (organization_id = current_setting('app.organization_id', true)::uuid);

CREATE POLICY ai_sources_tenant_policy ON ai_source_references
    USING (organization_id = current_setting('app.organization_id', true)::uuid)
    WITH CHECK (organization_id = current_setting('app.organization_id', true)::uuid);

CREATE POLICY ai_actions_tenant_policy ON ai_action_requests
    USING (organization_id = current_setting('app.organization_id', true)::uuid)
    WITH CHECK (organization_id = current_setting('app.organization_id', true)::uuid);

CREATE POLICY ai_memory_tenant_policy ON ai_memory_records
    USING (organization_id = current_setting('app.organization_id', true)::uuid)
    WITH CHECK (organization_id = current_setting('app.organization_id', true)::uuid);
