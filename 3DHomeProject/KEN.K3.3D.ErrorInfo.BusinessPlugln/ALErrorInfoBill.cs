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
                //删除对应错误信息表中的数据
                string filter = getSelectedRowsElements("fid");
                string strSql = string.Format(@"/*dialect*/ Delete Allocationtable where fid in ({0})", filter);
                DBUtils.Execute(this.Context, strSql);
                //将接口待处理表对应数据状态置为0 等待抽数接口检查
                filter = getSelectedRowsElements("FBILLNO");
                strSql = string.Format(@"/*dialect*/ Update altablein set status=0 where id in ({0})", filter);
                DBUtils.Execute(this.Context, strSql);
            }
            //处理物料启用BOM管理，但是销售订单中未选中BOM版本
            if (string.Equals(e.BarItemKey.ToUpperInvariant(), "NoBom", StringComparison.CurrentCultureIgnoreCase))
            {
                if (!check("物料启用BOM管理，但是销售订单中未选中BOM版本"))
                {
                    return;
                }
                //删除对应错误信息表中的数据
                string filter = getSelectedRowsElements("fid");
                string strSql = string.Format(@"/*dialect*/ Delete Allocationtable where fid in ({0})", filter);
                DBUtils.Execute(this.Context, strSql);
                //将接口待处理表对应数据状态置为0 等待抽数接口检查
                filter = getSelectedRowsElements("FBILLNO");
                strSql = string.Format(@"/*dialect*/ Update altablein set status=0 where id in ({0})", filter);
                DBUtils.Execute(this.Context, strSql);
            }
            //重新审核
            if (string.Equals(e.BarItemKey.ToUpperInvariant(), "ReAudit", StringComparison.CurrentCultureIgnoreCase))
            {
                if (!check("ReAudit"))
                {
                    return;
                }
                //预检前端选择数据
                if (!checkData("ReAudit"))
                {
                    return;
                }
                //删除对应错误信息表中的数据
                string filter = getSelectedRowsFErrorBillNo("FErrorBillNo");
                string strSql = string.Format(@"/*dialect*/ Delete Allocationtable where FErrorBillNo in ({0})", filter);
                DBUtils.Execute(this.Context, strSql);
                //将接口待处理表对应数据状态置为5 3D业务人员手工审核成功
                strSql = string.Format(@"/*dialect*/ Update altablein set status=5,ferrormsg='3D业务人员手工审核成功',fsubdate=GETDATE() where fbillno in ({0})  and status=2", filter);
                DBUtils.Execute(this.Context, strSql);
            }
            //处理采购件无对应仓库
            if (string.Equals(e.BarItemKey.ToUpperInvariant(), "NoStock", StringComparison.CurrentCultureIgnoreCase))
            {

                if (!check("采购件无对应仓库"))
                {
                    return;
                }

                //删除对应错误信息表中的数据
                string filter = getSelectedRowsElements("fid");
                string strSql = string.Format(@"/*dialect*/ Delete Allocationtable where fid in ({0})", filter);
                DBUtils.Execute(this.Context, strSql);
                //将接口待处理表对应数据状态置为0 等待抽数接口检查
                filter = getSelectedRowsElements("FBILLNO");
                strSql = string.Format(@"/*dialect*/ Update altablein set status=0,isPur=0 where id in ({0})", filter);
                DBUtils.Execute(this.Context, strSql);
            }
            //物料未维护生产车间
            if (string.Equals(e.BarItemKey.ToUpperInvariant(), "NoProduction", StringComparison.CurrentCultureIgnoreCase))
            {

                if (!check("物料未维护生产车间"))
                {
                    return;
                }
                //删除对应错误信息表中的数据
                string filter = getSelectedRowsElements("fid");
                string strSql = string.Format(@"/*dialect*/ Delete Allocationtable where fid in ({0})", filter);
                DBUtils.Execute(this.Context, strSql);
                //将接口待处理表对应数据状态置为0 等待抽数接口检查
                filter = getSelectedRowsElements("FBILLNO");
                strSql = string.Format(@"/*dialect*/ Update altablein set status=0 where id in ({0})", filter);
                DBUtils.Execute(this.Context, strSql);
            }
            //tbnoBillStaus5  
            //查询状态为5的，未生成单据号，单据号为空 
            if (string.Equals(e.BarItemKey.ToUpperInvariant(), "tbnoBillStaus5", StringComparison.CurrentCultureIgnoreCase))
            {
                
                string strSql = string.Format(@"/*dialect*/ update altablein set status=0   
     where   status=5   and fbillno=' '
  and id not in (
  select atl.id   from altablein atl 
 left join  (
  select spinentry.FSALENUMBER,spinentry.FLINENUMBER,spin.FBILLNO from T_SP_INSTOCK spin
  inner join T_SP_INSTOCKentry spinentry on spin.FID= spinentry.fid
  ) b on atl.salenumber=b.FSALENUMBER and atl.linenumber=b.FLINENUMBER

  where   atl.status=5   and atl.fbillno=' ' and b.FBILLNO is not null
  )
");
                DBUtils.Execute(this.Context, strSql);
            }
            //挂起
            // tbGQ
            if (string.Equals(e.BarItemKey.ToUpperInvariant(), "tbGQ", StringComparison.CurrentCultureIgnoreCase))
            {
                //获取选中行
                ListSelectedRowCollection selectRows = this.ListView.SelectedRowsInfo;
                //检查选中行数
                if (selectRows.Count() < 1)
                {
                    this.View.ShowErrMessage("请至少选中一条数据！");
                    return ;
                }
                string filter = getSelectedRowsFErrorBillNo("fid");
                string strSql = string.Format(@"/*dialect*/ update Allocationtable set FErrorBillNo='挂起' where fid in ({0})", filter);
                DBUtils.Execute(this.Context, strSql);
            }
            //反挂起
            // tbfGQ
            if (string.Equals(e.BarItemKey.ToUpperInvariant(), "tbfGQ", StringComparison.CurrentCultureIgnoreCase))
            {
                //获取选中行
                ListSelectedRowCollection selectRows = this.ListView.SelectedRowsInfo;
                //检查选中行数
                if (selectRows.Count() < 1)
                {
                    this.View.ShowErrMessage("请至少选中一条数据！");
                    return;
                }
                string filter = getSelectedRowsFErrorBillNo("fid");
                string strSql = string.Format(@"/*dialect*/ update Allocationtable set FErrorBillNo=' ' where fid in ({0})", filter);
                DBUtils.Execute(this.Context, strSql);
            }
            //

            //清除按钮：采购件无对应仓库、物料未维护生产车间、无对应销售订单，A,B,C 表清楚掉；问题错误表对应删掉
            if (string.Equals(e.BarItemKey.ToUpperInvariant(), "tbDELCQRK", StringComparison.CurrentCultureIgnoreCase))
            {
                //删除 调拨错误信息表数据

                string strSql = string.Format(@"/*dialect*/ delete   Allocationtable   where  FBILLNO in ( 
 select  id from altablein where ferrormsg in ('采购件无对应仓库','无对应销售订单','物料未维护生产车间')    and fbillno=' ' and status=2 )
  ");
                DBUtils.Execute(this.Context, strSql);
                // 删除 调拨 B表
                string strSql2 = string.Format(@"/*dialect*/ delete altable from altablein c where c.Salenumber=altable.Salenumber and c.Linenumber=altable.Linenumber and  c.ferrormsg in ('采购件无对应仓库','无对应销售订单','物料未维护生产车间')  and c.fbillno=' ' and c.status=2 ");

                DBUtils.Execute(this.Context, strSql2);

                // 删除 调拨 C表
                string strSql3 = string.Format(@"/*dialect*/ delete from altablein where ferrormsg in ('采购件无对应仓库','无对应销售订单','物料未维护生产车间') and fbillno=' ' and status=2 ");

                DBUtils.Execute(this.Context, strSql3);


            }

            //重置按钮：调拨C表中的状态为1，只生成简单生产入库单状态为审核中 调整设置为 0
            if (string.Equals(e.BarItemKey.ToUpperInvariant(), "tbStatus1", StringComparison.CurrentCultureIgnoreCase))
            {
                //重置 为0 将不是处理中的数据，处理日期不是当天的代表进程已经卡死了

                string strSql = string.Format(@"/*dialect*/ update   altablein set status=0 where status=1 and Fsubdate+1<getdate() and
   fbillno = ' '  
 ");
                DBUtils.Execute(this.Context, strSql);
                // 重置 未生成简单生产入库 如果生成请先删除掉
                string strSql2 = string.Format(@"/*dialect*/    update altablein  set status=0  where status=1 and Fsubdate+1<getdate() and
   fbillno  not in (
     select  fbillno from T_SP_INSTOCK
   )  and fbillno like 'JDSC%'
");

                DBUtils.Execute(this.Context, strSql2);

                // 重置 未生成直接调拨单 如果生成请先删除掉
                string strSql3 = string.Format(@"/*dialect*/     update altablein  set status=0   where status=1 and Fsubdate+1<getdate() and
   fbillno  not in (
     select  fbillno from T_STK_STKTRANSFERIN  
   )  and fbillno like 'ZJDB%'
 ");

                DBUtils.Execute(this.Context, strSql3);


            }
            // tbgz 45 问题45 ，销售订单单号为：8885449，第3行，异常列表无报错，调拨单未生成，入库C表中报采购件无对应仓库的错（8885792第2行）
            if (string.Equals(e.BarItemKey.ToUpperInvariant(), "tbgz", StringComparison.CurrentCultureIgnoreCase))
            {
                //删除 错误信息表对应的数据

                string strSql = string.Format(@"/*dialect*/  delete from Allocationtable where fbillno in (
 select id from altablein where status=2  and fbillno=' '  and ferrormsg  like '%关账%'
 )
");
                DBUtils.Execute(this.Context, strSql);
                // 重置 状态 为 0
                string strSql2 = string.Format(@"/*dialect*/    update altablein set status=0  where status=2  and fbillno=' '  and ferrormsg  like '%关账%'
");

                DBUtils.Execute(this.Context, strSql2);

            }
            //大于可调拨数量 重新抽取  tbdykdb

            //清除按钮：大于可调拨数量，重新抽取
            if (string.Equals(e.BarItemKey.ToUpperInvariant(), "tbdykdb", StringComparison.CurrentCultureIgnoreCase))
            {

                //获取选中行
                ListSelectedRowCollection selectRows = this.ListView.SelectedRowsInfo;
                //检查选中行数
                if (selectRows.Count() < 1)
                {
                    this.View.ShowErrMessage("请至少选中一条数据！");
                    return;
                }
                // 删除 调拨 B 表
                string filter = getSelectedRowsElements("FBILLNO");
                string strSql2 = string.Format(@"/*dialect*/ delete altable from altablein c where c.Salenumber=altable.Salenumber and c.Linenumber=altable.Linenumber and  c.ferrormsg in ('大于可调拨数量')   and c.status=2  and c.id in ({0}) ", filter);

                DBUtils.Execute(this.Context, strSql2);

                // 删除 调拨 C表
                string strSql3 = string.Format(@"/*dialect*/ delete   altablein  where   ferrormsg in ('大于可调拨数量')   and  status=2  and id in ({0}) ", filter );

                DBUtils.Execute(this.Context, strSql3);


                //删除 调拨错误信息表数据
                filter = getSelectedRowsFErrorBillNo("fid");
                string strSql = string.Format(@"/*dialect*/  delete from Allocationtable where REASON  in ('大于可调拨数量')     where fid in ({0}) ", filter);
                DBUtils.Execute(this.Context, strSql);


            }

            this.View.Refresh();
        }
        private Boolean checkData(String key)
        {
            if (string.Equals(key, "ReAudit", StringComparison.CurrentCultureIgnoreCase))
            {
                string filter = getSelectedRowsFErrorBillNo("FErrorBillNo");
                //查询无上游单据数据 写入错误信息表
                string strSql = string.Format(@"/*dialect*/	select distinct FBILLNO from T_STK_STKTRANSFERIN where FDOCUMENTSTATUS='B' and FBILLNO in ({0})", filter);
                DynamicObjectCollection FBILLNOCol = DBUtils.ExecuteDynamicObject(this.Context, strSql);
                if (FBILLNOCol.Count>0)
                {
                    string message = "单据编号为 ：";
                    for (int i = 0; i < FBILLNOCol.Count; i++)
                    {
                        message = message + Convert.ToString(FBILLNOCol[i]["FBILLNO"] + ",");
                    }
                    message.TrimEnd(',');
                    message = message + "的直接调拨单尚未审核！";
                    //e.Cancel = true;
                    this.View.ShowErrMessage(message);
                    return false;
                }
            }
            return true;
            
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
        private string getSelectedRowsFErrorBillNo(String key)
        {
            ListSelectedRowCollection selectRows = this.ListView.SelectedRowsInfo;
            string Elements = string.Empty;
            for (int i = 0; i < selectRows.Count(); i++)
            {
                Elements = Elements +"'"+ Convert.ToString(selectRows[i].DataRow[key]) +"'"+ ",";
            }
            Elements = Elements.TrimEnd(',');
            return Elements;
        }
        private Boolean check(String key)
        {
            //获取选中行
            ListSelectedRowCollection selectRows = this.ListView.SelectedRowsInfo;
            //检查选中行数
            if (selectRows.Count() < 1)
            {
                this.View.ShowErrMessage("请至少选中一条数据！");
                return false;
            }

            //判断问题类型
            if (string.Equals(key, "ReAudit", StringComparison.CurrentCultureIgnoreCase))
            {

                string Reason = "更新库存不成功！";
                for (int i = 1; i < selectRows.Count(); i++)
                {
                    if (Convert.ToString(selectRows[i].DataRow["FREASON"]).Length >= 8 && !string.Equals(Reason, GetLastStr(Convert.ToString(selectRows[i].DataRow["FREASON"]),8), StringComparison.CurrentCultureIgnoreCase))
                    {
                        this.View.ShowErrMessage("只能选择由于库存不足导致审核失败问题！");
                        return false;
                    }
                }
                return true;

            }
            else
            {
                //判断报错信息是否一致
                string Reason = Convert.ToString(selectRows[0].DataRow["FREASON"]);
                for (int i = 0; i < selectRows.Count(); i++)
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
        /// <summary>
        /// 获取后几位数
        /// </summary>
        /// <param name="str">要截取的字符串</param>
        /// <param name="num">返回的具体位数</param>
        /// <returns>返回结果的字符串</returns>
        public string GetLastStr(string str, int num)
        {
            int count = 0;
            if (str.Length > num)
            {
                count = str.Length - num;
                str = str.Substring(count, num);
            }
            return str;
        }
    }
}
