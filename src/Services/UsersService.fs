namespace Services

module UsersService =
  open Giraffe
  open Models
  open Repository
  open ModelsMappers.ResponseToDbMappers
  open ModelsMappers.DbToResponseMappers
  open Utils
  open User


  let postRegisterUser (request: RegisterRequest) =
    fun next ctx ->
      task {
        let model = request.toDbModel
        let! user = UsersRepository.createAndGetUser model
        return! Successful.CREATED (user.toLoggedUserResponse) next ctx
      }


  let postLoginUser (request: LoginRequest) : HttpHandler =
    fun next ctx ->
      task {
        let! user = UsersRepository.getUserByEmail request.user.email

        let isPassowrdCorrect =
          Hashing.verifyPassword request.user.password user.password

        return!
          match isPassowrdCorrect with
          | false ->
            RequestErrors.UNAUTHORIZED
              "scheme"
              "realm"
              "Don't know who you are"
              next
              ctx
          | true -> json user.toLoggedUserResponse next ctx
      }

  let getCurrentUser =
    fun next ctx ->
      task {
        let! user = getLoggedInUser ctx
        return! json user.toLoggedUserResponse next ctx
      }

  let postUpdateUser (updateRequest: UpdateUserRequest) : HttpHandler =
    fun next ctx ->
      task {
        let! user = getLoggedInUser ctx
        let updatedUser = user.updateUser updateRequest

        let! returnedUser =
          UsersRepository.updateAndReturnUser user.email updatedUser

        return! json returnedUser.toLoggedUserResponse next ctx
      }