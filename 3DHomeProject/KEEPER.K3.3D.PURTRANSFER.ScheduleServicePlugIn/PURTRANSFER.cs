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
using KEEPER.K3._3D.Core.Entity;
using Kingdee.BOS.Core.Bill;

namespace KEEPER.K3._3D.PURTRANSFER.ScheduleServicePlugIn
{
    [Description("采购调拨执行计划")]
    public class PURTRANSFER : IScheduleService
    {
        private List<SalOrderTransferList> purTransferData;
        private SalOrderTransferList transferData;
        public void Run(Context ctx, Schedule schedule)
        {
            if (_3DServiceHelper._3DServiceHelper.isTransfer(ctx))
            {
                purTransferData = _3DServiceHelper._3DServiceHelper.getPurTransferData(ctx);
                purTransferData.Select(p => p.salOrderTransfer.Select(s => s.MATERIALID)).ToList();
                List<DynamicObject> modelList = new List<DynamicObject>();
                foreach (var item in purTransferData)
                {
                    transferData = item;
                    Action<IDynamicFormViewService> fillBillPropertys = new Action<IDynamicFormViewService>(fillPropertys);
                    DynamicObject billModel = _3DServiceHelper._3DServiceHelper.CreateBillMode(ctx, "STK_TransferDirect", fillBillPropertys);
                    modelList.Add(billModel);
                }

                DynamicObject[] model = modelList.Select(p => p).ToArray() as DynamicObject[];
                IOperationResult saveResult = _3DServiceHelper._3DServiceHelper.BatchSave(ctx, "STK_TransferDirect", model);
            }

        }


        private void fillPropertys(IDynamicFormViewService dynamicFormView)
        {
            //((IDynamicFormView)dynamicFormView).InvokeFieldUpdateService("FSTAFFNUMBER", 0);//SetItemValueByNumber不会触发值更新事件，需要继续调用该函数
            //调出库存组织，默认组织编码[102.01]
            dynamicFormView.SetItemValueByNumber("FStockOutOrgId","102.01",0);
            //调入库存组织，默认组织编码[102.01]
            dynamicFormView.SetItemValueByNumber("FStockOrgId", "102.01", 0);
            //日期，Convert.ToDateTime("2018-9-27");
            dynamicFormView.UpdateValue("FDate", 0, transferData.BusinessDate);
            //备注
            dynamicFormView.UpdateValue("FNote", 0, "KEEPER");

            //分录
            //物料
            int num = transferData.salOrderTransfer.Count();
            List<SalOrderTransfer> a = transferData.salOrderTransfer.ToList();
            //新增分录
            //((IBillView)dynamicFormView).Model.CreateNewEntryRow("FBillEntry");
            //如果预知有多条分录，可以使用这个方法进行批量新增
            ((IBillView)dynamicFormView).Model.BatchCreateNewEntryRow("FBillEntry", num-1);
            for (int i = 0; i < a.Count; i++)
            {
                dynamicFormView.SetItemValueByID("FMaterialId", a[i].MATERIALID, i);
                //dynamicFormView.SetItemValueByNumber("FMaterialId", "01.06.0004", 0);
                //辅助资料
                if (a[i].AUXPROPID!=0)
                {
                    dynamicFormView.SetItemValueByID("FAuxPropId", a[i].AUXPROPID, i);
                }
                //调拨数量
                dynamicFormView.UpdateValue("FQty", i, a[i].amount);
                //调出仓库
                dynamicFormView.SetItemValueByNumber("FSrcStockId", a[i].stocknumber, i);
                //调入仓库
                dynamicFormView.SetItemValueByNumber("FDestStockId", "8", i);
                //批号
                if (a[i].Lot!=0)
                {
                    dynamicFormView.SetItemValueByID("FLot", a[i].Lot, i);
                }
            }
            
            
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
