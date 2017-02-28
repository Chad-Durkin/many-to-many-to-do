using Xunit;
using System.Collections.Generic;
using System;
using System.Data;
using System.Data.SqlClient;

namespace ToDoList
{
    public class ToDoTest : IDisposable
    {
        public ToDoTest()
        {
            DBConfiguration.ConnectionString = "Data Source=(localdb)\\mssqllocaldb;Initial Catalog=to_do_test;Integrated Security=SSPI;";
        }

        [Fact]
        public void Test_DatabaseEmptyAtFirst()
        {
            //Arrange, Act
            int result = Task.GetAll().Count;

            //Assert
            Assert.Equal(0, result);
        }

        [Fact]
        public void Test_Equal_ReturnsTrueIfDescriptionsAreTheSame()
        {
            //Arrange, Act
            Task firstTask = new Task("Mow the lawn", "2017-02-17");
            Task secondTask = new Task("Mow the lawn", "2017-02-17");

            //Assert
            Assert.Equal(firstTask, secondTask);
        }

        [Fact]
        public void Test_Save_SavesToDatabase()
        {
            //Arrange
            Task testTask = new Task("Mow the lawn", "2017-02-17");

            //Act
            testTask.Save();
            List<Task> result = Task.GetAll();
            List<Task> testList = new List<Task>{testTask};

            //Assert
            Assert.Equal(testList, result);
        }

        [Fact]
        public void Test_Save_AssignsIdToObject()
        {
            //Arrange
            Task testTask = new Task("Mow the lawn", "2017-02-17");

            //Act
            testTask.Save();
            Task savedTask = Task.GetAll()[0];

            int result = savedTask.GetId();
            int testId = testTask.GetId();

            //Assert
            Assert.Equal(testId, result);
        }

        [Fact]
        public void Test_Find_FindsTaskInDatabase()
        {
            //Arrange
            Task testTask = new Task("Mow the lawn", "2017-02-17");
            testTask.Save();

            //Act
            Task foundTask = Task.Find(testTask.GetId());

            //Assert
            Assert.Equal(testTask, foundTask);
        }

        [Fact]
        public void Test_DueDate_SavesInRightFormat()
        {
            //Arrange
            string testDate = "2017-02-17";
            string taskDate;
            Task testTask = new Task("Mow the lawn", "2017-02-17");
            testTask.Save();

            //Act
            Task foundTask = Task.Find(testTask.GetId());
            taskDate = foundTask.GetDueDate();
            //Assert
            Assert.Equal(testDate, taskDate);
        }

        [Fact]
        public void Test_AddCategory_AddsCategoryToTask()
        {
            //Arrange
            Task testTask = new Task("mow the lawn", "2017-02-17");
            testTask.Save();

            Category testCategory = new Category("Home stuff");
            testCategory.Save();

            //Act
            testTask.AddCategory(testCategory);

            List<Category> result = testTask.GetCategories();
            List<Category> testList = new List<Category>{testCategory};

            //Assert
            Assert.Equal(testList, result);
        }

        [Fact]
        public void Test_GetCategories_ReturnsAllTaskCategories()
        {
            //Arrange
            Task testTask = new Task("Mow the Lawn", "2017-02-17");
            testTask.Save();

            Category testCategory1 = new Category("Home Stuff");
            testCategory1.Save();

            Category testCategory2 = new Category("Work Stuff");
            testCategory2.Save();

            //Act
            testTask.AddCategory(testCategory1);
            List<Category> result = testTask.GetCategories();
            List<Category> testList = new List<Category> {testCategory1};

            //Assert
            Assert.Equal(testList, result);
        }

        [Fact]
        public void Test_Delete_DeletesTaskAssociationsFrmDatabase()
        {
            //Arrange
            Category testCategory = new Category("Home stuff");
            testCategory.Save();

            Task testTask = new Task("mow the lawn", "2017-02-17");
            testTask.Save();

            //Act
            testTask.AddCategory(testCategory);
            testTask.Delete();

            List<Task> resultCategoryTasks = testCategory.GetTasks();
            List<Task> testCategoryTasks = new List<Task> {};

            //Assert
            Assert.Equal(testCategoryTasks, resultCategoryTasks);
        }

        [Fact]
        public void Test_Done_DoneIsSetToTrueOrFalse()
        {
            //Arrange
            int result = 1;
            Category testCategory = new Category("Home stuff");
            testCategory.Save();

            Task testTask = new Task("mow the lawn", "2017-02-17");
            testTask.Save();

            //Act
            testTask.SetDone(1);

            //Assert
            Assert.Equal(result, testTask.GetDone());
        }

        public void Dispose()
        {
            Category.DeleteAll();
            Task.DeleteAll();
        }
    }
}
