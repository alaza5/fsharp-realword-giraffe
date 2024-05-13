namespace Repository

open Models
open Npgsql.FSharp
open Npgsql

type Parameters = list<string * SqlValue>

type Count = { count: int64 }

type SqlWithParameters =
  { sql: string
    parameters: list<Parameters> }


module Common =
  NpgsqlConnection.GlobalTypeMapper.EnableDynamicJson() |> ignore

  // TODO move it to config
  let connectionString: string =
    Sql.host "localhost"
    |> Sql.port 5432
    |> Sql.database "fsharp_giraffe_database"
    |> Sql.username "myuser"
    |> Sql.password "mypassword"
    |> Sql.formatConnectionString

  let readUser (read: RowReader) : DbTables.UsersTable =
    { id = read.uuid "id"
      email = read.string "email"
      username = read.string "username"
      bio = read.stringOrNone "bio"
      password = read.string "password"
      image = read.stringOrNone "image"
      created_at = read.dateTime "created_at"
      updated_at = read.dateTime "updated_at" }

  let readComment (read: RowReader) : DbTables.CommentsTable =
    { id = read.uuid "id"
      author_id = read.uuid "author_id"
      article_id = read.uuid "article_id"
      body = read.string "body"
      created_at = read.dateTime "created_at"
      updated_at = read.dateTime "updated_at" }

  let createInsertTagQueries (tags: string list) : SqlWithParameters =
    let sqlInsertTags =
      @"
      INSERT INTO tags 
          (name) 
      VALUES 
          (@tag) 
      ON CONFLICT (name) 
      DO NOTHING
       "

    let tagParams: Parameters list =
      tags |> List.map (fun tag -> [ "tag", Sql.string tag ])

    { sql = sqlInsertTags
      parameters = tagParams }


  let createInsertArticle (article: DbTables.ArticlesTable) =
    let sqlInsertArticle =
      @"
      INSERT INTO articles 
          (id, author_id, slug, title, description, body, created_at, updated_at)
      VALUES 
          (@id, @author_id, @slug, @title, @description, @body, @created_at, @updated_at)
      "

    let insertArticleParam: Parameters list =
      [ [ "id", Sql.uuid article.id
          "author_id", Sql.uuid article.author_id
          "slug", Sql.string article.slug
          "title", Sql.string article.title
          "description", Sql.string article.description
          "body", Sql.string article.body
          "created_at", Sql.date article.created_at
          "updated_at", Sql.date article.updated_at ] ]

    { sql = sqlInsertArticle
      parameters = insertArticleParam }


  let readArticleRow (read: RowReader) : DbTables.ArticlesTable =
    { id = read.uuid "id"
      author_id = read.uuid "author_id"
      slug = read.string "slug"
      title = read.string "title"
      description = read.string "description"
      body = read.string "body"
      created_at = read.dateTime "created_at"
      updated_at = read.dateTime "updated_at" }