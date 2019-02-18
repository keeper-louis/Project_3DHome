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

namespace KEEPER.K3._3D.ScheduleServicePlugIn
{
    [Description("自动下推汇报执行计划demo")]
    public class ConvertDemo : IScheduleService
    {
        public void Run(Context ctx, Schedule schedule)
        {
            ctx.UserId = 214076;
            ctx.UserName = "扫码专用账户";
            ConvertOption option = new ConvertOption();
            option.SourceFormId = "SFC_OperationPlanning";
            option.TargetFormId = "SFC_OperationReport";
            List<long> sourceBillIds = new List<long>();
            //sourceBillIds.Add(1016279)*;
            sourceBillIds.Add(1015824);
            option.SourceBillIds = sourceBillIds;
            List<long> sourceBillEntryIds = new List<long>();
            //sourceBillEntryIds.Add(4135730)*;
            //sourceBillEntryIds.Add(4135731)*;
            //sourceBillEntryIds.Add(4135732)*;
            //sourceBillEntryIds.Add(4135733)*;
            //sourceBillEntryIds.Add(4135734)*;
            //sourceBillEntryIds.Add(4135735)*;
            //sourceBillEntryIds.Add(4135736)*;
            //sourceBillEntryIds.Add(4135737)*;
            //sourceBillEntryIds.Add(4135738)*;
            sourceBillEntryIds.Add(4132133);
            sourceBillEntryIds.Add(4132134);
            sourceBillEntryIds.Add(4132136);
            option.SourceBillEntryIds = sourceBillEntryIds;
            option.SourceEntryEntityKey = "FSubEntity";
            //_3DServiceHelper._3DServiceHelper.ConvertBills(ctx, option);
        }
    }
}
