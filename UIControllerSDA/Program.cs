using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace UIControllerSDA
{
    public class TeacherModel
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Department { get; set; }
    }
    public interface IView
    {
        void Show();
        void Close();
    }
    public interface ITeacherView : IView
    {
        event EventHandler AddTeacher;
        void SetTeacherList(List<TeacherModel> teachers);
        TeacherModel GetTeacherDetails();
    }
    public interface IRepository<T>
    {
        void Add(T entity);
        List<T> GetAll();
    }

    public class BaseRepository
    {
        protected string connectionString = "your_connection_string_here";
    }

    public class TeacherRepository : BaseRepository, IRepository<TeacherModel>
    {
        public void Add(TeacherModel teacher)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                var query = "INSERT INTO Teachers (FirstName, LastName, Department) VALUES (@FirstName, @LastName, @Department)";
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@FirstName", teacher.FirstName);
                    command.Parameters.AddWithValue("@LastName", teacher.LastName);
                    command.Parameters.AddWithValue("@Department", teacher.Department);
                    command.ExecuteNonQuery();
                }
            }
        }

        public List<TeacherModel> GetAll()
        {
            var teachers = new List<TeacherModel>();
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                var query = "SELECT * FROM Teachers";
                using (var command = new SqlCommand(query, connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            teachers.Add(new TeacherModel
                            {
                                Id = reader.GetInt32(0),
                                FirstName = reader.GetString(1),
                                LastName = reader.GetString(2),
                                Department = reader.GetString(3)
                            });
                        }
                    }
                }
            }
            return teachers;
        }
    }
    public class TeacherPresenter
    {
        private readonly ITeacherView _view;
        private readonly IRepository<TeacherModel> _repository;

        public TeacherPresenter(ITeacherView view, IRepository<TeacherModel> repository)
        {
            _view = view;
            _repository = repository;
            _view.AddTeacher += OnAddTeacher;
            LoadTeachers();
        }

        private void LoadTeachers()
        {
            var teachers = _repository.GetAll();
            _view.SetTeacherList(teachers);
        }

        private void OnAddTeacher(object sender, EventArgs e)
        {
            var teacher = _view.GetTeacherDetails();
            _repository.Add(teacher);
            LoadTeachers();
        }
    }



    internal static class Program
    {
        public partial class TeacherView : Form, ITeacherView
        {
            public event EventHandler AddTeacher;

            public TeacherView()
            {
                InitializeComponent();
            }

            private void btnAddTeacher_Click(object sender, EventArgs e)
            {
                AddTeacher?.Invoke(this, EventArgs.Empty);
            }

            public void SetTeacherList(List<TeacherModel> teachers)
            {
                // Код для отображения списка преподавателей в интерфейсе
            }

            public TeacherModel GetTeacherDetails()
            {
                return new TeacherModel
                {
                    FirstName = txtFirstName.Text,
                    LastName = txtLastName.Text,
                    Department = txtDepartment.Text
                };
            }
        }
        public partial class MainView : Form
        {
            public MainView()
            {
                InitializeComponent();
            }

            private void btnOpenTeacherView_Click(object sender, EventArgs e)
            {
                ITeacherView teacherView = new TeacherView();
                IRepository<TeacherModel> teacherRepository = new TeacherRepository();
                TeacherPresenter presenter = new TeacherPresenter(teacherView, teacherRepository);
                teacherView.Show();
            }
        }
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }
}
