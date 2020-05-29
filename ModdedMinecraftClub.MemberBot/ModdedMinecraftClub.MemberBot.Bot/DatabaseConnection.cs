using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using ModdedMinecraftClub.MemberBot.Bot.Models;
using MySql.Data.MySqlClient;

namespace ModdedMinecraftClub.MemberBot.Bot
{
    public class DatabaseConnection : IDisposable
    {
        private readonly MySqlConnection _connection;

        public DatabaseConnection()
        {
            _connection = new MySqlConnection(Helper.GetMySqlConnectionString());
        }
        
        #region Startup Checks
        
        public bool DoesTableExist()
        {
            var name = Program.Config.Mysql.DatabaseName;
            const string sql =
                "SELECT count(*) FROM information_schema.TABLES WHERE (TABLE_SCHEMA = @name) AND (TABLE_NAME = 'applications');";
            
            var q = _connection.Query<int>(sql, new { name }).ToList();

            return q[0] != 0;
        }

        public void CreateTable()
        {
            const string sql =
                "create table applications (\n\tAppId int not null auto_increment,\n\tAppStatus int not null,\n\tAppTime varchar(50) null,\n\tAuthorName varchar(50) not null,\n\tAuthorDiscordId long not null,\n\tMessageContent varchar(800) null,\n    MessageUrl varchar(250) not null,\n\tImageUrl varchar(250) null,\n\tprimary key (AppId)\n);";

            _connection.Execute(sql);
        }
        
        #endregion
        
        #region Member Applications
        
        public void InsertNewApplication(Application application)
        {
            const string sql =
                "INSERT INTO applications (AppStatus, AppTime, AuthorName, AuthorDiscordId, MessageContent, MessageUrl, ImageUrl) VALUES (@AppStatus, @AppTime, @AuthorName, @AuthorDiscordId, @MessageContent, @MessageUrl, @ImageUrl)";

            _connection.Execute(sql, application);
        }

        public List<Application> GetAllPending()
        {
            const string sql =
                "SELECT * FROM applications WHERE AppStatus = 0 ORDER BY AppId";

            return _connection.Query<Application>(sql).ToList();
        }
        
        public List<Application> GetLast20Approved()
        {
            const string sql =
                "SELECT * FROM applications WHERE AppStatus = 1 ORDER BY AppId LIMIT 20";
            
            return _connection.Query<Application>(sql).ToList();
        }
        
        public List<Application> GetLast20Rejected()
        {
            const string sql =
                "SELECT * FROM applications WHERE AppStatus = 2 ORDER BY AppId LIMIT 20";
            
            return _connection.Query<Application>(sql).ToList();
        }
        
        public Application GetById(int applicationId)
        {
            const string sql =
                "SELECT * FROM applications WHERE AppId = @AppId";
            
            var l = _connection.Query<Application>(sql, new { AppId = applicationId }).ToList();

            return !l.Any() ? null : l[0];
        }

        public void MarkAsApproved(int applicationId)
        {
            const string sql =
                "UPDATE applications SET AppStatus = 1 WHERE AppId = @AppId";

            _connection.Execute(sql, new { AppId = applicationId });
        }

        public void MarkAsRejected(int applicationId)
        {
            const string sql =
                "UPDATE applications SET AppStatus = 2 WHERE AppId = @AppId";

            _connection.Execute(sql, new { AppId = applicationId });
        }
        
        #endregion Member Applications
        
        public void Dispose()
        {
           _connection.Dispose();
        }
    }
}