using NewsAggregator.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewsAggregator.Application.Interfaces
{
    public interface INewsAppService
    {
        Task<List<Article>> GetAllNews();
    }
}
