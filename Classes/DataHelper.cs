using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Runtime.Remoting.Contexts;
using System.Web.Mvc;
using System.Web.UI.WebControls;
using WebsiteBlogCMS.Data;

namespace WebsiteBlogCMS.Classes
{
    public static class DataHelper
    {
        public static class PostUtils
        {
            public static Post GetPost(BlogDBDataContext ctx, int id)
            {
                return ctx.Posts.Where(x => x.id.Equals(id)).SingleOrDefault();
            }

            public static Post GetPost(BlogDBDataContext ctx, string title)
            {
                return ctx.Posts.Where(x => x.title.Equals(title)).SingleOrDefault();
            }

            public static List<Post> GetPosts(BlogDBDataContext ctx, bool onlyVisible = true)
            {
                var result = ctx.Posts.ToList();

                if (onlyVisible)
                {
                    result = result.Where(x => x.isVisible).ToList();
                }

                return result;
            }

            public static List<Category> GetPostCategories(BlogDBDataContext ctx, int id)
            {
                var result = (from categories in ctx.Categories
                              join postCats in ctx.PostCategories on categories.id equals postCats.categoryId
                              where postCats.postId == id
                              select categories).ToList();
                return result;
            }

            public static List<Tag> GetPostTags(BlogDBDataContext ctx, int id)
            {
                var result = (from tags in ctx.Tags
                              join postTags in ctx.PostTags on tags.id equals postTags.tagId
                              where postTags.postId == id
                              select tags).ToList();
                return result;
            }

            public static List<Post> GetPostsByCategory(BlogDBDataContext ctx, Category category, bool onlyVisible = true)
            {
                var result = (from posts in ctx.Posts
                              join postCats in ctx.PostCategories on posts.id equals postCats.postId
                              where postCats.categoryId == category.id
                              select posts).ToList();

                if (onlyVisible)
                {
                    result = result.Where(x => x.isVisible).ToList();
                }

                return result;
            }

            public static List<Post> GetPostsByTag(BlogDBDataContext ctx, Tag tag, bool onlyVisible = true)
            {
                var result = (from posts in ctx.Posts
                              join postTags in ctx.PostTags on posts.id equals postTags.postId
                              where postTags.tagId == tag.id
                              select posts).ToList();

                if (onlyVisible)
                {
                    result = result.Where(x => x.isVisible).ToList();
                }

                return result;
            }

            public static List<Post> GetUserPosts(BlogDBDataContext ctx, int userId, bool onlyVisible = true)
            {
                var result = ctx.Posts.Where(x => x.authorId == userId).ToList();

                if (onlyVisible)
                {
                    result = result.Where(x => x.isVisible).ToList();
                }

                return result;
            }

            public static bool IsPostValid(BlogDBDataContext ctx, Post post)
            {
                return !ctx.Posts.Any(x =>
                    x.title.Equals(post.title) &&
                    (post.id == null || !x.id.Equals(post.id))
                );
            }
        }

        public static class CommentUtils
        {
            public static Comment GetComment(BlogDBDataContext ctx, int id)
            {
                return ctx.Comments.Where(x => x.id.Equals(id)).SingleOrDefault();
            }

            public static Comment GetComment(int id)
            {
                using(var ctx = DbHelper.DataContext)
                {
                    return GetComment(ctx, id);
                }
            }

            public static List<Comment> GetPostComments(BlogDBDataContext ctx, Post post, bool onlyVisible = true)
            {
                return ctx.Comments.Where(x => x.postId == post.id && (!onlyVisible || x.isVisible)).ToList();
            }

            public static List<Comment> GetRootComments(BlogDBDataContext ctx, Post post, bool onlyVisible = true)
            {
                return ctx.Comments.Where(x => x.postId == post.id && x.parentId == null && (!onlyVisible || x.isVisible)).ToList();
            }

            public static List<Comment> GetNestedComments(Comment root, bool onlyVisible = true)
            {
                List<Comment> result = new List<Comment>();
                using(var ctx = DbHelper.DataContext)
                {
                    var children = ctx.Comments.Where(x => x.parentId == root.id && (!onlyVisible || x.isVisible)).ToList();
                    result.AddRange(children);
                    foreach(var child in children)
                    {
                        result.AddRange(GetNestedComments(child, onlyVisible));
                    }
                }
                return result;
            }
        }

        public static class RoleUtils
        {
            public static List<Role> GetRoles(BlogDBDataContext ctx)
            {
                return ctx.Roles.ToList();
            }

            public static List<Role> GetRoles()
            {
                using(var ctx = DbHelper.DataContext)
                {
                    return GetRoles(ctx);
                }
            }

            public static Role GetRole(BlogDBDataContext ctx, int id)
            {
                return ctx.Roles.Where(x => x.id.Equals(id)).SingleOrDefault();
            }
        }

        public static class UserUtils
        {
            public static Dictionary<string, string> GenerateActivationLink()
            {
                string token = TokenService.GenerateToken();
                var dictionary = new Dictionary<string, string>()
                {
                    {"token", token },
                    {"link", $"https://localhost:44313/Admin/Activate?activationCode={token}"}
                };
                return dictionary;
            }

            public static User GetUser(BlogDBDataContext ctx, string login)
            {
                return ctx.Users.Where(x => x.login.Equals(login)).Single();
            }

            public static User GetUser(string login)
            {
                using(var ctx = DbHelper.DataContext)
                {
                    return GetUser(ctx, login);
                }
            }

            public static User GetUser(BlogDBDataContext ctx, int id)
            {
                return ctx.Users.Where(x => x.id.Equals(id)).Single();
            }

            public static Role GetUserRole(int id)
            {
                using(var ctx = DbHelper.DataContext)
                {
                    DataLoadOptions options = new DataLoadOptions();
                    options.LoadWith<User>(x => x.Role);
                    ctx.LoadOptions = options;

                    User user = GetUser(ctx, id);
                    return user.Role;
                }
            }

            public static List<User> GetUsers(BlogDBDataContext ctx)
            {
                return ctx.Users.ToList();
            }

            public static User AuthenticateUser(BlogDBDataContext ctx, string login, string password)
            {
                return ctx.Users.SingleOrDefault(x => x.login.Equals(login) && x.passwordHash.Equals(CryptHelper.Encrypt(password)));
            }

            public static bool HasPermission(int level, string login)
            {
                using(var ctx = DbHelper.DataContext)
                {
                    User user = GetUser(ctx, login);
                    return level >= user.roleId && IsUserConfigured(user);
                } 
            }

            public static string GetUserName(BlogDBDataContext ctx, int id)
            {
                User user = GetUser(ctx, id);
                return $"{user.firstName} {user.lastName}";
            }

            public static string GetUserName(int id)
            {
                using(var ctx = DbHelper.DataContext)
                {
                    return GetUserName(ctx, id);
                }
            }

            public static int GetAdminsCount(BlogDBDataContext ctx)
            {
                return ctx.Users.Where(x => x.Role.name.Equals("Administrator")).Count();
            }

            public static bool IsLastAdmin(BlogDBDataContext ctx, int id)
            {
                User user = GetUser(ctx,id);
                return user.Role.name.Equals("Administrator") && GetAdminsCount(ctx).Equals(1);
            }

            public static bool IsUserConfigured(User user)
            {
                return !string.IsNullOrEmpty(user.firstName)
                    && !string.IsNullOrEmpty(user.lastName)
                    && !string.IsNullOrEmpty(user.intro)
                    && !string.IsNullOrEmpty(user.profile);
            }

            public static bool IsUserValid(BlogDBDataContext ctx, User user)
            {
                return !ctx.Users.Any(x =>
                    x.login.Equals(user.login));
            }
        }

        public static class TagUtils
        {
            public static Tag GetTag(BlogDBDataContext ctx, int id)
            {
                return ctx.Tags.Where(x => x.id.Equals(id)).SingleOrDefault();
            }

            public static List<Tag> GetTags(BlogDBDataContext ctx)
            {
                return ctx.Tags.ToList();
            }

            public static List<Tag> GetAssociatedTags(BlogDBDataContext ctx, bool onlyVisible = true)
            {
                return ctx.Tags.Where(x => x.PostTags.Any(y => !onlyVisible || y.Post.isVisible)).ToList();
            }

            public static bool IsTagValid(BlogDBDataContext ctx, Tag tag)
            {
                return !ctx.Tags.Any(x =>
                    x.title.Equals(tag.title) &&
                    (tag.id == null || !x.id.Equals(tag.id))
                );
            }
        }

        public static class CategoryUtils
        {
            public static Category GetCategory(BlogDBDataContext ctx, int id)
            {
                return ctx.Categories.Where(x => x.id.Equals(id)).SingleOrDefault();
            }

            public static Category GetCategory(int id)
            {
                using (var ctx = DbHelper.DataContext)
                {
                    return GetCategory(ctx, id);
                }
            }

            public static List<Category> GetCategories(BlogDBDataContext ctx)
            {
                return ctx.Categories.ToList();
            }

            public static List<Category> GetAssociatedCategories(BlogDBDataContext ctx, bool onlyVisible = true)
            {
                return ctx.Categories.Where(x => x.PostCategories.Any(y => !onlyVisible || y.Post.isVisible)).ToList();
            }

            public static List<Category> GetRootCategories(BlogDBDataContext ctx)
            {
                return ctx.Categories.Where(x => x.parentId == null).ToList();
            }

            public static List<Category> GetRootCategories()
            {
                using(var ctx = DbHelper.DataContext)
                {
                    return GetRootCategories(ctx);
                }
            }

            public static bool IsCategoryValid(BlogDBDataContext ctx, Category category)
            {
                return !ctx.Categories.Any(x =>
                    x.title.Equals(category.title) &&
                    (category.id == null || !x.id.Equals(category.id))
                );
            }

            public static List<Category> GetSubCats(BlogDBDataContext ctx, Category category, bool onlyVisible = true)
            {
                return ctx.Categories.Where(x => x.parentId.Equals(category.id) && x.PostCategories.Any(y => !onlyVisible || y.Post.isVisible)).ToList();
            }

            public static List<Category> GetSubCats(Category category, bool onlyVisible = true)
            {
                using(var ctx = DbHelper.DataContext)
                {
                    return GetSubCats(ctx, category, onlyVisible);
                }
            }

            private static bool IsChildRecursive(List<Category> categories, int id, int? parentId)
            {
                if (!parentId.HasValue)
                    return false;

                if (parentId == id)
                    return true;

                // Apply recursive logic in memory
                return categories
                    .Where(c => c.id == parentId)
                    .Any(c => IsChildRecursive(categories, id, c.parentId));
            }

            public static bool WillCreateLoop(int id, int? parentId)
            {
                using (var ctx = DbHelper.DataContext)
                {
                    var categories = ctx.Categories.ToList();
                    return IsChildRecursive(categories, id, parentId);
                }
            }
        }

        public static class SliderUtils
        {
            public static Slider GetSlider(BlogDBDataContext ctx, int id)
            {
                return ctx.Sliders.Where(x => x.id.Equals(id)).SingleOrDefault();
            }

            public static List<Slider> GetSliders(BlogDBDataContext ctx)
            {
                return ctx.Sliders.ToList();
            }

            public static bool IsSliderValid(BlogDBDataContext ctx, Slider slider)
            {
                return !ctx.Sliders.Any(x =>
                    x.title.Equals(slider.title) &&
                    (slider.id == null || !x.id.Equals(slider.id))
                );
            }
        }

        public static class ModelUtils
        {
            public static string GetErrorMessages(ModelStateDictionary modelState)
            {
                string errorMessage = string.Empty;
                foreach (var key in modelState.Keys)
                {
                    foreach (var error in modelState[key].Errors)
                    {
                        errorMessage += $"{error.ErrorMessage} ";
                    }
                }
                return errorMessage;
            }
        }
    }
}