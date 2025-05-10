using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using News.Application.DTOs;
using News.Application.Interfaces;

namespace News.API.Controllers
{
    [EnableRateLimiting("fixed")]
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
                var result = await _newsAppService.GetNewsById(id);

                if (!result.Success)
                {
                    if (result.ErrorMessage?.Contains("Invalid ID format") == true)
                    {
                        return BadRequest(result.ErrorMessage);
                    }

                    if (result.ErrorMessage?.Contains("Not found!") == true)
                    {
                        return NotFound(result.ErrorMessage);
                    }

                    return BadRequest(result.ErrorMessage);
                }

                return Ok(result.Data);
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
            try
            {
                var result = await _newsAppService.CreateArticle(article);

                if (!result.Success)
                    return BadRequest(result.ErrorMessage);

                return CreatedAtAction(nameof(GetNewsById), new { id = result.Data!.Id }, result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        //PUT /api/news/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateNews(string id, [FromBody] UpdateArticleDto article)
        {
            try
            {
                var result = await _newsAppService.UpdateArticle(id, article);

                if (!result.Success)
                {
                    if (result.ErrorMessage?.Contains("Invalid ID format") == true ||
                        result.ErrorMessage?.Contains("Invalid article data") == true)
                    {
                        return BadRequest(result.ErrorMessage);
                    }

                    if (result.ErrorMessage?.Contains("Not found!") == true)
                    {
                        return NotFound(result.ErrorMessage);
                    }

                    if (result.ErrorMessage?.Contains("Failed to update article") == true)
                    {
                        return StatusCode(500, new { error = result.ErrorMessage });
                    }

                    return BadRequest(result.ErrorMessage);
                }

                return NoContent();  // Successful update, no content to return
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        // DELETE /api/news/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteNews(string id)
        {
            try
            {
                var result = await _newsAppService.DeleteArticle(id);
                if (!result.Success)
                {
                    if (result.ErrorMessage?.Contains("Invalid ID format") == true)
                    {
                        return BadRequest(result.ErrorMessage);
                    }

                    if (result.ErrorMessage?.Contains("Not found!") == true)
                    {
                        return NotFound(result.ErrorMessage);
                    }

                    if (result.ErrorMessage?.Contains("Failed to delete article") == true)
                    {
                        return StatusCode(500, new { error = result.ErrorMessage });
                    }

                    return BadRequest(result.ErrorMessage);
                }

                return NoContent();  // Successfully deleted
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}
