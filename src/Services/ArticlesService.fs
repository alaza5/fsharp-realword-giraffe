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

      return! json responseList next ctx
    }

  //TODO need follows
  let getFeedArticles (next: HttpFunc) (ctx: HttpContext) = text "ok" next ctx

  // let getArticle (slug: string) (next: HttpFunc) (ctx: HttpContext) =
  //   task {
  //     let! article = Repository.getArticleBySlug slug
  //     let! author = Repository.getUserById article.author_id
  //     let! tags = Repository.getTagsForArticle article.id
  //     let tagNames = tags |> List.map (fun x -> x.name)
  //     let response = article.toArticleResponse author tagNames
  //     return! json response next ctx
  //   }

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
      let! user = getCurrentlyLoggedInUser ctx
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
      let! user = getCurrentlyLoggedInUser ctx
      let! response = Repository.insertComment slug user.id request.body
      return! json response next ctx
    }

  let getArticleComments (slug: string) (next: HttpFunc) (ctx: HttpContext) =
    task {
      let! response = Repository.getComments slug
      return! json response next ctx
    }

  let deleteComment (article: string, commentId: string) (next: HttpFunc) (ctx: HttpContext) =
    text "ok" next ctx


  let postAddFavoriteArticle (slug: string) (next: HttpFunc) (ctx: HttpContext) = text "ok" next ctx

  let deleteRemoveFavoriteArticle (slug: string) (next: HttpFunc) (ctx: HttpContext) =
    text "ok" next ctx


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