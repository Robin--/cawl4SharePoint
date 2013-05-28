cawl4SharePoint
===============

http://www.cawl4sharepoint.com/

cawl for SharePoint is a powerful library with a very small footprint, built for Sharepoint coders who need a simple and elegant toolkit to create full-featured web applications.  Its goal is to enable you to develop projects much faster than you could if you were writing code from scratch, by providing a rich set of functions for commonly needed tasks, such as writing caml queries. cawl lets you creatively focus on your project by minimizing the amount of code needed for a given task.


cawl4SharePoint
===============

cawl for SharePoint is a powerful library with a very small footprint, built for Sharepoint coders who need a simple and elegant toolkit to create full-featured web applications.  Its goal is to enable you to develop projects much faster than you could if you were writing code from scratch, by providing a rich set of functions for commonly needed tasks, such as writing caml queries. cawl lets you creatively focus on your project by minimizing the amount of code needed for a given task.


// create an object cawl
cawl_QueryBuilder cawl = new cawl_QueryBuilder();

//Write some conditions
cawl.Where("Name","=","Murat");
cawl.or_Where("Name","=","Joe");
// condition is defined here

//use get function to build query.
//It is important to specify list name so that don't worry about field types.
cawl.Get("Users");

//Until here we build a caml query with condition for User list.

//now use QueryString function to get caml query string.
string finalquery= cawl.QueryString();

//then do whatever you want with query string



These are the cawl_StringBuilder class functions:

    Where()
    or_Where()
    Get()
    QueryString()
    Query()
    Order_by()
    Recursive()
    RowLimit()
    Site()
    ListItemCollection()
    Delete()
    Set()
    Insert()
    Update()
    Select() New!
    Join() New!

Usage
cawl_QueryBuilder cawl = new cawl_QueryBuilder();


Where();
This function enables you to set WHERE clauses.
  
//cawl.Where("Field","Operator","Value");
// WHERE Name = 'Murat' AND status = 'Active'
cawl.Where('Name',"=", "Murat");
cawl.Where('Status', "=","Active");





or_Where();
This function is identical to the one above, except that multiple instances are joined by OR:
	
// WHERE name != 'Murat' OR id > 30
cawl.Where("Name", "!=", "Murat");
cawl.or_Where("Age",">", "30");




Operators
Here are the operators that can be used in Where or Or_Where function

"="  //Equal
"!="  //Not Equal
">" //Greater Then
">=" //Greater Then or Equal
"<" //Less Then
"<=" //Less Then or Equal
"IsNull"  //Is Null
"IsNotNull"  //Is Not Null
"Contains" //Contains
"Begins" //Begins With


Get();
Create the selection query. Use List name that want to be use for query as parameter. So that cawl find out your filed type and use to build caml query.
	
//Build caml query : Select all users where name ="Murat"
cawl.Where("Name", "=", "Murat");
cawl.Get("Users");



QueryString();
This function is give you the caml query string as a string.

	
cawl.Where("Name", "!=", "Murat");
cawl.Get("Users");
 
strign camlquery= cawl.QueryString();


Query();
This function is identical to the one above, except that it return query as SPQuery object so that it canbe use easily.
	
cawl.Where("Name", "!=", "Murat");
cawl.Get("Users");
SPQuery camlquery= cawl.Query();


Order_by();
Lets you set an ORDER BY clause. The first parameter contains the name of the column you would like to order by. The second parameter lets you set the direction of the result. Options are ASC or DESC

cawl.Order_by("Title", "ASC");
cawl.Order_by("Age", "DESC");

Recursive();
If you add this function cawl, query all file and folders all folders deep.
	
cawl.Recursive();
// Produces:


RowLimit();
Lets you limit the number of rows you would like returned by the query:
	
cawl.RowLimit(10);


Site();
This function set the current site (in sharepoint term: current web). If it is not used, cawl use current web.
In case cawl is wanted to use with timer jobs projects or event handler projects then it may be set by this function.
	
cawl.Site("http:\\www.sp2010test.com\TestSite");


ListItemCollection();
This function run the builded caml query and return results as a SPListItemCollection object.
	
cawl.Where("Name", "!=", "Murat");
cawl.Get("Users");
SPListItemCollection Result = cawl.ListItemCollection();
foreach (SPListItem items in Result )
{
stringbuilder.Append(items["Name"].To_String());
}


Delete();
This function generates a delete query string and runs the query.
	
cawl.Where("Name", "=", "Murat");
cawl.Delete("Users");
//Delete all the items that have name "Murat"


Set();
This function enables you to set values for inserts or updates.
	
cawl.Set("Name", "Murat");
cawl.Set("Age", "30");
cawl.Set("Status", "Active");
cawl.Insert('Users');


Insert();
Generates an insert string based on the data you supply, and runs the query.
	
cawl.Set("Name", "Murat");
cawl.Set("Age", "30");
cawl.Set("Status", "Active");
cawl.Insert('Users');


Update();
Generates an update string and runs the query based on the data you supply.
	
cawl.Set("Status", "Passive");
cawl.Where("JoinDate";"<";"12.12.2010");
cawl.Update('Users');


Select();
Permits you to write the view fields portion of your query:
Note: If you are selecting all columns from a list you do not need to use this function. When omitted, cawl assumes you wish to SELECT all list columns
	
cawl.Select("Title");
cawl.Select("Status");
cawl.Get('Users');


Join();
Permits you to write the JOIN portion of your query. Multiple function calls can be made if you need several joins in one query.
First parameter is the list name that you want to join, second parameter is the look up column name in list that you run the query.

In the following example, there are two list, one is UserCars list which contains the car list with Owner column other one is Users list that contains users information.
UserCars list

    CarName (Title Column)
    Owner (LookUp column)
    Model (Text)

Users

    UserName (Title Column)
    Age (Number)
    Sex (Text)

Note:If you want to use join you must specify fields by Select() function otherwise query wont work. For the fileds that come from the list which you want to join must be specify like this: “JoinedList_Fieldname”
Note2:The column that come from joined list will be lookup value so you may need to convert to an SPFieldLookupValue

	
cawl.Select("Users_Title");
cawl.Select("Users_Age");
cawl.Select("Users_Sex");
cawl.Select("CarBrand");
cawl.Join("Users";"Owner");
cawl.Get('UserCars');
 
StringBuilder Result = new StringBuilder();
 
foreach (SPListItem item in cawl.ListItemCollection())
  {
   Result.Append(item["Users_Title"].ToString() +
                 item["Users_Age"].ToString() +
                 item["Users_Sex"].ToString() +
                 item["CarBrand"].ToString());
  }
 Label1.Text = Result .ToString();
