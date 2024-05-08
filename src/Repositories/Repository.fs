namespace Repository

module Repository =
  open System.Data
  open Dapper.FSharp.PostgreSQL
  open Models

  let registerUser (conn: IDbConnection) (data: DatabaseModels.users) =
    insert {
      into DatabaseModels.usersTable
      value data
    }
    |> conn.InsertOutputAsync<DatabaseModels.users, DatabaseModels.users>


  let fetchCurrentUser (conn: IDbConnection) (email: string) =
    task {
      let! users =
        select {
          for user in DatabaseModels.usersTable do
            where (user.email = email)
        }
        |> conn.SelectAsync<DatabaseModels.users>

      return
        match users |> Seq.tryHead with
        | Some x -> Ok x
        | None -> Error "User not found"
    }

  let updateUser
    (conn: IDbConnection)
    (currentUserEmail: string)
    (updatedUser: DatabaseModels.users)
    =
    update {
      for user in DatabaseModels.usersTable do
        set updatedUser
        where (user.email = currentUserEmail)
    }
    |> conn.UpdateOutputAsync<DatabaseModels.users, DatabaseModels.users>



  let createArticle (conn: IDbConnection) (article: DatabaseModels.articles) =
    task {
      let! retVal =
        insert {
          into DatabaseModels.articlesTable
          value article
        }
        |> conn.InsertOutputAsync<DatabaseModels.articles, DatabaseModels.articles>

      return
        match retVal |> Seq.tryHead with
        | None -> Error "Did't inserted article"
        | Some x -> Ok x
    }