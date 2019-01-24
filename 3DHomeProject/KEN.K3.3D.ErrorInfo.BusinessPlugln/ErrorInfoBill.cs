using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.List;
using Kingdee.BOS.Core.List.PlugIn;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Util;
using System;
using System.ComponentModel;
using System.Linq;


namespace KEN.K3._3D.ErrorInfo.BusinessPlugln

{
    public class ErrorInfoBill : AbstractListPlugIn
    {
        [Description("错误信息表列表插件")]
        public override void BarItemClick(BarItemClickEventArgs e)
        {
            ListSelectedRowCollection selectRows = this.ListView.SelectedRowsInfo;
            //处理无上游单据
            if (String.Equals(e.BarItemKey.ToUpperInvariant(), "NoUpStream", StringComparison.CurrentCultureIgnoreCase))
            {
                string filter = string.Empty;
                    for(int i=0;i< selectRows.Count(); i++)
                {
                    filter = filter+Convert.ToString(selectRows[i].DataRow["FPrtableinID"])+",";
                }
                filter = filter.TrimEnd(',');


                //分是否显示单据体处理
                if (selectRows.Count() != 1 || this.ListView.SelectedRowsInfo[0].EntryEntityKey != "FSaleOrderEntry"
                    || this.ListView.SelectedRowsInfo[0].EntryPrimaryKeyValue.IsNullOrEmptyOrWhiteSpace())
                {
                    e.Cancel = true;
                    this.View.ShowMessage("请选择唯一一行单据分录进行配置操作！");

                }
            }

        }
    }
}

