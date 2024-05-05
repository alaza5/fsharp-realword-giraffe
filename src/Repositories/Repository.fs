namespace Repository

module Repository =
  open System.Data
  open Dapper.FSharp.PostgreSQL
  open Models
  open System
  open InternalSecurity

  let registerUser (conn: IDbConnection) (request: RegisterRequest) =
    let user: DatabaseModels.users =
      { id = Guid.NewGuid()
        email = request.email
        username = request.username
        password = InternalSecurity.hashPassword request.password
        bio = None
        image = None
        created_at = DateTime.UtcNow
        updated_at = DateTime.UtcNow }

    insert {
      into DatabaseModels.usersTable
      value user
    }
    |> conn.InsertAsync