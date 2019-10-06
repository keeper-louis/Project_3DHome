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
using Kingdee.BOS.App.Data;

namespace KEN.K3._3D.Allocation.SAVE.ScheduleServicePlugIn
{
    [Description("调拨接口生成入库单执行计划")]
    public class InStockSAVE : IScheduleService
    {
        //private List<SalOrder2DirectTransList> purTransferData;
        private SalOrder2DirectTransList transferData;
        public void Run(Context ctx, Schedule schedule)
        {
            ctx.UserId = 101901;
            ctx.UserName = "扫码专用账户";

            string strDateSql = string.Format(@"/*dialect*/select top 1 fdate from altablein where status=3 and isPur =0 and ferrorstatus=0 order by fdate");
            DynamicObjectCollection dateData = DBUtils.ExecuteDynamicObject(ctx, strDateSql, null);
            if (dateData.Count() > 0)
            {
                //添加按照车间分组取不同的车间
                string strproducesql = string.Format(@"/*dialect*/select   distinct  tbm.FWORKSHOPID  FWORKSHOPID
from  altablein   alin
left join T_SAL_ORDER tso on alin.salenumber=tso.FBILLNO 
left join T_SAL_ORDERENTRY tsoe on tso.fid=tsoe.FID and alin.Linenumber=tsoe.FSEQ
left join t_BD_MaterialBase tbmb on  tbmb.FMATERIALID=tsoe.FMATERIALID
left join  t_BD_MaterialProduce tbm on   tbm.FMATERIALID=tbmb.FMATERIALID
where    status=3  and tbm.FWORKSHOPID>0   and tbm.FWORKSHOPID>0    and isPur =0 and ferrorstatus=0  and  alin.fdate='{0}'", Convert.ToDateTime(dateData[0]["fdate"]));
                DynamicObjectCollection workshopDataObject = DBUtils.ExecuteDynamicObject(ctx, strproducesql, null);
                foreach (DynamicObject itemworkshopDataObject in workshopDataObject)
                {
                    long workshopid = Convert.ToInt64(itemworkshopDataObject["FWORKSHOPID"]);//取车间ID




                    if (_3DServiceHelper.isTransfer(ctx, ObjectEnum.AlInStock, UpdateAltableinEnum.BeforeSave))
                    {
                        //调用新的根据生产车间ID
                        transferData = _3DServiceHelper.getALInstockSaveData(ctx, UpdateAltableinEnum.BeforeSave, ObjectEnum.AlInStock, workshopid);//本次执行计划处理的数据
                        long[] bb = transferData.SalOrder2DirectTrans.Select(s => s.altID).ToArray();//本次执行计划要处理的数据的主键ID
                                                                                                     //根据主键对数据标识进行更新，同时设置更新时间戳
                        _3DServiceHelper.updateAltableStatus(ctx, UpdateAltableinEnum.AfterGetDate, ObjectEnum.AlInStock, bb);

                        List<DynamicObject> modelList = new List<DynamicObject>();
                        Action<IDynamicFormViewService> fillBillPropertys = new Action<IDynamicFormViewService>(fillPropertys);
                        DynamicObject billModel = _3DServiceHelper.CreateBillMode(ctx, "SP_InStock", fillBillPropertys);
                        modelList.Add(billModel);

                        DynamicObject[] model = modelList.Select(p => p).ToArray() as DynamicObject[];
                        IOperationResult saveResult = _3DServiceHelper.BatchSave(ctx, "SP_InStock", model);
                        String id = Convert.ToString(model[0]["id"]);
                        //对结果进行处理
                        //数据包整理完成后，将实际的分录主键反写回altablein表，便于未来反写其他状态所用
                        List<UpdateAltableinEntity> updateAltList = _3DServiceHelper.InstallUpdateAlPackage(ctx, UpdateAltableinEnum.AfterCreateModel, ObjectEnum.AlInStock, model, null, null, null, null, 0, "", "Entity");
                        if (updateAltList != null)
                        {
                            _3DServiceHelper.updateAltableStatus(ctx, UpdateAltableinEnum.AfterCreateModel, ObjectEnum.AlInStock, null, updateAltList);
                        }
                        //处理保存结果，成功、失败
                        if (saveResult.SuccessDataEnity != null)
                        {
                            object[] ids = (from c in saveResult.SuccessDataEnity
                                            select c[0]).ToArray();//保存成功的结果
                            IOperationResult submitResult = ids.Count() > 0 ? _3DServiceHelper.Submit(ctx, "SP_InStock", ids) : null;
                            if (submitResult.SuccessDataEnity != null)
                            {
                                List<UpdateAltableinEntity> exceptPrtList = new List<UpdateAltableinEntity>();
                                List<UpdateAltableinEntity> successPrtList = new List<UpdateAltableinEntity>();
                                foreach (DynamicObject item in submitResult.SuccessDataEnity)
                                {
                                    object[] ips = new object[] { item[0] };
                                    IOperationResult auditResult = _3DServiceHelper.Audit(ctx, "SP_InStock", ips);
                                    if (auditResult.IsSuccess)
                                    {
                                        successPrtList = _3DServiceHelper.InstallUpdateAlPackage(ctx, UpdateAltableinEnum.AuditSucess, ObjectEnum.AlInStock, null, null, successPrtList, auditResult, null);
                                        //更新altablein表审核成功信息
                                        _3DServiceHelper.updateAltableStatus(ctx, UpdateAltableinEnum.AuditSucess, ObjectEnum.AlInStock, null, successPrtList);
                                    }
                                    else if (!auditResult.InteractionContext.SimpleMessage.Equals("") && auditResult.InteractionContext.SimpleMessage != null)
                                    {
                                        exceptPrtList = _3DServiceHelper.InstallUpdateAlPackage(ctx, UpdateAltableinEnum.AuditError, ObjectEnum.AlInStock, null, null, exceptPrtList, auditResult, item);
                                        //更新altablein表 审核错误信息
                                        _3DServiceHelper.updateAltableStatus(ctx, UpdateAltableinEnum.AuditError, ObjectEnum.AlInStock, null, exceptPrtList);

                                        //插入审核错误信息进入错误信息表
                                        _3DServiceHelper.insertAllocationtableTable(ctx, UpdateAltableinEnum.AuditError, ObjectEnum.AlInStock, id);
                                    }
                                }

                            }
                        }
                        if (((List<ValidationErrorInfo>)saveResult.ValidationErrors).Count() > 0)
                        {
                            List<UpdateAltableinEntity> updateVoSavePrtList = _3DServiceHelper.InstallUpdateAlPackage(ctx, UpdateAltableinEnum.SaveError, ObjectEnum.AlInStock, null, saveResult);
                            //更新altablein表状态
                            _3DServiceHelper.updateAltableStatus(ctx, UpdateAltableinEnum.SaveError, ObjectEnum.AlInStock, bb, updateVoSavePrtList);

                            //插入保存错误信息进入错误信息表
                            _3DServiceHelper.insertAllocationtableTable(ctx, UpdateAltableinEnum.SaveError, ObjectEnum.AlInStock, id);

                        }

                    }
                }
            }

        }
        private void fillPropertys(IDynamicFormViewService dynamicFormView)
        {
            //((IDynamicFormView)dynamicFormView).InvokeFieldUpdateService("FSTAFFNUMBER", 0);//SetItemValueByNumber不会触发值更新事件，需要继续调用该函数
            //调出库存组织，默认组织编码[102.01]
            //dynamicFormView.SetItemValueByNumber("FStockOutOrgId", "102.01", 0);
            //调入库存组织，默认组织编码[102.01]
            dynamicFormView.SetItemValueByNumber("FStockOrgId", "101.01", 0);
            //日期，Convert.ToDateTime("2018-9-27");
            dynamicFormView.UpdateValue("FDate", 0, transferData.BusinessDate);
            //备注
            dynamicFormView.UpdateValue("FDescription", 0, "KEN");

            //分录
            //物料
            int num = transferData.SalOrder2DirectTrans.Count();
            List<SalOrder2DirectTrans> a = transferData.SalOrder2DirectTrans.ToList();
            //新增分录
            //((IBillView)dynamicFormView).Model.CreateNewEntryRow("FBillEntry");
            //如果预知有多条分录，可以使用这个方法进行批量新增
            ((IBillView)dynamicFormView).Model.BatchCreateNewEntryRow("FEntity", num);
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
                //入库数量
                dynamicFormView.UpdateValue("FRealQty", i, a[i].amount);
                //入库仓库
                dynamicFormView.SetItemValueByNumber("FStockId", a[i].stocknumberout, i);
                //销售订单号
                dynamicFormView.UpdateValue("Fsalenumber", i, a[i].saleNumber);
                //销售订单行号
                dynamicFormView.UpdateValue("Flinenumber", i, a[i].lineNumber);
                //包装码
                dynamicFormView.UpdateValue("Fpackcode", i, a[i].packcode);
                //BOMID
                if (!a[i].Fbomid.Equals("")&& a[i].FISENABLE.Equals("1"))
                {
                    dynamicFormView.SetItemValueByID("FBomId",a[i].Fbomid,i);
                }

                //altablein 内码
                dynamicFormView.UpdateValue("Faltableinid", i, a[i].altID);
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
