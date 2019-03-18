using KEEPER.K3._3D.Contracts;
using KEEPER.K3._3D.Core.Entity;
using KEEPER.K3._3D.Core.ParamOption;
using Kingdee.BOS;
using Kingdee.BOS.Contracts;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.Validation;
using Kingdee.BOS.Orm.DataEntity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KEEPER.K3._3D._3DServiceHelper
{
    /// <summary>
    /// 
    /// </summary>
    public class _3DServiceHelper
    {
        public static DynamicObject[] ConvertBills(Context ctx, List<ConvertOption> option,string SourceFormId,string TargetFormId,string SourceEntryEntityKey)
        {
            ICommonService service = KEEPER.K3._3D.Contracts.ServiceFactory.GetService<ICommonService>(ctx);
            return service.ConvertBills(ctx, option,SourceFormId,TargetFormId,SourceEntryEntityKey);
        }

        /// <summary>
        /// 构建业务对象数据包
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="FormID">对象标识</param>
        /// <param name="fillBillPropertys">填充业务对象属性委托对象</param>
        /// <returns></returns>
        public static DynamicObject CreateBillMode(Context ctx, string FormID, Action<IDynamicFormViewService> fillBillPropertys)
        {
            ICommonService service = KEEPER.K3._3D.Contracts.ServiceFactory.GetService<ICommonService>(ctx);
            DynamicObject model = service.installCostRequestPackage(ctx, FormID, fillBillPropertys, "");
            return model;
        }

        public static IOperationResult Save(Context ctx, string FormID, DynamicObject[] dyObject)
        {
            ICommonService service = KEEPER.K3._3D.Contracts.ServiceFactory.GetService<ICommonService>(ctx);
            IOperationResult saveResult = service.SaveBill(ctx, FormID, dyObject);
            return saveResult;
        }

        public static IOperationResult BatchSave(Context ctx, string FormID, DynamicObject[] dyObject)
        {
            ICommonService service = KEEPER.K3._3D.Contracts.ServiceFactory.GetService<ICommonService>(ctx);
            IOperationResult saveResult = service.BatchSaveBill(ctx, FormID, dyObject);
            return saveResult;
        }

        public static Boolean isTransfer(Context ctx,ObjectEnum Obstatus,UpdatePrtableinEnum status)
        {
            ICommonService service = KEEPER.K3._3D.Contracts.ServiceFactory.GetService<ICommonService>(ctx);
            return service.isTransfer(ctx, Obstatus,status);
        }

        public static List<SalOrderTransferList> getPurTransferData(Context ctx,UpdatePrtableinEnum status)
        {
            ICommonService service = KEEPER.K3._3D.Contracts.ServiceFactory.GetService<ICommonService>(ctx);
            return service.getPurTransferData(ctx,status);
        }

        public static List<ConvertOption> getOLAPushData(Context ctx, ObjectEnum status)
        {
            ICommonService service = KEEPER.K3._3D.Contracts.ServiceFactory.GetService<ICommonService>(ctx);
            return service.getOLAPushData(ctx, status);
        }


        public static List<UpdatePrtableEntity> getAuditErrorData(Context ctx, UpdatePrtableinEnum status)
        {
            ICommonService service = KEEPER.K3._3D.Contracts.ServiceFactory.GetService<ICommonService>(ctx);
            return service.getAuditErrorData(ctx, status);
        }

        public static void updateTableStatus(Context ctx, UpdatePrtableinEnum status,ObjectEnum Obstatus, long[] ids = null,List<UpdatePrtableEntity> uyList = null)
        {
            ICommonService service = KEEPER.K3._3D.Contracts.ServiceFactory.GetService<ICommonService>(ctx);
            service.updateTableStatus(ctx, status,Obstatus,ids,uyList);
        }

        public static void insertErrorTable(Context ctx, UpdatePrtableinEnum status,ObjectEnum Obstatus)
        {
            ICommonService service = KEEPER.K3._3D.Contracts.ServiceFactory.GetService<ICommonService>(ctx);
            service.insertErrorTable(ctx, status,Obstatus);
        }


        /// <summary>
        /// 审核业务对象
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="formID">业务对象标识</param>
        /// <param name="ids">业务对象ID集合</param>
        /// <returns></returns>
        public static IOperationResult Audit(Context ctx, string formID, Object[] ids)
        {
            ICommonService service = KEEPER.K3._3D.Contracts.ServiceFactory.GetService<ICommonService>(ctx);
            IOperationResult auditResult = service.AuditBill(ctx, formID, ids);
            return auditResult;
        }

        /// <summary>
        /// 业务对象提交
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="FormID">业务对象标识</param>
        /// <param name="ids">业务对象ID集合</param>
        /// <returns></returns>
        public static IOperationResult Submit(Context ctx, string formID, Object[] ids)
        {
            ICommonService service = KEEPER.K3._3D.Contracts.ServiceFactory.GetService<ICommonService>(ctx);
            IOperationResult submitResult = service.SubmitBill(ctx, formID, ids);
            return submitResult;
        }

        public static List<UpdatePrtableEntity> InstallUpdatePackage(Context ctx, UpdatePrtableinEnum status,ObjectEnum Obstatus, DynamicObject[] trasferbill = null, List<ValidationErrorInfo> vo = null, List<UpdatePrtableEntity> exceptPrtList = null, IOperationResult auditResult = null, DynamicObject submitResult = null,long k3cloudheadid = 0,string billno = "",string formid = "")
        {
            ICommonService service = KEEPER.K3._3D.Contracts.ServiceFactory.GetService<ICommonService>(ctx);
            return service.InstallUpdatePackage(ctx, status, Obstatus, trasferbill, vo, exceptPrtList, auditResult, submitResult,k3cloudheadid,billno,formid);
        }
        public static SalOrder2DirectTransList getALSaveData(Context ctx, UpdateAltableinEnum status, ObjectEnum Obstatus)
        {
            ICommonService service = KEEPER.K3._3D.Contracts.ServiceFactory.GetService<ICommonService>(ctx);
            return service.getALSaveData(ctx, status, Obstatus);
        }
        public static Boolean isTransfer(Context ctx, ObjectEnum Obstatus, UpdateAltableinEnum status)
        {
            ICommonService service = KEEPER.K3._3D.Contracts.ServiceFactory.GetService<ICommonService>(ctx);
            return service.isTransfer(ctx, Obstatus, status);
        }
        public static void updateAltableStatus(Context ctx, UpdateAltableinEnum status, ObjectEnum Obstatus, long[] ids = null, List<UpdateAltableinEntity> uyList = null)
        {
            ICommonService service = KEEPER.K3._3D.Contracts.ServiceFactory.GetService<ICommonService>(ctx);
            service.updateAltableStatus(ctx, status, Obstatus, ids, uyList);
        }
        public static List<UpdateAltableinEntity> InstallUpdateAlPackage(Context ctx, UpdateAltableinEnum status, ObjectEnum Obstatus, DynamicObject[] trasferbill = null, IOperationResult vo = null, List<UpdateAltableinEntity> exceptPrtList = null, IOperationResult auditResult = null, DynamicObject submitResult = null, long k3cloudheadid = 0, string billno = "", string formid = "")
        {
            ICommonService service = KEEPER.K3._3D.Contracts.ServiceFactory.GetService<ICommonService>(ctx);
            return service.InstallUpdateAlPackage(ctx, status, Obstatus, trasferbill, vo, exceptPrtList, auditResult, submitResult, k3cloudheadid, billno, formid);
        }
        public static void insertAllocationtableTable(Context ctx, UpdateAltableinEnum status, ObjectEnum Obstatus)
        {
            ICommonService service = KEEPER.K3._3D.Contracts.ServiceFactory.GetService<ICommonService>(ctx);
            service.insertAllocationtableTable(ctx, status, Obstatus);
        }
        public static void updateDetableStatus(Context ctx, UpdatePrtableinEnum status, ObjectEnum Obstatus, long[] ids = null, List<UpdatePrtableEntity> uyList = null)
        {
            ICommonService service = KEEPER.K3._3D.Contracts.ServiceFactory.GetService<ICommonService>(ctx);
            service.updateDetableStatus(ctx, status, Obstatus, ids, uyList);
        }

        public static DynamicObject[] ConvertOutStockBills(Context ctx, List<ConvertOption> option, string SourceFormId, string TargetFormId, string SourceEntryEntityKey)
        {
            ICommonService service = KEEPER.K3._3D.Contracts.ServiceFactory.GetService<ICommonService>(ctx);
            return service.ConvertOutStockBills(ctx, option, SourceFormId, TargetFormId, SourceEntryEntityKey);
        }
        public static List<UpdatePrtableEntity> InstallUpdateDePackage(Context ctx, UpdatePrtableinEnum status, ObjectEnum Obstatus, DynamicObject[] trasferbill = null, List<ValidationErrorInfo> vo = null, List<UpdatePrtableEntity> exceptPrtList = null, IOperationResult auditResult = null, DynamicObject submitResult = null, long k3cloudheadid = 0, string billno = "", string formid = "")
        {
            ICommonService service = KEEPER.K3._3D.Contracts.ServiceFactory.GetService<ICommonService>(ctx);
            return service.InstallUpdateDePackage(ctx, status, Obstatus, trasferbill, vo, exceptPrtList, auditResult, submitResult, k3cloudheadid, billno, formid);
        }


    }
}
