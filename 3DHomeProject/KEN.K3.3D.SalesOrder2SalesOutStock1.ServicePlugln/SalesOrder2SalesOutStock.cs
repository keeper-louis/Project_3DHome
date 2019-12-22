using Kingdee.BOS.App;
using Kingdee.BOS.Contracts;
using Kingdee.BOS.Core;
using Kingdee.BOS.Core.DynamicForm.Operation;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.Metadata.ConvertElement.PlugIn;
using Kingdee.BOS.Core.Metadata.ConvertElement.PlugIn.Args;
using Kingdee.BOS.Orm;
using Kingdee.BOS.Orm.DataEntity;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace KEN.K3._3D.SalesOrder2SalesOutStock.ServicePlugln
{

    public class SalesOrder2SalesOutStock : AbstractConvertPlugIn
    {
        public override void OnAfterCreateLink(CreateLinkEventArgs e)
        {
            base.OnAfterCreateLink(e);
            OperateOption Option = base.Option;
            List<decimal> amount = new List<decimal>();
            
            if (Option.TryGetVariableValue<List<decimal>>("OutStockAmount", out amount))
            {
                List<int> srcbillseq = Option.GetVariableValue<List<int>>("srcbillseq");
                DateTime FDATE = Option.GetVariableValue<DateTime>("FDATE");
                (e.TargetExtendedDataEntities.FindByEntityKey("FBillHead"))[0].DataEntity["Date"] = FDATE;
                for (int i = 0; i < e.TargetExtendedDataEntities.FindByEntityKey("FEntity").Count(); i++)
                {
                    (e.TargetExtendedDataEntities.FindByEntityKey("FEntity"))[i].DataEntity["SALBASEQTY"] = amount[i];
                    (e.TargetExtendedDataEntities.FindByEntityKey("FEntity"))[i].DataEntity["BaseUnitQty"] = amount[i];
                    (e.TargetExtendedDataEntities.FindByEntityKey("FEntity"))[i].DataEntity["PRICEBASEQTY"] = amount[i];
                    (e.TargetExtendedDataEntities.FindByEntityKey("FEntity"))[i].DataEntity["Fsrcbillseq"] = srcbillseq[i];

                }
            }
      }
    }
}
