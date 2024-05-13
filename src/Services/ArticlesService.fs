namespace ArticlesService

module ArticlesService =
  open System
  open Giraffe
  open Microsoft.AspNetCore.Http
  open Models
  open Repository
  open System.Data
  open UsersService.UsersService
  open ModelsMappers.ResponseToDbMappers
  open ModelsMappers.DbToResponseMappers
  open System.Threading.Tasks
  open Helpers



  let getListArticles (next: HttpFunc) (ctx: HttpContext) =
    task {
      let filters =
        Models.articlesFilters
        |> Models.withTag (ctx.TryGetQueryStringValue "tag")
        |> Models.withAuthor (ctx.TryGetQueryStringValue "author")
        |> Models.withLimit (ctx.TryGetQueryStringValue "limit" |> Option.bind Helpers.stringToInt)
        |> Models.withOffset (
          ctx.TryGetQueryStringValue "offset" |> Option.bind Helpers.stringToInt
        )

      let! data = Repository.getArticlesWithUsersAndTags filters

      let responseList =
        data
        |> List.map (fun x ->
          let tagList = x.tags |> Array.toList
          x.article.toArticleResponse x.user tagList)

      let response: ArticlesResponse =
        { articles = responseList
          articlesCount = responseList.Length }

      return! json response next ctx
    }

  let getFeedArticles (next: HttpFunc) (ctx: HttpContext) =
    task {
      let! currentUser = getLoggedInUser ctx

      let filters =
        Models.articlesFilters
        |> Models.withTag (ctx.TryGetQueryStringValue "tag")
        |> Models.withAuthor (ctx.TryGetQueryStringValue "author")
        |> Models.withLimit (ctx.TryGetQueryStringValue "limit" |> Option.bind Helpers.stringToInt)
        |> Models.withOffset (
          ctx.TryGetQueryStringValue "offset" |> Option.bind Helpers.stringToInt
        )
        |> Models.withUserId (Some currentUser.id)

      let! data = Repository.getArticlesWithUsersAndTags filters

      let responseList =
        data
        |> List.map (fun x ->
          let tagList = x.tags |> Array.toList
          x.article.toArticleResponse x.user tagList)

      let response: ArticlesResponse =
        { articles = responseList
          articlesCount = responseList.Length }

      return! json response next ctx

    }


  let getArticle (slug: string) (next: HttpFunc) (ctx: HttpContext) =
    task {
      let filters =
        Models.articlesFilters
        |> Models.withSlug (Some slug)
        |> Models.withLimit (Some 1)

      let! articles = Repository.getArticlesWithUsersAndTags filters
      printfn $">> articles {articles}"

      return!
        match articles |> List.tryHead with
        | Some x -> json x next ctx
        | None -> RequestErrors.NOT_FOUND "Article not found" next ctx
    }


  let postCreateArticle (next: HttpFunc) (ctx: HttpContext) =
    task {
      let! createArticleRequest = ctx.BindJsonAsync<CreateArticleRequest>()
      let! user = getLoggedInUser ctx
      let articleToInsert = createArticleRequest.toDbModel user

      let tagList = createArticleRequest.tagList |> Option.defaultValue [] |> List.toArray

      let! response = Repository.createArticleWithTags articleToInsert tagList
      return! json response next ctx
    }



  let putUpdateArticle (slug: string) (next: HttpFunc) (ctx: HttpContext) =
    task {
      let! updateArticleRequest = ctx.BindJsonAsync<UpdateArticleRequest>()
      let! article = Repository.getArticleBySlug slug
      let updatedArticle = article.updateArticle updateArticleRequest

      let! _ = Repository.updateArticle updatedArticle

      let filters =
        Models.articlesFilters
        |> Models.withSlug (Some updatedArticle.slug)
        |> Models.withLimit (Some 1)

      let! articles = Repository.getArticlesWithUsersAndTags filters

      return!
        match articles |> List.tryHead with
        | Some x -> json x next ctx
        | None -> RequestErrors.NOT_FOUND "Article not found" next ctx
    }

  let deleteArticle (slug: string) (next: HttpFunc) (ctx: HttpContext) =
    task {
      let! articles = Repository.deleteArticle slug
      return! json articles next ctx
    }

  let postArticleComment (slug: string) (next: HttpFunc) (ctx: HttpContext) =
    task {
      let! request = ctx.BindJsonAsync<AddArticleCommentRequest>()
      let! user = getLoggedInUser ctx
      let! comments = Repository.insertComment slug user.id request.body
      return! json comments next ctx
    }

  let getArticleComments (slug: string) (next: HttpFunc) (ctx: HttpContext) =
    task {
      let! commentsWithUsers = Repository.getComments slug

      let response =
        commentsWithUsers
        |> List.map (fun commentWithUser ->
          let comment = commentWithUser.comment
          let author = commentWithUser.user.toAuthorResponse
          comment.toCommentResponse (author))

      return! json response next ctx
    }

  let deleteComment (slug: string, commentId: string) (next: HttpFunc) (ctx: HttpContext) =
    task {
      let! response = Repository.deleteComment slug commentId
      return! json response next ctx
    }


  let postAddFavoriteArticle (slug: string) (next: HttpFunc) (ctx: HttpContext) =
    task {
      // probably can just pass email and not user
      let! user = getLoggedInUser ctx
      let! response = Repository.addFavoriteArticle user.id slug
      return! json response next ctx
    }

  let deleteRemoveFavoriteArticle (slug: string) (next: HttpFunc) (ctx: HttpContext) =
    task {
      let! user = getLoggedInUser ctx
      let! response = Repository.removeFavoriteArticle user.id slug
      return! json response next ctx
    }

  let getTags (next: HttpFunc) (ctx: HttpContext) =
    task {
      let! tags = Repository.getAllTags
      let response = tags |> Seq.map (fun x -> x.name)
      return! json response next ctx
    }

  let postAddTags (next: HttpFunc) (ctx: HttpContext) =
    task {
      let! addTagsRequest = ctx.BindJsonAsync<AddTagsRequest>()
      let! response = Repository.addTags addTagsRequest.tags
      return! json response next ctx
    }