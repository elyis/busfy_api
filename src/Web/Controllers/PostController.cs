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
        private readonly ISubscriptionToAdditionalContentRepository _subscriptionRepository;
        private readonly ISelectedUserCategoryRepository _selectedUserCategoryRepository;
        private readonly IUserRepository _userRepository;
        private readonly IPostRepository _postRepository;

        public PostController(
            IJwtService jwtService,
            IPostRepository postRepository,
            IContentCategoryRepository contentCategoryRepository,
            ISelectedUserCategoryRepository selectedUserCategoryRepository,
            ISubscriptionToAdditionalContentRepository subscriptionRepository,
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

            var result = await _postRepository.AddAsync(body, creator, contentCategory);
            return Ok(new
            {
                result.Id
            });
        }

        [HttpPost("post/comment"), Authorize]
        [SwaggerOperation("Добавить комментарий к посту")]
        [SwaggerResponse(200)]
        [SwaggerResponse(400)]
        [SwaggerResponse(409)]

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
            return result == null ? Conflict() : Ok();
        }

        [HttpGet("post/{id}")]
        [SwaggerOperation("Получить пост по id")]
        [SwaggerResponse(200)]
        [SwaggerResponse(404)]
        public async Task<IActionResult> GetPostById(Guid id)
        {
            var post = await _postRepository.GetAsync(id);
            return post == null ? NotFound() : Ok(post.ToPostBody());
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
            IEnumerable<Post> posts;
            int totalPosts = 0;
            Guid? userId = null;

            if (token == null)
            {
                posts = await _postRepository.GetAll(count, offset, isDescending);
                totalPosts = await _postRepository.GetCountPosts();
            }
            else
            {
                var tokenPaylod = _jwtService.GetTokenPayload(token);
                userId = tokenPaylod.UserId;
                var categories = await _selectedUserCategoryRepository.GetAllByUserIdAsync((Guid)userId);
                if (categories.Any())
                {
                    var categoryNames = categories.Select(e => e.CategoryName);
                    posts = await _postRepository.GetAllByCategories(count, offset, categoryNames, isDescending);
                    totalPosts = await _postRepository.GetCountPostsByCategories(categoryNames);
                }
                else
                {
                    posts = await _postRepository.GetAll(count, offset, true);
                    totalPosts = await _postRepository.GetCountPosts();
                }
            }

            var items = posts.Select(e => e.ToPostBody()).ToList();
            if (userId != null)
            {
                var postIds = items.Select(e => e.Id);
                var favouritePosts = await _postRepository.GetAllLikes((Guid)userId, postIds);
                foreach (var favouritePost in favouritePosts)
                {
                    var temp = items.First(e => e.Id == favouritePost.PostId);
                    temp.HasEvaluated = true;
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
            var posts = await _postRepository.GetAllByCategory(name, count, offset, isDescending);
            var totalPosts = await _postRepository.GetCountPostsByCategories(new string[] { name });
            var items = posts.Select(e => e.ToPostBody()).ToList();

            if (token != null)
            {
                var tokenPayload = _jwtService.GetTokenPayload(token);
                var postIds = items.Select(e => e.Id);
                var favouritePosts = await _postRepository.GetAllLikes(tokenPayload.UserId, postIds);

                foreach (var favouritePost in favouritePosts)
                {
                    var temp = items.First(e => e.Id == favouritePost.PostId);
                    temp.HasEvaluated = true;
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
            var posts = await _postRepository.GetFavouritePosts(tokenPayload.UserId, count, offset, isDescending);
            var totalPosts = await _postRepository.GetCountFavouritePosts(tokenPayload.UserId);

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
        [SwaggerResponse(200, Type = typeof(PaginationResponse<PostBody>))]

        public async Task<IActionResult> GetTapeBySubscruptions(
            [FromHeader(Name = nameof(HttpRequestHeader.Authorization))] string token,
            [FromQuery, Range(0, int.MaxValue)] int count = 10,
            [FromQuery, Range(0, int.MaxValue)] int offset = 0,
            [FromQuery] bool isDescending = true
        )
        {
            var tokenPayload = _jwtService.GetTokenPayload(token);

            var subscriptions = await _subscriptionRepository.GetSubscriptionsWithAuthorAsync(tokenPayload.UserId, count, offset);
            var creatorIds = subscriptions.Select(e => e.AuthorId);

            var posts = await _postRepository.GetAllByCreators(creatorIds, count, offset, isDescending);
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
    }
}