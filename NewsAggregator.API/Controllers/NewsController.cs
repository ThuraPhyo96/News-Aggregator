using Microsoft.AspNetCore.Mvc;
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

        //// POST /api/news
        //[HttpPost]
        //public async Task<IActionResult> CreateNews([FromBody] News news)
        //{
        //    // Logic to create a new news item
        //    return CreatedAtAction(nameof(GetNewsById), new { id = news.Id }, news);
        //}

        //// PUT /api/news/{id}
        //[HttpPut("{id}")]
        //public async Task<IActionResult> UpdateNews(int id, [FromBody] News news)
        //{
        //    // Logic to update an existing news item
        //    return NoContent();  // Successful update, no content to return
        //}

        //// DELETE /api/news/{id}
        //[HttpDelete("{id}")]
        //public async Task<IActionResult> DeleteNews(int id)
        //{
        //    // Logic to delete a news item
        //    return NoContent();  // Successfully deleted
        //}
    }
}
