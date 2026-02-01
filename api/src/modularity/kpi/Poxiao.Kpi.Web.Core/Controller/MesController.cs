using Microsoft.AspNetCore.Authorization;

namespace Poxiao.Kpi.Web.Core;

[AllowAnonymous]
[Route("api/mes")]
public class MesController : IDynamicApiController
{
    // 获取订单01生产进度
    //{
    //       "reference": "282",
    //       "computed_color": "102.0",
    //       "color": "102.00000000",
    //       "item__name": "上漆的椅子",
    //       "operation__location__name": "工厂",
    //       "operation": "为椅子上漆",
    //       "demands": [
    //           [
    //               "5.0",
    //               "订单16"
    //           ],
    //           [
    //               "5.0",
    //               "订单15"
    //           ],
    //           [
    //               "5.0",
    //               "订单13"
    //           ],
    //           [
    //               "5.0",
    //               "订单12"
    //           ],
    //           [
    //               "10.0",
    //               "订单14"
    //           ]
    //       ],
    //       "startdate": "2025-04-29 17:00:00",
    //       "enddate": "2025-04-30 08:00:00",
    //       "opplan_duration": 54000,
    //       "opplan_net_duration": 54000,
    //       "quantity": "30.00000000",
    //       "status": "proposed",
    //       "criticality": "2.00000000",
    //       "delay": -172800,
    //       "material": [
    //           [
    //               "椅子",
    //               -30
    //           ]
    //       ],
    //       "resource": [
    //           [
    //               "操作员3",
    //               1
    //           ]
    //       ],
    //       "owner": "288",
    //       "lastmodified": "2024-05-04 15:52:17",
    //       "operation__type": "time_per",
    //       "operation__duration": 0,
    //       "operation__duration_per": 1800,
    //       "operation__sizeminimum": "1.00000000",
    //       "operation__priority": "1",
    //       "operation__lastmodified": "2024-05-04 15:34:27.676976",
    //       "feasible": "True",
    //       "operation__location__available": "工作日",
    //       "operation__location__owner": "全部",
    //       "operation__location__lastmodified": "2024-05-04 15:22:51.597730",
    //       "end_items": [
    //           [
    //               "上漆的椅子",
    //               30
    //           ]
    //       ],
    //       "item__latedemandcount": "0",
    //       "item__latedemandquantity": "0E-8",
    //       "item__latedemandvalue": "0E-8",
    //       "item__unplanneddemandcount": "0",
    //       "item__unplanneddemandquantity": "0E-8",
    //       "item__unplanneddemandvalue": "0E-8"
    //   }

}
