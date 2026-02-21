IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
CREATE TABLE [Tenants] (
    [Id] int NOT NULL IDENTITY,
    [FirebaseUid] nvarchar(128) NOT NULL,
    [PhoneNumber] nvarchar(32) NOT NULL,
    [FullName] nvarchar(200) NULL,
    [Email] nvarchar(256) NULL,
    [JoinDate] datetime2 NULL,
    [AdvanceAmount] decimal(18,2) NULL,
    [RoomRent] decimal(18,2) NULL,
    [IdProofType] nvarchar(100) NULL,
    [IdProofNumber] nvarchar(100) NULL,
    [CreatedAtUtc] datetime2 NOT NULL,
    [UpdatedAtUtc] datetime2 NOT NULL,
    CONSTRAINT [PK_Tenants] PRIMARY KEY ([Id])
);

CREATE UNIQUE INDEX [IX_Tenants_FirebaseUid] ON [Tenants] ([FirebaseUid]);

CREATE INDEX [IX_Tenants_PhoneNumber] ON [Tenants] ([PhoneNumber]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260208185850_AddRoomRentToTenant', N'9.0.8');

COMMIT;
GO

