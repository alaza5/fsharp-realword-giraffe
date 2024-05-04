namespace Controller

open Giraffe
open Microsoft.AspNetCore.Http
open UsersService


module Controller =

  let private gets: HttpFunc -> HttpContext -> HttpFuncResult =
    let hello = route "/hello" >=> text "hello"
    GET >=> choose [ hello ]


  let private posts: HttpFunc -> HttpContext -> HttpFuncResult =
    let loginUser = route "/api/users/login" >=> UsersService.loginUser
    POST >=> choose [ loginUser ]


  let private defaultHandler = setStatusCode 404 >=> text "Not Found"

  let root: HttpFunc -> HttpContext -> HttpFuncResult =
    choose [ gets; posts; defaultHandler ]