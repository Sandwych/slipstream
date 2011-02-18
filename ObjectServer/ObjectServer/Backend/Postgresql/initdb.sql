-- 注意，此文件中，除了语句结束以外不允许再有分号，而且所有语句结束后必须有分号

create table core_model (
    id bigserial not null,
    "name" varchar not null,
    label varchar,
    info text,
    module varchar,
    primary key(id)
);

create table core_field (
    id bigserial not null,
    model bigint,
    "name" varchar not null,
    relation varchar,
    label varchar,
    "type" varchar not null,
    help text,
    primary key(id),
    foreign key(model) references core_model(id) on delete cascade
);

create table core_module (
    id bigserial not null,  
    website varchar(256),
    "name" varchar(128) not null,
    author varchar(128),
    url varchar(128),
    state varchar(16) not null,
    latest_version varchar(64),
    shortdesc varchar(256),
    certificate varchar(64),
    description text,
    demo boolean default false,
    web boolean default false,
    license varchar(32),
    primary key(id)
);
alter table core_module add constraint name_uniq unique (name);