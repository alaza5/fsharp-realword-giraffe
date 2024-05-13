namespace Controller


module ArticlesController =
  open Giraffe
  open Services
  open Models
  open Utils

  let getListArticles: HttpHandler =
    route "/api/articles" >=> ArticlesService.getListArticles

  let getFeedArticles: HttpHandler =
    route "/api/articles/feed" >=> ArticlesService.getFeedArticles

  let getArticle: HttpHandler =
    routef "/api/articles/%s" ArticlesService.getArticleBySlug

  let postCreateArticle: HttpHandler =
    route "/api/articles"
    >=> JwtHelper.auth
    >=> bindJson<CreateArticleRequest> (
      validateModel ArticlesService.postCreateArticle
    )

  let putUpdateArticle: HttpHandler =
    routef "/api/articles/%s" (fun slug ->
      let serviceFunc = ArticlesService.putUpdateArticle slug

      JwtHelper.auth
      >=> bindJson<UpdateArticleRequest> (validateModel serviceFunc))

  let deleteArticle: HttpHandler =
    routef "/api/articles/%s" ArticlesService.deleteArticle

  let postArticleComment: HttpHandler =
    routef "/api/articles/%s/comments" (fun slug ->
      let serviceFunc = ArticlesService.postAddArticleComment slug

      JwtHelper.auth
      >=> bindJson<AddArticleCommentRequest> (validateModel serviceFunc))

  let getArticleComments: HttpHandler =
    routef "/api/articles/%s/comments" ArticlesService.getArticleComments


  let deleteComment: HttpHandler =
    routef "/api/articles/%s/comments/%s" (fun slugs ->
      JwtHelper.auth >=> (ArticlesService.deleteComment slugs))


  let postAddFavoriteArticle: HttpHandler =
    routef "/api/articles/%s/favorite" (fun slug ->
      JwtHelper.auth >=> ArticlesService.postAddFavoriteArticle slug)

  let deleteRemoveFavoriteArticle: HttpHandler =
    routef "/api/articles/%s/favorite" (fun slug ->
      JwtHelper.auth >=> ArticlesService.deleteRemoveFavoriteArticle slug)

  // let postTags: HttpHandler =
  //   route "/api/tags"
  //   >=> JwtHelper.auth
  //   >=> bindJson<AddTagsRequest> (validateModel ArticlesService.postAddTags)

  let getTags: HttpHandler = route "/api/tags" >=> ArticlesService.getTags