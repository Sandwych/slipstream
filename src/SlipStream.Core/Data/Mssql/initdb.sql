-- 注意，此文件中，尽管“GO”语句不是 PostgreSQL 支持的 SQL 语言的一部分，
-- 但是每个 SQL 语句之后仍然需要一个 GO 命令将其与其它语句隔开

CREATE TABLE core_meta_entity (
    _id BIGINT NOT NULL IDENTITY(1,1),
    [name] NVARCHAR(256) NOT NULL UNIQUE,
    label NVARCHAR(256) NULL,
    info NTEXT NULL,
    module NVARCHAR(128) NOT NULL,
    PRIMARY KEY(_id)
);
GO

CREATE INDEX index_core_meta_entity_name ON core_meta_entity ([name]);
GO

CREATE TABLE core_meta_field (
    _id BIGINT NOT NULL IDENTITY(1,1),
    [module] NVARCHAR(128) NOT NULL,
    meta_entity BIGINT NOT NULL,
    [name] NVARCHAR(64) NOT NULL,
    [required] BIT NOT NULL,
    [readonly] BIT NOT NULL,
    relation NVARCHAR(256) NULL,
    label NVARCHAR(256) NULL,
    [type] NVARCHAR(32) NOT NULL,
    help NTEXT NULL,
    PRIMARY KEY(_id),
    FOREIGN KEY(meta_entity) REFERENCES core_meta_entity(_id) ON DELETE CASCADE
);
GO

CREATE INDEX index_core_meta_field_name ON core_meta_field ([name]);
GO

CREATE TABLE core_module (
    _id BIGINT NOT NULL IDENTITY(1,1),
    [name] NVARCHAR(128) NOT NULL UNIQUE,
    label NVARCHAR(256) NULL,
    [state] NVARCHAR(16) NOT NULL,
    demo BIT NOT NULL DEFAULT 0,
    author NVARCHAR(128) NULL,
    url NVARCHAR(128) NULL,
    version NVARCHAR(64) NULL,
    info NTEXT NULL,
    license NVARCHAR(32) NULL,
    PRIMARY KEY(_id)
);
GO

CREATE UNIQUE INDEX index_core_module_name ON core_module ([name]);
GO

CREATE TABLE core_module_dependency (
    _id BIGINT NOT NULL IDENTITY(1,1),
    name NVARCHAR(128) NOT NULL,
    module BIGINT NOT NULL,
    PRIMARY KEY(_id),
    FOREIGN KEY (module) REFERENCES core_module ON DELETE CASCADE
);
GO

CREATE UNIQUE INDEX index_core_module_dependency_name ON core_module_dependency ([name]);
GO

CREATE TABLE core_user (
    _id BIGINT NOT NULL IDENTITY(1,1),
    PRIMARY KEY(_id)
);
GO

CREATE TABLE core_organization (
    _id BIGINT NOT NULL IDENTITY(1,1),
    PRIMARY KEY(_id)
);
GO


CREATE TABLE core_entity_data (
	_id BIGINT NOT NULL IDENTITY(1,1),
	name NVARCHAR(128) NOT NULL,
	module NVARCHAR(64) NOT NULL,
	entity NVARCHAR(64) NOT NULL,
	ref_id BIGINT NOT NULL,
	value NTEXT NULL,
	PRIMARY KEY(_id)
);
GO

CREATE UNIQUE INDEX index_core_entity_data_name ON core_entity_data ([name]);
GO

CREATE TABLE [core_session] (
	[_id] BIGINT NOT NULL IDENTITY(1,1),
	[token] NVARCHAR(128) NOT NULL,
	[start_time] DATETIME NOT NULL,
	[last_activity_time] DATETIME NOT NULL,
	[userid] BIGINT NOT NULL,
	[login] NVARCHAR(128) NOT NULL,
	PRIMARY KEY(_id)
);
GO

CREATE UNIQUE INDEX [index_core_sessionid] ON [core_session]([token]);
GO

-- 下面全部是存储过程/函数

-- 处理树的函数

