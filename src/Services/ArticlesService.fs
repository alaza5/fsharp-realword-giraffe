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

      let queryParams: GetArticlesQueryParams =
        { tag = ctx.TryGetQueryStringValue "tag"
          author = ctx.TryGetQueryStringValue "author"
          limit = ctx.TryGetQueryStringValue "limit" |> Option.bind Helpers.stringToInt
          offset = ctx.TryGetQueryStringValue "offset" |> Option.bind Helpers.stringToInt }

      printfn $">> queryParams {queryParams}"

      let! data = Repository.getArticlesWithUsersAndTags queryParams

      let responseList =
        data
        |> List.map (fun x ->
          let tagList = x.tags |> Array.toList
          x.article.toArticleResponse x.user tagList)

      return! json responseList next ctx
    }

  let getFeedArticles (next: HttpFunc) (ctx: HttpContext) = text "ok" next ctx

  // let getArticle (slug: string) (next: HttpFunc) (ctx: HttpContext) = text "ok" next ctx
  let getArticle (slug: string) (next: HttpFunc) (ctx: HttpContext) =
    task {
      let! article = Repository.getArticleBySlug slug
      let! author = Repository.getUserById article.author_id
      let! tags = Repository.getTagsForArticle article.id
      let tagNames = tags |> List.map (fun x -> x.name)
      let response = article.toArticleResponse author tagNames
      return! json response next ctx
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
    printfn $">> slug {slug}"

    task {
      let! updateArticle = ctx.BindJsonAsync<UpdateArticleRequest>()
      return! json updateArticle next ctx
    }

  let deleteArticle (slug: string) (next: HttpFunc) (ctx: HttpContext) = text "ok" next ctx

  let postArticleComment (slug: string) (next: HttpFunc) (ctx: HttpContext) = text "ok" next ctx

  let getArticleComments (slug: string) (next: HttpFunc) (ctx: HttpContext) = text "ok" next ctx


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