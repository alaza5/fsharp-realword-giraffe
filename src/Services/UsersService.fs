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

        // TODO can this even fail?
        let! _ = Repository.registerUser conn model
        let response = model.response
        return! json response next ctx
      with ex ->
        return! RequestErrors.BAD_REQUEST $"Exception: {ex.Message}" next ctx
    }


  let postLoginUser (next: HttpFunc) (ctx: HttpContext) =
    task {
      try
        let! request = ctx.BindJsonAsync<LoginRequest>()
        use conn = ctx.GetService<IDbConnection>()
        let! users = Repository.getUsersByEmail conn request.email

        return!
          (match users |> Seq.tryHead with
           | None -> RequestErrors.NOT_FOUND $"User not found"
           | Some user ->
             let passwordCorrect = Hashing.verifyPassword request.password user.password

             match passwordCorrect with
             | false -> RequestErrors.UNAUTHORIZED "scheme" "realm" "Don't know who you are"
             | true -> json user.response)
            next
            ctx

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
        let! users = Repository.getUsersByEmail conn email

        return!
          (match users |> Seq.tryHead with
           | None -> RequestErrors.NOT_FOUND $"User not found"
           // TODO Do I need to return new token? api says so kinda?
           | Some user -> json user.response)
            next
            ctx

      with ex ->
        return! RequestErrors.BAD_REQUEST $"Exception: {ex.Message}" next ctx
    }

  let postUpdateUser (next: HttpFunc) (ctx: HttpContext) =
    task {
      try

        let userId = ctx.User.FindFirst(ClaimTypes.NameIdentifier)
        let currentUserEmail = userId.Value

        use conn = ctx.GetService<IDbConnection>()
        let! users = Repository.getUsersByEmail conn currentUserEmail
        let! updateRequest = ctx.BindJsonAsync<UpdateUserRequest>()

        match users |> Seq.tryHead with
        | None -> return! RequestErrors.NOT_FOUND $"User not found" next ctx
        | Some user ->
          let updatedUser = user.updateUser (updateRequest)
          let! returnedUser = Repository.updateUser conn currentUserEmail updatedUser

          match returnedUser |> Seq.tryHead with
          | None -> return! RequestErrors.NOT_FOUND $"User not found" next ctx
          | Some u -> return! json u.response next ctx


      with ex ->
        return! RequestErrors.BAD_REQUEST $"Exception: {ex.Message}" next ctx
    }