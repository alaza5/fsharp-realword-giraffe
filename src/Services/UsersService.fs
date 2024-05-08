namespace UsersService

module UsersService =
  open Giraffe
  open Microsoft.AspNetCore.Http
  open Models
  open Repository
  open System.Data
  open ModelsMappers.DatabaseMappers
  open ModelsMappers.ResponseMappers
  open InternalSecurity
  open System.Security.Claims

  let postRegisterUser (next: HttpFunc) (ctx: HttpContext) =
    task {
      try
        // TODO validation and better mapping?
        let! request = ctx.BindJsonAsync<RegisterRequest>()
        let model = request.toDbModel

        use conn = ctx.GetService<IDbConnection>()

        let! users = Repository.registerUser conn model

        let response =
          match users |> Seq.tryHead with
          | None -> RequestErrors.NOT_FOUND $"User not found"
          | Some user -> json user.response

        return! response next ctx

      with ex ->
        return! RequestErrors.BAD_REQUEST $"Exception: {ex.Message}" next ctx
    }


  let postLoginUser (next: HttpFunc) (ctx: HttpContext) =
    task {
      try
        let! request = ctx.BindJsonAsync<LoginRequest>()
        use conn = ctx.GetService<IDbConnection>()
        let! user = Repository.fetchCurrentUser conn request.email

        let response =
          match user with
          | Error message -> RequestErrors.NOT_FOUND message
          | Ok user ->
            let passwordCorrect = Hashing.verifyPassword request.password user.password

            match passwordCorrect with
            | false -> RequestErrors.UNAUTHORIZED "scheme" "realm" "Don't know who you are"
            | true -> json user.response

        return! response next ctx
      with ex ->
        return! RequestErrors.BAD_REQUEST $"Exception: {ex.Message}" next ctx
    }


  let getCurrentUser (next: HttpFunc) (ctx: HttpContext) =
    task {
      try

        // TODO can I do FindFirstValue?
        let userId = ctx.User.FindFirst(ClaimTypes.NameIdentifier)
        let email = userId.Value

        use conn = ctx.GetService<IDbConnection>()
        let! user = Repository.fetchCurrentUser conn email

        let response =
          match user with
          | Error message -> RequestErrors.NOT_FOUND message
          | Ok user -> json user.response

        return! response next ctx
      with ex ->
        return! RequestErrors.BAD_REQUEST $"Exception: {ex.Message}" next ctx
    }

  let postUpdateUser (next: HttpFunc) (ctx: HttpContext) =
    task {
      try

        let userId = ctx.User.FindFirst(ClaimTypes.NameIdentifier)
        let currentUserEmail = userId.Value

        use conn = ctx.GetService<IDbConnection>()
        let! user = Repository.fetchCurrentUser conn currentUserEmail
        let! updateRequest = ctx.BindJsonAsync<UpdateUserRequest>()

        match user with
        | Error message -> return! RequestErrors.NOT_FOUND message next ctx
        | Ok user ->
          let updatedUser = user.updateUser (updateRequest)
          let! returnedUser = Repository.updateUser conn currentUserEmail updatedUser

          match returnedUser |> Seq.tryHead with
          | None -> return! RequestErrors.NOT_FOUND $"User not found" next ctx
          | Some u -> return! json u.response next ctx


      with ex ->
        return! RequestErrors.BAD_REQUEST $"Exception: {ex.Message}" next ctx
    }