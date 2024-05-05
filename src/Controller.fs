namespace Controller

open Giraffe
open Microsoft.AspNetCore.Http
open UsersService
open ProfilesService
open ArticlesService


module Controller =
  open Microsoft.AspNetCore.Authentication.JwtBearer


  let giraffeAuthorizeEndpoint: HttpFunc -> HttpContext -> HttpFuncResult =
    let chall = challenge JwtBearerDefaults.AuthenticationScheme
    requiresAuthentication chall

  let private defaultHandler = setStatusCode 404 >=> text "Not Found"

  let root: HttpFunc -> HttpContext -> HttpFuncResult =
    let getUser = route "/api/user" >=> UsersService.getUser
    let postLoginUser = route "/api/users/login" >=> UsersService.postLoginUser
    let postRegisterUser = route "/api/users" >=> UsersService.postRegisterUser
    let postUpdateUser = route "/api/user" >=> UsersService.postUpdateUser

    let getProfile = routef "/api/user/%s" ProfilesService.getProfile
    let postFollowUser = routef "/api/profiles/%s/follow" ProfilesService.postFollowUser
    let deleteFollowUser = routef "/api/profiles/%s/follow" ProfilesService.deleteFollowUser
    let getListArticles = route "/api/articles" >=> ArticlesService.getListArticles
    let getFeedArticles = route "/api/articles/feed" >=> ArticlesService.getFeedArticles
    let getArticle = routef "/api/articles/%s" ArticlesService.getArticle
    let postCreateArticle = route "/api/articles" >=> ArticlesService.postCreateArticle
    let putUpdateArticle = routef "/api/articles/%s" ArticlesService.putUpdateArticle
    let deleteArticle = routef "/api/articles/%s" ArticlesService.deleteArticle
    let postArticleComment = routef "/api/articles/%s/comments" ArticlesService.postArticleComment
    let getArticleComments = routef "/api/articles/%s/comments" ArticlesService.getArticleComments
    let deleteComment = routef "/api/articles/%s/comments/%s" ArticlesService.deleteComment
    let postAddFavoriteArticle = routef "/api/articles/%s/favorite" ArticlesService.postAddFavoriteArticle
    let deleteRemoveFavoriteArticle = routef "/api/articles/%s/favorite" ArticlesService.deleteRemoveFavoriteArticle
    let getTags = route "/api/tags" >=> ArticlesService.getTags


    let gets =
      GET
      >=> choose
        [ getUser
          getProfile
          getListArticles
          getFeedArticles
          getArticle
          getArticleComments
          getTags ]

    let posts =
      POST
      >=> choose
        [ postLoginUser
          postRegisterUser
          postUpdateUser
          postFollowUser
          postCreateArticle
          postArticleComment
          postAddFavoriteArticle ]

    let puts = PUT >=> choose [ putUpdateArticle ]

    let deletes =
      DELETE
      >=> choose
        [ deleteFollowUser
          deleteArticle
          deleteComment
          deleteRemoveFavoriteArticle ]

    choose [ gets; posts; puts; deletes; defaultHandler ]