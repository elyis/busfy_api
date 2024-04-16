using System.ComponentModel.DataAnnotations;
using busfy_api.src.Domain.Entities.Request;
using busfy_api.src.Domain.Entities.Response;
using busfy_api.src.Domain.IRepository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace busfy_api.src.Web.Controllers
{
    [ApiController]
    [Route("api")]
    public class ContentCategoryController : ControllerBase
    {
        private readonly IContentCategoryRepository _contentCategoryRepository;

        public ContentCategoryController(IContentCategoryRepository contentCategoryRepository)
        {
            _contentCategoryRepository = contentCategoryRepository;
        }

        [HttpPost("content-category"), Authorize]
        [SwaggerOperation("Создать категорию контента")]
        [SwaggerResponse(200)]
        [SwaggerResponse(409)]

        public async Task<IActionResult> AddContentCategory([FromBody] CreateContentCategoryBody body)
        {
            var category = await _contentCategoryRepository.AddAsync(body);
            if (category == null)
                return Conflict("Category already exists");

            return Ok(category);
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
        [SwaggerResponse(200, Type = typeof(IEnumerable<ContentCategoryBody>))]

        public async Task<IActionResult> GetAllContentCategories([FromQuery, Range(1, int.MaxValue)] int count = 10, [FromQuery, Range(0, int.MaxValue)] int offset = 0)
        {
            var categories = await _contentCategoryRepository.GetAllAsync(count, offset);
            return Ok(categories);
        }
    }
}