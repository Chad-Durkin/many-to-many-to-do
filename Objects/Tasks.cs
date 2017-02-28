using System.Collections.Generic;
using System.Data.SqlClient;
using System;

namespace ToDoList
{
    public class Task
    {
        private int _id;
        private string _description;
        private string _dueDate;
        private int _done;

        public Task(string Description, string dueDate, int done = 0, int Id = 0)
        {
            _id = Id;
            _description = Description;
            _dueDate = dueDate;
            _done = done;
        }

        public override bool Equals(System.Object otherTask)
        {
            if (!(otherTask is Task))
            {
                return false;
            }
            else
            {
                Task newTask = (Task) otherTask;
                bool idEquality = this.GetId() == newTask.GetId();
                bool descriptionEquality = this.GetDescription() == newTask.GetDescription();
                bool dueDateEquality = this.GetDueDate() == newTask.GetDueDate();
                bool doneEquality = this.GetDone() == newTask.GetDone();
                return (idEquality && descriptionEquality && dueDateEquality);
            }
        }

        public int GetDone()
        {
            return _done;
        }

        public void SetDone(int done)
        {
            _done = done;
        }

        public override int GetHashCode()
        {
            return this.GetDescription().GetHashCode();
        }

        public string GetDueDate()
        {
            return _dueDate;
        }

        public int GetId()
        {
            return _id;
        }

        public string GetDescription()
        {
            return _description;
        }

        public void UpdateDone(int done)
        {
            SqlConnection conn = DB.Connection();
            conn.Open();

            SqlCommand cmd = new SqlCommand("UPDATE tasks SET done = @Done OUTPUT INSERTED.description WHERE id = @TaskId;", conn);

            SqlParameter newNameParameter = new SqlParameter();
            newNameParameter.ParameterName = "@Done";
            newNameParameter.Value = done;
            cmd.Parameters.Add(newNameParameter);


            SqlParameter categoryIdParameter = new SqlParameter();
            categoryIdParameter.ParameterName = "@TaskId";
            categoryIdParameter.Value = this.GetId();
            cmd.Parameters.Add(categoryIdParameter);
            SqlDataReader rdr = cmd.ExecuteReader();

            while(rdr.Read())
            {
                this._description = rdr.GetString(0);
            }

            if(rdr != null)
            {
                rdr.Close();
            }

            if(conn != null)
            {
                conn.Close();
            }
        }

        public static List<Task> GetAll()
        {
            List<Task> AllTasks = new List<Task>{};

            SqlConnection conn = DB.Connection();
            conn.Open();

            SqlCommand cmd = new SqlCommand("SELECT * FROM tasks ORDER BY due_date DESC;", conn);
            SqlDataReader rdr = cmd.ExecuteReader();

            while(rdr.Read())
            {
                int taskId = rdr.GetInt32(0);
                string taskDescription = rdr.GetString(1);
                string taskDueDate = rdr.GetDateTime(2).ToString("yyyy-MM-dd");
                int taskDone = rdr.GetByte(3);
                Task newTask = new Task(taskDescription, taskDueDate, taskDone, taskId);
                AllTasks.Add(newTask);
            }
            if (rdr != null)
            {
                rdr.Close();
            }
            if (conn != null)
            {
                conn.Close();
            }
            return AllTasks;
        }

        public void AddCategory(Category newCategory)
        {
            SqlConnection conn = DB.Connection();
            conn.Open();

            SqlCommand cmd = new SqlCommand("INSERT INTO categories_tasks (category_id, task_id) VALUES (@CategoryId, @TaskId);", conn);

            SqlParameter categoryIdParameter = new SqlParameter();
            categoryIdParameter.ParameterName ="@CategoryId";
            categoryIdParameter.Value = newCategory.GetId();
            cmd.Parameters.Add(categoryIdParameter);

            SqlParameter taskIdParameter = new SqlParameter();
            taskIdParameter.ParameterName = "@TaskId";
            taskIdParameter.Value = this.GetId();
            cmd.Parameters.Add(taskIdParameter);

            cmd.ExecuteNonQuery();

            if (conn != null)
            {
                conn.Close();
            }
        }

        public List<Category> GetCategories()
        {
            SqlConnection conn = DB.Connection();
            conn.Open();

            SqlCommand cmd = new SqlCommand("SELECT category_id FROM categories_tasks WHERE task_id = @TaskId;", conn);

            SqlParameter taskIdParameter = new SqlParameter();
            taskIdParameter.ParameterName ="@TaskId";
            taskIdParameter.Value = this.GetId();
            cmd.Parameters.Add(taskIdParameter);

            SqlDataReader rdr = cmd.ExecuteReader();

            List<int> categoryIds = new List<int> {};

            while (rdr.Read())
            {
                int categoryId = rdr.GetInt32(0);
                categoryIds.Add(categoryId);
            }

            if (rdr != null)
            {
                rdr.Close();
            }

            List<Category> categories = new List<Category> {};

            foreach (int categoryId in categoryIds)
            {
                SqlCommand categoryQuery = new SqlCommand("SELECT * FROM categories WHERE id = @CategoryID;", conn);

                categoryQuery.Parameters.Add(new SqlParameter("@CategoryId", categoryId));

                SqlDataReader  queryReader = categoryQuery.ExecuteReader();
                while (queryReader.Read())
                {
                    int thisCategoryId = queryReader.GetInt32(0);
                    string categoryName = queryReader.GetString(1);
                    Category foundCategory = new Category(categoryName, thisCategoryId);
                    categories.Add(foundCategory);
                }
                if (queryReader != null)
                {
                    queryReader.Close();
                }
            }
            if(conn != null)
            {
                conn.Close();
            }

            return categories;
        }


        public void Save()
        {
            SqlConnection conn = DB.Connection();
            conn.Open();

            SqlCommand cmd = new SqlCommand("INSERT INTO tasks (description, due_date, done) OUTPUT INSERTED.id VALUES (@TaskDescription, @TaskDueDate, @TaskDone);", conn);

            SqlParameter descriptionParameter = new SqlParameter();
            descriptionParameter.ParameterName = "@TaskDescription";
            descriptionParameter.Value = this.GetDescription();

            SqlParameter dueDateParameter = new SqlParameter();
            dueDateParameter.ParameterName = "@TaskDueDate";
            dueDateParameter.Value = this.GetDueDate();


            SqlParameter doneParameter = new SqlParameter();
            doneParameter.ParameterName = "@TaskDone";
            doneParameter.Value = this.GetDone();

            cmd.Parameters.Add(descriptionParameter);
            cmd.Parameters.Add(dueDateParameter);
            cmd.Parameters.Add(doneParameter);

            SqlDataReader rdr = cmd.ExecuteReader();

            while(rdr.Read())
            {
                this._id = rdr.GetInt32(0);
            }
            if (rdr != null)
            {
                rdr.Close();
            }
            if (conn != null)
            {
                conn.Close();
            }
        }

        public static Task Find(int id)
        {
            SqlConnection conn = DB.Connection();
            conn.Open();

            SqlCommand cmd = new SqlCommand("SELECT * FROM tasks WHERE id = @TaskId;", conn);
            SqlParameter taskIdParameter = new SqlParameter();
            taskIdParameter.ParameterName = "@TaskId";
            taskIdParameter.Value = id.ToString();
            cmd.Parameters.Add(taskIdParameter);
            SqlDataReader rdr = cmd.ExecuteReader();

            int foundTaskId = 0;
            string foundTaskDescription = null;
            string foundTaskDueDate = null;
            int foundTaskDone = 0;

            while(rdr.Read())
            {
                foundTaskId = rdr.GetInt32(0);
                foundTaskDescription = rdr.GetString(1);
                foundTaskDueDate = rdr.GetDateTime(2).ToString("yyyy-MM-dd");
                foundTaskDone = rdr.GetByte(3);
            }
            Task foundTask = new Task(foundTaskDescription, foundTaskDueDate, foundTaskDone, foundTaskId);

            if (rdr != null)
            {
                rdr.Close();
            }
            if (conn != null)
            {
                conn.Close();
            }
            return foundTask;
        }

        public void Delete()
        {
            SqlConnection conn = DB.Connection();
            conn.Open();

            SqlCommand cmd = new SqlCommand("DELETE FROM tasks WHERE id = @TaskId; DELETE FROM categories_tasks WHERE task_id = @TaskId;", conn);

            cmd.Parameters.Add(new SqlParameter("@TaskId", this.GetId()));
            cmd.ExecuteNonQuery();

            if(conn != null)
            {
                conn.Close();
            }
        }

        public static void DeleteAll()
        {
            SqlConnection conn = DB.Connection();
            conn.Open();
            SqlCommand cmd = new SqlCommand("DELETE FROM tasks; DELETE FROM categories_tasks", conn);
            cmd.ExecuteNonQuery();
            conn.Close();
        }
    }
}
