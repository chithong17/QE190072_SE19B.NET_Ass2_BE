-- ============================================================
-- PRN232 Assignment 2 — SystemAccount Table & Seed
-- ============================================================

CREATE TABLE IF NOT EXISTS "SystemAccount" (
    "AccountID"     SERIAL          PRIMARY KEY,
    "FullName"      VARCHAR(100)    NOT NULL,
    "Email"         VARCHAR(150)    NOT NULL UNIQUE,
    "PasswordHash"  VARCHAR(255)    NOT NULL,
    "Role"          SMALLINT        NOT NULL DEFAULT 0, -- 0 = Staff, 1 = Admin
    "CreatedDate"   TIMESTAMP       NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-- Optional audit fields on Task table (supports Bonus & Delete account constraint)
ALTER TABLE "Task" 
ADD COLUMN IF NOT EXISTS "CreatedByID" INT NULL,
ADD COLUMN IF NOT EXISTS "UpdatedByID" INT NULL;

DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint WHERE conname = 'FK_Task_CreatedBy'
    ) THEN
        ALTER TABLE "Task"
        ADD CONSTRAINT "FK_Task_CreatedBy" FOREIGN KEY ("CreatedByID")
        REFERENCES "SystemAccount" ("AccountID") ON DELETE RESTRICT;
    END IF;

    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint WHERE conname = 'FK_Task_UpdatedBy'
    ) THEN
        ALTER TABLE "Task"
        ADD CONSTRAINT "FK_Task_UpdatedBy" FOREIGN KEY ("UpdatedByID")
        REFERENCES "SystemAccount" ("AccountID") ON DELETE SET NULL;
    END IF;
END $$;

-- Seed Admin account directly (password: AdminPass@123)
-- BCrypt hash generated with 12 rounds
INSERT INTO "SystemAccount" ("FullName", "Email", "PasswordHash", "Role", "CreatedDate")
VALUES (
    'System Administrator',
    'admin@tasktrack.com',
    '$2b$12$zChRFFhiP3GPQGFOR8jrX.x0tFssKY6F9QyIgSQq/8cdR242rVfei',
    1,
    CURRENT_TIMESTAMP
)
ON CONFLICT ("Email") DO NOTHING;
