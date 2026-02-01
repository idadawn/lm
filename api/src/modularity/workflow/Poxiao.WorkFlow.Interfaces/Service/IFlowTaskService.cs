using Poxiao.Infrastructure.Models.WorkFlow;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Poxiao.WorkFlow.Interfaces.Service
{
    /// <summary>
    /// 流程任务.
    /// </summary>
    public interface IFlowTaskService
    {
        /// <summary>
        /// 新建.
        /// </summary>
        /// <param name="flowTaskSubmit">请求参数.</param>
        /// <returns></returns>
        Task<dynamic> Create(FlowTaskSubmitModel flowTaskSubmit);
    }
}
