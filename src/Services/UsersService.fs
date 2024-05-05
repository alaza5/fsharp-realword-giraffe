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

  let postRegisterUser (next: HttpFunc) (ctx: HttpContext) =
    task {
      try
        // TODO validation and better mapping?
        let! request = ctx.BindJsonAsync<RegisterRequest>()
        let model = request.toDbModel ()

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
        let! users = Repository.getUsers conn request

        return!
          (match users |> Seq.tryHead with
           | None -> RequestErrors.NOT_FOUND $"User not found"
           | Some user ->
             let passwordCorrect = Hashing.verifyPassword request.password user.password

             match passwordCorrect with
             | false -> RequestErrors.UNAUTHORIZED "scheme" "realm" "Don't know who you are"
             | true ->
               let userResponse = user.response
               json userResponse)
            next
            ctx

      with ex ->
        return! RequestErrors.BAD_REQUEST $"Exception: {ex.Message}" next ctx
    }


  let getUser (next: HttpFunc) (ctx: HttpContext) = text "ok" next ctx

  let postUpdateUser (next: HttpFunc) (ctx: HttpContext) =
    task {
      try
        let! loginRequest = ctx.BindJsonAsync<UpdateUserRequest>()
        return! json loginRequest next ctx
      with ex ->
        return! RequestErrors.BAD_REQUEST $"Exception: {ex.Message}" next ctx
    }