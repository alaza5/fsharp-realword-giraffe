namespace Services

module ProfilesService =
  open Giraffe
  open ModelsMappers.DbToResponseMappers
  open Repository
  open User

  let getProfile (username: string) =
    fun next ctx ->
      task {
        let! currentUserData = getLoggedInUserDataMaybe ctx

        let currentUserFollowings =
          currentUserData
          |> Option.map (fun user -> user.followingList)
          |> Option.defaultValue []

        let! userProfile = UsersRepository.getUserByUsername username
        let response = userProfile.toProfileResponse currentUserFollowings

        return! json response next ctx
      }

  let postFollowUser (username: string) =
    fun next ctx ->
      task {
        let! currentUser = getLoggedInUser ctx
        let! userToFollow = UsersRepository.getUserByUsername username
        let! _ = UsersRepository.addFollow currentUser.id userToFollow.id

        let! followedUserProfile = UsersRepository.getUserByUsername username

        let! currentUserFollowings =
          UsersRepository.getFollowingUsers currentUser.id

        let response =
          followedUserProfile.toProfileResponse currentUserFollowings

        return! json response next ctx
      }

  let deleteFollowUser (username: string) =
    fun next ctx ->
      task {
        let! currentUser = getLoggedInUser ctx
        let! toDeleteFollow = UsersRepository.getUserByUsername username
        let! _ = UsersRepository.removeFollow currentUser.id toDeleteFollow.id

        let! userProfile = UsersRepository.getUserByUsername username

        let! currentUserFollowings =
          UsersRepository.getFollowingUsers currentUser.id

        let response = userProfile.toProfileResponse currentUserFollowings

        return! json response next ctx
      }