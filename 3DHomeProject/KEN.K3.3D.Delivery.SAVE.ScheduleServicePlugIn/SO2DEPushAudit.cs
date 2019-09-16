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
            ctx.UserId = 101901;
            ctx.UserName = "扫码专用账户";
            //判断需要审核的单据
            if (_3DServiceHelper.isTransfer(ctx, ObjectEnum.SO2DE, UpdatePrtableinEnum.Audit))
            {

                    //获取需要审核的单据cloudheadid
                    string strSql = string.Format(@"/*dialect*/select distinct top 10  fcloudheadid from detablein where status = 4");
                    DynamicObjectCollection auditCol = DBUtils.ExecuteDynamicObject(ctx, strSql);
                    object[] ips = (from p in auditCol
                                    select p["fcloudheadid"]).ToArray();
                    List<UpdatePrtableEntity> exceptPrtList = new List<UpdatePrtableEntity>();
                    List<UpdatePrtableEntity> successPrtList = new List<UpdatePrtableEntity>();
                    foreach (int item in ips)
                    {
                        object[] id = new object[1] { item };
                        IOperationResult auditResult = _3DServiceHelper.Audit(ctx, "SAL_OUTSTOCK", id);
                    try
                    {
                        if (auditResult.IsSuccess)
                        {
                            //更新altablein表审核成功信息
                            successPrtList = _3DServiceHelper.InstallUpdateDePackage(ctx, UpdatePrtableinEnum.AuditSucess, ObjectEnum.SO2DE, null, null, successPrtList, auditResult, null);
                            _3DServiceHelper.updateDetableStatus(ctx, UpdatePrtableinEnum.AuditSucess, ObjectEnum.SO2DE, null, successPrtList);

                        }
                        else if (!auditResult.InteractionContext.SimpleMessage.Equals("") && auditResult.InteractionContext.SimpleMessage != null)
                        {

                            //更新altablein表 审核错误信息
                            exceptPrtList = _3DServiceHelper.InstallUpdateDePackage(ctx, UpdatePrtableinEnum.AuditError, ObjectEnum.SO2DE, null, null, exceptPrtList, auditResult, null, Convert.ToInt64(item));
                            _3DServiceHelper.updateDetableStatus(ctx, UpdatePrtableinEnum.AuditError, ObjectEnum.SO2DE, null, exceptPrtList);
                            //插入审核错误信息进入错误信息表
                            _3DServiceHelper.insertErrorTable(ctx, UpdatePrtableinEnum.AuditError, ObjectEnum.SO2DE, Convert.ToString(id[0]));
                        }
                    }
                catch (Exception)
                {
                        string dropSql = string.Format(@"/*dialect*/  update detablein set status = 2, 
ferrormsg = '更新库存出现异常情况，更新库存不成功！', FErrorStatus = 2, Fsubdate = GETDATE()  
where fcloudheadid = {0}", id);
                        DBUtils.Execute(ctx, dropSql);

                        // C表状态为2，查不到报错信息。
                        string insertql = string.Format(@"/*dialect*/  insert into Deliverytable
 select id FBILLNO,'A' FDOCUMENTSTATUS, detablein.salenumber SALENUMBER, detablein.linenumber LINENUMBER, id PRTABLEINID,
'更新库存出现异常情况，更新库存不成功！' REASON,fdate FDATE, getdate() FSUBDATE,'' from detablein    
   where fcloudheadid = {0}", id);
                        DBUtils.Execute(ctx, insertql);


                    }
                }



            }
        }
    }
}
