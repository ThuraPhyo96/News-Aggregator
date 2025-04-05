﻿using NewsAggregator.Application.DTOs;
using NewsAggregator.Domain.Models;

namespace NewsAggregator.Application.Mappers
{
    public class SourceMapper
    {
        public static SourceDto? ToDto(Source source)
        {
            if (source == null) return null;

            return new SourceDto
            {
                Id = source.Id,
                Name = source.Name
            };
        }

        public static Source? ToEntity(SourceDto source)
        {
            if (source == null) return null;

            return new Source(source.Id, source.Name);
        }
    }
}
