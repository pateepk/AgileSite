using System;
using System.Data;

using CMS.Blogs;
using CMS.DocumentEngine;
using CMS.Helpers;
using CMS.Membership;
using CMS.PortalEngine;
using CMS.SiteProvider;

namespace APIExamples
{
     /// <summary>
    /// Holds API examples related to blogs.
    /// </summary>
    /// <pageTitle>Blogs</pageTitle>
    internal class Blogs
    {
        /// <summary>
        /// Holds blog API examples.
        /// </summary>
        /// <groupHeading>Blogs</groupHeading>
        private class Blog
        {
            /// <heading>Creating a new blog</heading>
            private void CreateBlog()
            {
                // Prepares a TreeProvider instance
                TreeProvider tree = new TreeProvider(MembershipContext.AuthenticatedUser);

                // Gets a parent page under which the blog will be created
                TreeNode parentPage = tree.SelectNodes()
                    .Path("/Blogs")
                    .OnCurrentSite()
                    .Culture("en-us")
                    .FirstObject;

                if (parentPage != null)
                {
                    // Creates a new blog page 
                    TreeNode blogPage = TreeNode.New("CMS.Blog", tree);

                    // Sets the basic page properties for the blog
                    blogPage.DocumentName = "NewBlog";
                    blogPage.DocumentCulture = "en-us";

                    // Sets values for the fields of the blog page type
                    blogPage.SetValue("BlogName", "NewBlog");
                    blogPage.SetValue("BlogDescription", "This blog was created through the API.");
                    blogPage.SetValue("BlogOpenCommentsFor", "##ALL##");
                    blogPage.SetValue("BlogAllowAnonymousComments", "true");
                    blogPage.SetValue("BlogModerateComments", "");
                    blogPage.SetValue("BlogOpenCommentsFor", "");
                    blogPage.SetValue("BlogUseCAPTCHAForComments", "");
                    blogPage.SetValue("BlogEnableSubscriptions", "true");

                    // Gets a page template for the blog page
                    PageTemplateInfo pageTemplate = PageTemplateInfoProvider.GetPageTemplateInfo("CorporateSite.BlogDetail");
                    if (pageTemplate != null)
                    {
                        // Assigns the page template to the blog
                        blogPage.SetDefaultPageTemplateID(pageTemplate.PageTemplateId);
                    }

                    // Inserts the blog under the specified parent page
                    blogPage.Insert(parentPage);                    
                }
            }


            /// <heading>Updating a blog</heading>
            private void GetAndUpdateBlog()
            {
                // Prepares a TreeProvider instance
                TreeProvider tree = new TreeProvider(MembershipContext.AuthenticatedUser);

                // Gets the blog page
                TreeNode blogPage = tree.SelectNodes()
                    .Path("/Blogs/NewBlog")
                    .OnCurrentSite()
                    .Culture("en-us")
                    .FirstObject;


                if (blogPage != null)
                {
                    // Changes the name of the blog page
                    blogPage.DocumentName = blogPage.DocumentName.ToLower();
                    
                    // Saves the updated blog to the database
                    blogPage.Update();
                }
            }


            /// <heading>Updating multiple blogs</heading>
            private void GetAndBulkUpdateBlogs()
            {
                // Prepares a TreeProvider instance
                TreeProvider tree = new TreeProvider(MembershipContext.AuthenticatedUser);

                // Gets all blog pages under the current site's "/Blogs" section
                var blogPages = tree.SelectNodes("CMS.Blog")
                                                .OnCurrentSite()
                                                .Path("/Blogs", PathTypeEnum.Children);                
                
                // Loops through individual blog pages
                foreach (TreeNode blogPage in blogPages)
                {
                    // Changes the name of the blog page
                    blogPage.DocumentName = blogPage.DocumentName.ToUpper();

                    // Saves the updated blog to the database
                    blogPage.Update();
                }
            }


            /// <heading>Deleting a blog</heading>
            private void DeleteBlog()
            {
                // Prepares a TreeProvider instance
                TreeProvider tree = new TreeProvider(MembershipContext.AuthenticatedUser);

                // Gets the blog page
                TreeNode blogPage = tree.SelectNodes()
                    .Path("/Blogs/NewBlog")
                    .OnCurrentSite()
                    .Culture("en-us")
                    .FirstObject;

                if (blogPage != null)
                {
                    // Deletes the blog (all language versions)
                    blogPage.DeleteAllCultures();
                }
            }
        }


        /// <summary>
        /// Holds blog post API examples.
        /// </summary>
        /// <groupHeading>Blog posts</groupHeading>
        private class BlogPosts
        {
            /// <heading>Creating a blog post</heading>
            private void CreateBlogPost()
            {
                // Prepares a TreeProvider instance
                TreeProvider tree = new TreeProvider(MembershipContext.AuthenticatedUser);

                // Gets a parent blog page
                TreeNode parentBlog = tree.SelectNodes()
                    .Path("/Blogs/NewBlog")
                    .OnCurrentSite()
                    .Culture("en-us")
                    .FirstObject;

                if (parentBlog != null)
                {
                    // Creates a new blog post page
                    TreeNode blogPost = TreeNode.New("CMS.BlogPost", tree);

                    // Sets the page properties and blog post field values
                    blogPost.DocumentName = "NewBlogPost";
                    blogPost.SetValue("BlogPostTitle", "NewBlogPost");
                    blogPost.SetValue("BlogPostDate", DateTime.Now);
                    blogPost.SetValue("BlogPostSummary", "Blog post summary");
                    blogPost.SetValue("BlogPostBody", "Blog post body");
                    blogPost.SetValue("BlogPostAllowComments", "true");
                    blogPost.DocumentCulture = "en-us";

                    // Prepares a blog month page as the parent for the post under the specified blog
                    TreeNode parentMonth = DocumentHelper.EnsureBlogPostHierarchy(blogPost, parentBlog, tree);

                    // Creates the blog post
                    blogPost.Insert(parentMonth);
                }
            }


            /// <heading>Updating blog posts</heading>
            private void GetAndBulkUpdateBlogPosts()
            {
                // Prepares a TreeProvider instance
                TreeProvider tree = new TreeProvider(MembershipContext.AuthenticatedUser);

                // Gets all published blog posts under "/Blogs/NewBlog" as a DataSet
                DataSet posts = BlogHelper.GetBlogPosts(SiteContext.CurrentSiteName, "/Blogs/NewBlog", null, true, null, null, true);

                if (!DataHelper.DataSourceIsEmpty(posts))
                {
                    // Loops through individual blog posts in the DataSet
                    foreach (DataRow postDr in posts.Tables[0].Rows)
                    {
                        // Creates a page object from the DataRow
                        TreeNode modifyPost = TreeNode.New("cms.blogpost", postDr, tree);

                        // Updates the blog post properties
                        modifyPost.SetValue("BlogPostBody", "The blog post body was updated.");

                        // Saves the updated blog post to the database
                        modifyPost.Update();
                    }
                }
            }


            /// <heading>Deleting a blog post</heading>
            private void DeleteBlogPost()
            {
                // Prepares a TreeProvider instance
                TreeProvider tree = new TreeProvider(MembershipContext.AuthenticatedUser);

                // Gets all published blog posts under "/Blogs/NewBlog" as a DataSet
                DataSet posts = BlogHelper.GetBlogPosts(SiteContext.CurrentSiteName, "/Blogs/NewBlog", null, true, null, null, true);
                
                if (!DataHelper.DataSourceIsEmpty(posts))
                {
                    // Gets the first blog post page from the DataSet
                    TreeNode deletePost = TreeNode.New("cms.blogpost", posts.Tables[0].Rows[0], tree);
                    
                    // Deletes the blog post (all language versions)                    
                    deletePost.DeleteAllCultures();
                }
            }
        }


        /// <summary>
        /// Holds blog comments API examples.
        /// </summary>
        /// <groupHeading>Blog comments</groupHeading>
        private class BlogComments
        {
            /// <heading>Creating a blog comment</heading>
            private void CreateBlogComment()
            {
                // Prepares the blog post object
                TreeNode blogPost = null;

                // Gets all published blog posts under "/Blogs/NewBlog" as a DataSet
                DataSet posts = BlogHelper.GetBlogPosts(SiteContext.CurrentSiteName, "/Blogs/NewBlog", null, true, null, null, true);

                if (!DataHelper.DataSourceIsEmpty(posts))
                {
                    // Prepares a TreeProvider instance
                    TreeProvider tree = new TreeProvider(MembershipContext.AuthenticatedUser);

                    // Gets the first blog post from the DataSet
                    blogPost = TreeNode.New("cms.blogpost", posts.Tables[0].Rows[0], tree);
                }

                if (blogPost != null)
                {
                    // Creates a new blog comment object
                    BlogCommentInfo newComment = new BlogCommentInfo();

                    // Sets the comment properties
                    newComment.CommentText = "New comment";
                    newComment.CommentUserName = MembershipContext.AuthenticatedUser.UserName;
                    newComment.CommentUserID = MembershipContext.AuthenticatedUser.UserID;
                    newComment.CommentApprovedByUserID = MembershipContext.AuthenticatedUser.UserID;
                    newComment.CommentPostDocumentID = blogPost.DocumentID;
                    newComment.CommentDate = DateTime.Now;

                    // Saves the blog comment to the database
                    BlogCommentInfoProvider.SetBlogCommentInfo(newComment);
                }
            }


            /// <heading>Updating blog comments</heading>
            private void GetAndUpdateBlogComment()
            {
                // Prepares the blog post object
                TreeNode blogPost = null;

                // Gets all published blog posts under "/Blogs/NewBlog" as a DataSet
                DataSet posts = BlogHelper.GetBlogPosts(SiteContext.CurrentSiteName, "/Blogs/NewBlog", null, true, null, null, true);

                if (!DataHelper.DataSourceIsEmpty(posts))
                {
                    // Prepares a TreeProvider instance
                    TreeProvider tree = new TreeProvider(MembershipContext.AuthenticatedUser);

                    // Gets the first blog post from the DataSet
                    blogPost = TreeNode.New("cms.blogpost", posts.Tables[0].Rows[0], tree);
                }

                if (blogPost != null)
                {
                    // Gets all comments under the specified blog post that are not marked as spam
                    var blogComments = BlogCommentInfoProvider.GetBlogComments()
                                                                    .WhereEquals("CommentPostDocumentID", blogPost.DocumentID)                                                                    
                                                                    .WhereEquals("CommentIsSpam", 0);

                    // Loops through individual blog comments
                    foreach (BlogCommentInfo blogComment in blogComments)
                    {
                        // Updates the comment text
                        blogComment.CommentText = blogComment.CommentText.ToUpper();

                        // Saves the updated blog comment to the database
                        BlogCommentInfoProvider.SetBlogCommentInfo(blogComment);
                    }
                }
            }


            /// <heading>Deleting blog comments</heading>
            private void DeleteBlogComment()
            {
                // Prepares the blog post object
                TreeNode blogPost = null;

                // Gets all published blog posts under "/Blogs/NewBlog" as a DataSet
                DataSet posts = BlogHelper.GetBlogPosts(SiteContext.CurrentSiteName, "/Blogs/NewBlog", null, true, null, null, true);

                if (!DataHelper.DataSourceIsEmpty(posts))
                {
                    // Prepares a TreeProvider instance
                    TreeProvider tree = new TreeProvider(MembershipContext.AuthenticatedUser);

                    // Gets the first blog post from the DataSet
                    blogPost = TreeNode.New("cms.blogpost", posts.Tables[0].Rows[0], tree);
                }

                if (blogPost != null)
                {
                    // Gets all comments under the specified blog post that are marked as spam
                    var blogComments = BlogCommentInfoProvider.GetBlogComments()
                                                                    .WhereEquals("CommentPostDocumentID", blogPost.DocumentID)                                                                    
                                                                    .WhereEquals("CommentIsSpam", 1);

                    // Loops through individual blog comments
                    foreach (BlogCommentInfo blogComment in blogComments)
                    {
                        // Deletes the blog comment
                        BlogCommentInfoProvider.DeleteBlogCommentInfo(blogComment);
                    }
                }
            }
        }


        /// <summary>
        /// Holds blog post subscription API examples.
        /// </summary>
        /// <groupHeading>Blog post subscriptions</groupHeading>
        private class BlogPostSubscriptions
        {
            /// <heading>Creating a blog post subscription</heading>
            private void CreateBlogPostSubscription()
            {
                // Prepares the blog post object
                TreeNode blogPost = null;

                // Gets all published blog posts under "/Blogs/NewBlog" as a DataSet
                DataSet posts = BlogHelper.GetBlogPosts(SiteContext.CurrentSiteName, "/Blogs/NewBlog", null, true, null, null, true);

                if (!DataHelper.DataSourceIsEmpty(posts))
                {
                    // Prepares a TreeProvider instance
                    TreeProvider tree = new TreeProvider(MembershipContext.AuthenticatedUser);

                    // Gets the first blog post from the DataSet
                    blogPost = TreeNode.New("cms.blogpost", posts.Tables[0].Rows[0], tree);
                }

                if (blogPost != null)
                {
                    // Creates a new blog post subscription object
                    BlogPostSubscriptionInfo newSubscription = new BlogPostSubscriptionInfo();

                    // Sets the subscription properties (subscribes the current user)
                    newSubscription.SubscriptionPostDocumentID = blogPost.DocumentID;
                    newSubscription.SubscriptionUserID = MembershipContext.AuthenticatedUser.UserID;
                    newSubscription.SubscriptionEmail = MembershipContext.AuthenticatedUser.Email;

                    // Saves the blog post subscription to the database
                    BlogPostSubscriptionInfoProvider.SetBlogPostSubscriptionInfo(newSubscription);
                }
            }


            /// <heading>Updating a blog post subscription</heading>
            private void GetAndUpdateBlogPostSubscription()
            {
                // Prepares the blog post object
                TreeNode blogPost = null;

                // Gets all published blog posts under "/Blogs/NewBlog" as a DataSet
                DataSet posts = BlogHelper.GetBlogPosts(SiteContext.CurrentSiteName, "/Blogs/NewBlog", null, true, null, null, true);

                if (!DataHelper.DataSourceIsEmpty(posts))
                {
                    // Prepares a TreeProvider instance
                    TreeProvider tree = new TreeProvider(MembershipContext.AuthenticatedUser);

                    // Gets the first blog post from the DataSet
                    blogPost = TreeNode.New("cms.blogpost", posts.Tables[0].Rows[0], tree);
                }

                if (blogPost != null)
                {
                    // Gets the subscription for the specified blog post and email address
                    BlogPostSubscriptionInfo updateSubscription = BlogPostSubscriptionInfoProvider.GetBlogPostSubscriptionInfo("administrator@localhost.local", blogPost.DocumentID);

                    if (updateSubscription != null)
                    {
                        // Updates the subscription properties
                        updateSubscription.SubscriptionApproved = true;

                        // Saves the updated blog post subscription to the database
                        BlogPostSubscriptionInfoProvider.SetBlogPostSubscriptionInfo(updateSubscription);
                    }
                }
            }


            /// <heading>Updating multiple blog post subscriptions</heading>
            private void GetAndBulkUpdateBlogPostSubscriptions()
            {
                // Gets all blog post subscriptions in the system whose email address ends with 'localhost.local'
                var subscriptions = BlogPostSubscriptionInfoProvider.GetBlogPostSubscriptions().WhereEndsWith("SubscriptionEmail", "localhost.local");
                
                // Loops through individual subscriptions
                foreach (BlogPostSubscriptionInfo subscription in subscriptions)
                {
                    // Updates the subscription properties
                    subscription.SubscriptionApproved = false;

                    // Saves the updated blog post subscription to the database
                    BlogPostSubscriptionInfoProvider.SetBlogPostSubscriptionInfo(subscription);
                }
            }


            /// <heading>Deleting a blog post subscription</heading>
            private void DeleteBlogPostSubscription()
            {
                // Prepares the blog post object
                TreeNode blogPost = null;

                // Gets all published blog posts under "/Blogs/NewBlog" as a DataSet
                DataSet posts = BlogHelper.GetBlogPosts(SiteContext.CurrentSiteName, "/Blogs/NewBlog", null, true, null, null, true);

                if (!DataHelper.DataSourceIsEmpty(posts))
                {
                    // Prepares a TreeProvider instance
                    TreeProvider tree = new TreeProvider(MembershipContext.AuthenticatedUser);

                    // Gets the first blog post from the DataSet
                    blogPost = TreeNode.New("cms.blogpost", posts.Tables[0].Rows[0], tree);
                }

                if (blogPost != null)
                {
                    // Gets the subscription for the specified blog post and email address
                    BlogPostSubscriptionInfo deleteSubscription = BlogPostSubscriptionInfoProvider.GetBlogPostSubscriptionInfo("administrator@localhost.local", blogPost.DocumentID);

                    if (deleteSubscription != null)
                    {
                        // Deletes the blog post subscription
                        BlogPostSubscriptionInfoProvider.DeleteBlogPostSubscriptionInfo(deleteSubscription);
                    }
                }
            }
        }
    }
}
