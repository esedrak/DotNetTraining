-- 002_create_transactions.sql
-- Bank transactions table for Go Bank training exercise

CREATE TABLE IF NOT EXISTS transactions (
    id          VARCHAR(64)   NOT NULL PRIMARY KEY,
    account_id  VARCHAR(64)   NOT NULL REFERENCES accounts(id),
    amount      BIGINT NOT NULL,
    type        VARCHAR(16)   NOT NULL,
    created_at  TIMESTAMPTZ   NOT NULL DEFAULT NOW()
);
