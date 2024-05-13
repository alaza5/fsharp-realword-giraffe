namespace ModelsMappers




module DbToResponseMappers =
  open Utils
  open Models
  open System
  open Models.DbTables

  type DbTables.UsersTable with

    member this.toLoggedUserResponse: UserResponse =
      let response: UserResponse =
        { user =
            {| email = this.email
               token = JwtHelper.generateToken this.email
               username = this.username
               bio = this.bio
               image = this.image |}

        }

      response


    member this.toAuthorResponse
      (loggedInUserFollowingList: DbTables.UsersTable list)
      : AuthorResponse =
      let following =
        loggedInUserFollowingList |> List.exists (fun user -> user.id = this.id)

      let response: AuthorResponse =
        { username = this.username
          bio = this.bio |> Option.defaultValue ""
          image = this.image |> Option.defaultValue ""
          following = following }

      response


    member this.updateUser(request: UpdateUserRequest) : DbTables.UsersTable =
      let user = request.user

      let email = user.email |> Option.defaultValue this.email
      let username = user.username |> Option.defaultValue this.username
      let password = user.password |> Option.defaultValue this.password
      let updated_at = DateTime.UtcNow

      let bio =
        match user.bio with
        | Some x -> Some x
        | None -> this.bio

      let image =
        match user.image with
        | Some x -> Some x
        | None -> this.image

      let response: DbTables.UsersTable =
        { this with
            email = email
            username = username
            password = password
            updated_at = updated_at
            image = image
            bio = bio }

      response


  type DbTables.ArticlesTable with

    member this.updateArticle
      (updateRequest: UpdateArticleRequest)
      : DbTables.ArticlesTable =
      let article = updateRequest.article

      let title = article.title |> Option.defaultValue this.title

      let description =
        article.description |> Option.defaultValue this.description

      let body = article.body |> Option.defaultValue this.body


      let slug =
        match article.title with
        | Some newTitle -> Helpers.generateSlug newTitle
        | None -> this.slug

      let response: DbTables.ArticlesTable =
        { id = this.id
          author_id = this.author_id
          slug = slug
          title = title
          description = description
          body = body
          created_at = this.created_at
          updated_at = DateTime.Now }

      response

  type DbTables.CommentsTable with


    member this.toSingularCommentResponse
      (author: AuthorResponse)
      : SingularCommentResponse =
      let response: SingularCommentResponse =
        { id = this.id
          createdAt = this.created_at.ToString(Helpers.DATE_TIME_FORMAT)
          updatedAt = this.updated_at.ToString(Helpers.DATE_TIME_FORMAT)
          body = this.body
          author = author }

      response

  type DbTables.UsersTable with

    member this.toProfileResponse
      (loggedInUserFollowingList: DbTables.UsersTable list)
      : ProfileResponse =
      let isFollowing =
        loggedInUserFollowingList |> List.exists (fun user -> user.id = this.id)

      let response: ProfileResponse =
        { profile =
            {| username = this.username
               bio = (this.bio |> Option.defaultValue "")
               image = (this.image |> Option.defaultValue "")
               following = isFollowing |} }

      response

  type DbModels.ArticleUserTags with

    member this.toSingularArticleResponse
      (loggedInUserFollowingList: DbTables.UsersTable list)
      : SingularArticleResponse =

      let favorited =
        this.favoritedUsers
        |> Array.exists (fun username -> this.user.username = username)

      let tags = this.tags |> Array.sort |> Array.toList

      let response: SingularArticleResponse =
        { slug = this.article.slug
          title = this.article.title
          description = this.article.description
          body = this.article.body
          tagList = tags
          createdAt = this.article.created_at.ToString(Helpers.DATE_TIME_FORMAT)
          updatedAt = this.article.updated_at.ToString(Helpers.DATE_TIME_FORMAT)
          favorited = favorited
          favoritesCount = this.favoritedUsers.Length
          author = this.user.toAuthorResponse loggedInUserFollowingList }

      response