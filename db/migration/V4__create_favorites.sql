-- liquibase formatted sql
-- changeset alaza5:create_favorites

CREATE TABLE favorites (
  article_id UUID NOT NULL,
  user_id UUID NOT NULL,
  favorited_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP NOT NULL,
  PRIMARY KEY (article_id, user_id),
  FOREIGN KEY (article_id) REFERENCES articles(id) ON DELETE CASCADE,
  FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE
);