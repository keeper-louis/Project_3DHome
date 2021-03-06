﻿using Kingdee.BOS.Contracts;
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

namespace KEN.K3._3D.Allocation.SAVE.ScheduleServicePlugIn
{
    [Description("调拨接口生成调拨单执行计划")]
    public class AllocationSAVE : IScheduleService
    {
        //private List<SalOrder2DirectTransList> purTransferData;
        private SalOrder2DirectTransList transferData;
        public void Run(Context ctx, Schedule schedule)
        {
            ctx.UserId = 101901;
            ctx.UserName = "扫码专用账户";
            if (_3DServiceHelper.isTransfer(ctx, ObjectEnum.AlTransfer, UpdateAltableinEnum.BeforeSave))
            {
                transferData = _3DServiceHelper.getALSaveData(ctx, UpdateAltableinEnum.BeforeSave, ObjectEnum.AlTransfer);//本次执行计划处理的数据
                long[] bb = transferData.SalOrder2DirectTrans.Select(s => s.altID).ToArray();//本次执行计划要处理的数据的主键ID
                //根据主键对数据标识进行更新，同时设置更新时间戳
                _3DServiceHelper.updateAltableStatus(ctx, UpdateAltableinEnum.AfterGetDate, ObjectEnum.AlTransfer, bb);

                List<DynamicObject> modelList = new List<DynamicObject>();
                Action<IDynamicFormViewService> fillBillPropertys = new Action<IDynamicFormViewService>(fillPropertys);
                DynamicObject billModel = _3DServiceHelper.CreateBillMode(ctx, "STK_TransferDirect", fillBillPropertys);
                modelList.Add(billModel);

                DynamicObject[] model = modelList.Select(p => p).ToArray() as DynamicObject[];
                IOperationResult saveResult = _3DServiceHelper.BatchSave(ctx, "STK_TransferDirect", model);
                String id = Convert.ToString(model[0]["id"]);
                //对结果进行处理
                //数据包整理完成后，将实际的分录主键反写回altablein表，便于未来反写其他状态所用
                List<UpdateAltableinEntity> updateAltList = _3DServiceHelper.InstallUpdateAlPackage(ctx, UpdateAltableinEnum.AfterCreateModel, ObjectEnum.AlTransfer, model, null, null, null, null, 0, "", "TransferDirectEntry");
                if (updateAltList != null)
                {
                    _3DServiceHelper.updateAltableStatus(ctx, UpdateAltableinEnum.AfterCreateModel, ObjectEnum.AlTransfer, null, updateAltList);
                }
                //处理保存结果，成功、失败
                if (saveResult.SuccessDataEnity != null)
                {
                    object[] ids = (from c in saveResult.SuccessDataEnity
                                    select c[0]).ToArray();//保存成功的结果
                    IOperationResult submitResult = ids.Count() > 0 ? _3DServiceHelper.Submit(ctx, "STK_TransferDirect", ids) : null;
                    if (submitResult.SuccessDataEnity != null)
                    {
                        List<UpdateAltableinEntity> exceptPrtList = new List<UpdateAltableinEntity>();
                        List<UpdateAltableinEntity> successPrtList = new List<UpdateAltableinEntity>();
                        foreach (DynamicObject item in submitResult.SuccessDataEnity)
                        {
                            object[] ips = new object[] { item[0] };
                            IOperationResult auditResult = _3DServiceHelper.Audit(ctx, "STK_TransferDirect", ips);
                            if (auditResult.IsSuccess)
                            {
                                successPrtList = _3DServiceHelper.InstallUpdateAlPackage(ctx, UpdateAltableinEnum.AuditSucess, ObjectEnum.AlTransfer, null, null, successPrtList, auditResult, null);
                                //更新altablein表审核成功信息
                                _3DServiceHelper.updateAltableStatus(ctx, UpdateAltableinEnum.AuditSucess, ObjectEnum.AlTransfer, null, successPrtList);
                            }
                            else if (!auditResult.InteractionContext.SimpleMessage.Equals("") && auditResult.InteractionContext.SimpleMessage != null)
                            {
                                exceptPrtList = _3DServiceHelper.InstallUpdateAlPackage(ctx, UpdateAltableinEnum.AuditError, ObjectEnum.AlTransfer, null, null, exceptPrtList, auditResult, item);
                                //更新altablein表 审核错误信息
                                _3DServiceHelper.updateAltableStatus(ctx, UpdateAltableinEnum.AuditError, ObjectEnum.AlTransfer, null, exceptPrtList);

                                //插入审核错误信息进入错误信息表
                                _3DServiceHelper.insertAllocationtableTable(ctx, UpdateAltableinEnum.AuditError, ObjectEnum.AlTransfer,id);
                            }
                        }

                    }
                }
                if (((List<ValidationErrorInfo>)saveResult.ValidationErrors).Count() > 0)
                {
                    List<UpdateAltableinEntity> updateVoSavePrtList = _3DServiceHelper.InstallUpdateAlPackage(ctx, UpdateAltableinEnum.SaveError, ObjectEnum.AlTransfer, null, saveResult);
                    //更新altablein表状态
                    _3DServiceHelper.updateAltableStatus(ctx, UpdateAltableinEnum.SaveError, ObjectEnum.AlTransfer, bb, updateVoSavePrtList);

                    //插入保存错误信息进入错误信息表
                    _3DServiceHelper.insertAllocationtableTable(ctx, UpdateAltableinEnum.SaveError, ObjectEnum.AlTransfer,id);

                }

            }

        }
        private void fillPropertys(IDynamicFormViewService dynamicFormView)
        {
            //((IDynamicFormView)dynamicFormView).InvokeFieldUpdateService("FSTAFFNUMBER", 0);//SetItemValueByNumber不会触发值更新事件，需要继续调用该函数
            //调出库存组织，默认组织编码[102.01]
            dynamicFormView.SetItemValueByNumber("FStockOutOrgId", "101.01", 0);
            //调入库存组织，默认组织编码[102.01]
            dynamicFormView.SetItemValueByNumber("FStockOrgId", "101.01", 0);
            //日期，Convert.ToDateTime("2018-9-27");
            dynamicFormView.UpdateValue("FDate", 0, transferData.BusinessDate);
            //备注
            dynamicFormView.UpdateValue("FNote", 0, "KEN");

            //分录
            //物料
            int num = transferData.SalOrder2DirectTrans.Count();
            List<SalOrder2DirectTrans> a = transferData.SalOrder2DirectTrans.ToList();
            //新增分录
            //((IBillView)dynamicFormView).Model.CreateNewEntryRow("FBillEntry");
            //如果预知有多条分录，可以使用这个方法进行批量新增
            ((IBillView)dynamicFormView).Model.BatchCreateNewEntryRow("FBillEntry", num);
            //((IBillView)dynamicFormView).Model.BatchCreateNewEntryRow("FBillEntry", num);
            for (int i = 0; i < a.Count; i++)
            {
                dynamicFormView.SetItemValueByID("FMaterialId", a[i].MATERIALID, i);
                //dynamicFormView.SetItemValueByNumber("FMaterialId", "01.06.0004", 0);
                //辅助资料
                if (a[i].AUXPROPID != 0)
                {
                    dynamicFormView.SetItemValueByID("FAuxPropId", a[i].AUXPROPID, i);
                }
                //调拨数量
                dynamicFormView.UpdateValue("FQty", i, a[i].amount);
                //调出仓库
                dynamicFormView.SetItemValueByNumber("FSrcStockId", a[i].stocknumberout, i);
                //调入仓库
                dynamicFormView.SetItemValueByNumber("FDestStockId", a[i].stocknumberin, i);
                //销售订单号
                dynamicFormView.UpdateValue("Fsalenumber", i, a[i].saleNumber);
                //销售订单行号
                dynamicFormView.UpdateValue("Flinenumber", i, a[i].lineNumber);
                //包装码
                dynamicFormView.UpdateValue("Fpackcode", i, a[i].packcode);
                //altablein 内码
                dynamicFormView.UpdateValue("Faltableinid", i, a[i].altID);
                //BOMID
                if (!a[i].Fbomid.Equals("") && a[i].FISENABLE.Equals("1"))
                {
                    dynamicFormView.SetItemValueByID("FBomId", a[i].Fbomid, i);
                }
                //批号
                if (a[i].Lot != 0)
                {
                    dynamicFormView.SetItemValueByID("FLot", a[i].Lot, i);
                }

                // PMS 型号
                dynamicFormView.UpdateValue("F_PAEZ_PMSMODEL", i, a[i].PMSModel);
                // A 值
                dynamicFormView.UpdateValue("F_PAEZ_DESCA", i, a[i].descA);
                // B 值
                dynamicFormView.UpdateValue("F_PAEZ_DESCB", i, a[i].descB);
                // C 值
                dynamicFormView.UpdateValue("F_PAEZ_DESCC", i, a[i].descC);
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
