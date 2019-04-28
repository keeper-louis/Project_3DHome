﻿using Kingdee.BOS;
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
                string strSql = string.Format(@"/*dialect*/ Delete Deliverytable where FErrorBillNo in ({0})", filter);
                DBUtils.Execute(this.Context, strSql);
                //将接口待处理表对应数据状态置为5 3D业务人员手工审核成功
                strSql = string.Format(@"/*dialect*/ Update detablein set status=5,ferrormsg='3D业务人员手工审核成功' where fbillno in ({0})", filter);
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