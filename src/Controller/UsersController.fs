namespace Controller


module UsersController =
  open Giraffe
  open Services
  open Models
  open Utils

  let postRegisterUser: HttpHandler =
    route "/api/users"
    >=> bindJson<RegisterRequest> (validateModel UsersService.postRegisterUser)

  let postLoginUser: HttpHandler =
    route "/api/users/login"
    >=> bindJson<LoginRequest> UsersService.postLoginUser

  let getUser: HttpHandler =
    route "/api/user" >=> JwtHelper.auth >=> UsersService.getCurrentUser

  let putUpdateUser: HttpHandler =
    route "/api/user"
    >=> JwtHelper.auth
    >=> bindJson<UpdateUserRequest> (validateModel UsersService.postUpdateUser)