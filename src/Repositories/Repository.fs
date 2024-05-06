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

  let getUsersByEmail (conn: IDbConnection) (email: string) =
    select {
      for user in DatabaseModels.usersTable do
        where (user.email = email)
    }
    |> conn.SelectAsync<DatabaseModels.users>

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