using Microsoft.AspNetCore.Mvc;
using NewsAggregator.Application.DTOs;
using NewsAggregator.Application.Interfaces;

namespace NewsAggregator.API.Controllers
{
    [Route("api/news")]
    [ApiController]
    public class NewsController : ControllerBase
    {
        private readonly INewsAppService _newsAppService;

        public NewsController(INewsAppService newsAppService)
        {
            _newsAppService = newsAppService;
        }

        /// <summary>
        /// Retrieves all stored news articles from MongoDB.
        /// </summary>
        /// GET /api/news
        [HttpGet]
        public async Task<IActionResult> GetAllNews()
        {
            try
            {
                var articles = await _newsAppService.GetAllNews();
                return Ok(articles);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Retrieve stored news articles from MongoDB by id
        /// </summary>
        // GET /api/news/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetNewsById(string id)
        {
            try
            {
                var article = await _newsAppService.GetNewsById(id);
                return Ok(article);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        // POST /api/news
        [HttpPost]
        public async Task<IActionResult> CreateNews([FromBody] CreateArticleDto article)
        {
            var result = await _newsAppService.CreateArticle(article);

            if (!result.Success)
                return BadRequest(result.ErrorMessage);

            return CreatedAtAction(nameof(GetNewsById), new { id = result.Data!.Id }, result);
        }

        //PUT /api/news/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateNews(string id, [FromBody] UpdateArticleDto article)
        {
            var result = await _newsAppService.UpdateArticle(id, article);

            if (!result.Success)
                return BadRequest(result.ErrorMessage);

            return NoContent();  // Successful update, no content to return
        }

        // DELETE /api/news/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteNews(string id)
        {
            var result = await _newsAppService.DeleteArticle(id);
            if (!result.Success)
                return BadRequest(result.ErrorMessage);

            return NoContent();  // Successfully deleted
        }
    }
}
