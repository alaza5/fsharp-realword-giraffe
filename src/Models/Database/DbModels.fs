namespace Models

open System

module DbModels =
  type ArticleUserTags =
    { article: DbTables.ArticlesTable
      user: DbTables.UsersTable
      tags: string array
      favoritedUsers: string array }

  type CommentsWithAuthors =
    { comment: DbTables.CommentsTable
      user: DbTables.UsersTable }

  type UserData =
    { user: DbTables.UsersTable
      followingList: DbTables.UsersTable list }

  type UserWithFollowerList =
    { user: DbTables.UsersTable
      followers: DbTables.UsersTable list }