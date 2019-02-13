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
using Kingdee.BOS.Core.Const;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Core.Metadata.FormElement;
using Kingdee.BOS.Core.Bill;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.App.Data;
using KEEPER.K3._3D.Core.Entity;
using System.Data;
using System.Data.SqlClient;
using Kingdee.BOS.Core.Validation;

namespace KEEPER.K3.APP
{
    public class CommonService : ICommonService
    {
        #region 审核
        public IOperationResult AuditBill(Context ctx, string FormID, object[] ids)
        {
            IMetaDataService metaService = ServiceHelper.GetService<IMetaDataService>();//元数据服务
            FormMetadata Meta = metaService.Load(ctx, FormID) as FormMetadata;//获取元数据
            OperateOption AuditOption = OperateOption.Create();
            AuditOption.SetIgnoreWarning(true);
            AuditOption.SetIgnoreInteractionFlag(true);
            IOperationResult AuditResult = BusinessDataServiceHelper.Audit(ctx, Meta.BusinessInfo, ids, AuditOption);
            return AuditResult;
        }
        #endregion

        #region 批量保存
        public IOperationResult BatchSaveBill(Context ctx, string FormID, DynamicObject[] dyObject)
        {
            IMetaDataService metaService = ServiceHelper.GetService<IMetaDataService>();//元数据服务
            FormMetadata Meta = metaService.Load(ctx, FormID) as FormMetadata;//获取元数据
            OperateOption SaveOption = OperateOption.Create();
            IOperationResult SaveResult = BusinessDataServiceHelper.Save(ctx, Meta.BusinessInfo, dyObject, SaveOption, "Save");
            return SaveResult;
        }
        #endregion

        #region 下推
        public DynamicObject[] ConvertBills(Context ctx, ConvertOption option)
        {
            //IEnumerable<DynamicObject> targetDatas = null;
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
            Dictionary<long, List<Tuple<string, int>>> dic = new Dictionary<long, List<Tuple<string, int>>>();
            foreach (long billId in option.SourceBillIds)
            {
                srcSelectedRows = new List<ListSelectedRow>();
                int rowKey = -1;
                // 把待下推的源单内码，逐个创建ListSelectedRow对象，添加到集合中
                //srcSelectedRows.Add(new ListSelectedRow(billId.ToString(), string.Empty, 0, option.SourceFormId));
                // 特别说明：上述代码，是整单下推；
                // 如果需要指定待下推的单据体行，请参照下句代码，在ListSelectedRow中，指定EntryEntityKey以及EntryId
                foreach (long billEntryId in option.SourceBillEntryIds)
                {
                    //srcSelectedRows.Add(new ListSelectedRow(billId.ToString(), billEntryId.ToString(), rowKey++, option.SourceFormId) { EntryEntityKey = option.SourceEntryEntityKey });
                    ListSelectedRow row = new ListSelectedRow(billId.ToString(), billEntryId.ToString(), rowKey++, option.SourceFormId);
                    row.EntryEntityKey = option.SourceEntryEntityKey;
                    Dictionary<string, string> fieldValues = new Dictionary<string, string>();
                    fieldValues.Add(option.SourceEntryEntityKey,billEntryId.ToString());
                    row.FieldValues = fieldValues;
                    srcSelectedRows.Add(row);
                    if (rowKey == 1)
                    {
                        dic.Add(billEntryId, new List<Tuple<string, int>> { new Tuple<string, int>(" ", 3) });
                    }
                    else
                    {
                        dic.Add(billEntryId, new List<Tuple<string, int>> { new Tuple<string, int>(" ", 1) });
                    }
                    
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
            OperateOption option1 = OperateOption.Create();
            option1.SetVariableValue(BOSConst.CST_ConvertValidatePermission, false);
            option1.SetVariableValue("OpQtydic", dic);
            option1.SetVariableValue("IsScanning", true);
            ConvertOperationResult operationResult = convertService.Push(ctx, pushArgs, option1);
            //targetDatas = convertService.Push(ctx, pushArgs, OperateOption.Create()).TargetDataEntities
              //  .Select(s => s.DataEntity);
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
            var targetBillMeta = metaService.Load(ctx, option.TargetFormId) as FormMetadata;
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
            return targetBillObjs;
        }

        #endregion

        #region 获取审核失败数据集合
        public List<UpdatePrtableEntity> getAuditErrorData(Context ctx, UpdatePrtableinEnum status)
        {
            if (status == UpdatePrtableinEnum.AuditError)
            {
                string strDateSql = string.Format(@"/*dialect*/select distinct prtIn.fdate from prtablein prtIn where prtIn.state = 3 and prtIn.status = 3 and prtIn.ferrorstatus = 2");
                DynamicObjectCollection dateData = DBUtils.ExecuteDynamicObject(ctx, strDateSql, null);
                if (dateData.Count() > 0)
                {
                        string strSql = string.Format(@"/*dialect*/select distinct prtIn.fcloudheadid,prtIn.fbillno
  from prtablein prtIn
 where prtIn.state = 3
   and prtIn.status = 3
   and prtIn.ferrorstatus = 2");
                        DynamicObjectCollection PurTransferData = DBUtils.ExecuteDynamicObject(ctx, strSql, null);
                        List<UpdatePrtableEntity> uPrtabEntity = new List<UpdatePrtableEntity>();
                        foreach (DynamicObject purTransferData in PurTransferData)
                        {
                            UpdatePrtableEntity salEntryData = new UpdatePrtableEntity();
                            salEntryData.k3cloudheadID = Convert.ToInt64(purTransferData["fcloudheadid"]);
                            salEntryData.billNo = Convert.ToString(purTransferData["fbillno"]);
                            uPrtabEntity.Add(salEntryData);
                        }
                    
                    return uPrtabEntity;
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }
        #endregion

        #region 获取除审核失败其余采购件调拨单数据
        public List<SalOrderTransferList> getPurTransferData(Context ctx,UpdatePrtableinEnum status)
        {
            if (status == UpdatePrtableinEnum.BeforeSave)
            {
                string strDateSql = string.Format(@"/*dialect*/select distinct prtIn.fdate from prtablein prtIn where prtIn.state = 3 and prtIn.status = 3 and prtIn.ferrorstatus <> 2");
                DynamicObjectCollection dateData = DBUtils.ExecuteDynamicObject(ctx, strDateSql, null);
                if (dateData.Count() > 0)
                {
                    List<SalOrderTransferList> list = new List<SalOrderTransferList>();
                    foreach (DynamicObject dateCol in dateData)
                    {
                        SalOrderTransferList sallist = new SalOrderTransferList();
                        sallist.BusinessDate = Convert.ToDateTime(dateCol["fdate"]);
                        string strSql = string.Format(@"/*dialect*/select prtIn.fdate,
       prtIn.id,
       prtIn.salenumber,
       prtIn.linenumber,
       prtIn.technicscode,
       orderentry.FMATERIALID,
       orderentry.FAUXPROPID,
       orderentry.FLOT,
       prtIn.amount,
       prtIn.Fstockid
  from prtablein prtIn
 inner join t_sal_order salorder
    on prtIn.salenumber = salorder.fbillno
   and prtIn.state = 3
   and prtIn.status = 3
   and prtIn.ferrorstatus <> 2
 inner join t_sal_orderentry orderentry
    on salorder.fid = orderentry.fid
   and prtIn.linenumber = orderentry.fseq
 where prtIn.fdate = '{0}'", Convert.ToDateTime(dateCol["fdate"]));
                        DynamicObjectCollection PurTransferData = DBUtils.ExecuteDynamicObject(ctx, strSql, null);
                        List<SalOrderTransfer> salEntryDataList = new List<SalOrderTransfer>();
                        foreach (DynamicObject purTransferData in PurTransferData)
                        {
                            SalOrderTransfer salEntryData = new SalOrderTransfer();
                            salEntryData.prtID = Convert.ToInt64(purTransferData["id"]);
                            salEntryData.FDATE = Convert.ToDateTime(purTransferData["fdate"]);
                            salEntryData.saleNumber = Convert.ToString(purTransferData["salenumber"]);
                            salEntryData.lineNumber = Convert.ToString(purTransferData["linenumber"]);
                            salEntryData.technicsCode = Convert.ToString(purTransferData["technicscode"]);
                            salEntryData.MATERIALID = Convert.ToInt64(purTransferData["FMATERIALID"]);
                            salEntryData.AUXPROPID = Convert.ToInt64(purTransferData["FAUXPROPID"]);
                            salEntryData.Lot = Convert.ToInt64(purTransferData["FLOT"]);
                            salEntryData.amount = Convert.ToInt32(purTransferData["amount"]);
                            salEntryData.stocknumber = Convert.ToString(purTransferData["Fstockid"]);
                            salEntryDataList.Add(salEntryData);
                        }
                        sallist.salOrderTransfer = salEntryDataList;
                        list.Add(sallist);
                    }
                    return list;
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
            
        }

        #endregion

        #region 错误信息表插入
        public void insertErrorTable(Context ctx, UpdatePrtableinEnum status)
        {
            if (status == UpdatePrtableinEnum.SaveError)
            {
                string strSql = string.Format(@"/*dialect*/insert into processtable
  select Prtablein.id FBILLNO,
         'A' FDOCUMENTSTATUS,
         Prtablein.salenumber SALENUMBER,
         Prtablein.linenumber LINENUMBER,
         Prtablein.technicscode TECHNICSCODE,
         Prtablein.id PRTABLEINID,
         Prtablein.ferrormsg REASON,
         Prtablein.state STATE,
         Prtablein.fdate FDATE,
         getdate() FSUBDATE
    from Prtablein
   where Prtablein.state = 3
     and Prtablein.Ferrorstatus = 1
     and Prtablein.status = 2
     and not exists
   (select 1
            from Processtable
           where Processtable.FBILLNO = Prtablein.id)");
                DBUtils.Execute(ctx, strSql);
            }

            if (status == UpdatePrtableinEnum.AuditError)
            {
                string strSql = string.Format(@"/*dialect*/insert into processtable
  select Prtablein.id FBILLNO,
         'A' FDOCUMENTSTATUS,
         Prtablein.salenumber SALENUMBER,
         Prtablein.linenumber LINENUMBER,
         Prtablein.technicscode TECHNICSCODE,
         Prtablein.id PRTABLEINID,
         Prtablein.ferrormsg REASON,
         Prtablein.state STATE,
         Prtablein.fdate FDATE,
         getdate() FSUBDATE
    from Prtablein
   where Prtablein.state = 3
     and Prtablein.Ferrorstatus = 2
     and Prtablein.status = 2
     and not exists
   (select 1
            from Processtable
           where Processtable.FBILLNO = Prtablein.id)");
                DBUtils.Execute(ctx, strSql);
            }
        }

        #endregion

        #region 构建单据数据包
        public DynamicObject installCostRequestPackage(Context ctx, string FormID, Action<IDynamicFormViewService> fillBillPropertys, string BillTypeId = "")
        {
            FormMetadata Meta = MetaDataServiceHelper.Load(ctx, FormID) as FormMetadata;//获取元数据
            Form form = Meta.BusinessInfo.GetForm();
            IDynamicFormViewService dynamicFormViewService = (IDynamicFormViewService)Activator.CreateInstance(Type.GetType("Kingdee.BOS.Web.Import.ImportBillView,Kingdee.BOS.Web"));
            // 创建视图加载参数对象，指定各种参数，如FormId, 视图(LayoutId)等
            BillOpenParameter openParam = new BillOpenParameter(form.Id, Meta.GetLayoutInfo().Id);
            openParam.Context = ctx;
            openParam.ServiceName = form.FormServiceName;
            openParam.PageId = Guid.NewGuid().ToString();
            openParam.FormMetaData = Meta;
            openParam.Status = OperationStatus.ADDNEW;
            openParam.CreateFrom = CreateFrom.Default;
            // 单据类型
            openParam.DefaultBillTypeId = BillTypeId;
            openParam.SetCustomParameter("ShowConfirmDialogWhenChangeOrg", false);
            // 插件
            List<AbstractDynamicFormPlugIn> plugs = form.CreateFormPlugIns();
            openParam.SetCustomParameter(FormConst.PlugIns, plugs);
            PreOpenFormEventArgs args = new PreOpenFormEventArgs(ctx, openParam);
            foreach (var plug in plugs)
            {
                plug.PreOpenForm(args);
            }
            // 动态领域模型服务提供类，通过此类，构建MVC实例
            IResourceServiceProvider provider = form.GetFormServiceProvider(false);

            dynamicFormViewService.Initialize(openParam, provider);
            IBillView billView = dynamicFormViewService as IBillView;
            ((IBillViewService)billView).LoadData();

            // 触发插件的OnLoad事件：
            // 组织控制基类插件，在OnLoad事件中，对主业务组织改变是否提示选项进行初始化。
            // 如果不触发OnLoad事件，会导致主业务组织赋值不成功
            DynamicFormViewPlugInProxy eventProxy = billView.GetService<DynamicFormViewPlugInProxy>();
            eventProxy.FireOnLoad();
            if (fillBillPropertys != null)
            {
                fillBillPropertys(dynamicFormViewService);
            }
            // 设置FormId
            form = billView.BillBusinessInfo.GetForm();
            if (form.FormIdDynamicProperty != null)
            {
                form.FormIdDynamicProperty.SetValue(billView.Model.DataObject, form.Id);
            }
            return billView.Model.DataObject;
        }
        #endregion

        #region 构建更新数据包集合
        public List<UpdatePrtableEntity> InstallUpdatePackage(Context ctx, UpdatePrtableinEnum status, DynamicObject[] trasferbill = null, List<ValidationErrorInfo> vo = null, List<UpdatePrtableEntity> exceptPrtList = null, IOperationResult auditResult = null, DynamicObject submitResult = null,long k3cloudheadid = 0,string billnos = "")
        {
            if (status == UpdatePrtableinEnum.AfterCreateModel)
            {
                List<UpdatePrtableEntity> updatePrtList = new List<UpdatePrtableEntity>();
                foreach (DynamicObject item in trasferbill)
                {
                    long id = Convert.ToInt64(item["Id"]);
                    string billno = item["BillNo"] != null ? Convert.ToString(item["BillNo"]) : "";
                    foreach (DynamicObject aa in item["TransferDirectEntry"] as DynamicObjectCollection)
                    {
                        UpdatePrtableEntity uy = new UpdatePrtableEntity();
                        uy.k3cloudheadID = id;
                        uy.billNo = billno;
                        uy.k3cloudID = Convert.ToInt64(aa["Id"]);
                        uy.prtID = Convert.ToInt32(aa["Fprtableinid"]);
                       // uy.salenumber = Convert.ToString(aa["Fsalenumber"]);
                       // uy.linenumber = Convert.ToString(aa["Flinenumber"]);
                       // uy.techcode = Convert.ToString(aa["Ftechcode"]);
                        updatePrtList.Add(uy);
                    }
                }
                return updatePrtList;
            }
            if (status == UpdatePrtableinEnum.SaveError)
            {
                List<UpdatePrtableEntity> updatePrtList = new List<UpdatePrtableEntity>();
                foreach (ValidationErrorInfo item in vo)
                {
                    UpdatePrtableEntity uy = new UpdatePrtableEntity();
                    uy.k3cloudID = Convert.ToInt64(item.BillPKID);
                    uy.errorMsg = item.Message;
                    updatePrtList.Add(uy);
                }
                return updatePrtList;
            }
            if (status == UpdatePrtableinEnum.AuditError)
            {
                if (submitResult!=null)
                {
                    UpdatePrtableEntity uy = new UpdatePrtableEntity();
                    uy.k3cloudheadID = Convert.ToInt64(submitResult["Id"]);//pk
                    uy.errorMsg = Convert.ToString(submitResult["BillNo"]) + auditResult.InteractionContext.SimpleMessage;
                    exceptPrtList.Add(uy);
                    return exceptPrtList;
                }
                else
                {
                    UpdatePrtableEntity uy = new UpdatePrtableEntity();
                    uy.k3cloudheadID = k3cloudheadid;
                    uy.errorMsg = billnos + auditResult.InteractionContext.SimpleMessage;
                    exceptPrtList.Add(uy);
                    return exceptPrtList;
                }
                
            }
            if (status ==UpdatePrtableinEnum.AuditSucess)
            {
                UpdatePrtableEntity uy = new UpdatePrtableEntity();
                object[] ids = (from c in auditResult.SuccessDataEnity
                                select c[0]).ToArray();
                uy.k3cloudheadID = Convert.ToInt64(ids[0]);//pk
                exceptPrtList.Add(uy);
                return exceptPrtList;
            }
            return null;
        }
        #endregion

        #region 判断是否有未处理过的采购件调拨数据
        public bool isTransfer(Context ctx,UpdatePrtableinEnum status)
        {
            if (status == UpdatePrtableinEnum.BeforeSave)
            {
                //[采购件]and[预检完成]and[处理错误状态不等于审核]的数据
                string strSql = string.Format(@"/*dialect*/select count(*) amount from prtablein where state = 3 and status = 3 and ferrorstatus <> 2");
                int amount = DBUtils.ExecuteScalar<int>(ctx, strSql, 0, null);
                if (amount == 0)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            else if (status == UpdatePrtableinEnum.AuditError)
            {
                //[采购件]and[预检完成]and[处理错误状态等于审核]的数据
                string strSql = string.Format(@"/*dialect*/select count(*) amount from prtablein where state = 3 and status = 3 and ferrorstatus = 2");
                int amount = DBUtils.ExecuteScalar<int>(ctx, strSql, 0, null);
                if (amount == 0)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            else
            {
                return false;
            }
            
            
        }
        #endregion

        #region 单个保存
        public IOperationResult SaveBill(Context ctx, string FormID, DynamicObject dyObject)
        {
            IMetaDataService metaService = ServiceHelper.GetService<IMetaDataService>();//元数据服务
            FormMetadata Meta = metaService.Load(ctx, FormID) as FormMetadata;//获取元数据
            OperateOption SaveOption = OperateOption.Create();
            IOperationResult SaveResult = BusinessDataServiceHelper.Save(ctx, Meta.BusinessInfo, dyObject, SaveOption, "Save");
            //BusinessDataServiceHelper.Save()
            return SaveResult;
        }
        #endregion

        #region 提交
        public IOperationResult SubmitBill(Context ctx, string FormID, object[] ids)
        {
            IMetaDataService metaService = ServiceHelper.GetService<IMetaDataService>();//元数据服务
            FormMetadata Meta = metaService.Load(ctx, FormID) as FormMetadata;//获取元数据
            OperateOption submitOption = OperateOption.Create();
            IOperationResult submitResult = BusinessDataServiceHelper.Submit(ctx, Meta.BusinessInfo, ids, "Submit", submitOption);
            return submitResult;
        }
        #endregion

        #region 更新prtablein表
        public void updateTableStatus(Context ctx, UpdatePrtableinEnum status,long[] ids = null,List<UpdatePrtableEntity> uyList = null)
        {
            if (status == UpdatePrtableinEnum.AfterGetDate)
            {
                //创建临时表，将ids存入临时表，通过匹配更新数据处理状态，更新完成后删除临时表
                DataTable dt = new DataTable();
                dt.TableName = "prtablein";
                var idCol = dt.Columns.Add("id");
                idCol.DataType = typeof(long);
                var statusCol = dt.Columns.Add("status");
                statusCol.DataType = typeof(int);
                var subdate = dt.Columns.Add("Fsubdate");
                subdate.DataType = typeof(DateTime);
                // 灌入测试数据
                dt.BeginLoadData();     // 执行此方法，可以提升灌入数据性能
                foreach (long item in ids)
                {
                    dt.LoadDataRow(new object[] { item, 1, DateTime.Now }, true);
                }
                dt.EndLoadData();
                // 准备批量更新服务参数
                // tableName : 待更新的物理表格名
                // dt : 待更新的数据
                BatchSqlParam batchUpdateParam = new BatchSqlParam("prtablein", dt);

                // 设置匹配字段：即DataTable中的数据，与物理表格以那个字段进行匹配
                // 匹配字段可以添加多个
                // columnName: DataTable中的列名
                // fieldName : 物料表格中匹配的字段名
                batchUpdateParam.AddWhereExpression("id", KDDbType.Int64, "id");
                // 设置待更新的字段
                // columnName: DataTable中的列名
                // fieldName : 对应的物料表格字段名
                batchUpdateParam.AddSetExpression("status", KDDbType.Int32, "status");
                // 设置待更新的字段
                // columnName: DataTable中的列名
                // fieldName : 对应的物料表格字段名
                batchUpdateParam.AddSetExpression("Fsubdate", KDDbType.DateTime, "Fsubdate");
                // 执行批量更新
                Kingdee.BOS.App.Data.DBUtils.BatchUpdate(ctx, batchUpdateParam);
            }
            if (status == UpdatePrtableinEnum.AfterCreateModel)
            {
                //创建临时表，将ids存入临时表，通过匹配更新数据处理状态，更新完成后删除临时表
                DataTable dt = new DataTable();
                dt.TableName = "prtablein";
                var idCol = dt.Columns.Add("id");
                idCol.DataType = typeof(long);
                var cloudid = dt.Columns.Add("fcloudid");
                idCol.DataType = typeof(long);
                var cloudheadid = dt.Columns.Add("fcloudheadid");
                cloudheadid.DataType = typeof(long);
                var cloudbillno = dt.Columns.Add("fbillno");
                cloudbillno.DataType = typeof(string);
                var subdate = dt.Columns.Add("Fsubdate");
                subdate.DataType = typeof(DateTime);
                // 灌入测试数据
                dt.BeginLoadData();     // 执行此方法，可以提升灌入数据性能
                foreach (var item in uyList)
                {
                    dt.LoadDataRow(new object[] { item.prtID, item.k3cloudID, item.k3cloudheadID,item.billNo,DateTime.Now }, true);
                }
                dt.EndLoadData();
                // 准备批量更新服务参数
                // tableName : 待更新的物理表格名
                // dt : 待更新的数据
                BatchSqlParam batchUpdateParam = new BatchSqlParam("prtablein", dt);

                // 设置匹配字段：即DataTable中的数据，与物理表格以那个字段进行匹配
                // 匹配字段可以添加多个
                // columnName: DataTable中的列名
                // fieldName : 物料表格中匹配的字段名
                batchUpdateParam.AddWhereExpression("id", KDDbType.Int64, "id");
                // 设置待更新的字段
                // columnName: DataTable中的列名
                // fieldName : 对应的物料表格字段名
                batchUpdateParam.AddSetExpression("fcloudid", KDDbType.Int64, "fcloudid");
                // 设置待更新的字段
                // columnName: DataTable中的列名
                // fieldName : 对应的物料表格字段名
                batchUpdateParam.AddSetExpression("fcloudheadid", KDDbType.Int64, "fcloudheadid");
                // 设置待更新的字段
                // columnName: DataTable中的列名
                // fieldName : 对应的物料表格字段名
                batchUpdateParam.AddSetExpression("fbillno", KDDbType.String, "fbillno");
                // 设置待更新的字段
                // columnName: DataTable中的列名
                // fieldName : 对应的物料表格字段名
                batchUpdateParam.AddSetExpression("Fsubdate", KDDbType.DateTime, "Fsubdate");
                // 执行批量更新
                Kingdee.BOS.App.Data.DBUtils.BatchUpdate(ctx, batchUpdateParam);
            }
            if (status == UpdatePrtableinEnum.SaveError)
            {
                //创建临时表，将ids存入临时表，通过匹配更新数据处理状态，更新完成后删除临时表
                DataTable dt = new DataTable();
                dt.TableName = "prtablein";
                var idCol = dt.Columns.Add("fcloudid");
                idCol.DataType = typeof(long);
                var statusCol = dt.Columns.Add("status");
                statusCol.DataType = typeof(int);
                var errStatusCol = dt.Columns.Add("Ferrorstatus");
                errStatusCol.DataType = typeof(int);
                var errmsg = dt.Columns.Add("ferrormsg");
                errmsg.DataType = typeof(string);
                var subdate = dt.Columns.Add("Fsubdate");
                subdate.DataType = typeof(DateTime);
                // 灌入测试数据
                dt.BeginLoadData();     // 执行此方法，可以提升灌入数据性能
                foreach (var item in uyList)
                {
                    dt.LoadDataRow(new object[] { item.k3cloudID, 2, 1, item.errorMsg,DateTime.Now }, true);
                }
                dt.EndLoadData();
                // 准备批量更新服务参数
                // tableName : 待更新的物理表格名
                // dt : 待更新的数据
                BatchSqlParam batchUpdateParam = new BatchSqlParam("prtablein", dt);

                // 设置匹配字段：即DataTable中的数据，与物理表格以那个字段进行匹配
                // 匹配字段可以添加多个
                // columnName: DataTable中的列名
                // fieldName : 物料表格中匹配的字段名
                batchUpdateParam.AddWhereExpression("fcloudid", KDDbType.Int64, "fcloudid");
                // 设置待更新的字段
                // columnName: DataTable中的列名
                // fieldName : 对应的物料表格字段名
                batchUpdateParam.AddSetExpression("status", KDDbType.Int32, "status");
                // 设置待更新的字段
                // columnName: DataTable中的列名
                // fieldName : 对应的物料表格字段名
                batchUpdateParam.AddSetExpression("Ferrorstatus", KDDbType.Int32, "Ferrorstatus");
                // 设置待更新的字段
                // columnName: DataTable中的列名
                // fieldName : 对应的物料表格字段名
                batchUpdateParam.AddSetExpression("ferrormsg", KDDbType.String, "ferrormsg");
                // 设置待更新的字段
                // columnName: DataTable中的列名
                // fieldName : 对应的物料表格字段名
                batchUpdateParam.AddSetExpression("Fsubdate", KDDbType.DateTime, "Fsubdate");
                // 执行批量更新
                Kingdee.BOS.App.Data.DBUtils.BatchUpdate(ctx, batchUpdateParam);
            }
            if (status == UpdatePrtableinEnum.AuditError)
            {
                //创建临时表，将ids存入临时表，通过匹配更新数据处理状态，更新完成后删除临时表
                DataTable dt = new DataTable();
                dt.TableName = "prtablein";
                var idCol = dt.Columns.Add("fcloudheadid");
                idCol.DataType = typeof(long);
                var statusCol = dt.Columns.Add("status");
                statusCol.DataType = typeof(int);
                var errStatusCol = dt.Columns.Add("Ferrorstatus");
                errStatusCol.DataType = typeof(int);
                var errmsg = dt.Columns.Add("ferrormsg");
                errmsg.DataType = typeof(string);
                var subdate = dt.Columns.Add("Fsubdate");
                subdate.DataType = typeof(DateTime);
                // 灌入测试数据
                dt.BeginLoadData();     // 执行此方法，可以提升灌入数据性能
                foreach (var item in uyList)
                {
                    dt.LoadDataRow(new object[] { item.k3cloudheadID, 2, 2, item.errorMsg, DateTime.Now }, true);
                }
                dt.EndLoadData();
                // 准备批量更新服务参数
                // tableName : 待更新的物理表格名
                // dt : 待更新的数据
                BatchSqlParam batchUpdateParam = new BatchSqlParam("prtablein", dt);

                // 设置匹配字段：即DataTable中的数据，与物理表格以那个字段进行匹配
                // 匹配字段可以添加多个
                // columnName: DataTable中的列名
                // fieldName : 物料表格中匹配的字段名
                batchUpdateParam.AddWhereExpression("fcloudheadid", KDDbType.Int64, "fcloudheadid");
                // 设置待更新的字段
                // columnName: DataTable中的列名
                // fieldName : 对应的物料表格字段名
                batchUpdateParam.AddSetExpression("status", KDDbType.Int32, "status");
                // 设置待更新的字段
                // columnName: DataTable中的列名
                // fieldName : 对应的物料表格字段名
                batchUpdateParam.AddSetExpression("Ferrorstatus", KDDbType.Int32, "Ferrorstatus");
                // 设置待更新的字段
                // columnName: DataTable中的列名
                // fieldName : 对应的物料表格字段名
                batchUpdateParam.AddSetExpression("ferrormsg", KDDbType.String, "ferrormsg");
                // 设置待更新的字段
                // columnName: DataTable中的列名
                // fieldName : 对应的物料表格字段名
                batchUpdateParam.AddSetExpression("Fsubdate", KDDbType.DateTime, "Fsubdate");
                // 执行批量更新
                Kingdee.BOS.App.Data.DBUtils.BatchUpdate(ctx, batchUpdateParam);
            }
            if (status == UpdatePrtableinEnum.AuditSucess)
            {
                //创建临时表，将ids存入临时表，通过匹配更新数据处理状态，更新完成后删除临时表
                DataTable dt = new DataTable();
                dt.TableName = "prtablein";
                var idCol = dt.Columns.Add("fcloudheadid");
                idCol.DataType = typeof(long);
                var statusCol = dt.Columns.Add("status");
                statusCol.DataType = typeof(int);
                var subdate = dt.Columns.Add("Fsubdate");
                subdate.DataType = typeof(DateTime);
                // 灌入测试数据
                dt.BeginLoadData();     // 执行此方法，可以提升灌入数据性能
                foreach (var item in uyList)
                {
                    dt.LoadDataRow(new object[] { item.k3cloudheadID, 4, DateTime.Now }, true);
                }
                dt.EndLoadData();
                // 准备批量更新服务参数
                // tableName : 待更新的物理表格名
                // dt : 待更新的数据
                BatchSqlParam batchUpdateParam = new BatchSqlParam("prtablein", dt);

                // 设置匹配字段：即DataTable中的数据，与物理表格以那个字段进行匹配
                // 匹配字段可以添加多个
                // columnName: DataTable中的列名
                // fieldName : 物料表格中匹配的字段名
                batchUpdateParam.AddWhereExpression("fcloudheadid", KDDbType.Int64, "fcloudheadid");
                // 设置待更新的字段
                // columnName: DataTable中的列名
                // fieldName : 对应的物料表格字段名
                batchUpdateParam.AddSetExpression("status", KDDbType.Int32, "status");
                // 设置待更新的字段
                // columnName: DataTable中的列名
                // fieldName : 对应的物料表格字段名
                batchUpdateParam.AddSetExpression("Fsubdate", KDDbType.DateTime, "Fsubdate");
                // 执行批量更新
                Kingdee.BOS.App.Data.DBUtils.BatchUpdate(ctx, batchUpdateParam);
            }
        }
        #endregion


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
