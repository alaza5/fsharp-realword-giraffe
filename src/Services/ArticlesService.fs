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

  let getListArticles (next: HttpFunc) (ctx: HttpContext) = text "ok" next ctx

  let getFeedArticles (next: HttpFunc) (ctx: HttpContext) = text "ok" next ctx

  let getArticle (slug: string) (next: HttpFunc) (ctx: HttpContext) = text "ok" next ctx
  // task {
  //   let! insertedArticle = Repository.getArticleBySlug slug
  //   return! json insertedArticle next ctx
  // }


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
      let! tags = Repository.getTags

      let response = tags |> Seq.map (fun x -> x.name)
      return! json response next ctx
    }

  let postAddTags (next: HttpFunc) (ctx: HttpContext) =
    task {
      let! addTagsRequest = ctx.BindJsonAsync<AddTagsRequest>()
      // let listOfTags: DatabaseModels.tags list =
      //   addTagsRequest.tags |> List.map (fun tag -> { id = Guid.NewGuid(); name = tag })
      let! response = Repository.addTags (addTagsRequest.tags |> List.toArray)

      return! json response next ctx
    }

  let getTag (next: HttpFunc) (ctx: HttpContext) =
    task {
      let! req = ctx.BindJsonAsync<{| name: string |}>()
      let! response = Repository.findTag req.name
      return! json response next ctx

    }

  let getTagForArticle (next: HttpFunc) (ctx: HttpContext) =
    task {
      let! req = ctx.BindJsonAsync<{| articleId: Guid |}>()
      printfn $">> req {req}"
      let! response = Repository.getTagForArticle req.articleId
      return! json response next ctx

    }

  let insertTag (next: HttpFunc) (ctx: HttpContext) =
    task {
      let! req = ctx.BindJsonAsync<{| name: string |}>()

      let data: DatabaseModels.tags = { id = Guid.NewGuid(); name = req.name }
      let! response = Repository.insertTag data
      return! json response next ctx

    }