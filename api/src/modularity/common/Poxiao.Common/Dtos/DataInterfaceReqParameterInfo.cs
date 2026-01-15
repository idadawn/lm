namespace Poxiao.Infrastructure.Dtos
{
    /// <summary>
    /// 数据接口请求参数.
    /// </summary>
    [SuppressSniffer]
    public class DataInterfaceReqParameterInfo
    {
        /// <summary>
        /// 字段.
        /// </summary>
        public string field { get; set; }

        /// <summary>
        /// 值.
        /// </summary>
        public string defaultValue { get; set; }

        /// <summary>
        /// 主键.
        /// </summary>
        public string id { get; set; }

        /// <summary>
        /// 类型.
        /// </summary>
        public string dataType { get; set; }

        /// <summary>
        /// 必填.
        /// </summary>
        public int required { get; set; }

        /// <summary>
        /// 注释.
        /// </summary>
        public string fieldName { get; set; }
    }
}
