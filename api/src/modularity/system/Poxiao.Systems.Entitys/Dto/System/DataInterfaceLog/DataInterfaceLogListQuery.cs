using Poxiao.Infrastructure.Filter;

namespace Poxiao.Systems.Entitys.Dto.System.DataInterfaceLog
{
    public class DataInterfaceLogListQuery : PageInputBase
    {
        /// <summary>
        /// 开始时间.
        /// </summary>
        public long? startTime { get; set; }

        /// <summary>
        /// 结束时间.
        /// </summary>
        public long? endTime { get; set; }
    }
}
