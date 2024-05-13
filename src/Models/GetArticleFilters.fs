namespace Models

module Models =

  type GetArticlesFilters =
    { tag: string option
      authorName: string option
      limit: int64 option
      offset: int64 option
      slug: string option
      favoritedUserName: string option
      followedUserName: string option }

  let articlesFilters =
    { tag = None
      authorName = None
      limit = Some 20
      offset = None
      slug = None
      favoritedUserName = None
      followedUserName = None }

  // maybe can use active patterns here?

  let withTagOpt tag filters = { filters with tag = tag }
  let withTag tag filters = withTagOpt (Some tag) filters

  let withAuthorStringOpt authorString filters =
    { filters with
        authorName = authorString }

  let withAuthorString authorString filters =
    withAuthorStringOpt (Some authorString) filters


  let withFavoritedUserNameOpt favoritedUserName filters =
    { filters with
        favoritedUserName = favoritedUserName }

  let withFavoritedUserName favoritedUserName filters =
    withFavoritedUserNameOpt (Some favoritedUserName) filters


  let withLimitOpt (limit: int64 option) filters =
    { filters with limit = limit }

  let withLimit limit filters = withLimitOpt (Some limit) filters


  let withOffsetOpt offset filters = { filters with offset = offset }
  let withOffset offset filters = withOffsetOpt (Some offset) filters

  let withSlugOpt slug filters = { filters with slug = slug }
  let withSlug slug filters = withSlugOpt (Some slug) filters

  let withFollowedUserNameOpt followedUserName filters =
    { filters with
        followedUserName = followedUserName }

  let withFollowedUserName followedUserName filters =
    withFollowedUserNameOpt (Some followedUserName) filters