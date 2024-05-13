namespace Models

open System

module DbTables =
  [<CLIMutable>]
  type UsersTable =
    { id: Guid
      email: string
      username: string
      password: string
      bio: string option
      image: string option
      created_at: DateTime
      updated_at: DateTime }

  [<CLIMutable>]
  type ArticlesTable =
    { id: Guid
      author_id: Guid
      slug: string
      title: string
      description: string
      body: string
      created_at: DateTime
      updated_at: DateTime }

  type ArticlesTagsTable = { article_id: Guid; tag_id: Guid }

  type TagsTable = { id: Guid; name: string }

  type CommentsTable =
    { id: Guid
      author_id: Guid
      article_id: Guid
      body: string
      created_at: DateTime
      updated_at: DateTime }