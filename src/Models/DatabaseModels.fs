namespace Models

open System

module DatabaseModels =
  open Dapper.FSharp.PostgreSQL

  [<CLIMutable>]
  type users =
    { id: Guid
      email: string
      username: string
      password: string
      bio: string option
      image: string option
      created_at: DateTime
      updated_at: DateTime }

  let usersTable = table<users>