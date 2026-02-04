CREATE TABLE users (
    id UUID PRIMARY KEY,
    password VARCHAR(255) NOT NULL,
    email VARCHAR(255) NOT NULL UNIQUE,
    user_type VARCHAR(50) NOT NULL,
    enabled BOOLEAN DEFAULT TRUE,
    locked BOOLEAN DEFAULT FALSE
);

CREATE UNIQUE INDEX idx_users_email ON users(email);