namespace ArticlesService

module ArticlesService =
  open Giraffe
  open Giraffe.Core
  open Microsoft.AspNetCore.Http
  open Models

  let getListArticles (next: HttpFunc) (ctx: HttpContext) = text "ok" next ctx

  let getFeedArticles (next: HttpFunc) (ctx: HttpContext) = text "ok" next ctx

  let getArticle (slug: string) (next: HttpFunc) (ctx: HttpContext) =
    text "ok" next ctx

  let postCreateArticle (next: HttpFunc) (ctx: HttpContext) = text "ok" next ctx

  let putUpdateArticle (slug: string) (next: HttpFunc) (ctx: HttpContext) =
    printfn $">> slug {slug}"

    task {
      try
        let! updateArticle = ctx.BindJsonAsync<UpdateArticleRequest>()
        return! json updateArticle next ctx
      with ex ->
        return! RequestErrors.BAD_REQUEST $"Exception: {ex.Message}" next ctx
    }

  let deleteArticle (slug: string) (next: HttpFunc) (ctx: HttpContext) =
    text "ok" next ctx

  let postArticleComment (slug: string) (next: HttpFunc) (ctx: HttpContext) =
    text "ok" next ctx

  let getArticleComments (slug: string) (next: HttpFunc) (ctx: HttpContext) =
    text "ok" next ctx


  let deleteComment
    (article: string, commentId: string)
    (next: HttpFunc)
    (ctx: HttpContext)
    =
    text "ok" next ctx


  let postAddFavoriteArticle
    (slug: string)
    (next: HttpFunc)
    (ctx: HttpContext)
    =
    text "ok" next ctx

  let deleteRemoveFavoriteArticle
    (slug: string)
    (next: HttpFunc)
    (ctx: HttpContext)
    =
    text "ok" next ctx


  let getTags (next: HttpFunc) (ctx: HttpContext) = text "ok" next ctx