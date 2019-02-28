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
            //从视图插入数据到视图物理表
            strSql = string.Format(@"/*dialect*/select*,CONVERT(varchar(10),scantime, 120) fdate into  Allocationview from [192.168.1.77].[DB].dbo.Allocationview v ");
            DBUtils.Execute(ctx, strSql);

            //删除问题数据
            strSql = string.Format(@"/*dialect*/delete Allocationview where linenumber='加急，单独入库' 
or linenumber like '190%' or linenumber like '201%' 
or linenumber='c' ");
            DBUtils.Execute(ctx, strSql);
            strSql = string.Format(@"/*dialect*/delete Allocationview where Salenumber = '8851168' and Linenumber = '1' and Amount<> 1 ");
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

            //匹配不存在于接口数据表的视图物理表的数据 插入到 接口数据表中 sum(amount)
            strSql = string.Format(@"/*dialect*/ insert into altable select salenumber,linenumber,packcode,sum(amount),warehouseout, warehousein,[procedure],0,scantime,fdate ,getdate() Fsubdate
from Allocationview v where not exists(select * from altable pr where pr.salenumber=v.Salenumber and pr.linenumber=v.Linenumber and 
v.packcode=pr.packcode and v.scantime=pr.scantime) group by salenumber,linenumber,packcode,warehouseout, warehousein,[procedure],scantime,fdate
 ");
            DBUtils.Execute(ctx, strSql);


            //标识过期数据 status=4
            strSql = @"/*dialect*/ update altable set status = 4 where Scantime< '2018-12-1' and status=0 ";
            DBUtils.Execute(ctx, strSql);
            //去除packcode第二次数据 status=2
            strSql = @"/*dialect*/update altable set status=2 from (
select Salenumber,Linenumber,Packcode,min(Scantime) Scantime from altable group by Salenumber,Linenumber,Packcode) a
 where altable.Salenumber=a.Salenumber and altable.Linenumber=a.Linenumber and altable.Packcode=a.Packcode and altable.Scantime<>a.Scantime and status=0 ";
            DBUtils.Execute(ctx, strSql);

            //把接口数据表中status为0的数据（未插入到待录入表的数据状态） sum数量 插入到 待录入表临时表Altabletemp
            strSql = @"/*dialect*/ insert into altablein select * from altable where status=0";
            DBUtils.Execute(ctx, strSql);

            //把接口数据表中status全置为1 （已插入到待录入表的数据状态）
            strSql = @"/*dialect*/update altable set status=1 where  status=0 ";
            DBUtils.Execute(ctx, strSql);
        }
        private void checkData(Context ctx)
        {

        }
    }
}
