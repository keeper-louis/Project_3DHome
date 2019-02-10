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
        public static DynamicObject[] ConvertBills(Context ctx, ConvertOption option)
        {
            ICommonService service = KEEPER.K3._3D.Contracts.ServiceFactory.GetService<ICommonService>(ctx);
            DynamicObject[] targetDatas = service.ConvertBills(ctx, option);
            return targetDatas;
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

        public static IOperationResult Save(Context ctx, string FormID, DynamicObject dyObject)
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

        public static Boolean isTransfer(Context ctx)
        {
            ICommonService service = KEEPER.K3._3D.Contracts.ServiceFactory.GetService<ICommonService>(ctx);
            return service.isTransfer(ctx);
        }

        public static List<SalOrderTransferList> getPurTransferData(Context ctx)
        {
            ICommonService service = KEEPER.K3._3D.Contracts.ServiceFactory.GetService<ICommonService>(ctx);
            return service.getPurTransferData(ctx);
        }

        public static void updateTableStatus(Context ctx, UpdatePrtableinEnum status, long[] ids = null,List<UpdatePrtableEntity> uyList = null)
        {
            ICommonService service = KEEPER.K3._3D.Contracts.ServiceFactory.GetService<ICommonService>(ctx);
            service.updateTableStatus(ctx, status, ids,uyList);
        }

        public static void insertErrorTable(Context ctx, UpdatePrtableinEnum status)
        {
            ICommonService service = KEEPER.K3._3D.Contracts.ServiceFactory.GetService<ICommonService>(ctx);
            service.insertErrorTable(ctx, status);
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

        public static List<UpdatePrtableEntity> InstallUpdatePackage(Context ctx, UpdatePrtableinEnum status, Object[] trasferentry = null, List<ValidationErrorInfo> vo = null)
        {
            ICommonService service = KEEPER.K3._3D.Contracts.ServiceFactory.GetService<ICommonService>(ctx);
            return service.InstallUpdatePackage(ctx, status, trasferentry, vo);
        }

    }
}
