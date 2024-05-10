namespace Repository

module Repository =
  open System.Data
  open Models
  open System
  open Npgsql.FSharp
  open System.Threading.Tasks

  type Params = list<string * SqlValue>

  // TODO move it to config
  let connectionString: string =
    Sql.host "localhost"
    |> Sql.port 5432
    |> Sql.database "fsharp_giraffe_database"
    |> Sql.username "myuser"
    |> Sql.password "mypassword"
    |> Sql.formatConnectionString

  let registerUser (data: DatabaseModels.users) : Task<DatabaseModels.users> =
    printfn $">> connectionString {connectionString}"

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


  let fetchCurrentUser (email: string) : Task<DatabaseModels.users> =
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


  let createArticle (data: DatabaseModels.articles) : Task<DatabaseModels.articles> =
    connectionString
    |> Sql.connect
    |> Sql.query
      @"INSERT INTO articles 
          (id, author_id, slug, title, description, body, created_at, updated_at)
          VALUES 
          (@id, @author_id, @slug, @title, @description, @body, @created_at, @updated_at)
          RETURNING *
        "
    |> Sql.parameters
      [ "id", Sql.uuid data.id
        "author_id", Sql.uuid data.author_id
        "slug", Sql.string data.slug
        "title", Sql.string data.title
        "description", Sql.string data.description
        "body", Sql.string data.body
        "created_at", Sql.date data.created_at
        "updated_at", Sql.date data.updated_at ]
    |> Sql.executeRowAsync (fun read ->
      { id = read.uuid "id"
        author_id = read.uuid "author_id"
        slug = read.string "slug"
        title = read.string "title"
        description = read.string "description"
        body = read.string "body"
        created_at = read.dateTime "created_at"
        updated_at = read.dateTime "updated_at" })


  let addTags (tags: string array) : Task<int> =

    let tagParams =
      tags |> Array.map (fun tag -> ("tag", Sql.string tag)) |> Array.toList

    connectionString
    |> Sql.connect
    |> Sql.query
      @"
      INSERT INTO tags 
          (name) 
      VALUES 
          (@tag) 
      ON CONFLICT (name) 
      DO NOTHING
       "
    |> Sql.parameters tagParams
    |> Sql.executeNonQueryAsync


  let getTags: Task<DatabaseModels.tags list> =
    connectionString
    |> Sql.connect
    |> Sql.query @"SELECT * FROM tags"
    |> Sql.executeAsync (fun read ->
      { id = read.uuid "id"
        name = read.string "name" })


  let findTags (tagsToFind: string array) : Task<DatabaseModels.tags list> =
    connectionString
    |> Sql.connect
    |> Sql.query @"SELECT * FROM tags WHERE name = ANY (@tagsToFind)"
    |> Sql.parameters [ "@tagsToFind", Sql.stringArray tagsToFind ]
    |> Sql.executeAsync (fun read ->
      { id = read.uuid "id"
        name = read.string "name" })


  let findTag (name: string) : Task<DatabaseModels.tags> =
    connectionString
    |> Sql.connect
    |> Sql.query
      @"SELECT * 
        FROM tags 
        WHERE name = @name
      "
    |> Sql.parameters [ "name", Sql.string name ]
    |> Sql.executeRowAsync (fun read ->
      { id = read.uuid "id"
        name = read.string "name" })


  let insertTag (tag: DatabaseModels.tags) : Task<DatabaseModels.tags> =
    connectionString
    |> Sql.connect
    |> Sql.query
      @"
        INSERT INTO tags (id, name) 
        VALUES (@id, @name) 
        RETURNING *
        "
    |> Sql.parameters [ "id", Sql.uuid tag.id; "name", Sql.string tag.name ]
    |> Sql.executeRowAsync (fun read ->
      { id = read.uuid "id"
        name = read.string "name" })


  let addArticlesTags (articleId: Guid) (tagIds: Guid list) : Task<int list> =
    let parameters: list<Params> =
      tagIds
      |> List.map (fun tagId -> [ "articleId", Sql.uuid articleId; "tagId", Sql.uuid tagId ])

    let insertArticleTags =
      ("INSERT INTO articles_tags (article_id, tag_id) VALUES (@articleId, @tagId)", parameters)

    connectionString
    |> Sql.connect
    |> Sql.executeTransactionAsync [ insertArticleTags ]



  let getTagForArticle (articleId: Guid) : Task<DatabaseModels.tags> =
    connectionString
    |> Sql.connect
    |> Sql.query
      @"SELECT t.id, t.name
      FROM articles_tags at
      JOIN tags t ON at.tag_id = t.id
      WHERE at.article_id = @articleId"

    |> Sql.parameters [ "@articleId", Sql.uuid articleId ]
    |> Sql.executeRowAsync (fun read ->
      { id = read.uuid "id"
        name = read.string "name" })



  let getTagsForArticle (articleId: Guid) : Task<DatabaseModels.tags list> =
    connectionString
    |> Sql.connect
    |> Sql.query
      @"SELECT tags.id, tags.name
        FROM articles_tags
        JOIN tags ON articles_tags.tag_id = tags.id
        WHERE articles_tags.article_id = @articleId
       "

    |> Sql.parameters [ "@articleId", Sql.uuid articleId ]
    |> Sql.executeAsync (fun read ->
      { id = read.uuid "id"
        name = read.string "name" })




  let createArticleWithTags (data: DatabaseModels.articles) (tags: string array) : Task<int list> =
    let articleId = data.id

    // TODO extract it properly
    let sqlInsertArticle =
      @"
      INSERT INTO articles 
          (id, author_id, slug, title, description, body, created_at, updated_at)
      VALUES 
          (@id, @author_id, @slug, @title, @description, @body, @created_at, @updated_at)
      "

    let insertArticleParam: Params list =
      [ [ "id", Sql.uuid articleId
          "author_id", Sql.uuid data.author_id
          "slug", Sql.string data.slug
          "title", Sql.string data.title
          "description", Sql.string data.description
          "body", Sql.string data.body
          "created_at", Sql.date data.created_at
          "updated_at", Sql.date data.updated_at ] ]

    let insertArticleTags = (sqlInsertArticle, insertArticleParam)

    // TODO extract it properly
    let sqlInsertTags =
      @"
      INSERT INTO tags 
          (name) 
      VALUES 
          (@tag) 
      ON CONFLICT (name) 
      DO NOTHING
       "

    let tagParams: Params list =
      tags |> Array.map (fun tag -> [ "tag", Sql.string tag ]) |> Array.toList

    let insertTags = (sqlInsertTags, tagParams)

    let sqlInsertArticlesTags =
      @"
      INSERT INTO articles_tags (article_id, tag_id)
      SELECT @articleId AS article_id, id AS tag_id
      FROM tags
      WHERE name = ANY (@tagsToFind);
      "

    let insertArticlesTagsParams =
      [ [ "tagsToFind", Sql.stringArray tags; "articleId", Sql.uuid articleId ] ]

    let insertArticlesTags = (sqlInsertArticlesTags, insertArticlesTagsParams)

    connectionString
    |> Sql.connect
    |> Sql.executeTransactionAsync [ insertArticleTags; insertTags; insertArticlesTags ]