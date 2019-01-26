using Kingdee.BOS.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kingdee.BOS;
using Kingdee.BOS.Core;
using System.ComponentModel;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Orm.DataEntity;
using KEEPER.K3._3D._3DServiceHelper;

namespace KEEPER.K3._3D.PURTRANSFER.ScheduleServicePlugIn
{
    [Description("采购调拨执行计划")]
    public class PURTRANSFER : IScheduleService
    {
        public void Run(Context ctx, Schedule schedule)
        {
            Action<IDynamicFormViewService> fillBillPropertys = new Action<IDynamicFormViewService>(fillPropertys);
            DynamicObject billModel = _3DServiceHelper._3DServiceHelper.CreateBillMode(ctx, "STK_TransferDirect", fillBillPropertys);
        }


        private void fillPropertys(IDynamicFormViewService dynamicFormView)
        {
            //((IDynamicFormView)dynamicFormView).InvokeFieldUpdateService("FSTAFFNUMBER", 0);//SetItemValueByNumber不会触发值更新事件，需要继续调用该函数

            //调出库存组织，默认组织编码[102.01]
            dynamicFormView.SetItemValueByNumber("FStockOutOrgId","102.01",0);
            //调入库存组织，默认组织编码[102.01]
            dynamicFormView.SetItemValueByNumber("FStockOrgId", "102.01", 0);
            //日期，Convert.ToDateTime("2018-9-27");
            dynamicFormView.UpdateValue("FDate", 0,Convert.ToDateTime("2018-9-27"));
            //备注
            dynamicFormView.UpdateValue("FNote", 0, "KEEPER");

            //分录
            //物料
            dynamicFormView.SetItemValueByNumber("FMaterialId", "01.06.0004", 0);
            //辅助资料
            dynamicFormView.SetItemValueByID("FAuxPropId", 100004, 0);
            //调拨数量
            dynamicFormView.UpdateValue("FQty", 0,10);
            //调出仓库
            dynamicFormView.SetItemValueByNumber("FSrcStockId","2.02.02",0);
            //调入仓库
            dynamicFormView.SetItemValueByNumber("FDestStockId", "8",0);
            
            //新增分录
            //((IBillView)dynamicFormView).Model.CreateNewEntryRow("FEntity");
            //如果预知有多条分录，可以使用这个方法进行批量新增
            //((IBillView)dynamicFormView).Model.BatchCreateNewEntryRow("FEntity",100);
            //dynamicFormView.SetItemValueByNumber("FExpenseItemID", "CI001", 1);
            //申请金额：固定值：10000
            //dynamicFormView.UpdateValue("FOrgAmount", 1, 20000);
        }

    }
}
