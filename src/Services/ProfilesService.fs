namespace ProfilesService

module ProfilesService =
  open Giraffe
  open Microsoft.AspNetCore.Http
  open Models
  open UsersService.UsersService
  open Repository

  let getProfile (user: string) (next: HttpFunc) (ctx: HttpContext) = text user next ctx

  let postFollowUser (username: string) (next: HttpFunc) (ctx: HttpContext) =
    task {
      let! currentUser = getLoggedInUser ctx
      let! toFollow = Repository.getUserByUsername username

      let! response = Repository.addFollow currentUser.id toFollow.id
      return! json response next ctx
    }

  let deleteFollowUser (username: string) (next: HttpFunc) (ctx: HttpContext) =
    task {
      let! currentUser = getLoggedInUser ctx
      let! toFollow = Repository.getUserByUsername username

      let! response = Repository.removeFollow currentUser.id toFollow.id
      return! json response next ctx
    }