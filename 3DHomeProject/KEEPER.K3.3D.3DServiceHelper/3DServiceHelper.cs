using KEEPER.K3._3D.Contracts;
using KEEPER.K3._3D.Core.Entity;
using KEEPER.K3._3D.Core.ParamOption;
using Kingdee.BOS;
using Kingdee.BOS.Contracts;
using Kingdee.BOS.Core.DynamicForm;
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

        public static void updateTableStatus(Context ctx, long[] ids, int status)
        {
            ICommonService service = KEEPER.K3._3D.Contracts.ServiceFactory.GetService<ICommonService>(ctx);
            service.updateTableStatus(ctx, ids, status);
        }

    }
}
