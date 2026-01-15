using Poxiao.Systems.Entitys.Model.DataInterFace;

namespace Poxiao.Systems.Entitys.Model.System.DataInterFace;

public class DataInterfaceProperJson
{
    /// <summary>
    /// 总数sql.
    /// </summary>
    public string countSql { get; set; }

    /// <summary>
    /// 回显sql.
    /// </summary>
    public string echoSql { get; set; }

    /// <summary>
    /// 回显地址.
    /// </summary>
    public string echoPath { get; set; }

    /// <summary>
    /// 回显方式.
    /// </summary>
    public string echoReqMethod { get; set; }

    /// <summary>
    /// 回显请求参数.
    /// </summary>
    public List<DataInterfaceReqParameter> echoReqParameters { get; set; } = new List<DataInterfaceReqParameter>();

    /// <summary>
    /// 回显头部参数.
    /// </summary>
    public List<DataInterfaceReqParameter> echoReqHeaders { get; set; } = new List<DataInterfaceReqParameter>();

    public List<DataInterfaceReqParameter> pageParameters { get; set; } = new List<DataInterfaceReqParameter>();

    public List<DataInterfaceReqParameter> echoParameters { get; set; } = new List<DataInterfaceReqParameter>();
}
