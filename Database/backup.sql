CREATE TABLE users (
    user_id INTEGER PRIMARY KEY AUTOINCREMENT,
    username TEXT UNIQUE NOT NULL,
    password TEXT NOT NULL,
    salt TEXT NOT NULL
, is_admin INTEGER default 0 not null);
CREATE TABLE sqlite_sequence(name,seq);
CREATE TABLE rooms (
    room_id INTEGER PRIMARY KEY AUTOINCREMENT,
    room_name TEXT NOT NULL
);
CREATE TABLE room_users (
    room_id INTEGER NOT NULL,
    user_id INTEGER NOT NULL,
    PRIMARY KEY (room_id, user_id),
    FOREIGN KEY (room_id) REFERENCES rooms (room_id) ON DELETE CASCADE,
    FOREIGN KEY (user_id) REFERENCES users (user_id) ON DELETE CASCADE
);
CREATE TABLE messages (
    message_id INTEGER PRIMARY KEY AUTOINCREMENT,
    room_id INTEGER NOT NULL,
    timestamp DATETIME DEFAULT CURRENT_TIMESTAMP, message_data text,
    FOREIGN KEY (room_id) REFERENCES rooms (room_id) ON DELETE CASCADE
);
CREATE TABLE IF NOT EXISTS "auth_tokens"
(
    token_id integer
        primary key autoincrement,
    token    text not null
);
CREATE TABLE IF NOT EXISTS "user_tokens"
(
    user_id  INTEGER not null
        references users,
    token_id INTEGER not null
        references auth_tokens (tokken_id),
    primary key (user_id, token_id)
);