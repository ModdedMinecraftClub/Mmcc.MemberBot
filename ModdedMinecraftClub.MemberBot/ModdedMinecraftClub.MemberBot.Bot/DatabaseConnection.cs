using System;
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
    }
    
    public class DatabaseConnection : IDisposable
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

        public void Dispose()
        {
           _connection.Dispose();
        }
    }
}