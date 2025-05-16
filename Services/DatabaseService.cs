using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;

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

            cmd.CommandText = @"CREATE TABLE IF NOT EXISTS Quizzes (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Title TEXT NOT NULL
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

        public static int InsertQuiz(string title)
        {
            if (_connection == null) throw new InvalidOperationException("DB not initialized");
            using var tx = _connection.BeginTransaction();
            using var cmd = _connection.CreateCommand();
            cmd.Transaction = tx;
            cmd.CommandText = "INSERT INTO Quizzes (Title) VALUES ($t);";
            cmd.Parameters.AddWithValue("$t", title);
            cmd.ExecuteNonQuery();

            cmd.CommandText = "SELECT last_insert_rowid();";
            var quizId = (long)cmd.ExecuteScalar()!;
            tx.Commit();
            return (int)quizId;
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

            foreach (var ans in answers)
            {
                cmd.CommandText = @"INSERT INTO Answers (QuestionId, Text, IsCorrect)
                    VALUES ($qid, $atext, $corr);";
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("$qid", questionId);
                cmd.Parameters.AddWithValue("$atext", ans.Text);
                cmd.Parameters.AddWithValue("$corr", ans.IsCorrect ? 1 : 0);
                cmd.ExecuteNonQuery();
            }
            tx.Commit();
        }

        public static List<QuizInfo> GetAllQuizzes()
        {
            if (_connection == null) throw new InvalidOperationException("DB not initialized");
            var result = new List<QuizInfo>();
            using var cmd = _connection.CreateCommand();
            cmd.CommandText = @"SELECT Q.Id, Q.Title, COUNT(Qu.Id) AS QuestionCount
                FROM Quizzes Q
                LEFT JOIN Questions Qu ON Q.Id = Qu.QuizId
                GROUP BY Q.Id, Q.Title
                ORDER BY Q.Id;";
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
        public static void DeleteQuiz(int quizId)
        {
            if (_connection == null) throw new InvalidOperationException("DB not initialized");
            using var tx = _connection.BeginTransaction();
            using var cmd = _connection.CreateCommand();
            cmd.Transaction = tx;

            // Сначала удаляем ответы
            cmd.CommandText = @"
        DELETE FROM Answers
         WHERE QuestionId IN (
             SELECT Id FROM Questions WHERE QuizId = $q
         );";
            cmd.Parameters.AddWithValue("$q", quizId);
            cmd.ExecuteNonQuery();

            // Затем удаляем вопросы
            cmd.CommandText = "DELETE FROM Questions WHERE QuizId = $q;";
            cmd.ExecuteNonQuery();

            // Наконец удаляем саму викторину
            cmd.CommandText = "DELETE FROM Quizzes WHERE Id = $q;";
            cmd.ExecuteNonQuery();

            tx.Commit();
        }


        public static List<QuestionDataModel> GetQuestionsForQuiz(int quizId)
        {
            if (_connection == null) throw new InvalidOperationException("DB not initialized");
            var list = new List<QuestionDataModel>();
            using var cmd = _connection.CreateCommand();
            cmd.CommandText = @"SELECT Id, Text, TimerSeconds FROM Questions WHERE QuizId = $q;";
            cmd.Parameters.AddWithValue("$q", quizId);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new QuestionDataModel
                {
                    Id = reader.GetInt32(0),
                    Text = reader.GetString(1),
                    TimerSeconds = reader.GetInt32(2)
                });
            }
            foreach (var q in list)
            {
                cmd.CommandText = @"SELECT Text, IsCorrect FROM Answers WHERE QuestionId = $qid;";
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("$qid", q.Id);
                using var r2 = cmd.ExecuteReader();
                while (r2.Read())
                {
                    q.Answers.Add(new AnswerDataModel
                    {
                        Text = r2.GetString(0),
                        IsCorrect = r2.GetInt32(1) == 1
                    });
                }
            }
            return list;
        }
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
        public string Text { get; set; } = string.Empty;
        public int TimerSeconds { get; set; }
        public List<AnswerDataModel> Answers { get; set; } = new();
    }

    public class AnswerDataModel
    {
        public string Text { get; set; } = string.Empty;
        public bool IsCorrect { get; set; }
    }
}