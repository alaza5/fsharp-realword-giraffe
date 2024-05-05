namespace ModelsMappers


open Models

module ResponseMappers =

  type DatabaseModels.users with

    member this.toResponse() : UserResponse =
      let response: UserResponse =
        { email = this.email
          token = "FIXME"
          username = this.username
          bio = this.bio
          image = this.image }

      response