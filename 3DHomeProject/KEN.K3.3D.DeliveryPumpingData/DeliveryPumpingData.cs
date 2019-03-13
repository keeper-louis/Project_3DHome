using Kingdee.BOS.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kingdee.BOS;
using Kingdee.BOS.Core;
using Kingdee.BOS.Orm.DataEntity;
using System.ComponentModel;
using Kingdee.BOS.App.Data;

namespace KEN.K3._3D.DeliveryPumpingData
{
    [Description("销售出库接口抽数，校验数据")]
    public class DeliveryPumpingData : IScheduleService
    {

        public void Run(Context ctx, Schedule schedule)
        {
            PumpingData(ctx);
            checkData(ctx);

        }

        private void PumpingData(Context ctx)
        {
            //清空视图物理表
            string strSql = string.Format(@"/*dialect*/drop table Deliveryview");
            DBUtils.Execute(ctx, strSql);

            //从视图插入数据到视图物理表
            strSql = string.Format(@"/*dialect*/select*,CONVERT(varchar(10),scantime, 120) fdate into  Deliveryview from [192.168.1.77].[DB].dbo.Deliveryview v ");
            DBUtils.Execute(ctx, strSql);

            //删除问题数据
            strSql = string.Format(@"/*dialect*/delete Deliveryview where linenumber='加急，单独入库' 
or linenumber like '190%' or linenumber like '201%' 
or linenumber='c' ");
            DBUtils.Execute(ctx, strSql);
            strSql = string.Format(@"/*dialect*/delete Deliveryview where Salenumber = '8851168' and Linenumber = '1' and Amount<> 1");
            DBUtils.Execute(ctx, strSql);

            //匹配不存在于接口数据表的视图物理表的数据 插入到 接口数据表中
            strSql = string.Format(@"/*dialect*/insert into detable select salenumber,linenumber,amount,'0' status,
scantime,CONVERT(varchar(10),scantime, 120) fdate ,getdate() Fsubdate
from Deliveryview v where not exists(select * from detable pr where pr.salenumber=v.Salenumber and pr.linenumber=v.Linenumber and v.scantime=pr.scantime)");
            DBUtils.Execute(ctx, strSql);


            //标识过期数据 status=4
            strSql = @"/*dialect*/update detable set status = 4 where Scantime< '2018-12-1' and status=0 ";
            DBUtils.Execute(ctx, strSql);


            //把接口数据表中status为0的数据（未插入到待录入表的数据状态） sum数量 插入到 待录入表临时表Prtabletemp
            strSql = string.Format(@"/*dialect*/insert into detabletemp  select  '',Salenumber,Linenumber, sum(Amount) amount,'0' status,scantime,'' FsubDate,0,0,'','',''
 from(select Salenumber, Linenumber, CONVERT(varchar(10), pr.scantime, 120) scantime, amount
 from detable pr where pr.status = 0  ) a  group by
 Salenumber, Linenumber, scantime");
            DBUtils.Execute(ctx, strSql);
            //查询detablein中与detabletemp中s/l/t/scantime相同 并且detablein中status=2或3 ，Ferrorstatus<>2 插入到Prtabletemp中
            strSql = string.Format(@"/*dialect*/insert into detabletemp  select pr.* from detablein pr,detabletemp prt where pr.salenumber=prt.salenumber
and pr.linenumber=prt.linenumber and pr.fdate=prt.fdate
and pr.status in (2,3) and pr.Ferrorstatus<>2");
            DBUtils.Execute(ctx, strSql);
            //删除detablein中与detabletemp中id相同的数据
            strSql = string.Format(@"/*dialect*/delete detablein  from Prtabletemp prt where detablein.id=prt.id");
            DBUtils.Execute(ctx, strSql);
            //删除Deliverytablee中与detabletemp中id相同的数据
            strSql = string.Format(@"/*dialect*/delete Deliverytable  from Prtabletemp prt where Deliverytable.FBILLNO=prt.id");
            DBUtils.Execute(ctx, strSql);
            //prtabletemp表中sum(amount) 插入到 prtablein
            strSql = string.Format(@"/*dialect*/insert into detablein  select  Salenumber,Linenumber,sum(Amount) amount,'0' status,fdate,'' FsubDate,0,0,'','',''
 from detabletemp group by Salenumber,Linenumber,fdate");
            //清空detabletemp表
            strSql = string.Format(@"/*dialect*/truncate table detabletemp ");

            //把接口数据表中status全置为1 （已插入到待录入表的数据状态）
            strSql = string.Format(@"/*dialect*/update detable set status = 1 where status=0");
            DBUtils.Execute(ctx, strSql);


        }
        private void checkData(Context ctx)
        {
            //把已存在于错误信息表中s/l/t相同的数据直接写入到 错误信息表中 
            string strSql = string.Format(@"/*dialect*/INSERT INTO Deliverytable select pr.id FBILLNO,'A' FDOCUMENTSTATUS, pr.salenumber SALENUMBER, pr.linenumber LINENUMBER, pr.id PRTABLEINID,
    prt.Reason, pr.fdate FDATE, getdate() FSUBDATE from detablein pr, Deliverytable prt
    where pr.salenumber = prt.Salenumber and pr.linenumber = prt.Linenumber and pr.status = 0 ");
            DBUtils.Execute(ctx, strSql);


            //把已存在于错误信息表中s/l/t相同的数据status标识为2
            strSql = string.Format(@"/*dialect*/update detablein set status=2 from Deliverytable prt 
where detablein.salenumber=prt.Salenumber and detablein.linenumber=prt.Linenumber and detablein.status=0");
            DBUtils.Execute(ctx, strSql);

            //查询无上游单据数据 写入错误信息表
            strSql = string.Format(@"/*dialect*/INSERT INTO Processtable select id FBILLNO,'A' FDOCUMENTSTATUS, salenumber SALENUMBER,linenumber LINENUMBER,technicscode TECHNICSCODE,id PRTABLEINID,
'无上游单据' REASON,state STATE,fdate FDATE,getdate() FSUBDATE from  Prtablein 
 where not EXISTS (select  tso.FSALEORDERNUMBER,tso.FSALEORDERENTRYSEQ
 from T_SFC_OPERPLANNING tso , T_SFC_OPERPLANNINGDETAIL tsod ,T_SFC_OPERPLANNINGDETAIL_B tsodb
 where tsodb.FDETAILID=tsod.FDETAILID and tso.fid=tsodb.FENTRYID 
and Linenumber=tso.FSALEORDERENTRYSEQ and Salenumber=tso.FSALEORDERNUMBER and technicscode=tsod.FOperNumber ) and State<>3 and status=0
");
            DBUtils.Execute(ctx, strSql);
            //把无上游单据status标识为2
            strSql = string.Format(@"/*dialect*/update Prtablein set status=2
 where not EXISTS (select  tso.FSALEORDERNUMBER,tso.FSALEORDERENTRYSEQ
 from T_SFC_OPERPLANNING tso , T_SFC_OPERPLANNINGDETAIL tsod ,T_SFC_OPERPLANNINGDETAIL_B tsodb
 where tsodb.FDETAILID=tsod.FDETAILID and tso.fid=tsodb.FENTRYID 
and Linenumber=tso.FSALEORDERENTRYSEQ and Salenumber=tso.FSALEORDERNUMBER and technicscode=tsod.FOperNumber ) and State<>3 and status=0");
            DBUtils.Execute(ctx, strSql);
            //查询汇报数量大于工序计划单中可汇报数量的数据  写入错误信息表
            strSql = string.Format(@"/*dialect*/INSERT INTO Processtable select id FBILLNO,'A' FDOCUMENTSTATUS, Prtablein.salenumber SALENUMBER,Prtablein.linenumber LINENUMBER,Prtablein.technicscode TECHNICSCODE,id PRTABLEINID,
'大于可汇报数量' REASON,state STATE,fdate FDATE,getdate() FSUBDATE  from Prtablein ,
 (select a.Salenumber,a.Linenumber,a.Technicscode 
 from (select Salenumber,Linenumber,Technicscode,sum(Amount) amount from Prtablein where status<>2 and status<>4 group by  Salenumber,Linenumber,Technicscode) a,
  (select  tso.FSALEORDERNUMBER salenumber,tso.FSALEORDERENTRYSEQ linenumber,tsod.FOPERNUMBER technicscode,(FOPERQTY-FREPORTBASEQTY) amount
 from T_SFC_OPERPLANNING tso, T_SFC_OPERPLANNINGDETAIL tsod ,T_SFC_OPERPLANNINGDETAIL_B tsodb
 where tsodb.FDETAILID=tsod.FDETAILID
 and tso.fid=tsodb.FENTRYID )b where a.Salenumber=b.salenumber and a.Linenumber=b.linenumber and a.Technicscode=b.technicscode and a.amount>b.amount) c
 where Prtablein.Salenumber=c.Salenumber and Prtablein.Linenumber=c.Linenumber and Prtablein.Technicscode=c.Technicscode and Prtablein.status=0");
            DBUtils.Execute(ctx, strSql);
            //把汇报数量大于工序计划单中可汇报数量的数据status标识为2
            strSql = string.Format(@"/*dialect*/ update Prtablein set status=2 from
 (select a.Salenumber,a.Linenumber,a.Technicscode 
 from (select Salenumber,Linenumber,Technicscode,sum(Amount) amount from Prtablein where status<>2 and status<>4  group by  Salenumber,Linenumber,Technicscode) a,
  (select  tso.FSALEORDERNUMBER salenumber,tso.FSALEORDERENTRYSEQ linenumber,tsod.FOPERNUMBER technicscode,(FOPERQTY-FREPORTBASEQTY) amount
 from T_SFC_OPERPLANNING tso, T_SFC_OPERPLANNINGDETAIL tsod ,T_SFC_OPERPLANNINGDETAIL_B tsodb
 where tsodb.FDETAILID=tsod.FDETAILID
 and tso.fid=tsodb.FENTRYID )b where a.Salenumber=b.salenumber and a.Linenumber=b.linenumber and a.Technicscode=b.technicscode and a.amount>b.amount) c
 where Prtablein.Salenumber=c.Salenumber and Prtablein.Linenumber=c.Linenumber and Prtablein.Technicscode=c.Technicscode and Prtablein.status=0");
            DBUtils.Execute(ctx, strSql);
        }
    }
}
