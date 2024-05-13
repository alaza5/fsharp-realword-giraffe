namespace Repository



module ArticlesRepository =
  open Models.Models
  open Models
  open System
  open Npgsql.FSharp
  open System.Threading.Tasks

  let getAllTags: Task<DbTables.TagsTable list> =
    Common.connectionString
    |> Sql.connect
    |> Sql.query @"SELECT * FROM tags"
    |> Sql.executeAsync (fun read ->
      { id = read.uuid "id"
        name = read.string "name" })


  let getArticleTags (articleId: Guid) : Task<DbTables.TagsTable list> =
    Common.connectionString
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

  let createArticle
    (article: DbTables.ArticlesTable)
    (tags: string array)
    : Task<int list> =
    let insertArticle = Common.createInsertArticle article
    let insertTags = Common.createInsertTagQueries (tags |> Array.toList)

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

    Common.connectionString
    |> Sql.connect
    |> Sql.executeTransactionAsync
      [ (insertArticle.sql, insertArticle.parameters)
        (insertTags.sql, insertTags.parameters)
        insertArticlesTags ]


  // could change it to get by GetArticlesFilters
  let getArticleBySlug (slug: string) : Task<DbTables.ArticlesTable> =
    Common.connectionString
    |> Sql.connect
    |> Sql.query
      @"
        SELECT *
        FROM articles
        WHERE slug = @slug
      "
    |> Sql.parameters [ "@slug", Sql.string slug ]
    |> Sql.executeRowAsync Common.readArticleRow

  // https://stackoverflow.com/a/64223435
  let getArticlesWithUsersAndTags
    (filters: GetArticlesFilters)
    // TODO kind of sucks but no default/optional parameters in fsharp?
    : Task<DbModels.ArticleUserTags list> =

    Common.connectionString
    |> Sql.connect
    |> Sql.query
      @"
      WITH tag_array AS (
          SELECT 
              at.article_id,
              ARRAY_AGG(t.name) AS tags
          FROM articles_tags AS at 
          JOIN tags AS t ON t.id = at.tag_id
          GROUP BY at.article_id
      ),
      favorites_info AS (
          SELECT 
              article_id,
              ARRAY_AGG(u.username) AS favorited_users
          FROM favorites
          left join users u on favorites.user_id = u.id
          GROUP BY article_id
      ),
      following_users AS (
      SELECT 
          f.user_id,
          ARRAY_AGG(u.username) AS following_usernames
      FROM follows f
      LEFT JOIN users u ON f.following_id = u.id 
      GROUP BY f.user_id
      )
      SELECT 
        to_json(a) as article,
        to_json(u) as author,
        COALESCE(tag_array.tags, ARRAY[]::text[]) AS tags,
        COALESCE(fav_info.favorited_users, ARRAY[]::text[]) AS favorited_users,
        COALESCE(following_users.following_usernames, ARRAY[]::text[]) AS following_users

      FROM
          users AS u 
      JOIN
          articles AS a on a.author_id = u.id
      LEFT JOIN 
          tag_array ON tag_array.article_id = a.id
      LEFT JOIN
          favorites_info AS fav_info ON fav_info.article_id = a.id
      left join 
      	following_users on following_users.user_id = u.id

      WHERE 
          (@author::text IS NULL OR u.username = @author)
      AND 
          (@slug::text IS NULL OR a.slug = @slug)
      AND 
          (@tag::text IS NULL OR @tag::text = ANY(tags))
      AND
          (@followedUserName::text IS NULL OR @followedUserName = any(following_users.following_usernames) )
      AND
          (@favoritedUserName::text IS NULL OR @favoritedUserName = any(fav_info.favorited_users) )

      ORDER BY 
          a.created_at
      LIMIT
          @limit
     "
    |> Sql.parameters
      [ "@author", Sql.stringOrNone filters.authorName
        "@slug", Sql.stringOrNone filters.slug
        "@tag", Sql.stringOrNone filters.tag
        "@author", Sql.stringOrNone filters.authorName
        "@favoritedUserName", Sql.stringOrNone filters.favoritedUserName
        "@followedUserName", Sql.stringOrNone filters.followedUserName
        "@limit", Sql.int64OrNone filters.limit ]
    |> Sql.executeAsync (fun read ->
      // extract the readeres maybe?
      { article = read.fieldValue<DbTables.ArticlesTable> "article"
        user = read.fieldValue<DbTables.UsersTable> "author"
        tags = read.stringArray "tags"
        favoritedUsers = read.stringArray "favorited_users" })



  //  could be done by UPDATE SET COALESCE(@title, title)
  let updateArticle (data: DbTables.ArticlesTable) =
    Common.connectionString
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
       "
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
    Common.connectionString
    |> Sql.connect
    |> Sql.query
      @"
      DELETE FROM articles
      WHERE articles.slug = @slug
      "
    |> Sql.parameters [ "slug", Sql.string slug ]
    |> Sql.executeNonQueryAsync

  let insertAndReturnComment
    (articleId: Guid)
    (userId: Guid)
    (body: string)
    : Task<DbTables.CommentsTable> =
    Common.connectionString
    |> Sql.connect
    |> Sql.query
      @"
      INSERT INTO comments (author_id, article_id, body) 
      VALUES 
        (
        @userId,
        @articleId,
        @body
        )
      RETURNING *
      "
    |> Sql.parameters
      [ "@userId", Sql.uuid userId
        "@articleId", Sql.uuid articleId
        "@body", Sql.string body ]
    |> Sql.executeRowAsync Common.readComment

  let getCommentsWithAuthors
    (slug: string)
    : Task<DbModels.CommentsWithAuthors list> =
    Common.connectionString
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
      { comment = read.fieldValue<DbTables.CommentsTable> "comment"
        user = read.fieldValue<DbTables.UsersTable> "user" })


  let deleteComment (slug: string) (commentId: string) : Task<int> =
    Common.connectionString
    |> Sql.connect
    |> Sql.query
      @"
      DELETE 
      FROM comments c
      USING articles a 
      WHERE a.id = c.article_id
      AND a.slug = @slug
      AND c.id = @commentId::uuid
      "
    |> Sql.parameters
      [ "slug", Sql.string slug; "commentId", Sql.string commentId ]
    |> Sql.executeNonQueryAsync

  let addFavoriteArticle (userId: Guid) (slug: string) =
    Common.connectionString
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
    Common.connectionString
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