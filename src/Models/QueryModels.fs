namespace Models

module Models =
  open System
  // TODO
  // favorited = ctx.TryGetQueryStringValue "favorited"
  type GetArticlesFilters =
    { tag: string option
      author: string option
      limit: int64 option
      offset: int64 option
      slug: string option
      userId: Guid option }

  let articlesFilters =
    { tag = None
      author = None
      limit = None
      offset = None
      slug = None
      userId = None }

  let withTag tag filters = { filters with tag = tag }
  let withAuthor author filters = { filters with author = author }
  let withLimit limit filters = { filters with limit = limit }
  let withOffset offset filters = { filters with offset = offset }
  let withSlug slug filters = { filters with slug = slug }
  let withUserId userId filters = { filters with userId = userId }