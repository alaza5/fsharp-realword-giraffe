namespace ArticlesService

module ArticlesService =
  open Giraffe
  open Microsoft.AspNetCore.Http
  open Models
  open Repository
  open System.Data
  open UsersService.UsersService
  open ModelsMappers.ResponseToDbMappers
  open ModelsMappers.DbToResponseMappers

  let getListArticles (next: HttpFunc) (ctx: HttpContext) = text "ok" next ctx

  let getFeedArticles (next: HttpFunc) (ctx: HttpContext) = text "ok" next ctx

  let getArticle (slug: string) (next: HttpFunc) (ctx: HttpContext) = text "ok" next ctx

  let postCreateArticle (next: HttpFunc) (ctx: HttpContext) =
    task {
      try
        let! createArticleRequest = ctx.BindJsonAsync<CreateArticleRequest>()
        use conn = ctx.GetService<IDbConnection>()
        let! user = getCurrentlyLoggedInUser ctx

        match user with
        | Error message -> return! RequestErrors.NOT_FOUND message next ctx
        | Ok user ->
          let articleToInsert = createArticleRequest.toDbModel user
          let! insertedArticle = Repository.createArticle conn articleToInsert

          match insertedArticle with
          | Error message -> return! ServerErrors.INTERNAL_ERROR message next ctx
          | Ok article -> return! json article next ctx

      with ex ->
        return! RequestErrors.BAD_REQUEST $"Exception: {ex.Message}" next ctx
    }

  let putUpdateArticle (slug: string) (next: HttpFunc) (ctx: HttpContext) =
    printfn $">> slug {slug}"

    task {
      try
        let! updateArticle = ctx.BindJsonAsync<UpdateArticleRequest>()
        return! json updateArticle next ctx
      with ex ->
        return! RequestErrors.BAD_REQUEST $"Exception: {ex.Message}" next ctx
    }

  let deleteArticle (slug: string) (next: HttpFunc) (ctx: HttpContext) = text "ok" next ctx

  let postArticleComment (slug: string) (next: HttpFunc) (ctx: HttpContext) = text "ok" next ctx

  let getArticleComments (slug: string) (next: HttpFunc) (ctx: HttpContext) = text "ok" next ctx


  let deleteComment (article: string, commentId: string) (next: HttpFunc) (ctx: HttpContext) =
    text "ok" next ctx


  let postAddFavoriteArticle (slug: string) (next: HttpFunc) (ctx: HttpContext) = text "ok" next ctx

  let deleteRemoveFavoriteArticle (slug: string) (next: HttpFunc) (ctx: HttpContext) =
    text "ok" next ctx


  let getTags (next: HttpFunc) (ctx: HttpContext) = text "ok" next ctx