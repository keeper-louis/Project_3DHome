using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Orm.DataEntity;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace LC.K3._3D.SaleOrderSaveOpr.ServicePlugIn
{
    [Description("销售订单保存时携带单价")]
    public class SaleOrderServicePlugin : AbstractOperationServicePlugIn
    {

        public override void OnPreparePropertys(PreparePropertysEventArgs e)
        {
            base.OnPreparePropertys(e);
           
        }
        public override void EndOperationTransaction(EndOperationTransactionArgs e)
        {
            base.EndOperationTransaction(e);

            if (this.FormOperation.Operation.Equals("Save") || this.FormOperation.Operation.Equals("Sumbit"))
            {
                if (e.DataEntitys != null && e.DataEntitys.Count<DynamicObject>() > 0)
                {
                    foreach (DynamicObject item in e.DataEntitys)
                    {
                        // 订单明细分录
                        DynamicObjectCollection SaleOrderEntry = item["SaleOrderEntry"] as DynamicObjectCollection;

                        // 源单明细中有记录
                        if (SaleOrderEntry != null && SaleOrderEntry.Count() > 0)
                        {
                            // MaterialId

                             

                        }
                    }
                }
            }
        }
    }
}
