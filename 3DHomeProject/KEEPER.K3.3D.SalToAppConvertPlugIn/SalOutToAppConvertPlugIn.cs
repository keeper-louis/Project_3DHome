using Kingdee.BOS.Core;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.Metadata.ConvertElement.PlugIn;
using Kingdee.BOS.Core.Metadata.ConvertElement.PlugIn.Args;
using Kingdee.BOS.Core.Metadata.EntityElement;
using Kingdee.BOS.Core.Metadata.FormElement;
using Kingdee.BOS.Orm.DataEntity;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kingdee.BOS;
using Kingdee.BOS.App.Data;

namespace KEEPER.K3.THD.SalToAppConvertPlugIn
{
    [Description("销售出库-应收单，按照销售价目表重新计算单价")]
    public class SalOutToAppConvertPlugIn : AbstractConvertPlugIn
    {
        /// <summary>
        /// 主单据体的字段携带完毕，与源单的关联关系创建好之后，触发此事件
        /// </summary>
        /// <param name="e"></param>
        public override void OnAfterCreateLink(CreateLinkEventArgs e)
        {
            base.OnAfterCreateLink(e);
            //预先获取一些必要的元数据，后续代码要用到

            //目标单第一单据体
            Entity mainEntity = e.TargetBusinessInfo.GetEntity("FEntityDetail");



            // 获取生成的全部下游单据
            ExtendedDataEntity[] billDataEntitys = e.TargetExtendedDataEntities.FindByEntityKey("FBillHead");

            // 对下游单据，逐张单据进行处理
            foreach (var item in billDataEntitys)
            {
                DynamicObject dataObject = item.DataEntity;

                // 定义一个集合，用于收集本单对应的源单内码
                HashSet<long> srcBillIds = new HashSet<long>();

                // 开始到主单据体中，读取关联的源单内码
                DynamicObjectCollection mainEntryRows =
                    mainEntity.DynamicProperty.GetValue(dataObject) as DynamicObjectCollection;

                //获取销售价目表XSJMB0002内容
                Dictionary<long, double> disCounts = GetDiscounts(base.Context);
                //遍历应收单，通过销售折扣计算折后的含税单价赋值。
                foreach (DynamicObject mainEntryRow in mainEntryRows)
                {
                    double discount = 0.0;
                    if (disCounts.TryGetValue(Convert.ToInt64(mainEntryRow["MATERIALID_Id"]), out discount))
                    {
                        mainEntryRow["TaxPrice"] = discount;
                    }
                }
            }
        }

        /// <summary>
        /// 获取销售价目表XSJMB0002的明细内容
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        private Dictionary<long, double> GetDiscounts(Context context)
        {
            Dictionary<long, double> disCounts = new Dictionary<long, double>();
            string strSql = string.Format(@"select/*dialect*/ m.FMATERIALID, m.FPRICE
  from (SELECT distinct c.fseq, c.FMATERIALID, c.FPRICE
          FROM t_sal_pricelist a, t_sal_pricelistentry c
         WHERE a.FID = c.FID
           AND a.FDOCUMENTSTATUS = 'C'
           AND a.FFORBIDSTATUS = 'A'
           AND c.FFORBIDSTATUS = 'A'
           AND c.FROWAUDITSTATUS = 'A'
           AND a.FNUMBER = 'XSJMB0002') m
 inner join (SELECT distinct min(c.fseq) fseq, c.FMATERIALID
               FROM t_sal_pricelist a, t_sal_pricelistentry c
              WHERE a.FID = c.FID
                AND a.FDOCUMENTSTATUS = 'C'
                AND a.FFORBIDSTATUS = 'A'
                AND c.FFORBIDSTATUS = 'A'
                AND c.FROWAUDITSTATUS = 'A'
                AND a.FNUMBER = 'XSJMB0002'
              group by c.FMATERIALID) n
    on m.FMATERIALID = n.FMATERIALID
   and m.fseq = n.fseq
");
            DynamicObjectCollection appDiscounts = DBUtils.ExecuteDynamicObject(context, strSql);
            if (appDiscounts != null && appDiscounts.Count() > 0)
            {
                foreach (DynamicObject item in appDiscounts)
                {
                    disCounts.Add(Convert.ToInt64(item["FMATERIALID"]), Convert.ToDouble(item["FPRICE"]));
                }
            }
            return disCounts;
        }
    }
}
