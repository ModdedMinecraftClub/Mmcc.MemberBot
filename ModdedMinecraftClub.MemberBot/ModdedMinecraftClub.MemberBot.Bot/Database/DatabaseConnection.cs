using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper;
using ModdedMinecraftClub.MemberBot.Bot.Models;
using MySql.Data.MySqlClient;

namespace ModdedMinecraftClub.MemberBot.Bot.Database
{
    public class DatabaseConnection : IDisposable
    {
        private readonly MySqlConnection _connection;
        private readonly ConfigRoot _config;

        public DatabaseConnection(ConfigRoot config)
        {
            _config = config;
            _connection = new MySqlConnection($"Server={_config.Mysql.ServerIp};Port={_config.Mysql.Port};Database={_config.Mysql.DatabaseName};Uid={_config.Mysql.Username};Pwd={_config.Mysql.Password};Allow User Variables=True");
        }
        
        #region Startup Checks
        
        public async Task<bool> DoesTableExistAsync()
        {
            var name = _config.Mysql.DatabaseName;
            const string sql =
                "SELECT count(*) FROM information_schema.TABLES WHERE (TABLE_SCHEMA = @name) AND (TABLE_NAME = 'applications');";

            var q = await _connection.QueryFirstOrDefaultAsync<int>(sql, new {name});

            return q != 0;
        }

        public async Task CreateTableAsync()
        {
            const string sql =
                "create table applications (\n\tAppId int not null auto_increment,\n\tAppStatus int not null,\n\tAppTime varchar(50) null,\n\tAuthorName varchar(50) not null,\n\tAuthorDiscordId long not null,\n\tMessageContent varchar(800) null,\n    MessageUrl varchar(250) not null,\n\tImageUrl varchar(250) null,\n\tprimary key (AppId)\n);";

            await _connection.ExecuteAsync(sql);
        }
        
        #endregion
        
        #region Member Applications
        
        public async Task InsertNewApplicationAsync(Application application)
        {
            const string sql =
                "INSERT INTO applications (AppStatus, AppTime, AuthorName, AuthorDiscordId, MessageContent, MessageUrl, ImageUrl) VALUES (@AppStatus, @AppTime, @AuthorName, @AuthorDiscordId, @MessageContent, @MessageUrl, @ImageUrl)";

            await _connection.ExecuteAsync(sql, application);
        }

        public Task<IEnumerable<Application>> GetAllPendingAsync()
        {
            const string sql =
                "SELECT * FROM applications WHERE AppStatus = 0 ORDER BY AppId";

            var res = _connection.QueryAsync<Application>(sql);

            return res;
        }
        
        public async Task<IEnumerable<Application>> GetLast20ApprovedAsync()
        {
            const string sql =
                "SELECT * FROM applications WHERE AppStatus = 1 ORDER BY AppId LIMIT 20";

            var res = await _connection.QueryAsync<Application>(sql);

            return res;
        }
        
        public async Task<IEnumerable<Application>> GetLast20RejectedAsync()
        {
            const string sql =
                "SELECT * FROM applications WHERE AppStatus = 2 ORDER BY AppId LIMIT 20";
            
            var res = await _connection.QueryAsync<Application>(sql);

            return res;
        }
        
        public async Task<Application> GetByIdAsync(int applicationId)
        {
            const string sql =
                "SELECT * FROM applications WHERE AppId = @AppId";

            var res = await _connection.QuerySingleOrDefaultAsync<Application>(sql, new {AppId = applicationId});

            return res;
        }

        public async Task MarkAsApprovedAsync(int applicationId)
        {
            const string sql =
                "UPDATE applications SET AppStatus = 1 WHERE AppId = @AppId";

            await _connection.ExecuteAsync(sql, new { AppId = applicationId });
        }

        public async Task MarkAsRejectedAsync(int applicationId)
        {
            const string sql =
                "UPDATE applications SET AppStatus = 2 WHERE AppId = @AppId";

            await _connection.ExecuteAsync(sql, new { AppId = applicationId });
        }
        
        #endregion Member Applications
        
        public void Dispose()
        {
           _connection.Dispose();
        }
    }
}