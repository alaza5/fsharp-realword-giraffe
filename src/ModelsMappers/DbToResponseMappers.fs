namespace ModelsMappers




module DbToResponseMappers =
  open InternalSecurity
  open Helpers
  open Models
  open System

  type DatabaseModels.users with

    member this.toUserResponse: UserResponse =
      let response: UserResponse =
        { email = this.email
          token = JwtHelper.generateToken this.email
          username = this.username
          bio = this.bio
          image = this.image }

      response

    member this.toAuthorResponse: AuthorResponse =
      let response: AuthorResponse =
        { username = this.username
          bio = this.bio |> Option.defaultValue ""
          image = this.image |> Option.defaultValue ""
          //TODO
          following = false }

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


  type DatabaseModels.articles with


    member this.toArticleResponse
      (user: DatabaseModels.users)
      (tags: string list)
      : ArticleResponse =
      let response: ArticleResponse =
        { slug = this.slug
          title = this.title
          description = this.description
          body = this.body
          tagList = tags
          createdAt = this.created_at
          updatedAt = this.updated_at
          //TODO
          favorited = false
          //TODO
          favoritesCount = 0
          author = user.toAuthorResponse }

      response

    member this.updateArticle(updateRequest: UpdateArticleRequest) : DatabaseModels.articles =
      let title = updateRequest.title |> Option.defaultValue this.title
      let description = updateRequest.description |> Option.defaultValue this.description
      let body = updateRequest.body |> Option.defaultValue this.body


      let slug =
        match updateRequest.title with
        | Some newTitle -> Helpers.generateSlug newTitle
        // | Some _ -> this.slug
        | None -> this.slug

      let response: DatabaseModels.articles =
        { id = this.id
          author_id = this.author_id
          slug = slug
          title = title
          description = description
          body = body
          created_at = this.created_at
          updated_at = DateTime.Now }

      response