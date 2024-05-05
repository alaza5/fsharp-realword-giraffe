namespace UsersService

module UsersService =
  open Giraffe
  open Microsoft.AspNetCore.Http
  open Models
  open Repository
  open System.Data
  open ModelsMappers.DatabaseMappers
  open ModelsMappers.ResponseMappers

  let postRegisterUser (next: HttpFunc) (ctx: HttpContext) =
    task {
      try
        // TODO validation and better mapping?
        let! request = ctx.BindJsonAsync<RegisterRequest>()
        let model = request.toDbModel ()

        use conn = ctx.GetService<IDbConnection>()

        // TODO can this even fail?
        let! _ = Repository.registerUser conn model
        let response = model.toResponse ()
        return! json response next ctx
      with ex ->
        return! RequestErrors.BAD_REQUEST $"Exception: {ex.Message}" next ctx
    }


  let postLoginUser (next: HttpFunc) (ctx: HttpContext) =
    task {
      try
        // TODO add validation
        let! loginRequest = ctx.BindJsonAsync<RegisterRequest>()

        return! json loginRequest next ctx
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