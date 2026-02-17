using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Auth.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CreateQuartzSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Create [quartz] schema if it doesn't exist
            migrationBuilder.Sql("IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'quartz') EXEC('CREATE SCHEMA [quartz]');");

            // Create QRTZ_CALENDARS table
            migrationBuilder.Sql(
                @"
                IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[quartz].[QRTZ_CALENDARS]') AND type in (N'U'))
                BEGIN
                    CREATE TABLE [quartz].[QRTZ_CALENDARS] (
                        [SCHED_NAME] NVARCHAR(120) NOT NULL,
                        [CALENDAR_NAME] NVARCHAR(200) NOT NULL,
                        [CALENDAR] VARBINARY(MAX) NOT NULL,
                        CONSTRAINT [PK_QRTZ_CALENDARS] PRIMARY KEY CLUSTERED (
                            [SCHED_NAME],
                            [CALENDAR_NAME]
                        )
                    );
                END
                ");

            // Create QRTZ_JOB_DETAILS table
            migrationBuilder.Sql(
                @"
                IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[quartz].[QRTZ_JOB_DETAILS]') AND type in (N'U'))
                BEGIN
                    CREATE TABLE [quartz].[QRTZ_JOB_DETAILS] (
                        [SCHED_NAME] NVARCHAR(120) NOT NULL,
                        [JOB_NAME] NVARCHAR(200) NOT NULL,
                        [JOB_GROUP] NVARCHAR(200) NOT NULL,
                        [DESCRIPTION] NVARCHAR(MAX) NULL,
                        [JOB_CLASS_NAME] NVARCHAR(500) NOT NULL,
                        [IS_DURABLE] BIT NOT NULL,
                        [IS_NONCONCURRENT] BIT NOT NULL,
                        [IS_UPDATE_DATA] BIT NOT NULL,
                        [REQUESTS_RECOVERY] BIT NOT NULL,
                        [JOB_DATA] VARBINARY(MAX) NULL,
                        CONSTRAINT [PK_QRTZ_JOB_DETAILS] PRIMARY KEY CLUSTERED (
                            [SCHED_NAME],
                            [JOB_NAME],
                            [JOB_GROUP]
                        )
                    );
                    CREATE INDEX [IDX_QRTZ_J_REQ_RECOVERY] ON [quartz].[QRTZ_JOB_DETAILS]
                        ([SCHED_NAME], [REQUESTS_RECOVERY]);
                    CREATE INDEX [IDX_QRTZ_J_GRP] ON [quartz].[QRTZ_JOB_DETAILS]
                        ([SCHED_NAME], [JOB_GROUP]);
                END
                ");

            // Create QRTZ_TRIGGERS table
            migrationBuilder.Sql(
                @"
                IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[quartz].[QRTZ_TRIGGERS]') AND type in (N'U'))
                BEGIN
                    CREATE TABLE [quartz].[QRTZ_TRIGGERS] (
                        [SCHED_NAME] NVARCHAR(120) NOT NULL,
                        [TRIGGER_NAME] NVARCHAR(200) NOT NULL,
                        [TRIGGER_GROUP] NVARCHAR(200) NOT NULL,
                        [JOB_NAME] NVARCHAR(200) NOT NULL,
                        [JOB_GROUP] NVARCHAR(200) NOT NULL,
                        [DESCRIPTION] NVARCHAR(MAX) NULL,
                        [NEXT_FIRE_TIME] BIGINT NULL,
                        [PREV_FIRE_TIME] BIGINT NULL,
                        [PRIORITY] INT NULL,
                        [TRIGGER_STATE] NVARCHAR(16) NOT NULL,
                        [TRIGGER_TYPE] NVARCHAR(8) NOT NULL,
                        [START_TIME] BIGINT NOT NULL,
                        [END_TIME] BIGINT NULL,
                        [CALENDAR_NAME] NVARCHAR(200) NULL,
                        [MISFIRE_INSTR] INT NULL,
                        [JOB_DATA] VARBINARY(MAX) NULL,
                        CONSTRAINT [PK_QRTZ_TRIGGERS] PRIMARY KEY CLUSTERED (
                            [SCHED_NAME],
                            [TRIGGER_NAME],
                            [TRIGGER_GROUP]
                        ),
                        CONSTRAINT [FK_QRTZ_TRIGGERS_JOB_DETAILS] FOREIGN KEY (
                            [SCHED_NAME],
                            [JOB_NAME],
                            [JOB_GROUP]
                        ) REFERENCES [quartz].[QRTZ_JOB_DETAILS] (
                            [SCHED_NAME],
                            [JOB_NAME],
                            [JOB_GROUP]
                        )
                    );
                    CREATE INDEX [IDX_QRTZ_T_J] ON [quartz].[QRTZ_TRIGGERS]
                        ([SCHED_NAME], [JOB_NAME], [JOB_GROUP]);
                    CREATE INDEX [IDX_QRTZ_T_JG] ON [quartz].[QRTZ_TRIGGERS]
                        ([SCHED_NAME], [JOB_GROUP]);
                    CREATE INDEX [IDX_QRTZ_T_C] ON [quartz].[QRTZ_TRIGGERS]
                        ([SCHED_NAME], [CALENDAR_NAME]);
                    CREATE INDEX [IDX_QRTZ_T_G] ON [quartz].[QRTZ_TRIGGERS]
                        ([SCHED_NAME], [TRIGGER_GROUP]);
                    CREATE INDEX [IDX_QRTZ_T_STATE] ON [quartz].[QRTZ_TRIGGERS]
                        ([SCHED_NAME], [TRIGGER_STATE]);
                    CREATE INDEX [IDX_QRTZ_T_NFT_ST] ON [quartz].[QRTZ_TRIGGERS]
                        ([SCHED_NAME], [NEXT_FIRE_TIME], [TRIGGER_STATE]);
                    CREATE INDEX [IDX_QRTZ_T_NFT_MISFIRE] ON [quartz].[QRTZ_TRIGGERS]
                        ([SCHED_NAME], [MISFIRE_INSTR], [NEXT_FIRE_TIME]);
                END
                ");

            // Create QRTZ_SIMPLE_TRIGGERS table
            migrationBuilder.Sql(
                @"
                IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[quartz].[QRTZ_SIMPLE_TRIGGERS]') AND type in (N'U'))
                BEGIN
                    CREATE TABLE [quartz].[QRTZ_SIMPLE_TRIGGERS] (
                        [SCHED_NAME] NVARCHAR(120) NOT NULL,
                        [TRIGGER_NAME] NVARCHAR(200) NOT NULL,
                        [TRIGGER_GROUP] NVARCHAR(200) NOT NULL,
                        [REPEAT_COUNT] INT NOT NULL,
                        [REPEAT_INTERVAL] BIGINT NOT NULL,
                        [TIMES_TRIGGERED] INT NOT NULL,
                        CONSTRAINT [PK_QRTZ_SIMPLE_TRIGGERS] PRIMARY KEY CLUSTERED (
                            [SCHED_NAME],
                            [TRIGGER_NAME],
                            [TRIGGER_GROUP]
                        ),
                        CONSTRAINT [FK_QRTZ_SIMPLE_TRIGGERS] FOREIGN KEY (
                            [SCHED_NAME],
                            [TRIGGER_NAME],
                            [TRIGGER_GROUP]
                        ) REFERENCES [quartz].[QRTZ_TRIGGERS] (
                            [SCHED_NAME],
                            [TRIGGER_NAME],
                            [TRIGGER_GROUP]
                        ) ON DELETE CASCADE
                    );
                END
                ");

            // Create QRTZ_CRON_TRIGGERS table
            migrationBuilder.Sql(
                @"
                IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[quartz].[QRTZ_CRON_TRIGGERS]') AND type in (N'U'))
                BEGIN
                    CREATE TABLE [quartz].[QRTZ_CRON_TRIGGERS] (
                        [SCHED_NAME] NVARCHAR(120) NOT NULL,
                        [TRIGGER_NAME] NVARCHAR(200) NOT NULL,
                        [TRIGGER_GROUP] NVARCHAR(200) NOT NULL,
                        [CRON_EXPRESSION] NVARCHAR(120) NOT NULL,
                        [TIME_ZONE_ID] NVARCHAR(80) NULL,
                        CONSTRAINT [PK_QRTZ_CRON_TRIGGERS] PRIMARY KEY CLUSTERED (
                            [SCHED_NAME],
                            [TRIGGER_NAME],
                            [TRIGGER_GROUP]
                        ),
                        CONSTRAINT [FK_QRTZ_CRON_TRIGGERS] FOREIGN KEY (
                            [SCHED_NAME],
                            [TRIGGER_NAME],
                            [TRIGGER_GROUP]
                        ) REFERENCES [quartz].[QRTZ_TRIGGERS] (
                            [SCHED_NAME],
                            [TRIGGER_NAME],
                            [TRIGGER_GROUP]
                        ) ON DELETE CASCADE
                    );
                END
                ");

            // Create QRTZ_SIMPROP_TRIGGERS table
            migrationBuilder.Sql(
                @"
                IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[quartz].[QRTZ_SIMPROP_TRIGGERS]') AND type in (N'U'))
                BEGIN
                    CREATE TABLE [quartz].[QRTZ_SIMPROP_TRIGGERS] (
                        [SCHED_NAME] NVARCHAR(120) NOT NULL,
                        [TRIGGER_NAME] NVARCHAR(200) NOT NULL,
                        [TRIGGER_GROUP] NVARCHAR(200) NOT NULL,
                        [STR_PROP_1] NVARCHAR(512) NULL,
                        [STR_PROP_2] NVARCHAR(512) NULL,
                        [STR_PROP_3] NVARCHAR(512) NULL,
                        [INT_PROP_1] INT NULL,
                        [INT_PROP_2] INT NULL,
                        [LONG_PROP_1] BIGINT NULL,
                        [LONG_PROP_2] BIGINT NULL,
                        [DEC_PROP_1] NUMERIC(13,4) NULL,
                        [DEC_PROP_2] NUMERIC(13,4) NULL,
                        [BOOL_PROP_1] BIT NULL,
                        [BOOL_PROP_2] BIT NULL,
                        CONSTRAINT [PK_QRTZ_SIMPROP_TRIGGERS] PRIMARY KEY CLUSTERED (
                            [SCHED_NAME],
                            [TRIGGER_NAME],
                            [TRIGGER_GROUP]
                        ),
                        CONSTRAINT [FK_QRTZ_SIMPROP_TRIGGERS] FOREIGN KEY (
                            [SCHED_NAME],
                            [TRIGGER_NAME],
                            [TRIGGER_GROUP]
                        ) REFERENCES [quartz].[QRTZ_TRIGGERS] (
                            [SCHED_NAME],
                            [TRIGGER_NAME],
                            [TRIGGER_GROUP]
                        ) ON DELETE CASCADE
                    );
                END
                ");

            // Create QRTZ_BLOB_TRIGGERS table
            migrationBuilder.Sql(
                @"
                IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[quartz].[QRTZ_BLOB_TRIGGERS]') AND type in (N'U'))
                BEGIN
                    CREATE TABLE [quartz].[QRTZ_BLOB_TRIGGERS] (
                        [SCHED_NAME] NVARCHAR(120) NOT NULL,
                        [TRIGGER_NAME] NVARCHAR(200) NOT NULL,
                        [TRIGGER_GROUP] NVARCHAR(200) NOT NULL,
                        [BLOB_DATA] VARBINARY(MAX) NULL,
                        CONSTRAINT [PK_QRTZ_BLOB_TRIGGERS] PRIMARY KEY CLUSTERED (
                            [SCHED_NAME],
                            [TRIGGER_NAME],
                            [TRIGGER_GROUP]
                        ),
                        CONSTRAINT [FK_QRTZ_BLOB_TRIGGERS] FOREIGN KEY (
                            [SCHED_NAME],
                            [TRIGGER_NAME],
                            [TRIGGER_GROUP]
                        ) REFERENCES [quartz].[QRTZ_TRIGGERS] (
                            [SCHED_NAME],
                            [TRIGGER_NAME],
                            [TRIGGER_GROUP]
                        ) ON DELETE CASCADE
                    );
                END
                ");

            // Create QRTZ_FIRED_TRIGGERS table
            migrationBuilder.Sql(
                @"
                IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[quartz].[QRTZ_FIRED_TRIGGERS]') AND type in (N'U'))
                BEGIN
                    CREATE TABLE [quartz].[QRTZ_FIRED_TRIGGERS] (
                        [SCHED_NAME] NVARCHAR(120) NOT NULL,
                        [ENTRY_ID] NVARCHAR(95) NOT NULL,
                        [TRIGGER_NAME] NVARCHAR(200) NOT NULL,
                        [TRIGGER_GROUP] NVARCHAR(200) NOT NULL,
                        [INSTANCE_NAME] NVARCHAR(200) NOT NULL,
                        [FIRED_TIME] BIGINT NOT NULL,
                        [SCHED_TIME] BIGINT NOT NULL,
                        [PRIORITY] INT NOT NULL,
                        [STATE] NVARCHAR(16) NOT NULL,
                        [JOB_NAME] NVARCHAR(200) NULL,
                        [JOB_GROUP] NVARCHAR(200) NULL,
                        [IS_NONCONCURRENT] BIT NULL,
                        [REQUESTS_RECOVERY] BIT NULL,
                        CONSTRAINT [PK_QRTZ_FIRED_TRIGGERS] PRIMARY KEY CLUSTERED (
                            [SCHED_NAME],
                            [ENTRY_ID]
                        )
                    );
                    CREATE INDEX [IDX_QRTZ_FT_TRIG_INST_NAME] ON [quartz].[QRTZ_FIRED_TRIGGERS]
                        ([SCHED_NAME], [INSTANCE_NAME]);
                    CREATE INDEX [IDX_QRTZ_FT_INST_JOB_REQ_RCVRY] ON [quartz].[QRTZ_FIRED_TRIGGERS]
                        ([SCHED_NAME], [INSTANCE_NAME], [REQUESTS_RECOVERY]);
                    CREATE INDEX [IDX_QRTZ_FT_J_G] ON [quartz].[QRTZ_FIRED_TRIGGERS]
                        ([SCHED_NAME], [JOB_NAME], [JOB_GROUP]);
                    CREATE INDEX [IDX_QRTZ_FT_JG] ON [quartz].[QRTZ_FIRED_TRIGGERS]
                        ([SCHED_NAME], [JOB_GROUP]);
                    CREATE INDEX [IDX_QRTZ_FT_T_G] ON [quartz].[QRTZ_FIRED_TRIGGERS]
                        ([SCHED_NAME], [TRIGGER_NAME], [TRIGGER_GROUP]);
                    CREATE INDEX [IDX_QRTZ_FT_TG] ON [quartz].[QRTZ_FIRED_TRIGGERS]
                        ([SCHED_NAME], [TRIGGER_GROUP]);
                END
                ");

            // Create QRTZ_SCHEDULER_STATE table
            migrationBuilder.Sql(
                @"
                IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[quartz].[QRTZ_SCHEDULER_STATE]') AND type in (N'U'))
                BEGIN
                    CREATE TABLE [quartz].[QRTZ_SCHEDULER_STATE] (
                        [SCHED_NAME] NVARCHAR(120) NOT NULL,
                        [INSTANCE_NAME] NVARCHAR(200) NOT NULL,
                        [LAST_CHECKIN_TIME] BIGINT NOT NULL,
                        [CHECKIN_INTERVAL] BIGINT NOT NULL,
                        CONSTRAINT [PK_QRTZ_SCHEDULER_STATE] PRIMARY KEY CLUSTERED (
                            [SCHED_NAME],
                            [INSTANCE_NAME]
                        )
                    );
                END
                ");

            // Create QRTZ_LOCKS table
            migrationBuilder.Sql(
                @"
                IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[quartz].[QRTZ_LOCKS]') AND type in (N'U'))
                BEGIN
                    CREATE TABLE [quartz].[QRTZ_LOCKS] (
                        [SCHED_NAME] NVARCHAR(120) NOT NULL,
                        [LOCK_NAME] NVARCHAR(40) NOT NULL,
                        CONSTRAINT [PK_QRTZ_LOCKS] PRIMARY KEY CLUSTERED (
                            [SCHED_NAME],
                            [LOCK_NAME]
                        )
                    );
                END
                ");

            // Create QRTZ_PAUSED_TRIGGER_GRPS table
            migrationBuilder.Sql(
                @"
                IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[quartz].[QRTZ_PAUSED_TRIGGER_GRPS]') AND type in (N'U'))
                BEGIN
                    CREATE TABLE [quartz].[QRTZ_PAUSED_TRIGGER_GRPS] (
                        [SCHED_NAME] NVARCHAR(120) NOT NULL,
                        [TRIGGER_GROUP] NVARCHAR(200) NOT NULL,
                        CONSTRAINT [PK_QRTZ_PAUSED_TRIGGER_GRPS] PRIMARY KEY CLUSTERED (
                            [SCHED_NAME],
                            [TRIGGER_GROUP]
                        )
                    );
                END
                ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop all tables in [quartz] schema (in reverse order of dependencies)
            migrationBuilder.Sql("IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[quartz].[QRTZ_PAUSED_TRIGGER_GRPS]') AND type in (N'U')) DROP TABLE [quartz].[QRTZ_PAUSED_TRIGGER_GRPS];");
            migrationBuilder.Sql("IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[quartz].[QRTZ_LOCKS]') AND type in (N'U')) DROP TABLE [quartz].[QRTZ_LOCKS];");
            migrationBuilder.Sql("IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[quartz].[QRTZ_SCHEDULER_STATE]') AND type in (N'U')) DROP TABLE [quartz].[QRTZ_SCHEDULER_STATE];");
            migrationBuilder.Sql("IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[quartz].[QRTZ_FIRED_TRIGGERS]') AND type in (N'U')) DROP TABLE [quartz].[QRTZ_FIRED_TRIGGERS];");
            migrationBuilder.Sql("IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[quartz].[QRTZ_BLOB_TRIGGERS]') AND type in (N'U')) DROP TABLE [quartz].[QRTZ_BLOB_TRIGGERS];");
            migrationBuilder.Sql("IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[quartz].[QRTZ_SIMPROP_TRIGGERS]') AND type in (N'U')) DROP TABLE [quartz].[QRTZ_SIMPROP_TRIGGERS];");
            migrationBuilder.Sql("IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[quartz].[QRTZ_CRON_TRIGGERS]') AND type in (N'U')) DROP TABLE [quartz].[QRTZ_CRON_TRIGGERS];");
            migrationBuilder.Sql("IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[quartz].[QRTZ_SIMPLE_TRIGGERS]') AND type in (N'U')) DROP TABLE [quartz].[QRTZ_SIMPLE_TRIGGERS];");
            migrationBuilder.Sql("IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[quartz].[QRTZ_TRIGGERS]') AND type in (N'U')) DROP TABLE [quartz].[QRTZ_TRIGGERS];");
            migrationBuilder.Sql("IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[quartz].[QRTZ_JOB_DETAILS]') AND type in (N'U')) DROP TABLE [quartz].[QRTZ_JOB_DETAILS];");
            migrationBuilder.Sql("IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[quartz].[QRTZ_CALENDARS]') AND type in (N'U')) DROP TABLE [quartz].[QRTZ_CALENDARS];");
            migrationBuilder.Sql("IF EXISTS (SELECT * FROM sys.schemas WHERE name = 'quartz') EXEC('DROP SCHEMA [quartz]');");
        }
    }
}
