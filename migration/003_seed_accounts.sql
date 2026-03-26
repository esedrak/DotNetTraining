-- 003_seed_accounts.sql
-- Seed two accounts for local development and Bonus Quest testing

INSERT INTO accounts (id, owner, balance, status, created_at, updated_at)
VALUES
    ('ACC-1', 'alice', 10000, 'OPEN', NOW(), NOW()),
    ('ACC-2', 'bob',   8000, 'OPEN', NOW(), NOW())
ON CONFLICT (id) DO NOTHING;
