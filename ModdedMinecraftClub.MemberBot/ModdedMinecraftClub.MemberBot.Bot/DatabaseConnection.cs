﻿using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using MySql.Data.MySqlClient;

namespace ModdedMinecraftClub.MemberBot.Bot
{
    public interface IDatabaseConnection
    {
        void ExecuteRawNonQuery(string sql);
        void ExecuteRawNonQuery<T>(string sql, T objectToInsert);
        List<T> ExecuteRawQuery<T>(string sql);
        bool DoesTableExist();
        void CreateTable();
        List<Application> GetAllPending();
        Application GetById(int applicationId);
        List<Application> GetLast20Approved();
        List<Application> GetLast20Rejected();
        void MarkAsApproved(int applicationId);
        void MarkAsRejected(int applicationId);
    }
    
    public class DatabaseConnection : IDisposable, IDatabaseConnection
    {
        private readonly MySqlConnection _connection;

        public DatabaseConnection()
        {
            _connection = new MySqlConnection(GetConnectionString());
        }

        private static string GetConnectionString()
        {
            var config = Program.Config.Mysql;
            
            return $"Server={config.ServerIp};Port={config.Port};Database={config.DatabaseName};Uid={config.Username};Pwd={config.Password}";
        }
        
        public void ExecuteRawNonQuery(string sql)
        {
            _connection.Execute(sql);
        }

        public void ExecuteRawNonQuery<T>(string sql, T objectToInsert)
        {
            _connection.Execute(sql, objectToInsert);
        }

        public List<T> ExecuteRawQuery<T>(string sql)
        {
            return _connection.Query<T>(sql).ToList();
        }

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
                "create table applications\n(\n\tAppId int not null auto_increment,\n\tAppStatus int not null,\n\tAppTime varchar(50) null,\n\tAuthorName varchar(50) not null,\n\tAuthorDiscordId long null,\n\tMessageContent varchar(800) null,\n    MessageUrl varchar(250) not null,\n\tImageUrl varchar(250) null,\n\tprimary key (AppId)\n);";

            _connection.Execute(sql);
        }

        public void InsertNewApplication(Application application)
        {
            const string sql =
                "insert into applications (AppStatus, AppTime, AuthorName, AuthorDiscordId, MessageContent, MessageUrl, ImageUrl) values (@AppStatus, @AppTime, @AuthorName, @AuthorDiscordId, @MessageContent, @MessageUrl, @ImageUrl)";

            _connection.Execute(sql, application);
        }

        public List<Application> GetAllPending()
        {
            const string sql =
                "select * from applications where AppStatus = 0 order by AppId";

            return _connection.Query<Application>(sql).ToList();
        }
        
        public Application GetById(int applicationId)
        {
            const string sql =
                "select * from applications where AppId = @AppId";
            
            var l = _connection.Query<Application>(sql, new { AppId = applicationId }).ToList();

            return !l.Any() ? null : l[0];
        }
        
        public List<Application> GetLast20Approved()
        {
            const string sql =
                "select * from applications where AppStatus = 1 order by AppId limit 20";
            
            return _connection.Query<Application>(sql).ToList();
        }
        
        public List<Application> GetLast20Rejected()
        {
            const string sql =
                "select * from applications where AppStatus = 2 order by AppId limit 20";
            
            return _connection.Query<Application>(sql).ToList();
        }

        public void MarkAsApproved(int applicationId)
        {
            const string sql =
                "update applications\nset AppStatus = 1\nwhere AppId = @AppId";

            _connection.Execute(sql, new { AppId = applicationId });
        }

        public void MarkAsRejected(int applicationId)
        {
            const string sql =
                "update applications set AppStatus = 2 where AppId = @AppId";

            _connection.Execute(sql, new { AppId = applicationId });
        }

        public void Dispose()
        {
           _connection.Dispose();
        }
    }
}