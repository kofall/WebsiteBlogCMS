using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using WebsiteBlogCMS.Data;

namespace WebsiteBlogCMS.Classes
{
    public static class TokenService
    {
        public static string GenerateToken()
        {
            return Guid.NewGuid().ToString();
        }

        public static void StoreToken(int userId, string token)
        {
            var ctx = DbHelper.DataContext;
            {
                UserToken userToken = new UserToken();

                userToken.userId = userId;
                userToken.token = token;

                ctx.UserTokens.InsertOnSubmit(userToken);
                ctx.SubmitChanges();
            }
        }

        public static bool ValidateToken(string token)
        {
            var ctx = DbHelper.DataContext;
            {
                var tokenData = ctx.UserTokens.Where(x => x.token.Equals(token)).SingleOrDefault();

                if(tokenData != null)
                {
                    if(tokenData.expireDate > DateTime.Now)
                    {
                        return true;
                    }

                    RemoveToken(token);
                }
                return false;
            }
        }

        public static void RemoveToken(string token)
        {
            var ctx = DbHelper.DataContext;
            {
                var tokenData = ctx.UserTokens.Where(x => x.token.Equals(token)).SingleOrDefault();

                if(tokenData != null)
                {
                    ctx.UserTokens.DeleteOnSubmit(tokenData);
                    ctx.SubmitChanges();
                }
            }
        }
    }
}