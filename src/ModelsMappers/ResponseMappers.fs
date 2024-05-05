namespace ModelsMappers


open Models

module ResponseMappers =
  open InternalSecurity

  type DatabaseModels.users with

    member this.response: UserResponse =
      let response: UserResponse =
        { email = this.email
          token = JwtHelper.generateToken this.email
          username = this.username
          bio = this.bio
          image = this.image }

      response