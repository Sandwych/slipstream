-- 注意，此文件中，除了语句结束以外不允许再有分号，而且所有语句结束后必须有分号

CREATE TABLE core_model (
    _id BIGSERIAL NOT NULL,
    "name" VARCHAR NOT NULL UNIQUE,
    label VARCHAR,
    info TEXT,
    module VARCHAR NOT NULL,
    PRIMARY KEY(_id)
);
CREATE INDEX index_core_model_name ON core_model ("name");

CREATE TABLE core_field (
    _id BIGSERIAL NOT NULL,
    "module" VARCHAR NOT NULL,
    model BIGINT NOT NULL,
    "name" VARCHAR NOT NULL,
    "required" BOOLEAN NOT NULL,
    "readonly" BOOLEAN NOT NULL,
    relation VARCHAR,
    label VARCHAR,
    "type" VARCHAR NOT NULL,
    help TEXT,
    PRIMARY KEY(_id),
    FOREIGN KEY(model) REFERENCES core_model(_id) ON DELETE CASCADE
);
CREATE INDEX index_core_field_name ON core_field ("name");

CREATE TABLE core_module (
    _id BIGSERIAL NOT NULL,  
    "name" VARCHAR(128) NOT NULL UNIQUE,
    label VARCHAR(256),
    "state" VARCHAR(16) NOT NULL,
    demo BOOLEAN NOT NULL DEFAULT FALSE,
    author VARCHAR(128),
    url VARCHAR(128),
    version VARCHAR(64),
    info TEXT,
    license VARCHAR(32),
    PRIMARY KEY(_id)
);
CREATE UNIQUE INDEX index_core_module_name ON core_module ("name");


CREATE TABLE core_user (
    _id BIGSERIAL NOT NULL,
    PRIMARY KEY(_id)
);


CREATE TABLE core_organization (
    _id BIGSERIAL NOT NULL,
    PRIMARY KEY(_id)
);


CREATE TABLE core_model_data (
	_id BIGSERIAL NOT NULL,
	name VARCHAR(128) NOT NULL,
	module VARCHAR(64) NOT NULL,
	model VARCHAR(64) NOT NULL,
	ref_id BIGINT NOT NULL,
	value TEXT,
	PRIMARY KEY(_id)
);
CREATE UNIQUE INDEX index_core_model_data_name ON core_model_data ("name");

CREATE TABLE core_session (
	_id BIGSERIAL NOT NULL,
	sid VARCHAR(128) NOT NULL,
	start_time TIMESTAMP NOT NULL,
	last_activity_time TIMESTAMP NOT NULL,
	userid BIGINT NOT NULL,
	"login" VARCHAR(128) NOT NULL,
	PRIMARY KEY(_id)
);
CREATE UNIQUE INDEX index_core_sessionid ON core_session("sid");
