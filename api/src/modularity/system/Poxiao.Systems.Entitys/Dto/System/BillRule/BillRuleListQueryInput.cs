using Poxiao.Infrastructure.Filter;

namespace Poxiao.Systems.Entitys.Dto.System.BillRule
{
    public class BillRuleListQueryInput : PageInputBase
    {
        /// <summary>
        /// 分类id.
        /// </summary>
        public string categoryId { get; set; }
    }
}
