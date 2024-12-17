using System.Text.Json.Serialization;

namespace RZTask.Server.Data
{
    public class QueryCondition
    {
        [JsonPropertyName("key")]
        public string Key { get; set; } = "id";

        [JsonPropertyName("value")]
        public string Value { get; set; } = "0";

        [JsonPropertyName("type")]
        public string Type { get; set; } = "=";
    }

    public class QueryParameters
    {
        [JsonPropertyName("query")]
        public List<QueryCondition> Query { get; set; } = new List<QueryCondition>();

        [JsonPropertyName("query_type")]
        public string QueryType { get; set; } = "AND";

        [JsonPropertyName("order_by")]
        public string OrderBy { get; set; } = "id";

        [JsonPropertyName("is_ascending")]
        public bool IsAscending { get; set; } = true; // 是否正序 (默认正序)

        [JsonPropertyName("limit")]
        public int Limit { get; set; } = 20;

        [JsonPropertyName("page")]
        public int Page { get; set; } = 1;
    }
}
