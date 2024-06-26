using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Http.Headers;
using busfy_api.src.Domain.Entities.Request;
using busfy_api.src.Domain.Entities.Response;
using busfy_api.src.Domain.Enums;
using busfy_api.src.Domain.IRepository;
using busfy_api.src.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using webApiTemplate.src.App.IService;

namespace busfy_api.src.Web.Controllers
{
    [ApiController]
    [Route("api")]
    public class PostController : ControllerBase
    {
        private readonly IJwtService _jwtService;
        private readonly IContentCategoryRepository _contentCategoryRepository;
        private readonly ISubscriptionRepository _subscriptionRepository;
        private readonly ISelectedUserCategoryRepository _selectedUserCategoryRepository;
        private readonly IUserRepository _userRepository;
        private readonly IPostRepository _postRepository;

        public PostController(
            IJwtService jwtService,
            IPostRepository postRepository,
            IContentCategoryRepository contentCategoryRepository,
            ISelectedUserCategoryRepository selectedUserCategoryRepository,
            ISubscriptionRepository subscriptionRepository,
            IUserRepository userRepository
        )
        {
            _jwtService = jwtService;
            _postRepository = postRepository;
            _contentCategoryRepository = contentCategoryRepository;
            _selectedUserCategoryRepository = selectedUserCategoryRepository;
            _userRepository = userRepository;
            _subscriptionRepository = subscriptionRepository;
        }

        [HttpPost("post"), Authorize]
        [SwaggerOperation("Создать пост")]
        [SwaggerResponse(200)]
        [SwaggerResponse(400)]

        public async Task<IActionResult> CreatePost(
            CreatePostBody body,
            [FromQuery, Required] string categoryName,
            [FromHeader(Name = nameof(HttpRequestHeader.Authorization))] string token)
        {
            var tokenInfo = _jwtService.GetTokenPayload(token);
            var contentCategory = await _contentCategoryRepository.Get(categoryName);
            if (contentCategory == null)
                return BadRequest();

            var creator = await _userRepository.GetAsync(tokenInfo.UserId);
            if (creator == null)
                return BadRequest();

            Subscription? subscription = null;
            if (body.SubscriptionId != null)
                subscription = await _subscriptionRepository.GetSubscriptionAsync((Guid)body.SubscriptionId);

            var result = await _postRepository.AddAsync(body, creator, contentCategory, subscription);
            return Ok(new
            {
                result.Id
            });
        }

        [HttpPost("post/comment"), Authorize]
        [SwaggerOperation("Добавить комментарий к посту")]
        [SwaggerResponse(200)]
        [SwaggerResponse(400)]

        public async Task<IActionResult> CreatePostComment(
            [FromHeader(Name = nameof(HttpRequestHeader.Authorization))] string token,
            [FromQuery, Required] Guid postId,
            CreateCommentBody body
        )
        {
            var post = await _postRepository.GetAsync(postId);
            if (post == null || !post.IsCommentingAllowed)
                return BadRequest();

            var tokenPayload = _jwtService.GetTokenPayload(token);
            var user = await _userRepository.GetAsync(tokenPayload.UserId);
            if (user == null)
                return BadRequest();

            var result = await _postRepository.AddPostComment(post, user, body);
            return Ok();
        }

        [HttpGet("post/{id}")]
        [SwaggerOperation("Получить пост по id")]
        [SwaggerResponse(200)]
        [SwaggerResponse(404)]
        public async Task<IActionResult> GetPostById(
            [FromHeader(Name = nameof(HttpRequestHeaders.Authorization))] string? token,
            Guid id)
        {
            var post = await _postRepository.GetAsync(id);
            if (post == null)
                return NotFound();

            var result = post.ToPostBody();
            if (token != null)
            {
                var tokenPayload = _jwtService.GetTokenPayload(token);
                var like = await _postRepository.GetPostLike(tokenPayload.UserId, result.Id);
                if (like != null)
                    result.HasEvaluated = true;
            }

            return Ok(result);
        }

        [HttpPost("post/like"), Authorize]
        [SwaggerOperation("Оценить пост")]
        [SwaggerResponse(200, Type = typeof(EvaluationStatus))]
        [SwaggerResponse(400)]

        public async Task<IActionResult> CreatePostLike(
            [FromHeader(Name = nameof(HttpRequestHeader.Authorization))] string token,
            [FromQuery, Required] Guid postId
        )
        {
            var post = await _postRepository.GetAsync(postId);
            if (post == null)
                return BadRequest();

            var tokenInfo = _jwtService.GetTokenPayload(token);
            var user = await _userRepository.GetAsync(tokenInfo.UserId);
            if (user == null)
                return BadRequest();

            var like = await _postRepository.GetPostLike(user.Id, postId);
            if (like == null)
            {
                var result = await _postRepository.AddPostLike(post, user);
                return Ok(EvaluationStatus.Evaluated);
            }

            await _postRepository.RemoveLike(postId, user.Id);
            return Ok(EvaluationStatus.NotEvaluated);
        }

        [HttpGet("tape/recommendations")]
        [SwaggerOperation("Получить ленту рекомендации")]
        [SwaggerResponse(200, Type = typeof(PaginationResponse<PostBody>))]
        public async Task<IActionResult> GetCommonTape(
            [FromHeader(Name = nameof(HttpRequestHeaders.Authorization))] string? token,
            [FromQuery, Range(0, int.MaxValue)] int count = 10,
            [FromQuery, Range(0, int.MaxValue)] int offset = 0,
            [FromQuery] bool isDescending = true
        )
        {
            var types = new List<ContentSubscriptionType>() { ContentSubscriptionType.Public };
            Guid? userId = null;

            if (token != null)
            {
                var tokenPayload = _jwtService.GetTokenPayload(token);
                userId = tokenPayload.UserId;

                var userSubscription = await _subscriptionRepository.GetSubscriptionsByUserAndSubscription(tokenPayload.UserId, int.MaxValue, 0);

                var subscriptionsTypes = userSubscription
                    .Select(e => e.Subscription.Type)
                    .Distinct()
                    .Select(Enum.Parse<ContentSubscriptionType>);
                types.AddRange(subscriptionsTypes);

                var uniqueContent = userSubscription
                    .Where(e => e.Subscription.Type == ContentSubscriptionType.Single.ToString())
                    .Select(e => e.Subscription.Posts.First().Id);


                var categories = await _selectedUserCategoryRepository.GetAllByUserIdAsync(userId.Value);
                if (categories.Any())
                {
                    var categoryNames = categories.Select(e => e.CategoryName);
                    var categoryPosts = await _postRepository.GetAllByCategories(types, count, offset, categoryNames, isDescending);
                    return await CreateResponse(categoryPosts, count, offset, categoryNames, userId);
                }
            }

            var posts = await _postRepository.GetAll(types, count, offset, isDescending);
            return await CreateResponse(posts, count, offset, null, userId);
        }

        [HttpGet("tape/category")]
        [SwaggerOperation("Получить ленту по категории")]
        [SwaggerResponse(200, Type = typeof(PaginationResponse<PostBody>))]

        public async Task<IActionResult> GetTapeByCategory(
            [FromQuery] string name,
            [FromHeader(Name = nameof(HttpRequestHeaders.Authorization))] string? token,
            [FromQuery, Range(0, int.MaxValue)] int count = 10,
            [FromQuery, Range(0, int.MaxValue)] int offset = 0,
            [FromQuery] bool isDescending = true
        )
        {
            Guid? userId = null;
            var types = new List<ContentSubscriptionType>() { ContentSubscriptionType.Public };
            var category = await _contentCategoryRepository.Get(name);
            if (category == null)
                return BadRequest();

            if (token != null)
            {
                var tokenPayload = _jwtService.GetTokenPayload(token);
                userId = tokenPayload.UserId;

                var subscriptionsTypes = (await _subscriptionRepository.GetSubscriptionsByUserAndSubscription(tokenPayload.UserId, int.MaxValue, 0))
                    .Select(e => e.Subscription.Type)
                    .Distinct()
                    .Select(Enum.Parse<ContentSubscriptionType>);

                types.AddRange(subscriptionsTypes);
            }

            var postsByCategory = await _postRepository.GetAllByCategory(types, category.Name, count, offset, isDescending);
            return await CreateResponse(postsByCategory, count, offset, new List<string> { category.Name }, userId);
        }

        [HttpGet("tape/favourites"), Authorize]
        [SwaggerOperation("Получить список избранных")]
        [SwaggerResponse(200, Type = typeof(PaginationResponse<PostBody>))]

        public async Task<IActionResult> GetFavourites(
            [FromHeader(Name = nameof(HttpRequestHeaders.Authorization))] string token,
            [FromQuery, Range(0, int.MaxValue)] int count = 10,
            [FromQuery, Range(0, int.MaxValue)] int offset = 0,
            [FromQuery] bool isDescending = true
        )
        {
            var tokenPayload = _jwtService.GetTokenPayload(token);
            var favoritePosts = await _postRepository.GetFavoritePostsAndPost(tokenPayload.UserId, count, offset, isDescending);
            var posts = favoritePosts.Select(e => e.Post);

            var items = posts.Select(e => e.ToPostBody()).ToList();
            var postIds = items.Select(e => e.Id);
            var favouritePosts = await _postRepository.GetAllLikes(tokenPayload.UserId, postIds);
            foreach (var favouritePost in favouritePosts)
            {
                var temp = items.FirstOrDefault(e => e.Id == favouritePost.PostId);
                if (temp != null)
                {
                    temp.HasEvaluated = true;
                }
            }

            var totalFavoritePost = await _postRepository.GetFavoritePostCount(tokenPayload.UserId);
            return Ok(new PaginationResponse<PostBody>
            {
                Count = count,
                Offset = offset,
                Total = totalFavoritePost,
                Items = items
            });
        }

        [HttpPost("post/favourite"), Authorize]
        [SwaggerOperation("Добавить избранные")]
        [SwaggerResponse(200)]
        [SwaggerResponse(400, "PostId не существует")]
        [SwaggerResponse(409)]
        public async Task<IActionResult> CreateFavoritePost(
            [FromHeader(Name = nameof(HttpRequestHeaders.Authorization))] string token,
            [FromQuery, Required] Guid postId
        )
        {
            var tokenPayload = _jwtService.GetTokenPayload(token);
            var post = await _postRepository.GetAsync(postId);
            if (post == null)
                return BadRequest("postId is not exits");

            var user = await _userRepository.GetAsync(tokenPayload.UserId);
            if (user == null)
                return BadRequest();

            var result = await _postRepository.AddFavoritePost(user, post);
            return result == null ? Conflict() : Ok();
        }

        [HttpDelete("post/favourite"), Authorize]
        [SwaggerOperation("Удалить из избранных")]
        [SwaggerResponse(204)]
        public async Task<IActionResult> DeleteFavoritePost(
            [FromHeader(Name = nameof(HttpRequestHeaders.Authorization))] string token,
            [FromQuery, Required] Guid postId
        )
        {
            var tokenPayload = _jwtService.GetTokenPayload(token);
            var result = await _postRepository.RemoveFavoritePost(tokenPayload.UserId, postId);
            return NoContent();
        }

        [HttpGet("post/likes")]
        [SwaggerOperation("Получить число лайков в посте")]
        [SwaggerResponse(200, Type = typeof(int))]
        public async Task<IActionResult> GetCountLikes([FromQuery, Required] Guid postId)
        {
            var count = await _postRepository.GetCountLikes(postId);
            return Ok(new
            {
                count
            });
        }

        [HttpGet("tape/subscriptions"), Authorize]
        [SwaggerOperation("Получить ленту по подпискам")]
        [SwaggerResponse(200, Type = typeof(PaginationResponse<PostBody>))]

        public async Task<IActionResult> GetTapeBySubscruptions(
            [FromHeader(Name = nameof(HttpRequestHeader.Authorization))] string token,
            [FromQuery, Range(0, int.MaxValue)] int count = 10,
            [FromQuery, Range(0, int.MaxValue)] int offset = 0,
            [FromQuery] bool isDescending = true
        )
        {
            var types = new List<ContentSubscriptionType>() { ContentSubscriptionType.Public };
            var tokenPayload = _jwtService.GetTokenPayload(token);

            var userSubscriptions = await _subscriptionRepository.GetSubscriptionsByUserAndSubscription(tokenPayload.UserId, int.MaxValue, 0);
            var subscriptionsTypes = userSubscriptions
                .Select(e => e.Subscription.Type)
                .Distinct()
                .Select(Enum.Parse<ContentSubscriptionType>);

            types.AddRange(subscriptionsTypes);
            var creatorIds = userSubscriptions.GroupBy(e => e.Subscription.CreatorId).Select(e => e.Key);

            var posts = await _postRepository.GetAllByCreators(types, creatorIds, count, offset, isDescending);
            var totalPosts = await _postRepository.GetCountPostByCreators(creatorIds);
            var items = posts.Select(e => e.ToPostBody()).ToList();

            var postIds = items.Select(e => e.Id);
            var favouritePosts = await _postRepository.GetAllLikes(tokenPayload.UserId, postIds);

            foreach (var favouritePost in favouritePosts)
            {
                var temp = items.First(e => e.Id == favouritePost.PostId);
                temp.HasEvaluated = true;
            }

            return Ok(new PaginationResponse<PostBody>
            {
                Count = count,
                Offset = offset,
                Total = totalPosts,
                Items = items
            });
        }

        [HttpPatch("post"), Authorize]
        [SwaggerOperation("Обновить подписку поста на single")]
        [SwaggerResponse(200)]
        [SwaggerResponse(400)]
        [SwaggerResponse(403)]

        public async Task<IActionResult> UpdateSubscriptionType(
            [FromHeader(Name = nameof(HttpRequestHeaders.Authorization))] string token,
            UpdatePostSubscriptionBody body
        )
        {
            var tokenPayload = _jwtService.GetTokenPayload(token);

            var subscription = await _subscriptionRepository.GetSubscriptionAsync(body.SubscriptionId);
            if (subscription == null)
                return BadRequest();

            if (subscription.CreatorId != tokenPayload.UserId)
                return Forbid();

            var result = await _postRepository.UpdateSubscriptionType(body.PostId, subscription);
            return result == null ? BadRequest() : Ok();
        }

        [HttpGet("tape/creator")]
        [SwaggerResponse(200, Type = typeof(PaginationResponse<PostBody>))]

        public async Task<IActionResult> GetTapeByCreator(
            [FromHeader(Name = nameof(HttpRequestHeader.Authorization))] string? token,
            [FromQuery, Required] Guid userId,
            [FromQuery, Range(0, int.MaxValue)] int count = 10,
            [FromQuery, Range(0, int.MaxValue)] int offset = 0,
            [FromQuery] bool isDescending = true
        )
        {
            var types = new List<ContentSubscriptionType>() { ContentSubscriptionType.Public };

            if (token != null)
            {
                var tokenPayload = _jwtService.GetTokenPayload(token);
                userId = tokenPayload.UserId;

                var subscriptionsTypes = (await _subscriptionRepository.GetSubscriptionsByUserAndSubscription(tokenPayload.UserId, int.MaxValue, 0))
                    .Select(e => e.Subscription.Type)
                    .Distinct()
                    .Select(Enum.Parse<ContentSubscriptionType>);

                types.AddRange(subscriptionsTypes);
            }

            var posts = await _postRepository.GetAllByCreator(types, userId, count, offset, isDescending);
            var items = posts.Select(e => e.ToPostBody()).ToList();
            var totalPosts = await _postRepository.GetCountPostsByCreator(userId);

            if (token != null)
            {
                var tokenPayload = _jwtService.GetTokenPayload(token);
                var postIds = items.Select(e => e.Id);
                var favouritePosts = await _postRepository.GetAllLikes(userId, postIds);

                foreach (var favouritePost in favouritePosts)
                {
                    var item = items.First(e => e.Id == favouritePost.PostId);
                    item.HasEvaluated = true;
                }
            }

            return Ok(new PaginationResponse<PostBody>
            {
                Count = count,
                Offset = offset,
                Total = totalPosts,
                Items = items
            });
        }


        [HttpGet("comments")]
        [SwaggerOperation("Получить список комментариев к посту")]
        [SwaggerResponse(200, Type = typeof(PaginationResponse<CommentBody>))]
        public async Task<IActionResult> GetComments(
            [FromQuery, Required] Guid postId,
            [FromQuery, Range(1, int.MaxValue)] int count = 5,
            [FromQuery, Range(0, int.MaxValue)] int offset = 0,
            [FromQuery] bool isDescending = true
        )
        {
            var comments = await _postRepository.GetCommentsWithUser(postId, count, offset, isDescending);
            var totalComments = await _postRepository.GetCountComments(postId);

            return Ok(new PaginationResponse<CommentBody>
            {
                Count = count,
                Total = totalComments,
                Items = comments.Select(e => e.ToCommentBody()),
                Offset = offset
            });
        }
        private async Task<IActionResult> CreateResponse(IEnumerable<Post> posts, int count, int offset, IEnumerable<string>? categoryNames, Guid? userId)
        {
            var items = posts.Select(e => e.ToPostBody()).ToList();
            if (userId != null)
            {
                var postIds = items.Select(e => e.Id);
                var favouritePosts = await _postRepository.GetAllLikes(userId.Value, postIds);
                foreach (var favouritePost in favouritePosts)
                {
                    var temp = items.FirstOrDefault(e => e.Id == favouritePost.PostId);
                    if (temp != null)
                    {
                        temp.HasEvaluated = true;
                    }
                }
            }

            var totalPosts = categoryNames != null ?
                await _postRepository.GetCountPostsByCategories(categoryNames) :
                await _postRepository.GetCountPosts();

            return Ok(new PaginationResponse<PostBody>
            {
                Count = count,
                Offset = offset,
                Total = totalPosts,
                Items = items
            });
        }
    }
}