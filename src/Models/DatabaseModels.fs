namespace Models

open System

module DatabaseModels =
  open Dapper.FSharp.PostgreSQL

  [<CLIMutable>]
  type users =
    { id: Guid
      email: string
      username: string
      password: string
      bio: string option
      image: string option
      created_at: DateTime
      updated_at: DateTime }

  let usersTable = table<users>


  [<CLIMutable>]
  type articles =
    { id: Guid
      author_id: Guid
      slug: string
      title: string
      description: string
      body: string
      created_at: DateTime
      updated_at: DateTime }

  let articlesTable = table<articles>

  type articles_tags = { article_id: Guid; tag_id: Guid }

  let articles_tagsTable = table<articles_tags>