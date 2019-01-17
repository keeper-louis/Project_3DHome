using KEEPER.K3._3D.Contracts;
using KEEPER.K3._3D.Core.ParamOption;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kingdee.BOS;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.App;
using Kingdee.BOS.Contracts;
using Kingdee.BOS.Core.List;
using Kingdee.BOS.Core.Metadata.ConvertElement.ServiceArgs;
using Kingdee.BOS.Orm;
using Kingdee.BOS.Core.DynamicForm.Operation;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Util;
using Kingdee.BOS.Core.Interaction;
using Kingdee.BOS.Core.DynamicForm;

namespace KEEPER.K3.APP
{
    public class CommonService : ICommonService
    {
        public IEnumerable<DynamicObject> ConvertBills(Context ctx, ConvertOption option)
        {
            IEnumerable<DynamicObject> targetDatas = null;
            IConvertService convertService = ServiceHelper.GetService<IConvertService>();
            var rules = convertService.GetConvertRules(ctx, option.SourceFormId, option.TargetFormId);
            if (rules == null || rules.Count == 0)
            {
                throw new KDBusinessException("", string.Format("未找到{0}到{1}之间，启用的转换规则，无法自动下推！", option.SourceFormId, option.TargetFormId));
            }
            // 取勾选了默认选项的规则
            var rule = rules.FirstOrDefault(t => t.IsDefault);
            // 如果无默认规则，则取第一个
            if (rule == null)
            {
                rule = rules[0];
            }
            // 开始构建下推参数：
            // 待下推的源单数据行
            List<ListSelectedRow> srcSelectedRows = new List<ListSelectedRow>();
            foreach (long billId in option.SourceBillIds)
            {

                // 把待下推的源单内码，逐个创建ListSelectedRow对象，添加到集合中
                //srcSelectedRows.Add(new ListSelectedRow(billId.ToString(), string.Empty, 0, option.SourceFormId));
                // 特别说明：上述代码，是整单下推；
                // 如果需要指定待下推的单据体行，请参照下句代码，在ListSelectedRow中，指定EntryEntityKey以及EntryId
                foreach (long billEntryId in option.SourceBillEntryIds)
                {
                    srcSelectedRows.Add(new ListSelectedRow(billId.ToString(), billEntryId.ToString(), 0, option.SourceFormId) { EntryEntityKey = option.SourceEntryEntityKey });
                } 
            }
            // 指定目标单单据类型:情况比较复杂，直接留空，会下推到默认的单据类型
            string targetBillTypeId = string.Empty;
            // 指定目标单据主业务组织：情况更加复杂，
            // 建议在转换规则中，配置好主业务组织字段的映射关系：运行时，由系统根据映射关系，自动从上游单据取主业务组织，避免由插件指定
            long targetOrgId = 0;
            // 自定义参数字典：把一些自定义参数，传递到转换插件中；转换插件再根据这些参数，进行特定处理
            Dictionary<string, object> custParams = new Dictionary<string, object>();
            //custParams.Add("1", 1);
            //custParams.Add("2", 2);
            // 组装下推参数对象
            PushArgs pushArgs = new PushArgs(rule, srcSelectedRows.ToArray())
            {
                TargetBillTypeId = targetBillTypeId,
                TargetOrgId = targetOrgId,
                CustomParams = custParams
            };
            // 调用下推服务，生成下游单据数据包
            ConvertOperationResult operationResult = convertService.Push(ctx, pushArgs, OperateOption.Create());
            targetDatas = convertService.Push(ctx, pushArgs, OperateOption.Create()).TargetDataEntities
                .Select(s => s.DataEntity);
            // 开始处理下推结果:
            // 获取下推生成的下游单据数据包
            DynamicObject[] targetBillObjs = (from p in operationResult.TargetDataEntities select p.DataEntity).ToArray();
            if (targetBillObjs.Length == 0)
            {
                // 未下推成功目标单，抛出错误，中断审核
                throw new KDBusinessException("", string.Format("由{0}自动下推{1}，没有成功生成数据包，自动下推失败！", option.SourceFormId, option.TargetFormId));
            }
            // 对下游单据数据包，进行适当的修订，以避免关键字段为空，自动保存失败
            // 示例代码略
            // 读取目标单据元数据
            IMetaDataService metaService = ServiceHelper.GetService<IMetaDataService>();
            var targetBillMeta = metaService.Load(ctx, "目标单据ID") as FormMetadata;
            // 构建保存操作参数：设置操作选项值，忽略交互提示
            OperateOption saveOption = OperateOption.Create();
            // 忽略全部需要交互性质的提示，直接保存；
            //saveOption.SetIgnoreWarning(true);              // 忽略交互提示
            //saveOption.SetInteractionFlag(this.Option.GetInteractionFlag());        // 如果有交互，传入用户选择的交互结果
            // using Kingdee.BOS.Core.Interaction;
            //saveOption.SetIgnoreInteractionFlag(this.Option.GetIgnoreInteractionFlag());

            //// 如下代码，强制要求忽略交互提示(演示案例不需要，注释掉)
            saveOption.SetIgnoreWarning(true);
            //// using Kingdee.BOS.Core.Interaction;
            saveOption.SetIgnoreInteractionFlag(true);
            // 调用保存服务，自动保存
            ISaveService saveService = ServiceHelper.GetService<ISaveService>();
            var saveResult = saveService.Save(ctx, targetBillMeta.BusinessInfo, targetBillObjs, saveOption, "Save");
            // 判断自动保存结果：只有操作成功，才会继续
            if (this.CheckOpResult(saveResult, saveOption))
            {
                //return;
            }
            return targetDatas;
        }

        /// <summary>
        /// 判断操作结果是否成功，如果不成功，则直接抛错中断进程
        /// </summary>
        /// <param name="opResult">操作结果</param>
        /// <param name="opOption">操作参数</param>
        /// <returns></returns>
        private bool CheckOpResult(IOperationResult opResult, OperateOption opOption)
        {
            bool isSuccess = false;
            if (opResult.IsSuccess == true)
            {
                // 操作成功
                isSuccess = true;
            }
            else
            {
                if (opResult.InteractionContext != null
                    && opResult.InteractionContext.Option.GetInteractionFlag().Count > 0)
                {// 有交互性提示
                    // 传出交互提示完整信息对象
                    //this.OperationResult.InteractionContext = opResult.InteractionContext;
                    // 传出本次交互的标识，
                    // 用户在确认继续后，会重新进入操作；
                    // 将以此标识取本交互是否已经确认过，避免重复交互
                    //this.OperationResult.Sponsor = opResult.Sponsor;
                    // 抛出交互错误，把交互信息传递给前端
                    new KDInteractionException(opOption, opResult.Sponsor);
                }
                else
                {
                    // 操作失败，拼接失败原因，然后抛出中断
                    opResult.MergeValidateErrors();
                    if (opResult.OperateResult == null)
                    {// 未知原因导致提交失败
                        throw new KDBusinessException("", "未知原因导致自动提交、审核失败！");
                    }
                    else
                    {
                        StringBuilder sb = new StringBuilder();
                        sb.AppendLine("自动操作失败：");
                        foreach (var operateResult in opResult.OperateResult)
                        {
                            sb.AppendLine(operateResult.Message);
                        }
                        throw new KDBusinessException("", sb.ToString());
                    }
                }
            }
            return isSuccess;
        }
    }
}
