using System.ComponentModel.DataAnnotations;
using System.Net.Http.Headers;
using busfy_api.src.Domain.Entities.Request;
using busfy_api.src.Domain.Entities.Response;
using busfy_api.src.Domain.Enums;
using busfy_api.src.Domain.IRepository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using webApiTemplate.src.App.IService;

namespace busfy_api.src.Web.Controllers
{
    [ApiController]
    [Route("api")]
    public class ContentCategoryController : ControllerBase
    {
        private readonly IContentCategoryRepository _contentCategoryRepository;
        private readonly ISelectedUserCategoryRepository _selectedUserCategoryRepository;
        private readonly IUserRepository _userRepository;
        private readonly IJwtService _jwtService;

        public ContentCategoryController(
            IContentCategoryRepository contentCategoryRepository,
            ISelectedUserCategoryRepository selectedUserCategoryRepository,
            IUserRepository userRepository,
            IJwtService jwtService
        )
        {
            _contentCategoryRepository = contentCategoryRepository;
            _selectedUserCategoryRepository = selectedUserCategoryRepository;
            _userRepository = userRepository;
            _jwtService = jwtService;
        }

        [HttpPost("content-category"), Authorize(Roles = nameof(UserRole.Admin))]
        [SwaggerOperation("Создать категорию контента")]
        [SwaggerResponse(200, Type = typeof(ContentCategoryBody))]
        [SwaggerResponse(409)]

        public async Task<IActionResult> AddContentCategory([FromBody] CreateContentCategoryBody body)
        {
            var category = await _contentCategoryRepository.AddAsync(body);
            if (category == null)
                return Conflict("Category already exists");

            return Ok(category.ToContentCategoryBody());
        }

        [HttpPost("content-categories/user"), Authorize]
        [SwaggerOperation("Добавить выбранные категории пользователю")]
        [SwaggerResponse(200)]
        [SwaggerResponse(400)]

        public async Task<IActionResult> AddContentCategoryToUser(
            [FromHeader(Name = nameof(HttpRequestHeaders.Authorization))] string token,
            [FromBody] IEnumerable<string> names
        )
        {
            if (!names.Any())
                return BadRequest();

            var tokenPayload = _jwtService.GetTokenPayload(token);
            var user = await _userRepository.GetAsync(tokenPayload.UserId);
            if (user == null)
                return BadRequest();

            var categories = await _contentCategoryRepository.GetAll(names);
            if (!categories.Any())
                return BadRequest();

            foreach (var category in categories)
            {
                var result = await _selectedUserCategoryRepository.AddAsync(user, category);
            }

            return Ok();
        }

        [HttpGet("content-category")]
        [SwaggerOperation("Получить категорию по имени")]
        [SwaggerResponse(200, Type = typeof(ContentCategoryBody))]
        [SwaggerResponse(404)]

        public async Task<IActionResult> GetContentCategory([FromQuery, Required] string name)
        {
            var category = await _contentCategoryRepository.Get(name);
            return category != null ? Ok(category.ToContentCategoryBody()) : NotFound();
        }

        [HttpGet("content-categories")]
        [SwaggerOperation("Получить список категорий контента")]
        [SwaggerResponse(200, Type = typeof(PaginationResponse<ContentCategoryBody>))]

        public async Task<IActionResult> GetAllContentCategories(
            [FromQuery, Range(1, int.MaxValue)] int count = 10,
            [FromQuery, Range(0, int.MaxValue)] int offset = 0
            )
        {
            var categories = await _contentCategoryRepository.GetAllAsync(count, offset);
            var total = await _contentCategoryRepository.GetCountAsync();
            return Ok(new PaginationResponse<ContentCategoryBody>
            {
                Count = count,
                Offset = offset,
                Total = total,
                Items = categories.Select(e => e.ToContentCategoryBody()),
            });
        }
    }
}