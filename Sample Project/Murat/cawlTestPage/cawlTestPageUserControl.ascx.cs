using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using cawl4SharePoint;
using Microsoft.SharePoint;
using System.Web;
using System.Text;

namespace Murat.cawlTestPage
{
    public partial class cawlTestPageUserControl : UserControl
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            
         

        }


        protected void Button4_Click(object sender, EventArgs e)
        {
            cawl_QueryBuilder cawl = new cawl_QueryBuilder();

            cawl.Where("Title", "=", "Murat");
            cawl.or_Where("Title", "=", "Joe");
            cawl.Order_by("Title", "ASC");

            //Assuming you already created users list
            cawl.Get("Users");

            SPListItemCollection listitems = cawl.ListItemCollection();

            GridView1.DataSource = listitems.GetDataTable();
            GridView1.DataBind();

            Label1.Text = HttpUtility.HtmlEncode(cawl.QueryString());
        }

        protected void Button1_Click(object sender, EventArgs e)
        {
            cawl_QueryBuilder cawl = new cawl_QueryBuilder();

            cawl.Set("Title", "Murat");
            cawl.Set("Status", "Active");
            cawl.Insert("Users");

            cawl.Set("Title", "Joe");
            cawl.Set("Status", "Active");
            cawl.Insert("Users");

            cawl.Set("Title", "Mike");
            cawl.Insert("Users");

            cawl.Set("Title", "Mike");
            cawl.Set("Status", "Active");
            cawl.Insert("Users");

            cawl.Set("Title", "Mike");
            cawl.Insert("Users");


        }

        protected void Button2_Click(object sender, EventArgs e)
        {
            cawl_QueryBuilder cawl = new cawl_QueryBuilder();

            cawl.Set("Status", "Passive");
            cawl.Where("Title", "=", "Mike");
            cawl.Update("Users");

        }

        protected void Button3_Click(object sender, EventArgs e)
        {

            cawl_QueryBuilder cawl = new cawl_QueryBuilder();

            cawl.Where("Status", "=", "Passive");
            cawl.Delete("Users");
        }

       

        

 














    }
}

