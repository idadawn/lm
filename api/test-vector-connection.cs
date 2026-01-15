using System;
using System.Net.Http;
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
        string endpoint = "http://47.105.59.151:8918";
        string collectionName = "appearance_features";
        
        try
        {
            using var httpClient = new HttpClient();
            httpClient.Timeout = TimeSpan.FromSeconds(10);
            
            // 测试服务是否可达
            Console.WriteLine($"   测试连接: {endpoint}/collections");
            var collectionsResponse = await httpClient.GetAsync($"{endpoint}/collections");
            
            if (collectionsResponse.IsSuccessStatusCode)
            {
                var content = await collectionsResponse.Content.ReadAsStringAsync();
                Console.WriteLine("✅ Qdrant 服务连接成功!");
                Console.WriteLine($"   响应: {content.Substring(0, Math.Min(200, content.Length))}...");
                
                // 测试Collection是否存在
                Console.WriteLine($"   检查Collection: {collectionName}");
                var collectionResponse = await httpClient.GetAsync($"{endpoint}/collections/{collectionName}");
                
                if (collectionResponse.IsSuccessStatusCode)
                {
                    Console.WriteLine($"✅ Collection '{collectionName}' 已存在!");
                }
                else
                {
                    Console.WriteLine($"⚠️  Collection '{collectionName}' 不存在 (Status: {collectionResponse.StatusCode})");
                    Console.WriteLine("   尝试创建Collection...");
                    
                    // 尝试创建Collection
                    var createRequest = new
                    {
                        vectors = new
                        {
                            size = 1024,
                            distance = "Cosine"
                        }
                    };
                    
                    var json = System.Text.Json.JsonSerializer.Serialize(createRequest);
                    var requestContent = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
                    var createResponse = await httpClient.PutAsync($"{endpoint}/collections/{collectionName}", requestContent);
                    
                    if (createResponse.IsSuccessStatusCode)
                    {
                        Console.WriteLine($"✅ Collection '{collectionName}' 创建成功!");
                    }
                    else
                    {
                        var errorContent = await createResponse.Content.ReadAsStringAsync();
                        Console.WriteLine($"❌ Collection 创建失败: {createResponse.StatusCode}");
                        Console.WriteLine($"   错误: {errorContent}");
                    }
                }
            }
            else
            {
                var errorContent = await collectionsResponse.Content.ReadAsStringAsync();
                Console.WriteLine($"❌ Qdrant 连接失败: {collectionsResponse.StatusCode}");
                Console.WriteLine($"   错误: {errorContent}");
            }
        }
        catch (TaskCanceledException)
        {
            Console.WriteLine($"❌ Qdrant 连接超时 (10秒)");
        }
        catch (HttpRequestException httpEx)
        {
            Console.WriteLine($"❌ Qdrant 网络错误: {httpEx.Message}");
            if (httpEx.InnerException != null)
            {
                Console.WriteLine($"   内部异常: {httpEx.InnerException.Message}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Qdrant 连接异常: {ex.Message}");
            Console.WriteLine($"   类型: {ex.GetType().Name}");
        }
    }

    static async Task TestTEIConnection()
    {
        Console.WriteLine("\n2. 测试 TEI 嵌入服务...");
        try
        {
            using var httpClient = new HttpClient();
            var response = await httpClient.GetAsync("http://47.105.59.151:8081/health");

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

            var json = System.Text.Json.JsonSerializer.Serialize(requestData);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            var response = await httpClient.PostAsync("http://47.105.59.151:8081/embed", content);

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
            var response = await httpClient.GetAsync("http://47.105.59.151:8082/v1/models");

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