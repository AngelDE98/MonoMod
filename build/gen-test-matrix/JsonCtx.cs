﻿using GenTestMatrix.Models;
using System.Text.Json.Serialization;

namespace GenTestMatrix
{
    [JsonSourceGenerationOptions(
        PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
        GenerationMode = JsonSourceGenerationMode.Default,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
        UseStringEnumConverter = true)]
    [JsonSerializable(typeof(OS))]
    [JsonSerializable(typeof(Dotnet))]
    [JsonSerializable(typeof(Job))]
    [JsonSerializable(typeof(MatrixResult))]
    internal partial class JsonCtx : JsonSerializerContext
    {
    }
}
