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
    public class DBCreationModel
    {
        private string serverPassword;
        [Display(Name = "服务器管理密码")]
        [Required(ErrorMessage = "服务器密码必须填写")]
        public string ServerPassword
        {
            get { return this.serverPassword; }
            set
            {
                var vc = new ValidationContext(this, null, null)
                {
                    MemberName = "ServerPassword"
                };
                Validator.ValidateProperty(value, vc);
                this.serverPassword = value;
            }
        }

        private string dbName;
        [Display(Name = "数据库名")]
        [Required(ErrorMessage = "数据库名必须填写")]
        public string DBName
        {
            get { return this.dbName; }
            set
            {
                var vc = new ValidationContext(this, null, null)
                {
                    MemberName = "DBName"
                };
                Validator.ValidateProperty(value, vc);
                this.dbName = value;
            }
        }

        private string adminPassowrd;
        [Display(Name = "管理员用户密码")]
        [Required(ErrorMessage = "管理员用户密码必须填写")]
        public string AdminPassword
        {
            get { return this.adminPassowrd; }
            set
            {
                var vc = new ValidationContext(this, null, null)
                {
                    MemberName = "AdminPassword"
                };
                Validator.ValidateProperty(value, vc);
                this.adminPassowrd = value;
            }
        }

        private string adminPassowrdConfirmation;
        [Display(Name = "管理员用户密码确认")]
        [Required(ErrorMessage = "管理员用户密码确认必须与管理员用户密码一致必须填写")]
        public string AdminPasswordConfirmation
        {
            get { return this.adminPassowrdConfirmation; }
            set
            {
                var vc = new ValidationContext(this, null, null)
                {
                    MemberName = "AdminPasswordConfirmation"
                };
                Validator.ValidateProperty(value, vc);
                this.adminPassowrdConfirmation= value;
            }
        }
    }
}
