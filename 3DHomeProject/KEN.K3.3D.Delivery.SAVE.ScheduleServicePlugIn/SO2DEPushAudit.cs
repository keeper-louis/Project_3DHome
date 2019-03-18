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
using KEEPER.K3._3D._3DServiceHelper;
using KEEPER.K3._3D.Core.ParamOption;
using KEEPER.K3._3D.Core.Entity;

namespace KEN.K3._3D.Delivery.PUSH.ScheduleServicePlugIn
{
    [Description("生产入库单审核执行计划")]
    public class SO2DEPushAudit : IScheduleService
    {
        public void Run(Context ctx, Schedule schedule)
        {
            ctx.UserId = 214076;
            ctx.UserName = "扫码专用账户";
            //判断需要审核的单据
            if (_3DServiceHelper.isTransfer(ctx, ObjectEnum.SO2DE, UpdatePrtableinEnum.Audit))
            {
                //获取需要审核的单据cloudheadid
                string strSql = string.Format(@"/*dialect*/select distinct top 100  fcloudheadid from detablein where status = 4");
                DynamicObjectCollection auditCol = DBUtils.ExecuteDynamicObject(ctx, strSql);
                object[] ips = (from p in auditCol
                                select p["fcloudheadid"]).ToArray();
                //审核
                IOperationResult auditResult = _3DServiceHelper.Audit(ctx, "SAL_OUTSTOCK", ips);
                //处理审核结果
                //审核成功
                if (auditResult.SuccessDataEnity != null && auditResult.SuccessDataEnity.Count() > 0)
                {
                    //正确的更新prtablein表
                    List<UpdatePrtableEntity> uList = _3DServiceHelper.InstallUpdateDePackage(ctx,UpdatePrtableinEnum.AuditSucess,ObjectEnum.SO2DE, null, null, null, auditResult, null);
                    _3DServiceHelper.updateDetableStatus(ctx,UpdatePrtableinEnum.AuditSucess,ObjectEnum.SO2DE, null, uList);
                }
                //审核失败
                if (((List<ValidationErrorInfo>)auditResult.ValidationErrors).Count() > 0)
                {
                    //错误的更新prtablein表

                    List<UpdatePrtableEntity> uList = _3DServiceHelper.InstallUpdateDePackage(ctx,UpdatePrtableinEnum.AuditError,ObjectEnum.SO2DE, null, (List<ValidationErrorInfo>)auditResult.ValidationErrors);
                    _3DServiceHelper.updateDetableStatus(ctx,UpdatePrtableinEnum.AuditError,ObjectEnum.SO2DE, null, uList);
                    //错误的更新processtable表
                    _3DServiceHelper.insertErrorTable(ctx,UpdatePrtableinEnum.AuditError,ObjectEnum.SO2DE);
                }



            }
        }
    }
}
