namespace Repository

module Repository =
  open System.Data
  open Dapper.FSharp.PostgreSQL
  open Dapper
  open Models
  open System
  open DapperExtensions.DapperExtensions


  let sqlInsertUser =
    """
      INSERT INTO users (id, email, username, password, bio, image, created_at, updated_at) 
            OUTPUT inserted.*
            VALUES (@id, @email, @username, @password, @bio, @image, @created_at, @updated_at)
    """

  let registerUser (conn: IDbConnection) (data: DatabaseModels.users) =
    conn.QuerySingleAsyncResult<DatabaseModels.users>(sqlInsertUser, data)



  let private sqlGetUserByEmail = "SELECT * FROM users WHERE email = @email"

  let fetchCurrentUser (conn: IDbConnection) (email: string) =
    conn.QuerySingleAsyncResult<DatabaseModels.users>(sqlGetUserByEmail, {| email = email |})



  let private sqlUpdateUser =
    "UPDATE users SET id = @id, email = @email, username = @username, password = @password, bio = @bio, image = @image, created_at = @created_at, updated_at = @updated_at WHERE email = @currentUserEmail"

  let updateUser
    (conn: IDbConnection)
    (currentUserEmail: string)
    (updatedUser: DatabaseModels.users)
    =
    let param =
      {| updatedUser with
          currentUserEmail = currentUserEmail |}

    conn.QuerySingleAsyncResult<DatabaseModels.users>(sqlUpdateUser, param)



  let sqlCreateArticle =
    "INSERT INTO articles (id, author_id, slug, title, description, body, created_at, updated_at) VALUES (@id, @author_id, @slug, @title, @description, @body, @created_at, @updated_at)"

  let createArticle (conn: IDbConnection) (article: DatabaseModels.articles) =
    conn.QuerySingleAsyncResult<DatabaseModels.articles>(sqlUpdateUser, article)



  let sqlAddTags =
    @"INSERT INTO tags (id, name) VALUES (@id, @name) ON CONFLICT (name) DO NOTHING;"

  let addTags (conn: IDbConnection) (tagsRequest: AddTagsRequest) =
    let listOfTags =
      tagsRequest.tags |> List.map (fun tag -> {| id = Guid.NewGuid(); name = tag |})

    conn.QuerySingleAsyncResult<DatabaseModels.tags>(sqlUpdateUser, listOfTags)


  let sqlGetTags = "SELECT * FROM tags"

  let getTags (conn: IDbConnection) =
    conn.QueryAsyncResult<DatabaseModels.tags>(sqlGetTags)


  let sqlFindInTags = sprintf "SELECT * FROM tags WHERE name IN @tagsToFind"

  let findTags (conn: IDbConnection) (tagsToFind: string list) =
    let data = {| tagsToFind = tagsToFind |}
    conn.QuerySingleAsync<DatabaseModels.tags>(sqlFindInTags, data)