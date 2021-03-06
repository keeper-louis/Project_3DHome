﻿using Kingdee.BOS.Contracts;
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

namespace KEN.K3._3D.AltablePumpingData
{
    [Description("调拨接口抽数，校验数据")]
    public class AltablePumpingData : IScheduleService
    {
        public void Run(Context ctx, Schedule schedule)
        {
            PumpingData(ctx);
            checkData(ctx);

        }
        private void PumpingData(Context ctx)
        {
            //清空视图物理表
            string strSql = string.Format(@"/*dialect*/drop table Allocationview");
            DBUtils.Execute(ctx, strSql);
             strSql = string.Format(@"/*dialect*/drop table Tatest ");
            DBUtils.Execute(ctx, strSql);
            strSql = string.Format(@"/*dialect*/ select* into Tatest from[192.168.1.77].[DB].dbo.Allocationview where linenumber<>'' or linenumber is not null ");
            DBUtils.Execute(ctx, strSql);


//从视图插入数据到视图物理表
            strSql = string.Format(@"/*dialect*/select*,CONVERT(varchar(10),scantime, 120) fdate into 
                                            Allocationview from Tatest v where scantime>='2019-5-1' and (linenumber<>'' or linenumber is not null )");
            DBUtils.Execute(ctx, strSql);


            strSql = string.Format(@"/*dialect*/ insert into Allocationview select c.*,CONVERT(varchar(10),c.scantime, 120) fdate  from  Tatest c ,
(select * from detablein a where not exists (select * from altablein b where a.Salenumber=b.Salenumber and a.Linenumber=b.Linenumber )) d
where c.salenumber=d.Salenumber and c.linenumber=d.Linenumber and c.scantime<'2019-5-1'");
            DBUtils.Execute(ctx, strSql);

            //删除问题数据
            strSql = string.Format(@"/*dialect*/delete Allocationview where linenumber in (select Linenumber from Allocationview de where PATINDEX('%[^0-9]%', de.Linenumber)<>0) ");
            DBUtils.Execute(ctx, strSql);
            //删除 
            //modify 2020/04/16 物料编码为10.4开头的都是铝材门。铝材门的扫码数据应该以PMS对应的销售订单号、行号、物料编码（10.4开头）、ID号（M开头）作为获取数据的条件并执行去重规则（去重规则不变,重点：只有物料编码为10.4开头的使用此中获取数据规则，因为后期配件录入系统其ID号不为M开头，以免影响此类数据生成）。该调整只影响入库和调拨接口。3月数据不必重新生成。
            strSql = string.Format(@"/*dialect*/  delete from Allocationview where id in ( select    alt.id from Allocationview  alt, T_SAL_ORDERENTRY  tsoe , T_SAL_ORDER tso,t_BD_Material t_BD_Material,T_BD_MATERIAL_L T_BD_MATERIAL_L
   where tsoe.fid=tso.fid and t_BD_Material.FMASTERID=T_BD_MATERIAL_L.FMATERIALID
  and t_BD_Material.FMATERIALID=tsoe.FMATERIALID    and tso.FBILLNO=alt.Salenumber and  tsoe.FSEQ=alt.Linenumber
  and  (
 t_BD_Material.fnumber  like '10.401%' or t_BD_Material.fnumber  like '10.402%' 
or t_BD_Material.fnumber  like '10.403%' or t_BD_Material.fnumber  like '10.404%'  
or t_BD_Material.fnumber  like '10.405%'
or t_BD_Material.fnumber  like '10.406%' 
or t_BD_Material.fnumber  like '10.410%' or t_BD_Material.fnumber  like '10.411%' 
or t_BD_Material.fnumber  like '10.412%' 
or t_BD_Material.fnumber  like '10.413%' 
or t_BD_Material.fnumber  like '10.414%'
or t_BD_Material.fnumber  like '10.415%'
or t_BD_Material.fnumber  like '10.416%'
or t_BD_Material.fnumber  like '10.417%'
  )
  and id like 'T%'
  ) and id like 'T%' ");
            DBUtils.Execute(ctx, strSql);
            //删除问题数据
            strSql = string.Format(@"/*dialect*/delete Allocationview where linenumber ='' or linenumber is null or amount is null or amount ='' or amount=0 ");
            DBUtils.Execute(ctx, strSql);

            //删除重复扫码数据
            strSql = string.Format(@"/*dialect*/delete Allocationview from
  (select alt.Salenumber,alt.Linenumber,alt.Amount,min(alt.Packcode) packcode,T_BD_MATERIAL_L.FNAME
  from T_SAL_ORDERENTRY  tsoe , T_SAL_ORDER tso,t_BD_Material t_BD_Material,T_BD_MATERIAL_L T_BD_MATERIAL_L,Allocationview alt
   where tsoe.fid=tso.fid and t_BD_Material.FMASTERID=T_BD_MATERIAL_L.FMATERIALID
  and t_BD_Material.FMATERIALID=tsoe.FMATERIALID and (t_BD_Material.fnumber  like '10.401%' or t_BD_Material.fnumber  like '10.402%' 
or t_BD_Material.fnumber  like '10.403%' or t_BD_Material.fnumber  like '10.404%'  
or t_BD_Material.fnumber  like '10.405%'
or t_BD_Material.fnumber  like '10.406%' or t_BD_Material.fnumber  like '10.407%' 
or t_BD_Material.fnumber  like '10.408%' or t_BD_Material.fnumber  like '10.409%'  
or t_BD_Material.fnumber  like '10.410%' or t_BD_Material.fnumber  like '10.411%' 
or t_BD_Material.fnumber  like '10.412%' 
or t_BD_Material.fnumber  like '10.413%' 
or t_BD_Material.fnumber  like '10.414%'
or t_BD_Material.fnumber  like '10.415%'
or t_BD_Material.fnumber  like '10.416%'
or t_BD_Material.fnumber  like '10.417%'
or t_BD_Material.fnumber  like '01.07.0142%' or t_BD_Material.fnumber  like '05.902.2027.225.215%' 
or t_BD_Material.fnumber  like '05.902.2027.435.215%' or t_BD_Material.fnumber  like '05.902.2027.827.225%'
or t_BD_Material.fnumber  like '05.902.2027.827.435%' or t_BD_Material.fnumber  like '06.03.0003%' 
or t_BD_Material.fnumber  like '06.03.0006%' or t_BD_Material.fnumber  like '06.99.0026%'  
or t_BD_Material.fnumber  like '06.99.0028%' or t_BD_Material.fnumber  like '06.99.0029%'
or t_BD_Material.fnumber  like '06.99.0045%') and tso.FBILLNO=alt.Salenumber and  tsoe.FSEQ=alt.Linenumber 
 group by  alt.Salenumber,alt.Linenumber,alt.Amount,T_BD_MATERIAL_L.FNAME) a 
 where Allocationview.Salenumber=a.Salenumber and Allocationview.Linenumber=a.Linenumber
 and Allocationview.Packcode<>a.Packcode  ");
            DBUtils.Execute(ctx, strSql);

//            //滑动门 继续去重复扫码 lc add 20190924
//            strSql = string.Format(@"/*dialect*/delete Allocationview from
//  (select alt.Salenumber,alt.Linenumber,alt.Amount,alt.Packcode ,min(alt.scantime) scantime ,T_BD_MATERIAL_L.FNAME
//  from T_SAL_ORDERENTRY  tsoe , T_SAL_ORDER tso,t_BD_Material t_BD_Material,T_BD_MATERIAL_L T_BD_MATERIAL_L,Allocationview alt
//   where tsoe.fid=tso.fid and t_BD_Material.FMASTERID=T_BD_MATERIAL_L.FMATERIALID
//  and t_BD_Material.FMATERIALID=tsoe.FMATERIALID and (t_BD_Material.fnumber  like '10.401%' or t_BD_Material.fnumber  like '10.402%' 
//or t_BD_Material.fnumber  like '10.403%' or t_BD_Material.fnumber  like '10.404%'  
//or t_BD_Material.fnumber  like '10.405%'
//or t_BD_Material.fnumber  like '10.406%' or t_BD_Material.fnumber  like '10.407%' 
//or t_BD_Material.fnumber  like '10.408%' or t_BD_Material.fnumber  like '10.409%'  
//or t_BD_Material.fnumber  like '10.410%' or t_BD_Material.fnumber  like '10.411%' 
//or t_BD_Material.fnumber  like '10.412%' 
//or t_BD_Material.fnumber  like '10.413%' 
//) and tso.FBILLNO=alt.Salenumber and  tsoe.FSEQ=alt.Linenumber 
// group by  alt.Salenumber,alt.Linenumber,alt.Amount,alt.Packcode,T_BD_MATERIAL_L.FNAME) a 
// where Allocationview.Salenumber=a.Salenumber and Allocationview.Linenumber=a.Linenumber
// and Allocationview.Packcode=a.Packcode and  Allocationview.scantime<>a.scantime  ");
//            DBUtils.Execute(ctx, strSql);

            //删除重复扫码数据
            strSql = string.Format(@"/*dialect*/delete Allocationview from
  (select alt.Salenumber,alt.Linenumber,alt.Amount,min(alt.Packcode) packcode,T_BD_MATERIAL_L.FNAME,t_BD_Material.fnumber
  from T_SAL_ORDERENTRY  tsoe , T_SAL_ORDER tso,t_BD_Material t_BD_Material,T_BD_MATERIAL_L T_BD_MATERIAL_L,Allocationview alt
   where tsoe.fid=tso.fid and t_BD_Material.FMASTERID=T_BD_MATERIAL_L.FMATERIALID
  and t_BD_Material.FMATERIALID=tsoe.FMATERIALID and t_BD_Material.fnumber  like '10.501%' and (LEN(t_BD_Material.fnumber ) - LEN(REPLACE(t_BD_Material.fnumber , '.', '  ')))<=-5
   and tso.FBILLNO=alt.Salenumber and  tsoe.FSEQ=alt.Linenumber 
 group by  alt.Salenumber,alt.Linenumber,alt.Amount,T_BD_MATERIAL_L.FNAME,t_BD_Material.fnumber) a 
 where Allocationview.Salenumber=a.Salenumber and Allocationview.Linenumber=a.Linenumber
 and Allocationview.Packcode<>a.Packcode and  dbo.getField6Value(a.FNUMBER,'.')>=500  and  dbo.getField4Value(a.FNUMBER,'.')>0  and dbo.getField5Value(a.FNUMBER,'.')>0  ");
            DBUtils.Execute(ctx, strSql);

            //匹配不存在于接口数据表的视图物理表的数据 插入到 接口数据表中 sum(amount)
            strSql = string.Format(@"/*dialect*/ insert into altable select salenumber,linenumber,packcode,sum(amount),warehouseout, warehousein,[procedure],0,scantime,fdate ,getdate() Fsubdate
from Allocationview v where not exists(select * from altable pr where pr.salenumber=v.Salenumber and pr.linenumber=v.Linenumber and 
v.packcode=pr.packcode and v.scantime=pr.scantime) group by salenumber,linenumber,packcode,warehouseout, warehousein,[procedure],scantime,fdate
 ");
            DBUtils.Execute(ctx, strSql);

            //去除packcode第二次数据 status=2
            strSql = @"/*dialect*/update altable set status=2 from (
select Salenumber,Linenumber,Packcode,min(Scantime) Scantime from altable group by Salenumber,Linenumber,Packcode) a
 where altable.Salenumber=a.Salenumber and altable.Linenumber=a.Linenumber and altable.Packcode=a.Packcode and altable.Scantime<>a.Scantime and status=0 ";
            DBUtils.Execute(ctx, strSql);

            //滑动门 去重复 20190924 按包装码相同，同一订单，

            strSql = @"/*dialect*/ 
update altable set status=2 
 from  
  (
  select alt.Salenumber,alt.Linenumber,1 amont, min(alt.scantime) scantime ,T_BD_MATERIAL_L.FNAME
  from T_SAL_ORDERENTRY  tsoe , T_SAL_ORDER tso,t_BD_Material t_BD_Material,T_BD_MATERIAL_L T_BD_MATERIAL_L,altable alt
   where tsoe.fid=tso.fid and t_BD_Material.FMASTERID=T_BD_MATERIAL_L.FMATERIALID
  and t_BD_Material.FMATERIALID=tsoe.FMATERIALID and (t_BD_Material.fnumber  like '10.401%' or t_BD_Material.fnumber  like '10.402%' 
or t_BD_Material.fnumber  like '10.403%' or t_BD_Material.fnumber  like '10.404%'  
or t_BD_Material.fnumber  like '10.405%'
or t_BD_Material.fnumber  like '10.406%' or t_BD_Material.fnumber  like '10.407%' 
or t_BD_Material.fnumber  like '10.408%' or t_BD_Material.fnumber  like '10.409%'  
or t_BD_Material.fnumber  like '10.410%' or t_BD_Material.fnumber  like '10.411%' 
or t_BD_Material.fnumber  like '10.412%' 
or t_BD_Material.fnumber  like '10.413%' 
or t_BD_Material.fnumber  like '10.414%'
or t_BD_Material.fnumber  like '10.415%'
or t_BD_Material.fnumber  like '10.416%'
or t_BD_Material.fnumber  like '10.417%'
) and tso.FBILLNO=alt.Salenumber and  tsoe.FSEQ=alt.Linenumber 
and alt.status=0  
 group by  alt.Salenumber,alt.Linenumber,T_BD_MATERIAL_L.FNAME
 
 ) a 
 where altable.Salenumber=a.Salenumber and altable.Linenumber=a.Linenumber
   and altable.scantime<>a.scantime and altable.status=0
  ";
            DBUtils.Execute(ctx, strSql);


            //把接口数据表中status为0的数据（未插入到待录入表的数据状态） sum数量 插入到 待录入表临时表Altabletemp
            strSql = @"/*dialect*/insert into altablein select Salenumber,Linenumber,Packcode,Amount,Warehouseout,Warehousein,[Procedure],status,Scantime,fdate,Fsubdate,0,'','','','',0,'' from altable where status=0";
            DBUtils.Execute(ctx, strSql);

            //把接口数据表中status全置为1 （已插入到待录入表的数据状态）
            strSql = @"/*dialect*/update altable set status=1 where  status=0 ";
            DBUtils.Execute(ctx, strSql);
        }
        private void checkData(Context ctx)
        {
            //删除amount=0的数据
            string strSql = string.Format(@"/*dialect*/delete altablein where amount=0 ");
            DBUtils.Execute(ctx, strSql);

            strSql = string.Format(@"/*dialect*/update altablein set status=0 ,Fsubdate='1900-01-01 00:00:00.000',ferrorstatus=0,ferrormsg='' where ferrormsg='3D业务人员手工审核成功' and fbillno=''
 ");
            DBUtils.Execute(ctx, strSql);

            //查询无上游单据数据 写入错误信息表
            strSql = string.Format(@"/*dialect*/	INSERT INTO Allocationtable select id FBILLNO,'A' FDOCUMENTSTATUS, salenumber SALENUMBER,linenumber LINENUMBER,packcode,id PRTABLEINID,
'无对应销售订单' REASON,fdate FDATE,getdate() FSUBDATE,'' from altablein de 
left join (select tso.fid fid,tsoe.FENTRYID FDETAILID,tso.FBILLNO,tsoe.FSEQ ,tso.fdocumentstatus
from T_SAL_ORDER tso,T_SAL_ORDERENTRY tsoe where tso.fid=tsoe.FID) a
on de.Salenumber=a.FBILLNO and de.Linenumber=a.FSEQ
where de.status=0 and (a.fid is null or a.FDETAILID is null or a.FDOCUMENTSTATUS<>'C')
");
            DBUtils.Execute(ctx, strSql);
            //把无上游单据status标识为2
            strSql = string.Format(@"/*dialect*/update altablein set status=2,ferrormsg='无对应销售订单' from 
(select de.id from altablein de 
left join (select tso.fid fid,tsoe.FENTRYID FDETAILID,tso.FBILLNO,tsoe.FSEQ ,tso.fdocumentstatus
from T_SAL_ORDER tso,T_SAL_ORDERENTRY tsoe where tso.fid=tsoe.FID) a
on de.Salenumber=a.FBILLNO and de.Linenumber=a.FSEQ
where de.status=0 and (a.fid is null or a.FDETAILID is null or a.FDOCUMENTSTATUS<>'C' ) ) b where  altablein.id=b.id");
            DBUtils.Execute(ctx, strSql);

            //查询物料启用BOM管理，但是销售订单中未选中BOM版本
            strSql = string.Format(@"/*dialect*/ INSERT INTO Allocationtable select id FBILLNO,'A' FDOCUMENTSTATUS, salenumber SALENUMBER,linenumber LINENUMBER,packcode,id PRTABLEINID,
'物料启用BOM管理，但是销售订单中未选中BOM版本' REASON,fdate FDATE,getdate() FSUBDATE,'' from altablein de 
left join (select tso.fid fid,tsoe.FENTRYID FDETAILID,tso.FBILLNO,tsoe.FSEQ ,tsoe.FBOMID,tbm.FISENABLE
from T_SAL_ORDER tso,T_SAL_ORDERENTRY tsoe, t_BD_MaterialInvPty tbm
 where tso.fid=tsoe.FID and tsoe.FMATERIALID=tbm.FMATERIALID and tbm.FINVPTYID='10003') a
on de.Salenumber=a.FBILLNO and de.Linenumber=a.FSEQ
where a.FISENABLE=1 and a.FBOMID=0 and de.status=0
");
            DBUtils.Execute(ctx, strSql);
            //把查询物料启用BOM管理，但是销售订单中未选中BOM版本的单据status标识为2
            strSql = string.Format(@"/*dialect*/update altablein set status=2,ferrormsg='物料启用BOM管理，但是销售订单中未选中BOM版本' from 
(select de.id from altablein de 
left join (select tso.fid fid,tsoe.FENTRYID FDETAILID,tso.FBILLNO,tsoe.FSEQ ,tsoe.FBOMID,tbm.FISENABLE
from T_SAL_ORDER tso,T_SAL_ORDERENTRY tsoe, t_BD_MaterialInvPty tbm
 where tso.fid=tsoe.FID and tsoe.FMATERIALID=tbm.FMATERIALID and tbm.FINVPTYID='10003') a
on de.Salenumber=a.FBILLNO and de.Linenumber=a.FSEQ
where a.FISENABLE=1 and a.FBOMID=0 and de.status=0 ) b where  altablein.id=b.id");
            DBUtils.Execute(ctx, strSql);


            //查询物料未维护生产车间
            strSql = string.Format(@"/*dialect*/ INSERT INTO Allocationtable select id FBILLNO,'A' FDOCUMENTSTATUS, salenumber SALENUMBER,linenumber LINENUMBER,packcode,id PRTABLEINID,
'物料未维护生产车间' REASON,fdate FDATE,getdate() FSUBDATE,'' from altablein de 
left join (select tso.fid fid,tsoe.FENTRYID FDETAILID,tso.FBILLNO,tsoe.FSEQ ,tbm.FWorkShopId
from T_SAL_ORDER tso,T_SAL_ORDERENTRY tsoe, t_BD_MaterialProduce tbm,t_BD_MaterialBase tbmb
 where tso.fid=tsoe.FID and tsoe.FMATERIALID=tbm.FMATERIALID and tbm.FMATERIALID=tbmb.FMATERIALID and FERPCLSID <> '1') a
on de.Salenumber=a.FBILLNO and de.Linenumber=a.FSEQ
where a.FWORKSHOPID=0  and de.status=0 
");
            DBUtils.Execute(ctx, strSql);
            //把查询物料未维护生产车间的单据status标识为2
            strSql = string.Format(@"/*dialect*/update altablein set status=2,ferrormsg='物料未维护生产车间' from 
(select de.id from altablein de 
left join (select tso.fid fid,tsoe.FENTRYID FDETAILID,tso.FBILLNO,tsoe.FSEQ ,tbm.FWorkShopId
from T_SAL_ORDER tso,T_SAL_ORDERENTRY tsoe, t_BD_MaterialProduce tbm,t_BD_MaterialBase tbmb
 where tso.fid=tsoe.FID and tsoe.FMATERIALID=tbm.FMATERIALID and tbm.FMATERIALID=tbmb.FMATERIALID and FERPCLSID <> '1') a
on de.Salenumber=a.FBILLNO and de.Linenumber=a.FSEQ
where a.FWORKSHOPID=0  and de.status=0  ) b where  altablein.id=b.id");
            DBUtils.Execute(ctx, strSql);


            //五金件标识为4 准备调拨状态
            strSql = string.Format(@"/*dialect*/update altablein set status=4 where Warehouseout='7.01' and status=0");
            DBUtils.Execute(ctx, strSql);


            //标识采购件
            strSql = string.Format(@"/*dialect*/   update altablein set isPur=1
 from T_SAL_ORDER tso,T_SAL_ORDERENTRY tsoe, t_BD_Material t_BD_Material
 where tso.fid = tsoe.FID and altablein.Linenumber = tsoe.FSEQ and altablein.Salenumber = tso.FBILLNO
 and t_BD_Material.FMATERIALID = tsoe.FMATERIALID
and tsoe.FMATERIALID in (select tbm.FMATERIALID
from t_BD_MaterialBase tbmb, t_BD_Material tbm
 where FERPCLSID='1' and tbm.FMATERIALID = tbmb.FMATERIALID) and altablein.isPur=0 and altablein.status=0");
            DBUtils.Execute(ctx, strSql);

            //标识采购件仓库
            strSql = string.Format(@"/*dialect*/ update altablein set PurStockId=a.FNUMBER from (
select distinct tbs.FNUMBER,pr.salenumber,pr.linenumber from T_SAL_ORDER tso,  
T_SAL_ORDERENTRY tsoe,altablein pr  ,Purchase2Stock ps,t_BD_Stock tbs
where tso.fid=tsoe.FID and pr.Linenumber=tsoe.FSEQ and pr.Salenumber=tso.FBILLNO and tsoe.FMATERIALID=ps.FMATERIAL and tbs.FSTOCKID=ps.FSTOCK and pr.isPur=1
and pr.status=0
 ) a where altablein.salenumber=a.salenumber and altablein.linenumber=a.linenumber");
            DBUtils.Execute(ctx, strSql);
            strSql = string.Format(@"/*dialect*/update altablein set PurStockId = '' where PurStockId is null");
            DBUtils.Execute(ctx, strSql);
            strSql = string.Format(@"/*dialect*/update altablein set isPur = '' where isPur is null");
            DBUtils.Execute(ctx, strSql);

            //查询没有对应仓库的采购件 写入错误信息表
            strSql = string.Format(@"/*dialect*/ insert into  Allocationtable select id FBILLNO,'A' FDOCUMENTSTATUS, pr.salenumber SALENUMBER,pr.linenumber LINENUMBER,pr.Packcode Packcode,id PRTABLEINID,
'采购件无对应仓库' REASON,fdate FDATE,getdate() FSUBDATE,'' from altablein pr where pr.status=0 and pr.PurStockId ='' and pr.isPur=1");
            DBUtils.Execute(ctx, strSql);
            //把采购件无对应仓库的数据 status标识为2
            strSql = string.Format(@"/*dialect*/update altablein set status=2, ferrormsg='采购件无对应仓库' where  altablein.status=0 
and altablein.PurStockId ='' and altablein.isPur=1 and altablein.Warehouseout<>'7.01' ");
            DBUtils.Execute(ctx, strSql);
            //调拨出库判断大于 销售订单数量 大于可调拨数量 写入错误信息表 lcdoit add 20190616
            strSql = string.Format(@"/*dialect*/insert into Allocationtable
select id FBILLNO,'A' FDOCUMENTSTATUS, altablein.salenumber SALENUMBER,altablein.linenumber LINENUMBER,packcode Packcode,id PRTABLEINID,
'大于可调拨数量' REASON,fdate FDATE,getdate() FSUBDATE,'' from altablein ,
 (select a.Salenumber,a.Linenumber
 from (select Salenumber,Linenumber,sum(Amount) amount from altablein where fbillno<>' ' group by  Salenumber,Linenumber) a,
  (select  tso.fbillno salenumber,tsoe.fseq linenumber, fqty amount
 from T_SAL_ORDER tso,T_SAL_ORDERENTRY tsoe,T_SAL_ORDERENTRY_R tsop 
where tso.fid=tsoe.fid and tsoe.FENTRYID=tsop.FENTRYID  ) b where a.Salenumber=b.salenumber and a.Linenumber=b.linenumber and a.amount>b.amount
) c
 where altablein.Salenumber=c.Salenumber and altablein.Linenumber=c.Linenumber  and  altablein.status=0 ");
            DBUtils.Execute(ctx, strSql);
            //把调拨数量大于销售订单中可出库数量的数据 status标识为2 lcdoit add 20190616
            strSql = string.Format(@"/*dialect*/ update altablein set status=2 ,ferrormsg='大于可调拨数量' from
 (select a.Salenumber,a.Linenumber
 from (select Salenumber,Linenumber,sum(Amount) amount from altablein where fbillno<>' ' group by  Salenumber,Linenumber) a,
  (select  tso.fbillno salenumber,tsoe.fseq linenumber,fqty amount
 from T_SAL_ORDER tso,T_SAL_ORDERENTRY tsoe,T_SAL_ORDERENTRY_R tsop 
where tso.fid=tsoe.fid and tsoe.FENTRYID=tsop.FENTRYID  ) b where a.Salenumber=b.salenumber and a.Linenumber=b.linenumber and a.amount>b.amount
) c
 where altablein.Salenumber=c.Salenumber and altablein.Linenumber=c.Linenumber  and  altablein.status=0 ");
            DBUtils.Execute(ctx, strSql);

            //标识为 3 预检完成准备入库状态
            strSql = string.Format(@"/*dialect*/ update altablein set status=3 where  status=0  ");
            DBUtils.Execute(ctx, strSql);

            strSql = string.Format(@"/*dialect*/ update Allocationtable set fdate = '2019-05-1 00:00:00.000' where fdate< '2019-05-1 00:00:00.000'  ");
            DBUtils.Execute(ctx, strSql);

            strSql = string.Format(@"/*dialect*/ update altablein set fdate = '2019-05-1 00:00:00.000' where fdate< '2019-05-1 00:00:00.000'  ");
            DBUtils.Execute(ctx, strSql);
            



        }
    }
}
