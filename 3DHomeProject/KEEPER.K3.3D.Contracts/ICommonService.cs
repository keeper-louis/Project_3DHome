﻿using KEEPER.K3._3D.Core.Entity;
using KEEPER.K3._3D.Core.ParamOption;
using Kingdee.BOS;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.Validation;
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
        Boolean isTransfer(Context ctx,UpdatePrtableinEnum status);

        /// <summary>
        /// 获取采购调拨数据
        /// </summary>
        /// <param name="ctx"></param>
        /// <returns></returns>
        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        List<SalOrderTransferList> getPurTransferData(Context ctx,UpdatePrtableinEnum status);

        /// <summary>
        /// 获取审核失败数据集合
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="status">业务状态码</param>
        /// <returns></returns>
        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        List<UpdatePrtableEntity> getAuditErrorData(Context ctx, UpdatePrtableinEnum status);


        /// <summary>
        /// 更新prtablein中间表状态以及更新时间。。。。。
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="status">不同状态更新枚举</param>
        /// <param name="ids">prtIDS</param>
        /// <param name="uyList">更新数据包实体集合</param>
        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        void updateTableStatus(Context ctx, UpdatePrtableinEnum status,long[] ids = null,List<UpdatePrtableEntity> uyList = null);

        /// <summary>
        /// 将错误信息插入错误信息表中
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="status">不同状态更新枚举</param>
        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        void insertErrorTable(Context ctx, UpdatePrtableinEnum status);

        /// <summary>
        /// 提交单据
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="FormID"></param>
        /// <param name="ids"></param>
        /// <returns></returns>
        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        IOperationResult SubmitBill(Context ctx, string FormID, object[] ids);

        /// <summary>
        /// 审核单据
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="FormID"></param>
        /// <param name="ids"></param>
        /// <returns></returns>
        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        IOperationResult AuditBill(Context ctx, string FormID, object[] ids);

        /// <summary>
        /// 组装更新数据包实体
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="status">不同场景的更新枚举</param>
        /// <param name="trasferentry">调拨单更新的集合</param>
        /// <param name="vo">错误更新的集合</param>
        /// <returns></returns>
        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        List<UpdatePrtableEntity> InstallUpdatePackage(Context ctx, UpdatePrtableinEnum status, DynamicObject[] trasferbill = null,List<ValidationErrorInfo> vo = null, List<UpdatePrtableEntity> exceptPrtList = null, IOperationResult auditResult = null,DynamicObject submitResult = null,long k3cloudheadid = 0,string billno = "");
    }
}
