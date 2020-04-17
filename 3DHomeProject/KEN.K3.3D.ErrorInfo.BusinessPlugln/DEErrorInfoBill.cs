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
    public class DEErrorInfoBill : AbstractListPlugIn
    {
        [Description("出库接口错误信息表列表插件")]

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
                string strSql = string.Format(@"/*dialect*/ Delete Deliverytable where fid in ({0})", filter);
                DBUtils.Execute(this.Context, strSql);
                //将接口待处理表对应数据状态置为0 等待抽数接口检查
                filter = getSelectedRowsElements("FBILLNO");
                strSql = string.Format(@"/*dialect*/ Update detablein set status=0 where id in ({0})", filter);
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
                string strSql = string.Format(@"/*dialect*/ Delete Deliverytable where fid in ({0})", filter);
                DBUtils.Execute(this.Context, strSql);
                //将接口待处理表对应数据状态置为0 等待抽数接口检查
                filter = getSelectedRowsElements("FBILLNO");
                strSql = string.Format(@"/*dialect*/ Update detablein set status=0 where id in ({0})", filter);
                DBUtils.Execute(this.Context, strSql);
            }
            //重新审核
            if (string.Equals(e.BarItemKey.ToUpperInvariant(), "ReAudit", StringComparison.CurrentCultureIgnoreCase))
            {
                if (!check("更新库存出现异常情况，更新库存不成功！"))
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
                string filterid = getSelectedRowsElements("fid");
                string strSql = string.Format(@"/*dialect*/ Delete Deliverytable  where fid in ({0}) ", filterid);
                DBUtils.Execute(this.Context, strSql);
                //将接口待处理表对应数据状态置为5 3D业务人员手工审核成功
                strSql = string.Format(@"/*dialect*/ Update detablein set status=5,ferrormsg='3D业务人员手工审核成功',fsubdate=GETDATE() where fbillno in ({0}) and status=2 ", filter);
                DBUtils.Execute(this.Context, strSql);
            }
            //大于可出库数量
            if (string.Equals(e.BarItemKey.ToUpperInvariant(), "OverOutStockQua", StringComparison.CurrentCultureIgnoreCase))
            {
                if (!check("大于可出库数量"))
                {
                    return;
                }
                //预检前端选择数据
                if (!checkData("OverOutStockQua"))
                {
                    return;
                }
                //删除对应错误信息表中的数据
                ListSelectedRowCollection selectRows = this.ListView.SelectedRowsInfo;
                for (int i = 0; i < selectRows.Count(); i++)
                {

                    string strSql = string.Format(@"/*dialect*/Delete Deliverytable where Salenumber='{0}' and Linenumber='{1}'",
Convert.ToString(selectRows[i].DataRow["FSalenumber"]), Convert.ToString(selectRows[i].DataRow["FLinenumber"]));
                    DBUtils.Execute(this.Context, strSql);
                    strSql = string.Format(@"/*dialect*/Delete detablein where Salenumber='{0}' and Linenumber='{1}'",
Convert.ToString(selectRows[i].DataRow["FSalenumber"]), Convert.ToString(selectRows[i].DataRow["FLinenumber"]));
                    DBUtils.Execute(this.Context, strSql);
                    strSql = string.Format(@"/*dialect*/Delete detable where Salenumber='{0}' and Linenumber='{1}'",
Convert.ToString(selectRows[i].DataRow["FSalenumber"]), Convert.ToString(selectRows[i].DataRow["FLinenumber"]));
                    DBUtils.Execute(this.Context, strSql);
                }

            }
            //lc add 查询状态为5的，未生成单据号，单据号为空 
            if (string.Equals(e.BarItemKey.ToUpperInvariant(), "tbstatusnobill", StringComparison.CurrentCultureIgnoreCase))
            {
                string strSql = string.Format(@"/*dialect*/   update detablein set status=0 where status=5 and ( fbillno=' ' or fbillno='0' )
  and not id in (
  select detl.id  from detablein detl,
  (select  c.FSRCBILLNO,a.FSRCBILLSEQ 
  from T_SAL_OUTSTOCKENTRY a
  inner join T_SAL_OUTSTOCK b on a.FID=b.fid
  left join T_SAL_OUTSTOCKENTRY_R c on a.FENTRYID=c.FENTRYID
  ) outbill where detl.status=5 and detl.fbillno=' ' and detl.Salenumber=outbill.FSRCBILLNO and detl.Linenumber=outbill.FSRCBILLSEQ
  )

");
                DBUtils.Execute(this.Context, strSql);
            }
            // lc tbOutStatus5 这些已经生成出库单 状态设置为5 错误信息表也删掉；
            if (string.Equals(e.BarItemKey.ToUpperInvariant(), "tbOutStatus5", StringComparison.CurrentCultureIgnoreCase))
            {
                //先删除 出库错误信息表
                string strSql = string.Format(@"/*dialect*/   
 delete  from Deliverytable where DETABLEINID in 
  (
select     id from detablein din
left join  (select c.FSRCBILLNO,a.FSRCBILLSEQ,b.FBILLNO from  
  T_SAL_OUTSTOCKENTRY a  
  inner join T_SAL_OUTSTOCK b on a.FID=b.fid
  left join T_SAL_OUTSTOCKENTRY_R c on a.FENTRYID=c.FENTRYID ) b
  on din.Salenumber=b.FSRCBILLNO and din.Linenumber=b.FSRCBILLSEQ
  where din.status=2 and din.ferrormsg='大于可出库数量' and b.FBILLNO is not     null

  )



");
                DBUtils.Execute(this.Context, strSql);

               strSql = string.Format(@"/*dialect*/     update detablein set status=5  where id in ( select din.id  from  detablein din
left join  (select c.FSRCBILLNO,a.FSRCBILLSEQ,b.FBILLNO from  
  T_SAL_OUTSTOCKENTRY a  
  inner join T_SAL_OUTSTOCK b on a.FID=b.fid
  left join T_SAL_OUTSTOCKENTRY_R c on a.FENTRYID=c.FENTRYID ) b
  on din.Salenumber=b.FSRCBILLNO and din.Linenumber=b.FSRCBILLSEQ
  where din.status=2 and din.ferrormsg='大于可出库数量' and b.FBILLNO is not     null

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
                    return;
                }
                string filter = getSelectedRowsFErrorBillNo("fid");
                string strSql = string.Format(@"/*dialect*/ update Deliverytable set FErrorBillNo='挂起' where fid in ({0})", filter);
                DBUtils.Execute(this.Context, strSql);
            }
            // tbGQ 反挂起
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
                string strSql = string.Format(@"/*dialect*/ update Deliverytable set FErrorBillNo=' ' where fid in ({0})", filter);
                DBUtils.Execute(this.Context, strSql);
            }
            //清除按钮：采购件无对应仓库、物料未维护生产车间、无对应销售订单，A,B,C 表清楚掉；问题错误表对应删掉
            if (string.Equals(e.BarItemKey.ToUpperInvariant(), "tbDELCQRK", StringComparison.CurrentCultureIgnoreCase))
            {
                //删除 出库错误信息表数据

                string strSql = string.Format(@"/*dialect*/  delete Deliverytable   where  FBILLNO in  (
    select   id from detablein where ferrormsg in('采购件无对应仓库','无对应销售订单','物料未维护生产车间')   and status=2  ) ");
                DBUtils.Execute(this.Context, strSql);
                // 删除 出库 B表
                string strSql2 = string.Format(@"/*dialect*/ 	delete detable from  						
							detablein c where c.Salenumber=detable.Salenumber and c.Linenumber=detable.Linenumber and  c.ferrormsg in('物料未维护生产车间','无对应销售订单')  and c.status=2
 ");

                DBUtils.Execute(this.Context, strSql2);

                // 删除 出库 C表
                string strSql3 = string.Format(@"/*dialect*/  delete from  detablein where ferrormsg in('采购件无对应仓库','无对应销售订单','物料未维护生产车间')   and status=2  ");

                DBUtils.Execute(this.Context, strSql3);


            }

            //重置按钮：无对应销售订单：81700101-00504-0    出库C表状态为1     无出库单生成
            if (string.Equals(e.BarItemKey.ToUpperInvariant(), "tbStatus1", StringComparison.CurrentCultureIgnoreCase))
            {
                //重置 为0 将不是处理中的数据，处理日期不是当天的代表进程已经卡死了

                string strSql = string.Format(@"/*dialect*/  update detablein set status=0 where status =1 and Fsubdate+0.5<getdate() and  ( fbillno=' ' or fbillno='0' )
 ");
                DBUtils.Execute(this.Context, strSql);
                // 重置 未生成出库单 如果生成请先删除掉
                string strSql2 = string.Format(@"/*dialect*/     update detablein set status=0  where status =1 and Fsubdate+0.5<getdate() 
 and fbillno not in (
   select fbillno from T_SAL_OUTSTOCK  
 ) and fbillno like 'XSCK%'
");

                DBUtils.Execute(this.Context, strSql2);

            }
            //20200320 删除关账错误数据 lcadd
            if (string.Equals(e.BarItemKey.ToUpperInvariant(), "btndelcloseGZ", StringComparison.CurrentCultureIgnoreCase))
            {
                string strSql = string.Format(@"/*dialect*/  delete from  Deliverytable where reason  like  '%关账%'  ");
                DBUtils.Execute(this.Context, strSql);
            }
            // 重置 辅助属性错误的 设置状态为0 辅助属性错误数据重置0 add lc 20200409 
            if (string.Equals(e.BarItemKey.ToUpperInvariant(), "btnresetFz", StringComparison.CurrentCultureIgnoreCase))
            {
              
                //获取选中行
                ListSelectedRowCollection selectRows = this.ListView.SelectedRowsInfo;
                //检查选中行数
                if (selectRows.Count() < 1)
                {
                    this.View.ShowErrMessage("请至少选中一条数据！");
                    return;
                }
                // 辅助属性错误的 设置状态为0
                string filter = getSelectedRowsElements("FBILLNO");
                string strSql = string.Format(@"/*dialect*/ Update detablein set status=0  , FErrorStatus =0 ,ferrormsg ='' where ferrormsg like '%辅助属性不能为空%' and   id    in ({0}) and status=2 ", filter);
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
                string strSql = string.Format(@"/*dialect*/	select distinct FBILLNO from T_SAL_OUTSTOCK where FDOCUMENTSTATUS='B' and FBILLNO in ({0})", filter);
                DynamicObjectCollection FBILLNOCol = DBUtils.ExecuteDynamicObject(this.Context, strSql);
                if (FBILLNOCol.Count > 0)
                {
                    string message = "单据编号为 ：";
                    for (int i = 0; i < FBILLNOCol.Count; i++)
                    {
                        message = message + Convert.ToString(FBILLNOCol[i]["FBILLNO"] + ",");
                    }
                    message.TrimEnd(',');
                    message = message + "的销售出库单尚未审核！";
                    //e.Cancel = true;
                    this.View.ShowErrMessage(message);
                    return false;
                }

            }
            if (string.Equals(key, "OverOutStockQua", StringComparison.CurrentCultureIgnoreCase))
            {

                ListSelectedRowCollection selectRows = this.ListView.SelectedRowsInfo;
                string message = "单据编号为 ：";
                int count = 0;
                for (int i = 0; i < selectRows.Count(); i++)
                {
                    string strSql = string.Format(@"/*dialect*/select distinct tso.FBILLNO FBILLNO from T_SAL_OUTSTOCK tso,T_SAL_OUTSTOCKENTRY_r tsoer ,T_SAL_OUTSTOCKENTRY tsoe 
where tso.fid=tsoe.fid and tso.fid=tsoer.fid and tsoe.FENTRYID=tsoer.FENTRYID and tsoer.FSRCBILLNO='{0}' and tsoe.Fsrcbillseq='{1}'",
                    Convert.ToString(selectRows[i].DataRow["FSalenumber"]), Convert.ToString(selectRows[i].DataRow["FLinenumber"]));

                    DynamicObjectCollection FBILLNOCol = DBUtils.ExecuteDynamicObject(this.Context, strSql);

                    if (FBILLNOCol.Count > 0)
                    {
                        count++;
                        for (int k = 0; k < FBILLNOCol.Count; k++)
                        {
                            message = message + Convert.ToString(FBILLNOCol[k]["FBILLNO"] + ",");
                        }

                    }

                }
                if (count > 0)
                {
                    message.TrimEnd(',');
                    message = message + "的销售出库单尚未删除！";
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
                Elements = Elements + "'" + Convert.ToString(selectRows[i].DataRow[key]) + "'" + ",";
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
                    if (Convert.ToString(selectRows[i].DataRow["FREASON"]).Length >= 8 && !string.Equals(Reason, GetLastStr(Convert.ToString(selectRows[i].DataRow["FREASON"]), 8), StringComparison.CurrentCultureIgnoreCase))
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
