using KEEPER.K3._3D.Core.Entity;
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


        /// <summary>
        /// 保存单据
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="FormID"></param>
        /// <param name="ids"></param>
        /// <returns></returns>
        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        IOperationResult SaveBill(Context ctx, string FormID, DynamicObject dyObject);

        /// <summary>
        /// 批量保存单据
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="FormID"></param>
        /// <param name="ids"></param>
        /// <returns></returns>
        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        IOperationResult BatchSaveBill(Context ctx, string FormID, DynamicObject[] dyObject);


        /// <summary>
        /// 判断是否继续进入采购调拨计划
        /// </summary>
        /// <param name="ctx"></param>
        /// <returns></returns>
        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        Boolean isTransfer(Context ctx);

        /// <summary>
        /// 获取采购调拨数据
        /// </summary>
        /// <param name="ctx"></param>
        /// <returns></returns>
        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        List<SalOrderTransferList> getPurTransferData(Context ctx);

        /// <summary>
        /// 更新中间表状态以及更新时间
        /// </summary>
        /// <param name="ctx"></param>
        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        void updateTableStatus(Context ctx,long[] ids,int status);
    }
}
