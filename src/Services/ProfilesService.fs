namespace ProfilesService

module ProfilesService =
  open Giraffe
  open Microsoft.AspNetCore.Http
  open Models

  let getProfile (user: string) (next: HttpFunc) (ctx: HttpContext) =
    text user next ctx

  let postFollowUser (user: string) (next: HttpFunc) (ctx: HttpContext) =
    text user next ctx

  let deleteFollowUser (user: string) (next: HttpFunc) (ctx: HttpContext) =
    text user next ctx