﻿using Newtonsoft.Json;

namespace Nalai.CoreConnector.Models;

public class NalaiResult<T>
{
    [JsonProperty("success")] public bool Success { get; set; }

    [JsonProperty("code")] public string Code { get; set; }

    [JsonProperty("data")] public T Data { get; set; }
}