using Kingdee.BOS.Core;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Orm.DataEntity;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LC.K3._3D.YDDSAVE.ServicePlugIn
{
    [Description("预定单保存后去掉ABC值小数")]
    public class YDDBIllService : AbstractOperationServicePlugIn
    {
        public override void BeforeExecuteOperationTransaction(BeforeExecuteOperationTransaction e)
        {
            base.BeforeExecuteOperationTransaction(e);
            if (e.SelectedRows == null || e.SelectedRows.Count<ExtendedDataEntity>() <= 0)
            {
                return;
            }
            foreach (ExtendedDataEntity current in e.SelectedRows)
            {
                DynamicObjectCollection SaleOrderEntry = current.DataEntity["FEntity"] as DynamicObjectCollection;

                //原因：之前业务A、B 值都为整数，业务上没有小数，处理方案 代码预定保存的时候给去掉
                foreach (DynamicObject entry in SaleOrderEntry)
                {
                    if (entry["F_YJ_TEXT5"] != null)
                    {
                        string[] splits = Convert.ToString(entry["F_YJ_TEXT5"]).Split('.');
                        entry["F_YJ_TEXT5"] = splits[0];
                    }
                    if (entry["F_YJ_TEXT6"] != null)
                    {
                        string[] splits = Convert.ToString(entry["F_YJ_TEXT6"]).Split('.');
                        entry["F_YJ_TEXT6"] = splits[0];
                    }
                    if (entry["F_YJ_TEXT7"] != null)
                    {
                        string[] splits = Convert.ToString(entry["F_YJ_TEXT7"]).Split('.');
                        entry["F_YJ_TEXT7"] = splits[0];
                    }

                }
            }
        }
    }
}
