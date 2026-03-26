-- 001_create_accounts.sql
-- Bank accounts table for Go Bank training exercise

CREATE TABLE IF NOT EXISTS accounts (
    id          VARCHAR(64)   NOT NULL PRIMARY KEY,
    owner       VARCHAR(255)  NOT NULL,
    balance     BIGINT NOT NULL DEFAULT 0,
    status      VARCHAR(16)   NOT NULL DEFAULT 'OPEN',
    created_at  TIMESTAMPTZ   NOT NULL DEFAULT NOW(),
    updated_at  TIMESTAMPTZ   NOT NULL DEFAULT NOW()
);
