CREATE TABLE core_model (
    id bigserial not null,
    "name" varchar not null,
    label varchar,
    info text,
    module varchar,
    PRIMARY KEY(id)
);

CREATE TABLE core_field (
    id bigserial NOT NULL,
    model bigint,
    "name" varchar NOT NULL,
    relation varchar,
    label varchar,
    "type" varchar NOT NULL,
    help text,
    PRIMARY KEY(id),
    FOREIGN KEY (model) REFERENCES core_model(id) ON DELETE CASCADE
);

CREATE TABLE core_module (
    id bigserial NOT NULL,  
    website varchar(256),
    "name" varchar(128) NOT NULL,
    author varchar(128),
    url varchar(128),
    state varchar(16) NOT NULL,
    latest_version varchar(64),
    shortdesc varchar(256),
    certificate varchar(64),
    description text,
    demo boolean default false,
    web boolean default false,
    license varchar(32),
    primary key(id)
);
ALTER TABLE core_module add constraint name_uniq unique (name);