namespace Repository

open System.Data
open Models
open System
open Npgsql.FSharp
open System.Threading.Tasks

type Parameters = list<string * SqlValue>

type SqlWithParameters =
  { sql: string
    parameters: list<Parameters> }

module Sqls =
  let createInsertTags (tags: string list) : SqlWithParameters =
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


  let createInsertArticle (article: DatabaseModels.articles) =
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



module Repository =

  // TODO move it to config
  let connectionString: string =
    Sql.host "localhost"
    |> Sql.port 5432
    |> Sql.database "fsharp_giraffe_database"
    |> Sql.username "myuser"
    |> Sql.password "mypassword"
    |> Sql.formatConnectionString

  let createAndGetUser (data: DatabaseModels.users) : Task<DatabaseModels.users> =
    connectionString
    |> Sql.connect
    |> Sql.query
      @"INSERT INTO users
          (id, email, username, password, bio, image, created_at, updated_at)
        VALUES
          (@id, @Email, @username, @password, @bio, @image, @created_at, @updated_at)
        RETURNING *"
    |> Sql.parameters
      [ "id", Sql.uuid data.id
        "email", Sql.string data.email
        "username", Sql.string data.username
        "bio", Sql.stringOrNone data.bio
        "password", Sql.string data.password
        "image", Sql.stringOrNone data.image
        "created_at", Sql.date data.created_at
        "updated_at", Sql.date data.updated_at ]
    |> Sql.executeRowAsync (fun read ->
      { id = read.uuid "id"
        email = read.string "email"
        username = read.string "username"
        bio = read.stringOrNone "bio"
        password = read.string "password"
        image = read.stringOrNone "image"
        created_at = read.dateTime "created_at"
        updated_at = read.dateTime "updated_at" })


  let getUserByEmail (email: string) : Task<DatabaseModels.users> =
    connectionString
    |> Sql.connect
    |> Sql.query
      @"
        SELECT 
            id, email, username, bio, password, image, created_at, updated_at
        FROM 
            users 
        WHERE 
            email = @email
        "
    |> Sql.parameters [ "email", Sql.string email ]
    |> Sql.executeRowAsync (fun read ->
      { id = read.uuid "id"
        email = read.string "email"
        username = read.string "username"
        bio = read.stringOrNone "bio"
        password = read.string "password"
        image = read.stringOrNone "image"
        created_at = read.dateTime "created_at"
        updated_at = read.dateTime "updated_at" })

  // maybe extract getting user by different props somehow? will see
  let getUserById (userId: Guid) : Task<DatabaseModels.users> =
    connectionString
    |> Sql.connect
    |> Sql.query
      @"
        SELECT 
            id, email, username, bio, password, image, created_at, updated_at
        FROM 
            users 
        WHERE 
            id = @id
        "
    |> Sql.parameters [ "id", Sql.uuid userId ]
    |> Sql.executeRowAsync (fun read ->
      { id = read.uuid "id"
        email = read.string "email"
        username = read.string "username"
        bio = read.stringOrNone "bio"
        password = read.string "password"
        image = read.stringOrNone "image"
        created_at = read.dateTime "created_at"
        updated_at = read.dateTime "updated_at" })


  let updateUser
    (currentUserEmail: string)
    (data: DatabaseModels.users)
    : Task<DatabaseModels.users> =
    connectionString
    |> Sql.connect
    |> Sql.query
      @"UPDATE users 
          SET 
            id = @id, 
            email = @Email, 
            username = @username, 
            password = @password, 
            bio = @bio, 
            image = @image, 
            created_at = @created_at, 
            updated_at = @updated_at 
          WHERE 
            email = @CurrentUserEmail
        "
    |> Sql.parameters
      [ "id", Sql.uuid data.id
        "Email", Sql.string data.email
        "username", Sql.string data.username
        "password", Sql.string data.password
        "bio", Sql.stringOrNone data.bio
        "image", Sql.stringOrNone data.image
        "created_at", Sql.date data.created_at
        "updated_at", Sql.date data.updated_at
        "CurrentUserEmail", Sql.string currentUserEmail ]
    |> Sql.executeRowAsync (fun read ->
      { id = read.uuid "id"
        email = read.string "email"
        username = read.string "username"
        bio = read.stringOrNone "bio"
        password = read.string "password"
        image = read.stringOrNone "image"
        created_at = read.dateTime "created_at"
        updated_at = read.dateTime "updated_at" })


  let addTags (tags: string list) : Task<int list> =
    let insertTags = Sqls.createInsertTags tags

    connectionString
    |> Sql.connect
    |> Sql.executeTransactionAsync [ (insertTags.sql, insertTags.parameters) ]


  let getAllTags: Task<DatabaseModels.tags list> =
    connectionString
    |> Sql.connect
    |> Sql.query @"SELECT * FROM tags"
    |> Sql.executeAsync (fun read ->
      { id = read.uuid "id"
        name = read.string "name" })


  let getTagsForArticle (articleId: Guid) : Task<DatabaseModels.tags list> =
    printfn $">> articleId {articleId}"

    connectionString
    |> Sql.connect
    |> Sql.query
      @"
      SELECT tags.id, tags.name 
      FROM articles_tags as at
      LEFT JOIN tags on at.tag_id = tags.id
      where at.article_id = @articleId
      "
    |> Sql.parameters [ "articleId", Sql.uuid articleId ]
    |> Sql.executeAsync (fun read ->
      { id = read.uuid "id"
        name = read.string "name" })



  // let findTags (tagsToFind: string array) : Task<DatabaseModels.tags list> =
  //   connectionString
  //   |> Sql.connect
  //   |> Sql.query @"SELECT * FROM tags WHERE name = ANY (@tagsToFind)"
  //   |> Sql.parameters [ "@tagsToFind", Sql.stringArray tagsToFind ]
  //   |> Sql.executeAsync (fun read ->
  //     { id = read.uuid "id"
  //       name = read.string "name" })


  // let insertTag (tag: DatabaseModels.tags) : Task<DatabaseModels.tags> =
  //   connectionString
  //   |> Sql.connect
  //   |> Sql.query
  //     @"
  //       INSERT INTO tags (id, name)
  //       VALUES (@id, @name)
  //       RETURNING *
  //       "
  //   |> Sql.parameters [ "id", Sql.uuid tag.id; "name", Sql.string tag.name ]
  //   |> Sql.executeRowAsync (fun read ->
  //     { id = read.uuid "id"
  //       name = read.string "name" })


  let createArticleWithTags
    (article: DatabaseModels.articles)
    (tags: string array)
    : Task<int list> =
    let insertArticle = Sqls.createInsertArticle article
    let insertTags = Sqls.createInsertTags (tags |> Array.toList)

    let sqlInsertArticlesTags =
      @"
      INSERT INTO articles_tags (article_id, tag_id)
      SELECT @articleId AS article_id, id AS tag_id
      FROM tags
      WHERE name = ANY (@tagsToFind);
      "

    let insertArticlesTagsParams =
      [ [ "tagsToFind", Sql.stringArray tags; "articleId", Sql.uuid article.id ] ]

    let insertArticlesTags = (sqlInsertArticlesTags, insertArticlesTagsParams)

    connectionString
    |> Sql.connect
    |> Sql.executeTransactionAsync
      [ (insertArticle.sql, insertArticle.parameters)
        (insertTags.sql, insertTags.parameters)
        insertArticlesTags ]


  let getArticleBySlug (slug: string) : Task<DatabaseModels.articles> =
    connectionString
    |> Sql.connect
    |> Sql.query
      @"SELECT *
        FROM articles
        WHERE articles.slug = @slug
      "
    |> Sql.parameters [ "slug", Sql.string slug ]
    |> Sql.executeRowAsync (fun read ->
      { id = read.uuid "id"
        author_id = read.uuid "author_id"
        slug = read.string "slug"
        title = read.string "title"
        description = read.string "description"
        body = read.string "body"
        created_at = read.dateTime "created_at"
        updated_at = read.dateTime "updated_at" })