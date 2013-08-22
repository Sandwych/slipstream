using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.ComponentModel.DataAnnotations;

namespace SlipStream.Client.Agos.Models
{
    public class LoginModel
    {
        public LoginModel()
        {
            this.Address = "http://localhost:9287/jsonrpc";
            this.Login = "root";
            this.Password = "root";
        }

        private string address;
        [Display(Name = "服务器地址", Description = "服务器的 URI 地址")]
        [Required(ErrorMessage = "服务器地址必填")]
        public string Address
        {
            get { return this.address; }
            set
            {
                var vc = new ValidationContext(this, null, null)
                {
                    MemberName = "Address"
                };
                Validator.ValidateProperty(value, vc);
                this.address = value;
            }
        }

        private string database;
        [Display(Name = "数据库", Description = "要连接的数据库名称")]
        [Required(ErrorMessage = "数据库名称不能为空")]
        public string Database
        {
            get { return this.database; }
            set
            {
                Validator.ValidateProperty(value, new ValidationContext(this, null, null) { MemberName = "Database" });
                this.database = value;
            }
        }


        private string login;
        [Display(Name = "用户名", Description = "用于登录的用户帐号")]
        [Required(ErrorMessage = "用户名必填")]
        public string Login
        {
            get { return this.login; }
            set
            {
                Validator.ValidateProperty(value, new ValidationContext(this, null, null) { MemberName = "Login" });
                this.login = value;
            }
        }

        private string password;
        [Display(Name = "密码", Description = "必须填写密码")]
        [Required(ErrorMessage = "必须填写密码")]
        public string Password
        {
            get { return this.password; }
            set
            {

                var vc = new ValidationContext(this, null, null)
                {
                    MemberName = "Password"
                };

                Validator.ValidateProperty(value, vc);

                this.password = value;
            }
        }
    }
}
