using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using BCrypt.Net;

namespace WpfApp14.Services
{
    public static class DatabaseService
    {
        private static SqliteConnection? _connection;

        public static void Initialize(string dbPath)
        {
            if (_connection == null)
            {
                var connString = new SqliteConnectionStringBuilder
                {
                    DataSource = dbPath,
                    Mode = SqliteOpenMode.ReadWriteCreate
                }.ToString();
                _connection = new SqliteConnection(connString);
            }

            if (_connection.State != System.Data.ConnectionState.Open)
                _connection.Open();

            using var cmd = _connection.CreateCommand();

            // Создание таблицы Users
            cmd.CommandText = @"CREATE TABLE IF NOT EXISTS Users (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Username TEXT NOT NULL UNIQUE,
                PasswordHash TEXT NOT NULL,
                IQ INTEGER DEFAULT 100,
                TotalQuizzes INTEGER DEFAULT 0,
                CorrectAnswers INTEGER DEFAULT 0,
                TotalAnswers INTEGER DEFAULT 0
            );";
            cmd.ExecuteNonQuery();

            // Обновление таблицы Quizzes с полем UserId
            cmd.CommandText = @"CREATE TABLE IF NOT EXISTS Quizzes (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                UserId INTEGER NOT NULL,
                Title TEXT NOT NULL,
                FOREIGN KEY(UserId) REFERENCES Users(Id)
            );";
            cmd.ExecuteNonQuery();

            cmd.CommandText = @"CREATE TABLE IF NOT EXISTS Questions (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                QuizId INTEGER NOT NULL,
                Text TEXT NOT NULL,
                TimerSeconds INTEGER NOT NULL,
                FOREIGN KEY(QuizId) REFERENCES Quizzes(Id)
            );";
            cmd.ExecuteNonQuery();

            cmd.CommandText = @"CREATE TABLE IF NOT EXISTS Answers (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                QuestionId INTEGER NOT NULL,
                Text TEXT NOT NULL,
                IsCorrect INTEGER NOT NULL,
                FOREIGN KEY(QuestionId) REFERENCES Questions(Id)
            );";
            cmd.ExecuteNonQuery();
        }

        // Регистрация нового пользователя
        public static int RegisterUser(string username, string password)
        {
            if (_connection == null) throw new InvalidOperationException("DB not initialized");

            using var tx = _connection.BeginTransaction();
            using var cmd = _connection.CreateCommand();
            cmd.Transaction = tx;

            string passwordHash = BCrypt.Net.BCrypt.HashPassword(password);
            cmd.CommandText = @"INSERT INTO Users (Username, PasswordHash) 
                VALUES ($u, $p);";
            cmd.Parameters.AddWithValue("$u", username);
            cmd.Parameters.AddWithValue("$p", passwordHash);
            cmd.ExecuteNonQuery();

            cmd.CommandText = "SELECT last_insert_rowid();";
            var userId = (long)cmd.ExecuteScalar()!;
            tx.Commit();

            return (int)userId;
        }

        // Аутентификация пользователя
        public static User? AuthenticateUser(string username, string password)
        {
            if (_connection == null) throw new InvalidOperationException("DB not initialized");

            using var cmd = _connection.CreateCommand();
            cmd.CommandText = @"SELECT Id, Username, PasswordHash, IQ, TotalQuizzes, CorrectAnswers, TotalAnswers 
                FROM Users WHERE Username = $u;";
            cmd.Parameters.AddWithValue("$u", username);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                string storedHash = reader.GetString(2);
                if (BCrypt.Net.BCrypt.Verify(password, storedHash))
                {
                    return new User
                    {
                        Id = reader.GetInt32(0),
                        Username = reader.GetString(1),
                        IQ = reader.GetInt32(3),
                        TotalQuizzes = reader.GetInt32(4),
                        CorrectAnswers = reader.GetInt32(5),
                        TotalAnswers = reader.GetInt32(6)
                    };
                }
            }
            return null;
        }

        // Вставка викторины с учетом UserId
        public static int InsertQuiz(int userId, string title)
        {
            if (_connection == null) throw new InvalidOperationException("DB not initialized");

            using var tx = _connection.BeginTransaction();
            using var cmd = _connection.CreateCommand();
            cmd.Transaction = tx;

            cmd.CommandText = "INSERT INTO Quizzes (UserId, Title) VALUES ($u, $t);";
            cmd.Parameters.AddWithValue("$u", userId);
            cmd.Parameters.AddWithValue("$t", title);
            cmd.ExecuteNonQuery();

            cmd.CommandText = "SELECT last_insert_rowid();";
            var quizId = (long)cmd.ExecuteScalar()!;
            tx.Commit();

            return (int)quizId;
        }

        // Получение викторин для конкретного пользователя
        public static List<QuizInfo> GetAllQuizzes(int userId)
        {
            if (_connection == null) throw new InvalidOperationException("DB not initialized");

            var result = new List<QuizInfo>();
            using var cmd = _connection.CreateCommand();

            cmd.CommandText = @"SELECT Q.Id, Q.Title, COUNT(Qu.Id) AS QuestionCount
                FROM Quizzes Q
                LEFT JOIN Questions Qu ON Q.Id = Qu.QuizId
                WHERE Q.UserId = $u
                GROUP BY Q.Id, Q.Title
                ORDER BY Q.Id;";
            cmd.Parameters.AddWithValue("$u", userId);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                result.Add(new QuizInfo
                {
                    Id = reader.GetInt32(0),
                    Title = reader.GetString(1),
                    QuestionCount = reader.GetInt32(2)
                });
            }

            return result;
        }

        public static void InsertQuestion(int quizId, string text, int timerSeconds, (string Text, bool IsCorrect)[] answers)
        {
            if (_connection == null) throw new InvalidOperationException("DB not initialized");

            using var tx = _connection.BeginTransaction();
            using var cmd = _connection.CreateCommand();
            cmd.Transaction = tx;

            cmd.CommandText = @"INSERT INTO Questions (QuizId, Text, TimerSeconds)
                VALUES ($q, $txt, $tm);";
            cmd.Parameters.AddWithValue("$q", quizId);
            cmd.Parameters.AddWithValue("$txt", text);
            cmd.Parameters.AddWithValue("$tm", timerSeconds);
            cmd.ExecuteNonQuery();

            cmd.CommandText = "SELECT last_insert_rowid();";
            var questionId = (long)cmd.ExecuteScalar()!;

            foreach (var (answerText, isCorrect) in answers)
            {
                cmd.CommandText = @"INSERT INTO Answers (QuestionId, Text, IsCorrect)
                    VALUES ($qid, $atext, $corr);";
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("$qid", questionId);
                cmd.Parameters.AddWithValue("$atext", answerText);
                cmd.Parameters.AddWithValue("$corr", isCorrect ? 1 : 0);
                cmd.ExecuteNonQuery();
            }

            tx.Commit();
        }

        public static void DeleteQuiz(int quizId)
        {
            if (_connection == null) throw new InvalidOperationException("DB not initialized");

            using var tx = _connection.BeginTransaction();
            using var cmd = _connection.CreateCommand();
            cmd.Transaction = tx;

            cmd.CommandText = @"DELETE FROM Answers
                WHERE QuestionId IN (
                    SELECT Id FROM Questions WHERE QuizId = $q
                );";
            cmd.Parameters.AddWithValue("$q", quizId);
            cmd.ExecuteNonQuery();

            cmd.CommandText = "DELETE FROM Questions WHERE QuizId = $q;";
            cmd.ExecuteNonQuery();

            cmd.CommandText = "DELETE FROM Quizzes WHERE Id = $q;";
            cmd.ExecuteNonQuery();

            tx.Commit();
        }

        public static List<QuestionDataModel> GetQuestionsForQuiz(int quizId)
        {
            if (_connection == null) throw new InvalidOperationException("DB not initialized");

            var result = new List<QuestionDataModel>();

            using var cmd = _connection.CreateCommand();
            cmd.CommandText = "SELECT * FROM Questions WHERE QuizId = $q;";
            cmd.Parameters.AddWithValue("$q", quizId);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                var question = new QuestionDataModel
                {
                    Id = reader.GetInt32(reader.GetOrdinal("Id")),
                    QuizId = quizId,
                    Text = reader.GetString(reader.GetOrdinal("Text")),
                    TimerSeconds = reader.GetInt32(reader.GetOrdinal("TimerSeconds")),
                    Answers = new List<AnswerDataModel>()
                };

                using var cmdAns = _connection.CreateCommand();
                cmdAns.CommandText = "SELECT * FROM Answers WHERE QuestionId = $qid;";
                cmdAns.Parameters.AddWithValue("$qid", question.Id);

                using var ansReader = cmdAns.ExecuteReader();
                while (ansReader.Read())
                {
                    question.Answers.Add(new AnswerDataModel
                    {
                        Id = ansReader.GetInt32(ansReader.GetOrdinal("Id")),
                        QuestionId = question.Id,
                        Text = ansReader.GetString(ansReader.GetOrdinal("Text")),
                        IsCorrect = ansReader.GetInt32(ansReader.GetOrdinal("IsCorrect")) != 0
                    });
                }

                result.Add(question);
            }

            return result;
        }

        public class User
        {
            public int Id { get; set; }
            public string Username { get; set; } = string.Empty;
            public int IQ { get; set; }
            public int TotalQuizzes { get; set; }
            public int CorrectAnswers { get; set; }
            public int TotalAnswers { get; set; }
            public double CorrectAnswerPercentage => TotalAnswers > 0 ? (double)CorrectAnswers / TotalAnswers * 100 : 0;
        }

        public class QuizInfo
        {
            public int Id { get; set; }
            public string Title { get; set; } = string.Empty;
            public int QuestionCount { get; set; }
        }

        public class QuestionDataModel
        {
            public int Id { get; set; }
            public int QuizId { get; set; }
            public string Text { get; set; } = string.Empty;
            public int TimerSeconds { get; set; }
            public List<AnswerDataModel> Answers { get; set; } = new();
        }

        public class AnswerDataModel
        {
            public int Id { get; set; }
            public int QuestionId { get; set; }
            public string Text { get; set; } = string.Empty;
            public bool IsCorrect { get; set; }
        }
    }
}