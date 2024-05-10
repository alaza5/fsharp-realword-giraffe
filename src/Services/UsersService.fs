namespace UsersService

module UsersService =
  open Giraffe
  open Microsoft.AspNetCore.Http
  open Models
  open Repository
  open ModelsMappers.ResponseToDbMappers
  open ModelsMappers.DbToResponseMappers
  open InternalSecurity
  open System.Security.Claims

  let getCurrentlyLoggedInUser (ctx: HttpContext) =
    task {
      let userId = ctx.User.FindFirst(ClaimTypes.NameIdentifier)
      let currentUserEmail = userId.Value
      return! Repository.fetchCurrentUser currentUserEmail
    }

  let postRegisterUser (next: HttpFunc) (ctx: HttpContext) =
    task {
      // TODO validation and better mapping?
      let! request = ctx.BindJsonAsync<RegisterRequest>()
      let model = request.toDbModel
      let! user = Repository.registerUser model
      return! json user.toUserResponse next ctx
    }

  let postLoginUser (next: HttpFunc) (ctx: HttpContext) =
    task {
      let! request = ctx.BindJsonAsync<LoginRequest>()
      let! user = Repository.fetchCurrentUser request.email

      let passwordCorrect = Hashing.verifyPassword request.password user.password

      let response =
        match passwordCorrect with
        | false -> RequestErrors.UNAUTHORIZED "scheme" "realm" "Don't know who you are"
        | true -> json user.toUserResponse

      return! response next ctx
    }


  let getCurrentUser (next: HttpFunc) (ctx: HttpContext) =
    task {
      let! user = getCurrentlyLoggedInUser ctx
      return! json user.toUserResponse next ctx
    }

  let postUpdateUser (next: HttpFunc) (ctx: HttpContext) =
    task {
      let! user = getCurrentlyLoggedInUser ctx
      let! updateRequest = ctx.BindJsonAsync<UpdateUserRequest>()

      let updatedUser = user.updateUser (updateRequest)
      let! returnedUser = Repository.updateUser user.email updatedUser
      return! json returnedUser.toUserResponse next ctx
    }