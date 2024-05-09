namespace Repository

module Repository =
  open System.Data
  // open Dapper.FSharp.PostgreSQL
  open Models
  open System
  open Npgsql.FSharp
  open System.Threading.Tasks

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


  let addTags (tags: string array) : Task<DatabaseModels.tags list> =
    connectionString
    |> Sql.connect
    |> Sql.query
      @"INSERT INTO tags 
        (id, name) 
       VALUES 
        (@tags) 
       ON CONFLICT (name) 
       DO NOTHING
       RETURNING *;
        "
    |> Sql.parameters [ "tags", Sql.stringArray tags ]
    |> Sql.executeAsync (fun read ->
      { id = read.uuid "id"
        name = read.string "name" })


  let getTags: Task<DatabaseModels.tags list> =
    connectionString
    |> Sql.connect
    |> Sql.query @"SELECT * FROM tags"
    |> Sql.executeAsync (fun read ->
      { id = read.uuid "id"
        name = read.string "name" })


  let findTags (tagsToFind: string list) : Task<DatabaseModels.tags list> =
    connectionString
    |> Sql.connect
    |> Sql.query @"SELECT * FROM tags WHERE name IN @tagsToFind"
    |> Sql.parameters [ "tagsToFind", Sql.stringArray (tagsToFind |> List.toArray) ]
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