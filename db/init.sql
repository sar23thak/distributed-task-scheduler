USE taskscheduler;

CREATE TABLE IF NOT EXISTS Jobs (
    Id          CHAR(36)        NOT NULL PRIMARY KEY,
    Type        VARCHAR(100)    NOT NULL,
    Payload     LONGTEXT        NOT NULL,
    Status      TINYINT         NOT NULL DEFAULT 0,
    Priority    INT             NOT NULL DEFAULT 0,
    RetryCount  INT             NOT NULL DEFAULT 0,
    MaxRetries  INT             NOT NULL DEFAULT 3,
    LastError   TEXT            NULL,
    CreatedAt   DATETIME        NOT NULL DEFAULT CURRENT_TIMESTAMP,
    ScheduledAt DATETIME        NULL,
    StartedAt   DATETIME        NULL,
    CompletedAt DATETIME        NULL,

    INDEX idx_status_priority (Status, Priority DESC),
    INDEX idx_scheduled (ScheduledAt),
    INDEX idx_created (CreatedAt)
);