using KEEPER.K3._3D.Core.ParamOption;
using Kingdee.BOS;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Rpc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace KEEPER.K3._3D.Contracts
{
    /// <summary>
    /// 服务契约
    /// </summary>
    [RpcServiceError]
    [ServiceContract]
    public interface ICommonService
    {
        /// <summary>
        /// 后台调用单据转换生成目标单
        /// </summary>
        /// <param name="ctx">上下文</param>
        /// <param name="option">单据转换参数</param>
        /// <returns></returns>
        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        DynamicObject[] ConvertBills(Context ctx, ConvertOption option);


        /// <summary>
        /// 组装采购件直接调拨数据包
        /// </summary>
        /// <param name="ctx">上下文</param>
        /// <param name="FormID">直接调拨单标识</param>
        /// <param name="fillBillPropertys">数据</param>
        /// <param name="BillTypeId">单据类型ID</param>
        /// <returns></returns>
        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        DynamicObject installCostRequestPackage(Context ctx, string FormID, Action<IDynamicFormViewService> fillBillPropertys, string BillTypeId);
    }
}
