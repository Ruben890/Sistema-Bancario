set database_name sistema_bancario

SELECT format('CREATE DATABASE %I', :'database_name')
WHERE NOT EXISTS (
    SELECT 1
    FROM pg_database
    WHERE datname = :'database_name'
)

connect sistema_bancario

CREATE SCHEMA IF NOT EXISTS banking;

CREATE TABLE IF NOT EXISTS banking.users (
    "Id" uuid PRIMARY KEY,
    "Name" varchar(150) NOT NULL,
    "Email" varchar(256) NOT NULL,
    "PasswordHash" varchar(500) NOT NULL,
    "Role" varchar(30) NOT NULL,
    "IsActive" boolean NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone NULL,
    CONSTRAINT "CK_users_Role"
        CHECK ("Role" IN ('Customer', 'Admin'))
);

CREATE UNIQUE INDEX IF NOT EXISTS "IX_users_Email"
    ON banking.users ("Email");

CREATE TABLE IF NOT EXISTS banking.loans (
    "Id" uuid PRIMARY KEY,
    "UserId" uuid NOT NULL,
    "Amount" numeric(18, 2) NOT NULL,
    "TermInMonths" integer NOT NULL,
    "Purpose" varchar(500) NOT NULL,
    "Status" varchar(30) NOT NULL,
    "ReviewedByUserId" uuid NULL,
    "ReviewedAt" timestamp with time zone NULL,
    "RejectionReason" varchar(500) NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone NULL,
    CONSTRAINT "CK_loans_Amount"
        CHECK ("Amount" > 0 AND "Amount" <= 5000000),
    CONSTRAINT "CK_loans_TermInMonths"
        CHECK ("TermInMonths" BETWEEN 1 AND 120),
    CONSTRAINT "CK_loans_Status"
        CHECK ("Status" IN ('Pending', 'Approved', 'Rejected')),
    CONSTRAINT "FK_loans_users_UserId"
        FOREIGN KEY ("UserId")
        REFERENCES banking.users ("Id")
        ON DELETE RESTRICT,
    CONSTRAINT "FK_loans_users_ReviewedByUserId"
        FOREIGN KEY ("ReviewedByUserId")
        REFERENCES banking.users ("Id")
        ON DELETE RESTRICT
);

CREATE INDEX IF NOT EXISTS "IX_loans_UserId"
    ON banking.loans ("UserId");

CREATE INDEX IF NOT EXISTS "IX_loans_Status"
    ON banking.loans ("Status");

CREATE TABLE IF NOT EXISTS banking."__EFMigrationsHistory" (
    "MigrationId" varchar(150) NOT NULL PRIMARY KEY,
    "ProductVersion" varchar(32) NOT NULL
);
