namespace UsersService

module UsersService =
  open Giraffe
  open Microsoft.AspNetCore.Http
  open Models

  let postLoginUser (next: HttpFunc) (ctx: HttpContext) =
    task {
      try
        let! loginRequest = ctx.BindJsonAsync<LoginRequest>()
        return! json loginRequest next ctx
      with ex ->
        return! RequestErrors.BAD_REQUEST $"Exception: {ex.Message}" next ctx
    }

  let postRegisterUser (next: HttpFunc) (ctx: HttpContext) =
    task {
      try
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