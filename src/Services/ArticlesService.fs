namespace Services

module ArticlesService =
  open Giraffe
  open Models
  open Repository
  open ModelsMappers.ResponseToDbMappers
  open ModelsMappers.DbToResponseMappers
  open Utils
  open User



  let getListArticles: HttpHandler =
    fun next ctx ->
      task {
        //TODO can't I do tryBindQuery
        let filters =
          Models.articlesFilters
          |> Models.withTagOpt (ctx.TryGetQueryStringValue "tag")
          |> Models.withAuthorStringOpt (ctx.TryGetQueryStringValue "author")
          |> Models.withFavoritedUserNameOpt (
            ctx.TryGetQueryStringValue "favorited"
          )
          |> Models.withLimitOpt (
            ctx.TryGetQueryStringValue "limit"
            |> Option.bind Helpers.stringToInt
          )
          |> Models.withOffsetOpt (
            ctx.TryGetQueryStringValue "offset"
            |> Option.bind Helpers.stringToInt
          )


        let! data = ArticlesRepository.getArticlesWithUsersAndTags filters
        let! userWithFollows = getLoggedInUserDataMaybe ctx

        let followingList =
          userWithFollows
          |> Option.map (fun u -> u.followingList)
          |> Option.defaultValue []

        let responseList =
          data |> List.map (fun x -> x.toSingularArticleResponse followingList)

        let response: ArticlesResponse =
          { articles = responseList
            articlesCount = responseList.Length }

        return! json response next ctx
      }

  let getFeedArticles: HttpHandler =
    fun next ctx ->
      task {
        let! userData = getLoggedInUserData ctx

        let filters =
          Models.articlesFilters
          |> Models.withTagOpt (ctx.TryGetQueryStringValue "tag")
          |> Models.withAuthorStringOpt (ctx.TryGetQueryStringValue "author")
          |> Models.withLimitOpt (
            ctx.TryGetQueryStringValue "limit"
            |> Option.bind Helpers.stringToInt
          )
          |> Models.withOffsetOpt (
            ctx.TryGetQueryStringValue "offset"
            |> Option.bind Helpers.stringToInt
          )
          |> Models.withFollowedUserName (userData.user.username)

        let! data = ArticlesRepository.getArticlesWithUsersAndTags filters

        let singularArticleList =
          data
          |> List.map (fun x ->
            x.toSingularArticleResponse userData.followingList)

        let response: ArticlesResponse =
          { articles = singularArticleList
            articlesCount = singularArticleList.Length }

        return! json response next ctx
      }


  let getArticleBySlug (slug: string) : HttpHandler =
    fun next ctx ->
      task {
        let filters =
          Models.articlesFilters |> Models.withSlug slug |> Models.withLimit 1

        let! articles = ArticlesRepository.getArticlesWithUsersAndTags filters

        match articles |> List.tryHead with
        | None -> return! RequestErrors.NOT_FOUND "Article not found" next ctx
        | Some x ->
          let! userWithFollows = getLoggedInUserData ctx

          let singularArticle =
            x.toSingularArticleResponse userWithFollows.followingList

          let response: ArticleResponse = { article = singularArticle }
          return! json response next ctx
      }


  let postCreateArticle
    (createArticleRequest: CreateArticleRequest)
    : HttpHandler =
    fun next ctx ->
      task {
        let! userWithFollows = getLoggedInUserData ctx

        let articleToInsert =
          createArticleRequest.toDbModel userWithFollows.user

        let tagList =
          createArticleRequest.article.tagList
          |> Option.defaultValue []
          |> List.toArray

        let! _ = ArticlesRepository.createArticle articleToInsert tagList


        let filters =
          Models.articlesFilters
          |> Models.withSlug articleToInsert.slug
          |> Models.withLimit 1

        let! createdArticles =
          ArticlesRepository.getArticlesWithUsersAndTags filters

        match createdArticles |> List.tryHead with
        | None -> return! RequestErrors.NOT_FOUND "Article not found" next ctx
        | Some article ->
          let! usersData = getLoggedInUserData ctx

          let singularArticle =
            article.toSingularArticleResponse usersData.followingList

          let response: ArticleResponse = { article = singularArticle }
          return! json response next ctx
      }



  let putUpdateArticle
    (slug: string)
    (updateArticleRequest: UpdateArticleRequest)
    : HttpHandler =
    fun next ctx ->
      task {
        let! oldArticle = ArticlesRepository.getArticleBySlug slug
        let newArticle = oldArticle.updateArticle updateArticleRequest

        let! rowsAffected = ArticlesRepository.updateArticle newArticle

        if rowsAffected = 0 then
          return! RequestErrors.NOT_FOUND "Article not found" next ctx
        else
          let filters =
            Models.articlesFilters
            |> Models.withSlug newArticle.slug
            |> Models.withLimit 1

          let! updatedArticles =
            ArticlesRepository.getArticlesWithUsersAndTags filters

          match updatedArticles |> List.tryHead with
          | None -> return! RequestErrors.NOT_FOUND "Article not found" next ctx
          | Some article ->
            let! usersData = getLoggedInUserData ctx

            let singularArticle =
              article.toSingularArticleResponse usersData.followingList

            let response: ArticleResponse = { article = singularArticle }
            return! json response next ctx
      }

  let deleteArticle (slug: string) =
    fun next ctx ->
      task {
        let! rowAffected = ArticlesRepository.deleteArticle slug

        match rowAffected with
        | _ when rowAffected > 0 ->
          return! Successful.OK "Deleted article" next ctx
        | _ -> return! RequestErrors.NOT_FOUND "Article not updated" next ctx
      }

  let postAddArticleComment
    (slug: string)
    (request: AddArticleCommentRequest)
    : HttpHandler =
    fun next ctx ->
      task {
        let! userData = getLoggedInUserData ctx
        let body = request.comment.body

        printfn $">> slug {slug}"
        let! article = ArticlesRepository.getArticleBySlug slug

        let! comment =
          ArticlesRepository.insertAndReturnComment
            article.id
            userData.user.id
            body

        let authorResponse =
          userData.user.toAuthorResponse (userData.followingList)

        let singularComment = comment.toSingularCommentResponse authorResponse
        let response: CommentResponse = { comment = singularComment }

        return! json response next ctx
      }

  let getArticleComments (slug: string) =
    fun next ctx ->
      task {
        let! commentsWithUsers = ArticlesRepository.getCommentsWithAuthors slug
        let! userWithFollows = getLoggedInUserDataMaybe ctx

        let followingList =
          userWithFollows
          |> Option.map (fun u -> u.followingList)
          |> Option.defaultValue []

        let singularComments =
          commentsWithUsers
          |> List.map (fun commentWithUser ->
            let comment = commentWithUser.comment
            let author = commentWithUser.user.toAuthorResponse followingList
            comment.toSingularCommentResponse (author))

        let response: CommentsResponse = { comments = singularComments }
        return! json response next ctx
      }

  let deleteComment (slug: string, commentId: string) =
    fun next ctx ->
      task {

        let! comments = ArticlesRepository.getCommentsWithAuthors slug

        let commentToDelete =
          comments
          |> List.tryFind (fun x -> x.comment.id.ToString() = commentId)

        match commentToDelete with
        | None -> return! RequestErrors.NOT_FOUND "Comment not found" next ctx
        | Some commentData ->
          let! _ =
            ArticlesRepository.deleteComment
              slug
              (commentData.comment.id.ToString())

          let! userData = getLoggedInUserData ctx

          let authorResponse =
            commentData.user.toAuthorResponse userData.followingList

          let singularCommentResponse =
            commentData.comment.toSingularCommentResponse authorResponse


          let response: CommentResponse = { comment = singularCommentResponse }
          return! json response next ctx
      }


  let postAddFavoriteArticle (slug: string) =
    fun next ctx ->
      task {
        let! userData = getLoggedInUserData ctx
        let! _ = ArticlesRepository.addFavoriteArticle userData.user.id slug

        let filters =
          Models.articlesFilters |> Models.withSlug slug |> Models.withLimit 1

        let! articles = ArticlesRepository.getArticlesWithUsersAndTags filters

        match articles |> List.tryHead with
        | None -> return! RequestErrors.NOT_FOUND "Article not found" next ctx
        | Some article ->
          let singularArticle =
            article.toSingularArticleResponse userData.followingList

          let response: ArticleResponse = { article = singularArticle }
          return! json response next ctx
      }

  let deleteRemoveFavoriteArticle (slug: string) =
    fun next ctx ->
      task {
        let! userData = getLoggedInUserData ctx
        let! _ = ArticlesRepository.removeFavoriteArticle userData.user.id slug

        let filters =
          Models.articlesFilters |> Models.withSlug slug |> Models.withLimit 1


        let! articles = ArticlesRepository.getArticlesWithUsersAndTags filters

        match articles |> List.tryHead with
        | None -> return! RequestErrors.NOT_FOUND "Article not found" next ctx
        | Some article ->
          let singularArticle =
            article.toSingularArticleResponse userData.followingList

          let response: ArticleResponse = { article = singularArticle }
          return! json response next ctx
      }

  let getTags =
    fun next ctx ->
      task {
        let! allTags = ArticlesRepository.getAllTags

        let sortedTags =
          allTags |> Seq.map (fun x -> x.name) |> Seq.sort |> Seq.toList

        let response: TagsResponse = { tags = sortedTags }

        return! json response next ctx
      }

// let postAddTags (addTagsRequest: AddTagsRequest) : HttpHandler =
//   fun next ctx ->
//     task {
//       let! response = ArticlesRepository.addTags addTagsRequest.tags
//       return! json response next ctx
//     }