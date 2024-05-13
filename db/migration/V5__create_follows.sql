-- liquibase formatted sql
-- changeset alaza5:create_follows

CREATE TABLE follows (
  user_id UUID NOT NULL,
  following_id UUID NOT NULL,
  PRIMARY KEY (user_id, following_id),
  followed_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP NOT NULL,
  FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE,
  FOREIGN KEY (following_id) REFERENCES users(id) ON DELETE CASCADE
);