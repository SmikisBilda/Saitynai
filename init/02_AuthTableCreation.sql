CREATE TABLE IF NOT EXISTS saitynai.users (
  id serial4 PRIMARY KEY,
  username text NOT NULL UNIQUE,
  email text Not NULL Unique,
  password_hash text NOT NULL
);

CREATE TABLE IF NOT EXISTS saitynai.roles (
  id serial4 PRIMARY KEY,
  "name" text NOT NULL UNIQUE
);

CREATE TABLE IF NOT EXISTS saitynai.user_roles (
  user_id int4 NOT NULL,
  role_id int4 NOT NULL,
  PRIMARY KEY (user_id, role_id),
  CONSTRAINT user_roles_user_id_fkey
    FOREIGN KEY (user_id)
    REFERENCES saitynai.users(id)
    ON DELETE CASCADE,
  CONSTRAINT user_roles_role_id_fkey
    FOREIGN KEY (role_id)
    REFERENCES saitynai.roles(id)
    ON DELETE CASCADE
);

CREATE TABLE IF NOT EXISTS saitynai.permissions (
  id serial4 PRIMARY KEY,
  "name" text NOT NULL UNIQUE
);


CREATE TABLE IF NOT EXISTS saitynai.resource_types (
  id serial4 PRIMARY KEY,
  "name" text NOT NULL UNIQUE
);

INSERT INTO saitynai.resource_types ("name") VALUES
('Building'),
('Floor'),
('Point'),
('AccessPoint'),
('Scan');

CREATE TABLE IF NOT EXISTS saitynai.role_permissions (
  role_id int4 NOT NULL,
  permission_id int4 NOT NULL,
  resource_type_id int4 NOT NULL REFERENCES saitynai.resource_types(id) ON DELETE CASCADE,
  resource_id int4 NOT NULL,
  allow bool NOT NULL DEFAULT true,
  cascade bool NOT NULL DEFAULT true, 
  

  PRIMARY KEY (role_id, permission_id, resource_type_id, resource_id),
  
  CONSTRAINT role_permissions_role_id_fkey
    FOREIGN KEY (role_id)
    REFERENCES saitynai.roles(id)
    ON DELETE CASCADE,
  CONSTRAINT role_permissions_permission_id_fkey
    FOREIGN KEY (permission_id)
    REFERENCES saitynai.permissions(id)
    ON DELETE CASCADE
);

CREATE TABLE IF NOT EXISTS saitynai.refresh_tokens (
    id SERIAL4 PRIMARY KEY,
    user_id INT4 NOT NULL,
    token TEXT NOT NULL,
    expires_on TIMESTAMPTZ NOT NULL,
    created_on TIMESTAMPTZ NOT NULL,
    revoked_on TIMESTAMPTZ NULL, -- When was this token used/revoked?
    replaced_by_token TEXT NULL, -- Which new token replaced this one?
    
    CONSTRAINT fk_refresh_tokens_user_id
        FOREIGN KEY (user_id)
        REFERENCES saitynai.users(id)
        ON DELETE CASCADE
);