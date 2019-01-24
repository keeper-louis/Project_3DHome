using Kingdee.BOS.App.Data;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Orm.DataEntity;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace KEN.K3._3D.ErrorInfo.ServicePlugln
{
    [Description("处理无上游单据操作")]
    public class NoUpStream : AbstractOperationServicePlugIn
    {
        public override void OnPreparePropertys(Kingdee.BOS.Core.DynamicForm.PlugIn.Args.PreparePropertysEventArgs e)
        {

        }
        public override void BeginOperationTransaction(Kingdee.BOS.Core.DynamicForm.PlugIn.Args.BeginOperationTransactionArgs e)
        {
            if (e.DataEntitys != null && e.DataEntitys.Count<DynamicObject>() > 0)
            {
                foreach (DynamicObject item in e.DataEntitys)
                {




                    long id = Convert.ToInt64(item["id"]);
                    long custId = Convert.ToInt64(item["FCUSTOMERID_Id"]);
                    long orgId = Convert.ToInt64(item["F_PAEZ_OrgId_Id"]);
                    string beginDate = Convert.ToDateTime(item["FBeginDate"]).ToString();
                    DateTime endDate = Convert.ToDateTime(item["FEndDate"]);
                    string bfYear = Convert.ToString(endDate.Year - 1);//上一年
                    string strSql = string.Format(@"/*dialect*/select FRECEIVEAMOUNT,FREFUNDBILL,FJXAMOUNT,FSOAMOUNT,FREBATAMOUNT,FSALERETURNAMOUNT,FALLAMOUNT,FBalance from K_CS_FINALSTATEMENT where FCSYEAR = {0} and F_PAEZ_OrgId = {1} and FCUSTOMERID = {2}", bfYear, orgId, custId);
                    DynamicObjectCollection queryResult = DBUtils.ExecuteDynamicObject(this.Context, strSql);
                    if (queryResult.Count != 0)
                    {
                        //上一年存在年结单
                    }
                    else
                    {
                        //上一年不存在年结单
                        beginDate = "1970/1/1 0:00:00";
                    }

                    //获取上一年年结单中各个指标的数据
                    //Decimal RECEIVEAMOUN = Convert.ToDecimal(queryResult[0]["FRECEIVEAMOUNT"]);//累计收款
                    //Decimal JXAMOUNT = Convert.ToDecimal(queryResult[0]["FJXAMOUNT"]);//累计利息
                    //Decimal SOAMOUNT = Convert.ToDecimal(queryResult[0]["FSOAMOUNT"]);//累计订单
                    //Decimal REBATAMOUNT = Convert.ToDecimal(queryResult[0]["FREBATAMOUNT"]);//累计返利费用
                    //Decimal SALERETURNAMOUNT = Convert.ToDecimal(queryResult[0]["FSALERETURNAMOUNT"]);//累计退货
                    //Decimal ALLAMOUNT = Convert.ToDecimal(queryResult[0]["FALLAMOUNT"]);//累计应收费用
                    //今年各项指标的数据
                    //获取临时表名
                    TimeSpan ts = DateTime.Now.ToUniversalTime() - new DateTime(1970, 1, 1);
                    string tem = ts.TotalMilliseconds.ToString();
                    int index = tem.IndexOf('.');
                    string tp = tem.Substring(0, index);
                    string em = tem.Substring(index + 1, tem.Length - 1 - index);
                    string tmpTableName = "TMP" + tp + em;
                    string createSql = string.Format(@"/*dialect*/ create table {0} (FSeq bigint,FBILLNO nvarchar(255),FormId nvarchar(255),
                                            FDATE datetime ,FCUSTOMERID int,FORGID int,FAMOUNT decimal(38, 10),
                                            FCUSTOMERNUMBER nvarchar(255),FORMNAME nvarchar(255),FCUSTOMERNAME nvarchar(255))", tmpTableName);

                    DBUtils.Execute(this.Context, createSql);
                    string strSql_1 = string.Format(@"/*dialect*/
                                   insert into {0} Select distinct ROW_NUMBER() OVER(order by  FBILLNO) AS FSeq,
                                    C.* 
                                    ,TMPC.FNUMBER AS FCUSTOMERNUMBER
                                    ,OL.FNAME AS FORMNAME
                                    ,TMPCL.FNAME AS FCUSTOMERNAME
                                    FROM (
										SELECT
                                        R.FBILLNO,
                                        'AR_RECEIVEBILL' AS FormId,
                                        R.FDATE,
                                        R.FCONTACTUNIT AS FCUSTOMERID,
										r.FSETTLEORGID AS FORGID,
                                        SUM(RE.FRECTOTALAMOUNTFOR) AS FAMOUNT
                                        FROM T_AR_RECEIVEBILL R
                                        INNER JOIN  T_AR_RECEIVEBILLENTRY RE ON R.FID=RE.FID
                                        WHERE R.FBILLTYPEID='36cf265bd8c3452194ed9c83ec5e73d2' 
                                        AND R.FDOCUMENTSTATUS='C'
                                        AND R.FCONTACTUNITTYPE='BD_Customer'
                                        GROUP BY R.FBILLNO,R.FDATE,R.FCONTACTUNIT ,FSETTLEORGID 

                                        UNION ALL

                                        SELECT
                                        R.FBILLNO,
                                        'AR_REFUNDBILL' AS FormId,
                                        R.FDATE,
                                        R.FCONTACTUNIT AS FCUSTOMERID,
										r.FSETTLEORGID AS FORGID,
                                        SUM(RE.FREFUNDAMOUNTFOR) AS FAMOUNT
                                        FROM T_AR_REFUNDBILL R
                                        INNER JOIN  T_AR_REFUNDBILLENTRY RE ON R.FID=RE.FID
                                        WHERE  R.FDOCUMENTSTATUS='C'
                                        AND R.FCONTACTUNITTYPE='BD_Customer'
                                        GROUP BY R.FBILLNO,R.FDATE,R.FCONTACTUNIT ,FSETTLEORGID 

                                        UNION ALL

                                        SELECT 
                                        T1.FBILLNO,
                                        'KLM_SKJXSUMBILL' AS FormId,
                                        T1.FREQDATE AS FDATE,
                                        T2.FKLMCUSTOMER AS FCUSTOMERID,
                                        t2.FKLMORGID AS FORGID,
                                        SUM(T2.FKLMSUMAMOUNT) AS FAMOUNT
                                        FROM T_KLMSKJXSUM T1 
                                        INNER JOIN T_KLMSKJXSUMENTRY T2 ON T1.FID=T2.FID
                                        WHERE T1.FDOCUMENTSTATUS='C' 
                                        GROUP BY T1.FBILLNO,T1.FREQDATE,T2.FKLMCUSTOMER,t2.FKLMORGID

                                        UNION ALL

                                        SELECT 
                                        R.FBILLNO,
                                        'SAL_RETURNSTOCK' AS FormId,
										R.FDATE,
										R.FRETCUSTID AS FCUSTOMERID,
										FSALEORGID AS FORGID,
                                        SUM(RE.FREALQTY*REF.FTAXPRICE) AS FAMOUNT
                                        FROM  T_SAL_RETURNSTOCK R 
                                        INNER JOIN T_SAL_RETURNSTOCKENTRY RE ON  R.FID=RE.FID
                                        INNER JOIN T_SAL_RETURNSTOCKENTRY_F REF ON RE.FENTRYID=REF.FENTRYID
                                        WHERE R.FDOCUMENTSTATUS='C' 
                                        GROUP BY R.FBILLNO,R.FDATE,R.FRETCUSTID,FSALEORGID

                                        UNION ALL

                                        SELECT   
                                         SO.FBILLNO,
                                        'SAL_SaleOrder' AS FormId,
                                        SO.FDATE,SO.FCUSTID AS FCUSTOMERID,
										SO.FSALEORGID AS FORGID,
                                        SUM(SOEF.FALLAMOUNT) AS FAMOUNT
                                        FROM T_SAL_ORDER SO 
                                        INNER JOIN  T_SAL_ORDERENTRY SOE ON SO.FID=SOE.FID
                                        INNER JOIN  T_SAL_ORDERENTRY_F SOEF ON SOE.FENTRYID=SOEF.FENTRYID
                                        WHERE SO.FDOCUMENTSTATUS='C'
                                        AND SO.FCANCELSTATUS='A' AND SOE.FENTRYID NOT IN(
                                            SELECT  T5.FENTRYID
                                                FROM T_SAL_OUTSTOCK T1
                                                INNER JOIN T_SAL_OUTSTOCKENTRY T2 ON T1.FID=T2.FID
                                                INNER JOIN T_SAL_OUTSTOCKENTRY_R T2R ON T2.FENTRYID=T2R.FENTRYID
                                                INNER JOIN T_AR_RECEIVABLEENTRY_LK T3LK ON T3LK.FSID=T2.FENTRYID AND T3LK.FSBILLID=T2.FID AND T3LK.FSTABLENAME='T_SAL_OUTSTOCKENTRY'
                                                INNER JOIN T_AR_RECEIVABLEENTRY T3 ON T3.FENTRYID=T3LK.FENTRYID
                                                INNER JOIN T_AR_RECEIVABLE T4 ON T3.FID=T4.FID AND T4.FSETACCOUNTTYPE='3'
                                                INNER JOIN T_SAL_ORDERENTRY T5 ON T5.FENTRYID=T2R.FSOENTRYID
                                                INNER JOIN T_SAL_ORDER T6 ON T6.FBILLNO=T2R.FSOORDERNO
                                        )                                      
                                        GROUP BY SO.FBILLNO,SO.FDATE,SO.FCUSTID,SO.FSALEORGID
                                        
                                        UNION ALL

                                        SELECT   
                                        SO.FBILLNO,
                                        'SAL_OUTSTOCK' AS FormId,
                                        SO.FDATE,SO.FCustomerID AS FCUSTOMERID,
										SO.FSALEORGID AS FORGID,
                                        SUM(SOEF.FALLAMOUNT) AS FAMOUNT
                                        FROM T_SAL_OUTSTOCK SO 
                                        INNER JOIN  T_SAL_OUTSTOCKENTRY SOE ON SO.FID=SOE.FID
                                        INNER JOIN T_SAL_OUTSTOCKENTRY_R SOER ON SOE.FENTRYID=SOER.FENTRYID
                                        INNER JOIN  T_SAL_OUTSTOCKENTRY_F SOEF ON SOE.FENTRYID=SOEF.FENTRYID
                                        WHERE SO.FDOCUMENTSTATUS='C'
                                        AND SO.FCANCELSTATUS='A'AND SOER.FSRCTYPE ='' 
                                        AND SOE.FENTRYID NOT IN(
                                        SELECT DISTINCT T2.FENTRYID
                                        FROM T_SAL_OUTSTOCK T1
                                        INNER JOIN T_SAL_OUTSTOCKENTRY T2 ON T1.FID=T2.FID
                                        INNER JOIN T_SAL_OUTSTOCKENTRY_R T2R ON T2.FENTRYID=T2R.FENTRYID
                                        INNER JOIN T_AR_RECEIVABLEENTRY_LK T3LK ON T3LK.FSID=T2.FENTRYID AND T3LK.FSBILLID=T2.FID AND T3LK.FSTABLENAME='T_SAL_OUTSTOCKENTRY'
                                        INNER JOIN T_AR_RECEIVABLEENTRY T3 ON T3.FENTRYID=T3LK.FENTRYID
                                        INNER JOIN T_AR_RECEIVABLE T4 ON T3.FID=T4.FID
                                        WHERE FSRCTYPE ='' and t4.FSETACCOUNTTYPE='3')
                                        GROUP BY SO.FBILLNO,SO.FDATE,SO.FCustomerID,SO.FSALEORGID

                                        UNION ALL

                                        SELECT 
                                        R.FBILLNO,
                                        'KLM_RebateBill' AS FormId,
                                        R.FDATE,
                                        R.FCUSTOMERID AS FCUSTOMERID, 
                                        R.F_KLM_ORGID AS FORGID,
                                        SUM(FAMOUNT) AS FAMOUNT
                                        FROM  T_KLM_RebateBill R 
                                        GROUP BY R.FBILLNO,R.FDATE,R.FCUSTOMERID,R.F_KLM_ORGID

                                        UNION ALL

                                        select FBILLNO,'AR_receivable' as FormId,FDATE,FCUSTOMERID,FPAYORGID AS FORGID,sum(FALLAMOUNT) as FAMOUNT
                                        from t_AR_receivable TAR,t_AR_receivableFIN TARF 
                                        where TAR.FID=TARF.FID and FBILLTYPEID='00505694799c955111e325bc9e6eb056' and FDOCUMENTSTATUS='c' 
                                        GROUP BY FBILLNO,FDATE,FCUSTOMERID,FPAYORGID

                                        UNION ALL

                                        select FBILLNO,'PAEZ_Balance_Aadjustment' as FormId, FDATE,FCUSTOMERID,F_PAEZ_OrgId as FORGID , sum(FAmount) as FAMOUNT
                                        from K_CS_CAUSEOfADJUSTMENT 
                                        where FDOCUMENTSTATUS = 'c'
                                        GROUP BY FBILLNO,FDATE,FCUSTOMERID,F_PAEZ_OrgId

                                      ) AS C
                                    INNER JOIN T_BD_CUSTOMER TMPC ON C.FCUSTOMERID=TMPC.FCUSTID
                                    INNER JOIN T_BD_CUSTOMER_L TMPCL ON TMPC.FCUSTID=TMPCL.FCUSTID AND  TMPCL.FLOCALEID=2052
                                    INNER JOIN T_META_OBJECTTYPE_L OL ON C.FORMID=OL.FID AND OL.FLOCALEID=2052

                                    where tmpc.FCUSTID={1} and c.FDATE>= '{2}' and c.FDATE<= '{3}' and c.FORGID ={4}
                                    ", tmpTableName, custId, beginDate, endDate, orgId);

                    DBUtils.Execute(this.Context, strSql_1);

                    string strSql_2 = string.Format(@"/*dialect*/
                                    SELECT 
                                    FRebatAMOUNT + {0} as FRebatAMOUNT,
                                    FREFUNDBILL + {8} as FREFUNDBILL,
                                    FSOAMOUNT + {1} as FSOAMOUNT,
                                    FReceiveAMOUNT+ {2} as FReceiveAMOUNT,
                                    FSaleReturnAMOUNT+ {3} as FSaleReturnAMOUNT,
                                    FJXAMOUNT+ {4} as FJXAMOUNT,
                                    Freceivable+ {5} as Freceivable,
                                    FBalance+{6} as FBalance
                                    FROM ( 
                                    select FCUSTOMERName,FCUSTOMERNUMBER,FCUSTOMERID,FORGID,
                                    cast(sum(case when formId='KLM_RebateBill'  then fAmount else 0 end ) as decimal(18,2)) as FRebatAMOUNT,
                                    cast(sum(case when formId='AR_REFUNDBILL'  then fAmount else 0 end ) as decimal(18,2)) as FREFUNDBILL,
                                    cast(sum(case when formId='SAL_SaleOrder'  then fAmount else 0 end ) as decimal(18,2)) as FSOAMOUNT,
                                    cast(sum(case when formId='AR_RECEIVEBILL' then fAmount else 0 end ) as decimal(18,2)) as FReceiveAMOUNT,
                                    cast(sum(case when formId='SAL_RETURNSTOCK' then fAmount else 0 end ) as decimal(18,2)) as FSaleReturnAMOUNT,
                                    cast(sum(case when formId='KLM_SKJXSUMBILL' then fAmount else 0 end ) as decimal(18,2)) as FJXAMOUNT,
                                    cast(sum(case when formId='AR_receivable' then fAmount else 0 end ) as decimal(18,2)) as Freceivable,
                                    cast(sum(case when formId='PAEZ_Balance_Aadjustment' then fAmount else 0 end ) as decimal(18,2)) as FBalance
                                    from {7}
                                    group by FCUSTOMERName,FCUSTOMERNUMBER,FCUSTOMERID,FORGID
                                    ) C
                                    ", queryResult.Count == 0 ? 0 : Convert.ToDecimal(queryResult[0]["FREBATAMOUNT"]), queryResult.Count == 0 ? 0 : Convert.ToDecimal(queryResult[0]["FSOAMOUNT"]),
                 queryResult.Count == 0 ? 0 : Convert.ToDecimal(queryResult[0]["FRECEIVEAMOUNT"]), queryResult.Count == 0 ? 0 : Convert.ToDecimal(queryResult[0]["FSALERETURNAMOUNT"]),
                 queryResult.Count == 0 ? 0 : Convert.ToDecimal(queryResult[0]["FJXAMOUNT"]), queryResult.Count == 0 ? 0 : Convert.ToDecimal(queryResult[0]["FALLAMOUNT"]), queryResult.Count == 0 ? 0 : Convert.ToDecimal(queryResult[0]["FBalance"]), tmpTableName, queryResult.Count == 0 ? 0 : Convert.ToDecimal(queryResult[0]["FREFUNDBILL"]));
                    DynamicObjectCollection finalResult = DBUtils.ExecuteDynamicObject(this.Context, strSql_2);
                    if (finalResult.Count <= 0)
                    {
                        string strSql_5 = string.Format(@"/*dialect*/UPDATE K_CS_FINALSTATEMENT set FRECEIVEAMOUNT = 0,FJXAMOUNT = 0,FSOAMOUNT = 0,FREBATAMOUNT = 0,FSALERETURNAMOUNT = 0,FALLAMOUNT = 0,FBalance = 0,FCSAMOUNT = 0,FREFUNDBILL = 0 where FID = {0}", id);
                        DBUtils.Execute(this.Context, strSql_5);
                        string strSql_6 = string.Format(@"/*dialect*/drop table {0}", tmpTableName);
                        DBUtils.Execute(this.Context, strSql_6);
                        continue;
                    }
                    string strSql_3 = string.Format(@"/*dialect*/UPDATE K_CS_FINALSTATEMENT set FRECEIVEAMOUNT = {0},FJXAMOUNT = {1},FSOAMOUNT = {2},FREBATAMOUNT = {3},FSALERETURNAMOUNT = {4},FALLAMOUNT = {5},FBalance = {6},FCSAMOUNT = {7},FREFUNDBILL ={9}  where FID = {8}",
                        finalResult[0]["FReceiveAMOUNT"], finalResult[0]["FJXAMOUNT"], finalResult[0]["FSOAMOUNT"], finalResult[0]["FRebatAMOUNT"], finalResult[0]["FSaleReturnAMOUNT"], finalResult[0]["Freceivable"], finalResult[0]["FBalance"],
                        Convert.ToDecimal(finalResult[0]["FReceiveAMOUNT"]) + Convert.ToDecimal(finalResult[0]["FJXAMOUNT"]) - Convert.ToDecimal(finalResult[0]["FSOAMOUNT"]) + Convert.ToDecimal(finalResult[0]["FRebatAMOUNT"]) + Convert.ToDecimal(finalResult[0]["FSaleReturnAMOUNT"]) - Convert.ToDecimal(finalResult[0]["Freceivable"]) + Convert.ToDecimal(finalResult[0]["FBalance"]) - Convert.ToDecimal(finalResult[0]["FREFUNDBILL"]), id, finalResult[0]["FREFUNDBILL"]);
                    DBUtils.Execute(this.Context, strSql_3);
                    string strSql_4 = string.Format(@"/*dialect*/drop table {0}", tmpTableName);
                    DBUtils.Execute(this.Context, strSql_4);
                }
            }
        }
    }
}
