namespace ProfilesService

module ProfilesService =
  open Giraffe
  open Microsoft.AspNetCore.Http
  open UsersService.UsersService
  open ModelsMappers.ResponseToDbMappers
  open ModelsMappers.DbToResponseMappers
  open Repository

  let getProfile (username: string) (next: HttpFunc) (ctx: HttpContext) =
    task {
      // can do 1 sql request
      let! currentUser = getLoggedInUser ctx
      let! userProfile = Repository.getUserByUsername username
      let! isFollowing = Repository.isFollowing currentUser.id userProfile.id
      let response = userProfile.toProfileResponse (isFollowing.count > 0)

      return! json response next ctx
    }

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