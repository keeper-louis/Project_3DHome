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

namespace KEN.K3._3D.ReportPumpingData
{
    [Description("工序汇报接口抽数，校验数据")]
    class ReportPumpingData : IScheduleService
    {
        public void Run(Context ctx, Schedule schedule)
        {
            PumpingData(ctx);
            checkData(ctx);

        }

        private void PumpingData(Context ctx)
        {
            //清空视图物理表
            string strSql = string.Format(@"/*dialect*/drop table processview");
            DBUtils.Execute(ctx, strSql);

            //从视图插入数据到视图物理表
            strSql = string.Format(@"/*dialect*/select*,scantime fdate into  processview from [192.168.1.77].[DB].dbo.processview v ");
            DBUtils.Execute(ctx, strSql);

            //删除问题数据
            strSql = string.Format(@"/*dialect*/delete processview where linenumber='加急，单独入库' 
or linenumber like '190%' or linenumber like '201%' 
or linenumber='c' ");
            DBUtils.Execute(ctx, strSql);
            strSql = string.Format(@"/*dialect*/delete processview where Salenumber = '8851168' and Linenumber = '1' and Amount<> 1 ");
            DBUtils.Execute(ctx, strSql);

            //匹配不存在于接口数据表的视图物理表的数据 插入到 接口数据表中
            strSql = string.Format(@"/*dialect*/insert into prtable select salenumber,linenumber,Technicscode,amount,state,'0' status,scantime,scantime fdate ,getdate() Fsubdate
from processview v where not exists(select * from prtable pr where pr.salenumber=v.Salenumber and pr.linenumber=v.Linenumber and 
v.Technicscode=pr.Technicscode and v.scantime=pr.scantime) ");
            DBUtils.Execute(ctx, strSql);

            //标识重复扫描数据 status=2
            strSql = @"/*dialect*/update Prtable set status=2 from
(select pr.Salenumber,pr.Linenumber,pr.Technicscode,pr.Amount,min(pr.Scantime) Scantime
 from T_SAL_ORDER tso,T_SAL_ORDERENTRY tsoe,Prtable pr ,t_BD_Material t_BD_Material
where tso.fid=tsoe.FID and pr.Linenumber=tsoe.FSEQ and pr.Salenumber=tso.FBILLNO 
 and t_BD_Material.FMATERIALID=tsoe.FMATERIALID 
and (t_BD_Material.fnumber like '10.401%'  or t_BD_Material.fnumber like '10.402%'
or t_BD_Material.fnumber like '10.403%' or t_BD_Material.fnumber like '10.404%'  or t_BD_Material.fnumber like '10.405%'  ) 
group by Salenumber,Linenumber,Technicscode,Amount) a where Prtable.Salenumber=a.Salenumber and Prtable.Linenumber=a.Linenumber
and Prtable.Amount=a.Amount and Prtable.Technicscode=a.Technicscode and Prtable.Scantime<>a.Scantime and Prtable.status=0 ";
            DBUtils.Execute(ctx, strSql);
            strSql = @"/*dialect*/update Prtable set status=2 from
(select pr.Salenumber,pr.Linenumber,pr.Technicscode,pr.Amount,min(pr.Scantime) Scantime
 from T_SAL_ORDER tso,T_SAL_ORDERENTRY tsoe,Prtable pr ,t_BD_Material t_BD_Material
where tso.fid=tsoe.FID and pr.Linenumber=tsoe.FSEQ and pr.Salenumber=tso.FBILLNO 
 and t_BD_Material.FMATERIALID=tsoe.FMATERIALID 
and (t_BD_Material.fnumber like '10.406%' or t_BD_Material.fnumber like '10.407%' 
or t_BD_Material.fnumber like '10.408%' or t_BD_Material.fnumber like '10.409%'  or t_BD_Material.fnumber like '10.410%'
or t_BD_Material.fnumber like '10.411%' 
or t_BD_Material.fnumber  like '01.07.0142%' or t_BD_Material.fnumber  like '05.902.2027.225.215%'   ) 
group by Salenumber,Linenumber,Technicscode,Amount) a where Prtable.Salenumber=a.Salenumber and Prtable.Linenumber=a.Linenumber
and Prtable.Amount=a.Amount and Prtable.Technicscode=a.Technicscode and Prtable.Scantime<>a.Scantime and Prtable.status=0 ";
            DBUtils.Execute(ctx, strSql);
            strSql = @"/*dialect*/update Prtable set status=2 from
(select pr.Salenumber,pr.Linenumber,pr.Technicscode,pr.Amount,min(pr.Scantime) Scantime
 from T_SAL_ORDER tso,T_SAL_ORDERENTRY tsoe,Prtable pr ,t_BD_Material t_BD_Material
where tso.fid=tsoe.FID and pr.Linenumber=tsoe.FSEQ and pr.Salenumber=tso.FBILLNO 
 and t_BD_Material.FMATERIALID=tsoe.FMATERIALID 
and ( t_BD_Material.fnumber  like '05.902.2027.435.215%' or t_BD_Material.fnumber  like '05.902.2027.827.225%'
or t_BD_Material.fnumber  like '05.902.2027.827.435%' or t_BD_Material.fnumber  like '06.03.0003%' 
or t_BD_Material.fnumber  like '06.03.0006%' or t_BD_Material.fnumber  like '06.99.0026%'  
or t_BD_Material.fnumber  like '06.99.0028%' or t_BD_Material.fnumber  like '06.99.0029%'
or t_BD_Material.fnumber  like '06.99.0045%'   ) 
group by Salenumber,Linenumber,Technicscode,Amount) a where Prtable.Salenumber=a.Salenumber and Prtable.Linenumber=a.Linenumber
and Prtable.Amount=a.Amount and Prtable.Technicscode=a.Technicscode and Prtable.Scantime<>a.Scantime and Prtable.status=0 ";
            DBUtils.Execute(ctx, strSql);
            strSql = @"/*dialect*/update Prtable set status=2 from
(select pr.Salenumber,pr.Linenumber,pr.Technicscode,pr.Amount,min(pr.Scantime) Scantime
 from T_SAL_ORDER tso,T_SAL_ORDERENTRY tsoe,Prtable pr ,t_BD_Material t_BD_Material
where tso.fid=tsoe.FID and pr.Linenumber=tsoe.FSEQ and pr.Salenumber=tso.FBILLNO 
 and t_BD_Material.FMATERIALID=tsoe.FMATERIALID 
and (  t_BD_Material.fnumber  in ( select    materialnumber from v_3d_saleorderview 
where   materialname like '%套板%'  and cast(rtrim(ltrim(c)) as int)>=500 
and salenumber<>'8845104' and salelinenumber<>1 ) )
group by Salenumber,Linenumber,Technicscode,Amount) a where Prtable.Salenumber=a.Salenumber and Prtable.Linenumber=a.Linenumber
and Prtable.Amount=a.Amount and Prtable.Technicscode=a.Technicscode and Prtable.Scantime<>a.Scantime and Prtable.status=0 ";
            DBUtils.Execute(ctx, strSql);


            //标识采购件 state=3
            strSql = @"/*dialect*/update prtable set State='3'
 from T_SAL_ORDER tso,T_SAL_ORDERENTRY tsoe, t_BD_Material t_BD_Material
 where tso.fid = tsoe.FID and prtable.Linenumber = tsoe.FSEQ and prtable.Salenumber = tso.FBILLNO
 and t_BD_Material.FMATERIALID = tsoe.FMATERIALID
and tsoe.FMATERIALID in (select tbm.FMATERIALID
from t_BD_MaterialBase tbmb, t_BD_Material tbm
 where FCATEGORYID = '237' and tbm.FMATERIALID = tbmb.FMATERIALID) ";
            DBUtils.Execute(ctx, strSql);

            //采购件只保留10工序 status=3
            strSql = string.Format(@"/*dialect*/update prtable set status=3 where State=3 and Technicscode<>10 and status=0");
            DBUtils.Execute(ctx, strSql);

            //标识过期数据 status=4
            strSql = @"/*dialect*/   update prtable set status = 4 where Scantime< '2018-11-1' and status=0 ";
            DBUtils.Execute(ctx, strSql);

            //根据业务改变fdate的时间
            strSql = @"/*dialect*/   ";
            DBUtils.Execute(ctx, strSql);

            //把接口数据表中status为0的数据（未插入到待录入表的数据状态） sum数量 插入到 待录入表
            strSql = string.Format(@"/*dialect*/insert into Prtablein  select  Salenumber,Linenumber,Technicscode, sum(Amount) amount,State,'0' status,scantime,'' FsubDate
 from(select Salenumber, Linenumber, Technicscode, CONVERT(varchar(10), pr.scantime, 120) scantime, amount, State
 from prtable pr where pr.status = 0  ) a  group by
 Salenumber, Linenumber, Technicscode, scantime, State ");
            DBUtils.Execute(ctx, strSql);
            //把接口数据表中status全置为1 （已插入到待录入表的数据状态）
            strSql = string.Format(@"/*dialect*/update prtable set status = 1 where status=0");
            DBUtils.Execute(ctx, strSql);


        }

        private void checkData(Context ctx)
        {

            //把已存在于错误信息表中s/l/t相同的数据直接写入到 错误信息表中 
            string strSql = string.Format(@"/*dialect*/ INSERT INTO Processtable select pr.id FBILLNO,'A' FDOCUMENTSTATUS, pr.salenumber SALENUMBER, pr.linenumber LINENUMBER, pr.technicscode TECHNICSCODE, pr.id PRTABLEINID,
    prt.Reason,state STATE, pr.fdate FDATE, getdate() FSUBDATE from Prtablein pr, Processtable prt
    where pr.salenumber = prt.Salenumber and pr.linenumber = prt.Linenumber and pr.Technicscode = prt.Technicscode and pr.status = 0");
            DBUtils.Execute(ctx, strSql);


            //把已存在于错误信息表中s/l/t相同的数据status标识为2
            strSql = string.Format(@"/*dialect*/update Prtablein set status=2 from Processtable prt 
where Prtablein.salenumber=Prtablein.Salenumber and Prtablein.linenumber=prt.Linenumber and Prtablein.Technicscode=prt.Technicscode and Prtablein.status=0");
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
 from (select Salenumber,Linenumber,Technicscode,sum(Amount) amount from Prtablein where status=0 group by  Salenumber,Linenumber,Technicscode) a,
  (select  tso.FSALEORDERNUMBER salenumber,tso.FSALEORDERENTRYSEQ linenumber,tsod.FOPERNUMBER technicscode,(FOPERQTY-FREPORTBASEQTY) amount
 from T_SFC_OPERPLANNING tso, T_SFC_OPERPLANNINGDETAIL tsod ,T_SFC_OPERPLANNINGDETAIL_B tsodb
 where tsodb.FDETAILID=tsod.FDETAILID
 and tso.fid=tsodb.FENTRYID )b where a.Salenumber=b.salenumber and a.Linenumber=b.linenumber and a.Technicscode=b.technicscode and a.amount>b.amount) c
 where Prtablein.Salenumber=c.Salenumber and Prtablein.Linenumber=c.Linenumber and Prtablein.Technicscode=c.Technicscode");
            DBUtils.Execute(ctx, strSql);
            //把汇报数量大于工序计划单中可汇报数量的数据status标识为2
            strSql = string.Format(@"/*dialect*/update Prtablein set status=2 from
 (select a.Salenumber,a.Linenumber,a.Technicscode 
 from (select Salenumber,Linenumber,Technicscode,sum(Amount) amount from Prtablein where status=0 group by  Salenumber,Linenumber,Technicscode) a,
  (select  tso.FSALEORDERNUMBER salenumber,tso.FSALEORDERENTRYSEQ linenumber,tsod.FOPERNUMBER technicscode,(FOPERQTY-FREPORTBASEQTY) amount
 from T_SFC_OPERPLANNING tso, T_SFC_OPERPLANNINGDETAIL tsod ,T_SFC_OPERPLANNINGDETAIL_B tsodb
 where tsodb.FDETAILID=tsod.FDETAILID
 and tso.fid=tsodb.FENTRYID )b where a.Salenumber=b.salenumber and a.Linenumber=b.linenumber and a.Technicscode=b.technicscode and a.amount>b.amount) c
 where Prtablein.Salenumber=c.Salenumber and Prtablein.Linenumber=c.Linenumber and Prtablein.Technicscode=c.Technicscode");
            DBUtils.Execute(ctx, strSql);

            //查询sacntime小于下达日期数据 写入错误信息表
            strSql = string.Format(@"/*dialect*/INSERT INTO Processtable select id FBILLNO,'A' FDOCUMENTSTATUS, pr.salenumber SALENUMBER,pr.linenumber LINENUMBER,pr.technicscode TECHNICSCODE,id PRTABLEINID,
'sacntime小于下达日期' REASON,state STATE,fdate FDATE,getdate() FSUBDATE  
 from Prtablein pr,(select FPlanStartDate ,tso.FSaleOrderNumber,tso.FSaleOrderEntrySeq 
 from T_PRD_MO trm,T_PRD_MOENTRY  trme ,T_SFC_OPERPLANNING tso where trm.fid=trme.fid
 and  trm.FBILLNO=tso.FMONumber and tso.FMOEntrySeq=trme.fseq ) a
 where pr.fdate<a.FPLANSTARTDATE and pr.salenumber=a.FSaleOrderNumber and pr.linenumber=a.FSaleOrderEntrySeq and Prtablein.status=0");
            DBUtils.Execute(ctx, strSql);
            //把sacntime小于下达日期status标识为2
            strSql = string.Format(@"/*dialect*/update Prtablein set status=2 from(select FPlanStartDate ,tso.FSaleOrderNumber,tso.FSaleOrderEntrySeq 
 from T_PRD_MO trm,T_PRD_MOENTRY  trme ,T_SFC_OPERPLANNING tso where trm.fid=trme.fid
 and  trm.FBILLNO=tso.FMONumber and tso.FMOEntrySeq=trme.fseq ) a
 where Prtablein.fdate<a.FPLANSTARTDATE and Prtablein.salenumber=a.FSaleOrderNumber and Prtablein.linenumber=a.FSaleOrderEntrySeq and Prtablein.status=0");
            DBUtils.Execute(ctx, strSql);




        }
    }
}