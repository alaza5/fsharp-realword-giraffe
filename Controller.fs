namespace Controller

open Giraffe
open Microsoft.AspNetCore.Http


module Controller =

  let private gets: HttpFunc -> HttpContext -> HttpFuncResult =
    let hello = route "/hello" >=> text "hello"
    GET >=> choose [ hello ]

  let private defaultHandler = setStatusCode 404 >=> text "Not Found"

  let root: HttpFunc -> HttpContext -> HttpFuncResult =
    choose [ gets; defaultHandler ]