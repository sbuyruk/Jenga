-- Fazla mesai modülü. Hedef Jenga veritabanında bir kez uygulanır; tekrar çalıştırılabilir.
-- Uygulama bu betiği otomatik çalıştırmaz. Mevcut rol/izinler değiştirilmez.
SET NOCOUNT ON;
SET XACT_ABORT ON;
BEGIN TRANSACTION;

IF OBJECT_ID(N'dbo.OvertimePeriod_Table', N'U') IS NULL
CREATE TABLE dbo.OvertimePeriod_Table (
    Id int IDENTITY PRIMARY KEY, Name nvarchar(200) NOT NULL,
    StartDate date NOT NULL, EndDate date NOT NULL, DueDate date NOT NULL,
    Status int NOT NULL, SignatureRevision int NOT NULL DEFAULT 0, FinalizedAt datetime2 NULL,
    SignedFileName nvarchar(200) NULL, SignedContentType nvarchar(100) NULL, SignedDocument varbinary(max) NULL,
    Version rowversion NOT NULL,
    Aciklama nvarchar(max) NULL, Olusturan nvarchar(max) NULL, OlusturmaTarihi datetime2 NULL,
    Degistiren nvarchar(max) NULL, DegistirmeTarihi datetime2 NULL,
    CONSTRAINT CK_OvertimePeriod_Dates CHECK (EndDate >= StartDate AND DueDate >= EndDate
        AND DATEDIFF(day, CONVERT(date,'19000101',112), StartDate) % 7 = 0
        AND DATEDIFF(day, CONVERT(date,'19000107',112), EndDate) % 7 = 0),
    CONSTRAINT CK_OvertimePeriod_Status CHECK (Status BETWEEN 0 AND 2),
    CONSTRAINT CK_OvertimePeriod_Finalized CHECK (Status <> 2 OR (SignedDocument IS NOT NULL AND FinalizedAt IS NOT NULL))
);

IF OBJECT_ID(N'dbo.OvertimeEntry_Table', N'U') IS NULL
CREATE TABLE dbo.OvertimeEntry_Table (
    Id int IDENTITY PRIMARY KEY, PersonelId int NOT NULL, UnitId int NOT NULL, WorkDate date NOT NULL,
    FullName nvarchar(200) NOT NULL, SicilNo int NOT NULL, UnitName nvarchar(200) NOT NULL, Title nvarchar(200) NOT NULL,
    NormalWeeklyMinutes int NOT NULL, Reason nvarchar(2000) NOT NULL, Status int NOT NULL,
    ReviewReason nvarchar(2000) NULL, PeriodId int NULL, Version rowversion NOT NULL,
    Aciklama nvarchar(max) NULL, Olusturan nvarchar(max) NULL, OlusturmaTarihi datetime2 NULL,
    Degistiren nvarchar(max) NULL, DegistirmeTarihi datetime2 NULL,
    CONSTRAINT FK_OvertimeEntry_Period FOREIGN KEY (PeriodId) REFERENCES dbo.OvertimePeriod_Table(Id),
    CONSTRAINT CK_OvertimeEntry_Normal CHECK (NormalWeeklyMinutes IN (2400,2700)),
    CONSTRAINT CK_OvertimeEntry_Status CHECK (Status BETWEEN 0 AND 4),
    CONSTRAINT CK_OvertimeEntry_Reason CHECK (LEN(LTRIM(RTRIM(Reason))) > 0)
);

IF OBJECT_ID(N'dbo.OvertimeInterval_Table', N'U') IS NULL
CREATE TABLE dbo.OvertimeInterval_Table (
    Id int IDENTITY PRIMARY KEY, EntryId int NOT NULL, StartMinute int NOT NULL, EndMinute int NOT NULL,
    CONSTRAINT FK_OvertimeInterval_Entry FOREIGN KEY (EntryId) REFERENCES dbo.OvertimeEntry_Table(Id) ON DELETE CASCADE,
    CONSTRAINT CK_OvertimeInterval_Range CHECK (StartMinute >= 0 AND StartMinute < 1440 AND EndMinute > StartMinute AND EndMinute <= 1440)
);

IF OBJECT_ID(N'dbo.OvertimeDelivery_Table', N'U') IS NULL
CREATE TABLE dbo.OvertimeDelivery_Table (
    Id int IDENTITY PRIMARY KEY, PeriodId int NOT NULL, UnitId int NOT NULL, ManagerId int NULL,
    UnitName nvarchar(200) NOT NULL, Status int NOT NULL, Revision int NOT NULL DEFAULT 0,
    IssuedAt datetime2 NULL, ReceivedAt datetime2 NULL, Version rowversion NOT NULL,
    Aciklama nvarchar(max) NULL, Olusturan nvarchar(max) NULL, OlusturmaTarihi datetime2 NULL,
    Degistiren nvarchar(max) NULL, DegistirmeTarihi datetime2 NULL,
    CONSTRAINT FK_OvertimeDelivery_Period FOREIGN KEY (PeriodId) REFERENCES dbo.OvertimePeriod_Table(Id),
    CONSTRAINT CK_OvertimeDelivery_Status CHECK (Status BETWEEN 0 AND 2)
);

IF OBJECT_ID(N'dbo.OvertimeSchedule_Table', N'U') IS NULL
CREATE TABLE dbo.OvertimeSchedule_Table (
    Id int IDENTITY PRIMARY KEY, TitleId int NOT NULL, WeeklyHours int NOT NULL,
    Aciklama nvarchar(max) NULL, Olusturan nvarchar(max) NULL, OlusturmaTarihi datetime2 NULL,
    Degistiren nvarchar(max) NULL, DegistirmeTarihi datetime2 NULL,
    CONSTRAINT CK_OvertimeSchedule_Hours CHECK (WeeklyHours IN (40,45))
);

IF OBJECT_ID(N'dbo.OvertimeResult_Table', N'U') IS NULL
CREATE TABLE dbo.OvertimeResult_Table (
    Id int IDENTITY PRIMARY KEY, PeriodId int NOT NULL, PersonelId int NOT NULL,
    FullName nvarchar(200) NOT NULL, SicilNo int NOT NULL, RawMinutes int NOT NULL, RoundedMinutes int NOT NULL,
    RuleVersion nvarchar(50) NOT NULL, WeeksJson nvarchar(max) NOT NULL,
    Aciklama nvarchar(max) NULL, Olusturan nvarchar(max) NULL, OlusturmaTarihi datetime2 NULL,
    Degistiren nvarchar(max) NULL, DegistirmeTarihi datetime2 NULL,
    CONSTRAINT FK_OvertimeResult_Period FOREIGN KEY (PeriodId) REFERENCES dbo.OvertimePeriod_Table(Id),
    CONSTRAINT CK_OvertimeResult_Minutes CHECK (RawMinutes >= 0 AND RoundedMinutes >= RawMinutes AND RoundedMinutes % 30 = 0)
);

IF OBJECT_ID(N'dbo.OvertimeEvent_Table', N'U') IS NULL
CREATE TABLE dbo.OvertimeEvent_Table (
    Id int IDENTITY PRIMARY KEY, EntryId int NULL, PeriodId int NULL, Action nvarchar(80) NOT NULL, Details nvarchar(max) NOT NULL,
    Aciklama nvarchar(max) NULL, Olusturan nvarchar(max) NULL, OlusturmaTarihi datetime2 NULL,
    Degistiren nvarchar(max) NULL, DegistirmeTarihi datetime2 NULL
);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'dbo.OvertimeEntry_Table') AND name = N'UX_OvertimeEntry_PersonDate')
    CREATE UNIQUE INDEX UX_OvertimeEntry_PersonDate ON dbo.OvertimeEntry_Table(PersonelId, WorkDate);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'dbo.OvertimeEntry_Table') AND name = N'IX_OvertimeEntry_UnitDate')
    CREATE INDEX IX_OvertimeEntry_UnitDate ON dbo.OvertimeEntry_Table(UnitId, WorkDate);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'dbo.OvertimeEntry_Table') AND name = N'IX_OvertimeEntry_Period')
    CREATE INDEX IX_OvertimeEntry_Period ON dbo.OvertimeEntry_Table(PeriodId);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'dbo.OvertimeInterval_Table') AND name = N'IX_OvertimeInterval_Entry')
    CREATE INDEX IX_OvertimeInterval_Entry ON dbo.OvertimeInterval_Table(EntryId);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'dbo.OvertimeDelivery_Table') AND name = N'UX_OvertimeDelivery_PeriodUnit')
    CREATE UNIQUE INDEX UX_OvertimeDelivery_PeriodUnit ON dbo.OvertimeDelivery_Table(PeriodId, UnitId);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'dbo.OvertimeSchedule_Table') AND name = N'UX_OvertimeSchedule_Title')
    CREATE UNIQUE INDEX UX_OvertimeSchedule_Title ON dbo.OvertimeSchedule_Table(TitleId);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'dbo.OvertimeResult_Table') AND name = N'UX_OvertimeResult_PeriodPerson')
    CREATE UNIQUE INDEX UX_OvertimeResult_PeriodPerson ON dbo.OvertimeResult_Table(PeriodId, PersonelId);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'dbo.OvertimeEvent_Table') AND name = N'IX_OvertimeEvent_Entry')
    CREATE INDEX IX_OvertimeEvent_Entry ON dbo.OvertimeEvent_Table(EntryId);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'dbo.OvertimeEvent_Table') AND name = N'IX_OvertimeEvent_Period')
    CREATE INDEX IX_OvertimeEvent_Period ON dbo.OvertimeEvent_Table(PeriodId);

COMMIT;
