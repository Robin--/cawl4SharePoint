using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.SharePoint;
using System.Collections;
using Microsoft.SharePoint.Utilities;

namespace cawl4SharePoint
{

    /*
    * cawl4sharepoint: http://www.makdeniz.com/cawl4sharepoint/
    *
    * Copyright (c) 2012 Zenithsoft.co
    * Author: Murat Akdeniz 
    * www.makdeniz.com
    * Version : 3.0
    * 
    * Permission is hereby granted, free of charge, to any person obtaining a copy
    * of this software and associated documentation files (the "Software"), to deal
    * in the Software without restriction, including without limitation the rights
    * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
    * copies of the Software, and to permit persons to whom the Software is
    * furnished to do so, subject to the following conditions:
    *
    * The above copyright notice and this permission notice shall be included in
    * all copies or substantial portions of the Software.
    *
    * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
    * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
    * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
    * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
    * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
    * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
    * THE SOFTWARE.
    */


    public class cawl_QueryBuilder
    {


        SPWeb web = SPContext.Current.Site.OpenWeb();
        
        ArrayList _where    = new ArrayList();
        ArrayList _orderby  = new ArrayList();
        ArrayList _options  = new ArrayList();
        ArrayList _set      = new ArrayList();
        ArrayList _join     = new ArrayList();
        ArrayList _select   = new ArrayList();
        SPQuery   _query    = new SPQuery();


        string _List_Name = null;
        string _debug = "";
        string _CreatedItemId = null;

       
        public void Where(string Field, string Operator, string Value)
        {
            _where.Add(new string[] { Field, Operator, Value, "and" });
        }

        public void or_Where(string Field, string Operator, string Value)
        {
            _where.Add(new string[] { Field, Operator, Value, "or" });
        }

        public void Join(string ParentListName, string ChildListLookupColumnName)
        {
            _join.Add(new string[] { ParentListName, ChildListLookupColumnName });
        }

        public void Select(string Field)
        {
            _select.Add(new string[] { Field });
        }

        public void Set(string Setting, string Value)
        {
            _set.Add(new string[] { Setting, Value });
        }

        public void Site(string SiteName )
        {
                SPSite newsite = new SPSite(SiteName);
                web = newsite.OpenWeb();
            
        }

        public void Order_by(string Field, string Order)
        {
            _orderby.Add(new string[] { Field, Order, });
        }

        public void Recursive()
        {
            _query.ViewAttributes = "Scope=\"RecursiveAll\"";

        }

        public void RowLimit(int row)
        {
            _options.Add(new string[] { "<RowLimit>" + row + "</RowLimit>" });
        }

        public void RunWithElevatedPrivileges()
        {
            SPSecurity.RunWithElevatedPrivileges(delegate
                       {
                           using (var site = new SPSite(SPContext.Current.Site.ID))
                           {
                               using (var newWeb = site.OpenWeb(SPContext.Current.Web.ID))
                               {
                                   web= newWeb;
                               }
                           }
                       });

            
        }


        private void _InsertDebug(string text)
        {
            _debug = _debug + "</br>" + DateTime.Now.ToString() + " >" + text;
        }

        public SPQuery Query()
        {
            return _query;
        }

        public string QueryString()
        {
            return _query.Query.ToString();
        }

        private SPList getListAsObject(string ListName)
        {
            // Create a full url of list by internal name of the list
            string listUrl = web.ServerRelativeUrl + "/lists/" + ListName;

            // Check if this list exist, if not it might be document library
            if (web.GetList(listUrl) == null)
            {
                listUrl = web.ServerRelativeUrl + "/" + ListName;
            }

            return web.GetList(listUrl);
        }

        public SPListItemCollection ListItemCollection()
        {
            // Get the list object 
            SPList list = getListAsObject(_List_Name);
            
            // Create item collection by query.
            // It is assumed that _query (SPQuery object) is already filled with query string. It means that Get function is already called.
            SPListItemCollection ItemCollection = list.GetItems(_query);
            
            return ItemCollection;
        }

        public string CreatedItemId()
        {
            return _CreatedItemId.ToString();
        }
        
        public string Debug()
        {
            return _debug.ToString();
        }
        
        public void Get(string List_Name)
        {
            _List_Name = List_Name;
            
            StringBuilder queryString = new StringBuilder();

            // Here we bulid where and or_where conditions
            #region where

            if (_where.Count != 0)
            {
                queryString.Append("<Where>");
                
                // We reversed where array becasue of the syntax of caml query
                // last written condition comes first on caml query
                _where.Reverse();

                int counter = 1;
                foreach (object itemOfWhereConditions in _where)
                {
                    string[] ItemArray = ((string[])itemOfWhereConditions);

                    if (counter < _where.Count)
                    {
                        if (ItemArray[3] == "and")
                        {
                            queryString.Append("<And>");
                        }
                        else
                        {
                            queryString.Append("<Or>");
                        }
                    }
                    counter++;
                }

                // Again reverse it to initial order to work on fields
                _where.Reverse();

                int k = 1;

                foreach (object itemOfWhereConditionsAfterReverse in _where)
                {
                    string[] ItemArray = ((string[])itemOfWhereConditionsAfterReverse);
                    
                    // Here we get the filed type by looking to list properties
                    string field_type = Field_Type(ItemArray[0]);

                    // We build real FieldRef node and add to query string
                    // FieldRef(string Name, string Type, string Value, string Operator)
                    // Outcome will be like : <FieldRef Name='UserName'/><Value Type='Text'>makdeniz</Value>
                    queryString.Append(FieldRef(ItemArray[0], field_type, ItemArray[2], ItemArray[1]));

                    if (k > 1)
                    {
                        if (ItemArray[3] == "and")
                        {
                            queryString.Append("</And>");
                        }
                        else
                        {
                            queryString.Append("</Or>");
                        }


                    }
                    k++;

                }
                queryString.Append("</Where>");
          }
          #endregion Where

            #region Order By
            if (_orderby.Count != 0)
            {

                queryString.Append("<OrderBy>");
                foreach (object item in _orderby)
                {
                    string[] ItemArray = ((string[])item);
                    if (ItemArray[1] == "ASC")
                    {
                        queryString.Append("<FieldRef Name='" + ItemArray[0] + "'/>");
                    }
                    else
                    {
                        queryString.Append("<FieldRef Name='" + ItemArray[0] + "' Ascending='FALSE' />");
                    }


                }
                queryString.Append("</OrderBy>");
            }

            #endregion Order By

            #region Option
            if (_options.Count != 0)
            {
                queryString.Append("<QueryOptions>");
                foreach (object item in _options)
                {
                    string[] ItemArray = ((string[])item);
                    queryString.Append(ItemArray[0]);
                }
                queryString.Append("</QueryOptions>");
            }
            #endregion Option

            #region Join

            if (_join.Count != 0)
            {
                StringBuilder joinstring = new StringBuilder();

                foreach (object item in _join)
                {
                    string[] ItemArray = ((string[])item);
                    joinstring.Append(join(ItemArray[0], ItemArray[1]));
                }

                _query.Joins = joinstring.ToString();
                
                StringBuilder tt = new StringBuilder();
                
                
                if (_join.Count != 0)
                {

                    StringBuilder ProjectedFields = new StringBuilder();

                    //
                    foreach (object Join_item in _join)
                    {
                        string[] Join_ItemArray = ((string[])Join_item);
                        if (_select.Count != 0)
                        {

                            foreach (object select_field in _select)
                            {
                                string[] Select_ItemArray = ((string[])select_field);

                                if (Select_ItemArray[0].Contains(Join_ItemArray[0] + "_"))
                                {
                                    string tst = Join_ItemArray[0].ToString() + "_";

                                    ProjectedFields.Append("<Field Name='" + Select_ItemArray[0] + "' Type='Lookup' List='" + Join_ItemArray[0] + "' ShowField='" + Select_ItemArray[0].Replace(tst.ToString(), "") + "'/>");
                                }
                            }

                        }//select end


                    }//join end

                    _query.ProjectedFields = ProjectedFields.ToString();
                }






            }




            #endregion Join

            #region viewfileds

            if (_select.Count != 0)
            {
                StringBuilder viewfileds = new StringBuilder();
                foreach (object item in _select)
                {
                    string[] ItemArray = ((string[])item);

                    viewfileds.Append("<FieldRef Name='" + ItemArray[0] + "' />");
                }

                _query.ViewFields = viewfileds.ToString();
            }

            #endregion viewfields
            
            // Every thing is done now we set the query string of _query object
            _query.Query = queryString.ToString();

        }//get end

        public void Delete(string ListName, Boolean ElevatedPrivileges = false)
        {
            // First build the query for given conditions
            Get(ListName);

            web.AllowUnsafeUpdates = true;

            // Get the list items
            SPListItemCollection listItems = ListItemCollection();

            int itemCount = listItems.Count;
            for (int i = itemCount - 1; i > -1; i--)
            {
                SPListItem item = listItems[i];
                // Here we delete list item
                listItems.Delete(i);
            }

            web.AllowUnsafeUpdates = false;
            _set.Clear();
        }

        public void Update(string ListName, Boolean ElevatedPrivileges = false)
        {
            // First build the query for given conditions
            Get(ListName);

            web.AllowUnsafeUpdates = true;

            // Get the list item that will be updated
            SPListItemCollection UpdateListitems = ListItemCollection();

            foreach (SPListItem Updateitem in UpdateListitems)
            {
                // Here we build fields and its value from _set Array Object 
                foreach (object setitem in _set)
                {
                    string[] ItemArray = ((string[])setitem);
                    string setting = ItemArray[0];
                    string value = ItemArray[1];
                    // Setting a  signle field name and its value 
                    Updateitem[setting] = value;
                }
                
                // All the fields and their values are ready.
                // Now we update a single list items fields by given values
                Updateitem.Update();
            }
            web.AllowUnsafeUpdates = false;

            _set.Clear();


        }

        public void Insert(string ListName)
        {
            
            _List_Name = ListName;
            web.AllowUnsafeUpdates = true;

            // Get the list object 
            SPList list = getListAsObject(_List_Name);
            
            SPListItemCollection listItems = list.Items;
            SPListItem item = listItems.Add();

            // Here we build fields and its value from _set Array Object 
            foreach (object setitem in _set)
            {
                string[] ItemArray = ((string[])setitem);
                string setting = ItemArray[0];
                string value = ItemArray[1];
                // Setting a  signle field name and its value 
                item[setting] = value;

            }

            // All the fields and their values are ready.
            // Now we update a single list items fields by given values. In this case list item will be inserted.
            item.Update();

            web.AllowUnsafeUpdates = false;

            // When the item created, unique value of ID is given.
            // Here we set _CreatedItemId to use later
            _CreatedItemId = item.ID.ToString();
            
            _set.Clear();
        }


        private string FieldRef(string Name, string Type, string Value, string Operator)
        {

            if (Operator == "IsNull")
            {
                return "<IsNull><FieldRef Name='" + Name + "' /></IsNull>";
            }
            else if (Operator == "IsNotNull")
            {
                return "<IsNotNull><FieldRef Name='" + Name + "' /></IsNotNull>";
            }
            {
                string s ="";
                if (Type == "Lookup") { s = " LookupId='TRUE'"; }
                return Add_Operator(Operator, "<FieldRef Name='" + Name + "'"+s+" /><Value Type='" + Type + "'>" + Value + "</Value>");
            }

        }

        private string Add_Operator(string Operator, string FieldRef)
        {
            string result = null;

            switch (Operator)
            {

                case "=":
                    result = "<Eq>" + FieldRef + "</Eq>";
                    break;
                case "!=":
                    result = "<Neq>" + FieldRef + "</Neq>";
                    break;
                case ">":
                    result = "<Gt>" + FieldRef + "</Gt>";
                    break;
                case ">=":
                    result = "<Geq>" + FieldRef + "</Geq>";
                    break;
                case "<":
                    result = "<Lt>" + FieldRef + "</Lt>";
                    break;
                case "<=":
                    result = "<Leq>" + FieldRef + "</Leq>";
                    break;
                case "BeginsWith":
                    result = "<BeginsWith>" + FieldRef + "</BeginsWith>";
                    break;
                case "Contains":
                    result = "<Contains>" + FieldRef + "</Contains>";
                    break;
            }

            return result;
        }

        private string Field_Type(string field)
        {
            _InsertDebug("Field Name:" + field);
            string t = "";
            try
            {
                // Get the list object 
                SPList list = getListAsObject(_List_Name);

                t = list.Fields.GetFieldByInternalName(field).Type.ToString();
                _InsertDebug("Field Type:" + t.ToString());
            }
            catch(Exception ex)
            {
                _InsertDebug("Field Type ex:" + ex.Message.ToString());
            }
            return t;
        }

        private string join(string ParentListName, string ChildListLookupColumnName)
        {
            return "<Join Type='LEFT' ListAlias='" + ParentListName + "'>" +
                            "<Eq>" +
                                "<FieldRef Name='" + ChildListLookupColumnName + "' RefType='Id'/>" +
                                "<FieldRef List='" + ParentListName + "' Name='ID'/>" +
                            "</Eq>" +
                         "</Join>";
        }

        


    }

    public class cawl_Functions
    {
        //to avoid null point exception it can be used.
        //When there is no value in field you get null pointer exception.
        public string ChecklistItem(SPListItem listItem, string columnName)
        {

            if ((listItem.Fields.ContainsField(columnName)) &&
                (listItem[columnName] != null))
            {
                return listItem[columnName].ToString();
            }
            else
            {
                return "-";
            }
        }

        //it will automatically convert 12#XXXXXx value to object 
        public string GivelookUpValue(SPListItem listItem, string columnName,Boolean Id=false)
        {

            if ((listItem.Fields.ContainsField(columnName)) &&
                (listItem[columnName] != null))
            {
                SPFieldLookupValue s = new SPFieldLookupValue(listItem[columnName].ToString());

                if (Id == false)
                {
                    return s.LookupValue.ToString();
                }
                else
                {
                    return s.LookupId.ToString();
                }

            }
            else
            {
               return "-";
            }
        }

        // to check whether item is null or not
        public string Check4Null(SPListItem listItem, string columnName)
        {

            if ((listItem.Fields.ContainsField(columnName)) &&
                (listItem[columnName] != null))
            {
                return listItem[columnName].ToString();
            }
            else
            {
                return "-";
            }
        }
        
        public SPUser makeFiledUserObject(SPListItem item, string fieldName)
        {
            
                string fieldValue = item[fieldName] as string;
                if (string.IsNullOrEmpty(fieldValue)) return null;
                int id = int.Parse(fieldValue.Split(';')[0]);
                SPUser user = item.Web.AllUsers.GetByID(id);
                return user;
            
            
        }

        // to check whether current user is member of a sharepoint group
        public bool IsMemberOf(string groupName)
        {
            SPUser user = SPContext.Current.Web.CurrentUser;

            try
            {
                if (user.Groups[groupName] != null)
                    return true;
                else
                    return false;
            }
            catch
            {
                return false;
            }
        }

    }


}
