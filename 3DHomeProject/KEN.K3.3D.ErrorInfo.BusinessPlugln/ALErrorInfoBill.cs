using Kingdee.BOS;
using Kingdee.BOS.App.Data;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.List;
using Kingdee.BOS.Core.List.PlugIn;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;
using System;
using System.ComponentModel;
using System.Linq;


namespace KEN.K3._3D.ErrorInfo.BusinessPlugln
{
    public class ALErrorInfoBill : AbstractListPlugIn
    {
        [Description("调拨接口错误信息表列表插件")]

        public override void BarItemClick(BarItemClickEventArgs e)
        {
            //处理无上游无对应销售订单
            if (string.Equals(e.BarItemKey.ToUpperInvariant(), "NoUpStream", StringComparison.CurrentCultureIgnoreCase))
            {
                if (!check("无对应销售订单"))
                {
                    return;
                }
                //预检前端选择数据
                checkData();
                //删除对应错误信息表中的数据
                string filter = getSelectedRowsElements("fid");
                string strSql = string.Format(@"/*dialect*/ Delete Allocationtable where fid in ({0})", filter);
                DBUtils.Execute(this.Context, strSql);
                //将接口待处理表对应数据状态置为3 预检完成状态 供接口再次执行
                filter = getSelectedRowsElements("FBILLNO");
                strSql = string.Format(@"/*dialect*/ Update altablein set status=3 where id in ({0})", filter);
                DBUtils.Execute(this.Context, strSql);
            }
            //处理采购件无对应仓库
            if (string.Equals(e.BarItemKey.ToUpperInvariant(), "NoStock", StringComparison.CurrentCultureIgnoreCase))
            {

                if (!check("采购件无对应仓库"))
                {
                    return;
                }

                string filter = getSelectedRowsElements("FBILLNO");
                //判断前端数据中是否有非采购件数据 如果有直接return返回信息 
                string strSql = string.Format(@"/*dialect*/ select id from altablein where id in ({0}) and isPur<>1  ", filter);
                DynamicObjectCollection periodColl = DBUtils.ExecuteDynamicObject(this.Context, strSql);
                if (periodColl.Count != 0)
                {
                    string message = "单据编号为 ：";
                    for (int i = 0; i < periodColl.Count; i++)
                    {
                        message = message + Convert.ToString(periodColl[i]["id"] + ",");
                    }
                    message.TrimEnd(',');
                    message = message + "的报错信息并非采购件 无法处理！";
                    //e.Cancel = true;
                    this.View.ShowErrMessage(message);
                    return;
                }
                //删除对应错误信息表中的数据
                strSql = string.Format(@"/*dialect*/ Delete Allocationtable where FBILLNO in ({0})", filter);

                DBUtils.Execute(this.Context, strSql);
                //把选中数据的仓库号更新 并把status=0
                strSql = string.Format(@"/*dialect*/ update altablein set PurStockId=a.FNUMBER,status=0 from (
select distinct tbs.FNUMBER,pr.salenumber,pr.linenumber from
altablein pr left join   T_SAL_ORDER tso on pr.Salenumber=tso.FBILLNO left join  T_SAL_ORDERENTRY tsoe on tso.fid=tsoe.FID and pr.Linenumber=tsoe.FSEQ
  left join Purchase2Stock ps on tsoe.FMATERIALID=ps.FMATERIAL left join t_BD_Stock tbs on tbs.FSTOCKID=ps.FSTOCK
where pr.isPur=1
and pr.id in  ({0})
 ) a where altablein.salenumber=a.salenumber and altablein.linenumber=a.linenumber ", filter);

                DBUtils.Execute(this.Context, strSql);


                //查询没有对应仓库的采购件 写入错误信息表
                strSql = string.Format(@"/*dialect*/ insert into  Allocationtable select id FBILLNO,'A' FDOCUMENTSTATUS, pr.salenumber SALENUMBER,pr.linenumber LINENUMBER,pr.Packcode Packcode,id PRTABLEINID,
'采购件无对应仓库' REASON,fdate FDATE,getdate() FSUBDATE from altablein pr where pr.status=0 and pr.PurStockId ='' and pr.isPur=1 and pr.id in ({0})", filter);
                DBUtils.Execute(this.Context, strSql);
                //把采购件无对应仓库的数据 status标识为2
                strSql = string.Format(@"/*dialect*/update altablein set status=2, ferrormsg='采购件无对应仓库'
where  altablein.status=0 and altablein.PurStockId ='' and altablein.isPur=1 and altablein.Warehouseout<>'7.01' and altablein.id in ({0})", filter);
                DBUtils.Execute(this.Context, strSql);
                //剩余的 把status=3
                strSql = string.Format(@"/*dialect*/update altablein set status=3 where altablein.status<>2 and altablein.id in ({0})", filter);
                DBUtils.Execute(this.Context, strSql);


            }
            this.View.Refresh();
        }
        private void checkData()
        {
            string filter = getSelectedRowsElements("FBILLNO");
            //查询无上游单据数据 写入错误信息表
            string strSql = string.Format(@"/*dialect*/	INSERT INTO Allocationtable select id FBILLNO,'A' FDOCUMENTSTATUS, salenumber SALENUMBER,linenumber LINENUMBER,packcode,id PRTABLEINID,
'无对应销售订单' REASON,fdate FDATE,getdate() FSUBDATE from altablein de 
left join (select tso.fid fid,tsoe.FENTRYID FDETAILID,tso.FBILLNO,tsoe.FSEQ 
from T_SAL_ORDER tso,T_SAL_ORDERENTRY tsoe where tso.fid=tsoe.FID) a
on de.Salenumber=a.FBILLNO and de.Linenumber=a.FSEQ
where de.status=0 and (a.fid is null or a.FDETAILID is null) and de.id in ({0})", filter);
            DBUtils.Execute(this.Context, strSql);
        }
        private string getSelectedRowsElements(String key)
        {
            ListSelectedRowCollection selectRows = this.ListView.SelectedRowsInfo;
            string Elements = string.Empty;
            for (int i = 0; i < selectRows.Count(); i++)
            {
                Elements = Elements + Convert.ToString(selectRows[i].DataRow[key]) + ",";
            }
            Elements = Elements.TrimEnd(',');
            return Elements;
        }
        private Boolean check(String key)
        {

            ListSelectedRowCollection selectRows = this.ListView.SelectedRowsInfo;


            if (selectRows.Count() < 1)
            {
                this.View.ShowErrMessage("请至少选中一条数据！");
                return false;
            }

            //判断报错信息是否一致

            string Reason = Convert.ToString(selectRows[0].DataRow["FREASON"]);
            for (int i = 1; i < selectRows.Count(); i++)
            {
                if (string.Equals(Reason, Convert.ToString(selectRows[i].DataRow["FREASON"]), StringComparison.CurrentCultureIgnoreCase))
                {
                    Reason = Convert.ToString(selectRows[i].DataRow["FREASON"]);
                }
                else
                {
                    this.View.ShowErrMessage("选中数据必须为相同问题类型！");
                    return false;
                }
            }

            if (!string.Equals(Reason, key, StringComparison.CurrentCultureIgnoreCase))
            {
                this.View.ShowErrMessage("请选择正确的处理方法！");
                return false;

            }
            return true;
        }
    }
}
