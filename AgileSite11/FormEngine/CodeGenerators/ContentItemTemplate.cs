﻿// ------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version: 15.0.0.0
//  
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
// ------------------------------------------------------------------------------
namespace CMS.FormEngine
{
    using System;
    
    /// <summary>
    /// Class to produce the template output
    /// </summary>
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.TextTemplating", "15.0.0.0")]
    internal partial class ContentItemTemplate : ContentItemTemplateBase
    {
        /// <summary>
        /// Create the template output
        /// </summary>
        public virtual string TransformText()
        {
            this.Write(@"//--------------------------------------------------------------------------------------------------
// <auto-generated>
//
//     This code was generated by code generator tool.
//
//     To customize the code use your own partial class. For more info about how to use and customize
//     the generated code see the documentation at http://docs.kentico.com.
//
// </auto-generated>
//--------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;

using CMS;
using CMS.Base;
using CMS.Helpers;
using CMS.DataEngine;
using ");
            this.Write(this.ToStringHelper.ToStringWithCulture(Namespace));
            this.Write(";\r\n");

foreach (String us in mClassUsings)
{

            this.Write("using ");
            this.Write(this.ToStringHelper.ToStringWithCulture(us));
            this.Write(";\r\n");

}

            this.Write("\r\n[assembly: ");
            this.Write(this.ToStringHelper.ToStringWithCulture(mAssemblyRegisterName));
            this.Write("(");
            this.Write(this.ToStringHelper.ToStringWithCulture(ItemTypeName));
            this.Write(".CLASS_NAME, typeof(");
            this.Write(this.ToStringHelper.ToStringWithCulture(ItemTypeName));
            this.Write("))]\r\n\r\nnamespace ");
            this.Write(this.ToStringHelper.ToStringWithCulture(Namespace));
            this.Write("\r\n{\r\n\t/// <summary>\r\n\t/// Represents a content item of type ");
            this.Write(this.ToStringHelper.ToStringWithCulture(ItemTypeName));
            this.Write(".\r\n\t/// </summary>\r\n\tpublic partial class ");
            this.Write(this.ToStringHelper.ToStringWithCulture(ItemTypeName));
            this.Write(" : ");
            this.Write(this.ToStringHelper.ToStringWithCulture(mBaseClassType.Name));
            this.Write("\r\n\t{\r\n\t\t#region \"Constants and variables\"\r\n\r\n\t\t/// <summary>\r\n\t\t/// The name of t" +
                    "he data class.\r\n\t\t/// </summary>\r\n\t\tpublic const string CLASS_NAME = \"");
            this.Write(this.ToStringHelper.ToStringWithCulture(mItemClassName));
            this.Write("\";\r\n\r\n\r\n\t\t/// <summary>\r\n\t\t/// The instance of the class that provides extended A" +
                    "PI for working with ");
            this.Write(this.ToStringHelper.ToStringWithCulture(ItemTypeName));
            this.Write(" fields.\r\n\t\t/// </summary>\r\n\t\tprivate readonly ");
            this.Write(this.ToStringHelper.ToStringWithCulture(ItemTypeName));
            this.Write("Fields mFields;\r\n");
 if (IsProduct) 
{ 

            this.Write("\r\n\r\n\t\t/// <summary>\r\n\t\t/// The instance of the class that provides extended API f" +
                    "or working with SKU fields.\r\n\t\t/// </summary>\r\n\t\tprivate readonly ProductFields " +
                    "mProduct;\r\n");
 
} 

            this.Write("\r\n\t\t#endregion\r\n\r\n\r\n\t\t#region \"Properties\"\r\n");

foreach (var field in Fields)
{
	if (field.DataType == CMS.DataEngine.FieldDataType.DocRelationships)
	{
		continue;
	}

            this.Write("\r\n\t\t/// <summary>\r\n\t\t/// ");
            this.Write(this.ToStringHelper.ToStringWithCulture(GetSummary(field, 2)));
            this.Write("\r\n\t\t/// </summary>\r\n");
 if (!field.IsDummyField) { 
            this.Write("\t\t");
            this.Write(this.ToStringHelper.ToStringWithCulture((field.PrimaryKey ? "[DatabaseIDField]" : "[DatabaseField]")));
            this.Write("\r\n");
 } 
            this.Write("\t\tpublic ");
            this.Write(this.ToStringHelper.ToStringWithCulture(GetDataType(field)));
            this.Write(" ");
            this.Write(this.ToStringHelper.ToStringWithCulture(GetPropertyName(field)));
            this.Write("\r\n\t\t{\r\n\t\t\tget\r\n\t\t\t{\r\n\t\t\t\treturn ValidationHelper.");
            this.Write(this.ToStringHelper.ToStringWithCulture(GetValidationHelperMethodName(field.DataType)));
            this.Write("(GetValue(\"");
            this.Write(this.ToStringHelper.ToStringWithCulture(field.Name));
            this.Write("\"), ");
            this.Write(this.ToStringHelper.ToStringWithCulture(GetDefaultValue(field)));
            this.Write(");\r\n\t\t\t}\r\n\t\t\tset\r\n\t\t\t{\r\n\t\t\t\tSetValue(\"");
            this.Write(this.ToStringHelper.ToStringWithCulture(field.Name));
            this.Write("\", value);\r\n\t\t\t}\r\n\t\t}\r\n\r\n");

}

            this.Write("\r\n\t\t/// <summary>\r\n\t\t/// Gets an object that provides extended API for working wi" +
                    "th ");
            this.Write(this.ToStringHelper.ToStringWithCulture(ItemTypeName));
            this.Write(" fields.\r\n\t\t/// </summary>\r\n\t\t[RegisterProperty]\r\n\t\tpublic ");
            this.Write(this.ToStringHelper.ToStringWithCulture(ItemTypeName));
            this.Write("Fields Fields\r\n\t\t{\r\n\t\t\tget\r\n\t\t\t{\r\n\t\t\t\treturn mFields;\r\n\t\t\t}\r\n\t\t}\r\n");
 
if (IsProduct) 
{

            this.Write("\r\n\r\n\t\t/// <summary>\r\n\t\t/// Gets an object that provides extended API for working " +
                    "with SKU fields.\r\n\t\t/// </summary>\r\n        [RegisterProperty]\r\n\t\tpublic Product" +
                    "Fields Product\r\n\t\t{\r\n\t\t\tget\r\n\t\t\t{\r\n\t\t\t\treturn mProduct;\r\n\t\t\t}\r\n\t\t}\r\n");

}

            this.Write("\r\n\r\n\t\t/// <summary>\r\n\t\t/// Provides extended API for working with ");
            this.Write(this.ToStringHelper.ToStringWithCulture(ItemTypeName));
            this.Write(" fields.\r\n\t\t/// </summary>\r\n\t\t[RegisterAllProperties]\r\n\t\tpublic partial class ");
            this.Write(this.ToStringHelper.ToStringWithCulture(ItemTypeName));
            this.Write("Fields : AbstractHierarchicalObject<");
            this.Write(this.ToStringHelper.ToStringWithCulture(ItemTypeName));
            this.Write("Fields>\r\n\t\t{\r\n\t\t\t/// <summary>\r\n\t\t\t/// The content item of type ");
            this.Write(this.ToStringHelper.ToStringWithCulture(ItemTypeName));
            this.Write(" that is a target of the extended API.\r\n\t\t\t/// </summary>\r\n\t\t\tprivate readonly ");
            this.Write(this.ToStringHelper.ToStringWithCulture(ItemTypeName));
            this.Write(" mInstance;\r\n\r\n\r\n\t\t\t/// <summary>\r\n\t\t\t/// Initializes a new instance of the <see " +
                    "cref=\"");
            this.Write(this.ToStringHelper.ToStringWithCulture(ItemTypeName));
            this.Write("Fields\" /> class with the specified content item of type ");
            this.Write(this.ToStringHelper.ToStringWithCulture(ItemTypeName));
            this.Write(".\r\n\t\t\t/// </summary>\r\n\t\t\t/// <param name=\"instance\">The content item of type ");
            this.Write(this.ToStringHelper.ToStringWithCulture(ItemTypeName));
            this.Write(" that is a target of the extended API.</param>\r\n\t\t\tpublic ");
            this.Write(this.ToStringHelper.ToStringWithCulture(ItemTypeName));
            this.Write("Fields(");
            this.Write(this.ToStringHelper.ToStringWithCulture(ItemTypeName));
            this.Write(" instance)\r\n\t\t\t{\r\n\t\t\t\tmInstance = instance;\r\n\t\t\t}\r\n");

foreach (var field in Fields)
{
	if (field.DataType == CMS.DataEngine.FieldDataType.File)
	{

            this.Write("\r\n\r\n\t\t\t/// <summary>\r\n\t\t\t/// ");
            this.Write(this.ToStringHelper.ToStringWithCulture(GetSummary(field)));
            this.Write("\r\n\t\t\t/// </summary>\r\n\t\t\tpublic Attachment ");
            this.Write(this.ToStringHelper.ToStringWithCulture(GetNestedPropertyName(field)));
            this.Write("\r\n\t\t\t{\r\n\t\t\t\tget\r\n\t\t\t\t{\r\n\t\t\t\t\treturn mInstance.GetFieldAttachment(\"");
            this.Write(this.ToStringHelper.ToStringWithCulture(field.Name));
            this.Write("\");\r\n\t\t\t\t}\r\n\t\t\t}\r\n");

	}
	else if (field.DataType == CMS.DataEngine.FieldDataType.DocAttachments)
	{

            this.Write("\r\n\r\n\t\t\t/// <summary>\r\n\t\t\t/// ");
            this.Write(this.ToStringHelper.ToStringWithCulture(GetSummary(field)));
            this.Write("\r\n\t\t\t/// </summary>\r\n\t\t\tpublic IEnumerable<Attachment> ");
            this.Write(this.ToStringHelper.ToStringWithCulture(GetNestedPropertyName(field)));
            this.Write("\r\n\t\t\t{\r\n\t\t\t\tget\r\n\t\t\t\t{\r\n\t\t\t\t\treturn mInstance.GetFieldAttachments(\"");
            this.Write(this.ToStringHelper.ToStringWithCulture(field.Name));
            this.Write("\");\r\n\t\t\t\t}\r\n\t\t\t}\r\n");

	}
	else if (field.DataType == CMS.DataEngine.FieldDataType.DocRelationships)
	{

            this.Write("\r\n\r\n\t\t\t/// <summary>\r\n\t\t\t/// ");
            this.Write(this.ToStringHelper.ToStringWithCulture(GetSummary(field)));
            this.Write("\r\n\t\t\t/// </summary>\r\n\t\t\tpublic IEnumerable<TreeNode> ");
            this.Write(this.ToStringHelper.ToStringWithCulture(GetNestedPropertyName(field)));
            this.Write("\r\n\t\t\t{\r\n\t\t\t\tget\r\n\t\t\t\t{\r\n\t\t\t\t\treturn mInstance.GetRelatedDocuments(\"");
            this.Write(this.ToStringHelper.ToStringWithCulture(field.Name));
            this.Write("\");\r\n\t\t\t\t}\r\n\t\t\t}\r\n");

	}
	else
	{

            this.Write("\r\n\r\n\t\t\t/// <summary>\r\n\t\t\t/// ");
            this.Write(this.ToStringHelper.ToStringWithCulture(GetSummary(field)));
            this.Write("\r\n\t\t\t/// </summary>\r\n\t\t\tpublic ");
            this.Write(this.ToStringHelper.ToStringWithCulture(GetDataType(field)));
            this.Write(" ");
            this.Write(this.ToStringHelper.ToStringWithCulture(GetNestedPropertyName(field)));
            this.Write("\r\n\t\t\t{\r\n\t\t\t\tget\r\n\t\t\t\t{\r\n\t\t\t\t\treturn mInstance.");
            this.Write(this.ToStringHelper.ToStringWithCulture(GetPropertyName(field)));
            this.Write(";\r\n\t\t\t\t}\r\n\t\t\t\tset\r\n\t\t\t\t{\r\n\t\t\t\t\tmInstance.");
            this.Write(this.ToStringHelper.ToStringWithCulture(GetPropertyName(field)));
            this.Write(" = value;\r\n\t\t\t\t}\r\n\t\t\t}\r\n");

	}
}

            this.Write("\t\t}\r\n");

if (IsProduct)
{

            this.Write(@"

		/// <summary>
		/// Provides extended API for working with SKU fields.
		/// </summary>
        [RegisterAllProperties]
		public class ProductFields : AbstractHierarchicalObject<ProductFields>
		{
		    /// <summary>
			/// The content item of type <see cref=""");
            this.Write(this.ToStringHelper.ToStringWithCulture(ItemTypeName));
            this.Write("\" /> that is a target of the extended API.\r\n\t\t\t/// </summary>\r\n\t\t\tprivate readonl" +
                    "y ");
            this.Write(this.ToStringHelper.ToStringWithCulture(ItemTypeName));
            this.Write(" mInstance;\r\n\r\n\r\n\t\t\t/// <summary>\r\n\t\t\t/// The <see cref=\"PublicStatusInfo\" /> obj" +
                    "ect related to product based on value of <see cref=\"SKUInfo.SKUPublicStatusID\" /" +
                    "> column. \r\n\t\t\t/// </summary>\r\n\t\t\tprivate PublicStatusInfo mPublicStatus = null;" +
                    "\r\n\r\n\r\n\t\t\t/// <summary>\r\n\t\t\t/// The <see cref=\"ManufacturerInfo\" /> object relate" +
                    "d to product based on value of <see cref=\"SKUInfo.SKUManufacturerID\" /> column. " +
                    "\r\n\t\t\t/// </summary>\r\n\t\t\tprivate ManufacturerInfo mManufacturer = null;\r\n\r\n\r\n\t\t\t/" +
                    "// <summary>\r\n\t\t\t/// The <see cref=\"DepartmentInfo\" /> object related to product" +
                    " based on value of <see cref=\"SKUInfo.SKUDepartmentID\" /> column. \r\n\t\t\t/// </sum" +
                    "mary>\r\n\t\t\tprivate DepartmentInfo mDepartment = null;\r\n\r\n\r\n\t\t\t/// <summary>\r\n\t\t\t/" +
                    "// The <see cref=\"SupplierInfo\" /> object related to product based on value of <" +
                    "see cref=\"SKUInfo.SKUSupplierID\" /> column. \r\n\t\t\t/// </summary>\r\n\t\t\tprivate Supp" +
                    "lierInfo mSupplier = null;\r\n\r\n\r\n\t\t\t/// <summary>\r\n\t\t\t/// The <see cref=\"TaxClass" +
                    "Info\" /> object related to product based on value of <see cref=\"SKUInfo.SKUTaxCl" +
                    "assID\" /> column. \r\n\t\t\t/// </summary>\r\n\t\t\tprivate TaxClassInfo mTaxClass = null;" +
                    "\r\n\r\n\r\n\t\t\t/// <summary>\r\n\t\t\t/// The <see cref=\"BrandInfo\" /> object related to pr" +
                    "oduct based on value of <see cref=\"SKUInfo.SKUBrandID\" /> column. \r\n\t\t\t/// </sum" +
                    "mary>\r\n\t\t\tprivate BrandInfo mBrand = null;\r\n\r\n\r\n\t\t\t/// <summary>\r\n\t\t\t/// The <se" +
                    "e cref=\"CollectionInfo\" /> object related to product based on value of <see cref" +
                    "=\"SKUInfo.SKUCollectionID\" /> column. \r\n\t\t\t/// </summary>\r\n\t\t\tprivate Collection" +
                    "Info mCollection = null;\r\n\r\n\r\n\t\t\t/// <summary>\r\n\t\t\t/// The shortcut to <see cref" +
                    "=\"SKUInfo\" /> object which is a target of this extended API.\r\n\t\t\t/// </summary>\r" +
                    "\n\t\t\tprivate SKUInfo SKU\r\n\t\t\t{\r\n\t\t\t\tget \r\n\t\t\t\t{\r\n\t\t\t\t\treturn mInstance.SKU;\r\n\t\t\t\t" +
                    "}\r\n\t\t\t}\r\n\r\n\t\t\t\t\t\t\r\n\t\t\t/// <summary>\r\n\t\t\t/// Initializes a new instance of the <s" +
                    "ee cref=\"ProductFields\" /> class with SKU related fields of type <see cref=\"");
            this.Write(this.ToStringHelper.ToStringWithCulture(ItemTypeName));
            this.Write("\" /> .\r\n\t\t\t/// </summary>\r\n\t\t\t/// <param name=\"instance\">The content item of type" +
                    " ");
            this.Write(this.ToStringHelper.ToStringWithCulture(ItemTypeName));
            this.Write(" that is a target of the extended API.</param>\r\n\t\t\tpublic ProductFields(");
            this.Write(this.ToStringHelper.ToStringWithCulture(ItemTypeName));
            this.Write(" instance)\r\n\t\t\t{\r\n\t\t\t\tmInstance = instance;\r\n\t\t\t}\r\n");
 
	foreach (var field in SystemSKUFields)
	{		

            this.Write("\r\n\r\n\t\t\t/// <summary>\r\n\t\t\t/// ");
            this.Write(this.ToStringHelper.ToStringWithCulture(GetSummary(field)));
            this.Write("\r\n\t\t\t/// </summary>\r\n\t\t\tpublic ");
            this.Write(this.ToStringHelper.ToStringWithCulture(GetDataType(field)));
            this.Write(" ");
            this.Write(this.ToStringHelper.ToStringWithCulture(GetSKUNestedPropertyName(field)));
            this.Write("\r\n\t\t\t{\r\n\t\t\t\tget\r\n\t\t\t\t{\r\n\t\t\t\t\treturn (SKU != null) ? SKU.");
            this.Write(this.ToStringHelper.ToStringWithCulture(GetSKUPropertyName(field)));
            this.Write(" : ");
            this.Write(this.ToStringHelper.ToStringWithCulture(GetDefaultValue(field)));
            this.Write(";\r\n\t\t\t\t}\r\n\t\t\t\tset\r\n\t\t\t\t{\r\n\t\t\t\t\tif (SKU != null)\r\n\t\t\t\t\t{\r\n\t\t\t\t\t\tSKU.");
            this.Write(this.ToStringHelper.ToStringWithCulture(GetSKUPropertyName(field)));
            this.Write(" = value;\r\n\t\t\t\t\t}\r\n\t\t\t\t}\r\n\t\t\t}\r\n");

	}

            this.Write("\r\n\r\n\t\t\t/// <summary>\r\n\t\t\t/// Gets <see cref=\"PublicStatusInfo\" /> object based on" +
                    " value of <see cref=\"SKUInfo.SKUPublicStatusID\" /> column. \r\n\t\t\t/// </summary>\r\n" +
                    "\t\t\tpublic PublicStatusInfo PublicStatus\r\n\t\t\t{\t\r\n\t\t\t\tget\r\n\t\t\t\t{\r\n\t\t\t\t\tif (SKU == " +
                    "null)\r\n\t\t\t\t\t{\r\n\t\t\t\t\t\treturn null;\r\n\t\t\t\t\t}\r\n\r\n\t\t\t\t\tvar id = SKU.SKUPublicStatusID" +
                    ";\r\n\r\n\t\t\t\t    if ((mPublicStatus == null) && (id > 0))\r\n\t\t\t\t    {\r\n              " +
                    "          mPublicStatus = PublicStatusInfoProvider.GetPublicStatusInfo(id);\r\n\t\t\t" +
                    "\t    }\r\n\r\n\t\t\t\t    return mPublicStatus;\r\n\t\t\t\t}\r\n\t\t\t}\r\n\r\n\r\n\t\t\t/// <summary>\r\n\t\t\t/" +
                    "// Gets <see cref=\"ManufacturerInfo\" /> object based on value of <see cref=\"SKUI" +
                    "nfo.SKUManufacturerID\" /> column. \r\n\t\t\t/// </summary>\r\n\t\t\tpublic ManufacturerInf" +
                    "o Manufacturer\r\n\t\t\t{\t\r\n\t\t\t\tget\r\n\t\t\t\t{\r\n\t\t\t\t\tif (SKU == null)\r\n\t\t\t\t\t{\r\n\t\t\t\t\t\tretu" +
                    "rn null;\r\n\t\t\t\t\t}\r\n\r\n\t\t\t\t\tvar id = SKU.SKUManufacturerID;\r\n\r\n\t\t\t\t    if ((mManufa" +
                    "cturer == null) && (id > 0))\r\n\t\t\t\t    {\r\n                        mManufacturer =" +
                    " ManufacturerInfoProvider.GetManufacturerInfo(id);\r\n\t\t\t\t    }\r\n\r\n\t\t\t\t    return " +
                    "mManufacturer;\r\n\t\t\t\t}\r\n\t\t\t}\r\n\r\n\r\n\t\t\t/// <summary>\r\n\t\t\t/// Gets <see cref=\"Depart" +
                    "mentInfo\" /> object based on value of <see cref=\"SKUInfo.SKUDepartmentID\" /> col" +
                    "umn. \r\n\t\t\t/// </summary>\r\n\t\t\tpublic DepartmentInfo Department\r\n\t\t\t{\t\r\n\t\t\t\tget\r\n\t" +
                    "\t\t\t{\r\n\t\t\t\t\tif (SKU == null)\r\n\t\t\t\t\t{\r\n\t\t\t\t\t\treturn null;\r\n\t\t\t\t\t}\r\n\r\n\t\t\t\t    var i" +
                    "d = SKU.SKUDepartmentID;\r\n\r\n\t\t\t\t    if ((mDepartment == null) && (id > 0))\r\n\t\t\t\t" +
                    "    {\r\n\t\t\t\t        mDepartment = DepartmentInfoProvider.GetDepartmentInfo(id);\r\n" +
                    "                    }\r\n\r\n\t\t\t\t\treturn mDepartment;\r\n\t\t\t\t}\r\n\t\t\t}\r\n\r\n\r\n\t\t\t/// <summ" +
                    "ary>\r\n\t\t\t/// Gets <see cref=\"SupplierInfo\" /> object based on value of <see cref" +
                    "=\"SKUInfo.SKUSupplierID\" /> column. \r\n\t\t\t/// </summary>\r\n\t\t\tpublic SupplierInfo " +
                    "Supplier\r\n\t\t\t{\t\r\n\t\t\t\tget\r\n\t\t\t\t{\r\n\t\t\t\t\tif (SKU == null)\r\n\t\t\t\t\t{\r\n\t\t\t\t\t\treturn nul" +
                    "l;\r\n\t\t\t\t\t}\r\n\r\n\t\t\t\t\tvar id = SKU.SKUSupplierID;\r\n\r\n\t\t\t\t    if ((mSupplier == null" +
                    ") && (id > 0))\r\n\t\t\t\t    {\r\n                        mSupplier = SupplierInfoProvi" +
                    "der.GetSupplierInfo(id);\r\n                    }\r\n\r\n\t\t\t\t    return mSupplier;\r\n\t\t" +
                    "\t\t}\r\n\t\t\t}\r\n\r\n\r\n\t\t\t/// <summary>\r\n\t\t\t/// Gets <see cref=\"TaxClassInfo\" /> object " +
                    "based on value of <see cref=\"SKUInfo.SKUTaxClassID\" /> column. \r\n\t\t\t/// </summar" +
                    "y>\r\n\t\t\tpublic TaxClassInfo TaxClass\r\n\t\t\t{\t\r\n\t\t\t\tget\r\n\t\t\t\t{\r\n\t\t\t\t\tif (SKU == null" +
                    ")\r\n\t\t\t\t\t{\r\n\t\t\t\t\t\treturn null;\r\n\t\t\t\t\t}\r\n\r\n\t\t\t\t\tvar id = SKU.SKUTaxClassID;\r\n\r\n\t\t\t" +
                    "\t    if ((mTaxClass == null) && (id > 0))\r\n\t\t\t\t    {\r\n\t\t\t\t\t\tmTaxClass = TaxClass" +
                    "InfoProvider.GetTaxClassInfo(id);\r\n\t\t\t\t    }\r\n\t\t\t\t    \r\n\t\t\t\t    return mTaxClass" +
                    ";\r\n\t\t\t\t}\r\n\t\t\t}\r\n\r\n\r\n\t\t\t/// <summary>\r\n\t\t\t/// Gets <see cref=\"BrandInfo\" /> objec" +
                    "t based on value of <see cref=\"SKUInfo.SKUBrandID\" /> column. \r\n\t\t\t/// </summary" +
                    ">\r\n\t\t\tpublic BrandInfo Brand\r\n\t\t\t{\t\r\n\t\t\t\tget\r\n\t\t\t\t{\r\n\t\t\t\t\tif (SKU == null)\r\n\t\t\t\t" +
                    "\t{\r\n\t\t\t\t\t\treturn null;\r\n\t\t\t\t\t}\r\n\r\n\t\t\t\t\tvar id = SKU.SKUBrandID;\r\n\r\n\t\t\t\t\tif ((mBr" +
                    "and == null) && (id > 0))\r\n\t\t\t\t\t{\r\n\t\t\t\t\t\tmBrand = BrandInfoProvider.GetBrandInfo" +
                    "(id);\r\n\t\t\t\t\t}\r\n\r\n\t\t\t\t\treturn mBrand;\r\n\t\t\t\t}\r\n\t\t\t}\r\n\r\n\r\n\t\t\t/// <summary>\r\n\t\t\t/// " +
                    "Gets <see cref=\"CollectionInfo\" /> object based on value of <see cref=\"SKUInfo.S" +
                    "KUCollectionID\" /> column. \r\n\t\t\t/// </summary>\r\n\t\t\tpublic CollectionInfo Collect" +
                    "ion\r\n\t\t\t{\t\r\n\t\t\t\tget\r\n\t\t\t\t{\r\n\t\t\t\t\tif (SKU == null)\r\n\t\t\t\t\t{\r\n\t\t\t\t\t\treturn null;\r\n\t" +
                    "\t\t\t\t}\r\n\r\n\t\t\t\t\tvar id = SKU.SKUCollectionID;\r\n\r\n\t\t\t\t\tif ((mCollection == null) &&" +
                    " (id > 0))\r\n\t\t\t\t\t{\r\n\t\t\t\t\t\tmCollection = CollectionInfoProvider.GetCollectionInfo" +
                    "(id);\r\n\t\t\t\t\t}\r\n\r\n\t\t\t\t\treturn mCollection;\r\n\t\t\t\t}\r\n\t\t\t}\r\n\r\n\r\n\t\t\t/// <summary>\r\n\t\t" +
                    "\t/// Localized name of product.\r\n\t\t\t/// </summary>\r\n\t\t\tpublic string Name\r\n\t\t\t{\r" +
                    "\n\t\t\t\tget\r\n\t\t\t\t{\r\n\t\t\t\t\treturn mInstance.DocumentSKUName;\r\n\t\t\t\t}\r\n\t\t\t\tset\r\n\t\t\t\t{\r\n" +
                    "\t\t\t\t\tmInstance.DocumentSKUName = value;\r\n\t\t\t\t}\r\n\t\t\t}\r\n\r\n\r\n\t\t\t/// <summary>\r\n\t\t\t/" +
                    "// Localized description of product.\r\n\t\t\t/// </summary>\r\n\t\t\tpublic string Descri" +
                    "ption\r\n\t\t\t{\r\n\t\t\t\tget\r\n\t\t\t\t{\r\n\t\t\t\t\treturn mInstance.DocumentSKUDescription;\r\n\t\t\t\t" +
                    "}\r\n\t\t\t\tset\r\n\t\t\t\t{\r\n\t\t\t\t\tmInstance.DocumentSKUDescription = value;\r\n\t\t\t\t}\r\n\t\t\t}\r\n" +
                    "\r\n\r\n\t\t\t/// <summary>\r\n\t\t\t/// Localized short description of product.\r\n\t\t\t/// </s" +
                    "ummary>\r\n\t\t\tpublic string ShortDescription\r\n\t\t\t{\r\n\t\t\t\tget\r\n\t\t\t\t{\r\n\t\t\t\t\treturn mI" +
                    "nstance.DocumentSKUShortDescription;\r\n\t\t\t\t}\r\n\t\t\t\tset\r\n\t\t\t\t{\r\n\t\t\t\t\tmInstance.Docu" +
                    "mentSKUShortDescription = value;\r\n\t\t\t\t}\r\n\t\t\t}\r\n");

	foreach (var field in CustomSKUFields)
	{

            this.Write("\r\n\r\n\t\t\t/// <summary>\r\n\t\t\t/// ");
            this.Write(this.ToStringHelper.ToStringWithCulture(GetSummary(field)));
            this.Write("\r\n\t\t\t/// </summary>\r\n\t\t\tpublic ");
            this.Write(this.ToStringHelper.ToStringWithCulture(GetDataType(field)));
            this.Write(" ");
            this.Write(this.ToStringHelper.ToStringWithCulture(GetSKUNestedPropertyName(field)));
            this.Write("\r\n\t\t\t{\r\n\t\t\t\tget\r\n\t\t\t\t{\r\n\t\t\t\t\tif (SKU == null)\r\n\t\t\t\t\t{\r\n\t\t\t\t\t\treturn ");
            this.Write(this.ToStringHelper.ToStringWithCulture(GetDefaultValue(field)));
            this.Write(";\r\n\t\t\t\t\t}\r\n\r\n\t\t\t\t\treturn ValidationHelper.");
            this.Write(this.ToStringHelper.ToStringWithCulture(GetValidationHelperMethodName(field.DataType)));
            this.Write("(SKU.GetValue(\"");
            this.Write(this.ToStringHelper.ToStringWithCulture(field.Name));
            this.Write("\"), ");
            this.Write(this.ToStringHelper.ToStringWithCulture(GetDefaultValue(field)));
            this.Write(");\r\n\t\t\t\t}\r\n\t\t\t\tset\r\n\t\t\t\t{\r\n\t\t\t\t\tif (SKU != null)\r\n\t\t\t\t\t{\r\n\t\t\t\t\t\tSKU.SetValue(\"");
            this.Write(this.ToStringHelper.ToStringWithCulture(field.Name));
            this.Write("\", value);\r\n\t\t\t\t\t}\r\n\t\t\t\t}\r\n\t\t\t}\r\n");

	}

            this.Write("\t\t}\r\n");

}

            this.Write("\r\n\t\t#endregion\r\n\r\n\r\n\t\t#region \"Constructors\"\r\n\r\n\t\t/// <summary>\r\n\t\t/// Initialize" +
                    "s a new instance of the <see cref=\"");
            this.Write(this.ToStringHelper.ToStringWithCulture(ItemTypeName));
            this.Write("\" /> class.\r\n\t\t/// </summary>\r\n\t\tpublic ");
            this.Write(this.ToStringHelper.ToStringWithCulture(ItemTypeName));
            this.Write("() : base(CLASS_NAME)\r\n\t\t{\r\n\t\t\tmFields = new ");
            this.Write(this.ToStringHelper.ToStringWithCulture(ItemTypeName));
            this.Write("Fields(this);\r\n");
 
if (IsProduct) 
{ 

            this.Write("\t\t\tmProduct = new ProductFields(this);\r\n");

}

            this.Write("\t\t}\r\n\r\n\t\t#endregion\r\n\t}\r\n}");
            return this.GenerationEnvironment.ToString();
        }
    }
    #region Base class
    /// <summary>
    /// Base class for this transformation
    /// </summary>
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.TextTemplating", "15.0.0.0")]
    internal class ContentItemTemplateBase
    {
        #region Fields
        private global::System.Text.StringBuilder generationEnvironmentField;
        private global::System.CodeDom.Compiler.CompilerErrorCollection errorsField;
        private global::System.Collections.Generic.List<int> indentLengthsField;
        private string currentIndentField = "";
        private bool endsWithNewline;
        private global::System.Collections.Generic.IDictionary<string, object> sessionField;
        #endregion
        #region Properties
        /// <summary>
        /// The string builder that generation-time code is using to assemble generated output
        /// </summary>
        protected System.Text.StringBuilder GenerationEnvironment
        {
            get
            {
                if ((this.generationEnvironmentField == null))
                {
                    this.generationEnvironmentField = new global::System.Text.StringBuilder();
                }
                return this.generationEnvironmentField;
            }
            set
            {
                this.generationEnvironmentField = value;
            }
        }
        /// <summary>
        /// The error collection for the generation process
        /// </summary>
        public System.CodeDom.Compiler.CompilerErrorCollection Errors
        {
            get
            {
                if ((this.errorsField == null))
                {
                    this.errorsField = new global::System.CodeDom.Compiler.CompilerErrorCollection();
                }
                return this.errorsField;
            }
        }
        /// <summary>
        /// A list of the lengths of each indent that was added with PushIndent
        /// </summary>
        private System.Collections.Generic.List<int> indentLengths
        {
            get
            {
                if ((this.indentLengthsField == null))
                {
                    this.indentLengthsField = new global::System.Collections.Generic.List<int>();
                }
                return this.indentLengthsField;
            }
        }
        /// <summary>
        /// Gets the current indent we use when adding lines to the output
        /// </summary>
        public string CurrentIndent
        {
            get
            {
                return this.currentIndentField;
            }
        }
        /// <summary>
        /// Current transformation session
        /// </summary>
        public virtual global::System.Collections.Generic.IDictionary<string, object> Session
        {
            get
            {
                return this.sessionField;
            }
            set
            {
                this.sessionField = value;
            }
        }
        #endregion
        #region Transform-time helpers
        /// <summary>
        /// Write text directly into the generated output
        /// </summary>
        public void Write(string textToAppend)
        {
            if (string.IsNullOrEmpty(textToAppend))
            {
                return;
            }
            // If we're starting off, or if the previous text ended with a newline,
            // we have to append the current indent first.
            if (((this.GenerationEnvironment.Length == 0) 
                        || this.endsWithNewline))
            {
                this.GenerationEnvironment.Append(this.currentIndentField);
                this.endsWithNewline = false;
            }
            // Check if the current text ends with a newline
            if (textToAppend.EndsWith(global::System.Environment.NewLine, global::System.StringComparison.CurrentCulture))
            {
                this.endsWithNewline = true;
            }
            // This is an optimization. If the current indent is "", then we don't have to do any
            // of the more complex stuff further down.
            if ((this.currentIndentField.Length == 0))
            {
                this.GenerationEnvironment.Append(textToAppend);
                return;
            }
            // Everywhere there is a newline in the text, add an indent after it
            textToAppend = textToAppend.Replace(global::System.Environment.NewLine, (global::System.Environment.NewLine + this.currentIndentField));
            // If the text ends with a newline, then we should strip off the indent added at the very end
            // because the appropriate indent will be added when the next time Write() is called
            if (this.endsWithNewline)
            {
                this.GenerationEnvironment.Append(textToAppend, 0, (textToAppend.Length - this.currentIndentField.Length));
            }
            else
            {
                this.GenerationEnvironment.Append(textToAppend);
            }
        }
        /// <summary>
        /// Write text directly into the generated output
        /// </summary>
        public void WriteLine(string textToAppend)
        {
            this.Write(textToAppend);
            this.GenerationEnvironment.AppendLine();
            this.endsWithNewline = true;
        }
        /// <summary>
        /// Write formatted text directly into the generated output
        /// </summary>
        public void Write(string format, params object[] args)
        {
            this.Write(string.Format(global::System.Globalization.CultureInfo.CurrentCulture, format, args));
        }
        /// <summary>
        /// Write formatted text directly into the generated output
        /// </summary>
        public void WriteLine(string format, params object[] args)
        {
            this.WriteLine(string.Format(global::System.Globalization.CultureInfo.CurrentCulture, format, args));
        }
        /// <summary>
        /// Raise an error
        /// </summary>
        public void Error(string message)
        {
            System.CodeDom.Compiler.CompilerError error = new global::System.CodeDom.Compiler.CompilerError();
            error.ErrorText = message;
            this.Errors.Add(error);
        }
        /// <summary>
        /// Raise a warning
        /// </summary>
        public void Warning(string message)
        {
            System.CodeDom.Compiler.CompilerError error = new global::System.CodeDom.Compiler.CompilerError();
            error.ErrorText = message;
            error.IsWarning = true;
            this.Errors.Add(error);
        }
        /// <summary>
        /// Increase the indent
        /// </summary>
        public void PushIndent(string indent)
        {
            if ((indent == null))
            {
                throw new global::System.ArgumentNullException("indent");
            }
            this.currentIndentField = (this.currentIndentField + indent);
            this.indentLengths.Add(indent.Length);
        }
        /// <summary>
        /// Remove the last indent that was added with PushIndent
        /// </summary>
        public string PopIndent()
        {
            string returnValue = "";
            if ((this.indentLengths.Count > 0))
            {
                int indentLength = this.indentLengths[(this.indentLengths.Count - 1)];
                this.indentLengths.RemoveAt((this.indentLengths.Count - 1));
                if ((indentLength > 0))
                {
                    returnValue = this.currentIndentField.Substring((this.currentIndentField.Length - indentLength));
                    this.currentIndentField = this.currentIndentField.Remove((this.currentIndentField.Length - indentLength));
                }
            }
            return returnValue;
        }
        /// <summary>
        /// Remove any indentation
        /// </summary>
        public void ClearIndent()
        {
            this.indentLengths.Clear();
            this.currentIndentField = "";
        }
        #endregion
        #region ToString Helpers
        /// <summary>
        /// Utility class to produce culture-oriented representation of an object as a string.
        /// </summary>
        public class ToStringInstanceHelper
        {
            private System.IFormatProvider formatProviderField  = global::System.Globalization.CultureInfo.InvariantCulture;
            /// <summary>
            /// Gets or sets format provider to be used by ToStringWithCulture method.
            /// </summary>
            public System.IFormatProvider FormatProvider
            {
                get
                {
                    return this.formatProviderField ;
                }
                set
                {
                    if ((value != null))
                    {
                        this.formatProviderField  = value;
                    }
                }
            }
            /// <summary>
            /// This is called from the compile/run appdomain to convert objects within an expression block to a string
            /// </summary>
            public string ToStringWithCulture(object objectToConvert)
            {
                if ((objectToConvert == null))
                {
                    throw new global::System.ArgumentNullException("objectToConvert");
                }
                System.Type t = objectToConvert.GetType();
                System.Reflection.MethodInfo method = t.GetMethod("ToString", new System.Type[] {
                            typeof(System.IFormatProvider)});
                if ((method == null))
                {
                    return objectToConvert.ToString();
                }
                else
                {
                    return ((string)(method.Invoke(objectToConvert, new object[] {
                                this.formatProviderField })));
                }
            }
        }
        private ToStringInstanceHelper toStringHelperField = new ToStringInstanceHelper();
        /// <summary>
        /// Helper to produce culture-oriented representation of an object as a string
        /// </summary>
        public ToStringInstanceHelper ToStringHelper
        {
            get
            {
                return this.toStringHelperField;
            }
        }
        #endregion
    }
    #endregion
}
