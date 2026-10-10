BEGIN TRANSACTION;
ALTER TABLE [TaghvimTermis] ADD [RoozTerm] int NULL;

CREATE TABLE [Dars] (
    [Id] int NOT NULL IDENTITY,
    [CodeDars] nvarchar(20) NOT NULL,
    [NaamDars] nvarchar(200) NOT NULL,
    [VahedTeori] decimal(4,2) NULL,
    [VahedAmali] decimal(4,2) NULL,
    [SaatTeoriOrginal] int NULL,
    [SaatAmaliOrginal] int NULL,
    [SaatTeori] int NULL,
    [SaatAmali] int NULL,
    [TermAkhz] int NULL,
    [NoeDars] nvarchar(50) NULL,
    [NoeAzmoon] nvarchar(50) NULL,
    [ReshtehId] int NULL,
    [Zarfiat] int NULL,
    [CreatedAt] datetime2 NULL,
    [UpdatedAt] datetime2 NULL,
    CONSTRAINT [PK_Dars] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Dars_Reshtehs_ReshtehId] FOREIGN KEY ([ReshtehId]) REFERENCES [Reshtehs] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [SakhtemanKelass] (
    [Id] int NOT NULL IDENTITY,
    [MarkazId] int NOT NULL,
    [CodeSakhteman] int NOT NULL,
    [NaamSakhteman] nvarchar(100) NOT NULL,
    [CodeClass] int NOT NULL,
    [NammClass] nvarchar(100) NOT NULL,
    [NoeClass] nvarchar(50) NULL,
    [Vazeeyat] bit NOT NULL,
    [Zarfiat] int NULL,
    [Tabagheh] int NULL,
    [ZarfiatEmtahani] int NULL,
    [TedadWhiteboard] int NULL,
    [Projector] bit NOT NULL,
    [Emkanat] nvarchar(500) NULL,
    [Tozihat] nvarchar(500) NULL,
    [Telephon] nvarchar(20) NULL,
    [CreatedAt] datetime2 NULL,
    [UpdatedAt] datetime2 NULL,
    CONSTRAINT [PK_SakhtemanKelass] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_SakhtemanKelass_Markazes_MarkazId] FOREIGN KEY ([MarkazId]) REFERENCES [Markazes] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [DarsEraeh] (
    [Id] int NOT NULL IDENTITY,
    [CodeTerm] nvarchar(20) NOT NULL,
    [MarkazId] int NOT NULL,
    [ReshtehId] int NOT NULL,
    [DarsId] int NOT NULL,
    [CodeDars] nvarchar(20) NOT NULL,
    [Grooh] int NOT NULL,
    [NoeTadris] nvarchar(100) NULL,
    [UserIdMojavezDahandeh] int NULL,
    [NaghshMarkazMojavezDahandeh] nvarchar(100) NULL,
    [TarikheEraheh] datetime2 NULL,
    [Jensiat] int NULL,
    [VazeeyatDars] bit NOT NULL,
    [NahvehEraehDars] int NULL,
    [EmkanAkhzSayerMarakez] bit NOT NULL,
    [EmkanLinkBeSayerMarakez] bit NOT NULL,
    [ErtebatDehiId] int NULL,
    [BarnamehRizi] nvarchar(100) NULL,
    [SabtNami] int NULL,
    [CreatedAt] datetime2 NULL,
    [UpdatedAt] datetime2 NULL,
    CONSTRAINT [PK_DarsEraeh] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_DarsEraeh_DarsEraeh_ErtebatDehiId] FOREIGN KEY ([ErtebatDehiId]) REFERENCES [DarsEraeh] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_DarsEraeh_Dars_DarsId] FOREIGN KEY ([DarsId]) REFERENCES [Dars] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_DarsEraeh_Markazes_MarkazId] FOREIGN KEY ([MarkazId]) REFERENCES [Markazes] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_DarsEraeh_Reshtehs_ReshtehId] FOREIGN KEY ([ReshtehId]) REFERENCES [Reshtehs] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [ManbaDars] (
    [Id] int NOT NULL IDENTITY,
    [DarsId] int NOT NULL,
    [ShomareManba] nvarchar(20) NULL,
    [NoeManba] nvarchar(100) NULL,
    [Onvan] nvarchar(300) NOT NULL,
    [Nevisandeh] nvarchar(200) NULL,
    [Motarjem] nvarchar(200) NULL,
    [SalEnteshar] nvarchar(10) NULL,
    [SalEntesharMiladi] nvarchar(10) NULL,
    [Shabak] nvarchar(30) NULL,
    [Nasher] nvarchar(200) NULL,
    [NobateChap] nvarchar(20) NULL,
    [Vazeeyat] nvarchar(100) NULL,
    [CodePeyvast] nvarchar(50) NULL,
    [SharhPeyvast] nvarchar(1000) NULL,
    [TermUpdate] nvarchar(20) NULL,
    [CreatedAt] datetime2 NULL,
    [UpdatedAt] datetime2 NULL,
    CONSTRAINT [PK_ManbaDars] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ManbaDars_Dars_DarsId] FOREIGN KEY ([DarsId]) REFERENCES [Dars] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [DarsEraehOstad] (
    [Id] int NOT NULL IDENTITY,
    [OstadId] int NOT NULL,
    [DarsEraehId] int NOT NULL,
    [Asli] bit NOT NULL,
    CONSTRAINT [PK_DarsEraehOstad] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_DarsEraehOstad_DarsEraeh_DarsEraehId] FOREIGN KEY ([DarsEraehId]) REFERENCES [DarsEraeh] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_DarsEraehOstad_Ostads_OstadId] FOREIGN KEY ([OstadId]) REFERENCES [Ostads] ([Id]) ON DELETE NO ACTION
);

CREATE UNIQUE INDEX [IX_Dars_CodeDars_ReshtehId_Unique] ON [Dars] ([CodeDars], [ReshtehId]) WHERE [ReshtehId] IS NOT NULL;

CREATE INDEX [IX_Dars_NaamDars] ON [Dars] ([NaamDars]);

CREATE INDEX [IX_Dars_Reshteh_Term] ON [Dars] ([ReshtehId], [TermAkhz]);

CREATE INDEX [IX_Dars_ReshtehId] ON [Dars] ([ReshtehId]);

CREATE INDEX [IX_Dars_TermAkhz] ON [Dars] ([TermAkhz]);

CREATE INDEX [IX_DarsEraeh_DarsId] ON [DarsEraeh] ([DarsId]);

CREATE INDEX [IX_DarsEraeh_ErtebatDehiId] ON [DarsEraeh] ([ErtebatDehiId]);

CREATE INDEX [IX_DarsEraeh_MarkazId] ON [DarsEraeh] ([MarkazId]);

CREATE INDEX [IX_DarsEraeh_ReshtehId] ON [DarsEraeh] ([ReshtehId]);

CREATE INDEX [IX_DarsEraeh_Term_BarnamehRizi] ON [DarsEraeh] ([CodeTerm], [BarnamehRizi]);

CREATE INDEX [IX_DarsEraeh_Term_CodeDars] ON [DarsEraeh] ([CodeTerm], [CodeDars]);

CREATE INDEX [IX_DarsEraeh_Term_CodeDars_Grooh] ON [DarsEraeh] ([CodeTerm], [CodeDars], [Grooh]);

CREATE INDEX [IX_DarsEraeh_Term_Dars] ON [DarsEraeh] ([CodeTerm], [DarsId]);

CREATE INDEX [IX_DarsEraeh_Term_Markaz] ON [DarsEraeh] ([CodeTerm], [MarkazId]);

CREATE INDEX [IX_DarsEraeh_Term_Markaz_Reshteh] ON [DarsEraeh] ([CodeTerm], [MarkazId], [ReshtehId]);

CREATE INDEX [IX_DarsEraeh_Term_Markaz_Vazeeyat] ON [DarsEraeh] ([CodeTerm], [MarkazId], [VazeeyatDars]);

CREATE INDEX [IX_DarsEraeh_Term_Reshteh] ON [DarsEraeh] ([CodeTerm], [ReshtehId]);

CREATE UNIQUE INDEX [IX_DarsEraeh_Unique_Grooh] ON [DarsEraeh] ([CodeTerm], [MarkazId], [DarsId], [Grooh]);

CREATE INDEX [IX_OstadDars_Dars_Asli] ON [DarsEraehOstad] ([DarsEraehId], [Asli]);

CREATE INDEX [IX_OstadDars_DarsEraehId] ON [DarsEraehOstad] ([DarsEraehId]);

CREATE INDEX [IX_OstadDars_OstadId] ON [DarsEraehOstad] ([OstadId]);

CREATE UNIQUE INDEX [IX_OstadDars_Unique_Dars_Ostad] ON [DarsEraehOstad] ([DarsEraehId], [OstadId]);

CREATE INDEX [IX_ManbaDars_Dars_Shomare] ON [ManbaDars] ([DarsId], [ShomareManba]);

CREATE INDEX [IX_ManbaDars_DarsId] ON [ManbaDars] ([DarsId]);

CREATE INDEX [IX_ManbaDars_Onvan] ON [ManbaDars] ([Onvan]);

CREATE INDEX [IX_ManbaDars_Vazeeyat] ON [ManbaDars] ([Vazeeyat]);

CREATE INDEX [IX_SakhtemanKelass_Markaz_CodeSakhteman] ON [SakhtemanKelass] ([MarkazId], [CodeSakhteman]);

CREATE UNIQUE INDEX [IX_SakhtemanKelass_Markaz_Sakhteman_Class_Unique] ON [SakhtemanKelass] ([MarkazId], [CodeSakhteman], [CodeClass]);

CREATE INDEX [IX_SakhtemanKelass_MarkazId] ON [SakhtemanKelass] ([MarkazId]);

CREATE INDEX [IX_SakhtemanKelass_Vazeeyat] ON [SakhtemanKelass] ([Vazeeyat]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20261004140829_DarsManbaEraeh', N'10.0.9');

COMMIT;
GO

BEGIN TRANSACTION;
ALTER TABLE [Reshtehs] ADD [Vazeeat] bit NULL;

ALTER TABLE [GrooheAmoozeshis] ADD [Vazeeat] bit NULL;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20261005114250_AddVazeeatToGroohAmoozeshiReshteh', N'10.0.9');

COMMIT;
GO

