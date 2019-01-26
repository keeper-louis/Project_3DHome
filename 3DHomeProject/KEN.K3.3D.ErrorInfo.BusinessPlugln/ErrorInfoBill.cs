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
    public class ErrorInfoBill : AbstractListPlugIn
    {
        [Description("错误信息表列表插件")]

        public override void BarItemClick(BarItemClickEventArgs e)
        {
            //处理无上游单据
            if (String.Equals(e.BarItemKey.ToUpperInvariant(), "NoUpStream", StringComparison.CurrentCultureIgnoreCase))
            {
                //预检前端选择数据
                checkData();
                //删除对应错误信息表中的数据
                string filter = getSelectedRowsElements("fid");
                string strSql = string.Format(@"/*dialect*/ Delete Processtable where fid in ({0})", filter);
                DBUtils.Execute(this.Context, strSql);
                //将接口待处理表对应数据状态置为3 预检完成状态 供接口再次执行
                filter = getSelectedRowsElements("FBILLNO");
                strSql = string.Format(@"/*dialect*/ Update Prtablein set status=3 where id in ({0})", filter);
                DBUtils.Execute(this.Context, strSql);
             

            }
            //处理数量大于可汇报数量
            if (String.Equals(e.BarItemKey.ToUpperInvariant(), "OverReportableQua", StringComparison.CurrentCultureIgnoreCase))
            {
                //检查cloud工序汇报中是否还有没删除的单据，如果有残留单据不删除，没有的话删除

                string filter = getSelectedRowsElements("FBILLNO");

                //没有残留单据删除Processtable/prtablein/prtable 所有对应的数据
                string strSql = string.Format(@"/*dialect*/ select salenumber,linenumber,technicscode 
from  Processtable where exists (select  * from T_SFC_OPERPLANNING tso, T_SFC_OPERPLANNINGDETAIL tsod ,T_SFC_OPERPLANNINGDETAIL_B tsodb
 where tsodb.FDETAILID=tsod.FDETAILID
 and tso.fid=tsodb.FENTRYID and tso.FSALEORDERNUMBER=Processtable.salenumber 
 and tso.FSALEORDERENTRYSEQ=Processtable.linenumber and tsod.FOPERNUMBER=Processtable.technicscode and FReportBaseQty=0)
 and Processtable.FBILLNO in ({0})", filter);
                DynamicObjectCollection periodColl = DBUtils.ExecuteDynamicObject(this.Context, strSql);
                if (periodColl.Count != 0) {
                    for (int i = 0; i < periodColl.Count; i++)
                    {
                        strSql = string.Format(@"/*dialect*/ delete Processtable 
where salenumber='{0}' and linenumber='{1}' and technicscode='{2}')"
, Convert.ToString(periodColl[i]["salenumber"]), Convert.ToString(periodColl[i]["linenumber"]), Convert.ToString(periodColl[i]["technicscode"]));
                        DBUtils.Execute(this.Context, strSql);
                        strSql = string.Format(@"/*dialect*/ delete prtablein 
where salenumber='{0}' and linenumber='{1}' and technicscode='{2}')"
, Convert.ToString(periodColl[i]["salenumber"]), Convert.ToString(periodColl[i]["linenumber"]), Convert.ToString(periodColl[i]["technicscode"]));
                        DBUtils.Execute(this.Context, strSql);
                        strSql = string.Format(@"/*dialect*/ delete prtable 
where salenumber='{0}' and linenumber='{1}' and technicscode='{2}')"
, Convert.ToString(periodColl[i]["salenumber"]), Convert.ToString(periodColl[i]["linenumber"]), Convert.ToString(periodColl[i]["technicscode"]));
                        DBUtils.Execute(this.Context, strSql);
                    }

                }


                //查询有残留单据的数据 并提示
                        strSql = string.Format(@"/*dialect*/ select Processtable.fbillno
  from Processtable where not exists (select  * from T_SFC_OPERPLANNING tso, T_SFC_OPERPLANNINGDETAIL tsod ,T_SFC_OPERPLANNINGDETAIL_B tsodb
 where tsodb.FDETAILID=tsod.FDETAILID
 and tso.fid=tsodb.FENTRYID and tso.FSALEORDERNUMBER=Processtable.salenumber 
 and tso.FSALEORDERENTRYSEQ=Processtable.linenumber and tsod.FOPERNUMBER=Processtable.technicscode and FReportBaseQty=0)
 and Processtable.FBILLNO in ({0})", filter);
                periodColl = DBUtils.ExecuteDynamicObject(this.Context, strSql);
                if (periodColl.Count != 0)
                {
                    string message = "单据编号为 ：";
                    for (int i = 0; i < periodColl.Count; i++)
                    {
                        message = message + Convert.ToString(periodColl[i]["fbillno"] +",");
                    }
                    message.TrimEnd(',');
                    message = message + "的报错信息 在工序汇报中有尚未删除的单据，请删除后再试！";
                    //e.Cancel = true;
                    this.View.ShowErrMessage(message);
                    //return;
                }
            }


            //处理sacntime小于下达日期
            if (String.Equals(e.BarItemKey.ToUpperInvariant(), "SantimeLessThanIssueDate", StringComparison.CurrentCultureIgnoreCase))
            {

                //预检前端选择数据
                checkData();
                //删除对应错误信息表中的数据
                string filter = getSelectedRowsElements("fid");
                string strSql = string.Format(@"/*dialect*/ Delete Processtable where fid in ({0})", filter);
                DBUtils.Execute(this.Context, strSql);
                //将接口待处理表对应数据状态置为3 预检完成状态 供接口再次执行
                filter = getSelectedRowsElements("FBILLNO");
                strSql = string.Format(@"/*dialect*/ Update Prtablein set status=3 where id in ({0})", filter);
                DBUtils.Execute(this.Context, strSql);

            }
            this.View.Refresh();

        }

        private void checkData()
        {
            string filter = getSelectedRowsElements("FBILLNO");
            //校验选中数据是否无上游单据 并写入错误信息表
            string strSql = string.Format(@"/*dialect*/INSERT INTO Processtable select id FBILLNO,'A' FDOCUMENTSTATUS, salenumber SALENUMBER,linenumber LINENUMBER,technicscode TECHNICSCODE,id PRTABLEINID,
'无上游单据' REASON,state STATE,fdate FDATE,getdate() FSUBDATE from  Prtablein 
 where not EXISTS (select  tso.FSALEORDERNUMBER,tso.FSALEORDERENTRYSEQ
 from T_SFC_OPERPLANNING tso , T_SFC_OPERPLANNINGDETAIL tsod ,T_SFC_OPERPLANNINGDETAIL_B tsodb
 where tsodb.FDETAILID=tsod.FDETAILID and tso.fid=tsodb.FENTRYID 
and Linenumber=tso.FSALEORDERENTRYSEQ and Salenumber=tso.FSALEORDERNUMBER and technicscode=tsod.FOperNumber ) and State<>3 and status<>0
and Prtablein.id in ({0})
", filter);
            DBUtils.Execute(this.Context, strSql);
            //校验选中数据的数量是否大于可汇报数量 并写入错误信息表
            strSql = string.Format(@"/*dialect*/INSERT INTO Processtable select id FBILLNO,'A' FDOCUMENTSTATUS, Prtablein.salenumber SALENUMBER,Prtablein.linenumber LINENUMBER,Prtablein.technicscode TECHNICSCODE,id PRTABLEINID,
'大于可汇报数量' REASON,state STATE,fdate FDATE,getdate() FSUBDATE  from Prtablein ,
 (select a.Salenumber,a.Linenumber,a.Technicscode 
 from (select pri.Salenumber,pri.Linenumber,pri.Technicscode,sum(pri.Amount) amount from Prtablein pri 
,(select * from Prtablein where id in ({0}) )k
 where pri.status<>2 and pri.status<>4 and pri.salenumber=k.salenumber and 
 pri.linenumber=k.linenumber and pri.Technicscode=k.Technicscode group by  pri.Salenumber,pri.Linenumber,pri.Technicscode) a,
  (select  tso.FSALEORDERNUMBER salenumber,tso.FSALEORDERENTRYSEQ linenumber,tsod.FOPERNUMBER technicscode,(FOPERQTY-FREPORTBASEQTY) amount
 from T_SFC_OPERPLANNING tso, T_SFC_OPERPLANNINGDETAIL tsod ,T_SFC_OPERPLANNINGDETAIL_B tsodb
 where tsodb.FDETAILID=tsod.FDETAILID
 and tso.fid=tsodb.FENTRYID )b where a.Salenumber=b.salenumber and a.Linenumber=b.linenumber and a.Technicscode=b.technicscode and a.amount>b.amount) c
 where Prtablein.Salenumber=c.Salenumber and Prtablein.Linenumber=c.Linenumber and Prtablein.Technicscode=c.Technicscode and Prtablein.id in ({0}) 
", filter);
            DBUtils.Execute(this.Context, strSql);
            //校验选中数据的sacntime是否小于下达日期数据 并写入错误信息表
            strSql = string.Format(@"/*dialect*/INSERT INTO Processtable select id FBILLNO,'A' FDOCUMENTSTATUS, pr.salenumber SALENUMBER,pr.linenumber LINENUMBER,pr.technicscode TECHNICSCODE,id PRTABLEINID,
'sacntime小于下达日期' REASON,state STATE,fdate FDATE,getdate() FSUBDATE  
 from Prtablein pr,(select FPlanStartDate ,tso.FSaleOrderNumber,tso.FSaleOrderEntrySeq 
 from T_PRD_MO trm,T_PRD_MOENTRY  trme ,T_SFC_OPERPLANNING tso where trm.fid=trme.fid
 and  trm.FBILLNO=tso.FMONumber and tso.FMOEntrySeq=trme.fseq ) a
 where pr.fdate<a.FPLANSTARTDATE and pr.salenumber=a.FSaleOrderNumber and pr.linenumber=a.FSaleOrderEntrySeq  
and Pr.id in ({0})", filter);

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
    }
}

