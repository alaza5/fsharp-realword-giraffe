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
    |> conn.InsertAsync

  let getUsersByEmail (conn: IDbConnection) (email: string) =
    select {
      for user in DatabaseModels.usersTable do
        where (user.email = email)
    }
    |> conn.SelectAsync<DatabaseModels.users>