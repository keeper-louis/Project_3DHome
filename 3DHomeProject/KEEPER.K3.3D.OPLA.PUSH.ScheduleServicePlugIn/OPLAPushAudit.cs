using Kingdee.BOS.Contracts;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kingdee.BOS;
using Kingdee.BOS.Core;
using Kingdee.BOS.App.Data;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.Validation;
using KEEPER.K3._3D.Core.Entity;

namespace KEEPER.K3._3D.OPLA.PUSH.ScheduleServicePlugIn
{
    [Description("合格品工序汇报提交成功的单据进行审核执行计划")]
    public class OPLAPushAudit : IScheduleService
    {
        public void Run(Context ctx, Schedule schedule)
        {
            ctx.UserId = 214076;
            ctx.UserName = "扫码专用账户";
            //判断需要审核的单据
            if (_3DServiceHelper._3DServiceHelper.isTransfer(ctx,Core.ParamOption.ObjectEnum.OPLAQual,Core.ParamOption.UpdatePrtableinEnum.Audit))
            {
                //获取需要审核的单据cloudheadid
                string strSql = string.Format(@"/*dialect*/select distinct fcloudheadid from prtablein where state = 0 and status = 5");
                DynamicObjectCollection auditCol = DBUtils.ExecuteDynamicObject(ctx, strSql);
                object[] ips = (from p in auditCol
                                select p["fcloudheadid"]).ToArray();
                //审核
                IOperationResult auditResult = _3DServiceHelper._3DServiceHelper.Audit(ctx, "SFC_OperationReport", ips);
                //处理审核结果
                //审核成功
                if (auditResult.SuccessDataEnity!=null&&auditResult.SuccessDataEnity.Count()>0)
                {
                    //正确的更新prtablein表
                    List<UpdatePrtableEntity> uList = _3DServiceHelper._3DServiceHelper.InstallUpdatePackage(ctx, Core.ParamOption.UpdatePrtableinEnum.AuditSucess, Core.ParamOption.ObjectEnum.OPLAQual, null, null, null, auditResult, null);
                    _3DServiceHelper._3DServiceHelper.updateTableStatus(ctx, Core.ParamOption.UpdatePrtableinEnum.AuditSucess, Core.ParamOption.ObjectEnum.OPLAQual, null, uList);
                }
                //审核失败
                if (((List<ValidationErrorInfo>)auditResult.ValidationErrors).Count() > 0)
                {
                    //错误的更新prtablein表
                   
                    List<UpdatePrtableEntity> uList = _3DServiceHelper._3DServiceHelper.InstallUpdatePackage(ctx, Core.ParamOption.UpdatePrtableinEnum.AuditError, Core.ParamOption.ObjectEnum.OPLAQual, null, (List<ValidationErrorInfo>)auditResult.ValidationErrors);
                    _3DServiceHelper._3DServiceHelper.updateTableStatus(ctx, Core.ParamOption.UpdatePrtableinEnum.AuditError, Core.ParamOption.ObjectEnum.OPLAQual, null, uList);
                    //错误的更新processtable表
                    _3DServiceHelper._3DServiceHelper.insertErrorTable(ctx, Core.ParamOption.UpdatePrtableinEnum.AuditError, Core.ParamOption.ObjectEnum.OPLAQual);
                }



            }
        }
    }

}

