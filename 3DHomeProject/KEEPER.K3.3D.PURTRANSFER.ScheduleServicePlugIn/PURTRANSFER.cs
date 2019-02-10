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
using Kingdee.BOS.Core.Validation;
using KEEPER.K3._3D.Core.ParamOption;

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
                purTransferData = _3DServiceHelper._3DServiceHelper.getPurTransferData(ctx);//本次执行计划处理的数据
                //List<List<long>> aa = purTransferData.Select(p => p.salOrderTransfer.Select(s => s.MATERIALID).ToList()).ToList();
                long[] bb = purTransferData.SelectMany(t => t.salOrderTransfer.Select(s => s.prtID).ToList()).ToArray();//本次执行计划要处理的数据的主键ID
                //根据主键对数据标识进行更新，同时设置更新时间戳
                _3DServiceHelper._3DServiceHelper.updateTableStatus(ctx, UpdatePrtableinEnum.AfterGetDate,bb);
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
                //对结果进行处理
                //数据包整理完成后，将实际的分录主键反写回prtablein表，便于未来反写其他状态所用
                object[] trasferentry = (from p in model
                                          select p[77]).ToArray();
                List<UpdatePrtableEntity> updatePrtList = _3DServiceHelper._3DServiceHelper.InstallUpdatePackage(ctx, UpdatePrtableinEnum.AfterCreateModel, trasferentry, null);
                if (updatePrtList!=null)
                {
                    _3DServiceHelper._3DServiceHelper.updateTableStatus(ctx, UpdatePrtableinEnum.AfterCreateModel, null, updatePrtList);
                }
                //处理保存结果，成功、失败
                if (saveResult.SuccessDataEnity!=null)
                {
                    object[] ids = (from c in saveResult.SuccessDataEnity
                                    select c[0]).ToArray();//保存成功的结果
                    IOperationResult submitResult = ids.Count() > 0 ? _3DServiceHelper._3DServiceHelper.Submit(ctx, "STK_TransferDirect", ids) : null;
                    if (submitResult.SuccessDataEnity != null)
                    {
                        foreach (DynamicObject item in submitResult.SuccessDataEnity)
                        {
                            object[] ips = new object[] { item[0] };
                            IOperationResult auditResult = _3DServiceHelper._3DServiceHelper.Audit(ctx, "STK_TransferDirect", ips);
                        }
                        //object[] ips = (from c in submitResult.SuccessDataEnity
                        //                select c[0]).ToArray();
                        //IOperationResult auditResult = _3DServiceHelper._3DServiceHelper.Audit(ctx, "STK_TransferDirect", ips);
                        //审核成功的回写table c  update status = 接口处理完成状态
                        //if (auditResult.SuccessDataEnity!=null)
                        //{

                        //}
                        //审核失败回写table d insert 错误信息，回写table c 审核报错，将对应单号赋予每一个错误信息的条
                       // if (((List<ValidationErrorInfo>)auditResult.ValidationErrors).Count() > 0)
                       // {
                            //List<UpdatePrtableEntity> updateSavePrtList = _3DServiceHelper._3DServiceHelper.InstallUpdatePackage(ctx, UpdatePrtableinEnum.SaveError, null, (List<ValidationErrorInfo>)saveResult.ValidationErrors);
                            ////更新prtablein表状态
                            //_3DServiceHelper._3DServiceHelper.updateTableStatus(ctx, UpdatePrtableinEnum.SaveError, null, updateSavePrtList);
                            ////插入Processtable表信息
                            //_3DServiceHelper._3DServiceHelper.insertErrorTable(ctx, UpdatePrtableinEnum.SaveError);
                        //}
                    }
                }
                if (((List<ValidationErrorInfo>)saveResult.ValidationErrors).Count()>0)
                {
                    List<UpdatePrtableEntity> updateSavePrtList = _3DServiceHelper._3DServiceHelper.InstallUpdatePackage(ctx, UpdatePrtableinEnum.SaveError, null, (List<ValidationErrorInfo>)saveResult.ValidationErrors);
                    //更新prtablein表状态
                    _3DServiceHelper._3DServiceHelper.updateTableStatus(ctx, UpdatePrtableinEnum.SaveError, null, updateSavePrtList);
                    //插入Processtable表信息
                    _3DServiceHelper._3DServiceHelper.insertErrorTable(ctx, UpdatePrtableinEnum.SaveError);

                }
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
            //((IBillView)dynamicFormView).Model.BatchCreateNewEntryRow("FBillEntry", num);
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
                //销售订单号
                dynamicFormView.UpdateValue("Fsalenumber", i, a[i].saleNumber);
                //销售订单行号
                dynamicFormView.UpdateValue("Flinenumber", i, a[i].lineNumber);
                //工序号
                dynamicFormView.UpdateValue("Ftechcode", i, a[i].technicsCode);
                //prtablein 内码
                dynamicFormView.UpdateValue("Fprtableinid", i, a[i].prtID);
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
