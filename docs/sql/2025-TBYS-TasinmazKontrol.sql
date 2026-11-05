/*
  TBYS - Taşınmaz Kontrolü şeması (idempotent).
  Jenga.Models.TBYS.Kontrol.* varlıklarıyla ve ApplicationDbContext yapılandırmasıyla birebirdir.
  Enum kolonları int olarak saklanır (EF varsayılanı). Hiçbir FK cascade delete yapmaz (geçmiş korunur).
  Form şablonu (TASINMAZ-KONTROL v1) uygulama açılışında KontrolFormSeed ile idempotent eklenir.
*/
SET NOCOUNT ON;
SET XACT_ABORT ON;
BEGIN TRANSACTION;

IF OBJECT_ID(N'dbo.Tasinmaz_Table', N'U') IS NULL
BEGIN
	RAISERROR('Tablo bulunamadı: Tasinmaz_Table', 16, 1);
	ROLLBACK TRANSACTION; RETURN;
END;

/* ---------- TasinmazKontrolForm_Table ---------- */
IF OBJECT_ID(N'dbo.TasinmazKontrolForm_Table', N'U') IS NULL
BEGIN
	CREATE TABLE dbo.TasinmazKontrolForm_Table
	(
		Id                     INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_TasinmazKontrolForm_Table PRIMARY KEY,
		Kod                    NVARCHAR(50)   NOT NULL,
		Ad                     NVARCHAR(200)  NOT NULL,
		Surum                  INT            NOT NULL CONSTRAINT DF_TasinmazKontrolForm_Surum DEFAULT(1),
		Kapsam                 INT            NOT NULL CONSTRAINT DF_TasinmazKontrolForm_Kapsam DEFAULT(0),
		KapsamCinsi            NVARCHAR(100)  NULL,
		KapsamTasinmazId       INT            NULL,
		Durum                  INT            NOT NULL CONSTRAINT DF_TasinmazKontrolForm_Durum DEFAULT(0),
		KaynakFormId           INT            NULL,
		VarsayilanPeriyotSure  INT            NULL,
		VarsayilanPeriyotBirim INT            NULL,
		TanimJson              NVARCHAR(MAX)  NOT NULL CONSTRAINT DF_TasinmazKontrolForm_TanimJson DEFAULT(N'{}'),
		YayimTarihi            DATETIME2      NULL,
		Yayimlayan             NVARCHAR(MAX)  NULL,
		ArsivTarihi            DATETIME2      NULL,
		Arsivleyen             NVARCHAR(MAX)  NULL,
		KayitSurumu            INT            NOT NULL CONSTRAINT DF_TasinmazKontrolForm_KayitSurumu DEFAULT(0),
		Aciklama               NVARCHAR(MAX)  NULL,
		Olusturan              NVARCHAR(MAX)  NULL,
		OlusturmaTarihi        DATETIME2      NULL,
		Degistiren             NVARCHAR(MAX)  NULL,
		DegistirmeTarihi       DATETIME2      NULL,
		CONSTRAINT FK_TasinmazKontrolForm_KaynakForm FOREIGN KEY (KaynakFormId) REFERENCES dbo.TasinmazKontrolForm_Table(Id) ON DELETE NO ACTION
	);
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UX_TasinmazKontrolForm_Kod_Surum' AND object_id = OBJECT_ID(N'dbo.TasinmazKontrolForm_Table'))
	CREATE UNIQUE INDEX UX_TasinmazKontrolForm_Kod_Surum ON dbo.TasinmazKontrolForm_Table(Kod, Surum);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_TasinmazKontrolForm_Kapsam' AND object_id = OBJECT_ID(N'dbo.TasinmazKontrolForm_Table'))
	CREATE INDEX IX_TasinmazKontrolForm_Kapsam ON dbo.TasinmazKontrolForm_Table(Kapsam, KapsamCinsi, KapsamTasinmazId, Durum);

/* ---------- TasinmazKontrol_Table ---------- */
IF OBJECT_ID(N'dbo.TasinmazKontrol_Table', N'U') IS NULL
BEGIN
	CREATE TABLE dbo.TasinmazKontrol_Table
	(
		Id                          INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_TasinmazKontrol_Table PRIMARY KEY,
		TasinmazId                  INT            NOT NULL,
		FormId                      INT            NOT NULL,
		BolgeId                     INT            NULL,
		BolgeAdi                    NVARCHAR(200)  NULL,
		KontrolEdenler              NVARCHAR(500)  NULL,
		ImzalayanMakam              NVARCHAR(200)  NULL,
		RaporNo                     NVARCHAR(50)   NULL,
		KontrolTarihi               DATETIME2      NOT NULL,
		Durum                       INT            NOT NULL CONSTRAINT DF_TasinmazKontrol_Durum DEFAULT(0),
		TamamlanmaTarihi            DATETIME2      NULL,
		Tamamlayan                  NVARCHAR(MAX)  NULL,
		TasinmazSnapshotJson        NVARCHAR(MAX)  NOT NULL CONSTRAINT DF_TasinmazKontrol_Snapshot DEFAULT(N'{}'),
		CevaplarJson                NVARCHAR(MAX)  NOT NULL CONSTRAINT DF_TasinmazKontrol_Cevaplar DEFAULT(N'{}'),
		OncekiPeriyotSure           INT            NULL,
		OncekiPeriyotBirim          INT            NULL,
		SecilenPeriyotSure          INT            NULL,
		SecilenPeriyotBirim         INT            NULL,
		PeriyotDegisiklikGerekcesi  NVARCHAR(MAX)  NULL,
		HesaplananSonrakiTarih      DATETIME2      NULL,
		ElleBelirlenenSonrakiTarih  DATETIME2      NULL,
		TarihDegisiklikGerekcesi    NVARCHAR(MAX)  NULL,
		PlanGuncellendi             BIT            NOT NULL CONSTRAINT DF_TasinmazKontrol_PlanGuncellendi DEFAULT(0),
		RevizeEdilenKontrolId       INT            NULL,
		IslemGerekcesi              NVARCHAR(MAX)  NULL,
		IslemYapan                  NVARCHAR(MAX)  NULL,
		IslemTarihi                 DATETIME2      NULL,
		KayitSurumu                 INT            NOT NULL CONSTRAINT DF_TasinmazKontrol_KayitSurumu DEFAULT(0),
		Aciklama                    NVARCHAR(MAX)  NULL,
		Olusturan                   NVARCHAR(MAX)  NULL,
		OlusturmaTarihi             DATETIME2      NULL,
		Degistiren                  NVARCHAR(MAX)  NULL,
		DegistirmeTarihi            DATETIME2      NULL,
		CONSTRAINT FK_TasinmazKontrol_Form     FOREIGN KEY (FormId)                REFERENCES dbo.TasinmazKontrolForm_Table(Id) ON DELETE NO ACTION,
		CONSTRAINT FK_TasinmazKontrol_Tasinmaz FOREIGN KEY (TasinmazId)            REFERENCES dbo.Tasinmaz_Table(Id)            ON DELETE NO ACTION,
		CONSTRAINT FK_TasinmazKontrol_Revize   FOREIGN KEY (RevizeEdilenKontrolId) REFERENCES dbo.TasinmazKontrol_Table(Id)     ON DELETE NO ACTION
	);
END;

/* Sonradan eklenen sütunlar (mevcut kurulumlar için) */
IF COL_LENGTH(N'dbo.TasinmazKontrol_Table', N'ImzalayanMakam') IS NULL
	ALTER TABLE dbo.TasinmazKontrol_Table ADD ImzalayanMakam NVARCHAR(200) NULL;
IF COL_LENGTH(N'dbo.TasinmazKontrol_Table', N'RaporNo') IS NULL
	ALTER TABLE dbo.TasinmazKontrol_Table ADD RaporNo NVARCHAR(50) NULL;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_TasinmazKontrol_Tasinmaz_Tarih' AND object_id = OBJECT_ID(N'dbo.TasinmazKontrol_Table'))
	CREATE INDEX IX_TasinmazKontrol_Tasinmaz_Tarih ON dbo.TasinmazKontrol_Table(TasinmazId, KontrolTarihi);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_TasinmazKontrol_Bolge_Durum' AND object_id = OBJECT_ID(N'dbo.TasinmazKontrol_Table'))
	CREATE INDEX IX_TasinmazKontrol_Bolge_Durum ON dbo.TasinmazKontrol_Table(BolgeId, Durum);

/* ---------- TasinmazKontrolEk_Table ---------- */
IF OBJECT_ID(N'dbo.TasinmazKontrolEk_Table', N'U') IS NULL
BEGIN
	CREATE TABLE dbo.TasinmazKontrolEk_Table
	(
		Id               INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_TasinmazKontrolEk_Table PRIMARY KEY,
		KontrolId        INT            NOT NULL,
		AlanKodu         NVARCHAR(100)  NOT NULL,
		SatirNo          INT            NULL,
		GrupKodu         NVARCHAR(100)  NULL,
		DosyaAdi         NVARCHAR(260)  NOT NULL,
		SaklananAd       NVARCHAR(260)  NOT NULL,
		IcerikTipi       NVARCHAR(100)  NULL,
		Boyut            BIGINT         NOT NULL CONSTRAINT DF_TasinmazKontrolEk_Boyut DEFAULT(0),
		Aciklama         NVARCHAR(MAX)  NULL,
		Olusturan        NVARCHAR(MAX)  NULL,
		OlusturmaTarihi  DATETIME2      NULL,
		Degistiren       NVARCHAR(MAX)  NULL,
		DegistirmeTarihi DATETIME2      NULL,
		CONSTRAINT FK_TasinmazKontrolEk_Kontrol FOREIGN KEY (KontrolId) REFERENCES dbo.TasinmazKontrol_Table(Id) ON DELETE NO ACTION
	);
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_TasinmazKontrolEk_Kontrol' AND object_id = OBJECT_ID(N'dbo.TasinmazKontrolEk_Table'))
	CREATE INDEX IX_TasinmazKontrolEk_Kontrol ON dbo.TasinmazKontrolEk_Table(KontrolId);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UX_TasinmazKontrolEk_SaklananAd' AND object_id = OBJECT_ID(N'dbo.TasinmazKontrolEk_Table'))
	CREATE UNIQUE INDEX UX_TasinmazKontrolEk_SaklananAd ON dbo.TasinmazKontrolEk_Table(SaklananAd);

/* ---------- TasinmazKontrolPlani_Table ---------- */
IF OBJECT_ID(N'dbo.TasinmazKontrolPlani_Table', N'U') IS NULL
BEGIN
	CREATE TABLE dbo.TasinmazKontrolPlani_Table
	(
		Id                   INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_TasinmazKontrolPlani_Table PRIMARY KEY,
		TasinmazId           INT            NOT NULL,
		PeriyotSure          INT            NULL,
		PeriyotBirim         INT            NULL,
		SonrakiKontrolTarihi DATETIME2      NULL,
		KaynakKontrolId      INT            NULL,
		DegisiklikGerekcesi  NVARCHAR(MAX)  NULL,
		KayitSurumu          INT            NOT NULL CONSTRAINT DF_TasinmazKontrolPlani_KayitSurumu DEFAULT(0),
		Aciklama             NVARCHAR(MAX)  NULL,
		Olusturan            NVARCHAR(MAX)  NULL,
		OlusturmaTarihi      DATETIME2      NULL,
		Degistiren           NVARCHAR(MAX)  NULL,
		DegistirmeTarihi     DATETIME2      NULL,
		CONSTRAINT FK_TasinmazKontrolPlani_Tasinmaz FOREIGN KEY (TasinmazId)      REFERENCES dbo.Tasinmaz_Table(Id)        ON DELETE NO ACTION,
		CONSTRAINT FK_TasinmazKontrolPlani_Kontrol  FOREIGN KEY (KaynakKontrolId) REFERENCES dbo.TasinmazKontrol_Table(Id) ON DELETE NO ACTION
	);
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UX_TasinmazKontrolPlani_Tasinmaz' AND object_id = OBJECT_ID(N'dbo.TasinmazKontrolPlani_Table'))
	CREATE UNIQUE INDEX UX_TasinmazKontrolPlani_Tasinmaz ON dbo.TasinmazKontrolPlani_Table(TasinmazId);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_TasinmazKontrolPlani_SonrakiTarih' AND object_id = OBJECT_ID(N'dbo.TasinmazKontrolPlani_Table'))
	CREATE INDEX IX_TasinmazKontrolPlani_SonrakiTarih ON dbo.TasinmazKontrolPlani_Table(SonrakiKontrolTarihi);

/* ---------- Menü (Auth_MenuItem_Table) ----------
   Mevcut TBYS üst menüsü altına eklenir; TBYS üst menüsü bulunamazsa uyarı verir ve atlanır.
   Kimlik yerine Url üzerinden idempotent. */
IF OBJECT_ID(N'dbo.Auth_MenuItem_Table', N'U') IS NOT NULL
BEGIN
	DECLARE @tbysParent INT = (SELECT TOP 1 Id FROM dbo.Auth_MenuItem_Table WHERE Url LIKE N'/tbys/%' AND ParentId IS NOT NULL ORDER BY Id);
	SET @tbysParent = (SELECT ParentId FROM dbo.Auth_MenuItem_Table WHERE Id = @tbysParent);

	IF @tbysParent IS NULL
		PRINT N'UYARI: TBYS üst menüsü bulunamadı; menü kayıtları eklenmedi. /tbys/tasinmaz-kontrol ve /tbys/tasinmaz-kontrol-formlari adreslerini elle ekleyin.';
	ELSE
	BEGIN
		DECLARE @sira INT = ISNULL((SELECT MAX(DisplayOrder) FROM dbo.Auth_MenuItem_Table WHERE ParentId = @tbysParent), 0);

		IF NOT EXISTS (SELECT 1 FROM dbo.Auth_MenuItem_Table WHERE Url = N'/tbys/tasinmaz-kontrol')
			INSERT INTO dbo.Auth_MenuItem_Table (Title, ParentId, Url, DisplayOrder, IsVisible, Aciklama)
			VALUES (N'Taşınmaz Kontrolleri', @tbysParent, N'/tbys/tasinmaz-kontrol', @sira + 1, 1, N'Taşınmaz kontrol planlama ve geçmişi');

		IF NOT EXISTS (SELECT 1 FROM dbo.Auth_MenuItem_Table WHERE Url = N'/tbys/tasinmaz-kontrol-formlari')
			INSERT INTO dbo.Auth_MenuItem_Table (Title, ParentId, Url, DisplayOrder, IsVisible, Aciklama)
			VALUES (N'Kontrol Form Şablonları', @tbysParent, N'/tbys/tasinmaz-kontrol-formlari', @sira + 2, 1, N'Taşınmaz kontrol formu şablonları ve sürümleri');
	END;
END;

COMMIT TRANSACTION;
PRINT N'TBYS Taşınmaz Kontrolü şeması hazır.';
