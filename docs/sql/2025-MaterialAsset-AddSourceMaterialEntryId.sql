-- Tech-Debt #1 — Phase A
-- MaterialAsset_Table'a SourceMaterialEntryId (nullable FK) ekler.
-- Mevcut kayıtlar NULL kalır; geriye dönük tahmin yapılmaz.
-- Bu script idempotent'tir: tekrar çalıştırılırsa hata vermez.

IF NOT EXISTS (
    SELECT 1
      FROM sys.columns
     WHERE Name = N'SourceMaterialEntryId'
       AND Object_ID = Object_ID(N'dbo.MaterialAsset_Table')
)
BEGIN
    ALTER TABLE dbo.MaterialAsset_Table
        ADD SourceMaterialEntryId INT NULL;
END
GO

IF NOT EXISTS (
    SELECT 1
      FROM sys.foreign_keys
     WHERE name = N'FK_MaterialAsset_Table_MaterialEntry_Table_SourceMaterialEntryId'
)
BEGIN
    ALTER TABLE dbo.MaterialAsset_Table
        ADD CONSTRAINT FK_MaterialAsset_Table_MaterialEntry_Table_SourceMaterialEntryId
        FOREIGN KEY (SourceMaterialEntryId)
        REFERENCES dbo.MaterialEntry_Table (Id)
        ON DELETE SET NULL;
END
GO

IF NOT EXISTS (
    SELECT 1
      FROM sys.indexes
     WHERE name = N'IX_MaterialAsset_Table_SourceMaterialEntryId'
       AND object_id = OBJECT_ID(N'dbo.MaterialAsset_Table')
)
BEGIN
    CREATE INDEX IX_MaterialAsset_Table_SourceMaterialEntryId
        ON dbo.MaterialAsset_Table (SourceMaterialEntryId);
END
GO
