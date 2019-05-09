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
            strSql = string.Format(@"/*dialect*/select*,CONVERT(varchar(10),scantime, 120) fdate into  Deliveryview from [192.168.1.77].[DB].dbo.Deliveryview v where scantime>='2019-5-1'");
            DBUtils.Execute(ctx, strSql);

            //删除问题数据
            strSql = string.Format(@"/*dialect*/delete Deliveryview where linenumber in (select Linenumber from Deliveryview de where PATINDEX('%[^0-9]%', de.Linenumber)<>0) ");
            DBUtils.Execute(ctx, strSql);



            //删除重复扫码数据
            strSql = string.Format(@"/*dialect*/delete Deliveryview from
  (select alt.Salenumber,alt.Linenumber,alt.Amount,min(alt.scantime) scantime,T_BD_MATERIAL_L.FNAME
  from T_SAL_ORDERENTRY  tsoe , T_SAL_ORDER tso,t_BD_Material t_BD_Material,T_BD_MATERIAL_L T_BD_MATERIAL_L,Deliveryview alt
   where tsoe.fid=tso.fid and t_BD_Material.FMASTERID=T_BD_MATERIAL_L.FMATERIALID
  and t_BD_Material.FMATERIALID=tsoe.FMATERIALID and (t_BD_Material.fnumber  like '10.401%' or t_BD_Material.fnumber  like '10.402%' 
or t_BD_Material.fnumber  like '10.403%' or t_BD_Material.fnumber  like '10.404%'  
or t_BD_Material.fnumber  like '10.405%'
or t_BD_Material.fnumber  like '10.406%' or t_BD_Material.fnumber  like '10.407%' 
or t_BD_Material.fnumber  like '10.408%' or t_BD_Material.fnumber  like '10.409%'  
or t_BD_Material.fnumber  like '10.410%' or t_BD_Material.fnumber  like '10.411%'
or t_BD_Material.fnumber  like '01.07.0142%' or t_BD_Material.fnumber  like '05.902.2027.225.215%' 
or t_BD_Material.fnumber  like '05.902.2027.435.215%' or t_BD_Material.fnumber  like '05.902.2027.827.225%'
or t_BD_Material.fnumber  like '05.902.2027.827.435%' or t_BD_Material.fnumber  like '06.03.0003%' 
or t_BD_Material.fnumber  like '06.03.0006%' or t_BD_Material.fnumber  like '06.99.0026%'  
or t_BD_Material.fnumber  like '06.99.0028%' or t_BD_Material.fnumber  like '06.99.0029%'
or t_BD_Material.fnumber  like '06.99.0045%') and tso.FBILLNO=alt.Salenumber and  tsoe.FSEQ=alt.Linenumber 
 group by  alt.Salenumber,alt.Linenumber,alt.Amount,T_BD_MATERIAL_L.FNAME) a 
 where Deliveryview.Salenumber=a.Salenumber and Deliveryview.Linenumber=a.Linenumber
 and Deliveryview.scantime<>a.scantime ");
            DBUtils.Execute(ctx, strSql);

            //匹配不存在于接口数据表的视图物理表的数据 插入到 接口数据表中
            strSql = string.Format(@"/*dialect*/insert into detable select salenumber,linenumber,amount,'0' status,
scantime,CONVERT(varchar(10),scantime, 120) fdate ,getdate() Fsubdate
from Deliveryview v where not exists(select * from detable pr where pr.salenumber=v.Salenumber and pr.linenumber=v.Linenumber and v.scantime=pr.scantime)");
            DBUtils.Execute(ctx, strSql);

            //把detable中status为0的数据（未插入到待录入表的数据状态） sum数量 插入到 待录入表临时表Prtabletemp
            strSql = string.Format(@"/*dialect*/insert into detabletemp  select  '',Salenumber,Linenumber, sum(Amount) amount,'0' status,scantime,'' FsubDate,0,0,'','',''
 from(select Salenumber, Linenumber, CONVERT(varchar(10), pr.scantime, 120) scantime, amount
 from detable pr where pr.status = 0  ) a  group by
 Salenumber, Linenumber, scantime");
            DBUtils.Execute(ctx, strSql);
            //查询detablein中与detabletemp中s/l/t/scantime相同 并且detablein中status=2或3 ，Ferrorstatus<>2(前台未生成单据) 插入到detabletemp中
            strSql = string.Format(@"/*dialect*/insert into detabletemp  select pr.* from detablein pr,detabletemp prt where pr.salenumber=prt.salenumber
and pr.linenumber=prt.linenumber and pr.fdate=prt.fdate
and pr.status in (2,3) and pr.Ferrorstatus<>2");
            DBUtils.Execute(ctx, strSql);
            //删除detablein中与detabletemp中id相同的数据
            strSql = string.Format(@"/*dialect*/delete detablein  from detabletemp prt where detablein.id=prt.id");
            DBUtils.Execute(ctx, strSql);
            //删除Deliverytablee中与detabletemp中id相同的数据
            strSql = string.Format(@"/*dialect*/delete Deliverytable  from detabletemp prt where Deliverytable.FBILLNO=prt.id");
            DBUtils.Execute(ctx, strSql);
            //prtabletemp表中sum(amount) 插入到 prtablein
            strSql = string.Format(@"/*dialect*/insert into detablein  select  Salenumber,Linenumber,sum(Amount) amount,'0' status,fdate,'' FsubDate,0,0,'','',''
 from detabletemp group by Salenumber,Linenumber,fdate");
            DBUtils.Execute(ctx, strSql);
            //清空detabletemp表
            strSql = string.Format(@"/*dialect*/truncate table detabletemp ");
            DBUtils.Execute(ctx, strSql);
            //把接口数据表中status全置为1 （已插入到待录入表的数据状态）
            strSql = string.Format(@"/*dialect*/update detable set status = 1 where status=0");
            DBUtils.Execute(ctx, strSql);


        }
        private void checkData(Context ctx)
        {
            //删除amount=0的数据
            string strSql = string.Format(@"/*dialect*/delete detablein where amount=0 ");
            DBUtils.Execute(ctx, strSql);



            //查询无上游单据数据 写入错误信息表
            strSql = string.Format(@"/*dialect*/INSERT INTO Deliverytable select id FBILLNO,'A' FDOCUMENTSTATUS, salenumber SALENUMBER,linenumber LINENUMBER,id PRTABLEINID,
'无对应销售订单' REASON,fdate FDATE,getdate() FSUBDATE,'' from detablein de 
left join (select tso.fid fid,tsoe.FENTRYID FDETAILID,tso.FBILLNO,tsoe.FSEQ ,tso.fdocumentstatus
from T_SAL_ORDER tso,T_SAL_ORDERENTRY tsoe where tso.fid=tsoe.FID) a
on de.Salenumber=a.FBILLNO and de.Linenumber=a.FSEQ
where de.status=0 and (a.fid is null or a.FDETAILID is null or a.FDOCUMENTSTATUS<>'C')
");
            DBUtils.Execute(ctx, strSql);
            //把无上游单据status标识为2
            strSql = string.Format(@"/*dialect*/update detablein set status=2,ferrormsg='无对应销售订单' from 
(select de.id from detablein de 
left join (select tso.fid fid,tsoe.FENTRYID FDETAILID,tso.FBILLNO,tsoe.FSEQ ,tso.fdocumentstatus
from T_SAL_ORDER tso,T_SAL_ORDERENTRY tsoe where tso.fid=tsoe.FID) a
on de.Salenumber=a.FBILLNO and de.Linenumber=a.FSEQ
where de.status=0 and (a.fid is null or a.FDETAILID is null or a.FDOCUMENTSTATUS<>'C' ) ) b where  detablein.id=b.id");
            DBUtils.Execute(ctx, strSql);

            //查询物料启用BOM管理，但是销售订单中未选中BOM版本
            strSql = string.Format(@"/*dialect*/ INSERT INTO Deliverytable select id FBILLNO,'A' FDOCUMENTSTATUS, salenumber SALENUMBER,linenumber LINENUMBER,id PRTABLEINID,
'物料启用BOM管理，但是销售订单中未选中BOM版本' REASON,fdate FDATE,getdate() FSUBDATE,'' from detablein de 
left join (select tso.fid fid,tsoe.FENTRYID FDETAILID,tso.FBILLNO,tsoe.FSEQ ,tsoe.FBOMID,tbm.FISENABLE
from T_SAL_ORDER tso,T_SAL_ORDERENTRY tsoe, t_BD_MaterialInvPty tbm
 where tso.fid=tsoe.FID and tsoe.FMATERIALID=tbm.FMATERIALID and tbm.FINVPTYID='10003') a
on de.Salenumber=a.FBILLNO and de.Linenumber=a.FSEQ
where a.FISENABLE=1 and a.FBOMID=0 and de.status=0
");
            DBUtils.Execute(ctx, strSql);
            //把查询物料启用BOM管理，但是销售订单中未选中BOM版本的单据status标识为2
            strSql = string.Format(@"/*dialect*/update detablein set status=2,ferrormsg='物料启用BOM管理，但是销售订单中未选中BOM版本' from 
(select de.id from detablein de 
left join (select tso.fid fid,tsoe.FENTRYID FDETAILID,tso.FBILLNO,tsoe.FSEQ ,tsoe.FBOMID,tbm.FISENABLE
from T_SAL_ORDER tso,T_SAL_ORDERENTRY tsoe, t_BD_MaterialInvPty tbm
 where tso.fid=tsoe.FID and tsoe.FMATERIALID=tbm.FMATERIALID and tbm.FINVPTYID='10003') a
on de.Salenumber=a.FBILLNO and de.Linenumber=a.FSEQ
where a.FISENABLE=1 and a.FBOMID=0 and de.status=0 ) b where  detablein.id=b.id");
            DBUtils.Execute(ctx, strSql);

            //查询物料未维护生产车间
            strSql = string.Format(@"/*dialect*/ INSERT INTO Deliverytable select id FBILLNO,'A' FDOCUMENTSTATUS, salenumber SALENUMBER,linenumber LINENUMBER,id PRTABLEINID,
'物料未维护生产车间' REASON,fdate FDATE,getdate() FSUBDATE,'' from detablein de 
left join (select tso.fid fid,tsoe.FENTRYID FDETAILID,tso.FBILLNO,tsoe.FSEQ ,tbm.FWorkShopId
from T_SAL_ORDER tso,T_SAL_ORDERENTRY tsoe, t_BD_MaterialProduce tbm,t_BD_MaterialBase tbmb
 where tso.fid=tsoe.FID and tsoe.FMATERIALID=tbm.FMATERIALID and tbm.FMATERIALID=tbmb.FMATERIALID and FERPCLSID <> '1') a
on de.Salenumber=a.FBILLNO and de.Linenumber=a.FSEQ
where a.FWORKSHOPID=0  and de.status=0 
");
            DBUtils.Execute(ctx, strSql);
            //把查询物料未维护生产车间的单据status标识为2
            strSql = string.Format(@"/*dialect*/update detablein set status=2,ferrormsg='物料未维护生产车间' from 
(select de.id from detablein de 
left join (select tso.fid fid,tsoe.FENTRYID FDETAILID,tso.FBILLNO,tsoe.FSEQ ,tbm.FWorkShopId
from T_SAL_ORDER tso,T_SAL_ORDERENTRY tsoe, t_BD_MaterialProduce tbm,t_BD_MaterialBase tbmb
 where tso.fid=tsoe.FID and tsoe.FMATERIALID=tbm.FMATERIALID and tbm.FMATERIALID=tbmb.FMATERIALID and FERPCLSID <> '1') a
on de.Salenumber=a.FBILLNO and de.Linenumber=a.FSEQ
where a.FWORKSHOPID=0  and de.status=0  ) b where  detablein.id=b.id");
            DBUtils.Execute(ctx, strSql);


            //查询出库数量大于销售订单中可出库数量的数据  写入错误信息表
            strSql = string.Format(@"/*dialect*/insert into Deliverytable
select id FBILLNO,'A' FDOCUMENTSTATUS, detablein.salenumber SALENUMBER,detablein.linenumber LINENUMBER,id PRTABLEINID,
'大于可出库数量' REASON,fdate FDATE,getdate() FSUBDATE,'' from detablein ,
 (select a.Salenumber,a.Linenumber
 from (select Salenumber,Linenumber,sum(Amount) amount from detablein where status=0 group by  Salenumber,Linenumber) a,
  (select  tso.fbillno salenumber,tsoe.fseq linenumber,FRemainOutQty amount
 from T_SAL_ORDER tso,T_SAL_ORDERENTRY tsoe,T_SAL_ORDERENTRY_R tsop 
where tso.fid=tsoe.fid and tsoe.FENTRYID=tsop.FENTRYID  )b where a.Salenumber=b.salenumber and a.Linenumber=b.linenumber and a.amount>b.amount) c
 where detablein.Salenumber=c.Salenumber and detablein.Linenumber=c.Linenumber  and detablein.status=0 ");
            DBUtils.Execute(ctx, strSql);
            //把出库数量大于销售订单中可出库数量的数据 status标识为2
            strSql = string.Format(@"/*dialect*/ update detablein set status=2 ,ferrormsg='大于可出库数量' from
 (select a.Salenumber,a.Linenumber
 from (select Salenumber,Linenumber,sum(Amount) amount from detablein where status=0 group by  Salenumber,Linenumber) a,
  (select  tso.fbillno salenumber,tsoe.fseq linenumber,FRemainOutQty amount
 from T_SAL_ORDER tso,T_SAL_ORDERENTRY tsoe,T_SAL_ORDERENTRY_R tsop 
where tso.fid=tsoe.fid and tsoe.FENTRYID=tsop.FENTRYID  )b where a.Salenumber=b.salenumber and a.Linenumber=b.linenumber and a.amount>b.amount) c
 where detablein.Salenumber=c.Salenumber and detablein.Linenumber=c.Linenumber  and detablein.status=0 ");
            DBUtils.Execute(ctx, strSql);

            //把剩余status=0的数据置为3 预检完成
            strSql = string.Format(@"/*dialect*/update detablein set status = 3 where detablein.status = 0 ");
            DBUtils.Execute(ctx, strSql);
        }
    }
}
