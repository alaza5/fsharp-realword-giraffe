namespace Repository

module Repository =
  open System.Data
  open Dapper.FSharp.PostgreSQL
  open Dapper
  open Models
  open System
  open DapperExtensions.DapperExtensions
  open System.Collections.Generic


  let sqlInsertUser =
    @"INSERT INTO users (id, email, username, password, bio, image, created_at, updated_at) VALUES (@id, @Email, @username, @password, @bio, @image, @created_at, @updated_at);"
  // RETURNING *;

  let registerUser (conn: IDbConnection) (data: DatabaseModels.users) =
    printfn $">> data {data}"
    // let dictionary = new Dictionary<string, object>
    // {
    //     { "@ProductId", 1 }
    // };


    // let data2 =
    //   {| id = data.id
    //      email = data.email
    //      username = data.username
    //      password = data.password
    //      bio = data.bio
    //      image = data.image
    //      created_at = data.created_at
    //      updated_at = data.updated_at

    //   |}

    // let parameters = DynamicParameters(data2)
    // let dictionary = dict [ "@Email", "xd" ]
    // let parameters = DynamicParameters(dictionary)

    conn.Execute(sqlInsertUser, data)
  // conn.ExecuteAsync<DatabaseModels.users>(sqlInsertUser, data)
  // conn.QuerySingleAsyncResult<DatabaseModels.users>(sqlInsertUser, data)
  // conn.ExecuteAsync<DatabaseModels.users>(sqlInsertUser, data)
  // conn.QuerySingleAsyncResult<DatabaseModels.users>(sqlInsertUser, {| email = "lol" |})



  let private sqlGetUserByEmail = @"SELECT * FROM users WHERE email = @email"

  let fetchCurrentUser (conn: IDbConnection) (email: string) =
    conn.QuerySingleAsyncResult<DatabaseModels.users>(sqlGetUserByEmail, {| email = email |})



  let private sqlUpdateUser =
    @"UPDATE users SET id = @id, email = @email, username = @username, password = @password, bio = @bio, image = @image, created_at = @created_at, updated_at = @updated_at WHERE email = @currentUserEmail"

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
    @"INSERT INTO articles (id, author_id, slug, title, description, body, created_at, updated_at) VALUES (@id, @author_id, @slug, @title, @description, @body, @created_at, @updated_at)"

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