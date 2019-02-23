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
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Orm.DataEntity;
using KEEPER.K3._3D.Core.Entity;
using Kingdee.BOS.Core.Validation;

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
                long[] bb = options.SelectMany(x => x.prtInId).ToList().ToArray();//本次执行计划要处理的数据的主键ID
                //根据主键对数据标识进行更新，同时设置更新时间戳
                _3DServiceHelper._3DServiceHelper.updateTableStatus(ctx, UpdatePrtableinEnum.AfterGetDate,ObjectEnum.OPLAQual, bb);
                if (options!=null&&options.Count()>0)
                {
                    DynamicObject[] ConvertCol = _3DServiceHelper._3DServiceHelper.ConvertBills(ctx, options, "SFC_OperationPlanning", "SFC_OperationReport", "FSubEntity");
                    if (ConvertCol!=null&&ConvertCol.Count()>0)
                    {
                        IOperationResult saveResult = _3DServiceHelper._3DServiceHelper.Save(ctx, "SFC_OperationReport", ConvertCol);
                        //对结果进行处理
                        //数据包整理完成后，将实际的分录主键反写回prtablein表，便于未来反写其他状态所用
                        //object[] trasferentry = (from p in model
                        //                          select p[77]).ToArray();
                        List<UpdatePrtableEntity> updatePrtList = _3DServiceHelper._3DServiceHelper.InstallUpdatePackage(ctx, UpdatePrtableinEnum.AfterCreateModel, ConvertCol, null,null,null,null,0,"", "OptRptEntry");
                        if (updatePrtList != null)
                        {
                            _3DServiceHelper._3DServiceHelper.updateTableStatus(ctx, UpdatePrtableinEnum.AfterCreateModel,ObjectEnum.OPLAQual, null, updatePrtList);
                        }
                        //处理保存结果，成功、失败
                        if (saveResult.SuccessDataEnity != null&& saveResult.SuccessDataEnity.Count()>0)
                        {
                            object[] ids = (from c in saveResult.SuccessDataEnity
                                            select c[0]).ToArray();//保存成功的结果
                            IOperationResult submitResult = ids.Count() > 0 ? _3DServiceHelper._3DServiceHelper.Submit(ctx, "SFC_OperationReport", ids) : null;
                            if (submitResult.SuccessDataEnity != null && submitResult.SuccessDataEnity.Count()>0)
                            {
                                List<UpdatePrtableEntity> successPrtList =_3DServiceHelper._3DServiceHelper.InstallUpdatePackage(ctx, UpdatePrtableinEnum.SubmitSucess, null, null, null, submitResult);
                                _3DServiceHelper._3DServiceHelper.updateTableStatus(ctx, UpdatePrtableinEnum.SubmitSucess,ObjectEnum.OPLAQual, null, successPrtList);
                                //foreach (DynamicObject item in submitResult.SuccessDataEnity)
                                //{
                                //    object[] ips = new object[] { item[0] };
                                //    IOperationResult auditResult = _3DServiceHelper._3DServiceHelper.Audit(ctx, "STK_TransferDirect", ips);
                                //    if (auditResult.IsSuccess)
                                //    {
                                //        successPrtList = _3DServiceHelper._3DServiceHelper.InstallUpdatePackage(ctx, UpdatePrtableinEnum.AuditSucess, null, null, successPrtList, auditResult, null);
                                //    }
                                //    else if (((List<ValidationErrorInfo>)auditResult.ValidationErrors).Count() > 0)
                                //    {

                                //    }
                                //    else if (!auditResult.InteractionContext.SimpleMessage.Equals("") && auditResult.InteractionContext.SimpleMessage != null)
                                //    {
                                //        exceptPrtList = _3DServiceHelper._3DServiceHelper.InstallUpdatePackage(ctx, UpdatePrtableinEnum.AuditError, null, null, exceptPrtList, auditResult, item);
                                //    }
                                //}
                                ////更新prtablein表 审核错误信息
                                //_3DServiceHelper._3DServiceHelper.updateTableStatus(ctx, UpdatePrtableinEnum.AuditError, null, exceptPrtList);
                                ////更新prtablein表审核成功信息
                                //_3DServiceHelper._3DServiceHelper.updateTableStatus(ctx, UpdatePrtableinEnum.AuditSucess, null, successPrtList);
                                ////插入审核错误信息进入错误信息表
                                //_3DServiceHelper._3DServiceHelper.insertErrorTable(ctx, UpdatePrtableinEnum.AuditError);
                            }
                        }
                        if (((List<ValidationErrorInfo>)saveResult.ValidationErrors).Count() > 0)
                        {
                            List<UpdatePrtableEntity> updateSavePrtList = _3DServiceHelper._3DServiceHelper.InstallUpdatePackage(ctx, UpdatePrtableinEnum.SaveError, null, (List<ValidationErrorInfo>)saveResult.ValidationErrors);
                            //更新prtablein表状态
                            _3DServiceHelper._3DServiceHelper.updateTableStatus(ctx, UpdatePrtableinEnum.SaveError,ObjectEnum.OPLAQual, null, updateSavePrtList);
                            //插入Processtable表信息
                            _3DServiceHelper._3DServiceHelper.insertErrorTable(ctx, UpdatePrtableinEnum.SaveError,ObjectEnum.OPLAQual);

                        }
                    }
                    
                }
            }
        }
    }
}
