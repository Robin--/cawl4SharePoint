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
    * cawl4sharepoint: http://www.cawl4sharepoint.com/
    *
    * Copyright (c) 2012 Zenithsoft.co
    * Author: Murat Akdeniz 
    * www.makdeniz.com
    * Version : 2.0
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

    public class cawl_Calendar
    {
        
        string start_day = "Monday";
        string month_type = "long";
        string day_type = "short";
        string show_next_prev = "false";
        string next_prev_url  = null;
        ArrayList _data = new ArrayList();


        //month names
        string _short_January = "Jan";
        string _short_February = "Feb";
        string _short_March = "Mar";
        string _short_April = "Apr";
        string _short_May = "May";
        string _short_June = "Jun";
        string _short_July = "Jul";
        string _short_August = "Aug";
        string _short_September = "Sep";
        string _short_October = "Oct";
        string _short_November = "Nov";
        string _short_December = "Dec";

        string _long_January = "January";
        string _long_February = "February";
        string _long_March = "March";
        string _long_April = "April";
        string _long_May = "May";
        string _long_June = "June";
        string _long_July = "July";
        string _long_August = "August";
        string _long_September = "September";
        string _long_October = "October";
        string _long_November = "November";
        string _long_December = "December";

        //days name

        string _long_Sunday = "Sunday";
        string _long_Monday = "Monday";
        string _long_Tuesday = "Tuesday";
        string _long_Wednesday = "Wednesday";
        string _long_Thursday = "Thursday";
        string _long_Friday = "Friday";
        string _long_Saturday = "Saturday";

        string _short_Sunday = "Sun";
        string _short_Monday = "Mon";
        string _short_Tuesday = "Tue";
        string _short_Wednesday = "Wed";
        string _short_Thursday = "Thu";
        string _short_Friday = "Fri";
        string _short_Saturday = "Sat";




        //template
        string _Template_table_open = "<table border=\"0\" cellpadding=\"4\" cellspacing=\"0\">";
        string _Template_heading_row_start = "<tr>";
        string _Template_heading_previous_cell = "<th><a href=\"{previous_url}\">&lt;&lt;</a></th>";
        string _Template_heading_title_cell = "<th colspan=\"{colspan}\">{heading}</th>";
        string _Template_heading_next_cell = "<th><a href=\"{next_url}\">&gt;&gt;</a></th>";
        string _Template_heading_row_end = "</tr>";
        string _Template_week_row_start = "<tr>";
        string _Template_week_day_cell = "<td>{week_day}</td>";
        string _Template_week_row_end = "</tr>";
        string _Template_cal_row_start = "<tr>";
        string _Template_cal_cell_start = "<td>";
        string _Template_cal_cell_start_today = "<td>";
        string _Template_cal_cell_content = "<a href=\"{content}\">{day}</a>";
        string _Template_cal_cell_content_today = "<a href=\"{content}\"><strong>{day}</strong></a>";
        string _Template_cal_cell_no_content = "{day}";
        string _Template_cal_cell_no_content_today = "<strong>{day}</strong>";
        string _Template_cal_cell_blank = "&nbsp;";
        string _Template_cal_cell_end = "</td>";
        string _Template_cal_cell_end_today = "</td>";
        string _Template_cal_row_end = "</tr>";
        string _Template_table_close = "</table>";



        public string Generate(string year = "", string month = "")
        {
            // Set and validate the supplied month/year
            if (year == "") { year = DateTime.Now.Year.ToString(); }

            if (month == "") { month = DateTime.Now.Month.ToString(); }

            if (year.Length == 1) { year = "200" + year; }
            if (year.Length == 2) { year = "20" + year; }
            if (month.Length == 1) { month = "0" + month; }

            ArrayList adjusted_date = adjust_date(month, year);
            foreach (object item in adjusted_date)
            {
                string[] ItemArray = ((string[])item);
                month = ItemArray[0].ToString();
                year = ItemArray[1].ToString();


            }


            // Determine the total days in the month
            int total_days = get_total_days(month, year);

            // Set the starting day of the week
            int startday = start_days();


            // Set the starting day number
            DateTime date = new DateTime();
            date = DateTime.ParseExact(year + "-" + month + "-01", "yyyy-MM-dd", null);
            int day = startday + 1 - day_no_from_name(date.DayOfWeek.ToString());

            while (day > 1)
            {
                day -= 7;
            }

            // Set the current month/year/day
            // We use this to determine the "today" date
            string cur_year = DateTime.Now.Year.ToString();
            string cur_month = DateTime.Now.Month.ToString();
            string cur_day = DateTime.Now.Day.ToString();

            string is_current_month = (cur_year == year & cur_month == month) ? "TRUE" : "FALSE";

            // Begin building the calendar output
            StringBuilder output = new StringBuilder();

            output.Append(_Template_table_open);
            output.Append("\n");
            output.Append("\n");

            output.Append(_Template_heading_row_start);
            output.Append("\n");

            // "previous" month link
            if (show_next_prev != "false")
            {
                
                ArrayList adjusted_date_link = adjust_date((Convert.ToInt16(month)-1).ToString(), year);
                string month_link="";
                string year_link="";
                foreach (object item in adjusted_date_link)
                    {
                        string[] ItemArray = ((string[])item);
                        month_link = ItemArray[0].ToString();
                        year_link = ItemArray[1].ToString();


                    }
                
                output.Append(_Template_heading_previous_cell.Replace("{previous_url}",next_prev_url+"Year="+year_link+"&Month="+month_link));
                
                output.Append("\n");
            }

            // Heading containing the month/year
            int colspan = (show_next_prev != null) ? 5 : 7;

            _Template_heading_title_cell = _Template_heading_title_cell.Replace("{colspan}", colspan.ToString());
            _Template_heading_title_cell = _Template_heading_title_cell.Replace("{heading}", get_month_name(month) + "&nbsp;" + year);

            output.Append(_Template_heading_title_cell);
            output.Append("\n");

            // "next" month link
            
            if (show_next_prev != null)
            {

                ArrayList adjusted_date_link = adjust_date((Convert.ToInt16(month) + 1).ToString(), year);
                string month_link = "";
                string year_link = "";
                foreach (object item in adjusted_date_link)
                {
                    string[] ItemArray = ((string[])item);
                    month_link = ItemArray[0].ToString();
                    year_link = ItemArray[1].ToString();


                }


                output.Append(_Template_heading_next_cell.Replace("{next_url}", next_prev_url + "Year="+year_link + "&Month=" + month_link));
                output.Append("\n");
            }


            output.Append("\n");
            output.Append(_Template_heading_row_end);
            output.Append("\n");

            // Write the cells containing the days of the week
            output.Append("\n");
            output.Append(_Template_week_row_start);
            output.Append("\n");

            for (int i = 0; i < 7; i++)
            {
                output.Append(_Template_week_day_cell.Replace("{week_day}", get_day_names((startday + i) % 7)));
            }

            output.Append("\n");
            output.Append(_Template_week_row_end);
            output.Append("\n");

            // Build the main body of the calendar
            while (day <= total_days)
            {
                output.Append("\n");
                output.Append(_Template_cal_row_start);
                output.Append("\n");

                for (int i = 0; i < 7; i++)
                {
                    output.Append((is_current_month == "TRUE" & day.ToString() == cur_day) ? _Template_cal_cell_start_today : _Template_cal_cell_start);

                    if (day > 0 & day <= total_days)
                    {
                        if (_data.Count != 0)
                        {
                            // Cells with content
                            string temp = (is_current_month == "TRUE" & day.ToString() == cur_day) ? _Template_cal_cell_content_today : _Template_cal_cell_content;
                            
                            string content = "";
                            foreach (object item in _data)
                            {
                                string[] DataItemArray = ((string[])item);
                                if (DataItemArray[0] == day.ToString())
                                {
                                    content = content + DataItemArray[1];
                                }

                            }

                            output.Append(temp.Replace("{content}", content).Replace("{day}", day.ToString()));


                        }
                        else
                        {
                            // Cells with no content
                            string temp = (is_current_month == "TRUE" & day.ToString() == cur_day) ? _Template_cal_cell_no_content_today : _Template_cal_cell_no_content;
                            output.Append(temp.Replace("{day}", day.ToString()));
                        }
                    }
                    else
                    {
                        // Blank cells
                        output.Append(_Template_cal_cell_blank);
                    }

                    output.Append((is_current_month == "TRUE" & day.ToString() == cur_day) ? _Template_cal_cell_end_today : _Template_cal_cell_end);
                    day++;
                }

                output.Append("\n");
                output.Append(_Template_cal_row_end);
                output.Append("\n");
            }

            output.Append("\n");
            output.Append(_Template_table_close);

            return output.ToString();

        }
        
        private int start_days()
        {
            if (start_day == null)
            {
                return 0;
            }
            else
            {
                if (start_day == "Sunday") { return 0; }
                else if (start_day == "Monday") { return 1; }
                else if (start_day == "Tuesday") { return 2; }
                else if (start_day == "Wednesday") { return 3; }
                else if (start_day == "Thursday") { return 4; }
                else if (start_day == "Friday") { return 5; }
                else if (start_day == "Saturday") { return 6; }
                else { return 0; };

            }


        }
        
        public void AddEvent(int DayNumber, string Content)
        {
            _data.Add(new string[] { DayNumber.ToString(), Content });
        }

        public void SetLongDayNameTemplate(string Sunday, string Monday, string Tuesday, string Wednesday, string Thursday, string Friday, string Saturday)
        {
            _long_Sunday = Sunday;
            _long_Monday = Monday;
            _long_Tuesday = Tuesday;
            _long_Wednesday = Wednesday;
            _long_Thursday = Thursday;
            _long_Friday = Friday;
            _long_Saturday = Saturday;

        }

        public void SetShortDayNameTemplate(string Sunday, string Monday, string Tuesday, string Wednesday, string Thursday, string Friday, string Saturday)
        {
            _short_Sunday = Sunday;
            _short_Monday = Monday;
            _short_Tuesday = Tuesday;
            _short_Wednesday = Wednesday;
            _short_Thursday = Thursday;
            _short_Friday = Friday;
            _short_Saturday = Saturday;

        }

        public void SetLongMonthNameTemplate(string January, string February, string March, string April, string May, string June, string July, string August, string September, string October, string November, string December)
        {
            _long_January = January;
            _long_February = February;
            _long_March = March;
            _long_April = April;
            _long_May = May;
            _long_June = June;
            _long_July = July;
            _long_August = August;
            _long_September = September;
            _long_October = October;
            _long_November = November;
            _long_December = December;

        }

        public void SetShortMonthNameTemplate(string January, string February, string March, string April, string May, string June, string July, string August, string September, string October, string November, string December)
        {
            _short_January = January;
            _short_February = February;
            _short_March = March;
            _short_April = April;
            _short_May = May;
            _short_June = June;
            _short_July = July;
            _short_August = August;
            _short_September = September;
            _short_October = October;
            _short_November = November;
            _short_December = December;

        }

        public void SetDayNameType(string DayNameType)
        {
            if (DayNameType == "short")
            {
                day_type = "short";
            }
            else
            {
                day_type = "long";
            }
        }

        public void SetMonthNameType(string MonthNameType)
        {
            if (MonthNameType == "short")
            {
                month_type = "short";
            }
            else
            {
                month_type = "long";
            }
        }

        public void SetStartDay(string StartDay)
        {
            if (StartDay == "Saturday") { start_day = StartDay; }
            else if (StartDay == "Monday") { start_day = StartDay; }
            else if (StartDay == "Tuesday") { start_day = StartDay; }
            else if (StartDay == "Wednesday") { start_day = StartDay; }
            else if (StartDay == "Thursday") { start_day = StartDay; }
            else if (StartDay == "Friday") { start_day = StartDay; }
            else { start_day = StartDay; }

        }

        public void SetPrevNextUrl(string url)
        {
             show_next_prev = "true";
             next_prev_url = url; 
        }
        
        private ArrayList adjust_date(string month, string year)
        {
            ArrayList date = new ArrayList();

            int new_month = Convert.ToInt16(month);
            int new_year = Convert.ToInt16(year);
            string r_month;
            string r_year;

            while (new_month > 12)
            {
                new_month -= 12;
                new_year++;
                r_month = new_month.ToString();
                r_year = new_year.ToString();
            }

            while (new_month <= 0)
            {
                new_month += 12;
                new_year--;

            }

            r_month = new_month.ToString();
            r_year = new_year.ToString();

            if (r_month.Length == 1)
            {
                r_month = '0' + r_month;
            }



            date.Add(new string[] { r_month, r_year });

            return date;
        }

        private int get_total_days(string month, string year)
        {
            int[] days_in_month = new int[12] { 31, 28, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31 };

            if (Convert.ToInt16(month) < 1 || Convert.ToInt16(month) > 12)
            {
                return 0;
            }

            // Is the year a leap year?
            if (Convert.ToInt16(month) == 2)
            {
                if (Convert.ToInt16(year) % 400 == 0 || (Convert.ToInt16(year) % 4 == 0 & Convert.ToInt16(year) % 100 != 0))
                {
                    return 29;
                }
            }

            return days_in_month[Convert.ToInt16(month) - 1];
        }

        private string get_month_name(string month)
        {
            string name = "";
            if (month_type == "short")
            {
                if (month == "01") { name = _short_January; }
                if (month == "02") { name = _short_February; }
                if (month == "03") { name = _short_March; }
                if (month == "04") { name = _short_April; }
                if (month == "05") { name = _short_May; }
                if (month == "06") { name = _short_June; }
                if (month == "07") { name = _short_July; }
                if (month == "08") { name = _short_August; }
                if (month == "09") { name = _short_September; }
                if (month == "10") { name = _short_October; }
                if (month == "11") { name = _short_November; }
                if (month == "12") { name = _long_December; }

            }
            else
            {

                if (month == "01") { name = _long_January; }
                if (month == "02") { name = _long_February; }
                if (month == "03") { name = _long_March; }
                if (month == "04") { name = _long_April; }
                if (month == "05") { name = _long_May; }
                if (month == "06") { name = _long_June; }
                if (month == "07") { name = _long_July; }
                if (month == "08") { name = _long_August; }
                if (month == "09") { name = _long_September; }
                if (month == "10") { name = _long_October; }
                if (month == "11") { name = _long_November; }
                if (month == "12") { name = _long_December; }

            }

            return name;
        }

        private string get_day_names(int daynumber)
        {
            
            string value = "";
            if (day_type == "long")
            {
                if (daynumber == 0) { value = _long_Sunday; }
                if (daynumber == 1) { value = _long_Monday; }
                if (daynumber == 2) { value = _long_Tuesday; }
                if (daynumber == 3) { value = _long_Wednesday; }
                if (daynumber == 4) { value = _long_Thursday; }
                if (daynumber == 5) { value = _long_Friday; }
                if (daynumber == 6) { value = _long_Saturday; }

            }
            else if (day_type == "short")
            {
                if (daynumber == 0) { value = _short_Sunday; }
                if (daynumber == 1) { value = _short_Monday; }
                if (daynumber == 2) { value = _short_Tuesday; }
                if (daynumber == 3) { value = _short_Wednesday; }
                if (daynumber == 4) { value = _short_Thursday; }
                if (daynumber == 5) { value = _short_Friday; }
                if (daynumber == 6) { value = _short_Saturday; }

            }
            else
            {
                if (daynumber == 0) { value = "su"; }
                if (daynumber == 1) { value = "mo"; }
                if (daynumber == 2) { value = "tu"; }
                if (daynumber == 3) { value = "we"; }
                if (daynumber == 4) { value = "th"; }
                if (daynumber == 5) { value = "fr"; }
                if (daynumber == 6) { value = "sa"; }
            }

            return value;

        }

        private int day_no_from_name(string dayname)
        {
            int daynumber = 0;

            if (dayname == "Sunday") { daynumber = 0; }
            if (dayname == "Monday") { daynumber = 1; }
            if (dayname == "Tuesday") { daynumber = 2; }
            if (dayname == "Wednesday") { daynumber = 3; }
            if (dayname == "Thursday") { daynumber = 4; }
            if (dayname == "Friday") { daynumber = 5; }
            if (dayname == "Saturday") { daynumber = 6; }

            return daynumber;
        }

        #region template

        public void Template_table_open(string template) { _Template_table_open = template; }
        public void Template_heading_row_start(string template) { _Template_heading_row_start = template; }
        public void Template_heading_previous_cell(string template) { _Template_heading_previous_cell = template; }
        public void Template_heading_title_cell(string template) { _Template_heading_title_cell = template; }
        public void Template_heading_next_cell(string template) { _Template_heading_next_cell = template; }
        public void Template_heading_row_end(string template) { _Template_heading_row_end = template; }
        public void Template_week_row_start(string template) { _Template_week_row_start = template; }
        public void Template_week_day_cell(string template) { _Template_week_day_cell = template; }
        public void Template_week_row_end(string template) { _Template_week_row_end = template; }
        public void Template_cal_row_start(string template) { _Template_cal_row_start = template; }
        public void Template_cal_cell_start(string template) { _Template_cal_cell_start = template; }
        public void Template_cal_cell_start_today(string template) { _Template_cal_cell_start_today = template; }
        public void Template_cal_cell_content(string template) { _Template_cal_cell_content = template; }
        public void Template_cal_cell_content_today(string template) { _Template_cal_cell_content_today = template; }
        public void Template_cal_cell_no_content(string template) { _Template_cal_cell_no_content = template; }
        public void Template_cal_cell_no_content_today(string template) { _Template_cal_cell_no_content_today = template; }
        public void Template_cal_cell_blank(string template) { _Template_cal_cell_blank = template; }
        public void Template_cal_cell_end(string template) { _Template_cal_cell_end = template; }
        public void Template_cal_cell_end_today(string template) { _Template_cal_cell_end_today = template; }
        public void Template_cal_row_end(string template) { _Template_cal_row_end = template; }
        public void Template_table_close(string template) { _Template_table_close = template; }


        #endregion template
    }

    public class cawl_Functions
    {

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

    }


}
