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


  let readArticleRow (read: RowReader) : DatabaseModels.articles =
    { id = read.uuid "id"
      author_id = read.uuid "author_id"
      slug = read.string "slug"
      title = read.string "title"
      description = read.string "description"
      body = read.string "body"
      created_at = read.dateTime "created_at"
      updated_at = read.dateTime "updated_at" }

  let readUserRow (read: RowReader) : DatabaseModels.users =
    { id = read.uuid "id"
      email = read.string "email"
      username = read.string "username"
      bio = read.stringOrNone "bio"
      password = read.string "password"
      image = read.stringOrNone "image"
      created_at = read.dateTime "created_at"
      updated_at = read.dateTime "updated_at" }



module Repository =
  open Npgsql
  open Models.Models
  // Fix this
  NpgsqlConnection.GlobalTypeMapper.EnableDynamicJson() |> ignore

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
          (@id, @email, @username, @password, @bio, @image, @created_at, @updated_at)
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
    |> Sql.executeRowAsync Sqls.readUserRow


  // maybe extract getting user by different props somehow? will see
  let getUserByEmail (email: string) : Task<DatabaseModels.users> =
    connectionString
    |> Sql.connect
    |> Sql.query
      @"
        SELECT *
        FROM users 
        WHERE email = @email
       "
    |> Sql.parameters [ "email", Sql.string email ]
    |> Sql.executeRowAsync Sqls.readUserRow

  let getUserById (userId: Guid) : Task<DatabaseModels.users> =
    connectionString
    |> Sql.connect
    |> Sql.query
      @"
        SELECT *
        FROM users
        WHERE id = @id
       "
    |> Sql.parameters [ "id", Sql.uuid userId ]
    |> Sql.executeRowAsync Sqls.readUserRow


  let getUserByUsername (username: string) : Task<DatabaseModels.users> =
    connectionString
    |> Sql.connect
    |> Sql.query
      @"
        SELECT *
        FROM users 
        WHERE username = @username
       "
    |> Sql.parameters [ "@username", Sql.string username ]
    |> Sql.executeRowAsync Sqls.readUserRow


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
            email = @currentUserEmail
          RETURNING *
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
        "currentUserEmail", Sql.string currentUserEmail ]
    |> Sql.executeRowAsync Sqls.readUserRow


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


  // could change it to get by GetArticlesFilters
  let getArticleBySlug (slug: string) : Task<DatabaseModels.articles> =
    connectionString
    |> Sql.connect
    |> Sql.query
      @"SELECT *
        FROM articles
        WHERE articles.slug = @slug
        LIMIT 1
      "
    |> Sql.parameters [ "slug", Sql.string slug ]
    |> Sql.executeRowAsync Sqls.readArticleRow

  // https://stackoverflow.com/a/64223435
  let getArticlesWithUsersAndTags
    (filters: GetArticlesFilters)
    // TODO kind of sucks but no default/optional parameters in fsharp?
    : Task<DatabaseModels.ArticleUserTags list> =
    printfn $">> queryParams {filters}"

    connectionString
    |> Sql.connect
    |> Sql.query
      @"
      WITH tag_array AS (
        SELECT 
            at.article_id,
            ARRAY_AGG(t.name) AS tags
        FROM articles_tags AS at 
        join tags AS t ON t.id = at.tag_id
        GROUP BY at.article_id
      )
      SELECT 
          to_json(a) as article,
          to_json(u) as user,
          COALESCE(tag_array.tags, ARRAY[]::text[]) AS tags
      FROM 
          articles AS a
      JOIN 
          users AS u ON a.author_id = u.id 
      LEFT JOIN 
          tag_array ON tag_array.article_id = a.id
      WHERE 
          (@author::text IS NULL OR u.username = @author)
      AND 
          (@slug::text IS NULL OR a.slug = @slug)
      AND 
          (@tag::text IS NULL OR @tag::text = ANY(tags))
      ORDER BY 
          a.created_at
      LIMIT
          @limit
     "
    |> Sql.parameters
      [ "@author", Sql.stringOrNone filters.author
        "@slug", Sql.stringOrNone filters.slug
        "@tag", Sql.stringOrNone filters.tag
        "@limit", Sql.int64OrNone filters.limit ]
    |> Sql.executeAsync (fun read ->
      // extract the readeres maybe?
      { article = read.fieldValue<DatabaseModels.articles> "article"
        user = read.fieldValue<DatabaseModels.users> "user"
        tags = read.stringArray "tags" })



  //  could be done by UPDATE SET COALESCE(@title, title)
  let updateArticle (data: DatabaseModels.articles) =
    connectionString
    |> Sql.connect
    |> Sql.query
      @"
      UPDATE articles 
      SET 
        author_id = @author_id,
        slug = @slug,
        title = @title,
        description = @description,
        body = @body,
        created_at = @created_at,
        updated_at = @updated_at 
      WHERE 
        id = @id
      RETURNING *
       "
    // TODO doesnt this need checking if author is the same?
    // AND author_id = @authorId
    |> Sql.parameters
      [ "@id", Sql.uuid data.id
        "@author_id", Sql.uuid data.author_id
        "@slug", Sql.string data.slug
        "@title", Sql.string data.title
        "@description", Sql.string data.description
        "@body", Sql.string data.body
        "@created_at", Sql.date data.created_at
        "@updated_at", Sql.date data.updated_at ]
    |> Sql.executeNonQueryAsync


  let deleteArticle (slug: string) =
    connectionString
    |> Sql.connect
    |> Sql.query
      @"
      DELETE FROM articles
      WHERE articles.slug = @slug
      "
    |> Sql.parameters [ "slug", Sql.string slug ]
    |> Sql.executeNonQueryAsync

  let insertComment (slug: string) (userId: Guid) (body: string) =
    connectionString
    |> Sql.connect
    |> Sql.query
      @"
      INSERT INTO comments (author_id, article_id, body) 
      VALUES 
        (
        @userId,
        (SELECT id FROM articles a WHERE a.slug = @slug LIMIT 1),
        @body
        )
      "
    |> Sql.parameters
      [ "@userId", Sql.uuid userId
        "@slug", Sql.string slug
        "@body", Sql.string body ]
    |> Sql.executeNonQueryAsync

  let getComments (slug: string) : Task<DatabaseModels.CommentsWithAuthors list> =
    connectionString
    |> Sql.connect
    |> Sql.query
      @"
      SELECT 
        to_json(c) as comment, to_json(u) as user
      FROM 
        comments c
      LEFT JOIN 
        articles a on a.id = article_id
      LEFT JOIN 
        users u on u.id = c.author_id
      WHERE 
        a.slug = @slug
      "
    |> Sql.parameters [ "@slug", Sql.string slug ]
    |> Sql.executeAsync (fun read ->
      { comment = read.fieldValue<DatabaseModels.comments> "comment"
        user = read.fieldValue<DatabaseModels.users> "user" })


  let deleteComment (slug: string) (commentId: string) =
    connectionString
    |> Sql.connect
    |> Sql.query
      @"
      DELETE 
      FROM comments c
      USING articles a 
      WHERE a.id = c.article_id
      AND a.slug = @slug
      AND c.id = @commentId::uuid;
      "
    |> Sql.parameters [ "slug", Sql.string slug; "commentId", Sql.string commentId ]
    |> Sql.executeNonQueryAsync

  let addFavoriteArticle (userId: Guid) (slug: string) =
    connectionString
    |> Sql.connect
    |> Sql.query
      @"
      INSERT INTO favorites
          (article_id, user_id)
      SELECT 
        a.id, @userId
      FROM
          articles a
      WHERE
          a.slug = @slug
      ON CONFLICT (article_id, user_id) 
      DO NOTHING
      "
    |> Sql.parameters [ "@slug", Sql.string slug; "@userId", Sql.uuid userId ]
    |> Sql.executeNonQueryAsync

  let removeFavoriteArticle (userId: Guid) (slug: string) =
    printfn $">> (slug {slug}"
    printfn $">> userId {userId}"

    connectionString
    |> Sql.connect
    |> Sql.query
      @"
      DELETE 
      FROM favorites f
      USING articles a 
      WHERE a.id = f.article_id
      AND a.slug = @slug
      AND f.user_id = @userId
      "
    |> Sql.parameters [ "@slug", Sql.string slug; "@userId", Sql.uuid userId ]
    |> Sql.executeNonQueryAsync


  // let addFollow (userId: Guid) (followerUsername: string) =
  //   connectionString
  //   |> Sql.connect
  //   |> Sql.query
  //     @"
  //     INSERT INTO follows
  //       (user_id, follower_id)
  //     VALUES
  //       (
  //       (SELECT id FROM users u WHERE u.username = @followerUsername LIMIT 1),
  //       @userId
  //       )
  //     "
  //   |> Sql.parameters
  //     [ "@userId", Sql.uuid userId; "@followerUsername", Sql.string followerUsername ]
  //   |> Sql.executeNonQueryAsync

  // let removeFollow (userId: Guid) (followerUsername: string) =
  //   connectionString
  //   |> Sql.connect
  //   |> Sql.query
  //     @"
  //     DELETE FROM follows
  //     WHERE
  //       user_id = (SELECT id FROM users WHERE username = @followerUsername LIMIT 1)
  //     AND
  //       follower_id = @userId;
  //     "
  //   |> Sql.parameters
  //     [ "@userId", Sql.uuid userId; "@followerUsername", Sql.string followerUsername ]
  //   |> Sql.executeNonQueryAsync


  let addFollow (userId: Guid) (followingId: Guid) =
    connectionString
    |> Sql.connect
    |> Sql.query
      @"
      INSERT INTO follows
        (user_id, following_id)
      VALUES 
        (@userId, @followingId)
      ON CONFLICT (user_id, following_id)
      DO NOTHING
      "
    |> Sql.parameters [ "@userId", Sql.uuid userId; "@followingId", Sql.uuid followingId ]
    |> Sql.executeNonQueryAsync

  let removeFollow (userId: Guid) (followingId: Guid) =
    connectionString
    |> Sql.connect
    |> Sql.query
      @"
      DELETE FROM follows
      WHERE 
        user_id = @userId
      AND 
        following_id = @followingId
      "
    |> Sql.parameters [ "@userId", Sql.uuid userId; "@followingId", Sql.uuid followingId ]
    |> Sql.executeNonQueryAsync


  type Count = { count: int64 }

  let isFollowing (userId: Guid) (followingId: Guid) : Task<Count> =
    connectionString
    |> Sql.connect
    |> Sql.query
      @"
        SELECT COUNT(*)
        FROM follows
        WHERE user_id = @followingId
        AND following_id = @userId
        "
    |> Sql.parameters [ "@userId", Sql.uuid userId; "@followingId", Sql.uuid followingId ]
    |> Sql.executeRowAsync (fun read -> { count = read.int64 "count" })