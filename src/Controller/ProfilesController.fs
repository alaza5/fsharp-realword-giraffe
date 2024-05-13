namespace Controller


module ProfilesController =
  open Giraffe
  open Services
  open Utils

  let getProfile: HttpHandler =
    routef "/api/profiles/%s" ProfilesService.getProfile

  let postFollowUser: HttpHandler =
    routef "/api/profiles/%s/follow" (fun slug ->
      JwtHelper.auth >=> ProfilesService.postFollowUser slug)

  let deleteFollowUser: HttpHandler =
    routef "/api/profiles/%s/follow" (fun slug ->
      JwtHelper.auth >=> ProfilesService.deleteFollowUser slug)