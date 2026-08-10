CREATE TABLE IF NOT EXISTS ai_conversations (
    id uuid PRIMARY KEY,
    organization_id uuid NOT NULL,
    user_id uuid NOT NULL,
    assistant_mode text NOT NULL,
    status text NOT NULL DEFAULT 'active',
    prompt_template_version text NOT NULL,
    retention_class text NOT NULL,
    created_at timestamptz NOT NULL DEFAULT now(),
    closed_at timestamptz
);

CREATE TABLE IF NOT EXISTS ai_messages (
    id uuid PRIMARY KEY,
    organization_id uuid NOT NULL,
    conversation_id uuid NOT NULL,
    actor_type text NOT NULL,
    content_ciphertext bytea NOT NULL,
    safety_state text NOT NULL,
    created_at timestamptz NOT NULL DEFAULT now()
);

CREATE TABLE IF NOT EXISTS ai_source_references (
    id uuid PRIMARY KEY,
    organization_id uuid NOT NULL,
    message_id uuid NOT NULL,
    source_type text NOT NULL,
    source_id uuid,
    source_version text,
    locator text,
    confidence numeric(5,4),
    created_at timestamptz NOT NULL DEFAULT now()
);

CREATE TABLE IF NOT EXISTS ai_action_requests (
    id uuid PRIMARY KEY,
    organization_id uuid NOT NULL,
    conversation_id uuid NOT NULL,
    tool_id text NOT NULL,
    tool_version text NOT NULL,
    risk_class text NOT NULL,
    status text NOT NULL,
    requested_by uuid NOT NULL,
    approved_by uuid,
    expires_at timestamptz,
    row_version bigint NOT NULL DEFAULT 1,
    created_at timestamptz NOT NULL DEFAULT now(),
    resolved_at timestamptz
);

CREATE TABLE IF NOT EXISTS ai_memory_records (
    id uuid PRIMARY KEY,
    organization_id uuid NOT NULL,
    subject_user_id uuid,
    memory_class text NOT NULL,
    value_ciphertext bytea NOT NULL,
    source_reference text NOT NULL,
    confidence numeric(5,4),
    classification text NOT NULL,
    retention_until timestamptz,
    legal_hold boolean NOT NULL DEFAULT false,
    created_by uuid NOT NULL,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NOT NULL DEFAULT now()
);

ALTER TABLE ai_conversations ENABLE ROW LEVEL SECURITY;
ALTER TABLE ai_messages ENABLE ROW LEVEL SECURITY;
ALTER TABLE ai_source_references ENABLE ROW LEVEL SECURITY;
ALTER TABLE ai_action_requests ENABLE ROW LEVEL SECURITY;
ALTER TABLE ai_memory_records ENABLE ROW LEVEL SECURITY;
