namespace Services

module User =
  open Microsoft.AspNetCore.Http
  open Models
  open Repository
  open System.Security.Claims
  open System.Threading.Tasks


  let getLoggedInUser (ctx: HttpContext) =
    task {
      let claim = ctx.User.FindFirst(ClaimTypes.NameIdentifier)
      let currentUserEmail = claim.Value
      return! UsersRepository.getUserByEmail currentUserEmail
    }

  let getLoggedInUserData (ctx: HttpContext) =
    task {
      let! user = getLoggedInUser ctx
      let! following = UsersRepository.getFollowingUsers user.id

      let retVal: DbModels.UserData =
        { user = user
          followingList = following }

      return retVal
    }

  let getLoggedInUserDataMaybe
    (ctx: HttpContext)
    : Task<DbModels.UserData option> =
    task {
      try
        let! userWithData = getLoggedInUserData ctx
        return Some userWithData
      with ex ->
        return None
    }