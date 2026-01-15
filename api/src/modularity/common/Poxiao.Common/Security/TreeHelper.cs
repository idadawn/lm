using Poxiao.Infrastructure.Extension;

namespace Poxiao.Infrastructure.Security;

/// <summary>
/// 树结构帮助类.
/// </summary>
[SuppressSniffer]
public static class TreeHelper
{
    /// <summary>
    /// 建造树结构.
    /// </summary>
    /// <param name="allNodes">所有的节点.</param>
    /// <param name="parentId">节点.</param>
    /// <returns></returns>
    public static List<T> ToTree<T>(this List<T> allNodes, string parentId = "0")
        where T : TreeModel, new()
    {
        List<T> resData = new List<T>();

        // 查找出父类对象
        List<T> rootNodes = allNodes.FindAll(x => x.ParentId == parentId || x.ParentId.IsNullOrEmpty());

        // 移除父类对象
        allNodes.RemoveAll(x => x.ParentId == parentId || x.ParentId.IsNullOrEmpty());
        resData = rootNodes;
        resData.ForEach(aRootNode =>
        {
            aRootNode.HasChildren = HaveChildren(allNodes, aRootNode.Id);
            if (aRootNode.HasChildren)
            {
                aRootNode.Children = _GetChildren(allNodes, aRootNode);
                aRootNode.Num = aRootNode.Children.Count();
            }
            else
            {
                aRootNode.IsLeaf = !aRootNode.HasChildren;
                aRootNode.Children = null;
            }
        });
        return resData;
    }

    #region 私有成员

    /// <summary>
    /// 获取所有子节点.
    /// </summary>
    /// <typeparam name="T">树模型（TreeModel或继承它的模型.</typeparam>
    /// <param name="nodes">所有节点列表.</param>
    /// <param name="parentNode">父节点Id.</param>
    /// <returns></returns>
    private static List<object> _GetChildren<T>(List<T> nodes, T parentNode)
        where T : TreeModel, new()
    {
        Type type = typeof(T);
        var properties = type.GetProperties().ToList();
        List<object> resData = new List<object>();

        // 查找出父类对象
        var children = nodes.FindAll(x => x.ParentId == parentNode.Id);

        // 移除父类对象
        nodes.RemoveAll(x => x.ParentId == parentNode.Id);
        children.ForEach(aChildren =>
        {
            T newNode = new T();
            resData.Add(newNode);

            // 赋值属性
            foreach (var aProperty in properties.Where(x => x.CanWrite))
            {
                var value = aProperty.GetValue(aChildren, null);
                aProperty.SetValue(newNode, value);
            }

            newNode.HasChildren = HaveChildren(nodes, aChildren.Id);
            if (newNode.HasChildren)
            {
                newNode.Children = _GetChildren(nodes, newNode);
            }
            else
            {
                newNode.IsLeaf = !newNode.HasChildren;
                newNode.Children = null;
            }
        });
        return resData;
    }

    /// <summary>
    /// 判断当前节点是否有子节点.
    /// </summary>
    /// <typeparam name="T">树模型.</typeparam>
    /// <param name="nodes">所有节点.</param>
    /// <param name="nodeId">当前节点Id.</param>
    /// <returns></returns>
    private static bool HaveChildren<T>(List<T> nodes, string nodeId)
        where T : TreeModel, new()
    {
        return nodes.Exists(x => x.ParentId == nodeId);
    }

    #endregion
}

/// <summary>
/// 树模型基类.
/// </summary>
public class TreeModel
{
    /// <summary>
    /// 获取节点id.
    /// </summary>
    /// <returns></returns>
    [JsonProperty("id")]
    public string Id { get; set; }

    /// <summary>
    /// 获取节点父id.
    /// </summary>
    /// <returns></returns>
    /// <summary>
    /// 父级.
    /// </summary>
    [JsonProperty("parentId")]
    public string ParentId { get; set; }

    /// <summary>
    /// 是否有子级.
    /// </summary>isLeaf
    [JsonProperty("hasChildren")]
    public bool HasChildren { get; set; }

    /// <summary>
    /// 设置Children.
    /// </summary>
    [JsonProperty("children")]
    public List<object>? Children { get; set; } = new List<object>();

    /// <summary>
    /// 子节点数量.
    /// </summary>
    [JsonProperty("num")]
    public int Num { get; set; }

    /// <summary>
    /// 是否为子节点.
    /// </summary>
    [JsonProperty("isLeaf")]
    public bool IsLeaf { get; set; } = false;
}