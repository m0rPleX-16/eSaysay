using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Identity;

namespace eSaysay.Models.Entities
{
    public class AdaptiveLearning
    {
        [Key]
        public int AdaptiveID { get; set; }

        [Required]
        [ForeignKey("User")]
        public string UserID { get; set; } = string.Empty;
        public ApplicationUser? User { get; set; }


        [Required]
        [Range(1, 15)]
        public int CurrentLevel { get; set; }

        [Required]
        [Column(TypeName = "nvarchar(MAX)")]
        [JsonConverter(typeof(JsonStringListConverter))]
        public List<int> RecommendedLessons { get; set; } = new List<int>();

        [Required]
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

        public void UpdateLastUpdated()
        {
            LastUpdated = DateTime.UtcNow;
        }
    }
    public class JsonStringListConverter : JsonConverter<List<int>>
    {
        public override List<int> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            try
            {
                return JsonSerializer.Deserialize<List<int>>(ref reader, options) ?? new List<int>();
            }
            catch (JsonException)
            {
                return new List<int>();
            }
        }

        public override void Write(Utf8JsonWriter writer, List<int> value, JsonSerializerOptions options)
        {
            JsonSerializer.Serialize(writer, value, options);
        }
    }
}
