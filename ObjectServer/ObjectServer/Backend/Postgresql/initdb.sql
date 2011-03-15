-- 注意，此文件中，除了语句结束以外不允许再有分号，而且所有语句结束后必须有分号

CREATE TABLE core_model (
    id BIGSERIAL NOT NULL,
    "name" VARCHAR NOT NULL,
    label VARCHAR,
    info TEXT,
    module VARCHAR NOT NULL,
    PRIMARY KEY(id)
);

CREATE TABLE core_field (
    id BIGSERIAL NOT NULL,
    "module" VARCHAR NOT NULL,
    model BIGINT NOT NULL,
    "name" VARCHAR NOT NULL,
    relation VARCHAR,
    label VARCHAR,
    "type" VARCHAR NOT NULL,
    help TEXT,
    PRIMARY KEY(id),
    FOREIGN KEY(model) REFERENCES core_model(id) ON DELETE CASCADE
);

CREATE TABLE core_module (
    id BIGSERIAL NOT NULL,  
    website VARCHAR(256),
    "name" VARCHAR(128) NOT NULL,
    author VARCHAR(128),
    url VARCHAR(128),
    "state" VARCHAR(16) NOT NULL,
    latest_version VARCHAR(64),
    shortdesc VARCHAR(256),
    "certificate" VARCHAR(64),
    info TEXT,
    demo BOOLEAN DEFAULT FALSE,
    web BOOLEAN DEFAULT FALSE,
    license VARCHAR(32),
    PRIMARY KEY(id)
);
ALTER TABLE core_module ADD CONSTRAINT name_uniq UNIQUE (name);


CREATE TABLE core_user (
    id BIGSERIAL NOT NULL,
    PRIMARY KEY(id)
);


CREATE TABLE core_organization (
    id BIGSERIAL NOT NULL,
    PRIMARY KEY(id)
);