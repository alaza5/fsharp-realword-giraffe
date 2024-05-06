namespace ModelsMappers


open Models
open System


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


    member this.updateUser(request: UpdateUserRequest) : DatabaseModels.users =

      let email = request.email |> Option.defaultValue this.email
      let username = request.username |> Option.defaultValue this.username
      let password = request.password |> Option.defaultValue this.password
      let updated_at = DateTime.UtcNow

      let bio =
        match request.bio with
        | Some x -> Some x
        | None -> this.bio

      let image =
        match request.image with
        | Some x -> Some x
        | None -> this.image

      let response: DatabaseModels.users =
        { this with
            email = email
            username = username
            password = password
            updated_at = updated_at
            image = image
            bio = bio }

      response