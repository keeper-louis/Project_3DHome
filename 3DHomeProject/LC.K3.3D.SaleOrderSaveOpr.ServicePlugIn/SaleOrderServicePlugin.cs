using Kingdee.BOS.App.Data;
using Kingdee.BOS.Core;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Orm.DataEntity;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace LC.K3._3D.SaleOrderSaveOpr.ServicePlugIn
{
    [Description("销售订单保存时携带单价")]
    public class SaleOrderServicePlugin : AbstractOperationServicePlugIn
    {

        public override void OnPreparePropertys(PreparePropertysEventArgs e)
        {
            base.OnPreparePropertys(e);
           
        }
        public override void EndOperationTransaction(EndOperationTransactionArgs e)
        {
            base.EndOperationTransaction(e);

            if (this.FormOperation.Operation.Equals("Save") || this.FormOperation.Operation.Equals("Sumbit"))
            {
                if (e.DataEntitys != null && e.DataEntitys.Count<DynamicObject>() > 0)
                {
                    foreach (DynamicObject item in e.DataEntitys)
                    {
                        // 订单明细分录
                        DynamicObjectCollection SaleOrderEntry = item["SaleOrderEntry"] as DynamicObjectCollection;
                        //业务类型
                        item["BusinessType"] = "NORMAL";//普通销售

                        // 源单明细中有记录
                        if (SaleOrderEntry != null && SaleOrderEntry.Count() > 0)
                        {
                            foreach (DynamicObject entry in SaleOrderEntry)
                            {
                                if (entry["MaterialId"] != null)
                                {
                                    // MaterialId
                                    string strSql = "";
                                    String FNUMBER = "";
                                    String FMATERIALID = "";
                                    FNUMBER = Convert.ToString(((DynamicObject)entry["MaterialId"])["Number"]);
                                    FMATERIALID = Convert.ToString(((DynamicObject)entry["MaterialId"])["Id"]);
                                    string Strm = "0";
                                    string Stra = "0";
                                    string Strb = "0";
                                    string Strc = "0";
                                    string Strd = "0";
                                    if (getElementLength(FNUMBER, ".") >= 3)
                                    {
                                        Strm = getElement(FNUMBER, 0, ".") + "." + getElement(FNUMBER, 1, ".") + "." + getElement(FNUMBER, 2, ".");
                                    }
                                    if (getElementLength(FNUMBER, ".") >= 4)
                                    {
                                        Stra = getElement(FNUMBER, 3, ".");
                                    }
                                    if (getElementLength(FNUMBER, ".") >= 5)
                                    {
                                        Strb = getElement(FNUMBER, 4, ".");
                                    }
                                    if (getElementLength(FNUMBER, ".") >= 6)
                                    {
                                        Strc = getElement(FNUMBER, 5, ".");
                                    }
                                    if (getElementLength(FNUMBER, ".") >= 7)
                                    {
                                        Strd = getElement(FNUMBER, 6, ".");
                                    }

                                    if (Stra.Equals("5"))
                                    {
                                        Stra = "6";
                                    }
                                    if (Stra.Equals("6"))
                                    {
                                        Stra = "5";
                                    }

                                    string Strm1 = "0";
                                    string Stra1 = "0";
                                    string Strb1 = "0";
                                    string Strc1 = "0";
                                    string Strd1 = "0";
                                    if (getElementLength(FNUMBER, ".") >= 4)
                                    {
                                        Strm1 = getElement(FNUMBER, 0, ".") + "." + getElement(FNUMBER, 1, ".") + "." + getElement(FNUMBER, 2, ".") + "." + getElement(FNUMBER, 3, ".");
                                    }
                                    if (getElementLength(FNUMBER, ".") >= 5)
                                    {
                                        Stra1 = getElement(FNUMBER, 4, ".");
                                    }
                                    if (getElementLength(FNUMBER, ".") >= 6)
                                    {
                                        Strb1 = getElement(FNUMBER, 5, ".");
                                    }
                                    if (getElementLength(FNUMBER, ".") >= 7)
                                    {
                                        Strc1 = getElement(FNUMBER, 6, ".");
                                    }
                                    if (getElementLength(FNUMBER, ".") >= 8)
                                    {
                                        Strd1 = getElement(FNUMBER, 7, ".");
                                    }


                                    strSql = string.Format(@"/*dialect*/select FCOSTITEMSTPYE,FPROJECTUNIT,FAREAFORMULA,FMATERIALRANGE,FNAME,FORDEROFVALUE ,F_PAEZ_DECIMAL saleprice from T_PAEZ_MRelatedType tpm 
inner join T_PAEZ_MRelatedTypeEntry tpme on tpm.FID=tpme.FID 
inner join PAEZ_t_Cust_Entry100011 a on tpme.FAREAFORMULA=a.fid
inner join PAEZ_t_Cust_Entry100011_L b on a.fid=b.fid where FMATERIALRANGE='{0}' or FMATERIALRANGE='{1}' ", Strm, Strm1);
                                    DynamicObjectCollection Col = DBUtils.ExecuteDynamicObject(this.Context, strSql);
                                    if (Col.Count() > 1)
                                    {

                                    }
                                    else if (Col.Count() == 0)
                                    {

                                    }
                                    else if (Col.Count() == 1)
                                    {
                                        entry["TaxPrice"] = Convert.ToDecimal(Col[0]["saleprice"]);

                                        
                                        entry["Price"] = Col[0]["saleprice"];
                                        
                                    }

                                }
                            }
                        }
                    }
                }
            }
        }

        public override void BeforeExecuteOperationTransaction(BeforeExecuteOperationTransaction e)
        {
            if (e.SelectedRows == null || e.SelectedRows.Count<ExtendedDataEntity>() <= 0)
            {
                return;
            }
            foreach (ExtendedDataEntity current in e.SelectedRows)
            {
                DynamicObjectCollection SaleOrderEntry = current.DataEntity["SaleOrderEntry"] as DynamicObjectCollection;
                 

                    foreach (DynamicObject entry in SaleOrderEntry)
                    {
                        if (entry["MaterialId"] != null)
                        {
                            // MaterialId
                            string strSql = "";
                            String FNUMBER = "";
                            String FMATERIALID = "";
                            FNUMBER = Convert.ToString(((DynamicObject)entry["MaterialId"])["Number"]);
                            FMATERIALID = Convert.ToString(((DynamicObject)entry["MaterialId"])["Id"]);
                            string Strm = "0";
                            string Stra = "0";
                            string Strb = "0";
                            string Strc = "0";
                            string Strd = "0";
                            if (getElementLength(FNUMBER, ".") >= 3)
                            {
                                Strm = getElement(FNUMBER, 0, ".") + "." + getElement(FNUMBER, 1, ".") + "." + getElement(FNUMBER, 2, ".");
                            }
                            if (getElementLength(FNUMBER, ".") >= 4)
                            {
                                Stra = getElement(FNUMBER, 3, ".");
                            }
                            if (getElementLength(FNUMBER, ".") >= 5)
                            {
                                Strb = getElement(FNUMBER, 4, ".");
                            }
                            if (getElementLength(FNUMBER, ".") >= 6)
                            {
                                Strc = getElement(FNUMBER, 5, ".");
                            }
                            if (getElementLength(FNUMBER, ".") >= 7)
                            {
                                Strd = getElement(FNUMBER, 6, ".");
                            }

                            if (Stra.Equals("5"))
                            {
                                Stra = "6";
                            }
                            if (Stra.Equals("6"))
                            {
                                Stra = "5";
                            }

                            string Strm1 = "0";
                            string Stra1 = "0";
                            string Strb1 = "0";
                            string Strc1 = "0";
                            string Strd1 = "0";
                            if (getElementLength(FNUMBER, ".") >= 4)
                            {
                                Strm1 = getElement(FNUMBER, 0, ".") + "." + getElement(FNUMBER, 1, ".") + "." + getElement(FNUMBER, 2, ".") + "." + getElement(FNUMBER, 3, ".");
                            }
                            if (getElementLength(FNUMBER, ".") >= 5)
                            {
                                Stra1 = getElement(FNUMBER, 4, ".");
                            }
                            if (getElementLength(FNUMBER, ".") >= 6)
                            {
                                Strb1 = getElement(FNUMBER, 5, ".");
                            }
                            if (getElementLength(FNUMBER, ".") >= 7)
                            {
                                Strc1 = getElement(FNUMBER, 6, ".");
                            }
                            if (getElementLength(FNUMBER, ".") >= 8)
                            {
                                Strd1 = getElement(FNUMBER, 7, ".");
                            }


                            strSql = string.Format(@"/*dialect*/select FCOSTITEMSTPYE,FPROJECTUNIT,FAREAFORMULA,FMATERIALRANGE,FNAME,FORDEROFVALUE ,F_PAEZ_DECIMAL saleprice from T_PAEZ_MRelatedType tpm 
inner join T_PAEZ_MRelatedTypeEntry tpme on tpm.FID=tpme.FID 
inner join PAEZ_t_Cust_Entry100011 a on tpme.FAREAFORMULA=a.fid
inner join PAEZ_t_Cust_Entry100011_L b on a.fid=b.fid where FMATERIALRANGE='{0}' or FMATERIALRANGE='{1}' ", Strm, Strm1);
                            DynamicObjectCollection Col = DBUtils.ExecuteDynamicObject(this.Context, strSql);
                            if (Col.Count() > 1)
                            {

                            }
                            else if (Col.Count() == 0)
                            {

                            }
                            else if (Col.Count() == 1)
                            {
                                entry["TaxPrice"] = Convert.ToDecimal(Col[0]["saleprice"]);


                                entry["Price"] = Col[0]["saleprice"];

                            }

                        }
                    }
                
             
            }
        }
      private int getElementLength(String str, String c)
        {
            string[] xx = str.Split(new string[] { c }, StringSplitOptions.None);

            return xx.Length;
        }

        private string getElement(String str, int a, String c)
        {
            string[] xx = str.Split(new string[] { c }, StringSplitOptions.None);

            return xx[a];
        }
    }
}
