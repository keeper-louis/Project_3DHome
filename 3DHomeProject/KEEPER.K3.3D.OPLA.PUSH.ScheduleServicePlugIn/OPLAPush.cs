using Kingdee.BOS.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kingdee.BOS;
using Kingdee.BOS.Core;
using System.ComponentModel;
using KEEPER.K3._3D.Core.ParamOption;
using KEEPER.K3._3D._3DServiceHelper;

namespace KEEPER.K3._3D.OPLA.PUSH.ScheduleServicePlugIn
{
    [Description("工序计划下推工序汇报执行计划")]
    public class OPLAPush : IScheduleService
    {
        public void Run(Context ctx, Schedule schedule)
        {
            ctx.UserId = 214076;
            ctx.UserName = "扫码专用账户";
            if (_3DServiceHelper._3DServiceHelper.isTransfer(ctx, ObjectEnum.OPLAQual, UpdatePrtableinEnum.BeforeSave))
            {
                List<ConvertOption>  options = _3DServiceHelper._3DServiceHelper.getOLAPushData(ctx, ObjectEnum.OPLAQual);
                if (options!=null)
                {
                    _3DServiceHelper._3DServiceHelper.ConvertBills(ctx, options, "SFC_OperationPlanning", "SFC_OperationReport", "FSubEntity");
                }
            }
        }
    }
}
