namespace Controller



module Controller =
  open Giraffe
  open Microsoft.AspNetCore.Http

  let private defaultHandler = setStatusCode 404 >=> text "Endpoint Not Found"

  let root: HttpFunc -> HttpContext -> HttpFuncResult =
    let gets =
      GET
      >=> choose
        [ UsersController.getUser
          ProfilesController.getProfile
          ArticlesController.getListArticles
          ArticlesController.getFeedArticles
          ArticlesController.getArticle
          ArticlesController.getArticleComments
          ArticlesController.getTags ]

    let posts =
      POST
      >=> choose
        [ UsersController.postLoginUser
          UsersController.postRegisterUser
          ProfilesController.postFollowUser
          ArticlesController.postCreateArticle
          ArticlesController.postArticleComment
          ArticlesController.postAddFavoriteArticle
          // ArticlesController.postTags
          ]

    let puts =
      PUT
      >=> choose
        [ ArticlesController.putUpdateArticle; UsersController.putUpdateUser ]

    let deletes =
      DELETE
      >=> choose
        [ ProfilesController.deleteFollowUser
          ArticlesController.deleteArticle
          ArticlesController.deleteComment
          ArticlesController.deleteRemoveFavoriteArticle ]

    choose [ gets; posts; puts; deletes; defaultHandler ]