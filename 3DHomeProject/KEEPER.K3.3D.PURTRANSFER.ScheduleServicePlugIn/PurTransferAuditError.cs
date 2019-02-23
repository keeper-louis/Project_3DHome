using Kingdee.BOS.Contracts;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kingdee.BOS;
using Kingdee.BOS.Core;
using KEEPER.K3._3D.Core.ParamOption;
using KEEPER.K3._3D.Core.Entity;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.Validation;

namespace KEEPER.K3._3D.PURTRANSFER.ScheduleServicePlugIn
{
    [Description("采购调拨审核失败重启计划")]
    //没测试。。。。。。。
    public class PurTransferAuditError : IScheduleService
    {

        private List<UpdatePrtableEntity> purTransferData;
        public void Run(Context ctx, Schedule schedule)
        {
            if (_3DServiceHelper._3DServiceHelper.isTransfer(ctx,ObjectEnum.PurTransfer,UpdatePrtableinEnum.AuditError))
            {
                //获取审核失败数据集合
                purTransferData = _3DServiceHelper._3DServiceHelper.getAuditErrorData(ctx, UpdatePrtableinEnum.AuditError);
                long[] ids = purTransferData.Select(s => s.k3cloudheadID).ToArray();//本次需要再次重新审核的主键集合
                List<UpdatePrtableEntity> exceptPrtList = new List<UpdatePrtableEntity>();
                List<UpdatePrtableEntity> successPrtList = new List<UpdatePrtableEntity>();
                foreach (UpdatePrtableEntity item in purTransferData)
                {
                    object[] ips = new object[] { item.k3cloudheadID };
                    IOperationResult auditResult = _3DServiceHelper._3DServiceHelper.Audit(ctx, "STK_TransferDirect", ips);
                    if (auditResult.IsSuccess)
                    {
                        successPrtList = _3DServiceHelper._3DServiceHelper.InstallUpdatePackage(ctx, UpdatePrtableinEnum.AuditSucess,ObjectEnum.PurTransfer, null, null, successPrtList, auditResult, null);
                    }
                    else if (((List<ValidationErrorInfo>)auditResult.ValidationErrors).Count() > 0)
                    {

                    }
                    else if (!auditResult.InteractionContext.SimpleMessage.Equals("") && auditResult.InteractionContext.SimpleMessage != null)
                    {
                        exceptPrtList = _3DServiceHelper._3DServiceHelper.InstallUpdatePackage(ctx, UpdatePrtableinEnum.AuditError,ObjectEnum.PurTransfer, null, null, exceptPrtList, auditResult, null,item.k3cloudheadID,item.billNo);
                    }
                }
                //更新prtablein表 审核错误信息
                _3DServiceHelper._3DServiceHelper.updateTableStatus(ctx, UpdatePrtableinEnum.AuditError,ObjectEnum.PurTransfer, null, exceptPrtList);
                //更新prtablein表审核成功信息
                _3DServiceHelper._3DServiceHelper.updateTableStatus(ctx, UpdatePrtableinEnum.AuditSucess,ObjectEnum.PurTransfer, null, successPrtList);
                //插入审核错误信息进入错误信息表
                _3DServiceHelper._3DServiceHelper.insertErrorTable(ctx, UpdatePrtableinEnum.AuditError,ObjectEnum.PurTransfer);
            }
        }
    }
}
