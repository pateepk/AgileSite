using System;

using CMS.DocumentEngine;
using CMS.Taxonomy;
using CMS.Helpers;
using CMS.Blogs.Web.UI;

/// <summary>
/// Blog functions.
/// </summary>
public class BlogFunctions : BlogTransformationFunctions
{
    public string GetBlogCategories(int documentId, string documentListAliasPath)
    {
        if (documentId < 1)
        {
            throw new Exception("Invalid document ID");
        }

        // Uses the current page's alias path if one is not specified for the category list page
        if (documentListAliasPath == null)
        {
            documentListAliasPath = DocumentContext.CurrentAliasPath;
        }

        // Initializes the HTML code result
        string result = "";

        // Gets the categories of the specified page
        var categories = DocumentCategoryInfoProvider.GetDocumentCategories(documentId)
                                                            .Columns("CMS_Category.CategoryID, CategoryDisplayName");

        foreach (CategoryInfo category in categories)
        {
            // Constructs links for the assigned categories
            // The links lead to a page containing a list of pages that belong to the same category, with the category ID in the query string
            int categoryId = category.CategoryID;
            string categoryName = category.CategoryDisplayName;

            result += "<a href=\"" + URLHelper.ResolveUrl(DocumentURLProvider.GetUrl(documentListAliasPath));
            result += "?category=" + categoryId;
            result += "\">" + categoryName + "</a>&nbsp;";
        }

        return result;
    }
}