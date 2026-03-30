-- 001_seed_accounts.sql
-- Seed two accounts for local development and Bonus Quest testing.
-- Run automatically by `make db-migrate` after the EF Core schema migration.
--
-- Fixed UUIDs so the README and .http file can reference them by value:
--   alice  →  00000000-0000-0000-0000-000000000001
--   bob    →  00000000-0000-0000-0000-000000000002

INSERT INTO "Accounts" ("Id", "Owner", "Balance", "CreatedAt", "UpdatedAt")
VALUES
    ('00000000-0000-0000-0000-000000000001', 'alice', 1000.00, NOW(), NOW()),
    ('00000000-0000-0000-0000-000000000002', 'bob',    800.00, NOW(), NOW())
ON CONFLICT ("Id") DO NOTHING;
