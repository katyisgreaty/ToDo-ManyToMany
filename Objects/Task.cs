using System.Collections.Generic;
using System.Data.SqlClient;
using System;

namespace ToDoList
{
    public class Task
    {
        private int _id;
        private string _description;
        private bool _complete;


        public Task(string Description, bool Complete = false, int Id = 0)
        {
            _id = Id;
            _description = Description;
            _complete = Complete;
        }

        public override bool Equals(System.Object otherTask)
        {
            if (!(otherTask is Task))
            {
                return false;
            }
            else {
                Task newTask = (Task) otherTask;
                bool idEquality = this.GetId() == newTask.GetId();
                bool descriptionEquality = this.GetDescription() == newTask.GetDescription();
                bool completeEquality = this.GetComplete() == newTask.GetComplete();
                return (idEquality && descriptionEquality && completeEquality);
            }
        }

        public override int GetHashCode()
        {
            return this.GetDescription().GetHashCode();
        }

        public bool GetComplete()
        {
            return _complete;
        }

        public void SetComplete(bool newComplete)
        {
            _complete = newComplete;
        }

        public int TranslateComplete()
        {
            if (this._complete == false)
            {
                return 0;
            }
            else
            {
                return 1;
            }
        }

        public string TaskUserViewComplete()
         {
             if (this._complete == true)
             {
                 return "Completed";
             }
             else
             {
                 return "Not yet completed";
             }
         }

        public int GetId()
        {
            return _id;
        }

        public string GetDescription()
        {
            return _description;
        }

        public void SetDescription(string newDescription)
        {
            _description = newDescription;
        }


        public static List<Task> GetAll()
        {
            List<Task> AllTasks = new List<Task>{};

            SqlConnection conn = DB.Connection();
            conn.Open();

            SqlCommand cmd = new SqlCommand("SELECT * FROM tasks;", conn);
            SqlDataReader rdr = cmd.ExecuteReader();

            while(rdr.Read())
            {
                int taskId = rdr.GetInt32(0);
                string taskDescription = rdr.GetString(1);
                bool taskComplete;
                if (rdr.GetByte(2) == 1)
                {
                    taskComplete = true;
                }
                else{
                    taskComplete = false;
                }
                Task newTask = new Task(taskDescription, taskComplete, taskId);
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


        public void Save()
        {
            SqlConnection conn = DB.Connection();
            conn.Open();

            SqlCommand cmd = new SqlCommand("INSERT INTO tasks (description, completed) OUTPUT INSERTED.id VALUES (@TaskDescription, @TaskComplete)", conn);

            SqlParameter descriptionParam = new SqlParameter();
            descriptionParam.ParameterName = "@TaskDescription";
            descriptionParam.Value = this.GetDescription();

            SqlParameter completeParam = new SqlParameter();
            completeParam.ParameterName = "@TaskComplete";
            completeParam.Value = this.TranslateComplete();

            cmd.Parameters.Add(descriptionParam);
            cmd.Parameters.Add(completeParam);


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

        public static void DeleteAll()
        {
            SqlConnection conn = DB.Connection();
            conn.Open();
            SqlCommand cmd = new SqlCommand("DELETE FROM tasks;", conn);
            cmd.ExecuteNonQuery();
            conn.Close();
        }

        public static Task Find(int id)
        {
            SqlConnection conn = DB.Connection();
            conn.Open();

            SqlCommand cmd = new SqlCommand("SELECT * FROM tasks WHERE id = @TaskId", conn);
            SqlParameter taskIdParameter = new SqlParameter();
            taskIdParameter.ParameterName = "@TaskId";
            taskIdParameter.Value = id.ToString();
            cmd.Parameters.Add(taskIdParameter);
            SqlDataReader rdr = cmd.ExecuteReader();

            int foundTaskId = 0;
            string foundTaskDescription = null;
            bool foundTaskComplete = false;

            while(rdr.Read())
            {
                foundTaskId = rdr.GetInt32(0);
                foundTaskDescription = rdr.GetString(1);
                if (rdr.GetByte(2) == 1)
                {
                    foundTaskComplete = true;
                }
                else
                {
                    foundTaskComplete = false;
                }
            }
            Task foundTask = new Task(foundTaskDescription, foundTaskComplete, foundTaskId);

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

        public void AddCategory(Category newCategory)
        {
            SqlConnection conn = DB.Connection();
            conn.Open();

            SqlCommand cmd = new SqlCommand("INSERT INTO categories_tasks (category_id, task_id) VALUES (@CategoryId, @TaskId);", conn);

            SqlParameter categoryIdParameter = new SqlParameter();
            categoryIdParameter.ParameterName = "@CategoryId";
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
            taskIdParameter.ParameterName = "@TaskId";
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
                SqlCommand categoryQuery = new SqlCommand("SELECT * FROM categories WHERE id = @CategoryId;", conn);

                SqlParameter categoryIdParameter = new SqlParameter();
                categoryIdParameter.ParameterName = "@CategoryId";
                categoryIdParameter.Value = categoryId;
                categoryQuery.Parameters.Add(categoryIdParameter);

                SqlDataReader queryReader = categoryQuery.ExecuteReader();
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
            if (conn != null)
            {
                conn.Close();
            }
            return categories;
        }

        public static List<Task> GetCompleted()
        {
            SqlConnection conn = DB.Connection();
            conn.Open();

            List<Task> CompletedTasks = new List<Task>{};

            SqlCommand taskQuery = new SqlCommand("SELECT * FROM tasks WHERE completed = 1;", conn);

            SqlDataReader rdr = taskQuery.ExecuteReader();
            while(rdr.Read())
            {
                int thisTaskId = rdr.GetInt32(0);
                string taskDescription = rdr.GetString(1);
                bool taskComplete;
                if (rdr.GetByte(2) == 1)
                {
                    taskComplete = true;
                }
                else
                {
                    taskComplete = false;
                }
                Task foundTask = new Task(taskDescription, taskComplete, thisTaskId);
                CompletedTasks.Add(foundTask);
            }

            if (rdr != null)
            {
                rdr.Close();
            }

            if (conn != null)
            {
                conn.Close();
            }
            return CompletedTasks;
        }

        public void Delete()
        {
            SqlConnection conn = DB.Connection();
            conn.Open();

            SqlCommand cmd = new SqlCommand("DELETE FROM tasks WHERE id = @TaskId; DELETE FROM categories_tasks WHERE task_id = @TaskId;", conn);
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
    }
}
