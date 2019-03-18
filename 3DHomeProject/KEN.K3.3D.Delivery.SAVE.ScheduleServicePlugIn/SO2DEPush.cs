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

namespace KEN.K3._3D.Delivery.PUSH.ScheduleServicePlugIn
{
    [Description("销售订单下推销售出库执行计划")]
    public class SO2DEPush : IScheduleService
    {
        public void Run(Context ctx, Schedule schedule)
        {
            ctx.UserId = 214076;
            ctx.UserName = "扫码专用账户";
            if (_3DServiceHelper.isTransfer(ctx, ObjectEnum.SO2DE, UpdatePrtableinEnum.BeforeSave))
            {
                List<ConvertOption> options = _3DServiceHelper.getOLAPushData(ctx, ObjectEnum.SO2DE);
                long[] bb = options.SelectMany(x => x.prtInId).ToList().ToArray();//本次执行计划要处理的数据的主键ID
                //根据主键对数据标识进行更新，同时设置更新时间戳
                _3DServiceHelper.updateDetableStatus(ctx, UpdatePrtableinEnum.AfterGetDate, ObjectEnum.SO2DE, bb);
                if (options != null && options.Count() > 0)
                {
                    DynamicObject[] ConvertCol = _3DServiceHelper.ConvertOutStockBills(ctx,options, "SAL_SaleOrder", "SAL_OUTSTOCK", "FSaleOrderEntry");
                    if (ConvertCol != null && ConvertCol.Count() > 0)
                    {
                        IOperationResult saveResult = _3DServiceHelper.Save(ctx, "SAL_OUTSTOCK", ConvertCol);
                        List<UpdatePrtableEntity> updatePrtList = _3DServiceHelper.InstallUpdateDePackage(ctx, UpdatePrtableinEnum.AfterCreateModel, ObjectEnum.SO2DE, ConvertCol, null, null, null, null, 0, "", "SAL_OUTSTOCKENTRY");
                        if (updatePrtList != null)
                        {
                            _3DServiceHelper.updateDetableStatus(ctx, UpdatePrtableinEnum.AfterCreateModel, ObjectEnum.SO2DE, null, updatePrtList);
                        }
                        //处理保存结果，成功、失败
                        if (saveResult.SuccessDataEnity != null && saveResult.SuccessDataEnity.Count() > 0)
                        {
                            object[] ids = (from c in saveResult.SuccessDataEnity
                                            select c[0]).ToArray();//保存成功的结果
                            IOperationResult submitResult = ids.Count() > 0 ? _3DServiceHelper.Submit(ctx, "SAL_OUTSTOCK", ids) : null;
                            if (submitResult.SuccessDataEnity != null && submitResult.SuccessDataEnity.Count() > 0)
                            {
                                List<UpdatePrtableEntity> successPrtList = _3DServiceHelper.InstallUpdateDePackage(ctx, UpdatePrtableinEnum.SubmitSucess, ObjectEnum.SO2DE, null, null, null, submitResult);
                                _3DServiceHelper.updateDetableStatus(ctx, UpdatePrtableinEnum.SubmitSucess, ObjectEnum.SO2DE, null, successPrtList);
                            }
                        }
                        if (((List<ValidationErrorInfo>)saveResult.ValidationErrors).Count() > 0)
                        {
                            List<UpdatePrtableEntity> updateSavePrtList = _3DServiceHelper.InstallUpdateDePackage(ctx, UpdatePrtableinEnum.SaveError, ObjectEnum.SO2DE, null, (List<ValidationErrorInfo>)saveResult.ValidationErrors);
                            //更新prtablein表状态
                            _3DServiceHelper.updateDetableStatus(ctx, UpdatePrtableinEnum.SaveError, ObjectEnum.SO2DE, null, updateSavePrtList);
                            //插入Processtable表信息
                            _3DServiceHelper.insertErrorTable(ctx, UpdatePrtableinEnum.SaveError, ObjectEnum.SO2DE);

                        }
                    }

                }
            }
        }
    }
}
