using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

class VectorDBConnectionTest
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("正在测试向量数据库连接...");

        // 测试 Qdrant 连接
        await TestQdrantConnection();

        // 测试 TEI 嵌入服务
        await TestTEIConnection();

        // 测试 vLLM 服务
        await TestVLLMConnection();
    }

    static async Task TestQdrantConnection()
    {
        Console.WriteLine("\n1. 测试 Qdrant 向量数据库...");
        try
        {
            using var httpClient = new HttpClient();
            var response = await httpClient.GetAsync("http://localhost:6333/");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                Console.WriteLine("✅ Qdrant 连接成功!");
                Console.WriteLine($"响应: {content}");
            }
            else
            {
                Console.WriteLine($"❌ Qdrant 连接失败: {response.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Qdrant 连接异常: {ex.Message}");
        }
    }

    static async Task TestTEIConnection()
    {
        Console.WriteLine("\n2. 测试 TEI 嵌入服务...");
        try
        {
            using var httpClient = new HttpClient();
            var response = await httpClient.GetAsync("http://localhost:8081/health");

            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine("✅ TEI 服务连接成功!");

                // 测试嵌入生成
                await TestEmbeddingGeneration();
            }
            else
            {
                Console.WriteLine($"❌ TEI 服务连接失败: {response.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ TEI 服务连接异常: {ex.Message}");
        }
    }

    static async Task TestEmbeddingGeneration()
    {
        Console.WriteLine("   测试嵌入生成...");
        try
        {
            using var httpClient = new HttpClient();
            var requestData = new
            {
                inputs = "外观缺陷",
                model = "bge-m3",
                parameters = new
                {
                    normalize = true,
                    truncate = true
                }
            };

            var json = JsonSerializer.Serialize(requestData);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            var response = await httpClient.PostAsync("http://localhost:8081/embed", content);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadAsStringAsync();
                Console.WriteLine("✅ 嵌入生成成功!");
                Console.WriteLine($"   嵌入向量长度: {result.Length}");
            }
            else
            {
                Console.WriteLine($"❌ 嵌入生成失败: {response.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ 嵌入生成异常: {ex.Message}");
        }
    }

    static async Task TestVLLMConnection()
    {
        Console.WriteLine("\n3. 测试 vLLM 服务...");
        try
        {
            using var httpClient = new HttpClient();
            var response = await httpClient.GetAsync("http://localhost:8082/v1/models");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                Console.WriteLine("✅ vLLM 服务连接成功!");
                Console.WriteLine($"可用模型: {content}");
            }
            else
            {
                Console.WriteLine($"❌ vLLM 服务连接失败: {response.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ vLLM 服务连接异常: {ex.Message}");
        }
    }
}
