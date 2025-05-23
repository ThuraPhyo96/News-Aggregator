﻿using System.Text.Json.Serialization;

namespace NewsAggregator.Domain.Models
{
    public class Source
    {
        [JsonPropertyName("id")]
        public string? Id { get; private set; }

        [JsonPropertyName("name")]
        public string? Name { get; private set; }

        public Source()
        {
                
        }

        public Source(string? id, string? name)
        {
            Id = id;
            Name = name;
        }
    }
}
