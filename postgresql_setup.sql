create table user_profiles
(
    uuid        varchar(64)                                not null
        constraint user_profiles_pk
            primary key,
    username    varchar(32)                                not null,
    pfp_url     varchar(256) default ''::character varying not null,
    description varchar(100) default ''::character varying not null,
    is_disabled boolean      default false                 not null,
    is_deleted  boolean      default false                 not null
);

create table chat_rooms_info
(
    room_id         varchar(64)                                not null
        constraint chat_rooms_pk
            primary key,
    room_name       varchar(32)                                not null,
    description     varchar(80)  default ''::character varying not null,
    created_at      bigint                                     not null,
    last_activity   bigint                                     not null,
    is_public       boolean      default true                  not null,
    hashed_password varchar(100) default ''::character varying not null,
    max_users       integer      default 50                    not null
);

create table chat_rooms_personnel
(
    user_id      varchar(64)       not null
        constraint fk_user
            references user_profiles,
    chat_room_id varchar(64)       not null
        constraint fk_chat_room
            references chat_rooms_info,
    role         integer default 0 not null,
    constraint chat_rooms_personnel_pk
        primary key (user_id, chat_room_id)
);

create table message_contents
(
    message_id      varchar(64) not null
        constraint message_contents_pk
            primary key,
    message_content text        not null
);

create table message_reactions
(
    message_id     varchar(64) not null
        constraint fk_message_contents
            references message_contents,
    user_id        varchar(64) not null
        constraint fk_user_profiles
            references user_profiles,
    reaction_emoji varchar(1)  not null,
    constraint message_reactions_pk
        primary key (user_id, message_id),
    constraint message_reactions_uc
        unique (message_id, user_id)
);

create table monolithic_messages_pool
(
    message_id        varchar(64)      not null
        constraint fk_messages
            references message_contents,
    chat_room_id      varchar(64)      not null
        constraint fk_chat_rooms
            references chat_rooms_info,
    sender_id         varchar(64)      not null
        constraint fk_users
            references user_profiles,
    insertion_time    bigint,
    modification_time bigint,
    client_stamp      bigint default 0 not null,
    constraint monolithic_messages_pool_pk
        primary key (chat_room_id, message_id),
    constraint monolithic_messages_pool_uc
        unique (message_id, chat_room_id)
);

create table chat_room_invitations
(
    invitation_string varchar(32)           not null
        constraint chat_room_invitations_pk
            primary key,
    instigator_id     varchar(64)
        constraint chat_room_invitations_user_profiles_uuid_fk
            references user_profiles,
    chat_room_id      varchar(64)
        constraint chat_room_invitations_chat_rooms_info_room_id_fk
            references chat_rooms_info,
    is_enabled        boolean default false not null
);

