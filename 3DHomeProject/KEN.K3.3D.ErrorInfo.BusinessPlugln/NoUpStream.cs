using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.List;
using Kingdee.BOS.Core.List.PlugIn;
using Kingdee.BOS.Util;
using System;
using System.ComponentModel;
using System.Linq;


namespace KEN.K3._3D.ErrorInfo.BusinessPlugln

{
    public class NoUpStream : AbstractListPlugIn
    {
        [Description("处理无上游单据")]
        public override void BarItemClick(BarItemClickEventArgs e)
        {
            ListSelectedRowCollection selectRows = this.ListView.SelectedRowsInfo;
            string objectID = this.ListView.BillBusinessInfo.GetForm().Id;
            string entiryKey = this.ListView.Model.BillBusinessInfo.GetField("FQty").EntityKey;

            switch (e.BarItemKey.ToUpperInvariant())
            {
                case "TBBOMCONFIG":
                    //分是否显示单据体处理
                    if (selectRows.Count() != 1 || this.ListView.SelectedRowsInfo[0].EntryEntityKey != "FSaleOrderEntry"
                        || this.ListView.SelectedRowsInfo[0].EntryPrimaryKeyValue.IsNullOrEmptyOrWhiteSpace())
                    {
                        e.Cancel = true;
                        this.View.ShowMessage("请选择唯一一行单据分录进行配置操作！");
                        break;
                    }

                    //私有函数，进行BOM配置，具体逻辑略去
                    //BomConfigViewEdit();
                    break;
                    //其他case分支处理逻辑略去
            }
        }
    }
}

